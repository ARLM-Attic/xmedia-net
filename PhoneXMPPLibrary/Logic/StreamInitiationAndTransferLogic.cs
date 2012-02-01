using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using System.IO;

using SocketServer;

namespace PhoneXMPPLibrary
{


    ///<iq id="vsjwT-32" to="test@ninethumbs.com/CALCULON" from="test2@ninethumbs.com/calculon" type="set">
    ///<si xmlns="http://jabber.org/protocol/si" id="jsi_1513143543466953357" mime-type="image/png" profile="http://jabber.org/protocol/si/profile/file-transfer">
    ///<file xmlns="http://jabber.org/protocol/si/profile/file-transfer" name="image_yd.png" size="7057">
    /// <desc>Sending file</desc>
    ///</file>
    ///<feature xmlns="http://jabber.org/protocol/feature-neg">
    ///<x xmlns="jabber:x:data" type="form">
    ///<field var="stream-method" type="list-single">
    ///<option>
    /// <value>http://jabber.org/protocol/bytestreams</value>
    /// </option>
    /// <option>
    /// <value>http://jabber.org/protocol/ibb</value>
    /// </option>
    /// </field>
    /// </x>
    /// </feature></si></iq>

    [Flags]
    public enum StreamOptions
    {
        none,
        bytestreams,
        ibb,
    }

    public enum StreamInitIQType
    {
        Offer,
        Result,
    }

    public class StreamInitIQ : IQ
    {
        public StreamInitIQ(StreamInitIQType type)
            : base()
        {
            StreamInitIQType = type;
        }

        public StreamInitIQ(string strXML)
            : base(strXML)
        {
        }

        public static StreamInitIQ BuildDefaultStreamInitOffer(string strFileName, int nFileSize, string strMimeType)
        {
            StreamInitIQ q = new StreamInitIQ(StreamInitIQType.Offer);
            q.mimetype = strMimeType;
            q.filename = strFileName;
            q.sid = GetNextId();
            q.filesize = nFileSize;
            q.StreamOptions = StreamOptions.bytestreams | StreamOptions.ibb;
            return q;
        }

        public static StreamInitIQ BuildDefaultStreamInitResult(StreamOptions choosenoption)
        {
            StreamInitIQ q = new StreamInitIQ(StreamInitIQType.Result);
            q.sid = null;
            q.mimetype = null;
            q.StreamOptions = choosenoption;
            return q;
        }

        public static int nBaseId = 2334;
        public static object objBaseIdLock = new object();
        public static string GetNextId()
        {
            lock (objBaseIdLock)
            {
                string strRet = nBaseId.ToString();
                nBaseId++;
                return strRet;
            }
        }

        public string mimetype = null;
        public string profile = "http://jabber.org/protocol/si/profile/file-transfer";
        public string sid = null;

        public string filename = null; // if present, we'll add the <file.. element
        internal string FullFileName = null; //internal use only
        public int filesize = 0;
        public string filehash = null;
        public string filedate = null;
        public string filedesc = "sending file";
        public StreamOptions StreamOptions = StreamOptions.bytestreams | StreamOptions.ibb;
        public StreamInitIQType StreamInitIQType = StreamInitIQType.Offer;

        public byte[] TemporaryFileBytes = null;

