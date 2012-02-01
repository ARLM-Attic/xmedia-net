using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;

namespace RTP
{
    public enum StunClass : byte
    {
        Request = 0,
        Inidication = 1,
        Success = 2,
        Error = 3,
    }

    public enum StunMethod : ushort
    {
        Binding = 1,
    }

    public enum StunAttributeType : ushort
    {
        Reserved = 0x0000,
        MappedAddress = 0x0001,
        LegacyResponseAddress = 0x0002,
        LegacyChangeAddress = 0x0003,
        LegacySourceAddress = 0x0004,
        LegacyChangedAddress = 0x0005,
        UserName = 0x0006,
        MessageIntegrity = 0x0008,
        ErrorCode = 0x0009,
        UnknownAttributes = 0x000A,
        Realm = 0x0014,
        Nonce = 0x0015,
        XorMappedAddress = 0x0020,
        Software = 0x8022,
        AlternateServer = 0x8023,
        Fingerprint = 0x8028
    }

    public class STUNAttribute
    {
        public STUNAttribute(StunAttributeType nType)
        {
            Type = nType;
        }
        public STUNAttribute()
        {
        }


        protected StunAttributeType m_nType = 0;
        public StunAttributeType Type
        {
            get { return m_nType; }
            set { m_nType = value; }
        }

        protected byte[] m_bBytes = new byte[] { };
        public virtual byte[] GetBytes(STUNMessage parentmessage)
        {
            return m_bBytes;
        }
        public virtual void SetBytes(byte[] bBytes, STUNMessage parentmessage)
        {
            m_bBytes = bBytes;
        }
    }

    public enum StunAddressFamily : byte
    {
        IPv4 = 1,
        IPv6 = 2,
    }

    public class MappedAddressAttribute : STUNAttribute
    {
        public MappedAddressAttribute()
            : base(StunAttributeType.MappedAddress)
        {
        }

        private StunAddressFamily m_eAddressFamily = StunAddressFamily.IPv4;
        public StunAddressFamily AddressFamily
        {
            get { return m_eAddressFamily; }
            set { m_eAddressFamily = value; }
        }

        protected ushort m_nPort = 0x00;

        public virtual ushort Port
        {
            get { return m_nPort; }
            set { m_nPort = value; }
        }

        protected IPAddress m_objIPAddress = new IPAddress(0);

        public virtual IPAddress IPAddress
        {
            get { return m_objIPAddress; }
            set { m_objIPAddress = value; }
        }



        public override byte[] GetBytes(STUNMessage parentmessage)
        {
            byte[] bIPaddress = IPAddress.GetAddressBytes();
            m_bBytes = new byte[4 + bIPaddress.Length];
            m_bBytes[0] = 0;
            m_bBytes[1] = (byte)AddressFamily;
            m_bBytes[2] = (byte)((Port & 0xFF00) >> 8);
            m_bBytes[3] = (byte)(Port & 0xFF);
            Array.Copy(bIPaddress, 0, m_bBytes, 4, 4);

            return m_bBytes;
        }

        public override void SetBytes(byte[] bBytes, STUNMessage parentmessage)
        {
            m_bBytes = bBytes;
            if (m_bBytes.Length < 8) // somethin rong
                return;

            AddressFamily = (StunAddressFamily)m_bBytes[1];
            Port = (ushort)((m_bBytes[2] << 8) | (m_bBytes[3]));

            int nIPLength = 4;
            if (AddressFamily == RTP.StunAddressFamily.IPv6)
                nIPLength = 16;
            byte[] bIPAddress = new byte[nIPLength];

            Array.Copy(m_bBytes, 4, bIPAddress, 0, nIPLength);
            IPAddress = new IPAddress(bIPAddress);
        }
    }

    public class AlternateServerAttribute : MappedAddressAttribute
    {
        public AlternateServerAttribute()
            : base()
        {
            Type = StunAttributeType.AlternateServer;
        }
    }

