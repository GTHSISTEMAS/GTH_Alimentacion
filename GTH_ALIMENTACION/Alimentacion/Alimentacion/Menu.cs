using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Data.Common;
using System.Configuration;

namespace Alimentacion
{
    public partial class Menu : Form
    {
        string ran_nombre, emp_nombre;
        int ran_id, emp_id;
        int rxemp;
        ConnSIO conn = new ConnSIO();
        bool bconfig;
        bool menu;
        int horaCorte;
        DateTime fInicio, fFin;
        string cadenaExe;
        string ranchos;
        int prorrateo;
        bool empresa;
        int tipo;

        public Menu()
        {
            InitializeComponent();
            conn.Iniciar("");
            getInfo();
        }
        private DateTime MaxDate(bool empresa)
        {
            DateTime fecha;
            DataTable dt;
            string query = "";

            if (empresa)
            {
                query = "SELECT MAX(T.Fecha) FROM( SELECT r.ran_id AS Rancho, CONVERT(date, MAX(r.rac_fecha)) AS Fecha FROM racion r "
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
        private void Menu_Load(object sender, EventArgs e)
        {
            DateTime fechaMax = MaxDate(empresa);
            DateTime fecha = DateTime.Now.Date > fechaMax.Date ? fechaMax : DateTime.Now;

            if (Boolean.Parse(ConfigurationManager.AppSettings["Reportes_Alim"]))
            {
                //if (!(DateTime.Now.Date > fechaMax.Date))
                //{
                DataTable dt;
                string query = "SELECT * FROM ALIMENTACION";
                conn.QueryMovGanado(query, out dt);

                Reporte_Diario re = new Reporte_Diario(ran_id, ran_nombre, emp_id, emp_nombre, tipo);
                foreach (DataRow row in dt.Rows)
                {
                    if (bool.Parse(row["diario"].ToString()))
                    {
                        re.Reporte(row["al_id"].ToString(), row["al_nombre"].ToString(), fecha, fecha, false, "Dia");
                    }
                    if (bool.Parse(row["acumulado"].ToString()))
                    {
                        re.Reporte(row["al_id"].ToString(), row["al_nombre"].ToString(), new DateTime(fecha.Year, fecha.Month, 1), fecha, false, "Acumulado");
                    }
                    if (bool.Parse(row["emp_dia"].ToString()))
                    {
                        re.Reporte(row["al_id"].ToString(), row["al_nombre"].ToString(), fecha, fecha, true, "Dia_EMPRESA");
                    }
                    if (bool.Parse(row["emp_acum"].ToString()))
                    {
                        re.Reporte(row["al_id"].ToString(), row["al_nombre"].ToString(), new DateTime(fecha.Year, fecha.Month, 1), fecha, true, "Acumulado_EMPRESA");
                    }
                    if (bool.Parse(row["diarioxemp"].ToString()))
                    {
                        Reporte_Empresa diario = new Reporte_Empresa(ran_id, emp_id, emp_nombre, true);
                        diario.ReporteDE(fecha);//DateTime.Now);
                                                //Reporte_Empresa diario = new Reporte_Empresa(ran_id, emp_id, emp_nombre, true);
                                                //diario.ReporteDE(DateTime.Now);
                    }
                }
                //}
                Close();
            }
            else
            {
                Console.WriteLine(panelCI.Visible.ToString());
                DateTime temp = MaxDate();
                temp = new DateTime(temp.Year, temp.Month, 1);
                panelReportes.Visible = false;
                panelCaptura.Visible = false;
                panelProgramas.Visible = false;
                panelContenedor.Visible = false;
                panelConfig.Visible = true;
                label1.Text = ran_nombre.ToUpper();
                //labelpwd.Visible = false;
                //tbPwd.Visible = false;
                button1.Cursor = Cursors.Hand;
                button2.Cursor = Cursors.Hand;
                button3.Cursor = Cursors.Hand;
                button4.Cursor = Cursors.Hand;
                button5.Cursor = Cursors.Hand;
                button6.Cursor = Cursors.Hand;
                button7.Cursor = Cursors.Hand;
                button8.Cursor = Cursors.Hand;
                button9.Cursor = Cursors.Hand;
                button11.Cursor = Cursors.Hand;
                button12.Cursor = Cursors.Hand;
                button13.Cursor = Cursors.Hand;
                button14.Cursor = Cursors.Hand;
                button15.Cursor = Cursors.Hand;
                button16.Cursor = Cursors.Hand;
                //button17.Cursor = Cursors.Hand;
                button18.Cursor = Cursors.Hand;
                button19.Cursor = Cursors.Hand;
                //button20.Cursor = Cursors.Hand;
                pictureBox1.Cursor = Cursors.Hand;
                checkBox1.Cursor = Cursors.Hand;
                btnCerrar.Cursor = Cursors.Hand;
                btnMaximizar.Cursor = Cursors.Hand;
                btnRestaurar.Cursor = Cursors.Hand;
                btnMinimizar.Cursor = Cursors.Hand;
                //panel39.Visible = false;
                bconfig = false;
                LlenarDGVS();
                menu = true;

                dtpFE.MinDate = MinDate();
                dtpFE.Cursor = Cursors.Hand;
                dtpIE.MaxDate = MaxDate();
                //dtpIE.MinDate = MinDate();
                dtpIE.Cursor = Cursors.Hand;
                dtpIE.Value = DateTime.Today.Day == 1 ? temp.AddMonths(-1) : temp;
                //dtpFE.Value = MaxDate();

                dtpFT.MinDate = MinDate();
                dtpFT.Cursor = Cursors.Hand;
                //dtpIT.MaxDate = MaxDate();
                //dtpIT.MinDate = MinDate();
                dtpIT.Cursor = Cursors.Hand;
                dtpIT.Value = temp;
                //dtpFT.Value = MaxDate();
                ranchos = checkBox1.Checked ? Establos() : ran_id.ToString();

                CargarGrafico();
                //panelReportes.Size = new Size(231, 125);
                if (rxemp == 0)
                {
                    checkBox1.Enabled = false;
                    checkBox1.Visible = false;
                }

                pictureBox2.Visible = false;
                cadenaExe = ConfigurationManager.AppSettings["ConsumoExe"];
                panelGE.Size = new Size(Convert.ToInt32(tabPage1.Width) / 2, panelGE.Height);
                //prorrateo = Convert.ToInt32(ConfigurationManager.AppSettings["prorrateo"]);

                //if (prorrateo == 0)
                //{
                //    panelBtnProrrateo.Visible = false;
                //    panelBtnExportar.Visible = false;
                //    panelProgramas.Size = new Size(231, 202);
                //}
                Console.WriteLine(panelCI.Visible.ToString());
                Console.WriteLine(panelRDE.Visible.ToString());
                CargarVistas();
            }
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            btnRestaurar.Visible = false;
            btnMaximizar.Visible = true;
        }

        private void btnMaximizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            btnMaximizar.Visible = false;
            btnRestaurar.Visible = true;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {            
            menu = true;
            pictureBox2.Visible = false;
            bconfig = false;
            this.Size = new Size(958, 500);
            panelConfig.Visible = true;
            panelContenedor.Visible = false;
            panel42.Visible = true;
            LlenarDGVS();
            CargarVistas();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!menu)
                CargarVistas();

            panelCaptura.Visible = false;
            panelProgramas.Visible = false;
            panelReportes.Visible = !panelReportes.Visible;
        }

        private void getInfo()
        {
            try
            {
                DataTable dt; DataTable dt1;
                string query = "SELECT rancholocal FROM RANCHOLOCAL";
                conn.QueryMovGanado(query, out dt);
                ran_id = Convert.ToInt32(dt.Rows[0][0]);

                query = "SELECT c.ran_desc, c.emp_id, c.emp_desc, c.ran_empresa, c.ran_corte, cr.tic_id "
                        + " FROM configuracion c LEFT JOIN configuracion_rancho cr ON c.ran_id = cr.ran_id "
                        + " where c.ran_id = " + ran_id.ToString(); 
                conn.QuerySIO(query, out dt1);
                ran_nombre = dt1.Rows[0][0].ToString();
                emp_id = Convert.ToInt32(dt1.Rows[0][1]);
                emp_nombre = dt1.Rows[0][2].ToString();
                rxemp = Convert.ToInt32(dt1.Rows[0][3]);
                horaCorte = Convert.ToInt32(dt1.Rows[0][4]);
                tipo = Convert.ToInt32(dt1.Rows[0][5]);
                empresa = tipo == 2 || tipo == 3 ? true : false;
            }
            catch
            {
                ran_id = 0;
                ran_nombre = "";
                emp_id = 0;
                emp_nombre = "";
                rxemp = 0;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(!menu)
                CargarVistas();

            panelReportes.Visible = false;
            panelProgramas.Visible = false;
            panelCaptura.Visible = !panelCaptura.Visible;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!menu)
                CargarVistas();

            panelCaptura.Visible = false;
            panelReportes.Visible = false;
            panelProgramas.Visible = !panelProgramas.Visible;
        }

        private Form activeForm = null;
        private void openChildFormPanel(Form childForm)
        {
            if (activeForm != null)
                activeForm.Close();
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panelContenedor.Controls.Add(childForm);
            if (panelContenedor.Width < childForm.Width || panelContenedor.Height < childForm.Height)
                panelContenedor.Size = childForm.Size;

            panelContenedor.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        public void log(string pwd)
        {
            if (pwd == "hc")
            {
                openChildFormPanel(new Configuraciones(ran_id, ran_nombre));
                panelContenedor.Visible = true;
                panelConfig.Visible = false;
                this.Size = new Size(910, 630);
            }
            else
            {
                DialogResult result = MessageBox.Show("Contraseña incorrecta", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            menu = false;
            bconfig = false;
            pictureBox2.Visible = true;
            bool cbEmpresa = checkBox1.Checked;
            //Reporte Diario
            openChildFormPanel(new Reporte_Diario(ran_id, ran_nombre, emp_id, emp_nombre, tipo));
            panelContenedor.Visible = true;
            panelConfig.Visible = false;
            this.Size = new Size(754, 544);
            Cursor = Cursors.Default;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            menu = false;
            pictureBox2.Visible = true;
            bconfig = false;
            //Reporte por Corral
            openChildFormPanel(new Reporte_Corral(ran_id, ran_nombre, emp_id, emp_nombre));
            panelContenedor.Visible = true;
            panelConfig.Visible = false;
            this.Size = new Size(804, 544);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            menu = false;
            pictureBox2.Visible = true;
            bconfig = false;
            //Consumo por Ingrediente
            openChildFormPanel(new Consumo_Por_Ingrediente(ran_id, emp_id, ran_nombre, emp_nombre, tipo));
            panelContenedor.Visible = true;
            panelConfig.Visible = false;
            this.Size = new Size(804, 544);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            menu = false;
            pictureBox2.Visible = true;
            bconfig = false;
            if (rxemp == 1)
            {
                if (checkBox1.Checked)
                {
                    openChildFormPanel(new Reporte_Empresa(ran_id, emp_id, emp_nombre));
                    panelContenedor.Visible = true;
                    panelConfig.Visible = false;
                    this.Size = new Size(754, 544);
                }
                else
                {
                    MessageBox.Show("No esta habiliada la opción por Empresa", "Infrmacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                DialogResult result = MessageBox.Show("Programa no disponible", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            menu = false;
            pictureBox2.Visible = true;
            bconfig = false;
            //Captura Diaria
            bool empresa = checkBox1.Checked;
            openChildFormPanel(new Racion_Tracker_Fuera(ran_id, emp_id, empresa, tipo));
            panelContenedor.Visible = true;
            panelConfig.Visible = false;
            this.Size = new Size(1180, 620);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //Relacion de Corrales
            Process.Start("C:\\Movganado\\Procesos\\RelaCorrales.exe");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //Nivel de Cribas
            menu = false;
            pictureBox2.Visible = true;
            bconfig = false;
            openChildFormPanel(new Cribas(ran_id, ran_nombre));
            panelContenedor.Visible = true;
            panelConfig.Visible = false;
            this.Size = new Size(754, 544);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            //Teoricos Precios Racion
            menu = false;
            pictureBox2.Visible = true;
            bconfig = false;
            openChildFormPanel(new PreciosTeoricos(ran_id, ran_nombre));
            panelContenedor.Visible = true;
            panelConfig.Visible = false;
            this.Size = new Size(804, 544);
        }
   
        private void button13_Click(object sender, EventArgs e)
        {
            menu = true;
            panelContenedor.Visible = false;
            panelConfig.Visible = true;
            pictureBox2.Visible = false;
            menu = false;
            pictureBox2.Visible = true;
            bconfig = false;

            //Prorrateo
            openChildFormPanel(new Prorrateo(emp_id, emp_nombre, ran_id, ran_nombre));
            FormCollection fc = Application.OpenForms;
            panelContenedor.Visible = true;
            bool ProrrateroOpen = false;

            foreach (Form frm in fc)
            {
                //iterate through
                if (frm.Name == "Prorrateo")
                {
                    ProrrateroOpen = true;
                } 
            }
            
            if(ProrrateroOpen)
            {
                panelConfig.Visible = false;
                //this.Size = new Size(1220, 800);
                this.Size = new Size(1300, 750);
                //Console.WriteLine(panelConfig.Size.ToString());
                this.MaximumSize = SystemInformation.PrimaryMonitorMaximizedWindowSize;
                this.WindowState = FormWindowState.Maximized;
                PanelSize();
                btnRestaurar.Visible = true;
                btnMaximizar.Visible = false;
            } else
            {
                menu = true;
                panelContenedor.Visible = false;
                panelConfig.Visible = true;
                pictureBox2.Visible = false;
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            menu = false;
            pictureBox2.Visible = true;
            bconfig = false;
            //Exportar / Enviar Prorrateo
            openChildFormPanel(new Exportar_Prorrateo(ran_id, ran_nombre, emp_id, emp_nombre, false, false));
            panelContenedor.Visible = true;
            panelConfig.Visible = false;
            this.Size = new Size(804, 544);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            //Agregar Carros
            Cursor = Cursors.WaitCursor;
            Process p = Process.Start("C:\\Movganado\\Consumos\\Procesos\\carros\\Debug\\Carros.exe");
            p.WaitForExit();
            Cursor = Cursors.Default;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            //Mapeo
            Cursor = Cursors.WaitCursor;
            Process p = Process.Start("C:\\Movganado\\Consumos\\Procesos\\Mapeo\\Debug\\mapeo.exe");
            p.WaitForExit();
            Cursor = Cursors.Default;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Process p = Process.Start("C:\\Movganado\\Procesos\\t1.bat");
            p.WaitForExit();
            Cursor = Cursors.Default;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            //Inventario Tracker
            Cursor = Cursors.WaitCursor;
            Process p = Process.Start("C:\\movganado\\procesos\\vacasxcorral.exe");
            p.WaitForExit();
            MessageBox.Show("INVENTARIO GENERADO", "INFO");
            Cursor = Cursors.Default;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Corte corte = new Corte();
                if (corte.ShowDialog() == DialogResult.OK)
                {
                    Cursor = Cursors.WaitCursor;
                    int days = corte.Dias;
                    DateTime inicio = corte.Inicio;
                    DateTime fin = corte.Fin;
                    if (days == 31)
                        if (DateTime.Today.Day >= 1 && DateTime.Today.Day < 5)
                            days = days + DateTime.Today.Day;

                    //Console.WriteLine("valor al regresar " + days.ToString());
                    //Console.WriteLine("inicio: " + inicio.ToString("dd/MM/yyyy"));
                    //Console.WriteLine("fin: " + fin.ToString("dd/MM/yyyy"));
                    string argumentos = days.ToString() + " " + inicio.ToString("yyyy-MM-dd") + " " + fin.ToString("yyyy-MM-dd");
                    Process p = new Process();
                    p.StartInfo.FileName = cadenaExe;
                    p.StartInfo.Arguments = argumentos;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                    p.Start();
                    p.BeginOutputReadLine();
                    p.WaitForExit();



                    //Process p = Process.Start(cadenaExe, argumentos);

                    //p.WaitForExit();
                    //Process.Start("C:\\Users\\lgomez\\Documents\\Proyectos\\GTH_Consumos\\Consumos\\Consumos\\bin\\Debug\\Consumos.exe", days.ToString());
                    LlenarDGVS();
                    Cursor = Cursors.Default;
                    MessageBox.Show("Corte Finalizado", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch { }
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Console.WriteLine(outLine.Data);
        }

        private void panelBarraTitulo_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void btnMaximizar_Click_1(object sender, EventArgs e)
        {
            Console.WriteLine(panelConfig.Size.ToString());
            this.MaximumSize = SystemInformation.PrimaryMonitorMaximizedWindowSize;
            this.WindowState = FormWindowState.Maximized;
            PanelSize();
            btnRestaurar.Visible = true;
            btnMaximizar.Visible = false;
        }

        private void btnRestaurar_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            PanelSize();
            btnMaximizar.Visible = true;
            btnRestaurar.Visible = false;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            menu = false;
            pictureBox2.Visible = true;
            if (bconfig == false)
            {
                Form2 f2 = new Form2();
                if (f2.ShowDialog() == DialogResult.OK)
                {
                    
                        openChildFormPanel(new Configuraciones(ran_id, ran_nombre));
                        panelConfig.Visible = false;
                        panelContenedor.Visible = true;
                        this.Size = new Size(910, 610);
                        bconfig = true;
                                        
                }                
            }

        }


        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]

        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        public void pictureBox2_Click(object sender, EventArgs e)
        {
            menu = true;
            panelContenedor.Visible = false;
            panelConfig.Visible = true;
            pictureBox2.Visible = false;
            this.Size = new Size(958, 500);
        }

        private DataTable Fechas()
        {
            DataTable dt;
            string query = "SELECT Tracker.Fecha AS FechaT, Sie.Fecha AS FechaS, Bascula.Fecha AS FechaB "
                + " FROM( "
                + " select ran_id AS Rancho, CONVERT(DATE, MAX(rac_fecha)) AS Fecha "
                + " FROM racion r "
                + " where ran_id = " + ran_id.ToString()
                + " GROUP BY ran_id ) AS Tracker "
                + " LEFT JOIN( "
                + " SELECT alm.ran_id AS Rancho, MAX(art.art_fecha) AS Fecha "
                + " FROM articulo art "
                + " LEFT JOIN [DBSIE].[dbo].almacen alm ON alm.alm_id = art.alm_id "
                + " WHERE alm.ran_id = " + ran_id.ToString()
                + " GROUP BY alm.ran_id "
                + " )Sie ON Sie.Rancho = Tracker.Rancho "
                + " LEFT JOIN( "
                + " SELECT bal.ran_id AS Rancho, CONVERT(Date, MAX(bol_fecha)) AS Fecha "
                + " FROM boleto bol "
                + " LEFT JOIN[DBSIE].[dbo].bascula bal ON bol.bal_clave = bal.bal_clave "
                + " WHERE bal.ran_id = " + ran_id.ToString()
                + " GROUP BY bal.ran_id "
                + " ) Bascula ON Bascula.Rancho = Tracker.Rancho ";
            conn.QueryAlimento(query, out dt);
            return dt;
        }

        private void LlenarDGVS()
        {
            DataTable dt1, dt2;
            string query;

            query = "SELECT 'Informacion del Tracker' AS Descripcion, FORMAT(MAX(rac_fecha), 'dd/MM/yyyy HH:mm', 'es-mx' ) AS Fecha "
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
            conn.QueryAlimento(query, out dt1);

            query = " SELECT alm_id AS Almacen, "
                + " CASE alm_tipo when 1 THEN 'ESTABLO' when 2 THEN 'ALIMENTO' "
                + " when 3 THEN 'FORRAJE'  when 4 THEN 'AGRICOLA' WHEN 5 THEN 'ANIMALES' END AS Tipo "
                + " FROM[DBSIE].[dbo].almacen alm  WHERE alm.ran_id = " + ran_id.ToString();
            conn.QuerySIE(query, out dt2);

            dgvActualizaciones.DataSource = dt1;
            dgvAlmacenes.DataSource = dt2;
        }
 
        private void BuscarTracker()
        {
            try
            {
                string startFloder = @"C:\Users\Public\Documents\TMR Tracker";
                string fileName = "";
                string directory = "";

                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(startFloder);
                IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
                IEnumerable<System.IO.FileInfo> fileQuery =
                    from file in fileList
                    where file.Extension == ".fdb" || file.Extension == ".FDB"
                    orderby file.Name
                    select file;

                var newestFile =
                    (from file in fileQuery
                     orderby file.CreationTime
                     select new { file.FullName, file.CreationTime }).Last();

                foreach (System.IO.FileInfo fi in fileQuery)
                {
                    if (fi.FullName == newestFile.FullName)
                    {
                        fileName = fi.Name;
                        directory = fi.DirectoryName;
                        break;
                    }
                }
                CopiarTracker(directory, fileName);
            }
            catch (IOException ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void CopiarTracker(string sourcePath, string fileName)
        {
            try
            {
                string ranNum = ran_id > 9 ? ran_id.ToString() : "0" + ran_id.ToString();
                string newFileName = "Tracker" + ranNum + ".FDB";
                string destino = @"C:\Movganado";


                string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                string destFile = System.IO.Path.Combine(destino, newFileName);

                System.IO.File.Copy(sourceFile, destFile, true);
            }
            catch (IOException ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error); }

        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                if (menu)
                {
                    DialogResult result = MessageBox.Show("¿Estás Seguro que deseas Salir?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        this.Close();
                        return true;
                    }
                }
                else
                {
                    menu = true;
                    this.WindowState = FormWindowState.Normal;
                    PanelSize();
                    btnMaximizar.Visible = true;
                    btnRestaurar.Visible = false;
                    pictureBox2.Visible = false;
                    bconfig = false;
                    this.Size = new Size(958, 500);
                    panelConfig.Visible = true;
                    panelContenedor.Visible = false;
                    panel42.Visible = true;
                    LlenarDGVS();
                    this.WindowState = FormWindowState.Normal;
                    PanelSize();
                    btnMaximizar.Visible = true;
                    btnRestaurar.Visible = false;
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        public static int ConvertToJulian(DateTime Date)
        {
            TimeSpan ts = (Date - Convert.ToDateTime("01/01/1900"));
            int julianday = ts.Days + 2;
            return julianday;
        }

        public void CargarGrafico()
        {
            int horas = horaCorte - 24;
            horas = horas == -24 ? 0 : horas;
            DateTime fecI, fecF;
            
            fecI = dtpIE.Value.Date; fecI = fecI.AddHours(horas);
            fecF = dtpFE.Value.Date; fecF = fecF.AddHours(horaCorte);

            ArrayList etapa = new ArrayList();
            ArrayList total = new ArrayList();
            ArrayList ingrediente = new ArrayList();
            ArrayList kilosT = new ArrayList();
            DataTable dt, dt5;

            //Premezclas(fecI, fecF);
            //string pmz = PremezclasCad(fecI, fecF);
            dt = PorcentajeEtapa(fecI,fecF);
            //dt5 = Top5Ingredientes(fecI, fecF, pmz);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                etapa.Add(dt.Rows[i][0].ToString());
                total.Add(Convert.ToDouble(dt.Rows[i][1]));
            }
            chart1.Series[0].Points.DataBindXY(etapa, total);

            //for(int i = 0; i < dt5.Rows.Count; i++)
            //{
            //    ingrediente.Add(dt5.Rows[i][0].ToString());
            //    kilosT.Add(Convert.ToDouble(dt5.Rows[i][1]));
            //}
            chart2.Series[0].Points.DataBindXY(ingrediente, kilosT);
            
            
            //dgvT5.DataSource = dt5;
            //FormatoGridDash(dgvT5);
            dgvTE.DataSource = TablaEtapas(fecI, fecF);
            FormatoGridDash(dgvTE);
        }     

        private void FormatoGridDash(DataGridView dgv)
        {
            dgv.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns[1].DefaultCellStyle.Format = "###,##0";
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(221, 235, 247);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 156, 241);
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            int h = panelReportes.Size.Height;
            if (checkBox1.Checked)
            {                
                panelRDE.Visible = true;
                panelReportes.Size = new Size(231, h+ 40);
                ranchos = Establos();
            }
            else
            {
                panelRDE.Visible = false;
                panelReportes.Size = new Size(231, h- 40);
                ranchos = ran_id.ToString();
            }            
        }

        private void CargarPremezcla(string premezcla, DateTime inicio, DateTime fin)
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
                            conn.InsertMasivAlimento("porcentaje_Premezcla", valores);
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
                            conn.InsertMasivAlimento("porcentaje_Premezcla", valores);
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

        private DataTable PorcentajeEtapa(DateTime inicio, DateTime fin)
        {
            DataTable dt;
            string query = "SELECT T4.Etapa, ROUND(T4.Porcentaje,2)  "
                        + " FROM( "
                        + " SELECT T2.Etapa, (T2.TOTAL / T3.TOTAL) * 100 AS Porcentaje "
                        + " FROM( "
                        + " SELECT T1.Rancho, T1.Etapa, SUM(T1.TOTAl)  AS TOTAL "
                        + " FROM( "
                        + " SELECT T.Rancho, "
                        + " CASE "
                        + " WHEN T.Id IN(10, 11, 12, 13) THEN 'PRODUCCION' "
                        + " WHEN T.Id = 21 THEN 'SECAS' "
                        + " WHEN T.Id = 22 THEN 'RETO' "
                        + " WHEN T.Id = 31 THEN 'JAULAS' "
                        + " WHEN T.Id = 32 THEN 'DESTETADAS 1' "
                        + " WHEN T.Id = 33 THEN 'DESTETADAS 2' "
                        + " WHEN T.Id = 34 THEN 'VAQUILLAS PREÑADAS' "
                        + " END AS Etapa, T.Total "
                        + " FROM( "
                                + " select r.ran_id AS Rancho, r.etp_id AS Id, e.etp_descripcion AS Etapa, SUM(r.rac_mh) AS Total "
                                + " FROM racion r "
                                + " LEFT JOIN etapa e ON e.etp_id = r.etp_id "
                                + " where r.rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND r.ran_id IN( " + ran_id.ToString() + ") "
                                + " AND r.etp_id not in (0, 90) AND e.etp_descripcion is not null "
                                + " GROUP BY r.ran_id, r.etp_id, e.etp_descripcion "
                        + " ) T) T1 "
                        + " GROUP BY T1.Rancho, T1.Etapa) T2 "
                        + " LEFT JOIN( "
                        + " select ran_id AS Rancho, SUM(rac_mh) AS TOTAL "
                        + " from racion "
                        + " where ran_id IN (" + ran_id + ") AND rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' "
                        + " AND etp_id not in (0, 90) "
                        + " GROUP BY ran_id "
                        + " ) T3 ON T2.Rancho = T3.Rancho) T4 "
                        + " WHERE T4.Porcentaje > 0";
            conn.QueryAlimento(query, out dt);
            return dt;
        }

        private DataTable TablaEtapas(DateTime inicio, DateTime fin)
        {
            DataTable dt;
            string query = "SELECT T1.Etapa, SUM(T1.TOTAl)  AS TOTAL "
                        + " FROM( "
                        + " SELECT CASE WHEN T.Id IN(10, 11, 12, 13) THEN 'PRODUCCION' "
                        + " WHEN T.Id = 21 THEN 'SECAS' WHEN T.Id = 22 THEN 'RETO' WHEN T.Id = 31 THEN 'JAULAS'  "
                        + " WHEN T.Id = 32 THEN 'DESTETADAS 1' WHEN T.Id = 33 THEN 'DESTETADAS 2' "
                        + " WHEN T.Id = 34 THEN 'VAQUILLAS PREÑADAS' END AS Etapa, T.Total "
                        + " FROM( "
                        + " select r.etp_id AS Id, e.etp_descripcion AS Etapa, SUM(r.rac_mh) AS Total "
                        + " FROM racion r "
                        + " LEFT JOIN etapa e ON e.etp_id = r.etp_id "
                        + " where r.rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") +"' AND r.ran_id IN(" + ranchos + ") "
                        + " AND r.etp_id not in (0, 90) AND e.etp_descripcion is not null "
                        + " GROUP BY  r.etp_id, e.etp_descripcion ) T ) T1 "
                        + " GROUP BY T1.Etapa "
                        + " UNION "
                        + " select 'TOTAL' AS Etapa, SUM(rac_mh) AS TOTAL "
                        + " from racion "
                        + " where ran_id IN(" + ranchos + ") AND rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") +"' "
                        + " AND etp_id not in (0, 90) "
                        + " ORDER BY 2";
            conn.QueryAlimento(query, out dt);
            return dt;
        }

        private DataTable Top5Ingredientes(DateTime inicio, DateTime fin, string premezcla)
        {
            DataTable dt;
            string query = "SELECT TOP(5) p.prod_nombre AS INGREDIENTE, SUM(R1.TOTAL) AS TOTAL "
                        + " FROM( "
                        + " SELECT ing_clave AS Clave, ing_descripcion AS INGREDIENTE, SUM(rac_mh) AS TOTAL "
                        + " FROM racion "
                        + " where rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND  '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranchos + ") "
                        + " AND ing_descripcion not in (" + premezcla + ") "
                        + " GROUP BY ing_clave, ing_descripcion "
                        + " UNION "
                        + " SELECT T2.Clave, T2.Ingrediente, T1.TOTAL * T2.Porcentaje AS TOTAL "
                        + " FROM( "
                        + " SELECT ing_descripcion AS Pmz, SUM(rac_mh) AS TOTAL "
                        + " FROM racion "
                        + " where rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND  '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranchos + ") "
                        + " AND ing_descripcion in (" + premezcla + ") "
                        + " GROUP BY ing_descripcion "
                        + " ) T1 "
                        + " LEFT JOIN( "
                        + " SELECT pmez_descripcion AS Pmz, ing_clave AS Clave, ing_descripcion AS Ingrediente, pmez_porcentaje As Porcentaje "
                        + " FROM porcentaje_Premezcla "
                        + " )T2 ON T1.Pmz = T2.Pmz "
                        + " ) R1 "
                        + " LEFT JOIN producto p ON R1.Clave = p.prod_clave "
                        + " WHERE p.prod_nombre is not null "
                        + " GROUP BY p.prod_nombre "
                        + " ORDER BY 2 desc ";
            conn.QueryAlimento(query, out dt);
            return dt;
        }

        private void botGenT5_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            int horas = horaCorte - 24;
            horas = horas == -24 ? 0 : horas;
            DateTime fecI, fecF;
            fecI = dtpIT.Value; fecI = fecI.AddHours(horas);
            fecF = dtpFT.Value; fecF = fecF.AddHours(horaCorte);
            Premezclas(fecI, fecF);
            string pmz = PremezclasCad(fecI, fecF);
            DataTable dt = Top5Ingredientes(fecI, fecF, pmz);

            ArrayList etiqueta = new ArrayList();
            ArrayList valor = new ArrayList();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                etiqueta.Add(dt.Rows[i][0].ToString());
                valor.Add(Convert.ToDouble(dt.Rows[i][1]));
            }

            chart2.Series[0].Points.DataBindXY(etiqueta, valor);
            dgvT5.DataSource = dt;
            FormatoGridDash(dgvTE);
            Cursor = Cursors.Default;
        }

        private DateTime MaxDate()
        {
            DateTime fecha, temp;
            DataTable dt;
            string query = "SELECT CONVERT(DATE, MAX(rac_fecha)) FROM racion WHERE ran_id IN(" + ran_id.ToString() +")";
            conn.QueryAlimento(query, out dt);
            fecha = Convert.ToDateTime(dt.Rows[0][0]);            
            return fecha;
        }
        private DateTime MinDate()
        {
            DateTime fecha, temp;
            DataTable dt;
            string query = "SELECT CONVERT(DATE, MIN(rac_fecha)) FROM racion WHERE ran_id IN(" + ran_id.ToString() + ")";
            conn.QueryAlimento(query, out dt);
            temp = Convert.ToDateTime(dt.Rows[0][0]);
            fecha = new DateTime(temp.Year, temp.Month, temp.Day, horaCorte, 0, 0);
            return fecha;
        }

        private void botGenEtapas_Click(object sender, EventArgs e)
        {
            int horas = horaCorte - 24;
            horas = horas == -24 ? 0 : horas;
            DateTime fecI, fecF;
            fecI = dtpIE.Value; fecI = fecI.AddHours(horas);
            fecF = dtpFE.Value; fecF = fecF.AddHours(horaCorte);

            DataTable dt = PorcentajeEtapa(fecI, fecF);
            ArrayList etiqueta = new ArrayList();
            ArrayList valor = new ArrayList();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                etiqueta.Add(dt.Rows[i][0].ToString());
                valor.Add(Convert.ToDouble(dt.Rows[i][1]));
            }
            chart1.Series[0].Points.DataBindXY(etiqueta, valor);
            dgvTE.DataSource = TablaEtapas(fecI, fecF);
            FormatoGridDash(dgvTE);
        }

        private string Establos()
        {
            string establos = "";
            DataTable dt;
            string query = "SELECT ran_id FROM [DBSIO].[dbo].configuracion WHERE emp_id = " +  emp_id;
            conn.QueryAlimento(query, out dt);

            for(int i = 0; i <dt.Rows.Count; i++)
            {
                establos += dt.Rows[i][0].ToString() + ",";
            }

            establos = establos.Length > 0 ? establos.Substring(0, establos.Length - 1) : "";
            return establos;
        }        

        private void Premezclas(DateTime inicio, DateTime fin)
        {
            DataTable dt, dt1;
            string query = "select DISTINCT ing_descripcion FROM racion "
                + " WHERE rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN(" + ranchos + ") "
                + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F','W') AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02')";
            conn.QueryAlimento(query, out dt);

            conn.DeleteAlimento("porcentaje_Premezcla", "");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                query = "SELECT TOP(5) * FROM premezcla where pmez_racion like '" + dt.Rows[i][0].ToString() + "' AND pmez_fecha <= '" + fin.ToString("yyyy-MM-dd HH:mm") + "'";
                conn.QueryAlimento(query, out dt1);

                if (dt1.Rows.Count == 0)
                    continue;

                CargarPremezcla(dt.Rows[i][0].ToString(), inicio, fin);
            }
        }

