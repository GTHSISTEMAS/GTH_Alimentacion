using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;
using System.IO;
using FirebirdSql.Data;
using FirebirdSql.Data.FirebirdClient;
using System.Data.OleDb;
using MySql.Data.MySqlClient;

namespace Alimentacion
{
    public class ConnSIO
    {
        //Conexiones
        FbConnection ConnFB = new FbConnection();
        SqlConnection ConnAlimento = new SqlConnection();
        SqlConnection connSIO = new SqlConnection();
        SqlConnection connSIE = new SqlConnection();       
        OleDbConnection ConnIIE = new OleDbConnection();
        MySqlConnection connMs = new MySqlConnection();

        //Comandos
        FbCommand cmdFB = new FbCommand();
        SqlCommand cmdSIO = new SqlCommand();
        SqlCommand cmdSIE = new SqlCommand();
        SqlCommand cmdAlimento = new SqlCommand();
        OleDbCommand cmdIIE = new OleDbCommand();
        MySqlCommand cmdMS = new MySqlCommand();


        public void Iniciar(string Catalogo)
        {
            //Sql
            //DBAlimento
            string cadenaAlimento = ConfigurationManager.AppSettings["CadenaCosumo"];
            cadenaAlimento = cadenaAlimento.Replace("$usr", "sa").Replace("$pas", "CiaPrest_");
            ConnAlimento.ConnectionString = cadenaAlimento;
            cmdAlimento.Connection = ConnAlimento;
            //DBSIO
            string cadenaSIO = ConfigurationManager.AppSettings["CadenaSIO"];
            cadenaSIO = cadenaSIO.Replace("$usr", "sa").Replace("$pas", "CiaPrest_");
            cadenaSIO = cadenaSIO.Replace("$catalogo", Catalogo);
            connSIO.ConnectionString = cadenaSIO;
            cmdSIO.Connection = connSIO;
            //DBSIE
            string cadenaSIE = ConfigurationManager.AppSettings["CadenaSIE"];
            cadenaSIE = cadenaSIE.Replace("$usr", "sa").Replace("$pas", "CiaPrest_");
            connSIE.ConnectionString = cadenaSIE;
            cmdSIE.Connection = connSIE;
           
            string cadenaAcces = ConfigurationManager.AppSettings["connMOVSIO"];
            cadenaAcces = cadenaAcces.Replace("$pwd", "CiaPrest_");
            ConnIIE.ConnectionString = cadenaAcces;
            cmdIIE.Connection = ConnIIE;

            //Mysql
            string cadenaMS = ConfigurationManager.AppSettings["db"];
            cadenaMS = cadenaMS.Replace("$user", "gth_test").Replace("$pwd", "gthTest0!");
            connMs.ConnectionString = cadenaMS;
            cmdMS.Connection = connMs;

            iniciarTracker();
        }


        public void Iniciar()
        {
            //DBAlimento
            string cadenaAlimento = ConfigurationManager.AppSettings["CadenaCosumo"];
            cadenaAlimento = cadenaAlimento.Replace("$usr", "sa").Replace("$pas", "CiaPrest_");
            ConnAlimento.ConnectionString = cadenaAlimento;
            cmdAlimento.Connection = ConnAlimento;
            //DBSIO
            string cadenaSIO = ConfigurationManager.AppSettings["CadenaSIO"];
            cadenaSIO = cadenaSIO.Replace("$usr", "sa").Replace("$pas", "CiaPrest_");
            connSIO.ConnectionString = cadenaSIO;
            cmdSIO.Connection = connSIO;
            //DBSIE
            string cadenaSIE = ConfigurationManager.AppSettings["CadenaSIE"];
            cadenaSIE = cadenaSIE.Replace("$usr", "sa").Replace("$pas", "CiaPrest_");
            connSIE.ConnectionString = cadenaSIE;
            cmdSIE.Connection = connSIE;            
            //MOVSIO
            string cadenaAcces = ConfigurationManager.AppSettings["connMOVSIO"];
            cadenaAcces = cadenaAcces.Replace("$pwd", "CiaPrest_");
            ConnIIE.ConnectionString = cadenaAcces;
            cmdIIE.Connection = ConnIIE;
            //Mysql
            string cadenaMS = ConfigurationManager.AppSettings["db"];
            cadenaMS = cadenaMS.Replace("$user", "gth_test").Replace("$pwd", "gthTest0!");
            connMs.ConnectionString = cadenaMS;
            cmdMS.Connection = connMs;

            iniciarTracker();
        }