    public class LegacySourceAddressAttribute : MappedAddressAttribute
    {
        public LegacySourceAddressAttribute()
            : base()
        {
            Type = StunAttributeType.LegacySourceAddress;
        }
    }

    public class LegacyResponseAddressAttribute : MappedAddressAttribute
    {
        public LegacyResponseAddressAttribute()
            : base()
        {
            Type = StunAttributeType.LegacyResponseAddress;
        }
    }

    public class LegacyChangeAddressAttribute : STUNAttribute
    {
        public LegacyChangeAddressAttribute()
            : base()
        {
            Type = StunAttributeType.LegacyChangeAddress;
        }

        private bool m_bChangeIP = false;

        public bool ChangeIP
        {
            get { return m_bChangeIP; }
            set { m_bChangeIP = value; }
        }

        private bool m_bChangePort = false;

        public bool ChangePort
        {
            get { return m_bChangePort; }
            set { m_bChangePort = value; }
        }

        public override byte[] GetBytes(STUNMessage parentmessage)
        {
            m_bBytes = new byte[4];
            if (ChangeIP == true)
                m_bBytes[3] |= 0x04;
            if (ChangePort == true)
                m_bBytes[3] |= 0x02;

            return m_bBytes;
        }

        public override void SetBytes(byte[] bBytes, STUNMessage parentmessage)
        {
            ChangeIP = ((m_bBytes[3] & 0x04) > 0) ? true : false;
            ChangePort = ((m_bBytes[3] & 0x02) > 0) ? true : false;
        }

    }

      public class LegacyChangedAddressAttribute : MappedAddressAttribute
    {
        public LegacyChangedAddressAttribute()
            : base()
        {
            Type = StunAttributeType.LegacyChangedAddress;
        }
    }

    public class XORMappedAddressAttribute : MappedAddressAttribute
    {
        public XORMappedAddressAttribute()
            : base()
        {
            Type = StunAttributeType.XorMappedAddress;
        }

        public override byte[] GetBytes(STUNMessage parentmessage)
        {
            byte[] bMagicCookie = new byte[4];
            bMagicCookie[0] = (byte)((parentmessage.MagicCookie & 0xFF000000) >> 24);
            bMagicCookie[1] = (byte)((parentmessage.MagicCookie & 0x00FF0000) >> 16);
            bMagicCookie[2] = (byte)((parentmessage.MagicCookie & 0x0000FF00) >> 08);
            bMagicCookie[3] = (byte)((parentmessage.MagicCookie & 0x000000FF) >> 00);

            byte[] bIPAddress = IPAddress.GetAddressBytes();
            m_bBytes = new byte[4 + bIPAddress.Length];
            m_bBytes[0] = 0;
            m_bBytes[1] = (byte)AddressFamily;
            m_bBytes[2] = (byte)(((Port & 0xFF00) >> 8) ^ bMagicCookie[0]);
            m_bBytes[3] = (byte)((Port & 0xFF) ^ bMagicCookie[1]);

            byte[] bXOR = new byte[16];
            Array.Copy(bMagicCookie, 0, bXOR, 0, 4);
            Array.Copy(parentmessage.TransactionId, 0, bXOR, 4, parentmessage.TransactionId.Length);
            for (int i = 0; i < bIPAddress.Length; i++)
            {
                bIPAddress[i] ^= bXOR[i];
            }


            Array.Copy(bIPAddress, 0, m_bBytes, 4, bIPAddress.Length);

            return m_bBytes;
        }

