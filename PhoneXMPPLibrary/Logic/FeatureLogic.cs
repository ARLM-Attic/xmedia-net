using System;
using System.Net;

using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using System.Text.RegularExpressions;

 
namespace System.Net.XMPP
{
    /// <summary>
    /// Responsible for querying what services the server supports
    /// </summary>
    public class FeatureLogic : Logic
    {
        public FeatureLogic(XMPPClient client)
            : base(client)
        {
        }


    }
}
