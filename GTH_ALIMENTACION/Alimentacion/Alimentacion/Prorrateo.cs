using ght001746q;
using ght001746q.StrongTypesNS;
using Microsoft.Reporting.WinForms;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WindowsInput.Native;
using WindowsInput;
namespace Alimentacion
{
    public partial class Prorrateo : Form
    {
        //variables
        int emp_id; //numero de empresa
        int ran_id;//numero de establo
        string rancho; //numero de rancho en cadena si es menor de 10 se le agrega un 0
        string emp_nombre; // nombre de la empresa
        string ran_nombre; //nombre del establo
        string ranNumero;
        string ranCadena;
        string sUrl = ConfigurationManager.AppSettings["url"];
        string bal_clave; //Numero de Bascula
        DateTime fecha = new DateTime();
        DateTime fecha_SIE = new DateTime();
        DateTime fecha_Tra = new DateTime();
        DateTime fecha_Bal = new DateTime();
        ConnSIO conn = new ConnSIO();
        int hora_corte;
        string ruta;
        bool bascula;
        string ali_alm_id;
        string f_alm_id;
        bool modT;
        bool modificarV;
        int ran_bascula;
        int versionId;
        int emp_prorrateo;
        string ranchosId;
        int rep;
        int prorrateo;
        public int conBasc;
        bool hb;
        bool existeProrrateo;
        double consumoAlas;
        double consumoAlfo;
        bool modCTF;
        bool modCTA;
        bool porcTActivo;
        double porcT;
        double porcDif;
        bool autorizar;
        double valorGrid;
        int dias_a;
        DateTime fecha_reg;
        bool load;
        bool almCerrados;
        bool btnGuardar;
        Tipificaciones tipificacion;
        bool tipificacionCorrecta;

        public Prorrateo(int emp_id, string emp_nombre, int ran_id, string ran_nombre)
        {
            InitializeComponent();
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            conn.DeleteAlimento("prorrateoTemp", "where ran_id = " + ran_id.ToString());
            DateTime hoy = DateTime.Today;
            DateTime temp = new DateTime(fecha.Year, fecha.Month, 1, hora_corte, 0, 0);
            temp = temp.AddDays(-1);
            TimeSpan ts = hoy - temp;
            int days = ts.Days;
            days = DateTime.Today.Day >= 1 && DateTime.Today.Day < 6 ? days + DateTime.Today.Day : days;
            string cadenaExe = ConfigurationManager.AppSettings["ConsumoExe"];
            //Process p = Process.Start(cadenaExe, days.ToString());
            Process p = new Process();
            p.StartInfo.FileName = cadenaExe;
            p.StartInfo.Arguments = days.ToString();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            p.Start();
            p.BeginOutputReadLine();
            p.WaitForExit();

            getParameters();
            tipificacionCorrecta = tipificacion.TipificacionesCorrectas();
            SetTitulos();
            if (rep == 1)
            {
                FillDGVAlimento(ranchosId);
            }
            else
                FillDGVAlimento();
            //FillDGVForraje();
            Forraje();
            PorcentajeT(modCTA, modCTF);
            Cursor = Cursors.Default;
            button1.Enabled = false;
            MessageBox.Show("Carga de informacion completada", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Console.WriteLine(outLine.Data);
        }

        private string AlmacenesExternos()
        {
            string alm = "";
            DataTable dt;
            string query = "SELECT alm_id FROM[DBSIE].dbo.almacen_externo WHERE ran_id = " + ran_id;
            conn.QueryAlimento(query, out dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                alm += "'" + dt.Rows[i][0].ToString() + "',";
            }

            return alm.Length > 0 ? alm.Substring(0, alm.Length - 1) : "''";
        }

        private void Forraje()
        {
            DateTime racion_fin = new DateTime(fecha.Year, fecha.Month, 1, hora_corte, 0, 0);
            racion_fin = racion_fin.AddMonths(1).AddDays(-1);

            DataTable dtV, dt, dtP;
            string query = "select * from prorrateo WHERE ran_id = " + ran_id + "  AND pro_fecha_reg = '" + fecha_reg.ToString("yyyy-MM-dd") + "' ";
            conn.QueryAlimento(query, out dtV);

            double existencia, bascula, tracker, cons, p = 0, t = 0, trackT = 0, bascT = 0, dif, pdif, taux, difAux;
            string exist;
            dt = new DataTable();
            if (dtV.Rows.Count == 0)
            {
                FillDGVForraje();
            }
            else
            {
                query = "SELECT FORMAT(pro_fecha, 'd','en-gb'), art_clave, prod_nombre, pro_existencia_sie, pro_inv_final, pro_consumo, pro_porc_b, pro_porc_t, pro_consumo_tra, "
                       + " pro_dif_kg, pro_dif, pro_bascula, pro_consumo_ext "
                       + " from prorrateo "
                       + " WHERE ran_id = " + ran_id + " AND pro_fecha_reg = '" + racion_fin.ToString("yyyy-MM-dd") + "' AND SUBSTRING(art_clave,1,4) IN('ALFO')";
                conn.QueryAlimento(query, out dtP);

                if (conBasc == 3)
                {
                    dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");//0
                    dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");//1
                    dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String"); //2
                    dt.Columns.Add("EXISTENCIA SIE").DataType = System.Type.GetType("System.Double");//3
                    dt.Columns.Add("INV FINAL").DataType = System.Type.GetType("System.Double");//4
                    dt.Columns.Add("CONSUMO").DataType = System.Type.GetType("System.Double");//5
                    dt.Columns.Add("%P").DataType = System.Type.GetType("System.Double");
                    dt.Columns.Add("%T").DataType = System.Type.GetType("System.Double");
                    dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.Double");//6
                    dt.Columns.Add("EXISTENCIA");//7
                    dt.Columns.Add("DIF / KG").DataType = System.Type.GetType("System.Double");//8
                    dt.Columns.Add("% DIF").DataType = System.Type.GetType("System.Double");//9

                    for (int i = 0; i < dtP.Rows.Count; i++)
                    {
                        existencia = Convert.ToDouble(dtP.Rows[i][3]);
                        bascula = Convert.ToDouble(dtP.Rows[i][4]); bascT += bascula;
                        tracker = Convert.ToDouble(dtP.Rows[i][5]); trackT += tracker;
                        cons = tracker > 0 ? bascula : 0;
                        exist = existencia >= cons ? "✔" : "X";
                        p += Convert.ToDouble(dtP.Rows[i][6]);
                        t += Convert.ToDouble(dtP.Rows[i][7]);
                        DataRow row = dt.NewRow();
                        row["FECHA"] = dtP.Rows[i][0].ToString();
                        row["CLAVE"] = dtP.Rows[i][1].ToString();
                        row["ARTICULO"] = dtP.Rows[i][2].ToString();
                        row["EXISTENCIA SIE"] = Convert.ToDouble(dtP.Rows[i][3]);
                        row["INV FINAL"] = Convert.ToDouble(dtP.Rows[i][4]);
                        row["CONSUMO"] = Convert.ToDouble(dtP.Rows[i][5]);
                        row["%P"] = Convert.ToDouble(dtP.Rows[i][6]);
                        row["%T"] = Convert.ToDouble(dtP.Rows[i][7]);
                        row["TRACKER"] = Convert.ToDouble(dtP.Rows[i][8]);
                        row["EXISTENCIA"] = exist;
                        row["DIF / KG"] = Convert.ToDouble(dtP.Rows[i][9]);
                        row["% DIF"] = Convert.ToDouble(dtP.Rows[i][10]);
                        dt.Rows.Add(row);
                    }

                    DataRow dr = dt.NewRow();
                    dr["ARTICULO"] = "TOTAL";
                    dr["CONSUMO"] = bascT;
                    dr["%P"] = p;
                    dr["%T"] = t;
                    dr["TRACKER"] = trackT;
                    dr["DIF / KG"] = (bascT - trackT);
                    dr["% DIF"] = ((bascT - trackT) / trackT * 100);
                    dt.Rows.Add(dr);


                }
                else if (conBasc == 4)
                {
                    dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");//0
                    dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");//1
                    dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String"); //2
                    dt.Columns.Add("EXISTENCIA SIE").DataType = System.Type.GetType("System.Double");//3
                    dt.Columns.Add("CONSUMO REAL").DataType = System.Type.GetType("System.Double");//4
                    dt.Columns.Add("%P").DataType = System.Type.GetType("System.Double");
                    dt.Columns.Add("%T").DataType = System.Type.GetType("System.Double");
                    dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.Double");//5
                    dt.Columns.Add("EXISTENCIA");//6
                    dt.Columns.Add("DIF / KG").DataType = System.Type.GetType("System.Double");//7
                    dt.Columns.Add("% DIF").DataType = System.Type.GetType("System.Double");//8
                    dt.Columns.Add("Bascula").DataType = System.Type.GetType("System.Double");
                    dt.Columns.Add("CONSUMO EXTERNO").DataType = System.Type.GetType("System.Double");

                    for (int i = 0; i < dtP.Rows.Count; i++)
                    {
                        existencia = Convert.ToDouble(dtP.Rows[i][3]);
                        bascula = Convert.ToDouble(dtP.Rows[i][5]); bascT += bascula;
                        tracker = Convert.ToDouble(dtP.Rows[i][8]); trackT += tracker;
                        cons = tracker > 0 ? bascula : 0;
                        exist = existencia >= cons ? "✔" : "X";
                        p += Convert.ToDouble(dtP.Rows[i][6]);
                        t += Convert.ToDouble(dtP.Rows[i][7]); taux = Convert.ToDouble(dtP.Rows[i][7]);
                        difAux = Convert.ToDouble(dtP.Rows[i][10]);
                        DataRow row = dt.NewRow();
                        row["FECHA"] = dtP.Rows[i][0].ToString();
                        row["CLAVE"] = dtP.Rows[i][1].ToString();
                        row["ARTICULO"] = dtP.Rows[i][2].ToString();
                        row["EXISTENCIA SIE"] = Convert.ToDouble(dtP.Rows[i][3]);
                        row["CONSUMO REAL"] = Convert.ToDouble(dtP.Rows[i][5]);
                        row["%P"] = Convert.ToDouble(dtP.Rows[i][6]);
                        row["%T"] = Convert.ToDouble(dtP.Rows[i][7]);
                        row["TRACKER"] = Convert.ToDouble(dtP.Rows[i][8]);
                        row["EXISTENCIA"] = exist;
                        row["DIF / KG"] = Convert.ToDouble(dtP.Rows[i][9]);
                        row["% DIF"] = Convert.ToDouble(dtP.Rows[i][10]);
                        row["BASCULA"] = Convert.ToDouble(dtP.Rows[i][11]);
                        row["CONSUMO EXTERNO"] = Convert.ToDouble(dtP.Rows[i][12]);
                        dt.Rows.Add(row);
                    }

                    DataRow dr = dt.NewRow();
                    dr["ARTICULO"] = "TOTAL";
                    dr["CONSUMO REAL"] = bascT;
                    dr["%P"] = p;
                    dr["%T"] = t;
                    dr["TRACKER"] = trackT;
                    dr["DIF / KG"] = (bascT - trackT);
                    dr["% DIF"] = ((bascT - trackT) / trackT * 100);
                    dt.Rows.Add(dr);
                }
                else
                {
                    dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");//0
                    dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");//1
                    dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String"); //2
                    dt.Columns.Add("EXISTENCIA SIE").DataType = System.Type.GetType("System.Double");//3
                    dt.Columns.Add("CONSUMO").DataType = System.Type.GetType("System.Double");//4
                    dt.Columns.Add("%P").DataType = System.Type.GetType("System.Double");
                    dt.Columns.Add("%T").DataType = System.Type.GetType("System.Double");
                    dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.Double");//5
                    dt.Columns.Add("EXISTENCIA");//6
                    dt.Columns.Add("DIF / KG").DataType = System.Type.GetType("System.Double");//7
                    dt.Columns.Add("% DIF").DataType = System.Type.GetType("System.Double");//8

                    for (int i = 0; i < dtP.Rows.Count; i++)
                    {
                        existencia = Convert.ToDouble(dtP.Rows[i][3]);
                        bascula = Convert.ToDouble(dtP.Rows[i][4]); bascT += bascula;
                        tracker = Convert.ToDouble(dtP.Rows[i][5]); trackT += tracker;
                        cons = tracker > 0 ? bascula : 0;
                        exist = existencia >= cons ? "✔" : "X";
                        p += Convert.ToDouble(dtP.Rows[i][6]);
                        t += Convert.ToDouble(dtP.Rows[i][7]);
                        DataRow row = dt.NewRow();
                        row["FECHA"] = dtP.Rows[i][0].ToString();
                        row["CLAVE"] = dtP.Rows[i][1].ToString();
                        row["ARTICULO"] = dtP.Rows[i][2].ToString();
                        row["EXISTENCIA SIE"] = Convert.ToDouble(dtP.Rows[i][3]);
                        row["CONSUMO"] = Convert.ToDouble(dtP.Rows[i][5]);
                        row["%P"] = Convert.ToDouble(dtP.Rows[i][6]);
                        row["%T"] = Convert.ToDouble(dtP.Rows[i][7]);
                        row["TRACKER"] = Convert.ToDouble(dtP.Rows[i][8]);
                        row["EXISTENCIA"] = exist;
                        row["DIF / KG"] = Convert.ToDouble(dtP.Rows[i][9]);
                        row["% DIF"] = Convert.ToDouble(dtP.Rows[i][10]);
                        dt.Rows.Add(row);
                    }

                    DataRow dr = dt.NewRow();
                    dr["ARTICULO"] = "TOTAL";
                    dr["CONSUMO"] = bascT;
                    dr["%P"] = p.ToString("#,0.0");
                    dr["%T"] = t.ToString("#,0.0");
                    dr["TRACKER"] = trackT;
                    dr["DIF / KG"] = (bascT - trackT);
                    dr["% DIF"] = ((bascT - trackT) / trackT * 100);
                    dt.Rows.Add(dr);
                }

                dataGridView2.DataSource = dt;
                FormatoGrid(dataGridView2, conBasc == 3);
                if (conBasc == 2)
                {
                    dataGridView2.Columns[4].Visible = false;
                    dataGridView2.Columns[9].Visible = false;
                    dataGridView2.Columns[10].Visible = false;
                }
            }
        }

        public void ColumnasForaje(out DataTable dt)
        {
            dt = new DataTable();
            if (conBasc == 3)
            {
                dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");//0
                dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");//1
                dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String"); //2
                dt.Columns.Add("EXISTENCIA SIE").DataType = System.Type.GetType("System.Double");//3
                dt.Columns.Add("INV FINAL").DataType = System.Type.GetType("System.Double");//4
                dt.Columns.Add("CONSUMO").DataType = System.Type.GetType("System.Double");//5
                dt.Columns.Add("%P").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("%T").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.Double");//6
                dt.Columns.Add("EXISTENCIA");//7
                dt.Columns.Add("DIF / KG").DataType = System.Type.GetType("System.Double");//8
                dt.Columns.Add("% DIF").DataType = System.Type.GetType("System.Double");//9
            }
            else if (conBasc == 4)
            {
                dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");//0
                dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");//1
                dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String"); //2
                dt.Columns.Add("EXISTENCIA SIE").DataType = System.Type.GetType("System.Double");//3
                dt.Columns.Add("CONSUMO REAL").DataType = System.Type.GetType("System.Double");//4
                dt.Columns.Add("%P").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("%T").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.Double");//5
                dt.Columns.Add("EXISTENCIA");//6
                dt.Columns.Add("DIF / KG").DataType = System.Type.GetType("System.Double");//7
                dt.Columns.Add("% DIF").DataType = System.Type.GetType("System.Double");//8
                dt.Columns.Add("Bascula").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("Consumo Externo").DataType = System.Type.GetType("System.Double");
            }
            else
            {
                dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");//0
                dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");//1
                dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String"); //2
                dt.Columns.Add("EXISTENCIA SIE").DataType = System.Type.GetType("System.Double");//3
                dt.Columns.Add("CONSUMO").DataType = System.Type.GetType("System.Double");//4
                dt.Columns.Add("%P").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("%T").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.Double");//5
                dt.Columns.Add("EXISTENCIA");//6
                dt.Columns.Add("DIF / KG").DataType = System.Type.GetType("System.Double");//7
                dt.Columns.Add("% DIF").DataType = System.Type.GetType("System.Double");//8
            }
        }

        private void FillDGVForraje()
        {
            double sumBascula = 0, sumTracker = 0, sumP = 0;
            int colP = conBasc != 3 ? 5 : 6;
            DataTable dt1;
            DateTime racion_inicio = new DateTime(fecha.Year, fecha.Month, 1, hora_corte, 0, 0);
            racion_inicio = racion_inicio.AddDays(dias_a);
            DateTime racion_fin = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora_corte, 0, 0);
            racion_fin = racion_inicio.Day == 1 && hora_corte > 0 ? racion_fin.AddDays(1) : racion_fin;
            DateTime exi_fecha = fecha_SIE;
            DateTime bal_inicio = new DateTime(fecha.Year, fecha.Month, 1);
            DateTime bal_fin = bal_inicio.AddMonths(1).AddDays(-1);
            string condicion = checkBox4.Checked ? " AND ((sie.Existencia > 0 AND (tracker.Peso > 0 AND bascula.Peso > 0)) OR tracker.Peso > 0 OR bascula.Peso >0 ) ORDER BY BASCULA DESC " : " ORDER BY BASCULA DESC";
            string bascula = "";
            if (ran_bascula == 1)
            {
                bascula = "SELECT b.ing_clave AS CLAVE, SUM(b.bol_neto) AS Peso, MAX(bol_fecha) AS Fecha "
                    + " FROM boleto b "
                    + " LEFT JOIN[DBSIE].[dbo].almacen a ON b.alm_origen = a.alm_id "
                    + " WHERE b.bal_clave IN(" + bal_clave + ") AND CONVERT(date, b.bol_fecha) BETWEEN '" + bal_inicio.ToString("yyyy-MM-dd") + "' AND '" + bal_fin.ToString("yyyy-MM-dd") + "' "
                    + " AND a.ran_id = " + ran_id + " AND a.alm_tipo = 3 GROUP BY b.ing_clave";
                //bascula = "SELECT ing_clave AS CLAVE, SUM(bol_neto) AS Peso, MAX(bol_fecha) AS Fecha " 
                //    + " FROM boleto " 
                //    + " WHERE bal_clave IN (" + bal_clave.ToString() + ") AND "
                //        + " CONVERT(date, bol_fecha) BETWEEN '" + bal_inicio.ToString("yyyy-MM-dd") + "' AND '" + bal_fin.ToString("yyyy-MM-dd") + "' GROUP BY ing_clave ";
            }
            else
            {
                bascula = "SELECT T.Clave, SUM(T.Peso) AS Peso, MAX(T.Fecha) AS Fecha FROM( SELECT ing_clave AS Clave, SUM(rac_mh) AS Peso, MAX(rac_fecha) AS Fecha "
                    + " FROM racion "
                    + " WHERE ran_id IN(" + ranchosId + ") AND rac_fecha >= '" + racion_inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racion_fin.ToString("yyyy-MM-dd HH:mm") + "' "
                    + " AND SUBSTRING(rac_descripcion, 3, 2) NOT IN('00', '01', '02') AND SUBSTRING(ing_clave, 1, 4) IN('ALFO') GROUP BY ing_clave "
                    + " UNION "
                    + " SELECT R.Clave, SUM(R.Peso) AS Peso, MAX(R.Fecha) AS Fecha"
                    + " FROM( "
                    + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, IIF(T.Pmz = '', T.Ing1, T.Ing2) AS Ing, T.Peso1 * T.Porc AS Peso, T.Fecha "
                    + " FROM( "
                    + " SELECT T1.Clave AS Clave1, T1.Ing AS Ing1, T1.Peso AS Peso1, ISNULL(T2.Pmz, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porc, 1) AS Porc, ISNULL(T1.Fecha, '') AS Fecha "
                    + " FROM( SELECT R.Clave, R.Ing, SUM(R.Peso) AS Peso, MAX(R.Fecha) AS Fecha FROM( SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, T1.Fecha "
                    + " FROM( "
                    + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, MAX(rac_fecha) AS Fecha "
                    + " FROM racion "
                    + " WHERE ran_id IN(" + ranchosId + ") AND rac_fecha >= '" + racion_inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racion_fin.ToString("yyyy-MM-dd HH:mm") + "' "
                    + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 4)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') GROUP BY ing_descripcion ) T1 "
                    + " LEFT JOIN( "
                        + " SELECT pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                        + " FROM porcentaje_Premezcla "
                    + " )T2 ON T1.Pmz = T2.Pmz) R "
                    + " GROUP BY  R.Clave, R.Ing) T1 "
                    + " LEFT JOIN( "
                        + " SELECT pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                        + " FROM porcentaje_Premezcla "
                    + " )T2 ON T1.Ing = T2.Pmz) T) R "
                    + " WHERE SUBSTRING(R.Clave, 1, 4) IN('ALFO') "
                    + " GROUP BY R.Clave ) T GROUP BY T.Clave";
            }
            DataTable dtC;

