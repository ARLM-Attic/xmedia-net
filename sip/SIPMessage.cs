using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

namespace sip
{
    ///  Simple SIP message... doesn't really handle multiple same time headers now
    /// </summary>
    public class SIPMessage
    {
        public SIPMessage(string strHeaders)
        {
            All = strHeaders;
        }

        public SIPMessage(byte[] bHeaders)
        {
            All = System.Text.UTF8Encoding.UTF8.GetString(bHeaders);
        }

        public string RequestResponseLine
        {
            get
            {
                int nFirstLineAt = All.IndexOf("\r\n");
                return All.Substring(0, nFirstLineAt + 2);
            }
            set
            {
                int nFirstLineAt = All.IndexOf("\r\n");
                if (value.IndexOf("\r\n") < 0)
                    value = value + "\r\n";
                All = value + All.Substring(nFirstLineAt + 2);
            }
        }

        private string m_strAll = "";
        public string All
        {
            get { return m_strAll; }
            set { m_strAll = value; }
        }

        public string GetHeaderValue(string strHeader)
        {
            Match matchman = Regex.Match(m_strAll, string.Format(@"^\s*{0}:\s*(.*?)$", strHeader), RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (matchman.Success == false)
                return null;

            return matchman.Groups[1].Value;
        }

        public void SetHeaderValue(string strHeader, string strValue)
        {
            Match matchman = Regex.Match(m_strAll, string.Format(@"^\s*{0}:\s*(.*?)$", strHeader), RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (matchman.Success == false)
            {
                AppendHeader(strHeader, strValue);
            }
            else
            {
                string strSub = string.Format("{0}: {1}\r\n", strHeader, strValue);

                All = All.Substring(0, matchman.Index) + strSub + All.Substring(matchman.Index + matchman.Length);
            }
        }

        public void AppendHeader(string strHeader, string strValue)
        {
            string strSub = string.Format("{0}: {1}\r\n", strHeader, strValue);
            All = All.Insert(All.Length - 3, strSub);
        }

    }

    public class SIPRequestMessage : SIPMessage
    {
        public SIPRequestMessage()
            : base("POST / HTTP/1.1\r\n\r\n")
        {
        }

        public SIPRequestMessage(byte[] bData)
            : base(bData)
        {
        }

        public SIPRequestMessage(string strData)
            : base(strData)
        {
        }

        public string Method
        {
            get
            {
                Match matchMethod = Regex.Match(All, @"(\S+)\s", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchMethod.Success == false)
                    return null;
                return matchMethod.Groups[1].Value;
            }
            set
            {
                Match matchMethod = Regex.Match(All, @"(\S+)\s", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchMethod.Success == true)
                {
                    All = value + All.Substring(matchMethod.Index + matchMethod.Length);
                }
            }

        }

        public string URL
        {
            get
            {
                Match matchLine = Regex.Match(All, @"^(\S+)\s(.+)\sHTTP/1.1$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchLine.Success == false)
                    return null;
                return matchLine.Groups[2].Value;
            }
            set
            {
                Match matchLine = Regex.Match(All, @"^(\S+)\s(.+)\sHTTP/1.1$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchLine.Success == true)
                {
                    string strNewLine = matchLine.Groups[1].Value + " " + value + " HTTP/1.1\r\n";
                    All = strNewLine + All.Substring(matchLine.Index + matchLine.Length);
                }
            }

        }

    }

    public class SIPResponseMessage : SIPMessage
    {
        public SIPResponseMessage()
            : base("HTTP/1.1 200 OK\r\n\r\n")
        {
        }


        public SIPResponseMessage(byte[] bData)
            : base(bData)
        {
        }

        public SIPResponseMessage(string strData)
            : base(strData)
        {
        }

        public string ResponseCode
        {
            get
            {
                Match matchResponse = Regex.Match(All, @"^HTTP/1.1\s+(\d+)\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResponse.Success == false)
                    return null;
                return matchResponse.Groups[1].Value;
            }
            set
            {
                Match matchResponse = Regex.Match(All, @"^HTTP/1.1\s+(\d+)\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResponse.Success == true)
                {
                    string strNewLine = "HTTP/1.1 " + value + " " + matchResponse.Groups[2].Value + "\r\n";
                    All = strNewLine + All.Substring(matchResponse.Index + matchResponse.Length);
                }
            }

        }

        public string ResponseString
        {
            get
            {
                Match matchResponse = Regex.Match(All, @"^HTTP/1.1\s+(\d+)\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResponse.Success == false)
                    return null;
                return matchResponse.Groups[2].Value;
            }
            set
            {
                Match matchResponse = Regex.Match(All, @"^HTTP/1.1\s+(\d+)\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResponse.Success == true)
                {
                    string strNewLine = "HTTP/1.1 " + matchResponse.Groups[1].Value + " " + value + "\r\n";
                    All = strNewLine + All.Substring(matchResponse.Index + matchResponse.Length);
                }
            }

        }

    }
}
