using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace ValkyrieWorkflowEngineLibrary.Database.SQLServer
{

	/// <summary>
	/// Example implementation of IDatabaseHandler for SQL Server
	/// </summary>
	public class SQLServerDBHandler : IDatabaseHandler
	{
		
		public DataTable GetPendingWFInstances()
		{
			return SQLConnHandler.ExecuteDataTableSP("GetPendingWFInstances");
		}

		public WFTemplateLoadingData LoadWFTemplates()
		{
			WFTemplateLoadingData LoadingData = new WFTemplateLoadingData();
			DataTable dt = SQLConnHandler.ExecuteDataTableSP("LoadWFTemplates");
			if (dt != null)
			{
				foreach (DataRow dr in dt.Rows)
				{
					if (dr["InProduction"].ToString() == "True")
					{
						if (LoadingData.LoadedActiveInstanceByType.ContainsKey(dr["WFType"].ToString()))
						{
							//warning, already here
						}
						LoadingData.LoadedActiveInstanceByType[dr["WFType"].ToString()] = int.Parse(dr["WFTemplateID"].ToString());
					}

					ValkWFStep FirstStep = new ValkWFStep();
					FirstStep.WFTemplateID = int.Parse(dr["WFTemplateID"].ToString());
					LoadingData.WFsToLoad.Add(FirstStep);

					DataTable steps =
						SQLConnHandler.ExecuteDataTableSP("LoadWFTemplateFirstStep",
						new SQLParameter() { ParamName = "@WFTemplateID", ParamValue = dr["WFTemplateID"].ToString() });
					if (steps.Rows.Count > 0)
					{
						LoadingData.WFsToLoad[LoadingData.WFsToLoad.Count - 1].HandleCategory = steps.Rows[0]["OnExecuteCall"].ToString();
						LoadingData.WFsToLoad[LoadingData.WFsToLoad.Count - 1].StepName = steps.Rows[0]["Name"].ToString();
						LoadingData.WFsToLoad[LoadingData.WFsToLoad.Count - 1].WFTemplateStepID = int.Parse(steps.Rows[0]["WFTemplateStepID"].ToString());
						LoadingData.WFsToLoad[LoadingData.WFsToLoad.Count - 1].SyncCount = int.Parse(steps.Rows[0]["SyncCount"].ToString());
						LoadingData.WFsToLoad[LoadingData.WFsToLoad.Count - 1].GroupID = int.Parse(steps.Rows[0]["GroupID"].ToString());

					}
					else
					{
						//todo: error, no first step for this WF
					}
				}
			}
			else
			{
				//todo: no templates!
			}
			return LoadingData;
		}

		public List<ValkWFStep> LoadChildSteps(ref ValkWFStep CurrentStep)
		{
			List<ValkWFStep> Children = new List<ValkWFStep>();
			DataTable dt = SQLConnHandler.ExecuteDataTableSP("LoadChildSteps",
				new SQLParameter() { ParamName = "@WFTemplateID", ParamValue = CurrentStep.WFTemplateID },
				new SQLParameter() { ParamName = "@WFTemplateStepID", ParamValue = CurrentStep.WFTemplateStepID });
			foreach(DataRow Row in dt.Rows)
			{
				ValkWFStep Step = new ValkWFStep();
				Step.WFTemplateID = CurrentStep.WFTemplateID;
				Step.HandleCategory = Row["OnExecuteCall"].ToString();
				Step.StepName = Row["Name"].ToString();
				Step.WFTemplateStepID = int.Parse(Row["WFTEmplateStepID_To"].ToString());
				Step.SyncCount = int.Parse(Row["SyncCount"].ToString());
				Step.GroupID = int.Parse(Row["GroupID"].ToString());
				Step.ParentStep = CurrentStep;
				Step.InstanceKey = CurrentStep.InstanceKey;
				Children.Add(Step);
			}
			return Children;
		}

		public void StartWFInstance(int WFInstanceID)
		{
			SQLConnHandler.ExecuteNonQuerySP("StartWFInstance",
				new SQLParameter() { ParamName = "@WFInstanceID", ParamValue = WFInstanceID });
		}

		public void InsertWFInstance(SortedDictionary<int, ValkWFStep> ToInsert)
		{
			SQLTransactionData TransData = SQLConnHandler.StartTransaction("InsertWFInstance");
			foreach (KeyValuePair<int, ValkWFStep> Step in ToInsert)
			{
				SQLConnHandler.ExecuteNonQuerySPTrans(TransData,
					new SQLParameter() { ParamName = "@InstanceKey", ParamValue = Step.Value.InstanceKey },
					new SQLParameter() { ParamName = "@WFTemplateID", ParamValue = Step.Value.WFTemplateID },
					new SQLParameter() { ParamName = "@Status", ParamValue = "inactive" },
					new SQLParameter() { ParamName = "@WFTemplateStepID", ParamValue = Step.Value.WFTemplateStepID },
					new SQLParameter() { ParamName = "@UserID", ParamValue = 1 },
					new SQLParameter() { ParamName = "@StartTime", ParamValue = SqlDateTime.MinValue },
					new SQLParameter() { ParamName = "@SyncCount", ParamValue = Step.Value.SyncCount },
					new SQLParameter() { ParamName = "@ExceptionID", ParamValue = -1 }
				);
			}

			SQLConnHandler.EndTransaction(TransData);
		}

		public void UpdateSteps(List<ValkQueueWFMessage> Updates)
		{
			//insert all the pending updates
			SQLTransactionData TransData = SQLConnHandler.StartTransaction("InsertUpdates");
			foreach (ValkQueueWFMessage Step in Updates)
			{
				SQLConnHandler.ExecuteNonQuerySPTrans(TransData,
					new SQLParameter() { ParamName = "@InstanceKey", ParamValue = Step.InstanceKey },
					new SQLParameter() { ParamName = "@WFTemplateID", ParamValue = Step.Step.WFTemplateID },
					new SQLParameter() { ParamName = "@NewStatus", ParamValue = Step.Step.Status },
					new SQLParameter() { ParamName = "@WFTemplateStepID", ParamValue = Step.Step.WFTemplateStepID }
				);
			}

			//temporarily replace command so we can run a different one
			//run updates in SQL
			SqlCommand command = new SqlCommand("RunUpdates", TransData.SQLConn);
			command.Transaction = TransData.Transaction;
			TransData.Command = command;
			SQLConnHandler.ExecuteNonQuerySPTrans(TransData);

			SQLConnHandler.EndTransaction(TransData);

		}
	}
}