        private String PremezclasCad(DateTime inicio, DateTime fin)
        {
            string premezclas = "";
            DataTable dt;
            string query = "select DISTINCT ing_descripcion FROM racion "
                + " WHERE rac_fecha BETWEEN '" + inicio.ToString("yyyy-MM-dd HH:mm") + "' AND '" + fin.ToString("yyyy-MM-dd HH:mm") + "' AND ran_id IN( " + ranchos + ") "
                + " AND SUBSTRING(ing_descripcion,1,1) NOT IN('A','F','W') AND SUBSTRING(ing_descripcion,3,2) IN('00', '01', '02')";
            conn.QueryAlimento(query, out dt);

            
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                premezclas += "'" + dt.Rows[i][0].ToString() + "',";
            }


            return premezclas.Length > 0 ? premezclas.Substring(0, premezclas.Length - 1) : "''";
        }

        private void PanelSize()
        {
            int h, w;
            if (panelConfig.Visible)
            {
                ArrayList height = new ArrayList();
                ArrayList width = new ArrayList();

                height.Add(Convert.ToInt32(tabPage1.Height));
                height.Add(Convert.ToInt32(tabPage2.Height));
                //height.Add(Convert.ToInt32(tabPage3.Height));
                height.Add(Convert.ToInt32(tabPage4.Height));
                //height.Add(Convert.ToInt32(tabPage5.Height));

                width.Add(Convert.ToInt32(tabPage1.Width));
                width.Add(Convert.ToInt32(tabPage2.Width));
                //width.Add(Convert.ToInt32(tabPage3.Width));
                width.Add(Convert.ToInt32(tabPage4.Width));
                //width.Add(Convert.ToInt32(tabPage5.Width));

                int index = btnMaximizar.Visible ? height.Count - 1:0;
                height.Sort();
                width.Sort();

                h = Convert.ToInt32(height[index]);
                w = Convert.ToInt32(width[index]);

                tabPage1.Size = new Size(w, h);
                tabPage2.Size = new Size(w, h);
                //tabPage3.Size = new Size(w, h);
                tabPage4.Size = new Size(w, h);
               // tabPage5.Size = new Size(w, h);

                panelGE.Size = new Size(tabPage4.Width / 2, tabPage4.Height);
                //panelT5G.Size = new Size(tabPage5.Width / 2, tabPage5.Height);
            }  
            else if(panelContenedor.Visible)
            {
                panelConfig.Size = new Size(panelContenedor.Width, panelContenedor.Height);
                h = panelConfig.Height;
                w = panelConfig.Width;
                tabPage1.Size = new Size(w, h);
                tabPage2.Size = new Size(w, h);
                //tabPage3.Size = new Size(w, h);
                tabPage4.Size = new Size(w, h);
                //tabPage5.Size = new Size(w, h);

                panelGE.Size = new Size(tabPage4.Width / 2, tabPage4.Height);
                //panelT5G.Size = new Size(tabPage5.Width / 2, tabPage5.Height);
            }           
        }