        public void iniciarTracker()
        {
            string rancho = GetRanchoCadena();
            string tracker = "Tracker" + rancho + ".FDB";
            string cadenaTracker = ConfigurationManager.AppSettings["CadenaTracker"];
            cadenaTracker = cadenaTracker.Replace("$user", "SYSDBA").Replace("$pwd", "masterkey").Replace("$tracker", tracker);
            ConnFB.ConnectionString = cadenaTracker;
            cmdFB.Connection = ConnFB;
        }
        public string GetRanchoCadena()
        {
            string rancho = "";
            try
            {
                ConnIIE.Open();
                DataTable dt;
                string query = "SELECT RANCHOLOCAL FROM RANCHOLOCAL";
                OleDbDataAdapter da = new OleDbDataAdapter(query, ConnIIE);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
                int ranId = Convert.ToInt32(dt.Rows[0][0]);
                rancho = ranId > 9 ? ranId.ToString() : "0" + ranId.ToString();
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                ConnIIE.Close();
            }
            return rancho;
        }
        public void QueryAlimento(string query)
        {
            try
            {
                ConnAlimento.Open();
                cmdAlimento.CommandText = query;
                cmdAlimento.ExecuteNonQuery();
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnAlimento.Close(); }
        }
       

        public void QueryAlimento(string query, out DataTable dt)
        {
            dt = new DataTable();
            try
            {
                ConnAlimento.Open();
                cmdAlimento.CommandText = "SET DATEFORMAT 'YMD'";
                cmdAlimento.ExecuteNonQuery();
                SqlDataAdapter da = new SqlDataAdapter(query, ConnAlimento);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnAlimento.Close(); }
        }

