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
