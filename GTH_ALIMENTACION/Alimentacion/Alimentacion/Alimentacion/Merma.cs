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
    public partial class Merma : Form
    {
        ConnSIO conn = new ConnSIO();
        Button Button;
        string Numero_Rancho = "";
        public Merma(Button buttonReporte , string numRancho)
        {
            InitializeComponent();
            conn.Iniciar("DBSIE");
            Button = buttonReporte;
            Numero_Rancho = numRancho;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Merma_Load(object sender, EventArgs e)
        {
            DateTime Actual = DateTime.Now;
            DateTime Inicio = new DateTime(Actual.Year, Actual.Month, 1);
            DataTable dt,dtmerma;

            Inicio = Inicio.AddDays(-1);



            string querymerma = @"SELECT Ingrediente as Ingrediente , Ing_Clave as Clave , Por_Merma as '% Merma'
                            FROM merma
                            Order by Ingrediente";




            conn.QueryAlimento(querymerma,out dtmerma);

            if (dtmerma.Rows.Count > 0)
            {
                dgv_Mermas.DataSource = dtmerma;
            }
            else
            {

                string query = @"
                                SELECT  T.Clave
		                               ,P.prod_nombre
	                                   ,'0' as '%Merma'
	                            FROM 
	                            (
					                            SELECT  ing_clave       AS Clave
					                            ,ing_descripcion AS INGREDIENTE
					                            ,SUM(rac_mh)     AS TOTAL
					                            FROM racion
					                            WHERE  ran_id IN ("+ Numero_Rancho + @") 
					                            AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) = 0 
					                            AND SUBSTRING(rac_descripcion,3,2) not in('00','01','02') 
					                            GROUP BY  ing_clave
							                             ,ing_descripcion 
					      UNION
					                            SELECT  T2.Clave
					                                   ,T2.Ing
					                                   ,(T1.Peso * T2.Porc) AS Total
					                            FROM 
					                            (
						                            SELECT  ing_descripcion AS Pmz
						                                   ,SUM(rac_mh)     AS Peso
						                            FROM racion
				
						                            WHERE ran_id IN (" + Numero_Rancho + @") 
						                            AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 
						                            AND SUBSTRING(ing_descripcion, 3, 2) IN ('00', '01', '02') 
						                            AND SUBSTRING(rac_descripcion,3,2) NOT IN ('00','01','02') 
						                            GROUP BY  ing_descripcion
					                            ) T1
					     LEFT JOIN 
					                            (
						                            SELECT  pmez_descripcion AS Pmz
						                                   ,ing_clave        AS Clave
						                                   ,ing_descripcion  AS Ing
						                                   ,pmez_porcentaje  AS Porc
						                            FROM porcentaje_Premezcla 
					                            )T2
					                            ON T1.Pmz = T2.Pmz
					                            ) T

					                            LEFT JOIN(
						                            SELECT 
							                               [prod_clave]
							                              ,prod_nombre
						                              FROM [DBALIMENTO].[dbo].[producto]
					    ) P ON P.prod_clave = T.Clave
					    Where P.prod_clave <> ''
					    group by T.Clave,P.prod_nombre
					    order by T.Clave";
                conn.QueryAlimento(query, out dt);
                dgv_Mermas.DataSource = dt;
                

            }

            dgv_Mermas.ReadOnly = false;
            foreach (DataGridViewColumn dgvc in dgv_Mermas.Columns)
            {
                dgvc.ReadOnly = true;
            }
            dgv_Mermas.Columns[2].ReadOnly = false;
            


        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Lb_ArticulosMerma_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btn_GuardarMerma_Click(object sender, EventArgs e)
        {
            DataTable dt, dtmerma;
            string querymerma = @"SELECT Ingrediente as Ingrediente , Ing_Clave as Clave , Por_Merma as '% Merma'
                            FROM merma
                            Order by Ingrediente";
            conn.QueryAlimento(querymerma, out dtmerma);
            int continuar = 1;
            for (int j = 0; j < dgv_Mermas.Rows.Count; j++)
            {
                if(Convert.ToInt32(dgv_Mermas[2, j].Value) >100)
                {
                    MessageBox.Show("Error en ingrediente "+ dgv_Mermas[0, j].Value.ToString() + " :\nNo puedes registar mas del 100% de Merma.", "ERROR",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    continuar = 0;
                    break;
                }
            }
            if (continuar == 1)
            {
                if (dtmerma.Rows.Count > 0)
                {
                    for (int i = 0; i < dgv_Mermas.Rows.Count; i++)
                    {
                        if (dgv_Mermas[1, i].Value == null || dgv_Mermas[1, i].Value == DBNull.Value || String.IsNullOrWhiteSpace(dgv_Mermas[1, i].Value.ToString()))
                        {

                        }
                        else
                        {

                            conn.UPDATEAlimento("merma", "Por_Merma =" + dgv_Mermas[2, i].Value.ToString(), "WHERE Ing_Clave = '" + dgv_Mermas[1, i].Value.ToString() + "'");
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < dgv_Mermas.Rows.Count; j++)
                    {
                        if (dgv_Mermas[1, j].Value == null || dgv_Mermas[1, j].Value == DBNull.Value || String.IsNullOrWhiteSpace(dgv_Mermas[1, j].Value.ToString()))
                        {

                        }
                        else
                        {

                            string nomb = dgv_Mermas[0, j].Value.ToString() == null ? "" : dgv_Mermas[0, j].Value.ToString();
                            string clave = dgv_Mermas[1, j].Value.ToString() == null ? "" : dgv_Mermas[1, j].Value.ToString();
                            float porcentaje = float.Parse(dgv_Mermas[2, j].Value.ToString()) == 0 ? 0 : float.Parse(dgv_Mermas[2, j].Value.ToString());

                            conn.InsertAlimento("Ingrediente,Ing_Clave,Por_Merma", "merma", "'" + nomb + "','" + clave + "'," + porcentaje);
                        }


                    }
                }
                Button.Enabled = true;

                this.Close();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]

        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void panelBarraTitulo_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgv_Mermas_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgv_Mermas.CurrentCell.ColumnIndex == 2)
            {
                e.Control.KeyPress += new KeyPressEventHandler(dgv_Mermas_KeyPress);
            }
        }

        private void dgv_Mermas_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }
    }
}
