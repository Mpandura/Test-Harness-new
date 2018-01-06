/////////////////////////////////////////////////////////////////////
// Client.cs - sends TestRequests, displays results                //
// author : Manjunath Panduranga                                    //
// source: Jim Fawcett,                                             //                                                                 //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Almost no functionality now.  Will be expanded to make 
 * Queries into Repository for Logs and Libraries.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
//using Utilities;
using FileTransferService;
using System.Xml.Linq;
using System.IO;
using System.ServiceModel;
namespace TestHarness
{
    public class Client : IClient
    {
       
        public SWTools.BlockingQueue<string> inQ_ { get; set; }
        private ITestHarness th_ = null;
        private IRepository repo_ = null;
        public Client(ITestHarness th)
        {
            th_ = th;
        }
        public void setRepository(IRepository repo)
        {
            repo_ = repo;
        }

        public void sendTestRequest(Message testRequest)
        {
            th_.sendTestRequest(testRequest);
        }
        public void sendResults(Message results)
        {
            RLog.write("\n  Client received results message:");
            RLog.write("\n  " + results.ToString());
            RLog.putLine();
        }
        public void makeQuery(string queryText)
        {
            RLog.write("\n  Results of client query for \"" + queryText + "\"");
            if (repo_ == null)
                return;
            List<string> files = repo_.queryLogs(queryText);
            RLog.write("\n  first 10 reponses to query \"" + queryText + "\"");
            for (int i = 0; i < 10; ++i)
            {
                if (i == files.Count())
                    break;
                RLog.write("\n  " + files[i]);
            }
        }
        void sendfile()
        {

            IFileService fs = null;
            int length = 0;
            string url = "http://localhost:8080/FileService";

            Console.Write("\n  Client of File Transfer Service");
            Console.Write("\n =================================\n");


            while (true)
            {
                try
                {
                    fs = CreateChannel(url);
                    break;
                }
                catch
                {
                    Console.Write("\n  connection to service failed {0} times", ++length);
                    Thread.Sleep(500);
                    continue;
                }
            }
            Console.Write("\n  Connected to {0}\n", url);
            string relativeFilePath = "Sending";


            string filepath = Path.GetFullPath(relativeFilePath);
            Console.Write("\n  retrieving files from\n  {0}\n", filepath);
            string[] files = Directory.GetFiles(filepath);
            foreach (string file in files)
            {
                string filename = Path.GetFileName(file);
                Console.Write("\n  sending file {0}", filename);

                if (!sending(fs, file))
                    Console.Write("\n  could not send file");
            }
            Console.Write("\n\n");

        }

        public Comm<Client> comm { get; set; } = new Comm<Client>();
        public string endPoint { get; } = Comm<Client>.makeEndPoint("http://localhost", 8081);
        private Thread rcvThread = null;

        //----< initialize receiver >------------------------------------

        public Client()
        {
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread = comm.rcvr.start(rcvThreadProc);
        }
        //----< join receive thread >------------------------------------

        public void wait()
        {
            rcvThread.Join();
        }
        //----< construct a basic message >------------------------------

        public Message makeMessage(string author, string fromEndPoint, string toEndPoint)
        {
            Message msg = new Message();
            msg.author = author;
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            return msg;
        }