        public override void AddInnerXML(System.Xml.Linq.XElement elemMessage)
        {
            XElement elemSI = new XElement("{http://jabber.org/protocol/si}si");
            elemMessage.Add(elemSI);
            if (StreamInitIQType == StreamInitIQType.Offer)
            {
                elemSI.Add(new XAttribute("id", sid),  new XAttribute("profile", profile));
                if (mimetype != null)
                    elemSI.Add(new XAttribute("mime-type", mimetype));

                XElement elemfile = new XElement("{http://jabber.org/protocol/si/profile/file-transfer}file");
                elemSI.Add(elemfile);
                if (filename != null) elemfile.Add(new XAttribute("name", filename));
                if (filesize > 0) elemfile.Add(new XAttribute("size", filesize));
                if (filehash != null) elemfile.Add(new XAttribute("hash", filehash));
                if (filedesc != null) elemfile.Add(new XElement("{http://jabber.org/protocol/si/profile/file-transfer}desc", filedesc));

                XElement elemfeature = new XElement("{http://jabber.org/protocol/feature-neg}feature");
                elemSI.Add(elemfeature);

                XElement x = new XElement("{jabber:x:data}x", new XAttribute("type", "form"));
                elemfeature.Add(x);

                XElement field = new XElement("{jabber:x:data}field", new XAttribute("var", "stream-method"), new XAttribute("type", "list-single"));
                x.Add(field);

                if ((StreamOptions & StreamOptions.bytestreams) == StreamOptions.bytestreams)
                    field.Add(new XElement("{jabber:x:data}option", new XElement("{jabber:x:data}value", "http://jabber.org/protocol/bytestreams")));
                if ((StreamOptions & StreamOptions.ibb) == StreamOptions.ibb)
                    field.Add(new XElement("{jabber:x:data}option", new XElement("{jabber:x:data}value", "http://jabber.org/protocol/ibb")));

            }
            else
            {
                XElement elemfeature = new XElement("{http://jabber.org/protocol/feature-neg}feature");
                elemSI.Add(elemfeature);

                XElement x = new XElement("{jabber:x:data}x", new XAttribute("type", "submit"));
                elemfeature.Add(x);

                XElement field = new XElement("{jabber:x:data}field", new XAttribute("var", "stream-method"));
                x.Add(field);

                // Only add one option.  If user or'd more than one, go with the first
                if ((StreamOptions & StreamOptions.bytestreams) == StreamOptions.bytestreams)
                    field.Add(new XElement("{jabber:x:data}value", "http://jabber.org/protocol/bytestreams"));
                else if ((StreamOptions & StreamOptions.ibb) == StreamOptions.ibb)
                    field.Add(new XElement("{jabber:x:data}value", "http://jabber.org/protocol/ibb"));

            }

            base.AddInnerXML(elemMessage);
        }

        public override void ParseInnerXML(System.Xml.Linq.XElement elem)
        {
            XElement si = elem.FirstNode as XElement;
            if (si == null)
                return;

            if (si.Name != "{http://jabber.org/protocol/si}si")
                return;

            if (si.Attribute("id") != null)
                sid = si.Attribute("id").Value;
            if (si.Attribute("mime-type") != null)
                mimetype = si.Attribute("mime-type").Value;
            if (si.Attribute("profile") != null)
                profile = si.Attribute("profile").Value;

            StreamOptions = StreamOptions.none;
            foreach (XElement nextelem in si.Descendants())
            {
                if (nextelem.Name == "{http://jabber.org/protocol/si/profile/file-transfer}file")
                {
                    if (nextelem.Attribute("name") != null)
                        filename = nextelem.Attribute("name").Value;
                    if (nextelem.Attribute("size") != null)
                        filesize = Convert.ToInt32(nextelem.Attribute("size").Value);
                    if (nextelem.Attribute("hash") != null)
                        filehash = nextelem.Attribute("hash").Value;
                    if (nextelem.Attribute("date") != null)
                        filedate = nextelem.Attribute("date").Value;

                    if (nextelem.Element("{http://jabber.org/protocol/si/profile/file-transfer}desc") != null)
                        filedesc = nextelem.Element("{http://jabber.org/protocol/si/profile/file-transfer}desc").Value;
                }
                else if (nextelem.Name == "{http://jabber.org/protocol/feature-neg}feature")
                {
                    XElement x = nextelem.Element("{jabber:x:data}x");
                    if (x != null)
                    {
                        if (x.Attribute("type") != null)
                        {
                            if (x.Attribute("type").Value == "form")
                                StreamInitIQType = StreamInitIQType.Offer;
                            else if (x.Attribute("type").Value == "submit")
                                StreamInitIQType = StreamInitIQType.Result;
                        }


                        XElement field = x.Element("{jabber:x:data}field");
                        if (field != null)
                        {
                            /// This may work for both form and submits, because the values are there in both cases, just wrapped in an option in form
                            foreach (XElement nextopt in field.Descendants("{jabber:x:data}value"))
                            {
                                if (nextopt.Value == "http://jabber.org/protocol/bytestreams")
                                    StreamOptions |= StreamOptions.bytestreams;
                                else if (nextopt.Value == "http://jabber.org/protocol/ibb")
                                    StreamOptions |= StreamOptions.ibb;
                            }
                        }
                    }
                        

                }
            }
            base.ParseInnerXML(elem);
        }
    }

