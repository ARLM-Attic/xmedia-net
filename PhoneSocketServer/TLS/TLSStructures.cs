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

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SocketServer.TLS
{
    public enum ClientTLSState
    {
        None,
        SentClientHello,
        ReceivedServerHello,
        ReceivedServerHelloDone,
        SentClientKeyExchange,
        SentClientFinished,
        ApplicationData,
    }

    
    public enum ConnectionEnd
    {
        client,
        server
    }

    public enum BulkEncryptionAlgorithm
    {
        nul, //null
        rc4,
        rc2,
        des,
        threedes, // 3des
        des40,
        idea,
        aes
    }

    public enum CipherType
    {
        stream,
        block
    }

    public enum MACAlgorithm
    {
        nul,
        md5,
        sha,
    }

    public enum CipherAlgorithm
    {
        
    }

    public enum CompressionMethod : byte
    {
        null0 = 0,
        TwoFiftyFive = 255,
    }

    public class SecurityParameters
    {
        public SecurityParameters()
        {
        }

        private ConnectionEnd m_eConnectionEnd = ConnectionEnd.client;
        public ConnectionEnd ConnectionEnd
        {
            get { return m_eConnectionEnd; }
            set { m_eConnectionEnd = value; }
        }

        private Cipher m_objCipher = null;

        public Cipher Cipher
        {
            get { return m_objCipher; }
            set { m_objCipher = value; }
        }


     
        private CompressionMethod m_eCompressionMethod = CompressionMethod.null0;
        public CompressionMethod CompressionMethod
        {
            get { return m_eCompressionMethod; }
            set { m_eCompressionMethod = value; }
        }

        private byte[] m_bMasterSecret = new byte[48];
        public byte[] MasterSecret
        {
            get { return m_bMasterSecret; }
            set { m_bMasterSecret = value; }
        }

        private byte[] m_bClientRandom = new byte[32];
        public byte[] ClientRandom
        {
            get { return m_bClientRandom; }
            set { m_bClientRandom = value; }
        }

        private byte[] m_bServerRandom = new byte[32];
        public byte[] ServerRandom
        {
            get { return m_bServerRandom; }
            set { m_bServerRandom = value; }
        }

        public byte[] SessionIdentifier = new byte[] { };
        public X509Certificate PeerCertificate = new X509Certificate();
        private bool m_bIsResumable = false;

        public bool IsResumable
        {
            get { return m_bIsResumable; }
            set { m_bIsResumable = value; }
        }
    }
    
    //client write MAC secret
    //server write MAC secret
    //client write key
    //write key

    public class ConnectionState
    {
        public ConnectionState()
        {
        }

        public SecurityParameters SecurityParameters = new SecurityParameters();

        public byte[] ClientWriteMACSecret = null;
        public byte[] ServerWriteMACSecret = null;
        public byte[] ClientWriteKey = null;
        public byte[] ServerWriteKey = null;

        public byte[] ServerWriteIV = null; 
        public byte[] ClientWriteIV = null;  //initialization vector

        public byte[] LastCipherTextBlock = null;

        Mono.Security.Cryptography.SymmetricAlgorithm EncryptionAlgorithm = null;
        ICryptoTransform EncryptionTransform = null;

        Mono.Security.Cryptography.SymmetricAlgorithm DecryptionAlgorithm = null;
        ICryptoTransform DecryptionTransform = null;

        //SymmetricAlgorithm EncryptionAlgorithm = null;
        //ICryptoTransform EncryptionTransform = null;

        //SymmetricAlgorithm DecryptionAlgorithm = null;
        //ICryptoTransform DecryptionTransform = null;

        KeyedHashAlgorithm ClientHMAC = null;
        KeyedHashAlgorithm ServerHMAC = null;


        /// We're not currently reporting pending connectinos states because we're only negotiating 1 cipher spec
        ///  we can add in the future if we see the need, but for now we'll just have encryption active or not
        /// 


        private bool m_bSendEncryptionActive = false;
       
        public bool SendEncryptionActive
        {
            get { return m_bSendEncryptionActive; }
            set { m_bSendEncryptionActive = value; }
        }
        private bool m_bReceiveEncryptionActive = false;

        public bool ReceiveEncryptionActive
        {
            get { return m_bReceiveEncryptionActive; }
            set { m_bReceiveEncryptionActive = value; }
        }

        public void Initialize()
        {
            if (SecurityParameters.Cipher.BulkEncryptionAlgorithm == TLS.BulkEncryptionAlgorithm.aes)
            {
                //EncryptionAlgorithm = new AesManaged();
                EncryptionAlgorithm = new Mono.Security.Cryptography.RijndaelManaged();
                EncryptionAlgorithm.BlockSize = SecurityParameters.Cipher.BlockSizeBytes * 8;
                EncryptionAlgorithm.KeySize = SecurityParameters.Cipher.KeySizeBits;
                EncryptionAlgorithm.Key = ClientWriteKey;
                EncryptionAlgorithm.IV = ClientWriteIV;
                EncryptionAlgorithm.Padding = Mono.Security.Cryptography.PaddingMode.None;
                //EncryptionTransform = EncryptionAlgorithm.CreateEncryptor(ClientWriteKey, ClientWriteIV);
                EncryptionTransform = EncryptionAlgorithm.CreateEncryptor();

                //Mono.Security.Cryptography.RSAManaged DecryptionAlgorithm = new Mono.Security.Cryptography.RSAManaged();
                DecryptionAlgorithm = new Mono.Security.Cryptography.RijndaelManaged();
                //DecryptionAlgorithm = new AesManaged();
                DecryptionAlgorithm.BlockSize = SecurityParameters.Cipher.BlockSizeBytes * 8;
                DecryptionAlgorithm.KeySize = SecurityParameters.Cipher.KeySizeBits;
                DecryptionAlgorithm.Key = ServerWriteKey;
                DecryptionAlgorithm.IV = ServerWriteIV;
                LastCipherTextBlock = ServerWriteIV;
                DecryptionAlgorithm.Padding = Mono.Security.Cryptography.PaddingMode.None;
                DecryptionTransform = DecryptionAlgorithm.CreateDecryptor(ServerWriteKey, ServerWriteIV);
            }
            /// TODO, add other sym encryption methods

            if (SecurityParameters.Cipher.MACAlgorithm == MACAlgorithm.sha)
            {
                ClientHMAC = new HMACSHA1(ClientWriteMACSecret);
                ServerHMAC = new HMACSHA1(ServerWriteMACSecret);
            }
        }


        /// Compression State
        /// cipher state
        /// MAC secret
        /// sequence number
        /// 
        public UInt64 ReadSequenceNumber = 0;
        public UInt64 WriteSequenceNumber = 0;


        public byte[] CompressEncryptOutgoingData(TLSRecord record)
        {
            if (SendEncryptionActive == false)
                return record.Content;
            /// TODO (or not).  We don't compress yet
            /// 

            ///Encrypt the compressed data.  The new data is now the encrypted data + a MAC.  
            /// Compute the MAC before encrypting
            /// 

            ///   The MAC is generated as:
            ///    HMAC_hash(MAC_write_secret, seq_num + TLSCompressed.type +
            ///                  TLSCompressed.version + TLSCompressed.length +
            ///                  TLSCompressed.fragment));
            /// where "+" denotes concatenation.
            /// 
            byte [] bTLSRecord = record.Bytes;
            byte[] bDataToBeHashed = new byte[8 + bTLSRecord.Length];
            byte [] bSequence = new byte[8];
            bSequence[0] = (byte) ((WriteSequenceNumber & 0xFF00000000000000) >> 56);
            bSequence[1] = (byte)((WriteSequenceNumber & 0x00FF000000000000) >> 48);
            bSequence[2] = (byte)((WriteSequenceNumber & 0x0000FF0000000000) >> 40);
            bSequence[3] = (byte)((WriteSequenceNumber & 0x000000FF00000000) >> 32);
            bSequence[4] = (byte)((WriteSequenceNumber & 0x00000000FF000000) >> 24);
            bSequence[5] = (byte)((WriteSequenceNumber & 0x0000000000FF0000) >> 16);
            bSequence[6] = (byte)((WriteSequenceNumber & 0x000000000000FF00) >> 08);
            bSequence[7] = (byte)((WriteSequenceNumber & 0x00000000000000FF) >> 00);
            Array.Copy(bSequence, 0, bDataToBeHashed, 0, 8);
            Array.Copy(bTLSRecord, 0, bDataToBeHashed, 8, bTLSRecord.Length);

            //ClientHMAC = new HMACSHA1(ClientWriteMACSecret);
            byte[] bMac = ClientHMAC.ComputeHash(bDataToBeHashed);

            
            /// GenericBlockCipher
           ///  block-ciphered struct 
           ///  {
           ///    opaque content[TLSCompressed.length];
           ///    opaque MAC[CipherSpec.hash_size];
           ///    uint8 padding[GenericBlockCipher.padding_length];
           ///    uint8 padding_length;
           ///   } 
           ///   

            byte[] bEncryptedRecord = EncryptRecord(record.Content, bMac);


            WriteSequenceNumber++;

            return bEncryptedRecord;
            //throw new Exception("Not finished");
            //return null;
        }

        public byte[] EncryptRecord(byte[] bContent, byte[] bMAC)
        {

            // Encryption ( fragment + mac [+ padding + padding_length] )
            int length = bContent.Length + bMAC.Length;
            int padlen = 0;
            // Calculate padding_length
            length++; // keep an extra byte
            padlen = (this.SecurityParameters.Cipher.BlockSizeBytes - length % this.SecurityParameters.Cipher.BlockSizeBytes);
            if (padlen == this.SecurityParameters.Cipher.BlockSizeBytes)
            {
                padlen = 0;
            }
            length += padlen;

            byte[] plain = new byte[length];
            Array.Copy(bContent, 0, plain, 0, bContent.Length);
            Array.Copy(bMAC, 0, plain, bContent.Length, bMAC.Length);
            if (padlen > 0)
            {
                int start = bContent.Length + bMAC.Length;
                for (int i = start; i < (start + padlen + 1); i++)
                {
                    plain[i] = (byte)padlen;
                }
            }


            //plain = this.EncryptionTransform.TransformFinalBlock(plain, 0, plain.Length);
            this.EncryptionTransform.TransformBlock(plain, 0, plain.Length, plain, 0);
            return plain;
        }


        public TLSRecord DecompressRecord(TLSRecord record)
        {
            if (ReceiveEncryptionActive == false)
            {
                ReadSequenceNumber++;
                return record;
            }

            /// Decompress, then verify hmac and seqnumber
            /// 
            byte [] bFragment = null;
            byte [] bMAC = null;

            byte []bCipherTextBlock = record.RawSetContent; 
            /// Store the cipher text block so we can use it as the next IV  (last block from the previous record)
                                                            /// 
            if (record.RawSetContent.Length > SecurityParameters.Cipher.BlockSizeBytes)
            {
                bCipherTextBlock = new byte[SecurityParameters.Cipher.BlockSizeBytes];
                Array.Copy(record.RawSetContent, record.RawSetContent.Length - SecurityParameters.Cipher.BlockSizeBytes, bCipherTextBlock, 0, SecurityParameters.Cipher.BlockSizeBytes);
            }
            
            DecryptRecord(record.RawSetContent, out bFragment, out bMAC);

            record.RawSetContent = bFragment;

            /// Compute the MAC ourselves so we can see if the man is correct
            ///Encrypt the compressed data.  The new data is now the encrypted data + a MAC.  
            /// Compute the MAC before encrypting
            /// 

            ///   The MAC is generated as:
            ///    HMAC_hash(MAC_write_secret, seq_num + TLSCompressed.type +
            ///                  TLSCompressed.version + TLSCompressed.length +
            ///                  TLSCompressed.fragment));
            /// where "+" denotes concatenation.
            /// 
            byte[] bTLSRecord = record.BytesFromRawContent;
            byte[] bDataToBeHashed = new byte[8 + bTLSRecord.Length];
            byte[] bSequence = new byte[8];
            bSequence[0] = (byte)((ReadSequenceNumber & 0xFF00000000000000) >> 56);
            bSequence[1] = (byte)((ReadSequenceNumber & 0x00FF000000000000) >> 48);
            bSequence[2] = (byte)((ReadSequenceNumber & 0x0000FF0000000000) >> 40);
            bSequence[3] = (byte)((ReadSequenceNumber & 0x000000FF00000000) >> 32);
            bSequence[4] = (byte)((ReadSequenceNumber & 0x00000000FF000000) >> 24);
            bSequence[5] = (byte)((ReadSequenceNumber & 0x0000000000FF0000) >> 16);
            bSequence[6] = (byte)((ReadSequenceNumber & 0x000000000000FF00) >> 08);
            bSequence[7] = (byte)((ReadSequenceNumber & 0x00000000000000FF) >> 00);
            Array.Copy(bSequence, 0, bDataToBeHashed, 0, 8);
            Array.Copy(bTLSRecord, 0, bDataToBeHashed, 8, bTLSRecord.Length);

            byte[] bMACComputed = ServerHMAC.ComputeHash(bDataToBeHashed);
            ReadSequenceNumber++;
            LastCipherTextBlock = bCipherTextBlock;

            if (ByteHelper.CompareArrays(bMAC, bMACComputed) == false)
            {
                System.Threading.Thread.Sleep(0);
                //throw new Exception("Computed MAC did not match incoming record's MAC");
            }


            return record;
        }

      

        public void DecryptRecord(byte[] fragment, out byte[] dcrFragment, out byte[] dcrMAC)
        {
            int fragmentSize = 0;
            int paddingLength = 0;

            // Decrypt message fragment ( fragment + mac [+ padding + padding_length] )
            //byte[] bOutput = new byte[fragment.Length];

            DecryptionTransform = DecryptionAlgorithm.CreateDecryptor(ServerWriteKey, LastCipherTextBlock);
            byte [] bOutput = this.DecryptionTransform.TransformFinalBlock(fragment, 0, fragment.Length);
            DecryptionTransform.Dispose();
            
            //this.DecryptionTransform.TransformBlock(fragment, 0, fragment.Length, bOutput, 0);

            if (bOutput.Length != fragment.Length)
            {
                /// It appears the above may remove all the padding, but lease the padding length byte on
                /// 
                dcrFragment = new byte[bOutput.Length - this.SecurityParameters.Cipher.HashSize-1];
                dcrMAC = new byte[this.SecurityParameters.Cipher.HashSize];

                Array.Copy(bOutput, 0, dcrFragment, 0, dcrFragment.Length);
                Array.Copy(bOutput, dcrFragment.Length, dcrMAC, 0, dcrMAC.Length);
                return;
            }

            fragment = bOutput;

            // optimization: decrypt "in place", worst case: padding will reduce the size of the data
            // this will cut in half the memory allocations (dcrFragment and dcrMAC remains)

            // Calculate fragment size
            // Calculate padding_length
            paddingLength = fragment[fragment.Length - 1];


            ///// Not sure what's going on, but the Server Finished handshake message has it's padding byte set to 0, and there are 11 bytes filled with 0s, 
            ///// I can't find documentation on why, so have to assume it's a bug in openssl... see if we can work around it
            ///// 
            //if (paddingLength == 0)
            //    paddingLength = 11;

            if (paddingLength > fragment.Length)
                throw new Exception("Decryption Failed, padding length is longer than the fragment length");

            fragmentSize = (fragment.Length - (paddingLength + 1)) - this.SecurityParameters.Cipher.HashSize;


            dcrFragment = new byte[fragmentSize];
            dcrMAC = new byte[this.SecurityParameters.Cipher.HashSize];

            Array.Copy(fragment, 0, dcrFragment, 0, dcrFragment.Length);
            Array.Copy(fragment, dcrFragment.Length, dcrMAC, 0, dcrMAC.Length);
        }
        
    }


}
