using ght001721x;
using ght001721x.StrongTypesNS;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alimentacion
{
    public partial class Exportar_Prorrateo : Form
    {
        ConnSIO conn = new ConnSIO();
        DateTime racionI;
        DateTime racionF;
        DateTime sie;
        DateTime fecha;
        //DateTime pmI;
        DateTime periodoI;
        DateTime periodoF;
        DateTime balIni;
        int hora_corte;
        int ran_id;
        string ran_numero; //numero a 2 digitos
        string ran_nombre;
        int emp_id;
        string emp_nombre;
        string ruta;
        bool forma;
        int ran_bascula;
        int bal_clave;
        int emp_prorrateo;
        int rep;
        string ranchosId;
        string sUrl;
        string erp;
        string pas;
        int tipoP;
        double porcT;
        double porcDif;
        int dias_a;
        DateTime fechaR;
        string tituloReporte;
        bool fiabilidad;
        string cadFiabilidad;

        public Exportar_Prorrateo(int ran_id, string ran_nombre, int emp_id, string emp_nombre, bool forma, bool fiabilidad)
        {
            InitializeComponent();
            //this.fecha = fecha;
            //this.hora_corte = hora_corte;
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;
            this.forma = forma;
            this.fiabilidad = fiabilidad;
            sUrl = ConfigurationManager.AppSettings["url"];
        }

        private double PorcentajeDif(string tipo)
        {
            double valor;
            DataTable dt;
            string query = "select (SUM(pro_consumo) - SUM(pro_consumo_tra)) /SUM(pro_consumo_tra) * 100 "
                        + " from prorrateo "
                        + " WHERE pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "' "
                        + " AND SUBSTRING(art_clave, 1,4) IN('" + tipo + "') ";
            conn.QueryAlimento(query, out dt);

            valor = dt.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dt.Rows[0][0]) : 0;

            return valor;
        }

        private double PesoDif(string tipo)
        {
            double valor;
            DataTable dt;
            string query = "select SUM(pro_consumo) - SUM(pro_consumo_tra) "
                        + " from prorrateo "
                        + " WHERE pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "' "
                        + " AND SUBSTRING(art_clave, 1,4) IN('" + tipo + "') ";
            conn.QueryAlimento(query, out dt);

            valor = dt.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dt.Rows[0][0]) : 0;

            return valor;

        }

        private void GetParameters()
        {
            DateTime hoy = DateTime.Today;
            int dia = 0;
            if (hoy.Day >= 1 && hoy.Day < 6)
            {
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
                fecha = hoy;
                if (fecha.Month == 1 || fecha.Month == 3 || fecha.Month == 5 || fecha.Month == 7 || fecha.Month == 8 || fecha.Month == 10 || fecha.Month == 12)
                    dia = 31;
                else if (fecha.Month == 2)
                    if ((fecha.Year % 4 == 0 && fecha.Year % 100 != 0) || fecha.Year % 400 == 0)
                        dia = 29;
                    else
                        dia = 28;
                else if (fecha.Month == 4 || fecha.Month == 6 || fecha.Month == 9 || fecha.Month == 11)
                    dia = 30;
                fecha = new DateTime(hoy.Year, hoy.Month, dia);
            }

            //Obtener de la hora corte
            DataTable dt1;
            string query1 = "select PARAMVALUE FROM bedrijf_params where name like 'DSTimeShift'";
            conn.QueryTracker(query1, out dt1);
            int temp = Convert.ToInt32(dt1.Rows[0][0]);
            hora_corte = 24 + temp;
            hora_corte = hora_corte > 24 ? hora_corte - 24 : hora_corte;
            dias_a = temp >= 0 ? 0 : -1;

            DataTable dt;
            query1 = "SELECT ran_bascula, emp_prorrateo, ran_emp_prorrateo, erp_id, pas_prorrateo FROM [DBSIO].[dbo].configuracion WHERE ran_id = " + ran_id.ToString();
            conn.QuerySIO(query1, out dt);
            ran_bascula = Convert.ToInt32(dt.Rows[0][0]);
            emp_prorrateo = Convert.ToInt32(dt.Rows[0][1]);
            rep = Convert.ToInt32(dt.Rows[0][2]);
            erp = dt.Rows[0][3].ToString();
            pas = dt.Rows[0][4].ToString();

            DataTable dt2;
            query1 = "SELECT bal_clave FROM [DBSIE].[dbo].bascula WHERE ran_id = " + ran_id.ToString();
            conn.QuerySIE(query1, out dt2);
            bal_clave = dt2.Rows.Count > 0 ? Convert.ToInt32(dt2.Rows[0][0]) : 0;

            DataTable dt3;
            query1 = "SELECT cr_bascula FROM configuracion_rancho WHERE ran_id = " + ran_id;
            conn.QuerySIO(query1, out dt3);
            tipoP = Convert.ToInt32(dt3.Rows[0][0]);

            DataTable dt4;
            query1 = "select pp_porc_dif, pp_porc_t from porcentaje_prorrateo";
            conn.QueryAlimento(query1, out dt4);
            porcDif = Convert.ToDouble(dt4.Rows[0][0]);
            porcT = Convert.ToDouble(dt4.Rows[0][1]);
            ranchosId = rep == 1 ? RanchosEP() : ran_id.ToString();
            tituloReporte = rep == 1 || ran_id == 4 ? emp_nombre : ran_nombre;
            
        }        

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidarProrrateo())
            {
                MessageBox.Show("NO SE HA GUARDADO EL PRORRATEO DE EL MES", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                CalificacionesProrrateo();

                DataTable dtPremezclas = new DataTable();
                string queryPM = "select DISTINCT ing_descripcion FROM racion "
                    + " WHERE rac_fecha BETWEEN '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                    + " AND ran_id IN(" + ranchosId + ") "
                    + " AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02') "
                    + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F') AND SUBSTRING(rac_descripcion,3,2) NOT IN('00','01','02')";
                conn.QueryAlimento(queryPM, out dtPremezclas);

                conn.DeleteAlimento("porcentaje_Premezcla", "");
                string qry = "";
                DataTable dt;
                for (int i = 0; i < dtPremezclas.Rows.Count; i++)
                {
                    //qry = "Select TOP(5) FROM premezcla WHERE pmez_racion like '" + dtPremezclas.Rows[i][0].ToString() + "'";
                    //conn.QueryAlimento(qry, out dt);

                    //if (dt.Rows.Count == 0)
                    //    continue;

                    CargarPremezcla(dtPremezclas.Rows[i][0].ToString(), racionI, racionF);
                }

                //DataTable Final
                DataTable dtProrrateo;
                ColumnasDT(out dtProrrateo);

                //Datatable para saltar un renglon
                DataTable dtSalto;
                ColumnasDT(out dtSalto);
                //dtSalto.Columns.Add("TOTAL").DataType = System.Type.GetType("System.Double");

                DataRow drEnter = dtSalto.NewRow();
                drEnter[0] = "";
                drEnter[1] = "";
                drEnter[2] = 0;
                drEnter[3] = 0;
                drEnter[4] = 0;
                drEnter[5] = 0;
                drEnter[6] = 0;
                drEnter[7] = 0;
                drEnter[8] = 0;
                drEnter[9] = 0;
                drEnter[10] = 0;
                dtSalto.Rows.Add(drEnter);

                
                string bascula = " LEFT JOIN ("
                            + "select art_clave AS Clave,  pro_consumo AS Peso "
                            + " FROM prorrateo "
                            + " WHERE SUBSTRING(art_clave,1,4) IN('ALFO') "
                            + " AND pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "' "
                            + " AND pro_consumo_tra > 0 AND ran_id = " + ran_id + ") bascula ON p.prod_clave = bascula.clave ";

                //ALFO
                DataTable dtf;
                string queryF = "SELECT R3.CLAVE, R3.INGREDIENTE, R3.JAULAS, R3.DESTETADAS1, R3.DESTETADAS2, R3.VAQUILLAS_PRENADAS, "
                    + " (R3.JAULAS + R3.DESTETADAS1 + R3.DESTETADAS2 + R3.VAQUILLAS_PRENADAS) AS TOTAL_KGS_CRIANZA, "
                    + " R3.PRODUCCION, R3.SECAS, (R3.PRODUCCION + R3.SECAS) AS TOTAL_KGS_GANADO, "
                    + " (R3.JAULAS + R3.DESTETADAS1 + R3.DESTETADAS2 + R3.VAQUILLAS_PRENADAS) +(R3.PRODUCCION + R3.SECAS) AS TOTAL "
                    + " FROM( "
                    + " SELECT  p.prod_clave AS CLAVE, p.prod_nombre AS INGREDIENTE, tracker.PorcJaulas * bascula.Peso AS JAULAS, "
                    + " tracker.PorcDest1 * bascula.Peso AS DESTETADAS1, tracker.PorcDest2 * bascula.Peso AS DESTETADAS2, "
                    + " tracker.PorcVP * bascula.Peso AS VAQUILLAS_PRENADAS, tracker.PorcProd * bascula.Peso AS PRODUCCION, tracker.PorcSecas * bascula.Peso AS SECAS "
                    + " FROM( SELECT DISTINCT * FROM producto )p "
                    + " LEFT JOIN( "
                    + " SELECT T.CLAVE, IIF(T.TOTAL > 0, (T.JAULAS / T.TOTAL), 0) AS PorcJaulas, IIF(T.TOTAL > 0, (T.DESTETADAS1 / T.TOTAL), 0) AS PorcDest1, "
                    + " IIF(T.TOTAL > 0, (T.DESTATADAS2 / T.TOTAL), 0) AS PorcDest2, IIF(T.TOTAL > 0, (T.VAQUILLAS_PRENADAS / T.TOTAL), 0) AS PorcVP, "
                    + " IIF(T.TOTAL > 0, (T.PRODUCCION / T.TOTAL), 0) AS PorcProd, IIF(T.TOTAL > 0, (T.SECAS / T.TOTAL), 0) AS PorcSecas "
                    + " FROM( "
                    + " SELECT R2.CLAVE, SUM( case when R2.Etapa IN(31) THEN R2.PESO ELSE 0 END) AS JAULAS, SUM( case when R2.Etapa IN(32) THEN R2.PESO ELSE 0 END) AS DESTETADAS1, "
                    + " SUM( case when R2.Etapa IN(33) THEN R2.PESO ELSE 0 END) AS DESTATADAS2, SUM( case when R2.Etapa IN(34) THEN R2.PESO ELSE 0 END) AS VAQUILLAS_PRENADAS, "
                    + " SUM( case when R2.Etapa IN(31, 32, 33, 34) THEN R2.PESO ELSE 0 END) AS TOTAL_KGS_CRIANZA, SUM( case when R2.Etapa IN(10, 11, 12, 13) THEN R2.PESO ELSE 0 END) AS PRODUCCION, "
                    + " SUM( case when R2.Etapa IN(21, 22) THEN R2.PESO ELSE 0 END) AS SECAS, SUM( case when R2.Etapa IN(10, 11, 12, 13, 21, 22) THEN R2.PESO ELSE 0 END) AS TOTAL_KGS_GANADO, "
                    + " SUM( case when R2.Etapa IN(10, 11, 12, 13, 21, 22, 31, 32, 33, 34) THEN R2.PESO ELSE 0 END) AS TOTAL "
                    + " FROM( "
                    + " SELECT R1.Etapa, R1.CLAVE, R1.INGREDIENTE, SUM(R1.PESO) AS PESO "
                    + " FROM( "
                    + " SELECT etp_id AS Etapa, ing_clave AS CLAVE, ing_descripcion AS INGREDIENTE, SUM(rac_mh) AS PESO "
                    + " FROM racion "
                    + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranchosId + ") AND ing_clave like 'ALFO%' "
                    + " GROUP BY etp_id, ing_clave, ing_descripcion "
                    + " UNION "
                    + " SELECT R.Etapa, IIF(R.Racion = '', R.Clave1, R.Clave2) AS Clave, IIF(R.Racion = '', R.Ingrediente1, R.Ingrediente2) AS Ingrediente, R.Peso * R.Porcentaje AS Peso "
                    + " FROM( "
                    + " SELECT T1.Etapa, T1.Clave AS Clave1, T1.Ingrediente AS Ingrediente1, T1.Peso, ISNULL(T2.Racion, '') AS Racion, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ingrediente, '') AS Ingrediente2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                    + " FROM( "
                    + " SELECT T1.Etapa, T2.Clave, T2.Ingrediente, T1.Peso * T2.Porcentaje As Peso "
                    + " FROM( "
                    + " SELECT etp_id AS Etapa, ing_descripcion AS Racion, SUM(rac_mh) AS Peso "
                    + " FROM racion "
                    + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                    + " AND ran_id IN(" + ranchosId + ") AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                    + " AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F') AND etp_id not in (0) "
                    + " GROUP BY etp_id, ing_descripcion) T1 "
                    + " LEFT JOIN( "
                    + " SELECT pmez_descripcion AS Racion, ing_clave AS Clave, ing_descripcion AS Ingrediente, pmez_porcentaje AS Porcentaje "
                    + " FROM porcentaje_Premezcla "
                    + " )T2 ON T1.Racion = T2.Racion) T1 "
                    + " LEFT JOIN( "
                    + " SELECT pmez_descripcion AS Racion, ing_clave AS Clave, ing_descripcion AS Ingrediente, pmez_porcentaje AS Porcentaje "
                    + " FROM porcentaje_Premezcla "
                    + " )T2 ON T1.Ingrediente = T2.Racion) R) R1 "
                    + " WHERE SUBSTRING(R1.Clave, 1, 4) LIKE 'ALFO' "
                    + " GROUP BY R1.Etapa, R1.Clave, R1.INGREDIENTE) R2 "
                    + " GROUP BY R2.CLAVE) T "

                   + " )tracker ON tracker.CLAVE = p.prod_clave "
                    + bascula
                    + " WHERE bascula.Peso IS NOT NULL OR tracker.PorcProd IS NOT NULL OR tracker.PorcDest1 IS NOT NULL "
                    + " OR tracker.PorcDest2 IS NOT NULL OR tracker.PorcJaulas IS NOT NULL OR tracker.PorcSecas IS NOT NULL OR tracker.PorcVP IS NOT NULL ) R3 "
                    + " WHERE (R3.JAULAS > 0 OR R3.DESTETADAS1 > 0 OR R3.DESTETADAS2 > 0 OR R3.VAQUILLAS_PRENADAS > 0 OR R3.PRODUCCION > 0 OR R3.SECAS > 0) "
                    + " ORDER BY 1";
                conn.QueryAlimento(queryF, out dtf);

                DataTable dtBal;
                string queryBal = "";
                
                queryBal = "select SUM(pro_consumo) AS Peso "
                           + " FROM prorrateo "
                           + " WHERE SUBSTRING(art_clave,1,4) IN('ALFO') "
                           + " AND pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "' "
                           + " AND pro_consumo_tra > 0 AND ran_id = " + ran_id;
                conn.QueryAlimento(queryBal, out dtBal);

                double totBal = Convert.ToDouble(dtBal.Rows[0][0]);


                double tf_jaulas = 0, tf_dest1 = 0, tf_dest2 = 0, tf_vp = 0, tf_kgCrianza = 0, tf_prod = 0, tf_secas = 0, tf_kgGanado = 0, tf_total = 0;

                for (int i = 0; i < dtf.Rows.Count; i++)
                {
                    tf_jaulas += Convert.ToDouble(dtf.Rows[i][2]);
                    tf_dest1 += Convert.ToDouble(dtf.Rows[i][3]);
                    tf_dest2 += Convert.ToDouble(dtf.Rows[i][4]);
                    tf_vp += Convert.ToDouble(dtf.Rows[i][5]);
                    tf_kgCrianza += Convert.ToDouble(dtf.Rows[i][6]);
                    tf_prod += Convert.ToDouble(dtf.Rows[i][7]);
                    tf_secas += Convert.ToDouble(dtf.Rows[i][8]);
                    tf_kgGanado += Convert.ToDouble(dtf.Rows[i][9]);
                    tf_total += Convert.ToDouble(dtf.Rows[i][10]);
                }

                //Renglon TOTAL FORRAJE
                DataRow drF = dtf.NewRow();
                drF[0] = "";
                drF[1] = "TOTAL FORRAJE";
                drF[2] = tf_jaulas;
                drF[3] = tf_dest1;
                drF[4] = tf_dest2;
                drF[5] = tf_vp;
                drF[6] = tf_kgCrianza;
                drF[7] = tf_prod;
                drF[8] = tf_secas;
                drF[9] = tf_kgGanado;
                drF[10] = tf_total;
                dtf.Rows.Add(drF);

                //Renglon TOTAL BASCULA
                drF = dtf.NewRow();
                drF[0] = "";
                drF[1] = "TOTAL BASCULA";
                drF[2] = 0;
                drF[3] = 0;
                drF[4] = 0;
                drF[5] = 0;
                drF[6] = 0;
                drF[7] = 0;
                drF[8] = 0;
                drF[9] = 0;
                drF[10] = totBal;
                dtf.Rows.Add(drF);

                //Renglon TOTAL FALTANTE BASCULA
                drF = dtf.NewRow();
                drF[0] = "";
                drF[1] = "TOTAL FALTANTE BASCULA";
                drF[2] = 0;
                drF[3] = 0;
                drF[4] = 0;
                drF[5] = 0;
                drF[6] = 0;
                drF[7] = 0;
                drF[8] = 0;
                drF[9] = 0;
                drF[10] = tf_total - totBal;
                dtf.Rows.Add(drF);

                //Renglon Diferencia Tracker
                drF = dtf.NewRow();
                drF[0] = "";
                drF[1] = "PORCENTAJE DIFERENCIA CON TRACKER";
                drF[2] = 0;
                drF[3] = 0;
                drF[4] = 0;
                drF[5] = 0;
                drF[6] = 0;
                drF[7] = 0;
                drF[8] = 0;
                drF[9] = 0;
                drF[10] = PorcentajeDif("ALFO");
                dtf.Rows.Add(drF);
     
                //ALAS
                DataTable dta1;
                string query = "SELECT Prorrateo.Clave AS CLAVE, Prorrateo.Ingrediente AS INGREDIENTE, Prorrateo.PorcProd * Prorrateo.Bascula AS PRODUCCION,  Prorrateo.PorcSecas* Prorrateo.Bascula AS SECAS, "
                            + " (Prorrateo.PorcSecas * Prorrateo.Bascula) + (Prorrateo.PorcProd * Prorrateo.Bascula) AS TOTAL_KGS_GANADO, Prorrateo.Bascula* Prorrateo.PorcJaulas AS JAULAS, "
                            + " Prorrateo.PorcDest1* Prorrateo.Bascula AS DESTETADAS1, Prorrateo.PorcDest2* Prorrateo.Bascula AS DESTETADAS2,  Prorrateo.PorcVaq* Prorrateo.Bascula AS VAQUILLAS_PRENADAS, "
                            + " ( Prorrateo.Bascula* Prorrateo.PorcJaulas) + (Prorrateo.PorcDest1* Prorrateo.Bascula)  + (Prorrateo.PorcDest2* Prorrateo.Bascula) + (Prorrateo.PorcVaq* Prorrateo.Bascula) AS TOTAL_KGS_CRIANZA, "
                            + " Prorrateo.Bascula AS TOTAL "
                            + " FROM( "
                            + " SELECT  p.prod_clave AS Clave, p.prod_nombre AS Ingrediente, ISNULL(produccion.Peso, 0) AS Produccion, "
                            + " IIF(Total.Peso > 0 AND produccion.Peso > 0, produccion.Peso / Total.Peso, 0) AS PorcProd, "
                            + " ISNULL(secas.Peso, 0) AS SECAS, IIF(Total.Peso > 0 AND secas.Peso > 0, secas.Peso / Total.Peso, 0) AS PorcSecas, "
                            + " ISNULL(jaulas.Peso, 0) AS Jaulas, IIF(Total.Peso > 0 AND jaulas.Peso > 0, jaulas.Peso / Total.Peso, 0) AS PorcJaulas, "
                            + " ISNULL(dest1.Peso, 0) AS Destetadas1, IIF(Total.Peso > 0 AND dest1.Peso > 0, dest1.Peso / Total.Peso, 0) AS PorcDest1, "
                            + " ISNULL(dest2.Peso, 0) AS Destetadas2, IIF(Total.Peso > 0 AND dest2.Peso > 0, dest2.Peso / Total.Peso, 0) AS PorcDest2, "
                            + " ISNULL(vaquillas.Peso, 0) AS Vaquillas, IIF(Total.Peso > 0 AND vaquillas.Peso > 0, vaquillas.Peso / Total.Peso, 0) AS PorcVaq, "
                            + " ISNULL(bascula.Peso, 0) AS Bascula, ISNULL(Total.Peso, 0) AS Total "
                            + " FROM(SELECT DISTINCT * FROM producto ) p "
                            + " LEFT JOIN( "
                            + " SELECT R.Clave, SUM(R.Peso) AS Peso "
                            + " FROM( "
                            + " SELECT ing_clave AS Clave, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND etp_id IN(10, 11, 12, 13) AND SUBSTRING(ing_clave, 1, 4) LIKE 'ALAS' "
                            + " GROUP BY ing_clave "
                            + " UNION "
                            + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, T.Peso * T.Porcentaje  AS Peso "
                            + " FROM( "
                            + " SELECT T1.Clave AS Clave1, T1.Ing AS ing1, T1.Peso, ISNULL(T2.Pmez, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                            + " FROM( "
                            + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porcentaje) AS Peso "
                            + " FROM( "
                            + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                            + " AND etp_id IN(10, 11, 12, 13) GROUP BY etp_id, ing_descripcion) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla "
                            + " )T2 ON T1.Pmz = T2.Pmez) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla )T2 ON T1.Ing = T2.Pmez) T)  R "
                            + " WHERE SUBSTRING(R.Clave, 1, 4) LIKE 'ALAS' GROUP BY R.Clave "
                            + " ) produccion ON p.prod_clave = produccion.Clave  "
                            + " LEFT JOIN( "
                            + " SELECT R.Clave, SUM(R.Peso) AS Peso "
                            + " FROM( "
                            + " SELECT ing_clave AS Clave, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN (" + ranchosId + ") AND etp_id IN(21,22) AND SUBSTRING(ing_clave, 1, 4) LIKE 'ALAS' "
                            + " GROUP BY ing_clave "
                            + " UNION "
                            + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, T.Peso * T.Porcentaje  AS Peso "
                            + " FROM( "
                            + " SELECT T1.Clave AS Clave1, T1.Ing AS ing1, T1.Peso, ISNULL(T2.Pmez, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                            + " FROM( "
                            + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porcentaje) AS Peso "
                            + " FROM( "
                            + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                            + " AND etp_id IN(21,22) GROUP BY etp_id, ing_descripcion) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla "
                            + " )T2 ON T1.Pmz = T2.Pmez) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla )T2 ON T1.Ing = T2.Pmez) T)  R "
                            + " WHERE SUBSTRING(R.Clave, 1, 4) LIKE 'ALAS' GROUP BY R.Clave "
                            + " )secas ON secas.Clave = p.prod_clave "
                            + " LEFT JOIN( "
                            + " SELECT R.Clave, SUM(R.Peso) AS Peso "
                            + " FROM( "
                            + " SELECT ing_clave AS Clave, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN (" + ranchosId + ") AND etp_id IN(31) AND SUBSTRING(ing_clave, 1, 4) LIKE 'ALAS' "
                            + " GROUP BY ing_clave "
                            + " UNION "
                            + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, T.Peso * T.Porcentaje  AS Peso "
                            + " FROM( "
                            + " SELECT T1.Clave AS Clave1, T1.Ing AS ing1, T1.Peso, ISNULL(T2.Pmez, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                            + " FROM( "
                            + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porcentaje) AS Peso "
                            + " FROM( "
                            + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                            + " AND etp_id IN(31) GROUP BY etp_id, ing_descripcion) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla "
                            + " )T2 ON T1.Pmz = T2.Pmez) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla )T2 ON T1.Ing = T2.Pmez) T)  R "
                            + " WHERE SUBSTRING(R.Clave, 1, 4) LIKE 'ALAS' GROUP BY R.Clave "
                            + " ) jaulas  ON jaulas.Clave = p.prod_clave "
                            + " LEFT JOIN( "
                            + " SELECT R.Clave, SUM(R.Peso) AS Peso "
                            + " FROM( "
                            + " SELECT ing_clave AS Clave, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND etp_id IN(32) AND SUBSTRING(ing_clave, 1, 4) LIKE 'ALAS' "
                            + " GROUP BY ing_clave "
                            + " UNION "
                            + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, T.Peso * T.Porcentaje  AS Peso "
                            + " FROM( "
                            + " SELECT T1.Clave AS Clave1, T1.Ing AS ing1, T1.Peso, ISNULL(T2.Pmez, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                            + " FROM( "
                            + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porcentaje) AS Peso "
                            + " FROM( "
                            + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                            + " AND etp_id IN(32) GROUP BY etp_id, ing_descripcion) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla "
                            + " )T2 ON T1.Pmz = T2.Pmez) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla )T2 ON T1.Ing = T2.Pmez) T)  R "
                            + " WHERE SUBSTRING(R.Clave, 1, 4) LIKE 'ALAS' GROUP BY R.Clave "
                            + " ) dest1 ON dest1.Clave = p.prod_clave "
                            + " LEFT JOIN( "
                            + " SELECT R.Clave, SUM(R.Peso) AS Peso "
                            + " FROM( "
                            + " SELECT ing_clave AS Clave, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND etp_id IN(33) AND SUBSTRING(ing_clave, 1, 4) LIKE 'ALAS' "
                            + " GROUP BY ing_clave "
                            + " UNION "
                            + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, T.Peso * T.Porcentaje  AS Peso "
                            + " FROM( "
                            + " SELECT T1.Clave AS Clave1, T1.Ing AS ing1, T1.Peso, ISNULL(T2.Pmez, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                            + " FROM( "
                            + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porcentaje) AS Peso "
                            + " FROM( "
                            + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                            + " AND etp_id IN(33) GROUP BY etp_id, ing_descripcion) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla "
                            + " )T2 ON T1.Pmz = T2.Pmez) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla )T2 ON T1.Ing = T2.Pmez) T)  R "
                            + " WHERE SUBSTRING(R.Clave, 1, 4) LIKE 'ALAS' GROUP BY R.Clave "
                            + " ) dest2 ON dest2.Clave = p.prod_clave "
                            + " LEFT JOIN( "
                            + " SELECT R.Clave, SUM(R.Peso) AS Peso "
                            + " FROM( "
                            + " SELECT ing_clave AS Clave, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND etp_id IN(34) AND SUBSTRING(ing_clave, 1, 4) LIKE 'ALAS' "
                            + " GROUP BY ing_clave "
                            + " UNION "
                            + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, T.Peso * T.Porcentaje  AS Peso "
                            + " FROM( "
                            + " SELECT T1.Clave AS Clave1, T1.Ing AS ing1, T1.Peso, ISNULL(T2.Pmez, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                            + " FROM( "
                            + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porcentaje) AS Peso "
                            + " FROM( "
                            + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                            + " AND etp_id IN(34) GROUP BY etp_id, ing_descripcion) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla "
                            + " )T2 ON T1.Pmz = T2.Pmez) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla )T2 ON T1.Ing = T2.Pmez) T)  R "
                            + " WHERE SUBSTRING(R.Clave, 1, 4) LIKE 'ALAS' GROUP BY R.Clave "
                            + " ) vaquillas ON vaquillas.Clave = p.prod_clave "
                            + " LEFT JOIN( "
                            + " SELECT art_clave AS Clave, pro_consumo AS Peso FROM prorrateo  where pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "' AND ran_id = " + ran_id
                            + " AND SUBSTRING(art_clave,1,4) IN('ALAS') "
                            + " )bascula ON bascula.Clave = p.prod_clave "
                            + " LEFT JOIN( "
                            + " SELECT R.Clave, SUM(R.Peso) AS Peso "
                            + " FROM( "
                            + " SELECT ing_clave AS Clave, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND etp_id NOT IN(0) AND SUBSTRING(ing_clave, 1, 4) LIKE 'ALAS' "
                            + " GROUP BY ing_clave "
                            + " UNION "
                            + " SELECT IIF(T.Pmz = '', T.Clave1, T.Clave2) AS Clave, T.Peso * T.Porcentaje  AS Peso "
                            + " FROM( "
                            + " SELECT T1.Clave AS Clave1, T1.Ing AS ing1, T1.Peso, ISNULL(T2.Pmez, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                            + " FROM( "
                            + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porcentaje) AS Peso "
                            + " FROM( "
                            + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ran_id IN(" + ranchosId + ") AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                            + " AND etp_id NOT IN(0) GROUP BY etp_id, ing_descripcion) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla "
                            + " )T2 ON T1.Pmz = T2.Pmez) T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_Premezcla )T2 ON T1.Ing = T2.Pmez) T)  R "
                            + " WHERE SUBSTRING(R.Clave, 1, 4) LIKE 'ALAS' GROUP BY R.Clave "
                            + " )TOTAL ON p.prod_clave = TOTAL.Clave "
                            + " WHERE(jaulas.Peso IS NOT NULL OR produccion.Peso IS NOT NULL OR secas.Peso IS NOT NULL OR dest1.Peso IS NOT NULL OR dest2.Peso IS NOT NULL OR vaquillas.Peso IS NOT NULL OR bascula.Peso IS NOT NULL) "
                            + " ) Prorrateo WHERE Prorrateo.Bascula > 0 ORDER BY 1";
                conn.QueryAlimento(query, out dta1);

                DataTable dtInv;
                query = "SELECT SUM(pro_consumo) FROM prorrateo where ran_id = " + ran_id.ToString() + " AND pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "' AND SUBSTRING(art_clave,1,4) IN('ALAS')"; // " AND pro_fecha = '" + racionF.ToString("yyyy-MM-dd") + "'";
                conn.QueryAlimento(query, out dtInv);

                double totInv = Convert.ToDouble(dtInv.Rows[0][0]);

                double ta_jaulas = 0, ta_dest1 = 0, ta_dest2 = 0, ta_vaq = 0, ta_kcrianza = 0, ta_prod = 0, ta_secas = 0, ta_kganado = 0, ta_basct = 0;

                for (int i = 0; i < dta1.Rows.Count; i++)
                {
                    ta_prod += Convert.ToDouble(dta1.Rows[i][2]);
                    ta_secas += Convert.ToDouble(dta1.Rows[i][3]);
                    ta_kganado += Convert.ToDouble(dta1.Rows[i][4]);
                    ta_jaulas += Convert.ToDouble(dta1.Rows[i][5]);
                    ta_dest1 += Convert.ToDouble(dta1.Rows[i][6]);
                    ta_dest2 += Convert.ToDouble(dta1.Rows[i][7]);
                    ta_vaq += Convert.ToDouble(dta1.Rows[i][8]);
                    ta_kcrianza += Convert.ToDouble(dta1.Rows[i][9]);
                    ta_basct += Convert.ToDouble(dta1.Rows[i][10]);
                }
                //Renglon Total concentrado
                DataRow dr1 = dta1.NewRow();
                dr1[0] = "";
                dr1[1] = "TOTAL CONCENTRADO";
                dr1[2] = ta_prod;
                dr1[3] = ta_secas;
                dr1[4] = ta_kganado;
                dr1[5] = ta_jaulas;
                dr1[6] = ta_dest1;
                dr1[7] = ta_dest2;
                dr1[8] = ta_vaq;
                dr1[9] = ta_kcrianza;
                dr1[10] = ta_basct;
                dta1.Rows.Add(dr1);

                //Renglon Total Inventario
                dr1 = dta1.NewRow();
                dr1[0] = "";
                dr1[1] = "TOTAL INVENTARIO";
                dr1[2] = 0;
                dr1[3] = 0;
                dr1[4] = 0;
                dr1[5] = 0;
                dr1[6] = 0;
                dr1[7] = 0;
                dr1[8] = 0;
                dr1[9] = 0;
                dr1[10] = totInv;
                dta1.Rows.Add(dr1);

                //Renglon Total Faltante inventario
                dr1 = dta1.NewRow();
                dr1[0] = "";
                dr1[1] = "TOTAL FALTANTE INVENTARIO";
                dr1[2] = 0;
                dr1[3] = 0;
                dr1[4] = 0;
                dr1[5] = 0;
                dr1[6] = 0;
                dr1[7] = 0;
                dr1[8] = 0;
                dr1[9] = 0;
                dr1[10] = ta_basct - totInv;
                dta1.Rows.Add(dr1);

                //Renglon Porcentaje Diferencia Con Tracker
                dr1 = dta1.NewRow();
                dr1[0] = "";
                dr1[1] = "PORCENTAJE DIFERENCIA CON TRACKER";
                dr1[2] = 0;
                dr1[3] = 0;
                dr1[4] = 0;
                dr1[5] = 0;
                dr1[6] = 0;
                dr1[7] = 0;
                dr1[8] = 0;
                dr1[9] = 0;
                dr1[10] = PorcentajeDif("ALAS");
                dta1.Rows.Add(dr1);


                //Renglon PORCENTAJE TOTAL Diferencia Con Tracker
                dr1 = dta1.NewRow();
                dr1[0] = "";
                dr1[1] = "PORCENTAJE TOTAL DE DIFERENCIA CON TRACKER";
                dr1[2] = 0;
                dr1[3] = 0;
                dr1[4] = 0;
                dr1[5] = 0;
                dr1[6] = 0;
                dr1[7] = 0;
                dr1[8] = 0;
                dr1[9] = 0;
                dr1[10] = PorcentajeDif("ALAS") + PorcentajeDif("ALFO");
                dta1.Rows.Add(dr1);
                //Renglon Porcentaje Diferencia Con Tracker
                //dr1 = dta1.NewRow();
                //dr1[0] = "";
                //dr1[1] = "PESO DIFERENCIA CON TRACKER";
                //dr1[2] = 0;
                //dr1[3] = 0;
                //dr1[4] = 0;
                //dr1[5] = 0;
                //dr1[6] = 0;
                //dr1[7] = 0;
                //dr1[8] = 0;
                //dr1[9] = 0;
                //dr1[10] = PesoDif("ALAS");
                //dta1.Rows.Add(dr1);

                // Juntar los DataTables en 1
                dtProrrateo.Merge(dtf);
                dtProrrateo.Merge(dtSalto);
                dtProrrateo.Merge(dta1);

                DataTable dtRebano;
                query = "select ia_rebano, ROUND(SUM(CONVERT(FLOAT,ia_jaulas))/ COUNT(DISTINCT ia_fecha),0) AS Jaulas, ROUND(SUM(CONVERT(FLOAT, ia_destetadas)) / COUNT(DISTINCT ia_fecha), 0) AS Destetadas, "
                    + " ROUND(SUM(CONVERT(FLOAT, ia_destetadas2)) / COUNT(DISTINCT ia_fecha), 0) AS Destetadas2, ROUND(SUM(CONVERT(FLOAT, ia_vaquillas)) / COUNT(DISTINCT ia_fecha), 0) AS Vaquillas, "
                    + " ROUND(SUM(CONVERT(FLOAT, ia_vacas_ord)) / COUNT(DISTINCT ia_fecha), 0) AS Produccion, ROUND(SUM(CONVERT(FLOAT, ia_vacas_secas+ ia_vcreto + ia_vqreto)) / COUNT(DISTINCT ia_fecha), 0) AS Secas "
                    + " from inventario_afir "
                    + " WHERE ia_fecha BETWEEN '" + periodoI.ToString("yyyy-MM-dd")  + "' AND '" + periodoF.ToString("yyyy-MM-dd") + "' AND ran_id = " + ran_id
                    + " GROUP BY ia_rebano ";
                conn.QueryAlimento(query, out dtRebano);

                if(dtRebano.Rows.Count > 1)
                {
                    DataTable dtRT;
                    DataTable[] dtPro = new DataTable[dtRebano.Rows.Count];
                    int prodT, secT, jaulasT, dest1T, dest2T, vpT, ganadoT, crianzaT, prodTAux, secTAux, jaulasTAux, dest1TAux, dest2TAux, vpTAux, ganadoTAux, crianzaTAux, rebano;
                    double produccion, secas, ganadoKg, jaulas, dest1, dest2, vqp, crianzak;
                    double p_prod, p_secas, p_ganado, p_jaulas, p_dest1, p_dest2, p_vqp, p_cza, invTotal, p_invT, invTAux;
                    

                    query = "SELECT ROUND(SUM(CONVERT(FLOAT,ia_jaulas))/ COUNT(DISTINCT ia_fecha),0) AS Jaulas, ROUND(SUM(CONVERT(FLOAT, ia_destetadas)) / COUNT(DISTINCT ia_fecha), 0) AS Destetadas, "
                        + " ROUND(SUM(CONVERT(FLOAT, ia_destetadas2)) / COUNT(DISTINCT ia_fecha), 0) AS Destetadas2, ROUND(SUM(CONVERT(FLOAT, ia_vaquillas)) / COUNT(DISTINCT ia_fecha), 0) AS Vaquillas, "
                        + " ROUND(SUM(CONVERT(FLOAT, ia_vacas_ord)) / COUNT(DISTINCT ia_fecha), 0) AS Produccion, ROUND(SUM(CONVERT(FLOAT, ia_vacas_secas + ia_vcreto + ia_vqreto)) / COUNT(DISTINCT ia_fecha), 0) AS Secas "
                        + " FROM inventario_afir "
                        + " WHERE ia_fecha BETWEEN '" + periodoI.ToString("yyyy-MM-dd") + "' AND '" + periodoF.ToString("yyyy-MM-dd") + "' AND ran_id = " +  ran_id;
                    conn.QueryAlimento(query, out dtRT);

                    jaulasT = Convert.ToInt32(dtRT.Rows[0][0]);
                    dest1T = Convert.ToInt32(dtRT.Rows[0][1]);
                    dest2T = Convert.ToInt32(dtRT.Rows[0][2]);
                    vpT = Convert.ToInt32(dtRT.Rows[0][3]);
                    prodT = Convert.ToInt32(dtRT.Rows[0][4]);
                    secT = Convert.ToInt32(dtRT.Rows[0][5]);
                    ganadoT = prodT + secT;
                    crianzaT = jaulasT + dest1T + dest2T + vpT;
                    invTotal = jaulasT + dest1T + dest2T + vpT + prodT + secT;


                    double jau, d1, d2, vp, p, rs, bal= 0, tf, inv= 0;
                   for(int i = 0; i < dtRebano.Rows.Count; i++)
                   {
                        rebano = Convert.ToInt32(dtRebano.Rows[i][0]);
                        jaulasTAux = Convert.ToInt32(dtRebano.Rows[i][1]);
                        dest1TAux = Convert.ToInt32(dtRebano.Rows[i][2]);
                        dest2TAux = Convert.ToInt32(dtRebano.Rows[i][3]);
                        vpTAux = Convert.ToInt32(dtRebano.Rows[i][4]);
                        prodTAux = Convert.ToInt32(dtRebano.Rows[i][5]);
                        secTAux = Convert.ToInt32(dtRebano.Rows[i][6]);
                        ganadoTAux = prodTAux + secTAux;
                        crianzaTAux = jaulasTAux + dest1TAux + dest2TAux + vpTAux;
                        invTAux = ganadoTAux + crianzaTAux;
                        ColumnasDT(out dtPro[i]);
                        
                        for(int j = 0; j < dtProrrateo.Rows.Count; j++)
                        {                            
                            if(dtProrrateo.Rows[j][1].ToString().Length != 0)
                            {
                                produccion = Convert.ToDouble(dtProrrateo.Rows[j][7]);
                                secas = Convert.ToDouble(dtProrrateo.Rows[j][8]);
                                ganadoKg = Convert.ToDouble(dtProrrateo.Rows[j][9]);
                                jaulas = Convert.ToDouble(dtProrrateo.Rows[j][2]);
                                dest1 = Convert.ToDouble(dtProrrateo.Rows[j][3]);
                                dest2 = Convert.ToDouble(dtProrrateo.Rows[j][4]);
                                vqp = Convert.ToDouble(dtProrrateo.Rows[j][5]);
                                crianzak = Convert.ToDouble(dtProrrateo.Rows[j][6]);

                                DataRow row = dtPro[i].NewRow();
                                switch (dtProrrateo.Rows[j][1].ToString())
                                {
                                    case "PORCENTAJE DIFERENCIA CON TRACKER":
                                        p = Convert.ToDouble(dtProrrateo.Rows[j][10]);
                                        p_invT = (invTAux / invTotal * 1.0);
                                        bal = p * p_invT;
                                        row[0] = dtProrrateo.Rows[j][0];
                                        row[1] = dtProrrateo.Rows[j][1];
                                        row[2] = dtProrrateo.Rows[j][2];
                                        row[3] = dtProrrateo.Rows[j][3];
                                        row[4] = dtProrrateo.Rows[j][4];
                                        row[5] = dtProrrateo.Rows[j][5];
                                        row[6] = dtProrrateo.Rows[j][6];
                                        row[7] = dtProrrateo.Rows[j][7];
                                        row[8] = dtProrrateo.Rows[j][8];
                                        row[9] = dtProrrateo.Rows[j][9];
                                        row[10] = bal;
                                        break;
                                    case "PORCENTAJE TOTAL DE DIFERENCIA CON TRACKER":
                                        p = Convert.ToDouble(dtProrrateo.Rows[j][10]);
                                        p_invT = (invTAux / invTotal * 1.0);
                                        bal = p * p_invT;
                                        row[0] = dtProrrateo.Rows[j][0];
                                        row[1] = dtProrrateo.Rows[j][1];
                                        row[2] = dtProrrateo.Rows[j][2];
                                        row[3] = dtProrrateo.Rows[j][3];
                                        row[4] = dtProrrateo.Rows[j][4];
                                        row[5] = dtProrrateo.Rows[j][5];
                                        row[6] = dtProrrateo.Rows[j][6];
                                        row[7] = dtProrrateo.Rows[j][7];
                                        row[8] = dtProrrateo.Rows[j][8];
                                        row[9] = dtProrrateo.Rows[j][9];
                                        row[10] = bal;
                                        break;
                                    case "TOTAL BASCULA":
                                        p = Convert.ToDouble(dtProrrateo.Rows[j][10]);
                                        p_prod = (ganadoTAux + crianzaTAux) / ((ganadoT + crianzaT) * 1.0);
                                        bal = p * p_prod;
                                        row[0] = dtProrrateo.Rows[j][0];
                                        row[1] = dtProrrateo.Rows[j][1];
                                        row[2] = dtProrrateo.Rows[j][2];
                                        row[3] = dtProrrateo.Rows[j][3];
                                        row[4] = dtProrrateo.Rows[j][4];
                                        row[5] = dtProrrateo.Rows[j][5];
                                        row[6] = dtProrrateo.Rows[j][6];
                                        row[7] = dtProrrateo.Rows[j][7];
                                        row[8] = dtProrrateo.Rows[j][8];
                                        row[9] = dtProrrateo.Rows[j][9];
                                        row[10] = bal;
                                    break;
                                    case "TOTAL FALTANTE BASCULA":
                                        p = Convert.ToDouble(dtPro[i].Rows[j - 2][10]) - Convert.ToDouble(dtPro[i].Rows[j-1][10]);
                                        row[0] = dtProrrateo.Rows[j][0];
                                        row[1] = dtProrrateo.Rows[j][1];
                                        row[2] = dtProrrateo.Rows[j][2];
                                        row[3] = dtProrrateo.Rows[j][3];
                                        row[4] = dtProrrateo.Rows[j][4];
                                        row[5] = dtProrrateo.Rows[j][5];
                                        row[6] = dtProrrateo.Rows[j][6];
                                        row[7] = dtProrrateo.Rows[j][7];
                                        row[8] = dtProrrateo.Rows[j][8];
                                        row[9] = dtProrrateo.Rows[j][9];
                                        row[10] = p;
                                        break;
                                    case "TOTAL INVENTARIO":
                                        tf = Convert.ToDouble(dtPro[i].Rows[j - 1][10]);
                                        p = Convert.ToDouble(dtProrrateo.Rows[j][10]);
                                        p_prod = (ganadoTAux + crianzaTAux) / ((ganadoT + crianzaT) * 1.0);
                                        bal = p * p_prod;
                                        row[0] = dtProrrateo.Rows[j][0];
                                        row[1] = dtProrrateo.Rows[j][1];
                                        row[2] = dtProrrateo.Rows[j][2];
                                        row[3] = dtProrrateo.Rows[j][3];
                                        row[4] = dtProrrateo.Rows[j][4];
                                        row[5] = dtProrrateo.Rows[j][5];
                                        row[6] = dtProrrateo.Rows[j][6];
                                        row[7] = dtProrrateo.Rows[j][7];
                                        row[8] = dtProrrateo.Rows[j][8];
                                        row[9] = dtProrrateo.Rows[j][9];
                                        row[10] = bal;
                                        break;
                                    case "TOTAL FALTANTE INVENTARIO":
                                        p = Convert.ToDouble(dtPro[i].Rows[j - 2][10]) - Convert.ToDouble(dtPro[i].Rows[j - 1][10]);
                                        row[0] = dtProrrateo.Rows[j][0];
                                        row[1] = dtProrrateo.Rows[j][1];
                                        row[2] = dtProrrateo.Rows[j][2];
                                        row[3] = dtProrrateo.Rows[j][3];
                                        row[4] = dtProrrateo.Rows[j][4];
                                        row[5] = dtProrrateo.Rows[j][5];
                                        row[6] = dtProrrateo.Rows[j][6];
                                        row[7] = dtProrrateo.Rows[j][7];
                                        row[8] = dtProrrateo.Rows[j][8];
                                        row[9] = dtProrrateo.Rows[j][9];
                                        row[10] = p;
                                        break;
                                    default:
                                        p_prod = prodTAux / (prodT * 1.0);
                                        p_jaulas = jaulasTAux / (1.0 * jaulasT);
                                        p_secas = secTAux / (secT * 1.0);
                                        p_ganado = ganadoTAux / (ganadoT * 1.0);
                                        p_dest1 = dest1TAux / (dest1T * 1.0);
                                        p_dest2 = dest2TAux / (dest2T * 1.0);
                                        p_vqp = vpTAux / (vpT * 1.0);
                                        p_cza = crianzaTAux / (crianzaT * 1.0);

                                        p = produccion * p_prod;
                                        jau = jaulas * p_jaulas;
                                        d1 = dest1 * p_dest1;
                                        d2 = dest2 * p_dest2;
                                        vp = vqp * p_vqp;
                                        rs = secas * p_secas;


                                        row[0] = dtProrrateo.Rows[j][0];
                                        row[1] = dtProrrateo.Rows[j][1];
                                        row[2] = jau;
                                        row[3] = d1;
                                        row[4] = d2;
                                        row[5] = vp;
                                        row[6] = jau + d1 + d2 + vp;
                                        row[7] = p;
                                        row[8] = rs;
                                        row[9] = p + rs;
                                        row[10] = jau + d1 + d2 + vp + p + rs;
                                        break;

                                }
                               
                                dtPro[i].Rows.Add(row);


                            }
                            else
                            {
                                DataRow row = dtPro[i].NewRow();
                                row[0] = "";
                                row[1] = "";
                                row[2] = 0;
                                row[3] = 0;
                                row[4] = 0;
                                row[5] = 0;
                                row[6] = 0;
                                row[7] = 0;
                                row[8] = 0;
                                row[9] = 0;
                                dtPro[i].Rows.Add(row);
                            } 
                        }                       
                    }

                   for(int i = 0; i < dtPro.Length; i++)
                    {
                        ReportDataSource source1 = new ReportDataSource("DataSet1", dtPro[i]);
                        reportViewer1.LocalReport.DataSources.Clear();
                        reportViewer1.LocalReport.DataSources.Add(source1);

                        ReportParameter[] parametros1 = new ReportParameter[3];
                        parametros1[0] = new ReportParameter("Establo", ran_nombre + " Rebaño: " + (i + 1).ToString());
                        parametros1[1] = new ReportParameter("periodo", "PERIODO DEL " + periodoI.ToString("dd/MM/yyyy") + " al " + periodoF.ToString("dd/MM/yyyy"));
                        parametros1[2] = new ReportParameter("fiabilidad", cadFiabilidad);
                        reportViewer1.LocalReport.SetParameters(parametros1);

                        reportViewer1.LocalReport.Refresh();
                        reportViewer1.RefreshReport();

                        GTHUtils.SavePDF(reportViewer1, ruta + "PRORRATEO_" + ran_nombre + "_" + (i + 1).ToString() + ".pdf");
                        Process.Start(ruta + "PRORRATEO_" + ran_nombre + "_" + (i + 1).ToString() + ".pdf");
                    }
                }              
               
                ReportDataSource source = new ReportDataSource("DataSet1", dtProrrateo);
                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(source);

                string nombre =  ran_id == 3 ? "PEDREÑA" : ran_nombre;
                ReportParameter[] parametros = new ReportParameter[3];
                parametros[0] = new ReportParameter("Establo", tituloReporte);
                parametros[1] = new ReportParameter("periodo", "PERIODO DEL " + periodoI.ToString("dd/MM/yyyy") + " al " + periodoF.ToString("dd/MM/yyyy"));
                parametros[2] = new ReportParameter("fiabilidad", cadFiabilidad);
                reportViewer1.LocalReport.SetParameters(parametros);

                reportViewer1.LocalReport.Refresh();
                reportViewer1.RefreshReport();

                GTHUtils.SavePDF(reportViewer1, ruta + "PRORRATEO_" + ran_nombre + ".pdf");
                Process.Start(ruta + "PRORRATEO_" + ran_nombre + ".pdf");
                checkBox1.Visible = true;
                textBox1.Enabled = true;
                label3.Visible = true;
                if(DateTime.Today.Day > 2 && DateTime.Today.Day < 6)
                {
                    label4.Visible = true;
                    textBox1.Visible = true;
                    button2.Visible = true;
                }
                textBox1.Focus();
              
            }
        }

        private void CalificacionesProrrateo()
        {
            double alimento, forraje;
            DataTable dt;
            string query = "SELECT  ( select Convert(FLOAT,SUM(IIF(pro_dif >= -10 AND pro_dif  <= 10,1,0))) / COUNT(*) from prorrateo "
                        + " WHERE SUBSTRING(art_clave,1,4) LIKE 'ALAS' AND ran_id = " + ran_id.ToString() + " AND pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd")+ "') Alimento,  "
                        + " (select Convert(FLOAT, SUM(IIF(pro_dif >= -10 AND pro_dif <= 10, 1, 0))) / COUNT(*) from prorrateo "
                         + " WHERE SUBSTRING(art_clave, 1, 4) LIKE 'ALFO' AND ran_id = " + ran_id.ToString() + " AND pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "') Forraje";
            conn.QueryAlimento(query, out dt);
            alimento = Convert.ToDouble(dt.Rows[0][0]); alimento = Math.Round(alimento, 2);
            forraje = Convert.ToDouble(dt.Rows[0][1]); forraje = Math.Round(forraje, 2);
            int juliana = ConvertToJulian(racionF);

            if (ExisteCalificacion(juliana))
            {
                query = "UPDATE calificacion_prorrateo SET alimento = " + alimento.ToString() + ", forraje = " + forraje
                    + " WHERE FECHA  = " + juliana + " AND ESTABLO = " + ran_id;
                conn.UpdateMovsio(query);
            }
            else
            {
                query = "INSERT INTO calificacion_prorrateo (ESTABLO, FECHA, ALIMENTO, FORRAJE) "
                    + " VALUES( " + ran_id.ToString() + "," + juliana + "," + alimento + "," + forraje + ")";
                conn.InsertMovsio(query);

            }
        }
        public static int ConvertToJulian(DateTime Date)
        {
            TimeSpan ts = (Date - Convert.ToDateTime("01/01/1900"));
            int julianday = ts.Days + 2;
            return julianday;
        }

        private bool ExisteCalificacion(int juliana)
        {
            DataTable dt;
            string query = "SELECT * FROM calificacion_prorrateo WHERE FECHA = " + juliana;
            conn.QueryMovGanado(query, out dt);

            return dt.Rows.Count > 0;
        }

        private void getRuta()
        {
            DataTable dt;
            string query = "SELECT rut_ruta FROM ruta where ran_id = " + ran_id;
            conn.QuerySIO(query, out dt);

            ruta = dt.Rows[0][0].ToString();
        }

        private void getDates()
        {
            // Racion
            DateTime temp = new DateTime(fecha.Year, fecha.Month, 1);
            hora_corte = hora_corte == 24 ? 0: hora_corte;
            racionI = hora_corte == 0 ? temp: temp.AddDays(dias_a);
            racionI = new DateTime(racionI.Year, racionI.Month, racionI.Day, hora_corte, 0, 0);
            racionF = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora_corte, 0, 0);
            racionF = racionI.Day == 1 && racionI.Hour > 0 ? racionF.AddDays(1) : racionF;
            //bascula
            balIni = racionI.AddDays(1);

            //
            periodoI = temp;
            periodoF = fecha;
            // sie
            sie = fecha;

        }

        private void Exportar_Prorrateo_Load(object sender, EventArgs e)
        {
            conn.Iniciar("DBSIE");
            ran_numero = ran_id > 9 ? ran_id.ToString() : "0" + ran_id.ToString();
            GetParameters();
            getDates();
            getRuta();
            label5.Visible = false;
            textBox2.Visible = false;
            button2.Enabled = false;
            if (DateTime.Today.Day != 4)
                textBox1.Enabled = false;
            label1.Text = tituloReporte;
            button1.Cursor = Cursors.Hand;
            button2.Cursor = Cursors.Hand;
            button3.Cursor = Cursors.Hand;
            checkBox1.Cursor = Cursors.Hand;
            //button1.Enabled = ValidarProrrateo();
            textBox1.PasswordChar = '\u25CF';
            textBox1.Cursor = Cursors.Hand;
            textBox2.PasswordChar = '\u25CF';
            textBox2.Cursor = Cursors.Hand;
            if (forma)
            {
                this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            }
            fechaR = new DateTime(DateTime.Today.Year, DateTime.Today.Month,1);
            fechaR = DateTime.Today.Day >= 1 && DateTime.Today.Day < 6 ? fechaR.AddDays(-1) : fechaR.AddMonths(1).AddDays(-1);
            fechaR = fecha;
            cadFiabilidad = fiabilidad ? "FIABILIDAD ALTA" : "FIABILIDAD BAJA";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Correo correo = new Correo(ran_id, ran_nombre, emp_id, emp_nombre);
            correo.Show();
        }
        private void EnviarCorreo(DataTable correo, DataTable reporte, string origen, string mensaje)
        {
            for (int i = 0; i < correo.Rows.Count; i++)
            {
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.To.Add(correo.Rows[i][0].ToString());
                msg.Subject = origen;
                msg.SubjectEncoding = System.Text.Encoding.UTF8;

                msg.Body = mensaje;
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.IsBodyHtml = false;
                msg.From = new System.Net.Mail.MailAddress("sistemas@gth.com.mx");

                foreach (DataRow row in reporte.Rows)
                {
                    try
                    {
                        Attachment data = new Attachment(row[0].ToString(), MediaTypeNames.Application.Octet);
                        msg.Attachments.Add(data);
                    }
                    catch { }
                }

                System.Net.Mail.SmtpClient cliente = new System.Net.Mail.SmtpClient();
                cliente.Credentials = new System.Net.NetworkCredential("sistemas@gth.com.mx", "sis06prestadora_");
                cliente.Port = 587;

                cliente.Host = "smtp.gth.com.mx";
                try
                {
                    cliente.Send(msg);
                }
                catch (System.Net.Mail.SmtpException ex)
                {
                    MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);

                }

            }

        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                label5.Visible = true;
                textBox2.Visible = true;
                textBox2.Focus();
            }
            else
            {
                label5.Visible = false;
                textBox2.Visible = false;
            }
        }
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox2.Text.ToUpper() == "HCC123")
                {
                    textBox1.Enabled = true;
                    textBox2.Text = "";
                    textBox2.Focus();
                    checkBox1.Checked = !checkBox1.Checked;
                    textBox1.Visible = true;
                    label4.Visible = true;
                    textBox1.Focus();
                    button2.Visible = true;
                }
                else
                {
                    textBox2.Text = "";
                    MessageBox.Show("Contraaseña Incorrecta", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
                }
            }
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox1.Text.ToUpper() == pas.ToUpper())
                {
                    button2.Enabled = true;
                }
                else
                {
                    textBox1.Text = "";
                    textBox1.Focus();
                }
            }
        }

        private void ReporteForraje()
         {
            DateTime ini = new DateTime(fechaR.Year, fechaR.Month, 1);
            DataTable dt, dtF;
            string query = "SELECT FORMAT(pro_fecha, 'd','en-gb'), art_clave, prod_nombre, pro_existencia_sie, pro_inv_final, pro_consumo, pro_porc_b, pro_porc_t, pro_consumo_tra, "
                        + " pro_dif_kg, pro_dif, pro_bascula, pro_consumo_ext "
                        + " from prorrateo "
                        + " WHERE ran_id = " + ran_id + " AND pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "' AND SUBSTRING(art_clave,1,4) IN('ALFO')";
            conn.QueryAlimento(query, out dt);

            
            ColumnasForraje(tipoP, out dtF);

            string periodo;
            double existencia, tracker, bascula, temp, t, dif;
            double basculaT = 0, pT = 0, tT = 0, trackerT = 0, pdif;
            if (tipoP == 3)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    existencia = Convert.ToDouble(dt.Rows[i][3]);
                    tracker = Convert.ToDouble(dt.Rows[i][8]); trackerT += tracker;
                    bascula = Convert.ToDouble(dt.Rows[i][5]); basculaT += bascula;
                    pT += Convert.ToDouble(dt.Rows[i][6]);
                    tT += Convert.ToDouble(dt.Rows[i][7]);
                    temp = tracker > 0 ? bascula : 0;
                    DataRow row = dtF.NewRow();
                    row["FECHA"] = dt.Rows[i][0].ToString();
                    row["CLAVE"] = dt.Rows[i][1].ToString();
                    row["ARTICULO"] = dt.Rows[i][2].ToString();
                    row["EXISTENCIASIE"] = Convert.ToDouble(dt.Rows[i][3]).ToString("#,0.0");
                    row["INVFINAL"] = Convert.ToDouble(dt.Rows[i][4]).ToString("#,0");
                    row["BASCULA"] = Convert.ToDouble(dt.Rows[i][5]).ToString("#,0");
                    row["P"] = Convert.ToDouble(dt.Rows[i][6]).ToString("#,0.0");
                    row["T"] = Convert.ToDouble(dt.Rows[i][7]).ToString("#,0.0");
                    row["TRACKER"] = Convert.ToDouble(dt.Rows[i][8]).ToString("#,0");
                    row["EXISTENCIA"] = Convert.ToDouble(dt.Rows[i][9]).ToString("#,0.0");
                    row["DIFKG"] = Convert.ToDouble(dt.Rows[i][10]).ToString("#,0.0");
                    row["DIFPORC"] = Convert.ToDouble(dt.Rows[i][11]).ToString("#,0.0");
                    dtF.Rows.Add(row);
                }

                DataRow dr = dtF.NewRow();
                dr["ARTICULO"] = "TOTAL";
                dr["BASCULA"] = basculaT.ToString("#,0.0");
                dr["P"] = pT.ToString("#,0.0");
                dr["T"] = tT.ToString("#,0.0");
                dr["TRACKER"] = trackerT.ToString("#,0.0");
                dr["DIFKG"] = (basculaT - trackerT).ToString("#,0.0");
                dr["DIFPORC"] = ((basculaT - trackerT) / trackerT).ToString("#,0.0");
                dtF.Rows.Add(dr);

                ReportDataSource source1 = new ReportDataSource("DataSet1", dtF);//<- es el dt que tiene existencia 
                reportViewer4.LocalReport.DataSources.Clear();
                reportViewer4.LocalReport.DataSources.Add(source1);

                ReportParameter[] parametros = new ReportParameter[3];
                parametros[0] = new ReportParameter("ESTABLO", "ESTABLO: " + ran_nombre.ToUpper(), true);
                periodo = "PERIODO DEL: " + ini.ToString("dd/MM/yyyy") + " al " + fechaR.ToString("dd/MM/yyyy");
                parametros[1] = new ReportParameter("PERIODO", periodo, true);
                parametros[2] = new ReportParameter("BASCULA", ran_bascula.ToString(), true);
                reportViewer4.LocalReport.SetParameters(parametros);
                reportViewer4.LocalReport.Refresh();
                reportViewer4.RefreshReport();

                GTHUtils.SavePDF(reportViewer4, ruta + "FORRAJE SIE Y BASCULA_" + ran_nombre + ".pdf");

            }
            else if (tipoP == 4)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    existencia = Convert.ToDouble(dt.Rows[i][3]);
                    tracker = Convert.ToDouble(dt.Rows[i][8]); trackerT += tracker;
                    bascula = Convert.ToDouble(dt.Rows[i][5]); basculaT += bascula;
                    pT = Convert.ToDouble(dt.Rows[i][6]);
                    t = Convert.ToDouble(dt.Rows[i][7]); tT += t;
                    dif = Convert.ToDouble(dt.Rows[i][10]);
                    temp = tracker > 0 ? bascula : 0;
                    DataRow row = dtF.NewRow();
                    row["FECHA"] = dt.Rows[i][0].ToString();
                    row["CLAVE"] = dt.Rows[i][1].ToString();
                    row["ARTICULO"] = dt.Rows[i][2].ToString();
                    row["EXISTENCIASIE"] = Convert.ToDouble(dt.Rows[i][3]).ToString("#,0.0");
                    row["CONREAL"] = Convert.ToDouble(dt.Rows[i][5]).ToString("#,0");
                    row["P"] = Convert.ToDouble(dt.Rows[i][6]).ToString("#,0.0");
                    row["T"] = Convert.ToDouble(dt.Rows[i][7]).ToString("#,0.0");
                    row["TRACKER"] = Convert.ToDouble(dt.Rows[i][8]).ToString("#,0");
                    row["EXISTENCIA"] = existencia >= temp ? "si" : "no";
                    row["DIFKG"] = Convert.ToDouble(dt.Rows[i][9]).ToString("#,0.0");
                    row["DIFPORC"] = Convert.ToDouble(dt.Rows[i][10]).ToString("#,0.0");
                    row["BASCULA"] = Convert.ToDouble(dt.Rows[i][11]).ToString("#,0");
                    row["CONEXTERNO"] = Convert.ToDouble(dt.Rows[i][12]).ToString("#,0.0");

                    if (t >= porcT)
                    {
                        if (dif >= porcDif || dif <= (porcDif * -1))
                            row["T_COLOR"] = true;
                        else
                            row["T_COLOR"] = false;
                    }
                    else
                        row["T_COLOR"] = false;

                    dtF.Rows.Add(row);
                }

                DataRow dr = dtF.NewRow();
                dr["ARTICULO"] = "TOTAL";
                dr["BASCULA"] = basculaT.ToString("#,0.0");
                dr["P"] = pT.ToString("#,0.0");
                dr["T"] = tT.ToString("#,0.0");
                dr["TRACKER"] = trackerT.ToString("#,0.0");
                dr["DIFKG"] = (basculaT - trackerT).ToString("#,0.0");
                dr["DIFPORC"] = ((basculaT - trackerT) / trackerT).ToString("#,0.0");
                dtF.Rows.Add(dr);

                ReportDataSource source1 = new ReportDataSource("DataSet1", dtF);
                reportViewer5.LocalReport.DataSources.Clear();
                reportViewer5.LocalReport.DataSources.Add(source1);

                ReportParameter[] parametros = new ReportParameter[3];
                parametros[0] = new ReportParameter("ESTABLO", "ESTABLO: " + ran_nombre.ToUpper(), true);
                periodo = "PERIODO DEL: " + ini.ToString("dd/MM/yyyy") + " al " + fechaR.ToString("dd/MM/yyyy");
                parametros[1] = new ReportParameter("PERIODO", periodo, true);
                parametros[2] = new ReportParameter("BASCULA", ran_bascula.ToString(), true);
                reportViewer5.LocalReport.SetParameters(parametros);
                reportViewer5.LocalReport.Refresh();
                reportViewer5.RefreshReport();

                GTHUtils.SavePDF(reportViewer5, ruta + "FORRAJE SIE Y BASCULA_" + ran_nombre + ".pdf");
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    existencia = Convert.ToDouble(dt.Rows[i][3]);
                    tracker = Convert.ToDouble(dt.Rows[i][8]); trackerT += tracker;
                    bascula = Convert.ToDouble(dt.Rows[i][5]); basculaT += bascula;
                    pT += Convert.ToDouble(dt.Rows[i][6]);
                    tT += Convert.ToDouble(dt.Rows[i][7]);
                    temp = tracker > 0 ? bascula : 0;
                    DataRow row = dtF.NewRow();
                    row["FECHA"] = dt.Rows[i][0].ToString();
                    row["CLAVE"] = dt.Rows[i][1].ToString();
                    row["ARTICULO"] = dt.Rows[i][2].ToString();
                    row["EXISTENCIASIE"] = Convert.ToDouble(dt.Rows[i][3]).ToString("#,0.0");
                    row["BASCULA"] = Convert.ToDouble(dt.Rows[i][5]).ToString("#,0.0");
                    row["P"] = Convert.ToDouble(dt.Rows[i][6]).ToString("#,0.0");
                    row["T"] = Convert.ToDouble(dt.Rows[i][7]).ToString("#,0.0");
                    row["TRACKER"] = Convert.ToDouble(dt.Rows[i][8]).ToString("#,0.0");
                    row["EXISTENCIA"] = existencia >= temp ? "si" : "no";
                    row["DIFKG"] = Convert.ToDouble(dt.Rows[i][9]).ToString("#,0.0");
                    row["DIFPORC"] = Convert.ToDouble(dt.Rows[i][10]).ToString("#,0.0");
                    dtF.Rows.Add(row);
                }

                DataRow dr = dtF.NewRow();
                dr["ARTICULO"] = "TOTAL";
                dr["BASCULA"] = basculaT.ToString("#,0");
                dr["P"] = pT.ToString("#,0.0");
                dr["T"] = tT.ToString("#,0.0");
                dr["TRACKER"] = trackerT.ToString("#,0.0");
                dr["DIFKG"] = (basculaT - trackerT).ToString("#,0.0");
                dr["DIFPORC"] = ((basculaT - trackerT) / trackerT * 100).ToString("#,0.0");
                dtF.Rows.Add(dr);


                ReportDataSource source1 = new ReportDataSource("DataSet1", dtF);
                reportViewer3.LocalReport.DataSources.Clear();
                reportViewer3.LocalReport.DataSources.Add(source1);

                ReportParameter[] parametros = new ReportParameter[4];
                parametros[0] = new ReportParameter("ESTABLO", "ESTABLO: " + ran_nombre.ToUpper(), true);
                periodo = "PERIODO DEL: " + ini.ToString("dd/MM/yyyy") + " al " + fechaR.ToString("dd/MM/yyyy");
                parametros[1] = new ReportParameter("PERIODO", periodo, true);
                parametros[2] = new ReportParameter("BASCULA", ran_bascula.ToString(), true);
                parametros[3] = new ReportParameter("TIPO", tipoP.ToString(), true); //<- poner variable tipo
                reportViewer3.LocalReport.SetParameters(parametros);
                reportViewer3.LocalReport.Refresh();
                reportViewer3.RefreshReport();

                GTHUtils.SavePDF(reportViewer3, ruta + "FORRAJE SIE Y BASCULA_" + ran_nombre + ".pdf");
            }
        }
        
        private void ColumnasForraje(int tipo, out DataTable dt)
        {
            dt = new DataTable();
            if(tipo == 3)
            {
                dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("EXISTENCIASIE").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("INVFINAL").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("BASCULA").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("P").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("T").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("EXISTENCIA").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("DIFKG").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("DIFPORC").DataType = System.Type.GetType("System.String");
            }
            else if(tipo == 4)
            {
                dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("EXISTENCIASIE").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("CONREAL").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("P").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("T").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("EXISTENCIA").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("DIFKG").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("DIFPORC").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("BASCULA").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("CONEXTERNO").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("T_COLOR").DataType = System.Type.GetType("System.Boolean");
            }
            else
            {
                dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("EXISTENCIASIE").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("BASCULA").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("P").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("T").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("TRACKER").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("EXISTENCIA").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("DIFKG").DataType = System.Type.GetType("System.String");
                dt.Columns.Add("DIFPORC").DataType = System.Type.GetType("System.String");
            }
        }



        private void ColumnasAlimento(out DataTable dt)
        {
            dt = new DataTable();
            dt.Columns.Add("FECHA").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("ALMACEN").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("ARTICULO").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("DISPONIBLE_SIE").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("INV_FINAL").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("CONSUMO").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("P").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("T").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("CONSUMO_TRACKER").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("DIF_KG").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("DIF").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("EXISTENCIA").DataType = System.Type.GetType("System.String");
            
        }

        private void ReporteAlimentos()
        {
            double consumo =0, p=0, t=0, tracker=0, dif=0, pdif=0;
            DateTime ini = new DateTime(racionF.Year, racionF.Month, 1);
            DataTable dt, dtA;
            ColumnasAlimento(out dtA);
            string query = "SELECT ISNULL(FORMAT(pro_fecha, 'd','en-gb'),'') AS FECHA,  ISNULL(alm_id,'') AS ALMACEN, art_clave AS CLAVE, prod_nombre AS ARTICULO, "
                + " pro_existencia_sie AS DISPONIBLE_SIE, pro_inv_final AS INV_FINAL, pro_consumo AS CONSUMO, pro_porc_b AS P, pro_porc_t AS T, " 
                + " pro_consumo_tra AS CONSUMO_TRACKER, pro_dif_kg AS DIF_KG, pro_dif AS DIF, IIF(pro_consumo <= pro_existencia_sie, 'si', 'no') "
                + " from prorrateo "
                + " WHERE ran_id = " + ran_id + " AND pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "' AND SUBSTRING(art_clave,1,4) IN('ALAS')";
            conn.QueryAlimento(query, out dt);

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                consumo += Convert.ToDouble(dt.Rows[i][6]);
                p += Convert.ToDouble(dt.Rows[i][7]);
                t += Convert.ToDouble(dt.Rows[i][8]);
                tracker += Convert.ToDouble(dt.Rows[i][9]);
            }

            dif = tracker - consumo;
            pdif = (tracker - consumo) / tracker * 100;

            for(int i = 0; i< dt.Rows.Count; i++)
            {
                DataRow dr = dtA.NewRow();
                dr[0] = dt.Rows[i][0].ToString();
                dr[1] = dt.Rows[i][1].ToString();
                dr[2] = dt.Rows[i][2].ToString();
                dr[3] = dt.Rows[i][3].ToString();
                dr[4] = Convert.ToDouble(dt.Rows[i][4]).ToString("#,0.0");
                dr[5] = Convert.ToDouble(dt.Rows[i][5].ToString()).ToString("#,0.0");
                dr[6] = Convert.ToDouble(dt.Rows[i][6].ToString()).ToString("#,0.0");
                dr[7] = Convert.ToDouble(dt.Rows[i][7].ToString()).ToString("#,0.0");
                dr[8] = Convert.ToDouble(dt.Rows[i][8].ToString()).ToString("#,0.0");
                dr[9] = Convert.ToDouble(dt.Rows[i][9].ToString()).ToString("#,0.0");
                dr[10] = Convert.ToDouble(dt.Rows[i][10].ToString()).ToString("#,0.0");
                dr[11] = Convert.ToDouble(dt.Rows[i][11].ToString()).ToString("#,0.0");
                dr[12] = dt.Rows[i][12].ToString();
                dtA.Rows.Add(dr);
            }

            DataRow row = dtA.NewRow();

            row[3] = "TOTAL";
            row[6] = consumo.ToString("#,0.0"); 
            row[7] = p.ToString("#,0.0");
            row[8] = t.ToString("#,0.0");
            row[9] = tracker.ToString("#,0.0");
            row[10] = dif.ToString("#,0.0");
            row[11] = pdif.ToString("#,0.0");
            dtA.Rows.Add(row);

            ReportDataSource source = new ReportDataSource("DataSet2", dtA);
            reportViewer2.LocalReport.DataSources.Clear();
            reportViewer2.LocalReport.DataSources.Add(source);
            ReportParameter[] parametros = new ReportParameter[2];
            parametros[0] = new ReportParameter("Establo", "ESTABLO: " + tituloReporte.ToUpper(), true);
            string periodo = "PERIODO DEL: " + ini.ToString("dd/MM/yyyy") + " al " + racionF.ToString("dd/MM/yyyy");
            parametros[1] = new ReportParameter("Periodo", periodo, true);
            reportViewer2.LocalReport.SetParameters(parametros);
            reportViewer2.LocalReport.Refresh();
            reportViewer2.RefreshReport();

            GTHUtils.SavePDF(reportViewer2, ruta + "ALIMENTO SIE Y TRACKER_" + ran_nombre + ".pdf");
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            ReporteAlimentos();
            ReporteForraje();
            Consumo();
            string query;
            DataTable reporte;
            query = "SELECT CONCAT(r.rut_ruta,rp.rep_nombre,'_',c.ran_desc,'.PDF') "
                + " FROM ruta r "
                + " LEFT JOIN tipo_ruta tru ON r.tip_ruta_id = tru.tip_ruta_id "
                + " LEFT JOIN tipo_reporte tre ON tru.tip_ruta_nombre = tre.tip_rep_nombre "
                + " LEFT JOIN reporte rp ON rp.tip_rep_id = tre.tip_rep_id "
                + " LEFT JOIN configuracion c ON r.ran_id = c.ran_id "
                + " WHERE tre.tip_rep_id IN(1,2,3) AND c.ran_id = " + ran_id;
            conn.QuerySIO(query, out reporte);

            DataTable dtR;
            query = "select DISTINCT ia_rebano from inventario_afir WHERE ran_id = " + ran_id + " AND ia_fecha BETWEEN '" + periodoI.ToString("yyyy-MM-dd") + "' AND '" + periodoF.ToString("yyyy-MM-dd") + "'";
            conn.QueryAlimento(query, out dtR);

            for(int i = 0; i < dtR.Rows.Count; i++)
            {
                DataRow row = reporte.NewRow();
                row[0] = "C:\\Movganado\\Consumos\\Reportes\\PRORRATEO_Cantri_" + (i + 1).ToString() + ".PDF";
                reporte.Rows.Add(row);
            }

            string mensaje = "Prorrateo - " + ran_nombre;//"Se anexan los reportes de PRORRATEO del establo " + ran_nombre;
            string asunto = "Reportes Consumos";

            DataTable dtcorreo;
            query = "SELECT em_mail FROM email";
            conn.QueryAlimento(query, out dtcorreo);
            SepeararProrrateo();
            GUARDARDIF();
            ExportarDll();
            if (dtcorreo.Rows.Count > 0)
            {
                EnviarCorreo(dtcorreo, reporte, mensaje, "Buen dia.\nSe anexan los reportes de prorrateo.");
                MessageBox.Show("Correos enviados exitosamente", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox1.Enabled = false;
                textBox1.Text = "";
            }
            else
                MessageBox.Show("No se agregaron correos", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
            button2.Enabled = false;

            Cursor = Cursors.Default;
            if (dtcorreo.Rows.Count > 0)
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }

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
                int repeticiones = 0;

                if (dtAux.Rows.Count == 0)
                {
                    if (prmz == "01")
                    {
                        query = "SELECT T1.Pmz, T1.Clave, T1.Ing, T1.Peso / T2.Total "
                        + " FROM( "
                        + " select T.pmez_racion AS Pmz, T.ing_clave AS Clave, T.ing_nombre AS Ing, SUM(T.pmez_peso) AS Peso "
                        + " FROM( "
                        + " SELECT DISTINCT * "
                        + " from premezcla "
                        + " where pmez_racion LIKE '" + premezcla + "' "
                        + " AND pmez_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND pmez_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "') T "
                        + " GROUP BY pmez_racion, ing_clave, ing_nombre ) T1 "
                        + " LEFT JOIN( "
                        + " SELECT T.pmez_racion AS Pmz, SUM(T.pmez_peso) AS Total "
                        + " FROM( "
                        + " SELECT DISTINCT * FROM premezcla "
                        + " WHERE pmez_racion LIKE '" + premezcla + "' "
                        + " AND pmez_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND pmez_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "') T "
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
                        fRacion = dt.Rows[0][2] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0][2]) : inicio;
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
                                    + " AND pmez_fecha >= '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND pmez_fecha < '" + fpmF.ToString("yyyy-MM-dd HH:mm") + "' ";
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
                                + " AND pmez_fecha >= '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND pmez_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                                + " AND ISNUMERIC(SUBSTRING(ing_nombre,1,1)) > 0 "
                                + " AND SUBSTRING(ing_nombre,3,2) IN('00', '01', '02')";
                        conn.QueryAlimento(query, out dtsPM);

                        //DiasPremezcla(premezcla, fpmI, fin);
                        for (int i = 0; i < dtsPM.Rows.Count; i++)
                            SupraMezcla(dtsPM.Rows[i][0].ToString(), fpmI, fin);

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
        private bool ValidarProrrateo()
        {
            DataTable dt;
            string query = "SELECT *  FROM prorrateo WHERE ran_id = " + ran_id.ToString()
                + " AND pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "'";
            conn.QueryAlimento(query, out dt);

            if (dt.Rows.Count > 0)
                return true;
            else
                return false;
        }
        private void ColumnasDT(out DataTable dt)
        {
            dt = new DataTable();
            dt.Columns.Add("CLAVE").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("INGREDIENTE").DataType = System.Type.GetType("System.String");
            dt.Columns.Add("JAULAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("DESTETADAS1").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("DESTETADAS2").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("VAQUILLAS_PRENADAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("TOTAL_KGS_CRIANZA").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("PRODUCCION").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("SECAS").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("TOTAL_KGS_GANADO").DataType = System.Type.GetType("System.Double");
            dt.Columns.Add("TOTAL").DataType = System.Type.GetType("System.Double");
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

        private void SepeararProrrateo()
        {
            if (ValidarProrrateo())
            {
                DataTable dtPremezclas = new DataTable();
                string queryPM = "select DISTINCT ing_descripcion FROM racion "
                    + " WHERE rac_fecha BETWEEN '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' "
                    + " AND ran_id IN(" + ranchosId + ") "
                    + " AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02') "
                    + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F') AND SUBSTRING(rac_descripcion,3,2) NOT IN('00','01','02')";
                conn.QueryAlimento(queryPM, out dtPremezclas);

                conn.DeleteAlimento("porcentaje_Premezcla", "");
                for (int i = 0; i < dtPremezclas.Rows.Count; i++)
                {
                    CargarPremezcla(dtPremezclas.Rows[i][0].ToString(), racionI, racionF);
                }

                string[] ranchos = ranchosId.Split(',');
                string query = "";
                DataTable dt;

                string almA = Almacen(ran_id.ToString(), "2");
                string almF = Almacen(ran_id.ToString(), "3");
                string almG = "";
                string valores = "";
                int cont = 0;

                string almacen, fecha, clave, almaceng, etapa;
                double kilos;
                
                if(ran_id != 25)
                {
                    conn.DeleteAlimento("prorrateo_sie", "WHERE alm_id = '" + almA.ToString() + "' AND ps_fecha = '" + fechaR.ToString("yyyy-MM-dd") + "'");
                    conn.DeleteAlimento("prorrateo_sie", "WHERE alm_id = '" + almF.ToString() + "' AND ps_fecha = '" + fechaR.ToString("yyyy-MM-dd") + "'");
                    for (int i = 0; i < ranchos.Length; i++)
                    {
                        almG = Almacen(ranchos[i].ToString(), "5");
                        query = "SELECT T.CLAVE, T.Jaulas, T.Dest1, T.Dest2, T.Vqp, T.Prod, T.Secas  "
                        + " FROM( "
                        + " SELECT R.CLAVE, R.PorcJaulas * Bascula.Peso AS Jaulas, R.PorcDest1 * Bascula.Peso AS Dest1, R.PorcDest2 * Bascula.Peso AS Dest2, "
                        + " R.PorcVp* Bascula.Peso AS Vqp, R.PorcProd* Bascula.Peso AS Prod, R.PorcSecas* Bascula.Peso AS Secas "
                        + " FROM( "
                        + " SELECT T1.CLAVE, ISNULL(T1.JAULAS / T2.TOTAL, 0) AS PorcJaulas, ISNULL(T1.DESTETADAS1 / T2.TOTAL, 0) AS PorcDest1, ISNULL(T1.DESTATADAS2 / T2.TOTAL, 0) AS PorcDest2, "
                        + " ISNULL(T1.VAQUILLAS_PRENADAS / T2.TOTAL, 0) AS PorcVp, ISNULL(T1.PRODUCCION / T2.TOTAL, 0) AS PorcProd, ISNULL(T1.SECAS / T2.TOTAL, 0) AS PorcSecas "
                        + " FROM( "
                        + " SELECT R2.CLAVE, SUM( case when R2.Etapa IN(31) THEN R2.PESO ELSE 0 END) AS JAULAS, SUM( case when R2.Etapa IN(32) THEN R2.PESO ELSE 0 END) AS DESTETADAS1, "
                        + " SUM( case when R2.Etapa IN(33) THEN R2.PESO ELSE 0 END) AS DESTATADAS2, SUM( case when R2.Etapa IN(34) THEN R2.PESO ELSE 0 END) AS VAQUILLAS_PRENADAS, "
                        + " SUM( case when R2.Etapa IN(10, 11, 12, 13) THEN R2.PESO ELSE 0 END) AS PRODUCCION, SUM( case when R2.Etapa IN(21, 22) THEN R2.PESO ELSE 0 END) AS SECAS "
                        + " FROM( "
                        + " SELECT R1.Etapa, R1.CLAVE, R1.INGREDIENTE, SUM(R1.PESO) AS PESO "
                        + " FROM( "
                        + " SELECT etp_id AS Etapa, ing_clave AS CLAVE, ing_descripcion AS INGREDIENTE, SUM(rac_mh) AS PESO "
                        + " FROM racion "
                        + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranchos[i] + ") AND SUBSTRING(ing_clave, 1, 4) IN('ALFO', 'ALAS') "
                        + " AND SUBSTRING(rac_descripcion, 3, 2) NOT IN('00', '01', '02') "
                        + " GROUP BY etp_id, ing_clave, ing_descripcion "
                        + " UNION "
                        + " SELECT R.Etapa, IIF(R.Racion = '', R.Clave1, R.Clave2) AS Clave, IIF(R.Racion = '', R.Ingrediente1, R.Ingrediente2) AS Ingrediente, R.Peso * R.Porcentaje AS Peso "
                        + " FROM( "
                        + " SELECT T1.Etapa, T1.Clave AS Clave1, T1.Ingrediente AS Ingrediente1, T1.Peso, ISNULL(T2.Racion, '') AS Racion, ISNULL(T2.Clave, '') AS Clave2, "
                        + " ISNULL(T2.Ingrediente, '') AS Ingrediente2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                        + " FROM( "
                        + " SELECT T1.Etapa, T2.Clave, T2.Ingrediente, T1.Peso * T2.Porcentaje As Peso "
                        + " FROM( "
                        + " SELECT etp_id AS Etapa, ing_descripcion AS Racion, SUM(rac_mh) AS Peso "
                        + " FROM racion "
                        + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "'  AND ran_id IN(" + ranchos[i] + ") AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                        + " AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F') AND etp_id not in (0) "
                        + " GROUP BY etp_id, ing_descripcion) T1 "
                        + " LEFT JOIN( "
                        + " SELECT pmez_descripcion AS Racion, ing_clave AS Clave, ing_descripcion AS Ingrediente, pmez_porcentaje AS Porcentaje "
                        + " FROM porcentaje_Premezcla)T2 ON T1.Racion = T2.Racion) T1 "
                        + " LEFT JOIN( "
                        + " SELECT pmez_descripcion AS Racion, ing_clave AS Clave, ing_descripcion AS Ingrediente, pmez_porcentaje AS Porcentaje "
                        + " FROM porcentaje_Premezcla "
                        + " )T2 ON T1.Ingrediente = T2.Racion ) R ) R1 "
                        + " WHERE SUBSTRING(R1.Clave, 1, 4) IN('ALFO', 'ALAS') "
                        + " GROUP BY R1.Etapa, R1.Clave, R1.INGREDIENTE ) R2 GROUP BY R2.CLAVE) T1 "
                        + " LEFT JOIN( "
                        + " SELECT R2.CLAVE, SUM( case when R2.Etapa IN(10, 11, 12, 13, 21, 22, 31, 32, 33, 34) THEN R2.PESO ELSE 0 END) AS TOTAL "
                        + " FROM( "
                        + " SELECT R1.Etapa, R1.CLAVE, R1.INGREDIENTE, SUM(R1.PESO) AS PESO  FROM(SELECT etp_id AS Etapa, ing_clave AS CLAVE, ing_descripcion AS INGREDIENTE, SUM(rac_mh) AS PESO "
                        + " FROM racion "
                        + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranchosId + ") AND SUBSTRING(ing_clave, 1, 4) IN('ALFO', 'ALAS') "
                        + " AND SUBSTRING(rac_descripcion, 3, 2) NOT IN('00', '01', '02') "
                        + " GROUP BY etp_id, ing_clave, ing_descripcion "
                        + " UNION "
                        + " SELECT R.Etapa, IIF(R.Racion = '', R.Clave1, R.Clave2) AS Clave, IIF(R.Racion = '', R.Ingrediente1, R.Ingrediente2) AS Ingrediente, R.Peso * R.Porcentaje AS Peso "
                        + " FROM( "
                        + " SELECT T1.Etapa, T1.Clave AS Clave1, T1.Ingrediente AS Ingrediente1, T1.Peso, ISNULL(T2.Racion, '') AS Racion, ISNULL(T2.Clave, '') AS Clave2, "
                        + " ISNULL(T2.Ingrediente, '') AS Ingrediente2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                        + " FROM( "
                        + " SELECT T1.Etapa, T2.Clave, T2.Ingrediente, T1.Peso * T2.Porcentaje As Peso "
                        + " FROM( "
                        + " SELECT etp_id AS Etapa, ing_descripcion AS Racion, SUM(rac_mh) AS Peso "
                        + " FROM racion "
                        + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "'  AND ran_id IN(" + ranchosId + ") AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                        + " AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F') AND etp_id not in (0) "
                        + " GROUP BY etp_id, ing_descripcion) T1 "
                        + " LEFT JOIN( "
                        + " SELECT pmez_descripcion AS Racion, ing_clave AS Clave, ing_descripcion AS Ingrediente, pmez_porcentaje AS Porcentaje "
                        + " FROM porcentaje_Premezcla)T2 ON T1.Racion = T2.Racion) T1 "
                        + " LEFT JOIN( "
                        + " SELECT pmez_descripcion AS Racion, ing_clave AS Clave, ing_descripcion AS Ingrediente, pmez_porcentaje AS Porcentaje "
                        + " FROM porcentaje_Premezcla)T2 ON T1.Ingrediente = T2.Racion ) R ) R1 "
                        + " WHERE SUBSTRING(R1.Clave, 1, 4) IN('ALFO', 'ALAS') "
                        + " GROUP BY R1.Etapa, R1.Clave, R1.INGREDIENTE ) R2 "
                        + " GROUP BY R2.CLAVE "
                        + " )T2 ON T1.Clave = T2.Clave) R "
                        + " LEFT JOIN( "
                        + " SELECT art_clave AS Clave, pro_consumo AS Peso "
                        + " FROM prorrateo "
                        + " where pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranchosId + ")  AND SUBSTRING(art_clave, 1, 4) IN('ALFO', 'ALAS') "
                        + " )Bascula ON R.CLAVE = Bascula.Clave) T "
                        + " WHERE T.Jaulas > 0 OR T.Dest1 > 0 OR T.Dest2 > 0 OR T.Vqp > 0 OR T.Prod > 0 OR T.Secas > 0";
                        conn.QueryAlimento(query, out dt);

                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            clave = dt.Rows[j][0].ToString();
                            if (clave.Substring(0, 4) == "ALAS")
                            {
                                kilos = Convert.ToDouble(dt.Rows[j][1]);
                                valores += "('" + almA + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C1'," + kilos + "),";
                                kilos = Convert.ToDouble(dt.Rows[j][2]);
                                valores += "('" + almA + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C2'," + kilos + "),";
                                kilos = Convert.ToDouble(dt.Rows[j][3]);
                                valores += "('" + almA + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C3'," + kilos + "),";
                                kilos = Convert.ToDouble(dt.Rows[j][4]);
                                valores += "('" + almA + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C4'," + kilos + "),";
                                kilos = Convert.ToDouble(dt.Rows[j][5]);
                                valores += "('" + almA + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','P1'," + kilos + "),";
                                kilos = Convert.ToDouble(dt.Rows[j][6]);
                                valores += "('" + almA + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','P1'," + kilos + "),";
                            }
                            else
                            {
                                kilos = Convert.ToDouble(dt.Rows[j][1]);
                                valores += "('" + almF + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C1'," + kilos + "),";
                                kilos = Convert.ToDouble(dt.Rows[j][2]);
                                valores += "('" + almF + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C2'," + kilos + "),";
                                kilos = Convert.ToDouble(dt.Rows[j][3]);
                                valores += "('" + almF + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C3'," + kilos + "),";
                                kilos = Convert.ToDouble(dt.Rows[j][4]);
                                valores += "('" + almF + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C4'," + kilos + "),";
                                kilos = Convert.ToDouble(dt.Rows[j][5]);
                                valores += "('" + almF + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','P1'," + kilos + "),";
                                kilos = Convert.ToDouble(dt.Rows[j][6]);
                                valores += "('" + almF + "','" + fechaR.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','P1'," + kilos + "),";

                            }
                            conn.InsertMasivAlimento("prorrateo_sie", valores.Substring(0, valores.Length - 1));
                            valores = "";
                        }
                    }
                }
                else
                {
                    conn.DeleteAlimento("prorrateo_sie", "WHERE alm_id = 'A40002' AND ps_fecha = '" + racionF.ToString("yyyy-MM-dd") + "'");
                    conn.DeleteAlimento("prorrateo_sie", "WHERE alm_id = 'A40003' AND ps_fecha = '" + racionF.ToString("yyyy-MM-dd") + "'");
                    conn.DeleteAlimento("prorrateo_sie", "WHERE alm_id = 'A41002' AND ps_fecha = '" + racionF.ToString("yyyy-MM-dd") + "'");
                    conn.DeleteAlimento("prorrateo_sie", "WHERE alm_id = 'A41003' AND ps_fecha = '" + racionF.ToString("yyyy-MM-dd") + "'");

                    int prodT, secT, jaulasT, dest1T, dest2T, vpT, ganadoT, crianzaT, prodTAux, secTAux, jaulasTAux, dest1TAux, dest2TAux, vpTAux, ganadoTAux, crianzaTAux, rebano;
                    double p_prod, p_secas, p_ganado, p_jaulas, p_dest1, p_dest2, p_vqp, p_cza;
                    double jau, d1, d2, vp, p, rs, bal = 0, tf, inv = 0;
                    query = "SELECT T.CLAVE, T.Jaulas, T.Dest1, T.Dest2, T.Vqp, T.Prod, T.Secas  "
                       + " FROM( "
                       + " SELECT R.CLAVE, R.PorcJaulas * Bascula.Peso AS Jaulas, R.PorcDest1 * Bascula.Peso AS Dest1, R.PorcDest2 * Bascula.Peso AS Dest2, "
                       + " R.PorcVp* Bascula.Peso AS Vqp, R.PorcProd* Bascula.Peso AS Prod, R.PorcSecas* Bascula.Peso AS Secas "
                       + " FROM( "
                       + " SELECT T1.CLAVE, ISNULL(T1.JAULAS / T2.TOTAL, 0) AS PorcJaulas, ISNULL(T1.DESTETADAS1 / T2.TOTAL, 0) AS PorcDest1, ISNULL(T1.DESTATADAS2 / T2.TOTAL, 0) AS PorcDest2, "
                       + " ISNULL(T1.VAQUILLAS_PRENADAS / T2.TOTAL, 0) AS PorcVp, ISNULL(T1.PRODUCCION / T2.TOTAL, 0) AS PorcProd, ISNULL(T1.SECAS / T2.TOTAL, 0) AS PorcSecas "
                       + " FROM( "
                       + " SELECT R2.CLAVE, SUM( case when R2.Etapa IN(31) THEN R2.PESO ELSE 0 END) AS JAULAS, SUM( case when R2.Etapa IN(32) THEN R2.PESO ELSE 0 END) AS DESTETADAS1, "
                       + " SUM( case when R2.Etapa IN(33) THEN R2.PESO ELSE 0 END) AS DESTATADAS2, SUM( case when R2.Etapa IN(34) THEN R2.PESO ELSE 0 END) AS VAQUILLAS_PRENADAS, "
                       + " SUM( case when R2.Etapa IN(10, 11, 12, 13) THEN R2.PESO ELSE 0 END) AS PRODUCCION, SUM( case when R2.Etapa IN(21, 22) THEN R2.PESO ELSE 0 END) AS SECAS "
                       + " FROM( "
                       + " SELECT R1.Etapa, R1.CLAVE, R1.INGREDIENTE, SUM(R1.PESO) AS PESO "
                       + " FROM( "
                       + " SELECT etp_id AS Etapa, ing_clave AS CLAVE, ing_descripcion AS INGREDIENTE, SUM(rac_mh) AS PESO "
                       + " FROM racion "
                       + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranchosId + ") AND SUBSTRING(ing_clave, 1, 4) IN('ALFO', 'ALAS') "
                       + " AND SUBSTRING(rac_descripcion, 3, 2) NOT IN('00', '01', '02') "
                       + " GROUP BY etp_id, ing_clave, ing_descripcion "
                       + " UNION "
                       + " SELECT R.Etapa, IIF(R.Racion = '', R.Clave1, R.Clave2) AS Clave, IIF(R.Racion = '', R.Ingrediente1, R.Ingrediente2) AS Ingrediente, R.Peso * R.Porcentaje AS Peso "
                       + " FROM( "
                       + " SELECT T1.Etapa, T1.Clave AS Clave1, T1.Ingrediente AS Ingrediente1, T1.Peso, ISNULL(T2.Racion, '') AS Racion, ISNULL(T2.Clave, '') AS Clave2, "
                       + " ISNULL(T2.Ingrediente, '') AS Ingrediente2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                       + " FROM( "
                       + " SELECT T1.Etapa, T2.Clave, T2.Ingrediente, T1.Peso * T2.Porcentaje As Peso "
                       + " FROM( "
                       + " SELECT etp_id AS Etapa, ing_descripcion AS Racion, SUM(rac_mh) AS Peso "
                       + " FROM racion "
                       + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "'  AND ran_id IN(" + ranchosId + ") AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                       + " AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F') AND etp_id not in (0) "
                       + " GROUP BY etp_id, ing_descripcion) T1 "
                       + " LEFT JOIN( "
                       + " SELECT pmez_descripcion AS Racion, ing_clave AS Clave, ing_descripcion AS Ingrediente, pmez_porcentaje AS Porcentaje "
                       + " FROM porcentaje_Premezcla)T2 ON T1.Racion = T2.Racion) T1 "
                       + " LEFT JOIN( "
                       + " SELECT pmez_descripcion AS Racion, ing_clave AS Clave, ing_descripcion AS Ingrediente, pmez_porcentaje AS Porcentaje "
                       + " FROM porcentaje_Premezcla "
                       + " )T2 ON T1.Ingrediente = T2.Racion ) R ) R1 "
                       + " WHERE SUBSTRING(R1.Clave, 1, 4) IN('ALFO', 'ALAS') "
                       + " GROUP BY R1.Etapa, R1.Clave, R1.INGREDIENTE ) R2 GROUP BY R2.CLAVE) T1 "
                       + " LEFT JOIN( "
                       + " SELECT R2.CLAVE, SUM( case when R2.Etapa IN(10, 11, 12, 13, 21, 22, 31, 32, 33, 34) THEN R2.PESO ELSE 0 END) AS TOTAL "
                       + " FROM( "
                       + " SELECT R1.Etapa, R1.CLAVE, R1.INGREDIENTE, SUM(R1.PESO) AS PESO  FROM(SELECT etp_id AS Etapa, ing_clave AS CLAVE, ing_descripcion AS INGREDIENTE, SUM(rac_mh) AS PESO "
                       + " FROM racion "
                       + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranchosId + ") AND SUBSTRING(ing_clave, 1, 4) IN('ALFO', 'ALAS') "
                       + " AND SUBSTRING(rac_descripcion, 3, 2) NOT IN('00', '01', '02') "
                       + " GROUP BY etp_id, ing_clave, ing_descripcion "
                       + " UNION "
                       + " SELECT R.Etapa, IIF(R.Racion = '', R.Clave1, R.Clave2) AS Clave, IIF(R.Racion = '', R.Ingrediente1, R.Ingrediente2) AS Ingrediente, R.Peso * R.Porcentaje AS Peso "
                       + " FROM( "
                       + " SELECT T1.Etapa, T1.Clave AS Clave1, T1.Ingrediente AS Ingrediente1, T1.Peso, ISNULL(T2.Racion, '') AS Racion, ISNULL(T2.Clave, '') AS Clave2, "
                       + " ISNULL(T2.Ingrediente, '') AS Ingrediente2, ISNULL(T2.Porcentaje, 1) AS Porcentaje "
                       + " FROM( "
                       + " SELECT T1.Etapa, T2.Clave, T2.Ingrediente, T1.Peso * T2.Porcentaje As Peso "
                       + " FROM( "
                       + " SELECT etp_id AS Etapa, ing_descripcion AS Racion, SUM(rac_mh) AS Peso "
                       + " FROM racion "
                       + " WHERE rac_fecha >= '" + racionI.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + racionF.ToString("yyyy-MM-dd HH:mm") + "'  AND ran_id IN(" + ranchosId + ") AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                       + " AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F') AND etp_id not in (0) "
                       + " GROUP BY etp_id, ing_descripcion) T1 "
                       + " LEFT JOIN( "
                       + " SELECT pmez_descripcion AS Racion, ing_clave AS Clave, ing_descripcion AS Ingrediente, pmez_porcentaje AS Porcentaje "
                       + " FROM porcentaje_Premezcla)T2 ON T1.Racion = T2.Racion) T1 "
                       + " LEFT JOIN( "
                       + " SELECT pmez_descripcion AS Racion, ing_clave AS Clave, ing_descripcion AS Ingrediente, pmez_porcentaje AS Porcentaje "
                       + " FROM porcentaje_Premezcla)T2 ON T1.Ingrediente = T2.Racion ) R ) R1 "
                       + " WHERE SUBSTRING(R1.Clave, 1, 4) IN('ALFO', 'ALAS') "
                       + " GROUP BY R1.Etapa, R1.Clave, R1.INGREDIENTE ) R2 "
                       + " GROUP BY R2.CLAVE "
                       + " )T2 ON T1.Clave = T2.Clave) R "
                       + " LEFT JOIN( "
                       + " SELECT art_clave AS Clave, pro_consumo AS Peso "
                       + " FROM prorrateo "
                       + " where pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranchosId + ")  AND SUBSTRING(art_clave, 1, 4) IN('ALFO', 'ALAS') "
                       + " )Bascula ON R.CLAVE = Bascula.Clave) T "
                       + " WHERE T.Jaulas > 0 OR T.Dest1 > 0 OR T.Dest2 > 0 OR T.Vqp > 0 OR T.Prod > 0 OR T.Secas > 0";
                    conn.QueryAlimento(query, out dt);

                    DataTable dtRebano, dtRT;
                    query = "select ia_rebano, ROUND(SUM(CONVERT(FLOAT,ia_jaulas))/ COUNT(DISTINCT ia_fecha),0) AS Jaulas, ROUND(SUM(CONVERT(FLOAT, ia_destetadas)) / COUNT(DISTINCT ia_fecha), 0) AS Destetadas, "
                        + " ROUND(SUM(CONVERT(FLOAT, ia_destetadas2)) / COUNT(DISTINCT ia_fecha), 0) AS Destetadas2, ROUND(SUM(CONVERT(FLOAT, ia_vaquillas)) / COUNT(DISTINCT ia_fecha), 0) AS Vaquillas, "
                        + " ROUND(SUM(CONVERT(FLOAT, ia_vacas_ord)) / COUNT(DISTINCT ia_fecha), 0) AS Produccion, ROUND(SUM(CONVERT(FLOAT, ia_vacas_secas+ ia_vcreto + ia_vqreto)) / COUNT(DISTINCT ia_fecha), 0) AS Secas "
                        + " from inventario_afir "
                        + " WHERE ia_fecha BETWEEN '" + periodoI.ToString("yyyy-MM-dd") + "' AND '" + periodoF.ToString("yyyy-MM-dd") + "' AND ran_id = " + ran_id
                        + " GROUP BY ia_rebano ";
                    conn.QueryAlimento(query, out dtRebano);

                    query = "SELECT ROUND(SUM(CONVERT(FLOAT,ia_jaulas))/ COUNT(DISTINCT ia_fecha),0) AS Jaulas, ROUND(SUM(CONVERT(FLOAT, ia_destetadas)) / COUNT(DISTINCT ia_fecha), 0) AS Destetadas, "
                      + " ROUND(SUM(CONVERT(FLOAT, ia_destetadas2)) / COUNT(DISTINCT ia_fecha), 0) AS Destetadas2, ROUND(SUM(CONVERT(FLOAT, ia_vaquillas)) / COUNT(DISTINCT ia_fecha), 0) AS Vaquillas, "
                      + " ROUND(SUM(CONVERT(FLOAT, ia_vacas_ord)) / COUNT(DISTINCT ia_fecha), 0) AS Produccion, ROUND(SUM(CONVERT(FLOAT, ia_vacas_secas + ia_vcreto + ia_vqreto)) / COUNT(DISTINCT ia_fecha), 0) AS Secas "
                      + " FROM inventario_afir "
                      + " WHERE ia_fecha BETWEEN '" + periodoI.ToString("yyyy-MM-dd") + "' AND '" + periodoF.ToString("yyyy-MM-dd") + "' AND ran_id = " + ran_id;
                    conn.QueryAlimento(query, out dtRT);

                    jaulasT = Convert.ToInt32(dtRT.Rows[0][0]);
                    dest1T = Convert.ToInt32(dtRT.Rows[0][1]);
                    dest2T = Convert.ToInt32(dtRT.Rows[0][2]);
                    vpT = Convert.ToInt32(dtRT.Rows[0][3]);
                    prodT = Convert.ToInt32(dtRT.Rows[0][4]);
                    secT = Convert.ToInt32(dtRT.Rows[0][5]);
                    ganadoT = prodT + secT;
                    crianzaT = jaulasT + dest1T + dest2T + vpT;

                    for (int i = 0; i < dtRebano.Rows.Count; i++)
                    {
                        rebano = Convert.ToInt32(dtRebano.Rows[i][0]);
                        jaulasTAux = Convert.ToInt32(dtRebano.Rows[i][1]);
                        dest1TAux = Convert.ToInt32(dtRebano.Rows[i][2]);
                        dest2TAux = Convert.ToInt32(dtRebano.Rows[i][3]);
                        vpTAux = Convert.ToInt32(dtRebano.Rows[i][4]);
                        prodTAux = Convert.ToInt32(dtRebano.Rows[i][5]);
                        secTAux = Convert.ToInt32(dtRebano.Rows[i][6]);
                        ganadoTAux = prodTAux + secTAux;
                        crianzaTAux = jaulasTAux + dest1TAux + dest2TAux + vpTAux;

                        p_prod = prodTAux / (prodT * 1.0);
                        p_jaulas = jaulasTAux / (1.0 * jaulasT);
                        p_secas = secTAux / (secT * 1.0);
                        p_ganado = ganadoTAux / (ganadoT * 1.0);
                        p_dest1 = dest1TAux / (dest1T * 1.0);
                        p_dest2 = dest2TAux / (dest2T * 1.0);
                        p_vqp = vpTAux / (vpT * 1.0);
                        p_cza = crianzaTAux / (crianzaT * 1.0);

                        almA = rebano == 1 ? "A40002" : "A41002";
                        almF = rebano == 1 ? "A40003" : "A41003";
                        almG = rebano == 1 ? "A40004" : "A41004";
                        for(int j = 0; j < dt.Rows.Count; j++)
                        {
                            clave = dt.Rows[j][0].ToString();
                            jau = Convert.ToDouble(dt.Rows[j][1]) * p_jaulas;
                            d1 = Convert.ToDouble(dt.Rows[j][2]) * p_dest1;
                            d2 = Convert.ToDouble(dt.Rows[j][3]) * p_dest2;
                            vp = Convert.ToDouble(dt.Rows[j][4]) * p_vqp;
                            p = Convert.ToDouble(dt.Rows[j][5]) * p_prod;
                            rs = Convert.ToDouble(dt.Rows[j][6]) * p_secas;

                            if(clave.Substring(0,4) == "ALAS")
                            {
                                valores += "('" + almA + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C1'," + jau + ")," 
                                    + "('" + almA + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C2'," + d1 + "),"
                                    + "('" + almA + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C3'," + d2 + "),"
                                    +  "('" + almA + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C4'," + vp + "),"
                                    + "('" + almA + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','P1'," + p + "),"
                                    + "('" + almA + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','P2'," + rs + "),";
                            }
                            else
                            {
                                valores += "('" + almF + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C1'," + jau + "),"
                                    + "('" + almF + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C2'," + d1 + "),"
                                    + "('" + almF + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C3'," + d2 + "),"
                                    + "('" + almF + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','C4'," + vp + "),"
                                    + "('" + almF + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','P1'," + p + "),"
                                    + "('" + almF + "','" + racionF.ToString("yyyy-MM-dd") + "','" + clave + "','" + almG + "','P2'," + rs + "),";
                            }
                        }
                        conn.InsertMasivAlimento("prorrateo_sie", valores.Substring(0, valores.Length-1));
                        valores = "";
                    }
                }
            }
        }


        private string Almacen(string rancho, string tipo)
        {
            DataTable dt;
            string query = "SELECT alm_id, alm_tipo FROM [DBSIE].dbo.Almacen WHERE ran_id IN(" + rancho + ") AND alm_tipo IN(" + tipo + ")";
            conn.QuerySIE(query, out dt);

            return dt.Rows.Count > 0 ? dt.Rows[0][0].ToString() : "";
        }

        private void ExportarDll()
        {
            string almA = Almacen(ran_id.ToString(), "2");
            string almF = Almacen(ran_id.ToString(), "3");
            DataTable dt;
            string query = "";
            if(ran_id != 25)
            {
                query = "SELECT alm_id, art_clave,SUM(ps_kilos), alm_ganado_id, ps_etapa, ps_fecha "
                   + " FROM prorrateo_sie WHERE alm_id IN('" + almA + "','" + almF + "') and ps_fecha = '" + fechaR.ToString("yyyy-MM-dd") + "' "
                   + " GROUP BY alm_id, art_clave, alm_ganado_id, ps_etapa, ps_fecha "
                   + " Having SUM(ps_kilos) > 0 ";
            }
            else
            {
                query = "SELECT alm_id, art_clave,SUM(ps_kilos), alm_ganado_id, ps_etapa, ps_fecha "
               + " FROM prorrateo_sie WHERE alm_id IN('A40002','A40003', 'A41002', 'A41003') and ps_fecha = '" + fechaR.ToString("yyyy-MM-dd") + "' "
               + " GROUP BY alm_id, art_clave, alm_ganado_id, ps_etapa, ps_fecha "
               + " Having SUM(ps_kilos) > 0 ";
            }
           
            conn.QueryAlimento(query, out dt);
            gth001721(dt);
        }

        private void gth001721(DataTable dt)
        {
            var Prorrateotable = new wMOVDOSIFICADataTable();
            var Prorrateotable1 = new wERRORDataTable();
            string url = sUrl.Replace("@", erp);

            foreach (DataRow row in dt.Rows)
            {

                decimal dec = 0;
                if (Convert.ToString(row[2]) == "")
                {
                    dec = 0;
                }
                else
                {
                    dec = Convert.ToDecimal(row[2]);
                }

                wMOVDOSIFICARow rowN = (wMOVDOSIFICARow)Prorrateotable.NewRow();
                rowN.AlmacenCve = row[0].ToString();
                rowN.ArticuloCve = row[1].ToString();
                rowN.Cantidad = dec;
                rowN.Establo = row[3].ToString();
                rowN.Etapa = row[4].ToString();
                rowN.Periodo = Convert.ToInt32(Convert.ToDateTime(row[5]).ToString("yyyyMM"));
                Prorrateotable.Rows.Add(rowN);
            }

            //ght001721 Prorrateo = new ght001721(url, "", "", "");
            ght001721 Prorrateo = new ght001721(url, "", "", "");
            Prorrateo.ght001721x(Prorrateotable, out Prorrateotable1);

            if (Prorrateotable1.Rows.Count > 0)
            {
                MessageBox.Show("ERROR AL INCORPORAR AL SIE", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Error er = new Error(Prorrateotable1);
                er.Show();
            }
            else
            {
                MessageBox.Show("INCORPORACION A SIE EXITOSA", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
           
        }

        private void GUARDARDIF()
        {
            DataTable dt;
            string query = "SELECT p.pro_fecha_reg, ALAS.DIFKG AS DKALAS, ALAS.PORCDIF AS PDALAS, ALFO.DIFKG AS DKALFO, ALFO.PORCDIF AS PDALFO "
                        + " FROM prorrateo p "
                        + " LEFT JOIN( "
                        + " SELECT ran_id AS Rancho, SUM(pro_consumo) - SUM(pro_consumo_tra) AS DIFKG, (SUM(pro_consumo) - SUM(pro_consumo_tra)) / SUM(pro_consumo_tra) * 100 AS PORCDIF "
                        + " from prorrateo p "
                        + " WHERE pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "' AND SUBSTRING(art_clave, 1, 4) IN('ALAS') "
                        + " GROUP BY ran_id) ALAS ON p.ran_id = Alas.Rancho "
                        + " LEFT JOIN( "
                        + " SELECT ran_id AS Rancho, SUM(pro_consumo) -SUM(pro_consumo_tra) AS DIFKG, (SUM(pro_consumo) - SUM(pro_consumo_tra)) / SUM(pro_consumo_tra) * 100 AS PORCDIF "
                        + " from prorrateo p "
                        + " WHERE pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "' AND SUBSTRING(art_clave, 1,4) IN('ALFO') "
                        + " GROUP BY ran_id ) ALFO ON p.ran_id = ALFO.Rancho "
                        + " WHERE p.pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "' "
                        + " GROUP BY p.pro_fecha_reg, ALAS.DIFKG, ALAS.PORCDIF , ALFO.DIFKG, ALFO.PORCDIF";
            conn.QueryAlimento(query, out dt);

            if (dt.Rows.Count > 0)
            {
                DateTime fecPro = dt.Rows[0][0] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0][0]) : fechaR;
                double difAlas = dt.Rows[0][1] != DBNull.Value ? Convert.ToDouble(dt.Rows[0][1]) : 0;
                double pdifAlas = dt.Rows[0][2] != DBNull.Value ? Convert.ToDouble(dt.Rows[0][2]) : 0;
                double difAlfo = dt.Rows[0][3] != DBNull.Value ? Convert.ToDouble(dt.Rows[0][3]) : 0;
                double pdifAlfo = dt.Rows[0][4] != DBNull.Value ? Convert.ToDouble(dt.Rows[0][4]) : 0;                
                int juliana = ConvertToJulian(fecPro);
                string valores = ran_id + "," + juliana + "," + pdifAlas + "," + difAlas + "," + pdifAlfo + "," + difAlfo + "," + fiabilidad;

                query = "DELETE FROM FIABILIDADPRORRATEO WHERE FECHA = " + juliana.ToString();
                conn.DeleteMovsio(query);

                conn.InsertMovsio("FIABILIDADPRORRATEO", valores);


            }
        }    
        
        private void Consumo()
        {
            DataTable dt;
            conn.DeleteAlimento("consumo", "where cons_fecha = '" + fechaR.ToString("yyyy-MM-dd") + "'");
            string query = "INSERT INTO consumo "
                        + " select p.pro_fecha AS Fecha, p.ran_id AS Establo, p.art_clave AS Clave, p.pro_consumo AS Consumo, p.pro_consumo_tra AS 'CONSUMO TRACKER', ISNULL(a.PRECIO, 0) AS Precio "
                        + " from prorrateo p "
                        + " LEFT JOIN( "
                            + " SELECT a.art_clave AS Clave, AVG(a.art_precio_uni) AS PRECIO "
                        + " FROM articulo a "
                        + " LEFT JOIN( "
                        + " SELECT art.art_clave AS Clave, art.alm_id AS Alm, MAX(art.art_fecha) AS Fecha "
                        + " FROM articulo art " 
                        + " LEFT JOIN[DBSIE].[dbo].almacen alm ON art.alm_id = alm.alm_id "
                        + " WHERE alm.ran_id = " + ran_id
                        + " GROUP BY art.art_clave, art.alm_id) b ON a.alm_id = b.Alm AND a.art_clave = b.Clave AND a.art_fecha = b.Fecha "
                        + " WHERE b.Clave IS NOT NULL "
                        + " GROUP BY a.art_clave "
                        + " )   a ON p.art_clave = a.Clave "
                        + " where pro_fecha_reg = '" + fechaR.ToString("yyyy-MM-dd") + "'";
            conn.QueryAlimento(query);


        }
    }
}