        public override void SetBytes(byte[] bBytes, STUNMessage parentmessage)
        {
            m_bBytes = bBytes;
            if (m_bBytes.Length < 8) // somethin rong
                return;

            byte[] bMagicCookie = new byte[4];
            bMagicCookie[0] = (byte)((parentmessage.MagicCookie & 0xFF000000) >> 24);
            bMagicCookie[1] = (byte)((parentmessage.MagicCookie & 0x00FF0000) >> 16);
            bMagicCookie[2] = (byte)((parentmessage.MagicCookie & 0x0000FF00) >> 08);
            bMagicCookie[3] = (byte)((parentmessage.MagicCookie & 0x000000FF) >> 00);

            AddressFamily = (StunAddressFamily)m_bBytes[1];
            Port = (ushort)(((m_bBytes[2] << 8) ^ bMagicCookie[0]) | (m_bBytes[3] ^ bMagicCookie[1]));

            int nIPLength = 4;
            if (AddressFamily == RTP.StunAddressFamily.IPv6)
                nIPLength = 16;
            byte[] bIPAddress = new byte[nIPLength];

            Array.Copy(m_bBytes, 4, bIPAddress, 0, nIPLength);

            byte[] bXOR = new byte[16];
            Array.Copy(bMagicCookie, 0, bXOR, 0, 4);
            Array.Copy(parentmessage.TransactionId, 0, bXOR, 4, parentmessage.TransactionId.Length);
            for (int i = 0; i < bIPAddress.Length; i++)
            {
                bIPAddress[i] ^= bXOR[i];
            }


            IPAddress = new IPAddress(bIPAddress);
        }

    }


    public class UserNameAttribute : STUNAttribute
    {
        public UserNameAttribute()
            : base(StunAttributeType.UserName)
        {
        }

        private string m_strUserName = "";

        public string UserName
        {
            get { return m_strUserName; }
            set { m_strUserName = value; }
        }


        public override byte[] GetBytes(STUNMessage parentmessage)
        {
            m_bBytes = System.Text.UTF8Encoding.UTF8.GetBytes(UserName);
            return m_bBytes;
        }

        public override void SetBytes(byte[] bBytes, STUNMessage parentmessage)
        {
            m_bBytes = bBytes;
            UserName = System.Text.UTF8Encoding.UTF8.GetString(m_bBytes, 0, m_bBytes.Length);
        }
    }

    public class MessageIntegrityAttribute : STUNAttribute
    {
        public MessageIntegrityAttribute()
            : base(StunAttributeType.MessageIntegrity)
        {
        }

        public byte[] HMAC
        {
            get { return m_bBytes; }
            set { m_bBytes = value; }
        }

        public void ComputeHMAC(byte[] bMessageBytes, string strUserName, string strRealm, string strPassword)
        {
            /// TODO... see section 15.4 RFC 5389
            throw new NotImplementedException();
        }


        public override byte[] GetBytes(STUNMessage parentmessage)
        {
            return m_bBytes;
        }

        public override void SetBytes(byte[] bBytes, STUNMessage parentmessage)
        {
            m_bBytes = bBytes;
        }
    }

    public class ErrorCodeAttribute : STUNAttribute
    {
        public ErrorCodeAttribute()
            : base(StunAttributeType.ErrorCode)
        {
        }

        private byte m_bClass = 0;

        public byte Class
        {
            get { return m_bClass; }
            set { m_bClass = value; }
        }
        private byte m_bNumber = 0;

        public byte Number
        {
            get { return m_bNumber; }
            set { m_bNumber = value; }
        }
        private string m_strReasonPhrase = "";

        public string ReasonPhrase
        {
            get { return m_strReasonPhrase; }
            set { m_strReasonPhrase = value; }
        }

        public override byte[] GetBytes(STUNMessage parentmessage)
        {
            byte[] bPhrase = System.Text.UTF8Encoding.UTF8.GetBytes(ReasonPhrase);
            m_bBytes = new byte[bPhrase.Length + 4];
            m_bBytes[2] = (byte)(Class & 0x07);
            m_bBytes[2] = (byte)Number;
            Array.Copy(bPhrase, 0, m_bBytes, 1, bPhrase.Length);
            return m_bBytes;
        }

        public override void SetBytes(byte[] bBytes, STUNMessage parentmessage)
        {
            m_bBytes = bBytes;
            Class = (byte)(bBytes[2] & 0x07);
            Number = bBytes[3];
            ReasonPhrase = System.Text.UTF8Encoding.UTF8.GetString(m_bBytes, 1, m_bBytes.Length - 1);
        }
    }