    public class StreamInitiationAndTransferLogic : Logic
    {
        public StreamInitiationAndTransferLogic(XMPPClient client)
            : base(client)
        {
        }

        public override bool NewIQ(IQ iq)
        {
            if (iq is StreamInitIQ)
            {
                StreamInitIQ siiq = iq as StreamInitIQ;
                if (iq.Type == IQType.result.ToString())
                {
                    /// May be a response to our pending request to send
                    /// 
                    StreamInitIQ initalrequest = null;
                    if (FileSendRequests.ContainsKey(iq.ID) == true)
                    {
                        initalrequest = FileSendRequests[iq.ID];
                        FileSendRequests.Remove(iq.ID);
                    }

                    if (initalrequest != null)
                    {
                        if (siiq.StreamOptions != StreamOptions.ibb)
                        {
                            /// Tell the host we failed to send the file because we only support ibb
                            return true;
                        }
                        
                        /// Looks like they agree, start an ibb file transfer logic to perform the transfer
                        /// 

                        InbandByteStreamLogic logic = null;
                        if (initalrequest.TemporaryFileBytes != null)
                            logic = new InbandByteStreamLogic(initalrequest.FullFileName, initalrequest.TemporaryFileBytes, initalrequest.sid, iq.From, XMPPClient);
                        else
                            logic = new InbandByteStreamLogic(initalrequest.FullFileName, initalrequest.sid, iq.From, XMPPClient);
                        XMPPClient.AddLogic(logic);
                        logic.Start();

                    }

                }
                //else if (siiq.StreamInitIQType == StreamInitIQType.Offer)
                else if (iq.Type == IQType.set.ToString())
                {
                    /// They want to send a file to us?
                    /// Ask the user if it's OK, and if it is, start an ibb to receive it and send ok
                    /// 

                    if ((siiq.sid == null) || (siiq.sid.Length <= 0) )
                    {
                        IQ iqresponse = new StreamInitIQ(StreamInitIQType.Result);
                        iqresponse.ID = siiq.ID;
                        iqresponse.Type = IQType.error.ToString();
                        iqresponse.To = iq.From;
                        iqresponse.From = XMPPClient.JID;
                        iqresponse.error = "Invalid stream id";
                        XMPPClient.SendXMPP(iqresponse);
                        return true;
                    }
                    if ((siiq.filename == null) || (siiq.filename.Length <= 0))
                    {
                        IQ iqresponse = new StreamInitIQ(StreamInitIQType.Result);
                        iqresponse.ID = siiq.ID;
                        iqresponse.Type = IQType.error.ToString();
                        iqresponse.To = iq.From;
                        iqresponse.From = XMPPClient.JID;
                        iqresponse.error = "Invalid filename";
                        XMPPClient.SendXMPP(iqresponse);
                        return true;
                    }
                    if ( (siiq.StreamOptions&StreamOptions.ibb) != StreamOptions.ibb)
                    {
                        IQ iqresponse = new StreamInitIQ(StreamInitIQType.Result);
                        iqresponse.ID = siiq.ID;
                        iqresponse.Type = IQType.error.ToString();
                        iqresponse.To = iq.From;
                        iqresponse.From = XMPPClient.JID;
                        iqresponse.error = "No valid transfer method";
                        XMPPClient.SendXMPP(iqresponse);
                        return true;
                    }

                    FileDownloads.Add(siiq.sid, siiq);

                    XMPPClient.AskUserIfTheyWantToReceiveFile(siiq.sid, siiq.filename, siiq.filesize, siiq.From);
                }

                return true;
            }
            return false;
        }


        Dictionary<string, StreamInitIQ> FileSendRequests = new Dictionary<string, StreamInitIQ>();

        Dictionary<string, StreamInitIQ> FileDownloads = new Dictionary<string, StreamInitIQ>();

