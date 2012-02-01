using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace SocketServer
{
    /// <summary>
    /// This class is used to create new socketclients of the correct typewithin ConnectMgr
    /// Derive from this class 
    /// </summary>
	public class SocketCreator
	{
		public SocketCreator()
		{
		}

		public virtual SocketClient AcceptSocket( Socket s, ConnectMgr cmgr )
		{
			s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 128000);
			return new SocketClient( s, cmgr );
		}

		public virtual SocketClient CreateSocket( Socket s, ConnectMgr cmgr )
		{
			s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 128000);
			return new SocketClient( s, cmgr );
		}
	}

	

}