    public class RealmAttribute : STUNAttribute
    {
        public RealmAttribute()
            : base(StunAttributeType.Realm)
        {
        }

        private string m_strRealm = "";

        public string Realm
        {
            get { return m_strRealm; }
            set { m_strRealm = value; }
        }


        public override byte[] GetBytes(STUNMessage parentmessage)
        {
            m_bBytes = System.Text.UTF8Encoding.UTF8.GetBytes(Realm);
            return m_bBytes;
        }

        public override void SetBytes(byte[] bBytes, STUNMessage parentmessage)
        {
            m_bBytes = bBytes;
            Realm = System.Text.UTF8Encoding.UTF8.GetString(m_bBytes, 0, m_bBytes.Length);
        }
    }

    public class NonceAttribute : STUNAttribute
    {
        public NonceAttribute()
            : base(StunAttributeType.Nonce)
        {
        }

        private string m_strNonce = "";

        public string Nonce
        {
            get { return m_strNonce; }
            set { m_strNonce = value; }
        }


        public override byte[] GetBytes(STUNMessage parentmessage)
        {
            m_bBytes = System.Text.UTF8Encoding.UTF8.GetBytes(Nonce);
            return m_bBytes;
        }

        public override void SetBytes(byte[] bBytes, STUNMessage parentmessage)
        {
            m_bBytes = bBytes;
            Nonce = System.Text.UTF8Encoding.UTF8.GetString(m_bBytes, 0, m_bBytes.Length);
        }
    }


    public class SoftwareAttribute : STUNAttribute
    {
        public SoftwareAttribute()
            : base(StunAttributeType.Software)
        {
        }

        private string m_strSoftware = "";

        public string Software
        {
            get { return m_strSoftware; }
            set { m_strSoftware = value; }
        }


        public override byte[] GetBytes(STUNMessage parentmessage)
        {
            m_bBytes = System.Text.UTF8Encoding.UTF8.GetBytes(Software);
            return m_bBytes;
        }

        public override void SetBytes(byte[] bBytes, STUNMessage parentmessage)
        {
            m_bBytes = bBytes;
            Software = System.Text.UTF8Encoding.UTF8.GetString(m_bBytes, 0, m_bBytes.Length);
        }
    }

  

    public class STUNAttributeContainer
    {
        public STUNAttributeContainer()
        {
        }

        public STUNAttributeContainer(STUNAttribute attr)
        {
            SetAttribute(attr);
        }

        void SetAttribute(STUNAttribute attr)
        {
            StunAttributeType = attr.Type;
            m_objParsedAttribute = attr;
        }

        private StunAttributeType m_eStunAttributeType = StunAttributeType.UnknownAttributes;

        public StunAttributeType StunAttributeType
        {
            get { return m_eStunAttributeType; }
            set { m_eStunAttributeType = value; }
        }
        private ushort m_nLength;

        public ushort Length
        {
            get { return m_nLength; }
            set { m_nLength = value; }
        }
        public byte[] m_bValue = new byte[] { };
        private STUNAttribute m_objParsedAttribute = new STUNAttribute();

        public STUNAttribute ParsedAttribute
        {
            get { return m_objParsedAttribute; }
            set { m_objParsedAttribute = value; }
        }


        public byte[] GetBytes(STUNMessage parentmessage)
        {
            byte[] bAttribute = m_objParsedAttribute.GetBytes(parentmessage);
            m_bValue = new byte[bAttribute.Length + 4];
            Length = (ushort) bAttribute.Length;
            m_bValue[0] = (byte)(((int)m_objParsedAttribute.Type & 0xFF00) >> 8);
            m_bValue[1] = (byte)(((int)m_objParsedAttribute.Type & 0x00FF) >> 0);

            m_bValue[2] = (byte)(((int)Length & 0xFF00) >> 8);
            m_bValue[3] = (byte)(((int)Length & 0x00FF) >> 0);

            Array.Copy(bAttribute, 0, m_bValue, 4, bAttribute.Length);

            return m_bValue;
        }

