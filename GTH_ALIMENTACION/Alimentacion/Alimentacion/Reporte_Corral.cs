using Microsoft.Reporting.WinForms;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alimentacion
{
    public partial class Reporte_Corral : Form
    {
        ConnSIO conn = new ConnSIO();
        int ran_id, emp_id, ran_pesadores;
        string ran_nombre, emp_nombre, ranNumero, ranCadena;
        List<string> listaCorrales;
        string ruta;

        private void textBox1_Click(object sender, EventArgs e)
        {
            int rancho = ran_id;
            Corral cor = new Corral(rancho);

            if (cor.ShowDialog() == DialogResult.OK)
            {
                txtCorral.Text = cor.textoCorrales;
                listaCorrales = cor.corrales;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Checked = !checkBox2.Checked;
                checkBox3.Checked = !checkBox2.Checked;
            }

            txtCorral.Enabled = checkBox2.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Checked = !checkBox1.Checked;
                checkBox3.Checked = !checkBox1.Checked;
                txtCorral.Text = "";
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                checkBox1.Checked = !checkBox3.Checked;
                checkBox2.Checked = !checkBox3.Checked;
            }
            txtCorral.Text = ";";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            InfoEstablo();
            DataTable dt = new DataTable();
            string query = "";
            DateTime inicio = dateTimePicker1.Value.Date;
            DateTime fin = dateTimePicker2.Value.Date;
            string rancb = ran_id.ToString();


            if (checkBox1.Checked)
            {
                query = "SELECT T.FECHA, T.LAC1 AS LACT1, T.LAC2 AS LACT2, T.LAC3 AS LACT3, T.TOTAL , T.PROD_TOTAL, T.SI, T.NO, T.SES1 AS PROD_SES1, T.SES2 AS PROD_SES2, "
                        + " T.SES3 AS PROD_SES3,  T.PSES1 AS PROM_SES1, T.PSES2 AS PROM_SES2, T.PSES3 AS PROM_SES3, T.X, T.DEL, IIF(T.TOTAL > 0, T.MH / T.TOTAL, 0) AS MH, "
                        + " IIF(T.TOTAL > 0, T.MS / T.TOTAL, 0) AS MS, IIF(T.MS > 0 AND T.TOTAL > 0, T.X / (T.MS / T.TOTAL), 0) AS EFICIENCIA, 0.0 AS ILCA, 0.0 AS IC, T.TEMP "
                        + " FROM( "
                        + " SELECT FORMAT(R.FECHA, 'd', 'en-gb') AS FECHA, SUM(R.Lac1) AS LAC1, SUM(R.Lac2) AS LAC2, SUM(R.Lac3) AS LAC3, SUM(R.TOTAL) AS TOTAL, "
                        + " SUM(R.PRODTOTAL) AS PROD_TOTAL, SUM(R.SI) AS SI, SUM(R.NO) AS NO, SUM(R.Ses1) AS SES1, SUM(R.Ses3) AS SES2, SUM(R.Ses3) AS SES3, "
                        + " SUM(R.PSes1) / COUNT(*) AS PSES1, SUM(R.PSes2) / COUNT(*) AS PSES2, SUM(R.PSes3) / COUNT(*) AS PSES3, SUM(R.X) / COUNT(*) AS X, "
                        + " ROUND(SUM(R.DEL) / COUNT(*), 0) AS DEL, SUM(R.MH) AS MH, SUM(R.MS) AS MS, IIF(SUM(R.CorralTemp) > 0, SUM(R.TEMP) / SUM(R.CorralTemp), 0) AS TEMP "
                        + " FROM( "
                        + " SELECT  T.RANCHO, T.CORRAL, T.FECHA, T.Lac1, T.Lac2, T.Lac3, T.TOTAL, T.PRODTOTAL, IIF((T.CId1 + T.CId2 + T.CId3) > 0, (T.Id1 + T.Id2 + T.Id3) / (T.CId1 + T.CId2 + T.CId3), 0) AS SI, "
                        + " IIF((T.CNId1 + T.CNId2 + T.CNId3) > 0, (T.NId1 + T.NId2 + T.NId3) / (T.CNId1 + T.CNId2 + T.CNId3), 0) AS NO, "
                        + " T.Ses1, T.Ses2, T.Ses3, T.PSes1, T.PSes2, T.PSes3, T.X, T.DEL, T.MS, T.MH, T.TEMP, IIF(T.TEMP > 0, 1, 0) AS CorralTemp "
                        + " FROM( "
                        + " SELECT DISTINCT c.ran_id As RANCHO, c.cor_id AS CORRAL, pc.pc_fecha AS FECHA, pc.pc_vaclac1 AS Lac1, pc.pc_vaclac2 AS Lac2, pc.pc_vaclac3 AS Lac3, "
                        + " (pc.pc_vaclac1 + pc.pc_vaclac2 + pc.pc_vaclac3) AS TOTAL, pc.pc_prodtotal AS PRODTOTAL, pc.pc_idcorral1 AS Id1, pc.pc_idcorral2 AS Id2, "
                        + " pc.pc_idcorral2 AS Id3, IIF(pc.pc_idcorral1 > 0, 1, 0) AS CId1, IIF(pc.pc_idcorral2 > 0, 1, 0) AS CId2, IIF(pc.pc_idcorral3 > 0, 1, 0) AS CId3, "
                        + " pc.pc_noidcorral1 AS NId1, pc.pc_noidcorral2 AS NId2, pc.pc_noidcorral3 AS NId3, IIF(pc.pc_noidcorral1 > 0, 1, 0) AS CNId1, IIF(pc.pc_noidcorral2 > 0, 1, 0) AS CNId2, "
                        + " IIF(pc.pc_noidcorral3 > 0, 1, 0) AS CNId3, pc.pc_prodses1 AS Ses1, pc.pc_prodses2 AS Ses2, pc.pc_prodses3 AS Ses3, "
                        + " pc.pc_promses1 AS PSes1, pc.pc_promses2 AS PSes2, pc.pc_promses3 AS PSes3, IIF(pc.pc_prodses1 > 0, 1, 0) AS CSes1, IIF(pc.pc_prodses2 > 0, 1, 0) AS CSes2, "
                        + " IIF(pc.pc_prodses3 > 0, 1, 0) AS CSes3, pc.pc_promses1 + pc.pc_promses2 + pc.pc_promses3 AS X, pc.pc_del AS DEL, ISNULL(cc.cc_ms, 0) AS MS, ISNULL(cc.cc_mh, 0) AS MH, ISNULL(tc.temperatura, 0) AS TEMP "
                        + " from corral c "
                        + " LEFT JOIN produccion_corral pc ON pc.pc_corral = c.cor_id AND pc.ran_id = c.ran_id "
                        + " LEFT JOIN consumo_corral cc ON cc.cc_corral = c.cor_id AND cc.ran_id = c.ran_id AND cc.cc_fecha = pc.pc_fecha "
                        + " LEFT JOIN TEMPCORRALES tc ON Tc.ran_clave = cc.ran_id AND tc.Fecha = cc.cc_fecha AND cc.cc_corral = tc.corral "
                        + " WHERE c.ran_id = " + rancb + " AND pc.pc_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' "
                        + " ) T "
                        + " ) R GROUP BY R.FECHA "
                        + " ) T ORDER BY 1";
                //query = "SELECT T.FECHA, T.LAC1 AS LACT1, T.LAC2 AS LACT2, T.LAC3 AS LACT3, T.TOTAL , T.PROD_TOTAL, T.SI, T.NO, T.SES1 AS PROD_SES1, T.SES2 AS PROD_SES2, "
                //    + " T.SES3 AS PROD_SES3,  T.PSES1 AS PROM_SES1, T.PSES2 AS PROM_SES2, T.PSES3 AS PROM_SES3, T.X, T.DEL, IIF(T.TOTAL > 0, T.MH / T.TOTAL, 0) AS MH, "
                //    + " IIF(T.TOTAL > 0, T.MS / T.TOTAL, 0) AS MS, IIF(T.MS > 0 AND T.TOTAL > 0, T.X / (T.MS / T.TOTAL), 0) AS EFICIENCIA, 0.0 AS ILCA, 0.0 AS IC, T.TEMP "
                //    + " FROM( "
                //    + " SELECT FORMAT(R.FECHA, 'd', 'en-gb') AS FECHA, SUM(R.Lac1) AS LAC1, SUM(R.Lac2) AS LAC2, SUM(R.Lac3) AS LAC3, SUM(R.TOTAL) AS TOTAL, "
                //    + " SUM(R.PRODTOTAL) AS PROD_TOTAL, SUM(R.SI) AS SI, SUM(R.NO) AS NO, SUM(R.Ses1) AS SES1, SUM(R.Ses3) AS SES2, SUM(R.Ses3) AS SES3, "
                //    + " SUM(R.PSes1) / COUNT(*) AS PSES1, SUM(R.PSes2) / COUNT(*) AS PSES2, SUM(R.PSes3) / COUNT(*) AS PSES3, SUM(R.X) / COUNT(*) AS X, "
                //    + " ROUND(SUM(R.DEL) / COUNT(*), 0) AS DEL, SUM(R.MH) AS MH, SUM(R.MS) AS MS, AVG(R.TEMP) AS TEMP "
                //    + " FROM( "
                //    + " SELECT  T.RANCHO, T.CORRAL, T.FECHA, T.Lac1, T.Lac2, T.Lac3, T.TOTAL, T.PRODTOTAL, (T.Id1 + T.Id2 + T.Id3) / (T.CId1 + T.CId2 + T.CId3) AS SI, "
                //    + " IIF((T.CNId1 + T.CNId2 + T.CNId3) > 0, (T.NId1 + T.NId2 + T.NId3) / (T.CNId1 + T.CNId2 + T.CNId3), 0) AS NO, "
                //    + " T.Ses1, T.Ses2, T.Ses3, T.PSes1, T.PSes2, T.PSes3, T.X, T.DEL, T.MS, T.MH, T.TEMP "
                //    + " FROM( "
                //    + " SELECT DISTINCT pc.ran_id As RANCHO, cc.cc_corral AS CORRAL, cc.cc_fecha AS FECHA, pc.pc_vaclac1 AS Lac1, pc.pc_vaclac2 AS Lac2, pc.pc_vaclac3 AS Lac3, "
                //    + " (pc.pc_vaclac1 + pc.pc_vaclac2 + pc.pc_vaclac3) AS TOTAL, pc.pc_prodtotal AS PRODTOTAL, pc.pc_idcorral1 AS Id1, pc.pc_idcorral2 AS Id2, pc.pc_idcorral2 AS Id3, "
                //    + " IIF(pc.pc_idcorral1 > 0, 1, 0) AS CId1, IIF(pc.pc_idcorral2 > 0, 1, 0) AS CId2, IIF(pc.pc_idcorral3 > 0, 1, 0) AS CId3, pc.pc_noidcorral1 AS NId1, "
                //    + " pc.pc_noidcorral2 AS NId2, pc.pc_noidcorral3 AS NId3, IIF(pc.pc_noidcorral1 > 0, 1, 0) AS CNId1, IIF(pc.pc_noidcorral2 > 0, 1, 0) AS CNId2, "
                //    + " IIF(pc.pc_noidcorral3 > 0, 1, 0) AS CNId3, pc.pc_prodses1 AS Ses1, pc.pc_prodses2 AS Ses2, pc.pc_prodses3 AS Ses3, pc.pc_promses1 AS PSes1, "
                //    + " pc.pc_promses2 AS PSes2, pc.pc_promses3 AS PSes3, IIF(pc.pc_prodses1 > 0, 1, 0) AS CSes1, IIF(pc.pc_prodses2 > 0, 1, 0) AS CSes2, "
                //    + " IIF(pc.pc_prodses3 > 0, 1, 0) AS CSes3, pc.pc_promses1 + pc.pc_promses2 + pc.pc_promses3 AS X, pc.pc_del AS DEL, ISNULL(cc.cc_ms, 0) AS MS, "
                //    + " ISNULL(cc.cc_mh, 0) AS MH, tc.temperatura AS TEMP "
                //    + " from consumo_corral cc "
                //    + " LEFT JOIN produccion_corral pc ON cc.cc_corral = pc.pc_corral AND cc.ran_id = pc.ran_id AND cc.cc_fecha = pc.pc_fecha "
                //    + " LEFT JOIN TEMPCORRALES tc ON Tc.ran_clave = cc.ran_id AND tc.Fecha = cc.cc_fecha AND cc.cc_corral = tc.corral "
                //    + " WHERE cc.ran_id = 19 AND pc.ran_id is not null  AND cc.cc_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' "
                //    + " ) T  ) R GROUP BY R.FECHA ) T ORDER BY 1 ";
          
                conn.QueryAlimento(query, out dt);

                Indicadores(dt);
                Diferencia(dt);
                Promedio(dt, inicio, fin);
                if(ran_pesadores == 1)
                {
                    ReportDataSource source1 = new ReportDataSource("DataSet1", dt);
                    reportViewer1.LocalReport.DataSources.Clear();
                    reportViewer1.LocalReport.DataSources.Add(source1);

                    ReportParameter[] parametros = new ReportParameter[2];
                    parametros[0] = new ReportParameter("EMPRESA", ran_nombre);
                    parametros[1] = new ReportParameter("CORRAL", "Todos");
                    reportViewer1.LocalReport.SetParameters(parametros);

                    reportViewer1.LocalReport.Refresh();
                    reportViewer1.RefreshReport();
                    GTHUtils.SavePDF(reportViewer1, ruta + "REPORTE CORRAL.pdf");
                }
                else
                {
                    ReportDataSource source1 = new ReportDataSource("DataSet1", dt);
                    reportViewer6.LocalReport.DataSources.Clear();
                    reportViewer6.LocalReport.DataSources.Add(source1);

                    ReportParameter[] parametros = new ReportParameter[2];
                    parametros[0] = new ReportParameter("EMPRESA", ran_nombre);
                    parametros[1] = new ReportParameter("CORRAL", "Todos");
                    reportViewer6.LocalReport.SetParameters(parametros);

                    reportViewer6.LocalReport.Refresh();
                    reportViewer6.RefreshReport();
                    GTHUtils.SavePDF(reportViewer6, ruta + "REPORTE CORRAL.pdf");
                }
                Process.Start(ruta + "REPORTE CORRAL.pdf");
            }

            if (checkBox2.Checked)
            {
                string corral = txtCorral.Text;
                string[] corrales = corral.Split(',');
                DataTable[] dtCorrales = new DataTable[corrales.Length];

                for (int i = 0; i < corrales.Length; i++)
                {
                    query = "SELECT T.FECHA, T.LAC1 AS LACT1, T.LAC2 AS LACT2, T.LAC3 AS LACT3, T.TOTAL , T.PROD_TOTAL, T.SI, T.NO, T.SES1 AS PROD_SES1, T.SES2 AS PROD_SES2, "
                        + " T.SES3 AS PROD_SES3,  T.PSES1 AS PROM_SES1, T.PSES2 AS PROM_SES2, T.PSES3 AS PROM_SES3, T.X, T.DEL, IIF(T.TOTAL > 0, T.MH / T.TOTAL, 0) AS MH, "
                        + " IIF(T.TOTAL > 0, T.MS / T.TOTAL, 0) AS MS, IIF(T.MS > 0 AND T.TOTAL > 0, T.X / (T.MS / T.TOTAL), 0) AS EFICIENCIA, 0.0 AS ILCA, 0.0 AS IC, T.TEMP "
                        + " FROM( "
                        + " SELECT FORMAT(R.FECHA, 'd', 'en-gb') AS FECHA, SUM(R.Lac1) AS LAC1, SUM(R.Lac2) AS LAC2, SUM(R.Lac3) AS LAC3, SUM(R.TOTAL) AS TOTAL, "
                        + " SUM(R.PRODTOTAL) AS PROD_TOTAL, SUM(R.SI) AS SI, SUM(R.NO) AS NO, SUM(R.Ses1) AS SES1, SUM(R.Ses3) AS SES2, SUM(R.Ses3) AS SES3, "
                        + " SUM(R.PSes1) / COUNT(*) AS PSES1, SUM(R.PSes2) / COUNT(*) AS PSES2, SUM(R.PSes3) / COUNT(*) AS PSES3, SUM(R.X) / COUNT(*) AS X, "
                        + " ROUND(SUM(R.DEL) / COUNT(*), 0) AS DEL, SUM(R.MH) AS MH, SUM(R.MS) AS MS, IIF(SUM(R.CorralTemp) > 0, SUM(R.TEMP) / SUM(R.CorralTemp), 0) AS TEMP "
                        + " FROM( "
                        + " SELECT  T.RANCHO, T.CORRAL, T.FECHA, T.Lac1, T.Lac2, T.Lac3, T.TOTAL, T.PRODTOTAL, IIF((T.CId1 + T.CId2 + T.CId3) > 0, (T.Id1 + T.Id2 + T.Id3) / (T.CId1 + T.CId2 + T.CId3), 0) AS SI, "
                        + " IIF((T.CNId1 + T.CNId2 + T.CNId3) > 0, (T.NId1 + T.NId2 + T.NId3) / (T.CNId1 + T.CNId2 + T.CNId3), 0) AS NO, "
                        + " T.Ses1, T.Ses2, T.Ses3, T.PSes1, T.PSes2, T.PSes3, T.X, T.DEL, T.MS, T.MH, T.TEMP, IIF(T.TEMP > 0, 1, 0) AS CorralTemp "
                        + " FROM( "
                        + " SELECT DISTINCT c.ran_id As RANCHO, c.cor_id AS CORRAL, pc.pc_fecha AS FECHA, pc.pc_vaclac1 AS Lac1, pc.pc_vaclac2 AS Lac2, pc.pc_vaclac3 AS Lac3, "
                        + " (pc.pc_vaclac1 + pc.pc_vaclac2 + pc.pc_vaclac3) AS TOTAL, pc.pc_prodtotal AS PRODTOTAL, pc.pc_idcorral1 AS Id1, pc.pc_idcorral2 AS Id2, "
                        + " pc.pc_idcorral2 AS Id3, IIF(pc.pc_idcorral1 > 0, 1, 0) AS CId1, IIF(pc.pc_idcorral2 > 0, 1, 0) AS CId2, IIF(pc.pc_idcorral3 > 0, 1, 0) AS CId3, "
                        + " pc.pc_noidcorral1 AS NId1, pc.pc_noidcorral2 AS NId2, pc.pc_noidcorral3 AS NId3, IIF(pc.pc_noidcorral1 > 0, 1, 0) AS CNId1, IIF(pc.pc_noidcorral2 > 0, 1, 0) AS CNId2, "
                        + " IIF(pc.pc_noidcorral3 > 0, 1, 0) AS CNId3, pc.pc_prodses1 AS Ses1, pc.pc_prodses2 AS Ses2, pc.pc_prodses3 AS Ses3, "
                        + " pc.pc_promses1 AS PSes1, pc.pc_promses2 AS PSes2, pc.pc_promses3 AS PSes3, IIF(pc.pc_prodses1 > 0, 1, 0) AS CSes1, IIF(pc.pc_prodses2 > 0, 1, 0) AS CSes2, "
                        + " IIF(pc.pc_prodses3 > 0, 1, 0) AS CSes3, pc.pc_promses1 + pc.pc_promses2 + pc.pc_promses3 AS X, pc.pc_del AS DEL, ISNULL(cc.cc_ms, 0) AS MS, ISNULL(cc.cc_mh, 0) AS MH, ISNULL(tc.temperatura, 0) AS TEMP "
                        + " from corral c "
                        + " LEFT JOIN produccion_corral pc ON pc.pc_corral = c.cor_id AND pc.ran_id = c.ran_id "
                        + " LEFT JOIN consumo_corral cc ON cc.cc_corral = c.cor_id AND cc.ran_id = c.ran_id AND cc.cc_fecha = pc.pc_fecha "
                        + " LEFT JOIN TEMPCORRALES tc ON Tc.ran_clave = cc.ran_id AND tc.Fecha = cc.cc_fecha AND cc.cc_corral = tc.corral "
                        + " WHERE c.ran_id = " + rancb + " AND pc.pc_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' "
                        + " AND c.cor_id = " + corrales[i].ToString()
                        + " ) T "
                        + " ) R GROUP BY R.FECHA "
                        + " ) T ORDER BY 1"; 

                    //query = "SELECT T.FECHA, T.LAC1 AS LACT1, T.LAC2 AS LACT2, T.LAC3 AS LACT3, T.TOTAL , T.PROD_TOTAL, T.SI, T.NO, T.SES1 AS PROD_SES1, T.SES2 AS PROD_SES2, T.SES3 AS PROD_SES3, "
                    //+ " T.PSES1 AS PROM_SES1, T.PSES2 AS PROM_SES2, T.PSES3 AS PROM_SES3, T.X, T.DEL, IIF(T.TOTAL >  0 ,T.MH / T.TOTAL,0) AS MH, " 
                    //+ " IIF(T.TOTAL >  0, T.MS / T.TOTAL,0) AS MS, IIF(T.MS > 0 AND T.TOTAL > 0, T.X / (T.MS / T.TOTAL),0) AS EFICIENCIA, T.TEMP, '' AS ILCA, '' AS IC"
                    //+ " FROM( "
                    //+ " SELECT FORMAT(R.FECHA, 'd', 'en-gb') AS FECHA, SUM(R.Lac1) AS LAC1, SUM(R.Lac2) AS LAC2, SUM(R.Lac3) AS LAC3, SUM(R.TOTAL) AS TOTAL, SUM(R.PRODTOTAL) AS PROD_TOTAL, "
                    //+ " SUM(R.SI) AS SI, SUM(R.NO) AS NO, SUM(R.Ses1) AS SES1, SUM(R.Ses3) AS SES2, SUM(R.Ses3) AS SES3, SUM(R.PSes1) / COUNT(*) AS PSES1, SUM(R.PSes2) / COUNT(*) AS PSES2, "
                    //+ " SUM(R.PSes3) / COUNT(*) AS PSES3, SUM(R.X) / COUNT(*) AS X, ROUND(SUM(R.DEL) / COUNT(*), 0) AS DEL, SUM(R.MH) AS MH, SUM(R.MS) AS MS, ISNULL(AVG(tc.temperatura),0) AS TEMP "
                    //+ " FROM( "
                    //+ " SELECT  T.RANCHO, T.CORRAL, T.FECHA, T.Lac1, T.Lac2, T.Lac3, T.TOTAL, T.PRODTOTAL, "
                    //+ " (T.Id1 + T.Id2 + T.Id3) / (T.CId1 + T.CId2 + T.CId3) AS SI, IIF((T.CNId1 + T.CNId2 + T.CNId3) > 0, (T.NId1 + T.NId2 + T.NId3) / (T.CNId1 + T.CNId2 + T.CNId3), 0) AS NO, "
                    //+ " T.Ses1, T.Ses2, T.Ses3, T.PSes1, T.PSes2, T.PSes3, T.X, T.DEL, T.MS, T.MH "
                    //+ " FROM( "
                    //+ " SELECT pc.ran_id As RANCHO, cc.cc_corral AS CORRAL, cc.cc_fecha AS FECHA, pc.pc_vaclac1 AS Lac1, pc.pc_vaclac2 AS Lac2, pc.pc_vaclac3 AS Lac3, "
                    //+ " (pc.pc_vaclac1 + pc.pc_vaclac2 + pc.pc_vaclac3) AS TOTAL, pc.pc_prodtotal AS PRODTOTAL, "
                    //+ " pc.pc_idcorral1 AS Id1, pc.pc_idcorral2 AS Id2, pc.pc_idcorral2 AS Id3, "
                    //+ " IIF(pc.pc_idcorral1 > 0, 1, 0) AS CId1, IIF(pc.pc_idcorral2 > 0, 1, 0) AS CId2, IIF(pc.pc_idcorral3 > 0, 1, 0) AS CId3, "
                    //+ " pc.pc_noidcorral1 AS NId1, pc.pc_noidcorral2 AS NId2, pc.pc_noidcorral3 AS NId3, "
                    //+ " IIF(pc.pc_noidcorral1 > 0, 1, 0) AS CNId1, IIF(pc.pc_noidcorral2 > 0, 1, 0) AS CNId2, IIF(pc.pc_noidcorral3 > 0, 1, 0) AS CNId3, "
                    //+ " pc.pc_prodses1 AS Ses1, pc.pc_prodses2 AS Ses2, pc.pc_prodses3 AS Ses3, "
                    //+ " pc.pc_promses1 AS PSes1, pc.pc_promses2 AS PSes2, pc.pc_promses3 AS PSes3, "
                    //+ " IIF(pc.pc_prodses1 > 0, 1, 0) AS CSes1, IIF(pc.pc_prodses2 > 0, 1, 0) AS CSes2, IIF(pc.pc_prodses3 > 0, 1, 0) AS CSes3, "
                    //+ " pc.pc_promses1 + pc.pc_promses2 + pc.pc_promses3 AS X, pc.pc_del AS DEL, ISNULL(cc.cc_ms, 0) AS MS, ISNULL(cc.cc_mh, 0) AS MH "
                    //+ " from consumo_corral cc "
                    //+ " LEFT JOIN produccion_corral pc ON cc.cc_corral = pc.pc_corral AND cc.ran_id = pc.ran_id AND cc.cc_fecha = pc.pc_fecha "
                    //+ " WHERE cc.ran_id = " + rancb + " AND pc.ran_id is not null AND pc.pc_corral = " + corrales[i].ToString()
                    //+ " AND cc.cc_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "') T ) R  "
                    //+ " LEFT JOIN TEMPCORRALES tc ON R.RANCHO = tc.ran_clave AND R.CORRAL = tc.corral AND R.RANCHO  = tc.ran_clave AND R.FECHA = tc.Fecha "
                    //+ " GROUP BY R.FECHA) T ORDER BY 1 ";
                    conn.QueryAlimento(query, out dtCorrales[i]);
                    Indicadores(dtCorrales[i]);
                    Diferencia(dtCorrales[i]);
                    Promedio(dtCorrales[i], inicio, fin);
                }

                int num_doc = 1;

                GTHUtils.DeleteFile("C:\\MOVGANADO\\consumos\\Reportes\\Auxiliar");

                DataTable promedio = new DataTable();
                promedio.Columns.Add("prod").DataType = System.Type.GetType("System.Double");
                promedio.Columns.Add("del").DataType = System.Type.GetType("System.Double");
                promedio.Columns.Add("efic").DataType = System.Type.GetType("System.Double");

                for (int i = 0; i < dtCorrales.Length; i++)
                {
                    DataRow row = promedio.NewRow();
                    row["prod"] = Convert.ToDouble(dtCorrales[i].Rows[dtCorrales[i].Rows.Count - 1][14].ToString());
                    //row["del"] = Int32.Parse(Math.Floor(Convert.ToDecimal(dtCorrales[i].Rows[dtCorrales[i].Rows.Count - 1][15].ToString())).ToString());
                    row["del"] = Convert.ToDouble(dtCorrales[i].Rows[dtCorrales[i].Rows.Count - 1][15].ToString());
                    row["efic"] = Convert.ToDouble(dtCorrales[i].Rows[dtCorrales[i].Rows.Count - 1][18].ToString());
                    promedio.Rows.Add(row);
                }

                //for (int i = 0; i < dtCorrales.Length - (dtCorrales.Length % 3); i += 3)
                //{
                //    ReportDataSource source1 = new ReportDataSource("DataSet1", dtCorrales[i]);
                //    reportViewer4.LocalReport.DataSources.Clear();
                //    reportViewer4.LocalReport.DataSources.Add(source1);

                //    source1 = new ReportDataSource("DataSet2", dtCorrales[i + 1]);
                //    reportViewer4.LocalReport.DataSources.Add(source1);

                //    source1 = new ReportDataSource("DataSet3", dtCorrales[i + 2]);
                //    reportViewer4.LocalReport.DataSources.Add(source1);

                //    ReportParameter[] parametros = new ReportParameter[4];
                //    parametros[0] = new ReportParameter("EMPRESA", "EMPRESA: ");
                //    parametros[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales[i]);
                //    parametros[2] = new ReportParameter("CORRAL2", "CORRAL: " + corrales[i + 1]);
                //    parametros[3] = new ReportParameter("CORRAL3", "CORRAL: " + corrales[i + 2]);
                //    reportViewer4.LocalReport.SetParameters(parametros);

                //    reportViewer4.LocalReport.Refresh();
                //    reportViewer4.RefreshReport();
                //    GTHUtils.SavePDF(reportViewer4, ruta + "Auxiliar\\" + num_doc + ".pdf");
                //    //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                //    num_doc++;
                //}

                if (ran_pesadores == 1)
                {

                    for (int i = 0; i < dtCorrales.Length - (dtCorrales.Length % 3); i += 3)
                    {
                        ReportDataSource source1 = new ReportDataSource("DataSet1", dtCorrales[i]);
                        reportViewer4.LocalReport.DataSources.Clear();
                        reportViewer4.LocalReport.DataSources.Add(source1);

                        source1 = new ReportDataSource("DataSet2", dtCorrales[i + 1]);
                        reportViewer4.LocalReport.DataSources.Add(source1);

                        source1 = new ReportDataSource("DataSet3", dtCorrales[i + 2]);
                        reportViewer4.LocalReport.DataSources.Add(source1);

                        ReportParameter[] parametros = new ReportParameter[4];
                        parametros[0] = new ReportParameter("EMPRESA", "EMPRESA: ");
                        parametros[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales[i]);
                        parametros[2] = new ReportParameter("CORRAL2", "CORRAL: " + corrales[i + 1]);
                        parametros[3] = new ReportParameter("CORRAL3", "CORRAL: " + corrales[i + 2]);
                        reportViewer4.LocalReport.SetParameters(parametros);

                        reportViewer4.LocalReport.Refresh();
                        reportViewer4.RefreshReport();
                        GTHUtils.SavePDF(reportViewer4, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;
                    }

                    if (dtCorrales.Length % 3 == 0)
                    {
                        ReportDataSource source1 = new ReportDataSource("DataSet1", promedio);
                        reportViewer5.LocalReport.DataSources.Clear();
                        reportViewer5.LocalReport.DataSources.Add(source1);
                        reportViewer5.RefreshReport();

                        ReportParameter[] parametros = new ReportParameter[1];
                        parametros[0] = new ReportParameter("EMPRESA", "EMPRESA: ");
                        reportViewer5.LocalReport.SetParameters(parametros);

                        reportViewer5.LocalReport.Refresh();
                        reportViewer5.RefreshReport();
                        GTHUtils.SavePDF(reportViewer5, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();
                    }
                    else if (dtCorrales.Length % 3 == 2)
                    {
                        ReportDataSource source = new ReportDataSource("DataSet1", dtCorrales[dtCorrales.Length - 2]);
                        reportViewer3.LocalReport.DataSources.Clear();
                        reportViewer3.LocalReport.DataSources.Add(source);
                        reportViewer3.RefreshReport();
                        source = new ReportDataSource("DataSet2", dtCorrales[dtCorrales.Length - 1]);
                        reportViewer3.LocalReport.DataSources.Add(source);
                        reportViewer3.RefreshReport();
                        ReportDataSource source1 = new ReportDataSource("DataSet3", promedio);
                        reportViewer3.LocalReport.DataSources.Add(source1);
                        reportViewer3.RefreshReport();
                        ReportParameter[] par = new ReportParameter[3];
                        par[0] = new ReportParameter("EMPRESA", "EMPRESA: ");
                        par[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales[dtCorrales.Length - 2]);
                        par[2] = new ReportParameter("CORRAL2", "CORRAL: " + corrales[dtCorrales.Length - 1]);
                        reportViewer3.LocalReport.SetParameters(par);

                        reportViewer3.LocalReport.Refresh();
                        reportViewer3.RefreshReport();
                        GTHUtils.SavePDF(reportViewer3, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();
                    }
                    else if (dtCorrales.Length % 3 == 1)
                    {
                        reportViewer2.LocalReport.DataSources.Clear();
                        ReportDataSource source = new ReportDataSource("DataSet1", dtCorrales[dtCorrales.Length - 1]);
                        reportViewer2.LocalReport.DataSources.Add(source);
                        reportViewer2.LocalReport.Refresh();
                        ReportDataSource source1 = new ReportDataSource("DataSet2", promedio);
                        reportViewer2.LocalReport.DataSources.Add(source1);
                        reportViewer2.LocalReport.Refresh();
                        ReportParameter[] par = new ReportParameter[2];
                        par[0] = new ReportParameter("EMPRESA", "EMPRESA: ");
                        par[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales[dtCorrales.Length - 1]);
                        reportViewer2.LocalReport.SetParameters(par);

                        reportViewer2.LocalReport.Refresh();
                        reportViewer2.RefreshReport();
                        GTHUtils.SavePDF(reportViewer2, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();

                    }
                }
                else
                {


                    for (int i = 0; i < dtCorrales.Length - (dtCorrales.Length % 3); i += 3)
                    {
                        ReportDataSource source1 = new ReportDataSource("DataSet1", dtCorrales[i]);
                        reportViewer9.LocalReport.DataSources.Clear();
                        reportViewer9.LocalReport.DataSources.Add(source1);

                        source1 = new ReportDataSource("DataSet2", dtCorrales[i + 1]);
                        reportViewer9.LocalReport.DataSources.Add(source1);

                        source1 = new ReportDataSource("DataSet3", dtCorrales[i + 2]);
                        reportViewer9.LocalReport.DataSources.Add(source1);

                        ReportParameter[] parametros = new ReportParameter[4];
                        parametros[0] = new ReportParameter("EMPRESA", "EMPRESA: ");
                        parametros[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales[i]);
                        parametros[2] = new ReportParameter("CORRAL2", "CORRAL: " + corrales[i + 1]);
                        parametros[3] = new ReportParameter("CORRAL3", "CORRAL: " + corrales[i + 2]);
                        reportViewer9.LocalReport.SetParameters(parametros);

                        reportViewer9.LocalReport.Refresh();
                        reportViewer9.RefreshReport();
                        GTHUtils.SavePDF(reportViewer9, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;
                    }

                    if (dtCorrales.Length % 3 == 0)
                    {
                        ReportDataSource source1 = new ReportDataSource("DataSet1", promedio);
                        reportViewer5.LocalReport.DataSources.Clear();
                        reportViewer5.LocalReport.DataSources.Add(source1);
                        reportViewer5.RefreshReport();

                        ReportParameter[] parametros = new ReportParameter[1];
                        parametros[0] = new ReportParameter("EMPRESA", "EMPRESA: ");
                        reportViewer5.LocalReport.SetParameters(parametros);

                        reportViewer5.LocalReport.Refresh();
                        reportViewer5.RefreshReport();
                        GTHUtils.SavePDF(reportViewer5, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();
                    }
                    else if (dtCorrales.Length % 3 == 2)
                    {
                        ReportDataSource source = new ReportDataSource("DataSet1", dtCorrales[dtCorrales.Length - 2]);
                        reportViewer8.LocalReport.DataSources.Clear();
                        reportViewer8.LocalReport.DataSources.Add(source);
                        reportViewer8.RefreshReport();
                        source = new ReportDataSource("DataSet2", dtCorrales[dtCorrales.Length - 1]);
                        reportViewer8.LocalReport.DataSources.Add(source);
                        reportViewer8.RefreshReport();
                        ReportDataSource source1 = new ReportDataSource("DataSet3", promedio);
                        reportViewer8.LocalReport.DataSources.Add(source1);
                        reportViewer8.RefreshReport();
                        ReportParameter[] par = new ReportParameter[3];
                        par[0] = new ReportParameter("EMPRESA", "EMPRESA: ");
                        par[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales[dtCorrales.Length - 2]);
                        par[2] = new ReportParameter("CORRAL2", "CORRAL: " + corrales[dtCorrales.Length - 1]);
                        reportViewer8.LocalReport.SetParameters(par);

                        reportViewer8.LocalReport.Refresh();
                        reportViewer8.RefreshReport();
                        GTHUtils.SavePDF(reportViewer8, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();
                    }
                    else if (dtCorrales.Length % 3 == 1)
                    {
                        reportViewer7.LocalReport.DataSources.Clear();
                        ReportDataSource source = new ReportDataSource("DataSet1", dtCorrales[dtCorrales.Length - 1]);
                        reportViewer7.LocalReport.DataSources.Add(source);
                        reportViewer7.LocalReport.Refresh();
                        ReportDataSource source1 = new ReportDataSource("DataSet2", promedio);
                        reportViewer7.LocalReport.DataSources.Add(source1);
                        reportViewer7.LocalReport.Refresh();
                        ReportParameter[] par = new ReportParameter[2];
                        par[0] = new ReportParameter("EMPRESA", "EMPRESA: ");
                        par[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales[dtCorrales.Length - 1]);
                        reportViewer7.LocalReport.SetParameters(par);

                        reportViewer7.LocalReport.Refresh();
                        reportViewer7.RefreshReport();
                        GTHUtils.SavePDF(reportViewer7, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();

                    }
                }
               
            }

            if (checkBox3.Checked)
            {
                DataTable corrales = Corrales();
                DataTable[] dtCorrales = new DataTable[corrales.Rows.Count];
                int cont = 0;
                for(int i = 0; i < corrales.Rows.Count; i++)
                {
                    query = "SELECT T.FECHA, T.LAC1 AS LACT1, T.LAC2 AS LACT2, T.LAC3 AS LACT3, T.TOTAL , T.PROD_TOTAL, T.SI, T.NO, T.SES1 AS PROD_SES1, T.SES2 AS PROD_SES2, "
                        + " T.SES3 AS PROD_SES3,  T.PSES1 AS PROM_SES1, T.PSES2 AS PROM_SES2, T.PSES3 AS PROM_SES3, T.X, T.DEL, IIF(T.TOTAL > 0, T.MH / T.TOTAL, 0) AS MH, "
                        + " IIF(T.TOTAL > 0, T.MS / T.TOTAL, 0) AS MS, IIF(T.MS > 0 AND T.TOTAL > 0, T.X / (T.MS / T.TOTAL), 0) AS EFICIENCIA, 0.0 AS ILCA, 0.0 AS IC, T.TEMP "
                        + " FROM( "
                        + " SELECT FORMAT(R.FECHA, 'd', 'en-gb') AS FECHA, SUM(R.Lac1) AS LAC1, SUM(R.Lac2) AS LAC2, SUM(R.Lac3) AS LAC3, SUM(R.TOTAL) AS TOTAL, "
                        + " SUM(R.PRODTOTAL) AS PROD_TOTAL, SUM(R.SI) AS SI, SUM(R.NO) AS NO, SUM(R.Ses1) AS SES1, SUM(R.Ses3) AS SES2, SUM(R.Ses3) AS SES3, "
                        + " SUM(R.PSes1) / COUNT(*) AS PSES1, SUM(R.PSes2) / COUNT(*) AS PSES2, SUM(R.PSes3) / COUNT(*) AS PSES3, SUM(R.X) / COUNT(*) AS X, "
                        + " ROUND(SUM(R.DEL) / COUNT(*), 0) AS DEL, SUM(R.MH) AS MH, SUM(R.MS) AS MS, IIF(SUM(R.CorralTemp) > 0, SUM(R.TEMP) / SUM(R.CorralTemp), 0) AS TEMP "
                        + " FROM( "
                        + " SELECT  T.RANCHO, T.CORRAL, T.FECHA, T.Lac1, T.Lac2, T.Lac3, T.TOTAL, T.PRODTOTAL, IIF((T.CId1 + T.CId2 + T.CId3) > 0, (T.Id1 + T.Id2 + T.Id3) / (T.CId1 + T.CId2 + T.CId3), 0) AS SI, "
                        + " IIF((T.CNId1 + T.CNId2 + T.CNId3) > 0, (T.NId1 + T.NId2 + T.NId3) / (T.CNId1 + T.CNId2 + T.CNId3), 0) AS NO, "
                        + " T.Ses1, T.Ses2, T.Ses3, T.PSes1, T.PSes2, T.PSes3, T.X, T.DEL, T.MS, T.MH, T.TEMP, IIF(T.TEMP > 0, 1, 0) AS CorralTemp "
                        + " FROM( "
                        + " SELECT DISTINCT c.ran_id As RANCHO, c.cor_id AS CORRAL, pc.pc_fecha AS FECHA, pc.pc_vaclac1 AS Lac1, pc.pc_vaclac2 AS Lac2, pc.pc_vaclac3 AS Lac3, "
                        + " (pc.pc_vaclac1 + pc.pc_vaclac2 + pc.pc_vaclac3) AS TOTAL, pc.pc_prodtotal AS PRODTOTAL, pc.pc_idcorral1 AS Id1, pc.pc_idcorral2 AS Id2, "
                        + " pc.pc_idcorral2 AS Id3, IIF(pc.pc_idcorral1 > 0, 1, 0) AS CId1, IIF(pc.pc_idcorral2 > 0, 1, 0) AS CId2, IIF(pc.pc_idcorral3 > 0, 1, 0) AS CId3, "
                        + " pc.pc_noidcorral1 AS NId1, pc.pc_noidcorral2 AS NId2, pc.pc_noidcorral3 AS NId3, IIF(pc.pc_noidcorral1 > 0, 1, 0) AS CNId1, IIF(pc.pc_noidcorral2 > 0, 1, 0) AS CNId2, "
                        + " IIF(pc.pc_noidcorral3 > 0, 1, 0) AS CNId3, pc.pc_prodses1 AS Ses1, pc.pc_prodses2 AS Ses2, pc.pc_prodses3 AS Ses3, "
                        + " pc.pc_promses1 AS PSes1, pc.pc_promses2 AS PSes2, pc.pc_promses3 AS PSes3, IIF(pc.pc_prodses1 > 0, 1, 0) AS CSes1, IIF(pc.pc_prodses2 > 0, 1, 0) AS CSes2, "
                        + " IIF(pc.pc_prodses3 > 0, 1, 0) AS CSes3, pc.pc_promses1 + pc.pc_promses2 + pc.pc_promses3 AS X, pc.pc_del AS DEL, ISNULL(cc.cc_ms, 0) AS MS, ISNULL(cc.cc_mh, 0) AS MH, ISNULL(tc.temperatura, 0) AS TEMP "
                        + " from corral c "
                        + " LEFT JOIN produccion_corral pc ON pc.pc_corral = c.cor_id AND pc.ran_id = c.ran_id "
                        + " LEFT JOIN consumo_corral cc ON cc.cc_corral = c.cor_id AND cc.ran_id = c.ran_id AND cc.cc_fecha = pc.pc_fecha "
                        + " LEFT JOIN TEMPCORRALES tc ON Tc.ran_clave = cc.ran_id AND tc.Fecha = cc.cc_fecha AND cc.cc_corral = tc.corral "
                        + " WHERE c.ran_id = " + rancb + " AND pc.pc_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' "
                        + " AND c.cor_id = " + corrales.Rows[i][0].ToString()
                        + " ) T "
                        + " ) R GROUP BY R.FECHA "
                        + " ) T ORDER BY 1";

                    //query = "SELECT T.FECHA, T.LAC1 AS LACT1, T.LAC2 AS LACT2, T.LAC3 AS LACT3, T.TOTAL , T.PROD_TOTAL, T.SI, T.NO, T.SES1 AS PROD_SES1, T.SES2 AS PROD_SES2, T.SES3 AS PROD_SES3, "
                    //  + " T.PSES1 AS PROM_SES1, T.PSES2 AS PROM_SES2, T.PSES3 AS PROM_SES3, T.X, T.DEL, IIF(T.TOTAL > 0, T.MH / T.TOTAL,0) AS MH, "
                    //  + " IIF(T.TOTAL >  0 , T.MS / T.TOTAL,0 ) AS MS, IIF(T.MS > 0 AND T.TOTAL > 0, T.X / (T.MS / T.TOTAL),0) AS EFICIENCIA, T.TEMP "
                    //  + " FROM( "
                    //  + " SELECT FORMAT(R.FECHA, 'd', 'en-gb') AS FECHA, SUM(R.Lac1) AS LAC1, SUM(R.Lac2) AS LAC2, SUM(R.Lac3) AS LAC3, SUM(R.TOTAL) AS TOTAL, SUM(R.PRODTOTAL) AS PROD_TOTAL, "
                    //  + " SUM(R.SI) AS SI, SUM(R.NO) AS NO, SUM(R.Ses1) AS SES1, SUM(R.Ses3) AS SES2, SUM(R.Ses3) AS SES3, SUM(R.PSes1) / COUNT(*) AS PSES1, SUM(R.PSes2) / COUNT(*) AS PSES2, "
                    //  + " SUM(R.PSes3) / COUNT(*) AS PSES3, SUM(R.X) / COUNT(*) AS X, ROUND(SUM(R.DEL) / COUNT(*), 0) AS DEL, SUM(R.MH) AS MH, SUM(R.MS) AS MS, ISNULL(AVG(tc.temperatura),0) AS TEMP "
                    //  + " FROM( "
                    //  + " SELECT  T.RANCHO, T.CORRAL, T.FECHA, T.Lac1, T.Lac2, T.Lac3, T.TOTAL, T.PRODTOTAL, "
                    //  + " (T.Id1 + T.Id2 + T.Id3) / (T.CId1 + T.CId2 + T.CId3) AS SI, IIF((T.CNId1 + T.CNId2 + T.CNId3) > 0, (T.NId1 + T.NId2 + T.NId3) / (T.CNId1 + T.CNId2 + T.CNId3), 0) AS NO, "
                    //  + " T.Ses1, T.Ses2, T.Ses3, T.PSes1, T.PSes2, T.PSes3, T.X, T.DEL, T.MS, T.MH "
                    //  + " FROM( "
                    //  + " SELECT pc.ran_id As RANCHO, cc.cc_corral AS CORRAL, cc.cc_fecha AS FECHA, pc.pc_vaclac1 AS Lac1, pc.pc_vaclac2 AS Lac2, pc.pc_vaclac3 AS Lac3, "
                    //  + " (pc.pc_vaclac1 + pc.pc_vaclac2 + pc.pc_vaclac3) AS TOTAL, pc.pc_prodtotal AS PRODTOTAL, "
                    //  + " pc.pc_idcorral1 AS Id1, pc.pc_idcorral2 AS Id2, pc.pc_idcorral2 AS Id3, "
                    //  + " IIF(pc.pc_idcorral1 > 0, 1, 0) AS CId1, IIF(pc.pc_idcorral2 > 0, 1, 0) AS CId2, IIF(pc.pc_idcorral3 > 0, 1, 0) AS CId3, "
                    //  + " pc.pc_noidcorral1 AS NId1, pc.pc_noidcorral2 AS NId2, pc.pc_noidcorral3 AS NId3, "
                    //  + " IIF(pc.pc_noidcorral1 > 0, 1, 0) AS CNId1, IIF(pc.pc_noidcorral2 > 0, 1, 0) AS CNId2, IIF(pc.pc_noidcorral3 > 0, 1, 0) AS CNId3, "
                    //  + " pc.pc_prodses1 AS Ses1, pc.pc_prodses2 AS Ses2, pc.pc_prodses3 AS Ses3, "
                    //  + " pc.pc_promses1 AS PSes1, pc.pc_promses2 AS PSes2, pc.pc_promses3 AS PSes3, "
                    //  + " IIF(pc.pc_prodses1 > 0, 1, 0) AS CSes1, IIF(pc.pc_prodses2 > 0, 1, 0) AS CSes2, IIF(pc.pc_prodses3 > 0, 1, 0) AS CSes3, "
                    //  + " pc.pc_promses1 + pc.pc_promses2 + pc.pc_promses3 AS X, pc.pc_del AS DEL, ISNULL(cc.cc_ms, 0) AS MS, ISNULL(cc.cc_mh, 0) AS MH "
                    //  + " from consumo_corral cc "
                    //  + " LEFT JOIN produccion_corral pc ON cc.cc_corral = pc.pc_corral AND cc.ran_id = pc.ran_id AND cc.cc_fecha = pc.pc_fecha "
                    //  + " WHERE cc.ran_id = " + rancb + " AND pc.ran_id is not null AND pc.pc_corral = " + corrales.Rows[i][0].ToString()
                    //  + " AND cc.cc_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "') T ) R " 
                    //  + " LEFT JOIN TEMPCORRALES tc ON R.RANCHO = tc.ran_clave AND R.CORRAL = tc.corral AND R.RANCHO  = tc.ran_clave AND R.FECHA = tc.Fecha "
                    //  + " GROUP BY R.FECHA) T ORDER BY 1 ";
                    conn.QueryAlimento(query, out dtCorrales[i]);
                    Indicadores(dtCorrales[i]);
                    Diferencia(dtCorrales[i]);
                    Promedio(dtCorrales[i], inicio, fin);

                    if (dtCorrales[i].Rows.Count > 0)
                        cont++;
                }
                List<DataTable> dtCor = new List <DataTable>();
                
                for(int i = 0; i < dtCorrales.Length; i++)
                {
                    if (dtCorrales[i].Rows.Count > 0)
                    {
                        dtCor.Add(dtCorrales[i]);

                    }

                }

                
                int num_doc = 1;

                GTHUtils.DeleteFile("C:\\MOVGANADO\\consumos\\Reportes\\Auxiliar");

                DataTable promedio = new DataTable();
                promedio.Columns.Add("prod").DataType = System.Type.GetType("System.Double");
                promedio.Columns.Add("del").DataType = System.Type.GetType("System.Double");
                promedio.Columns.Add("efic").DataType = System.Type.GetType("System.Double");

                for (int i = 0; i < dtCorrales.Length; i++)
                {
                    if (dtCorrales[i].Rows.Count > 0)
                    {
                        DataRow row = promedio.NewRow();
                        row["prod"] = Convert.ToDouble(dtCorrales[i].Rows[dtCorrales[i].Rows.Count - 1][14].ToString());
                        //row["del"] = Int32.Parse(Math.Floor(Convert.ToDecimal(dtCorrales[i].Rows[dtCorrales[i].Rows.Count - 1][15].ToString())).ToString());
                        row["del"] = Convert.ToDouble(dtCorrales[i].Rows[dtCorrales[i].Rows.Count - 1][15].ToString());
                        row["efic"] = Convert.ToDouble(dtCorrales[i].Rows[dtCorrales[i].Rows.Count - 1][18].ToString());
                        promedio.Rows.Add(row);

                    }
                }

                if(ran_pesadores == 1)
                {
                    for (int i = 0; i < dtCorrales.Length - (dtCorrales.Length % 3); i += 3)
                    {
                        ReportDataSource source1 = new ReportDataSource("DataSet1", dtCorrales[i]);

                        reportViewer4.LocalReport.DataSources.Clear();
                        reportViewer4.LocalReport.DataSources.Add(source1);

                        source1 = new ReportDataSource("DataSet2", dtCorrales[i + 1]);
                        reportViewer4.LocalReport.DataSources.Add(source1);

                        source1 = new ReportDataSource("DataSet3", dtCorrales[i + 2]);
                        reportViewer4.LocalReport.DataSources.Add(source1);

                        ReportParameter[] parametros = new ReportParameter[4];
                        parametros[0] = new ReportParameter("EMPRESA", "ESTABLO: " + ran_nombre.ToUpper());
                        parametros[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales.Rows[i][0].ToString());
                        parametros[2] = new ReportParameter("CORRAL2", "CORRAL: " + corrales.Rows[i + 1][0].ToString());
                        parametros[3] = new ReportParameter("CORRAL3", "CORRAL: " + corrales.Rows[i + 2][0].ToString());
                        reportViewer4.LocalReport.SetParameters(parametros);

                        reportViewer4.LocalReport.Refresh();
                        reportViewer4.RefreshReport();
                        GTHUtils.SavePDF(reportViewer4, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;
                    }

                    if (dtCorrales.Length % 3 == 0)
                    {
                        ReportDataSource source1 = new ReportDataSource("DataSet1", promedio);
                        reportViewer5.LocalReport.DataSources.Clear();
                        reportViewer5.LocalReport.DataSources.Add(source1);
                        reportViewer5.RefreshReport();

                        ReportParameter[] parametros = new ReportParameter[1];
                        parametros[0] = new ReportParameter("EMPRESA", "ESTABLO: " + ran_nombre);
                        reportViewer5.LocalReport.SetParameters(parametros);

                        reportViewer5.LocalReport.Refresh();
                        reportViewer5.RefreshReport();
                        GTHUtils.SavePDF(reportViewer5, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();
                    }
                    else if (dtCorrales.Length % 3 == 2)
                    {
                        ReportDataSource source = new ReportDataSource("DataSet1", dtCorrales[dtCorrales.Length - 2]);
                        reportViewer3.LocalReport.DataSources.Clear();
                        reportViewer3.LocalReport.DataSources.Add(source);
                        reportViewer3.RefreshReport();
                        source = new ReportDataSource("DataSet2", dtCorrales[dtCorrales.Length - 1]);
                        reportViewer3.LocalReport.DataSources.Add(source);
                        reportViewer3.RefreshReport();
                        ReportDataSource source1 = new ReportDataSource("DataSet3", promedio);
                        reportViewer3.LocalReport.DataSources.Add(source1);
                        reportViewer3.RefreshReport();
                        ReportParameter[] par = new ReportParameter[3];
                        par[0] = new ReportParameter("EMPRESA", "ESTABLO: " + ran_nombre.ToUpper());
                        par[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales.Rows[dtCorrales.Length - 2][0].ToString());
                        par[2] = new ReportParameter("CORRAL2", "CORRAL: " + corrales.Rows[dtCorrales.Length - 1][0].ToString());
                        reportViewer3.LocalReport.SetParameters(par);

                        reportViewer3.LocalReport.Refresh();
                        reportViewer3.RefreshReport();
                        GTHUtils.SavePDF(reportViewer3, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();
                    }
                    else if (dtCorrales.Length % 3 == 1)
                    {
                        reportViewer2.LocalReport.DataSources.Clear();
                        ReportDataSource source = new ReportDataSource("DataSet1", dtCorrales[dtCorrales.Length - 1]);
                        reportViewer2.LocalReport.DataSources.Add(source);
                        reportViewer2.LocalReport.Refresh();
                        ReportDataSource source1 = new ReportDataSource("DataSet2", promedio);
                        reportViewer2.LocalReport.DataSources.Add(source1);
                        reportViewer2.LocalReport.Refresh();
                        ReportParameter[] par = new ReportParameter[2];
                        par[0] = new ReportParameter("EMPRESA", "ESTABLO: " + ran_nombre.ToUpper());
                        par[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales.Rows[dtCorrales.Length - 1][0].ToString());
                        reportViewer2.LocalReport.SetParameters(par);

                        reportViewer2.LocalReport.Refresh();
                        reportViewer2.RefreshReport();
                        GTHUtils.SavePDF(reportViewer2, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();

                    }
                }
                else
                {
                    for (int i = 0; i < dtCorrales.Length - (dtCorrales.Length % 3); i += 3)
                    {
                        ReportDataSource source1 = new ReportDataSource("DataSet1", dtCorrales[i]);

                        reportViewer9.LocalReport.DataSources.Clear();
                        reportViewer9.LocalReport.DataSources.Add(source1);

                        source1 = new ReportDataSource("DataSet2", dtCorrales[i + 1]);
                        reportViewer9.LocalReport.DataSources.Add(source1);

                        source1 = new ReportDataSource("DataSet3", dtCorrales[i + 2]);
                        reportViewer9.LocalReport.DataSources.Add(source1);

                        ReportParameter[] parametros = new ReportParameter[4];
                        parametros[0] = new ReportParameter("EMPRESA", "ESTABLO: " + ran_nombre.ToUpper());
                        parametros[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales.Rows[i][0].ToString());
                        parametros[2] = new ReportParameter("CORRAL2", "CORRAL: " + corrales.Rows[i + 1][0].ToString());
                        parametros[3] = new ReportParameter("CORRAL3", "CORRAL: " + corrales.Rows[i + 2][0].ToString());
                        reportViewer9.LocalReport.SetParameters(parametros);

                        reportViewer9.LocalReport.Refresh();
                        reportViewer9.RefreshReport();
                        GTHUtils.SavePDF(reportViewer4, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;
                    }

                    if (dtCorrales.Length % 3 == 0)
                    {
                        ReportDataSource source1 = new ReportDataSource("DataSet1", promedio);
                        reportViewer5.LocalReport.DataSources.Clear();
                        reportViewer5.LocalReport.DataSources.Add(source1);
                        reportViewer5.RefreshReport();

                        ReportParameter[] parametros = new ReportParameter[1];
                        parametros[0] = new ReportParameter("EMPRESA", "ESTABLO: " + ran_nombre);
                        reportViewer5.LocalReport.SetParameters(parametros);

                        reportViewer5.LocalReport.Refresh();
                        reportViewer5.RefreshReport();
                        GTHUtils.SavePDF(reportViewer5, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();
                    }
                    else if (dtCorrales.Length % 3 == 2)
                    {
                        ReportDataSource source = new ReportDataSource("DataSet1", dtCorrales[dtCorrales.Length - 2]);
                        reportViewer8.LocalReport.DataSources.Clear();
                        reportViewer8.LocalReport.DataSources.Add(source);
                        reportViewer8.RefreshReport();
                        source = new ReportDataSource("DataSet2", dtCorrales[dtCorrales.Length - 1]);
                        reportViewer8.LocalReport.DataSources.Add(source);
                        reportViewer8.RefreshReport();
                        ReportDataSource source1 = new ReportDataSource("DataSet3", promedio);
                        reportViewer8.LocalReport.DataSources.Add(source1);
                        reportViewer8.RefreshReport();
                        ReportParameter[] par = new ReportParameter[3];
                        par[0] = new ReportParameter("EMPRESA", "ESTABLO: " + ran_nombre.ToUpper());
                        par[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales.Rows[dtCorrales.Length - 2][0].ToString());
                        par[2] = new ReportParameter("CORRAL2", "CORRAL: " + corrales.Rows[dtCorrales.Length - 1][0].ToString());
                        reportViewer8.LocalReport.SetParameters(par);

                        reportViewer8.LocalReport.Refresh();
                        reportViewer8.RefreshReport();
                        GTHUtils.SavePDF(reportViewer8, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();
                    }
                    else if (dtCorrales.Length % 3 == 1)
                    {
                        reportViewer7.LocalReport.DataSources.Clear();
                        ReportDataSource source = new ReportDataSource("DataSet1", dtCorrales[dtCorrales.Length - 1]);
                        reportViewer7.LocalReport.DataSources.Add(source);
                        reportViewer7.LocalReport.Refresh();
                        ReportDataSource source1 = new ReportDataSource("DataSet2", promedio);
                        reportViewer7.LocalReport.DataSources.Add(source1);
                        reportViewer7.LocalReport.Refresh();
                        ReportParameter[] par = new ReportParameter[2];
                        par[0] = new ReportParameter("EMPRESA", "ESTABLO: " + ran_nombre.ToUpper());
                        par[1] = new ReportParameter("CORRAL", "CORRAL: " + corrales.Rows[dtCorrales.Length - 1][0].ToString());
                        reportViewer7.LocalReport.SetParameters(par);

                        reportViewer7.LocalReport.Refresh();
                        reportViewer7.RefreshReport();
                        GTHUtils.SavePDF(reportViewer7, ruta + "Auxiliar\\" + num_doc + ".pdf");
                        //Process.Start(ruta + "Auxiliar\\" + num_doc + ".pdf");
                        num_doc++;

                        CombinarArchivos();

                    }
                }
                
              

            }

            if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked)
            {
                Console.WriteLine("Selecciona una opcion");
            }

            Cursor = Cursors.Default;
        }

        private void CombinarArchivos()
        {
            System.IO.DirectoryInfo files = new DirectoryInfo(ruta + "Auxiliar");
            //string[] files = GetFiles();

            PdfDocument outputDocument = new PdfDocument();
            foreach (FileInfo file in files.GetFiles())
            {
                PdfDocument inputDocument = PdfReader.Open(file.FullName, PdfDocumentOpenMode.Import);
                int count = inputDocument.PageCount;
                for (int idx = 0; idx < count; idx++)
                {
                    PdfPage page = inputDocument.Pages[idx];
                    outputDocument.AddPage(page);
                }
            }
            string filename = ruta + "REPORTE x CORRAL.pdf";
            outputDocument.Save(filename);
            Process.Start(filename);
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            DateTime fecha = dateTimePicker2.Value;
            dateTimePicker1.Value = fecha.AddDays(-7);

        }

        public Reporte_Corral(int ran_id, string ran_nombre, int emp_id, string emp_nombre)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;
        }

        private void Reporte_Corral_Load(object sender, EventArgs e)
        {
            conn.Iniciar();
            DataTable dtFechas = Fechas();
            button1.Cursor = Cursors.Hand;
            checkBox1.Cursor = Cursors.Hand;
            checkBox2.Cursor = Cursors.Hand;
            checkBox3.Cursor = Cursors.Hand;
            dateTimePicker2.MinDate = Convert.ToDateTime(dtFechas.Rows[0][0]);
            dateTimePicker2.MaxDate = Convert.ToDateTime(dtFechas.Rows[0][1]);
            dateTimePicker1.Cursor = Cursors.Hand;
            dateTimePicker2.Cursor = Cursors.Hand;
            txtCorral.Enabled = false;
            dateTimePicker1.Value = dateTimePicker2.Value.Date.AddDays(-7);

        }

        private DataTable Establos()
        {
            DataTable dt;
            string query = "SELECT ran_id AS Id, ran_desc AS Rancho FROM configuracion WHERE emp_id = " + emp_id;
            conn.QuerySIO(query, out dt);
            return dt;
        }

        private void InfoEstablo()
        {
            DataTable dt;
            string query = "SELECT rut_ruta from ruta WHERE ran_id = " + ran_id;
            conn.QuerySIO(query, out dt);
            ruta = dt.Rows[0][0].ToString();

            query = "SELECT ran_pesadores FROM configuracion where ran_id = " + ran_id;
            conn.QuerySIO(query, out dt);
            ran_pesadores = Convert.ToInt32(dt.Rows[0][0]);
        }

        private void Diferencia(DataTable dt)
        {
            if(dt.Rows.Count > 0)
            {
                int ultimo = dt.Rows.Count - 1;
                int penultimo = ultimo - 1;
                double dif, valor, prom;

                DataRow rowP = dt.NewRow();
                rowP[0] = "PROMEDIO";

                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    prom = 0;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        prom += dt.Rows[j][i] != DBNull.Value ? Convert.ToDouble(dt.Rows[j][i]) : 0;
                    }
                    rowP[i] = prom / dt.Rows.Count;
                }

                DataRow row = dt.NewRow();
                DataRow row1 = dt.NewRow();
                row[0] = "DIF. ULTIMO DIA";
                row1[0] = "% DIF.";
                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    dif = Convert.ToDouble(dt.Rows[ultimo][i]) - Convert.ToDouble(dt.Rows[penultimo][i]);
                    valor = Convert.ToDouble(dt.Rows[penultimo][i]);
                    row[i] = dif;
                    double d = valor > 0 ? dif / valor * 100 : 0;
                    d = Math.Abs(d);
                    row1[i] = d;
                }
                dt.Rows.Add(row);
                dt.Rows.Add(row1);
                dt.Rows.Add(rowP);

            }
        }

        private DataTable Corrales()
        {
            string ran_numero = ran_id > 9 ? "'" + ran_id.ToString() + "'": "'0" + ran_id.ToString() + "'";
            DataTable dt;
            string query = "SELECT cor_id FROM corral WHERE ran_id = " + ran_id;
            conn.QueryAlimento(query, out dt);
            return dt;
        }


        private DataTable Fechas()
        {
            DataTable dt;
            string query = "SELECT DATEADD(DAY,7, T.FechaMin) AS FechaMin, T.FechaMax "
                        + " FROM( "
                        + " select MIN(cc_fecha) AS FechaMin, MAX(cc_fecha) AS  FechaMax "
                        + " FROM consumo_corral "
                        + "WHERE ran_id = " +ran_id + ") T";
            conn.QueryAlimento(query, out dt);

            return dt;
        }

        private void Indicadores(DataTable dt)
        {
            DateTime fecha, inicio, fin;
            double costo, mh, ms, media, ilca, ic, costoms, msDt, costoF;
            DataTable dtMaterias;
            double precioLeche = PrecioL();

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                fecha = Convert.ToDateTime(dt.Rows[i][0]);
                HoraCorte(fecha, out inicio, out fin);
                costo = Costo("10,11,12,13", fecha, fecha,inicio, fin);
                Materias("10,11,12,13", fecha, fecha, inicio, fin, out dtMaterias);
                mh = Convert.ToDouble(dtMaterias.Rows[0][0]);
                ms = Convert.ToDouble(dtMaterias.Rows[0][1]);
                msDt = Convert.ToDouble(dt.Rows[i][17]);
                costoms = ms > 0 ? costo / ms : 0;
                costoF = costoms * msDt;
                media = Convert.ToDouble(dt.Rows[0][14]);
                ilca = costoF > 0 ? media * precioLeche / costoF : 0;
                ic = (media * precioLeche) - costoF;
                dt.Rows[i][19] = ilca;
                dt.Rows[i][20] = ic;
            }
        }
        private double Media(DateTime inicio, DateTime fin)
        {
            double media = 0;
            DataTable dt;
            string query = "SELECT IIF(SUM(ia.ia_vacas_ord)>0,ISNULL((SUM(m.med_produc)/SUM(ia.ia_vacas_ord)),0),0)  "
                        + " FROM media m LEFT JOIN inventario_afi ia ON ia.ran_id = m.ran_id AND ia.ia_fecha = m.med_fecha "
                        + " where m.ran_id IN(" + ran_id + ") AND med_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "'";
            conn.QueryAlimento(query, out dt);

            if (dt.Rows.Count > 0)
                media = dt.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dt.Rows[0][0]) : 0;

            return media;
        }

        private void HoraCorte(DateTime fecha, out DateTime inicio, out DateTime fin)
        {
            DataTable dt;
            string query = "select PARAMVALUE FROM bedrijf_params where name like 'DSTimeShift'";
            conn.QueryTracker(query, out dt);

            int horas = Convert.ToInt32(dt.Rows[0][0]);
            int hora_t = 24 + horas;
            hora_t = hora_t == 24 ? 0 : hora_t > 24 ? horas : hora_t;

            inicio = fecha.AddHours(horas);
            fin = inicio.AddDays(1);
        }

        private void Promedio(DataTable dt, DateTime inicio, DateTime fin)
        {
            if(dt.Rows.Count > 0)
            {
                double costo, mh, ms, media, ilca, ic, costoms, msDt, costoF;
                DataTable dtMaterias;
                DateTime fecI = inicio;
                DateTime fecF;
                int renglon = dt.Rows.Count - 1;
                int horas = DifereciaCorte();
                HoraCorte(inicio, out fecI, out fecF);
                fecF = fin.AddHours(horas);
                costo = Costo("10,11,12,13", inicio, fin, fecI, fecF);
                Materias("10,11,12,13", inicio, fin, fecI, fecF, out dtMaterias);
                mh = Convert.ToDouble(dtMaterias.Rows[0][0]);
                ms = Convert.ToDouble(dtMaterias.Rows[0][1]);
                media = Media(inicio, fin);
                double precioLeche = PrecioL();
                costoms = ms > 0 ? costo / ms : 0;
                ilca = costo > 0 ? media * precioLeche / costo : 0;
                ic = (media * precioLeche) - costo;
                dt.Rows[renglon][14] = media;
                dt.Rows[renglon][17] = ms;
                dt.Rows[renglon][16] = mh;
                dt.Rows[renglon][19] = ilca;
                dt.Rows[renglon][20] = ic;
            }
   
        }
        
        private int DifereciaCorte()
        {
            DataTable dt;
            string query = "select PARAMVALUE FROM bedrijf_params where name like 'DSTimeShift'";
            conn.QueryTracker(query, out dt);

            int horas = Convert.ToInt32(dt.Rows[0][0]);
            int hora_t = 24 + horas;

            return hora_t;
        }

        private double Costo(string etapa, DateTime fechaI , DateTime fechaF , DateTime inicio, DateTime fin)
        {
            double v = 0;
            DataTable dt;
            ColumnasDT(out dt);
            int auxh, auxhc;
            int vacas = Animales(fechaI, fechaF);

            DataTable dt1;
            string query = "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
            + " FROM( "
            + " SELECT R.Clave, R.Ing, ISNULL(i.ing_precio_sie, 0) AS Precio, ISNULL(it.ingt_porcentaje_ms, 0) AS PMS, "
            + " SUM(R.Peso) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoH, "
            + " (SUM(R.Peso) * ISNULL(it.ingt_porcentaje_ms, 0) / 100) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoS "
            + " FROM( "
            + " SELECT ran_id AS Ran, r.ing_clave AS Clave, r.ing_descripcion AS Ing, SUM(rac_mh) AS Peso "
            + " FROM racion r "
            + " WHERE ran_id IN(" + ran_id + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND etp_id IN(" + etapa + ") "
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
            + " WHERE ran_id IN(" + ran_id + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
            + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
            + " GROUP BY ran_id, ing_descripcion) T1 "
            + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
            + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
            + " GROUP BY T.Ran, T.Clave, T.Ing "
            + " UNION "
            + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh) "
            + " FROM racion "
            + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ran_id + ")  AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F', 'W') "
            + " AND SUBSTRING(ing_descripcion, 3, 2) NOT IN('00', '01', '02')  AND etp_id IN(" + etapa + ") "
            + " GROUP BY ran_id, ing_clave, ing_descripcion "
            + " UNION "
            + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh) "
            + " FROM racion "
            + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ran_id + ")  AND ing_descripcion IN('Agua', 'Water') "
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
                xvaca = vacas > 0 ? mh / vacas : 0;
                s_xvaca = vacas > 0 ? ms / vacas : 0;
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
                dtTemp.Rows[i]["porcvaca"] = txvaca > 0 ? xvaca / txvaca * 100 : 0;
                dtTemp.Rows[i]["porccosto"] = costoT > 0 ? costo / costoT * 100 : 0;
                dtTemp.Rows[i]["s_porcvaca"] = tsxvaca > 0 ? s_xvaca / tsxvaca * 100 : 0;
                dtTemp.Rows[i]["s_porccosto"] = costoT > 0 ? costo / costoT * 100 : 0;
            }

            string ing, ingA;

            for (int i = 0; i < dtTemp.Rows.Count; i++)
            {
                v += Convert.ToDouble(dtTemp.Rows[i]["COSTO"]);
            }
            return v;
        }

        private int Animales(DateTime inicio, DateTime fin)
        {
            DataTable dt;
            int animales = 0;
            string query = "SELECT ROUND(SUM(CONVERT(FLOAT, ia_vacas_ord)) / COUNT(DISTINCT ia_fecha), 0 ) AS Vacas FROM inventario_afi WHERE ran_id IN( " + ran_id + ") AND ia_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "'";
            conn.QueryAlimento(query, out dt);
            if (dt.Rows.Count > 0)
                Int32.TryParse(dt.Rows[0][0].ToString(), out animales);

            return animales;
        }      

        private void Materias(string etapa, DateTime fechaI, DateTime fechaF, DateTime inicio, DateTime fin, out DataTable dt)
        {
            //ColumnasDT(out dt);
            int auxh, auxhc;

            int vacas = Animales(fechaI, fechaF);
            string sob = Sobrantes();
            DataTable dt1;
            string query = "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
                + " FROM(  "
                + " SELECT R.Clave, R.Ing, ISNULL(i.ing_precio_sie, 0) AS Precio, ISNULL(SUM(R.PesoS) / SUM(R.PesoH), 0) AS PMS, SUM(R.PesoH) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoH, "
                + " SUM(R.PesoS) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoS "
                + "  FROM( "
                + " SELECT ran_id AS Ran, r.ing_clave AS Clave, r.ing_descripcion AS Ing, SUM(rac_mh) AS PesoH, SUM(rac_ms) AS PesoS "
                + " FROM racion r "
                + " WHERE ran_id IN(" + ran_id + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND etp_id IN(" + etapa + ")  AND SUBSTRING(ing_clave, 1, 4) IN('ALAS', 'ALFO') "
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
                + " WHERE ran_id IN(" + ran_id + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
                + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                + " GROUP BY ran_id, ing_descripcion "
                + " ) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
                + " GROUP BY T.Ran, T.Clave, T.Ing "
                + " UNION "
                + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh), SUM(rac_ms) "
                + " FROM racion "
                + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ran_id + ")  AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F', 'W') "
                + " AND SUBSTRING(ing_descripcion, 3, 2) NOT IN('00', '01', '02')  AND etp_id IN(" + etapa + ") "
                 + " GROUP BY ran_id, ing_clave, ing_descripcion "
                + " UNION "
                + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh), SUM(rac_ms) "
                + " FROM racion "
                + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ran_id + ")  AND ing_descripcion IN('Agua', 'Water')  AND etp_id IN(" + etapa + ") "
                + " GROUP BY ran_id, ing_clave, ing_descripcion "
                + " ) R "
                + " LEFT JOIN ingrediente i ON i.ing_clave = R.Clave AND i.ing_descripcion = R.Ing AND i.ran_id = R.Ran "
                + " LEFT JOIN ingrediente_tracker it ON it.ingt_clave = R.Clave AND R.Ing = it.ingt_descripcion AND R.Ran = it.ran_id "
                + " GROUP BY R.Ran, R.Clave, R.Ing, i.ing_precio_sie, it.ingt_porcentaje_ms ) X "
                + " WHERE X.PesoH > 0 AND X.Ing NOT IN(" + sob + ") GROUP BY x.Ing";
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
                xvaca = vacas > 0 ? mh / vacas : 0;
                s_xvaca = vacas > 0 ? ms / vacas : 0;
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
            dt = new DataTable();
            dt.Columns.Add("MH");
            dt.Columns.Add("MS");

            DataRow row = dt.NewRow();
            row["MH"] = txvaca;
            row["MS"] = tsxvaca;
            dt.Rows.Add(row);
        }

        private string Sobrantes()
        {
            DataTable dt;
            string sobrantes = "";
            string query = "SELECT description FROM ds_ingredient WHERE is_active = 1 AND is_deleted = 0 AND substring(description from 1 for 1) not in ('A','F','W') "
                    + "  AND SUBSTRING(description from 3 for 2) not in('00','01','02','90') ";
            conn.QueryTracker(query, out dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sobrantes += "'" + dt.Rows[i][0].ToString() + "',";
            }

            sobrantes = sobrantes.Length > 0 ? sobrantes.Substring(0, sobrantes.Length - 1) : "''";
            return sobrantes;
        }

        private double PrecioL()
        {
            double precio = 0;
            DataTable dt;
            string query = "SELECT TOP(1) hl_precio FROM historico_leche WHERE ran_id = " + ran_id + " ORDER BY hl_fecha_reg desc";
            conn.QueryAlimento(query, out dt);
            Double.TryParse(dt.Rows[0][0].ToString(), out precio);
            return precio;
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
    }
}
