namespace VatApp_Net
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblSoc = new System.Windows.Forms.Label();
            this.cmbSoc = new System.Windows.Forms.ComboBox();
            this.lblAnnee = new System.Windows.Forms.Label();
            this.txtAnnee = new System.Windows.Forms.TextBox();
            this.lblPeriod = new System.Windows.Forms.Label();
            this.txtPeriod = new System.Windows.Forms.TextBox();
            this.lblIF = new System.Windows.Forms.Label();
            this.txtIF = new System.Windows.Forms.TextBox();
            this.lblICE = new System.Windows.Forms.Label();
            this.txtICE = new System.Windows.Forms.TextBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.dgvDeductions = new System.Windows.Forms.DataGridView();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDeductions)).BeginInit();
            this.SuspendLayout();

            // lblSoc
            this.lblSoc.Location = new System.Drawing.Point(20, 20);
            this.lblSoc.Size = new System.Drawing.Size(150, 23);
            this.lblSoc.Text = "Société (Base SQL) :";

            // cmbSoc
            this.cmbSoc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSoc.Location = new System.Drawing.Point(180, 20);
            this.cmbSoc.Size = new System.Drawing.Size(200, 23);
            this.cmbSoc.SelectedIndexChanged += new System.EventHandler(this.cmbSoc_SelectedIndexChanged);

            // lblAnnee
            this.lblAnnee.Location = new System.Drawing.Point(20, 60);
            this.lblAnnee.Size = new System.Drawing.Size(150, 23);
            this.lblAnnee.Text = "Année (ex: 2025) :";

            // txtAnnee
            this.txtAnnee.Location = new System.Drawing.Point(180, 60);
            this.txtAnnee.Size = new System.Drawing.Size(100, 23);
            this.txtAnnee.Text = DateTime.Now.Year.ToString();

            // lblPeriod
            this.lblPeriod.Location = new System.Drawing.Point(20, 100);
            this.lblPeriod.Size = new System.Drawing.Size(150, 23);
            this.lblPeriod.Text = "Mois ou Trimestre :";

            // txtPeriod
            this.txtPeriod.Location = new System.Drawing.Point(180, 100);
            this.txtPeriod.Size = new System.Drawing.Size(100, 23);
            this.txtPeriod.PlaceholderText = "ex: 3 ou 1-3";

            // lblIF
            this.lblIF.Location = new System.Drawing.Point(400, 20);
            this.lblIF.Size = new System.Drawing.Size(110, 23);
            this.lblIF.Text = "Identifiant Fiscal :";

            // txtIF
            this.txtIF.Location = new System.Drawing.Point(520, 20);
            this.txtIF.Size = new System.Drawing.Size(150, 23);

            // lblICE
            this.lblICE.Location = new System.Drawing.Point(400, 60);
            this.lblICE.Size = new System.Drawing.Size(110, 23);
            this.lblICE.Text = "ICE Société :";

            // txtICE
            this.txtICE.Location = new System.Drawing.Point(520, 60);
            this.txtICE.Size = new System.Drawing.Size(150, 23);

            // btnPreview
            this.btnPreview.Location = new System.Drawing.Point(20, 140);
            this.btnPreview.Size = new System.Drawing.Size(260, 35);
            this.btnPreview.Text = "AFFICHER LE TABLEAU DE DEDUCTION";
            this.btnPreview.BackColor = System.Drawing.Color.SteelBlue;
            this.btnPreview.ForeColor = System.Drawing.Color.White;
            this.btnPreview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPreview.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);

            // dgvDeductions
            this.dgvDeductions.AllowUserToAddRows = false;
            this.dgvDeductions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDeductions.Location = new System.Drawing.Point(20, 185);
            this.dgvDeductions.Size = new System.Drawing.Size(760, 280);
            this.dgvDeductions.ReadOnly = true;

            // btnGenerate
            this.btnGenerate.Enabled = false;
            this.btnGenerate.Location = new System.Drawing.Point(20, 475);
            this.btnGenerate.Size = new System.Drawing.Size(760, 45);
            this.btnGenerate.Text = "LANCER LA GENERATION DU FICHIER XML (ZIP)";
            this.btnGenerate.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.btnGenerate.ForeColor = System.Drawing.Color.White;
            this.btnGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerate.Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold);
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);

            // lblStatus
            this.lblStatus.Location = new System.Drawing.Point(20, 525);
            this.lblStatus.Size = new System.Drawing.Size(760, 25);
            this.lblStatus.Text = "Sélectionnez une société et une période, puis cliquez sur 'Afficher le tableau'.";
            this.lblStatus.ForeColor = System.Drawing.Color.Gray;

            // Form1
            this.ClientSize = new System.Drawing.Size(950, 700);
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Controls.Add(this.lblSoc);
            this.Controls.Add(this.cmbSoc);
            this.Controls.Add(this.lblAnnee);
            this.Controls.Add(this.txtAnnee);
            this.Controls.Add(this.lblPeriod);
            this.Controls.Add(this.txtPeriod);
            this.Controls.Add(this.lblIF);
            this.Controls.Add(this.txtIF);
            this.Controls.Add(this.lblICE);
            this.Controls.Add(this.txtICE);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.dgvDeductions);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.lblStatus);
            this.Text = "Sage To Moroccan VAT - EDI XML Generator";
            this.MinimumSize = new System.Drawing.Size(950, 700);
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            ((System.ComponentModel.ISupportInitialize)(this.dgvDeductions)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblSoc;
        private System.Windows.Forms.ComboBox cmbSoc;
        private System.Windows.Forms.Label lblAnnee;
        private System.Windows.Forms.TextBox txtAnnee;
        private System.Windows.Forms.Label lblPeriod;
        private System.Windows.Forms.TextBox txtPeriod;
        private System.Windows.Forms.Label lblIF;
        private System.Windows.Forms.TextBox txtIF;
        private System.Windows.Forms.Label lblICE;
        private System.Windows.Forms.TextBox txtICE;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.DataGridView dgvDeductions;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Label lblStatus;
    }
}
