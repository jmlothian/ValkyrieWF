using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ValkyrieWorkflowEngineLibrary.Database;
using ZMQ;
using ZMQ.ZMQExt;

namespace ValkyrieWorkflowEngineLibrary
{
	/// <summary>
	/// This class is responsible for persisting updates to the database backend to keep in sync with
	/// changes to live data
	/// </summary>
	/// <remarks>
	/// It would be a good idea in the future to handle database-level errors better here
	/// and in implementations of the IDatabaseHandler
	/// </remarks>
	public class ValkQueueDBHandler
	{
		public static Context DBHandlerContext = null;
		private static Socket QueueDBHandlerComm;
		private static bool Exit = false;
		private static List<ValkQueueWFMessage> StepsToUpdate = new List<ValkQueueWFMessage>();
		static Stopwatch TotalWatch = new Stopwatch();
		private static int TotalSteps = 0;
		private static IDatabaseHandler dbHandler { get; set; }

		public static void RunDBHandler(IDatabaseHandler _dbHandler)
		{
			dbHandler = _dbHandler;
			//if the DB handler is ever moved outside of this process, it should be responsible for bind
			// rather than this connect
			QueueDBHandlerComm = DBHandlerContext.Socket(SocketType.REP);
			QueueDBHandlerComm.Connect("inproc://dbhandler");

			PollItem[] ControlPoller = new PollItem[1];
			ControlPoller[0] = QueueDBHandlerComm.CreatePollItem(IOMultiPlex.POLLIN);
			ControlPoller[0].PollInHandler += new PollHandler(ValkQueueDBHandler_PollInHandler);

			Stopwatch Watch = new Stopwatch();
			TotalWatch.Start();
			Watch.Start();
			while (!Exit)
			{
				//reasonable to block for a second
				DBHandlerContext.Poll(ControlPoller, 1000);
				//get 5 seconds worth of DB entries
				if (StepsToUpdate.Count > 0 && (Watch.Elapsed.Seconds >= 5 || StepsToUpdate.Count > 1000))
				{
					TotalSteps += StepsToUpdate.Count();
					Console.WriteLine(StepsToUpdate.Count.ToString() + "," + Watch.Elapsed.TotalSeconds + "," + TotalSteps + "," + TotalWatch.Elapsed.TotalSeconds);
					DoUpdates();
					Watch.Stop();
					Watch.Reset();
					Watch.Start();
				}
			}
		}
		static void DoUpdates()
		{
			dbHandler.UpdateSteps(StepsToUpdate);
			StepsToUpdate.Clear();
		}
		static void ValkQueueDBHandler_PollInHandler(Socket socket, IOMultiPlex revents)
		{
			ValkQueueWFMessage WFMessage = socket.Recv<ValkQueueWFMessage>();
			switch (WFMessage.Command)
			{
				case ValkQueueWFMessage.WFCommand.WFC_EXIT:
					socket.Send("OK", Encoding.Unicode);
					Exit = true;
					break;
				case ValkQueueWFMessage.WFCommand.WFC_ACTIVATE:
					StepsToUpdate.Add(WFMessage);
					socket.Send("OK", Encoding.Unicode);
					break;
				case ValkQueueWFMessage.WFCommand.WFC_COMPLETE:
					StepsToUpdate.Add(WFMessage);
					socket.Send("OK", Encoding.Unicode);
					break;
				case ValkQueueWFMessage.WFCommand.WFC_SKIP:
					StepsToUpdate.Add(WFMessage);
					socket.Send("OK", Encoding.Unicode);
					break;
				case ValkQueueWFMessage.WFCommand.WFC_EXCEPTION:
					break;
				case ValkQueueWFMessage.WFCommand.WFC_STOP:
					break;
				case ValkQueueWFMessage.WFCommand.WFC_SUBFLOW:
					break;
				default:
					break;
			}
		}
	}
}
