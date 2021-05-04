using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alimentacion
{
    public partial class Form1 : Form
    {
        string ran_nombre, emp_nombre;
        int ran_id,  emp_id;
        
        bool cbEmpresa = false;
        ConnSIO conn = new ConnSIO();
        

        public Form1()
        {
            InitializeComponent();
            conn.Iniciar("DBSIO");
            getInfo();
            panelExit.Visible = false;
            panelChild.Visible = false;
            panelConfig.Visible = true;
        }

        private void button8_Click(object sender, EventArgs e)
        {

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
            panelChild.Controls.Add(childForm);
            if (panelChild.Width < childForm.Width || panelChild.Height < childForm.Height)
                panelChild.Size = childForm.Size;

            panelChild.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }
        private void getInfo()
        {
            try
            {
                DataTable dt; DataTable dt1;
                string query = "SELECT rancholocal FROM RANCHOLOCAL";
                conn.QueryMovGanado(query, out dt);
                ran_id = Convert.ToInt32(dt.Rows[0][0]);

                query = "SELECT ran_desc, emp_id, emp_desc FROM configuracion where ran_id = " + ran_id.ToString();
                conn.QuerySIO(query, out dt1);
                ran_nombre = dt1.Rows[0][0].ToString();
                emp_id = Convert.ToInt32(dt1.Rows[0][1]);
                emp_nombre = dt1.Rows[0][2].ToString();

            }
            catch
            {
                ran_id = 0;
                ran_nombre = "";
                emp_id = 0;
                emp_nombre = "";
            }

        }

        //Consumo por Ingrediente
        private void button23_Click(object sender, EventArgs e)
        {
            //Consumo por Ingrediente
            bool empresa = checkBox1.Checked;
            openChildFormPanel(new Consumo_Por_Ingrediente(ran_id, emp_id, ran_nombre, emp_nombre));
            panelChild.Visible = true;
            panelExit.Visible = true;
            panelConfig.Visible = false;
        }

        //Reporte por Corral
        private void button24_Click(object sender, EventArgs e)
        {
            //openChildFormPanel(new Reporte_Corral());
            panelChild.Visible = true;
            panelExit.Visible = true;
            panelConfig.Visible = false;
        }

        //Reporte Diario
        private void button25_Click(object sender, EventArgs e)
        {
            //Reporte Diario
            openChildFormPanel(new Reporte_Diario(ran_id, ran_nombre, emp_id, emp_nombre));
            panelChild.Visible = true;
            panelExit.Visible = true;
            panelConfig.Visible = false;

        }

        // Reporte Diario por Empresa
        private void button26_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Programa no disponible", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Teoricos Precios Racion
        private void button29_Click(object sender, EventArgs e)
        {
            Process.Start("C:\\Movganado\\Procesos\\TeoricoPrecios.exe");
        }

        private void panelChild_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            panelConfig.Visible = true;
            panelChild.Visible = false;
            panelExit.Visible = false;
        }

        private void panelConfig_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button30_Click(object sender, EventArgs e)
        {
            label2.Visible = true;
            textBox1.Visible = true;
            textBox1.Select();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                log(textBox1.Text);
            }
        }

        public void log(string pwd)
        {
            if(pwd == "hc")
            {                
                
            }
            else
            {
                MessageBox.Show("Contraseña incorrecta", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

       //Agrupar Inventarios
        private void button14_Click(object sender, EventArgs e)
        {
            
        }

        //Prorrateo
        private void button17_Click(object sender, EventArgs e)
        {
            openChildFormPanel(new Prorrateo(emp_id, emp_nombre, ran_id, ran_nombre));
            //openChildFormPanel(new ProrrateoT(emp_id, emp_nombre, ran_id, ran_nombre));
            panelChild.Visible = true;
            panelExit.Visible = true;
            panelConfig.Visible = false;
            
        }

        //Agregar carros
        private void button16_Click(object sender, EventArgs e)
        {
            Process.Start("C:\\Movganado\\Procesos\\carros\\Debug\\Carros.exe");
        }

        //Captura Diaria
        private void button10_Click(object sender, EventArgs e)
        {
            bool empresa = checkBox1.Checked;
            //openChildFormPanel(new Racion_Tracker_Fuera(ran_id, emp_id, empresa, tipo));
            panelChild.Visible = true;
            panelExit.Visible = true;
            panelConfig.Visible = false;            
        }

        //Nivel de Cribas
        private void button12_Click(object sender, EventArgs e)
        {
            Process.Start("C:\\Movganado\\Procesos\\Nivelcribas.exe");
        }

        //Relacion de Corrales
        private void button28_Click(object sender, EventArgs e)
        {
            Process.Start("C:\\Movganado\\Procesos\\RelaCorrales.exe");
        }

        //Extraer Informacion Tracker
        private void button15_Click(object sender, EventArgs e)
        {
       
        }
     

        private void panel1_SizeChanged(object sender, EventArgs e)
        {

        }

    
        private void button22_Click(object sender, EventArgs e)
        {
            panelReportes.Visible = !panelReportes.Visible;
            if (panelReportes.Visible)
            {
                panelProgramas.Visible = false;
                panelCaptura.Visible = false;
            }
        }

        private void button27_Click(object sender, EventArgs e)
        {
            panelCaptura.Visible = !panelCaptura.Visible;
            if (panelCaptura.Visible)
            {
                panelReportes.Visible = false;
                panelProgramas.Visible = false;
            }

        }

        private void button13_Click(object sender, EventArgs e)
        {
            panelProgramas.Visible = !panelProgramas.Visible;
            if (panelProgramas.Visible)
            {
                panelReportes.Visible = false;
                panelCaptura.Visible = false;
            }

        }

        //Mapeo
        private void button18_Click(object sender, EventArgs e)
        {
            Process.Start("C:\\Movganado\\Procesos\\Mapeo\\Debug\\mapeo.exe");
        }

        //Inventario Tracker
        private void button19_Click(object sender, EventArgs e)
        {

        }

        //Exportar / Enviar Prorrateo    
        private void button20_Click(object sender, EventArgs e)
        {
            openChildFormPanel(new Exportar_Prorrateo(ran_id, ran_nombre,emp_id, emp_nombre,false, false));
            panelChild.Visible = true;
            panelExit.Visible = true;
            panelConfig.Visible = false;
        }
        //boton Exit
        private void button31_Click(object sender, EventArgs e)
        {
            panelChild.Visible = false;
            panelConfig.Visible = true;
            panelExit.Visible = false;
        }

        //Corte Manual
        private void button11_Click(object sender, EventArgs e)
        {
            
            Process.Start("C:\\Users\\lgomez\\Documents\\Proyectos\\GTH_Consumos\\Consumos\\Consumos\\bin\\Debug\\Consumos.exe");

        }

      

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            cbEmpresa = checkBox1.Checked;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            panelChild.Visible = false;
            panelConfig.Visible = true;
            panelExit.Visible = false;
            panelReportes.Visible = false;
            panelCaptura.Visible = false;
            panelProgramas.Visible = false;
            label1.Text = ran_nombre.ToUpper();
        }

        
    }
}
