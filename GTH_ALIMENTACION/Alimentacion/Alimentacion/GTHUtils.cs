using Alimentacion.Controller.Base;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alimentacion
{
    class GTHUtils
    {
        public static void SavePDF(ReportViewer viewer, string savePath)
        {
            try
            {
                byte[] Bytes = viewer.LocalReport.Render("PDF", "");

                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    stream.Write(Bytes, 0, Bytes.Length);
                }
            }
            catch (IOException ex) { Console.WriteLine(ex.Message); }
        }

        public static void DeleteFile(string ruta)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(ruta);
            try
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