        /// <summary>
        /// Starts a file transfer from this client to another.
        /// Right now, we only support ibb, so make sure to set that
        /// Returns a unique id that identifies this operation so the client can
        /// be notified if the operation finishes or times out
        /// </summary>
        /// <param name="strFileName"></param>
        internal string StartFileTransfer(string strFullFileName, string strMimeType, JID jidto)
        {
            string strFileName = Path.GetFileName(strFullFileName);
            FileInfo info = new FileInfo(strFullFileName);

            StreamInitIQ iq = StreamInitIQ.BuildDefaultStreamInitOffer(strFileName, (int) info.Length, strMimeType);
            iq.StreamOptions = StreamOptions.ibb; //|StreamOptions.bytestreams;
            iq.From = XMPPClient.JID;
            iq.To = jidto;
            iq.Type = IQType.set.ToString();
            iq.FullFileName = strFullFileName;

            string ext = "*.jpg";
            //\Applications\Data\66F652B2-CD0B-48F6-869F-D3B765EFC530\Data\PlatformData\PhotoChooser-907d2cc8-10a0-4745-9d08-ce328088e76e.jpg
#if WINDOWS_PHONE
            //\Applications\Data\66F652B2-CD0B-48F6-869F-D3B765EFC530\Data\PlatformData\PhotoChooser-907d2cc8-10a0-4745-9d08-ce328088e76e.jpg
            int nIndex = strFileName.LastIndexOf("\\");
            if (nIndex > 0)
                strFileName = strFileName.Substring(nIndex + 1);

            ext = Path.GetExtension(strFileName);
#else
            ext = Path.GetExtension(strFileName);
#endif
            if (ext == ".png")
                iq.mimetype = "image/png";
            else if (ext == ".jpg")
                iq.mimetype = "image/jpeg";
            else if (ext == ".bmp")
                iq.mimetype = "image/bmp";
            else
                iq.mimetype = null;
            FileSendRequests.Add(iq.ID, iq);
            XMPPClient.SendXMPP(iq);
            return iq.ID;
        }

      
        internal string StartFileTransfer(string strFileName, byte [] bData, JID jidto)
        {
#if WINDOWS_PHONE
            //\Applications\Data\66F652B2-CD0B-48F6-869F-D3B765EFC530\Data\PlatformData\PhotoChooser-907d2cc8-10a0-4745-9d08-ce328088e76e.jpg
            int nIndex = strFileName.LastIndexOf("\\");
            if (nIndex > 0)
                strFileName = strFileName.Substring(nIndex + 1);
#endif
            string ext = Path.GetExtension(strFileName);
            string strMimeType = "image/png";
            if (ext == ".png")
                strMimeType = "image/png";
            else if (ext == ".jpg")
                strMimeType = "image/jpeg";
            else if (ext == ".bmp")
                strMimeType = "image/bmp";
            else
                strMimeType = null;

            StreamInitIQ iq = StreamInitIQ.BuildDefaultStreamInitOffer(strFileName, (int)bData.Length, strMimeType);
            iq.StreamOptions = StreamOptions.ibb; //|StreamOptions.bytestreams;
            iq.From = XMPPClient.JID;
            iq.To = jidto;
            iq.Type = IQType.set.ToString();
            iq.FullFileName = strFileName;
            iq.mimetype = strMimeType;
            iq.TemporaryFileBytes = bData;
            FileSendRequests.Add(iq.ID, iq);
            XMPPClient.SendXMPP(iq);
            return iq.ID;
        }

        internal void AcceptIncomingFileRequest(string strRequestId, string strLocalFileName)
        {
            if (FileDownloads.ContainsKey(strRequestId) == true)
            {
                StreamInitIQ siiq = FileDownloads[strRequestId];
                FileDownloads.Remove(strRequestId);

                // Start our IBB logic
                InbandByteStreamLogic logic = new InbandByteStreamLogic(siiq.sid, siiq.From, siiq.filesize, XMPPClient, strLocalFileName);
                XMPPClient.AddLogic(logic);
                logic.Start();

                

                StreamInitIQ iqaccept = StreamInitIQ.BuildDefaultStreamInitResult(StreamOptions.ibb);
                iqaccept.ID = siiq.ID;
                iqaccept.From = XMPPClient.JID;
                iqaccept.To = siiq.From;
                iqaccept.Type = IQType.result.ToString();
                XMPPClient.SendXMPP(iqaccept);

            }

        }

