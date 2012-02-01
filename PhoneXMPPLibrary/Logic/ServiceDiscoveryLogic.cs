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


using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using System.Text.RegularExpressions;


namespace PhoneXMPPLibrary
{


    /// <summary>
    /// Responsible for setting our sessions' presence
    /// </summary>
    public class ServiceDiscoveryLogic : Logic
    {
        public ServiceDiscoveryLogic(XMPPClient client)
            : base(client)
        {
        }



        public override void  Start()
        {
            ServiceDiscoveryIQ iqrequest = new ServiceDiscoveryIQ(ServiceDiscoveryType.info);
            iqrequest.From = XMPPClient.JID;
            iqrequest.To = XMPPClient.Server;
            iqrequest.Type = IQType.get.ToString();
            iqrequest.Node = null;
            XMPPClient.SendXMPP(iqrequest);

            //ServiceDiscoveryIQ iqresponse = XMPPClient.QueryServiceDiscovery("ninethumbs.com", ServiceDiscoveryType.info, null);
            //if (iqresponse != null)
            //{
            //}
        }
     

        // Look for subscribe message to subscribe to presence
        public override bool NewIQ(IQ iq)
        {
    
            //// XEP-0030
            ///<iq type='get' from='romeo@montague.net/orchard' to='plays.shakespeare.lit' id='info1'>  
            ///   <query xmlns='http://jabber.org/protocol/disco#info'/>
            ///</iq>

            if ( (iq is ServiceDiscoveryIQ) && (iq.Type == IQType.get.ToString()) )
            {
                ServiceDiscoveryIQ response = new ServiceDiscoveryIQ();
                response.To = iq.From;
                response.From = XMPPClient.JID;
                response.Type = IQType.result.ToString();
                response.Features = XMPPClient.OurServiceDiscoveryFeatureList.ToArray();
                
                XMPPClient.SendXMPP(response);
                return true;
            }


            return base.NewIQ(iq);
        }


    }
}
