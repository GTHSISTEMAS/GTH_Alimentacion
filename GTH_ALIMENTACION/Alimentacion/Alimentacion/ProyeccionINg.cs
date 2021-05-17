using System;
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
using System.Linq;
using ght001720q;
using ght001720q.StrongTypesNS;

namespace Alimentacion
{
    public partial class ProyeccionINg : Form
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
        int numeroDeEstablos = 1;
        string rancho = "";
        string erp = "";
        string sUrl;
        DataTable dtFechaTracker;
        string queryFechaTracker;

        private void getinfoEstablo()
        {
            DataTable dt, dt1, dt2, dt3;
            string query = "SELECT rancholocal FROM RANCHOLOCAL";
            conn.Iniciar();
            conn.QueryMovGanado(query, out dt);
            rancho = dt.Rows[0][0].ToString();

            string condicion = "where ran_id = " + rancho.ToString();
            conn.QuerySIO("select erp_id, emp_id, track_id, ran_sie from configuracion "+ condicion, out dt1);
            erp = dt1.Rows[0][0].ToString();
            emp_id = Convert.ToInt32(dt1.Rows[0][1]);

        }

        private void Consumo_Por_Ingrediente_Load(object sender, EventArgs e)
        {
            getinfoEstablo();
            conn.Iniciar("DBSIE");
            sUrl = ConfigurationSettings.AppSettings["url"];
            GetParameters();
            button1.Cursor = Cursors.Hand;
            //dtpFinal.Cursor = Cursors.Hand;
            //dtpInicial.Cursor = Cursors.Hand; 
            fechaMax = MaxDate();
            fechaMin = MinDate();
            //dtpFinal.MaxDate = fechaMax;
            //dtpFinal.MinDate = fechaMin;
            //dtpInicial.MaxDate = fechaMax;
            //dtpInicial.MinDate = fechaMin;
            if (tipo == 2 || tipo == 3)
            {
                if(tipo == 3)
                {
                    //clbRanchos.DataSource = Establos(tipo);
                    //clbRanchos.DisplayMember = "RANCHO";
                    //clbRanchos.ValueMember = "ID";
                    //clbRanchos.Visible = true;
                    //label3.Visible = true;
                    //SelectDefault();
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

            this.reportViewer2.RefreshReport();

            ActualizarFecha();
           


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

        public ProyeccionINg(int ran_id, int emp_id, string ran_nombre, string emp_nombre)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;            
        }
        public ProyeccionINg(int ran_id, int emp_id, string ran_nombre, string emp_nombre, int tipo)
        {
            InitializeComponent();
            this.ran_id = ran_id;
            this.ran_nombre = ran_nombre;
            this.emp_id = emp_id;
            this.emp_nombre = emp_nombre;
            this.tipo = tipo;
        }

        private void InventarioAlmacen(int emp, string almacenes, out DataTable Articulos)
        {
            try
            {
                int dia = 0;
                string url = sUrl.Replace("@", erp);
                string articuloCve, almacen, valores;
                double existencia, precio;
                DateTime hoy = DateTime.Now;
                DateTime mesAnt = hoy.AddMonths(-1);
                int c = 0;

                ght001720 sie = new ght001720(url, "", "", "");
                wARTXALMDataTable articulo = new wARTXALMDataTable();

                sie.ght001720q(emp, hoy.Year, hoy.Month, true,out articulo);

                Articulos = articulo;
               
               
            }
            catch { Articulos = null; }


        }

        private String GetAlmacenes(int emp_id)
        {
            string alm = "";
            DataTable dt;
            string query = "SELECT alm_id "
                        + " FROM[DBSIE].[dbo].almacen "
                        + " WHERE alm_tipo IN(2, 3) AND emp_id IN(" + emp_id + ")";
            conn.QueryAlimento(query, out dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                alm += "'" + dt.Rows[i][0].ToString() + "',";
            }
            return alm.Length > 0 ? alm.Substring(0, alm.Length - 1) : "''";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            DateTime InicioDia = DateTime.Now;
            //InicioDia = dtpInicial.Value.Date;
            Cursor = Cursors.Hand;
            string query = "", premezcla = "", titulo = "", qaux = "", pmzaux = "", condicion = "", sob;
            //DateTime fIni = dtpInicial.Value.Date;
            //DateTime fFin = dtpFinal.Value.Date;
            DateTime fActual = DateTime.Now.Date;
            int hcorte = 0;
            int horas;
            Hora_Corte(out horas, out hcorte);
            int dif = 24 + horas;
            int dias_a = horas > 0 ? 0 : -1;
            //int comparacion = DateTime.Compare(fIni, fFin);
            //int comparacionDiaActual = DateTime.Compare(fActual, fIni);
            double total = 0, x_dia = 0, x_7 = 0, x_14 = 0, x_21 = 0, x_30 = 0;
            bal_clave = "";
            int seleccionados = TotalSeleccionados();



            if (txtDiasProyeccion.Text != "" && Convert.ToInt32(txtDiasProyeccion.Text) > 0 )
            {

                //fIni = dif > 24 ? fIni.AddHours(horas).AddDays(dias_a) : fIni.AddHours(horas);
                InicioDia = dif > 24 ? fActual.AddHours(dif).AddDays(dias_a) : fActual.AddHours(dif);
                try
                {

                    if (!cbEmpresa.Checked)
                    {
                        string rantemp = GetSelectRanchos();
                        ran_numero = rantemp.Length > 0 ? rantemp : ran_numero;
                        titulo = seleccionados > 1 ? seleccionados == clbRanchos.Items.Count ? emp_codigo : Titulos(ran_numero) : Titulos(ran_numero);
                    }

                    sob = Sobrantes();
                    DataTable dtFinal = new DataTable();
                    DataTable dtFinalCopy = new DataTable();
                    // Añadimos las columnas a nuestro DT
                    dtFinalCopy.Columns.Add("Clave", typeof(String));
                    dtFinalCopy.Columns.Add("Producto", typeof(String));
                    dtFinalCopy.Columns.Add("Existencia", typeof(Double));
                    dtFinalCopy.Columns.Add("Consumo", typeof(Double));
                    dtFinalCopy.Columns.Add("Costos", typeof(Double));
                    dtFinalCopy.Columns.Add("NumeroFecha", typeof(Double));
                    dtFinalCopy.Columns.Add("Por_Merma", typeof(Double));
                    dtFinalCopy.Columns.Add("Por_Extra", typeof(Double));


                    Proyeccion(sob, InicioDia, out dtFinal);

                    DateTime oPrimerDiaDelMes = new DateTime(InicioDia.Year, InicioDia.Month, 1, InicioDia.Hour, InicioDia.Minute, InicioDia.Second);

                    int Eliminar = 0;
                    if (ChBox_Alimento.Checked && ChBoxForraje.Checked == false)
                    {
                        Eliminar = 1;
                    }
                    else
                    if (ChBox_Alimento.Checked == false && ChBoxForraje.Checked)
                    {
                        Eliminar = 2;
                    }
                    else
                    if (ChBox_Alimento.Checked && ChBoxForraje.Checked)
                    {

                        Eliminar = 0;
                    }
                    else
                    {
                        Eliminar = 0;
                    }

                    if (Eliminar == 1)
                    {
                        for (int i = 0; i < dtFinal.Rows.Count; i++)
                        {
                            string inicio = dtFinal.Rows[i][0].ToString().Substring(0, 4);
                            if (inicio == "ALAS")
                            {
                                DataRow dr = dtFinal.Rows[i];
                                dtFinalCopy.ImportRow(dr);
                                dtFinalCopy.AcceptChanges();

                            }

                        }
                        dtFinal = dtFinalCopy;
                    }
                    else
                    if (Eliminar == 2)
                    {
                        for (int i = 0; i < dtFinal.Rows.Count; i++)
                        {
                            string inicio = dtFinal.Rows[i][0].ToString().Substring(0, 4);
                            if (inicio == "ALFO")
                            {

                                DataRow dr = dtFinal.Rows[i];
                                dtFinalCopy.ImportRow(dr);
                                dtFinalCopy.AcceptChanges();

                            }

                        }
                        dtFinal = dtFinalCopy;
                    }


                    dtFinal.Columns.Add("Actual", typeof(Double));
                    dtFinal.Columns.Add("ConsumoxDia", typeof(Double));
                    dtFinal.Columns.Add("Dias", typeof(Double));
                    dtFinal.Columns.Add("Meses", typeof(Double));
                    dtFinal.Columns.Add("Importe", typeof(Double));
                    dtFinal.Columns.Add("Proyeccion", typeof(Double));
                    dtFinal.Columns.Add("ProyeccionActual", typeof(Double));

                    dtFinal.AcceptChanges();






                    for (int i = 0; i < dtFinal.Rows.Count; i++)
                    {
                        string NumDias = dtFinal.Rows[i][5].ToString();

                        if (NumDias == "")
                        {
                            dtFinal.Rows[i].Delete();
                        }
                        else
                        {
                            string numdiasProyect = txtDiasProyeccion.Text;
                            int TotalExistencia = Convert.ToInt32(dtFinal.Rows[i][2]);
                            int Consumos = Convert.ToInt32(dtFinal.Rows[i][3]);
                            double NumDiass = dtFinal.Rows[i][5].ToString() != "" ? Convert.ToDouble(dtFinal.Rows[i][5]) : 0;
                            double X_DIA = Consumos / NumDiass;

                            if (Convert.ToDateTime(dtFechaTracker.Rows[0][1]) < InicioDia)
                            {
                                Consumos = Consumos + (int)X_DIA;
                                dtFinal.Rows[i][3] = Consumos;

                            }

                            int Actual = TotalExistencia - Consumos;
                            double merma = dtFinal.Rows[i][6].ToString() != "" ? Convert.ToDouble(dtFinal.Rows[i][6]) : 0;
                            dtFinal.Rows[i][6] = merma;
                            double Extra = dtFinal.Rows[i][7].ToString() != "" ? Convert.ToDouble(dtFinal.Rows[i][7]) : 0;
                            dtFinal.Rows[i][7] = Extra;
                            double ActualConMerma = Actual - ((merma * Actual) / 100);
                            dtFinal.Rows[i][8] = ActualConMerma;                            
                            dtFinal.Rows[i][9] = X_DIA;
                            double DIAS = (double)ActualConMerma / X_DIA;
                            dtFinal.Rows[i][10] = Math.Round(DIAS, 1);

                            double MESES = DIAS / 30.35;
                            dtFinal.Rows[i][11] = Math.Round(MESES, 1);
                            double COSTO = dtFinal.Rows[i][4].ToString() != "" ? Convert.ToDouble(dtFinal.Rows[i][4]) : 0;
                            double IMPORTE = COSTO * TotalExistencia;
                            dtFinal.Rows[i][12] = IMPORTE;
                            double DiasFaltantes = Convert.ToInt32(numdiasProyect) - Math.Round(DIAS, 1);
                            double Proyeccion = 0;
                            if (DiasFaltantes > 0)
                            {

                                Proyeccion = Math.Round(X_DIA, 0) * DiasFaltantes;
                                if (Extra > 0)
                                {
                                    double proyeccionPor = (Proyeccion * Extra) / 100;
                                    Proyeccion = Proyeccion + proyeccionPor;
                                }
                                
                            }
                            else
                            {
                                Proyeccion = 0;
                            }

                            dtFinal.Rows[i][13] = Proyeccion;
                            DateTime time = DateTime.Now;

                            if(NumDiass+1 < time.Day)
                            {
                                dtFinal.Rows[i][14] = 1;
                            } else
                            {
                                dtFinal.Rows[i][14] = 0;
                            }

                        }
                    }

                    dtFinal.AcceptChanges();


                    ReportDataSource source = new ReportDataSource("DataSet1", dtFinal);
                    reportViewer1.LocalReport.DataSources.Clear();
                    reportViewer1.LocalReport.DataSources.Add(source);

                    ReportParameter[] parameters = new ReportParameter[3];
                    if (cbEmpresa.Checked)
                    {
                        string temp = tipo == 2 ? Empresa() : emp_codigo;
                        parameters[0] = new ReportParameter("Establo", temp.ToUpper());
                        parameters[1] = new ReportParameter("Proyeccion", "Proyeccion Generada para " + txtDiasProyeccion.Text + " Dias.");
                        parameters[2] = new ReportParameter("Consumo", "Consumo\n" + oPrimerDiaDelMes.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        parameters[0] = new ReportParameter("Establo", titulo.ToUpper());
                        parameters[1] = new ReportParameter("Proyeccion", "Proyeccion Generada para " + txtDiasProyeccion.Text + " Dias.");
                        parameters[2] = new ReportParameter("Consumo", "Consumo\n" + oPrimerDiaDelMes.ToString("dd/MM/yyyy"));
                    }

                    reportViewer1.LocalReport.SetParameters(parameters);

                    reportViewer1.LocalReport.Refresh();
                    reportViewer1.RefreshReport();

                    GTHUtils.SavePDF(reportViewer1, ruta + "Consumo_Proyeccion.pdf");
                    //MessageBox.Show("Reporte generado correctamente", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    string rutapdf = ruta + "Consumo_Proyeccion.pdf";
                    Process.Start(rutapdf);

                }
                catch (IOException ex) { MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                catch (DbException ex) { MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }


                Cursor = Cursors.Default;
            } else
            {
                MessageBox.Show("NECESITAS ESCRIBIR DIAS EN LA PROYECCION.", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                numeroDeEstablos = dt.Rows.Count;
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

        private void ConsumoXIngrediente(string sobrante, DateTime inicio, DateTime fin, out DataTable dt)
        {
            string query = "SELECT R.INGREDIENTE, SUM(R.TOTAL) AS TOTAL, '0' AS Bascula, '0' AS Porcentaje, "
                        + " SUM(R.TOTAL) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "') AS X_DIA, "
                        + " SUM(R.TOTAL) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "') * 7 AS SIETE, "
                        + " SUM(R.TOTAL) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "') * 14 AS CATORCE, "
                        + " SUM(R.TOTAL) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "') * 21 AS VEINTIUNO, "
                        + " SUM(R.TOTAL) / DATEDIFF(DAY, '" + inicio.ToString("yyyy-MM-dd HH:mm") + "', '" + fin.ToString("yyyy-MM-dd HH:mm") + "') * 30 AS TREINTA "
                        + " FROM( "
                        + " SELECT ing_clave AS Clave, ing_descripcion AS INGREDIENTE, SUM(rac_mh) AS TOTAL "
                        + " FROM racion "
                        + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                        + " AND ran_id IN(" + ran_numero + ") AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) = 0 AND SUBSTRING(rac_descripcion,3,2) not in('00','01','02') "
                        + " GROUP BY ing_clave, ing_descripcion "
                        + " UNION "
                        + " SELECT T.Clave, T.Ingrediente, SUM(T.Total) "
                        + " FROM( "
                        + " SELECT IIF(R.Pmz = '', R.Clave1, R.Clave2) AS Clave, IIF(R.Pmz = '', R.Ing1, R.Ing2) AS Ingrediente, R.Total * R.Porc AS Total "
                        + " FROM( "
                        + " SELECT T1.Clave AS Clave1, T1.Ing AS Ing1, T1.Total, ISNULL(T2.Pmz, '') AS Pmz, ISNULL(T2.Clave, '') AS Clave2, ISNULL(T2.Ing, '') AS Ing2, ISNULL(T2.Porc, 1) AS Porc "
                        + " FROM( "
                        + " SELECT R.Clave, R.Ing, SUM(R.Total) AS Total "
                        + " FROM( "
                        + " SELECT T2.Clave, T2.Ing, (T1.Peso * T2.Porc) AS Total "
                        + " FROM( "
                        + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS Peso "
                        + " FROM racion "
                        + " WHERE rac_fecha >= '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND rac_fecha < '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                        + " AND ran_id IN(" + ran_numero + ") AND ISNUMERIC(SUBSTRING(ing_descripcion, 1, 1)) > 0 "
                        + " AND SUBSTRING(ing_descripcion, 3, 2) IN('00', '01', '02') "
                        + " AND SUBSTRING(rac_descripcion,3,2) NOT IN('00','01','02')"
                        + " GROUP BY ing_descripcion) T1 "
                        + " LEFT JOIN( "
                        + " SELECT pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                        + " FROM porcentaje_Premezcla "
                        + " )T2 ON T1.Pmz = T2.Pmz) R "
                        + " GROUP BY R.Clave, R.Ing) T1 "
                        + " LEFT JOIN( "
                        + " SELECT pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ing, pmez_porcentaje AS Porc "
                        + " FROM porcentaje_Premezcla "
                        + " )T2 ON T1.Ing = T2.Pmz) R) T "
                        + " GROUP BY T.Clave, T.Ingrediente) R "
                        + " WHERE R.INGREDIENTE NOT IN(" + sobrante + ") "
                        + " GROUP BY R.Clave, R.INGREDIENTE";

                        conn.QueryAlimento(query, out dt);
        }

        
        private void Proyeccion(string sobrante, DateTime inicio, out DataTable dt)
        {
            DataTable Articulos, Fecha_Contadores, dtCreacion, dtAlmacenes;
            dt = new DataTable();
            // Añadimos las columnas a nuestro DT
            dt.Columns.Add("Clave", typeof(String));
            dt.Columns.Add("Producto", typeof(String));
            dt.Columns.Add("Existencia", typeof(Double));
            dt.Columns.Add("Consumo", typeof(Double));
            dt.Columns.Add("Costos", typeof(Double));
            dt.Columns.Add("NumeroFecha", typeof(Double));
            dt.Columns.Add("Por_Merma", typeof(Double));
            dt.Columns.Add("Por_Extra", typeof(Double));

            //El dia dependiendo del dia lo seteamos.
            DateTime oUltimoDiaDelMes;
            if (inicio.Day >= 1 && inicio.Day <= 5)
            {
                DateTime oPrimerDiaDelMes = new DateTime(inicio.Year, inicio.Month, 1, inicio.Hour, inicio.Minute, inicio.Second);

                oUltimoDiaDelMes = oPrimerDiaDelMes.AddDays(-1);
            }
            else
            {
                oUltimoDiaDelMes = inicio;

            }

            //COnsultamos el ddl para sacar las existencia y los ingrediente
            InventarioAlmacen(emp_id, GetAlmacenes(emp_id), out Articulos);


            //Seleccionamos los alamecene en caso de cada rancho
            string queryALM = "SELECT alm_id "
                      + " FROM[DBSIE].[dbo].almacen "
                      + " WHERE alm_tipo IN(2, 3) AND ran_id IN(" + ran_numero + ")";
            conn.QueryAlimento(queryALM, out dtAlmacenes);

            // Verificamos que el check de empresa este Check para sacar el reporte o en todo caso eliminar los almacenes no correspondientes al establo
            if (!cbEmpresa.Checked)
            {
                for (int i = 0; i < Articulos.Rows.Count; i++)
                {
                    int Existe_Almacen = 0;
                    for (int x = 0; x < dtAlmacenes.Rows.Count; x++)
                    {
                        if (dtAlmacenes.Rows[x][0].ToString() == Articulos.Rows[i][1].ToString())
                        {
                            Existe_Almacen = 1;
                        }
                    }

                    if (Existe_Almacen != 1)
                    {
                        Articulos.Rows[i].Delete();
                    }
                }
            }

            List<string> excludedAlmacen = new List<string>();
            for (int x = 0; x < dtAlmacenes.Rows.Count; x++)
            {
                excludedAlmacen.Add(dtAlmacenes.Rows[x][0].ToString());
            }

            Articulos.AcceptChanges();

            //Agrupamos y sumamos en caso de que fuera por empresa, la existencia y por clave.
            var ArticulosGroup = Articulos.AsEnumerable().GroupBy(
            r => new
            {
                clave = r.Field<string>("ArticuloCve"),
                almacen = r.Field<string>("AlmacenCve"),
                existencia = r.Field<decimal>("Existencia")
            }).Where(n => excludedAlmacen.Contains(n.Key.almacen));

            var ArticulosGroup2 = ArticulosGroup.AsEnumerable().GroupBy(
            Ren => new
            {
                clave = Ren.Key.clave
            }).Select(x => new
            {
                ArticuloCve = x.Key.clave,
                Existencia = x.Sum(y => y.Key.existencia),
            });


            //Llenamos nuestro datatable Articulos, con los Articulos del SIE y sus existencias.
            var dtAux = new DataTable();
            dtAux.Columns.Add("ArticuloCve", typeof(string));
            dtAux.Columns.Add("Existencia", typeof(double));
            foreach (var item in ArticulosGroup2)
            {
                DataRow nrow = dtAux.NewRow();
                nrow["ArticuloCve"] = item.ArticuloCve;
                nrow["Existencia"] = item.Existencia;
                dtAux.Rows.Add(nrow);
            }

            Articulos = dtAux;

            //Recorremos Datatable por articulos
            for (int i = 0; i < Articulos.Rows.Count; i++)
            {

                //Buscamos la fecha de contador de cada ingrediente.
                string query_FechaContadores = @" SELECT 
                                            DISTINCT ART.art_clave,
                                            iif(Art.art_fecha_cont = '0001-01-01','2020-01-01',Art.art_fecha_cont),
                                            A.ran_id,
                                            A.alm_id
                                            FROM [DBALIMENTO].[dbo].[articulo] ART
                                            LEFT JOIN DBSIE.dbo.almacen A ON A.alm_id = ART.alm_id
                                            where A.alm_tipo in(2,3) and A.ran_id  IN (" + ran_numero + ") and Day(Art.art_fecha) = " + oUltimoDiaDelMes.ToString("dd") + " AND MONTH(Art.art_fecha) = " + oUltimoDiaDelMes.ToString("MM") + " AND YEAR(Art.art_fecha) =" + oUltimoDiaDelMes.ToString("yyyy") + " and ART.art_clave = '" + Articulos.Rows[i][0].ToString() + "' and ART.art_fecha_cont != ''";
                conn.QueryAlimento(query_FechaContadores, out Fecha_Contadores);
                string query_Consumos = "";
                string select_NumFehcas = "";

                if (Fecha_Contadores.Rows.Count > 0)
                {
                    //Recorremos el datatable de Fecha_Contadores para generar su consumo desde el inicio de fecha de contadores.
                    for (int j = 0; j < Fecha_Contadores.Rows.Count; j++)
                    {
                        string QueryComplemental = "";
                        if (j == 0)
                        {

                            DateTime dateTime;

                            if (Fecha_Contadores.Rows[j][1].ToString() == "01/01/0001 12:00:00 a. m.")
                            {
                                dateTime = Convert.ToDateTime("2020-01-01");
                                TimeSpan ts = new TimeSpan(inicio.Hour, inicio.Minute, inicio.Second);
                                dateTime = dateTime.Date + ts;
                            }
                            else
                            {
                                dateTime = Convert.ToDateTime(Fecha_Contadores.Rows[j][1].ToString());
                                TimeSpan ts = new TimeSpan(inicio.Hour, inicio.Minute, inicio.Second);
                                dateTime = dateTime.Date + ts;
                            }



                            query_Consumos = @"SELECT  ing_clave       AS Clave
	                                       ,SUM(rac_mh)     AS TOTAL
	                                FROM racion
	                                WHERE 
                                    ing_clave = '" + Articulos.Rows[i][0].ToString() + @"'
                                    AND rac_fecha >= '" + dateTime.ToString("yyyy-MM-dd HH:mm") + @"'
                                    AND rac_fecha < '" + inicio.ToString("yyyy-MM-dd HH:mm") + @"'
	                                AND ran_id IN(" + ran_numero + @") 
	                                GROUP BY  ing_clave
	                                          UNION
	                                SELECT  T.Clave
	                                       ,SUM(T.Total)
	                                FROM 
	                                (
		                                SELECT  IIF(R.Pmz = '',R.Clave1,R.Clave2) AS Clave
		                                       ,IIF(R.Pmz = '',R.Ing1,R.Ing2)     AS Ingrediente
		                                       ,R.Total * R.Porc                  AS Total
		                                FROM 
		                                (
			                                SELECT  T1.Clave            AS Clave1
			                                       ,T1.Ing              AS Ing1
			                                       ,T1.Total
			                                       ,ISNULL(T2.Pmz,'')   AS Pmz
			                                       ,ISNULL(T2.Clave,'') AS Clave2
			                                       ,ISNULL(T2.Ing,'')   AS Ing2
			                                       ,ISNULL(T2.Porc,1)   AS Porc
			                                FROM 
			                                (
				                                SELECT  R.Clave
				                                       ,R.Ing
				                                       ,SUM(R.Total) AS Total
				                                FROM 
				                                (
					                                SELECT  T2.Clave
					                                       ,T2.Ing
					                                       ,(T1.Peso * T2.Porc) AS Total
					                                FROM 
					                                (
						                                SELECT  ing_descripcion AS Pmz
						                                       ,SUM(rac_mh)     AS Peso
						                                FROM racion
						                                WHERE 
                                                        ing_clave = '" + Articulos.Rows[i][0].ToString() + @"'
                                                        AND rac_fecha >= '" + dateTime.ToString("yyyy-MM-dd HH:mm") + @"'
                                                        AND rac_fecha < '" + inicio.ToString("yyyy-MM-dd HH:mm") + @"'
	                                                    AND ran_id IN(" + ran_numero + @") 
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
				                                ) R
				                                GROUP BY  R.Clave
				                                         ,R.Ing
			                                ) T1
			                                LEFT JOIN 
			                                (
				                                SELECT  pmez_descripcion AS Pmz
				                                       ,ing_clave        AS Clave
				                                       ,ing_descripcion  AS Ing
				                                       ,pmez_porcentaje  AS Porc
				                                FROM porcentaje_Premezcla 
			                                )T2
			                                ON T1.Ing = T2.Pmz
		                                ) R
	                                ) T
	                                GROUP BY  T.Clave
	                                        
                                ";


                            select_NumFehcas = @"  
                                                SELECT  ing_clave       AS Clave,
                                                count(Distinct(FORMAT (rac_fecha, 'dd-MM-yy'))) as NumeroFecha
											    FROM racion
												WHERE 
												ing_clave = '" + Articulos.Rows[i][0].ToString() + @"'
                                                AND rac_fecha >= '" + dateTime.ToString("yyyy-MM-dd HH:mm") + @"'
                                                AND rac_fecha < '" + inicio.ToString("yyyy-MM-dd HH:mm") + @"'
	                                            AND ran_id IN(" + ran_numero + @") 
												GROUP BY  ing_clave
												 ";




                        }
                    }


                    //GEneramos el query de proyeccion en el cual consultaremos las claves, el consumo , el nombre del producto y la merma.
                    string Query_Proyeccion = @"SELECT 
	                                            Existencia.Clave as CLAVE,
                                                Producto.prod_nombre as INGREDIENTE,
	                                            " + Articulos.Rows[i][1].ToString() + @" as EXISTENCIA,
	                                            ConsumosTracker.TOTAL AS CONSUMO,
	                                            Round(Existencia.PRECIO / Existencia.EXISTENCIA, 2) as COSTOS,
	                                            Fechas.NumeroFecha AS NUMERODIAS,
	                                            Merma.Por_Merma as MERMA,
                                                Merma.Por_Extra as EXTRA
                                            FROM(
                                                    SELECT 
													sum(Art.art_existencia * ART.art_precio_uni) AS PRECIO,
													ART.art_clave AS CLAVE, 
													sum(Art.art_existencia) AS EXISTENCIA
													FROM [DBALIMENTO].[dbo].[articulo] ART
													LEFT JOIN DBSIE.dbo.almacen A ON A.alm_id = ART.alm_id
													where A.alm_tipo in(2,3) and A.ran_id IN (" + ran_numero + @") and ART.art_clave = '" + Articulos.Rows[i][0].ToString() + @"'
                                                    group by Art.art_clave
												 ) Existencia

                                                LEFT JOIN(
												 " + select_NumFehcas + @"
												 )Fechas on Fechas.Clave = Existencia.CLAVE

												 LEFT JOIN (
                                                 "
                                                    + query_Consumos +
                                                      @"
													)ConsumosTracker on ConsumosTracker.CLAVE = Existencia.Clave
												LEFT JOIN(
												  SELECT Ingrediente,
														 Por_Merma,
                                                         Por_Extra
														 FROM [DBALIMENTO].[dbo].[merma]
												) Merma ON Merma.Ingrediente = ConsumosTracker.CLAVE
                                                LEFT JOIN(
            					                            SELECT 
            						                               [prod_clave]
            						                              ,prod_nombre
            					                              FROM [DBALIMENTO].[dbo].[producto]
                                                ) Producto ON Producto.prod_clave = Existencia.CLAVE    
                                                 where ConsumosTracker.TOTAL > 0";

                    conn.QueryAlimento(Query_Proyeccion, out dtCreacion);

                    // Agregamos nuestro resultado al dt final.
                    foreach (DataRow dr in dtCreacion.Rows)
                    {
                        dt.Rows.Add(dr.ItemArray);
                    }

                }
            }


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

        Merma Merma;
        private void button2_Click_1(object sender, EventArgs e)
        {
            DataTable dt;
            int rancho_num = 0;
            dt = Establos(tipo);
            string ran_numero_aux = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                rancho_num = Convert.ToInt32(dt.Rows[i][0]);
                ran_numero_aux += rancho_num.ToString() + ",";

            }
           
            ran_numero_aux = ran_numero_aux.Remove(ran_numero_aux.Length - 1, 1);

            if(Merma != null)
            {
                if (!Merma.Visible)
                {
                    Merma = new Merma(this.button1, ran_numero_aux);
                    Merma.Show();
                }
                else
                {
                    Merma.Focus();
                }
            } else
            {
                Merma =new Merma(this.button1, ran_numero_aux);
                Merma.Show();
            }


        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private void txtDiasProyeccion_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
            (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

           // string argumentos = days.ToString() + " " + inicio.ToString("yyyy-MM-dd") + " " + fin.ToString("yyyy-MM-dd");
            Process p = new Process();
            string cadenaExe = ConfigurationManager.AppSettings["ConsumoExe"];
            p.StartInfo.FileName = cadenaExe;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            p.Start();
            p.BeginOutputReadLine();
            p.WaitForExit();

            //Process.Start("C:\\Users\\lgomez\\Documents\\Proyectos\\GTH_Consumos\\Consumos\\Consumos\\bin\\Debug\\Consumos.exe", days.ToString());
            ActualizarFecha();
            Cursor = Cursors.Default;
            
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Console.WriteLine(outLine.Data);
        }

        private int TotalSeleccionados()
        {
            return clbRanchos.CheckedItems.Count;
        }

        public void ActualizarFecha()
        {

            queryFechaTracker = "SELECT 'Informacion del Tracker' AS Descripcion, FORMAT(MAX(rac_fecha), 'dd/MM/yyyy HH:mm', 'es-mx' ) AS Fecha "
                    + " FROM racion r "
                    + " WHERE ran_id = " + ran_id.ToString() + " AND ing_polvo = 0 "
                    + " UNION "
                    + " SELECT 'Existencia' AS Descripcion, FORMAT(MAX(art.art_fecha), 'dd/MM/yyyy HH:mm', 'es-mx') AS Fecha "
                    + " FROM articulo art "
                    + " LEFT JOIN[DBSIE].[dbo].almacen alm ON alm.alm_id = art.alm_id "
                    + " WHERE alm.ran_id = " + ran_id.ToString()
                    + " UNION "
                    + " SELECT 'Bascula' , IIF(ran_bascula = 1, (SELECT FORMAT(MAX(bol_fecha), 'dd/MM/yyyy HH:mm', 'es-mx') "
                    + " FROM boleto bol LEFT JOIN[DBSIE].[dbo].bascula bal ON bol.bal_clave = bal.bal_clave WHERE bal.ran_id = " + ran_id.ToString() + "), "
                    + " (SELECT FORMAT(MAX(rac_fecha), 'dd/MM/yyyy HH:mm', 'es-mx') FROM racion where ran_id = " + ran_id.ToString() + ")) AS Fecha "
                    + " FROM[DBSIO].[dbo].configuracion "
                    + " where ran_id = " + ran_id.ToString();
            conn.QueryAlimento(queryFechaTracker, out dtFechaTracker);

            label3.Text = Convert.ToDateTime(dtFechaTracker.Rows[0][1]).ToString("dd/MM/yyyy HH:mm");

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
