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
    public partial class Tipificaciones : Form
    {
        ConnSIO conn = new ConnSIO();
        string ran_id;
        int emp_id;
        DateTime fechaIni;
        DateTime fechaFin;
        DataTable dtAlas;
        DataTable dtAlfo;
        double porcT;
        double porcDif;
        int tipo;

        public Tipificaciones(string ran_id, int emp_id, DateTime fechaIni, DateTime fechaFin)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.emp_id = emp_id;
            this.fechaIni = fechaIni;
            this.fechaFin = fechaFin;
            conn.Iniciar();
        }

        public Tipificaciones(string ran_id, int emp_id, DateTime fechaIni, DateTime fechaFin, DataTable dtAlas, DataTable dtAlfo, int tipo)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.emp_id = emp_id;
            this.fechaIni = fechaIni;
            this.fechaFin = fechaFin;
            this.dtAlas = dtAlas;
            this.dtAlfo = dtAlfo;
            this.tipo = tipo;
            conn.Iniciar();
            GetPorcentajes();
        }

        private void Raciones_Load(object sender, EventArgs e)
        {            
            DataTable dtRaciones, dtIng;
            RacionMalTipificada(fechaIni, fechaFin, out dtRaciones);
            IngredienteMalTipíficado(ran_id, fechaIni, fechaFin, out dtIng);
            DataTable dt;
            ColumnasTabla(out dt);            

            if (dtRaciones.Rows.Count > 0)
            {
                DataRow row = dt.NewRow();
                row[0] = "";
                row[1] = dtRaciones.Rows[0][0].ToString();
                row[2] = "RACION MAL TIPIFICADA";
                dt.Rows.Add(row);
            }

            if (dtIng.Rows.Count > 0)
            {
                DataRow row = dt.NewRow();
                row[0] = "";
                row[1] = dtIng.Rows[0][0].ToString();
                row[2] = "INGREDIENTE MAL TIPIFICADA";
                dt.Rows.Add(row);                
            }

            if (dtAlas.Rows.Count > 0 || dtAlfo.Rows.Count > 0)
            {
                DataTable dtA, dtF;
                Alimentos(out dtA);                
                Forraje(out dtF);

                if (dtA.Rows.Count > 0)
                {
                    for (int i = 0; i < dtA.Rows.Count; i++)
                    {
                        DataRow row = dt.NewRow();
                        row[0] = dtA.Rows[i][0].ToString();
                        row[1] = dtA.Rows[i][1].ToString();
                        row[2] = dtA.Rows[i][2].ToString();
                        dt.Rows.Add(row);
                    }
                }

                if (dtF.Rows.Count > 0)
                {
                    for (int i = 0; i < dtF.Rows.Count; i++)
                    {
                        DataRow row = dt.NewRow();
                        row[0] = dtF.Rows[i][0].ToString();
                        row[1] = dtF.Rows[i][1].ToString();
                        row[2] = dtF.Rows[i][2].ToString();
                        dt.Rows.Add(row);
                    }
                }
            }
            dgvRevisar.DataSource = dt;
            FormatoGrid(dgvRevisar);
        }

        private void ColumnasTabla(out DataTable dt)
        {
            dt = new DataTable();
            dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("DESCRIPCION").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("MOTIVO").DataType = System.Type.GetType("System.String");
        }

        public void RacionMalTipificada(DateTime inicio, DateTime fin, out DataTable dt)
        {
            string query = "SELECT rac_descripcion AS Racion"
                        + " FROM racion "
                        + " WHERE ran_id = 0 AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                        + " GROUP BY rac_descripcion";
            conn.QueryAlimento(query, out dt);
        }

        public void IngredienteMalTipíficado(string ranId, DateTime inicio, DateTime fin, out DataTable dt)
        {
            string query = "select DISTINCT ing_descripcion AS Ingrediente"
                        + " from racion "
                        + " where ran_id IN(" + ranId + ") AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha< '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                        + " AND SUBSTRING(ing_clave, 1,4) NOT IN('ALAS', 'ALFO') AND ing_descripcion not IN('AGUA', 'WATER') "
                        + " AND(ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) not IN('00', '01', '02', '90')) "
                        + " AND(SUBSTRING(ing_descripcion, 1, 1) NOT IN('1', '2', '3', '4')  AND ing_descripcion like '%SOB%')"; 
            conn.QueryAlimento(query, out dt);
        }

        private void FormatoGrid(DataGridView dgv)
        {            
            BloquearDGV(dgv);
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 156, 241);
            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Pixel);
            }

        }

        private void BloquearDGV(DataGridView dgv)
        {
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                dgv.Columns[i].ReadOnly = true;
                dgv.Columns[i].Frozen = false;
            }
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToOrderColumns = false;

        }

        public bool TipificacionesCorrectas()
        {
            bool validacion = false;
            DataTable dt;
            string query = "select DISTINCT ing_descripcion AS Tipificacion "
                            + " from racion "
                            + " where ran_id IN(" + ran_id +  ") AND rac_fecha >= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha< '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "' "               
                            + " AND SUBSTRING(ing_clave, 1,4) NOT IN('ALAS', 'ALFO') " 
                            + " AND ing_descripcion not IN('AGUA', 'WATER') "
                            + " AND(ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) not IN('00', '01', '02', '90')) "
                            + " AND(SUBSTRING(ing_descripcion, 1, 1) NOT IN('1', '2', '3', '4')  AND ing_descripcion like '%SOB%') "
                            + " UNION "
                            + " SELECT rac_descripcion "
                            + " FROM racion "
                            + " WHERE ran_id = 0 "
                            + " AND rac_fecha >= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha< '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " GROUP BY rac_descripcion";
            conn.QueryAlimento(query, out dt);
            validacion = dt.Rows.Count == 0 ? true : dt.Rows[0][0] != DBNull.Value ? true : false;

            return validacion;
        }
     
        private void Alimentos(out DataTable dt)
        {
            dt = new DataTable();
            if (dtAlas.Rows.Count > 0)
            {
                dt.Columns.Add("CLAVE");
                dt.Columns.Add("INGREDIENTE");
                dt.Columns.Add("MOTIVO");
                double sie, invF, consumo, consumoT, t, dif;
                bool validacion;
                string motivo;
                for (int i = 0; i < dtAlas.Rows.Count; i++)
                {
                    motivo = "";
                    validacion = false;
                    sie = dtAlas.Rows[i][4] != DBNull.Value ? Convert.ToDouble(dtAlas.Rows[i][4]) : 0;
                    invF = dtAlas.Rows[i][5] != DBNull.Value ? Convert.ToDouble(dtAlas.Rows[i][5]) : 0;
                    consumo = dtAlas.Rows[i][6] != DBNull.Value ? Convert.ToDouble(dtAlas.Rows[i][6]) : 0;
                    consumoT = dtAlas.Rows[i][9] != DBNull.Value ? Convert.ToDouble(dtAlas.Rows[i][9]) : 0;
                    t = dtAlas.Rows[i][8] != DBNull.Value ? Convert.ToDouble(dtAlas.Rows[i][8]) : 0;
                    dif = dtAlas.Rows[i][11] != DBNull.Value ? Convert.ToDouble(dtAlas.Rows[i][11]) : 0;

                    if (invF == 0)
                    { 
                        validacion = true;
                        motivo = "INVENTARIO FINAL EN 0,";
                    }
                    
                    if ((sie < consumo) || (invF == 0 && sie < consumoT) )
                    {
                        validacion = true;
                        motivo += " INVENTARIO INSUFICIENTE,";

                    }
                    
                    if ((consumo == 0 && consumoT > 0) || (consumo > 0 && consumoT == 0))
                    {
                        validacion = true;
                        motivo += consumo == 0 && consumoT > 0 ? " NO HAY CONSUMO PERO HAY CONSUMO EN TRACKER," : " HAY CONSUMO PERO NO HAY CONSUMO EN TRACKER,";

                    }

                    if (t >= porcT)
                        if ((dif >= porcDif) || dif <= (porcDif * -1))
                        {
                            validacion = true;
                            motivo += " PORCENTAJES ESTAN ARRIBA DE LO PERMITIDO ";
                        }
                    
                    if(validacion)
                    {
                        DataRow row = dt.NewRow();
                        row[0] = dtAlas.Rows[i][2].ToString();
                        row[1] = dtAlas.Rows[i][3].ToString();
                        row[2] = motivo.Substring(0, motivo.Length -1);
                        dt.Rows.Add(row);

                    }
                }
            }          
            
        }

        private void Forraje(out DataTable dt)
        {
            dt = new DataTable();            
            
            if (dtAlfo.Rows.Count > 0)
            {
                dt.Columns.Add("CLAVE");
                dt.Columns.Add("INGREDIENTE");
                dt.Columns.Add("MOTIVO");

                int colBascula = tipo == 3 ? 5 : 4;
                int colTracker = tipo == 3 ? 8 : 7;
                int colT = tipo == 3 ? 7 : 6;
                int colDif = tipo == 3 ? 11 : 10;
                double inv, bascula, tracker, t, dif;
                bool validacion;
                string motivo;
                for (int i = 0; i < dtAlfo.Rows.Count; i++)
                {
                    motivo = "";
                    validacion = false;
                    inv = dtAlfo.Rows[i][3] != DBNull.Value ? Convert.ToDouble(dtAlfo.Rows[i][3]) : 0;
                    bascula = dtAlfo.Rows[i][colBascula] != DBNull.Value ? Convert.ToDouble(dtAlfo.Rows[i][colBascula]) : 0;
                    tracker = dtAlfo.Rows[i][colTracker] != DBNull.Value ? Convert.ToDouble(dtAlfo.Rows[i][colTracker]) : 0;
                    t = dtAlfo.Rows[i][colT] != DBNull.Value ? Convert.ToDouble(dtAlfo.Rows[i][colT]) : 0;
                    dif = dtAlfo.Rows[i][colDif] != DBNull.Value ? Convert.ToDouble(dtAlfo.Rows[i][colDif]) : 0;

                    if (inv < bascula)
                    {
                        validacion = true;
                        motivo += " INVENTARIO INSUFICIENTE,";
                    }
                    
                    if (bascula > 0 && tracker == 0)
                    {
                        validacion = true;
                        motivo += " NO HAY CONSUMO EN BASCULA PERO HAY CONSUMO EN TRACKER,"; 
                    }
                    if (bascula == 0 && tracker > 0)
                    {
                        validacion = true;
                        motivo += " HAY CONSUMO EN BASCULA PERO NO HAY CONSUMO EN TRACKER,";
                    }

                    if (t >= porcT)
                        if (dif >= porcDif || dif <= (porcDif * -1))
                        {
                            validacion = true;
                            motivo += " PORCENTAJES ESTAN ARRIBA DE LO PERMITIDO ";
                        }


                    if (validacion)
                    {
                        DataRow row = dt.NewRow();
                        row[0] = dtAlfo.Rows[i][1].ToString();
                        row[1] = dtAlfo.Rows[i][2].ToString();
                        row[2] = motivo.Substring(0, motivo.Length - 1);
                        dt.Rows.Add(row);

                    }
                            
                }

            }
        }

        private void GetPorcentajes()
        {
            DataTable dt;
            string query = "select pp_porc_dif, pp_porc_t from porcentaje_prorrateo";
            conn.QueryAlimento(query, out dt);

            porcDif = dt.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dt.Rows[0][0]) : 0;
            porcT = dt.Rows[0][1] != DBNull.Value ? Convert.ToDouble(dt.Rows[0][1]) : 0;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessDialogKey(keyData);
        }
      
    }
}
