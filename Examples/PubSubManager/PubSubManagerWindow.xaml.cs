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
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace PubSubManager
{
    /// <summary>
    /// Interaction logic for PubSubListManagerWindow.xaml
    /// </summary>
    public partial class PubSubManagerWindow : Window
    {
        public XMPPClient XMPPClient = new XMPPClient();
        public PubSubManagerClass PubSubManagerClass = new PubSubManagerClass();
        //PubSubNodeManager<GroceryItem> GroceryNode = null;


        public PubSubManagerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = XMPPClient;
            
            // this.ButtonConnect.DataContext = XMPPClient;   
        }

        void StartPubSubManager()
        {
            TextBoxServerName.Text = XMPPClient.Server;

            PubSubManagerClass.RegisterXMPPClient(XMPPClient);
         //   this.DataContext = PubSubManagerClass;

            ButtonQueryServer_Click(null, null);
            // PubSubManagerClass.XMPPServerName = "ninethumbs.com";

          //  if (PubSubManagerClass.PopulatePubSubManager())
            {
                //TreeViewPubSubNodes.DataContext = PubSubManager.PubSubDictionary;
            }
           // PubSubManagerClass.PropertyChanged += new PropertyChangedEventHandler(PubSubManager_PropertyChanged);
           
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
            PubSubManagerClass.PopulatePubSubManager();
        }

        private void ButtonAddNode_Click(object sender, RoutedEventArgs e)
        {
            if (PubSubManagerClass.CreateTopLevelNode(TextBoxNewNodeName.Text, TextBoxNewNodeName.Text))
            {

            }
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            PubSubManagerClass.PopulatePubSubManager();
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
            QueryServer();
        }

        private void QueryServer()
        {
            if (PubSubManagerClass.XMPPServerName == null  || 
                PubSubManagerClass.XMPPServerName.Trim().Length < 1)
                PubSubManagerClass.XMPPServerName = TextBoxServerName.Text;
            if (PubSubManagerClass.PopulatePubSubManager() == false)
            {
                MessageBox.Show("Error retrieving pub sub nodes for " + PubSubManagerClass.XMPPServerName);
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

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (XMPPClient.XMPPState == XMPPState.Unknown)
            {
                XMPPClient.AutoReconnect = true;

                LoginWindow loginwin = new LoginWindow();
                LoadAccount();

                loginwin.ActiveAccount = XMPPClient.XMPPAccount;
                loginwin.AllAccounts = new List<XMPPAccount>();
                loginwin.AllAccounts.Add(XMPPClient.XMPPAccount);

                if (loginwin.ShowDialog() == false)
                    return;
                if (loginwin.ActiveAccount == null)
                {
                    MessageBox.Show("Login window returned null account", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                SaveAccount();

                PubSubManagerClass.XMPPServerName = XMPPClient.XMPPAccount.Server;

                //XMPPClient.XMPPAccount.Capabilities = new Capabilities();
                //XMPPClient.XMPPAccount.Capabilities.Node = "http://xmedianet.codeplex.com/wpfclient/caps";
                //XMPPClient.XMPPAccount.Capabilities.Version = "1.0";
                //XMPPClient.XMPPAccount.Capabilities.Extensions = "voice-v1"; /// google talk capabilities

                XMPPClient.FileTransferManager.AutoDownload = true;
                XMPPClient.AutoAcceptPresenceSubscribe = true;
                XMPPClient.OnStateChanged += new EventHandler(XMPPClient_OnStateChanged);
                XMPPClient.Connect();

            }
            else if (XMPPClient.XMPPState > XMPPState.Connected)
            {
                XMPPClient.Disconnect();
            }
        }

        //void XMPPClient_OnStateChanged(object sender, EventArgs e)
        //{
        //    if (XMPPClient.XMPPState == XMPPState.Authenticated)
        //    {
        //        StartPubSubManager();
        //    }
        //    // throw new NotImplementedException();
        //}


        public delegate void DelegateVoid();
        public void XMPPClient_OnStateChanged(object obj, EventArgs arg)
        {
            HandleStateChanged();
            // seems to get stuck in the AddLogic lock when I use this Dispatcher.Invoke
            // Dispatcher.Invoke(new DelegateVoid(HandleStateChanged));
        }

        void HandleStateChanged()
        {
            if (XMPPClient.XMPPState == XMPPState.Connected)
            {

            }
            else if (XMPPClient.XMPPState == XMPPState.Unknown)
            {
                //this.FirePropertyChanged("ConnectedStateBrush");
                //ComboBoxPresence.IsEnabled = false;
                //ButtonAddBuddy.IsEnabled = false;
                //SaveAccounts();

                //if (AudioMuxerWindow.IsLoaded == true)
                //    AudioMuxerWindow.CloseAllSessions();
            }
            else if (XMPPClient.XMPPState == XMPPState.Ready)
            {
                StartPubSubManager();
                //this.FirePropertyChanged("ConnectedStateBrush");
                //ComboBoxPresence.IsEnabled = true;
                //ButtonAddBuddy.IsEnabled = true;
                //this.ImageAvatar.Source = XMPPClient.Avatar;
                //XMPPClient.SetGeoLocation(0, 0);
               

                //XMPPClient.SetGeoLocation(32.234, -97.3453);
            }
            else if (XMPPClient.XMPPState == XMPPState.AuthenticationFailed)
            {
                //if (MessageBox.Show(string.Format("Incorrect username or password.  Would you like to create user '{0}'?", XMPPClient.UserName), "Authentication Failed", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                //{
                //    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(CreateAccountAsync));
                //}
                //else
                {
                    XMPPClient.Disconnect();
                }
            }
            else if (XMPPClient.XMPPState == XMPPState.Authenticated)
            {
                // StartPubSubManager();
            }
            else
            {
            }
        }


        void SaveAccount()
        {

            string strPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string strFileName = string.Format("{0}\\{1}", strPath, "pubsubaccount.xml");
            FileStream location = null;

            try
            {
                location = new FileStream(strFileName, System.IO.FileMode.Create);
                DataContractSerializer ser = new DataContractSerializer(typeof(XMPPAccount));
                ser.WriteObject(location, this.XMPPClient.XMPPAccount);
            }
            catch (Exception)
            {
            }
            finally
            {
                if (location != null)
                    location.Close();
            }

        }

        void LoadAccount()
        {
            string strPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string strFileName = string.Format("{0}\\{1}", strPath, "pubsubaccount.xml");
            FileStream location = null;

            if (File.Exists(strFileName) == true)
            {
                try
                {
                    location = new FileStream(strFileName, System.IO.FileMode.Open);
                    DataContractSerializer ser = new DataContractSerializer(typeof(XMPPAccount));

                    XMPPClient.XMPPAccount = ser.ReadObject(location) as XMPPAccount;
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (location != null)
                        location.Close();
                }
            }


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

    public class PubSubManagerClass : INotifyPropertyChanged
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
            if (nodeNames == null)
            {
                MessageBox.Show("Error retrieving root nodes");
                return false;
            }
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
