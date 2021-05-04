using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alimentacion
{
    public partial class Correo : Form
    {
        ConnSIO conn = new ConnSIO();
        string ran_nombre, emp_nombre;
        int ran_id, emp_id;
        
        public Correo(int ran_id, string ran_nombre, int emp_id, string emp_nombre)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string correo = textBox1.Text;
            string query;

            if (correo != "")
            {
                bool esCorreo = EsCorreo(correo);
                if (esCorreo)
                {
                    //Validar si ya esta agregado
                    DataTable dt;
                    query = "SELECT * FROM email where em_mail like '" + correo + "'";
                    conn.QueryAlimento(query, out dt);

                    if(dt.Rows.Count == 0)
                    {
                        //id  a insertar
                        DataTable dt1;
                        query = "SELECT ISNULL(MAX(em_id),0) FROM email";
                        conn.QueryAlimento(query, out dt1);
                        int id = Convert.ToInt32(dt1.Rows[0][0]);

                        conn.InsertAlimento("email", id.ToString() + ",'" + correo + "'");

                        //recargar datagrid
                        DataTable dt2;
                        query = "SELECT em_mail AS Correo FROM email";
                        conn.QueryAlimento(query, out dt2);

                        dataGridView1.DataSource = dt2;
                    }
                    else
                    {
                        MessageBox.Show("CORREO YA INGRESADO", "iNFORMACION", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
                    }                   
                }
                else
                {
                    MessageBox.Show("NO ES UN CORREO VALIDO", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
                }
            }
            else
            {
                MessageBox.Show("INGRESE UN CORREO POR FAVOR", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false); 
            }
        }

        private bool EsCorreo(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int index = dataGridView1.CurrentRow.Index;
                string email = dataGridView1[0, index].Value.ToString();
               

                string condicion = "where em_mail like '" + email + "'";
                conn.DeleteAlimento("email", condicion);

                dataGridView1.Rows.RemoveAt(index);
            }
            catch { }
        }

        private void Correo_Load(object sender, EventArgs e)
        {
            conn.Iniciar("DBSIE");
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.AllowUserToAddRows = false;
            FillGrid();
        }

        private void FillGrid()
        {
            DataTable dt;
            string query = "select em_mail AS CORREO FROM email";
            conn.QueryAlimento(query, out dt);

            dataGridView1.DataSource = dt;
        }

        

    }
}