        private void button19_Click(object sender, EventArgs e)
        {
            //Agrupar Inventarios
            Cursor = Cursors.WaitCursor;
             Process p = Process.Start("C:\\movganado\\consumos\\procesos\\AgruparInventario\\Debug\\AgruparInv.exe");
            p.WaitForExit();
            Cursor = Cursors.Default;   
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            //Venta Racion
            Process.Start("C:\\MOVGANADO\\Consumos\\Procesos\\VentaRacion\\Programa\\Debug\\VentaRacion.exe");
        }

        private void btnProyeccion_Click(object sender, EventArgs e)
        {
            menu = false;
            pictureBox2.Visible = true;
            bconfig = false;
            //Consumo por Ingrediente
            openChildFormPanel(new ProyeccionINg(ran_id, emp_id, ran_nombre, emp_nombre, tipo));
            panelContenedor.Visible = true;
            panelConfig.Visible = false;
            this.Size = new Size(804, 544);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Process p = Process.Start("C:\\Movganado\\Procesos\\t1.bat");
            p.WaitForExit();
            Cursor = Cursors.Default;
        }

        private void panelActualizaciones_Paint(object sender, PaintEventArgs e)
        {

        }
        public void ActualizarFecha()
        {
            DataTable dtFechaTracker;
            string queryFechaTracker = "SELECT 'Informacion del Tracker' AS Descripcion, FORMAT(MAX(rac_fecha), 'dd/MM/yyyy HH:mm', 'es-mx' ) AS Fecha "
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
                    + " WHERE ran_id = " + ran_id.ToString();

            conn.QueryAlimento(queryFechaTracker, out dtFechaTracker);

            label3.Text = Convert.ToDateTime(dtFechaTracker.Rows[0][1]).ToString("dd/MM/yyyy HH:mm");

        }