            string query = "";
            if (conBasc != 4)
            {
                string existencia = "";
                if (ran_id == 25)
                {
                    existencia = "SELECT X.Almacen, X.Clave, SUM(X.Existencia) AS Existencia, MAX(X.Fecha) AS Fecha "
                            + " FROM( "
                            + " SELECT  IIF(T.Almacen = 'A41003', 'A40003', T.Almacen) AS Almacen, T.Clave, T.Existencia AS Existencia, T.Fecha "
                            + " FROM( "
                            + " SELECT art.alm_id AS Almacen, art.art_fecha AS Fecha, art.art_clave AS Clave, art.art_existencia AS Existencia "
                            + " FROM articulo art "
                            + " LEFT JOIN[DBSIE].[dbo].almacen alm ON art.alm_id = alm.alm_id "
                            + " WHERE alm.ran_id = " + ran_id + " AND alm.alm_tipo = 3 AND CONVERT(DATE, art.art_fecha) = '" + fecha.ToString("yyyy-MM-dd") + "' "
                            + " ) T) X "
                            + " GROUP BY X.Almacen, X.Clave";
                }
                else
                {
                    existencia = "SELECT art.alm_id AS Almacen, art.art_fecha AS Fecha, art.art_clave AS Clave, art.art_existencia AS Existencia "
                                 + " FROM articulo art "
                                 + " LEFT JOIN[DBSIE].[dbo].almacen alm ON art.alm_id = alm.alm_id "
                                 + " WHERE alm.ran_id = " + ran_id + " AND alm.alm_tipo = 3 AND CONVERT(DATE, art.art_fecha) = '" + fecha.ToString("yyyy-MM-dd") + "' ";
                }

                query = "SELECT DISTINCT  ISNULL(FORMAT(CONVERT(DATE,sie.Fecha) , 'd', 'en-gb' ),'" + fecha.ToString("dd/MM/yyyy") + "')  AS FECHA,  p.prod_clave AS CLAVE, p.prod_nombre AS ARTICULO, "
                + " ISNULL(sie.Existencia, 0) AS EXISTENCIASIE, ISNULL(bascula.Peso, 0) AS BASCULA, ISNULL(tracker.Peso, 0) AS TRACKER, "
                + " IIF(ISNULL(sie.Existencia, 0) >= IIF(ISNULL(tracker.Peso, 0) > 0, ISNULL(bascula.Peso, 0), 0), IIF(sie.Existencia > 0,1,0), 0) AS EXISTENCIA, "
                + " ISNULL(bascula.Peso, 0)-ISNULL(tracker.Peso, 0) AS DIFKG, IIF(tracker.Peso > 0, (ISNULL(bascula.Peso, 0) - ISNULL(tracker.Peso, 0)) / tracker.Peso * 100, 0) AS DIFPORC  "
                + " FROM producto p "
                + " LEFT JOIN( "
                + existencia
                + ") sie ON p.prod_clave = sie.Clave "
                + " LEFT JOIN ("
                + " SELECT T.Clave, SUM(T.Peso) AS Peso "
                + " FROM( "
                + " SELECT ing_clave AS Clave, SUM(rac_mh) AS Peso "
                + " FROM racion "
                + " WHERE ran_id IN(" + ranchosId + ") AND rac_fecha >= '" + racion_inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racion_fin.ToString("yyyy-MM-dd HH:mm") + "' "
                + " AND SUBSTRING(rac_descripcion, 3, 2) NOT IN('00', '01', '02') "
                + " AND SUBSTRING(ing_clave, 1, 4) IN('ALFO') "
                + " GROUP BY ing_clave "
                + " UNION "
                + " SELECT R.Clave, SUM(R.Peso) AS Peso "
                + " FROM( "
                + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, IIF(T.Pmz = '', T.Ing1, T.Ing2) AS Ing, "
                + " T.Peso1 * T.Porc AS Peso "
                + " FROM( "
                + " SELECT T1.Clave AS Clave1, T1.Ing AS Ing1, T1.Peso AS Peso1, ISNULL(T2.Pmz, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porc, 1) AS Porc "
                + " FROM( "
                + " SELECT R.Clave, R.Ing, SUM(R.Peso) AS Peso "
                + " FROM( "
                + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, T1.Fecha "
                + " FROM( "
                + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, MAX(rac_fecha) AS Fecha "
                + " FROM racion "
                + " WHERE ran_id IN(" + ranchosId + ") AND rac_fecha >= '" + racion_inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racion_fin.ToString("yyyy-MM-dd HH:mm") + "' "
                + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 "
                + " AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                + " GROUP BY ing_descripcion ) T1 "
                + " LEFT JOIN( "
                    + " SELECT pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                    + " FROM porcentaje_Premezcla "
                + " )T2 ON T1.Pmz = T2.Pmz) R "
                + " GROUP BY  R.Clave, R.Ing) T1 "
                + " LEFT JOIN( "
                    + " SELECT pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                    + " FROM porcentaje_Premezcla "
                + " )T2 ON T1.Ing = T2.Pmz) T) R "
                + " WHERE SUBSTRING(R.Clave, 1, 4) IN('ALFO') "
                + " GROUP BY R.Clave ) T "
                + " GROUP BY T.Clave "
                + " ) tracker ON tracker.Clave = p.prod_clave"
                + " LEFT JOIN( " + bascula + " ) bascula ON bascula.CLAVE = p.prod_clave "
                + " WHERE(p.prod_clave like 'ALFO%'  OR prod_clave IN(select ingt_clave from ingrediente_tracker where ingt_clave like 'ALFO%')) " + condicion;
            }
            else
            {
                string alm_externos = AlmacenesExternos();
                condicion = " AND sie.Existencia IS NOT NULL AND (tracker.Peso > 0 OR bascula.Peso > 0) ORDER BY CONSUMOREAL DESC";
                query = "SELECT DISTINCT  ISNULL(FORMAT(CONVERT(DATE,sie.Fecha) , 'd', 'en-gb' ),'')  AS FECHA,  p.prod_clave AS CLAVE, p.prod_nombre AS ARTICULO, "
               + " ISNULL(sie.Existencia, 0) AS EXISTENCIASIE, ISNULL(bascula.Peso, 0) - ISNULL(externo.Existencia, 0) AS CONSUMOREAL, ISNULL(tracker.Peso, 0) AS TRACKER, "
               + " IIF(ISNULL(sie.Existencia, 0) >= IIF(ISNULL(tracker.Peso, 0) > 0, ISNULL(bascula.Peso, 0), 0), 1, 0) AS EXISTENCIA, "
               + " (ISNULL(bascula.Peso, 0) - ISNULL(externo.Existencia, 0)) -ISNULL(tracker.Peso, 0)  AS DIFKG, "
               + " IIF(tracker.Peso > 0, ((ISNULL(bascula.Peso, 0) - ISNULL(externo.Existencia, 0)) - ISNULL(tracker.Peso, 0)) / tracker.Peso * 100, 0) AS DIFPORC,  "
               + "  ISNULL(bascula.Peso, 0) AS BASCULA, ISNULL(externo.Existencia, 0) AS ConsumoExterno "
               + " FROM producto p "
               + " LEFT JOIN( "
               + " SELECT art_clave AS Clave, art_existencia AS Existencia, art_precio_uni AS Precio, alm_id AS Almacen, T.Fecha "
               + " FROM( "
               + " SELECT alm_id AS Alm, art_clave AS Clave, MAX(art_fecha) AS Fecha "
               + " FROM articulo "
               + " where alm_id IN(" + f_alm_id + ") "
               + " GROUP BY art_clave, alm_id) T "
               + " LEFT JOIN articulo art ON art.art_clave = T.Clave AND T.Alm = art.alm_id AND T.Fecha = art.art_fecha"
               + ") sie ON p.prod_clave = sie.Clave "
               + " LEFT JOIN ("
               + " SELECT T.Clave, SUM(T.Peso) AS Peso "
               + " FROM( "
               + " SELECT ing_clave AS Clave, SUM(rac_mh) AS Peso "
               + " FROM racion "
               + " WHERE ran_id IN(" + ranchosId + ") AND rac_fecha >= '" + racion_inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racion_fin.ToString("yyyy-MM-dd HH:mm") + "' "
               + " AND SUBSTRING(rac_descripcion, 3, 2) NOT IN('00', '01', '02') "
               + " AND SUBSTRING(ing_clave, 1, 4) IN('ALFO') "
               + " GROUP BY ing_clave "
               + " UNION "
               + " SELECT R.Clave, SUM(R.Peso) AS Peso "
               + " FROM( "
               + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, IIF(T.Pmz = '', T.Ing1, T.Ing2) AS Ing, "
               + " T.Peso1 * T.Porc AS Peso "
               + " FROM( "
               + " SELECT T1.Clave AS Clave1, T1.Ing AS Ing1, T1.Peso AS Peso1, ISNULL(T2.Pmz, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porc, 1) AS Porc "
               + " FROM( "
               + " SELECT R.Clave, R.Ing, SUM(R.Peso) AS Peso "
               + " FROM( "
               + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, T1.Fecha "
               + " FROM( "
               + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, MAX(rac_fecha) AS Fecha "
               + " FROM racion "
               + " WHERE ran_id IN(" + ranchosId + ") AND rac_fecha >= '" + racion_inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racion_fin.ToString("yyyy-MM-dd HH:mm") + "' "
               + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 "
               + " AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
               + " GROUP BY ing_descripcion ) T1 "
               + " LEFT JOIN( "
                   + " SELECT pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                   + " FROM porcentaje_Premezcla "
               + " )T2 ON T1.Pmz = T2.Pmz) R "
               + " GROUP BY  R.Clave, R.Ing) T1 "
               + " LEFT JOIN( "
                   + " SELECT pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                   + " FROM porcentaje_Premezcla "
               + " )T2 ON T1.Ing = T2.Pmz) T) R "
               + " WHERE SUBSTRING(R.Clave, 1, 4) IN('ALFO') "
               + " GROUP BY R.Clave ) T "
               + " GROUP BY T.Clave "
               + " ) tracker ON tracker.Clave = p.prod_clave"
               + " LEFT JOIN( " + bascula + " ) bascula ON bascula.CLAVE = p.prod_clave "
               + "LEFT JOIN( "
                + " SELECT art_clave AS Clave, SUM(art_existencia) AS Existencia "
                + " FROM( "
                + " SELECT alm_id AS Alm, art_clave AS Clave, MAX(art_fecha) AS Fecha  FROM articulo  where alm_id IN(" + alm_externos + ")  GROUP BY art_clave, alm_id) T "
                + " LEFT JOIN articulo art ON art.art_clave = T.Clave AND T.Alm = art.alm_id AND T.Fecha = art.art_fecha "
                + " GROUP BY art_clave )externo ON p.prod_clave = externo.Clave "
               + " WHERE(p.prod_clave like 'ALFO%'  OR prod_clave IN(select ingt_clave from ingrediente_tracker where ingt_clave like 'ALFO%')) " + condicion;
            }

            conn.QueryAlimento(query, out dt1);

            DataTable dt = new DataTable();
            if (conBasc == 3)
            {
                dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");//0
                dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");//1
                dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String"); //2
                dt.Columns.Add("EXISTENCIA SIE").DataType = System.Type.GetType("System.Double");//3
                dt.Columns.Add("INV FINAL").DataType = System.Type.GetType("System.Double");//4
                dt.Columns.Add("CONSUMO").DataType = System.Type.GetType("System.Double");//5
                dt.Columns.Add("%P").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("%T").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.Double");//6
                dt.Columns.Add("EXISTENCIA");//7
                dt.Columns.Add("DIF / KG").DataType = System.Type.GetType("System.Double");//8
                dt.Columns.Add("% DIF").DataType = System.Type.GetType("System.Double");//9

                double sie, bal, track, dif, porcdif, inv;
                int exist;
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    sie = Convert.ToDouble(dt1.Rows[i][3]);
                    //bal = Convert.ToDouble(dt1.Rows[i][4]);
                    track = Convert.ToDouble(dt1.Rows[i][5]);
                    exist = Convert.ToInt32(dt1.Rows[i][6]);
                    inv = sie > 0 ? track > 0 ? 1 : sie : 0;
                    bal = sie - inv;
                    dif = Convert.ToDouble(dt1.Rows[i][7]);
                    porcdif = Convert.ToDouble(dt1.Rows[i][8]);
                    sumBascula += bal;
                    sumTracker += track;

                    DataRow dr = dt.NewRow();
                    dr["FECHA"] = dt1.Rows[i][0].ToString();
                    dr["CLAVE"] = dt1.Rows[i][1].ToString();
                    dr["ARTICULO"] = dt1.Rows[i][2].ToString();
                    dr["EXISTENCIA SIE"] = sie;
                    dr["INV FINAL"] = inv;
                    dr["CONSUMO"] = bal;
                    dr["TRACKER"] = track;
                    dr["EXISTENCIA"] = exist == 1 ? '✔' : 'X';
                    dr["DIF / KG"] = bal - track;
                    dr["% DIF"] = (bal - track) / track;
                    dt.Rows.Add(dr);
                }

