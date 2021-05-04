﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.Common;
using Microsoft.Reporting.WinForms;
using System.Diagnostics;
using System.Collections;
using System.Configuration;

namespace Alimentacion
{
    public partial class Consumo_Por_Ingrediente : Form
    {
        ConnSIO conn = new ConnSIO();
        int emp_id, ran_id;
        string emp_nombre, ran_nombre;
        string establosNumero; // numero a dos caracterese y con '' al principio y final del numero
        string ran_numero; // numero sin concatener '' 
        bool empresa;
        string ruta;
        string emp_codigo;
        int ran_bascula;
        string bal_clave;
        string empTipo2;
        DateTime fechaMax, fechaMin;
        int tipo;
        int ran_sie;

        private void Consumo_Por_Ingrediente_Load(object sender, EventArgs e)
        {
            conn.Iniciar("DBSIE");
            GetParameters();
            button1.Cursor = Cursors.Hand;
            dtpFinal.Cursor = Cursors.Hand;
            dtpInicial.Cursor = Cursors.Hand; 
            fechaMax = MaxDate();
            fechaMin = MinDate();
            dtpFinal.MaxDate = fechaMax;
            dtpFinal.MinDate = fechaMin;
            dtpInicial.MaxDate = fechaMax;
            dtpInicial.MinDate = fechaMin;
            if (tipo == 2 || tipo == 3)
            {
                if(tipo == 3)
                {
                    clbRanchos.DataSource = Establos(tipo);
                    clbRanchos.DisplayMember = "RANCHO";
                    clbRanchos.ValueMember = "ID";
                    clbRanchos.Visible = true;
                    label3.Visible = true;
                    SelectDefault();
                }
                cbEmpresa.Visible = true;
            }

            //Verificar si tiene Merma Cargada

            DataTable dtmerma;
            string querymerma = @"SELECT Ingrediente as Ingrediente , Ing_Clave as Clave , Por_Merma as '% Merma'
                            FROM merma
                            Order by Ingrediente";
            conn.QueryAlimento(querymerma, out dtmerma);

            if (dtmerma.Rows.Count > 0)
            {
                button1.Enabled = true;
            } else
            {
                button1.Enabled = false;
            }





        }

        private void SelectDefault()
        {
            int c = 0, r;
            foreach (var item in clbRanchos.Items)
            {
                DataRowView drv = item as DataRowView;
                Console.WriteLine(drv["ID".ToString()]);
                r = Convert.ToInt32(drv["ID"].ToString());
                if (r == ran_id)
                {
                    Console.WriteLine(drv["RANCHO".ToString()]);
                    clbRanchos.SetItemChecked(c, true);
                    break;
                }
                c++;
            }
        }

