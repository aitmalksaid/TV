using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using VatApp_Net.Models;

namespace VatApp_Net
{
    public class SageScanner
    {
        private string _connectionString;
        private string _dbName;

        public SageScanner(string server, string db)
        {
            _dbName = db;
            // Ajout de Encrypt=False pour la compatibilité avec SQL 2008 R2 (qui ne supporte pas toujours le cryptage forcé par les nouvelles versions de .NET)
            _connectionString = $"Server={server};Database={db};Integrated Security=True;TrustServerCertificate=True;Encrypt=False;";
        }

        public List<string> GetDatabases()
        {
            var dbs = new List<string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var sql = "SELECT name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') AND HAS_DBACCESS(name) = 1 ORDER BY name";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) dbs.Add(reader.GetString(0));
                }
            }
            return dbs;
        }

        public (string IFSoc, string ICESoc) GetVatConfig(string db)
        {
            String ifSoc = "", iceSoc = "";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                // 1. Chercher dans T_VAT_CONFIG (On peut être sur 'master', donc on ignore si la table manque)
                try {
                    var sql1 = "SELECT IdentifiantFiscal, ICE FROM T_VAT_CONFIG WHERE DBName = @db";
                    using (var cmd = new SqlCommand(sql1, conn))
                    {
                        cmd.Parameters.AddWithValue("@db", db);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                                return (reader["IdentifiantFiscal"].ToString().Trim(), reader["ICE"].ToString().Trim());
                        }
                    }
                } catch { /* Table manquante ou autre erreur, on continue */ }

                // 2. Sinon chercher dans P_DOSSIER de la base cible
                try {
                    var sql2 = $"SELECT D_Identifiant, D_Siret FROM [{db}].dbo.P_DOSSIER";
                    using (var cmd = new SqlCommand(sql2, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return (reader["D_Identifiant"].ToString().Trim(), reader["D_Siret"].ToString().Trim());
                    }
                } catch { /* Base non Sage ou autre erreur */ }
            }
            return ("", "");
        }

        public void SaveVatConfig(string db, string ifSoc, string iceSoc)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var sql = @"
                    IF EXISTS (SELECT 1 FROM T_VAT_CONFIG WHERE DBName = @db)
                        UPDATE T_VAT_CONFIG SET IdentifiantFiscal = @if, ICE = @ice WHERE DBName = @db
                    ELSE
                        INSERT INTO T_VAT_CONFIG (DBName, IdentifiantFiscal, ICE) VALUES (@db, @if, @ice)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@db", db);
                    cmd.Parameters.AddWithValue("@if", ifSoc);
                    cmd.Parameters.AddWithValue("@ice", iceSoc);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public (int totalRaw, int totalLettered, string errorInfo, int absoluteTotal) FetchDiagnostics(DateTime start, DateTime end)
        {
            int raw = 0, lettered = 0, absTotal = 0;
            string error = "";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // Diagnostic plus complet
                    var sql = $@"
                        SELECT 
                            (SELECT COUNT(*) FROM F_ECRITUREC) as AbsoluteTotal,
                            (SELECT COUNT(DISTINCT EC_Piece) FROM F_ECRITUREC WHERE EC_Date BETWEEN @start AND @end AND (CG_Num LIKE '6%' OR CG_Num LIKE '345%' OR CG_Num LIKE '445%' OR CG_Num LIKE '442%')) as TotalRaw,
                            (SELECT COUNT(DISTINCT CASE WHEN EC_Lettrage <> '' AND EC_Lettrage IS NOT NULL THEN EC_Piece END) FROM F_ECRITUREC WHERE EC_Date BETWEEN @start AND @end AND (CG_Num LIKE '6%' OR CG_Num LIKE '345%' OR CG_Num LIKE '445%' OR CG_Num LIKE '442%')) as TotalLettered";
                    
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                absTotal = Convert.ToInt32(reader["AbsoluteTotal"]);
                                raw = Convert.ToInt32(reader["TotalRaw"]);
                                lettered = Convert.ToInt32(reader["TotalLettered"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { error = ex.Message; }
            return (raw, lettered, error, absTotal);
        }

        public List<Deduction> GetDeductions(DateTime start, DateTime end)
        {
            var list = new List<Deduction>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                // Requête robuste compatible avec plusieurs versions de Sage
                // On simplifie les jointures pour éviter les problèmes de CTE complexes sur les vieux SQL Server
                var sql = @"
                    SELECT 
                        f_inv.EC_Piece, 
                        f_inv.EC_Date AS DateFacture, 
                        f_pay.EC_Date AS DatePaiement, 
                        f_pay.JO_Num AS JournalPaiement,
                        t.CT_Intitule AS NomFournisseur, 
                        t.CT_Identifiant AS IF_Fournisseur, 
                        t.CT_Siret AS ICE_Fournisseur,
                        f_inv.CT_Num,
                        f_inv.EC_Reference,
                        (SELECT SUM(CASE WHEN CG_Num LIKE '6%' THEN EC_Montant ELSE 0 END) 
                         FROM F_ECRITUREC WHERE EC_Piece = f_inv.EC_Piece AND EC_Date = f_inv.EC_Date) as HT,
                        (SELECT SUM(CASE WHEN (CG_Num LIKE '345%' OR CG_Num LIKE '445%') THEN EC_Montant ELSE 0 END) 
                         FROM F_ECRITUREC WHERE EC_Piece = f_inv.EC_Piece AND EC_Date = f_inv.EC_Date) as TVA
                    FROM F_ECRITUREC f_inv
                    JOIN F_ECRITUREC f_pay ON f_inv.EC_Lettrage = f_pay.EC_Lettrage AND f_inv.CT_Num = f_pay.CT_Num
                    JOIN F_COMPTET t ON f_inv.CT_Num = t.CT_Num
                    WHERE f_inv.EC_Lettrage <> '' 
                    AND f_inv.EC_Lettrage IS NOT NULL
                    AND f_inv.EC_Sens = 1 -- Crédit (Facture)
                    AND f_pay.EC_Sens = 0 -- Débit (Paiement)
                    AND t.CT_Type = 1    -- Fournisseurs uniquement
                    AND f_pay.EC_Date BETWEEN @start AND @end";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", end);
                    using (var reader = cmd.ExecuteReader())
                    {
                        int ord = 1;
                        while (reader.Read())
                        {
                            decimal ht = reader["HT"] != DBNull.Value ? Convert.ToDecimal(reader["HT"]) : 0;
                            decimal tva = reader["TVA"] != DBNull.Value ? Convert.ToDecimal(reader["TVA"]) : 0;

                            var d = new Deduction
                            {
                                Ordre = ord++,
                                NumeroFacture = reader["EC_Piece"].ToString(),
                                NomFournisseur = reader["NomFournisseur"].ToString(),
                                DateFacture = Convert.ToDateTime(reader["DateFacture"]),
                                DatePaiement = Convert.ToDateTime(reader["DatePaiement"]),
                                ReferenceEcriture = reader["EC_Reference"]?.ToString() ?? "",
                                IF_Fournisseur = reader["IF_Fournisseur"]?.ToString() ?? "",
                                ICE_Fournisseur = reader["ICE_Fournisseur"]?.ToString() ?? "",
                                ModePaiement = MapModePaiement(reader["JournalPaiement"].ToString()),
                                MontantHT = ht,
                                MontantTva = tva,
                                MontantTTC = ht + tva,
                                TauxTva = ht > 0 ? Math.Round((tva / ht) * 100, 0) : 0
                            };
                            list.Add(d);
                        }
                    }
                }
            }
            return list;
        }

        private int MapModePaiement(string journal)
        {
            if (journal.Contains("CHQ")) return 2;
            if (journal.Contains("VIR")) return 3;
            if (journal.Contains("CAI") || journal.Contains("ESP")) return 1;
            if (journal.Contains("LCN") || journal.Contains("EFF")) return 4;
            return 5; // Autre
        }

        public static List<string> GetLocalInstances()
        {
            var instances = new HashSet<string>();
            instances.Add(@".\SAGE100");
            instances.Add(@"(local)\SAGE100");
            instances.Add(@"localhost\SAGE100");

            try
            {
                // 1. Recherche dans le registre (Sage)
                // On scanne plusieurs endroits possibles pour le serveur SQL de Sage
                string[] registryPaths = {
                    @"Software\Sage\Sage 100 SQL\Connexion",
                    @"Software\Sage\Sage 100\SQL Server Name",
                    @"Software\Sage\Sage 100\Connexion",
                    @"Software\Sage\Common\SQLServer"
                };

                foreach (var path in registryPaths)
                {
                    using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(path))
                    {
                        if (key != null)
                        {
                            var server = key.GetValue("Server")?.ToString() ?? key.GetValue("")?.ToString();
                            if (!string.IsNullOrEmpty(server)) instances.Add(server);
                        }
                    }
                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path))
                    {
                        if (key != null)
                        {
                            var server = key.GetValue("Server")?.ToString() ?? key.GetValue("")?.ToString();
                            if (!string.IsNullOrEmpty(server)) instances.Add(server);
                        }
                    }
                }
            }
            catch { /* Ignorer les erreurs de scan */ }

            return new List<string>(instances);
        }
    }
}