        ActualizarInformacion actInfo;
        private void button21_Click_1(object sender, EventArgs e)
        {
            ActualizarInformacion f2 = new ActualizarInformacion();
            if (f2.ShowDialog() == DialogResult.OK)
            {
               
               
                if(f2.Seleccionador == 1)
                {
                    Cursor = Cursors.WaitCursor;
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
                    ActualizarFecha();
                    Cursor = Cursors.Default;
                } else
                if (f2.Seleccionador == 2)
                {
                    Cursor = Cursors.WaitCursor;
                    //Process p = Process.Start("C:\\Movganado\\Procesos\\t1.bat");
                    Process p = Process.Start("C:\\Movganado\\Procesos\\Copia.lnk");
                    p.WaitForExit();
                    Cursor = Cursors.Default;
                }


            }

        }

        public static void PrintIndexAndValues(IEnumerable myList)
        {
            int i = 0;
            foreach (Object obj in myList)
                Console.WriteLine("\t[{0}]:\t{1}", i++, obj);
            Console.WriteLine();
        }


        private void CargarVistas()
        {
            DataTable dt;
            string query = "SELECT vis_id FROM vista WHERE vis_id NOT IN( select v.vis_id FROM rancho_vista rv LEFT JOIN vista v ON rv.vis_id = v.vis_id "
                    + " LEFT JOIN menu m ON m.men_id = v.men_id WHERE rv.ran_id = " + ran_id + ")";
            conn.QuerySIO(query, out dt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                switch (Convert.ToInt32(dt.Rows[i][0]))
                {
                    case 1: panelBtnRDP.Visible = false; break;
                    case 2: panelBtnRC.Visible = false; break;
                    case 3: panelCI.Visible = false; break;
                    case 4: checkBox1.Visible = false; break;
                    case 5: panelBtnCD.Visible = false; break;
                    case 6: panelNC.Visible = false; break;
                    case 7: panelTPR.Visible = false; break;
                    case 8: panelBtnProrrateo.Visible = false; break;
                    case 9: panelBtnExportar.Visible = false; break;
                    case 10: panelBtnCarros.Visible = false; break;
                    case 11: panelBtnMapeo.Visible = false; break;
                    //case 12: panelBtnExtraer.Visible = false; break;
                    case 13: panelBtnInventario.Visible = false; break;
                    case 14: panel1BtnAgrupar.Visible = false; break;
                    case 15: panelBtnCorteManual.Visible = false; break;
                    case 16: panelBtnConfiguracion.Visible = false; break;
                }
            }            

            if(tipo == 1)
            {
                checkBox1.Visible = false;
            }
           
            query = "SELECT m.men_id, COUNT(v.vis_id) FROM rancho_vista rv LEFT JOIN vista v ON v.vis_id = rv.vis_id LEFT JOIN menu m ON m.men_id = v.men_id "
                    + " WHERE rv.ran_id = " + ran_id + " GROUP BY m.men_id";
            conn.QuerySIO(query, out dt);

            int cont;
            for(int i = 0; i < dt.Rows.Count; i++)
            {
                Int32.TryParse(dt.Rows[i][1].ToString(), out cont);
                switch (Convert.ToInt32(dt.Rows[i][0]))
                {
                    case 1:
                        if (cont > 0)
                        {
                            if (checkBox1.Visible && checkBox1.Checked == false || (checkBox1.Visible == false && tipo != 1))
                                cont = cont - 1;
                            panelReportes.Size = new Size(231, (55 * cont));
                        }
                    break;
                    case 2:
                        if (cont > 0)
                            panelCaptura.Size = new Size(231, (40 * cont));
                        break;
                    case 3:
                        if (cont > 0)
                            {
                                cont = cont - 1;
                                panelProgramas.Size = new Size(231, (40 * cont));
                            }
                        break;
                    default: break;
                }
            }

            query = "SELECT men_id FROM menu WHERE men_id not in( SELECT DISTINCT m.men_id FROM rancho_vista rv LEFT JOIN vista v ON v.vis_id = rv.vis_id "
                    + " LEFT JOIN menu m ON m.men_id = v.men_id  WHERE rv.ran_id = " + ran_id.ToString() + " )";
            conn.QuerySIO(query, out dt);

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                switch (Convert.ToInt32(dt.Rows[i][0]))
                {
                    case 1: panelBtnReportes.Visible = false; break;
                    case 2: panelBtnCaptura.Visible = false; break;
                    case 3: panelBtnProgramas.Visible = false; break;
                    case 4: panelBtnCorteManual.Visible = false; break;
                    case 5: panelBtnConfiguracion.Visible = false; break;
                }
            }

            if (tipo == 3)
            {
                panelBtnProrrateo.Visible = false;
                panelBtnExportar.Visible = false;
                panelProgramas.Size = new Size(231, panelProgramas.Height - 40);
            }
        }
        
        public void CerrarPanelMenus()
        {
            panelCaptura.Visible = false;
            panelProgramas.Visible = false;
            panelReportes.Visible = false;
        }
    }
}