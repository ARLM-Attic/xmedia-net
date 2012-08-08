using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFXMPPClient
{
    public class ConversationThreadManager
    {
        public static string strThreadPattern =
            @"\[(?<threadName>)[^\]]*\][\s](?<messageText>.*)";

        public static ThreadedMessage GetThreadedMessage(string strText)
        {
            ThreadedMessage threadedMessage = new ThreadedMessage();
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(strThreadPattern);
            System.Text.RegularExpressions.MatchCollection matchCollection = regex.Matches(strThreadPattern);
            if (matchCollection.Count > 0)
            {
                foreach (System.Text.RegularExpressions.Match match in matchCollection)
                {
                    if (match.Groups["threadName"] != null)
                    {
                        threadedMessage.ThreadName = match.Groups["threadName"].Value;
                    }
                    if (match.Groups["messageText"] != null)
                    {
                        threadedMessage.Text = match.Groups["messageText"].Value;
                    }
                    if (threadedMessage.IsPopulated)
                    {
                        return threadedMessage;
                    }
                }
            }
            return threadedMessage;
        }
    }

    public class ThreadedMessage
    {
        private string m_ThreadName = "";

        public string ThreadName
        {
            get { return m_ThreadName; }
            set { m_ThreadName = value; }
        }

        private string m_Text = "";

        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }

        private bool m_IsPopulated = false;

        public bool IsPopulated
        {
            get
            {
                if (Text != null && Text.Length > 0 && ThreadName != null && ThreadName.Length > 0)
                    m_IsPopulated = true;
                else
                    m_IsPopulated = false;
                return m_IsPopulated;
            }
        }
    }
}
