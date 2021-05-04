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
    public partial class PreciosTeoricos : Form
    {
        ConnSIO conn = new ConnSIO();
        int ran_id;
        string ran_nombre;

        public PreciosTeoricos(int ran_id, string ran_nombre)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
        }

        private void PreciosTeoricos_Load(object sender, EventArgs e)
        {
            conn.Iniciar("");
            label1.Text = ran_nombre.ToUpper();
            CargarPrecios(ConvertToJulian(DateTime.Now));
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

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
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

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
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

        private void textBox7_KeyPress(object sender, KeyPressEventArgs e)
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

        private void CargarPrecios(int juliana)
        {
            DataTable dt;
            string query = "SELECT PRODUCCION, RETO, SECAS, PREÑADAS, BECERRAS2, BECERRAS1, MS FROM PRECIOSTEORICOS WHERE FECHA = " + juliana;
            conn.QueryMovGanado(query, out dt);

            if(dt.Rows.Count > 0)
            {
                textBox1.Text = dt.Rows[0][0].ToString();                
                textBox2.Text = dt.Rows[0][1].ToString();
                textBox3.Text = dt.Rows[0][2].ToString();
                textBox4.Text = dt.Rows[0][3].ToString();
                textBox5.Text = dt.Rows[0][4].ToString();
                textBox6.Text = dt.Rows[0][5].ToString();
                textBox7.Text = dt.Rows[0][6].ToString();
            }
            else
            {
                textBox1.Text = "0";
                textBox2.Text = "0";
                textBox3.Text = "0";
                textBox4.Text = "0";
                textBox5.Text = "0";
                textBox6.Text = "0";
                textBox7.Text = "0";
            }
        }
        public static int ConvertToJulian(DateTime Date)
        {
            TimeSpan ts = (Date - Convert.ToDateTime("01/01/1900"));
            int julianday = ts.Days + 2;
            return julianday;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime fecha = Convert.ToDateTime(dateTimePicker1.Value);
            CargarPrecios(ConvertToJulian(fecha));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double produccion, reto, secas, vp, b7, b2, ms;
            int juliana = ConvertToJulian(dateTimePicker1.Value);
            Double.TryParse(textBox1.Text, out produccion);
            Double.TryParse(textBox2.Text, out reto);
            Double.TryParse(textBox3.Text, out secas);
            Double.TryParse(textBox4.Text, out vp);
            Double.TryParse(textBox5.Text, out b7);
            Double.TryParse(textBox6.Text, out b2);
            Double.TryParse(textBox7.Text, out ms);
            DateTime hoy = DateTime.Today;
            DateTime ayer = hoy.AddDays(-1);

            if (hoy.Date == dateTimePicker1.Value.Date || ayer.Date == dateTimePicker1.Value.Date)
            {
                if (Existe(juliana))
                {
                    string update = "UPDATE PRECIOSTEORICOS SET PRODUCCION = " + produccion.ToString() + ", RETO = " + reto + ", SECAS = " + secas
                        + ", PREÑADAS = " + vp + ", BECERRAS2 = " + b7 + ", BECERRAS1 = " + b2;
                    conn.UpdateMovsio(update);

                    textBox1.Text = produccion.ToString();
                    textBox2.Text = reto.ToString();
                    textBox3.Text = secas.ToString();
                    textBox4.Text = vp.ToString();
                    textBox5.Text = b7.ToString();
                    textBox6.Text = b2.ToString();
                    textBox7.Text = ms.ToString();
                }
                else
                {
                    string insert = "INSERT INTO PRECIOSTEORICOS(ESTABLO, FECHA, PRODUCCION, RETO, SECAS, PREÑADAS, BECERRAS2, BECERRAS1, MS) "
                        + " VALUES (" + ran_id.ToString() + "," + juliana + "," + produccion + "," + reto + "," + secas + "," + vp + "," + b7 + "," + b2 + "," + ms + ")";
                    conn.InsertMovsio(insert);

                    textBox1.Text = produccion.ToString();
                    textBox2.Text = reto.ToString();
                    textBox3.Text = secas.ToString();
                    textBox4.Text = vp.ToString();
                    textBox5.Text = b7.ToString();
                    textBox6.Text = b2.ToString();
                    textBox7.Text = ms.ToString();
                }
            }
            else
                MessageBox.Show("No es posible modificar en esa fecha", "ADVERTENCIA!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
        }

        private bool Existe(int juliana)
        {
            DataTable dt;
            string query = "SELECT * FROM PRECIOSTEORICOS WHERE FECHA = " + juliana;
            conn.QueryMovGanado(query, out dt);

            if (dt.Rows.Count > 0)
                return true;
            else
                return false;
        }
    }
}
