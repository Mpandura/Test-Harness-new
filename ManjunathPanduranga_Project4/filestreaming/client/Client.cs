///////////////////////////////////////////////////////////////////////
// Client.cs - WCF Timed, SelfHosted, File StreamService client      //
//                                                                   //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Summer 2009 //
///////////////////////////////////////////////////////////////////////
/*
 * Note:
 * - Uses Programmatic configuration, no app.config file used.
 * - Uses ChannelFactory to create proxy programmatically. 
 * - Expects to find ToSend directory under application with files
 *   to send.
 * - Will create SavedFiles directory if it does not already exist.
 */

using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CSE681
{
  class Client
  {
    IStreamService channel;
    string ToSendPath = "..\\..\\ToSend";
    string SavePath = "..\\..\\SavedFiles";
    int BlockSize = 1024;
    byte[] block;
    HRTimer.HiResTimer hrt = null;

    Client()
    {
      block = new byte[BlockSize];
      hrt = new HRTimer.HiResTimer();
    }
    static IStreamService CreateServiceChannel(string url)
    {
      BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;

      BasicHttpBinding binding = new BasicHttpBinding(securityMode);
      binding.TransferMode = TransferMode.Streamed;
      binding.MaxReceivedMessageSize = 500000000;
      EndpointAddress address = new EndpointAddress(url);

      ChannelFactory<IStreamService> factory
        = new ChannelFactory<IStreamService>(binding, address);
      return factory.CreateChannel();
    }

    void uploadFile(string filename)
    {
      string fqname = Path.Combine(ToSendPath, filename);
      try
      {
        hrt.Start();
        using (var inputStream = new FileStream(fqname, FileMode.Open))
        {
          FileTransferMessage msg = new FileTransferMessage();
          msg.filename = filename;
          msg.transferStream = inputStream;
          channel.upLoadFile(msg);
        }
        hrt.Stop();
        Console.Write("\n  Uploaded file \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
      }
      catch
      {
        Console.Write("\n  can't find \"{0}\"", fqname);
      }
    }

    void download(string filename)
    {
      int totalBytes = 0;
      try
      {
        hrt.Start();
        Stream strm = channel.downLoadFile(filename);
        string rfilename = Path.Combine(SavePath, filename);
        if (!Directory.Exists(SavePath))
          Directory.CreateDirectory(SavePath);
        using (var outputStream = new FileStream(rfilename, FileMode.Create))
        {
          while (true)
          {
            int bytesRead = strm.Read(block, 0, BlockSize);
            totalBytes += bytesRead;
            if (bytesRead > 0)
              outputStream.Write(block, 0, bytesRead);
            else
              break;
          }
        }
        hrt.Stop();
        ulong time = hrt.ElapsedMicroseconds;
        Console.Write("\n  Received file \"{0}\" of {1} bytes in {2} microsec.", filename, totalBytes, time);
      }
      catch (Exception ex)
      {
        Console.Write("\n  {0}",ex.Message);
      }
    }

    static void Main()
    {
      Console.Write("\n  Client of SelfHosted File Stream Service");
      Console.Write("\n ==========================================\n");

      Client clnt = new Client();
      clnt.channel = CreateServiceChannel("http://localhost:8000/StreamService");
      HRTimer.HiResTimer hrt = new HRTimer.HiResTimer();

      hrt.Start();
      clnt.uploadFile("test.txt");
      clnt.uploadFile("Channel_Client_Copy.exe");
      clnt.uploadFile("FileStreaming.zip");
      clnt.uploadFile("CreateVirtDir.doc");
      clnt.uploadFile("demoWinForm.ncb");
      hrt.Stop();
      Console.Write(
        "\n\n  total elapsed time for uploading = {0} microsec.\n",
        hrt.ElapsedMicroseconds
      );

      hrt.Start();
      clnt.download("test.txt");
      clnt.download("FileStreaming.zip");
      clnt.download("CreateVirtDir.doc");
      clnt.download("demoWinForm.ncb");
      clnt.download("foobar");
      hrt.Stop();

      Console.Write(
        "\n\n  Total elapsed time for downloading = {0}",
        hrt.ElapsedMicroseconds
      );
      Console.Write("\n\n  Press key to terminate client");
      Console.ReadKey();
      Console.Write("\n\n");
      ((IChannel)clnt.channel).Close();
    }
  }
}
