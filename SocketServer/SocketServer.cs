using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace SocketServer
{
   /// <summary>
   /// Summary description for Class1.
   /// </summary>
   /// 
   public class SocketEventArgs : System.EventArgs
   {
      public int Length = 0;
      public byte[] m_data = null;
      public SocketEventArgs( byte[] data, int nlen )
      {

         if ( m_data != null )
            m_data = null;
         m_data = new byte[nlen];
         System.Array.Copy( data, 0, m_data, 0, nlen);
         Length = nlen;
      }

      public SocketEventArgs()
      {
      }

      public byte[] GetData()
      {
         return m_data;
      }
      public string GetString()
      {
         return System.Text.Encoding.ASCII.GetString(m_data, 0, Length);
      }



   }

}
