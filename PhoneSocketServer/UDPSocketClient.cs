using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace SocketServer
{
   public class BufferSocketRef
   {
      public BufferSocketRef(UDPSocketClient client)
      {
         Client = client;

         /// See if we are using our pinned-memory protection, if we are use from the pool, if not, just new it
         if (UDPSocketClient.m_BufferPool != null)
            bRecv = UDPSocketClient.m_BufferPool.Checkout();
         else
            bRecv = new byte[UDPSocketClient.m_nBufferSize];
      }

      public UDPSocketClient Client;
      public byte[] bRecv;

      public void CheckInCopy(int nLen)
      {
         /// Copy our buffer to a non-pinned byte array, and release our pinned array back to the pool
         if (UDPSocketClient.m_BufferPool != null)
         {
            byte[] bPassIn = new byte[nLen];
            if (nLen > 0)
               Array.Copy(bRecv, 0, bPassIn, 0, nLen);

            UDPSocketClient.m_BufferPool.Checkin(bRecv);
            bRecv = bPassIn;
         }
      }

   }

	/// <summary>
	/// Summary description for UDPSocketClient.
	/// </summary>
	public class UDPSocketClient //: System.Net.Sockets.UdpClient
	{
		public UDPSocketClient(IPEndPoint ep) // : base(ep)
		{
         m_ipEp = ep;

         if (ep.AddressFamily == AddressFamily.InterNetworkV6)
         {
            s = new System.Net.Sockets.Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
         }
         else
         {
            s = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
         }

         if (ep.AddressFamily == AddressFamily.InterNetworkV6)
         {
             IPEndPoint sender = new IPEndPoint(IPAddress.IPv6Any, 0);
             m_tempRemoteEP = (System.Net.IPEndPoint)sender;
         }
         else
         {
             IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
             m_tempRemoteEP = (System.Net.IPEndPoint)sender;
         }
         //asyncb = new AsyncCallback(OnReceiveUDP);

      }

      //public DateTimePrecise DateTimePrecise = new DateTimePrecise();

      
      /// <summary>
      /// set for logging
      /// </summary>
      public ILogInterface m_Logger = null;
      public string OurGuid = "UDPClient";

      void LogMessage(MessageImportance importance, string strEventName, string strMessage)
      {
         if (m_Logger != null)
         {
            m_Logger.LogMessage(ToString(), importance, strMessage);
         }
      }

      void LogWarning(MessageImportance importance, string strEventName, string strMessage)
      {
         if (m_Logger != null)
         {
             m_Logger.LogWarning(ToString(), importance, strMessage);
         }
      }

      void LogError(MessageImportance importance, string strEventName, string strMessage)
      {
         if (m_Logger != null)
         {
             m_Logger.LogError(ToString(), importance, strMessage);
         }
      }

      public readonly IPEndPoint m_ipEp;         /// our endpoint
      public System.Net.Sockets.Socket s = null;

      public delegate void DelegateReceivePacket(byte [] bData, int nLength, IPEndPoint epfrom);
      public delegate void DelegateReceivePacket2(byte [] bData, int nLength, IPEndPoint epfrom, IPEndPoint epthis);
      public delegate void DelegateReceivePacket3(byte[] bData, int nLength, IPEndPoint epfrom, IPEndPoint epthis, DateTime dtReceived);
      public event DelegateReceivePacket OnReceivePacket = null;
      public event DelegateReceivePacket2 OnReceivePacket2 = null;
      public event DelegateReceivePacket3 OnReceivePacket3 = null;
      public static readonly int m_nBufferSize = 16 * 1024;
      
      protected IPEndPoint m_tempRemoteEP = null;  /// temp endpoint for receivefrom
      //protected System.AsyncCallback asyncb;
      protected bool m_bReceive = true;
      public int NumberOfReceivingThreads; // This would allow the consumer to know the status of Receiving operation.
		
		public delegate void DelegateReceivingStopped(string reason);

		public event DelegateReceivingStopped OnReceivingStopped=null;

      public object SyncRoot = new object();
      public string ThreadNameShutdown = "";
      public string ThreadNameDispose = "";

      //const uint SIO_UDP_CONNRESET = 0x9800000C;
      // 0x9800000C == 2440136844 (uint) == -1744830452 (int) == 0x9800000C
      const int SIO_UDP_CONNRESET = -1744830452;


      public static IBufferPool m_BufferPool = null;

      bool m_bIsBound = false;
      public bool Bind()
      {
         if (m_bIsBound == true)
            return true;

          m_bIsBound = true;
          /// Doesn't appear to be a bind option in windows phone 7
          /// 


         //lock (SyncRoot)
         //{
         //   m_bReceive = true;
         //   System.Net.EndPoint epBind = (EndPoint)m_ipEp;
         //   try
         //   {
         //      s.Bind(epBind);
         //   }
         //   catch (SocketException e3) /// winso
         //   {
         //      string strError = string.Format("{0} - {1}", e3.ErrorCode, e3.ToString());
         //      LogError(MessageImportance.High, "EXCEPTION", strError);
         //      return false;
         //   }
         //   catch (ObjectDisposedException e4) // socket was closed
         //   {
         //      string strError = e4.ToString();
         //      LogError(MessageImportance.High, "EXCEPTION", strError);
         //      return false;
         //   }

         //   m_bIsBound = true;
         //}

         return true;
      }

		public bool StartReceiving()
      {
         /// See http://blog.devstone.com/aaron/archive/2005/02/20/460.aspx
         /// This will stop winsock errors when receiving an ICMP packet 
         /// "Destination unreachable"
         byte[] inValue = new byte[] { 0, 0, 0, 0 };     // == false
         byte[] outValue = new byte[] { 0, 0, 0, 0 };    // initialize to 0
         //s.IOControl(SIO_UDP_CONNRESET, inValue, outValue);


         if (Bind() == false)
            return false;

         lock (SyncRoot)
         {
            m_bReceive = true;

            s.ReceiveBufferSize = 128000;
            //s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 128000);
            /// have 8 pending receives in the queue at all times
            this.NumberOfReceivingThreads = 1;

            DoReceive();
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
               DoReceive();
               DoReceive();
            }
            //DoReceive();
            //DoReceive();
            //DoReceive();
            //DoReceive();
            //DoReceive();
            //DoReceive();
            //DoReceive();
         }
         return true;
      }

      public void StopReceiving()
      {
         lock (SyncRoot)
         {
            if (m_bReceive == false)
            {
               this.LogError(MessageImportance.Highest, "error", string.Format("Can't call StopReceiving, Thread has been disposed by {0} or closed by {1}", this.ThreadNameDispose, this.ThreadNameShutdown));
               return;
            }

            ThreadNameShutdown = System.Threading.Thread.CurrentThread.Name;
            LogMessage(MessageImportance.Lowest, this.OurGuid, string.Format("Called StopReceiving for {0}", s));
            m_bReceive = false;
            // Shutdown not recommended on UDP
            //s.Shutdown(System.Net.Sockets.SocketShutdown.Both);
            s.Close();
         }
      }

      protected void DoReceive()
      {
         lock (SyncRoot)
         {
            if (m_bReceive == false)
            {
               this.LogError(MessageImportance.Highest, "error", string.Format("Can't call DoReceive, socket has been disposed by thread {0} or closed by {1}", this.ThreadNameDispose, this.ThreadNameShutdown));
               return;
            }

            
            System.Net.EndPoint ep = (System.Net.EndPoint)m_tempRemoteEP;
            try
            {
               LogMessage(MessageImportance.Lowest, this.OurGuid, string.Format("Called DoReceive"));
               BufferSocketRef objRef = new BufferSocketRef(this);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.UserToken = objRef;
                args.SetBuffer(objRef.bRecv, 0, m_nBufferSize);
                args.RemoteEndPoint = m_tempRemoteEP;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveUDP);
                s.ReceiveFromAsync(args);

                //s.BeginReceiveFrom(objRef.bRecv, 0, m_nBufferSize, System.Net.Sockets.SocketFlags.None, ref ep, asyncb, objRef);
            }
            catch (SocketException e3) /// winso
            {
               string strError = string.Format("{0} - {1}", e3.ErrorCode, e3.ToString());
               LogError(MessageImportance.High, "SocketEXCEPTION", strError);
               --(this.NumberOfReceivingThreads);
               if (this.NumberOfReceivingThreads == 0 && this.OnReceivingStopped != null)
                  this.OnReceivingStopped(strError);
               return;
            }
            catch (ObjectDisposedException e4) // socket was closed
            {
               string strError = e4.ToString();
               LogError(MessageImportance.High, "ObjectDisposedEXCEPTION", strError);
               --(this.NumberOfReceivingThreads);
               if (this.NumberOfReceivingThreads == 0 && this.OnReceivingStopped != null)
                  this.OnReceivingStopped(strError);
               return;
            }
            catch (Exception e5)
            {
               string strError = string.Format("{0}", e5.ToString());
               LogError(MessageImportance.High, "EXCEPTION", strError);
               --(this.NumberOfReceivingThreads);
               if (this.NumberOfReceivingThreads == 0 && this.OnReceivingStopped != null)
                  this.OnReceivingStopped(strError);
               return;
            }
         }
         return;
      }


      protected void OnReceiveUDP(object sender, SocketAsyncEventArgs e)
      {
          DateTime dtReceive = DateTime.Now; // DateTimePrecise.Now;

         BufferSocketRef objRef = e.UserToken as BufferSocketRef;
         System.Net.EndPoint ep = (System.Net.EndPoint)m_tempRemoteEP;
         int nRecv = 0;

         try
         {
             nRecv = e.BytesTransferred;
            //nRecv = s.EndReceiveFrom(ar, ref ep);
            objRef.CheckInCopy(nRecv);
         }
         catch (SocketException e3) /// winso
         {
            objRef.CheckInCopy(nRecv);
            string strError = string.Format("{0} - {1}", e3.ErrorCode, e3.ToString());
            LogError(MessageImportance.High, "EXCEPTION", strError);
            --(this.NumberOfReceivingThreads);
            //if (this.NumberOfReceivingThreads == 0 && this.OnReceivingStopped != null)
              // this.OnReceivingStopped(strError);

            /// Get 10054 if the other end is not listening (ICMP returned)... fixed above with IOControl
            if (e3.ErrorCode != 10054)
            {
            }
            return;

         }
         catch (ObjectDisposedException e4) // socket was closed
         {
            objRef.CheckInCopy(nRecv);
            string strError = e4.ToString();
            this.LogWarning(MessageImportance.Low, "EXCEPTION", strError);
            --(this.NumberOfReceivingThreads);
            if (this.NumberOfReceivingThreads == 0 && this.OnReceivingStopped != null)
               this.OnReceivingStopped(strError);
            return;
         }
         catch (Exception e5)
         {
            objRef.CheckInCopy(nRecv);
            string strError = string.Format("{0}", e5.ToString());
            LogError(MessageImportance.High, "EXCEPTION", strError);
            --(this.NumberOfReceivingThreads);
            if (this.NumberOfReceivingThreads == 0 && this.OnReceivingStopped != null)
               this.OnReceivingStopped(strError);
            return;
         }


         System.Net.IPEndPoint ipep = (System.Net.IPEndPoint) ep;

         OnRecv(objRef.bRecv, nRecv, ipep, dtReceive);
      }

      private void OnRecv(byte[] bRecv, int nRecv, IPEndPoint ipep, DateTime dtReceive)
      {

         if (nRecv > 0)
         {
            if (OnReceivePacket != null)
            {
               OnReceivePacket(bRecv, nRecv, ipep);
            }
            if (OnReceivePacket2 != null)
            {
               OnReceivePacket2(bRecv, nRecv, ipep, this.m_ipEp);
            }
            if (OnReceivePacket3 != null)
            {
               OnReceivePacket3(bRecv, nRecv, ipep, this.m_ipEp, dtReceive);
            }
            
         }

			if (m_bReceive == true)
				DoReceive();
			else
			{
				--(this.NumberOfReceivingThreads);
//				if(this.NumberOfReceivingThreads==0&&this.OnReceivingStopped!=null)
//					this.OnReceivingStopped(strError);
			}
      }

     

      public bool SendUDP(byte[] bData, int nLength, System.Net.IPEndPoint ep)
      {
         lock (SyncRoot)
         {
            if (m_bReceive == false)
            {
               this.LogError(MessageImportance.Highest, "error", string.Format("Can't call SendUDP, socket not valid or closed"));
               return false;
            }

            LogMessage(MessageImportance.Lowest, this.OurGuid, string.Format("SendUDP to {0}", ep));

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = ep;
            args.SetBuffer(bData, 0, nLength);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(SendUDP_Completed);
            return s.SendToAsync(args);
         }
      }

      void SendUDP_Completed(object sender, SocketAsyncEventArgs e)
      {
          
      }

      #region ReStarting DoReceive()
      //
      // Only want to start the DoReceive().  The main purpose is to keep ListenOn the local 
      // port while recover the error caused by operation of sending to a unreachable destination(usually un-listened port).
      // Dead computer does not cause error.
      //
      public bool RestartReceivingData()
      {
         if(this.NumberOfReceivingThreads!=0)return false;
         this.NumberOfReceivingThreads=8;
         for(int i=0;i<8;i++)
            DoReceive();
         return true;
      }
      #endregion
   }
}