        public Consumo_Por_Ingrediente(int ran_id, int emp_id, string ran_nombre, string emp_nombre)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;            
        }
        public Consumo_Por_Ingrediente(int ran_id, int emp_id, string ran_nombre, string emp_nombre, int tipo)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;
            this.tipo = tipo;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
            string query = "", premezcla = "", titulo = "", qaux = "", pmzaux = "", condicion = "", sob;
            DateTime fIni = dtpInicial.Value.Date;
            DateTime fFin = dtpFinal.Value.Date;
            int hcorte = 0;
            int horas;
            Hora_Corte(out horas, out hcorte);
            int dif = 24 + horas;
            int dias_a = horas > 0 ? 0 : -1;
            int comparacion = DateTime.Compare(fIni, fFin);
            double total = 0, x_dia = 0, x_7 = 0, x_14 = 0, x_21 = 0, x_30 = 0;
            bal_clave = "";
            int seleccionados = TotalSeleccionados();
            if(comparacion == 0 || comparacion == -1)
            {
                fIni = dif > 24 ? fIni.AddHours(horas).AddDays(dias_a) : fIni.AddHours(horas);
                fFin = dif > 24 ? fFin.AddHours(dif).AddDays(dias_a) : fFin.AddHours(dif);
                try
                {

                    if (!cbEmpresa.Checked)
                    {
                        string rantemp = GetSelectRanchos();
                        ran_numero = rantemp.Length > 0 ? rantemp : ran_numero;
                        titulo = seleccionados > 1 ? seleccionados == clbRanchos.Items.Count ? emp_codigo : Titulos(ran_numero) : Titulos(ran_numero);
                    }

                    DataTable dtPM;
                    query = "SELECT DISTINCT ing_descripcion "
                        + " FROM racion "
                        + " where rac_fecha BETWEEN '" + fIni.ToString("yyyy-MM-dd HH:mm") + "' AND  '" + fFin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ran_numero.ToString() + ") "
                        + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F') "
                        + " AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02')";
                    conn.QueryAlimento(query, out dtPM);

                    conn.DeleteAlimento("porcentaje_Premezcla", "");
                    
                    DataTable dtV;
                    for(int i = 0; i < dtPM.Rows.Count; i++)
                    {
                        query = "SELECT TOP(5) * FROM premezcla WHERE pmez_racion like '" + dtPM.Rows[i][0].ToString() + "' AND pmez_fecha < '" + fFin.ToString("yyyy-MM-dd HH:mm") + "'";
                        conn.QueryAlimento(query, out dtV);

                        if (dtV.Rows.Count == 0)
                            continue;

                        CargarPremezcla(dtPM.Rows[i][0].ToString(), fIni, fFin);
                        premezcla += "'" + dtPM.Rows[i][0].ToString() + "',";
                    }

                    premezcla = premezcla.Length > 0 ? premezcla.Substring(0, premezcla.Length - 1) : "''";
                    sob = Sobrantes();

                    if (ran_bascula == 1)
                    {
                        DataTable dtBal;
                        query = "SELECT DISTINCT bal_clave FROM [DBSIE].[dbo].bascula WHERE ran_id IN(" + ran_numero + ")";
                        conn.QuerySIE(query, out dtBal);
                        
                        for(int i = 0; i < dtBal.Rows.Count; i++)
                        {
                            bal_clave += dtBal.Rows[i][0].ToString() + ",";                                 
                        }
                        bal_clave = bal_clave.Substring(0, bal_clave.Length - 1);

                        qaux = "LEFT JOIN( "
                                + " SELECT b.ing_clave AS Clave, SUM(b.bol_neto) /DATEDIFF(DAY, '" + fFin.AddDays(-30).ToString("yyyy-MM-dd ") + "','" + fFin.ToString("yyyy-MM-dd") + "') AS Peso "
                                + " FROM boleto b "
                                + " WHERE bal_clave IN( " + bal_clave + ") "
                                + " AND CONVERT(date, bol_fecha) BETWEEN '" + fFin.AddDays(-30).ToString("yyyy-MM-dd") + "' AND '" + fFin.ToString("yyyy-MM-dd") + "' "
                                + " GROUP BY b.ing_clave )Bal ON Bal.Clave = R2.CLAVE";                        
                    }
                    else
                    {
                        query = "SELECT DISTINCT ing_descripcion "
                        + " FROM racion "
                        + " where rac_fecha BETWEEN '" + fFin.AddDays(-30).ToString("yyyy-MM-dd HH:mm") + "' AND  '" + fFin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ran_numero.ToString() + ") "
                        + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F') "
                        + " AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02')";
                        conn.QueryAlimento(query, out dtPM);

                        conn.DeleteAlimento("porcentaje_pmzaux", "");
                        for(int i = 0; i < dtPM.Rows.Count; i++)
                        {
                            //CargarPremezcla(dtPM.Rows[i][0].ToString(), "porcentaje_pmzaux", fFin.AddDays(-30), fFin);
                            pmzaux += "'" + dtPM.Rows[i][0].ToString() + "',";
                        }
                        pmzaux = pmzaux.Length > 0 ? pmzaux.Substring(0, pmzaux.Length - 1) : "''";

                        
                        qaux = "LEFT JOIN( "
                            + " SELECT T3.Clave, T3.Ingrediente, SUM(T3.Peso) AS Peso "
                            + " FROM( "
                            + " SELECT ing_clave AS Clave, ing_descripcion AS Ingrediente, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE ran_id IN(" + ran_numero + ") "
                            + " AND rac_fecha BETWEEN '" + fFin.AddDays(-30).ToString("yyyy-MM-dd HH:mm") + "' AND '" + fFin.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ing_descripcion not in (" + pmzaux +")"
                            + " GROUP BY ing_clave, ing_descripcion "
                            + " UNION "
                            + " SELECT T2.Clave, T2.Ingrediente, T1.Peso * T2.Porcentaje "
                            + " FROM( "
                            + " SELECT ing_descripcion AS PMZ, SUM(rac_mh) AS Peso "
                            + " FROM racion "
                            + " WHERE ran_id IN(" + ran_numero + ") "
                            + " AND rac_fecha BETWEEN '" + fFin.AddDays(-30).ToString("yyyy-MM-dd HH:mm") + "' AND '" + fFin.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " AND ing_descripcion in (" + pmzaux + " ) GROUP BY ing_descripcion )T1 "
                            + " LEFT JOIN( "
                            + " SELECT pmez_descripcion AS PMZ, ing_clave AS Clave, ing_descripcion As Ingrediente, pmez_porcentaje AS Porcentaje "
                            + " FROM porcentaje_pmzaux ) T2 ON T1.PMZ = T2.PMZ) T3 "
                            + " WHERE T3.Ingrediente not in (" + sob + ")"
                            + " GROUP BY T3.Clave, T3.Ingrediente "
                            + " )Bal ON Bal.Clave = R2.CLAVE AND Bal.Ingrediente = R2.INGREDIENTE";
                        condicion = "AND R2.INGREDIENTE not in ('0200 R ALTA 1ER LAC','0202 PREM ALTAS 1ERA','1902 PREM 7-10 MESES','1902 PREM INSEMINADA','1902 PREM PREÑADAS')";
                    }


                    DataTable dtFinal = new DataTable();
                    
                    ConsumoIngrediente(sob, fIni, fFin, out dtFinal);

                    //double ConTotal = 0, ConsumoXDia = 0, Costos = 0, Actual = 0, Importe = 0;

                    //for (int i = 0; i < dtFinal.Rows.Count; i++)
                    //{
                    //    if (dtFinal.Rows[i][9].ToString() != "")
                    //    {
                    //        total += Convert.ToDouble(dtFinal.Rows[i][9]);
                    //    } else
                    //    {
                    //        total += 0;
                    //    }

                    //    ConTotal +=  dtFinal.Rows[i][1].ToString() != "" ? Convert.ToDouble(dtFinal.Rows[i][1]) : 0 ;
                    //    ConsumoXDia += dtFinal.Rows[i][4].ToString() != "" ? Convert.ToDouble(dtFinal.Rows[i][4]) : 0;
                    //    Costos += dtFinal.Rows[i][10].ToString() != "" ? Convert.ToDouble(dtFinal.Rows[i][10]) : 0;
                    //    Actual += dtFinal.Rows[i][11].ToString() != "" ? Convert.ToDouble(dtFinal.Rows[i][11]): 0;
                    //    Importe += dtFinal.Rows[i][14].ToString() != "" ? Convert.ToDouble(dtFinal.Rows[i][14]) :0;

                    //}

                    //DataRow dr = dtFinal.NewRow();
                    //dr[0] = "TOTAL";
                    //dr[9] = total;
                    //dr[1] = ConTotal;
                    //dr[11] = Actual;
                    //dr[4] = ConsumoXDia;
                    //dr[10] = Costos;
                    //dr[14] = Importe;

                    ////dr[9] = total;
                    ////dr[4] = x_dia;
                    ////dr[5] = x_7;
                    ////dr[6] = x_14;
                    ////dr[7] = x_21;
                    ////dr[8] = x_30;
                    //dtFinal.Rows.Add(dr);         
                    int Eliminar = 0;
                    if(ChBox_Alimento.Checked && ChBoxForraje.Checked == false)
                    {
                        Eliminar = 1;
                    } else
                    if(ChBox_Alimento.Checked == false && ChBoxForraje.Checked)
                    {
                        Eliminar = 2;
                    }else
                    if (ChBox_Alimento.Checked && ChBoxForraje.Checked)
                    {
                        
                        Eliminar = 0;
                    } else
                    {
                        Eliminar = 0;
                    }

                    if (Eliminar == 1)
                    {
                        for (int i = 0; i < dtFinal.Rows.Count; i++)
                        {
                            string inicio = dtFinal.Rows[i][0].ToString().Substring(0,4);
                            if(inicio == "ALFO")
                            {
                                dtFinal.Rows[i].Delete();
                            }

                        }
                    } else
                    if (Eliminar == 2)
                    {
                        for (int i = 0; i < dtFinal.Rows.Count; i++)
                        {
                            string inicio = dtFinal.Rows[i][0].ToString().Substring(0, 4);
                            if (inicio == "ALAS")
                            {
                                dtFinal.Rows[i].Delete();
                            }

                        }
                    }

                    dtFinal.AcceptChanges();


                    ReportDataSource source = new ReportDataSource("DataSet1", dtFinal);
                    reportViewer1.LocalReport.DataSources.Clear();
                    reportViewer1.LocalReport.DataSources.Add(source);

                    ReportParameter[] parameters = new ReportParameter[2];
                    if (cbEmpresa.Checked)
                    {
                        string temp = tipo == 2 ? Empresa() : emp_codigo;
                        parameters[0] = new ReportParameter("Establo", temp.ToUpper());
                        parameters[1] = new ReportParameter("Periodo", "PERIODO DEL " + dtpInicial.Value.Date.ToString("dd/MM/yyyy") + " Al " + dtpFinal.Value.Date.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        parameters[0] = new ReportParameter("Establo", titulo.ToUpper());
                        parameters[1] = new ReportParameter("Periodo" , "PERIODO DEL " + dtpInicial.Value.Date.ToString("dd/MM/yyyy") + " Al " +dtpFinal.Value.Date.ToString("dd/MM/yyyy"));
                    }

                    reportViewer1.LocalReport.SetParameters(parameters);

                    reportViewer1.LocalReport.Refresh();
                    reportViewer1.RefreshReport();

                    GTHUtils.SavePDF(reportViewer1, ruta + "Consumo_Ingrediente.pdf");
                    //MessageBox.Show("Reporte generado correctamente", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    string rutapdf = ruta + "Consumo_Ingrediente.pdf";
                    Process.Start(rutapdf);

                }
                catch(IOException ex) { MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                catch(DbException ex) { MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR",MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }   
            else
            {
                MessageBox.Show("LA FECHA FINAL NO PUEDE SER MENOR A LA FECHA INICIAL", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor = Cursors.Default;
        }

        private void Hora_Corte(out int horas, out int hcorte)
        {
            DataTable dt;
            string query = "select paramvalue from bedrijf_params where name = 'DSTimeShift' ";
            conn.QueryTracker(query, out dt);

            horas = Convert.ToInt32(dt.Rows[0][0]);
            hcorte = 24 + horas;

        }
  
        private void GetParameters()
        {
            string query;
            establosNumero = "";
            ran_numero = "";
            int rancho_num;
            string temp;
            DataTable dt;
            establosNumero = "";
            if (cbEmpresa.Checked)
            {
                //query = "SELECT ran_id FROM [DBSIO].[dbo].configuracion WHERE emp_id = " + emp_id.ToString();
                //conn.QueryAlimento(query, out dt);

                dt = Establos(tipo);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    rancho_num = Convert.ToInt32(dt.Rows[i][0]);
                    temp = rancho_num > 9 ? rancho_num.ToString() : "0" + rancho_num.ToString();
                    establosNumero += "'" + temp + "',";
                    ran_numero += rancho_num.ToString() + ",";

                }

                establosNumero = establosNumero.Remove(establosNumero.Length - 1, 1);
                ran_numero = ran_numero.Remove(ran_numero.Length - 1, 1);                 
            }
            else
            {
                temp = ran_id > 9 ? ran_id.ToString() : "0" + ran_id.ToString();
                establosNumero = "'" + temp + "'";
                ran_numero = ran_id.ToString();
            }

            query = "select rut_ruta from ruta where ran_id = " + ran_id;
            conn.QuerySIO(query, out dt);

            if(dt.Rows.Count > 0)
            {
                ruta = dt.Rows[0][0].ToString();
            }
            else
            {
                MessageBox.Show("NO SE HA ESPECIFICADO LA RUTA PARA GUARDAR LOS REPORTES","ERROR!", MessageBoxButtons.OK,MessageBoxIcon.Error);
            }

            query = "SELECT emp_codigo, ran_bascula, ran_sie FROM configuracion WHERE ran_id = " + ran_id.ToString();
            conn.QuerySIO(query, out dt);
            emp_codigo = dt.Rows[0][0].ToString();
            ran_bascula = Convert.ToInt32(dt.Rows[0][1]);
            ran_sie = Convert.ToInt32(dt.Rows[0][2]);
        }

        private void cbEmpresa_CheckedChanged(object sender, EventArgs e)
        {
            GetParameters();
            if (cbEmpresa.Checked)
                for (int i = 0; i < clbRanchos.Items.Count; i++)
                    clbRanchos.SetItemChecked(i, cbEmpresa.Checked);
            else
            {
                if (TotalSeleccionados() == clbRanchos.Items.Count)
                    for (int i = 0; i < clbRanchos.Items.Count; i++)
                        clbRanchos.SetItemChecked(i, false);
                else
                    for(int i = 0; i < clbRanchos.Items.Count; i++)                 
                        clbRanchos.SetItemChecked(i, clbRanchos.GetItemChecked(i));

            }

        }

        private void ConsumoIngrediente(string sobrante, DateTime inicio, DateTime fin, out DataTable dt)
        {
        string query = @"SELECT  T.Clave                                                                                           AS CLAVE
	   ,P.prod_nombre                                                                                                              AS INGREDIENTE
	   ,SUM(T.TOTAL)                                                                                                               AS TOTAL
       ,'0'                                                                                                                        AS Bascula
       ,'0'                                                                                                                        AS Porcentaje
       ,SUM(T.TOTAL) / DATEDIFF(DAY,'" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + @"')                                                         AS X_DIA
       , SUM(T.TOTAL) / DATEDIFF(DAY,'" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + @"') * 7                                                     AS SIETE
       ,SUM(T.TOTAL) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + @"') * 14                                                    AS CATORCE
       ,SUM(T.TOTAL) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + @"') * 21                                                    AS VEINTIUNO
       ,SUM(T.TOTAL) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + @"') * 30                                                    AS TREINTA
       ,Art.art_existencia                                                                                                         AS TOTALEXISTENCIA
       ,Round(Art.art_precio_uni ,2)                                                                                               AS COSTOS
       ,IIF(M.Por_Merma > 0 ,(Art.art_existencia - SUM(T.TOTAL))-((M.Por_Merma*(Art.art_existencia - SUM(T.TOTAL)))/100)  ,Art.art_existencia - SUM(T.TOTAL) ) AS ACTUAL
       ,Round(IIF(M.Por_Merma > 0 ,(Art.art_existencia - SUM(T.TOTAL))-((M.Por_Merma*(Art.art_existencia - SUM(T.TOTAL)))/100)  ,Art.art_existencia - SUM(T.TOTAL) )/(SUM(T.TOTAL) / DATEDIFF(DAY,'" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + @"'))/ 30.35,1) AS MESES
       , Round(IIF(M.Por_Merma > 0 ,(Art.art_existencia - SUM(T.TOTAL))-((M.Por_Merma*(Art.art_existencia - SUM(T.TOTAL)))/100)  ,Art.art_existencia - SUM(T.TOTAL) )/(SUM(T.TOTAL) / DATEDIFF(DAY,'" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + @"')),1)        AS DIAS
       , Round(Art.art_existencia * Art.art_precio_uni,0)                                                                           AS IMPORTE
	   ,M.Por_Merma																												   AS MERMA

	FROM 
	(
					SELECT  ing_clave       AS Clave
					,ing_descripcion AS INGREDIENTE
					,SUM(rac_mh)     AS TOTAL
					FROM racion
					WHERE 
				    rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + @"'

                    AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + @"'

                    AND ran_id IN (" + ran_numero + @") 

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
					    rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + @"'

                        AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + @"'

                        AND ran_id IN (" + ran_numero + @")  

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
                    LEFT JOIN 
                    (
	                    SELECT  A.art_clave
	                           ,A.art_existencia
	                           ,A.Art_precio_uni
	                           ,A.art_fecha
	                           ,A.alm_id
	                    FROM articulo A
                    LEFT JOIN(
		                    SELECT  alm_id
		                           ,alm_nombre
		                           ,alm_tipo
		                           ,emp_id
		                           ,ran_id
		                    FROM [DBSIE].[dbo].[almacen]
		                    WHERE (alm_tipo = 2 or alm_tipo = 3) 
		                    AND ran_id = " + ran_numero + @" 
	                    ) Almac
	                    ON a.alm_id = Almac.alm_id
	                    WHERE DAY(art_fecha) = " + fin.Day + @"  
                        AND MONTH(art_fecha) = " + fin.Month + @"
                        AND YEAR(art_fecha) = " + fin.Year+ @"
                        AND (Almac.alm_tipo = 2 or Almac.alm_tipo = 3)  
                    ) Art
                    ON Art.art_clave = P.prod_clave
                    LEFT JOIN(
		                    SELECT Ingrediente,
			                       Por_Merma
		                    FROM [DBALIMENTO].[dbo].[merma]
                    ) M ON M.Ingrediente = P.prod_clave
                    WHERE T.Clave <> '' AND T.INGREDIENTE NOT IN ('101 SOBRANTE','102 SOBRANTE','103 SOBRANTE','104 SOBRANTE ','105 SOBRANTE','106 SOBRANTE','107 SOBRANTE','108 SOBRANTE','109 SOBRANTE','110 SOBRANTE','111 SOBRANTE','112 SOBRANTE','113 SOBRANTE','114 SOBRANTE','115 SOBRANTE','116 SOBRANTE','117 SOBRANTE','118 SOBRANTE','119 SOBRANTE','226 SOBRANTE S','227 SOBRANTE RV','228 SOBRANTE RV','430 SOBRANTE RV','432 SOBRANTE ','433 SOBRANTE ','424 SOBRANTE Rvq','225 SOBRANTE ','100 SOBR LAS VEGAS','137 SOBRANTE','138 SOBRANTE','139 SOBRANTE','135 SOBRANTE','136 SOBRANTE','431 SOBRANTE ','229 SOBRANTE ','425 SOBRANTE ','230 SOBRANTE S','231SOBRANTE S') 
                    GROUP BY  T.Clave,
		                      P.prod_nombre
                             ,Art.art_existencia
                             ,Art.art_precio_uni
		                     ,M.Por_Merma
                    ORDER BY T.Clave";
            
            conn.QueryAlimento(query, out dt);
        }

        private DateTime MaxDate()
        {
            DateTime fecha;
            DataTable dt;
            string query = "SELECT CONVERT(DATE,MAX(rac_fecha)) FROM racion where ran_id = " + ran_id.ToString();
            conn.QueryAlimento(query, out dt);
            fecha = Convert.ToDateTime(dt.Rows[0][0]);
            return fecha;
        }

        private DateTime MinDate()
        {
            DateTime fecha;
            DataTable dt;
            string query = "SELECT CONVERT(DATE, MIN(rac_fecha)) FROM racion where ran_id = " + ran_id.ToString();
            conn.QueryAlimento(query, out dt);
            fecha = Convert.ToDateTime(dt.Rows[0][0]);
            return fecha;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (var item in clbRanchos.CheckedItems)
            {
                DataRowView drv = item as DataRowView;
                Console.WriteLine(drv["ID"].ToString());
                Console.WriteLine(drv["RANCHO"].ToString());
            }
          
          
        }

        private String GetSelectRanchos()
        {
            string temp = "";
            foreach (var item in clbRanchos.CheckedItems)
            {
                DataRowView drv = item as DataRowView;
                temp += drv["ID"].ToString() + ",";
            }
            return temp.Length > 0 ? temp.Substring(0, temp.Length - 1) : "";
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
                        fRacion = Convert.ToDateTime(dt.Rows[0][2]);
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

        private void CargarPremezclaT(string premezcla, string tabla, DateTime inicio, DateTime fin)
        {
            try
            {
                //DateTime ftemp;
                DateTime fRacion, fIng;
                DateTime fin2 = inicio.AddDays(1);
                DateTime fpmI = inicio, fpmF = new DateTime();
                int temp = 0;
                DataTable dt;
                DataTable dt1 = new DataTable();
                string pmz, clave, ingrediente, valores = "", prmz, query;
                double porcentaje;

                prmz = premezcla[2].ToString() + premezcla[3];

                if (prmz == "01")
                {
                    query = query = "SELECT T1.Pmz, T1.Clave, T1.Ing, T1.Peso / T2.Total "
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
                        conn.InsertMasivAlimento(tabla, valores);
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
                               + " WHERE rac_fecha< '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_descripcion like '" + premezcla + "' "
                               + " GROUP BY rac_descripcion "
                               + " )T3 ON T1.Premezcla = T3.Premezcla";
                    conn.QueryAlimento(query, out dt);

                    fIng = Convert.ToDateTime(dt.Rows[0][1]);
                    fRacion = Convert.ToDateTime(dt.Rows[0][2]);
                    int comparacion = DateTime.Compare(fRacion, fIng);

                    if (comparacion == 1)
                    {
                        do
                        {
                            fpmI = inicio.AddDays(-1);
                            fpmF = fin2.AddDays(-1);

                            fpmI = fpmI.AddDays(temp);
                            fpmF = fpmF.AddDays(temp);

                            query = " SELECT * FROM premezcla WHERE pmez_racion like '" + premezcla + "' "
                                + " AND pmez_fecha BETWEEN '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fpmF.ToString("yyyy-MM-dd HH:mm") + "' ";
                            conn.QueryAlimento(query, out dt1);
                            temp--;
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
                        {
                            fpmI = new DateTime(fRacion.Year, fRacion.Month, fRacion.Day, inicio.Hour, 0, 0);
                        }
                    }

                    DataTable dtSPM;
                    query = "SELECT T.ing_nombre AS Ingrediente, SUM(pmez_peso) AS Peso FROM( SELECT DISTINCT * FROM premezcla "
                        + " WHERE pmez_racion like '" + premezcla + "' AND pmez_fecha BETWEEN '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' ) T "
                        + " WHERE SUBSTRING(T.ing_nombre, 1, 1) NOT IN('A','F') AND SUBSTRING(T.ing_nombre,3,2) IN('01', '02', '00') GROUP BY  T.ing_nombre";
                    conn.QueryAlimento(query, out dtSPM);

                    if (dtSPM.Rows.Count > 0)
                    {
                        string subpremezcla = "";
                        for (int i = 0; i < dtSPM.Rows.Count; i++)
                        {
                            subpremezcla += "'" + dtSPM.Rows[i][0].ToString() + "',";
                        }
                        subpremezcla = subpremezcla.Substring(0, subpremezcla.Length - 1);

                        query = "SELECT x.PMZ, ing.Clave AS Clave, x.Ingrediente, SUM(x.PORCENTAJE) AS Porcentaje FROM( "
                           + " SELECT T.PMZ, CASE WHEN T.Ingrediente2 IS NULL THEN T.Ingrediente ELSE T.Ingrediente2 END AS Ingrediente, "
                           + " CASE WHEN T.PORC2 IS NULL THEN T.PORC1 ELSE T.PORC2 * T.PORC1 END AS PORCENTAJE FROM( "
                           + " SELECT T1.Racion AS PMZ, T1.Ingrediente, T1.Peso / T2.Peso AS PORC1, "
                           + " T3.Ingrediente AS Ingrediente2, T3.PORC AS PORC2 "
                           + " FROM( "
                           + " SELECT rac_descripcion AS Racion, ing_descripcion AS Ingrediente, SUM(rac_mh)  AS Peso "
                           + " FROM racion "
                           + " where rac_fecha BETWEEN '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "'"
                           + " AND rac_descripcion like '" + premezcla + "' GROUP BY rac_descripcion, ing_descripcion ) T1 "
                           + " LEFT JOIN( SELECT rac_descripcion AS Racion, SUM(rac_mh)  AS Peso FROM racion "
                           + " where rac_fecha BETWEEN '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                           + " AND rac_descripcion like '" + premezcla + "' GROUP BY rac_descripcion )T2 ON T1.Racion = T2.Racion "
                           + " LEFT JOIN( SELECT T1.Racion, T1.Ingrediente, T1.Peso / T2.Peso AS PORC FROM( "
                           + " select rac_descripcion AS Racion, ing_descripcion AS Ingrediente, SUM(rac_mh)  AS Peso "
                           + " from racion where rac_descripcion in (" + subpremezcla + ") AND rac_fecha BETWEEN '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                           + " GROUP BY rac_descripcion, ing_descripcion ) T1 "
                           + " LEFT JOIN( SELECT rac_descripcion AS Racion, SUM(rac_mh)  AS Peso FROM racion "
                           + " where rac_fecha BETWEEN '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                           + " AND rac_descripcion IN(" + subpremezcla + ") GROUP BY rac_descripcion ) T2 ON T1.Racion = T2.Racion "
                           + " ) T3 ON T3.Racion = T1.Ingrediente) T)x LEFT JOIN ( "
                           + " SELECT ing_clave AS Clave, ing_descripcion AS Ingrediente FROM ingrediente where ran_id = " + ran_id.ToString()
                           + " )ing ON x.Ingrediente = ing.Ingrediente GROUP BY x.PMZ, x.Ingrediente, ing.Clave ORDER BY Clave desc";
                        conn.QueryAlimento(query, out dt);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            pmz = dt.Rows[i][0].ToString();
                            clave = dt.Rows[i][1].ToString();
                            ingrediente = dt.Rows[i][2].ToString();
                            porcentaje = Convert.ToDouble(dt.Rows[i][3]);
                            if (Char.IsDigit(ingrediente[0]) && Char.IsDigit(ingrediente[2]) && Char.IsDigit(ingrediente[3]) && (ingrediente[2].ToString() + ingrediente[3]) == "01")
                            {
                                Console.WriteLine(ingrediente);
                                DataTable dt01;
                                query = " SELECT T1.Pmz, T1.Clave, T1.Ingrediente, (T1.Peso / T2.Total) AS Porcentaje "
                                    + " FROM( "
                                    + " SELECT T.pmez_racion AS Pmz, ing_clave AS Clave, ing_nombre AS Ingrediente, SUM(T.pmez_peso) AS Peso "
                                    + " FROM( "
                                    + " SELECT DISTINCT * "
                                    + " FROM premezcla "
                                    + " WHERE pmez_racion LIKE '" + ingrediente + "' "
                                    + " AND pmez_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "') T "
                                    + " GROUP BY pmez_racion, ing_clave, ing_nombre) T1 "
                                    + " LEFT JOIN( "
                                    + " SELECT T.pmez_racion AS Pmz, SUM(T.pmez_peso) AS Total "
                                    + " FROM( "
                                    + " SELECT DISTINCT * "
                                    + " FROM premezcla "
                                    + " WHERE pmez_racion LIKE '" + ingrediente + "' "
                                    + " AND pmez_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "') T "
                                    + " GROUP BY pmez_racion) T2 ON T1.Pmz = T2.Pmz";
                                conn.QueryAlimento(query, out dt01);

                                for (int index = 0; index < dt01.Rows.Count; index++)
                                {
                                    double paux, p1, porc;
                                    clave = dt01.Rows[index][1].ToString();
                                    ingrediente = dt01.Rows[index][2].ToString();
                                    paux = Convert.ToDouble(dt01.Rows[index][3]);
                                    porc = porcentaje;
                                    p1 = porc * paux;
                                    valores += "('" + pmz + "','" + clave + "','" + ingrediente + "'," + p1.ToString() + "),";
                                }
                            }
                            else
                            {
                                valores += "('" + pmz + "','" + clave + "','" + ingrediente + "'," + porcentaje.ToString() + "),";

                            }

                        }

                        if (valores.Length > 0)
                        {
                            valores = valores.Substring(0, valores.Length - 1);
                            conn.InsertMasivAlimento(tabla, valores);
                        }

                    }
                    else
                    {
                        query = "SELECT T1.Pmz, T1.Clave, T1.Ingrediente, T1.Peso / T2.Peso AS Porcentaje "
                            + " FROM( "
                            + " SELECT T.pmez_racion AS Pmz, T.ing_clave AS Clave, T.ing_nombre AS Ingrediente, SUM(pmez_peso) AS Peso "
                            + " FROM( "
                            + " SELECT DISTINCT * "
                            + " FROM premezcla "
                            + " WHERE pmez_racion like '" + premezcla + "' AND pmez_fecha BETWEEN '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " ) T "
                            + " GROUP BY T.pmez_racion, T.ing_clave, T.ing_nombre "
                            + " ) T1 "
                            + " LEFT JOIN( "
                            + " SELECT T.pmez_racion AS Pmz, SUM(pmez_peso) AS Peso "
                            + " FROM( "
                            + " SELECT DISTINCT * "
                            + " FROM premezcla "
                            + " WHERE pmez_racion like '" + premezcla + "' AND pmez_fecha BETWEEN '" + fpmI.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                            + " ) T "
                            + " GROUP BY T.pmez_racion "
                            + " )T2 ON T1.Pmz = T2.Pmz";
                        conn.QueryAlimento(query, out dt);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            pmz = dt.Rows[i][0].ToString();
                            clave = dt.Rows[i][1].ToString();
                            ingrediente = dt.Rows[i][2].ToString();
                            porcentaje = Convert.ToDouble(dt.Rows[i][3]);

                            valores += "('" + pmz + "','" + clave + "','" + ingrediente + "'," + porcentaje.ToString() + "),";
                        }
                        if (valores.Length > 1)
                        {
                            valores = valores.Substring(0, valores.Length - 1);
                            conn.InsertMasivAlimento(tabla, valores);
                        }
                    }
                }
            }
            catch (DbException ex)
            {
                MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
            }
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


        private void clbRanchos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!cbEmpresa.Checked)            
            {
                int seleccionados = TotalSeleccionados();
                if (seleccionados == clbRanchos.Items.Count)
                    cbEmpresa.Checked = true;
                else
                    cbEmpresa.Checked = false;
            }
            else
            {
                int seleccionados = TotalSeleccionados();
                if (seleccionados < clbRanchos.Items.Count)
                    cbEmpresa.Checked = false;
            }
        }

