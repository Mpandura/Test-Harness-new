/////////////////////////////////////////////////////////////////////
// TestExec.cs - Demonstrate TestHarness, Client, and Repository   //
// ver 1.0                                                         //
// author : Manjunath Panduranga                                    //
// source: Jim Fawcett,                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * TestExec package orchestrates TestHarness, Client, and Repository
 * operations to show that all requirements for Project #2 have been
 * satisfied. 
 *
 * Required files:
 * ---------------
 * - TestExec.cs
 * - ITest.cs
 * - Client.cs, Repository.cs, TestHarness.cs
 * - LoadAndTest, Logger, Messages
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 16 Oct 2016
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FileTransferService;
using System.Web;
using System.IO;
using System.ServiceModel;

namespace TestHarness
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    class TestExec : IFileService
    {
        public Comm<TestExec> comm { get; set; } = new Comm<TestExec>();
        public string endPoint { get; } = Comm<TestExec>.makeEndPoint("http://localhost", 8080);
        private Thread rcvThread = null;
        public TestHarness testHarness { get; set; }
        public Client client { get; set; }
        public Repository repository { get; set; }
       
      public  TestExec()
    {
      RLog.write("\n  creating Test Executive - Req #9");
            testHarness = new TestHarness();
      client = new Client(testHarness as ITestHarness);
      testHarness.setClient(client);
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread = comm.rcvr.start(rcvThreadProc);
        }
        
        public Message makeMessage(string author, string fromEndPoint, string toEndPoint)
        {
            Message msg = new Message();
            msg.author = author;
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            return msg;
        }

        void sendTestRequest(Message testRequest)
    {
      client.sendTestRequest(testRequest);
    }
    Message buildTestMessage()
    {
      Message msg = new Message();
      msg.to = "TH";
      msg.from = "CL";
      msg.author = "Manjunath";
      testElement te1 = new testElement("test1");
      te1.addDriver("anothertestdriver.dll");
      te1.addCode("anothertestedcode.dll");
      testElement te2 = new testElement("test2");
      te2.addDriver("td1.dll");
      te2.addCode("tc1.dll");
      testElement te3 = new testElement("test3");
      te3.addDriver("testdriver.dll");
      te3.addCode("estedcode.dll");
      testElement tlg = new testElement("loggerTest");
      tlg.addDriver("logger.dll");
      testRequest tr = new testRequest();
      tr.author = "Manjunath";
      tr.tests.Add(te1);
      tr.tests.Add(te2);
      tr.tests.Add(te3);
      tr.tests.Add(tlg);
      msg.body = tr.ToString();
      return msg;
    }
        FileStream file_ = null;
        public void wait()
        {
            rcvThread.Join();
        }
        void rcvThreadProc()
        {
            while (true)
            {
                Message msg = comm.rcvr.GetMessage();
                msg.time = DateTime.Now;
                Console.Write("\n  {0} received message:", comm.name);
                msg.showMsg();
                if (msg.body == "quit")  break;
            }
        }

        string location = ".\\SentFiles";

        public void SetServerFilePath(string path)
        {
            location = path;
        }

        string dir = "";
        public bool OpenFileForWrite(string name)
        {
            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);
            dir = location + "\\" + name;
            try
            {
                file_ = File.Open(dir, FileMode.Create, FileAccess.Write);
                Console.Write("\n  opening {0}", dir);
                return true;
            }
            catch
            {
                Console.Write("\n failed opening {0} ", dir);
                return false;
            }
        }
       
        static ServiceHost CreateChannel(string url)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(url);
            Type service = typeof(TestExec);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(FileTransferService.IFileService), binding, baseAddress);
            return host;
        }
        static void Main(string[] args)
    {
      RLog.attach(RLog.makeConsoleStream());
      RLog.start();
      DLog.attach(DLog.makeConsoleStream());
      DLog.start();
      RLog.write("\n  Demonstrating TestHarness - Project #4");
      RLog.write("\n ========================================");
      TestExec te = new TestExec();
      Message msg_ = te.comm.rcvr.GetMessage();
      Message msg = te.buildTestMessage();
      te.testHarness.processMessages(msg_);
      te.comm.sndr.PostMessage(msg_);
      ServiceHost Shost = CreateChannel("http://localhost:8080/FileService1");
      Shost.Open();
      Console.Write("\n  Press key to terminate service:\n");
      Shost.Close();
      Console.Write("\n Requirement #9 ");          
      te.client.makeQuery("test1");
      DLog.flush();
      RLog.flush();
      Console.Write("\n  press key to exit");
      Console.ReadKey();
      DLog.stop();
      RLog.stop();
    }



        public bool WriteFileBlock(byte[] block)
        {
            try
            {
                Console.Write("\n  writing block with {0} bytes", block.Length);
                file_.Write(block, 0, block.Length);
                file_.Flush();
                return true;
            }
            catch { return false; }
        }
        public bool CloseFile()
        {
            try
            {
                file_.Close();
                Console.Write("\n  {0} is closed", dir);
                return true;

            }
            catch
            {
                return false;

            }
        }
    }
}
