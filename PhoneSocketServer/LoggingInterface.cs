using System;
using System.Runtime.Serialization;

namespace SocketServer
{
   /// <summary>
   /// The importance of the message.  The server will only report messages that are as or more important
   /// than the importance requested by the client.
   /// </summary>
   public enum MessageImportance
   {
      Lowest = 0,
      Low = 1,
      MediumLow = 2,
      Medium = 3,
      MediumHigh = 4,
      High = 5,
      Highest = 6,
   }


   public interface ILogInterface
   {
      ///  Functions to log messages
      void LogMessage(string strCateogry, string strGuid, MessageImportance importance, string strMessage, params object [] msgparams);
      void LogWarning(string strCateogry, string strGuid, MessageImportance importance, string strMessage, params object [] msgparams);
      void LogError(string strCateogry, string strGuid, MessageImportance importance, string strMessage, params object [] msgparams);

      void LogMessage(string strGuid, MessageImportance importance, string strMessage, params object [] msgparams);
      void LogWarning(string strGuid, MessageImportance importance, string strMessage, params object [] msgparams);
      void LogError(string strGuid, MessageImportance importance, string strMessage, params object[] msgparams);

      void ClearLog();
   }

}
