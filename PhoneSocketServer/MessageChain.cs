using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer
{
    public interface IMessageFilter
   {
      /// <summary>
      /// Transforms data for sending.  This may add additional headers, etc.  If it returns a zero length
      /// array, the data will not be passed to the next message in the chain
      /// For messages, only one message must be sent a time if there are mutiple message filters in the list
      /// </summary>
      /// <param name="bSend"></param>
      /// <returns></returns>
      byte[] TransformSendData(byte[] bSend);

      /// <summary>
      /// Transforms data that was read.  May be used to find message boundaries, decompress, unencrypt etc.  
      /// Returns a list of messages broken on message boundaries.   Each of these arrays is passed to the 
      /// next filter in the chain
      /// </summary>
      /// <param name="bSend"></param>
      /// <returns></returns>
      List<byte[]> TransformReceiveData(byte[] bSend);

      bool IsFilterActive { get; }
   }
}
