using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValkyrieWorkflowEngineLibrary.Database
{
	public class WFTemplateLoadingData
	{
		public SortedDictionary<string, int> LoadedActiveInstanceByType { get; set; }
		public List<ValkWFStep> WFsToLoad { get; set; }
		public WFTemplateLoadingData()
		{
			LoadedActiveInstanceByType = new SortedDictionary<string, int>();
			WFsToLoad = new List<ValkWFStep>();
		}
	}
}
