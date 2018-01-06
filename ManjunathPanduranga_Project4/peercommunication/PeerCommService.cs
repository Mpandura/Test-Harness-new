/////////////////////////////////////////////////////////////////////
// CommService.cs - Communicator Service                           //
// author : Manjunath Panduranga                                    //
// source: Jim Fawcett,                                             //
// ver 1.0                                                         //
// Jim Fawcett, CSE681 - Software Modeling & Analysis, Summer 2011 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defindes a Sender class and Receiver class that
 * manage all of the details to set up a WCF channel.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using SWTools;

namespace TestHarness
{
    ///////////////////////////////////////////////////////////////////
    // Receiver hosts Communication service used by other Peers

    public class Receiver<T> : ICommunicator
    {
        static BlockingQueue<Message> rcvBlockingQ = null;
        ServiceHost service = null;

        public string name { get; set; }

        public Receiver()
        {
            if (rcvBlockingQ == null)
                rcvBlockingQ = new BlockingQueue<Message>();
        }

        public Thread start(ThreadStart rcvThreadProc)
        {
            Thread rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            return rcvThread;
        }

        public void Close()
        {
            service.Close();
        }

        //  Create ServiceHost for Communication service

        public void CreateRecvChannel(string address)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(address);
            service = new ServiceHost(typeof(Receiver<T>), baseAddress);
            service.AddServiceEndpoint(typeof(ICommunicator), binding, baseAddress);
            service.Open();
        }

        // Implement service method to receive messages from other Peers

        public void PostMessage(Message msg)
        {
            rcvBlockingQ.enQ(msg);
        }

        // Implement service method to extract messages from other Peers.
        // This will often block on empty queue, so user should provide
        // read thread.

        public Message GetMessage()
        {
            Message msg = rcvBlockingQ.deQ();
            return msg;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Sender is client of another Peer's Communication service

    public class Sender
    {

        string current = "";
        ICommunicator channel;
        string lastError = "";
        BlockingQueue<Message> sndBlockingQ = null;
        Thread sndThrd = null;
        int tryCount = 0, MaxCount = 10;


        // Processing for sndThrd to pull msgs out of sndBlockingQ
        // and post them to another Peer's Communication service
        public string name { get; set; }
        void ThreadProc()
        {
            tryCount = 0;
            while (true)
            {
                Message msg = sndBlockingQ.deQ();
                if (msg.to != current)
                {
                        current = msg.to;
                    CreateSendChannel(current);
                }
                while (true)
                {
                    try
                    {
                        channel.PostMessage(msg);
                        tryCount = 0;
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (++tryCount < MaxCount)
                            Thread.Sleep(100);
                        else
                        {
                            Console.Write("\n  {0}", "can't connect\n");
                            current = "";
                            tryCount = 0;
                            break;
                        }
                    }
                }
                if (msg.body == "quit")
                    break;
            }
        }


        // Create Communication channel proxy, sndBlockingQ, and
        // start sndThrd to send messages that client enqueues

        public Sender()
        {
            sndBlockingQ = new BlockingQueue<Message>();
          /*  while (true)
            {
                try
                {
                    CreateSendChannel(url);
                    tryCount = 0;
                    break;
                }
                catch (Exception ex)
                {
                    if (++tryCount < MaxCount)
                        Thread.Sleep(100);
                    else
                    {
                        lastError = ex.Message;
                        break;
                    }
                }
            }*/
            sndThrd = new Thread(ThreadProc);
            sndThrd.IsBackground = true;
            sndThrd.Start();
        }

        //----< Create proxy to another Peer's Communicator >------------

        public void CreateSendChannel(string address)
        {
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, address);
            channel = factory.CreateChannel();

        }

 // Sender posts message to another Peer's queue using
    // Communication service hosted by receipient via sndThrd
        public void PostMessage(Message msg)
        {
            sndBlockingQ.enQ(msg);
        }

        public string GetLastError()
        {
            string temp = lastError;
            lastError = "";
            return temp;
        }

        //----< closes the send channel >--------------------------------

        public void Close()
        {
            ChannelFactory<ICommunicator> temp = (ChannelFactory<ICommunicator>)channel;
            temp.Close();
        }
    }
    class Cat { }
    public class peer_<T>
    {
        public Sender sndr { get; set; } = new Sender();
        public string name { get; set; } = typeof(T).Name;
        public Receiver<T> rcvr { get; set; } = new Receiver<T>();
        public static string makeEndPoint(string url, int port)
        {
            string s = url + ":" + port.ToString() + "/ICommunicator";
            return s;
        }
        public peer_()
        {
            sndr.name = name;
            rcvr.name = name;
        }
       
        //----< this thrdProc() used only for testing, below >-----------

        public void thrdProc()
        {
            while (true)
            {
                Message msg = rcvr.GetMessage();
                msg.showMsg();
                if (msg.body == "quit")
                {
                    break;
                }
            }
        }
    }
 
    class TestComm
    {
        [STAThread]
        static void Main(string[] args)
        {
            peer_<Cat> comm = new peer_<Cat>();
            comm.sndr = new Sender();
            string s = peer_<Cat>.makeEndPoint("http://localhost", 8080);
            comm.sndr.CreateSendChannel(s);
            comm.rcvr.CreateRecvChannel(s);
            comm.rcvr.start(comm.thrdProc);  
            Message msg = new Message();
            Message msg1= new Message();
            msg.body = "Message";
            msg1.body = "Message 1";
            comm.sndr.PostMessage(msg);
            comm.sndr.PostMessage(msg1);
            Console.Write("\n  Comm Service Running:");
            Console.Write("\n  Press key to quit");
            Console.ReadKey();
            Console.Write("\n\n");
        }
    }
}
