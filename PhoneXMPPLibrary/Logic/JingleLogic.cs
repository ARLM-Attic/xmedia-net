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

using System.Collections.Generic;
using System.Xml.Linq;

namespace PhoneXMPPLibrary
{
    // An audio/video payload 
    public class Payload
    {
        public Payload()
        {
        }
        private int m_nPayloadId = 0;

        public int PayloadId
        {
            get { return m_nPayloadId; }
            set { m_nPayloadId = value; }
        }
        private string m_strName = "speex";

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }
        private int m_nChannels = 1;

        public int Channels
        {
            get { return m_nChannels; }
            set { m_nChannels = value; }
        }
        private uint m_nClockRate = 16000;

        public uint ClockRate
        {
            get { return m_nClockRate; }
            set { m_nClockRate = value; }
        }
        public uint m_nPtime = 0;
        public uint m_nMaxptime = 0;
    }

    public class Candidate
    {
        public Candidate()
        {
        }
        public int component = 1;
        public int foundation = 1;
        public int generation = 0;
        public string id = "";
        public string ipaddress = "";
        public int network = 1;
        public int port = 8080;
        public int priority = 1231245;
        public string protocol = "udp";
        public string type = "host";
        public string reladdr = "";
        public int relport = 0;

    }

     /// <feature var='urn:xmpp:jingle:1'/>    
     /// <feature var='urn:xmpp:jingle:transports:ice-udp:0'/>    
     /// <feature var='urn:xmpp:jingle:transports:ice-udp:1'/>    
     /// <feature var='urn:xmpp:jingle:apps:rtp:1'/>    
     /// <feature var='urn:xmpp:jingle:apps:rtp:audio'/>    
     /// <feature var='urn:xmpp:jingle:apps:rtp:video'/>

    public class JingleMessage : IQ
    {
        public JingleMessage()
            : base()
        {
        }
        public JingleMessage(string strXML)
            : base(strXML)
        {
        }

        public string action = "session-initiate";
        public JID initiator = "";
        public string sid;

        public string media = "audio";

        public List<Payload> payloads = new List<Payload>();
        public List<Candidate> candidates = new List<Candidate>();


        public override void ParseInnerXML(System.Xml.Linq.XElement elem)
        {
            if (elem.Name != "{urn:xmpp:jingle:1}jingle")
                return;

            if (elem.Attribute("action") != null)
                action = elem.Attribute("action").Value;
            if (elem.Attribute("initiator") != null)
                initiator = elem.Attribute("initiator").Value;
            if (elem.Attribute("sid") != null)
                sid = elem.Attribute("sid").Value;

            foreach (XElement nextelem in elem.Descendants())
            {
                if (nextelem.Name == "{urn:xmpp:jingle:1}content")
                {
                }
                else if (nextelem.Name == "{urn:xmpp:jingle:apps:rtp:1}description")
                {
                    if (nextelem.Attribute("media") != null)
                        media = nextelem.Attribute("media").Value;

                    payloads.Clear();
                    foreach (XElement nextpayload in nextelem.Descendants("{urn:xmpp:jingle:apps:rtp:1}payload-type"))
                    {
                        Payload pay = new Payload();
                        if (nextpayload.Attribute("id") != null)
                            pay.PayloadId = Convert.ToInt32(nextpayload.Attribute("id").Value);
                        if (nextpayload.Attribute("name") != null)
                            pay.Name = nextpayload.Attribute("name").Value;
                        if (nextpayload.Attribute("clockrate") != null)
                            pay.ClockRate = Convert.ToUInt32(nextpayload.Attribute("clockrate").Value);
                        if (nextpayload.Attribute("channels") != null)
                            pay.Channels = Convert.ToInt32(nextpayload.Attribute("channels").Value);

                        payloads.Add(pay);
                    }

                    /// Parse payloads
                }
                else if (nextelem.Name == "{urn:xmpp:jingle:transports:ice-udp:1}transport")
                {
                    /// Parse candidates
                    /// 
                    candidates.Clear();
                    foreach (XElement nextcand in nextelem.Descendants("{urn:xmpp:jingle:transports:ice-udp:1}candidate"))
                    {
                        Candidate cand = new Candidate();
                        if (nextcand.Attribute("component") != null)
                            cand.component = Convert.ToInt32(nextcand.Attribute("component").Value);
                        if (nextcand.Attribute("foundation") != null)
                            cand.foundation = Convert.ToInt32(nextcand.Attribute("foundation").Value);
                        if (nextcand.Attribute("generation") != null)
                            cand.generation = Convert.ToInt32(nextcand.Attribute("generation").Value);
                        if (nextcand.Attribute("id") != null)
                            cand.id =nextcand.Attribute("id").Value;
                        if (nextcand.Attribute("ip") != null)
                            cand.ipaddress = nextcand.Attribute("ip").Value;
                        if (nextcand.Attribute("network") != null)
                            cand.network = Convert.ToInt32(nextcand.Attribute("network").Value);
                        if (nextcand.Attribute("port") != null)
                            cand.port = Convert.ToInt32(nextcand.Attribute("port").Value);
                        if (nextcand.Attribute("priority") != null)
                            cand.priority = Convert.ToInt32(nextcand.Attribute("priority").Value);
                        if (nextcand.Attribute("protocol") != null)
                            cand.protocol = nextcand.Attribute("protocol").Value;
                        if (nextcand.Attribute("type") != null)
                            cand.type = nextcand.Attribute("type").Value;
                        if (nextcand.Attribute("rel-addr") != null)
                            cand.reladdr = nextcand.Attribute("rel-addr").Value;
                        if (nextcand.Attribute("rel-port") != null)
                            cand.relport = Convert.ToInt32(nextcand.Attribute("rel-port").Value);

                        candidates.Add(cand);
                    }
                }
            }
        }

        public override void AddInnerXML(System.Xml.Linq.XElement elemMessage)
        {
            XElement elemJingle = new XElement("{urn:xmpp:jingle:1}jingle");
            elemMessage.Add(elemJingle);
            elemJingle.Add(new XAttribute("action", action), new XAttribute("initiator", initiator), new XAttribute("sid", sid));

            XElement elemContent = new XElement("{urn:xmpp:jingle:1}content");
            elemJingle.Add(elemContent);
            
            XElement elemDesc = new XElement("{urn:xmpp:jingle:apps:rtp:1}description");
            elemDesc.Add(new XAttribute("media", media));
            elemJingle.Add(elemDesc);
            foreach (Payload pay in payloads)
            {
                elemDesc.Add(new XElement("{urn:xmpp:jingle:apps:rtp:1}payload-type", new XAttribute("id", pay.PayloadId),
                                                                                      new XAttribute("name", pay.Name),
                                                                                      new XAttribute("clockrate", pay.ClockRate),
                                                                                      new XAttribute("channels", pay.Channels)));
            }

            XElement elemTransport = new XElement("{urn:xmpp:jingle:transports:ice-udp:1}transport");
            //elemTransport.Add(new XAttribute("media", media));
            elemJingle.Add(elemTransport);
            foreach (Candidate cand in candidates)
            {
                elemTransport.Add(new XElement("{urn:xmpp:jingle:transports:ice-udp:1}candidate", new XAttribute("component", cand.component),
                                                                                                  new XAttribute("foundation", cand.foundation),
                                                                                                  new XAttribute("generation", cand.generation),
                                                                                                  new XAttribute("id", cand.id),
                                                                                                  new XAttribute("ip", cand.ipaddress),
                                                                                                  new XAttribute("network", cand.network),
                                                                                                  new XAttribute("port", cand.port),
                                                                                                  new XAttribute("priority", cand.priority),
                                                                                                  new XAttribute("protocol", cand.protocol),
                                                                                                  new XAttribute("type", cand.type),
                                                                                                  new XAttribute("rel-addr", cand.reladdr),
                                                                                                  new XAttribute("rel-port", cand.relport)));
            }
        }

    }

    /// <summary>
    /// Initiates a jingle session, returns and event when done 
    /// </summary>
    public class JingleLogic : Logic
    {
        public JingleLogic(XMPPClient client)
            : base(client)
        {
        }

        JingleMessage RequestMessage = null;
        public void InitiateJingleSession(JID jidTo, string strLocalIP, int nLocalPort)
        {
            RequestMessage = new JingleMessage();
            RequestMessage.action = "session-initiate";
            RequestMessage.initiator = XMPPClient.JID;
            //RequestMessage.sid = ?
            RequestMessage.media = "audio";

            RequestMessage.payloads.Add(new Payload() { PayloadId = 96, Channels = 1, ClockRate = 16000, Name = "speex" });
            RequestMessage.payloads.Add(new Payload() { PayloadId = 97, Channels = 1, ClockRate = 8000, Name = "speex" });
            RequestMessage.payloads.Add(new Payload() { PayloadId = 0, Channels = 1, ClockRate = 8000, Name = "PCMU" });

            RequestMessage.candidates.Add(new Candidate() { ipaddress = strLocalIP, port = nLocalPort });

            XMPPClient.SendXMPP(RequestMessage);
        }

        public override bool NewIQ(IQ iq)
        {
            if ( (RequestMessage != null) &&  (iq.ID == RequestMessage.ID))
            {

                return true;
            }
            else if (iq is JingleMessage) //Our XMPPMessageFactory created a jingle message
            {
                /// See if this is a JINGLE message for our session 
                /// 
                JingleMessage msg = iq as JingleMessage;
                if ((RequestMessage != null) && (msg.sid == RequestMessage.sid))
                {
                    /// The session we initiated
                }
                else
                {
                    // A new incoming sesion request
                }
                return true;
            }
            return false;
        }


        /// Example Negotiation Below, from XEP-0167
        /// <jingle xmlns='urn:xmpp:jingle:1' action='session-initiate' initiator='romeo@montague.lit/orchard' sid='a73sjjvkla37jfea'>    
        ///     <content creator='initiator' name='voice'>      
        ///     <description xmlns='urn:xmpp:jingle:apps:rtp:1' media='audio'>        
        ///         <payload-type id='96' name='speex' clockrate='16000'/>        
        ///         <payload-type id='97' name='speex' clockrate='8000'/>        
        ///         <payload-type id='18' name='G729'/>        
        ///         <payload-type id='0' name='PCMU'/>        
        ///         <payload-type id='103' name='L16' clockrate='16000' channels='2'/>        
        ///         <payload-type id='98' name='x-ISAC' clockrate='8000'/>      
        ///     </description>      
        ///     <transport xmlns='urn:xmpp:jingle:transports:ice-udp:1' pwd='asd88fgpdd777uzjYhagZg' ufrag='8hhy'>
        ///         <candidate 
        ///             component='1'   
        ///             foundation='1' 
        ///             generation='0' 
        ///             id='el0747fg11' 
        ///             ip='10.0.1.1' 
        ///             network='1' 
        ///             port='8998' 
        ///             priority='2130706431' 
        ///             protocol='udp'
        ///             type='host'/>        
        ///             
        ///         <candidate component='1'
        ///             foundation='2'
        ///             generation='0'
        ///             id='y3s2b30v3r'
        ///             ip='192.0.2.3'
        ///             network='1'
        ///             port='45664'
        ///             priority='1694498815'
        ///             protocol='udp'
        ///             rel-addr='10.0.1.1'
        ///             rel-port='8998'
        ///             type='srflx'/>      
        ///         </transport>    
        ///     </content>  
        /// </jingle>

        ///<iq from='juliet@capulet.lit/balcony'    
        ///id='ih28sx61'    
        ///to='romeo@montague.lit/orchard'    
        ///type='result'/>

        ///<iq from='juliet@capulet.lit/balcony'    
        ///     id='i91fs6d5'    
        ///         to='romeo@montague.lit/orchard'    type='set'>  
        ///         <jingle xmlns='urn:xmpp:jingle:1'      
        ///             action='session-accept'          
        ///             initiator='romeo@montague.lit/orchard'          
        ///             responder='juliet@capulet.lit/balcony'          
        ///             sid='a73sjjvkla37jfea'>    
        ///             
        ///             <content creator='initiator' name='voice'>      
        ///                 <description xmlns='urn:xmpp:jingle:apps:rtp:1' media='audio'>        
        ///                     <payload-type id='97' name='speex' clockrate='8000'/>        
        ///                     <payload-type id='18' name='G729'/>      
        ///                 </description>      
        ///             
        ///                 <transport xmlns='urn:xmpp:jingle:transports:ice-udp:1'                 
        ///                 pwd='YH75Fviy6338Vbrhrlp8Yh'                 
        ///                 ufrag='9uB6'>        
        ///               
        ///                     <candidate component='1'                   
        ///                         foundation='1'                   
        ///                         generation='0'                   
        ///                         id='or2ii2syr1'                   
        ///                         ip='192.0.2.1'                   
        ///                         network='0'                   
        ///                         port='3478'                   
        ///                         priority='2130706431'                   
        ///                         protocol='udp'                   
        ///                         type='host'/>      
        ///                 </transport>    
        ///             </content>  
        ///         </jingle>
        /// </iq>


        /// <iq from='romeo@montague.lit/orchard'    
        ///     id='i91fs6d5'    
        ///     to='juliet@capulet.lit/balcony'    
        ///     type='result'/>
    }
}
