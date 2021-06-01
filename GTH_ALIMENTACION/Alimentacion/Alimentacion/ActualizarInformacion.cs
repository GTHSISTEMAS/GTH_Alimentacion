using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alimentacion
{
    public partial class ActualizarInformacion : Form
    {
        public int Seleccionador = 0;
        public ActualizarInformacion()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Seleccionador = 2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Seleccionador = 1;
        }

        private void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
