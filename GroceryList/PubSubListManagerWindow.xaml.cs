using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.XMPP;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GroceryList
{
    /// <summary>
    /// Interaction logic for PubSubListManagerWindow.xaml
    /// </summary>
    public partial class PubSubListManagerWindow : Window
    {        
        public XMPPClient XMPPClient = new XMPPClient();
        public PubSubManager PubSubManager = new PubSubManager();
        PubSubNodeManager<GroceryItem> GroceryNode = null;


        public PubSubListManagerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PubSubManager.RegisterXMPPClient(XMPPClient);

            ButtonQueryServer_Click(sender, e);
            PubSubManager.XMPPServerName = "ninethumbs.com";

            if (PubSubManager.PopulatePubSubManager())
            {
                //TreeViewPubSubNodes.DataContext = PubSubManager.PubSubDictionary;
            }
            PubSubManager.PropertyChanged += new PropertyChangedEventHandler(PubSubManager_PropertyChanged);
            this.DataContext = PubSubManager;
        }

        void PubSubManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // in case the bindings don't work.
            switch (e.PropertyName)
            {
                case "RootNodeNames":
                    //ComboBoxRootNodes.ItemsSource = PubSubManager.RootNodeNames;
                    //if (PubSubManager.RootNodeNames.Count() > 0)
                    //    ComboBoxRootNodes.SelectedIndex = 0;
                    break;
                 
                default:
                    break;
            }
        }

     

        private void ComboBoxRootNodes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // refresh, try to get top level.
            PubSubManager.PopulatePubSubManager();
        }

        private void ButtonAddNode_Click(object sender, RoutedEventArgs e)
        {
            if (PubSubManager.CreateTopLevelNode(TextBoxNewNodeName.Text, TextBoxNewNodeName.Text))
            {
                
            }
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            PubSubManager.PopulatePubSubManager();
        }

        private void TextBoxNewNodeName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TextBoxNewNodeName.Text == "Enter node name")
            {
                // yourcontrol.Style= (Style) LayoutRoot.Getresources("MyStyle");
                TextBoxNewNodeName.Style = this.Resources["TextBoxAfterClick"] as Style;
                TextBoxNewNodeName.Text = "";
            }
        }

        private void TextBoxNewNodeName_TouchDown(object sender, TouchEventArgs e)
        {

        }

        private void TextBoxNewNodeName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ButtonQueryServer_Click(object sender, RoutedEventArgs e)
        {
            PubSubManager.XMPPServerName = TextBoxServerName.Text;
            if (PubSubManager.PopulatePubSubManager() == false)
            {
                MessageBox.Show("Error retrieving pub sub nodes for " + PubSubManager.XMPPServerName);
            }
        }

        private void TextBoxNewItemName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ButtonAddItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TreeViewPubSubNodes_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Get selected node
            var newValue = e.NewValue;
            string strNodeId = "";
            //PubSubOperation.GetNodeItems(XMPPClient, newValue.Key, strNodeId);
 
        }

      
    }

    public class PubSubRoot : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }


        #endregion

    }

    public class PubSubManager : INotifyPropertyChanged
    {
        private string m_XMPPServerName = "";

        public string XMPPServerName
        {
            get { return m_XMPPServerName; }
            set
            {
                if (m_XMPPServerName != value)
                {
                    m_XMPPServerName = value;
                    FirePropertyChanged("XMPPServerName");
                }
            }
        }


        private XMPPClient m_XMPPClient = new XMPPClient();

        public XMPPClient XMPPClient
        {
            get { return m_XMPPClient; }
            set
            {
                m_XMPPClient = value;
                XMPPServerName = m_XMPPClient.JID.Domain;
            }
        }

        private ObservableCollection<string> m_RootNodeNames = new ObservableCollection<string>();

        public ObservableCollection<string> RootNodeNames
        {
            get { return m_RootNodeNames; }
            set
            {
                if (m_RootNodeNames != value)
                {
                    m_RootNodeNames = value;
                    FirePropertyChanged("RootNodeNames");
                }
            }
        }

        public void RegisterXMPPClient(XMPPClient client)
        {
            XMPPClient = client;
        }

        public bool PopulatePubSubManager()
        {
            string[] nodeNames = PubSubOperation.GetRootNodes(XMPPClient);
            RootNodeNames = new ObservableCollection<string>(nodeNames);
            foreach (string name in nodeNames)
            {
                if (PubSubDictionary.ContainsKey(name) == false)
                    PubSubDictionary.Add(name, null);

                item[] subNodeItems = GetSubNodeItems(name);
                foreach (item item in subNodeItems)
                {
                    PubSubDictionary[name] = item;
                }
            }

            PubSubItems = new ObservableCollection<item>(PubSubDictionary.Values);

            return true;
        }

        // This method does not work. The "Node" attribute is always null (at least it was for me when I queried Grocery List).
        // Should always use GetSubNodeItems() !
        public string[] GetSubNodeNames(string strNodeName)
        {
            string[] retList = PubSubOperation.GetSubNodes(XMPPClient, strNodeName);
            return retList;
        }

       public item[] GetSubNodeItems(string strNodeName)
        {
            item[] items = PubSubOperation.GetSubNodeItems(XMPPClient, strNodeName);
            return items;
        }

       public item CreateItem(string strItemName, string strNodeName)
       {
           item newItem = new item();

           return newItem;
       }

       public bool CreateTopLevelNode(string strNodeName, string strNodeDescription)
       {
           bool bExists = false;
           CheckCreateNode(strNodeName, strNodeDescription, null, "leaf");
           bExists = PubSubOperation.NodeExists(XMPPClient, strNodeName);
           return bExists;
       }

       public bool CheckCreateNode(string strNodeName, string strNodeDescription, string strParentNodeName, string strNodeType) // leaf or collection
       {
            bool bExists = PubSubOperation.NodeExists(XMPPClient, strNodeName);
            if (bExists == false)
            {
                PubSubConfigForm config = new PubSubConfigForm();
                config.AccessModel = "open";
                config.AllowSubscribe = true;
                config.DeliverNotifications = true;
                config.DeliverPayloads = true;
                config.MaxItems = "500";
                config.MaxPayloadSize = "16384";
                config.NodeName = strNodeName;
                config.NodeType = strNodeType;
                config.NotifyConfig = true;
                config.NotifyRetract = true;
                config.PublishModel = "open";
                config.PersistItems = true;
                config.ItemExpire = "86400";
                config.Title = strNodeDescription;
                PubSubOperation.CreateNode(XMPPClient, strNodeName, strParentNodeName, config);
            }

            bExists = PubSubOperation.NodeExists(XMPPClient, strNodeName);
            if (bExists == false)
            {
                // node creation failed
            }
            else
            {
                // node creation passed
            }

            // requery no matter if creation passed or failed
            if (PopulatePubSubManager())
            {

            }

             // PubSubOperation.CreateNode(XMPPClient, strNodeName, strParentNodeName, new PubSubConfigForm());
             string subid = PubSubOperation.SubscribeNode(XMPPClient, strNodeName, XMPPClient.JID, true);
             // GroceryNode.GetAllItems(subid);
             return bExists;
        }


       private Dictionary<string, item> m_PubSubDictionary = new Dictionary<string, item>();

       public Dictionary<string, item> PubSubDictionary
       {
           get { return m_PubSubDictionary; }
           set
           {
               if (m_PubSubDictionary != value)
               {
                   m_PubSubDictionary = value;
                   PubSubItems = new ObservableCollection<item>(m_PubSubDictionary.Values);
                   FirePropertyChanged("PubSubDictionary");
               }
           }
       }

       private ObservableCollection<item> m_PubSubItems = new ObservableCollection<item>();

       public ObservableCollection<item> PubSubItems
       {
           get { return m_PubSubItems; }
           set
           {
               if (m_PubSubItems != value)
               {
                   m_PubSubItems = value;
                   FirePropertyChanged("PubSubItems");
               }
           }
       }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }


        #endregion
    }
}
