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
    public partial class Etapa : Form
    {
        ConnSIO conn = new ConnSIO();
        int emp_id;
        int ran_id;
        bool empresa;
        string establos = "";
        int tipo;

        public Etapa(int ran_id, int emp_id, bool empresa, int tipo)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.emp_id = emp_id;
            this.empresa = empresa;
            this.tipo = tipo;
        }
     
        private String EstablosEmpresa(int emp_id)
        {
            string establos = "";
            DataTable dt;
            string query = "SELECT ran_id FROM[DBSIO].[dbo].configuracion WHERE emp_id = " + emp_id.ToString();
            conn.QueryAlimento(query, out dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                establos += dt.Rows[i][0] + ",";
            }
            establos = establos.Remove(establos.Length-1, 1);
            return establos;
        } 


        private DataTable LlenarEstablos()
        {
            DataTable dt;
            string query = "SELECT IIF(ran_id < 10,  CONCAT('0',ran_id) , CONCAT('',ran_id))  AS ID, ran_desc AS Rancho FROM[DBSIO].[dbo].configuracion where emp_id = "  + emp_id;
            conn.QueryAlimento(query, out dt);
            return dt;
            
        }

        private void LlenarCorrales()
        {          
            DataTable dt;
            string query = "SELECT CASE etp_id "
                        + " WHEN 11 THEN CONCAT('" + cbEstablo.SelectedValue.ToString() + "','11 LACTANCIA 1') "
                        + " WHEN 12 THEN CONCAT('" + cbEstablo.SelectedValue.ToString() + "','12 LACTANCIA 2') "
                        + " WHEN 13 THEN CONCAT('" + cbEstablo.SelectedValue.ToString() + "','13 LACTANCIA 3') "
                        + " WHEN 21 THEN CONCAT('" + cbEstablo.SelectedValue.ToString() + "','21 SECAS') "
                        + " WHEN 22 THEN CONCAT('" + cbEstablo.SelectedValue.ToString() + "','22 RETO') "
                        + " WHEN 31 THEN CONCAT('" + cbEstablo.SelectedValue.ToString() + "','31 JAULAS') "
                        + " WHEN 32 THEN CONCAT('" + cbEstablo.SelectedValue.ToString() + "','32 DESTETE I') "
                        + " WHEN 33 THEN CONCAT('" + cbEstablo.SelectedValue.ToString() + "','33 DESTETE II') "
                        + " WHEN 34 THEN CONCAT('" + cbEstablo.SelectedValue.ToString() + "','34 VAQUILLAS PREÑADAS') "
                        + " END AS CODIGO "
                        + " FROM etapa where etp_id IN(11,12,13,21,22,31,32,33,34)";
            conn.QueryAlimento(query, out dt);

            dataGridView1.DataSource = dt;

        }     

        public string textoCorrales { get; set; }

        public List<string> corrales = new List<string>();

        public string textoEstablo { get; set; }

        private void cbEstablo_SelectedIndexChanged(object sender, EventArgs e)
        {
            LlenarCorrales();
        }

        private void Etapa_Load_1(object sender, EventArgs e)
        {
            conn.Iniciar("DBSIE");
            if (empresa)
                establos = EstablosEmpresa(emp_id);
            else
                establos = ran_id > 9 ? ran_id.ToString() : "0" + ran_id.ToString();

            cbEstablo.DataSource = Establos(tipo);
            cbEstablo.DisplayMember = "Rancho";
            cbEstablo.ValueMember = "ID";

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            LlenarCorrales();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            
            foreach(DataGridViewRow row in dataGridView1.Rows)
            {
                try
                {
                    if (row.Cells[0].Value != null)
                    {
                        if ((bool)row.Cells[0].Value == true)
                        {
                            textoCorrales = Convert.ToString(row.Cells[1].Value + "," + textoCorrales);
                            corrales.Add(Convert.ToString(row.Cells[1].Value));
                        }
                    }
                }
                catch { }

                textoEstablo = cbEstablo.SelectedValue.ToString();
                DialogResult = DialogResult.OK;
                this.Close();
            }
            try
            {
                textoCorrales = textoCorrales.Remove(textoCorrales.Length - 1, 1);

            }
            catch { }

            textoEstablo = cbEstablo.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
            this.Close();
            
        }

        public bool BoolEmpresa { get; set; }

        private DataTable Establos(int tipo)
        {
            DataTable dt;
            string query = "";
            if (tipo == 2)
                query = "SELECT IIF(ran_id < 10,  CONCAT('0',ran_id) , CONCAT('',ran_id)) AS ID, ran_desc AS RANCHO FROM configuracion WHERE emp_prorrateo = ( SELECT cr.emp_id FROM configuracion c LEFT JOIN configuracion_rancho cr ON c.ran_id = cr.ran_id where c.ran_id = " + ran_id + ")";
            else if (tipo == 3)
                query = "SELECT IIF(ran_id < 10,  CONCAT('0',ran_id) , CONCAT('',ran_id)) AS ID, ran_desc AS RANCHO FROM configuracion WHERE emp_id = ( SELECT cr.cr_multiempresa FROM configuracion c LEFT JOIN configuracion_rancho cr ON c.ran_id = cr.ran_id where c.ran_id = " + ran_id + ")";
            else
                query = "SELECT IIF(ran_id < 10,  CONCAT('0',ran_id) , CONCAT('',ran_id)) AS ID, ran_desc AS RANCHO FROM configuracion WHERE ran_id = " + ran_id;
            conn.QuerySIO(query, out dt);

            return dt;
        }
    }
}