        internal void DeclineIncomingFileRequest(string strRequestId)
        {
            if (FileDownloads.ContainsKey(strRequestId) == true)
            {
                StreamInitIQ siiq = FileDownloads[strRequestId];
                FileDownloads.Remove(strRequestId);

                StreamInitIQ iqdecline = StreamInitIQ.BuildDefaultStreamInitResult(StreamOptions.ibb);
                iqdecline.ID = siiq.ID; 
                iqdecline.Type = IQType.error.ToString();
                iqdecline.To = siiq.From;
                iqdecline.From = XMPPClient.JID;
                iqdecline.error = "Declined";
                XMPPClient.SendXMPP(iqdecline);
            }
        }
    }

    public enum IBBMode
    {
        Send,
        Receive,
    }

    /// <summary>
    /// Sends or receives a file using XEP-0047, then notifies the XMPPClient it is finished and to 
    /// be removed from the logic stack
    /// </summary>
    public class InbandByteStreamLogic : Logic
    {
        public InbandByteStreamLogic(string strFileName, string strSID, JID jidto, XMPPClient client)
            : base(client)
        {
            IBBMode = IBBMode.Send;
            filename = strFileName;
            sid = strSID;
            To = jidto;
        }

         public InbandByteStreamLogic(string strFileName, byte [] bData, string strSID, JID jidto, XMPPClient client)
            : base(client)
        {
            IBBMode = IBBMode.Send;
            FileBytes = bData;
            filename = strFileName;
            sid = strSID;
            To = jidto;
        }

        public InbandByteStreamLogic(string strSID, JID jidto, int nFileSize, XMPPClient client, string strLocalFileNameSaveTo)
            : base(client)
        {
            IBBMode = IBBMode.Receive;
            filename = strLocalFileNameSaveTo;
            sid = strSID;
            To = jidto;
            nTotal = nFileSize;
            nRemaining = nFileSize;
        }

        

        public IBBMode IBBMode = IBBMode.Send;
        public string filename = null;
        public byte[] FileBytes = null;
        public JID To = "";
        public string sid = null;

        int nTotal = 0;
        int nRemaining = 0;
        int nActions = 0;
        IQ InitialIQ = null;
        IQ LastFileDataIQSent = null;
        ByteBuffer FileBuffer = new ByteBuffer();
        const int nBlockSize = 4096*4;
        int nSequence = 0;

        System.IO.FileStream outputstream = null;

        public override void Start()
        {
            if (IBBMode == PhoneXMPPLibrary.IBBMode.Send)
            {
                InitialIQ = new IQ();
                InitialIQ.From = XMPPClient.JID;
                InitialIQ.To = To;
                InitialIQ.Type = IQType.set.ToString();

                InitialIQ.InnerXML = string.Format("<open xmlns='http://jabber.org/protocol/ibb' block-size='{1}' sid='{0}' stanza='iq' />", sid, nBlockSize);
                XMPPClient.SendXMPP(InitialIQ);
            }
            else
            {
            }

            base.Start();
        }

        void SendNextFileIQ()
        {
            if (LastFileDataIQSent == null)
            {
                if (FileBytes != null)
                {
                    nTotal = FileBytes.Length;
                    nRemaining = FileBytes.Length;
                }
                else
                {
                    System.IO.FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    FileBytes = new byte[stream.Length];
                    nTotal = FileBytes.Length;
                    nRemaining = FileBytes.Length;
                    stream.Read(FileBytes, 0, FileBytes.Length);
                    stream.Close();
                }
                FileBuffer.AppendData(FileBytes);
                nSequence = 0;
            }

            if (FileBuffer.Size <= 0)
            {
                XMPPClient.ReportSendFinished(sid, filename, To);
                this.IsCompleted = true;
                return;
            }

            int nChunkSize = (FileBuffer.Size > nBlockSize) ? nBlockSize : FileBuffer.Size;
            nRemaining -= nChunkSize;
            byte[] bNext = FileBuffer.GetNSamples(nChunkSize);

            LastFileDataIQSent = new IQ();
            LastFileDataIQSent.From = XMPPClient.JID;
            LastFileDataIQSent.To = To;
            LastFileDataIQSent.Type = IQType.set.ToString();

            string strBase64 = Convert.ToBase64String(bNext);
            LastFileDataIQSent.InnerXML = string.Format("<data xmlns='http://jabber.org/protocol/ibb' seq='{0}' sid='{1}' >{2}</data>",
                                                        nSequence++, sid, strBase64);
            XMPPClient.SendXMPP(LastFileDataIQSent);
            nActions++;

            if ((nActions%10) == 0)
               XMPPClient.ReportFileProgress(sid, (nTotal - nRemaining), nTotal, To);
        }


