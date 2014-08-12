using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZMQ;
using ZMQ.ZMQExt;
using System.Threading;
using ValkyrieWorkflowEngineLibrary.StepHandler;

namespace ValkyrieWorkflowEngineLibrary
{
	/// <summary>
	/// Processors handle steps.  There should be no need to change the processor itself, since stephandlers can be customized.
	/// Multiple Processors can be run, each handling one or more steps, allowing for a distributed workload.
	/// </summary>
    public class ValkProcessor
    {
        public static Context QueueContext = null;
        private static bool Stop = true;
        private static bool Exit = false;
        private static Socket QueueComm;
        private static SortedDictionary<string, ValkProcessorStepHandler> StepHandlers = new SortedDictionary<string, ValkProcessorStepHandler>();
        private static ValkProcessorStepHandler DefaultHandler = null;
        public static void AddStepHandler(ValkProcessorStepHandler Handler)
        {
            if(Handler.HandlesStep == "")
            {
                DefaultHandler = Handler;
            } else
            {
				StepHandlers[Handler.HandlesStep] = Handler;
            }
        }
        //public void Initialize()
        //{
        //    Thread Processor;
        //    Processor = new Thread(new ThreadStart(ValkQueueWFSteps.RunQueue));
        //    Processor.Start();
        //}
        public static void RunProcessor()
        {
            if (QueueContext == null)
            {
                QueueContext = new Context(1);
            }
                //throw new System.Exception("QueueContext not set");
            QueueComm = QueueContext.Socket(SocketType.REQ);
            QueueComm.Connect("tcp://127.0.0.1:5000");
            /*
            ActivatorIntraComm.Bind("tcp://*:5001");
            ActivatorControllerIntraComm = ActContext.Socket(SocketType.REP);
            ActivatorControllerIntraComm.Connect("inproc://activatorcontrol");
            QueueComm = ActContext.Socket(SocketType.REQ);
            QueueComm.Connect("tcp://127.0.0.1:5000");
             * */
            ValkQueueWFMessage WFMessage = new ValkQueueWFMessage();
            WFMessage.Command = ValkQueueWFMessage.WFCommand.WFC_CANPROCESS;
            while (true)
            {

                try
                {
                    //Thread.Sleep(10000);
                    QueueComm.Send<ValkQueueWFMessage>(WFMessage);
                    WFMessage = QueueComm.Recv<ValkQueueWFMessage>();
                }
                catch (ZMQ.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                if (WFMessage.Command == ValkQueueWFMessage.WFCommand.WFC_WAIT)
                {
                    Thread.Sleep(5000);
                    WFMessage.Command = ValkQueueWFMessage.WFCommand.WFC_CANPROCESS;
                }
                else
                {
                    if(WFMessage.Command != ValkQueueWFMessage.WFCommand.WFC_CANPROCESS)
                        ProcessData(ref WFMessage);
                }
            }
        }
        public static void ProcessData(ref ValkQueueWFMessage WFMessage)
        {
            //default, automatically complete everything
            if (StepHandlers.ContainsKey(WFMessage.Step.HandleCategory))
            {
                StepHandlers[WFMessage.Step.HandleCategory].HandleStep(ref WFMessage);
            }
            else
            {
                if (DefaultHandler == null)
                {
                    Console.WriteLine("No handler for " + WFMessage.Step.HandleCategory + " and no default provided");
                }
                else
                {
                    DefaultHandler.HandleStep(ref WFMessage);
                }
            }
        }
    }
}
