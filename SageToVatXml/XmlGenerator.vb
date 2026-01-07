Imports System.Xml.Linq
Imports System.IO
Imports System.IO.Compression
Imports SageToVatXml.Models

Public Class XmlGenerator
    Public Sub GenerateZip(declaration As DeclarationTva, outputPath As String)
        Dim xmlDoc = New XDocument(
            New XDeclaration("1.0", "UTF-8", "yes"),
            New XElement("releveDeductions",
                New XElement("identifiantFiscal", declaration.IdentifiantFiscal),
                New XElement("ice", declaration.ICE),
                New XElement("annee", declaration.Annee),
                New XElement("periode", declaration.Periode),
                New XElement("regime", declaration.Regime),
                New XElement("rdps",
                    declaration.Deductions.Select(Function(d, index)
                        Return New XElement("rdp",
                            New XElement("ord", index + 1),
                            New XElement("num", d.NumeroFacture),
                            New XElement("des", d.NomFournisseur),
                            New XElement("tva", d.MontantTva.ToString("F2").Replace(",", ".")),
                            New XElement("ht", d.MontantHT.ToString("F2").Replace(",", ".")),
                            New XElement("ttc", d.MontantTTC.ToString("F2").Replace(",", ".")),
                            New XElement("ref", d.NumeroFacture),
                            New XElement("tx", d.TauxTva),
                            New XElement("mp", d.ModePaiement),
                            New XElement("dp", d.DatePaiement.ToString("yyyy-MM-dd")),
                            New XElement("df", d.DateFacture.ToString("yyyy-MM-dd")),
                            New XElement("if", d.IdFournisseur),
                            New XElement("ice", d.ICEFournisseur)
                        )
                    End Function)
                )
            )
        )

        Dim tempXmlPath = Path.Combine(Path.GetTempPath(), $"tva_{DateTime.Now:yyyyMMddHHmmss}.xml")
        xmlDoc.Save(tempXmlPath)

        If File.Exists(outputPath) Then File.Delete(outputPath)

        Using zip As ZipArchive = ZipFile.Open(outputPath, ZipArchiveMode.Create)
            ' Le fichier à l'intérieur du ZIP doit souvent avoir un nom spécifique ou simplement .xml
            zip.CreateEntryFromFile(tempXmlPath, Path.GetFileName(tempXmlPath).Replace(".xml", ".xml"))
        End Using

        File.Delete(tempXmlPath)
    End Sub
End Class
