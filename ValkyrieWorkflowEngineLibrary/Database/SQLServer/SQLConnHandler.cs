using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ValkyrieWorkflowEngineLibrary.Database.SQLServer
{
	/// <summary>
	/// utility class for handling database transactions
	/// </summary>
	public class SQLTransactionData
	{
		public SqlConnection SQLConn { get; set; }
		public SqlTransaction Transaction { get; set; }
		public SqlCommand Command { get; set; }
	}
	/// <summary>
	/// Utility class for creating a SQL Server connection and executing queries/nonqueries
	/// over parameterized stored procs
	/// </summary>
	public class SQLConnHandler
	{
		private static string ConnectionString = ConfigurationManager.ConnectionStrings["Valkyrie"].ConnectionString;
		public static DataTable ExecuteDataTableSP(string StoredProcedureName, params SQLParameter[] Params)
		{
			SqlConnection SQLConn = new SqlConnection(SQLConnHandler.ConnectionString);
			DataTable ResultTable = null;
			try
			{
				SQLConn.Open();
				SqlCommand Command = new SqlCommand(StoredProcedureName, SQLConn);
				Command.CommandType = CommandType.StoredProcedure;
				for (int i = 0; i < Params.Length; i++)
				{
					Command.Parameters.Add(new SqlParameter(Params[i].ParamName, Params[i].ParamValue));
				}
				SqlDataAdapter Adapter = new SqlDataAdapter(Command);
				ResultTable = new DataTable();
				Adapter.Fill(ResultTable);
			}
			catch (Exception Ex)
			{
				ResultTable = null;
			}
			finally
			{
				SQLConn.Close();
				SQLConn.Dispose();
			}
			return ResultTable;
		}
		public static int ExecuteNonQuerySP(string StoredProcedureName, params SQLParameter[] Params)
		{
			SqlConnection SQLConn = new SqlConnection(SQLConnHandler.ConnectionString);
			int Rows = -1;
			try
			{
				SQLConn.Open();
				SqlCommand Command = new SqlCommand(StoredProcedureName, SQLConn);
				Command.CommandType = CommandType.StoredProcedure;
				for (int i = 0; i < Params.Length; i++)
				{
					Command.Parameters.Add(new SqlParameter(Params[i].ParamName, Params[i].ParamValue));
				}

				Command.Connection = SQLConn;
				Rows = Command.ExecuteNonQuery();
			}
			catch (Exception Ex)
			{
				Rows = -1;
			}
			finally
			{
				SQLConn.Close();
				SQLConn.Dispose();
			}
			return Rows;
		}
		//todo: transaction handling needs better error detection and recovery
		// this is primarily for bulk inserts/updates
		public static SQLTransactionData StartTransaction(string StoredProcedureName)
		{
			SQLTransactionData TransData = new SQLTransactionData();
			TransData.SQLConn = new SqlConnection(SQLConnHandler.ConnectionString);
			TransData.SQLConn.Open();
			TransData.Transaction = TransData.SQLConn.BeginTransaction();
			TransData.Command = new SqlCommand(StoredProcedureName, TransData.SQLConn);
			TransData.Command.Transaction = TransData.Transaction;
			TransData.Command.CommandType = CommandType.StoredProcedure;
			return TransData;
		}
		public static void EndTransaction(SQLTransactionData TransData)
		{
			try { TransData.Transaction.Commit(); }
			catch (Exception)
			{
				TransData.Transaction.Rollback();
				throw;
			} finally
			{
				TransData.SQLConn.Close();
				TransData.SQLConn.Dispose();
			}
		}
		public static int ExecuteNonQuerySPTrans(SQLTransactionData TransData, params SQLParameter[] Params)
		{
			int Rows = -1;
			try
			{
				TransData.Command.Parameters.Clear();
				for (int i = 0; i < Params.Length; i++)
				{
					TransData.Command.Parameters.Add(new SqlParameter(Params[i].ParamName, Params[i].ParamValue));
				}

				Rows = TransData.Command.ExecuteNonQuery();
			}
			catch (Exception Ex)
			{
				Rows = -1;
			}
			finally
			{

			}
			return Rows;
		}
	}
}
