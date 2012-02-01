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

namespace SocketServer.TLS
{
    public class TLSApplicationMessage : TLSMessage
    {
        public TLSApplicationMessage()
        {
        }

        public override void DebugDump(bool bReceived)
        {
            System.Diagnostics.Debug.WriteLine("{0} TLSApplicationMessage ApplicationData Encrypted: \r\n{1}", bReceived ? "<--" : "-->", ByteHelper.HexStringFromByte(ApplicationData, true, 16));
        }



        byte[] m_bApplicationData = new byte[] { };
        public byte[] ApplicationData
        {
            get { return m_bApplicationData; }
            set { m_bApplicationData = value; }
        }

        public override byte[] Bytes
        {
            get
            {
                return ApplicationData;
            }
        }


        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public override uint ReadFromArray(byte[] bData, int nStartAt)
        {
            ApplicationData = bData;
            return (uint)bData.Length;
        }
    }
}
