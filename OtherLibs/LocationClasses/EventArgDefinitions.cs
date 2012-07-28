using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.XMPP;

namespace LocationClasses
{
    public class MarkerArgs : System.EventArgs
    {
        public MarkerArgs()
        {

        }

        private MapRosterItem m_mapRosterItem = null;

        public MapRosterItem MapRosterItem
        {
            get { return m_mapRosterItem; }
            set { m_mapRosterItem = value; }
        }
    }

    public class MapArgs : System.EventArgs
    {
        public MapArgs()
        {
        }

        private string m_FunctionName = "";

        public string FunctionName
        {
            get { return m_FunctionName; }
            set { m_FunctionName = value; }
        }

        private string m_UnparsedContents = "";

        public string UnparsedContents
        {
            get { return m_UnparsedContents; }
            set { m_UnparsedContents = value; }
        }

        private string m_ParameterList = "";

        public string ParameterList
        {
            get { return m_ParameterList; }
            set { m_ParameterList = value; }
        }

        private List<string> m_strParameters = new List<string>();

        public List<string> StrParameters
        {
            get { return m_strParameters; }
            set { m_strParameters = value; }
        }

        private RosterItem m_RosterItem = null;

        public RosterItem RosterItem
        {
            get { return m_RosterItem; }
            set { m_RosterItem = value; }
        }

        private MapRosterItem m_MapRosterItem = null;

        public MapRosterItem MapRosterItem
        {
            get { return m_MapRosterItem; }
            set { m_MapRosterItem = value; }
        }

    }

    public class ScriptArgs : System.EventArgs
    {
        public ScriptArgs()
        {
        }

        private string m_FunctionName = "";

        public string FunctionName
        {
            get { return m_FunctionName; }
            set { m_FunctionName = value; }
        }

        private string m_UnparsedContents = "";

        public string UnparsedContents
        {
            get { return m_UnparsedContents; }
            set { m_UnparsedContents = value; }
        }

        private string m_ParameterList = "";

        public string ParameterList
        {
            get { return m_ParameterList; }
            set { m_ParameterList = value; }
        }

        private List<string> m_strParameters = new List<string>();

        public List<string> StrParameters
        {
            get { return m_strParameters; }
            set { m_strParameters = value; }
        }

        private object[] m_Parameters = null;

        public object[] Parameters
        {
            get { return m_Parameters; }
            set { m_Parameters = value; }
        }
    }

    public class BrowserArgs : System.EventArgs
    {
        private string message;

        public BrowserArgs()
        {
        }

        public BrowserArgs(string m)
        {
            this.message = m;
        }

        public string Message()
        {
            return message;
        }
    }
}