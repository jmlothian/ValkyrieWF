using System;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0168  //get rid of "unused variable" warnings.

namespace ValkyrieGUI
{
    public enum Types { String, Date, Numeric };
	/// <summary>
	/// Legacy .Net 1.1 utility classes for SQL access.  Should be burned and replaced.  Only used
	/// in test code now.
	/// </summary>
    public class QuickData : IDisposable
    {
        public SqlConnection myConnection;
        public SqlDataReader myReader = null;
        public DataTable dt;
        public bool hasError = false;
        private bool disposed = false;
		private string mstrConString = ConfigurationManager.ConnectionStrings["Valkyrie"].ConnectionString;
		public string lastQuery = "";
        int lastInsertId = -1;
        public List<QDInserter> BulkInserts = new List<QDInserter>();
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (myConnection != null)
                    {
                        myConnection.Close();
                        myConnection.Dispose();
                    }
                }
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public QuickData()
        {
            myConnection = new SqlConnection(mstrConString);
            myConnection.Open();
        }
        public QuickData(string conStr)
        {
            myConnection = new SqlConnection(conStr);
            myConnection.Open();
            mstrConString = conStr;
        }
        public void open()
        {
            myConnection = new SqlConnection(mstrConString);
            myConnection.Open();
        }
        public void close()
        {
            if (myReader != null)
            {
                if (!myReader.IsClosed)
                    myReader.Close();
            }
            if (myConnection != null)
            {
                myConnection.Close();
                myConnection = null;
            }
        }
        public void CheckReader()
        {
            if (myReader != null)
            {
                if (!myReader.IsClosed)
                    myReader.Close();
            }
        }
        public bool nextRow()
        {
            if (hasError == true)
            {
                return false;
            }
            else
            {
                if (myReader != null)
                {
                    try
                    {
                        return myReader.Read();
                    }
                    catch (SystemException ex)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public Object this[int x]
        {
            get { return myReader[x]; }
        }
        public Object this[string x]
        {
            get { return myReader[x]; }
        }
        public DataTable getDataTable(string sql)
        {
            CheckReader();
            dt = new DataTable();
            using (SqlCommand mySqlCommand = new SqlCommand(sql, myConnection))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(mySqlCommand))
                {
                    sda.Fill(dt);
                }
            }
            return dt;
        }
        public void select(string sql)
        {
            lastQuery = sql;
            hasError = false;
            CheckReader();
            SqlCommand mySqlCommand = new SqlCommand(sql, myConnection);
            try
            {
                myReader = mySqlCommand.ExecuteReader();
            }
            catch (SqlException sqlex)
            {
                hasError = true;
            }
        }
        public void delete(string sql)
        {
            CheckReader();
            lastQuery = sql;
            using (SqlCommand mySqlCommand = new SqlCommand(sql, myConnection))
            {
                try
                {
                    mySqlCommand.ExecuteNonQuery();
                }
                catch (System.Exception ex)
                {
                }
            }
        }
        public void update(string sql)
        {
            CheckReader();
            lastQuery = sql;
            using (SqlCommand mySqlCommand = new SqlCommand(sql, myConnection))
            {
                try
                {
                    mySqlCommand.ExecuteNonQuery();
                }
                catch (System.Exception ex)
                {
                }
            }
        }
        public int insert(string sql)
        {
            CheckReader();
            lastQuery = sql;

            using (SqlCommand mySqlCommand = new SqlCommand(sql, myConnection))
            {
                try
                {
                    mySqlCommand.ExecuteNonQuery();
                    SqlCommand mySql2Command = new SqlCommand("SELECT @@IDENTITY as id", myConnection);
                    try
                    {
                        myReader = mySql2Command.ExecuteReader();
                        myReader.Read();
                        int.TryParse(myReader["id"].ToString(), out lastInsertId);
                    }
                    catch (SqlException sqlex)
                    {
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(sql);
                }
            }
            return lastInsertId;
        }
        public void insert(QDInserter ins)
        {
            string sql = "INSERT INTO " + ins.tableName + " ";
            string fields = "(";
            string values = "(";
            foreach (DictionaryEntry d in ins.ht)
            {
                fields += d.Key + ",";
                values += d.Value + ",";
            }
            fields = fields.TrimEnd(',') + ")";
            values = values.TrimEnd(',') + ")";
            sql += fields + " VALUES " + values;
            ins.lastInsertID = insert(sql);
        }
        public void DoBulkInsert()
        {
            if (BulkInserts.Count > 0)
            {
                string sql = "INSERT INTO " + BulkInserts[0].tableName + " ";
                string fields = "(";
                string values = "";
                foreach (DictionaryEntry d in BulkInserts[0].ht)
                {
                    fields += d.Key + ",";
                }
                fields = fields.TrimEnd(',') + ")";
                foreach (QDInserter ins in BulkInserts)
                {
                    values += "(";
                    foreach (DictionaryEntry d in ins.ht)
                    {
                        values += d.Value + ",";
                    }
                    values = values.TrimEnd(',') + "),";
                }
                values = values.TrimEnd(',');
                sql += fields + " VALUES " + values;
                BulkInserts[0].lastInsertID = insert(sql);
                BulkInserts.Clear();
            }
        }
        public void update(QDUpdater upd)
        {
            string sql = "UPDATE " + upd.tableName + " SET ";
            string sets = "";
            if (upd.condition == "")
            {
                throw new Exception("Condition of update not specified.  Refusing to update entire table.");
            }
            foreach (DictionaryEntry d in upd.ht)
            {
                sets += d.Key + "=" + d.Value + ",";
            }
            sql = sets.TrimEnd(',') + " WHERE " + upd.condition;
            update(sql);
        }
    }
}