        /// <summary>
        /// Read the next attribute from the stream
        /// </summary>
        /// <param name="bBytes"></param>
        /// <param name="parentmessage"></param>
        /// <returns>The number of bytes read, or 0 if no more can be read</returns>
        public int ReadFromBytes(byte[] bBytes, int nAt, STUNMessage parentmessage)
        {
            if ((bBytes.Length - nAt) < 4)
                return 0;

            ushort nType = (ushort)((bBytes[0 + nAt] << 8) | bBytes[1 + nAt]);
            ushort nLength = (ushort)((bBytes[2 + nAt] << 8) | bBytes[3 + nAt]);

            if ((bBytes.Length-nAt) < (nLength - 4))
                return 0;

            byte[] bAttribute = new byte[nLength];
            Array.Copy(bBytes, 4+nAt, bAttribute, 0, bAttribute.Length);

            BuildAttribute(nType, bAttribute, parentmessage);

            return 4 + nLength;
        }

        void BuildAttribute(ushort nType, byte[] bAttribute, STUNMessage parentmessage)
        {
            if (nType == (ushort)StunAttributeType.MappedAddress)
            {
                m_objParsedAttribute = new MappedAddressAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }
            else if (nType == (ushort)StunAttributeType.AlternateServer)
            {
                m_objParsedAttribute = new AlternateServerAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }
            else if (nType == (ushort)StunAttributeType.ErrorCode)
            {
                m_objParsedAttribute = new ErrorCodeAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }
            else if (nType == (ushort)StunAttributeType.MessageIntegrity)
            {
                m_objParsedAttribute = new MessageIntegrityAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }
            else if (nType == (ushort)StunAttributeType.Nonce)
            {
                m_objParsedAttribute = new NonceAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }
            else if (nType == (ushort)StunAttributeType.Realm)
            {
                m_objParsedAttribute = new RealmAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }
            else if (nType == (ushort)StunAttributeType.Software)
            {
                m_objParsedAttribute = new SoftwareAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }
            else if (nType == (ushort)StunAttributeType.UserName)
            {
                m_objParsedAttribute = new UserNameAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }
            else if (nType == (ushort)StunAttributeType.XorMappedAddress)
            {
                m_objParsedAttribute = new XORMappedAddressAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }
            else if (nType == (ushort)StunAttributeType.LegacySourceAddress)
            {
                m_objParsedAttribute = new LegacySourceAddressAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }            
            else if (nType == (ushort)StunAttributeType.LegacyResponseAddress)
            {
                m_objParsedAttribute = new LegacyResponseAddressAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }            
            else if (nType == (ushort)StunAttributeType.LegacyChangeAddress)
            {
                m_objParsedAttribute = new LegacyChangeAddressAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }
            else if (nType == (ushort)StunAttributeType.LegacyChangedAddress)
            {
                m_objParsedAttribute = new LegacyChangedAddressAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }            
               
            else 
            {
                m_objParsedAttribute = new STUNAttribute();
                m_objParsedAttribute.SetBytes(bAttribute, parentmessage);
            }
        }
    }


    public class STUNMessage
    {
        public STUNMessage()
        {
            new Random().NextBytes(TransactionId);
            MagicCookie = ConstMagicCookie;
        }

        public STUNMessage(byte[] bTransactionId)
        {
            if ((bTransactionId == null) || (bTransactionId.Length <= 0))
                new Random().NextBytes(TransactionId);
            else
            {
                int nCopyLen = (bTransactionId.Length < 8) ? bTransactionId.Length : 8;
                Array.Copy(bTransactionId, 0, TransactionId, 0, nCopyLen);
            }
            MagicCookie = ConstMagicCookie;
        }

        public const uint ConstMagicCookie = 0x2112A442;

        private ushort m_nMessageType; // most significant 2 bits must be 0  (Network Byte order - big endian)

