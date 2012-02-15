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

namespace XMPPClient
{
    public class Options
    {
        public Options()
        {}

        private bool m_bRunWithScreenLocked = true;
        public bool RunWithScreenLocked
        {
            get { return m_bRunWithScreenLocked; }
            set { m_bRunWithScreenLocked = value; }
        }

        private bool m_bLogXML = false;
        public bool LogXML
        {
            get { return m_bLogXML; }
            set { m_bLogXML = value; }
        }

        private bool m_bSendGeoCoordinates = false;
        public bool SendGeoCoordinates
        {
            get { return m_bSendGeoCoordinates; }
            set { m_bSendGeoCoordinates = value; }
        }

    }
}
