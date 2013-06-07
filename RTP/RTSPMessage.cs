using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

namespace RTP
{
    /// <summary>
    ///  Base class for headers and used for unknown headers
    /// </summary>
    public class RTSPHeader
    {
        public RTSPHeader(string strName, string strAbbrev)
        {
            HeaderName = strName;
            HeaderAbbrev = strAbbrev;
        }

        private string m_strHeaderName = "";

        public string HeaderName
        {
            get { return m_strHeaderName; }
            set { m_strHeaderName = value; }
        }

        private string m_strHeaderAbbrev = "";

        public string HeaderAbbrev
        {
            get { return m_strHeaderAbbrev; }
            set { m_strHeaderAbbrev = value; }
        }


        public static Regex RegexContentType = new Regex(@"^(\S+)\: (.+) $", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public static RTSPHeader Parse(string strLine)
        {
            Match matchman = RegexContentType.Match(strLine);
            if (matchman.Success == true)
            {
                RTSPHeader header = new RTSPHeader(matchman.Groups[1].Value, matchman.Groups[1].Value);
                header.Value = matchman.Groups[2].Value;
                return header;
            }
            return null;
        }

        private string m_strValue = "lksdfja;ldkfj";

        public string Value
        {
            get { return m_strValue; }
            set { m_strValue = value; }
        }
        public virtual string BuildString()
        {
            return "";
        }

    }


    public class RTSPCSeqHeader : RTSPHeader
    {
        public RTSPCSeqHeader() : base("CSEQ", "cseq")
        {
        }

        public RTSPCSeqHeader(int nSeqNum)
            : base("CSEQ", "cseq")
        {
            CSeqNumber = nSeqNum;
        }


        public static Regex RegexCSeq = new Regex(@"^cseq\: \s* (\d+) \s*$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public static RTSPHeader Parse(string strLine)
        {
            Match matchman = RegexCSeq.Match(strLine);
            if (matchman.Success == true)
            {
                int nNumber = Convert.ToInt32(matchman.Groups[1].Value);
                RTSPCSeqHeader header = new RTSPCSeqHeader(nNumber);
                return header;
            }
            return null;
        }

        private int m_nCSeqNumber = 0;

        public int CSeqNumber
        {
            get { return m_nCSeqNumber; }
            set { m_nCSeqNumber = value; }
        }


    }

    public class RTSPAcceptHeader : RTSPHeader
    {
        public RTSPAcceptHeader()
            : base("Accept", "Accept")
        {
        }

        public RTSPAcceptHeader(string strContentType)
            : base("Accept", "Accept")
        {
            ContentType = strContentType;
        }


        public static Regex RegexAccept= new Regex(@"^Accept\: \s* (\S+) \s*$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public static RTSPHeader Parse(string strLine)
        {
            Match matchman = RegexAccept.Match(strLine);
            if (matchman.Success == true)
            {
                RTSPContentTypeHeader header = new RTSPContentTypeHeader(matchman.Groups[1].Value);
                return header;
            }
            return null;
        }

        private string m_strContentType = "application/sdp";

        public string ContentType
        {
            get { return m_strContentType; }
            set { m_strContentType = value; }
        }


    }

    public class RTSPContentTypeHeader : RTSPHeader
    {
        public RTSPContentTypeHeader()
            : base("Content-Type", "Content-Type")
        {
        }

        public RTSPContentTypeHeader(string strContentType)
            : base("Content-Type", "Content-Type")
        {
            ContentType = strContentType;
        }


        public static Regex RegexContentType = new Regex(@"^Content-Type\: \s* (\S+) \s*$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public static RTSPHeader Parse(string strLine)
        {
            Match matchman = RegexContentType.Match(strLine);
            if (matchman.Success == true)
            {
                RTSPContentTypeHeader header = new RTSPContentTypeHeader(matchman.Groups[1].Value);
                return header;
            }
            return null;
        }

        private string m_strContentType = "application/octet-string";

        public string ContentType
        {
            get { return m_strContentType; }
            set { m_strContentType = value; }
        }


    }

    public class RTSPContentLengthHeader : RTSPHeader
    {
        public RTSPContentLengthHeader()
            : base("Content-Length", "l")
        {
        }

        public RTSPContentLengthHeader(int nLength)
            : base("Content-Length", "l")
        {
            ContentLength = nLength;
        }


        public static Regex RegexCcontentLength = new Regex(@"^Content-Length\: \s* (\d+) \s*$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public static RTSPHeader Parse(string strLine)
        {
            Match matchman = RegexCcontentLength.Match(strLine);
            if (matchman.Success == true)
            {
                int nNumber = Convert.ToInt32(matchman.Groups[1].Value);
                RTSPContentLengthHeader header = new RTSPContentLengthHeader(nNumber);
                return header;
            }
            return null;
        }

        private int m_nContentLength = 0;

        public int ContentLength
        {
            get { return m_nContentLength; }
            set { m_nContentLength = value; }
        }


    }

    public class RTSPContentBaseHeader : RTSPHeader
    {
        public RTSPContentBaseHeader()
            : base("Content-Base", "Content-Base")
        {
        }

        public RTSPContentBaseHeader(string strContentBase)
            : base("Content-Base", "Content-Base")
        {
            ContentBase = new Uri(strContentBase);
        }


        public static Regex RegexContentBase = new Regex(@"^Content-Base\: \s* (\S+) \s*$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public static RTSPHeader Parse(string strLine)
        {
            Match matchman = RegexContentBase.Match(strLine);
            if (matchman.Success == true)
            {
                RTSPContentTypeHeader header = new RTSPContentTypeHeader(matchman.Groups[1].Value);
                return header;
            }
            return null;
        }

        private Uri m_uriContentBase = new Uri("rtsp://0.0.0.0/0/");

        public Uri ContentBase
        {
            get { return m_uriContentBase; }
            set { m_uriContentBase = value; }
        }


    }


    public class RTSPBaseMessage
    {
        public RTSPBaseMessage()
        {
        }

        private List<RTSPHeader> m_listHeaders = new List<RTSPHeader>();
        public List<RTSPHeader> Headers
        {
            get { return m_listHeaders; }
            set { m_listHeaders = value; }
        }
    }
}
