using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZMQ;
using ZMQ.ZMQExt;
using System.Threading;
using ValkyrieWorkflowEngineLibrary.Database.SQLServer;
using System.Data;
using ValkyrieWorkflowEngineLibrary.Database;

namespace ValkyrieWorkflowEngineLibrary
{
	/// <summary>
	/// The ValkController orchestrates interactions and setup/teardown between the
	/// other services.
	/// </summary>
	public class ValkController
	{
		Thread Poller;
		Thread Activator;
		Thread WFStepQueue;
		Context GlobalContext;
		private Socket PollerControlIntraComm;
		private Socket ActivatorControlIntraComm;
		private Socket QueueControlIntraComm;

		public void Initialize(IDatabaseHandler dbHandler)
		{
			GlobalContext = new Context(1);
			ValkWFStepPoller.PollContext = GlobalContext;
			ValkWFActivator.ActContext = GlobalContext;
			ValkQueueWFSteps.QueueContext = GlobalContext;
			//setup our inprocess communication
			ActivatorControlIntraComm = GlobalContext.Socket(SocketType.REQ);
			ActivatorControlIntraComm.Bind("inproc://activatorcontrol");
			//poller requires activator, goes after
			PollerControlIntraComm = GlobalContext.Socket(SocketType.REQ);
			PollerControlIntraComm.Bind("inproc://pollercontrol");

			QueueControlIntraComm = GlobalContext.Socket(SocketType.REQ);
			QueueControlIntraComm.Bind("inproc://queuecontrol");

			//anonymous function to pass in dbhandler without parameterizedthreadstart obscurities
			WFStepQueue = new Thread(() => ValkQueueWFSteps.RunQueue(dbHandler));
			WFStepQueue.Start();

			Activator = new Thread(() => ValkWFActivator.ActivateResponder(dbHandler));
			Activator.Start();

			Poller = new Thread(() => ValkWFStepPoller.StartPolling(dbHandler));
			Poller.Start();
		}

		public void Start()
		{
			PollerControlIntraComm.Send("GO", Encoding.Unicode);
			string Message = PollerControlIntraComm.Recv(Encoding.Unicode);   //we dont actually care about what was recieved, only that it happened
			Console.WriteLine("Confirmed: " + Message);
			ActivatorControlIntraComm.Send("GO", Encoding.Unicode);
			Message = ActivatorControlIntraComm.Recv(Encoding.Unicode);   //we dont actually care about what was recieved, only that it happened
			Console.WriteLine("Confirmed: " + Message);
			QueueControlIntraComm.Send("GO", Encoding.Unicode);
			Message = QueueControlIntraComm.Recv(Encoding.Unicode);   //we dont actually care about what was recieved, only that it happened
			Console.WriteLine("Confirmed: " + Message);

		}
		public void Stop()
		{
			PollerControlIntraComm.Send("STOP", Encoding.Unicode);
			string Message = PollerControlIntraComm.Recv(Encoding.Unicode);   //we dont actually care about what was recieved, only that it happened
			Console.WriteLine("Confirmed: " + Message);
			ActivatorControlIntraComm.Send("STOP", Encoding.Unicode);
			Message = ActivatorControlIntraComm.Recv(Encoding.Unicode);   //we dont actually care about what was recieved, only that it happened
			Console.WriteLine("Confirmed: " + Message);
			QueueControlIntraComm.Send("STOP", Encoding.Unicode);
			Message = QueueControlIntraComm.Recv(Encoding.Unicode);   //we dont actually care about what was recieved, only that it happened
			Console.WriteLine("Confirmed: " + Message);

		}
		void StartStepControlQueue()
		{
		}
		public void Shutdown()
		{
			GlobalContext = null;
			PollerControlIntraComm.Send("EXIT", Encoding.Unicode);
			string Message = PollerControlIntraComm.Recv(Encoding.Unicode);   //we dont actually care about what was recieved, only that it happened
			Console.WriteLine("Confirmed: " + Message);
			ActivatorControlIntraComm.Send("EXIT", Encoding.Unicode);
			Message = ActivatorControlIntraComm.Recv(Encoding.Unicode);   //we dont actually care about what was recieved, only that it happened
			Console.WriteLine("Confirmed: " + Message);
			QueueControlIntraComm.Send("EXIT", Encoding.Unicode);
			Message = QueueControlIntraComm.Recv(Encoding.Unicode);   //we dont actually care about what was recieved, only that it happened
			Console.WriteLine("Confirmed: " + Message);

			WFStepQueue.Join();
			Activator.Join();
			Poller.Join();
		}
		//this should be disable-able, it is here only for legacy systems
		static void Poll()
		{
			//bool Stop = false;
			using (Context PollContext = new Context(1))
			{
				//setup inproc connection
				/*
				pollitem_t 
				Socket Receiver = ((Context)PollContext).Socket(SocketType.REP);
				Receiver.tim
				Receiver.Connect("inproc://workers");

				while (true) //this condition
				{
					while(Stop)
					{
						// wait for startup
						string Message = Receiver.Recv(Encoding.Unicode);
						if (Message == "GO")
						{
							Stop = false;
							Receiver.Send("GO:OK", Encoding.Unicode);
						}
					}
					while (!Stop)
					{

						//poll
						Thread.Sleep(10000);
					}
				}
				using (Socket worker = PollContext.Socket(SocketType.DEALER))
				{
					worker.StringToIdentity("B", Encoding.Unicode);
					worker.Connect("tcp://localhost:5555");
					int total = 0;
					while (true)
					{
						string request = worker.Recv(Encoding.Unicode);
						if (request.Equals("END"))
						{
							Console.WriteLine("B Received: {0}", total);
							break;
						}
						total++;
					}
				}
				 */
			}
		}
	}
}
