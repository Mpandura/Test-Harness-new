/////////////////////////////////////////////////////////////////////
// Repository.cs - holds test code for TestHarness                 //
//                                                                 //
// author : Manjunath Panduranga                                    //
// source: Jim Fawcett,                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Almost no functionality now.  Will be expanded to accept
 * Queries for Logs and Libraries.

 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using FileTransferService;
using System.Threading;
using System.ServiceModel;

namespace TestHarness
{ 
  public class Repository : IFileService
    {
        string location = "..\\..\\..\\Repository\\RepositoryStorage\\";
       FileStream file_ = null; 

        public void SetServerFilePath(string path)
        {
            location = path;
        } string dir = "";
       
        public bool WriteFileBlock(byte[] block)
        {
            try
            {
                Console.Write("\n  Writing blocks of bytes {0} ", block.Length);
              file_.Write(block, 0, block.Length);
               file_.Flush();
                return true;
            }
            catch { return false; }
        }
        
        static ServiceHost CreateChannel(string url)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(url);
            Type service = typeof(TestHarness.Repository);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(FileTransferService.IFileService), binding, baseAddress);
            return host;
        }

    public Repository()
    {
      DLog.write("\n  Creating instance of Repository");
    }
    //----< search for text in log files >---------------------------
    /*
     * This function should return a message.  I'll do that when I
     * get a chance.
     */
    public List<string> queryLogs(string queryText)
    {
      List<string> queryResults = new List<string>();
      string path = System.IO.Path.GetFullPath(location);
      string[] files = System.IO.Directory.GetFiles(location, "*.txt");
      foreach(string file in files)
      {
        string contents = File.ReadAllText(file);
        if (contents.Contains(queryText))
        {
          string name = System.IO.Path.GetFileName(file);
          queryResults.Add(name);
        }
      }
      return queryResults;
    }
    //----< send files with names on fileList >----------------------
    /*
     * This function is not currently being used.  It may, with a
     * Message interface, become part of Project #4.
     */
    public bool getFiles(string path, string fileList)
    {
      string[] files = fileList.Split(new char[] { ',' });
      //string repoStoragePath = "..\\..\\RepositoryStorage\\";

      foreach (string file in files)
      {
        string fqSrcFile = location + file;
        string fqDstFile = "";
        try
        {
          fqDstFile = path + "\\" + file;
          File.Copy(fqSrcFile, fqDstFile);
        }
        catch
        {
          RLog.write("\n  could not copy \"" + fqSrcFile + "\" to \"" + fqDstFile);
          return false;
        }
      }
      return true;
    }
    //----< intended for Project #4 >--------------------------------

    public void sendLog(string Log)
    {

    }

        static IFileService wcf(string url)
        {
            WSHttpBinding binding = new WSHttpBinding();
            EndpointAddress address = new EndpointAddress(url);
            ChannelFactory<IFileService> factory = new ChannelFactory<IFileService>(binding, address);
            return factory.CreateChannel();
        }

        static bool sending(IFileService service, string file)//send file
        {
            long blockSize = 1000;
            try
            {
                string filename = Path.GetFileName(file);
                service.OpenFileForWrite(filename);
                FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read);
                int bytesRead = 0;
                while (true)
                {
                    long remainder = (int)(fs.Length - fs.Position);
                    if (remainder == 0)  break;
                    long size = Math.Min(blockSize, remainder);
                    byte[] block = new byte[size];
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
        string str_ = "1";
        public string messge()
        {
            Console.Write("\n sending, from getString, \"{0}\"", str_);
            return str_;
        }
        static void Main(string[] args)
    {
            IFileService file_ = null;
            int length = 0;
            Console.Write("\n Here Files are getting Transferred");
            ServiceHost Shost = CreateChannel("http://localhost:8080/FileService");
            Shost.Open();
            Console.Write("\n  Press key to terminate service:\n");
            Console.ReadKey();
            Console.Write("\n");
            Shost.Close();
            Console.Write("\n  Client of File Transfer Service");
            string url = "http://localhost:8080/FileService1";
            Console.Write("\n =================================\n");

            while (true)
            {
                try
                {
                    file_ = wcf(url);
                    break;
                }
                catch
                {
                    Console.Write("\n  connection failed {0} times", ++length);
                    Thread.Sleep(500);
                    continue;
                }
            }
            Console.Write("\n  Connected to {0}\n", url);
            string path = "Sending";

            string path_ = Path.GetFullPath(path);
            Console.Write("\n  retrieving files\n  {0}\n", path_);
            string[] files = Directory.GetFiles(path_);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file); Console.Write("\n  sending file {0}", name);

                if (!sending(file_, file))
                    Console.Write("\n  failed");
            }
            Console.Write("\n\n");

        }

        public bool OpenFileForWrite(string name)
        {
            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);
            dir = location + "\\" + name;
            try
            {
                file_ = File.Open(dir, FileMode.Create, FileAccess.Write);
                Console.Write("\n Opening the file{0} ", dir); return true;
            }
            catch
            {
                Console.Write("\n  {0} is filed to open", dir);
                return false;
            }
        }
        public bool CloseFile()
        {
            try
            {
                file_.Close();
                Console.Write("\n The File {0} is closed \n ", dir);
                return true;
            }
            catch { return false; }
        }
    }
}
