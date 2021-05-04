using Alimentacion.Model.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Alimentacion.Controller.Base
{
    abstract class BaseController<T> where T : BaseEntity
    {
        protected Boolean transactionFlag;
        protected DbConnection connection;
        protected DbTransaction transaction;
        protected DbCommand command;
        string pattern = @"\@\w+";
        Regex rg;

        public DbConnection Connection
        {
            get { return connection; }
            set { connection = value; }
        }

        public DbCommand Command
        {
            get { return command; }
            set { command = value;  }
        }

        public BaseController()
        {
            this.rg = new Regex(pattern);
        }

        public BaseController(DbConnection connection)
        {
            this.connection = connection;
            this.rg = new Regex(pattern);
        }

        public void startTransaction(DbTransaction transaction)
        {
            command = connection.CreateCommand();
            command.Transaction = transaction;
            this.transaction = transaction;
            transactionFlag = true;
        }

        public void endTransaccion()
        {
            transactionFlag = false;
        }

        public void OpenConnection()
        {
            if (connection != null && connection.State != ConnectionState.Open && (!transactionFlag || connection.State == ConnectionState.Closed))
            {
                connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (connection != null && !transactionFlag)
            {
                connection.Close();
            }
        }
        abstract public bool Insert(T baseEntity);
        abstract public bool InsertMasiv(T[] baseEntities);
        abstract public bool Update(T baseEntity);
        abstract public bool Delete(T baseEntity);
        abstract public bool DeleteAll();
        abstract public bool DeleteAllToday();
        abstract public T Select(T baseEntity);
        abstract public T[] Search(T baseEntity, string query);

        protected void fillEntity(DbDataReader reader, T baseEntity)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string column = reader.GetName(i);
                try
                {
                    baseEntity.GetType().GetProperty(GetNameProperty(column)).SetValue(baseEntity, Convert.IsDBNull(reader[i]) ? null : reader[i]);

                }
                catch { }
            }
        }

        protected void SetParameters(DbCommand command, T baseEntity, string query)
        {
            MatchCollection parametros = rg.Matches(query);
            for (int count = 0; count < parametros.Count; count++)
            {
                string propiedad = GetNameProperty(parametros[count].Value.Substring(1));
                SetParameter(command, baseEntity, parametros[count].Value);
            }
        }

        protected string GetStrParameters(T baseEntity, string query)
        {
            string parameters = "(";
            MatchCollection parametros = rg.Matches(query);
            for (int count = 0; count < parametros.Count; count++)
            {
                string propiedad = GetNameProperty(parametros[count].Value.Substring(1));
                string id = parametros[count].Value;

                object value = baseEntity.GetType().GetProperty(GetNameProperty(id.Substring(1))).GetValue(baseEntity);
                if (value == null)
                {
                    parameters += " null";
                }
                else if (baseEntity.GetType().GetProperty(GetNameProperty(id.Substring(1))).PropertyType.Name == "String")
                {
                    parameters += " '" + value.ToString().Replace("'", "´") + "'";
                }
                else if (baseEntity.GetType().GetProperty(GetNameProperty(id.Substring(1))).PropertyType.Name == "DateTime")
                {
                    parameters += ((DateTime)value).ToString("dd/MM/yyyy HH:mm:ss");
                }
                else
                {
                    parameters += " " + value;
                }

                if (count < parametros.Count - 1)
                {
                    parameters += ",";
                }
            }
            return parameters += ")";
        }

        protected void SetParameters(DbCommand command, T baseEntity)
        {
            foreach (PropertyInfo propertyInfo in baseEntity.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    DbParameter param = command.CreateParameter();
                    SetType(param, propertyInfo.PropertyType.Name);
                    param.ParameterName = "@" + propertyInfo.Name.ToLower();
                    param.Value = propertyInfo.GetValue(baseEntity);
                    command.Parameters.Add(param);
                }
            }
        }

        protected void SetParameter(DbCommand command, T baseEntity, string id)
        {
            DbParameter param = command.CreateParameter();
            param.ParameterName = id;
            SetType(param, baseEntity.GetType().GetProperty(GetNameProperty(id.Substring(1))).PropertyType.Name);
            param.Value = baseEntity.GetType().GetProperty(GetNameProperty(id.Substring(1))).GetValue(baseEntity);
            command.Parameters.Add(param);
        }

        private void SetType(DbParameter param, string tipo)
        {
            //string tipo = type.Name;
            if ("Int64" == tipo)
            {
                param.DbType = DbType.Int64;
            }
            else if ("Int32" == tipo)
            {
                param.DbType = DbType.Int32;
            }
            else if ("Int16" == tipo)
            {
                param.DbType = DbType.Int16;
            }
            else if ("Decimal" == tipo)
            {
                param.DbType = DbType.Decimal;
            }
            else if ("Binary" == tipo)
            {
                param.DbType = DbType.Binary;
            }
            else if ("Byte" == tipo)
            {
                param.DbType = DbType.Byte;
            }
            else if ("Boolean" == tipo)
            {
                param.DbType = DbType.Boolean;
            }
            else if ("String" == tipo)
            {
                param.DbType = DbType.String;
            }
            else if ("AnsiString" == tipo)
            {
                param.DbType = DbType.AnsiString;
            }
            else if ("Time" == tipo)
            {
                param.DbType = DbType.Time;
            }
            else if ("Date" == tipo)
            {
                param.DbType = DbType.Date;
            }
            else if ("DateTime" == tipo)
            {
                param.DbType = DbType.DateTime;
            }
            else if ("DateTime2" == tipo)
            {
                param.DbType = DbType.DateTime2;
            }
            else if ("Xml" == tipo)
            {
                param.DbType = DbType.Xml;
            }
            else if ("UInt16" == tipo)
            {
                param.DbType = DbType.UInt16;
            }
            else if ("UInt32" == tipo)
            {
                param.DbType = DbType.UInt32;
            }
            else if ("UInt64" == tipo)
            {
                param.DbType = DbType.UInt64;
            }
        }

        public string GetNameProperty(string id)
        {
            if (id != null && id.Length > 0)
            {
                return id.First().ToString().ToUpper() + id.Substring(1).ToLower();
            }
            return "";
        }
    }

}
