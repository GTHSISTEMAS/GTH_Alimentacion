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
    public partial class Corral : Form
    {
        ConnSIO conn = new ConnSIO();
        int ran_id;
        string ran_numero;
                                                   
        public Corral(int ran_id)
        {
            InitializeComponent();
            this.ran_id = ran_id;
        }

        private void Corral_Load(object sender, EventArgs e)
        {
            conn.Iniciar();
            ran_numero = ran_id > 9 ? "'" + ran_id.ToString() + "'" : "'0" + ran_id.ToString() + "'";
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.DataSource = Corrales();
        }

        private DataTable Corrales()
        {
            DataTable dt;
            string query = "select cor_id from corral where ran_id = " + ran_id;
            conn.QueryAlimento(query, out dt);
            return dt;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            textoCorrales = "";
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                try
                {
                    if (row.Cells[0].Value != null)
                    {
                        if ((bool)row.Cells[0].Value == true)
                        {
                            textoCorrales += row.Cells[1].Value.ToString() + ",";
                            //textoCorrales = Convert.ToString(row.Cells[1].Value + "," + textoCorrales);
                            corrales.Add(Convert.ToString(row.Cells[1].Value));
                        }
                    }
                }
                catch { }
                DialogResult = DialogResult.OK;
                this.Close();
            }

            try
            {
                textoCorrales = textoCorrales.Remove(textoCorrales.Length - 1, 1);
            }
            catch { }
            DialogResult = DialogResult.OK;
            this.Close();
        }

        public string textoCorrales { get; set; }

        public List<string> corrales = new List<string>();


    }
}
