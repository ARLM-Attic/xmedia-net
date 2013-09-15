using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Net.XMPP;
using System.IO;

namespace XAlerts
{
    [DataContract]
    public class Subscription : System.ComponentModel.INotifyPropertyChanged
    {
        public Subscription()
        {
        }

        private string m_strNodeName = "";
        [DataMember]
        public string NodeName
        {
            get { return m_strNodeName; }
            set 
            {
                if (m_strNodeName != value)
                {
                    m_strNodeName = value;
                    FirePropertyChanged("NodeName");
                }
            }
        }

        private string m_strSubscriptionId = "";
        [DataMember]
        public string SubscriptionId
        {
            get { return m_strSubscriptionId; }
            set
            {
                if (m_strSubscriptionId != value)
                {
                    m_strSubscriptionId = value;
                    FirePropertyChanged("SubscriptionId");
                }
            }
        }



        private PubSubNodeManager<System.Net.XMPP.AlertMessage> m_objEvents = new PubSubNodeManager<AlertMessage>("node", null);
        public PubSubNodeManager<System.Net.XMPP.AlertMessage> Events
        {
            get { return m_objEvents; }
            set { m_objEvents = value; FirePropertyChanged("Events"); }
        }


        public static List<Subscription> Load(string strFileName)
        {
            List<Subscription> subs = new List<Subscription>();

            DataContractSerializer ser = new DataContractSerializer(typeof(Subscription[]));

            if (File.Exists(strFileName) == false)
                return subs;

            try
            {
                FileStream stream = new FileStream(strFileName, FileMode.Open, FileAccess.Read);
                Subscription[] subarray = ser.ReadObject(stream) as Subscription[];
                stream.Close();

                subs.AddRange(subarray);
            }
            catch (Exception ex)
            {
            }

            return subs;
        }

        public static void Save(string strFileName, List<Subscription> subs)
        {

            DataContractSerializer ser = new DataContractSerializer(typeof(Subscription[]));


            try
            {
                Subscription[] subarray = subs.ToArray();
                FileStream stream = new FileStream(strFileName, FileMode.Create, FileAccess.Write);
                ser.WriteObject(stream, subarray);
                stream.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strProp)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(strProp));
            }
        }
    }
}
