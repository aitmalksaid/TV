using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VatApp_Net.Models;

namespace VatApp_Net
{
    public partial class Form1 : Form
    {
        private SageScanner scanner;
        private List<Deduction> currentDeductions;
        private int anneeGlobal;
        private int periodeGlobal;
        private int regimeGlobal;
        private XmlGenerator xmlGen = new XmlGenerator();

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                scanner = new SageScanner(@"DESKTOP-GA5VJ5I\SAGE100", "BIJOU");
                var dbs = scanner.GetDatabases();
                cmbSoc.Items.Clear();
                foreach (var db in dbs) cmbSoc.Items.Add(db);
                if (cmbSoc.Items.Count > 0) cmbSoc.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement bases : " + ex.Message);
            }
        }

        private void cmbSoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSoc.SelectedItem == null) return;
            string db = cmbSoc.SelectedItem.ToString();
            try
            {
                var config = scanner.GetVatConfig(db);
                txtIF.Text = config.IFSoc;
                txtICE.Text = config.ICESoc;
            }
            catch
            {
                txtIF.Text = "";
                txtICE.Text = "";
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "Recherche des écritures...";
                lblStatus.ForeColor = Color.Blue;
                this.Cursor = Cursors.WaitCursor;
                dgvDeductions.DataSource = null;
                btnGenerate.Enabled = false;

                if (!int.TryParse(txtAnnee.Text, out anneeGlobal)) throw new Exception("Année invalide.");

                string moisStr = txtPeriod.Text.Trim();
                int moisDebut, moisFin;

                if (moisStr.Contains("-"))
                {
                    var parts = moisStr.Split('-');
                    moisDebut = int.Parse(parts[0]);
                    moisFin = int.Parse(parts[1]);
                    regimeGlobal = 2;
                    periodeGlobal = (int)Math.Ceiling(moisDebut / 3.0);
                }
                else
                {
                    periodeGlobal = int.Parse(moisStr);
                    moisDebut = periodeGlobal;
                    moisFin = periodeGlobal;
                    regimeGlobal = 1;
                }

                string db = cmbSoc.SelectedItem.ToString();
                var taskScanner = new SageScanner(@"DESKTOP-GA5VJ5I\SAGE100", db);
                DateTime start = new DateTime(anneeGlobal, moisDebut, 1);
                DateTime end = new DateTime(anneeGlobal, moisFin, 1).AddMonths(1).AddDays(-1);

                currentDeductions = taskScanner.GetDeductions(start, end);

                if (currentDeductions.Count == 0)
                {
                    lblStatus.Text = "Aucune déduction trouvée.";
                    lblStatus.ForeColor = Color.Orange;
                    return;
                }

                lblStatus.Text = $"{currentDeductions.Count} déductions trouvées.";
                lblStatus.ForeColor = Color.Green;

                dgvDeductions.DataSource = currentDeductions.Select(d => new {
                    Ordre = d.Ordre,
                    Facture = d.NumeroFacture,
                    Date = d.DateFacture.ToString("dd/MM/yyyy"),
                    Fournisseur = d.NomFournisseur,
                    HT = d.MontantHT,
                    TVA = d.MontantTva,
                    TTC = d.MontantTTC,
                    Taux = d.TauxTva + "%",
                    Paiement = d.DatePaiement.ToString("dd/MM/yyyy")
                }).ToList();

                // Formatage des colonnes
                dgvDeductions.Columns["HT"].DefaultCellStyle.Format = "N2";
                dgvDeductions.Columns["TVA"].DefaultCellStyle.Format = "N2";
                dgvDeductions.Columns["TTC"].DefaultCellStyle.Format = "N2";
                dgvDeductions.Columns["HT"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvDeductions.Columns["TVA"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvDeductions.Columns["TTC"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                dgvDeductions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                btnGenerate.Enabled = true;
                scanner.SaveVatConfig(db, txtIF.Text, txtICE.Text);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "ERREUR : " + ex.Message;
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("Erreur : " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentDeductions == null || currentDeductions.Count == 0) return;

                lblStatus.Text = "Génération du ZIP...";
                lblStatus.ForeColor = Color.Blue;

                var decla = new DeclarationTva {
                    Annee = anneeGlobal,
                    Periode = periodeGlobal,
                    Regime = regimeGlobal,
                    IdentifiantFiscal = txtIF.Text,
                    ICE = txtICE.Text,
                    Deductions = currentDeductions
                };

                string db = cmbSoc.SelectedItem.ToString();
                string suffix = regimeGlobal == 1 ? $"M{periodeGlobal}" : $"T{periodeGlobal}";
                string fileName = $"TVA_{db}_{anneeGlobal}_{suffix}.zip";

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.FileName = fileName;
                    sfd.Filter = "Fichiers ZIP (*.zip)|*.zip";
                    sfd.Title = "Enregistrer la déclaration TVA";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string outputPath = sfd.FileName;
                        xmlGen.GenerateZip(decla, outputPath);

                        lblStatus.Text = "Fichier XML (ZIP) généré avec succès !";
                        lblStatus.ForeColor = Color.Green;
                        MessageBox.Show("Déclaration générée avec succès :\n" + outputPath, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        lblStatus.Text = "Génération annulée.";
                        lblStatus.ForeColor = Color.Orange;
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erreur génération : " + ex.Message;
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }
    }
}
