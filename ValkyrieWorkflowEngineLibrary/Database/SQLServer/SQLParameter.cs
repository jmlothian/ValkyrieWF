using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValkyrieWorkflowEngineLibrary.Database.SQLServer
{
	/// <summary>
	/// Utility class for sending parameters to SQL queryes/nonqueries 
	/// </summary>
	public class SQLParameter
	{
		public string ParamName { get; set; }
		public object ParamValue { get; set; }
	}
}
