using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using VatApp_Net.Models;

namespace VatApp_Net
{
    public class XmlGenerator
    {
        public string GenerateZip(DeclarationTva decla, string outputPath)
        {
            string tempXmlPath = Path.Combine(Path.GetTempPath(), $"tva_{DateTime.Now:yyyyMMddHHmmss}.xml");
            
            var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = false };
            using (var writer = XmlWriter.Create(tempXmlPath, settings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("releveDeductions");
                
                writer.WriteElementString("identifiantFiscal", decla.IdentifiantFiscal);
                writer.WriteElementString("ice", decla.ICE);
                writer.WriteElementString("annee", decla.Annee.ToString());
                writer.WriteElementString("periode", decla.Periode.ToString());
                writer.WriteElementString("regime", decla.Regime.ToString());
                
                writer.WriteStartElement("rdps");
                foreach (var d in decla.Deductions)
                {
                    writer.WriteStartElement("rdp");
                    writer.WriteElementString("ord", d.Ordre.ToString());
                    writer.WriteElementString("num", d.NumeroFacture);
                    writer.WriteElementString("des", d.NomFournisseur);
                    writer.WriteElementString("tva", d.MontantTva.ToString("F2").Replace(",", "."));
                    writer.WriteElementString("ht", d.MontantHT.ToString("F2").Replace(",", "."));
                    writer.WriteElementString("ttc", d.MontantTTC.ToString("F2").Replace(",", "."));
                    writer.WriteElementString("ref", d.ReferenceEcriture);
                    writer.WriteElementString("tx", d.TauxTva.ToString("0").Replace(",", "."));
                    writer.WriteElementString("mp", d.ModePaiement.ToString());
                    writer.WriteElementString("dp", d.DatePaiement.ToString("yyyy-MM-dd"));
                    writer.WriteElementString("df", d.DateFacture.ToString("yyyy-MM-dd"));
                    writer.WriteElementString("if", d.IF_Fournisseur);
                    writer.WriteElementString("ice", d.ICE_Fournisseur);
                    writer.WriteEndElement(); // rdp
                }
                writer.WriteEndElement(); // rdps
                writer.WriteEndElement(); // releveDeductions
            }

            if (File.Exists(outputPath)) File.Delete(outputPath);

            using (ZipArchive zip = ZipFile.Open(outputPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(tempXmlPath, Path.GetFileName(tempXmlPath));
            }

            if (File.Exists(tempXmlPath)) File.Delete(tempXmlPath);
            
            return outputPath;
        }
    }
}
