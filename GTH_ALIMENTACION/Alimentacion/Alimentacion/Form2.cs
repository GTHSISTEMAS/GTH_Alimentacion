using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alimentacion
{
    public partial class Form2 : Form
    {
        public bool Vpwd;

        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pwd = textBox1.Text;
            if(pwd.ToUpper() == "HCc123")
            {
                Vpwd = true;
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Contraseña incorrecta", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //DialogResult = DialogResult.OK;
                //Vpwd = false;
            }
            
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (textBox1.Text != "")
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (textBox1.Text.ToUpper() == "HCC123")
                    {
                        Vpwd = true;
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        DialogResult = DialogResult.OK;
                        Vpwd = false;
                    }
                    this.Close();
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.PasswordChar = '\u25CF';
        }

       
    }
}
