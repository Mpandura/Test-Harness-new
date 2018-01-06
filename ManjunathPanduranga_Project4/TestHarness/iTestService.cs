// author : Manjunath Panduranga                                    //
// source: Jim Fawcett,                                             //
using System;
using System.ServiceModel;

namespace FileTransferService
{
    [ServiceContract(Namespace = "FileTransferService")]
    public interface IFileService
    {
        [OperationContract]//
        bool OpenFileForWrite(string name);

        [OperationContract]
        bool WriteFileBlock(byte[] block);

        [OperationContract]
        bool CloseFile();
    }
}