                double cons;
                consumoAlfo = sumBascula;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cons = Convert.ToDouble(dt.Rows[i]["CONSUMO"]);
                    dt.Rows[i]["%P"] = sumBascula > 0 ? cons / sumBascula * 100 : 0;
                    sumP += sumBascula > 0 ? cons / sumBascula * 100 : 0;
                }

                DataRow row = dt.NewRow();
                row["ARTICULO"] = "TOTAL";
                row["CONSUMO"] = sumBascula;
                row["TRACKER"] = sumTracker;
                row["DIF / KG"] = sumBascula - sumTracker;
                row["% DIF"] = (sumBascula - sumTracker) / sumBascula * 100;
                row["%P"] = sumP;
                dt.Rows.Add(row);

            }
            else if (conBasc == 4)
            {
                dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");//0
                dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");//1
                dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String"); //2
                dt.Columns.Add("EXISTENCIA SIE").DataType = System.Type.GetType("System.Double");//3
                dt.Columns.Add("CONSUMO REAL").DataType = System.Type.GetType("System.Double");//4
                dt.Columns.Add("%P").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("%T").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.Double");//5
                dt.Columns.Add("EXISTENCIA");//6
                dt.Columns.Add("DIF / KG").DataType = System.Type.GetType("System.Double");//7
                dt.Columns.Add("% DIF").DataType = System.Type.GetType("System.Double");//8
                dt.Columns.Add("Bascula").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("Consumo Externo").DataType = System.Type.GetType("System.Double");

                double bal, track, cons;
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    int existencia = Convert.ToInt32(dt1.Rows[i][6]);
                    bal = Convert.ToDouble(dt1.Rows[i][4]);
                    track = Convert.ToDouble(dt1.Rows[i][5]);
                    sumBascula += bal;
                    sumTracker += track;
                    //cons = Convert.ToDouble(dt1.Rows[i][4]);
                    //sumP += bal > 0 ? cons / sumBascula * 100 : 0;
                    DataRow row = dt.NewRow();
                    row["FECHA"] = dt1.Rows[i][0].ToString();
                    row["CLAVE"] = dt1.Rows[i][1].ToString();
                    row["ARTICULO"] = dt1.Rows[i][2].ToString();
                    row["EXISTENCIA SIE"] = Convert.ToDouble(dt1.Rows[i][3]);
                    row["CONSUMO REAL"] = Convert.ToDouble(dt1.Rows[i][4]);
                    row["TRACKER"] = Convert.ToDouble(dt1.Rows[i][5]); ;
                    row["EXISTENCIA"] = existencia == 1 ? '✔' : 'X';
                    row["DIF / KG"] = Convert.ToDouble(dt1.Rows[i][7]);
                    row["% DIF"] = Convert.ToDouble(dt1.Rows[i][8]);
                    row["Bascula"] = Convert.ToDouble(dt1.Rows[i][9]);
                    row["Consumo Externo"] = Convert.ToDouble(dt1.Rows[i][10]);
                    dt.Rows.Add(row);
                }

                DataRow dr = dt.NewRow();
                dr["ARTICULO"] = "TOTAL";
                dr["CONSUMO REAL"] = sumBascula;
                dr["TRACKER"] = sumTracker;
                dr["DIF / KG"] = sumBascula - sumTracker;
                dr["% DIF"] = (sumBascula - sumTracker) / sumBascula * 100;
                //dr["%P"] = sumP;
                dt.Rows.Add(dr);

            }
            else
            {
                dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");//0
                dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");//1
                dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String"); //2
                dt.Columns.Add("EXISTENCIA SIE").DataType = System.Type.GetType("System.Double");//3
                dt.Columns.Add("CONSUMO").DataType = System.Type.GetType("System.Double");//4
                dt.Columns.Add("%P").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("%T").DataType = System.Type.GetType("System.Double");
                dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.Double");//5
                dt.Columns.Add("EXISTENCIA");//6
                dt.Columns.Add("DIF / KG").DataType = System.Type.GetType("System.Double");//7
                dt.Columns.Add("% DIF").DataType = System.Type.GetType("System.Double");//8

                double sie, bal, track, dif, porcdif;
                int exist;
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    sie = Convert.ToDouble(dt1.Rows[i][3]);
                    bal = Convert.ToDouble(dt1.Rows[i][4]);
                    track = Convert.ToDouble(dt1.Rows[i][5]);
                    exist = Convert.ToInt32(dt1.Rows[i][6]);
                    dif = Convert.ToDouble(dt1.Rows[i][7]);
                    porcdif = Convert.ToDouble(dt1.Rows[i][8]);
                    sumBascula += bal;
                    sumTracker += track;

                    DataRow dr = dt.NewRow();
                    dr["FECHA"] = dt1.Rows[i][0].ToString();
                    dr["CLAVE"] = dt1.Rows[i][1].ToString();
                    dr["ARTICULO"] = dt1.Rows[i][2].ToString();
                    dr["EXISTENCIA SIE"] = sie;
                    dr["CONSUMO"] = bal;
                    dr["TRACKER"] = track;
                    dr["EXISTENCIA"] = exist == 1 ? '✔' : 'X';
                    dr["DIF / KG"] = dif;
                    dr["% DIF"] = porcdif;
                    dt.Rows.Add(dr);
                }


                double cons;
                consumoAlfo = sumBascula;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cons = Convert.ToDouble(dt.Rows[i]["CONSUMO"]);
                    dt.Rows[i]["%P"] = sumBascula > 0 ? cons / sumBascula * 100 : 0;
                    sumP += sumBascula > 0 ? cons / sumBascula * 100 : 0;
                }


                DataRow row = dt.NewRow();
                row["ARTICULO"] = "TOTAL";
                row["CONSUMO"] = sumBascula;
                row["TRACKER"] = sumTracker;
                row["DIF / KG"] = sumBascula - sumTracker;
                row["% DIF"] = (sumBascula - sumTracker) / sumBascula * 100;
                row["%P"] = sumP;
                dt.Rows.Add(row);

            }

            dataGridView2.DataSource = dt;
            dataGridView2.AutoResizeColumns();
            bool tb = conBasc == 3;
            FormatoGrid(dataGridView2, conBasc == 3);
            if (ran_bascula == 0)
            {
                dataGridView2.Columns[4].Visible = false;
                dataGridView2.Columns[9].Visible = false;
                dataGridView2.Columns[10].Visible = false;
            }
        }

        private void FillDGVAlimento()
        {
            double sumBascula = 0, sumTracker = 0, sumP = 0, sumT = 0;
            DateTime inicio = new DateTime(fecha.Year, fecha.Month, 1, hora_corte, 0, 0);
            inicio = inicio.AddDays(dias_a);
            DateTime corte = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora_corte, 0, 0);
            corte = inicio.Day == 1 && hora_corte > 0 ? corte.AddDays(1) : corte;
            DateTime fec_reg = new DateTime(fecha.Year, fecha.Month, 1, fecha.Hour, 0, 0);
            fec_reg = fec_reg.AddMonths(1).AddDays(-1);
            string almacen = "";
            string query;
            string condicion = checkBox2.Checked ? " AND (sie.Existencia IS NOT NULL OR tracker.Peso IS NOT NULL) AND (sie.Existencia > 0 OR tracker.peso > 0) AND tracker.Peso > 0 " : "";
            DataTable dtV = new DataTable();

            string qry = "SELECT ISNULL(FORMAT(pro_fecha , 'd', 'en-gb' ),'" + fecha.ToString("dd/MM/yyyy") + "') , alm_id, art_clave , prod_nombre, pro_existencia_sie, pro_inv_final, pro_consumo, pro_porc_b, pro_porc_t, pro_consumo_tra, pro_dif_kg, pro_dif "
                + " FROM prorrateoTemp "
                + "where ran_id = " + ran_id.ToString() + " AND pro_fecha_reg = '" + fec_reg.ToString("yyyy-MM-dd") + "' "
                + "  ";
            conn.QueryAlimento(qry, out dtV);

            DataTable dt = new DataTable();
            ColumnasAlimento(out dt);

            if (dtV.Rows.Count > 0)
            {
                for (int i = 0; i < dtV.Rows.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["FECHA"] = dtV.Rows[i][0];
                    dr["ALMACEN"] = dtV.Rows[i][1];
                    dr["CLAVE"] = dtV.Rows[i][2];
                    dr["ARTICULO"] = dtV.Rows[i][3];
                    dr["DISPONIBLE SIE"] = dtV.Rows[i][4];
                    dr["INV FINAL"] = dtV.Rows[i][5];
                    dr["CONSUMO"] = dtV.Rows[i][6];
                    dr["%P"] = dtV.Rows[i][7];
                    dr["%T"] = dtV.Rows[i][8];
                    dr["CONSUMO TRACKER"] = dtV.Rows[i][9];
                    dr["DIF/KG"] = dtV.Rows[i][10];
                    dr["% DIF"] = dtV.Rows[i][11];
                    dr["Existencia"] = Convert.ToDouble(dtV.Rows[i][4]) > Convert.ToDouble(dtV.Rows[i][6]) ? '✔' : 'X';
                    dt.Rows.Add(dr);
                }
            }
            else
            {
                DataTable dt1;
                query = " SELECT ISNULL(FORMAT(pro_fecha , 'd', 'en-gb' ),'" + fecha.ToString("dd/MM/yyyy") + "'), alm_id, art_clave , prod_nombre, pro_existencia_sie, pro_inv_final, pro_consumo, pro_porc_b, pro_porc_t ,pro_consumo_tra, pro_dif_kg, pro_dif FROM prorrateo "
                    + "where ran_id = " + ran_id.ToString() + " AND pro_fecha_reg = '" + fec_reg.ToString("yyyy-MM-dd") + "' AND SUBSTRING(art_clave,1,4) LIKE 'ALAS' "
                    + " ORDER BY pro_consumo_tra desc ";
                conn.QueryAlimento(query, out dt1);

                if (dt1.Rows.Count > 0)
                {
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr["FECHA"] = dt1.Rows[i][0];
                        dr["ALMACEN"] = dt1.Rows[i][1];
                        dr["CLAVE"] = dt1.Rows[i][2];
                        dr["ARTICULO"] = dt1.Rows[i][3];
                        dr["DISPONIBLE SIE"] = dt1.Rows[i][4];
                        dr["INV FINAL"] = dt1.Rows[i][5];
                        dr["CONSUMO"] = dt1.Rows[i][6];
                        dr["%P"] = dt1.Rows[i][7];
                        dr["%T"] = dt1.Rows[i][8];
                        dr["CONSUMO TRACKER"] = dt1.Rows[i][9];
                        dr["DIF/KG"] = dt1.Rows[i][10];
                        dr["% DIF"] = dt1.Rows[i][11];
                        dr["Existencia"] = Convert.ToDouble(dt1.Rows[i][4]) > Convert.ToDouble(dt1.Rows[i][6]) ? '✔' : 'X';
                        dt.Rows.Add(dr);
                        sumP += Convert.ToDouble(dt.Rows[i][7]);
                        sumT += Convert.ToDouble(dt.Rows[i][8]);
                    }
                    button6.Enabled = false;
                    button5.Enabled = false;
                    button2.Enabled = false;
                }
                else
                {
                    string pm1 = ran_id > 9 ? "'" + ran_id.ToString() + "00'" : "'0" + ran_id.ToString() + "00'";
                    string pm2 = ran_id > 9 ? "'" + ran_id.ToString() + "01'" : "'0" + ran_id.ToString() + "01'";
                    string pm3 = ran_id > 9 ? "'" + ran_id.ToString() + "02'" : "'0" + ran_id.ToString() + "02'";
                    string existencia = "";
                    if (ran_id == 25)
                    {
                        existencia = "SELECT R.Almacen,  MAX(R.Fecha) AS Fecha, R.Clave, SUM(R.Existencia) AS Existencia "
                                    + " FROM( "
                                    + " SELECT IIF(T.Almacen = 'A41002', 'A40002', T.Almacen) AS Almacen, T.Fecha, T.Clave, T.Existencia "
                                    + " FROM( "
                                    + " SELECT art.alm_id AS Almacen, art.art_fecha AS Fecha, art.art_clave AS Clave, SUM(art.art_existencia) AS Existencia "
                                    + " FROM articulo art "
                                    + " LEFT JOIN[DBSIE].[dbo].almacen alm ON art.alm_id = alm.alm_id "
                                    + " WHERE alm.ran_id = " + ran_id + " AND alm.alm_tipo = 2 AND CONVERT(DATE, art.art_fecha) = '" + fecha.ToString("yyyy-MM-dd") + "' "
                                    + " GROUP BY art.alm_id, art.art_fecha, art.art_clave) T) R "
                                    + " GROUP BY R.Almacen, R.Clave";
                        almacen = "'A40002'";
                    }
                    else
                    {
                        existencia = " SELECT art.alm_id AS Almacen, art.art_fecha AS Fecha, art.art_clave AS Clave, art.art_existencia AS Existencia "
                                + " FROM articulo art "
                                + " LEFT JOIN[DBSIE].[dbo].almacen alm ON art.alm_id = alm.alm_id "
                                + " WHERE alm.ran_id = " + ran_id + " AND alm.alm_tipo = 2 AND CONVERT(DATE, art.art_fecha) = '" + fecha.ToString("yyyy-MM-dd") + "' ";
                        almacen = ali_alm_id;
                    }



                    DataTable dt2;
                    double consumo, tracker, sie, invfinal, porcdif, difkg;
                    query = "SELECT DISTINCT ISNULL(FORMAT(sie.Fecha,'d','en-gb'),'" + fecha.ToString("dd/MM/yyyy") + "') AS Fecha, ISNULL(sie.Almacen, " + almacen + ") AS Almacen, p.prod_clave AS Clave, p.prod_nombre AS Articulo, ISNULL(sie.Existencia, 0) AS ExistenciaSIE, "
                            + " ISNULL(tracker.Peso, 0) AS ConsumoTracker "
                            + " FROM producto p "
                            + " LEFT JOIN( "
                            + existencia
                            //+ " SELECT art.alm_id AS Almacen, art.art_fecha AS Fecha, art.art_clave AS Clave, art.art_existencia AS Existencia "
                            //+ " FROM articulo art "
                            //+ " LEFT JOIN[DBSIE].[dbo].almacen alm ON art.alm_id = alm.alm_id "
                            //+ " WHERE alm.ran_id = " + ran_id + " AND alm.alm_tipo = 2 AND CONVERT(DATE, art.art_fecha) = '" + fecha_SIE.ToString("yyyy-MM-dd") + "' "
                            + " ) sie ON sie.Clave = p.prod_clave "
                            + " LEFT JOIN( "
                                + " SELECT X.Clave, SUM(x.Peso) AS Peso "
                                + " FROM ( "
                                    + " SELECT ing_clave AS Clave, ing_descripcion AS Ing, SUM(rac_mh) AS Peso "
                                    + " FROM racion "
                                    + " WHERE ran_id = " + ran_id
                                    + " AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + corte.ToString("yyyy-MM-dd HH:mm") + "' "
                                    + " AND SUBSTRING(rac_descripcion,3,2) NOT IN('00','01','02') "
                                    + " AND SUBSTRING(ing_clave,1,4) IN('ALAS') "
                                    + " GROUP BY ing_clave, ing_descripcion "
                                    + " UNION "
                                    + " SELECT R.Clave, R.Ing, SUM(R.Peso) "
                                    + " FROM( "
                                        + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, IIF(T.Pmz = '', T.Ing1, T.Ing2) AS Ing, T.Peso1 * T.Porc AS Peso "
                                        + " FROM( "
                                            + " SELECT T1.Clave AS Clave1, T1.Ing AS Ing1, T1.Peso AS Peso1, ISNULL(T2.Pmz, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, "
                                            + " ISNULL(T2.Porc, 1) AS Porc "
                                            + " FROM( "
                                                + " SELECT R.Clave, R.Ing, SUM(R.Peso) AS Peso "
                                                + " FROM( "
                                                    + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso "
                                                    + " FROM( "
                                                        + " select ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                                                        + " FROM racion "
                                                        + " WHERE ran_id = " + ran_id
                                                        + " AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + corte.ToString("yyyy-MM-dd HH:mm") + "' "
                                                        + " AND SUBSTRING(rac_descripcion, 3, 2) not in ('00','01','02') "
                                                        + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 "
                                                        + " AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                                                        + " GROUP BY ing_descripcion) T1 "
                                                    + " LEFT JOIN( "
                                                        + " select pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                                                        + " FROM porcentaje_Premezcla "
                                                        + " )T2 ON T1.Pmz = T2.Pmz) R "
                                                + " GROUP BY R.Clave, R.Ing) T1 "
                                            + " LEFT JOIN( "
                                                + " SELECT pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                                                + " FROM porcentaje_Premezcla "
                                            + " ) T2 ON T1.Ing = T2.Pmz) T) R "
                                    + " WHERE SUBSTRING(R.Clave, 1, 4)  IN('ALAS') "
                                    + " GROUP BY R.Clave, R.Ing ) X "
                            + " GROUP BY X.Clave ) tracker ON tracker.Clave = p.prod_clave "
                        + " WHERE SUBSTRING(p.prod_clave,1,4) LIKE 'ALAS' " + condicion
                        + " ORDER BY ConsumoTracker DESC, ExistenciaSIE DESC";
                    conn.QueryAlimento(query, out dt2);

                    for (int i = 0; i < dt2.Rows.Count; i++)
                    {
                        sie = Convert.ToDouble(dt2.Rows[i][4]);
                        tracker = Convert.ToDouble(dt2.Rows[i][5]);

                        if (prorrateo == 3)
                        {
                            if (tracker > 0)
                                consumo = sie > 0 ? sie - 1 : 0;
                            else
                                consumo = 0;

                            invfinal = sie - consumo;
                            if (invfinal == 0 && sie > 0)
                            {
                                invfinal = 1;
                                consumo = consumo - 1;
                            }
                        }
                        else
                        {
                            if (tracker > 0)
                                consumo = tracker < sie ? tracker : sie;
                            else
                                consumo = 0;

                            invfinal = sie - consumo;
                            if (invfinal == 0 && sie > 0)
                            {
                                invfinal = 1;
                                consumo = consumo - 1;
                            }
                        }

                        difkg = consumo - tracker;
                        porcdif = tracker > 0 ? difkg / tracker * 100 : 0;
                        DataRow dr = dt.NewRow();
                        dr["FECHA"] = dt2.Rows[i][0];
                        dr["ALMACEN"] = dt2.Rows[i][1];
                        dr["CLAVE"] = dt2.Rows[i][2];
                        dr["ARTICULO"] = dt2.Rows[i][3];
                        dr["DISPONIBLE SIE"] = sie;
                        dr["INV FINAL"] = invfinal;
                        dr["CONSUMO"] = consumo;
                        dr["CONSUMO TRACKER"] = tracker;
                        dr["DIF/KG"] = difkg;
                        dr["% DIF"] = porcdif;
                        dr["Existencia"] = sie > consumo ? '✔' : 'X';
                        dt.Rows.Add(dr);
                    }
                }
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sumBascula += Convert.ToDouble(dt.Rows[i]["CONSUMO"]);
                sumTracker += Convert.ToDouble(dt.Rows[i]["CONSUMO TRACKER"]);

            }

            double cons;
            consumoAlas = sumBascula;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                cons = Convert.ToDouble(dt.Rows[i]["CONSUMO"]);
                dt.Rows[i]["%P"] = sumBascula > 0 ? cons / sumBascula * 100 : 0;
                //sumP += sumBascula > 0 ? cons / sumBascula * 100 : 0;
            }

            DataRow drT = dt.NewRow();
            drT["ARTICULO"] = "TOTAL";
            drT["CONSUMO"] = sumBascula;
            drT["CONSUMO TRACKER"] = sumTracker;
            drT["DIF/KG"] = sumBascula - sumTracker;
            drT["% DIF"] = (sumBascula - sumTracker) / sumTracker * 100;
            drT["%P"] = sumP;
            //drT["%T"] = sumT;
            dt.Rows.Add(drT);

            dataGridView1.DataSource = dt;
            dataGridView1.AutoResizeColumns();
            FormatoGrid(dataGridView1, 4);
            if (conBasc == 3)
                BloquearDGV(dataGridView1);

        }

        private void CargarPremezcla(string premezcla, DateTime inicio, DateTime fin)
        {
            try
            {
                DateTime fRacion, fIng;
                DateTime fin2 = inicio.AddDays(1);
                DateTime fpmI = inicio, fpmF = new DateTime();
                int temp = 0;
                DataTable dt, dtAux;
                DataTable dt1 = new DataTable();
                string pmz, clave, ingrediente, valores = "", prmz, query;
                double porcentaje, porcentajeseca;
                prmz = premezcla[2].ToString() + premezcla[3];
                query = "SELECT * FROM porcentaje_Premezcla WHERE pmez_descripcion like '" + premezcla + "'";
                conn.QueryAlimento(query, out dtAux);
                int repeticiones = 0;
                if (dtAux.Rows.Count == 0)
                {
                    if (prmz == "01")
                    {
                        query = @"SELECT
	                                   T1.Pmz
                                      ,T1.Clave
                                      ,T1.Ing
	                                  ,T1.Peso / T2.Total
	                                  ,SEC2.Peso / SEC.Peso
                                FROM 
                                (
	                                SELECT  T.pmez_racion    AS Pmz
	                                       ,T.ing_clave      AS Clave
	                                       ,T.ing_nombre     AS Ing
	                                       ,SUM(T.pmez_peso) AS Peso
	                                FROM 
	                                (
		                                SELECT  DISTINCT *
		                                FROM premezcla
		                                WHERE pmez_racion  LIKE '" + premezcla + @"'
                                        AND pmez_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + @"'
                                        AND pmez_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + @"'
	                                ) T
	                                GROUP BY  pmez_racion
	                                         ,ing_clave
	                                         ,ing_nombre 
                                ) T1
                                LEFT JOIN 
                                (
	                                SELECT  T.pmez_racion    AS Pmz
	                                       ,SUM(T.pmez_peso) AS Total
	                                FROM 
	                                (
		                                SELECT  DISTINCT *
		                                FROM premezcla
		                                WHERE pmez_racion  LIKE '" + premezcla + @"'
                                        AND pmez_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + @"'
                                        AND pmez_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + @"'
	                                ) T
		                                GROUP BY  T.pmez_racion
                                )       T2 ON T1.Pmz = T2.Pmz
                                LEFT JOIN( 
		                                SELECT rac_descripcion AS Pmez, SUM(rac_ms) AS Peso 
		                                FROM racion 
		                                WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + @"'
		                                AND rac_fecha  < '" + fin.ToString("yyyy-MM-dd HH:mm") + @"'
		                                AND rac_descripcion LIKE '" + premezcla + @"' 
		                                GROUP BY rac_descripcion
                                )
		                                SEC ON SEC.Pmez = T1.Pmz
                                LEFT JOIN( 
		                                SELECT rac_descripcion AS Pmez, ing_clave AS Clave, SUM(rac_ms) AS Peso 
			                                FROM racion 
			                                WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + @"'
		                                    AND rac_fecha  < '" + fin.ToString("yyyy-MM-dd HH:mm") + @"'
		                                    AND rac_descripcion LIKE '" + premezcla + @"' 
		                                GROUP BY rac_descripcion, ing_clave
                                )
                                SEC2 ON SEC2.Clave = T1.Clave";

                        conn.QueryAlimento(query, out dt);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            pmz = dt.Rows[i][0].ToString();
                            clave = dt.Rows[i][1].ToString();
                            ingrediente = dt.Rows[i][2].ToString();
                            porcentaje = Convert.ToDouble(dt.Rows[i][3]);
                            porcentajeseca = Convert.ToDouble(dt.Rows[i][4]);
                            valores += "('" + pmz + "','" + clave + "','" + ingrediente + "'," + porcentaje + "," + porcentajeseca + "),";
                        }
                        if (valores.Length > 0)
                        {
                            valores = valores.Substring(0, valores.Length - 1);
                            conn.InsertMasivAlimento("porcentaje_Premezcla", valores);
                        }
                    }
                    else
                    {
                        DataTable dtv;
                        query = "SELECT * FROM premezcla WHERE pmez_racion like '" + premezcla + "'";
                        conn.QueryAlimento(query, out dtv);

                        if (dtv.Rows.Count > 0)
                        {
                            query = "SELECT T1.Premezcla, T1. Fecha AS PMIng, ISNULL(T2. Fecha, T3.Fecha) AS PMRac "
                                   + " FROM( "
                                   + " SELECT ing_descripcion AS Premezcla, MIN(rac_fecha) AS Fecha "
                                   + " FROM racion "
                                   + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                                   + " AND ing_descripcion like '" + premezcla + "'"
                                   + " GROUP BY ing_descripcion)T1 "
                                   + " LEFT JOIN( "
                                   + " SELECT rac_descripcion AS Premezcla, MIN(rac_fecha)  AS Fecha "
                                   + " FROM racion "
                                   + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                                   + " AND rac_descripcion like '" + premezcla + "' "
                                   + " GROUP BY rac_descripcion) T2 ON T1.Premezcla = T2.Premezcla "
                                   + " LEFT JOIN( "
                                   + " SELECT rac_descripcion AS Premezcla, MAX(rac_fecha)  AS Fecha "
                                   + " FROM racion "
                                   + " WHERE rac_fecha < '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_descripcion like '" + premezcla + "' "
                                   + " GROUP BY rac_descripcion "
                                   + " )T3 ON T1.Premezcla = T3.Premezcla";
                            conn.QueryAlimento(query, out dt);

                            fIng = Convert.ToDateTime(dt.Rows[0][1]);
                            fRacion = Convert.ToDateTime(dt.Rows[0][2]);
                            int comparacion = DateTime.Compare(fRacion, fIng);

                            if (comparacion == 1)
                            {
                                DataTable dtV2;
                                query = "SELECT TOP(5) * FROM premezcla where pmez_racion like '" + premezcla + "' AND pmez_fecha <= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "'";
                                conn.QueryAlimento(query, out dtV2);

                                if (dtV2.Rows.Count > 0)
                                {
                                    fpmI = inicio;
                                }
                                else
                                {
                                    do
                                    {
                                        if (repeticiones == 30)
                                            break;

                                        fpmI = inicio.AddDays(-1); fpmI = fpmI.AddDays(temp);
                                        fpmF = fin2.AddDays(-1); fpmF = fpmF.AddDays(temp);

                                        query = " SELECT * FROM premezcla WHERE pmez_racion like '" + premezcla + "' "
                                            + " AND pmez_fecha >= '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND pmez_fecha < '" + fpmF.ToString("yyyy-MM-dd HH:mm") + "' ";
                                        conn.QueryAlimento(query, out dt1);
                                        temp--;
                                        repeticiones++;
                                    }
                                    while (dt1.Rows.Count == 0);
                                }
                            }
                            else
                            {
                                if (fRacion.Hour < inicio.Hour)
                                {
                                    fpmI = new DateTime(fRacion.Year, fRacion.Month, fRacion.Day, inicio.Hour, 0, 0);
                                    fpmI = fpmI.AddDays(-1);
                                }
                                else
                                    fpmI = new DateTime(fRacion.Year, fRacion.Month, fRacion.Day, inicio.Hour, 0, 0);
                            }

                            DataTable dtsPM;
                            query = "SELECT DISTINCT ing_nombre "
                                    + "FROM premezcla "
                                    + " WHERE pmez_racion like '" + premezcla + "'"
                                    + " AND pmez_fecha >= '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND pmez_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                                    + " AND ISNUMERIC(SUBSTRING(ing_nombre,1,1)) > 0 "
                                    + " AND SUBSTRING(ing_nombre,3,2) IN('00', '01', '02')";
                            conn.QueryAlimento(query, out dtsPM);

                            DataTable dtV;
                            //DiasPremezcla(premezcla, fpmI, fin);
                            for (int i = 0; i < dtsPM.Rows.Count; i++)
                            {
                                query = "SELECT * FROM premezcla WHERE pmez_racion like '" + dtsPM.Rows[i][0].ToString() + "'";
                                conn.QueryAlimento(query, out dtV);

                                if (dtV.Rows.Count == 0)
                                    continue;

                                SupraMezcla(dtsPM.Rows[i][0].ToString(), fpmI, fin);
                            }


                            query = "INSERT INTO porcentaje_Premezcla "
                               + " SELECT T1.Pmez, T1.Clave, T1.Ing, T1.Peso / T2.Peso , T1.PesoSeco / T2.PesoSeco "
                                + " FROM( "
                                + " SELECT rac_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, SUM(rac_mh) AS Peso , SUM(rac_ms) AS PesoSeco "
                                + " FROM racion "
                                + " WHERE rac_fecha >= '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                                + " AND rac_descripcion LIKE '" + premezcla + "' "
                                + " GROUP BY rac_descripcion, ing_clave, ing_descripcion)T1 "
                                + " LEFT JOIN( "
                                + " SELECT rac_descripcion AS Pmez, SUM(rac_mh) AS Peso , SUM(rac_ms) AS PesoSeco"
                                + " FROM racion "
                                + " WHERE rac_fecha >= '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                                + " AND rac_descripcion LIKE '" + premezcla + "' "
                                + " GROUP BY rac_descripcion"
                                + " ) T2 ON T1.Pmez = T2.Pmez";

                            conn.InsertSelecttAlimento(query);
                        }
                    }
                }
            }
            catch (DbException ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void SupraMezcla(string premezcla, DateTime inicio, DateTime fin)
        {
            string pmz, ingCve, ing;
            double porc;
            DataTable dt, dtV;
            DateTime fini, ffin;
            DataTable dtF = new DataTable();
            string query = "SELECT * FROM porcentaje_Premezcla where pmez_descripcion like '" + premezcla + "'";
            conn.QueryAlimento(query, out dt);
            int temp = 0;
            int repeticiones = 0;
            if (dt.Rows.Count == 0)
            {
                query = "SELECT  TOP(5) * FROM premezcla WHERE pmez_racion like '" + premezcla + "' AND pmez_fecha <= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "'";
                conn.QueryAlimento(query, out dtV);

                if (dtV.Rows.Count > 0)
                {
                    while (dtF.Rows.Count == 0)
                    {

                        if (repeticiones == 30)
                            break;

                        fini = inicio.AddDays(temp);
                        ffin = fini.AddDays(1);

                        query = "SELECT * "
                            + " FROM premezcla "
                            + " WHERE pmez_fecha >= '" + fini.ToString("yyyy-MM-dd HH:mm") + "' AND pmez_fecha< '" + ffin.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND pmez_racion like '" + premezcla + "'";
                        conn.QueryAlimento(query, out dtF);
                        temp--;
                        repeticiones++;
                    }
                }
                else
                    fini = inicio;

                query = "INSERT INTO porcentaje_Premezcla "
                 + " SELECT T1.Pmz, T1.Clave, T1.Ing, (T1.Peso / T2.Peso) , SEC2.Peso / SEC.Peso "
                 + " FROM( "
                 + " SELECT pmez_racion AS Pmz, ing_clave AS Clave, ing_nombre AS Ing, SUM(pmez_peso) AS Peso "
                 + " FROM premezcla "
                 + " WHERE pmez_racion LIKE '" + premezcla + "' AND pmez_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND pmez_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                 + " GROUP BY pmez_racion, ing_clave, ing_nombre) T1 "
                 + " LEFT JOIN( "
                 + " SELECT pmez_racion AS Pmz, SUM(pmez_peso) AS Peso "
                 + " FROM premezcla "
                 + " WHERE pmez_racion LIKE '" + premezcla + "' AND pmez_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND pmez_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                 + " GROUP BY pmez_racion )T2 ON T1.Pmz = T2.Pmz" + @"
                    LEFT JOIN(
                                        SELECT rac_descripcion AS Pmez, SUM(rac_ms) AS Peso

                                        FROM racion

                                        WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + @"'

                                        AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + @"'

                                        AND rac_descripcion LIKE '" + premezcla + @"'

                                        GROUP BY rac_descripcion
                                )

                                        SEC ON SEC.Pmez = T1.Pmz
                                LEFT JOIN(
                                        SELECT rac_descripcion AS Pmez, ing_clave AS Clave, SUM(rac_ms) AS Peso
                                            FROM racion
                                            WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + @"'

                                            AND rac_fecha< '" + fin.ToString("yyyy-MM-dd HH:mm") + @"'

                                            AND rac_descripcion LIKE '" + premezcla + @"'

                                        GROUP BY rac_descripcion, ing_clave
                                )
                                SEC2 ON SEC2.Clave = T1.Clave";





                conn.InsertSelecttAlimento(query);
            }
        }

        private void getParameters()
        {
            //Obtencion de la fecha
            DateTime hoy = DateTime.Now;
            if (hoy.Day >= 1 && hoy.Day < 6)
            {
                int dia = 0;
                fecha = hoy.AddMonths(-1);
                if (fecha.Month == 1 || fecha.Month == 3 || fecha.Month == 5 || fecha.Month == 7 || fecha.Month == 8 || fecha.Month == 10 || fecha.Month == 12)
                    dia = 31;
                else if (fecha.Month == 2)
                    if ((fecha.Year % 4 == 0 && fecha.Year % 100 != 0) || fecha.Year % 400 == 0)
                        dia = 29;
                    else
                        dia = 28;
                else if (fecha.Month == 4 || fecha.Month == 6 || fecha.Month == 9 || fecha.Month == 11)
                    dia = 30;

                fecha = new DateTime(fecha.Year, fecha.Month, dia);
            }
            else
            {
                fecha = new DateTime(hoy.Year, hoy.Month, hoy.Day);
            }

            //Obtener del erp y version del tracker
            DataTable dt;
            string query = "SELECT c.erp_id, c.ran_bascula, c.track_id, c.emp_prorrateo, c.ran_emp_prorrateo, cr.cr_bascula "
                        + " FROM[DBSIO].[dbo].configuracion c "
                        + " LEFT JOIN[DBSIO].[dbo].configuracion_rancho cr ON c.ran_id = cr.ran_id "
                        + " WHERE c.ran_id = " + ran_id;
            conn.QuerySIO(query, out dt);
            string erp = dt.Rows[0][0].ToString();
            sUrl = sUrl.Replace("@", erp);
            ran_bascula = Convert.ToInt32(dt.Rows[0][1]);
            versionId = Convert.ToInt32(dt.Rows[0][2]);
            emp_prorrateo = Convert.ToInt32(dt.Rows[0][3]);
            rep = Convert.ToInt32(dt.Rows[0][4]);
            prorrateo = Convert.ToInt32(dt.Rows[0][5]);

            //Obtener de la hora corte
            DataTable dt1;
            string query1 = "select PARAMVALUE FROM bedrijf_params where name like 'DSTimeShift'";
            conn.QueryTracker(query1, out dt1);
            int temp = Convert.ToInt32(dt1.Rows[0][0]);
            dias_a = temp > 0 ? 0 : -1;
            hora_corte = 24 + temp;
            hora_corte = hora_corte == 24 ? 0 : hora_corte > 24 ? temp:hora_corte;

            DataTable dt2;
            string query2 = "SELECT Tracker.Rancho, Tracker.Fecha, ISNULL(Sie.Fecha,GETDATE()), ISNULL(Bascula.Fecha,GETDATE()) "
                            + " FROM( "
                            + " SELECT ran_id AS Rancho, MAX(rac_fecha) AS Fecha "
                            + " FROM racion "
                            + " WHERE ran_id = " + ran_id + " AND ing_polvo = 0 "
                            + " GROUP BY ran_id) Tracker "
                            + " LEFT JOIN( "
                            + " SELECT al.ran_id AS Rancho, MAX(ar.art_fecha) AS Fecha "
                            + " FROM articulo ar "
                            + " LEFT JOIN[DBSIE].[dbo].almacen al ON al.alm_id = ar.alm_id "
                            + " WHERE al.ran_id = " + ran_id
                            + " GROUP BY al.ran_id) Sie ON SIE.Rancho = Tracker.Rancho "
                            + " LEFT JOIN( "
                            + " SELECT bal.ran_id AS Rancho, MAX(bol_fecha) AS Fecha "
                            + " FROM boleto bol "
                            + " LEFT JOIN[DBSIE].[dbo].bascula bal ON bal.bal_clave = bol.bal_clave "
                            + " WHERE bal.ran_id = " + ran_id
                            + " GROUP BY bal.ran_id ) Bascula ON Bascula.Rancho = Tracker.Rancho";
            conn.QueryAlimento(query2, out dt2);

            fecha_Tra = Convert.ToDateTime(dt2.Rows[0][1]);
            fecha_SIE = Convert.ToDateTime(dt2.Rows[0][2]);
            fecha_Bal = ran_bascula == 1 ? Convert.ToDateTime(dt2.Rows[0][3]) : fecha_Tra;

            //obtener la ruta para guardar los pdfs
            DataTable dt3;
            string query3 = "select rut_ruta from ruta where ran_id = " + ran_id; ;
            conn.QuerySIO(query3, out dt3);
            ruta = dt3.Rows[0][0].ToString();

            //OBTENER si tiene bascula
            DataTable dt4;
            string query4 = "select bal_clave FROM bascula where ran_id = " + ran_id.ToString();
            conn.QuerySIE(query4, out dt4);

            if (dt4.Rows.Count > 0)
            {
                bal_clave = "";
                for (int i = 0; i < dt4.Rows.Count; i++)
                {
                    bal_clave += dt4.Rows[i][0].ToString() + ",";
                }

                bal_clave = bal_clave.Substring(0, bal_clave.Length - 1);
                bascula = true;
            }
            else
            {
                bal_clave = "0";
                bascula = false;
            }

            //OBTENER el almacen de alimento

            DataTable dt5;
            string query5 = "select alm_tipo,alm_id from [DBSIE].[dbo].almacen "
                            + " where ran_id = " + ran_id.ToString() + " and alm_tipo IN(2, 3) ";
            conn.QuerySIE(query5, out dt5);

            ali_alm_id = dt5.Rows.Count > 0 ? "" : "''";
            f_alm_id = dt5.Rows.Count > 0 ? "" : "''";
            for (int i = 0; i < dt5.Rows.Count; i++)
            {
                switch (Convert.ToInt32(dt5.Rows[i][0]))
                {
                    case 2:
                        ali_alm_id += "'" + dt5.Rows[i][1].ToString() + "',";
                        break;
                    case 3:
                        f_alm_id += "'" + dt5.Rows[i][1].ToString() + "',";
                        break;
                }
            }

            ali_alm_id = ali_alm_id.Remove(ali_alm_id.Length - 1, 1);
            f_alm_id = f_alm_id.Remove(f_alm_id.Length - 1, 1);
            //Obtener e
            ranNumero = ran_id > 9 ? ran_id.ToString() : "0" + ran_id.ToString();
            ranchosId = rep == 1 ? RanchosEP() : ran_id.ToString();

            DataTable dtC;
            string qry = "SELECT cr_bascula from configuracion_rancho WHERE ran_id = " + ran_id.ToString();
            conn.QuerySIO(qry, out dtC);
            conBasc = Convert.ToInt32(dtC.Rows[0][0]);
        }
        CheckTipificaciones checkTipificaciones;
        private void Prorrateo_Load(object sender, EventArgs e)
        {
            panel1Autorizar.Visible = false;
            almCerrados = true;
            btnGuardar = false;
            conn.Iniciar("DBSIE");
            load = true;
            dias_a = 0;
            getParameters();
            DateTime inicio = new DateTime(fecha.Year, fecha.Month, 1, hora_corte, 0, 0);
            inicio = inicio.AddDays(dias_a);
            DateTime corte = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora_corte, 0, 0);
            corte = inicio.Day == 1 && hora_corte > 0 ? corte.AddDays(1) : corte;
            tipificacion = new Tipificaciones(ranchosId, emp_id, inicio, corte);
            tipificacionCorrecta = tipificacion.TipificacionesCorrectas();
            porcDif = 0;
            porcT = 0;
            fecha_reg = new DateTime(fecha.Year, fecha.Month, 1);
            fecha_reg = fecha_reg.AddMonths(1).AddDays(-1);
            Porcentajes();
            SetTitulos();
            ValidarProrrateo();
            gth1746();
            checkBox2.Cursor = Cursors.Hand;
            checkBox3.Cursor = Cursors.Hand;
            checkBox4.Cursor = Cursors.Hand;
            button1.Cursor = Cursors.Hand;
            button2.Cursor = Cursors.Hand;
            button3.Cursor = Cursors.Hand;
            button4.Cursor = Cursors.Hand;
            button5.Cursor = Cursors.Hand;
            button6.Cursor = Cursors.Hand;
            txtPasword.PasswordChar = '\u25CF';
            txtPasword.Cursor = Cursors.Hand;
            txtPwdM.PasswordChar = '\u25CF';
            modT = false;
            modificarV = false;
            hb = false;
            modCTA = false;
            modCTF = false;
            porcTActivo = false;
            autorizar = false;
            //if (!button6.Enabled)
            BloquearDGV(dataGridView1);
            ////FormatoCeldaGrid(dataGridView1, 4, 7, 3);
            //fecha_reg = new DateTime(fecha.Year, fecha.Month, 1);
            //fecha_reg = fecha_reg.AddMonths(1).AddDays(-1);

            //if (!Permitir)
            //{
            if (DateTime.Today.Day > 2 && DateTime.Today.Day < 6)
            {
                //bool Alas = PermitirGuardar(dataGridView1, "ALIMENTOS", true, btnGuardar);
                //bool Alfo = PermitirGuardar(dataGridView2, "FORRAJE", true, btnGuardar);

                if (!ExisteProrrateo() && almCerrados && tipificacionCorrecta)
                {
                    button1.Visible = true;
                    button1.Enabled = true;
                    button5.Visible = true;
                    button6.Visible = true;
                    button9.Visible = true;
                    button10.Visible = true;
                    panelBtnProrrateo.Visible = true;
                }
                button3.Enabled = true;
                //panelBtnProrrateo.Visible = true;
                //panelBtnProrrateo.Visible = !ExportoProrrateo();
            }

            //}


            //Cambio de Fuente al iniciar pantalla para los DGVs
            this.dataGridView1.DefaultCellStyle.Font = new Font("Century Gothic", 9);
            this.dataGridView1.DefaultCellStyle.ForeColor = Color.Black;

            this.dataGridView2.DefaultCellStyle.Font = new Font("Century Gothic", 9);
            this.dataGridView2.DefaultCellStyle.ForeColor = Color.Black;


            label12.Text = ExisteProrrateo() ? "PRORRATEO GUARDADO" : "PRORRATEO SIN GUARDAR";
            label12.ForeColor = ExisteProrrateo() ? Color.Green : Color.Red;
            load = false;
            label13.Text = almCerrados ? "ALMACENES CERRRADOS" : "ALMACENES ABIERTOS";
            label13.ForeColor = almCerrados ? Color.Green : Color.Red;

            DateTime dateTime = DateTime.Now;
            DateTime dateTimeInicio = dateTime.AddDays(-(dateTime.Day));
            DataTable dtRaciones, dtIng;
            
            RacionMalTipificada(dateTimeInicio, dateTime, out dtRaciones);
            IngredienteMalTipíficado(ran_id.ToString(), dateTimeInicio, dateTime, out dtIng);

            DataTable dtALAS, dtALFO;
            ColumnasAlimento(out dtALAS);
            ColumnasForaje(out dtALFO);

            if (dtRaciones.Rows.Count > 0 || dtIng.Rows.Count > 0)
            {


                checkTipificaciones = new CheckTipificaciones(ranchosId, emp_id, dateTimeInicio, dateTime, dtALAS, dtALFO, conBasc);
                if (checkTipificaciones.ShowDialog() == DialogResult.OK)
                {
                    if(checkTipificaciones.Vpwd == false)
                    {
                        Console.WriteLine("No entrar a prorrateo");
                        this.Close();
                    } else
                    {

                    }

                }
            }


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

        private void Porcentajes()
        {
            DataTable dt;
            string query = "SELECT  pp_porc_dif, pp_porc_t FROM porcentaje_prorrateo";
            conn.QueryAlimento(query, out dt);

            tBPorcDif.Text = dt.Rows[0][0].ToString();
            tbPorcT.Text = dt.Rows[0][1].ToString();
            porcDif = Convert.ToDouble(dt.Rows[0][0]);
            porcT = Convert.ToDouble(dt.Rows[0][1]);
        }

        private void SetTitulos()
        {
            string formatof = fecha_SIE.ToString("dd") + " de " + fecha_SIE.ToString("MMMM") + " del " + fecha_SIE.ToString("yyyy") + " 11:59 p. m." ;
            label3.Text = "Existencia de Alimento al Dia: " + formatof;
            label7.Text = "Existencia de Forraje al Dia: " + formatof;
            formatof = fecha_Tra.ToString("dd") + " de " + fecha_Tra.ToString("MMMM") + " del " + fecha_Tra.ToString("yyyy") + " " + fecha_Tra.ToString("HH:mm tt");
            label4.Text = "Consumo Tracker al Dia: " + formatof;
            label9.Text = "Consumo Tracker al Dia: " + formatof;
            formatof = DateTime.Now.ToString("dd") + " de " + DateTime.Now.ToString("MMMM") + " del " + DateTime.Now.ToString("yyyy");
            label5.Text += " " + formatof;
            formatof = fecha_Bal.ToString("dd") + " de " + fecha_Bal.ToString("MMMM") + " del " + fecha_Bal.ToString("yyyy") + " " + fecha_Bal.ToString("HH:mm tt");
            label8.Text = "Bascula al Dia: " + formatof;
            label8.Visible = conBasc != 1 ? false : true;
            label2.Text = "Mes de Prorrateo: " + fecha.ToString("MMMM").ToUpper() + " " + fecha.ToString("yyyy");
            rancho = ran_id > 9 ? ran_id.ToString() : "0" + ran_id.ToString();
            ranCadena = ran_id > 9 ? "'" + ran_id.ToString() + "'" : "'0" + ran_id.ToString() + "'";
            checkBox1.Text = rep == 1 ? Titulo(ranchosId) : ran_nombre.ToUpper();
            checkBox1.Checked = true;
            checkBox1.Enabled = false;
            hora_corte = hora_corte > 24 ? hora_corte - 24 : hora_corte;
            DateTime inicio = new DateTime(fecha.Year, fecha.Month, 1, hora_corte, 0, 0);
            if (hora_corte != 0)
                inicio = inicio.AddDays(dias_a);
            DateTime corte = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora_corte, 0, 0);
            corte = inicio.Day == 1 && hora_corte != 0 ? corte.AddDays(1) : corte;
            DataTable dtPremezclas;
            string ran = rep == 1 ? ranchosId : ran_id.ToString();
            string query = "select DISTINCT ing_descripcion FROM racion "
                 + " WHERE rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + corte.ToString("yyyy-MM-dd HH:mm") + "' "
                 + " AND ran_id IN(" + ran + ") AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02') "
                 + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F')";
            conn.QueryAlimento(query, out dtPremezclas);

            conn.DeleteAlimento("porcentaje_Premezcla", "");
            DataTable dtV;
            for (int i = 0; i < dtPremezclas.Rows.Count; i++)
            {
                query = "SELECT TOP(5) * FROM premezcla where pmez_racion like '" + dtPremezclas.Rows[i][0].ToString() + "'";
                conn.QueryAlimento(query, out dtV);

                if (dtV.Rows.Count == 0)
                    continue;

                CargarPremezcla(dtPremezclas.Rows[i][0].ToString(), inicio, corte);
            }


            if (bal_clave != "0")
            {
                checkBox4.Text = "Tracker / Bascula";
                checkBox4.Checked = true;
            }

            checkBox2.Checked = true;
            checkBox4.Checked = true;
            PorcentajeT(modCTA, modCTF);
        }

        private void PorcentajeT(bool alas, bool alfo)
        {
            if (porcTActivo == false)
            {
                double total = 0;
                double valor, tvalor = 0, tvalor1 = 0, pvalor = 0, pvalor1 = 0;
                total = consumoAlas + consumoAlfo;
                int row = 0;
                int colT = dataGridView2.Columns.Count == 12 ? 7 : 6;
                int colB = dataGridView2.Columns.Count == 12 ? 5 : 4;
                int colP = dataGridView2.Columns.Count == 12 ? 6 : 5;
                if (alas == false || alfo == false)
                {
                    //total = consumoAlas + consumoAlfo;
                    tvalor = 0;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1[3, i].Value.ToString() == "TOTAL")
                            continue;

                        valor = Convert.ToDouble(dataGridView1[6, i].Value);
                        dataGridView1[8, i].Value = total > 0 ? valor / total * 100 : 0;
                        tvalor += Convert.ToDouble(dataGridView1[8, i].Value);
                    }

                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        if (dataGridView2[2, i].Value.ToString() == "TOTAL")
                            continue;

                        valor = Convert.ToDouble(dataGridView2[colB, i].Value);
                        dataGridView2[colT, i].Value = total > 0 ? valor / total * 100 : 0;
                        tvalor1 += Convert.ToDouble(dataGridView2[colT, i].Value);
                        dataGridView2[colP, i].Value = consumoAlfo > 0 ? valor / consumoAlfo * 100 : 0;
                        pvalor1 += Convert.ToDouble(dataGridView2[colP, i].Value);
                    }

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1[3, i].Value.ToString().Contains("TOTAL"))
                        {
                            row = i;
                            break;
                        }
                    }

                    dataGridView1[8, row].Value = tvalor;

                    row = 0;
                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        if (dataGridView2[2, i].Value.ToString().Contains("TOTAL"))
                        {
                            row = i;
                            break;
                        }
                    }

                    dataGridView2[colT, row].Value = tvalor1;
                    dataGridView2[colP, row].Value = pvalor1;
                }
                else if (alas)
                {
                    for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                    {
                        valor = Convert.ToDouble(dataGridView1[6, i].Value);
                        dataGridView1[8, i].Value = total > 0 ? valor / total * 100 : 0;
                    }

                    for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
                    {
                        valor = Convert.ToDouble(dataGridView2[colB, i].Value);
                        dataGridView2[colT, i].Value = total > 0 ? valor / total * 100 : 0;
                    }

                    alas = false;
                }
                else if (alfo)
                {
                    for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                    {
                        valor = Convert.ToDouble(dataGridView1[6, i].Value);
                        dataGridView1[8, i].Value = total > 0 ? valor / total * 100 : 0;
                    }

                    for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
                    {
                        valor = Convert.ToDouble(dataGridView2[colB, i].Value);
                        dataGridView2[colT, i].Value = total > 0 ? valor / total * 100 : 0;
                    }
                    alfo = false;
                }
            }
            porcTActivo = false;
        }

        private void FormatoGrid(DataGridView dgv, bool val)
        {
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (i == 2)
                    dgv.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                else
                    dgv.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            }
            BloquearDGV(dgv);

            dgv.AllowUserToResizeColumns = true;

            if (val)
            {
                dgv.Columns[3].DefaultCellStyle.Format = "###,##0";
                dgv.Columns[4].DefaultCellStyle.Format = "###,##0";
                dgv.Columns[5].DefaultCellStyle.Format = "###,##0";
                dgv.Columns[6].DefaultCellStyle.Format = "###,##0";
                dgv.Columns[6].DefaultCellStyle.Format = "###,##0.0";
                dgv.Columns[7].DefaultCellStyle.Format = "###,##0.0";
                dgv.Columns[8].DefaultCellStyle.Format = "###,##0";
                dgv.Columns[9].DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Pixel);
                dgv.Columns[10].DefaultCellStyle.Format = "###,##0";
                dgv.Columns[11].DefaultCellStyle.Format = "###,##0.0";


                //BLOQUEAR COLUMNAS -- 
                dgv.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
                dgv.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
                dgv.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[2].Width = 50;
                dgv.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[8].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[9].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[10].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[11].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                //---
            }
            else
            {
                //BLOQUEAR COLUMNAS -- 
                dgv.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
                dgv.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
                dgv.Columns[2].Width = 50;
                dgv.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[8].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[9].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgv.Columns[10].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                // --

                dgv.Columns[3].DefaultCellStyle.Format = "###,##0";
                dgv.Columns[4].DefaultCellStyle.Format = "###,##0";
                dgv.Columns[5].DefaultCellStyle.Format = "###,##0.0";
                dgv.Columns[6].DefaultCellStyle.Format = "###,##0.0";
                dgv.Columns[7].DefaultCellStyle.Format = "###,##0";
                dgv.Columns[8].DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Pixel);
                dgv.Columns[9].DefaultCellStyle.Format = "###,##0";
                dgv.Columns[10].DefaultCellStyle.Format = "###,##0.0";
                if (dgv.Columns.Count == 13)
                {
                    dgv.Columns[11].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                    dgv.Columns[12].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                    dgv.Columns[11].DefaultCellStyle.Format = "###,##0";
                    dgv.Columns[12].DefaultCellStyle.Format = "###,##0";
                }
            }


            //dgv.Columns[3].DefaultCellStyle.Format = "###,##0";
            //dgv.Columns[4].DefaultCellStyle.Format = "###,##0";
            //dgv.Columns[5].DefaultCellStyle.Format = "###,##0";
            //dgv.Columns[6].DefaultCellStyle.Format = "###,##0";
            //dgv.Columns[8].DefaultCellStyle.Format = "###,##0";
            //if(val)
            //{
            //    dgv.Columns[7].DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Pixel);
            //    dgv.Columns[9].DefaultCellStyle.Format = "###,##0";
            //}

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(221, 235, 247);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 156, 241);
            // dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            //dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToOrderColumns = false;

        }

        private void FormatoGrid(DataGridView dgv, int column)
        {
            // dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            //dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToOrderColumns = false;

            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (i != column)
                    dgv.Columns[0].ReadOnly = true;
                else
                    dgv.Columns[0].ReadOnly = false;

                if (i != 3)
                    dgv.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                else
                    dgv.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }


            //Bloquear Columnas
            dgv.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            dgv.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            dgv.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            dgv.Columns[3].Width = 50;
            dgv.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dgv.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dgv.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dgv.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dgv.Columns[8].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dgv.Columns[9].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dgv.Columns[10].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dgv.Columns[11].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dgv.Columns[12].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            //--

            dgv.Columns[4].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[5].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[6].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[7].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[8].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[9].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[10].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[11].DefaultCellStyle.Format = "###,##0.0";
            dgv.Columns[12].DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Pixel);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(221, 235, 247);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 156, 241);
        }

        private bool ExisteProrrateo()
        {
            DataTable dt;
            string query = "Select * From prorrateo WHERE pro_fecha_reg = '" + fecha_reg.ToString("yyyy-MM-dd") + "' AND ran_id = " + ran_id;
            conn.QueryAlimento(query, out dt);

            return dt.Rows.Count > 0;
        }

        private bool ExisteMalTipificacion()
        {
            DataTable dt;
            string query = "Select * From prorrateo WHERE pro_fecha_reg = '" + fecha_reg.ToString("yyyy-MM-dd") + "' AND ran_id = " + ran_id;
            conn.QueryAlimento(query, out dt);

            return dt.Rows.Count > 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Cursor = Cursors.WaitCursor;
            int contIng = 0, contIng2 = 0, contCons = 0;
            string cadIng = "", cadIng2 = "", cadIng3 = "", cadCons = "";

            DateTime perInicio = new DateTime(fecha.Year, fecha.Month, 1);
            DateTime racion_fin = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora_corte, 0, 0);
            DateTime racion_inicio = racion_fin.AddMonths(-1);
            DateTime exi_fecha = fecha_SIE;
            DateTime bal_inicio = new DateTime(fecha_Bal.Year, fecha.Month, 1);
            DateTime bal_fin = fecha_Bal;


            DataTable dt1 = new DataTable();
            if (dataGridView2.Columns.Count == 12)
            {
                dt1.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("EXISTENCIASIE").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("INVFINAL").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("BASCULA").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("P").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("T").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("TRACKER").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("EXISTENCIA").DataType = System.Type.GetType("System.Int32");
                dt1.Columns.Add("DIFKG").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("DIFPORC").DataType = System.Type.GetType("System.Double");

                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    DataRow dr = dt1.NewRow();
                    dr["FECHA"] = dataGridView2[0, i].Value;
                    dr["CLAVE"] = dataGridView2[1, i].Value;
                    dr["ARTICULO"] = dataGridView2[2, i].Value;
                    dr["EXISTENCIASIE"] = dataGridView2[3, i].Value;
                    dr["INVFINAL"] = dataGridView2[4, i].Value;
                    dr["BASCULA"] = dataGridView2[5, i].Value;
                    dr["P"] = dataGridView2[6, i].Value;
                    dr["T"] = dataGridView2[7, i].Value;
                    dr["TRACKER"] = dataGridView2[8, i].Value;
                    dr["EXISTENCIA"] = dataGridView2[9, i].Value.ToString() == "X" ? 0 : 1;
                    dr["DIFKG"] = dataGridView2[10, i].Value;
                    dr["DIFPORC"] = dataGridView2[11, i].Value;
                    dt1.Rows.Add(dr);
                }
            }
            else
            {
                dt1.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("EXISTENCIASIE").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("BASCULA").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("P").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("T").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("TRACKER").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("EXISTENCIA").DataType = System.Type.GetType("System.Int32");
                dt1.Columns.Add("DIFKG").DataType = System.Type.GetType("System.Double");
                dt1.Columns.Add("DIFPORC").DataType = System.Type.GetType("System.Double");

                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    DataRow dr = dt1.NewRow();
                    dr["FECHA"] = dataGridView2[0, i].Value;
                    dr["CLAVE"] = dataGridView2[1, i].Value;
                    dr["ARTICULO"] = dataGridView2[2, i].Value;
                    dr["EXISTENCIASIE"] = dataGridView2[3, i].Value;
                    dr["BASCULA"] = dataGridView2[4, i].Value;
                    dr["P"] = dataGridView2[5, i].Value;
                    dr["T"] = dataGridView2[6, i].Value;
                    dr["TRACKER"] = dataGridView2[7, i].Value;
                    dr["EXISTENCIA"] = dataGridView2[8, i].Value.ToString() == "X" ? 0 : 1;
                    dr["DIFKG"] = dataGridView2[9, i].Value;
                    dr["DIFPORC"] = dataGridView2[10, i].Value;
                    dt1.Rows.Add(dr);
                }
            }

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("ALMACEN").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("DISPONIBLE_SIE").DataType = System.Type.GetType("System.Double");
            dt2.Columns.Add("INV_FINAL").DataType = System.Type.GetType("System.Double");
            dt2.Columns.Add("CONSUMO").DataType = System.Type.GetType("System.Double");
            dt2.Columns.Add("P").DataType = System.Type.GetType("System.Double");
            dt2.Columns.Add("T").DataType = System.Type.GetType("System.Double");
            dt2.Columns.Add("CONSUMO_TRACKER").DataType = System.Type.GetType("System.Double");
            dt2.Columns.Add("DIF_KG").DataType = System.Type.GetType("System.Double");
            dt2.Columns.Add("DIF").DataType = System.Type.GetType("System.Double");
            dt2.Columns.Add("EXISTENCIA").DataType = System.Type.GetType("System.String");

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                DataRow dr = dt2.NewRow();
                dr["FECHA"] = dataGridView1[0, i].Value;
                dr["ALMACEN"] = dataGridView1[1, i].Value;
                dr["CLAVE"] = dataGridView1[2, i].Value;
                dr["ARTICULO"] = dataGridView1[3, i].Value;
                dr["DISPONIBLE_SIE"] = dataGridView1[4, i].Value;
                dr["INV_FINAL"] = dataGridView1[5, i].Value;
                dr["CONSUMO"] = dataGridView1[6, i].Value;
                dr["P"] = dataGridView1[7, i].Value;
                dr["T"] = dataGridView1[8, i].Value;
                dr["CONSUMO_TRACKER"] = dataGridView1[9, i].Value;
                dr["DIF_KG"] = dataGridView1[10, i].Value;
                dr["DIF"] = dataGridView1[11, i].Value;
                dr["EXISTENCIA"] = "";
                dt2.Rows.Add(dr);
            }
            string periodo = "";
            //IF SI ES BASCULA/TRACKER 3
            if (dataGridView2.Columns.Count == 12)
            {
                ReportDataSource source1 = new ReportDataSource("DataSet1", dt1);//<- es el dt que tiene existencia 
                reportViewer3.LocalReport.DataSources.Clear();
                reportViewer3.LocalReport.DataSources.Add(source1);

                ReportParameter[] parametros = new ReportParameter[3];
                parametros[0] = new ReportParameter("ESTABLO", "ESTABLO: " + ran_nombre.ToUpper(), true);
                periodo = "PERIODO DEL: " + perInicio.ToString("dd/MM/yyyy") + " al " + fecha.ToString("dd/MM/yyyy");
                parametros[1] = new ReportParameter("PERIODO", periodo, true);
                parametros[2] = new ReportParameter("BASCULA", ran_bascula.ToString(), true);
                reportViewer3.LocalReport.SetParameters(parametros);
                reportViewer3.LocalReport.Refresh();
                reportViewer3.RefreshReport();

                GTHUtils.SavePDF(reportViewer3, ruta + "FORRAJE SIE Y BASCULA_" + ran_nombre + ".pdf");
            }
            else
            {
                ReportDataSource source1 = new ReportDataSource("DataSet1", dt1);
                reportViewer2.LocalReport.DataSources.Clear();
                reportViewer2.LocalReport.DataSources.Add(source1);

                ReportParameter[] parametros = new ReportParameter[4];
                parametros[0] = new ReportParameter("ESTABLO", "ESTABLO: " + ran_nombre.ToUpper(), true);
                periodo = "PERIODO DEL: " + perInicio.ToString("dd/MM/yyyy") + " al " + fecha.ToString("dd/MM/yyyy");
                parametros[1] = new ReportParameter("PERIODO", periodo, true);
                parametros[2] = new ReportParameter("BASCULA", ran_bascula.ToString(), true);
                parametros[3] = new ReportParameter("TIPO", conBasc.ToString(), true); //<- poner variable tipo
                reportViewer2.LocalReport.SetParameters(parametros);
                reportViewer2.LocalReport.Refresh();
                reportViewer2.RefreshReport();

                GTHUtils.SavePDF(reportViewer2, ruta + "FORRAJE SIE Y BASCULA_" + ran_nombre + ".pdf");
            }
            //------------------------------------------------------------------------
            ReportDataSource source = new ReportDataSource("DataSet2", dt2);
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(source);

            ReportParameter[] parametros1 = new ReportParameter[2];
            parametros1[0] = new ReportParameter("Establo", "ESTABLO: " + ran_nombre.ToUpper(), true);
            parametros1[1] = new ReportParameter("Periodo", periodo, true);
            reportViewer1.LocalReport.SetParameters(parametros1);
            reportViewer1.LocalReport.Refresh();
            reportViewer1.RefreshReport();

            GTHUtils.SavePDF(reportViewer1, ruta + "ALIMENTO SIE Y TRACKER_" + ran_nombre + ".pdf");

            DataTable dt;
            string query = "select * FROM prorrateo WHERE ran_id = " + ran_id + " AND pro_fecha_reg = '" + fecha_reg.ToString("yyyy-MM-dd") + "'";
            conn.QueryAlimento(query, out dt);

            if (dt.Rows.Count > 0)
            {
                bool fiabilidad = autorizar == false ? Fiabilidad() : !autorizar;
                Exportar_Prorrateo extraer = new Exportar_Prorrateo(ran_id, ran_nombre, emp_id, emp_nombre, true, fiabilidad);
                if (extraer.ShowDialog() == DialogResult.OK)
                {
                    button3.Enabled = false;
                    InputSimulator sim = new InputSimulator();
                    sim.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);
                }
            }


            Cursor = Cursors.Default;
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (((DateTime.Today.Day > 2 && DateTime.Today.Day < 6 && conBasc != 3 && !ExisteProrrateo()) || hb) && modT == false && conBasc != 3 && almCerrados && tipificacionCorrecta)
                {
                    int r = dataGridView1.CurrentRow.Index;
                    double v = Convert.ToDouble(dataGridView1[9, r].Value.ToString());
                    dataGridView1.Columns[5].ReadOnly = false;
                    if (v >= 1)
                    {
                        dataGridView1.CurrentCell = dataGridView1.CurrentRow.Cells["INV FINAL"];
                        dataGridView1.BeginEdit(true);
                        valorGrid = Convert.ToDouble(dataGridView1[5, dataGridView1.CurrentRow.Index].Value);
                    }
                    else
                        dataGridView1.Rows[r].Cells["INV FINAL"].ReadOnly = true;
                }

                if (!existeProrrateo)
                {
                    if (modT)
                    {
                        if (dataGridView1.CurrentCell.ColumnIndex == 6 || dataGridView1.CurrentCell.ColumnIndex == 9)
                        {
                            dataGridView1.Columns[6].ReadOnly = false;
                            dataGridView1.Columns[9].ReadOnly = false;
                            dataGridView1.CurrentCell = dataGridView1.CurrentRow.Cells[dataGridView1.CurrentCell.ColumnIndex];
                            dataGridView1.BeginEdit(true);
                        }
                    }
                }
            }
            catch
            {

            }
        }
        private void Recalcular(DataGridView dgv, int Renglon, int columna)
        {
            modificarV = true;
            if (columna == 5)
            {
                double inv = 0, sie = 0, tracker = 0, consumo = 0, dif = 0, porcdif = 0;
                char existencia;
                Double.TryParse(dgv[5, Renglon].Value.ToString(), out inv);

                sie = Convert.ToDouble(dgv[4, Renglon].Value);
                tracker = Convert.ToDouble(dgv[9, Renglon].Value);
                consumo = sie - inv;
                dif = consumo - tracker;
                porcdif = tracker > 0 ? dif / tracker * 100 : 0;
                existencia = sie > consumo ? '✔' : 'X';

                dgv[5, Renglon].Value = inv;
                dgv[6, Renglon].Value = consumo;
                dgv[10, Renglon].Value = dif;
                dgv[11, Renglon].Value = porcdif;
                dgv[12, Renglon].Value = existencia;
            }
            else if (columna == 6)
            {
                double inv = 0, sie = 0, tracker = 0, consumo = 0, dif = 0, porcdif = 0;
                char existencia;
                Double.TryParse(dgv[6, Renglon].Value.ToString(), out consumo);

                Double.TryParse(dgv[4, Renglon].Value.ToString(), out sie);
                tracker = Convert.ToDouble(dgv[9, Renglon].Value);
                inv = sie - consumo;
                dif = consumo - tracker;
                porcdif = tracker > 0 ? dif / tracker * 100 : 0;
                existencia = sie > consumo ? '✔' : 'X';

                dgv[5, Renglon].Value = inv;
                dgv[6, Renglon].Value = consumo;
                dgv[10, Renglon].Value = dif;
                dgv[11, Renglon].Value = porcdif;
                dgv[12, Renglon].Value = existencia;

            }
            else if (columna == 9)
            {
                double tracker = 0, dif = 0, pdif = 0, consumo = 0;
                Double.TryParse(dgv[columna, Renglon].Value.ToString(), out tracker);
                consumo = Convert.ToDouble(dgv[6, Renglon].Value);
                dif = consumo - tracker;
                pdif = tracker > 0 ? dif / tracker * 100 : 0;

                dgv[9, Renglon].Value = tracker;
                dgv[10, Renglon].Value = dif;
                dgv[11, Renglon].Value = pdif;
            }
            modificarV = false;
            //PorcentajeT(modCTA, modCTF);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int columna = e.ColumnIndex;
            int renglon = dataGridView1.CurrentCell.RowIndex;

            if (modificarV == false)
            {
                modCTA = true;
                porcTActivo = true;
                if (e.ColumnIndex == 5)
                {
                    if (dataGridView1[columna, renglon].Value.ToString() != "")
                        Recalcular(dataGridView1, dataGridView1.CurrentCell.RowIndex, 5);
                    else
                    {
                        dataGridView1.CurrentCell = dataGridView1.CurrentRow.Cells["INV FINAL"];
                        dataGridView1.BeginEdit(true);
                    }
                }
                else if (e.ColumnIndex == 6)
                {
                    if (dataGridView1[columna, renglon].Value.ToString() != "")
                        Recalcular(dataGridView1, dataGridView1.CurrentCell.RowIndex, 6);
                }
                else if (e.ColumnIndex == 7)
                {
                    if (dataGridView1[columna, renglon].Value.ToString() != "")
                        Recalcular(dataGridView1, dataGridView1.CurrentCell.RowIndex, 7);
                    else
                    {
                        dataGridView1.CurrentCell = dataGridView1.CurrentRow.Cells["CONSUMO TRACKER"];
                        dataGridView1.BeginEdit(true);
                    }
                }
                else if (e.ColumnIndex == 9)
                {
                    if (dataGridView1[columna, renglon].Value.ToString() != "")
                        Recalcular(dataGridView1, dataGridView1.CurrentCell.RowIndex, 9);
                    else
                    {
                        dataGridView1.CurrentCell = dataGridView1.CurrentRow.Cells["CONSUMO TRACKER"];
                        dataGridView1.BeginEdit(true);
                    }

                }
                CalcularTotal();
            }

        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int columna;
            int renglon;
            DataGridView dgv = sender as DataGridView;
            if (dgv.Columns[e.ColumnIndex].Index == 12)
            {
                if (e.Value.ToString().Contains("✔"))
                {
                    e.CellStyle.ForeColor = Color.Green;
                }
                else
                {
                    e.CellStyle.ForeColor = Color.Red;
                }
            }
            else if (dgv.Columns[e.ColumnIndex].Index == 6)
            {
                if (double.Parse(e.Value.ToString()) < 0)
                    e.CellStyle.BackColor = Color.Red;

            }
            else if (dgv.Columns[e.ColumnIndex].Index == 11)
            {

                columna = e.ColumnIndex - 1;
                renglon = e.RowIndex;
                //if(renglon < (dataGridView1.Rows.Count - 1))
                //{
                switch (Convert.ToDouble(e.Value.ToString()))
                {
                    case double n when ((n >= 5 && n <= 10) || (n >= -10 && n <= -5)):
                        e.CellStyle.BackColor = Color.FromArgb(255, 245, 217);
                        dgv[columna, renglon].Style.BackColor = Color.FromArgb(255, 245, 217);
                        break;
                    case double n when (n > 10 || n < -10):
                        e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);
                        dgv[columna, renglon].Style.BackColor = Color.FromArgb(255, 201, 201);
                        break;
                    case double n when ((n >= 2.5 && n < 5) || (n >= -5 && n < -2.5)):
                        e.CellStyle.BackColor = Color.FromArgb(242, 242, 242);
                        dgv[columna, renglon].Style.BackColor = Color.FromArgb(242, 242, 242);
                        break;
                    case double n when ((n > 0 && n < 2.5) || (n > -2.5 && n < 0) && n != 0)://double n when (n > -2.5 && n < 2.5 && n != 0):
                        e.CellStyle.BackColor = Color.FromArgb(222, 237, 211);
                        dgv[columna, renglon].Style.BackColor = Color.FromArgb(222, 237, 211);
                        break;
                    case double n when (n == 0):
                        e.CellStyle.BackColor = Color.FromArgb(242, 242, 242);
                        dgv[columna, renglon].Style.BackColor = Color.FromArgb(242, 242, 242);
                        break;
                    default: break;

                }
                //}
            }
            else if (dgv.Columns[e.ColumnIndex].Index == 3)
            {
                columna = e.ColumnIndex;
                renglon = e.RowIndex;
                double existencia = 0, consumo = 0, tracker = 0;

                if (!e.Value.ToString().Contains("TOTAL"))
                {
                    if ((dgv.Rows[renglon].Cells[4].Value == null) == false)
                        Double.TryParse(dgv[4, renglon].Value.ToString(), out existencia);

                    if ((dgv.Rows[renglon].Cells[9].Value == null) == false)
                        Double.TryParse(dgv[9, renglon].Value.ToString(), out tracker);

                    if ((dgv.Rows[renglon].Cells[6].Value == null) == false)
                        Double.TryParse(dgv[6, renglon].Value.ToString(), out consumo);

                    if (existencia == 0 && tracker > 0)
                    {
                        e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);
                        dgv[columna, renglon].Style.BackColor = Color.FromArgb(255, 201, 201);

                    }
                    else if (consumo > 0 && tracker == 0)
                    {
                        e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);
                        dgv[columna, renglon].Style.BackColor = Color.FromArgb(255, 201, 201);
                    }

                }


                //if (renglon < dgv.Rows.Count - 1)
                //{
                //    if((dgv.Rows[renglon].Cells[4].Value == null) == false)
                //        Double.TryParse(dgv[4,renglon].Value.ToString(),out existencia);

                //    if ((dgv.Rows[renglon].Cells[9].Value == null) == false)
                //        Double.TryParse(dgv[9, renglon].Value.ToString(), out  consumo);

                //    if(existencia == 0 && consumo > 0)
                //    {
                //        e.CellStyle.BackColor = Color.FromArgb(255,201,201);
                //        dgv[columna, renglon].Style.BackColor = Color.FromArgb(255,201,201);
                //    }
                //}
            }
            else if (dgv.Columns[e.ColumnIndex].Index == 8)
            {
                int row = e.RowIndex;
                double t = 0, dif = 0;
                if (dgv.Rows[row].Cells[3].Value.ToString() != "TOTAL")
                {
                    if ((dgv.Rows[row].Cells[8].Value == null) == false)
                        Double.TryParse(dgv[8, row].Value.ToString(), out t);

                    if (t >= porcT)
                    {
                        if ((dgv.Rows[row].Cells[11].Value == null) == false)
                            Double.TryParse(dgv.Rows[row].Cells[11].Value.ToString(), out dif);

                        if (dif >= porcDif || dif <= (porcDif * -1))
                            dgv[8, row].Style.BackColor = Color.FromArgb(255, 201, 201);
                        else
                            dgv[8, row].Style.BackColor = dgv[0, row].Style.BackColor;
                    }
                    else
                        dgv[8, row].Style.BackColor = dgv[0, row].Style.BackColor;
                }
            }
        }

        private void dataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int column, row;
            DataGridView dgv = sender as DataGridView;
            if (dgv.Columns.Count == 11)
            {
                if (dgv.Columns[e.ColumnIndex].Index == 2)
                {
                    if (!e.Value.ToString().Contains("TOTAL"))
                    {
                        column = e.ColumnIndex;
                        row = e.RowIndex;
                        double consumo = Convert.ToDouble(dgv[4, row].Value);
                        double tracker = Convert.ToDouble(dgv[7, row].Value);
                        double sie = Convert.ToDouble(dgv[3, row].Value);

                        if (consumo == 0 && tracker > 0)
                            dgv[2, row].Style.BackColor = Color.FromArgb(255, 201, 201);
                        else if (consumo > 0 && tracker == 0)
                            dgv[2, row].Style.BackColor = Color.FromArgb(255, 201, 201);
                        else if (sie == 0 && tracker > 0 || sie == 0 && consumo > 0)
                            dgv[2, row].Style.BackColor = Color.FromArgb(255, 201, 201);


                    }

                }
                else if (dgv.Columns[e.ColumnIndex].Index == 8)
                {

                    if (e.Value.ToString().Contains("✔"))
                        e.CellStyle.ForeColor = Color.Green;
                    else
                        e.CellStyle.ForeColor = Color.Red;

                }
                else if (dgv.Columns[e.ColumnIndex].Index == 10)
                {
                    column = e.ColumnIndex - 1;
                    row = e.RowIndex;
                    switch (Convert.ToDouble(e.Value.ToString()))
                    {

                        case double n when ((n >= 5 && n <= 10) || (n >= -10 && n <= -5)):
                            e.CellStyle.BackColor = Color.FromArgb(255, 245, 217);
                            dgv[column, row].Style.BackColor = Color.FromArgb(255, 245, 217);
                            break;
                        case double n when (n > 10 || n < -10):
                            e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);
                            dgv[column, row].Style.BackColor = Color.FromArgb(255, 201, 201);
                            break;
                        case double n when ((n >= 2.5 && n < 5) || (n >= -5 && n < -2.5)):
                            e.CellStyle.BackColor = Color.FromArgb(242, 242, 242);
                            dgv[column, row].Style.BackColor = Color.FromArgb(242, 242, 242);
                            break;
                        case double n when (n > -2.5 && n < 2.5 && n != 0)://((n > 0 && n < 2.5) || (n > -2.5 && n < 0)):
                            e.CellStyle.BackColor = Color.FromArgb(222, 237, 211);
                            dgv[column, row].Style.BackColor = Color.FromArgb(222, 237, 211);
                            break;
                        case double n when (n == 0):
                            e.CellStyle.BackColor = Color.FromArgb(242, 242, 242);
                            dgv[column, row].Style.BackColor = Color.FromArgb(242, 242, 242);
                            break;
                        default: break;

                    }
                }
                else if (dgv.Columns[e.ColumnIndex].Index == 6)
                {
                    column = e.ColumnIndex;
                    row = e.RowIndex;
                    double t = 0;
                    double dif = 0;

                    if (dgv.Rows[row].Cells[2].Value.ToString() != "TOTAL")
                    {
                        if ((dgv.Rows[row].Cells[6].Value == null) == false)
                            Double.TryParse(dgv[6, row].Value.ToString(), out t);

                        if (t >= porcT)
                        {
                            if ((dgv.Rows[row].Cells[10].Value == null) == false)
                                Double.TryParse(dgv[10, row].Value.ToString(), out dif);

                            if (dif >= porcDif || dif <= (porcDif * -1))
                            {
                                dgv[6, row].Style.BackColor = Color.FromArgb(255, 201, 201);
                            }
                            else
                                dgv[6, row].Style.BackColor = dgv[0, row].Style.BackColor;
                        }
                    }

                }
            }
            else if (dgv.Columns.Count == 12)
            {
                if (dgv.Columns[e.ColumnIndex].Index == 2)
                {
                    if (!e.Value.ToString().Contains("TOTAL"))
                    {
                        column = e.ColumnIndex;
                        row = e.RowIndex;
                        double consumo = Convert.ToDouble(dgv[5, row].Value);
                        double tracker = Convert.ToDouble(dgv[8, row].Value);
                        double sie = Convert.ToDouble(dgv[3, row].Value);

                        if (consumo == 0 && tracker > 0)
                            e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);
                        else if (consumo > 0 && tracker == 0)
                            e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);
                        else if (sie == 0 && tracker > 0)
                            e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);

                    }
                }
                else if (dgv.Columns[e.ColumnIndex].Index == 9)
                {
                    if (e.Value.ToString().Contains("✔"))
                        e.CellStyle.ForeColor = Color.Green;
                    else
                        e.CellStyle.ForeColor = Color.Red;
                }
                else if (dgv.Columns[e.ColumnIndex].Index == 11)
                {
                    column = e.ColumnIndex - 1;
                    row = e.RowIndex;
                    switch (Convert.ToDouble(e.Value.ToString()))
                    {

                        case double n when ((n >= 5 && n <= 10) || (n >= -10 && n <= -5)):
                            e.CellStyle.BackColor = Color.FromArgb(255, 245, 217);
                            dgv[column, row].Style.BackColor = Color.FromArgb(255, 245, 217);
                            break;
                        case double n when (n > 10 || n < -0.10):
                            e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);
                            dgv[column, row].Style.BackColor = Color.FromArgb(255, 201, 201);
                            break;
                        case double n when ((n >= 2.5 && n < 5) || (n >= -5 && n < -2.5)):
                            e.CellStyle.BackColor = Color.FromArgb(242, 242, 242);
                            dgv[column, row].Style.BackColor = Color.FromArgb(242, 242, 242);
                            break;
                        case double n when ((n > 0 && n < 2.5) || (n > -2.5 && n < 0)):
                            e.CellStyle.BackColor = Color.FromArgb(222, 237, 211);
                            dgv[column, row].Style.BackColor = Color.FromArgb(222, 237, 211);
                            break;
                        case double n when (n == 0):
                            e.CellStyle.BackColor = Color.FromArgb(242, 242, 242);
                            dgv[column, row].Style.BackColor = Color.FromArgb(242, 242, 242);
                            break;
                        default: break;

                    }
                }
                else if (dgv.Columns[e.ColumnIndex].Index == 7)
                {
                    row = e.RowIndex;
                    double t = 0, dif = 0;
                    if (dgv.Rows[row].Cells[3].Value.ToString() != "TOTAL")
                    {
                        if ((dgv.Rows[row].Cells[7].Value == null) == false)
                            Double.TryParse(dgv.Rows[row].Cells[7].Value.ToString(), out t);

                        if (t >= porcT)
                        {
                            if ((dgv.Rows[row].Cells[11].Value == null) == false)
                                Double.TryParse(dgv.Rows[row].Cells[11].Value.ToString(), out dif);

                            if (dif >= porcDif || dif <= (porcDif * -1))
                                dgv[7, row].Style.BackColor = Color.FromArgb(255, 201, 201);
                            else
                                dgv[7, row].Style.BackColor = dgv[0, row].Style.BackColor;
                        }
                    }
                }

            }
            else if (dgv.Columns.Count == 13)
            {
                if (dgv.Columns[e.ColumnIndex].Index == 2)
                {
                    if (!e.Value.ToString().Contains("TOTAL"))
                    {
                        column = e.ColumnIndex;
                        row = e.RowIndex;
                        double consumo = Convert.ToDouble(dgv[4, row].Value);
                        double tracker = Convert.ToDouble(dgv[7, row].Value);
                        double sie = Convert.ToDouble(dgv[3, row].Value);

                        if (consumo == 0 && tracker > 0)
                            e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);
                        else if (consumo > 0 && tracker == 0)
                            e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);
                        else if (sie == 0 && tracker > 0)
                            e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);
                    }

                }
                else if (dgv.Columns[e.ColumnIndex].Index == 8)
                {

                    if (e.Value.ToString().Contains("✔"))
                        e.CellStyle.ForeColor = Color.Green;
                    else
                        e.CellStyle.ForeColor = Color.Red;

                }
                else if (dgv.Columns[e.ColumnIndex].Index == 10)
                {
                    column = e.ColumnIndex - 1;
                    row = e.RowIndex;
                    switch (Convert.ToDouble(e.Value.ToString()))
                    {

                        case double n when ((n >= 5 && n <= 10) || (n >= -10 && n <= -5)):
                            e.CellStyle.BackColor = Color.FromArgb(255, 245, 217);
                            dgv[column, row].Style.BackColor = Color.FromArgb(255, 245, 217);
                            break;
                        case double n when (n > 10 || n < -0.10):
                            e.CellStyle.BackColor = Color.FromArgb(255, 201, 201);
                            dgv[column, row].Style.BackColor = Color.FromArgb(255, 201, 201);
                            break;
                        case double n when ((n >= 2.5 && n < 5) || (n >= -5 && n < -2.5)):
                            e.CellStyle.BackColor = Color.FromArgb(242, 242, 242);
                            dgv[column, row].Style.BackColor = Color.FromArgb(242, 242, 242);
                            break;
                        case double n when ((n > 0 && n < 2.5) || (n > -2.5 && n < 0)):
                            e.CellStyle.BackColor = Color.FromArgb(222, 237, 211);
                            dgv[column, row].Style.BackColor = Color.FromArgb(222, 237, 211);
                            break;
                        case double n when (n == 0):
                            e.CellStyle.BackColor = Color.FromArgb(242, 242, 242);
                            dgv[column, row].Style.BackColor = Color.FromArgb(242, 242, 242);
                            break;
                        default: break;

                    }
                }
                else if (dgv.Columns[e.ColumnIndex].Index == 6)
                {
                    column = e.ColumnIndex;
                    row = e.RowIndex;
                    double t = 0;
                    double dif = 0;
                    if (dgv.Rows[row].Cells[2].Value.ToString() != "TOTAL")
                    {
                        if ((dgv.Rows[row].Cells[6].Value == null) == false)
                            Double.TryParse(dgv[6, row].Value.ToString(), out t);

                        if (t >= porcT)
                        {
                            if ((dgv.Rows[row].Cells[10].Value == null) == false)
                                Double.TryParse(dgv[10, row].Value.ToString(), out dif);

                            if (dif >= porcDif || dif <= (porcDif * -1))
                            {
                                dgv[6, row].Style.BackColor = Color.FromArgb(255, 201, 201);
                            }
                            else
                                dgv[6, row].Style.BackColor = dgv[0, row].Style.BackColor;
                        }
                    }
                }
            }
        }

        private Color FormatoCeldaExcel(double n)
        {
            Color color = new Color();
            if ((n >= 5 && n <= 10) || (n >= -10 && n <= -5))
                color = Color.FromArgb(255, 201, 201);
            else if (n > 10 || n < -0.10)
                color = Color.FromArgb(255, 201, 201);
            else if ((n >= 2.5 && n < 5) || (n >= -5 && n < -2.5))
                color = Color.FromArgb(242, 242, 242);
            else if ((n > 0 && n < 2.5) || (n > -2.5 && n < 0))
                color = Color.FromArgb(222, 237, 211);
            else if (n == 0)
                color = Color.FromArgb(242, 242, 242);

            return color;
        }
        //GuardarTemporal
        private void button5_Click(object sender, EventArgs e)
        {
            GuardarTemporal();
        }

        private void Guardar()
        {
            Cursor = Cursors.WaitCursor;
            DateTime hoy = DateTime.Now;
            DateTime fec;
            string sing, sing2, scons, sing3;
            int ing, ing2, consum;
            string alm_id, art_clave, prod_nombre, valores = "", fec_cadena = "";
            double exi_sie, cons, cons_tracker, dif, difporc, invfinal, porcT, porcP, bascula, cons_externo;
            DateTime fec_reg = new DateTime(fecha.Year, fecha.Month, 1, fecha.Hour, 0, 0);
            fec_reg = fec_reg.AddMonths(1).AddDays(-1);
            bool valAlimentos = PermitirGuardar(dataGridView1, "alimentos", false);
            bool valForraje = PermitirGuardar(dataGridView2, "forraje", false);

            if (valAlimentos == false || valForraje == false)
                MessageBox.Show("Validar Prorrateo", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                conn.DeleteAlimento("prorrateo", "where ran_id = " + ran_id.ToString() + " AND pro_fecha = '" + fec_reg.ToString("yyyy-MM-dd") + "'");

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (!dataGridView1[3, i].Value.ToString().Contains("TOTAL"))
                    {
                        if (dataGridView1[0, i].Value.ToString().Length > 0)
                        {
                            fec = Convert.ToDateTime(dataGridView1[0, i].Value);
                            fec_cadena = fec.ToString("yyyy-MM-dd");
                        }
                        else
                            fec_cadena = "";

                        alm_id = dataGridView1[1, i].Value.ToString();
                        art_clave = dataGridView1[2, i].Value.ToString();
                        prod_nombre = dataGridView1[3, i].Value.ToString();
                        exi_sie = Convert.ToDouble(dataGridView1[4, i].Value);
                        invfinal = Convert.ToDouble(dataGridView1[5, i].Value);
                        cons = Convert.ToDouble(dataGridView1[6, i].Value);
                        porcP = Convert.ToDouble(dataGridView1[7, i].Value);
                        porcT = Convert.ToDouble(dataGridView1[8, i].Value);
                        cons_tracker = Convert.ToDouble(dataGridView1[9, i].Value);
                        dif = Convert.ToDouble(dataGridView1[10, i].Value);
                        difporc = Convert.ToDouble(dataGridView1[11, i].Value);

                        if (fec_cadena != "")
                        {
                            valores += "(" + ran_id.ToString() + ",'" + fec_cadena + "', '" + alm_id + "', '" + art_clave + "', '" + prod_nombre + "', " + exi_sie.ToString() + "," + invfinal + "," + cons + "," + cons_tracker + ","
                               + dif.ToString() + ", " + difporc + ",'" + fec_reg.ToString("yyyy-MM-dd") + "'," + porcP + "," + porcT + ", NULL, NULL),";
                        }
                        else
                        {
                            valores += "(" + ran_id.ToString() + ", NULL , '" + alm_id + "', '" + art_clave + "', '" + prod_nombre + "', " + exi_sie.ToString() + "," + invfinal + "," + cons + "," + cons_tracker + ","
                            + dif.ToString() + ", " + difporc + ",'" + fec_reg.ToString("yyyy-MM-dd") + "'," + porcP + "," + porcT + ",NULL,NULL),";
                        }
                    }

                }

                int c = dataGridView2.Columns.Count == 12 ? 1 : 0;
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    if (!dataGridView2[2, i].Value.ToString().Contains("TOTAL"))
                    {

                        if (dataGridView2[0, i].Value.ToString().Length > 0)
                        {
                            fec = Convert.ToDateTime(dataGridView2[0, i].Value);
                            fec_cadena = fec.ToString("yyyy-MM-dd");
                        }
                        else
                            fec_cadena = "";

                        art_clave = dataGridView2[1, i].Value.ToString();
                        prod_nombre = dataGridView2[2, i].Value.ToString();
                        exi_sie = Convert.ToDouble(dataGridView2[3, i].Value);
                        cons = Convert.ToDouble(dataGridView2[4 + c, i].Value);
                        porcP = Convert.ToDouble(dataGridView2[5 + c, i].Value);
                        porcT = Convert.ToDouble(dataGridView2[6 + c, i].Value);
                        cons_tracker = Convert.ToDouble(dataGridView2[7 + c, i].Value);
                        dif = Convert.ToDouble(dataGridView2[9 + c, i].Value);
                        difporc = Convert.ToDouble(dataGridView2[10 + c, i].Value);
                        invfinal = c;

                        if (conBasc == 4)
                        {
                            bascula = Convert.ToDouble(dataGridView2[11, i].Value);
                            cons_externo = Convert.ToDouble(dataGridView2[12, i].Value);
                        }
                        else
                        {
                            bascula = 0;
                            cons_externo = 0;
                        }

                        if (fec_cadena != "")
                            valores += "(" + ran_id.ToString() + ",'" + fec_cadena + "', NULL, '" + art_clave + "', '" + prod_nombre + "', " + exi_sie + "," + invfinal + "," + cons + "," + cons_tracker + "," + dif + ", " + difporc + ",'" + fec_reg.ToString("yyyy-MM-dd") + "'," + porcP + "," + porcT + "," + bascula + "," + cons_externo + "),";
                        else
                            valores += "(" + ran_id.ToString() + ",NULL,NULL,'" + art_clave + "','" + prod_nombre + "'," + exi_sie + "," + invfinal + "," + cons + "," + cons_tracker + "," + dif + "," + difporc + ",'" + fec_reg.ToString("yyy-MM-dd") + "'," + porcP + "," + porcT + "," + bascula + "," + cons_externo + " ),";
                    }
                }

                valores = valores.Substring(0, valores.Length - 1);
                conn.InsertMasivAlimento("prorrateo", valores);

                conn.DeleteAlimento("prorrateoTemp", "where ran_id = " + ran_id.ToString());
                button1.Enabled = false;
                button1.Visible = false;
                button2.Enabled = false;
                button5.Enabled = false;
                button5.Visible = false;
                button6.Enabled = false;
                button6.Visible = false;
                button9.Enabled = false;
                button10.Enabled = false;
                button9.Visible = false;
                button10.Visible = false;

                if (hoy.Day > 2 && hoy.Day < 6)
                    button3.Enabled = true;

                BloquearDGV(dataGridView1);
                panelBtnProrrateo.Visible = true;
                button3.Enabled = true;
                hb = false;
                panel1Autorizar.Visible = false;
                label12.Text = "PRORRATEO GUARDADO";
                label12.ForeColor = Color.Green;
            }


            Cursor = Cursors.Default;
        }

        //Guardar definitivo
        private void button6_Click(object sender, EventArgs e)
        {
            btnGuardar = true;
            Guardar();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            int contIng = 0, contIng2 = 0, contCons = 0;
            string cadIng = "", cadIng2 = "", cadIng3 = "", cadCons = "";

            DateTime perInicio = new DateTime(fecha.Year, fecha.Month, 1);
            DateTime racion_fin = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora_corte, 0, 0);
            DateTime racion_inicio = racion_fin.AddMonths(-1);
            DateTime exi_fecha = fecha_SIE;
            DateTime bal_inicio = new DateTime(fecha_Bal.Year, fecha.Month, 1);
            DateTime bal_fin = fecha_Bal;

            double sum_p = 0, sum_t = 0;
            DataTable dt1 = new DataTable();
            if (dataGridView2.Columns.Count == 12)
            {
                dt1.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("EXISTENCIASIE").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("INVFINAL").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("BASCULA").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("P").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("T").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("TRACKER").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("EXISTENCIA").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("DIFKG").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("DIFPORC").DataType = System.Type.GetType("System.String");
                try
                {
                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        DataRow dr = dt1.NewRow();
                        dr["FECHA"] = dataGridView2[0, i].Value;
                        dr["CLAVE"] = dataGridView2[1, i].Value;
                        dr["ARTICULO"] = dataGridView2[2, i].Value;
                        try { dr["EXISTENCIASIE"] = Convert.ToDouble(dataGridView2[3, i].Value).ToString("#,0.0"); }
                        catch { }
                        try { dr["INVFINAL"] = Convert.ToDouble(dataGridView2[4, i].Value).ToString("#,0.0"); }
                        catch { }
                        try { dr["BASCULA"] = Convert.ToDouble(dataGridView2[5, i].Value).ToString("#,0"); }
                        catch { }
                        try { sum_p += Convert.ToDouble(dataGridView2[6, i-1].Value); }
                        catch { }
                        try { dr["P"] = dr["ARTICULO"].ToString() == "TOTAL" ? sum_p.ToString("#,0.0") : Convert.ToDouble(dataGridView2[6, i].Value).ToString("#,0.0"); }
                        catch { }
                        try { sum_t += Convert.ToDouble(dataGridView2[7, i-1].Value); }
                        catch { }
                        try { dr["T"] = dr["ARTICULO"].ToString() == "TOTAL" ? sum_t.ToString("#,0.0") : Convert.ToDouble(dataGridView2[7, i].Value).ToString("#,0.0"); }
                        catch { }
                        try { dr["TRACKER"] = Convert.ToDouble(dataGridView2[8, i].Value).ToString("#,0"); }
                        catch { }
                        dr["EXISTENCIA"] = dataGridView2[2, i].Value.ToString() != "TOTAL" ? dataGridView2[9, i].Value.ToString() == "✔" ? "si" : "no" : "";
                        try { dr["DIFKG"] = Convert.ToDouble(dataGridView2[10, i].Value).ToString("#,0.0"); }
                        catch { }
                        try { dr["DIFPORC"] = Convert.ToDouble(dataGridView2[11, i].Value).ToString("#,0.0"); }
                        catch { }
                        dt1.Rows.Add(dr);
                    }
                }
                catch { }
            }
            else if (dataGridView2.Columns.Count == 13)
            {
                dt1.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("EXISTENCIASIE").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("CONREAL").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("P").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("T").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("TRACKER").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("EXISTENCIA").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("DIFKG").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("DIFPORC").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("BASCULA").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("CONEXTERNO").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("T_COLOR").DataType = System.Type.GetType("System.Boolean");

                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    DataRow dr = dt1.NewRow();
                    dr["FECHA"] = dataGridView2[0, i].Value;
                    dr["CLAVE"] = dataGridView2[1, i].Value;
                    dr["ARTICULO"] = dataGridView2[2, i].Value;
                    try { dr["EXISTENCIASIE"] = Convert.ToDouble(dataGridView2[3, i].Value).ToString("#,0.0"); }
                    catch { }
                    try { dr["CONREAL"] = Convert.ToDouble(dataGridView2[4, i].Value).ToString("#,0"); }
                    catch { }
                    try { sum_p += Convert.ToDouble(dataGridView2[5, i].Value); }
                    catch { }
                    try { dr["P"] = dr["ARTICULO"].ToString() == "TOTAL" ? sum_p.ToString("#,0.0") : Convert.ToDouble(dataGridView2[5, i].Value).ToString("#,0.0"); }
                    catch { }
                    try { sum_t += Convert.ToDouble(dataGridView2[6, i].Value); }
                    catch { }
                    try { dr["T"] = dr["ARTICULO"].ToString() == "TOTAL" ? sum_t.ToString("#,0.0") : Convert.ToDouble(dataGridView2[6, i].Value).ToString("#,0.0"); }
                    catch { }
                    try { dr["TRACKER"] = Convert.ToDouble(dataGridView2[7, i].Value).ToString("#,0"); }
                    catch { }
                    dr["EXISTENCIA"] = dataGridView2[2, i].Value != "TOTAL" ? dataGridView2[8, i].Value.ToString() == "X" ? "no" : "si" : "";
                    try { dr["DIFKG"] = Convert.ToDouble(dataGridView2[9, i].Value).ToString("#,0.0"); }
                    catch { }
                    try { dr["DIFPORC"] = Convert.ToDouble(dataGridView2[10, i].Value).ToString("#,0.0"); }
                    catch { }
                    try { dr["BASCULA"] = Convert.ToDouble(dataGridView2[11, i].Value).ToString("#,0"); }
                    catch { }
                    try { dr["CONEXTERNO"] = Convert.ToDouble(dataGridView2[12, i].Value).ToString("#,0"); }
                    catch { }


                    double t = 0, dif = 0;
                    if (dataGridView2[2, i].Value != "TOTAL")
                    {
                        if ((dataGridView2[6, i].Value == null) == false)
                            Double.TryParse(dataGridView2[6, i].Value.ToString(), out t);

                        if (t >= porcT)
                        {
                            if ((dataGridView2[10, i].Value == null) == false)
                                Double.TryParse(dataGridView2[10, i].Value.ToString(), out dif);

                            if (dif >= porcDif || dif <= (porcDif * -1))
                                dr["T_COLOR"] = true;
                            else
                                dr["T_COLOR"] = false;
                        }
                        else
                            dr["T_COLOR"] = false;
                    }

                    dt1.Rows.Add(dr);
                }
            }
            else
            {
                dt1.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("EXISTENCIASIE").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("BASCULA").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("P").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("T").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("TRACKER").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("EXISTENCIA").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("DIFKG").DataType = System.Type.GetType("System.String");
                dt1.Columns.Add("DIFPORC").DataType = System.Type.GetType("System.String");

                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    DataRow dr = dt1.NewRow();
                    dr["FECHA"] = dataGridView2[0, i].Value;
                    dr["CLAVE"] = dataGridView2[1, i].Value;
                    dr["ARTICULO"] = dataGridView2[2, i].Value;
                    try { dr["EXISTENCIASIE"] = Convert.ToDouble(dataGridView2[3, i].Value).ToString("#,0.0"); }
                    catch { }
                    try { dr["BASCULA"] = Convert.ToDouble(dataGridView2[4, i].Value).ToString("#,0.0"); }
                    catch { }
                    try { sum_p += Convert.ToDouble(dataGridView2[5, i].Value); }
                    catch { }
                    try { dr["P"] = dr["ARTICULO"].ToString() == "TOTAL" ? sum_p.ToString("#,0.0") : Convert.ToDouble(dataGridView2[5, i].Value).ToString("#,0.0"); }
                    catch { }
                    try { sum_t += Convert.ToDouble(dataGridView2[6, i].Value); }
                    catch { }
                    try { dr["T"] = dr["ARTICULO"].ToString() == "TOTAL" ? sum_t.ToString("#,0.0") : Convert.ToDouble(dataGridView2[6, i].Value).ToString("#,0.0"); }
                    catch { }
                    try { dr["TRACKER"] = Convert.ToDouble(dataGridView2[7, i].Value).ToString("#,0"); }
                    catch { }
                    dr["EXISTENCIA"] = dataGridView2[2, i].Value != "TOTAL" ? dataGridView2[8, i].Value.ToString() == "X" ? "no" : "si" : "";
                    try { dr["DIFKG"] = Convert.ToDouble(dataGridView2[9, i].Value).ToString("#,0.0"); }
                    catch { }
                    try { dr["DIFPORC"] = Convert.ToDouble(dataGridView2[10, i].Value).ToString("#,0.0"); }
                    catch { }
                    dt1.Rows.Add(dr);
                }
            }
            sum_p = 0;
            sum_t = 0;
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("ALMACEN").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("DISPONIBLE_SIE").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("INV_FINAL").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("CONSUMO").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("P").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("T").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("CONSUMO_TRACKER").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("DIF_KG").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("DIF").DataType = System.Type.GetType("System.String");
            dt2.Columns.Add("EXISTENCIA").DataType = System.Type.GetType("System.String");

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                //if (i != dataGridView1.Rows.Count - 1)
                //{
                //    if (Convert.ToDouble(dataGridView1[4, i].Value) > 0)
                //    {
                //        cadIng += dataGridView1[3, i].Value.ToString() + "\n";
                //        contIng += Convert.ToDouble(dataGridView1[5, i].Value) > 0 ? 0 : 1;

                //    }
                //    else
                //    {
                //        cadIng2 += dataGridView1[3, i].Value.ToString() + "\n";
                //        contIng2 += Convert.ToDouble(dataGridView1[7, i].Value) > 0 ? 1 : 0;
                //    }

                //    if (Convert.ToDouble(dataGridView1[6, i].Value) > 0 && Convert.ToDouble(dataGridView1[7, i].Value) == 0)
                //    {
                //        cadCons += dataGridView1[3, i].Value.ToString()+ "\n";
                //        contCons += 1;
                //    }

                //    if( Convert.ToDouble(dataGridView1[7,i].Value) > 0  && Convert.ToDouble(dataGridView1[4,i].Value) == 0)
                //        cadIng3 += dataGridView1[3, i].Value.ToString() + "\n";
                //}

                DataRow dr = dt2.NewRow();
                dr["FECHA"] = dataGridView1[0, i].Value;
                dr["ALMACEN"] = dataGridView1[1, i].Value;
                dr["CLAVE"] = dataGridView1[2, i].Value;
                dr["ARTICULO"] = dataGridView1[3, i].Value;

                try { dr["DISPONIBLE_SIE"] = Convert.ToDouble(dataGridView1[4, i].Value).ToString("#,0.0"); }
                catch { }

                try { dr["INV_FINAL"] = Convert.ToDouble(dataGridView1[5, i].Value).ToString("#,0.0"); }
                catch { }

                try { dr["CONSUMO"] = Convert.ToDouble(dataGridView1[6, i].Value).ToString("#,0.0"); }
                catch { }
                try { sum_p += Convert.ToDouble(dataGridView1[7, i-1].Value); }
                catch { }
                try { dr["P"] = dr["ARTICULO"].ToString() == "TOTAL" ? sum_p.ToString("#,0.0") : Convert.ToDouble(dataGridView1[7, i].Value).ToString("#,0.0"); }
                catch { }
                try { sum_t += Convert.ToDouble(dataGridView1[8, i-1].Value); }
                catch { }
                try { dr["T"] = dr["ARTICULO"].ToString() == "TOTAL" ? sum_t.ToString("#,0.0") : Convert.ToDouble(dataGridView1[8, i].Value).ToString("#,0.0"); }
                catch { }

                try { dr["CONSUMO_TRACKER"] = Convert.ToDouble(dataGridView1[9, i].Value).ToString("#,0.0"); }
                catch { }

                try { dr["DIF_KG"] = Convert.ToDouble(dataGridView1[10, i].Value).ToString("#,0.0"); }
                catch { }

                try { dr["DIF"] = Convert.ToDouble(dataGridView1[11, i].Value).ToString("#,0.0"); }
                catch { }

                dr["EXISTENCIA"] = dataGridView1[3, i].Value != "TOTAL" ? Convert.ToDouble(dataGridView1[4, i].Value) > Convert.ToDouble(dataGridView1[6, i].Value) ? "si" : "no" : "";
                dt2.Rows.Add(dr);
            }
            string periodo = "";
            //IF SI ES BASCULA/TRACKER 3
            if (dataGridView2.Columns.Count == 12)
            {
                ReportDataSource source1 = new ReportDataSource("DataSet1", dt1);//<- es el dt que tiene existencia 
                reportViewer3.LocalReport.DataSources.Clear();
                reportViewer3.LocalReport.DataSources.Add(source1);

                ReportParameter[] parametros = new ReportParameter[3];
                parametros[0] = new ReportParameter("ESTABLO", "ESTABLO: " + ran_nombre.ToUpper(), true);
                periodo = "PERIODO DEL: " + perInicio.ToString("dd/MM/yyyy") + " al " + fecha.ToString("dd/MM/yyyy");
                parametros[1] = new ReportParameter("PERIODO", periodo, true);
                parametros[2] = new ReportParameter("BASCULA", ran_bascula.ToString(), true);
                reportViewer3.LocalReport.SetParameters(parametros);
                reportViewer3.LocalReport.Refresh();
                reportViewer3.RefreshReport();

                GTHUtils.SavePDF(reportViewer3, ruta + "FORRAJE SIE Y BASCULA_" + ran_nombre + ".pdf");
            }
            else if (dataGridView2.Columns.Count == 13)
            {
                ReportDataSource source1 = new ReportDataSource("DataSet1", dt1);
                reportViewer4.LocalReport.DataSources.Clear();
                reportViewer4.LocalReport.DataSources.Add(source1);

                ReportParameter[] parametros = new ReportParameter[3];
                parametros[0] = new ReportParameter("ESTABLO", "ESTABLO: " + ran_nombre.ToUpper(), true);
                periodo = "PERIODO DEL: " + perInicio.ToString("dd/MM/yyyy") + " al " + fecha.ToString("dd/MM/yyyy");
                parametros[1] = new ReportParameter("PERIODO", periodo, true);
                parametros[2] = new ReportParameter("BASCULA", ran_bascula.ToString(), true);
                reportViewer4.LocalReport.SetParameters(parametros);
                reportViewer4.LocalReport.Refresh();
                reportViewer4.RefreshReport();

                GTHUtils.SavePDF(reportViewer4, ruta + "FORRAJE SIE Y BASCULA_" + ran_nombre + ".pdf");
            }
            else
            {
                ReportDataSource source1 = new ReportDataSource("DataSet1", dt1);
                reportViewer2.LocalReport.DataSources.Clear();
                reportViewer2.LocalReport.DataSources.Add(source1);

                ReportParameter[] parametros = new ReportParameter[4];
                parametros[0] = new ReportParameter("ESTABLO", "ESTABLO: " + ran_nombre.ToUpper(), true);
                periodo = "PERIODO DEL: " + perInicio.ToString("dd/MM/yyyy") + " al " + fecha.ToString("dd/MM/yyyy");
                parametros[1] = new ReportParameter("PERIODO", periodo, true);
                parametros[2] = new ReportParameter("BASCULA", ran_bascula.ToString(), true);
                parametros[3] = new ReportParameter("TIPO", conBasc.ToString(), true); //<- poner variable tipo
                reportViewer2.LocalReport.SetParameters(parametros);
                reportViewer2.LocalReport.Refresh();
                reportViewer2.RefreshReport();

                GTHUtils.SavePDF(reportViewer2, ruta + "FORRAJE SIE Y BASCULA_" + ran_nombre + ".pdf");
            }
            //------------------------------------------------------------------------
            ReportDataSource source = new ReportDataSource("DataSet2", dt2);
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(source);

            ReportParameter[] parametros1 = new ReportParameter[2];
            parametros1[0] = new ReportParameter("Establo", "ESTABLO: " + ran_nombre.ToUpper(), true);
            parametros1[1] = new ReportParameter("Periodo", periodo, true);
            reportViewer1.LocalReport.SetParameters(parametros1);
            reportViewer1.LocalReport.Refresh();
            reportViewer1.RefreshReport();

            GTHUtils.SavePDF(reportViewer1, ruta + "ALIMENTO SIE Y TRACKER_" + ran_nombre + ".pdf");
            string rutaF = ruta + "FORRAJE SIE Y BASCULA_" + ran_nombre + ".pdf";
            string rutaA = ruta + "ALIMENTO SIE Y TRACKER_" + ran_nombre + ".pdf";
            Process.Start(rutaF);
            Process.Start(rutaA);

            Cursor = Cursors.Default;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DateTime fec = new DateTime(fecha.Year, fecha.Month, 1, fecha.Hour, 0, 0);
            fec = fec.AddMonths(1).AddDays(-1);
            conn.DeleteAlimento("prorrateoTemp", "");
            conn.DeleteAlimento("prorrateo", "WHERE pro_fecha_reg = '" + fec.ToString("yyyy-MM-dd") + "'");
            conn.DeleteAlimento("prorrateo_sie", "WHERE ps_fecha= '" + fec.ToString("yyyy-MM-dd") + "'");
            if (rep == 1)
                FillDGVAlimento(ranchosId);
            else
                FillDGVAlimento();
            //FillDGVForraje();
            Forraje();
            PorcentajeT(modCTA, modCTF);
            dataGridView1.ReadOnly = false;
            button1.Visible = true;
            button1.Enabled = true;
            if (DateTime.Today.Day > 2 && DateTime.Today.Day < 6 || hb)
            {
                bool alas = PermitirGuardar(dataGridView1, "ALIMENTOS", true);
                bool alfo = PermitirGuardar(dataGridView2, "FORRAJE", true);
                bool permitir = alas && alfo ? true : false;
                button5.Enabled = permitir;
                button6.Enabled = permitir;
                button5.Visible = permitir;
                button6.Visible = permitir;
                button9.Enabled = permitir;
                button10.Enabled = permitir;
                button9.Visible = permitir;
                button10.Visible = permitir;
            }

            autorizar = false;
            btnGuardar = false;
            panel1Autorizar.Visible = false;
            panelBtnProrrateo.Visible = false;
            button3.Enabled = false;
            label12.Text = ExisteProrrateo() ? "PRORRATEO GUARDADO" : "PRORRATEO SIN GUARDAR";
            label12.ForeColor = ExisteProrrateo() ? Color.Green : Color.Red;
            MessageBox.Show("Datos Restablecidos");
        }

        private void ValidarProrrateo()
        {
            DateTime hoy = DateTime.Now;
            DataTable dt;
            string query = "SELECT * FROM prorrateo where pro_fecha_reg = '" + fecha.ToString("yyyy-MM-dd") + "' AND ran_id = " + rancho;
            conn.QueryAlimento(query, out dt);

            if (dt.Rows.Count > 0)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                if (hoy.Day == 5)
                {
                    panelBtnProrrateo.Visible = true;
                    //button3.Enabled = true;
                }

            }
            else
            {
                panelBtnProrrateo.Visible = false;
                //button3.Enabled = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            panelHB.Visible = checkBox3.Checked;
            if (checkBox3.Checked)
                txtPasword.Focus();
            else
            {
                txtPasword.Text = "";
                hb = false;
                //if (DateTime.Today.Day >= 1 && DateTime.Today.Day < 6)
                //{
                button5.Visible = false;
                button6.Visible = false;
                button9.Visible = false;
                button10.Visible = false;

                panel1Autorizar.Visible = false;
                panelBtnProrrateo.Visible = false;
                panelBtnRestablecer.Visible = false;
                //}
            }
        }

        private void txtPasword_KeyDown(object sender, KeyEventArgs e)
        {
            DateTime fecha;
            if (DateTime.Today.Day >= 1 && DateTime.Today.Day < 6)
            {
                fecha = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                fecha = fecha.AddDays(-1);
            }
            else
            {
                fecha = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                fecha = fecha.AddMonths(1).AddDays(-1);
            }


            if (e.KeyCode == Keys.Enter)
            {
                if (txtPasword.Text.ToUpper() == "HCC123")
                {
                    //btnGuardar = true;
                    bool alas = PermitirGuardar(dataGridView1, "ALIMENTOS", true);
                    bool alfo = PermitirGuardar(dataGridView2, "FORRAJE", true);
                    bool permitir = alas && alfo ? true : false;
                    button2.Visible = true;

                    panelHB.Visible = false;
                    //checkBox3.Checked = false;
                    panelBtnRestablecer.Visible = true;

                    button2.Enabled = true;

                    hb = true;

                    DataTable dt;
                    string query = "SELECT * FROM prorrateo WHERE pro_fecha_reg = '" + fecha.ToString("yyyy-MM-dd") + "'";
                    conn.QueryAlimento(query, out dt);
                    DataTable dt1;
                    if (dt.Rows.Count > 0)
                    {
                        button5.Visible = false;
                        button6.Visible = false;
                        button9.Visible = false;
                        button10.Visible = false;
                        query = "SELECT * FROM prorrateo_sie WHERE ps_fecha = '" + fecha.ToString("yyyy-MM-dd") + "' AND alm_id IN(" + ali_alm_id + "," + f_alm_id + ")";
                        conn.QueryAlimento(query, out dt1);

                        if (dt1.Rows.Count == 0)
                        {
                            panelBtnProrrateo.Visible = true;
                            button3.Enabled = true;
                        }
                    }
                    else
                    {
                        if (almCerrados)
                        {
                            button1.Visible = true;
                            button1.Enabled = true;
                            button5.Visible = permitir;
                            button6.Visible = permitir;
                            button5.Enabled = permitir;
                            button6.Enabled = permitir;
                            button9.Visible = button5.Visible;
                            button10.Visible = button6.Visible;
                            button9.Enabled = permitir;
                            button10.Enabled = permitir;
                        }
                    }
                }
                else
                {
                    if (txtPasword.Text.Length > 0)
                    {
                        Help.ShowPopup(txtPasword, "Contraseña incorrecta", new Point(this.Location.X, this.Location.Y));
                    }
                    else
                    {
                        Help.ShowPopup(txtPasword, "Ingrese contraseña", new Point(this.Location.X, this.Location.Y));
                    }
                }
                txtPasword.Text = "";
            }
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            //e.Control.KeyPress -= new KeyPressEventHandler(Column5_keyPress);
            if (dataGridView1.CurrentCell.ColumnIndex == 5)
            {
                e.Control.KeyPress -= new KeyPressEventHandler(Column5_keyPress);
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column5_keyPress);
                }
            }

            if (dataGridView1.CurrentCell.ColumnIndex == 7)
            {
                e.Control.KeyPress -= new KeyPressEventHandler(Column7_keyPress);
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column7_keyPress);
                }
            }

        }

        private void Column5_keyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && e.KeyChar > 0)
            {
                e.Handled = true;
            }

            // solo 1 punto decimal
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void Column7_keyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && e.KeyChar > 0)
            {
                e.Handled = true;
            }

            // solo 1 punto decimal
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            //FillDGVForraje();
            Forraje();

            if (checkBox4.Checked == true)
            {
                if (bascula)
                    checkBox4.Text = "Tracker / Bascula";
                else
                    checkBox4.Text = "Tracker";
            }
            else
                checkBox4.Text = "TODO";
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (rep == 1)
                FillDGVAlimento(ranchosId);
            else
                FillDGVAlimento();

            if (checkBox2.Checked == true)
                checkBox2.Text = "Tracker / Inventario";
            else
                checkBox2.Text = "TODO";
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            panelMC.Visible = checkBox5.Checked;
            if (checkBox5.Checked == false)
            {
                modT = false;
                txtPwdM.Text = "";
                labelPDif.Visible = false;
                labelPT.Visible = false;
                tbPorcT.Visible = false;
                tBPorcDif.Visible = false;
                if (button1.Visible || button5.Visible || button6.Visible || button2.Visible)
                    hb = true;

                BloquearDGV(dataGridView1);
                BloquearDGV(dataGridView2);
            }
            else
                txtPwdM.Focus();
        }

        private void txtPwdM_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txtPwdM.Text.ToUpper() == "HCC123")
                {
                    modT = true;
                    txtPwdM.Text = "";
                    panelMC.Visible = false;
                    labelPDif.Visible = true;
                    labelPT.Visible = true;
                    tbPorcT.Visible = true;
                    tBPorcDif.Visible = true;
                    hb = false;
                }
                else
                {
                    if (txtPwdM.Text.Length > 0)
                        Help.ShowPopup(txtPasword, "Contraseña incorrecta", new Point(this.Location.X, this.Location.Y));
                    else
                        Help.ShowPopup(txtPasword, "Ingrese Contraseña", new Point(this.Location.X, this.Location.Y));
                }
            }

        }

        private void CalcularTotal()
        {
            modificarV = true;
            double consumo = 0, tracker = 0, dif = 0, pdif = 0, consumoF = 0;
            int renglon = dataGridView1.Rows.Count - 1;

            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                consumo += Convert.ToDouble(dataGridView1[6, i].Value);
                tracker += Convert.ToDouble(dataGridView1[9, i].Value);
            }

            int colB = conBasc == 3 ? 5 : 4;
            int colP = conBasc == 3 ? 6 : 5;
            int colT = conBasc == 3 ? 7 : 6;
            for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
            {
                consumoF += Convert.ToDouble(dataGridView2[colB, i].Value);
            }

            dif = consumo - tracker;
            pdif = dif / tracker * 100;

            consumoAlas = consumo;
            double valor;
            double total = consumo + consumoF;
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                valor = Convert.ToDouble(dataGridView1[6, i].Value);
                dataGridView1[7, i].Value = consumo > 0 ? valor / consumo * 100 : 0;
                dataGridView1[8, i].Value = total > 0 ? valor / total * 100 : 0;

            }

            for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
            {
                valor = Convert.ToDouble(dataGridView2[colB, i].Value);
                dataGridView2[colT, i].Value = total > 0 ? valor / total * 100 : 0;
            }

            dataGridView1[6, renglon].Value = consumo;
            dataGridView1[9, renglon].Value = tracker;
            dataGridView1[10, renglon].Value = dif;
            dataGridView1[11, renglon].Value = pdif;
            modificarV = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                int cont;
                Color c;
                SaveFileDialog fichero = new SaveFileDialog();
                fichero.Filter = "Excel (*.xls)|*.xls";
                fichero.FileName = "Prorrateo";
                if (fichero.ShowDialog() == DialogResult.OK)
                {
                    Microsoft.Office.Interop.Excel.Application aplicacion;
                    Microsoft.Office.Interop.Excel.Workbook libros;
                    Microsoft.Office.Interop.Excel.Worksheet hoja;
                    Microsoft.Office.Interop.Excel.Worksheet hoja2;
                    Microsoft.Office.Interop.Excel.Range rango1;
                    Microsoft.Office.Interop.Excel.Range rango2;

                    aplicacion = new Microsoft.Office.Interop.Excel.Application();
                    libros = aplicacion.Workbooks.Add();
                    hoja = (Microsoft.Office.Interop.Excel.Worksheet)libros.Worksheets.get_Item(1);
                    hoja.Name = "Forraje";

                    //hoja de Forraje
                    cont = 1;
                    for (int i = 0; i < dataGridView2.Columns.Count; i++)
                        hoja.Cells[1, i + 1] = dataGridView2.Columns[i].Name;

                    //rango1 = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[1, 1];
                    //rango2 = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[1, dataGridView2.Columns.Count];
                    //hoja.get_Range(rango1, rango2).Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(150, 207, 247));

                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {

                        for (int j = 0; j < dataGridView2.Columns.Count; j++)
                        {
                            if ((dataGridView2.Rows[i].Cells[j].Value == null) == false)
                                hoja.Cells[i + 2, j + 1] = dataGridView2.Rows[i].Cells[j].Value.ToString();
                        }

                        //rango1 = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[i + 2, 1];
                        //rango2 = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[i + 2, dataGridView2.Columns.Count];
                        //if ((cont % 2) == 0)
                        //    hoja.get_Range(rango1, rango2).Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(221, 235, 247));

                        cont++;
                    }

                    hoja.get_Range("A1", "I1").Font.Bold = true;
                    hoja.get_Range("A1", "I1").EntireColumn.AutoFit();
                    hoja.get_Range("G1").EntireColumn.Font.Bold = true;
                    hoja.get_Range("D:I").EntireColumn.NumberFormat = "#,0;-#,0";
                    hoja.get_Range("D:I").EntireColumn.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter; ;

                    hoja2 = libros.Worksheets.Add();
                    hoja2 = (Microsoft.Office.Interop.Excel.Worksheet)libros.Worksheets.get_Item(2);
                    hoja2 = (Microsoft.Office.Interop.Excel.Worksheet)libros.ActiveSheet;
                    hoja2.Name = "Alimentos";
                    //FormatosHoja2(hoja, dataGridView2.Rows.Count);
                    //hoja de Alimentos
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                        hoja2.Cells[1, i + 1] = dataGridView1.Columns[i].Name;


                    //rango1 = (Microsoft.Office.Interop.Excel.Range)hoja2.Cells[1, 1];
                    //rango2 = (Microsoft.Office.Interop.Excel.Range)hoja2.Cells[1, dataGridView1.Columns.Count];
                    //hoja2.get_Range(rango1, rango2).Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(150, 207, 247));

                    cont = 1;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {
                            if ((dataGridView1.Rows[i].Cells[j].Value == null) == false)
                                hoja2.Cells[i + 2, j + 1] = dataGridView1.Rows[i].Cells[j].Value.ToString();
                        }

                        //rango1 = (Microsoft.Office.Interop.Excel.Range)hoja2.Cells[i + 2, 1];
                        //rango2 = (Microsoft.Office.Interop.Excel.Range)hoja2.Cells[i + 2, dataGridView1.Columns.Count];

                        //if ((cont % 2) == 0)
                        //    hoja2.get_Range(rango1, rango2).Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(221, 235, 247));

                        cont++;
                    }
                    //string letra = "K" + dataGridView1.Rows.Count;
                    hoja2.get_Range("A1", "K1").Font.Bold = true;
                    hoja2.get_Range("A1", "K1").EntireColumn.AutoFit();
                    hoja2.get_Range("E:J").EntireColumn.NumberFormat = "#,0.0;-#,0.0";
                    hoja2.get_Range("E:K").EntireColumn.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter; ;
                    hoja2.get_Range("B1").EntireColumn.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    //FormatosHoja1(hoja2, dataGridView1.Rows.Count, dataGridView1.Columns.Count);

                    libros.SaveAs(fichero.FileName, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal);
                    libros.Close(true);
                    aplicacion.Quit();
                    Console.WriteLine(fichero.FileName.ToString());
                    Process.Start(fichero.FileName);
                }

            }
            catch (DbException ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatosHoja1(Microsoft.Office.Interop.Excel.Worksheet hoja, int rows, int columns)
        {
            Microsoft.Office.Interop.Excel.Range rango;
            double x;
            string cad;

            for (int i = 0; i < rows; i++)
            {
                rango = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[i + 2, 10];
                hoja.get_Range(rango, rango).Interior.Color = FormatoCeldaExcel(Convert.ToDouble(hoja.Cells[i + 2, 10].Value));
                rango = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[i + 2, 9];
                hoja.get_Range(rango, rango).Interior.Color = FormatoCeldaExcel(Convert.ToDouble(hoja.Cells[i + 2, 10].Value));
                cad = Convert.ToString(hoja.Cells[i + 2, columns].Value);

                if (cad != null)
                {
                    if (cad.Contains("✔"))
                    {
                        rango = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[i + 2, 11];
                        hoja.get_Range(rango, rango).Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Green);
                    }
                    else
                    {
                        rango = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[i + 2, 11];
                        hoja.get_Range(rango, rango).Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                        hoja.get_Range(rango, rango).Font.Bold = true;
                    }
                }

            }
        }

        private void FormatosHoja2(Microsoft.Office.Interop.Excel.Worksheet hoja, int rows)
        {
            Microsoft.Office.Interop.Excel.Range rango;
            double x;
            string cad;

            for (int i = 0; i < rows; i++)
            {
                rango = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[i + 2, 9];
                hoja.get_Range(rango, rango).Interior.Color = FormatoCeldaExcel(Convert.ToDouble(hoja.Cells[i + 2, 9].Value));
                rango = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[i + 2, 8];
                hoja.get_Range(rango, rango).Interior.Color = FormatoCeldaExcel(Convert.ToDouble(hoja.Cells[i + 2, 9].Value));
                cad = Convert.ToString(hoja.Cells[i + 2, 7].Value);

                if (cad != null)
                {
                    if (cad.Contains("✔"))
                    {
                        rango = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[i + 2, 7];
                        hoja.get_Range(rango, rango).Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Green);
                    }
                    else
                    {
                        rango = (Microsoft.Office.Interop.Excel.Range)hoja.Cells[i + 2, 7];
                        hoja.get_Range(rango, rango).Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                        hoja.get_Range(rango, rango).Font.Bold = true;
                    }
                }
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
            //dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToOrderColumns = false;
            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }


        private bool PermitirGuardar(DataGridView dgv, string tipo, bool load)
        {
            int cont = 0;
            double inv = 0, bascula = 0, tracker = 0, sie = 0, t = 0, dif = 0;
            int colT, colDif;

            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                if (tipo.ToUpper() == "ALIMENTOS")
                {
                    if (dgv[3, i].Value.ToString() != "TOTAL")
                    {
                        Double.TryParse(dgv[5, i].Value.ToString(), out inv);
                        Double.TryParse(dgv[6, i].Value.ToString(), out bascula);
                        Double.TryParse(dgv[9, i].Value.ToString(), out tracker);
                        Double.TryParse(dgv[4, i].Value.ToString(), out sie);
                        Double.TryParse(dgv[8, i].Value.ToString(), out t);
                        Double.TryParse(dgv[11, i].Value.ToString(), out dif);

                        if (inv == 0)
                            cont++;
                        else if (sie < bascula)
                            cont++;
                        else if ((bascula == 0 && tracker > 0) || (bascula > 0 && tracker == 0))
                            cont++;

                        if (btnGuardar && tipificacionCorrecta)
                            if (!load)
                                if (!autorizar)
                                    if (t >= porcT)
                                    {
                                        if (dif >= porcDif && raciones.ZeroTip == true || dif <= (porcDif * -1) && raciones.ZeroTip == true)
                                        {
                                            cont++;
                                            panel1Autorizar.Visible = true;
                                            button8.Enabled = true;
                                        }
                                    }
                    }
                }
                else if (tipo.ToUpper() == "FORRAJE")
                {
                    if (dgv[2, i].Value.ToString() != "TOTAL")
                    {
                        int colBal = dgv.Columns.Count == 12 ? 5 : 4;
                        int colTracker = dgv.Columns.Count == 12 ? 8 : 7;
                        colT = dgv.Columns.Count == 12 ? 7 : 6;
                        colDif = dgv.Columns.Count == 12 ? 11 : 10;

                        Double.TryParse(dgv[3, i].Value.ToString(), out inv);
                        Double.TryParse(dgv[colBal, i].Value.ToString(), out bascula);
                        Double.TryParse(dgv[colTracker, i].Value.ToString(), out tracker);
                        Double.TryParse(dgv[colT, i].Value.ToString(), out t);
                        Double.TryParse(dgv[colDif, i].Value.ToString(), out dif);


                        if (inv < bascula)
                            cont++;
                        else if (bascula > 0 && tracker == 0)
                            cont++;
                        else if (bascula == 0 && tracker > 0)
                            cont++;

                        if (btnGuardar && tipificacionCorrecta)
                            if (!load)
                                if (!autorizar)
                                {
                                    if (t >= porcT)
                                    {
                                        if (dif >= porcDif && raciones.ZeroTip == true || dif <= (porcDif * -1) && raciones.ZeroTip == true)
                                        {
                                            cont++;
                                            panel1Autorizar.Visible = true;
                                        }
                                    }
                                }
                    }

                }
            }

            return cont == 0;
        }

        private string Titulo(string ranIds)
        {
            string titulo = "";
            DataTable dt;
            string query = "SELECT ran_desc FROM configuracion WHERE ran_id IN(" + ranIds + ")";
            conn.QuerySIO(query, out dt);
            for (int i = 0; i < dt.Rows.Count; i++)
                titulo += dt.Rows[i][0].ToString().ToUpper() + ",";
            return titulo.Length > 0 ? titulo.Substring(0, titulo.Length - 1) : ran_nombre;
        }

        private string RanchosEP()
        {
            string ranchos = "";
            DataTable dt;
            string query = "SELECT ran_id FROM configuracion WHERE emp_prorrateo = " + emp_prorrateo;
            conn.QuerySIO(query, out dt);
            for (int i = 0; i < dt.Rows.Count; i++)
                ranchos += dt.Rows[i][0].ToString() + ",";

            return ranchos.Length > 0 ? ranchos.Substring(0, ranchos.Length - 1) : ran_id.ToString();
        }

        public void ColumnasAlimento(out DataTable dt)
        {
            dt = new DataTable();
            dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("ALMACEN").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("DISPONIBLE SIE").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("INV FINAL").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("CONSUMO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("%P").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("%T").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("CONSUMO TRACKER").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("DIF/KG").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("% DIF").DataType = System.Type.GetType("System.Double"); ;
            dt.Columns.Add("EXISTENCIA");
        }

        private void FillDGVAlimento(string ranIds)
        {
            double sumBascula = 0, sumTracker = 0, sumP = 0, sumT = 0;
            DateTime inicio = new DateTime(fecha.Year, fecha.Month, 1, hora_corte, 0, 0);
            inicio = inicio.AddDays(dias_a);
            DateTime corte = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora_corte, 0, 0);
            corte = inicio.Day == 1 && hora_corte > 0 ? corte.AddDays(1) : corte;

            DateTime fec_reg = new DateTime(fecha.Year, fecha.Month, 1, fecha.Hour, 0, 0);
            fec_reg = fec_reg.AddMonths(1).AddDays(-1);
            string query;
            string condicion = checkBox2.Checked ? " AND (sie.Existencia IS NOT NULL OR tracker.Peso IS NOT NULL) AND (sie.Existencia > 0 OR tracker.peso > 0) AND tracker.Peso > 0 " : "";
            DataTable dtV = new DataTable();

            string qry = "SELECT ISNULL(FORMAT(pro_fecha , 'd', 'en-gb' ),'') , alm_id, art_clave , prod_nombre, pro_existencia_sie, pro_inv_final, pro_consumo,pro_porc_b, pro_porc_t, pro_consumo_tra, pro_dif_kg, pro_dif "
                + " FROM prorrateoTemp "
                + "where ran_id = " + ran_id.ToString() + " AND pro_fecha_reg = '" + fec_reg.ToString("yyyy-MM-dd") + "' "
                + "  ";
            conn.QueryAlimento(qry, out dtV);

            DataTable dt = new DataTable();
            ColumnasAlimento(out dt);

            if (dtV.Rows.Count > 0)
            {
                for (int i = 0; i < dtV.Rows.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["FECHA"] = dtV.Rows[i][0];
                    dr["ALMACEN"] = dtV.Rows[i][1];
                    dr["CLAVE"] = dtV.Rows[i][2];
                    dr["ARTICULO"] = dtV.Rows[i][3];
                    dr["DISPONIBLE SIE"] = dtV.Rows[i][4];
                    dr["INV FINAL"] = dtV.Rows[i][5];
                    dr["CONSUMO"] = dtV.Rows[i][6];
                    dr["%P"] = dtV.Rows[i][7];
                    dr["%T"] = dtV.Rows[i][8];
                    dr["CONSUMO TRACKER"] = dtV.Rows[i][9];
                    dr["DIF/KG"] = dtV.Rows[i][10];
                    dr["% DIF"] = dtV.Rows[i][11];
                    dr["Existencia"] = Convert.ToDouble(dtV.Rows[i][4]) > Convert.ToDouble(dtV.Rows[i][6]) ? '✔' : 'X';
                    dt.Rows.Add(dr);
                }
            }
            else
            {
                DataTable dt1;
                query = "SELECT ISNULL(FORMAT(pro_fecha , 'd', 'en-gb' ),'') , alm_id, art_clave , prod_nombre, pro_existencia_sie, pro_inv_final, pro_consumo,pro_porc_b, pro_porc_t, pro_consumo_tra, pro_dif_kg, pro_dif "
                    + " FROM prorrateo"
                    + " where ran_id = " + ran_id.ToString() + " AND pro_fecha_reg = '" + fec_reg.ToString("yyyy-MM-dd") + "' AND SUBSTRING(art_clave,1,4) LIKE 'ALAS' "
                    + " ORDER BY pro_consumo_tra desc ";
                conn.QueryAlimento(query, out dt1);

                if (dt1.Rows.Count > 0)
                {
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr["FECHA"] = dt1.Rows[i][0];
                        dr["ALMACEN"] = dt1.Rows[i][1];
                        dr["CLAVE"] = dt1.Rows[i][2];
                        dr["ARTICULO"] = dt1.Rows[i][3];
                        dr["DISPONIBLE SIE"] = dt1.Rows[i][4];
                        dr["INV FINAL"] = dt1.Rows[i][5];
                        dr["CONSUMO"] = dt1.Rows[i][6];
                        dr["%P"] = dt1.Rows[i][7];
                        dr["%T"] = dt1.Rows[i][8];
                        dr["CONSUMO TRACKER"] = dt1.Rows[i][9];
                        dr["DIF/KG"] = dt1.Rows[i][10];
                        dr["% DIF"] = dt1.Rows[i][11];
                        dr["Existencia"] = Convert.ToDouble(dt1.Rows[i][4]) > Convert.ToDouble(dt1.Rows[i][6]) ? '✔' : 'X';
                        dt.Rows.Add(dr);

                    }
                    button6.Enabled = false;
                    button5.Enabled = false;
                    button2.Enabled = false;
                }
                else
                {
                    DataTable dt2;
                    double consumo, tracker, sie, invfinal, porcdif, difkg;

                    query = "SELECT DISTINCT T.Fecha, T.Almacen, T.Clave, T.Articulo, T.ExistenciaSIE, T.ConsumoTracker "
                        + "FROM ( "
                        + "SELECT DISTINCT ISNULL(FORMAT(sie.Fecha, 'd', 'en-gb'), '" + fecha.ToString("dd/MM/yyyy") + "') AS Fecha, ISNULL(sie.Almacen, " + ali_alm_id + ") AS Almacen, p.prod_clave AS Clave, p.prod_nombre AS Articulo, "
                            + " ISNULL(sie.Existencia, 0) AS ExistenciaSIE, ISNULL(tracker.Peso, 0) AS ConsumoTracker "
                            + " FROM producto p "
                            + " LEFT JOIN( "
                                + " SELECT alm.ran_id AS Rancho, alm.alm_id AS Almacen, art.art_clave AS Clave, art.art_existencia AS Existencia, art.art_fecha AS Fecha "
                                + " FROM articulo art "
                                + " LEFT JOIN[DBSIE].[dbo].almacen alm ON art.alm_id = alm.alm_id "
                                + " where CONVERT(DATE, art_fecha) = '" + fecha_SIE.ToString("yyyy-MM-dd") + "' AND alm.ran_id IN(" + ranIds + ") "
                                + " AND alm.alm_tipo = 2 "
                            + " )sie ON sie.Clave = p.prod_clave "
                            + " LEFT JOIN( "
                                    + " SELECT X.Clave, SUM(x.Peso) AS Peso "
                                    + " FROM( "
                                    + " SELECT ing_clave AS Clave, ing_descripcion AS Ing, SUM(rac_mh) AS Peso "
                                    + " FROM racion "
                                    + " WHERE ran_id IN(" + ranIds + ") "
                                    + " AND rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + corte.ToString("yyyy-MM-dd HH:mm") + "' "
                                    + " AND SUBSTRING(rac_descripcion, 3, 2) NOT IN('00', '01', '02') "
                                    + " AND SUBSTRING(ing_clave, 1, 4) IN('ALAS') "
                                    + " GROUP BY ing_clave, ing_descripcion "
                                    + " UNION "
                                    + " SELECT R.Clave, R.Ing, SUM(R.Peso) "
                                    + " FROM( "
                                    + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, IIF(T.Pmz = '', T.Ing1, T.Ing2) AS Ing, T.Peso1 * T.Porc AS Peso "
                                    + " FROM( "
                                    + " SELECT T1.Clave AS Clave1, T1.Ing AS Ing1, T1.Peso AS Peso1, ISNULL(T2.Pmz, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, "
                                    + " ISNULL(T2.Porc, 1) AS Porc "
                                    + " FROM( "
                                    + " SELECT R.Clave, R.Ing, SUM(R.Peso) AS Peso "
                                    + " FROM( "
                                    + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso "
                                    + " FROM( "
                                    + " select ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                                    + " FROM racion "
                                    + " WHERE ran_id IN(" + ranIds + ") "
                                    + " AND rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + corte.ToString("yyyy-MM-dd HH:mm") + "' "
                                    + " AND SUBSTRING(rac_descripcion, 3, 2) NOT IN('00', '01', '02') "
                                    + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 "
                                    + " AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                                    + " GROUP BY ing_descripcion) T1 "
                                    + " LEFT JOIN( "
                                    + " select pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                                    + " FROM porcentaje_Premezcla "
                                    + " )T2 ON T1.Pmz = T2.Pmz) R "
                                    + " GROUP BY R.Clave, R.Ing) T1 "
                                    + " LEFT JOIN( "
                                    + " SELECT pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                                    + " FROM porcentaje_Premezcla "
                                    + " ) T2 ON T1.Ing = T2.Pmz) T) R "
                                    + " WHERE SUBSTRING(R.Clave, 1, 4)  IN('ALAS') "
                                    + " GROUP BY R.Clave, R.Ing) X "
                                    + " GROUP BY X.Clave "
                           + " )tracker ON tracker.Clave = p.prod_clave "
                            + " WHERE SUBSTRING(p.prod_clave,1,4) IN ('ALAS') " + condicion + "  ) T"
                            + " ORDER BY T.ConsumoTracker desc, T.ExistenciaSIE desc ";
                    conn.QueryAlimento(query, out dt2);

                    for (int i = 0; i < dt2.Rows.Count; i++)
                    {
                        sie = Convert.ToDouble(dt2.Rows[i][4]);
                        tracker = Convert.ToDouble(dt2.Rows[i][5]);

                        if (prorrateo == 3)
                        {
                            if (tracker > 0)
                                consumo = sie > 0 ? sie - 1 : 0;
                            else
                                consumo = 0;

                            invfinal = sie - consumo;
                            if (invfinal == 0 && sie > 0)
                            {
                                invfinal = 1;
                                consumo = consumo - 1;
                            }
                        }
                        else
                        {
                            if (tracker > 0)
                                consumo = tracker < sie ? tracker : sie;
                            else
                                consumo = 0;

                            invfinal = sie - consumo;
                            if (invfinal == 0 && sie > 0)
                            {
                                invfinal = 1;
                                consumo = consumo - 1;
                            }
                        }

                        difkg = consumo - tracker;
                        porcdif = tracker > 0 ? difkg / tracker * 100 : 0;
                        DataRow dr = dt.NewRow();
                        dr["FECHA"] = dt2.Rows[i][0];
                        dr["ALMACEN"] = dt2.Rows[i][1];
                        dr["CLAVE"] = dt2.Rows[i][2];
                        dr["ARTICULO"] = dt2.Rows[i][3];
                        dr["DISPONIBLE SIE"] = sie;
                        dr["INV FINAL"] = invfinal;
                        dr["CONSUMO"] = consumo;
                        dr["CONSUMO TRACKER"] = tracker;
                        dr["DIF/KG"] = difkg;
                        dr["% DIF"] = porcdif;
                        dr["Existencia"] = sie > consumo ? '✔' : 'X';
                        dt.Rows.Add(dr);
                    }
                }
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sumBascula += Convert.ToDouble(dt.Rows[i]["CONSUMO"]);
                sumTracker += Convert.ToDouble(dt.Rows[i]["CONSUMO TRACKER"]);
            }

            double cons;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                cons = Convert.ToDouble(dt.Rows[i]["CONSUMO"]);
                dt.Rows[i]["%P"] = sumBascula > 0 ? cons / sumBascula * 100 : 0;
                sumP += sumBascula > 0 ? cons / sumBascula * 100 : 0;
            }



            DataRow drT = dt.NewRow();
            drT["ARTICULO"] = "TOTAL";
            drT["CONSUMO"] = sumBascula;
            drT["CONSUMO TRACKER"] = sumTracker;
            drT["DIF/KG"] = sumBascula - sumTracker;
            drT["% DIF"] = (sumBascula - sumTracker) / sumTracker * 100;
            drT["%P"] = sumP;
            drT["%T"] = sumT;
            dt.Rows.Add(drT);

            dataGridView1.DataSource = dt;
            dataGridView1.AutoResizeColumns();
            FormatoGrid(dataGridView1, 4);
        }

        private string Sobrantes(string ranIds)
        {
            DateTime racion_inicio = new DateTime(fecha.Year, fecha.Month, 1, hora_corte, 0, 0);
            racion_inicio = racion_inicio.AddDays(-1);
            DateTime racion_fin = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora_corte, 0, 0);
            string ranTemp = ranIds.Length > 0 ? ranIds : ran_id.ToString();
            string sob = "";
            DataTable dt;
            string query = "SELECT DISTINCT ing_descripcion "
                     + " FROM racion "
                     + " WHERE rac_fecha >= '" + racion_inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha< '" + racion_fin.ToString("yyyy-MM-dd HH:mm") + "' "
                     + " AND ran_id IN(" + ranTemp + ") "
                     + " AND SUBSTRING(ing_clave,1,4) not in('ALAS', 'ALFO') "
                     + " AND SUBSTRING(ing_descripcion,3,2) NOT IN('00','01','02') "
                     + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','W') ";
            conn.QueryAlimento(query, out dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sob += "'" + dt.Rows[i][0].ToString() + "',";
            }

            return sob.Length > 0 ? sob.Substring(0, sob.Length - 1) : "''";
        }
        //Funcion cuando cambias de tab, para activar y desactivar dgvs segun el caso.
        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                dataGridView1.Visible = true;
                dataGridView2.Visible = false;
            }
            else
            {
                dataGridView1.Visible = false;
                dataGridView2.Visible = true;

            }
        }

        private void dataGridView2_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                int colBal = dataGridView2.Columns.Count == 12 ? 5 : 4;
                int colTrack = dataGridView2.Columns.Count == 12 ? 8 : 7;

                if (modT)
                {
                    if (dataGridView2.CurrentCell.ColumnIndex == colBal || dataGridView2.CurrentCell.ColumnIndex == colTrack)
                    {
                        dataGridView2.Columns[colBal].ReadOnly = false;
                        dataGridView2.Columns[colTrack].ReadOnly = false;
                        dataGridView2.CurrentCell = dataGridView2.CurrentRow.Cells[dataGridView2.CurrentCell.ColumnIndex];
                        dataGridView2.BeginEdit(true);
                    }
                }
            }
            catch
            {

            }
        }

        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int columna = e.ColumnIndex;
            int renglon = e.RowIndex;
            int colBasc = dataGridView2.Columns.Count == 12 ? 5 : 4;
            int colTrack = dataGridView2.Columns.Count == 12 ? 8 : 7;

            if (modificarV == false)
            {
                porcTActivo = true;
                if (e.ColumnIndex == colBasc)
                {
                    if (dataGridView2[columna, renglon].Value.ToString() != "")
                        RecalcularF(dataGridView2, renglon, columna, dataGridView2.Columns.Count);
                    else
                    {
                        dataGridView2.CurrentCell = dataGridView2.CurrentRow.Cells[columna];
                        dataGridView2.BeginEdit(true);
                    }
                }
                else if (e.ColumnIndex == colTrack)
                {
                    if (dataGridView2[columna, renglon].Value.ToString() != "")
                        RecalcularF(dataGridView2, renglon, columna, dataGridView2.Columns.Count);
                    else
                    {
                        dataGridView2.CurrentCell = dataGridView2.CurrentRow.Cells[columna];
                        dataGridView2.BeginEdit(true);
                    }
                }
                CalcularTotalF(dataGridView2.Columns.Count);
            }
        }
        private void CalcularTotalF(int columnas)
        {
            modificarV = true;
            int coldif = columnas == 12 ? 10 : 9;
            int colpdif = columnas == 12 ? 11 : 10;
            int colBal = columnas == 12 ? 5 : 4;
            int colTrack = columnas == 12 ? 8 : 7;
            int colp = columnas == 12 ? 6 : 5;
            int colT = columnas == 12 ? 7 : 6;
            double sumBascula = 0, sumTracker = 0, dif = 0, pdif = 0, consumo, total = 0;
            for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
            {
                sumBascula += Convert.ToDouble(dataGridView2[colBal, i].Value);
                sumTracker += Convert.ToDouble(dataGridView2[colTrack, i].Value);
            }

            consumo = 0;
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                consumo += Convert.ToDouble(dataGridView1[6, i].Value);
            }

            double v;
            total = sumBascula + consumo;
            for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
            {
                v = Convert.ToDouble(dataGridView2[colBal, i].Value);
                dataGridView2[colp, i].Value = sumBascula > 0 ? v / sumBascula * 100 : 0;
                dataGridView2[colT, i].Value = total > 0 ? v / total * 100 : 0;
            }

            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                v = Convert.ToDouble(dataGridView1[6, i].Value);
                dataGridView1[8, i].Value = total > 0 ? v / total * 100 : 0;
            }

            dif = sumBascula - sumTracker;
            pdif = dif / sumTracker * 100;

            dataGridView2[colBal, dataGridView2.Rows.Count - 1].Value = sumBascula;
            dataGridView2[colTrack, dataGridView2.Rows.Count - 1].Value = sumTracker;
            dataGridView2[coldif, dataGridView2.Rows.Count - 1].Value = dif;
            dataGridView2[colpdif, dataGridView2.Rows.Count - 1].Value = pdif;
            consumoAlfo = sumBascula;
            modificarV = false;
        }

        private void RecalcularF(DataGridView dgv, int row, int column, int ColumnasT)
        {
            modificarV = true;
            modCTF = true;
            int coldif = ColumnasT == 12 ? 10 : 9;
            int colpdif = ColumnasT == 12 ? 11 : 10;
            int colBal = ColumnasT == 12 ? 5 : 4;
            int colTrack = ColumnasT == 12 ? 8 : 7;
            int colExist = ColumnasT == 12 ? 9 : 8;
            
            if (ran_bascula == 0)
            {
                dgv[4, row].Value = dgv[7, row].Value;
            }

            if (!dgv[2, row].Value.ToString().Contains("TOTAL"))
            {
                double bal = 0, track = 0, dif = 0, pdif = 0, inv;
                bool exist;
                inv = Convert.ToDouble(dgv[3, row].Value);
                bal = Convert.ToDouble(dgv[colBal, row].Value);
                track = Convert.ToDouble(dgv[colTrack, row].Value);
                exist = inv >= bal;
                char existencia = inv >= bal ? '✔' : 'X';
                dif = bal - track;
                pdif = track > 0 ? dif / track * 100 : 0;
                dgv[colExist, row].Value = existencia;
                dgv[coldif, row].Value = dif;
                dgv[colpdif, row].Value = pdif;
                modificarV = false;
               
                if((inv == 0 && bal == 0 && track == 0 ) || (inv > 0 && bal == 0 && track == 0 ))
                {
                    dgv.Rows.RemoveAt(row);
                }
            }
        }

        private void tBPorcDif_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Double.TryParse(tBPorcDif.Text, out porcDif);
                tBPorcDif.Text = porcDif.ToString();
                string query = "UPDATE porcentaje_prorrateo SET pp_porc_dif = " + porcDif;
                conn.QueryAlimento(query);
                label1.Focus();
            }
        }

        private void tbPorcT_Validated(object sender, EventArgs e)
        {
            if (tbPorcT.Text.Length > 0)
            {
                if (tbPorcT.Text.Length == 1)
                {
                    char caracter = Convert.ToChar(tbPorcT.Text);
                    if (caracter == '.')
                    {
                        porcT = 0;
                        tbPorcT.Text = porcT.ToString();
                        string query = "UPDATE porcentaje_prorrateo SET pp_porc_t = " + porcT.ToString();
                        conn.QueryAlimento(query);
                    }
                    else
                    {
                        porcT = Convert.ToDouble(tbPorcT.Text);

                    }
                }
                else
                {
                    porcT = Convert.ToDouble(tbPorcT.Text);
                    string query = "UPDATE porcentaje_prorrateo SET pp_porc_t = " + porcT.ToString();
                    conn.QueryAlimento(query);
                }
                FormatoCeldaT();
            }
            else
                tbPorcT.Text = porcT.ToString();
        }

        private void tbPorcT_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && e.KeyChar > 0)
            {
                e.Handled = true;
            }

            // solo 1 punto decimal
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void tBPorcDif_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && e.KeyChar > 0)
            {
                e.Handled = true;
            }

            // solo 1 punto decimal
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }

            //solo 1 signo de - 
            if ((e.KeyChar == '-') && ((sender as TextBox).Text.IndexOf('-') > -1))
            {
                e.Handled = true;
            }
        }

        private void tBPorcDif_Validated(object sender, EventArgs e)
        {
            if (tBPorcDif.Text.Length > 0)
            {
                string query;
                if (tBPorcDif.Text.Length == 1)
                {
                    char caracter = Convert.ToChar(tBPorcDif.Text);
                    if (char.IsDigit(caracter))
                        porcDif = Convert.ToDouble(tBPorcDif.Text);
                    else
                        porcDif = 0;

                }
                else
                    porcDif = Convert.ToDouble(tBPorcDif.Text);

                query = "UPDATE porcentaje_prorrateo SET pp_porc_dif = " + porcDif.ToString();
                conn.QueryAlimento(query);

                FormatoCeldaT();

            }
            else
                tBPorcDif.Text = porcDif.ToString();

        }

        private void tbPorcT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbPorcT.Text.Length > 0)
                {
                    Double.TryParse(tbPorcT.Text, out porcT);
                    tbPorcT.Text = porcT.ToString();
                    // porcT = Convert.ToDouble(tbPorcT.Text);
                    string query = "UPDATE porcentaje_prorrateo SET pp_porc_t = " + porcT.ToString();
                    conn.QueryAlimento(query);
                }
                else
                    tbPorcT.Text = porcT.ToString();

                label1.Focus();
            }
        }

        private void FormatoCeldaT()
        {
            double valorT, valorP;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (!dataGridView1[3, i].Value.ToString().Contains("TOTAL"))
                {
                    valorP = Convert.ToDouble(dataGridView1[11, i].Value);
                    valorT = Convert.ToDouble(dataGridView1[8, i].Value);

                    if (valorT >= porcT)
                    {
                        if (valorP >= porcDif || valorP <= (porcDif * -1))
                            dataGridView1[8, i].Style.BackColor = Color.FromArgb(255, 201, 201);
                        else
                            dataGridView1[8, i].Style.BackColor = dataGridView1[0, i].Style.BackColor;
                    }

                }
            }

            int colP = conBasc == 3 ? 11 : 10;
            int colT = conBasc == 3 ? 7 : 6;
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                if (!dataGridView2[2, i].Value.ToString().Contains("TOTAL"))
                {
                    valorP = Convert.ToDouble(dataGridView2[colP, i].Value);
                    valorT = Convert.ToDouble(dataGridView2[colT, i].Value);

                    if (valorT >= porcT)
                    {
                        if (valorP >= porcDif || valorP <= (porcDif * -1))
                            dataGridView2[colT, i].Style.BackColor = Color.FromArgb(255, 201, 201);
                        else
                            dataGridView2[colT, i].Style.BackColor = dataGridView1[0, i].Style.BackColor;
                    }
                    else
                        dataGridView2[colT, i].Style.BackColor = dataGridView1[0, i].Style.BackColor;

                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Autorizar f2 = new Autorizar(ran_id, ran_nombre, emp_id, emp_nombre);
            if (f2.ShowDialog() == DialogResult.OK)
            {
                if (f2.Vpwd)
                {
                    autorizar = true;
                    Guardar();
                }
                else
                {
                    MessageBox.Show("Contraseña Invalida", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            //button8.Enabled = false;

        }

        private void dataGridView1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            double v = 0, existencia;
            if (e.ColumnIndex == 5)
            {
                if (dataGridView1[3, e.RowIndex].Value.ToString() != "TOTAL")
                {
                    v = dataGridView1[e.ColumnIndex, e.RowIndex].Value != DBNull.Value ? Convert.ToDouble(dataGridView1[e.ColumnIndex, e.RowIndex].Value) : 0;
                    existencia = dataGridView1[4, e.RowIndex].Value != DBNull.Value ? Convert.ToDouble(dataGridView1[4, e.RowIndex].Value) : 0;
                    if (v < 1)
                        dataGridView1[e.ColumnIndex, e.RowIndex].Value = existencia > 0 ? 1 : 0;
                }


            }

            if ((DateTime.Today.Day > 2 && DateTime.Today.Day < 6 || hb || button2.Visible) && almCerrados)
            {
                bool alas = PermitirGuardar(dataGridView1, "ALIMENTOS", load);
                bool alfo = PermitirGuardar(dataGridView2, "FORRAJE", load);
                bool habBot = alas && alfo ? true : false;
                if (!ExisteProrrateo())
                {
                    button6.Enabled = habBot;
                    button5.Enabled = habBot;
                    button6.Visible = habBot;
                    button5.Visible = habBot;
                    button9.Enabled = habBot;
                    button10.Enabled = habBot;
                    button9.Visible = habBot;
                    button10.Visible = habBot;
                    button1.Visible = true;
                    button1.Enabled = true;

                }
                //button8.Enabled = habBot;
            }


        }

        private bool Fiabilidad()
        {
            bool fiabilidad;
            int num;
            DataTable dt;
            string query = "SELECT SUM(T.Val) AS Validar "
                            + " FROM( "
                            + " select prod_nombre, IIF(pro_porc_t > (SELECT pp_porc_t FROM porcentaje_prorrateo), IIF(pro_dif > (SELECT pp_porc_dif * -1 FROM porcentaje_prorrateo) OR pro_dif > (SELECT pp_porc_dif FROM porcentaje_prorrateo),0,1),0) AS Val "
                            + " from prorrateo "
                            + " where pro_fecha_reg = '" + fecha_reg.ToString("yyyy-MM-dd") + "' ) T";
            conn.QueryAlimento(query, out dt);
            num = dt.Rows[0][0] != DBNull.Value ? Convert.ToInt32(dt.Rows[0][0]) : 0;
            fiabilidad = num > 0 ? false : true;

            return fiabilidad;
        }

        private bool ExportoProrrateo()
        {
            DataTable dt;
            string query = "select * from prorrateo_sie WHERE ps_fecha = '" + fecha_reg.ToString("yyyy-MM-dd") + "' AND ps_kilos > 0";
            conn.QueryAlimento(query, out dt);

            return dt.Rows.Count > 0;
        }

        private bool PermitirGuardar(DataGridView dgv, string tipo)
        {
            int cont = 0;
            double inv = 0, bascula = 0, tracker = 0, sie = 0, t = 0, dif = 0;
            int colT, colDif;

            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                if (tipo.ToUpper() == "ALIMENTOS")
                {
                    if (dgv[3, i].Value.ToString() != "TOTAL")
                    {
                        Double.TryParse(dgv[5, i].Value.ToString(), out inv);
                        Double.TryParse(dgv[6, i].Value.ToString(), out bascula);
                        Double.TryParse(dgv[9, i].Value.ToString(), out tracker);
                        Double.TryParse(dgv[4, i].Value.ToString(), out sie);
                        Double.TryParse(dgv[8, i].Value.ToString(), out t);
                        Double.TryParse(dgv[11, i].Value.ToString(), out dif);

                        if (inv == 0)
                            cont++;
                        else if (sie < bascula)
                            cont++;
                        else if ((bascula == 0 && tracker > 0) || (bascula > 0 && tracker == 0))
                            cont++;
                        if (!autorizar)
                            if (t >= porcT)
                            {
                                if (dif >= porcDif || dif <= (porcDif * -1))
                                {
                                    cont++;
                                    //panel1Autorizar.Visible = true;
                                }
                            }
                    }
                }
                else if (tipo.ToUpper() == "FORRAJE")
                {
                    if (dgv[2, i].Value.ToString() != "TOTAL")
                    {
                        int colBal = dgv.Columns.Count == 12 ? 5 : 4;
                        int colTracker = dgv.Columns.Count == 12 ? 8 : 7;
                        colT = dgv.Columns.Count == 12 ? 7 : 6;
                        colDif = dgv.Columns.Count == 12 ? 11 : 10;

                        Double.TryParse(dgv[3, i].Value.ToString(), out inv);
                        Double.TryParse(dgv[colBal, i].Value.ToString(), out bascula);
                        Double.TryParse(dgv[colTracker, i].Value.ToString(), out tracker);
                        Double.TryParse(dgv[colT, i].Value.ToString(), out t);
                        Double.TryParse(dgv[colDif, i].Value.ToString(), out dif);


                        if (inv < bascula)
                            cont++;
                        else if (bascula > 0 && tracker == 0)
                            cont++;
                        else if (bascula == 0 && tracker > 0)
                            cont++;
                        if (!autorizar)
                        {
                            if (t >= porcT)
                            {
                                if (dif >= porcDif || dif <= (porcDif * -1))
                                {
                                    cont++;

                                }
                            }
                        }
                    }

                }
            }

            return cont == 0;
        }

        private void dataGridView2_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if ((DateTime.Today.Day > 2 && DateTime.Today.Day < 6 || hb || button2.Visible ) && almCerrados)
            {
                bool alas = PermitirGuardar(dataGridView1, "ALIMENTOS", load);
                bool alfo = PermitirGuardar(dataGridView2, "FORRAJE", load);
                bool habBot = alas && alfo ? true : false;
                if (!ExisteProrrateo())
                {
                    button6.Enabled = habBot;
                    button5.Enabled = habBot;
                    button6.Visible = habBot;
                    button5.Visible = habBot;
                    button9.Enabled = habBot;
                    button10.Enabled = habBot;
                    button9.Visible = habBot;
                    button10.Visible = habBot;
                    button1.Visible = true;
                    button1.Enabled = true;
                }
            }
        }

        private void gth1746()
        {
            bool almC = true;
            ght001746 gth = new ght001746(sUrl, "", "", "");
            wALMABIERTODataTable almacen = new wALMABIERTODataTable();
            string temp = fecha.Year.ToString();
            string temp1 = fecha.Month > 9 ? fecha.Month.ToString() : "0" + fecha.Month.ToString();
            temp = temp + temp1;
            int periodo = Convert.ToInt32(temp);
            gth.ght001746q(ran_id, periodo, out almacen);
            string[] almacenes = Almacenes(ran_id, "2,3").Split(',');

            bool abierto = true;
            foreach (wALMABIERTORow row in almacen)
            {
                for (int i = 0; i < almacenes.Length; i++)
                {
                    if (row.AlmClave.ToUpper() == almacenes[i].ToUpper())
                    {
                        almCerrados = !row.Abierto;
                        almC = !row.Abierto;
                    }

                    abierto = row.Abierto;
                    if (row.AlmClave.ToUpper() == almacenes[i].ToUpper() && row.Abierto == false)
                        break;
                }

                if (abierto == false)
                    break;
            }


            
            if (almC)
            {
                label13.Text = "ALMACENES CERADOS";
                label13.ForeColor = Color.Green;
            }
        }


        //private void ght1746()
        //{
        //    bool almC = true;
        //    ght001746 gth = new ght001746(sUrl, "", "", "");
        //    wALMABIERTODataTable almacen = new wALMABIERTODataTable();
        //    string temp = fecha.Year.ToString();
        //    string temp1 = fecha.Month > 9 ? fecha.Month.ToString() : "0" + fecha.Month.ToString();
        //    temp = temp + temp1;
        //    int periodo = Convert.ToInt32(temp);
        //    gth.ght001746q(ran_id, periodo, out almacen);
        //    string[] almacenes = Almacenes(ran_id).Split(',');

        //    foreach (wALMABIERTORow row in almacen)
        //    {
        //        for(int i = 0; i < almacenes.Length; i++) 
        //        {
        //            if(row.AlmClave == almacenes[i].ToUpper())
        //            {

        //            }
        //        } 
        //    }

        //}

        private string Almacenes(int ran_id, string tipo)
        {
            string alm = "";
            DataTable dt;
            string query = "select alm_id from almacen WHERE ran_id = " + ran_id.ToString() + " AND alm_tipo IN(" + tipo + ")";
            conn.QuerySIE(query, out dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                alm += dt.Rows[i][0].ToString() + ",";
            }

            return alm.Length > 0 ? alm.Substring(0, alm.Length - 1) : alm;
        }

        private void GuardarTemporal()
        {
            Cursor = Cursors.WaitCursor;
            DateTime fec;
            string alm_id, art_clave, prod_nombre, valores = "", fechaCadena = "";
            double exi_sie, cons, cons_tracker, dif, difporc, invfinal, porcP, porcT;
            string sing, sing2, scons, sing3, fec_cadena = "";
            int ing, ing2, consum;
            DateTime fec_reg = new DateTime(fecha.Year, fecha.Month, 1, fecha.Hour, 0, 0);
            fec_reg = fec_reg.AddMonths(1).AddDays(-1);

            bool valAlimentos = PermitirGuardar(dataGridView1, "alimentos", false);


            if (valAlimentos == false)
                MessageBox.Show("Validar Prorrateo", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                conn.DeleteAlimento("prorrateoTemp", "where ran_id = " + ran_id + " AND pro_fecha = '" + fec_reg.ToString("yyyy-MM-dd") + "'");
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (!dataGridView1[3, i].Value.ToString().Contains("TOTAL"))
                    {
                        if (dataGridView1[0, i].Value.ToString().Length > 0)
                        {
                            fec = Convert.ToDateTime(dataGridView1[0, i].Value);
                            fec_cadena = fec.ToString("yyyy-MM-dd");
                        }
                        else
                            fec_cadena = "";

                        alm_id = dataGridView1[1, i].Value.ToString();
                        art_clave = dataGridView1[2, i].Value.ToString();
                        prod_nombre = dataGridView1[3, i].Value.ToString();
                        exi_sie = Convert.ToDouble(dataGridView1[4, i].Value);
                        invfinal = Convert.ToDouble(dataGridView1[5, i].Value);
                        cons = Convert.ToDouble(dataGridView1[6, i].Value);
                        porcP = Convert.ToDouble(dataGridView1[7, i].Value);
                        porcT = Convert.ToDouble(dataGridView1[8, i].Value);
                        cons_tracker = Convert.ToDouble(dataGridView1[9, i].Value);
                        dif = Convert.ToDouble(dataGridView1[10, i].Value);
                        difporc = Convert.ToDouble(dataGridView1[11, i].Value);

                        if (fec_cadena != "")
                        {
                            valores += "(" + ran_id.ToString() + ",'" + fec_cadena + "', '" + alm_id + "', '" + art_clave + "', '" + prod_nombre + "', " + exi_sie.ToString() + "," + invfinal + "," + cons + "," + cons_tracker + ","
                               + dif.ToString() + ", " + difporc + ",'" + fec_reg.ToString("yyyy-MM-dd") + "'," + porcP + "," + porcT + ", NULL, NULL),";
                        }
                        else
                        {
                            valores += "(" + ran_id.ToString() + ", NULL , '" + alm_id + "', '" + art_clave + "', '" + prod_nombre + "', " + exi_sie.ToString() + "," + invfinal + "," + cons + "," + cons_tracker + ","
                            + dif.ToString() + ", " + difporc + ",'" + fec_reg.ToString("yyyy-MM-dd") + "'," + porcP + "," + porcT + ",NULL,NULL),";
                        }
                    }
                }
                valores = valores.Substring(0, valores.Length - 1);
                conn.InsertMasivAlimento("prorrateoTemp", valores);
            }

            Cursor = Cursors.Default;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            GuardarTemporal();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Guardar();
        }
        Tipificaciones raciones;
        private void button11_Click(object sender, EventArgs e)
        {
            DateTime racion_inicio = new DateTime(fecha.Year, fecha.Month, 1, hora_corte, 0, 0);
            racion_inicio = racion_inicio.AddDays(dias_a);
            DateTime racion_fin = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora_corte, 0, 0);
            racion_fin = racion_inicio.Day == 1 && hora_corte > 0 ? racion_fin.AddDays(1) : racion_fin;            
            DataTable dtALAS, dtALFO;
            ColumnasAlimento(out dtALAS);
            ColumnasForaje(out dtALFO);

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1[3, i].Value.ToString() != "TOTAL")
                {
                    DataRow row = dtALAS.NewRow();
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        row[j] = dataGridView1[j, i].Value;

                    dtALAS.Rows.Add(row);
                }
            }

            for (int i = 0; i < dataGridView2.Rows.Count; i++) 
            {
                if (dataGridView2[2,i].Value.ToString() != "TOTAL")
                {
                    DataRow row = dtALFO.NewRow();
                    for (int j = 0; j < dtALFO.Columns.Count; j++)
                        row[j] = dataGridView2[j, i].Value;

                    dtALFO.Rows.Add(row);
                }
            }

            //Tipificaciones raciones = new Tipificaciones(ranchosId, emp_id, racion_inicio, racion_fin, dtALAS, dtALFO, conBasc);
            raciones = new Tipificaciones(ranchosId, emp_id, racion_inicio, racion_fin, dtALAS, dtALFO, conBasc);
            if (raciones.ShowDialog() == DialogResult.OK)
            {

            }

            /*if (raciones.ZeroTip == true)
            {
                panel1Autorizar.Visible = true;
                panel1Autorizar.Enabled = true;
            }*/

        }

        private void button8_VisibleChanged(object sender, EventArgs e)
        {
            if (button8.Visible)
            {
                button5.Visible = false;
                button6.Visible = false;
                button9.Visible = false;
                button10.Visible = false;
            }
        }
    }
}

