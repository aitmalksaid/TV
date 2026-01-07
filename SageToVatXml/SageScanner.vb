Imports Microsoft.Data.SqlClient
Imports SageToVatXml.Models

Public Class SageScanner
    Private _connectionString As String

    Public Sub New(connectionString As String)
        _connectionString = connectionString
    End Sub

    Public Function GetCompanyInfo() As (IFSoc As String, ICESoc As String)
        Using conn As New SqlConnection(_connectionString)
            conn.Open()
            Dim sql As String = "SELECT D_Identifiant, D_Siret FROM P_DOSSIER"
            Using cmd As New SqlCommand(sql, conn)
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Return (reader("D_Identifiant").ToString().Trim(), reader("D_Siret").ToString().Trim())
                    End If
                End Using
            End Using
        End Using
        Return ("", "")
    End Function

    Public Function GetDeductions(dateDebut As DateTime, dateFin As DateTime) As List(Of Deduction)
        Dim deductions As New List(Of Deduction)()

        Using conn As New SqlConnection(_connectionString)
            conn.Open()

            ' Cette requête identifie les duos Facture (Crédit) / Paiement (Débit) lettrés
            ' où le paiement a eu lieu dans la période demandée.
            Dim sql As String = "
                SELECT 
                    f_inv.EC_Piece, 
                    f_inv.EC_Date AS DateFacture, 
                    f_inv.EC_Reference,
                    f_pay.EC_Date AS DatePaiement, 
                    f_pay.JO_Num AS JournalPaiement,
                    t.CT_Intitule AS NomFournisseur, 
                    t.CT_Identifiant AS IF_Fournisseur, 
                    t.CT_Siret AS ICE_Fournisseur,
                    f_inv.CT_Num,
                    f_inv.EC_No
                FROM F_ECRITUREC f_inv
                JOIN F_ECRITUREC f_pay ON f_inv.EC_Lettrage = f_pay.EC_Lettrage AND f_inv.CT_Num = f_pay.CT_Num
                JOIN F_COMPTET t ON f_inv.CT_Num = t.CT_Num
                WHERE f_inv.EC_Lettre = 1 
                AND f_inv.EC_Sens = 1 -- Crédit dans Sage SQL (0=Débit, 1=Crédit)
                AND f_pay.EC_Sens = 0 -- Débit
                AND f_pay.EC_Date BETWEEN @start AND @end
                AND f_inv.EC_Lettrage <> ''"

            Using cmd As New SqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@start", dateDebut)
                cmd.Parameters.AddWithValue("@end", dateFin)

                Using reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim d As New Deduction()
                        d.NumeroFacture = reader("EC_Piece").ToString()
                        d.DateFacture = Convert.ToDateTime(reader("DateFacture"))
                        d.DatePaiement = Convert.ToDateTime(reader("DatePaiement"))
                        d.NomFournisseur = reader("NomFournisseur").ToString()
                        d.IdFournisseur = reader("IF_Fournisseur").ToString()
                        d.ICEFournisseur = reader("ICE_Fournisseur").ToString()
                        
                        ' Tentative de récupération du mode de paiement via le code journal
                        d.ModePaiement = MapJournalToMode(reader("JournalPaiement").ToString())
                        
                        ' TODO: Récupérer les montants HT et TVA en croisant avec les autres lignes de l'écriture
                        ' Pour cette version, nous ferons une sous-requête ou une jointure supplémentaire
                        deductions.Add(d)
                    End While
                End Using
            End Using

            ' Enrichissement avec les montants HT et TVA
            For Each d In deductions
                FillAmounts(conn, d)
            Next
        End Using

        Return deductions
    End Function

    Private Sub FillAmounts(conn As SqlConnection, d As Deduction)
        ' On cherche les lignes de la même pièce comptable qui ne sont pas sur le compte tiers
        ' Typiquement les comptes de classe 6 (HT) et 3455 (TVA)
        Dim sql As String = "
            SELECT 
                SUM(CASE WHEN CG_Num LIKE '6%' THEN EC_Montant ELSE 0 END) as HT,
                SUM(CASE WHEN CG_Num LIKE '3455%' OR CG_Num LIKE '4455%' THEN EC_Montant ELSE 0 END) as TVA,
                MAX(EC_Montant) as TTC -- Sur la ligne tiers
            FROM F_ECRITUREC 
            WHERE EC_Piece = @piece AND EC_Date = @dateFacture"
        
        Using cmd As New SqlCommand(sql, conn)
            cmd.Parameters.AddWithValue("@piece", d.NumeroFacture)
            cmd.Parameters.AddWithValue("@dateFacture", d.DateFacture)
            Using reader As SqlDataReader = cmd.ExecuteReader()
                If reader.Read() Then
                    d.MontantHT = If(IsDBNull(reader("HT")), 0, Convert.ToDecimal(reader("HT")))
                    d.MontantTva = If(IsDBNull(reader("TVA")), 0, Convert.ToDecimal(reader("TVA")))
                    d.MontantTTC = d.MontantHT + d.MontantTva
                    
                    ' Calcul automatique du taux si possible
                    If d.MontantHT > 0 Then
                        d.TauxTva = Math.Round((d.MontantTva / d.MontantHT) * 100, 0)
                    End If
                End If
            End Using
        End Using
    End Sub

    Private Function MapJournalToMode(journalCode As String) As Integer
        ' Mapage simplifié des codes journaux Sage vers les codes DGI Maroc
        ' 1: Espèces, 2: Chèque, 3: Virement, 4: Traite, 5: Autre
        Select Case journalCode.ToUpper()
            Case "BQ", "BNK", "BANK" : Return 3 ' Virement par défaut pour la banque
            Case "CHQ" : Return 2
            Case "CAI", "CASH" : Return 1
            Case Else : Return 5
        End Select
    End Function
End Class
