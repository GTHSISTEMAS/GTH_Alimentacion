namespace Alimentacion
{
    partial class Consumo_Por_Ingrediente
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpInicial = new System.Windows.Forms.DateTimePicker();
            this.dtpFinal = new System.Windows.Forms.DateTimePicker();
            this.reportViewer1 = new Microsoft.Reporting.WinForms.ReportViewer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.reportViewer2 = new Microsoft.Reporting.WinForms.ReportViewer();
            this.cbEmpresa = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.ChBoxForraje = new System.Windows.Forms.CheckBox();
            this.ChBox_Alimento = new System.Windows.Forms.CheckBox();
            this.clbRanchos = new System.Windows.Forms.CheckedListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(34, 173);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Fecha Inicial:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(34, 229);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Fecha Final:";
            // 
            // dtpInicial
            // 
            this.dtpInicial.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.dtpInicial.Location = new System.Drawing.Point(153, 172);
            this.dtpInicial.Name = "dtpInicial";
            this.dtpInicial.Size = new System.Drawing.Size(256, 20);
            this.dtpInicial.TabIndex = 2;
            this.dtpInicial.Value = new System.DateTime(2021, 5, 1, 0, 0, 0, 0);
            // 
            // dtpFinal
            // 
            this.dtpFinal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.dtpFinal.Location = new System.Drawing.Point(153, 228);
            this.dtpFinal.Name = "dtpFinal";
            this.dtpFinal.Size = new System.Drawing.Size(256, 20);
            this.dtpFinal.TabIndex = 3;
            this.dtpFinal.Value = new System.DateTime(2021, 5, 4, 0, 0, 0, 0);
            // 
            // reportViewer1
            // 
            this.reportViewer1.LocalReport.ReportEmbeddedResource = "Alimentacion.Reporte_Proyeccion.rdlc";
            this.reportViewer1.Location = new System.Drawing.Point(0, 0);
            this.reportViewer1.Name = "reportViewer1";
            this.reportViewer1.ServerReport.BearerToken = null;
            this.reportViewer1.Size = new System.Drawing.Size(396, 246);
            this.reportViewer1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.reportViewer2);
            this.panel1.Controls.Add(this.cbEmpresa);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(565, 494);
            this.panel1.TabIndex = 5;
            // 
            // reportViewer2
            // 
            this.reportViewer2.DocumentMapWidth = 60;
            this.reportViewer2.LocalReport.ReportEmbeddedResource = "Alimentacion.Reporte_Ingredientes.rdlc";
            this.reportViewer2.Location = new System.Drawing.Point(-37, -23);
            this.reportViewer2.Name = "reportViewer2";
            this.reportViewer2.ServerReport.BearerToken = null;
            this.reportViewer2.Size = new System.Drawing.Size(62, 39);
            this.reportViewer2.TabIndex = 0;
            this.reportViewer2.Visible = false;
            // 
            // cbEmpresa
            // 
            this.cbEmpresa.AutoSize = true;
            this.cbEmpresa.Location = new System.Drawing.Point(39, 26);
            this.cbEmpresa.Name = "cbEmpresa";
            this.cbEmpresa.Size = new System.Drawing.Size(67, 17);
            this.cbEmpresa.TabIndex = 6;
            this.cbEmpresa.Text = "Empresa";
            this.cbEmpresa.UseVisualStyleBackColor = true;
            this.cbEmpresa.Visible = false;
            this.cbEmpresa.CheckedChanged += new System.EventHandler(this.cbEmpresa_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.button3);
            this.panel2.Controls.Add(this.ChBoxForraje);
            this.panel2.Controls.Add(this.ChBox_Alimento);
            this.panel2.Controls.Add(this.clbRanchos);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.dtpFinal);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.dtpInicial);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Location = new System.Drawing.Point(39, 49);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(472, 390);
            this.panel2.TabIndex = 5;
            // 
            // button3
            // 
            this.button3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(80)))), ((int)(((byte)(200)))));
            this.button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button3.FlatAppearance.BorderSize = 0;
            this.button3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(134)))), ((int)(((byte)(223)))));
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.button3.Location = new System.Drawing.Point(347, 306);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(105, 66);
            this.button3.TabIndex = 11;
            this.button3.Text = "Generar Reporte";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // ChBoxForraje
            // 
            this.ChBoxForraje.AutoSize = true;
            this.ChBoxForraje.Location = new System.Drawing.Point(256, 355);
            this.ChBoxForraje.Name = "ChBoxForraje";
            this.ChBoxForraje.Size = new System.Drawing.Size(58, 17);
            this.ChBoxForraje.TabIndex = 10;
            this.ChBoxForraje.Text = "Forraje";
            this.ChBoxForraje.UseVisualStyleBackColor = true;
            // 
            // ChBox_Alimento
            // 
            this.ChBox_Alimento.AutoSize = true;
            this.ChBox_Alimento.Location = new System.Drawing.Point(256, 306);
            this.ChBox_Alimento.Name = "ChBox_Alimento";
            this.ChBox_Alimento.Size = new System.Drawing.Size(66, 17);
            this.ChBox_Alimento.TabIndex = 9;
            this.ChBox_Alimento.Text = "Alimento";
            this.ChBox_Alimento.UseVisualStyleBackColor = true;
            this.ChBox_Alimento.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // clbRanchos
            // 
            this.clbRanchos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.clbRanchos.CheckOnClick = true;
            this.clbRanchos.FormattingEnabled = true;
            this.clbRanchos.Location = new System.Drawing.Point(153, 39);
            this.clbRanchos.Name = "clbRanchos";
            this.clbRanchos.Size = new System.Drawing.Size(256, 109);
            this.clbRanchos.TabIndex = 7;
            this.clbRanchos.Visible = false;
            this.clbRanchos.SelectedIndexChanged += new System.EventHandler(this.clbRanchos_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(34, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Establo:";
            this.label3.Visible = false;
            // 
            // Consumo_Por_Ingrediente
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(565, 494);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimizeBox = false;
            this.Name = "Consumo_Por_Ingrediente";
            this.Text = "Consumo_Por_Ingrediente";
            this.Load += new System.EventHandler(this.Consumo_Por_Ingrediente_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpInicial;
        private System.Windows.Forms.DateTimePicker dtpFinal;
        private Microsoft.Reporting.WinForms.ReportViewer reportViewer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox cbEmpresa;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox clbRanchos;
        private System.Windows.Forms.CheckBox ChBoxForraje;
        private System.Windows.Forms.CheckBox ChBox_Alimento;
        private System.Windows.Forms.Button button3;
        private Microsoft.Reporting.WinForms.ReportViewer reportViewer2;
    }
}