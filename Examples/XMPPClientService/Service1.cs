using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Net.XMPP;

namespace XMPPClientService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        public XMPPClient XMPPClient = null;

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }

        protected bool Startup()
        {
            bool bRes = true;

            return bRes;
        }

    }
}
