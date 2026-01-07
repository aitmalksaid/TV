Namespace Models
    Public Class DeclarationTva
        Public Property IdentifiantFiscal As String
        Public Property ICE As String
        Public Property Periode As Integer
        Public Property Annee As Integer
        Public Property Regime As Integer ' 1 pour mensuel, 2 pour trimestriel
        Public Property Deductions As New List(Of Deduction)
    End Class

    Public Class Deduction
        Public Property NumeroFacture As String
        Public Property DateFacture As Date
        Public Property DesigneeTable As String
        Public Property MontantHT As Decimal
        Public Property TauxTva As Decimal
        Public Property MontantTva As Decimal
        Public Property MontantTTC As Decimal
        Public Property IdFournisseur As String
        Public Property NomFournisseur As String
        Public Property ICEFournisseur As String
        Public Property ModePaiement As Integer ' Code DGI (1: Espèces, 2: Chèque, etc.)
        Public Property DatePaiement As Date
    End Class
End Namespace
