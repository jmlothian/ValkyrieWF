using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ValkyrieWorkflowEngineLibrary.Database
{
	/// <summary>
	/// Database handling interface.  Provided so developers can implement their own backend systems.
	/// </summary>
	public interface IDatabaseHandler
	{
		/// <summary>
		/// Gets the pending workflow instances
		/// </summary>
		/// <returns></returns>
		DataTable GetPendingWFInstances();
		/// <summary>
		/// Loads WF templates so they can be instantiated
		/// </summary>
		/// <returns></returns>
		WFTemplateLoadingData LoadWFTemplates();
		/// <summary>
		/// Given a current step, returns all children
		/// </summary>
		/// <param name="CurrentStep"></param>
		/// <returns></returns>
		List<ValkWFStep> LoadChildSteps(ValkWFStep CurrentStep);
		/// <summary>
		/// Marks a workflow instance as "active" in the database
		/// </summary>
		/// <param name="WFInstanceID"></param>
		void StartWFInstance(int WFInstanceID);
		/// <summary>
		/// Inserts one or more new workflow instances into the database but does not set them to active.
		/// </summary>
		/// <param name="ToInsert"></param>
		void InsertWFInstance(SortedDictionary<int, ValkWFStep> ToInsert);
		/// <summary>
		/// Updates the database with one or more changes to workflow steps
		/// </summary>
		/// <param name="Updates"></param>
		void UpdateSteps(List<ValkQueueWFMessage> Updates);
	}
}
