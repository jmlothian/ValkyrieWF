using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ValkyrieWorkflowEngineLibrary;
using System.Threading;
using ValkyrieWorkflowEngineLibrary.Database.SQLServer;
using ValkyrieWorkflowEngineLibrary.StepHandler;

namespace ValkyrieGUI
{
    public partial class Form1 : Form
    {
        int insertIDAt = 0;
        ValkController ValkyrieController = new ValkController();
        public Form1()
        {
            InitializeComponent();
			//Initialize a new controller
            ValkyrieController.Initialize(new SQLServerDBHandler());
            using (QuickData Query = new QuickData())
            {
				Query.delete("delete from WFInstances");
                Query.insert("DBCC CHECKIDENT (WFInstances, RESEED, 1)");
                Query.delete("delete from StepStatus");
                Query.delete("delete from PendingStatusUpdates");
				Query.insert("DBCC CHECKIDENT (PendingStatusUpdates, RESEED, 1)");
            }
			//create a new step handler
            ValkProcessorStepHandler StepHandler = new ValkProcessorStepHandler();
			//register step handler with a static processor
            ValkProcessor.AddStepHandler(StepHandler);
			//kickoff processor
			Thread Processor = new Thread(new ThreadStart(ValkProcessor.RunProcessor));
            Processor.Start();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
			//by this point, a processor should be running.  Start the controller
			//which will start all of the sub-services and stephandler it controls
            ValkyrieController.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            ValkyrieController.Stop();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ValkyrieController.Shutdown();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled == false)
            {
                InsertOrders();
                timer1.Enabled = true;
            }
            else
            {
                timer1.Enabled = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            InsertOrders();
        }
        private void InsertOrders()
        {
            for (int i = 0; i < 100; i++)
            {
                insertIDAt++;
                using (QuickData Query = new QuickData())
                {
                    QDInserter ins = new QDInserter("WFInstances");
                    ins.insert("InstanceKey", "P" + insertIDAt.ToString(), Types.String);
                    ins.insert("Status", "pending", Types.String);
                    ins.insert("WFType", "P", Types.String);
                    Query.insert(ins);
                }
            }
			Console.WriteLine("Orders Inserted");
        }
    }
}
