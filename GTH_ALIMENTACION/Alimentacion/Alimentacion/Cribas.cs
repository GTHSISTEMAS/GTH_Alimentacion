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
    public partial class Cribas : Form
    {
        int ran_id;
        string ran_nombre;
        ConnSIO conn = new ConnSIO();

        public Cribas(int ran_id, string ran_nombre)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
        }

        private void Cribas_Load(object sender, EventArgs e)
        {
            conn.Iniciar("");
            label2.Text = ran_nombre.ToUpper();
            CargarNiveles(DateTime.Now);
        }

        public static int ConvertToJulian(DateTime Date)
        {
            TimeSpan ts = (Date - Convert.ToDateTime("01/01/1900"));
            int julianday = ts.Days + 2;
            return julianday;
        }

        private void CargarNiveles(DateTime fecha)
        {
            DataTable dt;
            int juliana = ConvertToJulian(fecha);
            string query = "SELECT nivel1, nivel2, nivel3, nivel4 from nivelcriba where fecha = " + juliana;
            conn.QueryMovGanado(query, out dt);

            if(dt.Rows.Count == 0)
            {
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                label9.Text = "0";
            }
            else
            {
                textBox1.Text = dt.Rows[0][0].ToString();
                textBox2.Text = dt.Rows[0][1].ToString();
                textBox3.Text = dt.Rows[0][2].ToString();
                textBox4.Text = dt.Rows[0][3].ToString();
                label9.Text = "100";
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // solo 1 punto decimal
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // solo 1 punto decimal
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // solo 1 punto decimal
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // solo 1 punto decimal
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            double n1, n2, n3, n4, suma;
            Double.TryParse(textBox1.Text, out n1);
            Double.TryParse(textBox2.Text, out n2);
            Double.TryParse(textBox3.Text, out n3);
            Double.TryParse(textBox4.Text, out n4);

            suma = n1 + n2 + n3 + n4;
            label9.Text = suma.ToString();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            double n1, n2, n3, n4, suma;
            Double.TryParse(textBox1.Text, out n1);
            Double.TryParse(textBox2.Text, out n2);
            Double.TryParse(textBox3.Text, out n3);
            Double.TryParse(textBox4.Text, out n4);

            suma = n1 + n2 + n3 + n4;
            label9.Text = suma.ToString();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            double n1, n2, n3, n4, suma;
            Double.TryParse(textBox1.Text, out n1);
            Double.TryParse(textBox2.Text, out n2);
            Double.TryParse(textBox3.Text, out n3);
            Double.TryParse(textBox4.Text, out n4);

            suma = n1 + n2 + n3 + n4;
            label9.Text = suma.ToString();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            double n1, n2, n3, n4, suma;
            Double.TryParse(textBox1.Text, out n1);
            Double.TryParse(textBox2.Text, out n2);
            Double.TryParse(textBox3.Text, out n3);
            Double.TryParse(textBox4.Text, out n4);

            suma = n1 + n2 + n3 + n4;
            label9.Text = suma.ToString();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime fecha = Convert.ToDateTime(dateTimePicker1.Value);
            CargarNiveles(fecha);
        }

        private DateTime ConvertToNormalDate(int juliana)
        {
            DateTime fecha = new DateTime(1900, 1, 1);
            fecha = fecha.AddDays(juliana - 2);
            return fecha;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double n1, n2, n3, n4, suma;
            string s;
            int juliana = ConvertToJulian(dateTimePicker1.Value);
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "")
            {
                n1 = Convert.ToDouble(textBox1.Text);
                n2 = Convert.ToDouble(textBox2.Text);
                n3 = Convert.ToDouble(textBox3.Text);
                n4 = Convert.ToDouble(textBox4.Text);
                //Double.TryParse(textBox1.Text, out n1);
                //Double.TryParse(textBox2.Text, out n2);
                //Double.TryParse(textBox3.Text, out n3);
                //Double.TryParse(textBox4.Text, out n4);
                s = (n1 + n2 + n3 + n4).ToString("#,0.0");
                suma = Convert.ToDouble(s);

                if(suma > 100 || suma < 100)                
                    MessageBox.Show("La suma de los niveles debe ser de 100%. Revise la captura.", "ADVERTENCIA!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                {
                    DateTime hoy = DateTime.Today;
                    DateTime ayer = hoy.AddDays(-1);
                    if ((hoy.Date == dateTimePicker1.Value.Date) || ayer.Date == dateTimePicker1.Value.Date)
                    {
                        if (Existe(juliana))
                        {
                            string update = "UPDATE NIVELCRIBA SET nivel1 = " + n1.ToString() + ", nivel2 = " + n2 + ", nivel3 = " + n3 + ", nivel4 = " + n4
                                + " where fecha = " + juliana;
                            conn.UpdateMovsio(update);
                        }
                        else
                        {
                            string insert = "INSERT INTO NIVELCRIBA(ESTABLO, FECHA, NIVEL1, NIVEL2, NIVEL3, NIVEL4) VALUES("
                                + ran_id + "," + juliana + "," + n1 + "," + n2 + "," + n3 + "," + n4 + ")";
                            conn.InsertMovsio(insert);
                        }

                    }
                    else
                        MessageBox.Show("No es posible modificar en esa fecha", "ADVERTENCIA!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Es necesario llenar todos los campos", "ADVERTENCIA!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool Existe(int fecha)
        {
            DataTable dt;
            string query = "Select * From nivelcriba where fecha = " + fecha;
            conn.QueryMovGanado(query, out dt);

            if (dt.Rows.Count > 0)
                return true;
            else
                return false;
        }
    }
}