        public void QuerySIE(string campos, string tabla, string condicion, out DataTable dt)
        {
            dt = new DataTable();
            try
            {
                connSIE.Open();
                cmdSIE.CommandText = "SET DATEFORMAT 'YMD'";
                cmdSIE.ExecuteNonQuery();
                string query = "SELECT " + campos + " FROM " + tabla + " " + condicion;
                SqlDataAdapter da = new SqlDataAdapter(query, connSIE);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { connSIE.Close(); }
        }

        public void QuerySIE(string query, out DataTable dt)
        {
            dt = new DataTable();
            try
            {
                connSIE.Open();
                cmdSIE.CommandText = "SET DATEFORMAT 'YMD'";
                cmdSIE.ExecuteNonQuery();
                SqlDataAdapter da = new SqlDataAdapter(query, connSIE);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                connSIE.Close();
            }
        }

        public void QueryTracker(string query, out DataTable dt)
        {
            dt = new DataTable();
            try
            {
                ConnFB.Open();
                FbDataAdapter da = new FbDataAdapter(query, ConnFB);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                ConnFB.Close();
            }
        }
        public void QueryTracker(string racion, string query, out DataTable dt)
        {
            dt = new DataTable();
            try
            {
                ConnFB.Open();
                FbDataAdapter da = new FbDataAdapter(query, ConnFB);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (DbException e) { MessageBox.Show("Error por caracteres especiales en: " + racion, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                ConnFB.Close();
            }
        }
        public void QueryMovGanado(string query, out DataTable dt)
        {
            dt = new DataTable();
            try
            {
                ConnIIE.Open();
                OleDbDataAdapter da = new OleDbDataAdapter(query, ConnIIE);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                ConnIIE.Close();
            }
        }

        public void InsertAllSIO(string tabla, string valores)
        {
            try
            {
                int insertCount = 0;
                connSIO.Open();
                cmdSIO.CommandText = "SET DATEFORMAT 'YMD'";
                cmdSIO.ExecuteNonQuery();
                string query = "INSERT INTO " + tabla + " VALUES(" + valores + ")";
                cmdSIO.CommandText = query;
                insertCount = cmdSIO.ExecuteNonQuery();
                Console.WriteLine("{0} registros insertados en la tabla {1}", insertCount, tabla);
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { connSIO.Close(); }
        }

        public void QueryMs(string query, out DataTable dt)
        {
            dt = new DataTable();
            try
            {
                connMs.Open();
                MySqlDataAdapter da = new MySqlDataAdapter(query, connMs);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { connMs.Close(); }
        }

        public void QuerySIO(string query, out DataTable dt)
        {
            dt = new DataTable();
            try
            {
                connSIO.Open();
                cmdSIO.CommandText = "SET DATEFORMAT 'YMD'";
                cmdSIO.ExecuteNonQuery();
                SqlDataAdapter da = new SqlDataAdapter(query, connSIO);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { connSIO.Close(); }
        }
    
        public void InsertMasivAlimento(string campos, string tabla, string valores)
        {
            try
            {
                int insertCount = 0;
                ConnAlimento.Open();
                cmdAlimento.CommandText = "SET DATEFORMAT 'YMD'";
                cmdAlimento.ExecuteNonQuery();
                string query = "INSERT INTO " + tabla + "(" + campos + ") VALUES " + valores;
                cmdAlimento.CommandTimeout = 120;
                cmdAlimento.CommandText = query;
                insertCount = cmdAlimento.ExecuteNonQuery();
                Console.WriteLine("{0} registros insertados en la tabla {1}", insertCount, tabla);
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                ConnAlimento.Close();
            }
        }

        public void InsertMasivAlimento(string tabla, string valores)
        {
            try
            {
                int insertCount = 0;
                ConnAlimento.Open();
                cmdAlimento.CommandText = "SET DATEFORMAT 'YMD'";
                cmdAlimento.ExecuteNonQuery();
                string query = "INSERT INTO " + tabla  + " VALUES " + valores;
                cmdAlimento.CommandTimeout = 120;
                cmdAlimento.CommandText = query;
                insertCount = cmdAlimento.ExecuteNonQuery();
                Console.WriteLine("{0} registros insertados en la tabla {1}", insertCount, tabla);
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                ConnAlimento.Close();
            }
        }

        public void InsertAlimento(string campos, string tabla, string valores)
        {
            try
            {
                int insertCount = 0;
                ConnAlimento.Open();
                cmdAlimento.CommandText = "SET DATEFORMAT 'YMD'";
                cmdAlimento.ExecuteNonQuery();
                string query = "INSERT INTO " + tabla + "( " + campos + ") VALUES (" + valores + ")";
                cmdAlimento.CommandText = query;
                insertCount = cmdAlimento.ExecuteNonQuery();
                Console.WriteLine("{0} registros insertados en la tabla {1}", insertCount, tabla);
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnAlimento.Close(); }
        }

        public void InsertSelecttAlimento(string query)
        {
            try
            {
                int insertCount = 0;
                ConnAlimento.Open();
                cmdAlimento.CommandText = "SET DATEFORMAT 'YMD'";
                cmdAlimento.ExecuteNonQuery();
                cmdAlimento.CommandText = query;
                insertCount = cmdAlimento.ExecuteNonQuery();
                Console.WriteLine("{0} registros insertados", insertCount);
            }
            catch(DbException ex) { MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnAlimento.Close(); }
        }

        public void InsertAlimento(string tabla, string valores)
        {
            try
            {
                int insertCount = 0;
                ConnAlimento.Open();
                cmdAlimento.CommandText = "SET DATEFORMAT 'YMD'";
                cmdAlimento.ExecuteNonQuery();
                string query = "INSERT INTO " + tabla + " VALUES(" + valores + ")";
                cmdAlimento.CommandText = query;
                insertCount = cmdAlimento.ExecuteNonQuery();
                Console.WriteLine("{0} registros insertados en la tabla {1}", insertCount, tabla);
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnAlimento.Close(); }
        }

        public void DeleteAlimento(string tabla, string condicion)
        {
            try
            {
                int deleteCount = 0;
                ConnAlimento.Open();
                cmdAlimento.CommandText = "SET DATEFORMAT 'YMD'";
                cmdAlimento.ExecuteNonQuery();
                string query = "delete from " + tabla + " " + condicion;
                cmdAlimento.CommandText = query;
                deleteCount = cmdAlimento.ExecuteNonQuery();
                Console.WriteLine("{0} registros eliminados en la tabla {1}", deleteCount, tabla);
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnAlimento.Close(); }
        }

        public void UPDATEAlimento(string tabla, string campos,string condicion)
        {
            try
            {
                ConnAlimento.Open();
                cmdAlimento.CommandText = "SET DATEFORMAT 'YMD'";
                cmdAlimento.ExecuteNonQuery();
                string query = "UPDATE " + tabla + " SET " + campos + " " + condicion;
                cmdAlimento.CommandText = query;
                cmdAlimento.ExecuteNonQuery();
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnAlimento.Close(); } 
        }
       
        public void UpdateMovsio(string query)
        {
            try
            {
                ConnIIE.Open();                
                cmdIIE.CommandText = query;
                cmdIIE.ExecuteNonQuery();
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnIIE.Close(); }

        }

        public void UpdateMovsio(string tabla, string campos, string condicion)
        {
            try
            {
                ConnIIE.Open();                
                string query = "UPDATE " + tabla + " SET " + campos + " " + condicion;
                cmdIIE.CommandText = query;
                cmdIIE.ExecuteNonQuery();                
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnIIE.Close(); }
        }

        public void InsertMovsio(string query)
        {
            try
            {
                ConnIIE.Open();
                cmdIIE.CommandText = query;
                cmdIIE.ExecuteNonQuery();
                
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnIIE.Close(); }
        }

        public void InsertMovsio(string campos, string tabla, string valores)
        {
            try
            {
                ConnIIE.Open();
                string query = "INSERT INTO " + tabla + "(" + campos + ") VALUES(" + valores + ")";
                cmdIIE.CommandText = query;
                cmdIIE.ExecuteNonQuery();
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnIIE.Close(); }
        }
        public void InsertMovsio(string tabla, string valores)
        {
            try
            {
                ConnIIE.Open();
                string query = "INSERT INTO " + tabla + " VALUES(" + valores + ")";
                cmdIIE.CommandText = query;
                cmdIIE.ExecuteNonQuery();
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnIIE.Close(); }
        }

        public void InsertMasivMovsio(string campos, string tabla, string valores)
        {
            try
            {
                ConnIIE.Open();
                string query = "INSERT INTO " + tabla + "(" + campos + ") VALUES " + valores;
                cmdIIE.CommandText = query;
                cmdIIE.ExecuteNonQuery();
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnIIE.Close(); }
        }

        public void InsertMasivMovsio(string tabla, string valores)
        {
            try
            {
                ConnIIE.Open();
                string query = "INSERT INTO " + tabla + " VALUES " + valores;
                cmdIIE.CommandText = query;
                cmdIIE.ExecuteNonQuery();
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnIIE.Close(); }
        }

        public void DeleteMovsio(string query)
        {
            try
            {
                int deleteCount = 0;
                ConnIIE.Open();
                cmdIIE.CommandText = query;
                deleteCount = cmdIIE.ExecuteNonQuery();
                Console.WriteLine("{0} elementos eliminados de la BD movsio",deleteCount);
            }
            catch (DbException e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception e) { MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { ConnIIE.Close(); }
        }
    }
}
