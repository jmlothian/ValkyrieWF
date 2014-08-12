using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValkyrieWorkflowEngineLibrary
{
	/// <summary>
	/// This is the message class used for almost all communications between services
	/// </summary>
    [Serializable]
    public class ValkQueueWFMessage
    {
        public enum WFCommand { WFC_EXIT, WFC_WAIT, WFC_CANPROCESS, WFC_LOAD, WFC_ACTIVATE, WFC_COMPLETE, WFC_EXCEPTION, WFC_STOP, WFC_SUBFLOW, WFC_SKIP};
		public WFCommand Command { get; set; }
		public string InstanceKey { get; set; }
		public int InstanceID { get; set; }
		public string InstanceType { get; set; }
		public ValkWFStep Step { get; set; }
    }
}
