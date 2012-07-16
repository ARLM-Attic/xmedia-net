using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Socks5Service
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        SocketServer.SOCKServer server = new SocketServer.SOCKServer();

        protected override void OnStart(string[] args)
        {
            server.Port = Socks5Service.Properties.Settings.Default.Port;
            server.Start();
        }

        protected override void OnStop()
        {
            
        }
    }
}
