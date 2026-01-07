Imports System
Imports System.IO
Imports SageToVatXml.Models

Module Program
    Sub Main(args As String())
        Console.WriteLine("=== Sage 100 vers XML TVA Maroc (EDI) ===")
        Console.WriteLine("-----------------------------------------")

        ' Configuration directe selon l'image SSMS
        Dim server = "DESKTOP-GA5VJ5I\SAGE100"
        Dim database = "BIJOU"

        Console.WriteLine($"Connexion au serveur : {server}")
        Console.WriteLine($"Base de données : {database}")
        Console.WriteLine("Mode : Authentification Windows")

        Dim connectionString = $"Server={server};Database={database};Integrated Security=True;TrustServerCertificate=True;"

        ' Paramètres de déclaration avec boucle robuste
        Dim annee As Integer
        Dim moisStr As String
        Dim moisDebut As Integer
        Dim moisFin As Integer
        Dim periode As Integer
        Dim regime As Integer = 1 ' 1: Mensuel, 2: Trimestriel

        While True
            Try
                Console.Write("Année (ex: 2025) : ")
                annee = Integer.Parse(Console.ReadLine())
                Exit While
            Catch
                Console.WriteLine("Format d'année invalide.")
            End Try
        End While

        While True
            Console.Write("Mois (1-12) ou Trimestre (1-3, 4-6, 7-9, 10-12) : ")
            moisStr = Console.ReadLine().Trim()
            
            If moisStr.Contains("-") Then
                ' Format Trimestre (ex: 1-3)
                Try
                    Dim parts = moisStr.Split("-"c)
                    moisDebut = Integer.Parse(parts(0))
                    moisFin = Integer.Parse(parts(1))
                    regime = 2
                    ' Calcul du code trimestre pour la DGI (1, 2, 3 ou 4)
                    periode = Math.Ceiling(moisDebut / 3) 
                    Exit While
                Catch
                    Console.WriteLine("Format de trimestre invalide (ex: 1-3).")
                End Try
            Else
                ' Format Mensuel
                Try
                    periode = Integer.Parse(moisStr)
                    moisDebut = periode
                    moisFin = periode
                    regime = 1
                    Exit While
                Catch
                    Console.WriteLine("Format de mois invalide.")
                End Try
            End If
        End While

        Dim ifSocDefault As String = ""
        Dim iceSocDefault As String = ""

        Try
            Dim scannerTmp As New SageScanner(connectionString)
            Dim info = scannerTmp.GetCompanyInfo()
            ifSocDefault = info.IFSoc
            iceSocDefault = info.ICESoc
        Catch
            ' Ignorer si erreur
        End Try

        Console.Write($"Identifiant Fiscal Société [{ifSocDefault}] : ")
        Dim ifSoc = Console.ReadLine()
        If String.IsNullOrEmpty(ifSoc) Then ifSoc = ifSocDefault

        Console.Write($"ICE Société [{iceSocDefault}] : ")
        Dim iceSoc = Console.ReadLine()
        If String.IsNullOrEmpty(iceSoc) Then iceSoc = iceSocDefault

        Try
            Dim scanner As New SageScanner(connectionString)
            Dim dateDebut As New DateTime(annee, moisDebut, 1)
            Dim dateFin As DateTime = New DateTime(annee, moisFin, 1).AddMonths(1).AddDays(-1)

            Console.WriteLine($"Recherche des écritures lettrées du {dateDebut:dd/MM/yyyy} au {dateFin:dd/MM/yyyy}...")
            
            Dim deductions = scanner.GetDeductions(dateDebut, dateFin)

            Console.WriteLine($"{deductions.Count} déductions trouvées.")

            If deductions.Count > 0 Then
                Dim decla As New DeclarationTva With {
                    .Annee = annee,
                    .Periode = periode,
                    .IdentifiantFiscal = ifSoc,
                    .ICE = iceSoc,
                    .Regime = regime,
                    .Deductions = deductions
                }

                Dim xmlGen As New XmlGenerator()
                Dim suffix = If(regime = 1, $"M{periode}", $"T{periode}")
                Dim outputZippath = Path.Combine(Directory.GetCurrentDirectory(), $"TVA_{annee}_{suffix}.zip")
                
                xmlGen.GenerateZip(decla, outputZippath)

                Console.WriteLine("-----------------------------------------")
                Console.WriteLine("Génération réussie !")
                Console.WriteLine("Fichier créé : " & outputZippath)
            End If

        Catch ex As Exception
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine("ERREUR : " & ex.Message)
            Console.ResetColor()
        End Try

        Console.WriteLine("Appuyez sur une touche pour quitter...")
        Console.ReadKey()
    End Sub
End Module

