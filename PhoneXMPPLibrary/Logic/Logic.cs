using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.Net.XMPP
{
    /// <summary>
    /// Handles different types of xml fragments on our streams, such as negotiation, IQ's, etc
    /// </summary>
    public class Logic
    {
        public Logic(XMPPClient client)
        {
            XMPPClient = client;
        }

        protected XMPPClient XMPPClient = null;

        public virtual void Start()
        {
        }


        /// <summary>
        /// A new XML fragment has been received
        /// </summary>
        /// <param name="node"></param>
        /// <returns>returns true if we handled this fragment, false if other wise</returns>
        public virtual bool NewXMLFragment(XMPPStanza stanza)
        {
            return false;
        }

        public virtual bool NewIQ(IQ iq)
        {
            return false;
        }

        public virtual bool NewMessage(Message iq)
        {
            return false;
        }

        public virtual bool NewPresence(PresenceMessage iq)
        {
            return false;
        }

        protected bool m_bCompleted = false;

        /// <summary>
        /// Set to true if we have completed our logic and should be removed from the logic list
        /// </summary>
        public bool IsCompleted
        {
            get { return m_bCompleted; }
            set { m_bCompleted = value; }
        }

        private bool m_bSuccess = false;

        public bool Success
        {
            get { return m_bSuccess; }
            set { m_bSuccess = value; }
        }

    }
}