        private String Sobrantes()
        {
            string sobrantes = "";
            DataTable dt;
            string query = "SELECT description FROM ds_ingredient WHERE is_active = 1 AND is_deleted = 0 AND substring(description from 1 for 1) not in ('A','F','W') "
                    + "  AND SUBSTRING(description from 3 for 2) not in('00','01','02','90') ";
            conn.QueryTracker(query, out dt);

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                sobrantes += "'" + dt.Rows[i][0].ToString() + "',";
            }
            return sobrantes.Length > 0 ? sobrantes.Substring(0, sobrantes.Length - 1) : "''";
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Merma Merma = new Merma(this.button1 , ran_numero);
            Merma.Show();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private int TotalSeleccionados()
        {
            return clbRanchos.CheckedItems.Count;
        }

        private String Titulos(string ranIds)
        {
            string title = "";
            DataTable dt;
            string query = "SELECT ran_desc FROM configuracion WHERE ran_id IN(" + ranIds + ")";
            conn.QuerySIO(query, out dt);
            for(int i = 0; i < dt.Rows.Count; i++)
            {
                title += dt.Rows[i][0].ToString() + ",";
            }
            return title.Length > 0 ? title.Substring(0, title.Length - 1) : "";
        }

        private void DiasPremezcla(string premezcla, DateTime inicio, DateTime fin)
        {
            TimeSpan ts = fin - inicio;
            int dias = ts.Days;

            DateTime ini, f1;
            DataTable dt;
            string query = "";

            string valores = "";
            int registros = 0;
            for (int i = 0; i < dias; i++)
            {
                ini = inicio.AddDays(i);
                f1 = ini.AddDays(1);
                Console.WriteLine("inicio: " + ini.ToString("yyyy-MM-dd HH:mm"));
                Console.WriteLine("fin: " + f1.ToString("yyyy-MM-dd HH:mm"));

                query = "SELECT DISTINCT ing_clave, ing_descripcion "
                        + " FROM racion "
                        + " WHERE rac_fecha >= '" + ini.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha< '" + f1.ToString("yyyy-MM-dd HH:mm") + "'"
                        + " AND rac_descripcion LIKE '" + premezcla + "' ";
                conn.QueryAlimento(query, out dt);

                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    valores += "('" + premezcla + "','" + dt.Rows[j][0].ToString() + "','" + dt.Rows[j][1].ToString() + "','" + f1.ToString("yyyy-MM-dd") + "'),";
                }

                registros += dt.Rows.Count;

                if (registros > 500)
                {
                    conn.InsertMasivAlimento("premezcla_dias", valores.Substring(0, valores.Length - 1));
                    registros = 0;
                    valores = "";
                }
            }

            if (valores.Length > 0)
                conn.InsertMasivAlimento("premezcla_dias", valores.Substring(0, valores.Length - 1));
        }

    }
}