        /// <summary>
        ///  Used for setting the raw message type, Use Method and class to parse out these bits
        /// </summary>
        public ushort MessageType
        {
            get { return m_nMessageType; }
            set { m_nMessageType = value; }
        }

        // bits 0,1,2,3,5,6,8,9,10,11,12,13 of MessageType
        // pos  0,1,2,3,4,5,6,7,8, 9, 10,11
        public StunMethod Method
        {
            get
            {
                ushort nValue = (ushort)(GetBitValueOffsetToNewValue(MessageType, 0, 0) |
                                    GetBitValueOffsetToNewValue(MessageType, 1, 1) |
                                    GetBitValueOffsetToNewValue(MessageType, 2, 2) |
                                    GetBitValueOffsetToNewValue(MessageType, 3, 3) |
                                    GetBitValueOffsetToNewValue(MessageType, 4, 5) |
                                    GetBitValueOffsetToNewValue(MessageType, 5, 6) |
                                    GetBitValueOffsetToNewValue(MessageType, 6, 8) |
                                    GetBitValueOffsetToNewValue(MessageType, 7, 9) |
                                    GetBitValueOffsetToNewValue(MessageType, 8, 10) |
                                    GetBitValueOffsetToNewValue(MessageType, 9, 11) |
                                    GetBitValueOffsetToNewValue(MessageType, 10, 12) |
                                    GetBitValueOffsetToNewValue(MessageType, 11, 13));
                return (StunMethod)nValue;
            }
            set
            {
                ushort nValue = (ushort)value;
                // 1111 1110 1110 1111b = 0xFEEF
                // 0000 0001 0001 0000b = 0x110
                MessageType &= 0x110; /// Clear the bits, everything but the class

                if ((nValue & 0x01) > 0) // get bit 0, set at 0
                    MessageType |= (1 << 0);
                else if ((nValue & 0x02) > 0) // get bit 1, set at 1
                    MessageType |= (1 << 1);
                else if ((nValue & 0x04) > 0) // get bit 2, set at 2
                    MessageType |= (1 << 2);
                else if ((nValue & 0x08) > 0) // get bit 3, set at 3
                    MessageType |= (1 << 3);
                else if ((nValue & 0x10) > 0) // get bit 4, set at 5
                    MessageType |= (1 << 5);
                else if ((nValue & 0x20) > 0) // get bit 5, set at 6
                    MessageType |= (1 << 6);
                else if ((nValue & 0x40) > 0) // get bit 6, set at 8
                    MessageType |= (1 << 8);
                else if ((nValue & 0x80) > 0) // get bit 7, set at 9
                    MessageType |= (1 << 9);
                else if ((nValue & 0x100) > 0) // get bit 8, set at 10
                    MessageType |= (1 << 10);
                else if ((nValue & 0x200) > 0) // get bit 9, set at 11
                    MessageType |= (1 << 11);
                else if ((nValue & 0x400) > 0) // get bit 10, set at 12
                    MessageType |= (1 << 12);
                else if ((nValue & 0x800) > 0) // get bit 11, set at 13
                    MessageType |= (1 << 13);

            }
        }

        // bits 4,8 of MessageType
        public StunClass Class
        {
            get
            {
                byte bValue = (byte)(GetBitValueOffsetToNewValue(MessageType, 4, 0) |
                                    GetBitValueOffsetToNewValue(MessageType, 8, 1));
                bValue &= 0x3; // only bottom 2 bits mean anything

                return (StunClass)bValue;
            }
            set
            {
                byte bValue = (byte)value;
                // 0000 0001 0001 0000b = 0x110
                // 1111 1110 1110 1111b = 0xFEEF
                MessageType &= 0xFEEF; /// Clear the bits
                if ((bValue & 2) > 0)
                    MessageType |= 0x100;    // 1 0000 0000b
                if ((bValue & 1) > 0)
                    MessageType |= 0x010;    // 0 0001 0000b

            }
        }

