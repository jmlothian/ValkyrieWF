using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ValkyrieWorkflowEngineLibrary.Database;
using ZMQ;
using ZMQ.ZMQExt;

namespace ValkyrieWorkflowEngineLibrary
{
	/// <summary>
	/// The primary class used to instantiate new workflows from a template
	/// </summary>
	public class ValkWFActivator
	{
		public static Context ActContext = null;
		private static bool Stop = true;
		private static bool Exit = false;

		private static Socket ActivatorIntraComm;
		private static Socket ActivatorControllerIntraComm;
		private static Socket QueueComm;

		private static Dictionary<int, ValkWF> WorkFlowTemplates = new Dictionary<int, ValkWF>();

		//this will keep track of all the instances we've loaded, so that we can ignore duplicate requests
		//list item is the instance_id, since it should be unique and fast to access;
		private static List<int> LoadedInstances = new List<int>();
		//all the loaded templates, index by wf_id
		private static SortedDictionary<int, ValkWFStep> LoadedInstanceTemplates = new SortedDictionary<int, ValkWFStep>();
		//all the -active- templates, indexed by type
		private static SortedDictionary<string, int> LoadedActiveInstanceByType = new SortedDictionary<string, int>();

		private static IDatabaseHandler dbHandler { get; set; }

		public static void ActivateResponder(IDatabaseHandler _dbHandler)
		{
			dbHandler = _dbHandler;
			if (ActContext == null)
				throw new System.Exception("ActContext not set");
			ActivatorIntraComm = ActContext.Socket(SocketType.REP);
			ActivatorIntraComm.Bind("inproc://activator");
			ActivatorIntraComm.Bind("tcp://*:5001");
			ActivatorControllerIntraComm = ActContext.Socket(SocketType.REP);
			ActivatorControllerIntraComm.Connect("inproc://activatorcontrol");
			QueueComm = ActContext.Socket(SocketType.REQ);
			QueueComm.Connect("tcp://127.0.0.1:5000");


			PollItem[] Poller = new PollItem[1];
			PollItem[] ControlPoller = new PollItem[1];
			Poller[0] = ActivatorIntraComm.CreatePollItem(IOMultiPlex.POLLIN);
			Poller[0].PollInHandler += new PollHandler(ValkWFActivator_PollInHandler);
			ControlPoller[0] = ActivatorControllerIntraComm.CreatePollItem(IOMultiPlex.POLLIN);
			ControlPoller[0].PollInHandler += new PollHandler(ValkWFActivatorCTRL_PollInHandler);

			//load workflow templates
			LoadWorkflowTemplates(dbHandler);

			//main loop
			while (!Exit)
			{
				//we may want a way to shut this down...
				ActContext.Poll(ControlPoller, 1);
				if (!Stop)
				{
					//block for a little while...
					ActContext.Poll(Poller, 1000);
				}
			}
		}
		private static void LoadWorkflowTemplates(IDatabaseHandler dbHandler)
		{
			//WorkFlowTemplates
			//list to store loads to help reduce recursive SQL queries
			WFTemplateLoadingData loadingData = dbHandler.LoadWFTemplates();
			LoadedActiveInstanceByType = loadingData.LoadedActiveInstanceByType;
			for (int i = 0; i < loadingData.WFsToLoad.Count(); i++)
			{
				ValkWFStep CurrentStep = loadingData.WFsToLoad[i];
				LoadChildren(CurrentStep, dbHandler);
				loadingData.WFsToLoad[i] = CurrentStep;
				LoadedInstanceTemplates[loadingData.WFsToLoad[i].WFTemplateID] = loadingData.WFsToLoad[i];
			}
			Console.WriteLine("Workflows Loaded");
		}
		private static void LoadChildren(ValkWFStep CurrentStep, IDatabaseHandler dbHandler)
		{
			List<ValkWFStep> Children = dbHandler.LoadChildSteps(CurrentStep);

			//do children loading outside, here
			for (int i = 0; i < Children.Count(); i++)
			{
				ValkWFStep substep = Children[i];
				LoadChildren(substep, dbHandler);
				Children[i] = substep;
			}
			CurrentStep.NextSteps = Children;
		}
		/// <summary>
		/// Handles control messages
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="revents"></param>
		static void ValkWFActivatorCTRL_PollInHandler(Socket socket, IOMultiPlex revents)
		{
			string Message = socket.Recv(Encoding.Unicode);
			Console.WriteLine(Message);
			socket.Send("OK:" + Message, Encoding.Unicode);
			if (Message == "GO")
			{
				Stop = false;
			}
			else if (Message == "STOP")
			{
				Stop = true;
			}
			else if (Message == "EXIT")
			{
				Stop = true;
				Exit = true;
			}

		}
		static void ValkWFActivator_PollInHandler(Socket socket, IOMultiPlex revents)
		{
			//handle workflow instantiations
			ValkQueueWFMessage WFMessage = socket.Recv<ValkQueueWFMessage>();
			if (WFMessage.Command == ValkQueueWFMessage.WFCommand.WFC_LOAD)
			{
				ValkWFStep NewInstance = null;
				//Console.WriteLine("Instance Insert Request Received: " + WFMessage.InstanceID + " " + WFMessage.InstanceKey);
				if (!LoadedInstances.Contains(WFMessage.InstanceID))
				{
					LoadedInstances.Add(WFMessage.InstanceID);
					SortedDictionary<int, ValkWFStep> ToInsert = new SortedDictionary<int, ValkWFStep>();

					dbHandler.StartWFInstance(WFMessage.InstanceID);
					if (LoadedActiveInstanceByType.ContainsKey(WFMessage.InstanceType))
					{
						NewInstance
							= LoadedInstanceTemplates[LoadedActiveInstanceByType[WFMessage.InstanceType]].DeepClone<ValkWFStep>();
						BuildInsertWFInstance(ToInsert, NewInstance, WFMessage.InstanceKey, true);
						//Console.WriteLine(DateTime.Now.ToLongTimeString() + ": Created New Instance: " + WFMessage.InstanceKey);
					}
					else
					{
						//error, no WF of this type found active
					}

					if (ToInsert.Count > 0)
					{
						InsertWFInstance(ToInsert);
						ValkQueueWFMessage QueueMessage = new ValkQueueWFMessage();
						QueueMessage.InstanceID = WFMessage.InstanceID;
						QueueMessage.InstanceKey = WFMessage.InstanceKey;
						QueueMessage.InstanceType = WFMessage.InstanceType;
						QueueMessage.Command = ValkQueueWFMessage.WFCommand.WFC_ACTIVATE;
						QueueMessage.Step = NewInstance;
						QueueComm.Send<ValkQueueWFMessage>(QueueMessage);
						string reply = QueueComm.Recv(Encoding.Unicode);
					}
					socket.Send("OK", Encoding.Unicode);
				}
				else
				{//else don't do anything, duplicate message
					socket.Send("WARN-Duplicate Request", Encoding.Unicode);
				}
			}
			else
			{
				//log error message
				socket.Send("ERROR-Wrong Request Type", Encoding.Unicode);
			}
		}
		static void InsertWFInstance(SortedDictionary<int, ValkWFStep> ToInsert)
		{
			dbHandler.InsertWFInstance(ToInsert);
		}
		/// <summary>
		/// Creates all child-steps of a workflow instance (recursively) so that the entire flow can be inserted
		/// prior to being set active.
		/// </summary>
		/// <param name="ToInsert"></param>
		/// <param name="Step"></param>
		/// <param name="InstanceID"></param>
		/// <param name="insert"></param>
		static void BuildInsertWFInstance(SortedDictionary<int, ValkWFStep> ToInsert, ValkWFStep Step, string InstanceID, bool insert)
		{
			Step.InstanceKey = InstanceID;
			if (insert && !ToInsert.ContainsKey(Step.WFTemplateStepID))
			{
				ToInsert[Step.WFTemplateStepID] = Step;
			}
			else
			{
				insert = false;
			}
			for (int i = 0; i < Step.NextSteps.Count; i++)
			{
				ValkWFStep substep = Step.NextSteps[i];
				BuildInsertWFInstance(ToInsert, substep, InstanceID, insert);
				Step.NextSteps[i] = substep;
			}
		}
	}
}
