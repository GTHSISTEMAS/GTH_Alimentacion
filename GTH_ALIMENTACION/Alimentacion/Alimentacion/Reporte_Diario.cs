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
using System.Data.OleDb;
using FirebirdSql.Data;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Reporting.WinForms;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Collections;
using System.Web.UI.WebControls;

namespace Alimentacion
{
    public partial class Reporte_Diario : Form
    {
        //
        ConnSIO conn = new ConnSIO();
        string emp_nombre, ran_nombre;
        int ran_id, emp_id, ran_sie;
        bool empresa;        
        string establosNumero = "", establos = "";
        string ranNumero, ranCadena, titulo, emp_codigo;
        string ruta, campo_precio;
        int dias;
        DateTime fechaMax, fechaMin;
        int tipo;
        bool reportes;
        
        public Reporte_Diario(int ran_id, string ran_nombre, int emp_id, string emp_nombre)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;
            reportes = Convert.ToBoolean(ConfigurationManager.AppSettings["Reportes_Alim"]);
        }

        public Reporte_Diario(int ran_id, string ran_nombre, int emp_id, string emp_nombre, int tipo)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;
            this.tipo = tipo;
            //this.empresa = empresa;
            reportes = Convert.ToBoolean(ConfigurationManager.AppSettings["Reportes_Alim"]);
        }

        public void getEstablos(bool empresa)
        {
            ranNumero = "";
            ranCadena = "";
            titulo = "";
            if (empresa)
            {
                DataTable dt = Establos(tipo);
                //string query = "SELECT ran_id FROM configuracion WHERE emp_id  = " + emp_id;
                //conn.QuerySIO(query, out dt);
                int establo; string estTemp;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    establo = Convert.ToInt32(dt.Rows[i][0]);
                    estTemp = establo > 9 ? establo.ToString() : "0" + establo.ToString();
                    ranCadena += "'" + estTemp + "',";
                    ranNumero += establo.ToString() + ",";
                }
                ranNumero = ranNumero.Remove(ranNumero.Length-1,1);
                ranCadena = ranCadena.Remove(ranCadena.Length - 1,1);
                titulo = emp_nombre;
            }
            else
            {
                establos = ran_id > 9 ? "'" + ran_id.ToString() + "'" : "'0" + ran_id.ToString() + "'";
                ranCadena = establos;
                ranNumero = ran_id.ToString();
                titulo = ran_nombre;
            }
        }

        private void getInfo()
        {
            DataTable dt,dt1;
            string query = "select rut_ruta from ruta where ran_id = " + ran_id;
            conn.QuerySIO(query, out dt);

            ruta =  dt.Rows[0][0].ToString();

            query = "SELECT emp_codigo, ran_sie FROM configuracion WHERE ran_id = " + ran_id;
            conn.QuerySIO(query, out dt1);

            emp_codigo = dt1.Rows[0][0].ToString();
            ran_sie = Convert.ToInt32(dt1.Rows[0][1]);
            campo_precio = ran_sie == 1 ? "i.ing_precio_sie" : "ing_precio_tracker";
        }

        private void getDias()
        {
            DateTime inicio = dtpInicial.Value.Date;
            DateTime fin = dtpFinal.Value.Date;

            TimeSpan ts = (fin - inicio);
            dias = ts.Days;
            dias = dias == 0 ? 1 : dias;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            getEstablos(checkBox1.Checked);
            if (checkBox1.Checked)
                for (int i = 0; i < clbRanchos.Items.Count; i++)
                    clbRanchos.SetItemChecked(i, checkBox1.Checked);
            else
            {
                if (TotalSeleccionados() == clbRanchos.Items.Count)
                    for (int i = 0; i < clbRanchos.Items.Count; i++)
                        clbRanchos.SetItemChecked(i, false);
                else
                    for (int i = 0; i < clbRanchos.Items.Count; i++)
                        clbRanchos.SetItemChecked(i, clbRanchos.GetItemChecked(i));
            }

        }
       

        private void button1_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    //getDias();
            //    int numAnimales = 0;
            //    double media = 0, pmsP = 0;
            //    double precioLeche = 0, lecheFederal = 0, costoT = 0, precioT = 0, totalRacion = 0;
            //    DataTable dtResult = new DataTable();
            //    DataTable dtindicadores = new DataTable();
            //    int hcorte = 0;
            //    int horas;
            //    Hora_Corte(out horas, out hcorte);
            //    DateTime fechaIni = dtpInicial.Value.Date;
            //    DateTime fechaFin = dtpFinal.Value.Date;
            //    DateTime inicio = dtpInicial.Value.Date;
            //    DateTime fin = dtpFinal.Value.Date;
            //    string query;
            //    string etapa = cbEtapa.SelectedValue.ToString();
            //    string etapa1 = cbEtapa.Text; 
            //    etapa = etapa == "10" ? "10,11,12,13" : etapa;
            //    int comparacion = DateTime.Compare(fechaIni, fechaFin);
            //    string campoAnimal = "";
            //    string tituloR = checkBox1.Checked ? emp_codigo : ran_nombre;
            //    int seleccionados = TotalSeleccionados();
            //    if(comparacion == 0 || comparacion == -1)
            //    {
            //        if (!checkBox1.Checked)
            //        {
            //            string ranTemp = GetSelectRanchos();
            //            ranNumero = ranTemp.Length > 0 ? ranTemp : ranNumero;
            //            tituloR = seleccionados > 1 ? seleccionados == clbRanchos.Items.Count ? emp_codigo : Titulos(ranNumero) : ran_nombre;
            //        }

            //        string sob = Sobrantes();
                   
            //        int dif = 24 + horas;
            //        fechaIni = fechaIni.AddHours(horas);
            //        fechaFin = fechaFin.AddHours(dif);

            //        DataTable dt1;
            //        query = "SELECT T.Total FROM( SELECT ISNULL(SUM(rac_mh) / DATEDIFF(DAY, '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "', '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "'), 0) AS Total "
            //                + " from racion where rac_fecha"
            //                + " >= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ") and etp_id IN(" + etapa + ") " 
            //                + " AND ing_descripcion not in(" + sob + ") ) T"
            //                + " WHERE T.Total > 0";
            //        conn.QueryAlimento(query, out dt1);

            //        if (dt1.Rows.Count > 0)
            //        {
            //            //Obtener las Premezclas que se dieron en este periodo
            //            DataTable dtPremezclas = new DataTable();
            //            query = "select DISTINCT ing_descripcion FROM racion "
            //                + " WHERE rac_fecha >= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "' "
            //                + " AND ran_id IN(" + ranNumero.ToString() + ") AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02') "
            //                + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F') AND etp_id IN(" + etapa + ")";
            //            conn.QueryAlimento(query, out dtPremezclas);

            //            conn.DeleteAlimento("porcentaje_Premezcla", "");
            //            DataTable dtt;
            //            for (int i = 0; i < dtPremezclas.Rows.Count; i++)
            //            {
            //                query = "SELECT TOP(5) * FROM premezcla where pmez_racion like '" + dtPremezclas.Rows[i][0].ToString() + "' AND pmez_fecha <= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "'";
            //                conn.QueryAlimento(query, out dtt);

            //                if (dtt.Rows.Count == 0)
            //                    continue;

            //                CargarPremezcla(dtPremezclas.Rows[i][0].ToString(), fechaIni, fechaFin);
            //            }
            //            string etp = "";
            //            switch (etapa)
            //            {
            //                case "10,11,12,13":
            //                    etp = "1";
            //                    campoAnimal = "ia_vacas_ord";
            //                    break;
            //                case "21":
            //                    etp = "2";
            //                    campoAnimal = "ia_vacas_secas";
            //                    break;
            //                case "22":
            //                    etp = "4";
            //                    campoAnimal = "ia_vqreto + ia_vcreto";
            //                    break;
            //                case "31":
            //                    etp = "3";
            //                    campoAnimal = "ia_jaulas";
            //                    break;
            //                case "32":
            //                    etp = "3";
            //                    campoAnimal = "ia_destetadas";
            //                    break;
            //                case "33":
            //                    etp = "3";
            //                    campoAnimal = "ia_destetadas2";
            //                    break;
            //                case "34":
            //                    etp = "3";
            //                    campoAnimal = "ia_vaquillas";
            //                    break;
            //                case "31,32,33,34":
            //                    etp = "3";
            //                    campoAnimal = "ia_jaulas + ia_destetadas + ia_destetadas2 + ia_vaquillas";
            //                    break;

            //            }

            //            //Seccion de Racion de alas
            //            DataTable dt3; DataTable dt2; DataTable dtAg;

            //            if (ran_sie == 1)
            //            {
            //                Alimentos(etapa, campoAnimal, fechaIni, fechaFin, out dt3);                            
            //                ForrajeSob(etapa, campoAnimal, fechaIni, fechaFin, out dt2);                            
            //                Agua(etapa, campoAnimal, fechaIni, fechaFin, out dtAg);
            //            }
            //            else
            //            {
            //                AlimentosT(etapa, campoAnimal, fechaIni, fechaFin, out dt3);
            //                ForrajeSobT(etapa, campoAnimal, fechaIni, fechaFin, out dt2);
            //                Agua(etapa, campoAnimal, fechaIni, fechaFin, out dtAg);
            //            }

            //            ////Seccion de Racion de Forraje y sobrante
            //            //DataTable dt2;
            //            //ForrajeSob(etapa, campoAnimal, fechaIni, fechaFin, out dt2);

            //            //DataTable dtAg;
            //            //Agua(etapa, campoAnimal, fechaIni, fechaFin, out dtAg);

            //                // Seccion Sobrante
            //            DataTable dt4;
            //             query = "SELECT 'SOBRANTE' AS Ingrediente, ISNULL(SUM(rac_mh)/ DATEDIFF(DAY, '" + fechaIni.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "', '" + fechaFin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "'),0) AS Peso "
            //                + " FROM racion where rac_fecha >= '" + fechaIni.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "' AND rac_descripcion like '%SOB%' "
            //                + " AND ing_descripcion like '" + etp + "%' AND ran_id IN(" + ranNumero + ") AND SUBSTRING(ing_descripcion,3,2) NOT IN('00','01','02','90')";
            //            conn.QueryAlimento(query, out dt4);

            //            //media y leche federal
            //            DataTable dt5;
            //            query = "SELECT IIF(SUM(ia.ia_vacas_ord)>0,ISNULL((SUM(m.med_produc)/SUM(ia.ia_vacas_ord)),0),0) , ISNULL(SUM(m.med_lecfederal + m.med_lecplanta) / COUNT(DISTINCT med_fecha),0) "
            //                    + " FROM media m LEFT JOIN inventario_afi ia ON ia.ran_id = m.ran_id AND ia.ia_fecha = m.med_fecha "
            //                    + " where m.ran_id IN(" + ranNumero + ") AND med_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' ";
            //            conn.QueryAlimento(query, out dt5);

            //            //Precio  Leche
            //            DataTable dt6;
            //            query = "SELECT AVG(T2.Precio) FROM( SELECT  ran_id AS Rancho, MAX(hl_fecha_reg) AS Fecha "
            //                    + " FROM historico_leche where ran_id IN(" + ranNumero + ") GROUP BY ran_id) T1 "
            //                    + " LEFT JOIN( "
            //                    + " SELECT ran_id AS Rancho, hl_fecha_reg AS Fecha, hl_precio AS Precio "
            //                    + " FROM historico_leche "
            //                    + " WHERE ran_id IN(" + ranNumero + "))T2 ON T1.Fecha = T2.Fecha AND T1.Rancho = T2.Rancho";
            //            conn.QueryAlimento(query, out dt6);                     

            //            //Numero de vacas
            //            DataTable dt7;
            //            query = "SELECT ROUND(SUM(CONVERT(FLOAT, " + campoAnimal + "))/ COUNT(DISTINCT ia_fecha),0) FROM inventario_afi "
            //                    + " WHERE ia_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' "
            //                    + " AND ran_id IN(" + ranNumero + ")";
            //            conn.QueryAlimento(query, out dt7);

            //            //Porcentaje de materia seca
            //            //DataTable dt8;
            //            //query = "SELECT ISNULL(SUM(rac_ms) / SUM(rac_mh) *100,0) AS PorcMS FROM racion "
            //            //        + " WHERE rac_fecha >= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "' "
            //            //        + " AND ran_id IN(" + ranNumero + ") AND etp_id IN(" + etapa + ")";
            //            //conn.QueryAlimento(query, out dt8);

            //            //Asignar valores a variables
            //            pmsP = PMS(etapa, fechaIni, fechaFin);
            //            try
            //            {
                            
            //                Int32.TryParse(dt7.Rows[0][0].ToString(), out numAnimales);
            //                Double.TryParse(dt6.Rows[0][0].ToString(), out precioLeche);
            //                Double.TryParse(dt5.Rows[0][1].ToString(), out lecheFederal);
            //                Double.TryParse(dt5.Rows[0][0].ToString(), out media);
            //                Double.TryParse(dt1.Rows[0][0].ToString(), out totalRacion);

            //                media = etp == "1" ? media : 0;
            //            }
            //            catch
            //            {
            //                pmsP = pmsP != 0 ? pmsP : 0;
            //                numAnimales = numAnimales != 0 ? numAnimales : 0;
            //                precioLeche = precioLeche != 0 ? precioLeche : 0;
            //                lecheFederal = lecheFederal != 0 ? lecheFederal : 0;
            //                media = media != 0 ? media : 0;
            //                totalRacion = Convert.ToDouble(dt1.Rows[0][0]); //Total de racion
            //            }

            //            DataTable dtALFO;
            //            ColumnasDT(out dtALFO);

            //            DataTable dtALAS;
            //            ColumnasDT(out dtALAS);

            //            DataTable dtTotalR;
            //            ColumnasDT(out dtTotalR);

            //            //double totalA = 0, xvacaA = 0, porcvacaA = 0;
            //            //DataTable dtAgua = Agua(etapa, fechaIni, fechaFin);
            //            //totalA = Convert.ToDouble(dtAgua.Rows[0][1]);
            //            //xvacaA = totalA / numAnimales;

            //            string ingrediente;
            //            double precioIng, peso, xvaca, costo, precio, porcR;

            //            double ingH = 0, xvacaH = 0, pvacaH = 0, totalH = 0, costoH = 0, precioH = 0, ingS = 0, xvacaS = 0, pvacaS = 0, totalS = 0, costoS = 0, precioS = 0, pch = 0, pcs= 0;

            //            for (int i = 0; i < dt2.Rows.Count; i++)
            //            {
            //                dtALFO.ImportRow(dt2.Rows[i]);
            //                xvacaH += Convert.ToDouble(dt2.Rows[i]["xvaca"]);
            //                pvacaH += Convert.ToDouble(dt2.Rows[i]["porcvaca"]);
            //                totalH += Convert.ToDouble(dt2.Rows[i]["TOTAL"]);
            //                xvacaS += Convert.ToDouble(dt2.Rows[i]["s_xvaca"]);
            //                pvacaS += Convert.ToDouble(dt2.Rows[i]["s_porcvaca"]);
            //                totalS += Convert.ToDouble(dt2.Rows[i]["s_TOTAL"]);
            //                pch += Convert.ToDouble(dt2.Rows[i]["porccosto"]);
            //                pcs += Convert.ToDouble(dt2.Rows[i]["s_porccosto"]);
            //                costoH += Convert.ToDouble(dt2.Rows[i]["COSTO"]);
            //                costoS += Convert.ToDouble(dt2.Rows[i]["s_COSTO"]);
            //                precioH += Convert.ToDouble(dt2.Rows[i]["PRECIO"]);
            //                precioS += Convert.ToDouble(dt2.Rows[i]["s_PRECIO"]);
            //            }
                    

            //            double ingH1 = 0, xvacaH1 = 0, pvacaH1 = 0, totalH1 = 0, costoH1 = 0, precioH1 = 0, ingS1 = 0, xvacaS1 = 0, pvacaS1 = 0, totalS1 = 0, costoS1 = 0, precioS1 = 0, pch1 = 0, pcs1 = 0;

            //            for (int i = 0; i < dt3.Rows.Count; i++)
            //            {
            //                dtALAS.ImportRow(dt3.Rows[i]);
            //                xvacaH1 += Convert.ToDouble(dt3.Rows[i]["xvaca"]);
            //                pvacaH1 += Convert.ToDouble(dt3.Rows[i]["porcvaca"]);
            //                totalH1 += Convert.ToDouble(dt3.Rows[i]["TOTAL"]);
            //                xvacaS1 += Convert.ToDouble(dt3.Rows[i]["s_xvaca"]);
            //                pvacaS1 += Convert.ToDouble(dt3.Rows[i]["s_porcvaca"]);
            //                totalS1 += Convert.ToDouble(dt3.Rows[i]["s_TOTAL"]);
            //                costoH1 += Convert.ToDouble(dt3.Rows[i]["COSTO"]);
            //                pch1 += Convert.ToDouble(dt3.Rows[i]["porccosto"]);
            //                pcs1 += Convert.ToDouble(dt3.Rows[i]["s_porccosto"]);
            //                costoH1 += Convert.ToDouble(dt3.Rows[i]["COSTO"]);
            //                costoS1 += Convert.ToDouble(dt3.Rows[i]["s_COSTO"]);
            //                precioH1 += Convert.ToDouble(dt3.Rows[i]["PRECIO"]);
            //                precioS1 += Convert.ToDouble(dt3.Rows[i]["s_PRECIO"]);
            //            }

            //            double xvacaAg = dtAg.Rows.Count > 0 ? Convert.ToDouble(dtAg.Rows[0]["xvaca"]) : 0;

            //            DataRow drA = dtALFO.NewRow();
            //            drA[0] = "TOTAL FORRAJE";
            //            drA[1] = -1;
            //            drA[2] = xvacaH;
            //            drA[3] = pvacaH;
            //            drA[4] = totalH;
            //            drA[5] = costoH;
            //            drA[6] = pch;
            //            drA[7] = precioH;
            //            drA[8] = -1;
            //            drA[9] = xvacaS;
            //            drA[10] = pvacaS;
            //            drA[11] = totalS;
            //            drA[12] = costoS;
            //            drA[13] = pcs;
            //            drA[14] = precioS;
            //            drA[15] = -1;
            //            dtALFO.Rows.Add(drA);

            //            DataRow drAL = dtALAS.NewRow();
            //            drAL[0] = "TOTAL CONCENTRADO";
            //            drAL[1] = -1;
            //            drAL[2] = xvacaH1;
            //            drAL[3] = pvacaH1;
            //            drAL[4] = totalH1;
            //            drAL[5] = costoH1;
            //            drAL[6] = pch1;
            //            drAL[7] = precioH1;
            //            drAL[8] = -1;
            //            drAL[9] = xvacaS1;
            //            drAL[10] = pvacaS1;
            //            drAL[11] = totalS1;
            //            drAL[12] = costoS1;
            //            drAL[13] = pcs1;
            //            drAL[14] = precioS1;
            //            drAL[15] = -1;
            //            dtALAS.Rows.Add(drAL);

            //            DataRow drTR = dtTotalR.NewRow();
            //            drTR[0] = "TOTAL RACION";
            //            drTR[1] = -1;
            //            drTR[2] = xvacaH + xvacaH1 + xvacaAg;
            //            drTR[3] = -1;
            //            drTR[4] = totalRacion;
            //            drTR[5] = costoH + costoH1;
            //            drTR[6] = -1;
            //            drTR[7] = precioH+ precioH1;
            //            drTR[8] = -1;
            //            drTR[9] = xvacaS + xvacaS1;
            //            drTR[10] = -1;
            //            drTR[11] = totalRacion * pmsP / 100;
            //            drTR[12] = costoS + costoS1;
            //            drTR[13] = -1;
            //            drTR[14] = precioS + precioS1;
            //            drTR[15] = -1;
            //            dtTotalR.Rows.Add(drTR);

            //            DataTable dtSob;
            //            ColumnasDT(out dtSob);
            //            DataRow drSob = dtSob.NewRow();
            //            double sobrante = Convert.ToDouble(dt4.Rows[0][1]);
            //            double xvacaSob = numAnimales > 0 ? sobrante / numAnimales : 0;
            //            drSob["ingrediente"] = "SOBRANTE";
            //            drSob["xvaca"] = xvacaSob;
            //            drSob["TOTAL"] = sobrante;
            //            dtSob.Rows.Add(drSob);

            //            DataTable dtRS;
            //            ColumnasDT(out dtRS);

            //            DataRow drRS = dtRS.NewRow();
            //            drRS["ingrediente"] = "TOTAL RACION - SOBRANTE";
            //            drRS["xvaca"] = numAnimales > 0 ? (totalRacion / numAnimales) - xvacaSob : 0;
            //            drRS["TOTAL"] = totalRacion - sobrante;
            //            dtRS.Rows.Add(drRS);

            //            DataTable enter;
            //            ColumnasDT(out enter);

            //            DataRow drEnter = enter.NewRow();
            //            drEnter["ingrediente"] = "";
            //            drEnter["precioIng"] = -1;
            //            drEnter["xvaca"] = -1;
            //            drEnter["porcvaca"] = -1;
            //            drEnter["TOTAL"] = -1;
            //            drEnter["COSTO"] = -1;
            //            drEnter["porccosto"] = -1;
            //            drEnter["PRECIO"] = -1;
            //            enter.Rows.Add(drEnter);

            //            double xvacaA = 0, porcvaca = 0, totalAg = 0;
            //            if(dtAg.Rows.Count > 0)
            //            {
            //                xvacaA = Convert.ToDouble(dtAg.Rows[0][2]);
            //                porcvaca = Convert.ToDouble(dtAg.Rows[0][3]);
            //                totalAg = Convert.ToDouble(dtAg.Rows[0][4]);
            //            }

            //            DataTable dtAgua;
            //            ColumnasDT(out dtAgua);

            //            DataRow drAgua = dtAgua.NewRow();
            //            drAgua["ingrediente"] = "AGUA";
            //            drAgua["xvaca"] = xvacaA;
            //            drAgua["porcvaca"] = porcvaca;
            //            drAgua["TOTAL"] = totalAg;
            //            dtAgua.Rows.Add(drAgua);

            //            dtResult.Merge(dtTotalR);
            //            dtResult.Merge(enter);
            //            dtResult.Merge(dtALFO);
            //            dtResult.Merge(enter);
            //            dtResult.Merge(dtALAS);
            //            dtResult.Merge(enter);
            //            dtResult.Merge(dtAgua);
            //            dtResult.Merge(enter);
            //            dtResult.Merge(dtTotalR);
            //            dtResult.Merge(enter);
            //            dtResult.Merge(dtSob);
            //            dtResult.Merge(enter);
            //            dtResult.Merge(dtRS);

            //            costoT = Costo(etapa, campoAnimal, fechaIni, fechaFin);
            //            //Indicadores
            //            DataTable dtIndicadores;
            //            ColumnasIndicadores(out dtIndicadores);

            //            DataRow drIndicadores = dtIndicadores.NewRow();
            //            drIndicadores["Animales"] = numAnimales;
            //            drIndicadores["media"] = media;
            //            drIndicadores["ilcavta"] = numAnimales > 0 && costoT >  0? (lecheFederal / numAnimales * precioLeche / costoT) : 0;
            //            drIndicadores["icventa"] = media > 0 && numAnimales > 0 ? (lecheFederal / numAnimales * precioLeche) - costoT : - costoT;
            //            drIndicadores["eaprod"] = media >  0 ? media / (pmsP * (totalRacion / numAnimales) / 100) : 0;
            //            drIndicadores["ilcaprod"] = media >  0 && costoT >  0? precioLeche * media / costoT : 0;
            //            drIndicadores["icprod"] = media > 0 ? (precioLeche * media) - costoT : -costoT;
            //            drIndicadores["preclprod"] = media > 0 ? costoT / media : 0;
            //            drIndicadores["mhprod"] = numAnimales > 0 ? totalRacion / numAnimales : 0;
            //            drIndicadores["porcmsprod"] = pmsP;
            //            drIndicadores["msprod"] = numAnimales > 0 ? pmsP * (totalRacion / numAnimales) / 100 : 0;
            //            drIndicadores["saprod"] = numAnimales > 0 ? sobrante / numAnimales : 0;
            //            drIndicadores["mssprod"] = numAnimales > 0 ? ((totalRacion - sobrante) / numAnimales) * pmsP / 100 : 0;
            //            drIndicadores["easprod"] = media > 0 && numAnimales> 0 ? media / ((totalRacion - sobrante) / numAnimales * pmsP / 100) : 0;
            //            drIndicadores["precprod"] = costoT >  0? costoT : 0;
            //            drIndicadores["precmsprod"] = numAnimales > 0 ? costoT / (pmsP * (totalRacion / numAnimales) / 100) : 0;
            //            dtIndicadores.Rows.Add(drIndicadores);

            //            ReportDataSource source1 = new ReportDataSource("DataSet2", dtIndicadores);
            //            reportViewer1.LocalReport.DataSources.Clear();
            //            reportViewer1.LocalReport.DataSources.Add(source1);
            //            ReportDataSource source = new ReportDataSource("DataSet1", dtResult);
            //            reportViewer1.LocalReport.DisplayName = "Reporte Diario";
            //            //this.reportViewer1.LocalReport.ReportPath = ConfigurationManager.AppSettings["reporteDiario"];
            //            //reportViewer1.LocalReport.DataSources.Clear();
            //            reportViewer1.LocalReport.DataSources.Add(source);

            //            string titulo2 = checkBox1.Checked ? tipo == 2 ? "Empresa: " + Empresa() : "Empresa: " +  emp_codigo : "Establos: " + Titulos(ranNumero);
            //            ReportParameter[] parametros = new ReportParameter[3];
            //            parametros[0] = new ReportParameter("Establo", titulo2.ToUpper());
            //            parametros[1] = new ReportParameter("periodo", "PERIODO DEL: " + dtpInicial.Value.Date.ToString("dd/MM/yyyy") + " AL: " + dtpFinal.Value.Date.ToString("dd/MM/yyyy"));
            //            parametros[2] = new ReportParameter("Etapa", "ETAPA: " + etapa1.Trim());
            //            reportViewer1.LocalReport.SetParameters(parametros);

            //            reportViewer1.LocalReport.Refresh();
            //            reportViewer1.RefreshReport();

            //            titulo = checkBox1.Checked ? emp_codigo : Titulos(ranNumero);
            //            GTHUtils.SavePDF(reportViewer1, ruta + "REPORTE DIARIO " + titulo + ".pdf");
            //            //MessageBox.Show("Reporte generado correctamente", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //            string rutapdf = ruta + "REPORTE DIARIO " + titulo + ".pdf";
            //            Process.Start(rutapdf);
            //        }
            //        else
            //        {
            //            string message = "No tienes informacion cargada en la Base de Datos de ese etapa en ese periodo"; 
            //            MessageBox.Show(message,"Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);

                        
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("LA FECHA FINAL NO PUEDE SER MENOR A LA FECHA INICIAL", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //}
            //catch (IOException ex){ MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            //catch (DbException ex){ MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            //catch (Exception ex){ MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }


            Reporte(cbEtapa.SelectedValue.ToString(), cbEtapa.Text, dtpInicial.Value.Date, dtpFinal.Value.Date, checkBox1.Checked, "");

        }

        public void Reporte(string etapa, string etapa_nombre, DateTime dtpInicial, DateTime dtpFinal, bool empresa, string reporte)
        {
            try
            {
                //getDias();
                if (reporte != "")
                {
                    conn.Iniciar("DBSIE");
                    getInfo();

                    DataTable dt;
                    string q_r = "SELECT rut_ruta FROM ruta WHERE rut_desc = 'sio'";
                    conn.QueryMovGanado(q_r, out dt);
                    ruta = dt.Rows[0][0].ToString();
                    getEstablos(empresa);
                }

                int numAnimales = 0;
                double media = 0, pmsP = 0, mh = 0, ms = 0;
                double precioLeche = 0, lecheFederal = 0, costoT = 0, precioT = 0, totalRacion = 0;
                DataTable dtResult = new DataTable();
                DataTable dtindicadores = new DataTable();
                DataTable dtM;
                int hcorte = 0;
                int horas;
                Hora_Corte(out horas, out hcorte);
                DateTime fechaIni = dtpInicial.Date;
                DateTime fechaFin = dtpFinal.Date;
                DateTime inicio = dtpInicial.Date;
                DateTime fin = dtpFinal.Date;
                string query;
                //string etapa = cbEtapa.SelectedValue.ToString();
                //string etapa1 = cbEtapa.Text;
                etapa = etapa == "10" ? "10,11,12,13" : etapa;
                int comparacion = DateTime.Compare(fechaIni, fechaFin);
                string campoAnimal = "";
                string tituloR = empresa ? emp_codigo : ran_nombre;
                int seleccionados = TotalSeleccionados();
                
                if (comparacion == 0 || comparacion == -1)
                {
                    if (!checkBox1.Checked && reporte == "")
                    {
                        string ranTemp = GetSelectRanchos();
                        ranNumero = ranTemp.Length > 0 ? ranTemp : ranNumero;
                        tituloR = seleccionados > 1 ? seleccionados == clbRanchos.Items.Count ? emp_codigo : Titulos(ranNumero) : ran_nombre;
                    }

                    string sob = Sobrantes();

                    int dif = 24 + horas;
                    int dias_a = horas > 0 ? 0 : -1;
                    fechaIni = dif > 24 ? fechaIni.AddHours(horas).AddDays(dias_a): fechaIni.AddHours(horas);
                    fechaFin = dif > 24 ? fechaFin.AddHours(dif).AddDays(dias_a) : fechaFin.AddHours(dif);

                    DataTable dtPremezclas = new DataTable();
                    query = "select DISTINCT ing_descripcion FROM racion "
                        + " WHERE rac_fecha >= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "' "
                        + " AND ran_id IN(" + ranNumero.ToString() + ") AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02') "
                        + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F') AND etp_id IN(" + etapa + ")";
                    conn.QueryAlimento(query, out dtPremezclas);

                    conn.DeleteAlimento("porcentaje_Premezcla", "");
                    DataTable dtt;
                    for (int i = 0; i < dtPremezclas.Rows.Count; i++)
                    {
                        query = "SELECT TOP(5) * FROM premezcla where pmez_racion like '" + dtPremezclas.Rows[i][0].ToString() + "' AND pmez_fecha <= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "'";
                        conn.QueryAlimento(query, out dtt);

                        if (dtt.Rows.Count == 0)
                            continue;

                        CargarPremezcla(dtPremezclas.Rows[i][0].ToString(), fechaIni, fechaFin);
                    }

                    DataTable dt1; 
                    TotalRacion(etapa, fechaIni, fechaFin, out dt1);
                    //query = "SELECT T.Total FROM( SELECT ISNULL(SUM(rac_mh) / DATEDIFF(DAY, '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "', '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "'), 0) AS Total "
                    //        + " from racion where rac_fecha"
                    //        + " >= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ") and etp_id IN(" + etapa + ") "
                    //        + " AND ing_descripcion not in(" + sob + ") ) T"
                    //        + " WHERE T.Total > 0";
                    //conn.QueryAlimento(query, out dt1);

                    if (dt1.Rows.Count > 0)
                    {
                        //Obtener las Premezclas que se dieron en este periodo
                      

                        string etp = "";
                        switch (etapa)
                        {
                            case "10,11,12,13":
                                etp = "'1'";
                                campoAnimal = "ia_vacas_ord";
                                break;
                            case "21":
                                etp = "'2'";
                                campoAnimal = "ia_vacas_secas";
                                break;
                            case "22":
                                etp = "'4'";
                                campoAnimal = "ia_vqreto + ia_vcreto";
                                break;
                            case "31":
                                etp = "'3'";
                                campoAnimal = "ia_jaulas";
                                break;
                            case "32":
                                etp = "'3'";
                                campoAnimal = "ia_destetadas";
                                break;
                            case "33":
                                etp = "'3'";
                                campoAnimal = "ia_destetadas2";
                                break;
                            case "34":
                                etp = "'3'";
                                campoAnimal = "ia_vaquillas";
                                break;
                            case "31,32,33,34":
                                etp = "'3'";
                                campoAnimal = "ia_jaulas + ia_destetadas + ia_destetadas2 + ia_vaquillas";
                                break;
                            case "10,11,12,13,21,22,31,32,33,34":
                                etp = "'1','2','3','4'";
                                campoAnimal = "ia_vacas_ord + ia_vacas_secas + ia_vqreto + ia_vcreto + ia_jaulas + ia_destetadas + ia_destetadas2 + ia_vaquillas";
                                break;
                        }

                        //Seccion de Racion de alas
                        DataTable dt3; DataTable dt2; DataTable dtAg;
                        Materias(etapa, campoAnimal, fechaIni, fechaFin, out dtM);
                        if (ran_sie == 1)
                        {
                            Alimentos(etapa, campoAnimal, fechaIni, fechaFin, out dt3);
                            ForrajeSob(etapa, campoAnimal, fechaIni, fechaFin, out dt2);
                            Agua(etapa, campoAnimal, fechaIni, fechaFin, out dtAg);
                        }
                        else
                        {
                            AlimentosT(etapa, campoAnimal, fechaIni, fechaFin, out dt3);
                            ForrajeSobT(etapa, campoAnimal, fechaIni, fechaFin, out dt2);
                            Agua(etapa, campoAnimal, fechaIni, fechaFin, out dtAg);
                        }

                        ////Seccion de Racion de Forraje y sobrante
                        //DataTable dt2;
                        //ForrajeSob(etapa, campoAnimal, fechaIni, fechaFin, out dt2);

                        //DataTable dtAg;
                        //Agua(etapa, campoAnimal, fechaIni, fechaFin, out dtAg);

                        // Seccion Sobrante
                        DataTable dt4;
                        query = "SELECT 'SOBRANTE' AS Ingrediente, ISNULL(SUM(rac_mh)/ DATEDIFF(DAY, '" + fechaIni.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "', '" + fechaFin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "'),0) AS Peso "
                           + " FROM racion where rac_fecha >= '" + fechaIni.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "' AND rac_descripcion like '%SOB%' "
                           + " AND SUBSTRING(ing_descripcion,1,1) IN(" + etp + ") AND ran_id IN(" + ranNumero + ") AND SUBSTRING(ing_descripcion,3,2) NOT IN('00','01','02','90')";
                        conn.QueryAlimento(query, out dt4);

                        //media y leche federal
                        DataTable dt5;
                       query = "SELECT IIF(SUM(ia.ia_vacas_ord)>0,ISNULL((SUM(m.med_produc)/SUM(ia.ia_vacas_ord)),0),0) , ISNULL(SUM(m.med_lecfederal + m.med_lecplanta) / COUNT(DISTINCT med_fecha),0) "
                                + " FROM media m LEFT JOIN inventario_afi ia ON ia.ran_id = m.ran_id AND ia.ia_fecha = m.med_fecha "
                                + " where m.ran_id IN(" + ranNumero + ") AND med_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' ";
                        conn.QueryAlimento(query, out dt5);

                        //Precio  Leche
                        DataTable dt6;
                        query = "SELECT AVG(T2.Precio) FROM( SELECT  ran_id AS Rancho, MAX(hl_fecha_reg) AS Fecha "
                                + " FROM historico_leche where ran_id IN(" + ranNumero + ") GROUP BY ran_id) T1 "
                                + " LEFT JOIN( "
                                + " SELECT ran_id AS Rancho, hl_fecha_reg AS Fecha, hl_precio AS Precio "
                                + " FROM historico_leche "
                                + " WHERE ran_id IN(" + ranNumero + "))T2 ON T1.Fecha = T2.Fecha AND T1.Rancho = T2.Rancho";
                        conn.QueryAlimento(query, out dt6);

                        //Numero de vacas
                        DataTable dt7;
                        query = "SELECT ROUND(SUM(CONVERT(FLOAT, " + campoAnimal + "))/ COUNT(DISTINCT ia_fecha),0) FROM inventario_afi "
                                + " WHERE ia_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' "
                                + " AND ran_id IN(" + ranNumero + ")";
                        conn.QueryAlimento(query, out dt7);

                        
                        //Porcentaje de materia seca
                        //DataTable dt8;
                        //query = "SELECT ISNULL(SUM(rac_ms) / SUM(rac_mh) *100,0) AS PorcMS FROM racion "
                        //        + " WHERE rac_fecha >= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "' "
                        //        + " AND ran_id IN(" + ranNumero + ") AND etp_id IN(" + etapa + ")";
                        //conn.QueryAlimento(query, out dt8);

                        //Asignar valores a variables
                        pmsP = PMS(etapa, fechaIni, fechaFin);
                        try
                        {
                            mh = dtM.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dtM.Rows[0][0]) : 0;
                            ms = dtM.Rows[0][1] != DBNull.Value ? Convert.ToDouble(dtM.Rows[0][1]) : 0;
                            numAnimales = dt7.Rows[0][0] != DBNull.Value ? Convert.ToInt32(dt7.Rows[0][0]) : 0;
                            //Int32.TryParse(dt7.Rows[0][0].ToString(), out numAnimales);
                            precioLeche = dt6.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dt6.Rows[0][0]) : 0;
                            //Double.TryParse(dt6.Rows[0][0].ToString(), out precioLeche);
                            lecheFederal = dt5.Rows[0][1] != DBNull.Value ? Convert.ToDouble(dt5.Rows[0][1]) : 0;
                            //Double.TryParse(dt5.Rows[0][1].ToString(), out lecheFederal);
                            media = dt5.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dt5.Rows[0][0]) : 0;
                            //Double.TryParse(dt5.Rows[0][0].ToString(), out media);
                            totalRacion = dt1.Rows[0][0] != DBNull.Value ? Convert.ToDouble(dt1.Rows[0][0]) : 0;
                            //Double.TryParse(dt1.Rows[0][0].ToString(), out totalRacion);                            
                            media = etp == "'1'" || etp == "'1','2','3','4'" ? media : 0;
                        }
                        catch
                        {
                            pmsP = pmsP != 0 ? pmsP : 0;
                            numAnimales = numAnimales != 0 ? numAnimales : 0;
                            precioLeche = precioLeche != 0 ? precioLeche : 0;
                            lecheFederal = lecheFederal != 0 ? lecheFederal : 0;
                            media = media != 0 ? media : 0;
                            totalRacion = Convert.ToDouble(dt1.Rows[0][0]); //Total de racion
                        }

                        DataTable dtALFO;
                        ColumnasDT(out dtALFO);

                        DataTable dtALAS;
                        ColumnasDT(out dtALAS);

                        DataTable dtTotalR;
                        ColumnasDT(out dtTotalR);

                        //double totalA = 0, xvacaA = 0, porcvacaA = 0;
                        //DataTable dtAgua = Agua(etapa, fechaIni, fechaFin);
                        //totalA = Convert.ToDouble(dtAgua.Rows[0][1]);
                        //xvacaA = totalA / numAnimales;

                        string ingrediente;
                        double precioIng, peso, xvaca, costo, precio, porcR;

                        double ingH = 0, xvacaH = 0, pvacaH = 0, totalH = 0, costoH = 0, precioH = 0, ingS = 0, xvacaS = 0, pvacaS = 0, totalS = 0, costoS = 0, precioS = 0, pch = 0, pcs = 0;

                        for (int i = 0; i < dt2.Rows.Count; i++)
                        {
                            dtALFO.ImportRow(dt2.Rows[i]);
                            xvacaH += Convert.ToDouble(dt2.Rows[i]["xvaca"]);
                            pvacaH += Convert.ToDouble(dt2.Rows[i]["porcvaca"]);
                            totalH += Convert.ToDouble(dt2.Rows[i]["TOTAL"]);
                            xvacaS += Convert.ToDouble(dt2.Rows[i]["s_xvaca"]);
                            pvacaS += Convert.ToDouble(dt2.Rows[i]["s_porcvaca"]);
                            totalS += Convert.ToDouble(dt2.Rows[i]["s_TOTAL"]);
                            pch += Convert.ToDouble(dt2.Rows[i]["porccosto"]);
                            pcs += Convert.ToDouble(dt2.Rows[i]["s_porccosto"]);
                            costoH += Convert.ToDouble(dt2.Rows[i]["COSTO"]);
                            costoS += Convert.ToDouble(dt2.Rows[i]["s_COSTO"]);
                            precioH += Convert.ToDouble(dt2.Rows[i]["PRECIO"]);
                            precioS += Convert.ToDouble(dt2.Rows[i]["s_PRECIO"]);
                        }


                        double ingH1 = 0, xvacaH1 = 0, pvacaH1 = 0, totalH1 = 0, costoH1 = 0, precioH1 = 0, ingS1 = 0, xvacaS1 = 0, pvacaS1 = 0, totalS1 = 0, costoS1 = 0, precioS1 = 0, pch1 = 0, pcs1 = 0;

                        for (int i = 0; i < dt3.Rows.Count; i++)
                        {
                            dtALAS.ImportRow(dt3.Rows[i]);
                            xvacaH1 += Convert.ToDouble(dt3.Rows[i]["xvaca"]);
                            pvacaH1 += Convert.ToDouble(dt3.Rows[i]["porcvaca"]);
                            totalH1 += Convert.ToDouble(dt3.Rows[i]["TOTAL"]);
                            xvacaS1 += Convert.ToDouble(dt3.Rows[i]["s_xvaca"]);
                            pvacaS1 += Convert.ToDouble(dt3.Rows[i]["s_porcvaca"]);
                            totalS1 += Convert.ToDouble(dt3.Rows[i]["s_TOTAL"]);
                            costoH1 += Convert.ToDouble(dt3.Rows[i]["COSTO"]);
                            pch1 += Convert.ToDouble(dt3.Rows[i]["porccosto"]);
                            pcs1 += Convert.ToDouble(dt3.Rows[i]["s_porccosto"]);
                            costoH1 += Convert.ToDouble(dt3.Rows[i]["COSTO"]);
                            costoS1 += Convert.ToDouble(dt3.Rows[i]["s_COSTO"]);
                            precioH1 += Convert.ToDouble(dt3.Rows[i]["PRECIO"]);
                            precioS1 += Convert.ToDouble(dt3.Rows[i]["s_PRECIO"]);
                        }

                        double xvacaAg = dtAg.Rows.Count > 0 ? Convert.ToDouble(dtAg.Rows[0]["xvaca"]) : 0;

                        DataRow drA = dtALFO.NewRow();
                        drA[0] = "TOTAL FORRAJE";
                        drA[1] = -1;
                        drA[2] = xvacaH;
                        drA[3] = pvacaH;
                        drA[4] = totalH;
                        drA[5] = costoH;
                        drA[6] = pch;
                        drA[7] = precioH;
                        drA[8] = -1;
                        drA[9] = xvacaS;
                        drA[10] = pvacaS;
                        drA[11] = totalS;
                        drA[12] = costoS;
                        drA[13] = pcs;
                        drA[14] = precioS;
                        drA[15] = -1;
                        dtALFO.Rows.Add(drA);

                        DataRow drAL = dtALAS.NewRow();
                        drAL[0] = "TOTAL CONCENTRADO";
                        drAL[1] = -1;
                        drAL[2] = xvacaH1;
                        drAL[3] = pvacaH1;
                        drAL[4] = totalH1;
                        drAL[5] = costoH1;
                        drAL[6] = pch1;
                        drAL[7] = precioH1;
                        drAL[8] = -1;
                        drAL[9] = xvacaS1;
                        drAL[10] = pvacaS1;
                        drAL[11] = totalS1;
                        drAL[12] = costoS1;
                        drAL[13] = pcs1;
                        drAL[14] = precioS1;
                        drAL[15] = -1;
                        dtALAS.Rows.Add(drAL);

                        DataRow drTR = dtTotalR.NewRow();
                        drTR[0] = "TOTAL RACION";
                        drTR[1] = -1;
                        drTR[2] = xvacaH + xvacaH1 + xvacaAg;
                        drTR[3] = -1;
                        drTR[4] = totalRacion;
                        drTR[5] = costoH + costoH1;
                        drTR[6] = -1;
                        drTR[7] = precioH + precioH1;
                        drTR[8] = -1;
                        drTR[9] = xvacaS + xvacaS1;
                        drTR[10] = -1;
                        drTR[11] = totalRacion * pmsP / 100;
                        drTR[12] = costoS + costoS1;
                        drTR[13] = -1;
                        drTR[14] = precioS + precioS1;
                        drTR[15] = -1;
                        dtTotalR.Rows.Add(drTR);

                        DataTable dtSob;
                        ColumnasDT(out dtSob);
                        DataRow drSob = dtSob.NewRow();
                        double sobrante = Convert.ToDouble(dt4.Rows[0][1]);
                        double xvacaSob = numAnimales > 0 ? sobrante / numAnimales : 0;
                        drSob["ingrediente"] = "SOBRANTE";
                        drSob["xvaca"] = xvacaSob;
                        drSob["TOTAL"] = sobrante;
                        dtSob.Rows.Add(drSob);

                        DataTable dtRS;
                        ColumnasDT(out dtRS);

                        DataRow drRS = dtRS.NewRow();
                        drRS["ingrediente"] = "TOTAL RACION - SOBRANTE";
                        drRS["xvaca"] = numAnimales > 0 ? (totalRacion / numAnimales) - xvacaSob : 0;
                        drRS["TOTAL"] = totalRacion - sobrante;
                        dtRS.Rows.Add(drRS);

                        DataTable enter;
                        ColumnasDT(out enter);

                        DataRow drEnter = enter.NewRow();
                        drEnter["ingrediente"] = "";
                        drEnter["precioIng"] = -1;
                        drEnter["xvaca"] = -1;
                        drEnter["porcvaca"] = -1;
                        drEnter["TOTAL"] = -1;
                        drEnter["COSTO"] = -1;
                        drEnter["porccosto"] = -1;
                        drEnter["PRECIO"] = -1;
                        enter.Rows.Add(drEnter);

                        double xvacaA = 0, porcvaca = 0, totalAg = 0;
                        if (dtAg.Rows.Count > 0)
                        {
                            xvacaA = Convert.ToDouble(dtAg.Rows[0][2]);
                            porcvaca = Convert.ToDouble(dtAg.Rows[0][3]);
                            totalAg = Convert.ToDouble(dtAg.Rows[0][4]);
                        }

                        DataTable dtAgua;
                        ColumnasDT(out dtAgua);

                        DataRow drAgua = dtAgua.NewRow();
                        drAgua["ingrediente"] = "AGUA";
                        drAgua["xvaca"] = xvacaA;
                        drAgua["porcvaca"] = porcvaca;
                        drAgua["TOTAL"] = totalAg;
                        dtAgua.Rows.Add(drAgua);

                        dtResult.Merge(dtTotalR);
                        dtResult.Merge(enter);
                        dtResult.Merge(dtALFO);
                        dtResult.Merge(enter);
                        dtResult.Merge(dtALAS);
                        dtResult.Merge(enter);
                        dtResult.Merge(dtAgua);
                        dtResult.Merge(enter);
                        dtResult.Merge(dtTotalR);
                        dtResult.Merge(enter);
                        dtResult.Merge(dtSob);
                        dtResult.Merge(enter);
                        dtResult.Merge(dtRS);

                        costoT = Costo(etapa, campoAnimal, fechaIni, fechaFin);
                        //Indicadores
                        DataTable dtIndicadores;
                        ColumnasIndicadores(out dtIndicadores);

                        DataRow drIndicadores = dtIndicadores.NewRow();
                        drIndicadores["Animales"] = numAnimales;
                        drIndicadores["media"] = media;
                        drIndicadores["ilcavta"] = numAnimales > 0 && costoT > 0 ? (lecheFederal / numAnimales * precioLeche / costoT) : 0;
                        drIndicadores["icventa"] = media > 0 && numAnimales > 0 ? (lecheFederal / numAnimales * precioLeche) - costoT : -costoT;
                        drIndicadores["eaprod"] = media > 0 ? media / (pmsP * (totalRacion / numAnimales) / 100) : 0;
                        drIndicadores["ilcaprod"] = media > 0 && costoT > 0 ? precioLeche * media / costoT : 0;
                        drIndicadores["icprod"] = media > 0 ? (precioLeche * media) - costoT : -costoT;
                        drIndicadores["preclprod"] = media > 0 ? costoT / media : 0;
                        drIndicadores["mhprod"] = mh;
                        drIndicadores["porcmsprod"] = pmsP;
                        drIndicadores["msprod"] = ms;
                        drIndicadores["saprod"] = numAnimales > 0 ? sobrante / numAnimales : 0;
                        drIndicadores["mssprod"] = numAnimales > 0 ? ((totalRacion - sobrante) / numAnimales) * pmsP / 100 : 0;
                        drIndicadores["easprod"] = media > 0 && numAnimales > 0 ? media / ((totalRacion - sobrante) / numAnimales * pmsP / 100) : 0;
                        drIndicadores["precprod"] = costoT > 0 ? costoT : 0;
                        drIndicadores["precmsprod"] = numAnimales > 0 ? costoT / (pmsP * (totalRacion / numAnimales) / 100) : 0;
                        dtIndicadores.Rows.Add(drIndicadores);

                        ReportDataSource source1 = new ReportDataSource("DataSet2", dtIndicadores);
                        reportViewer1.LocalReport.DataSources.Clear();
                        reportViewer1.LocalReport.DataSources.Add(source1);
                        ReportDataSource source = new ReportDataSource("DataSet1", dtResult);
                        reportViewer1.LocalReport.DisplayName = "Reporte Diario";
                        //this.reportViewer1.LocalReport.ReportPath = ConfigurationManager.AppSettings["reporteDiario"];
                        //reportViewer1.LocalReport.DataSources.Clear();
                        reportViewer1.LocalReport.DataSources.Add(source);

                        string titulo2 = empresa ? tipo == 2 ? "Empresa: " + Empresa() : "Empresa: " + emp_codigo : "Establo: " + Titulos(ranNumero);
                        ReportParameter[] parametros = new ReportParameter[3];
                        parametros[0] = new ReportParameter("Establo", titulo2.ToUpper());
                        parametros[1] = new ReportParameter("periodo", "PERIODO DEL: " + dtpInicial.Date.ToString("dd/MM/yyyy") + " AL: " + dtpFinal.Date.ToString("dd/MM/yyyy"));
                        parametros[2] = new ReportParameter("Etapa", "ETAPA: " + etapa_nombre.Trim());
                        reportViewer1.LocalReport.SetParameters(parametros);

                        reportViewer1.LocalReport.Refresh();
                        reportViewer1.RefreshReport();

                        titulo = empresa ? emp_codigo : Titulos(ranNumero);

                        if (reporte == "")
                        {
                            GTHUtils.SavePDF(reportViewer1, ruta + "REPORTE DIARIO " + titulo + ".pdf");
                            //MessageBox.Show("Reporte generado correctamente", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            string rutapdf = ruta + "REPORTE DIARIO " + titulo + ".pdf";
                            Process.Start(rutapdf);
                        }
                        else
                        {
                            GTHUtils.SavePDF(reportViewer1, ruta + "\\" + etapa_nombre + "_" + reporte + ".pdf");
                        }
                    }
                    else
                    {
                        if (reporte == "")
                        {
                            string message = "No tienes informacion cargada en la Base de Datos de ese etapa en ese periodo";
                            MessageBox.Show(message, "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                        }
                    }
                }
                else
                {
                    if (reporte == "")
                        MessageBox.Show("LA FECHA FINAL NO PUEDE SER MENOR A LA FECHA INICIAL", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (IOException ex) { if (reporte == "") MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (DbException ex) { if (reporte == "") MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception ex) { if (reporte == "") MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }

        }

        private void ForrajeSobT(string etapa, string campo, DateTime inicio, DateTime fin, out DataTable dt)
        {
            ColumnasDT(out dt);
            string sob = Sobrantes();
            int vacas = Animales(campo, inicio.AddDays(1), fin);
            DataTable dt1;
            string query = "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
                + " FROM(  "
                + " SELECT R.Clave,  CONCAT(IIF(SUBSTRING(R.Clave,1,4) = 'ALAS', 'A', IIF(SUBSTRING(R.Clave, 1,4) = 'ALFO', 'F', '')), R.Ing) AS Ing, "
                + " ISNULL(i.ing_precio_sie, 0) AS Precio, ISNULL(SUM(R.PesoS) / SUM(R.PesoH), 0) AS PMS, SUM(R.PesoH) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoH, "
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
                + " IIF(T2.Pmez IS NULL, T1.PesoS, T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, (T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, SUM(rac_ms) AS PesoS "
                + " FROM racion "
                + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
                + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                + " GROUP BY ran_id, ing_descripcion "
                + " ) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc , pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc , pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
                + " GROUP BY T.Ran, T.Clave, T.Ing "
                + " UNION "
                + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh), SUM(rac_ms) "
                + " FROM racion "
                + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ")  AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F', 'W') "
                + " AND SUBSTRING(ing_descripcion, 3, 2) NOT IN('00', '01', '02')  AND etp_id IN(" + etapa + ") AND ing_descripcion like '%90%' "
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
                + " WHERE X.PesoH > 0 AND X.Ing NOT IN(" + sob + ") GROUP BY x.Ing";
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
                s_xvaca = vacas > 0 ?  ms / vacas : 0;
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
                dtTemp.Rows[i]["porcvaca"] = txvaca >  0 ? xvaca / txvaca * 100 : 0;
                dtTemp.Rows[i]["porccosto"] =  costoT > 0?  costo / costoT * 100 : 0;
                dtTemp.Rows[i]["s_porcvaca"] = tsxvaca > 0 ?  s_xvaca / tsxvaca * 100 : 0;
                dtTemp.Rows[i]["s_porccosto"] = costoT > 0? costo / costoT * 100 :0;
            }

            string ing, ingA;

            for (int i = 0; i < dtTemp.Rows.Count; i++)
            {
                ing = dtTemp.Rows[i][0].ToString();
                ingA = ing[0].ToString() + ing[1] + ing[2] + ing[3];
                if (ing[0] != 'A' && ing[0] != 'W')
                {
                    dt.ImportRow(dtTemp.Rows[i]);
                }
            }

            string ingrediente;
            for(int i = 0; i < dt.Rows.Count; i++)
            {
                ingrediente = dt.Rows[i][0].ToString();
                dt.Rows[i][0] = ingrediente.Substring(1);
            }
        }


        private void AlimentosT(string etapa, string campo, DateTime inicio, DateTime fin, out DataTable dt)
        {
            ColumnasDT(out dt);
            int vacas = Animales(campo, inicio.AddDays(1), fin);
            string sob = Sobrantes();
            DataTable dt1;
            string query = "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
                + " FROM(  "
                + " SELECT R.Clave,  CONCAT(IIF(SUBSTRING(R.Clave,1,4) = 'ALAS', 'A', IIF(SUBSTRING(R.Clave, 1,4) = 'ALFO', 'F', '')), R.Ing) AS Ing, "
                + " ISNULL(i.ing_precio_sie, 0) AS Precio, ISNULL(SUM(R.PesoS) / SUM(R.PesoH), 0) AS PMS, SUM(R.PesoH) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "')  AS PesoH, "
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
                + " IIF(T2.Pmez IS NULL, T1.PesoS, T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, (T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, SUM(rac_ms) AS PesoS "
                + " FROM racion "
                + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
                + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                + " GROUP BY ran_id, ing_descripcion "
                + " ) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc, pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc, pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
                + " GROUP BY T.Ran, T.Clave, T.Ing "
                + " UNION "
                + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh), SUM(rac_ms) "
                + " FROM racion "
                + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ")  AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F', 'W') "
                + " AND SUBSTRING(ing_descripcion, 3, 2) NOT IN('00', '01', '02')  AND etp_id IN(" + etapa + ") AND ing_descripcion like '%90%' "
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
                xvaca = vacas > 0?  mh / vacas :0;
                s_xvaca = vacas > 0?  ms / vacas :0;
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
                dtTemp.Rows[i]["porcvaca"] = txvaca > 0 ?  xvaca / txvaca * 100 :0;
                dtTemp.Rows[i]["porccosto"] = costoT >  0 ?  costo / costoT * 100 : 0;
                dtTemp.Rows[i]["s_porcvaca"] =  tsxvaca > 0?  s_xvaca / tsxvaca * 100 :0;
                dtTemp.Rows[i]["s_porccosto"] = costoT > 0 ?  costo / costoT * 100 :0;
            }

            string ing, ingA;

            for (int i = 0; i < dtTemp.Rows.Count; i++)
            {
                ing = dtTemp.Rows[i][0].ToString();
                ingA = ing[0].ToString() + ing[1] + ing[2] + ing[3];
                if (ing[0] == 'A' && ingA.ToUpper() != "AGUA")
                {
                    dt.ImportRow(dtTemp.Rows[i]);
                }
            }

            string ingrediente;
            for(int i = 0; i < dt.Rows.Count; i++)
            {
                ingrediente = dt.Rows[i][0].ToString();
                dt.Rows[i][0] = ingrediente.Substring(1);
            }
        }

        #region METODO PASADO
        // NUEVO METODO

        //public void Reporte(string etapa, string etapa_nombre, DateTime dtpInicial, DateTime dtpFinal, bool empresa, string reporte)
        //{
        //    try
        //    {
        //        //getDias();
        //        conn.Iniciar("DBSIE");
        //        getInfo();

        //        DataTable dt;
        //        string q_r = "SELECT rut_ruta FROM ruta WHERE rut_desc = 'sio'";
        //        conn.QueryMovGanado(q_r, out dt);
        //        ruta = dt.Rows[0][0].ToString();
        //        getEstablos(empresa);
        //        int numAnimales = 0;
        //        double media = 0, pmsP = 0;
        //        double precioLeche = 0, lecheFederal = 0, costoT = 0, precioT = 0, totalRacion = 0;
        //        DataTable dtResult = new DataTable();
        //        DataTable dtindicadores = new DataTable();
        //        int hcorte = 0;
        //        int horas;
        //        Hora_Corte(out horas, out hcorte); 

        //        DateTime fec_max = MaxDate();
        //        dtpFinal = dtpFinal.Date > fec_max.Date ? fec_max : dtpFinal;

        //        DateTime fechaIni = dtpInicial.Date;
        //        DateTime fechaFin = dtpFinal.Date;
        //        DateTime inicio = dtpInicial.Date;
        //        DateTime fin = dtpFinal.Date;
        //        string query;
        //        //string etapa = cbEtapa.SelectedValue.ToString();
        //        string etapa1 = etapa_nombre;//cbEtapa.Text;
        //        etapa = etapa == "10" ? "10,11,12,13" : etapa;
        //        int comparacion = DateTime.Compare(fechaIni, fechaFin);
        //        string campoAnimal = "";
        //        string tituloR = empresa ? emp_codigo : ran_nombre;
        //        //int seleccionados = TotalSeleccionados();
        //        if (comparacion == 0 || comparacion == -1)
        //        {
        //            //if (!checkBox1.Checked)
        //            //{
        //            //    string ranTemp = GetSelectRanchos();
        //            //    ranNumero = ranTemp.Length > 0 ? ranTemp : ranNumero;
        //            //    tituloR = seleccionados > 1 ? seleccionados == clbRanchos.Items.Count ? emp_codigo : Titulos(ranNumero) : ran_nombre;
        //            //}

        //            string sob = Sobrantes();

        //            int dif = 24 + horas;
        //            fechaIni = fechaIni.AddHours(horas);
        //            fechaFin = fechaFin.AddHours(dif);

        //            DataTable dt1;
        //            query = "SELECT T.Total FROM( SELECT ISNULL(SUM(rac_mh) / DATEDIFF(DAY, '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "', '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "'), 0) AS Total "
        //                    + " from racion where rac_fecha"
        //                    + " >= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ") and etp_id IN(" + etapa + ") "
        //                    + " AND ing_descripcion not in(" + sob + ") ) T"
        //                    + " WHERE T.Total > 0";
        //            conn.QueryAlimento(query, out dt1);

        //            if (dt1.Rows.Count > 0)
        //            {
        //                //Obtener las Premezclas que se dieron en este periodo
        //                DataTable dtPremezclas = new DataTable();
        //                query = "select DISTINCT ing_descripcion FROM racion "
        //                    + " WHERE rac_fecha >= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "' "
        //                    + " AND ran_id IN(" + ranNumero.ToString() + ") AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02') "
        //                    + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F') AND etp_id IN(" + etapa + ")";
        //                conn.QueryAlimento(query, out dtPremezclas);

        //                conn.DeleteAlimento("porcentaje_Premezcla", "");
        //                conn.DeleteAlimento("premezcla_dias", "");
        //                DataTable dtt;
        //                for (int i = 0; i < dtPremezclas.Rows.Count; i++)
        //                {
        //                    query = "SELECT TOP(5) * FROM premezcla where pmez_racion like '" + dtPremezclas.Rows[i][0].ToString() + "'";
        //                    conn.QueryAlimento(query, out dtt);

        //                    if (dtt.Rows.Count == 0)
        //                        continue;

        //                    CargarPremezcla(dtPremezclas.Rows[i][0].ToString(), fechaIni, fechaFin);
        //                }
        //                string etp = "";
        //                switch (etapa)
        //                {
        //                    case "10,11,12,13":
        //                        etp = "1";
        //                        campoAnimal = "ia_vacas_ord";
        //                        break;
        //                    case "21":
        //                        etp = "2";
        //                        campoAnimal = "ia_vacas_secas";
        //                        break;
        //                    case "22":
        //                        etp = "4";
        //                        campoAnimal = "ia_vqreto + ia_vcreto";
        //                        break;
        //                    case "31":
        //                        etp = "3";
        //                        campoAnimal = "ia_jaulas";
        //                        break;
        //                    case "32":
        //                        etp = "3";
        //                        campoAnimal = "ia_destetadas";
        //                        break;
        //                    case "33":
        //                        etp = "3";
        //                        campoAnimal = "ia_destetadas2";
        //                        break;
        //                    case "34":
        //                        etp = "3";
        //                        campoAnimal = "ia_vaquillas";
        //                        break;
        //                }

        //                //Seccion de Racion de alas
        //                DataTable dt3;
        //                Alimentos(etapa, campoAnimal, fechaIni, fechaFin, out dt3);
        //                DataTable dt2; DataTable dtAg;

        //                if (ran_sie == 1)
        //                {
        //                    Alimentos(etapa, campoAnimal, fechaIni, fechaFin, out dt3);
        //                    ForrajeSob(etapa, campoAnimal, fechaIni, fechaFin, out dt2);
        //                    Agua(etapa, campoAnimal, fechaIni, fechaFin, out dtAg);
        //                }
        //                else
        //                {
        //                    AlimentosT(etapa, campoAnimal, fechaIni, fechaFin, out dt3);
        //                    ForrajeSobT(etapa, campoAnimal, fechaIni, fechaFin, out dt2);
        //                    Agua(etapa, campoAnimal, fechaIni, fechaFin, out dtAg);
        //                }
        //                ////Seccion de Racion de Forraje y sobrante
        //                //DataTable dt2;
        //                //ForrajeSob(etapa, campoAnimal, fechaIni, fechaFin, out dt2);

        //                //DataTable dtAg;
        //                //Agua(etapa, campoAnimal, fechaIni, fechaFin, out dtAg);
        //                // Seccion Sobrante
        //                DataTable dt4;
        //                query = "SELECT 'SOBRANTE' AS Ingrediente, ISNULL(SUM(rac_mh)/ DATEDIFF(DAY, '" + fechaIni.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "', '" + fechaFin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "'),0) AS Peso "
        //                   + " FROM racion where rac_fecha >= '" + fechaIni.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.AddDays(1).ToString("yyyy-MM-dd HH:mm") + "' AND rac_descripcion like '%SOB%' "
        //                   + " AND ing_descripcion like '" + etp + "%' AND ran_id IN(" + ranNumero + ") AND SUBSTRING(ing_descripcion,3,2) NOT IN('00','01','02','90')";
        //                conn.QueryAlimento(query, out dt4);

        //                //media y leche federal
        //                DataTable dt5;
        //                query = "SELECT IIF(SUM(ia.ia_vacas_ord)>0,ISNULL((SUM(m.med_produc)/SUM(ia.ia_vacas_ord)),0),0) , ISNULL(SUM(m.med_lecfederal + m.med_lecplanta) / COUNT(DISTINCT med_fecha),0) "
        //                        + " FROM media m LEFT JOIN inventario_afi ia ON ia.ran_id = m.ran_id AND ia.ia_fecha = m.med_fecha "
        //                        + " where m.ran_id IN(" + ranNumero + ") AND med_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' ";
        //                conn.QueryAlimento(query, out dt5);

        //                //Precio  Leche
        //                DataTable dt6;
        //                query = "SELECT AVG(T2.Precio) FROM( SELECT  ran_id AS Rancho, MAX(hl_fecha_reg) AS Fecha "
        //                        + " FROM historico_leche where ran_id IN(" + ranNumero + ") GROUP BY ran_id) T1 "
        //                        + " LEFT JOIN( "
        //                        + " SELECT ran_id AS Rancho, hl_fecha_reg AS Fecha, hl_precio AS Precio "
        //                        + " FROM historico_leche "
        //                        + " WHERE ran_id IN(" + ranNumero + "))T2 ON T1.Fecha = T2.Fecha AND T1.Rancho = T2.Rancho";
        //                conn.QueryAlimento(query, out dt6);

        //                //Numero de vacas
        //                DataTable dt7;
        //                query = "SELECT ROUND(SUM(CONVERT(FLOAT, " + campoAnimal + "))/ COUNT(DISTINCT ia_fecha),0) FROM inventario_afi "
        //                        + " WHERE ia_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' "
        //                        + " AND ran_id IN(" + ranNumero + ")";
        //                conn.QueryAlimento(query, out dt7);

        //                //Porcentaje de materia seca
        //                //DataTable dt8;
        //                //query = "SELECT ISNULL(SUM(rac_ms) / SUM(rac_mh) *100,0) AS PorcMS FROM racion "
        //                //        + " WHERE rac_fecha >= '" + fechaIni.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fechaFin.ToString("yyyy-MM-dd HH:mm") + "' "
        //                //        + " AND ran_id IN(" + ranNumero + ") AND etp_id IN(" + etapa + ")";
        //                //conn.QueryAlimento(query, out dt8);

        //                //Asignar valores a variables
        //                pmsP = PMS(etapa, fechaIni, fechaFin);
        //                try
        //                {

        //                    Int32.TryParse(dt7.Rows[0][0].ToString(), out numAnimales);
        //                    Double.TryParse(dt6.Rows[0][0].ToString(), out precioLeche);
        //                    Double.TryParse(dt5.Rows[0][1].ToString(), out lecheFederal);
        //                    Double.TryParse(dt5.Rows[0][0].ToString(), out media);
        //                    Double.TryParse(dt1.Rows[0][0].ToString(), out totalRacion);

        //                    media = etp == "1" ? media : 0;
        //                }
        //                catch
        //                {
        //                    pmsP = pmsP != 0 ? pmsP : 0;
        //                    numAnimales = numAnimales != 0 ? numAnimales : 0;
        //                    precioLeche = precioLeche != 0 ? precioLeche : 0;
        //                    lecheFederal = lecheFederal != 0 ? lecheFederal : 0;
        //                    media = media != 0 ? media : 0;
        //                    totalRacion = Convert.ToDouble(dt1.Rows[0][0]); //Total de racion
        //                }

        //                DataTable dtALFO;
        //                ColumnasDT(out dtALFO);

        //                DataTable dtALAS;
        //                ColumnasDT(out dtALAS);

        //                DataTable dtTotalR;
        //                ColumnasDT(out dtTotalR);

        //                //double totalA = 0, xvacaA = 0, porcvacaA = 0;
        //                //DataTable dtAgua = Agua(etapa, fechaIni, fechaFin);
        //                //totalA = Convert.ToDouble(dtAgua.Rows[0][1]);
        //                //xvacaA = totalA / numAnimales;

        //                string ingrediente;
        //                double precioIng, peso, xvaca, costo, precio, porcR;

        //                double ingH = 0, xvacaH = 0, pvacaH = 0, totalH = 0, costoH = 0, precioH = 0, ingS = 0, xvacaS = 0, pvacaS = 0, totalS = 0, costoS = 0, precioS = 0, pch = 0, pcs = 0;

        //                for (int i = 0; i < dt2.Rows.Count; i++)
        //                {
        //                    dtALFO.ImportRow(dt2.Rows[i]);
        //                    xvacaH += Convert.ToDouble(dt2.Rows[i]["xvaca"]);
        //                    pvacaH += Convert.ToDouble(dt2.Rows[i]["porcvaca"]);
        //                    totalH += Convert.ToDouble(dt2.Rows[i]["TOTAL"]);
        //                    xvacaS += Convert.ToDouble(dt2.Rows[i]["s_xvaca"]);
        //                    pvacaS += Convert.ToDouble(dt2.Rows[i]["s_porcvaca"]);
        //                    totalS += Convert.ToDouble(dt2.Rows[i]["s_TOTAL"]);
        //                    pch += Convert.ToDouble(dt2.Rows[i]["porccosto"]);
        //                    pcs += Convert.ToDouble(dt2.Rows[i]["s_porccosto"]);
        //                    costoH += Convert.ToDouble(dt2.Rows[i]["COSTO"]);
        //                    costoS += Convert.ToDouble(dt2.Rows[i]["s_COSTO"]);
        //                    precioH += Convert.ToDouble(dt2.Rows[i]["PRECIO"]);
        //                    precioS += Convert.ToDouble(dt2.Rows[i]["s_PRECIO"]);
        //                }


        //                double ingH1 = 0, xvacaH1 = 0, pvacaH1 = 0, totalH1 = 0, costoH1 = 0, precioH1 = 0, ingS1 = 0, xvacaS1 = 0, pvacaS1 = 0, totalS1 = 0, costoS1 = 0, precioS1 = 0, pch1 = 0, pcs1 = 0;

        //                for (int i = 0; i < dt3.Rows.Count; i++)
        //                {
        //                    dtALAS.ImportRow(dt3.Rows[i]);
        //                    xvacaH1 += Convert.ToDouble(dt3.Rows[i]["xvaca"]);
        //                    pvacaH1 += Convert.ToDouble(dt3.Rows[i]["porcvaca"]);
        //                    totalH1 += Convert.ToDouble(dt3.Rows[i]["TOTAL"]);
        //                    xvacaS1 += Convert.ToDouble(dt3.Rows[i]["s_xvaca"]);
        //                    pvacaS1 += Convert.ToDouble(dt3.Rows[i]["s_porcvaca"]);
        //                    totalS1 += Convert.ToDouble(dt3.Rows[i]["s_TOTAL"]);
        //                    costoH1 += Convert.ToDouble(dt3.Rows[i]["COSTO"]);
        //                    pch1 += Convert.ToDouble(dt3.Rows[i]["porccosto"]);
        //                    pcs1 += Convert.ToDouble(dt3.Rows[i]["s_porccosto"]);
        //                    costoH1 += Convert.ToDouble(dt3.Rows[i]["COSTO"]);
        //                    costoS1 += Convert.ToDouble(dt3.Rows[i]["s_COSTO"]);
        //                    precioH1 += Convert.ToDouble(dt3.Rows[i]["PRECIO"]);
        //                    precioS1 += Convert.ToDouble(dt3.Rows[i]["s_PRECIO"]);
        //                }

        //                double xvacaAg = dtAg.Rows.Count > 0 ? Convert.ToDouble(dtAg.Rows[0]["xvaca"]) : 0;

        //                DataRow drA = dtALFO.NewRow();
        //                drA[0] = "TOTAL FORRAJE";
        //                drA[1] = -1;
        //                drA[2] = xvacaH;
        //                drA[3] = pvacaH;
        //                drA[4] = totalH;
        //                drA[5] = costoH;
        //                drA[6] = pch;
        //                drA[7] = precioH;
        //                drA[8] = -1;
        //                drA[9] = xvacaS;
        //                drA[10] = pvacaS;
        //                drA[11] = totalS;
        //                drA[12] = costoS;
        //                drA[13] = pcs;
        //                drA[14] = precioS;
        //                drA[15] = -1;
        //                dtALFO.Rows.Add(drA);

        //                DataRow drAL = dtALAS.NewRow();
        //                drAL[0] = "TOTAL CONCENTRADO";
        //                drAL[1] = -1;
        //                drAL[2] = xvacaH1;
        //                drAL[3] = pvacaH1;
        //                drAL[4] = totalH1;
        //                drAL[5] = costoH1;
        //                drAL[6] = pch1;
        //                drAL[7] = precioH1;
        //                drAL[8] = -1;
        //                drAL[9] = xvacaS1;
        //                drAL[10] = pvacaS1;
        //                drAL[11] = totalS1;
        //                drAL[12] = costoS1;
        //                drAL[13] = pcs1;
        //                drAL[14] = precioS1;
        //                drAL[15] = -1;
        //                dtALAS.Rows.Add(drAL);

        //                DataRow drTR = dtTotalR.NewRow();
        //                drTR[0] = "TOTAL RACION";
        //                drTR[1] = -1;
        //                drTR[2] = xvacaH + xvacaH1 + xvacaAg;
        //                drTR[3] = -1;
        //                drTR[4] = totalRacion;
        //                drTR[5] = costoH + costoH1;
        //                drTR[6] = -1;
        //                drTR[7] = precioH + precioH1;
        //                drTR[8] = -1;
        //                drTR[9] = xvacaS + xvacaS1;
        //                drTR[10] = -1;
        //                drTR[11] = totalRacion * pmsP / 100;
        //                drTR[12] = costoS + costoS1;
        //                drTR[13] = -1;
        //                drTR[14] = precioS + precioS1;
        //                drTR[15] = -1;
        //                dtTotalR.Rows.Add(drTR);

        //                DataTable dtSob;
        //                ColumnasDT(out dtSob);
        //                DataRow drSob = dtSob.NewRow();
        //                double sobrante = Convert.ToDouble(dt4.Rows[0][1]);
        //                double xvacaSob = numAnimales > 0 ? sobrante / numAnimales : 0;
        //                drSob["ingrediente"] = "SOBRANTE";
        //                drSob["xvaca"] = xvacaSob;
        //                drSob["TOTAL"] = sobrante;
        //                dtSob.Rows.Add(drSob);

        //                DataTable dtRS;
        //                ColumnasDT(out dtRS);

        //                DataRow drRS = dtRS.NewRow();
        //                drRS["ingrediente"] = "TOTAL RACION - SOBRANTE";
        //                drRS["xvaca"] = numAnimales > 0 ? (totalRacion / numAnimales) - xvacaSob : 0;
        //                drRS["TOTAL"] = totalRacion - sobrante;
        //                dtRS.Rows.Add(drRS);

        //                DataTable enter;
        //                ColumnasDT(out enter);

        //                DataRow drEnter = enter.NewRow();
        //                drEnter["ingrediente"] = "";
        //                drEnter["precioIng"] = -1;
        //                drEnter["xvaca"] = -1;
        //                drEnter["porcvaca"] = -1;
        //                drEnter["TOTAL"] = -1;
        //                drEnter["COSTO"] = -1;
        //                drEnter["porccosto"] = -1;
        //                drEnter["PRECIO"] = -1;
        //                enter.Rows.Add(drEnter);

        //                double xvacaA = 0, porcvaca = 0, totalAg = 0;
        //                if (dtAg.Rows.Count > 0)
        //                {
        //                    xvacaA = Convert.ToDouble(dtAg.Rows[0][2]);
        //                    porcvaca = Convert.ToDouble(dtAg.Rows[0][3]);
        //                    totalAg = Convert.ToDouble(dtAg.Rows[0][4]);
        //                }

        //                DataTable dtAgua;
        //                ColumnasDT(out dtAgua);

        //                DataRow drAgua = dtAgua.NewRow();
        //                drAgua["ingrediente"] = "AGUA";
        //                drAgua["xvaca"] = xvacaA;
        //                drAgua["porcvaca"] = porcvaca;
        //                drAgua["TOTAL"] = totalAg;
        //                dtAgua.Rows.Add(drAgua);

        //                dtResult.Merge(dtTotalR);
        //                dtResult.Merge(enter);
        //                dtResult.Merge(dtALFO);
        //                dtResult.Merge(enter);
        //                dtResult.Merge(dtALAS);
        //                dtResult.Merge(enter);
        //                dtResult.Merge(dtAgua);
        //                dtResult.Merge(enter);
        //                dtResult.Merge(dtTotalR);
        //                dtResult.Merge(enter);
        //                dtResult.Merge(dtSob);
        //                dtResult.Merge(enter);
        //                dtResult.Merge(dtRS);

        //                costoT = Costo(etapa, campoAnimal, fechaIni, fechaFin);
        //                //Indicadores
        //                DataTable dtIndicadores;
        //                ColumnasIndicadores(out dtIndicadores);

        //                DataRow drIndicadores = dtIndicadores.NewRow();
        //                drIndicadores["Animales"] = numAnimales;
        //                drIndicadores["media"] = media;
        //                drIndicadores["ilcavta"] = numAnimales > 0 && costoT > 0 ? (lecheFederal / numAnimales * precioLeche / costoT) : 0;
        //                drIndicadores["icventa"] = media > 0 && numAnimales > 0 ? (lecheFederal / numAnimales * precioLeche) - costoT : 0;
        //                drIndicadores["eaprod"] = media > 0 ? media / (pmsP * (totalRacion / numAnimales) / 100) : 0;
        //                drIndicadores["ilcaprod"] = media > 0 && costoT > 0 ? precioLeche * media / costoT : 0;
        //                drIndicadores["icprod"] = media > 0 ? (precioLeche * media) - costoT : -costoT;
        //                drIndicadores["preclprod"] = media > 0 ? costoT / media : 0;
        //                drIndicadores["mhprod"] = numAnimales > 0 ? totalRacion / numAnimales : 0;
        //                drIndicadores["porcmsprod"] = pmsP;
        //                drIndicadores["msprod"] = numAnimales > 0 ? pmsP * (totalRacion / numAnimales) / 100 : 0;
        //                drIndicadores["saprod"] = numAnimales > 0 ? sobrante / numAnimales : 0;
        //                drIndicadores["mssprod"] = numAnimales > 0 ? ((totalRacion - sobrante) / numAnimales) * pmsP / 100 : 0;
        //                drIndicadores["easprod"] = media > 0 && numAnimales > 0 ? media / ((totalRacion - sobrante) / numAnimales * pmsP / 100) : 0;
        //                drIndicadores["precprod"] = costoT > 0 ? costoT : 0;
        //                drIndicadores["precmsprod"] = numAnimales > 0 ? costoT / (pmsP * (totalRacion / numAnimales) / 100) : 0;
        //                dtIndicadores.Rows.Add(drIndicadores);

        //                ReportDataSource source1 = new ReportDataSource("DataSet2", dtIndicadores);
        //                reportViewer1.LocalReport.DataSources.Clear();
        //                reportViewer1.LocalReport.DataSources.Add(source1);
        //                ReportDataSource source = new ReportDataSource("DataSet1", dtResult);
        //                reportViewer1.LocalReport.DisplayName = "Reporte Diario";
        //                reportViewer1.LocalReport.DataSources.Add(source);

        //                string titulo2 = checkBox1.Checked ? tipo == 2 ? "Empresa: " + Empresa() : "Empresa: " + emp_codigo : "Establos: " + Titulos(ranNumero);
        //                ReportParameter[] parametros = new ReportParameter[3];
        //                parametros[0] = new ReportParameter("Establo", titulo2.ToUpper());
        //                parametros[1] = new ReportParameter("periodo", "PERIODO DEL: " + dtpInicial.Date.ToString("dd/MM/yyyy") + " AL: " + dtpFinal.Date.ToString("dd/MM/yyyy"));
        //                parametros[2] = new ReportParameter("Etapa", "ETAPA: " + etapa1.Trim());
        //                reportViewer1.LocalReport.SetParameters(parametros);

        //                reportViewer1.LocalReport.Refresh();
        //                reportViewer1.RefreshReport();

        //                titulo = checkBox1.Checked ? emp_codigo : Titulos(ranNumero);
        //                GTHUtils.SavePDF(reportViewer1, ruta + "\\" + etapa_nombre + "_" + reporte + ".pdf");
        //                //GTHUtils.SavePDF(reportViewer1, ruta + "REPORTE DIARIO " + titulo + ".pdf");
        //                //MessageBox.Show("Reporte generado correctamente", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                //string rutapdf = ruta + "REPORTE DIARIO " + titulo + ".pdf";
        //                //Process.Start(rutapdf);
        //            }
        //            else
        //            {
        //            }
        //        }
        //        else
        //        {
        //        }
        //    }
        //    catch (IOException ex) { /*MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);*/ }
        //    catch (DbException ex) {/* MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);*/ }
        //    catch (Exception ex) { /*MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);*/ }
        //}
        //----
        #endregion
        private string Titulos(string ranIds)
        {
            string title = "";
            DataTable dt;
            string query = "SELECT ran_desc FROM configuracion WHERE ran_id IN(" + ranIds + ")";
            conn.QuerySIO(query, out dt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                title += dt.Rows[i][0].ToString() + ",";
            }
            return title.Length > 0 ? title.Substring(0, title.Length - 1) : "";
        }

        private string GetSelectRanchos()
        {
            string temp = "";
            foreach (var item in clbRanchos.CheckedItems)
            {
                DataRowView drv = item as DataRowView;
                temp += drv["ID"].ToString() + ",";
            }
            return temp.Length > 0 ? temp.Substring(0, temp.Length - 1) : "";
        }

        private void SelectDefault()
        {
            int c = 0, r;
            foreach(var item in clbRanchos.Items)
            {
                DataRowView drv = item as DataRowView;
                Console.WriteLine(drv["ID".ToString()]);
                r = Convert.ToInt32(drv["ID"].ToString());
                if (r == ran_id)
                {
                    clbRanchos.SetItemChecked(c, true);
                    break;
                }
                c++;
            }            
        }

        private void SupraMezcla(string premezcla, DateTime inicio, DateTime fin)
        {
            DataTable dt;
            DateTime fini = new DateTime(), ffin;
            DataTable dtF = new DataTable();
            string query = "SELECT * FROM porcentaje_Premezcla where pmez_descripcion like '" + premezcla + "'";
            conn.QueryAlimento(query, out dt);
            int temp = 0;
            DataTable dtV;
            int repeticiones = 0;
            if (dt.Rows.Count == 0)
            {
                query = "SELECT * FROM premezcla WHERE pmez_racion like '" + premezcla + "' AND pmez_fecha < '" + inicio.ToString("yyyy-MM-dd HH:mm") + "'";
                conn.QueryAlimento(query, out dtV);
                if(dtV.Rows.Count > 0)
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
                   + " SELECT T1.Pmz, T1.Clave, T1.Ing, (T1.Peso / T2.Peso) , SEC2.Peso / SEC.Peso"
                   + " FROM( "
                   + " SELECT pmez_racion AS Pmz, ing_clave AS Clave, ing_nombre AS Ing, SUM(pmez_peso) AS Peso "
                   + " FROM premezcla "
                   + " WHERE pmez_racion LIKE '" + premezcla + "' AND pmez_fecha >= '" + fini.ToString("yyyy-MM-dd HH:mm") + "' AND pmez_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                   + " GROUP BY pmez_racion, ing_clave, ing_nombre) T1 "
                   + " LEFT JOIN( "
                   + " SELECT pmez_racion AS Pmz, SUM(pmez_peso) AS Peso "
                   + " FROM premezcla "
                   + " WHERE pmez_racion LIKE '" + premezcla + "' AND pmez_fecha >= '" + fini.ToString("yyyy-MM-dd HH:mm") + "' AND pmez_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
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
                double porcentaje,porcentajesecas;
                int repeticiones = 0;
                prmz = premezcla[2].ToString() + premezcla[3];
                query = "SELECT * FROM porcentaje_Premezcla WHERE pmez_descripcion like '" + premezcla + "'";
                conn.QueryAlimento(query, out dtAux);

                if(dtAux.Rows.Count == 0)
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
                            porcentajesecas = Convert.ToDouble(dt.Rows[i][4]);
                            valores += "('" + pmz + "','" + clave + "','" + ingrediente + "'," + porcentaje + "," + porcentajesecas + "),";
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
                        fRacion = dt.Rows[0][2] == DBNull.Value ? fin : Convert.ToDateTime(dt.Rows[0][2]);
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
            catch (DbException ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }        

        private void Reporte_Diario_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-MX");
            conn.Iniciar("DBSIE");
            cbEtapa.DataSource = LlenarComboEtapa();
            cbEtapa.DisplayMember = "RACION";
            cbEtapa.ValueMember = "ID";
            getInfo();
            getEstablos(checkBox1.Checked);
            checkBox1.Cursor = Cursors.Hand;
            dtpFinal.Cursor = Cursors.Hand;
            dtpInicial.Cursor = Cursors.Hand;
            button1.Cursor = Cursors.Hand;
            cbEtapa.Cursor = Cursors.Hand;
            fechaMax = MaxDate();
            fechaMin = MinDate();
            dtpInicial.MinDate = fechaMin;
            dtpInicial.MaxDate = fechaMax;
            dtpFinal.MaxDate = fechaMax;
            dtpFinal.MinDate = fechaMin;
            if (tipo == 2 || tipo == 3)
            {
                if(tipo == 3)
                {
                    clbRanchos.DataSource = Establos(tipo);
                    clbRanchos.DisplayMember = "RANCHO";
                    clbRanchos.ValueMember = "ID";
                    clbRanchos.Visible = true;
                    SelectDefault();
                    label4.Visible = true;
                }
                checkBox1.Visible = true;
            }
        }     

        public void Hora_Corte(out int horas, out int hcorte)
        {
            DataTable dt;
            string query = "select paramvalue from bedrijf_params where name = 'DSTimeShift' ";
            conn.QueryTracker(query, out dt);

            horas = Convert.ToInt32(dt.Rows[0][0]);            
            hcorte = horas >  0 ? horas : 24 + horas;
            
       
        }

        private DataTable LlenarComboEtapa()
        {
            string[,] etapas = new string[9, 2] { 
                { "10", "PRODUCCION" }, 
                { "21", "SECAS" }, 
                { "22", "RETO" }, 
                { "31", "JAULAS" }, 
                { "32", "DESTETE 1" }, 
                { "33", "DESTETE 2" },
                { "34", "VAQUILAS PREÑADAS" }, 
                { "31,32,33,34", "CRIANZA" },
                {"10,11,12,13,21,22,31,32,33,34", "TODOS" } };
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("ID");
            dt1.Columns.Add("RACION");
            for(int i = 0; i < 9; i++)
            {
                DataRow row = dt1.NewRow();
                row["ID"] = etapas[i, 0];
                row["RACION"] = etapas[i, 1];
                dt1.Rows.Add(row);
            }




            //string query = " select  DISTINCT * from ( select DISTINCT " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) IN('10', '11', '12', '13'), '10', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '21', '21', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '22', '22', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '31', '31', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '32', '32', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '33', '33', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '34', '34', ''))))))) as ID, " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) IN('10', '11', '12', '13'), 'PRODUCCION', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '21', 'SECAS', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '22', 'RETO', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '31', 'JAULAS', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '32', 'DESTETE 1', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '33', 'DESTETE 2', " +
            //    "iif(SUBSTRING(description FROM 3 FOR 2) = '34', 'VAQUILLAS PREÑADAS', ''))))))) as Racion " +
            //    "from ds_ration where SUBSTRING(description FROM 3 FOR 2) in ('10', '11', '12', '13', '21', '22', '31', '32', '33', '34') " +
            //    "union all  " +
            //    "select DISTINCT '10' as ID, 'PRODUCCION' as Racion " +
            //    "from ds_ration where SUBSTRING(description FROM 3 FOR 2) not in ('10','11','12','13')  " +
            //    "union all  " +
            //    "select DISTINCT '21' as ID, 'SECAS' as Racion " +
            //    "from ds_ration where SUBSTRING(description FROM 3 FOR 2) not in ('21') " +
            //    "union all  " +
            //    "select DISTINCT '22' as ID, 'RETO' as Racion " +
            //    "from ds_ration where SUBSTRING(description FROM 3 FOR 2) not in ('22') " +
            //    "union all  " +
            //    "select DISTINCT '31' as ID, 'JAULAS' as Racion " +
            //    "from ds_ration where SUBSTRING(description FROM 3 FOR 2) not in ('31') " +
            //    "union all  " +
            //    "select DISTINCT '32' as ID, 'DESTETE 1' as Racion " +
            //    "from ds_ration where SUBSTRING(description FROM 3 FOR 2) not in ('32') " +
            //    "union all  " +
            //    "select DISTINCT '33' as ID, 'DESTETE 2' as Racion " +
            //    "from ds_ration where SUBSTRING(description FROM 3 FOR 2) not in ('33') " +
            //    "union all  " +
            //    "select DISTINCT '34' as ID, 'VAQUILLAS PREÑADAS' as Racion " +
            //    "from ds_ration where SUBSTRING(description FROM 3 FOR 2) not in ('34'))  " +
            //    "";
            //conn.QueryTracker(query, out dt1);            
            return dt1;
        }       

        private void clbRanchos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                int seleccionados = TotalSeleccionados();
                if (seleccionados == clbRanchos.Items.Count)
                    checkBox1.Checked = true;
                else
                    checkBox1.Checked = false;
            }
            else
            {
                int seleccionados = TotalSeleccionados();
                if (seleccionados < clbRanchos.Items.Count)
                    checkBox1.Checked = false;
            }
        }
        private int TotalSeleccionados()
        {
            return clbRanchos.CheckedItems.Count;
        }

        private double PMS(string etapa, DateTime inicio, DateTime fin)
        {
            double v = 0;
            DataTable dt;
            string sobrante = Sobrantes();         
            string query = "SELECT SUM(R.PesoS) / SUM(R.PesoH) * 100 "
                    + " FROM( "
                    +  "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
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
                    + " IIF(T2.Pmez IS NULL, T1.PesoS, T1.PesoS * T2.PorcSeca) AS PesoS "
                    + " FROM( "
                    + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, (T1.PesoS * T2.PorcSeca) AS PesoS "
                    + " FROM( "
                    + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, SUM(rac_ms) AS PesoS "
                    + " FROM racion "
                    + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
                    + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                    + " GROUP BY ran_id, ing_descripcion "
                    + " ) T1 "
                    + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc, pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
                    + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc, pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
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
            v = dt.Rows.Count > 0 ? Convert.ToDouble(dt.Rows[0][0]) : 0;
            return v;

        }

        private DataTable Establos(int tipo)
        {
            DataTable dt;
            string query = "";                 
            if (tipo == 2)
                query = "SELECT ran_id AS ID, ran_desc AS RANCHO FROM configuracion WHERE emp_prorrateo = ( SELECT cr.emp_id FROM configuracion c LEFT JOIN configuracion_rancho cr ON c.ran_id = cr.ran_id where c.ran_id = " + ran_id+ ")";
            else
                query = "SELECT ran_id AS ID, ran_desc AS RANCHO FROM configuracion WHERE emp_id = ( SELECT cr.cr_multiempresa FROM configuracion c LEFT JOIN configuracion_rancho cr ON c.ran_id = cr.ran_id where c.ran_id = " + ran_id + ")";
            //string query = "SELECT  ran_id AS ID, ran_desc AS RANCHO FROM configuracion WHERE emp_id = " + emp_id.ToString();
            conn.QuerySIO(query, out dt);

            return dt;
        }

        private DateTime MaxDate()
        {
            DateTime fecha;
            DataTable dt;
            string query = "";

            if (checkBox1.Checked)
            {
                 query = "SELECT MIN(T.Fecha) FROM( SELECT r.ran_id AS Rancho, CONVERT(date, MAX(r.rac_fecha)) AS Fecha FROM racion r "
                    + " LEFT JOIN[DBSIO].[dbo].configuracion c ON c.ran_id = r.ran_id WHERE c.emp_id = " + emp_id.ToString()
                    + " GROUP BY r.ran_id) T";
            }
            else
            {
                query = "SELECT CONVERT(DATE, MAX(rac_fecha)) FROM racion where ran_id = " + ran_id.ToString();
            }
            conn.QueryAlimento(query, out dt);

            fecha = Convert.ToDateTime(dt.Rows[0][0]);
            return fecha;
        }

        private DateTime MinDate()
        {
            DateTime fecha;
            DataTable dt;
            string query = "";


            if (checkBox1.Checked)
            {
                query = "SELECT MAX(T.Fecha) FROM( SELECT r.ran_id AS Rancho, CONVERT(date, MIN(r.rac_fecha)) AS Fecha FROM racion r "
                    + " LEFT JOIN[DBSIO].[dbo].configuracion c ON c.ran_id = r.ran_id WHERE c.emp_id = " + emp_id.ToString()
                    + " GROUP BY r.ran_id) T";
            }
            else
            {
                query = "SELECT CONVERT(DATE, MIN(rac_fecha)) FROM racion where ran_id = " + ran_id.ToString();
            }
            conn.QueryAlimento(query, out dt);

            fecha = Convert.ToDateTime(dt.Rows[0][0]);
            return fecha;
        }
        private int Animales(string campo, DateTime inicio, DateTime fin)
        {
            DataTable dt;
            int animales = 0;
            string query = "SELECT ROUND(SUM(CONVERT(FLOAT," + campo + ")) / COUNT(DISTINCT ia_fecha), 0 ) AS Vacas FROM inventario_afi WHERE ran_id IN( " + ranNumero + ") AND ia_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "'";
            conn.QueryAlimento(query, out dt);
            if (dt.Rows.Count > 0)
                Int32.TryParse(dt.Rows[0][0].ToString(), out animales);

            return animales;
        }

        private void TotalRacion(string etapa, DateTime inicio, DateTime fin, out DataTable dt)
        {
            //ColumnasDT(out dt);
            DateTime tempI = dtpInicial.Value.Date;
            DateTime tempF = dtpFinal.Value.Date;
            int auxh, auxhc;

            Hora_Corte(out auxh, out auxhc);

            string sob = Sobrantes();
            DataTable dt1;
            string query = "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
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
                + " IIF(T2.Pmez IS NULL, T1.PesoS, T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, (T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, SUM(rac_ms) AS PesoS "
                + " FROM racion "
                + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
                + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                + " GROUP BY ran_id, ing_descripcion "
                + " ) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc, pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc, pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
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
                + " WHERE X.PesoH > 0 AND X.Ing NOT IN(" + sob + ") GROUP BY x.Ing";
            conn.QueryAlimento(query, out dt1);

            DataTable dtTemp; ColumnasDT(out dtTemp);
            double xvaca, s_xvaca, totalR = 0, costoT = 0, txvaca = 0, tsxvaca = 0;
            double mh, ms, pms, precio, costo;

            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                mh = Convert.ToDouble(dt1.Rows[i][3]);
                totalR += mh;                               
            }

            dt = new DataTable();
            dt.Columns.Add();
            if(totalR > 0)
            {
                DataRow row = dt.NewRow();
                row[0] = totalR;
                dt.Rows.Add(row);
            }
           
        }


        private void Alimentos(string etapa, string campo, DateTime inicio, DateTime fin, out DataTable dt)
        {
            ColumnasDT(out dt);
            DateTime tempI = dtpInicial.Value.Date;
            DateTime tempF = dtpFinal.Value.Date;
            int auxh, auxhc;
            Hora_Corte(out auxh, out auxhc);

            int vacas = reportes ? auxh == 0? Animales(campo, inicio, fin.AddDays(-1)) : Animales(campo, inicio.AddDays(1), fin): Animales(campo, tempI, tempF);
            string sob = Sobrantes();
            DataTable dt1;
            string query = "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
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
                + " IIF(T2.Pmez IS NULL, T1.PesoS, T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, (T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, SUM(rac_ms) AS PesoS "
                + " FROM racion "
                + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
                + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                + " GROUP BY ran_id, ing_descripcion "
                + " ) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc , pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc , pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
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
                + " WHERE X.PesoH > 0 AND X.Ing NOT IN(" + sob + ") GROUP BY x.Ing";
            conn.QueryAlimento(query, out dt1);

            DataTable dtTemp; ColumnasDT(out dtTemp);
            double xvaca,  s_xvaca,  totalR = 0, costoT = 0, txvaca=0, tsxvaca=0;
            double mh, ms, pms, precio, costo;

            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                precio = Convert.ToDouble(dt1.Rows[i][1]);
                pms = Convert.ToDouble(dt1.Rows[i][2]);
                mh = Convert.ToDouble(dt1.Rows[i][3]); totalR += mh;
                ms = Convert.ToDouble(dt1.Rows[i][4]);
                xvaca = vacas > 0? mh / vacas : 0;
                s_xvaca = vacas >  0 ? ms / vacas : 0;
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
            
            for(int i = 0; i < dtTemp.Rows.Count; i++)
            {
                xvaca = Convert.ToDouble(dtTemp.Rows[i]["xvaca"]);
                s_xvaca = Convert.ToDouble(dtTemp.Rows[i]["s_xvaca"]);
                mh = Convert.ToDouble(dtTemp.Rows[i]["TOTAL"]);
                costo = Convert.ToDouble(dtTemp.Rows[i]["COSTO"]);
                dtTemp.Rows[i]["porcvaca"] = txvaca >  0 ? xvaca / txvaca * 100 : 0;
                dtTemp.Rows[i]["porccosto"] = costoT >  0 ?costo / costoT * 100 :0;
                dtTemp.Rows[i]["s_porcvaca"] = tsxvaca > 0 ? s_xvaca / tsxvaca * 100: 0;
                dtTemp.Rows[i]["s_porccosto"] = costoT >  0 ? costo / costoT * 100 :0;
            }

            string ing,ingA;

            for (int i = 0; i < dtTemp.Rows.Count; i++)
            {
                ing = dtTemp.Rows[i][0].ToString();
                ing = ing.Length > 0 ? ing: " ";
                if(ing[0] == 'A' && ing.ToUpper() != "AGUA")
                {
                    dt.ImportRow(dtTemp.Rows[i]);
                }
            }
        }
     
        private void ForrajeSob(string etapa, string campo, DateTime inicio, DateTime fin, out DataTable dt)
        {
            ColumnasDT(out dt);
            string sob = Sobrantes();
            DateTime tempI = dtpInicial.Value.Date;
            DateTime tempF = dtpFinal.Value.Date;
            int auxh, auxhc;
            Hora_Corte(out auxh, out auxhc);

            int vacas = reportes ? auxh == 0 ? Animales(campo, inicio, fin.AddDays(-1)) : Animales(campo, inicio.AddDays(1), fin) : Animales(campo, tempI, tempF);
            DataTable dt1;
            string query = "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
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
                + " IIF(T2.Pmez IS NULL, T1.PesoS, T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, (T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, SUM(rac_ms) AS PesoS "
                + " FROM racion "
                + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
                + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                + " GROUP BY ran_id, ing_descripcion "
                + " ) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc, pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc, pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
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
                + " WHERE X.PesoH > 0 AND X.Ing NOT IN(" + sob + ") GROUP BY x.Ing";
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
                xvaca = vacas > 0 ? mh / vacas :0;
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
                dtTemp.Rows[i]["porcvaca"] =  txvaca > 0 ? xvaca / txvaca * 100 : 0;
                dtTemp.Rows[i]["porccosto"] =  costoT > 0 ? costo / costoT * 100 : 0;
                dtTemp.Rows[i]["s_porcvaca"] =  tsxvaca > 0 ? s_xvaca / tsxvaca * 100 : 0;
                dtTemp.Rows[i]["s_porccosto"] = costoT >  0 ?  costo / costoT * 100 : 0;
            }

            string ing, ingA;

            for (int i = 0; i < dtTemp.Rows.Count; i++)
            {
                ing = dtTemp.Rows[i][0].ToString();
                ing = ing.Length > 0 ? ing : " ";
                if (ing[0] != 'A' && ing[0] != 'W' && ing != "")
                {
                    dt.ImportRow(dtTemp.Rows[i]);
                }
            }
        }       

        private void Materias(string etapa, string campo, DateTime inicio, DateTime fin, out DataTable dt)
        {
            //ColumnasDT(out dt);
            DateTime tempI = dtpInicial.Value.Date;
            DateTime tempF = dtpFinal.Value.Date;
            int auxh, auxhc;
            Hora_Corte(out auxh, out auxhc);

            int vacas = reportes ? auxh == 0 ? Animales(campo, inicio, fin.AddDays(-1)) : Animales(campo, inicio.AddDays(1), fin) : Animales(campo, tempI, tempF);
            string sob = Sobrantes();
            DataTable dt1;
            string query = "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
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
                + " IIF(T2.Pmez IS NULL, T1.PesoS, T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, (T1.PesoS * T2.PorcSeca) AS PesoS "
                + " FROM( "
                + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, SUM(rac_ms) AS PesoS "
                + " FROM racion "
                + " WHERE ran_id IN(" + ranNumero + ")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
                + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                + " GROUP BY ran_id, ing_descripcion "
                + " ) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc,pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
                + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc,pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
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


        private void Agua(string etapa, string campo, DateTime inicio, DateTime fin, out DataTable dt)
        {
            ColumnasDT(out dt);
            DateTime tempI = dtpInicial.Value.Date;
            DateTime tempF = dtpFinal.Value.Date;
            int auxh, auxhc;
            Hora_Corte(out auxh, out auxhc);

            int vacas = reportes ? auxh == 0 ? Animales(campo, inicio, fin.AddDays(-1)) : Animales(campo, inicio.AddDays(1), fin) : Animales(campo, tempI, tempF);
            DataTable dt1;
            string sob = Sobrantes();
            string query = "SELECT x.Ing, SUM(X.PesoH *X.Precio)/SUM(X.PesoH) AS Precio, SUM(X.PesoS) / SUM(X.PesoH)*100 AS PMS, SUM(X.PesoH) AS PesoH, SUM(X.PesoS) AS PesoS "
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
                    + " IIF(T2.Pmez IS NULL, T1.PesoS, T1.PesoS * T2.PorcSeca) AS PesoS "
                    + " FROM( "
                    + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso, (T1.PesoS * T2.PorcSeca) AS PesoS "
                    + " FROM( "
                    + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso, SUM(rac_ms) AS PesoS "
                    + " FROM racion "
                    + " WHERE ran_id IN(" + ranNumero +")  AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "'  AND etp_id IN(" + etapa + ") "
                    + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                    + " GROUP BY ran_id, ing_descripcion "
                    + " ) T1 "
                    + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc,pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
                    + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc,pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
                    + " GROUP BY T.Ran, T.Clave, T.Ing "
                    + " UNION "
                    + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh), SUM(rac_ms) "
                    + " FROM racion "
                    + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero +")  AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F', 'W') "
                    + " AND SUBSTRING(ing_descripcion, 3, 2) NOT IN('00', '01', '02')  AND etp_id IN(" + etapa +") "
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
                    + " WHERE X.PesoH > 0 AND X.Ing NOT IN(" + sob + ") GROUP BY x.Ing";        
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
                s_xvaca = vacas > 0 ? ms / vacas :0;
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
                dtTemp.Rows[i]["porcvaca"] = txvaca >  0?  xvaca / txvaca * 100 : 0;
                dtTemp.Rows[i]["porccosto"] = costoT >  0 ? costo / costoT * 100 : 0;
                dtTemp.Rows[i]["s_porcvaca"] =  tsxvaca > 0 ? s_xvaca / tsxvaca * 100 : 0;
                dtTemp.Rows[i]["s_porccosto"] = costoT > 0? costo / costoT * 100 : 0;
            }

            string ing, ingA;
            string agua;
            for (int i = 0; i < dtTemp.Rows.Count; i++)
            {
                ing = dtTemp.Rows[i][0].ToString().ToUpper();
                ing = ing.Length > 0 ? ing : " ";
                if (ing[0] == 'A' || ing[0] =='W')
                {                    
                    if (ing.ToUpper() == "AGUA" || ing.ToUpper() == "WATER")
                    {
                        dt.ImportRow(dtTemp.Rows[i]);
                    }
                }
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

        private void ColumnasIndicadores(out DataTable dt)
        {
            dt = new DataTable();
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

        private string Sobrantes()
        {
            DataTable dt;
            string sobrantes = "";
            string query = "";
            if(ran_sie == 1)
            {
                query = "SELECT description FROM ds_ingredient WHERE is_active = 1 AND is_deleted = 0 AND substring(description from 1 for 1) not in ('A','F','W') "
                    + "  AND SUBSTRING(description from 3 for 2) not in('00','01','02','90') ";
            }
            else
            {
                query = " SELECT description FROM ds_ingredient  WHERE is_active = 1 AND is_deleted = 0 AND description like '%SOB%' AND description not LIKE '%90 SOBRANTE'";
            }
            conn.QueryTracker(query, out dt);

            for(int i = 0; i <dt.Rows.Count; i++)
            {
                sobrantes += "'" + dt.Rows[i][0].ToString() + "',";
            }

            sobrantes = sobrantes.Length > 0 ? sobrantes.Substring(0, sobrantes.Length - 1) : "''";
            return sobrantes;
        }

        private string Empresa()
        {
            string emp;
            DataTable dt;
            string query = "SELECT emp_codigo FROM empresa where emp_id = ("
                + "SELECT emp_id FROM configuracion_rancho where ran_id = " + ran_id + ")";
            conn.QuerySIO(query, out dt);
            emp = dt.Rows[0][0].ToString();

            return emp.Length > 0 ? emp : "";
        }

        private void DiasPremezcla(string premezcla,DateTime inicio,DateTime fin)
        {
            TimeSpan ts = fin - inicio;
            int dias = ts.Days;

            DateTime ini, f1;
            DataTable dt;
            string query = "";

            string valores = "";
            int registros = 0;
            for(int i = 0; i < dias; i++)
            {
                ini = inicio.AddDays(i);
                f1 = ini.AddDays(1);
                Console.WriteLine("inicio: " + ini.ToString("yyyy-MM-dd HH:mm") );
                Console.WriteLine("fin: " + f1.ToString("yyyy-MM-dd HH:mm"));

                query = "SELECT DISTINCT ing_clave, ing_descripcion "
                        + " FROM racion "
                        + " WHERE rac_fecha >= '" + ini.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha< '" + f1.ToString("yyyy-MM-dd HH:mm") + "'"
                        + " AND rac_descripcion LIKE '" + premezcla + "' ";
                conn.QueryAlimento(query, out dt);

                for(int j = 0; j < dt.Rows.Count; j++)
                {
                    valores += "('" + premezcla + "','" + dt.Rows[j][0].ToString() + "','" + dt.Rows[j][1].ToString() + "','" + f1.ToString("yyyy-MM-dd") + "'),";
                }

                registros += dt.Rows.Count;

                if (registros > 500)
                {
                    conn.InsertMasivAlimento("premezcla_dias",valores.Substring(0,valores.Length-1));
                    registros = 0;
                    valores = "";
                }
            }

            if(valores.Length > 0)
                conn.InsertMasivAlimento("premezcla_dias", valores.Substring(0, valores.Length - 1));
        }

        private double Costo(string etapa, string campo, DateTime inicio, DateTime fin)
        {
            double v=0;
            DataTable dt;
            ColumnasDT(out dt);
            DateTime tempI = dtpInicial.Value.Date;
            DateTime tempF = dtpFinal.Value.Date;
            int auxh, auxhc;
            Hora_Corte(out auxh, out auxhc);

            int vacas = reportes ? auxh == 0 ? Animales(campo, inicio, fin.AddDays(-1)) : Animales(campo, inicio.AddDays(1), fin) : Animales(campo, tempI, tempF);
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
            + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc, pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Pmz = T2.Pmez) T1 "
            + " LEFT JOIN(SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc, pmez_porcentaje_seca as PorcSeca FROM porcentaje_Premezcla)T2 ON T1.Ing = T2.Pmez) T "
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
                xvaca = vacas >  0 ? mh / vacas : 0;
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
                dtTemp.Rows[i]["porcvaca"] = txvaca >  0?  xvaca / txvaca * 100 : 0;
                dtTemp.Rows[i]["porccosto"] = costoT >  0?  costo / costoT * 100:0;
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


        //private  double Costo(string etapa, string campo,  DateTime inicio, DateTime fin)
        //{
        //    DataTable dt;
        //    string query = "SELECT SUM(R.COSTO) FROM ("
        //              + "SELECT T1.Ing AS INGREDIENTE, T1.Precio AS '$ ING', IIF(T2.Vacas > 0 ,(T1.Peso / T2.Vacas),0) AS XVACA, T1.Peso / T3.Total * 100 AS '%', "
        //              + " T1.Peso AS TOTAL, T1.Precio* IIF(T2.Vacas > 0 ,(T1.Peso / T2.Vacas),0) AS COSTO, T1.Precio * T1.Peso AS PRECIO "
        //              + " FROM( "
        //              + " SELECT R.Ran, R.Clave, R.Ing, ISNULL(i.ing_precio_sie, 0) AS Precio, SUM(R.Peso)/DATEDIFF(DAY,'" + inicio.ToString("yyyy-MM-dd HH:mm") + "','" + fin.ToString("yyyy-MM-dd HH:mm") + "') AS Peso "
        //              + " FROM( "
        //              + " SELECT ran_id AS Ran, r.ing_clave AS Clave, r.ing_descripcion AS Ing, SUM(rac_mh) AS Peso "
        //              + " FROM racion r "
        //              + " WHERE ran_id IN(" + ranNumero + ") AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND etp_id IN(" + etapa + ") "
        //              + " AND SUBSTRING(ing_clave, 1, 4) IN('ALAS', 'ALFO') GROUP BY ran_id, ing_clave, ing_descripcion "
        //              + " UNION "
        //              + " SELECT T.Ran, T.Clave, T.Ing, SUM(T.Peso) "
        //              + " FROM( "
        //              + " SELECT T1.Ran, IIF(T2.Pmez IS NULL, T1.Clave, T2.Clave) AS Clave, IIF(T2.Pmez IS NULL, T1.Ing, T2.Ing) AS Ing, IIF(T2.Pmez IS NULL, T1.Peso, T1.Peso * T2.Porc) AS Peso "
        //              + " FROM( "
        //              + " SELECT T1.Ran, T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Peso "
        //              + " FROM( "
        //              + " SELECT ran_id As Ran, ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
        //              + " FROM racion "
        //              + " WHERE ran_id IN( " + ranNumero + ") AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
        //              + " AND etp_id IN(" + etapa + ") "
        //              + " AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
        //              + " GROUP BY ran_id, ing_descripcion) T1 "
        //              + " LEFT JOIN( "
        //              + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla )T2 ON T1.Pmz = T2.Pmez) T1 "
        //              + " LEFT JOIN( "
        //              + " SELECT pmez_descripcion AS Pmez, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc FROM porcentaje_Premezcla )T2 ON T1.Ing = T2.Pmez) T "
        //              + " GROUP BY T.Ran, T.Clave, T.Ing "
        //              + " UNION "
        //              + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh) "
        //              + " FROM racion "
        //              + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ") "
        //              + " AND SUBSTRING(ing_descripcion, 1, 1) NOT IN('A', 'F', 'W') AND SUBSTRING(ing_descripcion, 3, 2) NOT IN('00', '01', '02') "
        //              + " AND etp_id IN(" + etapa + ") GROUP BY ran_id, ing_clave, ing_descripcion "
        //              + " UNION "
        //               + " SELECT ran_id, ing_clave, ing_descripcion, SUM(rac_mh) "
        //                + " FROM racion "
        //            + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranNumero + ") "
        //            + " AND ing_descripcion IN('Agua', 'Water')  AND etp_id IN(" + etapa + ") GROUP BY ran_id, ing_clave, ing_descripcion ) R "
        //              + " LEFT JOIN ingrediente i ON i.ing_clave = R.Clave AND i.ing_descripcion = R.Ing AND i.ran_id = R.Ran "
        //              + " GROUP BY R.Ran, R.Clave, R.Ing, i.ing_precio_sie) T1 "
        //              + " LEFT JOIN( SELECT ran_id AS Ran, ROUND(SUM(CONVERT(FLOAT, " + campo+ ")) / COUNT(DISTINCT ia_fecha), 0)  AS Vacas "
        //              + " FROM inventario_afi WHERE ran_id IN(" + ranNumero + ") AND ia_fecha BETWEEN '" + inicio.AddDays(1).ToString("yyyy-MM-dd") + "' AND '" + fin.ToString("yyyy-MM-dd") + "' "
        //              + " GROUP BY ran_id )T2 ON T1.Ran = T2.Ran "
        //              + " LEFT JOIN( SELECT ran_id AS Rancho, SUM(rac_mh)/DATEDIFF(DAY,'" + inicio.ToString("yyyy-MM-dd HH:mm")+ "','" + fin.ToString("yyyy-MM-dd HH:mm") + "') AS Total FROM racion "
        //              + " WHERE ran_id IN(" + ranNumero + ") AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha< '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
        //              + " AND etp_id IN(" + etapa + ") "
        //              + " GROUP BY ran_id )T3 ON T1.Ran = T3.Rancho ) R";
        //    conn.QueryAlimento(query, out dt);

        //    return dt.Rows.Count > 0 ? Convert.ToDouble(dt.Rows[0][0]) : 0;
        //}

        private DataTable Agua(string etapa, DateTime inicio, DateTime fin)
        {
            DataTable dt;
            string query = "SELECT ing_descripcion, SUM(rac_mh)/DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "','" + fin.ToString("yyyy-MM-dd HH:mm") + "') "
                    + " FROM racion where ran_id IN(" + ranNumero + ") "
                    +    " AND rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha< '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                    + " AND ing_descripcion IN('Water', 'Agua') GROUP BY ing_descripcion ";
            conn.QueryAlimento(query, out dt);

            if(dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr[0] = "AGUA";
                dr[1] = 0;
                dt.Rows.Add(dr);
            }

            return dt;
        }
    }
}