        public static int GetBitValueOffsetToNewValue(int nValue, int nBitToGet, int nBitToPlace)
        {
            int nOr = (1 << nBitToGet);
            if ((nValue & nOr) > 0) /// bit is set
            {
                return (1 << nBitToPlace);
            }
            else
            {
                return 0;
            }
        }

        private ushort m_nMessageLength;
        /// <summary>
        ///  Size in bytes of the message not including the 20 byte header... We'll set this at build time and parse byte time
        /// </summary>
        public ushort MessageLength
        {
            get { return m_nMessageLength; }
            set { m_nMessageLength = value; }
        }

        private uint m_nMagicCookie;

        public uint MagicCookie
        {
            get { return m_nMagicCookie; }
            set { m_nMagicCookie = value; }
        }
        public byte[] TransactionId = new byte[12];

        public void AddAttribute(STUNAttribute attr)
        {
            STUNAttributeContainer cont = new STUNAttributeContainer(attr);
            Attributes.Add(cont);
        }


        /// 0 or more attributes (Type-Length-Value)
        /// 
        private List<STUNAttributeContainer> m_ListAttributes = new List<STUNAttributeContainer>();

        public List<STUNAttributeContainer> Attributes
        {
            get { return m_ListAttributes; }
            set { m_ListAttributes = value; }
        }



        public byte[] Bytes
        {
            get
            {
                int nTotalAttributeLength = 0;
                List<byte[]> AttributeBytes = new List<byte[]>();
                foreach (STUNAttributeContainer attr in Attributes)
                {
                    byte [] bAttrBytes = attr.GetBytes(this);
                    nTotalAttributeLength += bAttrBytes.Length;
                    AttributeBytes.Add(bAttrBytes);
                }

                MessageLength = (ushort) nTotalAttributeLength;

                byte[] bRet = new byte[20 + nTotalAttributeLength];
                bRet[0] = (byte)((MessageType & 0xFF00) >> 8);
                bRet[1] = (byte)((MessageType & 0x00FF) >> 0);

                bRet[2] = (byte)((MessageLength & 0xFF00) >> 8);
                bRet[3] = (byte)((MessageLength & 0x00FF) >> 0);
              
                bRet[4] = (byte)((MagicCookie & 0xFF000000) >> 24);
                bRet[5] = (byte)((MagicCookie & 0x00FF0000) >> 16);
                bRet[6] = (byte)((MagicCookie & 0x0000FF00) >> 08);
                bRet[7] = (byte)((MagicCookie & 0x000000FF) >> 00);

                Array.Copy(TransactionId, 0, bRet, 8, 12);

                int nAt = 20;
                foreach (byte[] bNextAttrBytes in AttributeBytes)
                {
                    Array.Copy(bNextAttrBytes, 0, bRet, nAt, bNextAttrBytes.Length);
                    nAt += bNextAttrBytes.Length;
                }
                return bRet;
            }
            set
            {
                if (value.Length < 20)
                    throw new Exception("Stun message must be at least 20 bytes");

                Attributes.Clear();

                /// Parse this out from the provide array
                /// 
                MessageType = (ushort)((value[0] << 8) | (value[1]));
                MessageLength = (ushort)((value[2] << 8) | (value[3]));
                MagicCookie = (uint)((value[4] << 24) | (value[5] << 16) | (value[6] << 8) | (value[7]));

                Array.Copy(value, 8, TransactionId, 0, 12);

                if (MessageLength <= 0)
                    return;

                byte[] bAttributes = new byte[MessageLength];
                Array.Copy(value, 20, bAttributes, 0, MessageLength);
                int nAt = 0;
                while (nAt < MessageLength)
                {
                    STUNAttributeContainer cont = new STUNAttributeContainer();
                    int nRead = cont.ReadFromBytes(bAttributes, nAt, this);

                    if (nRead <= 0)
                        break;

                    nAt += nRead;
                    Attributes.Add(cont);
                }

            }
        }

    }
}
