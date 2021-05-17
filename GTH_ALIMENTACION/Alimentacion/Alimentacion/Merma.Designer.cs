
namespace Alimentacion
{
    partial class Merma
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
            this.btn_GuardarMerma = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dgv_Mermas = new System.Windows.Forms.DataGridView();
            this.panelBarraTitulo = new System.Windows.Forms.Panel();
            this.btnCerrar = new System.Windows.Forms.PictureBox();
            this.txtFiltroClave = new System.Windows.Forms.TextBox();
            this.txtFiltroArticulo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Mermas)).BeginInit();
            this.panelBarraTitulo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnCerrar)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_GuardarMerma
            // 
            this.btn_GuardarMerma.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_GuardarMerma.BackColor = System.Drawing.Color.White;
            this.btn_GuardarMerma.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_GuardarMerma.FlatAppearance.BorderSize = 0;
            this.btn_GuardarMerma.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(134)))), ((int)(((byte)(223)))));
            this.btn_GuardarMerma.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_GuardarMerma.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_GuardarMerma.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btn_GuardarMerma.Location = new System.Drawing.Point(193, 378);
            this.btn_GuardarMerma.Name = "btn_GuardarMerma";
            this.btn_GuardarMerma.Size = new System.Drawing.Size(126, 39);
            this.btn_GuardarMerma.TabIndex = 10;
            this.btn_GuardarMerma.Text = "Guardar";
            this.btn_GuardarMerma.UseVisualStyleBackColor = false;
            this.btn_GuardarMerma.Click += new System.EventHandler(this.btn_GuardarMerma_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(166, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(172, 20);
            this.label1.TabIndex = 12;
            this.label1.Text = "Configurar %Merma:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // dgv_Mermas
            // 
            this.dgv_Mermas.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgv_Mermas.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgv_Mermas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Mermas.Location = new System.Drawing.Point(12, 125);
            this.dgv_Mermas.Name = "dgv_Mermas";
            this.dgv_Mermas.ReadOnly = true;
            this.dgv_Mermas.RowHeadersVisible = false;
            this.dgv_Mermas.Size = new System.Drawing.Size(486, 246);
            this.dgv_Mermas.TabIndex = 16;
            this.dgv_Mermas.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgv_Mermas_EditingControlShowing);
            this.dgv_Mermas.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.dgv_Mermas_KeyPress);
            // 
            // panelBarraTitulo
            // 
            this.panelBarraTitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(151)))), ((int)(((byte)(164)))), ((int)(((byte)(176)))));
            this.panelBarraTitulo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBarraTitulo.Controls.Add(this.btnCerrar);
            this.panelBarraTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBarraTitulo.Location = new System.Drawing.Point(0, 0);
            this.panelBarraTitulo.Name = "panelBarraTitulo";
            this.panelBarraTitulo.Size = new System.Drawing.Size(511, 30);
            this.panelBarraTitulo.TabIndex = 17;
            this.panelBarraTitulo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelBarraTitulo_MouseDown);
            // 
            // btnCerrar
            // 
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Image = global::Alimentacion.Properties.Resources.equix;
            this.btnCerrar.Location = new System.Drawing.Point(484, 0);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(25, 25);
            this.btnCerrar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.btnCerrar.TabIndex = 0;
            this.btnCerrar.TabStop = false;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // txtFiltroClave
            // 
            this.txtFiltroClave.Location = new System.Drawing.Point(9, 40);
            this.txtFiltroClave.Name = "txtFiltroClave";
            this.txtFiltroClave.Size = new System.Drawing.Size(100, 20);
            this.txtFiltroClave.TabIndex = 18;
            this.txtFiltroClave.TextChanged += new System.EventHandler(this.txtFiltroClave_TextChanged);
            this.txtFiltroClave.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtFiltroClave_MouseDown);
            // 
            // txtFiltroArticulo
            // 
            this.txtFiltroArticulo.Location = new System.Drawing.Point(132, 40);
            this.txtFiltroArticulo.Name = "txtFiltroArticulo";
            this.txtFiltroArticulo.Size = new System.Drawing.Size(326, 20);
            this.txtFiltroArticulo.TabIndex = 19;
            this.txtFiltroArticulo.TextChanged += new System.EventHandler(this.txtFiltroArticulo_TextChanged);
            this.txtFiltroArticulo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtFiltroArticulo_MouseDown);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(7, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 16);
            this.label2.TabIndex = 20;
            this.label2.Text = "Clave :";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(129, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 16);
            this.label3.TabIndex = 21;
            this.label3.Text = "Nombre :";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtFiltroArticulo);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtFiltroClave);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(21, 56);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(464, 66);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filtro";
            // 
            // Merma
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(98)))), ((int)(((byte)(166)))));
            this.ClientSize = new System.Drawing.Size(511, 425);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panelBarraTitulo);
            this.Controls.Add(this.dgv_Mermas);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_GuardarMerma);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Merma";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Merma";
            this.Load += new System.EventHandler(this.Merma_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Mermas)).EndInit();
            this.panelBarraTitulo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnCerrar)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btn_GuardarMerma;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgv_Mermas;
        private System.Windows.Forms.Panel panelBarraTitulo;
        private System.Windows.Forms.PictureBox btnCerrar;
        private System.Windows.Forms.TextBox txtFiltroClave;
        private System.Windows.Forms.TextBox txtFiltroArticulo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}