        public string m = "\n";
        void rcvThreadProc()
        {

            while (true)
            {
                Message msg = comm.rcvr.GetMessage();
                msg.time = DateTime.Now;
                m += "\n  " + comm.name + " sent message:";
                m += msg.showMsg();
                if (msg.body == "quit")
                    break;
            }
        }
        public Message buildTestMessage()
        {
            Message msg = new Message();
            msg.to = "TH";
            msg.from = "CL";
            msg.author = "Manjunath";
            testElement te1 = new testElement("test1");
            te1.addDriver("anothertestdriver.dll");
            te1.addCode("anothertestcode.dll");
            testElement te2 = new testElement("test2");
            te2.addDriver("td1.dll");
            te2.addCode("tc1.dll");
            testElement te3 = new testElement("test3");
            te3.addDriver("testdriver.dll");
            te3.addCode("testedcode.dll");
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
        //----< use private service method to receive a message >--------
      
        public string messge()
        {
            string str_ = "test 1";
            Console.Write("\n  sending, from repository, \"{0}\"", str_);
            return str_;
        }
        static IFileService CreateChannel(string url)
        {
            WSHttpBinding binding = new WSHttpBinding();
            EndpointAddress address = new EndpointAddress(url);
            ChannelFactory<IFileService> factory = new ChannelFactory<IFileService>(binding, address);
            return factory.CreateChannel();
        }

        static bool sending(IFileService service, string file)
        {
            long Size = 1000;
            try
            {
                string name = Path.GetFileName(file);
                service.OpenFileForWrite(name);
                FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read);
                int bytesRead = 0;
                while (true)
                {
                    long remainder = (int)(fs.Length - fs.Position);
                    if (remainder == 0) break;
                    long size_ = Math.Min(Size, remainder);
                    byte[] block = new byte[size_];
                    bytesRead = fs.Read(block, 0, block.Length);
                    service.WriteFileBlock(block);
                }
                fs.Close();
                service.CloseFile();
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n  can't open {0} for writing - {1}", file, ex.Message);
                return false;
            }
        }
       


       
        Message results()
        {
            Message trMsg = new Message();
            trMsg.author = "TestHarness";
            trMsg.to = "CL";
            trMsg.from = "TH";
            XDocument doc = new XDocument();
            XElement root = new XElement("testResultsMsg");
            doc.Add(root);
            XElement testKey = new XElement("testKey");
            root.Add(testKey);
            XElement timeStamp = new XElement("timeStamp");
            root.Add(timeStamp);
            XElement testResults = new XElement("testResults");
            root.Add(testResults);
            XElement testResult3 = new XElement("testResult");
            testResults.Add(testResult3);
            XElement testName3 = new XElement("testName");
            testName3.Value = "test1";
            testResult3.Add(testName3);
            XElement result3 = new XElement("result");
            result3.Value = "passed";
            testResult3.Add(result3);
            XElement log3 = new XElement("log");
            log3.Value = "demo test that passes";
            testResult3.Add(log3);
            XElement testResult1 = new XElement("testResult");
            testResults.Add(testResult1);
            XElement testName1 = new XElement("testName");
            testName1.Value = "test2";
            testResult1.Add(testName1);
            XElement result1 = new XElement("result");
            result1.Value = "failed";
            testResult1.Add(result1);
            XElement log1 = new XElement("log");
            log1.Value = "file not loaded";
            testResult1.Add(log1);
            XElement testResult2 = new XElement("testResult");
            testResults.Add(testResult2);
            XElement testName2 = new XElement("testName");
            testName2.Value = "test3";
            testResult2.Add(testName2);
            XElement result2 = new XElement("result");
            result2.Value = "failed";
            testResult2.Add(result2);
            XElement log2 = new XElement("log");
            log2.Value = "demo test that fails";
            testResult2.Add(log2);
            trMsg.body = doc.ToString();
            return trMsg;
        }
        public void showMsg(Message msg)
        {
            Console.Write("\n  formatted message:");
            string[] lines = msg.ToString().Split(new char[] { ',' });
            foreach (string line in lines)
            {
                Console.Write("\n    {0}", line.Trim());
            }
            Console.WriteLine();
        }
       
        static void Main(string[] args)
        {
            int length = 0;
            Client client = new Client();
            Console.Write("\n  Testing Client Demo");
            string url = "http://localhost:8080/FileService";
            Console.Write("\n  Client of File Transfer Service");
            Console.Write("\n =================================\n");

            IFileService fs = null;
            
            while (true)
            {
                try
                {
                    fs = CreateChannel(url);
                    break;
                }
                catch
                {
                    Console.Write("\n  connection to service failed {0} times - trying again", ++length);
                    Thread.Sleep(500);
                    continue;
                }
            }
            Console.Write("\n  Connected to {0}\n", url);
            string storedlocation = "Sending";
          

            string location = Path.GetFullPath(storedlocation);
            Console.Write("\n  retrieving files from\n  {0}\n", location);
            string[] files = Directory.GetFiles(location);
            foreach (string file in files)
            {
                string filename = Path.GetFileName(file);
                Console.Write("\n  sending file {0}", filename);
                if (!sending(fs, file))
                    Console.Write("\n  could not send file");
            }
            Console.Write("\n\n");
            Message msg = client.makeMessage("Manjunath", client.endPoint, client.endPoint);     msg = client.makeMessage("Manjunath", client.endPoint, client.endPoint);
            msg.body = MessageTest.makeTestRequest();
            client.comm.sndr.PostMessage(msg);
            string remoteEndPoint = Comm<Client>.makeEndPoint("http://localhost", 8080);
            msg = msg.copy_();
            msg.to = remoteEndPoint;
            client.comm.sndr.PostMessage(msg);
            Console.ReadKey();
            Message msg1 = client.results();
            client.showMsg(msg1);
            Console.Write("\n  received query: \"{0}\"", client.messge());
            msg.time = DateTime.Now;
            client.m += "\n  " + client.comm.name + " sent message:";
            client.m += msg.showMsg();
            Console.WriteLine("{0}", client.m);
            client.wait();
        }
    }
}
