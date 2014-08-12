using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValkyrieWorkflowEngineLibrary.StepHandler
{
    public class ValkProcessorStepHandler
    {
		protected string _HandlesStep = "";
		public string HandlesStep { get { return _HandlesStep; } private set { } }
        public virtual void HandleStep(ref ValkQueueWFMessage WFMessage)
        {
			//Default behavior.  Please override and do something useful instead.
            WFMessage.Command = ValkQueueWFMessage.WFCommand.WFC_COMPLETE;
            //Console.WriteLine("Default Behavior: Autocompleting step");
        }
    }
}
