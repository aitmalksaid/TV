using System;
using System.Collections.Generic;

namespace VatApp_Net.Models
{
    public class DeclarationTva
    {
        public string IdentifiantFiscal { get; set; } = "";
        public string ICE { get; set; } = "";
        public int Annee { get; set; }
        public int Periode { get; set; }
        public int Regime { get; set; } // 1: Mensuel, 2: Trimestriel
        public List<Deduction> Deductions { get; set; } = new List<Deduction>();
    }

    public class Deduction
    {
        public int Ordre { get; set; }
        public string NumeroFacture { get; set; } = "";
        public string NomFournisseur { get; set; } = "";
        public decimal MontantHT { get; set; }
        public decimal MontantTva { get; set; }
        public decimal MontantTTC { get; set; }
        public string ReferenceEcriture { get; set; } = "";
        public decimal TauxTva { get; set; }
        public int ModePaiement { get; set; }
        public DateTime DatePaiement { get; set; }
        public DateTime DateFacture { get; set; }
        public string IF_Fournisseur { get; set; } = "";
        public string ICE_Fournisseur { get; set; } = "";
    }
}
