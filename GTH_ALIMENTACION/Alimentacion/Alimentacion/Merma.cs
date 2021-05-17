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
        DataTable dtmerma;
        string filterField = "Clave";
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
            DataTable dt;

            Inicio = Inicio.AddDays(-1);



            string querymerma = @"
             SELECT 
                I.Clave as Clave,
                P.prod_nombre AS Ingrediente,
                IIF(M.Por_Merma is NULL, 0 , M.Por_Merma) AS '% Merma',
                IIF(M.Por_Extra is NULL, 0 , M.Por_Extra) AS '% Extra'
                FROM(
                    SELECT  ing_clave       AS Clave
					,ing_descripcion AS INGREDIENTE
					,SUM(rac_mh)     AS TOTAL
					FROM racion
					WHERE 
					 ran_id IN (" + Numero_Rancho + @")

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
				
						WHERE 
						ran_id IN (" + Numero_Rancho + @")  
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
					UNION
					SELECT DISTINCT [ing_clave]
					,[ing_descripcion]
					,0
					FROM [DBALIMENTO].[dbo].[ingrediente] where ran_id IN (" + Numero_Rancho + @")
  
                  ) I
                LEFT JOIN(
                   SELECT [prod_clave]
		                  ,prod_nombre
                   FROM [DBALIMENTO].[dbo].[producto]
                ) P ON P.prod_clave = I.Clave
                LEFT JOIN(
	                SELECT 
                       [Ingrediente]
                      ,[Por_Merma]
                      ,[Por_Extra]
                    FROM [DBALIMENTO].[dbo].[merma]
                ) M ON M.Ingrediente = I.Clave
                WHERE I.Clave <> ''
                group by I.Clave,P.prod_nombre,M.Por_Merma,M.Por_Extra";




            conn.QueryAlimento(querymerma, out dtmerma);
            dgv_Mermas.DataSource = dtmerma;
            dgv_Mermas.ReadOnly = false;
            foreach (DataGridViewColumn dgvc in dgv_Mermas.Columns)
            {
                dgvc.ReadOnly = true;
            }
            dgv_Mermas.Columns[2].ReadOnly = false;
            dgv_Mermas.Columns[3].ReadOnly = false;




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
                if (Convert.ToInt32(dgv_Mermas[2, j].Value) > 100)
                {
                    MessageBox.Show("Error en ingrediente " + dgv_Mermas[0, j].Value.ToString() + " :\nNo puedes registar mas del 100% de Merma.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continuar = 0;
                    break;
                }
            }

            if (continuar == 1)
            {

                for (int j = 0; j < dgv_Mermas.Rows.Count - 1; j++)
                {
                    if (dgv_Mermas[0, j].Value == null || dgv_Mermas[0, j].Value == DBNull.Value || String.IsNullOrWhiteSpace(dgv_Mermas[0, j].Value.ToString()))
                    {

                    }
                    else
                    {
                        DataTable dtmermaCreada;
                        string querymermaCreada = @"SELECT Ingrediente as Ingrediente , Ing_Clave as Clave , Por_Merma as '% Merma'
                            FROM merma WHERE Ingrediente = '" + dgv_Mermas[0, j].Value.ToString() + @"'
                            Order by Ingrediente";
                        conn.QueryAlimento(querymermaCreada, out dtmermaCreada);

                        if (dtmermaCreada.Rows.Count > 0)
                        {
                            if (dgv_Mermas[1, j].Value == null || dgv_Mermas[1, j].Value == DBNull.Value || String.IsNullOrWhiteSpace(dgv_Mermas[1, j].Value.ToString()))
                            {

                            }
                            else
                            {

                                conn.UPDATEAlimento("merma", "Por_Merma =" + dgv_Mermas[2, j].Value.ToString()+ ", Por_Extra = " + dgv_Mermas[3, j].Value.ToString(), "WHERE Ing_Clave = '" + dgv_Mermas[1, j].Value.ToString() + "'");
                            }
                        }
                        else
                        {
                            if (dgv_Mermas[1, j].Value == null || dgv_Mermas[1, j].Value == DBNull.Value || String.IsNullOrWhiteSpace(dgv_Mermas[1, j].Value.ToString()))
                            {

                            }
                            else
                            {

                                string nomb = dgv_Mermas[0, j].Value.ToString() == null ? "" : dgv_Mermas[0, j].Value.ToString();
                                string clave = dgv_Mermas[1, j].Value.ToString() == null ? "" : dgv_Mermas[1, j].Value.ToString();
                                float porcentaje = float.Parse(dgv_Mermas[2, j].Value.ToString()) == 0 ? 0 : float.Parse(dgv_Mermas[2, j].Value.ToString());
                                float extra = float.Parse(dgv_Mermas[3, j].Value.ToString()) == 0 ? 0 : float.Parse(dgv_Mermas[3, j].Value.ToString());

                                conn.InsertAlimento("Ingrediente,Ing_Clave,Por_Merma,Por_Extra", "merma", "'" + nomb + "','" + clave + "'," + porcentaje+","+extra);
                            }

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

        private void txtFiltroClave_TextChanged(object sender, EventArgs e)
        {
            dtmerma.DefaultView.RowFilter = $"{ dgv_Mermas.Columns[0].HeaderText} like '%{txtFiltroClave.Text}%'";
        }

        private void txtFiltroArticulo_TextChanged(object sender, EventArgs e)
        {
            dtmerma.DefaultView.RowFilter = $"{ dgv_Mermas.Columns[1].HeaderText} like '%{txtFiltroArticulo.Text}%'";
        }

        private void txtFiltroClave_MouseDown(object sender, MouseEventArgs e)
        {
            txtFiltroClave.Text = "";
        }

        private void txtFiltroArticulo_MouseDown(object sender, MouseEventArgs e)
        {
            txtFiltroArticulo.Text = "";
        }
    }
}
