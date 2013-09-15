using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;

namespace System.Net.XMPP.Server
{
    /// <summary>
    ///  The configuration needed for a server
    /// </summary>
    public class XMPPServerConfig
    {
        private int m_nPort = 5222;
        [DataMember]
        public int Port
        {
            get { return m_nPort; }
            set { m_nPort = value; }
        }

        private string m_strIPAddress = "0.0.0.0";
        [DataMember]
        public string IPAddress
        {
            get { return m_strIPAddress; }
            set { m_strIPAddress = value; }
        }

        private string m_strCertificate = null;
        [DataMember]
        public string Certificate
        {
            get { return m_strCertificate; }
            set { m_strCertificate = value; }
        }

        private bool m_bAllowTLS = true;
        [DataMember]
        public bool AllowTLS
        {
            get { return m_bAllowTLS; }
            set { m_bAllowTLS = value; }
        }

        private bool m_bTLSRequired = false;
        [DataMember]
        public bool TLSRequired
        {
            get { return m_bTLSRequired; }
            set { m_bTLSRequired = value; }
        }

        private string m_strDomainName = "";
        [DataMember]
        public string DomainName
        {
            get { return m_strDomainName; }
            set { m_strDomainName = value; }
        }

    }
}
