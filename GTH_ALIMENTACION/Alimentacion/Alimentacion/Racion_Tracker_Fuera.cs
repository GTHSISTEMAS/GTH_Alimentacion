using System;
using System;
using System.Collections;
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
    public partial class Racion_Tracker_Fuera : Form
    {
        ConnSIO conn = new ConnSIO();
        int emp_id;
        int ran_id;
        string ran_numero;
        bool empresa;
        List<string> listaCorrales;
        string textoEstablo;
        int tipo;

        public Racion_Tracker_Fuera(int ran_id, int emp_id, bool empresa, int tipo)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.emp_id = emp_id;
            this.empresa = empresa;
            this.tipo = tipo;
        }

    

        private void Racion_Tracker_Fuera_Load(object sender, EventArgs e)
        {
            ran_numero = ran_id > 9 ? ran_id.ToString() : "0" + ran_id.ToString();
            conn.Iniciar("DBSIE");
            cbIngrediente.DataSource = LlenerIngrediente();
            cbIngrediente.DisplayMember = "Ingrediente";
            cbIngrediente.ValueMember = "Clave";

            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;

            LlenarGridCaptura();
            LlenarGridPREM();

            cbIngrediente.Cursor = Cursors.Hand;
            button1.Cursor = Cursors.Hand;
            button2.Cursor = Cursors.Hand;
            txtEtapa.Cursor = Cursors.Hand;

            cbIngrediente.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            cbIngrediente.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cbIngrediente.AutoCompleteSource = AutoCompleteSource.ListItems;    
            
            
        }
     

        private DataTable LlenerIngrediente()
        {
            DataTable dt;
            string query = "SELECT DISTINCT i.ingt_clave AS Clave, p.prod_nombre AS Ingrediente "
                        + " FROM ingrediente_tracker i LEFT JOIN producto p ON i.ingt_clave = p.prod_clave "
                        + " where SUBSTRING(i.ingt_clave,1,4) IN('ALAS', 'ALFO') AND i.ran_id = " + ran_id.ToString() 
                        + " AND p.prod_nombre is not null ORDER BY p.prod_nombre";
            conn.QueryAlimento(query, out dt);
            return dt;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ArrayList etapas = new ArrayList();
            string ing_clave;
            double precio, pms, peso;
            string ingrediente, valores;
            string query;
            string condicion;
            DateTime fecha = DateTime.Now;
            string campos = "etap_id, rac_descripcion, ing_clave, cap_consumo, cap_porcentaje_ms, ing_precio, cap_fecha_reg, cap_fecha_act, cap_actualizacion";
            try
            {
                ing_clave = cbIngrediente.SelectedValue.ToString();
                if (txtEtapa.TextLength > 0 && txtGasto.TextLength > 0 && txtMs.TextLength > 0 && txtPrecio.TextLength > 0)
                {                    
                    if(txtEtapa.Text[0] == ',' || txtEtapa.Text[txtEtapa.Text.Length -1] == ',')
                    {
                        MessageBox.Show("checar Etapas", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtEtapa.Focus();
                    }
                    else
                    {
                        string etp = txtEtapa.Text.ToString();
                        string[] etapas = etp.Split(',');
                        string temp, temp1;
                        int etp_id;
                        //ing_clave = cbIngrediente.SelectedValue.ToString();
                        precio = Convert.ToDouble(txtPrecio.Text);
                        pms = Convert.ToDouble(txtMs.Text);
                        ingrediente = cbIngrediente.SelectedText.ToString();

                        DataTable dtV;                         

                        for (int i = 0; i < etapas.Length; i++)
                        {
                            temp = etapas[i];
                            temp1 = temp[2].ToString() + temp[3];
                            Console.WriteLine(temp1);
                            etp_id = Convert.ToInt32(temp1);
                            peso = Convert.ToDouble(txtGasto.Text);

                            query = "IF EXISTS(SELECT * FROM captura where rac_descripcion like '" + temp + "' AND ing_clave like '" + ing_clave + "') "
                                + " UPDATE captura SET cap_consumo = " + peso +" , cap_porcentaje_ms = " + pms + ", ing_precio = " + precio + ", cap_fecha_act = GETDATE() "
                                + " WHERE rac_descripcion like '" + temp + "' AND ing_clave like '" + ing_clave + "' "
                                + " ELSE "
                                + " INSERT INTO captura(etap_id, rac_descripcion, ing_clave, cap_consumo, cap_porcentaje_ms, ing_precio, cap_fecha_reg, cap_fecha_act, cap_actualizacion) "
                                + " VALUES(" + etp_id + ", '" + temp + "', '" + ing_clave + "'," + peso + "," + pms + ", " + precio + " , GETDATE(), GETDATE(), 0)";


                            query = "SELECT * FROM captura where ing_clave like '" + ing_clave + "' AND rac_descripcion like '" + temp + "'";
                            conn.QueryAlimento(query, out dtV);

                            if (dtV.Rows.Count > 0)
                            {
                                query = "cap_consumo = " + peso + ", cap_porcentaje_ms = " + pms.ToString() + ", cap_fecha_act = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "', cap_actualizacion =  1";
                                condicion = "where ing_clave = '" + ing_clave + "' AND rac_descripcion = '" + temp + "'";

                                conn.UPDATEAlimento("captura", query, condicion);

                            }
                            else
                            {
                                valores = etp_id + ", '" + temp + "' , '" + ing_clave + "'," + peso + "," + pms + "," + precio + ",'" + fecha.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + fecha.ToString("yyyy-MM-dd HH:mm:ss") + "', 0";
                                conn.InsertAlimento(campos, "captura", valores);
                            }
                        }

                        LlenarGridCaptura();
                    }
                }
                else
                {
                    MessageBox.Show("Llenar Todos los campos", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (txtEtapa.TextLength > 0)
                        txtGasto.Focus();
                    else if (txtGasto.TextLength > 0)
                        txtEtapa.Focus();
                }
            }
            catch 
            {
                MessageBox.Show("Checar Campos de captura", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cbIngrediente.Focus();
            }

            
            
        }

        public static int ConvertToJulian(DateTime fecha)
        {
            TimeSpan ts = (fecha - Convert.ToDateTime("01/01/1900"));
            int julianday = ts.Days + 2;
            return julianday;
        }

        private DataTable InventarioAFI()
        {
            DataTable dt;
            string query = " SELECT ran_id, ia_fecha, ia_jaulas, ia_destetadas, ia_destetadas2, ia_lactancia1, ia_lactancia2, "
                            + " ia_lactancia3, ia_lactancia4, ia_vaquillas, ia_inseminadas, ia_vacas_secas, ia_vacas_ord, ia_vqreto, ia_vcreto, ia_rebano, " 
                            + " ia_vacsecasl1, ia_vacassecasl2, ia_vacassecasl3, ia_vacassecasl4 ";
            conn.QueryAlimento(query, out dt);
            return dt;
        }

        private void txtEtapa_Click(object sender, EventArgs e)
        {
            Etapa etp = new Etapa(ran_id, emp_id, empresa, tipo);
            etp.BoolEmpresa = empresa;
            if (etp.ShowDialog() == DialogResult.OK)
            {
                txtEtapa.Text = etp.textoCorrales;
                listaCorrales = etp.corrales;
                textoEstablo = etp.textoEstablo;
            }
            
            //etp.ShowDialog();
        }

        private void cbIngrediente_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                string ing_clave = cbIngrediente.SelectedValue.ToString();
                double pms, precio;
                DataTable dt;

                if (ing_clave[0] == 'A')
                {
                    string query = "SELECT ingt_precio, ingt_porcentaje_ms FROM ingrediente_tracker WHERE ingt_clave like '" + ing_clave + "'";
                    conn.QueryAlimento(query, out dt);

                    precio = Convert.ToDouble(dt.Rows[0][0]);
                    pms = Convert.ToDouble(dt.Rows[0][1]);

                    txtPrecio.Text = precio.ToString("###,##0.00"); txtPrecio.Enabled = false;
                    txtMs.Text = pms.ToString(); txtMs.Enabled = false;
                    Console.WriteLine(ing_clave);
                }
            }
            catch { }
        }

        private void txtGasto_KeyPress(object sender, KeyPressEventArgs e)
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
        private void LlenarGridCaptura()
        {
            string ing_clave, query;
            double precio, pms;
            DataTable dt;
            if (txtMs.TextLength == 0 && txtPrecio.TextLength == 0)
            {
               // Console.WriteLine(cbIngrediente.SelectedValue.ToString());
                ing_clave = cbIngrediente.SelectedValue.ToString();

                query = "SELECT ingt_precio, ingt_porcentaje_ms FROM ingrediente_tracker WHERE ingt_clave like '" + ing_clave + "'";
                conn.QueryAlimento(query, out dt);

                precio = Convert.ToDouble(dt.Rows[0][0]);
                pms = Convert.ToDouble(dt.Rows[0][1]);

                txtPrecio.Text = precio.ToString("###,##0.00"); txtPrecio.Enabled = false;
                txtMs.Text = pms.ToString(); txtMs.Enabled = false;
            }

            query = "SELECT IIF(r.Fecha IS NOT NULL, r.Fecha, c.cap_fecha_act) AS FECHA, c.rac_descripcion AS ETAPA, "
                    + " p.prod_clave AS CLAVE, p.prod_nombre AS INGREDIENTE, c.cap_consumo AS 'GASTO X DIA X ANIMAL', c.ing_precio AS PRECIO, "
                    + " c.cap_porcentaje_ms AS MS, IIF(r.Fecha IS NOT NULL, 'ULTIMAVEZ ACTUALIZADO', 'ULTIMA VEZ AGREGADO') AS ESTATUS "
                    + " FROM captura c "
                    + " LEFT JOIN( "
                    + " SELECT rac_descripcion AS Racion, ing_clave AS Clave, MAX(rac_fecha) AS Fecha "
                    + " FROM racion "
                    + " WHERE ing_polvo = 1 "
                    + " GROUP BY rac_descripcion, ing_clave "
                    + " ) r ON r.Racion = c.rac_descripcion AND c.ing_clave = r.Clave "
                    + " LEFT JOIN producto p ON c.ing_clave = p.prod_clave";
            conn.QueryAlimento(query, out dt);

            dataGridView1.DataSource = dt;
            dataGridView1.ReadOnly = true;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            for(int i= 0; i < dataGridView1.Columns.Count; i++)
            {
                if(i == 1 || i == 2)
                    dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                else
                    dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

        }
        private void LlenarGridPREM()
        {
            DataTable dt;
            string query = " select distinct t3.display_name AS DISPLAYNAME ,  t3.description AS RACION "
                            + " from ds_rel_ration_ingredient t1, ds_ingredient t2, ds_ration t3 "
                            + " where t1.ingredient_id = t2.id "
                            + " and t1.ration_id = t3.id "
                            + " and SUBSTRING(t3.DESCRIPTION FROM 3 FOR 2) = '01' "
                            + " and t3.is_active = 1 "
                            + " and t2.is_active = 1 "
                            + " and UPPER(t2.DESCRIPTION) not in ('AGUA','SOBRANTE','', 'WATER')";
            conn.QueryTracker(query, out dt);
            dataGridView2.DataSource = dt;
            dataGridView2.ReadOnly = true;
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt;
                int index = dataGridView1.CurrentRow.Index;
                string etapa = dataGridView1[1, index].Value.ToString();
                string ingrediente = dataGridView1[2, index].Value.ToString();
                //conn.QueryAlimento("SELECT prod_clave from producto where prod_nombre like '" + ingrediente + "'", out dt);
                string clave = dataGridView1[2, index].Value.ToString();

                string condicion = "where rac_descripcion like '" + etapa + "' AND ing_clave like '" + clave + "'";
                conn.DeleteAlimento("captura",condicion);

                dataGridView1.Rows.RemoveAt(index);
                Console.WriteLine(etapa);
                Console.WriteLine(ingrediente);
            }
            catch { }
        }

        private void dataGridView2_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                int index = dataGridView2.CurrentRow.Index;
                string racion = dataGridView2[1, index].Value.ToString();
                string numpm = racion[2].ToString() + racion[3];
                DataTable dt;
                DataTable dtPM = new DataTable();

                string query = "select t2.description, (round(t1.amount *100,4)/100) / t4.Total * 100, round(t1.amount *100,4)/100, t2.dry_matter, t2.price from ds_rel_ration_ingredient t1 "
                        + " LEFT JOIN ds_ingredient t2 ON t1.ingredient_id = t2.id LEFT JOIN ds_ration t3 ON t1.ration_id = t3.id "
                        + "LEFT JOIN( SELECT t3.description AS description, SUM(round(t1.amount*100,4)/ 100) AS Total "
                        + " from ds_rel_ration_ingredient t1 LEFT JOIN ds_ingredient t2 ON t1.ingredient_id = t2.id "
                        + " LEFT JOIN ds_ration t3 ON t1.ration_id = t3.id where SUBSTRING(t3.DESCRIPTION FROM 3 FOR 2) = '" + numpm + "' "
                        + " and t3.is_active = 1 and t2.is_active = 1  and UPPER(t2.DESCRIPTION) not in ('AGUA', 'SOBRANTE', '', 'WATER') "
                        + " GROUP BY t3.description ) t4 ON t4.description = t3.description where SUBSTRING(t3.DESCRIPTION FROM 3 FOR 2) = '" + numpm + "' and t3.is_active = 1 "
                        + " and t2.is_active = 1  and UPPER(t2.DESCRIPTION) not in ('AGUA', 'SOBRANTE', '', 'WATER') ";

                conn.QueryTracker(query, out dt);

                dtPM.Columns.Add("INGREDIENTE").DataType = System.Type.GetType("System.String");
                dtPM.Columns.Add("PORCENTAJE RACION").DataType = System.Type.GetType("System.Double");
                dtPM.Columns.Add("KILOS").DataType = System.Type.GetType("System.Double");
                dtPM.Columns.Add("%MS").DataType = System.Type.GetType("System.Double");
                dtPM.Columns.Add("PRECIO").DataType = System.Type.GetType("System.Double");

                if (dt.Rows.Count > 0)
                {
                    for(int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow  dr = dtPM.NewRow();
                        dr["INGREDIENTE"] = dt.Rows[i][0].ToString();
                        dr["PORCENTAJE RACION"] = Convert.ToDouble(dt.Rows[i][1]);
                        dr["KILOS"] = Convert.ToDouble(dt.Rows[i][2]);
                        dr["%MS"] = Convert.ToDouble(dt.Rows[i][3]);
                        dr["PRECIO"] = Convert.ToDouble(dt.Rows[i][4]);
                        dtPM.Rows.Add(dr);
                    }
                    dataGridView3.DataSource = dtPM;
                    dataGridView3.Columns[1].DefaultCellStyle.Format = "###,##0.00";
                    dataGridView3.Columns[2].DefaultCellStyle.Format = "###,##0.000";
                    dataGridView3.Columns[4].DefaultCellStyle.Format = "###,##0.00";
                    dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                    for(int i = 0; i < dataGridView3.Columns.Count; i++)
                    {
                        if (i != 0)
                            dataGridView3.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        else
                            dataGridView3.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

                    }
                
                }
            }
            catch { }
            //Console.WriteLine(index.ToString());
        }

        private void FormatoGrid(DataGridView dgv)
        {
            for (int i = 0; i < dgv.Columns.Count; i++)
            {               
                if (i != 0)
                    dgv.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                else
                    dgv.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }

            dgv.Columns[4].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[5].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[6].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[7].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[8].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[9].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[10].DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Pixel);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(221, 235, 247);

        }

        private DataTable Establos(int tipo)
        {
            DataTable dt;
            string query = "";
            if (tipo == 2)
                query = "SELECT ran_id AS ID, ran_desc AS RANCHO FROM configuracion WHERE emp_prorrateo = ( SELECT cr.emp_id FROM configuracion c LEFT JOIN configuracion_rancho cr ON c.ran_id = cr.ran_id where c.ran_id = " + ran_id + ")";
            else
                query = "SELECT ran_id AS ID, ran_desc AS RANCHO FROM configuracion WHERE emp_id = ( SELECT cr.cr_multiempresa FROM configuracion c LEFT JOIN configuracion_rancho cr ON c.ran_id = cr.ran_id where c.ran_id = " + ran_id + ")";

            conn.QuerySIO(query, out dt);

            return dt;
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
       {
            if (e.KeyCode == Keys.Enter)
            {
                if (txtBuscar.Text.Length == 8)
                {
                    bool validar = false;

                    
                    cbIngrediente.SelectedValue = txtBuscar.Text.ToUpper();
                }
            }
        }
    }
}
