using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValkyrieWorkflowEngineLibrary
{
    [Serializable]
    public class ValkWFStep
    {
		public int WFTemplateID { get; set; }
		public int WFTemplateStepID { get; set; }
		public string InstanceKey { get; set; }
		public string Status { get; set; }
		public string StepName { get; set; }
		public string LocalStatus { get; set; } //for persistance and handshaking
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public int UserID { get; set; }
		public int SyncCount { get; set; }
		public bool Skip { get; set; }
        public List<ValkWFStep> NextSteps { get; set; }
		public ValkWFStep ParentStep { get; set; }
		public bool InException { get; set; } //is this needed?
		public string HandleCategory { get; set; }  //used to match against a service's "handled by"
		public int GroupID { get; set; }
		public ValkWFStep()
		{
			InstanceKey = null;
			Status = "inactive";
			StepName = "";
			StartTime = DateTime.MinValue;
			EndTime = DateTime.MinValue;
			UserID = 0;
			SyncCount = 0;
			Skip = false;
			ParentStep = null;
		}
    }
}
