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
    public partial class Error : Form
    {
        DataTable dt;

        public Error(DataTable dt)
        {
            InitializeComponent();
            this.dt = dt;
        }

        private void Error_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = dt;
        }
    }
}
