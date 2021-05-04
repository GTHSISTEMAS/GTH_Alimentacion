using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alimentacion
{
    public partial class Corte : Form
    {
        int dias;
        DateTime inicio;
        DateTime fin;
        public Corte()
        {
            InitializeComponent();
        }

        private void Corte_Load(object sender, EventArgs e)
        {
            textBox1.PasswordChar = '\u25CF';
        }

        private void button1_Click(object sender, EventArgs e)
        {
             dias = 0;
            
            if(radioButton1.Checked ==  false && radioButton2.Checked == false && radioButton3.Checked == false)
            {
                MessageBox.Show("Seleccione un rango de dias", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (radioButton1.Checked)
            {
                fin = DateTime.Today;
                inicio = fin.AddDays(-3);
                dias = 3;
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (radioButton3.Checked)
            {
                inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                fin = inicio.AddDays(-1);
                inicio = inicio.AddMonths(-1);
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                inicio = DateTime.Today.Day >= 1 && DateTime.Today.Day < 7 ? inicio.AddMonths(-1) : inicio;
                fin = DateTime.Today;
                dias = 31;
                DialogResult = DialogResult.OK;
                this.Close();
            }
         
            
        }

        public int Dias { get { return dias; } }
        public DateTime Inicio { get { return inicio; } }
        public DateTime Fin { get { return fin; } }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Visible = checkBox1.Checked;
            textBox1.Focus();
            if (checkBox1.Checked == false)
                radioButton3.Visible = false;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if (textBox1.Text.ToUpper() == "HCC123")
                {
                    radioButton3.Visible = true;
                    textBox1.Text = "";
                    textBox1.Visible = false;
                }
            }
        }
    }
}
