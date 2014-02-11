using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sip
{
    public class SIPUser
    {

        public string User { get; set; }
        public string AuthUser { get; set; }
        public string AuthPassword { get; set; }
        public string Realm { get; set; }
        public string Proxy { get; set; }
        public ushort ProxyPort { get; set; }
    }


}
