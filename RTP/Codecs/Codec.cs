using System;
using System.Net;


namespace RTP
{
    /// <summary>
    ///  Base class for codecs
    /// </summary>
    public class Codec
    {

        public Codec(string strName)
        {
            Name = strName;
        }

        private string m_strName = "Unknown";

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        private int m_nPTime = 20;

        public int PTime
        {
            get { return m_nPTime; }
            set { m_nPTime = value; }
        }
        private int m_nPayloadType = 0;

        public int PayloadType
        {
            get { return m_nPayloadType; }
            set { m_nPayloadType = value; }
        }

        public virtual RTPPacket[] Encode(short[] sData)
        {
            return null;
        }

        public virtual short[] DecodeToShorts(RTPPacket packet)
        {
            return null;
        }

        public virtual byte[] DecodeToBytes(RTPPacket packet)
        {
            return null;
        }
        
    }
}
