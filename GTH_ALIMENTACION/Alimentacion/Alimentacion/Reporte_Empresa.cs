using Microsoft.Reporting.WinForms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alimentacion
{
    public partial class Reporte_Empresa : Form
    {
        ConnSIO conn = new ConnSIO();
        string emp_nombre;
        int emp_id, ran_id;
        string rancho;
        string ranNumero, ranCadena;
        string ruta;
        int dias;
        DateTime fechaMax;
        DateTime fechaMin;
        string emp_codigo;
        bool origen;

        public Reporte_Empresa(int ran_id, int emp_id, string emp_nombre)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;
        }

        public Reporte_Empresa(int ran_id, int emp_id, string emp_nombre, bool origen)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;
            this.origen = origen;
        }

        private void Reporte_Empresa_Load(object sender, EventArgs e)
        {
            conn.Iniciar("DBSIE");
            GetInfo(origen);
            dtp.Cursor = Cursors.Hand;
            button1.Cursor = Cursors.Hand;
            fechaMax = MaxDate();
            fechaMin = MinDate();
            dtp.MaxDate = fechaMax;
            dtp.MinDate = fechaMin;
        }

        private void GetInfo(bool origen)
        {
            DataTable dt;
            //string query = "select rut_ruta from ruta where ran_id = " + ran_id.ToString();
            //conn.QuerySIO(query, out dt);
            //ruta = dt.Rows[0][0].ToString();

            // MODIFICADO
            string query;
            if (origen)
            {
                query = "SELECT rut_ruta FROM ruta WHERE rut_desc = 'sio'";
                conn.QueryMovGanado(query, out dt);
            }
            else
            {
                query = "select rut_ruta from ruta where ran_id = " + ran_id.ToString();
                conn.QuerySIO(query, out dt);
            }
            //---
            ruta = dt.Rows[0][0].ToString();

            DataTable dt1;
            query = "SELECT ran_id FROM configuracion where emp_id = " + emp_id.ToString();
            conn.QuerySIO(query, out dt1);

            ranNumero = ""; ranCadena = "";
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                ranNumero += dt1.Rows[i][0].ToString() + ",";
                ranCadena += "'" + dt1.Rows[i][0].ToString() + "',";
            }
            ranNumero = ranNumero.Substring(0, ranNumero.Length - 1);
            ranCadena = ranCadena.Substring(0, ranCadena.Length - 1);

            DataTable dt2;
            query = "SELECT emp_codigo FROM configuracion WHERE ran_id = " + ran_id.ToString();
            conn.QuerySIO(query, out dt2);
            emp_codigo = dt2.Rows[0][0].ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    Cursor = Cursors.WaitCursor;

            //    int hcorte = 0;
            //    int horas;
            //    Hora_Corte(out horas, out hcorte);
            //    DateTime hoy = DateTime.Now;
            //    DateTime fechaFin = dtp.Value.Date;
            //    DateTime fechaIni = new DateTime(fechaFin.Year, fechaFin.Month, 1);
            //    int dif = 24 + horas;
            //    fechaIni = fechaIni.AddHours(horas);
            //    fechaFin = fechaFin.AddHours(dif);
            //    DataTable dtFinal = new DataTable(), dtInfoDia = new DataTable(), dtFinalAnt = new DataTable(), dtInfoDiaAnt = new DataTable();
            //    DataTable dtindicadores = new DataTable();
            //    TimeSpan ts = fechaFin - fechaIni;
            //    dias = ts.Days;                
            //    double dec1 = 0, dec2 = 0, dec3 = 0;
            //    //double promTot = 0, promOrd = 0, promSecas = 0, promHato = 0, promLact = 0, promProt = 0, promUrea = 0, promPorcG = 0, promCcs = 0, prom CTD, 

            //    TraerInfo(fechaIni.AddYears(-1), fechaFin.AddYears(-1), out dtFinalAnt, out dtInfoDiaAnt, out dec1, out dec2, out dec3);
            //    TraerInfo(fechaIni, fechaFin, out dtFinal, out dtInfoDia, out dec1, out dec2, out dec3);

            //    Diferencia(dtFinalAnt, dtFinal);
            //    Diferencia(dtInfoDiaAnt, dtInfoDia);

            //    string[] meses = new string[] { "ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO", "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE" };
            //    ReportParameter[] parameters = new ReportParameter[5];
            //    parameters[0] = new ReportParameter("EMPRESA", emp_codigo, true);
            //    parameters[1] = new ReportParameter("MES", meses[fechaFin.Month - 1] + " " + fechaIni.Year.ToString(), true);
            //    parameters[2] = new ReportParameter("DEC_UNO", dec1.ToString(), true);
            //    parameters[3] = new ReportParameter("DEC_DOS", dec2.ToString(), true);
            //    parameters[4] = new ReportParameter("DEC_TRES", dec3.ToString(), true);

            //    reportViewer1.LocalReport.DataSources.Clear();
            //    ReportDataSource sour = new ReportDataSource("DataSet1", dtFinal);
            //    reportViewer1.LocalReport.DataSources.Add(sour);
            //    sour = new ReportDataSource("DataSet2", dtInfoDia);
            //    reportViewer1.LocalReport.DataSources.Add(sour);
            //    reportViewer1.LocalReport.SetParameters(parameters);
            //    reportViewer1.LocalReport.Refresh();
            //    reportViewer1.RefreshReport();

            //    byte[] Bytes = reportViewer1.LocalReport.Render(format: "PDF", deviceInfo: "");
            //    try
            //    {
            //        using (FileStream stream = new FileStream("C:\\MOVGANADO\\reportes\\Reportes\\Dia_Empresa.pdf", FileMode.Create))
            //        {
            //            stream.Write(Bytes, 0, Bytes.Length);
            //        }

            //        Process.Start("C:\\MOVGANADO\\reportes\\Reportes\\Dia_Empresa.pdf");
            //    }
            //    catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            //    Cursor = Cursors.Default;


            //}
            //catch (IOException ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2); }
            //catch (DbException ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2); }
            //catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2); }


            Cursor = Cursors.WaitCursor;
            ReporteDE(dtp.Value.Date);
            Cursor = Cursors.Default;
        }

        public void ReporteDE(DateTime dtp)
        {
            try
            {
                conn.Iniciar("DBSIE");
                GetInfo(origen);
                int hcorte = 0;
                int horas;
                Hora_Corte(out horas, out hcorte);
                DateTime hoy = DateTime.Now;
                DateTime fechaFin = dtp.Date;
                DateTime fechaIni = new DateTime(fechaFin.Year, fechaFin.Month, 1);
                int dif = 24 + horas;
                fechaIni = fechaIni.AddHours(horas);
                fechaFin = fechaFin.AddHours(dif);
                DataTable dtFinal = new DataTable(), dtInfoDia = new DataTable(), dtFinalAnt = new DataTable(), dtInfoDiaAnt = new DataTable();
                DataTable dtindicadores = new DataTable();
                TimeSpan ts = fechaFin - fechaIni;
                dias = ts.Days;
                double dec1 = 0, dec2 = 0, dec3 = 0;
                //double promTot = 0, promOrd = 0, promSecas = 0, promHato = 0, promLact = 0, promProt = 0, promUrea = 0, promPorcG = 0, promCcs = 0, prom CTD, 

                TraerInfo(fechaIni.AddYears(-1), fechaFin.AddYears(-1), out dtFinalAnt, out dtInfoDiaAnt, out dec1, out dec2, out dec3);
                TraerInfo(fechaIni, fechaFin, out dtFinal, out dtInfoDia, out dec1, out dec2, out dec3);

                Diferencia(dtFinalAnt, dtFinal);
                Diferencia(dtInfoDiaAnt, dtInfoDia);                              

                string[] meses = new string[] { "ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO", "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE" };
                ReportParameter[] parameters = new ReportParameter[5];
                parameters[0] = new ReportParameter("EMPRESA", emp_codigo, true);
                parameters[1] = new ReportParameter("MES", meses[fechaFin.Month - 1] + " " + fechaIni.Year.ToString(), true);
                parameters[2] = new ReportParameter("DEC_UNO", dec1.ToString(), true);
                parameters[3] = new ReportParameter("DEC_DOS", dec2.ToString(), true);
                parameters[4] = new ReportParameter("DEC_TRES", dec3.ToString(), true);

                reportViewer1.LocalReport.DataSources.Clear();
                ReportDataSource sour = new ReportDataSource("DataSet1", dtFinal);
                reportViewer1.LocalReport.DataSources.Add(sour);
                sour = new ReportDataSource("DataSet2", dtInfoDia);
                reportViewer1.LocalReport.DataSources.Add(sour);
                reportViewer1.LocalReport.SetParameters(parameters);
                reportViewer1.LocalReport.Refresh();
                reportViewer1.RefreshReport();

                byte[] Bytes = reportViewer1.LocalReport.Render(format: "PDF", deviceInfo: "");
                try
                {
                    ruta = ruta += origen ? "\\DIA EMPRESA.pdf" : "\\Dia_Empresa.pdf";
                    using (FileStream stream = new FileStream(ruta, FileMode.Create))
                    {
                        stream.Write(Bytes, 0, Bytes.Length);
                    }

                    if (!origen)
                        Process.Start(ruta);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }

                Cursor = Cursors.Default;
            }
            catch (IOException ex)
            {
                if (!origen)
                    MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
            }
            catch (DbException ex)
            {
                if (!origen)
                    MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
            }
            catch (Exception ex)
            {
                if (!origen)
                    MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
            }
        }


        private void ColumnasIndicadores(out DataTable dt)
        {
            dt = new DataTable();
            dt.Columns.Add("Dia").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("Animales").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("media").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("ilcavta").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("icventa").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("eaprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("ilcaprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("icprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("preclprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("mhprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("porcmsprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("msprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("saprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("mssprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("easprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("precprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("precmsprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("lecheprod").DataType = System.Type.GetType("System.Double");
        }

        private void ColumnasDt(out DataTable dt)
        {
            dt = new DataTable();
            dt.Columns.Add("ingrediente").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("precioIng").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("xvaca").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("porcvaca").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("TOTAL").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("COSTO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("porccosto").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIO").DataType = System.Type.GetType("System.Double");
        }

        private DataTable RacionCosto(string etapa, string campo, DateTime inicio, DateTime fin)
        {
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("totalR").DataType = System.Type.GetType("System.Double");
            dt1.Columns.Add("costoT").DataType = System.Type.GetType("System.Double");
            DataTable dt;
            int vacas = Animales(campo, fin);
            string query = "SELECT X.Clave, X.Ing, SUM(X.PesoH*X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoH) "
                            + " FROM( "
                            + " SELECT R.Ran, R.Clave, R.Ing, ISNULL(i.ing_precio_sie, 0) AS Precio, ISNULL(it.ingt_porcentaje_ms, 0) AS PMS, SUM(R.Peso)  AS PesoH "
                            + " FROM( "
                            + " SELECT ran_id AS Ran, r.ing_clave AS Clave, r.ing_descripcion AS Ing, SUM(rac_mh) AS Peso "
                            + " FROM racion r "
                            + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'"
                            + " AND etp_id IN(" + etapa + ")  AND SUBSTRING(ing_clave, 1, 4) IN('ALAS', 'ALFO') "
                            + " GROUP BY ran_id, ing_clave, ing_descripcion "
                            + " UNION "
                            + " SELECT T.Ran, T.Clave, T.Ing, SUM(T.Peso) "
                            + " FROM( "
                            + " SELECT T1.Ran, IIF(T2.Pmez IS NULL, T1.Clave, T2.Clave) AS Clave, IIF(T2.Pmez IS NULL, T1.Ing, T2.Ing) AS Ing, IIF(T2.Pmez IS NULL, T1.Peso, T1.Peso * T2.Porc) AS Peso "
                            + " FROM( "
                            + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso "
                            + " FROM( "
                            + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'"
                            + "  AND etp_id IN(" + etapa + ")  AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 "
                            + " AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                            + " GROUP BY ran_id, ing_descripcion) T1 "
                            + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
                            + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
                            + " GROUP BY T.Ran, T.Clave, T.Ing "
                            + " UNION "
                            + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh) "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ")  AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F', 'W') "
                            + " AND SUBSTRING(ing_descripcion, 3, 2) NOT IN('00', '01', '02')  AND etp_id IN(" + etapa + ") "
                            + " GROUP BY ran_id, ing_clave, ing_descripcion "
                            + " UNION "
                            + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh) "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ")  AND ing_descripcion IN('Agua', 'Water')  "
                            + " AND etp_id IN(" + etapa + ") "
                            + " GROUP BY ran_id, ing_clave, ing_descripcion) R "
                            + " LEFT JOIN ingrediente i ON i.ing_clave = R.Clave AND i.ing_descripcion = R.Ing AND i.ran_id = R.Ran "
                            + " LEFT JOIN ingrediente_tracker it ON it.ingt_clave = R.Clave AND R.Ing = it.ingt_descripcion AND R.Ran = it.ran_id "
                            + " GROUP BY R.Ran, R.Clave, R.Ing, i.ing_precio_sie, it.ingt_porcentaje_ms) X "
                            + " WHERE X.PesoH > 0 " 
                            + " GROUP BY X.Clave, X.Ing";
            conn.QueryAlimento(query, out dt);

            double totalRacion = 0, costoTotal = 0, xvaca;
            for(int i = 0; i < dt.Rows.Count; i++)
            {
                xvaca = Convert.ToDouble(dt.Rows[i][3]) * 1.0 / vacas;
                totalRacion += Convert.ToDouble(dt.Rows[i][3]);
                costoTotal += Convert.ToDouble(dt.Rows[i][2]) * xvaca;
            }
            DataRow dr = dt1.NewRow();
            dr[0] = totalRacion;
            dr[1] = costoTotal;
            dt1.Rows.Add(dr);
            return dt1;
        }

        private int Animales(string campo, DateTime fecha)
        {
            DataTable dt;
            int animales = 0;
            string query = "SELECT SUM(" + campo + ") AS Vacas FROM inventario_afi WHERE ran_id IN( " + ranNumero + ") AND ia_fecha = '" + fecha.ToString("yyyy-MM-dd") + "' ";
            conn.QueryAlimento(query, out dt);
            if (dt.Rows.Count > 0)
                Int32.TryParse(dt.Rows[0][0].ToString(), out animales);

            return animales;
        }
        private DataTable MediaLecheF(DateTime inicio, DateTime fin)
        {
            DataTable dt;
            string query = "SELECT IIF(SUM(ia.ia_vacas_ord)>0,ISNULL((SUM(m.med_produc)/SUM(ia.ia_vacas_ord)),0),0) , ISNULL(SUM(m.med_lecfederal + m.med_lecplanta) / COUNT(DISTINCT med_fecha),0) "
                    + " FROM media m LEFT JOIN inventario_afi ia ON ia.ran_id = m.ran_id AND ia.ia_fecha = m.med_fecha "
                    + " where m.ran_id IN(" + ranNumero + ") AND med_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' ";
            conn.QueryAlimento(query, out dt);

            return dt;
        }

        private Double PrecioL()
        {
            DataTable dt;
            string query = "SELECT AVG(T2.Precio) FROM( SELECT  ran_id AS Rancho, MAX(hl_fecha_reg) AS Fecha "
                               + " FROM historico_leche where ran_id IN(" + ranNumero + ") GROUP BY ran_id) T1 "
                               + " LEFT JOIN( "
                               + " SELECT ran_id AS Rancho, hl_fecha_reg AS Fecha, hl_precio AS Precio "
                               + " FROM historico_leche "
                               + " WHERE ran_id IN(" + ranNumero + "))T2 ON T1.Fecha = T2.Fecha AND T1.Rancho = T2.Rancho";
            conn.QueryAlimento(query, out dt);

            return Convert.ToDouble(dt.Rows[0][0]);
                
        }

        private string Sobrantes()
        {
            DataTable dt;
            string sobrantes = "";
            string query = "";
            query = "SELECT description FROM ds_ingredient WHERE is_active = 1 AND is_deleted = 0 AND substring(description from 1 for 1) not in ('A','F','W') "
                    + "  AND SUBSTRING(description from 3 for 2) not in('00','01','02','90') ";
            conn.QueryTracker(query, out dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sobrantes += "'" + dt.Rows[i][0].ToString() + "',";
            }

            sobrantes = sobrantes.Length > 0 ? sobrantes.Substring(0, sobrantes.Length - 1) : "''";
            return sobrantes;
        }


        private double PMS(string etapa, DateTime inicio, DateTime fin)
        {
            double v = 0;
            DataTable dt;
            string sobrante = Sobrantes();
            string query = "SELECT SUM(R.PesoS) / SUM(R.PesoH) * 100 "
                    + " FROM( "
                    + "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
                    + " FROM(  "
                    + " SELECT R.Clave, R.Ing, ISNULL(i.ing_precio_sie, 0) AS Precio, ISNULL(SUM(R.PesoS) / SUM(R.PesoH), 0) AS PMS, SUM(R.PesoH) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoH, "
                    + " SUM(R.PesoS) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoS "
                    + "  FROM( "
                    + " SELECT ran_id AS Ran, r.ing_clave AS Clave, r.ing_descripcion AS Ing, SUM(rac_mh) AS PesoH, SUM(rac_ms) AS PesoS "
                    + " FROM racion r "
                    + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND etp_id IN(" + etapa + ")  AND SUBSTRING(ing_clave, 1, 4) IN('ALAS', 'ALFO') "
                    + " GROUP BY ran_id, ing_clave, ing_descripcion "
                    + " UNION "
                    + " SELECT T.Ran, T.Clave, T.Ing, SUM(T.Peso), SUM(T.PesoS) "
                    + " FROM( "
                    + " SELECT T1.Ran, IIF(T2.Pmez IS NULL, T1.Clave, T2.Clave) AS Clave, IIF(T2.Pmez IS NULL, T1.Ing, T2.Ing) AS Ing, IIF(T2.Pmez IS NULL, T1.Peso, T1.Peso * T2.Porc) AS Peso, "
                    + " IIF(T2.Pmez IS NULL, T1.PesoS, T1.PesoS * T2.Porc) AS PesoS "
                    + " FROM( "
                    + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, (T1.PesoS * T2.Porc) AS PesoS "
                    + " FROM( "
                    + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, SUM(rac_ms) AS PesoS "
                    + " FROM racion "
                    + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
                    + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                    + " GROUP BY ran_id, ing_descripcion "
                    + " ) T1 "
                    + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
                    + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
                    + " GROUP BY T.Ran, T.Clave, T.Ing "
                    + " UNION "
                    + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh), SUM(rac_ms) "
                    + " FROM racion "
                    + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ")  AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F', 'W') "
                    + " AND SUBSTRING(ing_descripcion, 3, 2) NOT IN('00', '01', '02')  AND etp_id IN(" + etapa + ") "
                     + " GROUP BY ran_id, ing_clave, ing_descripcion "
                    + " UNION "
                    + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh), SUM(rac_ms) "
                    + " FROM racion "
                    + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ")  AND ing_descripcion IN('Agua', 'Water')  AND etp_id IN(" + etapa + ") "
                    + " GROUP BY ran_id, ing_clave, ing_descripcion "
                    + " ) R "
                    + " LEFT JOIN ingrediente i ON i.ing_clave = R.Clave AND i.ing_descripcion = R.Ing AND i.ran_id = R.Ran "
                    + " LEFT JOIN ingrediente_tracker it ON it.ingt_clave = R.Clave AND R.Ing = it.ingt_descripcion AND R.Ran = it.ran_id "
                    + " GROUP BY R.Ran, R.Clave, R.Ing, i.ing_precio_sie, it.ingt_porcentaje_ms ) X "
                    + " WHERE X.PesoH > 0 AND X.Ing NOT IN(" + sobrante + ") GROUP BY x.Ing"
            + " ) R";
            conn.QueryAlimento(query, out dt);
            v = dt.Rows.Count > 0 ?  dt.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dt.Rows[0][0]) : 0 : 0;
            return v;

        }

        private Double Sobrante(string etp, DateTime inicio, DateTime fin)
        {
            double sob = 0;
            DataTable dt;
            string query = "SELECT ISNULL(SUM(rac_mh)/ DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "'),0) AS Peso "
                            + " FROM racion where rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND rac_descripcion like '%SOB%' "
                            + " AND ing_descripcion like '" + etp + "%' AND ran_id IN(" + ranNumero + ") AND SUBSTRING(ing_descripcion,3,2) NOT IN('00','01','02')";
            conn.QueryAlimento(query, out dt);

            if (dt.Rows.Count > 0)
                Double.TryParse(dt.Rows[0][0].ToString(), out sob);
            return sob;
        }

        private void TraerInfo(DateTime fechaIni, DateTime fechaFin, out DataTable dtFinal, out DataTable dtInfoDia, out double dec1, out double dec2, out double dec3)
        {
            dec1 = 0; dec2 = 0; dec3 = 0;
            int numAnimales = 0;
            double media, pmsP;
            double precioLeche, lecheFederal, costoT, precioT, totalRacion, lecheprod, mh, sobrante;
            string query;
            string etapa = "10,11,12,13";
            DateTime tempIni, tempFin, fechaI,fechaF;
            int hcorte = 0;
            int horas;
            Hora_Corte(out horas, out hcorte);
            DataTable dtIndicadores, dtV;
            ColumnasIndicadores(out dtIndicadores);

            for (int i = 0; i < dias; i++)
            {
                media = 0; pmsP = 0;
                precioLeche = 0; lecheFederal = 0; costoT = 0; precioT = 0; totalRacion = 0; lecheprod = 0;
                if ((hcorte == 24 || hcorte == 0 ) && i == 30)
                    continue;

                tempIni = fechaIni.AddDays(i);
                tempFin = tempIni.AddDays(1);

                fechaI = hcorte == 0 || hcorte == 24 ? tempIni : tempFin;
                

                DataTable dtPremezclas = new DataTable();
                query = "select DISTINCT ing_descripcion FROM racion "
                    + " WHERE rac_fecha BETWEEN '" + tempIni.ToString("yyyy-MM-dd HH:mm") + "' AND '" + tempFin.ToString("yyyy-MM-dd HH:mm") + "' "
                    + " AND ran_id IN(" + ranNumero.ToString() + ") AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02') "
                    + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F') AND etp_id IN( " + etapa + ")";
                conn.QueryAlimento(query, out dtPremezclas);

                conn.DeleteAlimento("porcentaje_Premezcla", "");
                DataTable dtt;
                for (int j = 0; j < dtPremezclas.Rows.Count; j++)
                {
                    query = "SELECT TOP(5) * FROM premezcla where pmez_racion like '" + dtPremezclas.Rows[j][0].ToString() + "' AND pmez_fecha <= '" + tempFin.ToString("yyyy-MM-dd HH:mm") + "'";
                    conn.QueryAlimento(query, out dtt);

                    if (dtt.Rows.Count == 0)
                        continue;                    

                    CargarPremezcla(dtPremezclas.Rows[j][0].ToString(), tempIni, tempFin);
                }

               // DataTable dt2 = RacionCosto("10,11,12,13", "ia_vacas_ord", tempIni, tempFin);
                costoT = Costo("10,11,12,13", "ia_vacas_ord", tempIni, tempFin);
                DataTable dTTR;
                TotalRacion("10,11,12,13", tempIni, tempFin,out dTTR);
                if (dTTR.Rows.Count > 0)
                    if (dTTR.Rows[0][0] != DBNull.Value)
                        totalRacion = Convert.ToDouble(dTTR.Rows[0][0]);

                //if (dt2.Rows.Count > 0)
                //{
                //    Double.TryParse(dt2.Rows[0][0].ToString(), out totalRacion);
                //    Double.TryParse(dt2.Rows[0][1].ToString(), out costoT);
                //}
                numAnimales = Animales("ia_vacas_ord", fechaI);
                DataTable dtML = MediaLecheF(fechaI,fechaI);
                if (dtML.Rows.Count > 0)
                {
                    Double.TryParse(dtML.Rows[0][0].ToString(), out media);
                    Double.TryParse(dtML.Rows[0][1].ToString(), out lecheFederal);
                }
                precioLeche = PrecioL();
                pmsP = PMS("10,11,12,13", tempIni, tempFin);
                mh = numAnimales > 0 ? totalRacion / numAnimales : 0;
                sobrante = Sobrante("1", tempIni.AddDays(1), tempFin.AddDays(1));


                //Indicadores                
                DataRow drIndicadores = dtIndicadores.NewRow();
                drIndicadores["Dia"] = i + 1;   //0
                drIndicadores["Animales"] = numAnimales;
                drIndicadores["media"] = media;
                drIndicadores["ilcavta"] = numAnimales > 0 && costoT > 0 ? (lecheFederal / numAnimales * precioLeche / costoT) : 0;
                drIndicadores["icventa"] = numAnimales > 0 ? (lecheFederal / numAnimales * precioLeche) - costoT : 0; //4
                drIndicadores["eaprod"] = numAnimales > 0 && pmsP > 0? media / (pmsP * (totalRacion / numAnimales) / 100) : 0;
                drIndicadores["ilcaprod"] = media > 0 && costoT > 0 ? precioLeche * media / costoT : 0;
                drIndicadores["icprod"] = media > 0 ? (precioLeche * media) - costoT : 0;
                drIndicadores["preclprod"] = media > 0 ? costoT / media : 0;
                drIndicadores["mhprod"] = numAnimales > 0 ? totalRacion / numAnimales : 0; //9
                drIndicadores["porcmsprod"] = pmsP;
                drIndicadores["msprod"] = numAnimales > 0 ? pmsP * (totalRacion / numAnimales) / 100 : 0;
                drIndicadores["saprod"] = numAnimales > 0 ? sobrante / numAnimales : 0;
                drIndicadores["mssprod"] = numAnimales > 0 ? ((totalRacion - sobrante) / numAnimales) * pmsP / 100 : 0;
                drIndicadores["easprod"] = numAnimales > 0 ? media / ((totalRacion - sobrante) / numAnimales * pmsP / 100) : 0; //14
                drIndicadores["precprod"] = costoT > 0 ? costoT : 0;
                drIndicadores["precmsprod"] = numAnimales > 0 ? costoT / (pmsP * (totalRacion / numAnimales) / 100) : 0;
                drIndicadores["lecheprod"] = lecheprod;
                dtIndicadores.Rows.Add(drIndicadores);
            }

            fechaI = hcorte == 24 || hcorte == 0 ? new DateTime(fechaFin.Year, dtp.Value.Date.Month, 1) : fechaIni.AddDays(1);
            fechaF = hcorte == 24 || hcorte == 0 ? new DateTime(fechaFin.Year, dtp.Value.Date.Month, dtp.Value.Date.Day) : fechaFin;
            DataTable dtIL;
            query = "SELECT DATEPART(DAY, T.FECHA) AS DIA, SUM(T.LECHE) AS TOTAL, SUM(T.ORDENO) AS ORDENO,  SUM(T.SECAS) AS SECAS, "
                + " SUM(T.HATO) AS HATO, IIF(SUM(T.LECHE)> 0, SUM(T.Lactosa) / SUM(T.LECHE),0) AS '%LACT', IIF(SUM(T.LECHE)> 0, SUM(T.Proteina) / SUM(T.LECHE),0) AS '%PROT',  "
                + " IIF(SUM(T.LECHE)> 0, SUM(T.Urea) / SUM(T.LECHE),0) AS UREA, IIF(SUM(T.LECHE)> 0, SUM(T.Grasa) / SUM(T.LECHE),0) AS '%GRA', SUM(T.CCS) / SUM(T.LECHE) AS CCS, "
                + " IIF(SUM(T.LECHE)> 0, SUM( T.CTD) / SUM(T.LECHE),0) AS CTD,SUM(T.LECHEPROD) AS LECHE , SUM(T.ANTIB) AS ANTIB, AVG(T.Media) AS Media, SUM(T.Total)  AS TOTAL, "
                + " IIF(SUM(T.LECFP)> 0, SUM(T.LECFP * T.DELORD) / SUM(T.LECFP),0) AS DEL, SUM(T.ANT) AS ANT "
                + " FROM( "
                + " SELECT inv.FECHA, inv.ORDENO, inv.SECAS, inv.HATO, (indl.Lactosa * med.LECHE) AS Lactosa, (indl.Proteina * med.LECHE) AS Proteina, "
                + " (indl.Urea * med.LECHE) AS Urea, (indl.Grasa * med.LECHE) AS Grasa, (indl.CCS * med.LECHE) AS CCS, (indl.CTD * med.LECHE) AS CTD, "
                + " med.LECHE AS LECHE, med.ANTIB AS ANTIB, med.Media AS Media, med.TOTAL AS Total, med.LECFP, med.DELORD, med.ANT, med.LECHEPROD "
                + " FROM( "
                + " SELECT ran_id AS Rancho, ia_fecha AS FECHA, SUM(ia_vacas_ord) AS ORDENO, SUM(ia_vacassecasl1 + ia_vacassecasl2 + ia_vacassecasl3 + ia_vacassecasl4) AS SECAS, "
                + " SUM(ia_vacas_ord + ia_vacassecasl1 + ia_vacassecasl2 + ia_vacassecasl3 + ia_vacassecasl4) AS HATO "
                + " from inventario_afi "
                + " where ran_id IN(" + ranNumero + ") AND ia_fecha BETWEEN '" + fechaI.ToString("yyyy-MM-dd") + "' AND '" + fechaF.ToString("yyyy-MM-dd") + "' "
                + " GROUP BY ia_fecha, ran_id ) inv "
                + " LEFT JOIN( "
                + " SELECT ran_id AS Rancho, il_fecha AS Fecha, il_lactosa AS Lactosa, il_proteina AS Proteina, il_urea AS Urea, il_grasa AS Grasa, il_ccs AS CCS, il_ctd AS CTD "
                + " FROM indicadores_leche "
                + " WHERE ran_id IN(" + ranNumero + ") AND il_fecha BETWEEN '" + fechaI.ToString("yyyy-MM-dd") + "' AND '" + fechaF.ToString("yyyy-MM-dd") + "' "
                + " )indl ON inv.FECHA = indl.Fecha AND inv.Rancho = indl.Rancho "
                + " LEFT JOIN( "
                + " SELECT ran_id AS Rancho, med_fecha AS Fecha, SUM(med_lecfederal + med_lecplanta) AS LECHE, SUM(med_antib) AS ANTIB, "
                + " SUM(med_media) AS Media, SUM(med_produc) AS TOTAL, SUM(med_vcantib) AS ANT, "
                + " SUM(med_lecfederal + med_lecplanta) AS LECFP, SUM(med_delord) AS DELORD, SUM(med_lecproduc) AS LECHEPROD "
                + " FROM media "
                + " WHERE ran_id IN(" + ranNumero + ") AND med_fecha BETWEEN '" + fechaI.ToString("yyyy-MM-dd") + "' AND '" + fechaF.ToString("yyyy-MM-dd") + "' "
                + " GROUP BY med_fecha, ran_id "
                + " )med ON med.Fecha = inv.FECHA AND med.Rancho = inv.Rancho ) T "
                + " GROUP BY T.FECHA";
            conn.QueryAlimento(query, out dtIL);

            if(dias == 31 && dtIndicadores.Rows.Count < 31)
            {
                int dif = dias - dtIndicadores.Rows.Count;
                for(int i = 0; i < dif; i++)
                {
                    DataRow row = dtIndicadores.NewRow();
                    row[0] = 31;
                    for(int j = 1; j < dtIndicadores.Columns.Count; j++)
                    {
                        row[j] = 0;
                    }
                    dtIndicadores.Rows.Add(row);
                }

            }
            //if(dias == 31 && dtIL.Rows.Count < 31)
            //{
            //    int dif = dias - dtIL.Rows.Count;
            //    for (int i = 0; i < dif; i++)
            //    {
            //        DataRow row = dtIL.NewRow();
            //        row[0] = dif == 31 ? i : 31;
            //        for (int j = 1; j < dtIL.Columns.Count; j++)
            //        {
            //            row[j] = 0;
            //        }
            //        dtIL.Rows.Add(row);
            //    }
            //}
            AddRow(dias, dtIL);
            FillDTFinal(dias, dtIndicadores, dtIL, out dtFinal);
            DataTable dtJaulas, dtDest1, dtDest2, dtVp, dtSecas, dtReto, dtUti;
            RacionEtapas("31", fechaIni, fechaFin, out dtJaulas);
            RacionEtapas("32", fechaIni, fechaFin, out dtDest1);
            RacionEtapas("33", fechaIni, fechaFin, out dtDest2);
            RacionEtapas("34", fechaIni, fechaFin, out dtVp);
            RacionSR("21", fechaIni, fechaFin, out dtSecas);
            RacionSR("22", fechaIni, fechaFin, out dtReto);
            Utilidad(fechaI, fechaF, out dtUti);

            AddRow(dias, dtJaulas);
            AddRow(dias, dtDest1);
            AddRow(dias, dtDest2);
            AddRow(dias, dtVp);
            AddRow(dias, dtSecas);
            AddRow(dias, dtReto);
            AddRow(dias, dtUti);
            AddColumnsEtapas(out dtInfoDia);

            RemoveRow(dias, dtJaulas);
            RemoveRow(dias, dtDest1);
            RemoveRow(dias, dtDest2);
            RemoveRow(dias, dtVp);
            RemoveRow(dias, dtSecas);
            RemoveRow(dias, dtReto);
            RemoveRow(dias, dtUti);


            if (dtDest1.Rows.Count == dtDest2.Rows.Count && dtDest1.Rows.Count == dtVp.Rows.Count && dtDest1.Rows.Count == dtSecas.Rows.Count && dtReto.Rows.Count == dtDest1.Rows.Count && dtUti.Rows.Count == dtDest1.Rows.Count)
            {
                for (int i = 0; i < dtDest1.Rows.Count; i++)
                {
                    DataRow dr = dtInfoDia.NewRow();
                    dr["DIA"] = (i + 1).ToString();
                    //Jaulas
                    dr["INV"] = Convert.ToInt32(dtJaulas.Rows[i][1]);
                    dr["PRECIOJ"] = Convert.ToDouble(dtJaulas.Rows[i][3]);
                    //  Destetadas1 (2/7)
                    dr["INV2"] = Convert.ToInt32(dtDest1.Rows[i][1]);
                    dr["MH2"] = Convert.ToDouble(dtDest1.Rows[i][2]);
                    dr["PRECIO2"] = Convert.ToDouble(dtDest1.Rows[i][3]);
                    dr["PORCMS2"] = Convert.ToDouble(dtDest1.Rows[i][4]);
                    dr["MS2"] = Convert.ToDouble(dtDest1.Rows[i][5]);
                    dr["PRECIOMS2"] = Convert.ToDouble(dtDest1.Rows[i][6]);
                    // Destetadas2 (7/13)
                    dr["INV7"] = Convert.ToInt32(dtDest2.Rows[i][1]);
                    dr["MH7"] = Convert.ToDouble(dtDest2.Rows[i][2]);
                    dr["PRECIO7"] = Convert.ToDouble(dtDest2.Rows[i][3]);
                    dr["PORCMS7"] = Convert.ToDouble(dtDest2.Rows[i][4]);
                    dr["MS7"] = Convert.ToDouble(dtDest2.Rows[i][5]);
                    dr["PRECIOMS7"] = Convert.ToDouble(dtDest2.Rows[i][6]);
                    // Vaquillas (13 a mas)
                    dr["INV13"] = Convert.ToInt32(dtVp.Rows[i][1]);
                    dr["MH13"] = Convert.ToDouble(dtVp.Rows[i][2]);
                    dr["PRECIO13"] = Convert.ToDouble(dtVp.Rows[i][3]);
                    dr["PORCMS13"] = Convert.ToDouble(dtVp.Rows[i][4]);
                    dr["MS13"] = Convert.ToDouble(dtVp.Rows[i][5]);
                    dr["PRECIOMS13"] = Convert.ToDouble(dtVp.Rows[i][6]);
                    //Secas
                    dr["INVSECAS"] = Convert.ToInt32(dtSecas.Rows[i][1]);
                    dr["MHSECAS"] = Convert.ToDouble(dtSecas.Rows[i][2]);
                    dr["PORCMSSECAS"] = Convert.ToDouble(dtSecas.Rows[i][3]);
                    dr["MSSECAS"] = Convert.ToDouble(dtSecas.Rows[i][4]);
                    dr["SASECAS"] = Convert.ToDouble(dtSecas.Rows[i][5]);
                    dr["MSSSECAS"] = Convert.ToDouble(dtSecas.Rows[i][6]);
                    dr["PORCSSECAS"] = Convert.ToDouble(dtSecas.Rows[i][7]);
                    dr["PRECIOSECAS"] = Convert.ToDouble(dtSecas.Rows[i][8]);
                    dr["PRECIOMSSECAS"] = Convert.ToDouble(dtSecas.Rows[i][9]);
                    ////reto
                    dr["INVRETO"] = Convert.ToInt32(dtReto.Rows[i][1]);
                    dr["MHRETO"] = Convert.ToDouble(dtReto.Rows[i][2]);
                    dr["PORCMSRETO"] = Convert.ToDouble(dtReto.Rows[i][3]);
                    dr["MSRETO"] = Convert.ToDouble(dtReto.Rows[i][4]);
                    dr["SARETO"] = Convert.ToDouble(dtReto.Rows[i][5]);
                    dr["MSSRETO"] = Convert.ToDouble(dtReto.Rows[i][6]);
                    dr["PORCSRETO"] = Convert.ToDouble(dtReto.Rows[i][7]);
                    dr["PRECIORETO"] = Convert.ToDouble(dtReto.Rows[i][8]);
                    dr["PRECIOMSRETO"] = Convert.ToDouble(dtReto.Rows[i][9]);
                    //Utilidad por animal
                    dr["IXA"] = Convert.ToDouble(dtUti.Rows[i][1]);
                    dr["CXA"] = 0;
                    dr["PORCENTAJE1"] = 0;
                    dr["IT"] = Convert.ToDouble(dtUti.Rows[i][2]);
                    dr["UXA"] = 0;
                    dr["PORCENTAJE2"] = 0;
                    dtInfoDia.Rows.Add(dr);
                }

                for (int i = 0; i < dtInfoDia.Rows.Count; i++)
                {
                    double jaulas, dest1, dest2, vp, secas, reto, it, cxa, ixa, uxa, p1, p2, produc;
                    jaulas = Convert.ToDouble(dtInfoDia.Rows[i]["INV"]) * Convert.ToDouble(dtInfoDia.Rows[i]["PRECIOJ"]);
                    dest1 = Convert.ToDouble(dtInfoDia.Rows[i]["INV2"]) * Convert.ToDouble(dtInfoDia.Rows[i]["PRECIO2"]);
                    dest2 = Convert.ToDouble(dtInfoDia.Rows[i]["INV7"]) * Convert.ToDouble(dtInfoDia.Rows[i]["PRECIO7"]);
                    vp = Convert.ToDouble(dtInfoDia.Rows[i]["INV13"]) * Convert.ToDouble(dtInfoDia.Rows[i]["PRECIO13"]);
                    secas = Convert.ToDouble(dtInfoDia.Rows[i]["INVSECAS"]) * Convert.ToDouble(dtInfoDia.Rows[i]["PRECIOSECAS"]);
                    reto = Convert.ToDouble(dtInfoDia.Rows[i]["INVRETO"]) * Convert.ToDouble(dtInfoDia.Rows[i]["PRECIORETO"]);
                    produc = Convert.ToDouble(dtFinal.Rows[i]["ORDENO"]) * Convert.ToDouble(dtFinal.Rows[i]["PRECIOPROD"]);
                    it = Convert.ToDouble(dtInfoDia.Rows[i]["IT"]);
                    ixa = Convert.ToDouble(dtInfoDia.Rows[i]["IXA"]);
                    cxa = it > 0 ? (jaulas + dest1 + dest2 + vp + secas + reto + produc) / it : it;
                    p1 = ixa > 0 ? cxa / ixa * 100 : 0;
                    uxa = ixa - cxa;
                    p2 = ixa > 0 ? uxa / ixa * 100 : 0;
                    dtInfoDia.Rows[i]["CXA"] = cxa;
                    dtInfoDia.Rows[i]["PORCENTAJE1"] = p1;
                    dtInfoDia.Rows[i]["UXA"] = uxa;
                    dtInfoDia.Rows[i]["PORCENTAJE2"] = p2;
                }

                DataTable dtMetas;
                query = "select DATEPART(DAY,pt_fecha) AS Fecha, AVG(pt_produccion) AS Produccion, AVG(pt_reto) AS Reto, AVG(pt_secas) AS Secas, AVG(pt_prenadas) AS Prenadas, "
                       + " AVG(pt_becerras2) AS Becerras2, AVG(pt_becerras1) AS Becerras1, AVG(pt_ms) AS MS "
                       + " from preciosteoricos "
                       + " WHERE ran_id IN(" + ranNumero + ") AND pt_fecha BETWEEN '" + fechaIni.AddDays(1).ToString("yyyy-MM-dd") + "' AND  '" + fechaFin.ToString("yyyy-MM-dd") + "' "
                       + " GROUP BY pt_fecha";
                conn.QueryAlimento(query, out dtMetas);

                for (int i1 = 0; i1 < dtFinal.Rows.Count; i1++)
                {
                    for (int i2 = 0; i2 < dtMetas.Rows.Count; i2++)
                    {
                        if (Convert.ToInt32(dtFinal.Rows[i1]["DIA"]) != Convert.ToInt32(dtMetas.Rows[i2][0]))
                            continue;
                        else
                        {
                            dtFinal.Rows[i1]["METAPROD"] = Convert.ToDouble(dtMetas.Rows[i2][1]);
                            dtFinal.Rows[i1]["METAMS"] = Convert.ToDouble(dtMetas.Rows[i2][7]);
                        }
                    }
                }

                for (int i1 = 0; i1 < dtInfoDia.Rows.Count; i1++)
                {
                    for (int i2 = 0; i2 < dtMetas.Rows.Count; i2++)
                    {
                        if (Convert.ToInt32(dtInfoDia.Rows[i1][0]) != Convert.ToInt32(dtMetas.Rows[i2][0]))
                            continue;
                        else
                        {
                            dtInfoDia.Rows[i1]["METABEC1"] = Convert.ToDouble(dtMetas.Rows[i2][6]);
                            dtInfoDia.Rows[i1]["METABEC2"] = Convert.ToDouble(dtMetas.Rows[i2][5]);
                            dtInfoDia.Rows[i1]["METAVP"] = Convert.ToDouble(dtMetas.Rows[i2][4]);
                            dtInfoDia.Rows[i1]["METASECAS"] = Convert.ToDouble(dtMetas.Rows[i2][3]);
                            dtInfoDia.Rows[i1]["METARETO"] = Convert.ToDouble(dtMetas.Rows[i2][2]);
                        }
                    }
                }

                //double dec1 = 0, t1 = 0, dec2 = 0, t2 = 0, dec3 = 0, t3 = 0;
                double t1 = 0, t2 = 0, t3 = 0;
                int c1 = 0, c2 = 0, c3 = 0, rango = 0;

                if (dtFinal.Rows.Count > 0)
                {
                    rango = dtFinal.Rows.Count > 10 ? 10 : dtFinal.Rows.Count;
                    for (int i = 0; i < rango; i++)
                    {
                        c1 += 1;
                        t1 += Convert.ToDouble(dtFinal.Rows[i]["PORCGRA"]);
                    }
                    dec1 = t1 / c1;
                }

                if (dtFinal.Rows.Count > 10)
                {
                    rango = dtFinal.Rows.Count > 20 ? 20 : dtFinal.Rows.Count;
                    for (int i = 10; i < rango; i++)
                    {
                        c2 += 1;
                        t2 += Convert.ToDouble(dtFinal.Rows[i]["PORCGRA"]);
                    }
                    dec2 = t2 / c2;
                }

                if (dtFinal.Rows.Count > 20)
                {
                    rango = dtFinal.Rows.Count;
                    for (int i = 20; i < rango; i++)
                    {
                        c3 += 1;
                        t3 += Convert.ToDouble(dtFinal.Rows[i]["PORCGRA"]);
                    }
                    dec3 = t3 / c3;
                }

                for (int i = dtFinal.Rows.Count; i < 31; i++)
                {
                    DataRow dr = dtFinal.NewRow();
                    dr["DIA"] = "NA";
                    dtFinal.Rows.Add(dr);
                }

                for (int i = dtInfoDia.Rows.Count; i < 31; i++)
                {
                    DataRow dr = dtInfoDia.NewRow();
                    dr["DIA"] = "NA";
                    dtInfoDia.Rows.Add(dr);
                }

                double v;
                for (int i = 0; i < dtFinal.Rows.Count; i++)
                {
                    for (int j = 1; j < dtFinal.Columns.Count; j++)
                    {
                        if (dtFinal.Rows[i][j] != DBNull.Value)
                        {
                            v = Convert.ToDouble(dtFinal.Rows[i][j]);
                            if (v == 0)
                                dtFinal.Rows[i][j] = DBNull.Value;

                            if(j == 7)
                            {
                                if (v <= 0)
                                    dtFinal.Rows[i][j] = DBNull.Value;
                            }
                        }
                    }
                }

                for (int i = 0; i < dtInfoDia.Rows.Count; i++)
                {
                    for (int j = 1; j < dtInfoDia.Columns.Count; j++)
                    {
                        if (dtInfoDia.Rows[i][j] != DBNull.Value)
                        {
                            v = Convert.ToDouble(dtInfoDia.Rows[i][j]);
                            if (v <= 0)
                                dtInfoDia.Rows[i][j] = DBNull.Value;
                        }
                    }
                }

                //PromediosProd(dtFinal);
                Sum_Prom(dtFinal, dec1, dec2, dec3);
                Sum_Prom(dtInfoDia, 0, 0, 0);

                Console.WriteLine(dec1.ToString());
                Console.WriteLine(dec2.ToString());
                Console.WriteLine(dec3.ToString());
            }
        }        

        private void RemoveRow(int dias, DataTable dt)
        {
            if(dias < dt.Rows.Count)
            {
                dt.Rows.RemoveAt(dt.Rows.Count-1);
            }
        }

        private void Diferencia(DataTable dtFinalAnt, DataTable dtFinal)
        {
            if (dtFinalAnt.Rows.Count  == dtFinal.Rows.Count)
            {
                foreach (DataRow dr in dtFinalAnt.Rows)
                {
                    if (dr["DIA"].ToString() == "PROM")
                    {
                        dr["DIA"] = "AÑO ANT";
                        dtFinal.Rows.Add(dr.ItemArray);
                    }
                }

                DataRow rowP = dtFinal.NewRow();
                rowP[0] = "DIF %";
                DataRow rowN = dtFinal.NewRow();
                rowN[0] = "DIF #";
                int ultimo, penultimo, cont = 0;
                string dia;

                for (int i = 0; i < dtFinal.Rows.Count; i++)
                {
                    cont = i;
                    dia = dtFinal.Rows[i][0].ToString();
                    if (dia == "NA")
                        break;
                }

                ultimo = cont == dtFinal.Rows.Count - 1 ? 33: cont - 1;
                penultimo = cont == dtFinal.Rows.Count - 1 ? 32 : ultimo - 1;

                for (int i = 1; i < dtFinal.Columns.Count; i++)
                {
                    double act = dtFinal.Rows[penultimo][i].ToString() == "" ? 0 : Convert.ToDouble(dtFinal.Rows[penultimo][i]);
                    double ant = dtFinal.Rows[ultimo][i].ToString() == "" ? 0 : Convert.ToDouble(dtFinal.Rows[ultimo][i]);
                    double rest = act - ant;
                    rowN[i] = rest;
                    double porc = rest != 0  && act > 0 ? (rest / act) * 100 : 0;
                    rowP[i] = porc;
                }
                dtFinal.Rows.Add(rowP);
                dtFinal.Rows.Add(rowN);
            }
            else
            {
                DataRow row = dtFinal.NewRow();
                row["DIA"] = "X";
                dtFinal.Rows.Add(row);

                row = dtFinal.NewRow();
                row["DIA"] = "X";
                dtFinal.Rows.Add(row);

                row = dtFinal.NewRow();
                row["DIA"] = "X";
                dtFinal.Rows.Add(row);
            }
        }
       

        private void Hora_Corte(out int horas, out int hcorte)
        {
            DataTable dt;
            string query = "select paramvalue from bedrijf_params where name = 'DSTimeShift' ";
            conn.QueryTracker(query, out dt);

            horas = Convert.ToInt32(dt.Rows[0][0]);
            hcorte = 24 + horas;

        }

        private void SupraMezcla(string premezcla, DateTime inicio, DateTime fin)
        {
            DataTable dt;
            DateTime fini = new DateTime(), ffin;
            DataTable dtF = new DataTable();
            string query = "SELECT * FROM porcentaje_Premezcla where pmez_descripcion like '" + premezcla + "'";
            conn.QueryAlimento(query, out dt);
            int temp = 0;
            int repeticiones = 0;
            if (dt.Rows.Count == 0)
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

                query = "INSERT INTO porcentaje_Premezcla "
                    + " SELECT T1.Pmz, T1.Clave, T1.Ing, (T1.Peso / T2.Peso) "
                    + " FROM( "
                    + " SELECT rac_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, SUM(rac_mh) AS Peso "
                    + " FROM racion "
                    + " WHERE rac_descripcion like '" + premezcla + "' "
                    + " AND rac_fecha BETWEEN '" + fini.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                    + " GROUP BY rac_descripcion, ing_clave, ing_descripcion) T1 "
                    + " LEFT JOIN( "
                    + " SELECT rac_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                    + " FROM racion "
                    + " WHERE rac_descripcion like '" + premezcla + "' "
                    + " AND rac_fecha BETWEEN '" + fini.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                    + " GROUP BY rac_descripcion )T2 ON  T1.Pmz = T2.Pmz";
                conn.InsertSelecttAlimento(query);
            }
        }
        private void ColumnasDT(out DataTable dt)
        {
            dt = new DataTable();
            dt.Columns.Add("ingrediente").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("precioIng").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("xvaca").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("porcvaca").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("TOTAL").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("COSTO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("porccosto").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("s_precioIng").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("s_xvaca").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("s_porcvaca").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("s_TOTAL").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("s_COSTO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("s_porccosto").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("s_PRECIO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PMS").DataType = System.Type.GetType("System.Double");
        }

        private double Costo(string etapa, string campo, DateTime inicio, DateTime fin)
        {
            double v = 0;
            DataTable dt;
            ColumnasDT(out dt);
            int vacas = Animales(campo, fin);
            DataTable dt1;
            string query = "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
            + " FROM( "
            + " SELECT R.Clave, R.Ing, ISNULL(i.ing_precio_sie, 0) AS Precio, ISNULL(it.ingt_porcentaje_ms, 0) AS PMS, "
            + " SUM(R.Peso) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoH, "
            + " (SUM(R.Peso) * ISNULL(it.ingt_porcentaje_ms, 0) / 100) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoS "
            + " FROM( "
            + " SELECT ran_id AS Ran, r.ing_clave AS Clave, r.ing_descripcion AS Ing, SUM(rac_mh) AS Peso "
            + " FROM racion r "
            + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND etp_id IN(" + etapa + ") "
            + " AND SUBSTRING(ing_clave, 1, 4) IN('ALAS', 'ALFO') GROUP BY ran_id, ing_clave, ing_descripcion "
            + " UNION "
            + " SELECT T.Ran, T.Clave, T.Ing, SUM(T.Peso) "
            + " FROM( "
            + " SELECT T1.Ran, IIF(T2.Pmez IS NULL, T1.Clave, T2.Clave) AS Clave, IIF(T2.Pmez IS NULL, T1.Ing, T2.Ing) AS Ing, IIF(T2.Pmez IS NULL, T1.Peso, T1.Peso * T2.Porc) AS Peso "
            + " FROM( "
            + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso "
            + " FROM( "
            + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
            + " FROM racion "
            + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
            + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
            + " GROUP BY ran_id, ing_descripcion) T1 "
            + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
            + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
            + " GROUP BY T.Ran, T.Clave, T.Ing "
            + " UNION "
            + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh) "
            + " FROM racion "
            + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ")  AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F', 'W') "
            + " AND SUBSTRING(ing_descripcion, 3, 2) NOT IN('00', '01', '02')  AND etp_id IN(" + etapa + ") "
            + " GROUP BY ran_id, ing_clave, ing_descripcion "
            + " UNION "
            + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh) "
            + " FROM racion "
            + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ")  AND ing_descripcion IN('Agua', 'Water') "
            + " AND etp_id IN(" + etapa + ")  GROUP BY ran_id, ing_clave, ing_descripcion) R "
            + " LEFT JOIN ingrediente i ON i.ing_clave = R.Clave AND i.ing_descripcion = R.Ing AND i.ran_id = R.Ran "
            + " LEFT JOIN ingrediente_tracker it ON it.ingt_clave = R.Clave AND R.Ing = it.ingt_descripcion AND R.Ran = it.ran_id "
            + " GROUP BY R.Ran, R.Clave, R.Ing, i.ing_precio_sie, it.ingt_porcentaje_ms) X "
            + " WHERE X.PesoH > 0 GROUP BY x.Ing";
            conn.QueryAlimento(query, out dt1);

            DataTable dtTemp; ColumnasDT(out dtTemp);
            double xvaca, s_xvaca, totalR = 0, costoT = 0, txvaca = 0, tsxvaca = 0, costo;
            double mh, ms, pms, precio;

            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                precio = Convert.ToDouble(dt1.Rows[i][1]);
                pms = Convert.ToDouble(dt1.Rows[i][2]);
                mh = Convert.ToDouble(dt1.Rows[i][3]); totalR += mh;
                ms = Convert.ToDouble(dt1.Rows[i][4]);
                xvaca = vacas > 0 ?  mh / vacas : 0;
                s_xvaca = ms / vacas;
                txvaca += xvaca;
                tsxvaca += s_xvaca;
                costoT += precio * xvaca;
                DataRow dr = dtTemp.NewRow();
                dr["ingrediente"] = dt1.Rows[i][0].ToString();
                dr["precioIng"] = dt1.Rows[i][1].ToString();
                dr["xvaca"] = xvaca;
                dr["TOTAL"] = mh;
                dr["COSTO"] = precio * xvaca;
                dr["PRECIO"] = precio * mh;
                dr["s_precioIng"] = pms > 0 ? precio * 100 / pms : 0;
                dr["s_xvaca"] = s_xvaca;
                dr["s_TOTAL"] = ms;
                dr["s_COSTO"] = (pms > 0 ? precio * 100 / pms : 0) * s_xvaca;
                dr["s_PRECIO"] = (pms > 0 ? precio * 100 / pms : 0) * s_xvaca;
                dr["PMS"] = pms;
                dtTemp.Rows.Add(dr);
            }

            //for (int i = 0; i < dtTemp.Rows.Count; i++)
            //{
            //    xvaca = Convert.ToDouble(dtTemp.Rows[i]["xvaca"]);
            //    s_xvaca = Convert.ToDouble(dtTemp.Rows[i]["s_xvaca"]);
            //    mh = Convert.ToDouble(dtTemp.Rows[i]["TOTAL"]);
            //    costo = Convert.ToDouble(dtTemp.Rows[i]["COSTO"]);
            //    dtTemp.Rows[i]["porcvaca"] = xvaca / txvaca * 100;
            //    dtTemp.Rows[i]["porccosto"] = costo / costoT * 100;
            //    dtTemp.Rows[i]["s_porcvaca"] = s_xvaca / tsxvaca * 100;
            //    dtTemp.Rows[i]["s_porccosto"] = costo / costoT * 100;
            //}

            //string ing, ingA;

            for (int i = 0; i < dtTemp.Rows.Count; i++)
            {
                v += Convert.ToDouble(dtTemp.Rows[i]["COSTO"]);
            }
            return v;
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
                double porcentaje;
                prmz = premezcla[2].ToString() + premezcla[3];
                query = "SELECT * FROM porcentaje_Premezcla WHERE pmez_descripcion like '" + premezcla + "'";
                conn.QueryAlimento(query, out dtAux);
                int repeticiones=0;
                if (dtAux.Rows.Count == 0)
                {
                    if (prmz == "01")
                    {
                        query = "SELECT T1.Pmz, T1.Clave, T1.Ing, T1.Peso / T2.Total "
                                + " FROM( "
                                     + " select pmez_racion AS Pmz, ing_clave AS Clave, ing_nombre AS Ing, SUM(pmez_peso) AS Peso "
                                     + " from premezcla "
                                     + " where pmez_racion LIKE '" + premezcla + "' "
                                     + " AND pmez_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                                     + " GROUP BY pmez_racion, ing_clave, ing_nombre ) T1 "
                                     + " LEFT JOIN( "
                                     + " SELECT T.pmez_racion AS Pmz, SUM(T.pmez_peso) AS Total "
                                     + " FROM( "
                                     + " SELECT DISTINCT * "
                                        + " FROM premezcla "
                                        + " WHERE pmez_racion LIKE '" + premezcla + "' "
                                        + " AND pmez_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' ) T "
                                     + " GROUP BY T.pmez_racion) T2 ON T1.Pmz = T2.Pmz";
                        conn.QueryAlimento(query, out dt);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            pmz = dt.Rows[i][0].ToString();
                            clave = dt.Rows[i][1].ToString();
                            ingrediente = dt.Rows[i][2].ToString();
                            porcentaje = Convert.ToDouble(dt.Rows[i][3]);
                            valores += "('" + pmz + "','" + clave + "','" + ingrediente + "'," + porcentaje + "),";
                        }
                        if (valores.Length > 0)
                        {
                            valores = valores.Substring(0, valores.Length - 1);
                            conn.InsertMasivAlimento("porcentaje_Premezcla", valores);
                        }
                    }
                    else
                    {
                        query = "SELECT T1.Premezcla, T1. Fecha AS PMIng, ISNULL(T2. Fecha, T3.Fecha) AS PMRac "
                                   + " FROM( "
                                   + " SELECT ing_descripcion AS Premezcla, MIN(rac_fecha) AS Fecha "
                                   + " FROM racion "
                                   + " WHERE rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                                   + " AND ing_descripcion like '" + premezcla + "'"
                                   + " GROUP BY ing_descripcion)T1 "
                                   + " LEFT JOIN( "
                                   + " SELECT rac_descripcion AS Premezcla, MIN(rac_fecha)  AS Fecha "
                                   + " FROM racion "
                                   + " WHERE rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
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
                        fRacion = dt.Rows[0][2].ToString().Length > 0 ? Convert.ToDateTime(dt.Rows[0][2]) : inicio;
                        int comparacion = DateTime.Compare(fRacion, fIng);

                        if (comparacion == 1)
                        {
                            do
                            {
                                if (repeticiones == 30)
                                    break;
                                fpmI = inicio.AddDays(-1); fpmI = fpmI.AddDays(temp);
                                fpmF = fin2.AddDays(-1); fpmF = fpmF.AddDays(temp);

                                query = " SELECT * FROM premezcla WHERE pmez_racion like '" + premezcla + "' "
                                    + " AND pmez_fecha BETWEEN '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fpmF.ToString("yyyy-MM-dd HH:mm") + "' ";
                                conn.QueryAlimento(query, out dt1);
                                temp--;
                                repeticiones++;

                            }
                            while (dt1.Rows.Count == 0);
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
                                + " AND pmez_fecha BETWEEN '' AND '2021-01-31 14:00' "
                                + " AND ISNUMERIC(SUBSTRING(ing_nombre,1,1)) > 0 "
                                + " AND SUBSTRING(ing_nombre,3,2) IN('00', '01', '02')";
                        conn.QueryAlimento(query, out dtsPM);

                        //DiasPremezcla(premezcla, fpmI, fin);
                        DataTable dtV;
                        for (int i = 0; i < dtsPM.Rows.Count; i++)
                        {
                            query = "SELECT * FROM premezcla WHERE pmez_racion like '" + dtsPM.Rows[i][0].ToString() + "'";
                            conn.QueryAlimento(query, out dtV);

                            if (dtV.Rows.Count == 0)
                                continue;

                            SupraMezcla(dtsPM.Rows[i][0].ToString(), fpmI, fin);
                        }

                        query = "INSERT INTO porcentaje_Premezcla "
                           + " SELECT T1.Pmez, T1.Clave, T1.Ing, T1.Peso / T2.Peso "
                            + " FROM( "
                            + " SELECT rac_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND rac_descripcion LIKE '" + premezcla + "' "
                            + " GROUP BY rac_descripcion, ing_clave, ing_descripcion)T1 "
                            + " LEFT JOIN( "
                            + " SELECT rac_descripcion AS Pmez, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND rac_descripcion LIKE '" + premezcla + "' "
                            + " GROUP BY rac_descripcion "
                            + " ) T2 ON T1.Pmez = T2.Pmez ";
                        conn.InsertSelecttAlimento(query);
                    }
                }
            }
            catch (DbException ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void RacionEtapas(string etapa, DateTime inicio, DateTime fin, out DataTable dt)
        {
            DateTime tempIni, tempFin, fechaI, fechaF;
            string query;
            double media, pmsP;
            double precioLeche, lecheFederal, costoT, precioT, totalRacion, sobrante;
            TimeSpan ts = fin - inicio;
            int dias = ts.Days;
            int numAnimales = 0;
            int hcorte = 0;
            int horas = 0;
            Hora_Corte(out horas, out hcorte);
            DataTable dtIndicadores, dtV;
            ColumnasDTIAux(out dtIndicadores);
            for (int i = 0; i < dias; i++)
            {
                media = 0; pmsP = 0; precioLeche = 0; lecheFederal = 0; costoT = 0; precioT = 0; totalRacion = 0; numAnimales = 0; sobrante = 0;
                tempIni = inicio.AddDays(i);
                tempFin = tempIni.AddDays(1);
                fechaI = hcorte == 24 || hcorte == 0 ? tempIni : tempIni.AddDays(1);
                fechaF = hcorte == 24 || hcorte == 0 ? fechaI : tempFin;
                DataTable dt1;
                TotalRacion(etapa, tempIni, tempFin, out dt1);

                string campo = "", etp = "";
                switch (etapa)
                {
                    case "10,11,12,13":
                        etp = "1";
                        campo = "ia_vacas_ord";
                        break;
                    case "21":
                        etp = "2";
                        campo = "ia_vacas_secas";
                        break;
                    case "22":
                        etp = "4";
                        campo = "ia_vqreto + ia_vcreto";
                        break;
                    case "31":
                        etp = "3";
                        campo = "ia_jaulas";
                        break;
                    case "32":
                        etp = "3";
                        campo = "ia_destetadas";
                        break;
                    case "33":
                        etp = "3";
                        campo = "ia_destetadas2";
                        break;
                    case "34":
                        etp = "3";
                        campo = "ia_vaquillas";
                        break;
                }

                DataTable dt7;
                query = "SELECT ROUND(SUM(CONVERT(FLOAT, " + campo + "))/ COUNT(DISTINCT ia_fecha),0) FROM inventario_afi "
                        + " WHERE ia_fecha BETWEEN '" + fechaI.ToString("yyyy-MM-dd") + "' AND '" + fechaF.ToString("yyyy-MM-dd") + "' "
                        + " AND ran_id IN(" + ranNumero + ")";
                conn.QueryAlimento(query, out dt7);

                Int32.TryParse(dt7.Rows[0][0].ToString(), out numAnimales);
                if (dt1.Rows.Count > 0)
                {
                    DataTable dtPremezclas = new DataTable();
                    query = "select DISTINCT ing_descripcion FROM racion "
                        + " WHERE rac_fecha BETWEEN '" + tempIni.ToString("yyyy-MM-dd HH:mm") + "' AND '" + tempFin.ToString("yyyy-MM-dd HH:mm") + "' "
                        + " AND ran_id IN(" + ranNumero.ToString() + ") AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02') "
                        + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F') AND etp_id IN( " + etapa + ")";
                    conn.QueryAlimento(query, out dtPremezclas);

                    DataTable dtt;
                    for (int j = 0; j < dtPremezclas.Rows.Count; j++)
                    {
                        query = "SELECT  TOP(5) * FROM premezcla where pmez_racion like '" + dtPremezclas.Rows[j][0].ToString() + "' AND pmez_fecha < '" + tempFin.ToString("yyyy-MM-dd HH:mm") + "'";
                        conn.QueryAlimento(query, out dtt);

                        if (dtt.Rows.Count == 0)
                            continue;

                        CargarPremezcla(dtPremezclas.Rows[j][0].ToString(), tempIni, tempFin);
                    }
                   
                    DataTable dtTotal;
                    RacionCompleta(etapa, campo, tempIni, tempFin, out dt);

                    DataTable dt4;
                    query = "SELECT 'SOBRANTE' AS Ingrediente, ISNULL(SUM(rac_mh)/ DATEDIFF(DAY, '" + tempIni.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "', '" + tempFin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "'),0) AS Peso "
                       + " FROM racion where rac_fecha >= '" + tempIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + tempFin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "' AND rac_descripcion like '%SOB%' "
                       + " AND ing_descripcion like '" + etp + "%' AND ran_id IN(" + ranNumero + ") AND SUBSTRING(ing_descripcion,3,2) NOT IN('00','01','02')";
                    conn.QueryAlimento(query, out dt4);

                    DataTable dt5;
                    query = "SELECT IIF(SUM(ia.ia_vacas_ord)>0,ISNULL((SUM(m.med_produc)/SUM(ia.ia_vacas_ord)),0),0) , ISNULL(SUM(m.med_lecfederal + m.med_lecplanta) / COUNT(DISTINCT med_fecha),0) "
                            + " FROM media m LEFT JOIN inventario_afi ia ON ia.ran_id = m.ran_id AND ia.ia_fecha = m.med_fecha "
                            + " where m.ran_id IN(" + ranNumero + ") AND med_fecha BETWEEN '" + fechaI.ToString("yyyy-MM-dd") + "' AND '" + fechaF.ToString("yyyy-MM-dd") + "' ";
                    conn.QueryAlimento(query, out dt5);

                    DataTable dt6;
                    query = "SELECT AVG(T2.Precio) FROM( SELECT  ran_id AS Rancho, MAX(hl_fecha_reg) AS Fecha "
                            + " FROM historico_leche where ran_id IN(" + ranNumero + ") GROUP BY ran_id) T1 "
                            + " LEFT JOIN( "
                            + " SELECT ran_id AS Rancho, hl_fecha_reg AS Fecha, hl_precio AS Precio "
                            + " FROM historico_leche "
                            + " WHERE ran_id IN(" + ranNumero + "))T2 ON T1.Fecha = T2.Fecha AND T1.Rancho = T2.Rancho";
                    conn.QueryAlimento(query, out dt6);                    

                    pmsP = PMS(etapa, tempIni, tempFin);
                    costoT = Costo(etapa, campo, tempIni, tempFin);
                    try
                    {
                        precioLeche = dt6.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dt6.Rows[0][0]) : 0;
                        lecheFederal = dt5.Rows[0][1] != DBNull.Value ? Convert.ToDouble(dt5.Rows[0][1]) : 0;
                        media = dt5.Rows[0][1] != DBNull.Value ? Convert.ToDouble(dt5.Rows[0][1]) : 0;

                        //Double.TryParse(dt6.Rows[0][0].ToString(), out precioLeche);
                        //Double.TryParse(dt5.Rows[0][1].ToString(), out lecheFederal);
                        //Double.TryParse(dt5.Rows[0][0].ToString(), out media);
                        Double.TryParse(dt1.Rows[0][0].ToString(), out totalRacion);
                        sobrante = Convert.ToDouble(dt4.Rows[0][1]);
                        media = etp == "1" ? media : 0;
                    }
                    catch
                    {
                        numAnimales = numAnimales != 0 ? numAnimales : 0;
                        precioLeche = precioLeche != 0 ? precioLeche : 0;
                        lecheFederal = lecheFederal != 0 ? lecheFederal : 0;
                        media = media != 0 ? media : 0;
                        totalRacion = Convert.ToDouble(dt1.Rows[0][0]); //Total de racion
                    }

                    //Indicadores                       
                    DataRow drIndicadores = dtIndicadores.NewRow();
                    drIndicadores["Dia"] = i + 1;
                    drIndicadores["Animales"] = numAnimales;
                    drIndicadores["media"] = media;
                    drIndicadores["ilcavta"] = etp == "1"  && numAnimales > 0 ? (lecheFederal / numAnimales * precioLeche / costoT) : 0;
                    drIndicadores["icventa"] = etp == "1" && numAnimales > 0 ? (lecheFederal / numAnimales * precioLeche) - costoT : 0 - costoT;
                    drIndicadores["eaprod"] = etp == "1" && numAnimales > 0? media / (pmsP * (totalRacion / numAnimales) / 100):0;
                    drIndicadores["ilcaprod"] = costoT > 0 ? precioLeche * media / costoT : 0;
                    drIndicadores["icprod"] = (precioLeche * media) - costoT;
                    drIndicadores["preclprod"] = media > 0 ? costoT / media : 0;
                    drIndicadores["mhprod"] = numAnimales > 0 ? totalRacion / numAnimales : 0;
                    drIndicadores["porcmsprod"] = pmsP;
                    drIndicadores["msprod"] = numAnimales > 0 ? pmsP * (totalRacion / numAnimales) / 100 : 0;
                    drIndicadores["saprod"] = numAnimales > 0 ? sobrante / numAnimales : 0;
                    drIndicadores["mssprod"] = numAnimales > 0 ? ((totalRacion - sobrante) / numAnimales) * pmsP / 100 : 0;
                    drIndicadores["easprod"] = numAnimales > 0 ? media / ((totalRacion - sobrante) / numAnimales * pmsP / 100) : 0;
                    drIndicadores["precprod"] = costoT > 0 ? costoT : 0;
                    drIndicadores["precmsprod"] = numAnimales > 0 ? costoT / (pmsP * (totalRacion / numAnimales) / 100) : 0;
                    dtIndicadores.Rows.Add(drIndicadores);
                }
                else
                {
                    DataRow drIndicadores = dtIndicadores.NewRow();
                    drIndicadores["Dia"] = i + 1;
                    drIndicadores["Animales"] = numAnimales;
                    drIndicadores["media"] = 0;
                    drIndicadores["ilcavta"] = 0;
                    drIndicadores["icventa"] = 0;
                    drIndicadores["eaprod"] = 0;
                    drIndicadores["ilcaprod"] = 0;
                    drIndicadores["icprod"] = 0;
                    drIndicadores["preclprod"] = 0;
                    drIndicadores["mhprod"] = 0;
                    drIndicadores["porcmsprod"] = 0;
                    drIndicadores["msprod"] = 0;
                    drIndicadores["saprod"] = 0;
                    drIndicadores["mssprod"] = 0;
                    drIndicadores["precprod"] = 0;
                    drIndicadores["precmsprod"] = numAnimales > 0 && pmsP > 0? costoT / (pmsP * (totalRacion / numAnimales) / 100) : 0;
                    dtIndicadores.Rows.Add(drIndicadores);
                }
             }

            dt = new DataTable();
            dt.Columns.Add("dia").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("INV").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("MH").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCMS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIOMS").DataType = System.Type.GetType("System.Double");

            for (int i = 0; i < dtIndicadores.Rows.Count; i++)
            {
                DataRow dr = dt.NewRow();
                dr["Dia"] = Convert.ToInt32(dtIndicadores.Rows[i]["Dia"]);
                dr["INV"] = Convert.ToDouble(dtIndicadores.Rows[i]["Animales"]);
                dr["MH"] = Convert.ToDouble(dtIndicadores.Rows[i]["mhprod"]);
                dr["PRECIO"] = Convert.ToDouble(dtIndicadores.Rows[i]["precprod"]);
                dr["PORCMS"] = Convert.ToDouble(dtIndicadores.Rows[i]["porcmsprod"]);
                dr["MS"] = Convert.ToDouble(dtIndicadores.Rows[i]["msprod"]);
                dr["PRECIOMS"] = Convert.ToDouble(dtIndicadores.Rows[i]["precmsprod"]);
                dt.Rows.Add(dr);
            }
        }

        private void RacionCompleta(string etapa, string campo, DateTime inicio, DateTime fin, out DataTable dt)
        {
            ColumnasDT(out dt);
            int vacas = Animales(campo, fin);
            DataTable dt1;
            string query = "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
            + " FROM( "
            + " SELECT R.Clave, R.Ing, ISNULL(i.ing_precio_sie, 0) AS Precio, ISNULL(it.ingt_porcentaje_ms, 0) AS PMS, "
            + " SUM(R.Peso) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoH, "
            + " (SUM(R.Peso) * ISNULL(it.ingt_porcentaje_ms, 0) / 100) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoS "
            + " FROM( "
            + " SELECT ran_id AS Ran, r.ing_clave AS Clave, r.ing_descripcion AS Ing, SUM(rac_mh) AS Peso "
            + " FROM racion r "
            + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND etp_id IN(" + etapa + ") "
            + " AND SUBSTRING(ing_clave, 1, 4) IN('ALAS', 'ALFO') GROUP BY ran_id, ing_clave, ing_descripcion "
            + " UNION "
            + " SELECT T.Ran, T.Clave, T.Ing, SUM(T.Peso) "
            + " FROM( "
            + " SELECT T1.Ran, IIF(T2.Pmez IS NULL, T1.Clave, T2.Clave) AS Clave, IIF(T2.Pmez IS NULL, T1.Ing, T2.Ing) AS Ing, IIF(T2.Pmez IS NULL, T1.Peso, T1.Peso * T2.Porc) AS Peso "
            + " FROM( "
            + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso "
            + " FROM( "
            + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
            + " FROM racion "
            + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
            + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
            + " GROUP BY ran_id, ing_descripcion) T1 "
            + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
            + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
            + " GROUP BY T.Ran, T.Clave, T.Ing "
            + " UNION "
            + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh) "
            + " FROM racion "
            + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ")  AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F', 'W') "
            + " AND SUBSTRING(ing_descripcion, 3, 2) NOT IN('00', '01', '02')  AND etp_id IN(" + etapa + ") "
            + " GROUP BY ran_id, ing_clave, ing_descripcion "
            + " UNION "
            + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh) "
            + " FROM racion "
            + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ")  AND ing_descripcion IN('Agua', 'Water') "
            + " AND etp_id IN(" + etapa + ")  GROUP BY ran_id, ing_clave, ing_descripcion) R "
            + " LEFT JOIN ingrediente i ON i.ing_clave = R.Clave AND i.ing_descripcion = R.Ing AND i.ran_id = R.Ran "
            + " LEFT JOIN ingrediente_tracker it ON it.ingt_clave = R.Clave AND R.Ing = it.ingt_descripcion AND R.Ran = it.ran_id "
            + " GROUP BY R.Ran, R.Clave, R.Ing, i.ing_precio_sie, it.ingt_porcentaje_ms) X "
            + " WHERE X.PesoH > 0 GROUP BY x.Ing";
            conn.QueryAlimento(query, out dt1);

            DataTable dtTemp; ColumnasDT(out dtTemp);
            double xvaca, s_xvaca, totalR = 0, costoT = 0, txvaca = 0, tsxvaca = 0;
            double mh, ms, pms, precio, costo;

            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                precio = Convert.ToDouble(dt1.Rows[i][1]);
                pms = Convert.ToDouble(dt1.Rows[i][2]);
                mh = Convert.ToDouble(dt1.Rows[i][3]); totalR += mh;
                ms = Convert.ToDouble(dt1.Rows[i][4]);
                xvaca = mh / vacas;
                s_xvaca = ms / vacas;
                txvaca += xvaca;
                tsxvaca += s_xvaca;
                costoT += precio * xvaca;
                DataRow dr = dtTemp.NewRow();
                dr["ingrediente"] = dt1.Rows[i][0].ToString();
                dr["precioIng"] = dt1.Rows[i][1].ToString();
                dr["xvaca"] = xvaca;
                dr["TOTAL"] = mh;
                dr["COSTO"] = precio * xvaca;
                dr["PRECIO"] = precio * mh;
                dr["s_precioIng"] = pms > 0 ? precio * 100 / pms : 0;
                dr["s_xvaca"] = s_xvaca;
                dr["s_TOTAL"] = ms;
                dr["s_COSTO"] = (pms > 0 ? precio * 100 / pms : 0) * s_xvaca;
                dr["s_PRECIO"] = (pms > 0 ? precio * 100 / pms : 0) * s_xvaca;
                dr["PMS"] = pms;
                dtTemp.Rows.Add(dr);
            }

            for (int i = 0; i < dtTemp.Rows.Count; i++)
            {
                xvaca = Convert.ToDouble(dtTemp.Rows[i]["xvaca"]);
                s_xvaca = Convert.ToDouble(dtTemp.Rows[i]["s_xvaca"]);
                mh = Convert.ToDouble(dtTemp.Rows[i]["TOTAL"]);
                costo = Convert.ToDouble(dtTemp.Rows[i]["COSTO"]);
                dtTemp.Rows[i]["porcvaca"] = xvaca / txvaca * 100;
                dtTemp.Rows[i]["porccosto"] = costo / costoT * 100;
                dtTemp.Rows[i]["s_porcvaca"] = s_xvaca / tsxvaca * 100;
                dtTemp.Rows[i]["s_porccosto"] = costo / costoT * 100;
            }

        }


        //private void RacionEtapas(string etapa, DateTime inicio, DateTime fin, out DataTable dt)
        //{
        //    DateTime tempIni, tempFin;
        //    string query;
        //    double media, pmsP;
        //    double precioLeche, lecheFederal, costoT, precioT, totalRacion;
        //    TimeSpan ts = fin - inicio;
        //    int dias = ts.Days;
        //    int numAnimales = 0;

        //    DataTable dtIndicadores = new DataTable();
        //    ColumnasDTIAux(out dtIndicadores);

        //    for (int i = 0; i < dias; i++)
        //    {
        //        media = 0; pmsP = 0; 
        //        precioLeche = 0; lecheFederal = 0; costoT = 0; precioT = 0; totalRacion = 0; numAnimales = 0;

        //        tempIni = inicio.AddDays(i);
        //        tempFin = tempIni.AddDays(1);

        //        //Total Racion
        //        DataTable dt1;
        //        TotalRacion(etapa, tempIni, tempFin, out dt1);


        //        if (dt1.Rows.Count > 0)
        //        {
        //            DataTable dtPremezclas = new DataTable();
        //            query = "select DISTINCT ing_descripcion FROM racion "
        //                + " WHERE rac_fecha BETWEEN '" + tempIni.ToString("yyyy-MM-dd HH:mm") + "' AND '" + tempFin.ToString("yyyy-MM-dd HH:mm") + "' "
        //                + " AND ran_id IN(" + ranNumero.ToString() + ") AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02') "
        //                + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F') AND etp_id IN( " + etapa + ")";
        //            conn.QueryAlimento(query, out dtPremezclas);

        //            conn.DeleteAlimento("porcentaje_Premezcla", "");
        //            DataTable dtt;
        //            for (int j = 0; j < dtPremezclas.Rows.Count; j++)
        //            {
        //                query = "SELECT  TOP(5) * FROM premezcla where pmez_racion like '" + dtPremezclas.Rows[j][0].ToString() + "'";
        //                conn.QueryAlimento(query, out dtt);

        //                if (dtt.Rows.Count == 0)
        //                    continue;

        //                CargarPremezcla(dtPremezclas.Rows[j][0].ToString(), tempIni, tempFin);
        //            }

        //            //Seccion de Racion de Forraje y sobrante
        //            DataTable dt2;
        //            RacionAlfoSob(etapa, tempIni, tempFin, out dt2);

        //            //Seccion de Racion de alas
        //            DataTable dt3;
        //            RacionAlas(etapa, tempIni, tempFin, true, out dt3);

        //            // Seccion Sobrante
        //            DataTable dt4;
        //            TotalSob(etapa, tempIni, tempFin, out dt4);

        //            //media y leche federal
        //            DataTable dt5;
        //            MediaLeche(tempIni, out dt5);

        //            //Precio  Leche
        //            DataTable dt6;
        //            PrecioLeche(out dt6);

        //            //Numero de vacas
        //            DataTable dt7;
        //            TotalAnimales(etapa, tempIni, tempFin, out dt7);

        //            //Porcentaje de materia seca
        //            DataTable dt8;
        //            //PMS(etapa, tempIni, tempFin, out dt8);
        //            pmsP = PMS(etapa, tempIni, tempFin);
        //            //Asignar valores a variables
        //            try
        //            {
        //                numAnimales = 0;
        //                precioLeche = 0;
        //                lecheFederal = 0;
        //                media = 0;

        //                //if (dt8.Rows.Count > 0)
        //                //    Double.TryParse(dt8.Rows[0][0].ToString(), out pmsP);

        //                if (dt7.Rows.Count > 0)
        //                    Int32.TryParse(dt7.Rows[0][0].ToString(), out numAnimales);

        //                if (dt6.Rows.Count > 0)
        //                    Double.TryParse(dt6.Rows[0][0].ToString(), out precioLeche);

        //                if (dt5.Rows.Count > 0)
        //                {
        //                    Double.TryParse(dt5.Rows[0][0].ToString(), out media);
        //                    Double.TryParse(dt5.Rows[0][1].ToString(), out lecheFederal);
        //                }
        //            }
        //            catch
        //            {
        //                pmsP = pmsP != 0 ? pmsP : 0;
        //                numAnimales = numAnimales != 0 ? numAnimales : 0;
        //                precioLeche = precioLeche != 0 ? precioLeche : 0;
        //                lecheFederal = lecheFederal != 0 ? lecheFederal : 0;
        //                media = media != 0 ? media : 0;
        //            }

        //            totalRacion = Convert.ToDouble(dt1.Rows[0][0]); //Total de racion

        //            DataTable dtALFO = new DataTable();
        //            ColumnasDTRAux(out dtALFO);

        //            DataTable dtALAS = new DataTable();
        //            ColumnasDTRAux(out dtALAS);

        //            DataTable dtTotalR = new DataTable();
        //            ColumnasDTRAux(out dtTotalR);

        //            string ingrediente;
        //            double precioIng, peso, xvaca, costo, precio, porcR;

        //            //variables forraje
        //            double totingF = 0, totxvacaF = 0, totporcR = 0, totalF = 0, totcostoF = 0, totprecioF = 0;
        //            for (int j = 0; j < dt2.Rows.Count; j++)
        //            {
        //                ingrediente = dt2.Rows[j][1].ToString();
        //                peso = Convert.ToDouble(dt2.Rows[j][2]); totalF += peso;
        //                precioIng = Convert.ToDouble(dt2.Rows[j][3]); totingF += precioIng;
        //                xvaca = (peso / numAnimales); totxvacaF += xvaca;
        //                costo = precioIng * xvaca; totcostoF += costo;
        //                precio = precioIng * peso; totprecioF += precio;
        //                porcR = peso / totalRacion * 100; totporcR += porcR;
        //                costoT += costo; precioT += precio;
        //                DataRow dr = dtALFO.NewRow();
        //                dr["ingrediente"] = ingrediente;
        //                dr["precioIng"] = precioIng;
        //                dr["xvaca"] = xvaca;
        //                dr["porcvaca"] = porcR;
        //                dr["TOTAL"] = peso;
        //                dr["COSTO"] = costo;
        //                dr["porccosto"] = 0;
        //                dr["PRECIO"] = precio;
        //                dtALFO.Rows.Add(dr);
        //            }
        //            DataRow drA = dtALFO.NewRow();
        //            drA["ingrediente"] = "Total Forraje";
        //            drA["precioIng"] = totingF;
        //            drA["xvaca"] = totxvacaF;
        //            drA["porcvaca"] = totporcR;
        //            drA["TOTAL"] = totalF;
        //            drA["COSTO"] = totcostoF;
        //            drA["porccosto"] = 0;
        //            drA["PRECIO"] = totprecioF;
        //            dtALFO.Rows.Add(drA);

        //            //variables para alimentos
        //            double totingA = 0, totxvacaA = 0, totporcRA = 0, totalA = 0, totcostoA = 0, totprecioA = 0;
        //            for (int j = 0; j < dt3.Rows.Count; j++)
        //            {
        //                ingrediente = dt3.Rows[j][1].ToString();
        //                peso = Convert.ToDouble(dt3.Rows[j][2]); totalA += peso;
        //                precioIng = Convert.ToDouble(dt3.Rows[j][3]); totingA += precioIng;
        //                xvaca = (peso / numAnimales); totxvacaA += xvaca;
        //                costo = precioIng * xvaca; totcostoA += costo;
        //                precio = precioIng * peso; totprecioA += precio;
        //                porcR = peso / totalRacion * 100; totporcRA += porcR;
        //                costoT += costo; precioT += precio;
        //                DataRow dr = dtALAS.NewRow();
        //                dr["ingrediente"] = ingrediente;
        //                dr["precioIng"] = precioIng;
        //                dr["xvaca"] = xvaca;
        //                dr["porcvaca"] = porcR;
        //                dr["TOTAL"] = peso;
        //                dr["COSTO"] = costo;
        //                dr["porccosto"] = 0;
        //                dr["PRECIO"] = precio;
        //                dtALAS.Rows.Add(dr);
        //            }
        //            DataRow drALAS = dtALAS.NewRow();
        //            drALAS["ingrediente"] = "Total Concentrado";
        //            drALAS["precioIng"] = totingA;
        //            drALAS["xvaca"] = totxvacaA;
        //            drALAS["porcvaca"] = totporcRA;
        //            drALAS["TOTAL"] = totalA;
        //            drALAS["COSTO"] = totcostoA;
        //            drALAS["porccosto"] = 0;
        //            drALAS["PRECIO"] = totprecioA;
        //            dtALAS.Rows.Add(drALAS);

        //            double porcCosto;
        //            for (int j = 0; j < dtALFO.Rows.Count; j++)
        //            {
        //                porcCosto = Convert.ToDouble(dtALFO.Rows[j]["COSTO"]) / costoT * 100;
        //                dtALFO.Rows[j]["porccosto"] = porcCosto;
        //            }

        //            for (int j = 0; j < dtALAS.Rows.Count; j++)
        //            {
        //                porcCosto = Convert.ToDouble(dtALAS.Rows[j]["COSTO"]) / costoT * 100;
        //                dtALAS.Rows[j]["porccosto"] = porcCosto;
        //            }

        //            DataTable dtSob = new DataTable();
        //            ColumnasDTRAux(out dtSob);

        //            DataRow drSob = dtSob.NewRow();
        //            double sobrante = Convert.ToDouble(dt4.Rows[0][1]);
        //            double xvacaSob = sobrante / numAnimales;
        //            drSob["ingrediente"] = "Sobrante";
        //            drSob["xvaca"] = xvacaSob;
        //            drSob["TOTAL"] = sobrante;
        //            dtSob.Rows.Add(drSob);
        //            string campo = "";
        //            switch (etapa)
        //            {
        //                case "10,11,12,13": campo = "ia_vacas_ord"; break;
        //                case "21": campo = "ia_vacas_secas"; break;
        //                case "22": campo = "ia_vqreto + ia_vcreto"; break;
        //                case "31": campo = "ia_jaulas"; break;
        //                case "32": campo = "ia_destetadas"; break;
        //                case "33": campo = "ia_destetadas2"; break;
        //                case "34": campo = "ia_vaquillas"; break;
        //            }

        //            costoT = Costo(etapa, campo, inicio, fin);
        //            //Indicadores                       
        //            DataRow drIndicadores = dtIndicadores.NewRow();
        //            drIndicadores["Dia"] = i + 1;
        //            drIndicadores["Animales"] = numAnimales;
        //            drIndicadores["media"] = media;
        //            drIndicadores["ilcavta"] = (lecheFederal / numAnimales * precioLeche / costoT);
        //            drIndicadores["icventa"] = (lecheFederal / numAnimales * precioLeche) - costoT;
        //            drIndicadores["eaprod"] = media / (pmsP * (totalRacion / numAnimales) / 100);
        //            drIndicadores["ilcaprod"] = precioLeche * media / costoT;
        //            drIndicadores["icprod"] = (precioLeche * media) - costoT;
        //            drIndicadores["preclprod"] = costoT / media;
        //            drIndicadores["mhprod"] = totalRacion / numAnimales;
        //            drIndicadores["porcmsprod"] = pmsP;
        //            drIndicadores["msprod"] = pmsP * (totalRacion / numAnimales) / 100;
        //            drIndicadores["saprod"] = sobrante / numAnimales;
        //            drIndicadores["mssprod"] = ((totalRacion - sobrante) / numAnimales) * pmsP / 100;
        //            drIndicadores["easprod"] = media / ((totalRacion - sobrante) / numAnimales * pmsP / 100);
        //            drIndicadores["precprod"] = costoT;
        //            drIndicadores["precmsprod"] = costoT / (pmsP * (totalRacion / numAnimales) / 100);
        //            dtIndicadores.Rows.Add(drIndicadores);
        //        }
        //        else
        //        {
        //            DataTable dt7;
        //            TotalAnimales(etapa, tempIni, tempFin, out dt7);
        //            numAnimales = dt7.Rows.Count > 0 ? Convert.ToInt32(dt7.Rows[0][0]) : 0;

        //            DataRow drIndicadores = dtIndicadores.NewRow();
        //            drIndicadores["Dia"] = i + 1;
        //            drIndicadores["Animales"] = numAnimales;
        //            drIndicadores["media"] = 0;
        //            drIndicadores["ilcavta"] = 0;
        //            drIndicadores["icventa"] = 0;
        //            drIndicadores["eaprod"] = 0;
        //            drIndicadores["ilcaprod"] = 0;
        //            drIndicadores["icprod"] = 0;
        //            drIndicadores["preclprod"] = 0;
        //            drIndicadores["mhprod"] = 0;
        //            drIndicadores["porcmsprod"] = 0;
        //            drIndicadores["msprod"] = 0;
        //            drIndicadores["saprod"] = 0;
        //            drIndicadores["mssprod"] = 0;
        //            drIndicadores["easprod"] = 0;
        //            drIndicadores["precprod"] = 0;
        //            drIndicadores["precmsprod"] = 0;
        //            dtIndicadores.Rows.Add(drIndicadores);
        //        }
        //    }

        //    dt = new DataTable();
        //    dt.Columns.Add("dia").DataType = System.Type.GetType("System.Int32");            
        //    dt.Columns.Add("INV").DataType = System.Type.GetType("System.Int32");
        //    dt.Columns.Add("MH").DataType = System.Type.GetType("System.Double");
        //    dt.Columns.Add("PRECIO").DataType = System.Type.GetType("System.Double");
        //    dt.Columns.Add("PORCMS").DataType = System.Type.GetType("System.Double");
        //    dt.Columns.Add("MS").DataType = System.Type.GetType("System.Double");
        //    dt.Columns.Add("PRECIOMS").DataType = System.Type.GetType("System.Double");

        //    for (int i = 0; i < dtIndicadores.Rows.Count; i++)
        //    {
        //        DataRow dr = dt.NewRow();
        //        dr["Dia"] = Convert.ToInt32(dtIndicadores.Rows[i]["Dia"]);
        //        dr["INV"] = Convert.ToDouble(dtIndicadores.Rows[i]["Animales"]);
        //        dr["MH"] = Convert.ToDouble(dtIndicadores.Rows[i]["mhprod"]);
        //        dr["PRECIO"] = Convert.ToDouble(dtIndicadores.Rows[i]["precprod"]);
        //        dr["PORCMS"] = Convert.ToDouble(dtIndicadores.Rows[i]["porcmsprod"]);
        //        dr["MS"] = Convert.ToDouble(dtIndicadores.Rows[i]["msprod"]);
        //        dr["PRECIOMS"] = Convert.ToDouble(dtIndicadores.Rows[i]["precmsprod"]);
        //        dt.Rows.Add(dr);
        //    }
        //    Console.WriteLine("");
        //}

        private void RacionSR(string etapa, DateTime inicio, DateTime fin, out DataTable dt)
        {
            if (etapa == "22")
                Console.WriteLine("");
            DateTime tempIni, tempFin, fechaI, fechaF;
            string query;
            double media, pmsP;
            double precioLeche, lecheFederal, costoT, precioT, totalRacion, sobrante;
            TimeSpan ts = fin - inicio;
            int dias = ts.Days;
            int numAnimales = 0;
            int hcorte = 0;
            int horas = 0;
            Hora_Corte(out horas, out hcorte);

            DataTable dtIndicadores;
            ColumnasDTIAux(out dtIndicadores);
            for (int i = 0; i < dias; i++)
           {
                media = 0; pmsP = 0; precioLeche = 0; lecheFederal = 0; costoT = 0; precioT = 0; totalRacion = 0; numAnimales = 0; sobrante = 0;
                tempIni = inicio.AddDays(i);
                tempFin = tempIni.AddDays(1);
                DataTable dt1;
                TotalRacion(etapa, tempIni, tempFin, out dt1);
                fechaI = hcorte == 0 || hcorte == 24 ? tempIni : tempFin;
                fechaF = hcorte == 0 || hcorte == 24 ? tempIni : tempFin;

                string campo = "", etp = "";
                switch (etapa)
                {
                    case "10,11,12,13":
                        etp = "1";
                        campo = "ia_vacas_ord";
                        break;
                    case "21":
                        etp = "2";
                        campo = "ia_vacas_secas";
                        break;
                    case "22":
                        etp = "4";
                        campo = "ia_vqreto + ia_vcreto";
                        break;
                    case "31":
                        etp = "3";
                        campo = "ia_jaulas";
                        break;
                    case "32":
                        etp = "3";
                        campo = "ia_destetadas";
                        break;
                    case "33":
                        etp = "3";
                        campo = "ia_destetadas2";
                        break;
                    case "34":
                        etp = "3";
                        campo = "ia_vaquillas";
                        break;
                }

                DataTable dt7;
                query = "SELECT ROUND(SUM(CONVERT(FLOAT, " + campo + "))/ COUNT(DISTINCT ia_fecha),0) FROM inventario_afi "
                        + " WHERE ia_fecha BETWEEN '" + tempIni.AddDays(1).ToString("yyyy-MM-dd") + "' AND '" + tempFin.ToString("yyyy-MM-dd") + "' "
                        + " AND ran_id IN(" + ranNumero + ")";
                conn.QueryAlimento(query, out dt7);


                if (dt7.Rows.Count > 0)
                    if (dt7.Rows[0][0] != DBNull.Value)
                        numAnimales = Convert.ToInt32(dt7.Rows[0][0]);

                if (dt1.Rows.Count > 0)
                {
                    DataTable dtPremezclas = new DataTable();
                    query = "select DISTINCT ing_descripcion FROM racion "
                        + " WHERE rac_fecha BETWEEN '" + tempIni.ToString("yyyy-MM-dd HH:mm") + "' AND '" + tempFin.ToString("yyyy-MM-dd HH:mm") + "' "
                        + " AND ran_id IN(" + ranNumero.ToString() + ") AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02') "
                        + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F') AND etp_id IN( " + etapa + ")";
                    conn.QueryAlimento(query, out dtPremezclas);

                    DataTable dtt;
                    for (int j = 0; j < dtPremezclas.Rows.Count; j++)
                    {
                        query = "SELECT  TOP(5) * FROM premezcla where pmez_racion like '" + dtPremezclas.Rows[j][0].ToString() + "' AND pmez_fecha < '" + tempFin.ToString("yyyy-MM-dd HH:mm") + "'";
                        conn.QueryAlimento(query, out dtt);

                        if (dtt.Rows.Count == 0)
                            continue;

                        CargarPremezcla(dtPremezclas.Rows[j][0].ToString(), tempIni, tempFin);
                    }                   

                    DataTable dtTotal;
                    RacionCompleta(etapa, campo, tempIni, tempFin, out dt);

                    DataTable dt4;
                    query = "SELECT 'SOBRANTE' AS Ingrediente, ISNULL(SUM(rac_mh)/ DATEDIFF(DAY, '" + tempIni.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "', '" + tempFin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "'),0) AS Peso "
                       + " FROM racion where rac_fecha >= '" + tempIni.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + tempFin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "' AND rac_descripcion like '%SOB%' "
                       + " AND ing_descripcion like '" + etp + "%' AND ran_id IN(" + ranNumero + ") AND SUBSTRING(ing_descripcion,3,2) NOT IN('00','01','02')";
                    conn.QueryAlimento(query, out dt4);

                    DataTable dt5;
                    query = "SELECT IIF(SUM(ia.ia_vacas_ord)>0,ISNULL((SUM(m.med_produc)/SUM(ia.ia_vacas_ord)),0),0) , ISNULL(SUM(m.med_lecfederal + m.med_lecplanta) / COUNT(DISTINCT med_fecha),0) "
                            + " FROM media m LEFT JOIN inventario_afi ia ON ia.ran_id = m.ran_id AND ia.ia_fecha = m.med_fecha "
                            + " where m.ran_id IN(" + ranNumero + ") AND med_fecha BETWEEN '" + fechaI.ToString("yyyy-MM-dd") + "' AND '" + fechaF.ToString("yyyy-MM-dd") + "' ";
                    conn.QueryAlimento(query, out dt5);

                    DataTable dt6;
                    query = "SELECT AVG(T2.Precio) FROM( SELECT  ran_id AS Rancho, MAX(hl_fecha_reg) AS Fecha "
                            + " FROM historico_leche where ran_id IN(" + ranNumero + ") GROUP BY ran_id) T1 "
                            + " LEFT JOIN( "
                            + " SELECT ran_id AS Rancho, hl_fecha_reg AS Fecha, hl_precio AS Precio "
                            + " FROM historico_leche "
                            + " WHERE ran_id IN(" + ranNumero + "))T2 ON T1.Fecha = T2.Fecha AND T1.Rancho = T2.Rancho";
                    conn.QueryAlimento(query, out dt6);

                  

                    pmsP = PMS(etapa, tempIni, tempFin);
                    costoT = Costo(etapa, campo, tempIni, tempFin);
                    try
                    {
                        numAnimales = dt7.Rows[0][0] != DBNull.Value ? Convert.ToInt32(dt7.Rows[0][0]) : 0;
                        precioLeche = dt6.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dt6.Rows[0][0]) : 0;
                        lecheFederal = dt5.Rows[0][1] != DBNull.Value ? Convert.ToDouble(dt5.Rows[0][0]) : 0;
                        media = dt5.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dt5.Rows[0][0]) : 0;


                        //Int32.TryParse(dt7.Rows[0][0].ToString(), out numAnimales);
                        //Double.TryParse(dt6.Rows[0][0].ToString(), out precioLeche);
                        //Double.TryParse(dt5.Rows[0][1].ToString(), out lecheFederal);
                        //Double.TryParse(dt5.Rows[0][0].ToString(), out media);
                        Double.TryParse(dt1.Rows[0][0].ToString(), out totalRacion);
                        sobrante = Convert.ToDouble(dt4.Rows[0][1]);
                        media = etp == "1" ? media : 0;
                    }
                    catch
                    {
                        numAnimales = numAnimales != 0 ? numAnimales : 0;
                        precioLeche = precioLeche != 0 ? precioLeche : 0;
                        lecheFederal = lecheFederal != 0 ? lecheFederal : 0;
                        media = media != 0 ? media : 0;
                        totalRacion = Convert.ToDouble(dt1.Rows[0][0]); //Total de racion
                    }

                    //Indicadores                       
                    DataRow drIndicadores = dtIndicadores.NewRow();
                    drIndicadores["Dia"] = i + 1;
                    drIndicadores["Animales"] = numAnimales;
                    drIndicadores["media"] = media;
                    drIndicadores["ilcavta"] = etp =="1" & numAnimales > 0 && costoT > 0 ? (lecheFederal / numAnimales * precioLeche / costoT) : 0;
                    drIndicadores["icventa"] = etp == "1" &  numAnimales > 0 ? (lecheFederal / numAnimales * precioLeche) - costoT : 0;
                    drIndicadores["eaprod"] = etp == "1" & numAnimales >  0 ?  media / (pmsP * (totalRacion / numAnimales) / 100) : 0;
                    drIndicadores["ilcaprod"] = etp == "1" & costoT >  0 ? precioLeche * media / costoT : 0;
                    drIndicadores["icprod"] = etp == "1"  ?(precioLeche * media) - costoT :0;
                    drIndicadores["preclprod"] = media >  0 ?  costoT / media : 0;
                    drIndicadores["mhprod"] = numAnimales >  0 ?  totalRacion / numAnimales: 0;
                    drIndicadores["porcmsprod"] = pmsP;
                    drIndicadores["msprod"] = numAnimales >  0 ?  pmsP * (totalRacion / numAnimales) / 100 : 0;
                    drIndicadores["saprod"] = numAnimales >  0 ?  sobrante / numAnimales : 0;
                    drIndicadores["mssprod"] = numAnimales >  0 ?  ((totalRacion - sobrante) / numAnimales) * pmsP / 100 : 0;
                    drIndicadores["easprod"] = numAnimales > 0 ? media / ((totalRacion - sobrante) / numAnimales * pmsP / 100) : 0;
                    drIndicadores["precprod"] = costoT;
                    drIndicadores["precmsprod"] = numAnimales >  0 ?  costoT / (pmsP * (totalRacion / numAnimales) / 100) : 0;
                    dtIndicadores.Rows.Add(drIndicadores);
                }
                else
                {
                    DataRow drIndicadores = dtIndicadores.NewRow();
                    drIndicadores["Dia"] = i + 1;
                    drIndicadores["Animales"] = numAnimales;
                    drIndicadores["media"] = 0;
                    drIndicadores["ilcavta"] = 0;
                    drIndicadores["icventa"] = 0;
                    drIndicadores["eaprod"] = 0;
                    drIndicadores["ilcaprod"] = 0;
                    drIndicadores["icprod"] = 0;
                    drIndicadores["preclprod"] = 0;
                    drIndicadores["mhprod"] = 0;
                    drIndicadores["porcmsprod"] = 0;
                    drIndicadores["msprod"] = 0;
                    drIndicadores["saprod"] = 0;
                    drIndicadores["mssprod"] = 0;
                    drIndicadores["easprod"] = 0;
                    drIndicadores["precprod"] = 0;
                    drIndicadores["precmsprod"] = 0;
                    dtIndicadores.Rows.Add(drIndicadores);
                }
            }

            dt = new DataTable();
            dt.Columns.Add("Dia").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("INV").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("MH").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCMS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("SA").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MSS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIOMS").DataType = System.Type.GetType("System.Double");

            for (int i = 0; i < dtIndicadores.Rows.Count; i++)
            {
                double sa, mh;
                sa = Convert.ToDouble(dtIndicadores.Rows[i]["saprod"]);
                mh = Convert.ToDouble(dtIndicadores.Rows[i]["mhprod"]);
                DataRow dr = dt.NewRow();
                dr["Dia"] = Convert.ToInt32(dtIndicadores.Rows[i]["Dia"]);
                dr["INV"] = Convert.ToDouble(dtIndicadores.Rows[i]["Animales"]);
                dr["MH"] = mh;
                dr["PORCMS"] = Convert.ToDouble(dtIndicadores.Rows[i]["porcmsprod"]);
                dr["MS"] = Convert.ToDouble(dtIndicadores.Rows[i]["msprod"]);
                dr["SA"] = sa;
                dr["MSS"] = Convert.ToDouble(dtIndicadores.Rows[i]["mssprod"]);
                dr["PORCS"] = mh >  0 ? sa / mh * 100 : 0;
                dr["PRECIO"] = Convert.ToDouble(dtIndicadores.Rows[i]["precprod"]);
                dr["PRECIOMS"] = Convert.ToDouble(dtIndicadores.Rows[i]["precmsprod"]);
                dt.Rows.Add(dr);
            }

        }
        private void TotalRacion(string etapa, DateTime inicio, DateTime fin, out DataTable dt)
        {
            string query = "SELECT T.Total FROM( SELECT ISNULL(SUM(rac_mh) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "'), 0) AS Total "
                    + " from racion where rac_fecha"
                    + " >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ") and etp_id IN( " + etapa + ")) T"
                    + " WHERE T.Total > 0";
            conn.QueryAlimento(query, out dt);

        }

        private void RacionAlas(string etapa, DateTime inicio, DateTime fin, bool pmzCargadas, out DataTable dt)
        {
            string query = "SELECT T.Clave, T.ingrediente, SUM(T.Peso)/DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "','" + fin.ToString("yyyy-MM-dd HH:mm") + "') AS Peso, " +
                "ISNULL(SUM(T.Precio) / COUNT(*), 0) AS Precio "
                            + " FROM( "
                            + " SELECT R.Rancho, R.Clave, R.ingrediente, R.Peso, Ing.Precio "
                            + " FROM( "
                            + " SELECT T.Rancho, T.Clave, T.ingrediente, SUM(T.Peso) AS Peso "
                            + " FROM( "
                            + " SELECT ran_id AS Rancho, ing_clave AS Clave, ing_descripcion AS ingrediente, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE ran_id IN(" + ranNumero + ") "
                            + " AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND etp_id IN(" + etapa + ") AND SUBSTRING(ing_clave, 1, 4) IN('ALAS') "
                            + " GROUP BY ran_id, ing_clave, ing_descripcion "
                            + " UNION "
                            + " SELECT R.Rancho, R.Clave, R.Ingrediente, SUM(R.Peso) AS Peso "
                            + " FROM( "
                            + " SELECT T.Rancho, IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, IIF(T.Pmz = '', T.Ing1, T.Ing2) AS Ingrediente, T.Peso * T.Porc AS Peso "
                            + " FROM( "
                            + " SELECT T1.Rancho, T1.clave AS Clave1, T1.Ing AS Ing1, T1.Peso, ISNULL(T2.Pmz, '') AS Pmz, ISNULL(T2.clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porc, 1) AS Porc "
                            + " FROM( "
                            + " SELECT R.Rancho, R.clave, R.Ing, SUM(R.Peso) AS Peso "
                            + " FROM( "
                            + " SELECT T1.Rancho, T2.clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso "
                            + " FROM( "
                            + " SELECT ran_id AS Rancho, ing_descripcion As Pmz, SUM(rac_mh) AS Peso "
                            + "  FROM racion "
                            + " WHERE ran_id IN(" + ranNumero + ") "
                            + " AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0  AND etp_id IN(" + etapa + ") AND SUBSTRING(ing_descripcion,3,2) IN('00','01','02') "
                            + " GROUP BY ran_id, ing_descripcion "
                            + " ) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmz, ing_clave AS clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                            + " FROM porcentaje_Premezcla "
                            + " )T2 ON T1.Pmz = T2.Pmz "
                            + ") R "
                            + " GROUP BY R.Rancho, R.clave, R.Ing) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmz, ing_clave AS clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                            + " FROM porcentaje_Premezcla "
                            + " ) T2 ON T1.Ing = T2.Pmz) T) R "
                            + " GROUP BY R.Rancho, R.Clave, R.Ingrediente) T "
                            + " GROUP BY T.Rancho, T.Clave, T.ingrediente) R "
                            + " LEFT JOIN( "
                            + " SELECT DISTINCT i.ran_id AS Rancho, i.ing_clave AS Clave, i.ing_descripcion AS Ingrediente, IIF(c.ran_sie = 1, i.ing_precio_sie, i.ing_precio_tracker) AS Precio "
                            + " FROM ingrediente i "
                            + " LEFT JOIN[DBSIO].[dbo].configuracion c on i.ran_id = c.ran_id "
                            + " WHERE i.ran_id IN(" + ranNumero + ") "
                            + " ) Ing ON Ing.Rancho = R.Rancho AND Ing.Clave = R.Clave AND Ing.Ingrediente = R.ingrediente) T "
                            + " WHERE SUBSTRING(T.Clave, 1, 4) IN('ALAS') AND T.Peso > 0 "
                            + " GROUP BY T.Clave, T.ingrediente";
            conn.QueryAlimento(query, out dt);
        }
        

        private void RacionAlfoSob(string etapa, DateTime inicio, DateTime fin, out DataTable dt)
        {
            string query = "SELECT T.Clave, T.ingrediente, SUM(T.Peso)/DATEDIFF(DAY, '2020-12-31 14:00','2021-01-07 14:00') AS Peso, ISNULL(SUM(T.Precio) / COUNT(*), 0) AS Precio "
                       + " FROM( "
                       + " SELECT R.Rancho, R.Clave, R.ingrediente, R.Peso, Ing.Precio "
                       + " FROM( "
                       + " SELECT T.Rancho, T.Clave, T.ingrediente, SUM(T.Peso) AS Peso "
                       + " FROM( "
                       + " SELECT ran_id AS Rancho, ing_clave AS Clave, ing_descripcion AS ingrediente, SUM(rac_mh) AS Peso "
                       + " FROM racion "
                       + " WHERE ran_id IN(" + ranNumero + ")"
                       + " AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                       + " AND etp_id IN(" + etapa + ") AND SUBSTRING(ing_clave, 1, 4) IN('ALFO') "
                       + " GROUP BY ran_id, ing_clave, ing_descripcion "
                       + " UNION "
                       + " SELECT R.Rancho, R.Clave, R.Ingrediente, SUM(R.Peso) AS Peso "
                       + " FROM( "
                       + " SELECT T.Rancho, IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, IIF(T.Pmz = '', T.Ing1, T.Ing2) AS Ingrediente, T.Peso * T.Porc AS Peso "
                       + " FROM( "
                       + " SELECT T1.Rancho, T1.clave AS Clave1, T1.Ing AS Ing1, T1.Peso, ISNULL(T2.Pmz, '') AS Pmz, ISNULL(T2.clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porc, 1) AS Porc "
                       + " FROM( "
                       + " SELECT R.Rancho, R.clave, R.Ing, SUM(R.Peso) AS Peso "
                       + " FROM( "
                       + " SELECT T1.Rancho, T2.clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso "
                       + " FROM( "
                       + " SELECT ran_id AS Rancho, ing_descripcion As Pmz, SUM(rac_mh) AS Peso "
                       + " FROM racion "
                       + " WHERE ran_id IN(" + ranNumero + ")"
                       + " AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                       + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND etp_id IN(" + etapa + ") "
                       + " GROUP BY ran_id, ing_descripcion ) T1 "
                       + " LEFT JOIN( "
                       + " SELECT pmez_descripcion AS Pmz, ing_clave AS clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                       + " FROM porcentaje_Premezcla "
                       + " )T2 ON T1.Pmz = T2.Pmz) R "
                       + " GROUP BY R.Rancho, R.clave, R.Ing) T1 "
                       + " LEFT JOIN( "
                       + " SELECT pmez_descripcion AS Pmz, ing_clave AS clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                       + " FROM porcentaje_Premezcla "
                       + " ) T2 ON T1.Ing = T2.Pmz) T) R "
                       + " GROUP BY R.Rancho, R.Clave, R.Ingrediente) T "
                       + " GROUP BY T.Rancho, T.Clave, T.ingrediente) R "
                       + " LEFT JOIN( "
                       + " SELECT DISTINCT i.ran_id AS Rancho, i.ing_clave AS Clave, i.ing_descripcion AS Ingrediente, IIF(c.ran_sie = 1, i.ing_precio_sie, i.ing_precio_tracker) AS Precio "
                       + " FROM ingrediente i "
                       + " LEFT JOIN[DBSIO].[dbo].configuracion c on i.ran_id = c.ran_id "
                       + " WHERE i.ran_id IN(" + ranNumero + ") "
                       + " ) Ing ON Ing.Rancho = R.Rancho AND Ing.Clave = R.Clave AND Ing.Ingrediente = R.ingrediente) T "
                       + " WHERE SUBSTRING(T.Clave, 1, 4) IN('ALFO') AND T.Peso > 0 "
                       + " GROUP BY T.Clave, T.ingrediente "
                       + " UNION "
                       + " select ing_clave, ing_descripcion, SUM(rac_mh)/ DATEDIFF(DAY, '2020-12-31 14:00', '2021-01-07 14:00'), SUM(rac_precio_uni) "
                       + " FROM racion "
                       + " where rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                       + " AND etp_id IN(" + etapa + ") AND ran_id IN(" + ranNumero + ") "
                       + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F','W') AND SUBSTRING(ing_descripcion,3,2) NOT IN('00','01','02') "
                       + " GROUP BY ing_clave, ing_descripcion ";
            conn.QueryAlimento(query, out dt);
        }       

        private void TotalSob(string etapa, DateTime inicio, DateTime fin, out DataTable dt)
        {
            string etp = "";
            switch (etapa)
            {
                case "10,11,12,13": etp = "1"; break;
                case "21": etp = "2"; break;
                case "22": etp = "4"; break;
                case "31": case "32": case "33": case "34": etp = "3"; break;
            }

            string query = "SELECT 'SOBRANTE' AS Ingrediente, ISNULL(SUM(rac_mh)/ DATEDIFF(DAY, '" + inicio.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "', '" + fin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "'),0) AS Peso "
                        + " FROM racion where rac_fecha BETWEEN '" + inicio.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "' AND rac_descripcion like '%SOB%' "
                        + " AND ing_descripcion like '" + etp + "%' AND ran_id IN(" + ranNumero + ")";
            conn.QueryAlimento(query, out dt);
        }

        private void MediaLeche(DateTime inicio, out DataTable dt)
        {
            string query = "SELECT IIF(SUM(ia.ia_vacas_ord) > 0, (SUM(m.med_produc)/SUM(ia.ia_vacas_ord)),0) , SUM(m.med_lecfederal) / COUNT(DISTINCT med_fecha), AVG(m.med_lecfederal), "
                                + " SUM(m.med_lecproduc)"
                                + " FROM media m LEFT JOIN inventario_afi ia ON ia.ran_id = m.ran_id AND ia.ia_fecha = m.med_fecha "
                                + " where m.ran_id IN(" + ranNumero + ") AND med_fecha = '" + inicio.AddDays(1).ToString("yyyy-MM-dd") + "'";
            conn.QueryAlimento(query, out dt);
        }
        
        private void PrecioLeche(out DataTable dt)
        {
            string query = "SELECT AVG(T2.Precio) FROM( SELECT  ran_id AS Rancho, MAX(hl_fecha_reg) AS Fecha "
                               + " FROM historico_leche where ran_id IN(" + ranNumero + ") GROUP BY ran_id) T1 "
                               + " LEFT JOIN( "
                               + " SELECT ran_id AS Rancho, hl_fecha_reg AS Fecha, hl_precio AS Precio "
                               + " FROM historico_leche "
                               + " WHERE ran_id IN(" + ranNumero + "))T2 ON T1.Fecha = T2.Fecha AND T1.Rancho = T2.Rancho";
            conn.QueryAlimento(query, out dt);
        }

        private void TotalAnimales(string etapa, DateTime inicio, DateTime fin, out DataTable dt)
        {
            string campo = "";
            switch (etapa)
            {
                case "10,11,12,13": campo = "ia_vacas_ord"; break;
                case "21": campo = "ia_vacas_secas"; break;
                case "22": campo = "ia_vqreto + ia_vcreto"; break;
                case "31": campo = "ia_jaulas"; break;
                case "32": campo = "ia_destetadas"; break;
                case "33": campo = "ia_destetadas2"; break;
                case "34": campo = "ia_vaquillas"; break;
            }

            string query = "SELECT ISNULL(ROUND(SUM(CONVERT(FLOAT, " + campo + "))/ COUNT(DISTINCT ia_fecha),0),0) FROM inventario_afi "
                            + " WHERE ia_fecha BETWEEN '" + inicio.AddDays(1).ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' "
                            + " AND ran_id IN(" + ranNumero + ")";
            conn.QueryAlimento(query, out dt);
        }

        private void PMS(string etapa, DateTime inicio, DateTime fin, out DataTable dt)
        {
            string query = "SELECT ISNULL(SUM(rac_ms) / SUM(rac_mh) *100,0) AS PorcMS FROM racion "
                                + " WHERE rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                                + " AND ing_descripcion not in ('AGUA', 'WATER') "
                                + " AND ran_id IN("+  ranNumero + " ) AND etp_id IN(" + etapa + ")";
            conn.QueryAlimento(query, out dt);
        }

        private void Utilidad(DateTime inicio, DateTime fin, out DataTable dt)
        {
            string query = " SELECT DATEPART(DAY, T1.Fecha) AS Dia, (T1.LecProduc * T1.PrecLEc) / T2.IT AS IxA, T2.IT "
                + " FROM( "
                + " SELECT med_fecha AS Fecha, SUM(med_lecproduc) AS LecProduc, (SUM(med_precioleche) / COUNT(*)) AS PrecLEc "
                + " FROM media "
                + " where med_fecha between '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' AND ran_id IN(" + ranNumero + ") "
                + " GROUP BY  med_fecha) T1 "
                + " LEFT JOIN( "
                + " SELECT ia_fecha  AS Fecha, SUM(ia_vacas_ord + ia_vacas_secas + ia_vqreto + ia_vcreto + ia_jaulas + ia_destetadas + ia_destetadas2 + ia_vaquillas)  AS IT "
                + " FROM inventario_afi "
                + " where ia_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' AND ran_id IN(" + ranNumero + ") "
                + " GROUP BY ia_fecha "
                + " ) T2 ON T1.Fecha = T2.Fecha";
            conn.QueryAlimento(query, out dt);
        }        

        private void Sum_Prom(DataTable data, double dec1, double dec2, double dec3)
        {
            List<string> columnas = new List<string> { "TOTALVTA", "ORDENO", "SECAS", "HATO", "LECHE", "ANTIB", "TOTALPROD" };
            DataRow rowSum = data.NewRow();
            rowSum["DIA"] = "TOTAL";
            DataRow rowProm = data.NewRow();
            rowProm["DIA"] = "PROM";
            double sumatoria = 0, promedio = 0;
            string colum = "";
            int datos= 0;
            for (int i = 1; i < data.Columns.Count; i++)
            {
                sumatoria = promedio = 0;
                colum = "";
                datos = 0;
                for (int j = 0; j < data.Rows.Count; j++)
                {
                    sumatoria += data.Rows[j][i].ToString() == "" ? 0 : Convert.ToDouble(data.Rows[j][i]);
                    datos += data.Rows[j][i] != DBNull.Value ? 1 : 0;
                }
                promedio = sumatoria / datos;

                colum = columnas.FirstOrDefault(columna => columna == data.Columns[i].ToString());
                if (colum != null || data.Columns[i].ToString().StartsWith("INV"))
                    rowSum[i] = sumatoria;
                rowProm[i] = promedio > 0 ? promedio :0;

                if (data.Columns[i].ToString() == "DEC")
                {
                    if (dias < 11)
                        rowProm[i] = dec1;
                    else if (dias < 21)
                        rowProm[i] = (dec1 + dec2) / 2;
                    else
                        rowProm[i] = (dec1 + dec2 + dec3) / 3;
                }
            }

            for(int i = 1; i <  data.Columns.Count; i++)
            {
                if(rowProm[i] != DBNull.Value)
                    if (Convert.ToDouble(rowProm[i]) == 0)
                        rowProm[i] = DBNull.Value;
            }

            data.Rows.Add(rowSum);
            data.Rows.Add(rowProm);
        }        
      
        private void AddColumnsProduccion(out DataTable dt)
        {
            dt = new DataTable();
            //VENTA
            dt.Columns.Add("DIA").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("TOTALVTA").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("ORDENO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("SECAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("HATO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCLACT").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCPROT").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("UREA").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCGRA").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("CCS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("CTD").DataType = System.Type.GetType("System.Double");
            //PROD
            dt.Columns.Add("LECHE").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("ANTIB").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("X").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("TOTALPROD").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("DEL").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("ANT").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("METAPROD").DataType = System.Type.GetType("System.Double");
            //VENTA
            dt.Columns.Add("ILCAVTA").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("ICVTA").DataType = System.Type.GetType("System.Double");
            //ALIMENTACION PRODUCCION
            dt.Columns.Add("EA").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("ILCAPROD").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("ICPROD").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIOL").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MH").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCMS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("SA").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MSS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("EAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIOPROD").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIOMS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("METAMS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("DEC").DataType = System.Type.GetType("System.Double");
        }

        private void AddColumnsEtapas(out DataTable dt)
        {
            dt = new DataTable();
            dt.Columns.Add("DIA").DataType = System.Type.GetType("System.String");
            //Jaulas
            dt.Columns.Add("INV").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("PRECIOJ").DataType = System.Type.GetType("System.Double");
            // 2/7
            dt.Columns.Add("INV2").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("MH2").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIO2").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCMS2").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MS2").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIOMS2").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("METABEC1").DataType = System.Type.GetType("System.Double");
            // 7/13
            dt.Columns.Add("INV7").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("MH7").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIO7").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCMS7").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MS7").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIOMS7").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("METABEC2").DataType = System.Type.GetType("System.Double");
            // 13 a Mas
            dt.Columns.Add("INV13").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("MH13").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIO13").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCMS13").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MS13").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIOMS13").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("METAVP").DataType = System.Type.GetType("System.Double");
            //SEcas
            dt.Columns.Add("INVSECAS").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("MHSECAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCMSSECAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MSSECAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("SASECAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MSSSECAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCSSECAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIOSECAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIOMSSECAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("METASECAS").DataType = System.Type.GetType("System.Double");
            //Reto
            dt.Columns.Add("INVRETO").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("MHRETO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCMSRETO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MSRETO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("SARETO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("MSSRETO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCSRETO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIORETO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIOMSRETO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("METARETO").DataType = System.Type.GetType("System.Double");
            //Utilidad Por Animal
            dt.Columns.Add("IXA").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("CXA").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCENTAJE1").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("IT").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("UXA").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PORCENTAJE2").DataType = System.Type.GetType("System.Double");
        }

        private DateTime MaxDate()
        {
            DateTime fecha;
            DataTable dt;
            string query = "SELECT CONVERT(date, MAX(r.rac_fecha)) AS Fecha FROM racion r "
                + " LEFT JOIN[DBSIO].[dbo].configuracion c ON c.ran_id = r.ran_id WHERE c.emp_id = " + emp_id.ToString();
            conn.QueryAlimento(query, out dt);

            fecha = Convert.ToDateTime(dt.Rows[0][0]);
            return fecha;
        }

        private DateTime MinDate()
        {
            DateTime fecha;
            DataTable dt;
            string query = "SELECT MAX(T.Fecha) FROM( SELECT r.ran_id AS Rancho, CONVERT(date, MIN(r.rac_fecha)) AS Fecha FROM racion r "
                    + " LEFT JOIN[DBSIO].[dbo].configuracion c ON c.ran_id = r.ran_id WHERE c.emp_id = " + emp_id.ToString()
                    + " GROUP BY r.ran_id) T";
            conn.QueryAlimento(query, out dt);

            fecha = Convert.ToDateTime(dt.Rows[0][0]);
            return fecha;
        }

        private void ColumnasDTRAux(out DataTable dt)
        {
            dt = new DataTable();
            dt.Columns.Add("ingrediente").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("precioIng").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("xvaca").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("porcvaca").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("TOTAL").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("COSTO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("porccosto").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRECIO").DataType = System.Type.GetType("System.Double");
        }

        private void ColumnasDTIAux(out DataTable dt)
        {
            dt = new DataTable();
            dt.Columns.Add("Dia").DataType = System.Type.GetType("System.Int32");
            dt.Columns.Add("Animales").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("media").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("ilcavta").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("icventa").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("eaprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("ilcaprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("icprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("preclprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("mhprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("porcmsprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("msprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("saprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("mssprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("easprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("precprod").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("precmsprod").DataType = System.Type.GetType("System.Double");
        }
        
        private void FillDTFinal(int dias, DataTable dtIndicadores, DataTable dtIL, out DataTable dtFinal)
        {
            AddColumnsProduccion(out dtFinal);
            if(dtIndicadores.Rows.Count == 0 || dtIL.Rows.Count == 0)
            {
                for (int i = 0; i < dias; i++)
                {
                    DataRow dr = dtFinal.NewRow();
                    //Venta
                    dr["DIA"] = i + 1;
                    dr["TOTALVTA"] = 0;
                    dr["ORDENO"] = 0;
                    dr["SECAS"] = 0;
                    dr["HATO"] = 0;
                    dr["PORCLACT"] = 0;
                    dr["PORCPROT"] = 0;
                    dr["UREA"] = 0;
                    dr["PORCGRA"] = 0;
                    dr["CCS"] = 0;
                    dr["CTD"] = 0;
                    //PROD
                    dr["LECHE"] = 0;
                    dr["ANTIB"] = 0;
                    dr["X"] = 0;
                    dr["TOTALPROD"] = 0;
                    dr["DEL"] = 0;
                    dr["ANT"] = 0;
                    //Venta
                    dr["ILCAVTA"] = 0;
                    dr["ICVTA"] = 0;
                    //Alimentacion Produccion
                    dr["EA"] = 0;
                    dr["ILCAPROD"] = 0;
                    dr["ICPROD"] = 0;
                    dr["PRECIOL"] = 0;
                    dr["MH"] = 0;
                    dr["PORCMS"] = 0;
                    dr["MS"] = 0;
                    dr["SA"] = 0;
                    dr["MSS"] = 0;
                    dr["EAS"] = 0;
                    dr["PORCS"] = 0;
                    dr["PRECIOPROD"] = 0;
                    dr["PRECIOMS"] = 0;
                    
                    dtFinal.Rows.Add(dr);
                    
                }
            }
            else if (dtIndicadores.Rows.Count == dtIL.Rows.Count)
            {
                for (int i = 0; i < dtIL.Rows.Count; i++)
                {
                    double sa = Convert.ToDouble(dtIndicadores.Rows[i][12]);
                    double mh = Convert.ToDouble(dtIndicadores.Rows[i][9]);
                    double leche = Convert.ToDouble(dtIL.Rows[i][14]);
                    double ord = Convert.ToDouble(dtIL.Rows[i][2]);
                    DataRow dr = dtFinal.NewRow();
                    //Venta
                    dr["DIA"] = dtIL.Rows[i][0].ToString();
                    dr["TOTALVTA"] = dtIL.Rows[i][1] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][1]) : 0;
                    dr["ORDENO"] = dtIL.Rows[i][2] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][2]): 0;
                    dr["SECAS"] = dtIL.Rows[i][3] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][3]):0;
                    dr["HATO"] = dtIL.Rows[i][4] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][4]) :0;
                    dr["PORCLACT"] = dtIL.Rows[i][5] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][5]) : 0;
                    dr["PORCPROT"] = dtIL.Rows[i][6] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][6]) : 0;
                    dr["UREA"] = dtIL.Rows[i][7] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][7]) : 0;
                    dr["PORCGRA"] = dtIL.Rows[i][8] != DBNull.Value ?  Convert.ToDouble(dtIL.Rows[i][8]) :0;
                    dr["CCS"] = dtIL.Rows[i][9] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][9]) : 0;
                    dr["CTD"] = dtIL.Rows[i][10] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][10]) :0;
                    //PROD
                    dr["LECHE"] = dtIL.Rows[i][11] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][11]) : 0;
                    dr["ANTIB"] = dtIL.Rows[i][12] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][12]) : 0;
                    dr["X"] = ord > 0 ? leche / ord :0;
                    dr["TOTALPROD"] = dtIL.Rows[i][14] != DBNull.Value ?  Convert.ToDouble(dtIL.Rows[i][14]) :0;
                    dr["DEL"] = dtIL.Rows[i][15] != DBNull.Value ?  Convert.ToDouble(dtIL.Rows[i][15]) :0;
                    dr["ANT"] = dtIL.Rows[i][16] != DBNull.Value ? Convert.ToDouble(dtIL.Rows[i][16]) : 0;
                    //Venta
                    dr["ILCAVTA"] = dtIndicadores.Rows[i][3] != DBNull.Value ? Convert.ToDouble(dtIndicadores.Rows[i][3]) : 0;
                    dr["ICVTA"] = dtIndicadores.Rows[i][4] != DBNull.Value ? Convert.ToDouble(dtIndicadores.Rows[i][4]):0;
                    //Alimentacion Produccion
                    dr["EA"] = dtIndicadores.Rows[i][5] != DBNull.Value ?  Convert.ToDouble(dtIndicadores.Rows[i][5]) : 0;
                    dr["ILCAPROD"] = dtIndicadores.Rows[i][6] != DBNull.Value ?  Convert.ToDouble(dtIndicadores.Rows[i][6]) :0;
                    dr["ICPROD"] = dtIndicadores.Rows[i][7] != DBNull.Value ? Convert.ToDouble(dtIndicadores.Rows[i][7]) : 0;
                    dr["PRECIOL"] = dtIndicadores.Rows[i][8] != DBNull.Value ?  Convert.ToDouble(dtIndicadores.Rows[i][8]) : 0;
                    dr["MH"] = dtIndicadores.Rows[i][9] != DBNull.Value ?  Convert.ToDouble(dtIndicadores.Rows[i][9]) : 0;
                    dr["PORCMS"] = dtIndicadores.Rows[i][10] != DBNull.Value ?  Convert.ToDouble(dtIndicadores.Rows[i][10]) : 0;
                    dr["MS"] = dtIndicadores.Rows[i][11] != DBNull.Value ?  Convert.ToDouble(dtIndicadores.Rows[i][11]) :0;
                    dr["SA"] = dtIndicadores.Rows[i][12] != DBNull.Value ?  Convert.ToDouble(dtIndicadores.Rows[i][12]) : 0;
                    dr["MSS"] = dtIndicadores.Rows[i][13] != DBNull.Value ?  Convert.ToDouble(dtIndicadores.Rows[i][13]) :0;
                    dr["EAS"] = dtIndicadores.Rows[i][14] != DBNull.Value ?  Convert.ToDouble(dtIndicadores.Rows[i][14]) :0 ;
                    dr["PORCS"] = sa >  0 && mh > 0 ? Convert.ToDouble(sa / mh * 100) : 0;
                    dr["PRECIOPROD"] = dtIndicadores.Rows[i][15] != DBNull.Value ?  Convert.ToDouble(dtIndicadores.Rows[i][15])> 0 ? Convert.ToDouble(dtIndicadores.Rows[i][15]) : 0 : 0;
                    dr["PRECIOMS"] = dtIndicadores.Rows[i][16] != DBNull.Value ?  Convert.ToDouble(dtIndicadores.Rows[i][16]): 0;

                    dtFinal.Rows.Add(dr);
                }
            }
        }

        private void AddRow(int dias, DataTable dt)
        {
            if (dt.Rows.Count < 32)
            {
                int index;
                ArrayList faltantes = new ArrayList();
                ArrayList list2 = new ArrayList();
                for (int i = 0; i < 31; i++)
                {
                    faltantes.Add(i + 1);
                }

                int dia;

                if (dt.Rows.Count != 31)
                {
                    if (dt.Rows.Count == 0)
                    {
                        FillEmptyDT(dias, dt);
                    }
                    else
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dia = Convert.ToInt32(dt.Rows[i][0]);
                            index = faltantes.IndexOf(dia);
                            faltantes.RemoveAt(index);
                        }

                        if (dt.Rows.Count < 31)
                        {
                            int max = Convert.ToInt32(dt.Rows[dt.Rows.Count - 1][0]);
                            for (int i = 0; i < faltantes.Count; i++)
                            {
                                dia = Convert.ToInt32(faltantes[i]);
                                if (dia > max)
                                {
                                    list2.Add(dia);
                                }
                            }
                            for (int i = 0; i < list2.Count; i++)
                            {
                                faltantes.Remove(list2[i]);
                            }
                        }

                        int posicion;
                        for (int i = 0; i < faltantes.Count; i++)
                        {
                            DataRow row = dt.NewRow();
                            posicion = Convert.ToInt32(faltantes[i]);
                            row[0] = posicion;
                            for (int j = 1; j < dt.Columns.Count; j++)
                            {
                                row[j] = 0;
                            }
                            dt.Rows.InsertAt(row, posicion - 1);
                        }
                    }
                }
            }
           
        }

        private void FillEmptyDT(int dias, DataTable dt)
        {
            for(int i = 0; i < dias; i++)
            {
                DataRow dr = dt.NewRow();
                dr[0] = i + 1;
                for(int j =1; j < dt.Columns.Count; j++)
                {
                    dr[j] = 0;
                }
                dt.Rows.Add(dr);
            }
        }

    }
}
