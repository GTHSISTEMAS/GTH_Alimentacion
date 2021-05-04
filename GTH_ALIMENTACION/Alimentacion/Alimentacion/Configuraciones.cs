using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alimentacion
{
    public partial class Configuraciones : Form
    {
        ConnSIO conn = new ConnSIO();
        int ran_id;
        string ran_nombre;

        public Configuraciones(int ran_id, string ran_nombre)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
        }

        private void Configuraciones_Load(object sender, EventArgs e)
        {
            conn.Iniciar("");
            Cargar();
            Vistas();
            cbAlmacenes.DataSource = LlenarAlmacenes();
            cbAlmacenes.DisplayMember = "Almacen";
            cbAlmacenes.ValueMember = "ID";
            dgvAlmacenes.DataSource = AlmacenesExternos();
            BloquearDGV(dgvAlmacenes);

            cbAlmacenes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            cbAlmacenes.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cbAlmacenes.AutoCompleteSource = AutoCompleteSource.ListItems;

        }

        private void BloquearDGV(DataGridView dgv)
        {
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
        }
    
        private DataTable RellenarComboIngrediente()
        {
            DataTable dt = new DataTable();
            string query = "SELECT DESCRIPTION as ing_sie, display_name as clave "
                        + " FROM DS_INGREDIENT WHERE IS_ACTIVE = 1 AND INGREDIENT_TYPE NOT IN(99) "
                        + " AND IS_DELETED = 0 AND is_active = 1 ORDER BY DESCRIPTION ASC";
            conn.QueryTracker(query, out dt);
            return dt;
        }

        private DataTable LlenarAlmacenes()
        {
            DataTable dt;
            string query = " SELECT alm_id AS ID, CONCAT(alm_id, ' ', alm_nombre) AS Almacen FROM[DBSIE].[dbo].almacen a WHERE a.alm_tipo = 3";
            conn.QuerySIE(query, out dt);

            return dt;
        }

        private DataTable AlmacenesExternos()
        {
            DataTable dt;
            string query = "SELECT alm_id AS Almacen, alm_nombre AS Nombre FROM[DBSIE].[dbo].almacen_externo WHERE ran_id = " + ran_id;
            conn.QuerySIE(query, out dt);
            return dt;
        }

        private void Cargar()
        {           
            try
            {
                int tipo, prorrateo, sie, bascula;
                DataTable dt;
                string query = "SELECT cr.tic_id AS Tipo, c.ran_emp_prorrateo AS Prorrateo, IIF(c.ran_sie = 1,1,0) AS SIE, cr.cr_bascula "
                            + " FROM configuracion c "
                            + " LEFT JOIN configuracion_rancho cr ON cr.ran_id = c.ran_id "
                            + " WHERE c.ran_id = " + ran_id;
                conn.QuerySIO(query, out dt);

                Int32.TryParse(dt.Rows[0][0].ToString(), out tipo);
                Int32.TryParse(dt.Rows[0][1].ToString(), out prorrateo);
                sie = Convert.ToInt32(dt.Rows[0][2].ToString());
                Int32.TryParse(dt.Rows[0][3].ToString(), out bascula);
                //Int32.TryParse(dt.Rows[0][2].ToString(), out sie);

                switch (tipo)
                {
                    case 1: radioButton3.Checked = true; break;
                    case 2: radioButton2.Checked = true; break;
                    case 3: radioButton1.Checked = true; break;
                    default: break;
                }
                switch (prorrateo)
                {
                    case 0: radioButton5.Checked = true; break;
                    case 1: radioButton4.Checked = true; break;
                    default: break;
                }

                switch (sie)
                {
                    case 0: radioButton9.Checked = true;  break;
                    case 1: radioButton10.Checked = true; break;
                    default:break;
                }

                switch (bascula)
                {
                    case 1: radioButton8.Checked = true; break;
                    case 2: radioButton7.Checked = true; break;
                    case 3: radioButton6.Checked = true; break;
                    case 4: radioButton11.Checked = true; break;
                }

            }
            catch(DbException ex) { MessageBox.Show(ex.Message, "ERROR",MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private DataTable CargarVistas()
        {
            DataTable dt;
            string query = "";
            conn.QuerySIO(query, out dt);
            return dt;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int tipo, prorrateo, sie, bascula, ran_bascula;
                //Tipo
                tipo = radioButton1.Checked ? 3 : radioButton2.Checked ? 2 : 1;

                //Prorrateo
                prorrateo = radioButton4.Checked ? 1 : 0;

                //sie
                sie = radioButton10.Checked ? 1 : 0;
                bascula = radioButton8.Checked ? 1 : radioButton7.Checked ? 2 : radioButton6.Checked ? 3 : 4;
                ran_bascula = bascula != 2 ? 1 : 0;

                Console.WriteLine("tipo: {0}", tipo);
                Console.WriteLine("prorrateo: {0}", prorrateo);
                Console.WriteLine("sie: {0}", sie);

                string query = "UPDATE configuracion_rancho SET tic_id = " + tipo.ToString();
                conn.UPDATEAlimento("[DBSIO].[dbo].configuracion_rancho", "tic_id = " + tipo.ToString() + ", cr_bascula = " + bascula, "WHERE ran_id = " + ran_id);

                conn.UPDATEAlimento("[DBSIO].[dbo].configuracion", "ran_emp_prorrateo = " + prorrateo  + ", ran_sie = " + sie + ", ran_bascula = "  + ran_bascula," WHERE ran_id = " + ran_id);
                Application.Restart();
                //MessageBox.Show("Actualizacion Exitosa","Info",MessageBoxButtons.OK, MessageBoxIcon.Information);
                
            }
            catch (DbException ex) { MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void Vistas()
        {
            DataTable dt;
            string query = "select vis_id, vis_nombre FROM[DBSIO].[dbo].vista";
            conn.QuerySIO(query, out dt);

            //dataGridView1.DataSource = dt;            

            DataTable dt2;
            query = "select IIF(rv.vis_id IS NULL, 0,1) AS ver, v.vis_id AS id, v.vis_nombre AS vista "
                    + " FROM[DBSIO].[dbo].vista v "
                    + " LEFT JOIN[DBSIO].[dbo].rancho_vista rv ON v.vis_id = rv.vis_id "
                    + " AND rv.ran_id = " + ran_id + " ORDER BY v.vis_id";
            conn.QuerySIO(query, out dt2);
            int v = 0;

            bool ver;
            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                ver = Convert.ToInt32(dt2.Rows[i][0]) == 1;
                dataGridView1.Rows.Add(ver, dt2.Rows[i][2].ToString());
            }

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new Menu().CerrarPanelMenus();
            string buscar,query;
            DataTable dt2;
            query = "select IIF(rv.vis_id IS NULL, 0,1) AS ver, v.vis_id AS id, v.vis_nombre AS vista "
                    + " FROM[DBSIO].[dbo].vista v "
                    + " LEFT JOIN[DBSIO].[dbo].rancho_vista rv ON v.vis_id = rv.vis_id "
                    + " AND rv.ran_id = " + ran_id + " ORDER BY v.vis_id";
            conn.QuerySIO(query, out dt2);
            
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                try
                {
                    if ((bool)row.Cells[0].Value == true)
                    {
                        buscar = row.Cells[1].Value.ToString();
                        DataRow[] rows = dt2.Select("Vista = '" + buscar + "'");
                        for(int i = 0; i < rows.Length; i++)
                        {
                            conn.DeleteAlimento("[DBSIO].[dbo].rancho_vista", "where ran_id = " + ran_id.ToString() + " AND vis_id = " + rows[i][1].ToString());
                            conn.InsertAlimento("[DBSIO].[dbo].rancho_vista",ran_id.ToString() + "," + rows[i][1].ToString() + ",GETDATE(),'LG0010'");
                        }
                    }
                    else
                    {
                        buscar = row.Cells[1].Value.ToString();
                        DataRow[] rows = dt2.Select("Vista = '" + buscar + "'");
                        for (int i = 0; i < rows.Length; i++)
                        {
                            conn.DeleteAlimento("[DBSIO].[dbo].rancho_vista", "where ran_id = " + ran_id.ToString() + " AND vis_id = " + rows[i][1].ToString());                            
                        }
                    }                    
                }
                catch { }
            }
            MessageBox.Show("Actualizacion Exitosa", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Application.Restart();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string almacen, nombre = " ", id;
                almacen = cbAlmacenes.Text;
                string[] datos = almacen.Split(' ');
                id = datos[0];

                for(int i = 1; i < datos.Length; i++)
                {
                    nombre += datos[i] + " ";
                }

                nombre = nombre.Substring(0, nombre.Length - 1);

                Console.WriteLine("ID: {0}.\nNombre:{1}", id, nombre);
                string query = "IF NOT EXISTS( SELECT * FROM [DBSIE].dbo.almacen_externo  WHERE alm_id = '" + id + "' AND ran_id =" + ran_id + ") "
                            + " INSERT INTO [DBSIE].dbo.almacen_externo(alm_id, alm_nombre, ran_id) VALUES "
                            + " ('" + id + "', '" + nombre + "', " + ran_id.ToString() +")";
                conn.QueryAlimento(query);

            }
            catch { }
            dgvAlmacenes.DataSource = AlmacenesExternos();
            BloquearDGV(dgvAlmacenes);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                int index = dgvAlmacenes.CurrentRow.Index;
                string id = dgvAlmacenes[0, index].Value.ToString();
                string nombre = dgvAlmacenes[1, index].Value.ToString();
                string condicion = "WHERE alm_id = '" + id + "' AND alm_nombre = '" + nombre + "' AND ran_id = " + ran_id;
                conn.DeleteAlimento("[DBSIE].dbo.almacen_externo",condicion);

                dgvAlmacenes.Rows.RemoveAt(index);
            }
            catch { }
        }
    }
}
