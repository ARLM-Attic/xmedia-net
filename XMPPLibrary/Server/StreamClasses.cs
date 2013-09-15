/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Net;

using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace System.Net.XMPP.Server
{
    [XmlRoot(ElementName = "mechanisms", Namespace="urn:ietf:params:xml:ns:xmpp-sasl")]
    public class mechanisms
    {
        public mechanisms()
        {
        }

        private List<string> m_listMechanisms = new List<string>();
        [XmlElement(ElementName = "mechanism")]
        public List<string> Mechanisms
        {
            get { return m_listMechanisms; }
            set { m_listMechanisms = value; }
        }
    }

    [XmlRoot(ElementName = "starttls", Namespace = "urn:ietf:params:xml:ns:xmpp-tls")]
    public class starttls
    {
        public starttls()
        {
        }

        [XmlElement(ElementName = "required")]
        public string required = null;
    }

    [XmlRoot(ElementName = "starttls", Namespace = "urn:ietf:params:xml:ns:xmpp-tls")]
    public class tlsproceed
    {
        public tlsproceed()
        {
        }

        [XmlElement(ElementName = "required")]
        public string required = null;
    }




    [XmlRoot(ElementName = "compression", Namespace = "http://jabber.org/features/compress")]
    public class compression
    {
        public compression()
        {
        }

        [XmlElement(ElementName = "method")]
        public List<string> Methods = new List<string>();
    }

    [XmlRoot(ElementName = "register", Namespace = "urn:ietf:params:xml:ns:xmpp-tls")]
    public class register
    {
        public register()
        {
        }

    }

    [XmlRoot(ElementName = "bind", Namespace = "urn:ietf:params:xml:ns:xmpp-bind")]
    public class bind
    {
        public bind()
        {
        }

    }

    [XmlRoot(ElementName = "session", Namespace = "urn:ietf:params:xml:ns:xmpp-session")]
    public class session
    {
        public session()
        {
        }

    }

    [XmlRoot(ElementName = "stream:features")]
    //[XmlRoot(ElementName = "stream:features", Namespace="{http://etherx.jabber.org/streams}stream")]
    public class streamfeatures
    {
        public streamfeatures()
        {
        }

        [XmlElement(ElementName = "mechanisms", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
        public mechanisms mechanisms = new mechanisms();

        [XmlElement(ElementName = "starttls", Namespace = "urn:ietf:params:xml:ns:xmpp-tls")]
        public starttls starttls = new starttls();

        [XmlElement(ElementName = "compression", Namespace = "http://jabber.org/features/compress")]
        public compression compression = null;

        [XmlElement(ElementName = "register", Namespace = "http://jabber.org/features/iq-register")]
        public register register = null;

        [XmlElement(ElementName = "bind", Namespace = "urn:ietf:params:xml:ns:xmpp-bind")]
        public bind bind = null;

        [XmlElement(ElementName = "session", Namespace = "urn:ietf:params:xml:ns:xmpp-session")]
        public session session = null;
    }
}
