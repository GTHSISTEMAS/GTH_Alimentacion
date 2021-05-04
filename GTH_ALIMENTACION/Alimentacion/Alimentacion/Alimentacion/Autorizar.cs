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
    public partial class Autorizar : Form
    {
        ConnSIO conn = new ConnSIO();
        int ran_id;
        int emp_id;
        string ran_nombre;
        string emp_nombre;
        public bool Vpwd;
        string pwd;
        public Autorizar(int ran_id, string ran_nombre, int emp_id, string emp_nombre)
        {
            InitializeComponent();
            conn.Iniciar("");
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;

        }

        private void Autorizar_Load(object sender, EventArgs e)
        {
            GetInfo();
            textBox1.PasswordChar = '\u25CF';
        }

        private void GetInfo()
        {
            DataTable dt;
            string query = "Select pas_prorrateo from configuracion where ran_id = " + ran_id.ToString();
            conn.QuerySIO(query, out dt);
            pwd = dt.Rows[0][0].ToString();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pass = textBox1.Text;
            if (pass.ToUpper() == pwd.ToUpper())
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
                    if (textBox1.Text.ToUpper() == pwd.ToUpper())
                    {
                        Vpwd = true;
                        DialogResult = DialogResult.OK;
                    }
                    //else
                    //{
                    //    DialogResult = DialogResult.OK;
                    //    Vpwd = false;
                    //}
                }

            }
        }
    }
}
