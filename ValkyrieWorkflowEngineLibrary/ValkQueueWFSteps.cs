using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZMQ;
using ZMQ.ZMQExt;
using System.Threading;
using System.Diagnostics;
using ValkyrieWorkflowEngineLibrary.Database.SQLServer;
using ValkyrieWorkflowEngineLibrary.Database;

namespace ValkyrieWorkflowEngineLibrary
{
	/// <summary>
	/// Class responsible for handling the processing of individual workflow steps
	/// </summary>
	public class ValkQueueWFSteps
	{
		public static Context QueueContext = null;
		private static bool Stop = true;
		private static bool Exit = false;

		private static Socket QueueComm;
		private static Socket QueueControllerIntraComm;
		private static Socket QueueDBHandlerComm;
		//this would be more effecient as a tree of wf_step_function names 
		// so that we could lookup requests quickly
		static List<ValkWFStep> ActiveSteps = new List<ValkWFStep>();
		//wf_id, wf_step_id, wf_instanceid
		static SortedDictionary<int, SortedDictionary<int, SortedDictionary<string, ValkWFStep>>> StepsWithSyncWait
			= new SortedDictionary<int, SortedDictionary<int, SortedDictionary<string, ValkWFStep>>>();

		private static Thread DBHandler;
		public static void RunQueue(IDatabaseHandler dbHandler)
		{
			if (QueueContext == null)
				throw new System.Exception("PollContext not set");
			QueueComm = QueueContext.Socket(SocketType.REP);
			//Bind the comm to an inprocess channel and a TCP channel
			QueueComm.Bind("tcp://*:5000");
			QueueComm.Bind("inproc://queueself");
			QueueControllerIntraComm = QueueContext.Socket(SocketType.REP);
			QueueControllerIntraComm.Connect("inproc://queuecontrol");

			//if the DB handler is ever moved outside of this process, it should be responsible for bind
			// rather than this class
			QueueDBHandlerComm = QueueContext.Socket(SocketType.REQ);
			QueueDBHandlerComm.Bind("inproc://dbhandler");


			PollItem[] ControlPoller = new PollItem[1];
			ControlPoller[0] = QueueControllerIntraComm.CreatePollItem(IOMultiPlex.POLLIN);
			ControlPoller[0].PollInHandler += new PollHandler(ValkQueueWFStepsCTRL_PollInHandler);
			PollItem[] QueuePoller = new PollItem[1];
			QueuePoller[0] = QueueComm.CreatePollItem(IOMultiPlex.POLLIN);
			QueuePoller[0].PollInHandler += new PollHandler(ValkQueueWFSteps_PollInHandler);

			//setup DB handler
			ValkQueueDBHandler.DBHandlerContext = QueueContext;
			DBHandler = new Thread(() => ValkQueueDBHandler.RunDBHandler(dbHandler));
			DBHandler.Start();


			//main loop
			while (!Exit)
			{
				//we may want a way to shut this down...
				QueueContext.Poll(ControlPoller, 1);
				if (!Stop)
				{
					//block for a little while...
					QueueContext.Poll(QueuePoller, 1000);
				}
			}
		}
		static ValkQueueWFMessage GetNextStep(ValkQueueWFMessage WFMessage)
		{
			ValkQueueWFMessage ReplyMessage = new ValkQueueWFMessage();
			ReplyMessage.Command = ValkQueueWFMessage.WFCommand.WFC_WAIT;
			bool stepfound = false;
			int i = 0;
			//cycle through all of our current steps until we find an active one
			//todo: provide an actual queue of active steps so we don't have to go searching
			//      we should be adding to "active" as they come in, and moving to a list for "pending"
			//      once processing starts
			while (stepfound == false && i < ActiveSteps.Count)
			{
				if (ActiveSteps[i].Status == "active")
				{
					ActiveSteps[i].Status = "pending";
					ReplyMessage.Step = ActiveSteps[i].DeepClone();
					ReplyMessage.InstanceKey = ReplyMessage.Step.InstanceKey;
					ReplyMessage.Command = ValkQueueWFMessage.WFCommand.WFC_LOAD;
					stepfound = true;
				}
				i++;
			}
			return ReplyMessage;
		}
		static void ValkQueueWFSteps_PollInHandler(Socket socket, IOMultiPlex revents)
		{
			string reply = "";
			ValkQueueWFMessage WFMessage = socket.Recv<ValkQueueWFMessage>();
			switch (WFMessage.Command)
			{
				case ValkQueueWFMessage.WFCommand.WFC_WAIT:
					//great, don't do anything, probably a warning
					socket.Send("OK", Encoding.Unicode);
					break;
				case ValkQueueWFMessage.WFCommand.WFC_LOAD:
					//warning, we don't handle this
					socket.Send("OK", Encoding.Unicode);
					break;
				case ValkQueueWFMessage.WFCommand.WFC_CANPROCESS:
					//this will be a big one...
					socket.Send<ValkQueueWFMessage>(GetNextStep(WFMessage));
					break;
				case ValkQueueWFMessage.WFCommand.WFC_ACTIVATE:
					//set first step to active, insert neededness
					WFMessage.Step.Status = "active";
					QueueDBHandlerComm.Send<ValkQueueWFMessage>(WFMessage);
					reply = QueueDBHandlerComm.Recv(Encoding.Unicode);
					socket.Send("OK", Encoding.Unicode);
					ActiveSteps.Add(WFMessage.Step);

					break;
				case ValkQueueWFMessage.WFCommand.WFC_COMPLETE:
					//should also try to pick up the next step here, no reason to waste cycles
					WFMessage.Step.Status = "complete";
					QueueDBHandlerComm.Send<ValkQueueWFMessage>(WFMessage);
					reply = QueueDBHandlerComm.Recv(Encoding.Unicode);
					HandleComplete(WFMessage.Step);
					ValkQueueWFMessage NewMessage = new ValkQueueWFMessage();
					NewMessage.Command = ValkQueueWFMessage.WFCommand.WFC_CANPROCESS;
					socket.Send<ValkQueueWFMessage>(GetNextStep(NewMessage));

					break;
				case ValkQueueWFMessage.WFCommand.WFC_EXCEPTION:
					break;
				case ValkQueueWFMessage.WFCommand.WFC_STOP:
					socket.Send("OK", Encoding.Unicode);
					break;
				case ValkQueueWFMessage.WFCommand.WFC_SUBFLOW:
					socket.Send("OK", Encoding.Unicode);
					break;
				default:
					break;
			}
		}
		static void HandleComplete(ValkWFStep Step)
		{
			string reply = "";
			for (int i = 0; i < Step.NextSteps.Count; i++)
			{
				ValkWFStep NextStep = Step.NextSteps[i];
				if (!NextStep.Skip)
				{
					if (NextStep.SyncCount > 1)
					{
						UpdateSyncCounts(NextStep);
					}
					else
					{
						//no waiting!
						ValkQueueWFMessage WFMessage = new ValkQueueWFMessage();
						NextStep.Status = "active";
						WFMessage.Command = ValkQueueWFMessage.WFCommand.WFC_ACTIVATE;
						WFMessage.Step = NextStep;
						WFMessage.InstanceKey = NextStep.InstanceKey;
						QueueDBHandlerComm.Send<ValkQueueWFMessage>(WFMessage);
						reply = QueueDBHandlerComm.Recv(Encoding.Unicode);
						ActiveSteps.Add(NextStep);
					}
				}
				else
				{
					//we may need to update wait counts here
					//update database
					ValkQueueWFMessage WFMessage = new ValkQueueWFMessage();
					NextStep.Status = "skip";
					WFMessage.Step = NextStep;
					WFMessage.InstanceKey = NextStep.InstanceKey;
					WFMessage.Command = ValkQueueWFMessage.WFCommand.WFC_SKIP;
					QueueDBHandlerComm.Send<ValkQueueWFMessage>(WFMessage);
					reply = QueueDBHandlerComm.Recv(Encoding.Unicode);

					//TODO: update sync counts of children, recursively, will need to look for new activates as well
					//probably doesn't need to be recursive because the system will handle it as the children complete
					//will need to recurse (or re-insert) for children where this is the only parent to set them to skip
					// use inproc://queueself and create a new message channel to send messages directly back to itself?
					// --this won't work, because they will wait for (or expect) a reply on the other side, but its one thread
				}
				Step.NextSteps[i] = NextStep;
			}
			//todo remember to remove from active list
			for (int i = 0; i < ActiveSteps.Count; i++)
			{
				if (ActiveSteps[i].WFTemplateID == Step.WFTemplateID
					&& ActiveSteps[i].WFTemplateStepID == Step.WFTemplateStepID
					&& ActiveSteps[i].InstanceKey == Step.InstanceKey)
				{
					ActiveSteps.RemoveAt(i);
					//step back since the count changed and the next one will be one behind
					i--;
				}
			}
		}
		static void UpdateSyncCounts(ValkWFStep NextStep)
		{
			string reply = "";
			NextStep.SyncCount--;
			//TODO: update sync count in the DB
			if (SyncWaitExists(NextStep))
			{
				StepsWithSyncWait[NextStep.WFTemplateID][NextStep.WFTemplateStepID][NextStep.InstanceKey].SyncCount--;

				//if we're no longer waiting, set this step to active
				if (StepsWithSyncWait[NextStep.WFTemplateID][NextStep.WFTemplateStepID][NextStep.InstanceKey].SyncCount <= 0)
				{
					ValkQueueWFMessage WFMessage = new ValkQueueWFMessage();
					NextStep.Status = "active";
					WFMessage.Command = ValkQueueWFMessage.WFCommand.WFC_ACTIVATE;
					WFMessage.Step = NextStep;
					WFMessage.InstanceKey = NextStep.InstanceKey;
					QueueDBHandlerComm.Send<ValkQueueWFMessage>(WFMessage);
					reply = QueueDBHandlerComm.Recv(Encoding.Unicode);
					ActiveSteps.Add(NextStep);
				}
			}
			else
			{
				if (!StepsWithSyncWait.ContainsKey(NextStep.WFTemplateID))
					StepsWithSyncWait[NextStep.WFTemplateID] = new SortedDictionary<int, SortedDictionary<string, ValkWFStep>>();
				if (!StepsWithSyncWait[NextStep.WFTemplateID].ContainsKey(NextStep.WFTemplateStepID))
					StepsWithSyncWait[NextStep.WFTemplateID][NextStep.WFTemplateStepID] = new SortedDictionary<string, ValkWFStep>();
				StepsWithSyncWait[NextStep.WFTemplateID][NextStep.WFTemplateStepID][NextStep.InstanceKey] = NextStep.DeepClone();
				StepsWithSyncWait[NextStep.WFTemplateID][NextStep.WFTemplateStepID][NextStep.InstanceKey].SyncCount--;
			}
		}
		/// <summary>
		/// Steps can be programatically skipped (not all paths in a workflow are followed)
		/// This function updates the synccount of any children to include the skipped
		/// step as "complete"
		/// </summary>
		/// <param name="Step">Step being skipped</param>
		static void UpdateSkip(ref ValkWFStep Step)
		{
			for (int i = 0; i < Step.NextSteps.Count; i++)
			{
				ValkWFStep substep = Step.NextSteps[i];
				substep.SyncCount--;
				if (substep.SyncCount == 0)
				{
					substep.Skip = true;
					UpdateSkip(ref substep);
				}
				else
				{
					UpdateSyncCounts(substep);
				}
				Step.NextSteps[i] = substep;
			}
		}
		/// <summary>
		/// utility function to determine if this step currently has any sync counts
		/// </summary>
		/// <param name="Step"></param>
		/// <returns></returns>
		static bool SyncWaitExists(ValkWFStep Step)
		{
			bool Exists = false;
			if (StepsWithSyncWait.ContainsKey(Step.WFTemplateID))
			{
				if (StepsWithSyncWait[Step.WFTemplateID].ContainsKey(Step.WFTemplateStepID))
				{
					if (StepsWithSyncWait[Step.WFTemplateID][Step.WFTemplateStepID].ContainsKey(Step.InstanceKey))
						Exists = true;
				}
			}
			return Exists;
		}
		/// <summary>
		/// Control function for this class so we can gracefully startup/pause/exit the entire system
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="revents"></param>
		static void ValkQueueWFStepsCTRL_PollInHandler(Socket socket, IOMultiPlex revents)
		{
			string Message = socket.Recv(Encoding.Unicode);
			Console.WriteLine(Message);
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
				ValkQueueWFMessage ExitMessage = new ValkQueueWFMessage();
				ExitMessage.Command = ValkQueueWFMessage.WFCommand.WFC_EXIT;
				QueueDBHandlerComm.Send<ValkQueueWFMessage>(ExitMessage);
				string mesg = QueueDBHandlerComm.Recv(Encoding.Unicode);
				DBHandler.Join();
				Stop = true;
				Exit = true;
			}
			socket.Send("OK:" + Message, Encoding.Unicode);
		}
	}
}