        public override bool NewIQ(IQ iq)
        {
            if (IBBMode == IBBMode.Send)
            {
                if (iq.ID == InitialIQ.ID)
                {
                    if (iq.Type == IQType.error.ToString())
                    {
                        IsCompleted = true; /// Remove this guy
                        XMPPClient.ReportSendFinished(sid, filename, To);
                        return true;
                    }
                    else if (iq.Type == IQType.result.ToString())
                    {
                        // Send the next chunk
                        SendNextFileIQ();
                    }

                    return true;
                }
                else if ((LastFileDataIQSent != null) && (iq.ID == LastFileDataIQSent.ID))
                {
                    if (iq.Type == IQType.error.ToString())
                    {
                        /// TODO.. notify the user there was a failure transferring blocks
                        /// 
                        XMPPClient.ReportSendFinished(sid, filename, To);
                        IsCompleted = true;
                        return true;
                    }
                    else if (iq.Type == IQType.result.ToString())
                    {
                        // Send the next chunk
                        SendNextFileIQ();
                    }
                }
            }
            else
            {
                if (iq.InitalXMLElement != null)
                {
                    XElement elem = iq.InitalXMLElement.FirstNode as XElement;
                    if ((elem != null) && (elem.Name == "{http://jabber.org/protocol/ibb}open"))
                    {
                        string strStreamId = null;
                        if (elem.Attribute("sid") != null)
                            strStreamId = elem.Attribute("sid").Value;
                        if ((strStreamId == null) || (strStreamId != this.sid))
                            return false;

                        /// If we're receiving, don't do anything but look for IQ's with the right id and respond
                        outputstream = new FileStream(filename, FileMode.Create, FileAccess.Write);

                        /// SEnd ack to open
                        /// 
                        IQ iqresponse = new IQ();
                        iqresponse.ID = iq.ID;
                        iqresponse.From = XMPPClient.JID;
                        iqresponse.To = To;
                        iqresponse.Type = IQType.result.ToString();
                        XMPPClient.SendXMPP(iqresponse);

                        return true;
                    }
                    if ((elem != null) && (elem.Name == "{http://jabber.org/protocol/ibb}data"))
                    {
                        string strStreamId = null;
                        if (elem.Attribute("sid") != null)
                            strStreamId = elem.Attribute("sid").Value;
                        if ((strStreamId == null) || (strStreamId != this.sid))
                            return false;


                        byte[] bData = Convert.FromBase64String(elem.Value);

                        nRemaining -= bData.Length;
                        if (outputstream != null)
                           outputstream.Write(bData, 0, bData.Length);

                        /// SEnd ack
                        /// 
                        IQ iqresponse = new IQ();
                        iqresponse.ID = iq.ID;
                        iqresponse.From = XMPPClient.JID;
                        iqresponse.To = To;
                        iqresponse.Type = IQType.result.ToString();
                        XMPPClient.SendXMPP(iqresponse);


                        nActions++;

                        if ((nActions % 10) == 0)
                            XMPPClient.ReportFileProgress(sid, (nTotal - nRemaining), nTotal, To);

                        if (nRemaining <= 0)
                        {
                            outputstream.Close();
                            IsCompleted = true;
                            XMPPClient.ReportDownloadFinished(sid, filename, To);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public override bool NewMessage(Message iq)
        {
            return base.NewMessage(iq);
        }

    }
}
