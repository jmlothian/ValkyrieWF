using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ValkyrieWorkflowEngineLibrary.Database;
using ZMQ;
using ZMQ.ZMQExt;

namespace ValkyrieWorkflowEngineLibrary
{
	/// <summary>
	/// Checks the database every 5 seconds for items that were inserted directly.
	/// This is sub-optimal, as workflows should be inserted programmatically via the activator
	/// But for legacy systems or programs without ZeroMQ, this works.
	/// </summary>
	public class ValkWFStepPoller
	{
		public static Context PollContext = null;
		private static Socket PollerControlIntraComm;
		private static Socket ActivatorIntraComm;
		private static bool Stop = true;
		private static bool Exit = false;

		public static void StartPolling(IDatabaseHandler dbHandler)
		{
			if (PollContext == null)
				throw new System.Exception("PollContext not set");
			//  Prepare our sockets
			PollerControlIntraComm = PollContext.Socket(SocketType.REP);
			PollerControlIntraComm.Connect("inproc://pollercontrol");

			ActivatorIntraComm = PollContext.Socket(SocketType.REQ);
			ActivatorIntraComm.Connect("inproc://activator");

			PollItem[] Poller = new PollItem[1];
			Poller[0] = PollerControlIntraComm.CreatePollItem(IOMultiPlex.POLLIN);
			Poller[0].PollInHandler += new PollHandler(PollerCTRL_PollInHandler);
			while (!Exit)
			{
				PollContext.Poll(Poller, 5000);
				if (!Stop)
				{
					//poll
					//get all available pending workflow instances

					DataTable dt = dbHandler.GetPendingWFInstances();
					foreach (DataRow Row in dt.Rows)
					{
						//send to kickoff
						ValkQueueWFMessage Message = new ValkQueueWFMessage();
						Message.Command = ValkQueueWFMessage.WFCommand.WFC_LOAD;
						Message.InstanceID = int.Parse(Row["WFInstanceID"].ToString());
						Message.InstanceKey = Row["InstanceKey"].ToString();
						Message.InstanceType = Row["WFType"].ToString();
						ActivatorIntraComm.Send<ValkQueueWFMessage>(Message);
						string message = ActivatorIntraComm.Recv(Encoding.Unicode);
						//Console.WriteLine("Instance Insert Reply Received: " + message);
					}
				}
			}
		}
		private static void PollerCTRL_PollInHandler(Socket socket, IOMultiPlex revents)
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
	}
}
