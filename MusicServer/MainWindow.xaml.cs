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
using System.Windows.Navigation;
using System.Windows.Shapes;

using AudioClasses;
using ImageAquisition;
using System.Net.XMPP;

using System.Runtime.Serialization;
using System.IO;
using System.Net.XMPP;
using System.Net.XMPP.Jingle;
using RTP;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.ComponentModel;

namespace MusicServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IAudioSink
    {
        public MainWindow()
        {
            InitializeComponent();
            AudioFileReaderSpy = new AudioSourceSpy(AudioFileReader);
            AudioFileReaderSpy.OnPullSample += new DelegatePullSample(AudioFileReaderSpy_OnPullSample);

        }

        private AudioFileReader m_objAudioFileReader = new AudioFileReader(AudioFormat.SixteenBySixteenThousandMono);

        public AudioFileReader AudioFileReader
        {
            get { return m_objAudioFileReader; }
            set { m_objAudioFileReader = value; }
        }

        /// <summary>
        ///  Use this to provide feedback visually of what is playing
        /// </summary>
        /// <param name="sample"></param>
        void AudioFileReaderSpy_OnPullSample(MediaSample sample)
        {
           // SoundAudioSource.NewData(sample);
        }

        public XMPPClient XMPPClient = new XMPPClient();
        ObservableCollectionEx<MediaSession> ObservSessionList = new ObservableCollectionEx<MediaSession>();
        Dictionary<string, MediaSession> SessionList = new Dictionary<string, MediaSession>();
        IPAddress[] addresses = null;
        AudioConferenceMixer AudioMixer = new AudioConferenceMixer(AudioFormat.SixteenBySixteenThousandMono);
        AudioSourceSpy AudioFileReaderSpy = null;
        //WPFImageWindows.AudioSource SoundAudioSource = null;

        PushPullObject FileMixer = null;

        void StartMixer()
        {
            if (this.IsLoaded == false)
            {
                this.Show();
            }

            FileMixer = AudioMixer.AddInputOutputSource(AudioFileReaderSpy, null);

            //SoundAudioSource = new WPFImageWindows.AudioSource(this.AudioFileReaderSpy);
            //SoundAudioSource.ForeColor = Colors.BlueViolet;


            AudioMixer.Start();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ButtonConnect.DataContext = XMPPClient;
            this.ListViewPlayList.ItemsSource = CurrentPlaylist.Tracks;
            this.ListViewAudioSessions.ItemsSource = ObservSessionList;

            addresses = FindAddresses();
            //AudioFileReader.OnPlayFinished += new DelegateSong(AudioFileReader_OnPlayFinished);
            //AudioFileReader.OnPlayStarted += new DelegateSong(AudioFileReader_OnPlayStarted);

            XMPPClient.JingleSessionManager.OnNewSession += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventWithInfo(JingleSessionManager_OnNewSession);
            XMPPClient.JingleSessionManager.OnNewSessionAckReceived += new JingleSessionManager.DelegateJingleSessionEventBool(JingleSessionManager_OnNewSessionAckReceived);
            XMPPClient.JingleSessionManager.OnSessionAcceptedAckReceived += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventBool(JingleSessionManager_OnSessionAcceptedAckReceived);
            XMPPClient.JingleSessionManager.OnSessionAcceptedReceived += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventWithInfo(JingleSessionManager_OnSessionAcceptedReceived);
            XMPPClient.JingleSessionManager.OnSessionTerminated += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEvent(JingleSessionManager_OnSessionTerminated);
            XMPPClient.JingleSessionManager.OnSessionTransportInfoReceived += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventWithInfo(JingleSessionManager_OnSessionTransportInfoReceived);
            XMPPClient.JingleSessionManager.OnSessionTransportInfoAckReceived += new JingleSessionManager.DelegateJingleSessionEventBool(JingleSessionManager_OnSessionTransportInfoAckReceived);


            this.DataContext = CurrentPlaylist;
            ButtonPlaySong.DataContext = AudioFileReader;
            LabelPlaying.DataContext = CurrentPlaylist;
            StartMixer();

            this.CurrentPlaylist.PropertyChanged += new PropertyChangedEventHandler(CurrentPlaylist_PropertyChanged);
        }

        void CurrentPlaylist_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentTrack")
            {
                this.ListViewPlayList.SelectedItem = CurrentPlaylist.CurrentTrack;

                if (this.XMPPClient.XMPPState == XMPPState.Ready)
                {
                    if (CurrentPlaylist.CurrentTrack != null)
                        this.XMPPClient.SetTune(CurrentPlaylist.CurrentTrack.Name, CurrentPlaylist.CurrentTrack.Artist);
                    else
                        this.XMPPClient.SetTune("", "");
                }

            }
        }


        void JingleSessionManager_OnNewSession(string strSession, System.Net.XMPP.Jingle.JingleIQ iq, XMPPClient client)
        {
            foreach (MediaSession currsess in ObservSessionList)
            {
                if (currsess.RemoteJID == iq.From) /// Don't allow sessions from the same person twice
                {
                    XMPPClient.JingleSessionManager.TerminateSession(strSession, TerminateReason.Decline);
                    return;
                }
            }

            int nPort = GetNextPort();
            IPEndPoint ep = new IPEndPoint(addresses[0], nPort);
            JingleMediaSession session = new JingleMediaSession(strSession, iq, KnownAudioPayload.G722 | KnownAudioPayload.Speex16000 | KnownAudioPayload.Speex8000 | KnownAudioPayload.G711, ep, client);
            session.AudioRTPStream.RecvResampler = new BetterAudioResampler();
            session.AudioRTPStream.SendResampler = new BetterAudioResampler();

            session.AudioRTPStream.MediaType = RTP.MediaType.Send; // no decoding incoming packets

            if (session.RosterItem != null)
                session.RosterItem.PropertyChanged += new PropertyChangedEventHandler(RosterItem_PropertyChanged);

            try
            {
                SessionList.Add(strSession, session);
                ObservSessionList.Add(session);
                session.StartIncoming(iq);
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(DoUserAcceptCall), session);
            }
            catch (Exception ex)
            {
                SessionList.Remove(strSession);
                ObservSessionList.Remove(session);

                if (session.RosterItem != null)
                    session.RosterItem.PropertyChanged -= new PropertyChangedEventHandler(RosterItem_PropertyChanged);

                /// No compatible codecs probably
                XMPPClient.JingleSessionManager.TerminateSession(strSession, TerminateReason.MediaError);
                return;
            }


        }

        void RosterItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            /// See if this roster item goes off line, if it does, Terminate it's media session
            /// 
            RosterItem item = sender as RosterItem;
            if (item != null)
            {
                if (item.Presence.PresenceType != PresenceType.available)
                {
                    /// Find this session and remove it
                    /// 
                    JingleMediaSession foundsession = null;
                    foreach (JingleMediaSession session in ObservSessionList)
                    {
                        if (session.RosterItem == item)
                        {
                            foundsession = session;
                            break;
                        }
                    }

                    if (foundsession != null)
                    {
                        CloseSession(foundsession);
                    }
                }
            }
        }
        void DoUserAcceptCall(object obj)
        {
            JingleMediaSession session = obj as JingleMediaSession;
            session.UserAcceptSession();

            /// TODO.. reject if we're over a certain number
        }

        void JingleSessionManager_OnNewSessionAckReceived(string strSession, IQResponseAction response, XMPPClient client)
        {
            if (SessionList.ContainsKey(strSession) == true)
            {
                MediaSession session = SessionList[strSession];
                session.GotNewSessionAck();
            }
        }

        void JingleSessionManager_OnSessionTransportInfoReceived(string strSession, System.Net.XMPP.Jingle.JingleIQ jingleiq, XMPPClient client)
        {
            if (SessionList.ContainsKey(strSession) == true)
            {
                MediaSession session = SessionList[strSession];
                session.GotTransportInfo(jingleiq);
            }
        }

        void JingleSessionManager_OnSessionTransportInfoAckReceived(string strSession, IQResponseAction response, XMPPClient client)
        {
            if (SessionList.ContainsKey(strSession) == true)
            {
                MediaSession session = SessionList[strSession];
                session.GotSendTransportInfoAck();
            }
        }

        delegate void DelegateWindow(Window win);
        void SafeCloseWindow(Window win)
        {
            if ((win != null) && (win.IsLoaded == true))
                win.Close();
        }

        void JingleSessionManager_OnSessionTerminated(string strSession, XMPPClient client)
        {
            if (SessionList.ContainsKey(strSession) == true)
            {
                MediaSession session = SessionList[strSession];
                session.StopMedia(AudioMixer);
                SessionList.Remove(strSession);
                ObservSessionList.Remove(session);

                if (session.RosterItem != null)
                    session.RosterItem.PropertyChanged -= new PropertyChangedEventHandler(RosterItem_PropertyChanged);

            }
        }

        void JingleSessionManager_OnSessionAcceptedReceived(string strSession, System.Net.XMPP.Jingle.JingleIQ jingle, XMPPClient client)
        {
            Console.WriteLine("Session {0} has accepted our invitation", strSession);
            if (SessionList.ContainsKey(strSession) == true)
            {
                MediaSession session = SessionList[strSession];
                session.SessionAccepted(jingle, AudioMixer);
            }
        }

        void JingleSessionManager_OnSessionAcceptedAckReceived(string strSession, System.Net.XMPP.Jingle.IQResponseAction response, XMPPClient client)
        {
            if (response.AcceptIQ == true)
            {
                Console.WriteLine("Session {0} has said OK to our Accept invitation", strSession);
                if (SessionList.ContainsKey(strSession) == true)
                {
                    MediaSession session = SessionList[strSession];
                    session.GotAcceptSessionAck(AudioMixer);

                }
            }

        }

        private void ButtonClose_Click_1(object sender, RoutedEventArgs e)
        {
            MediaSession session = ((FrameworkElement)sender).DataContext as MediaSession;
            CloseSession(session);
        }

        void CloseSession(MediaSession session)
        {
            if (session != null)
            {
                session.StopMedia(AudioMixer);
                XMPPClient.JingleSessionManager.TerminateSession(session.Session, TerminateReason.Gone);
                SessionList.Remove(session.Session);
                ObservSessionList.Remove(session);

                if (session.RosterItem != null)
                    session.RosterItem.PropertyChanged -= new PropertyChangedEventHandler(RosterItem_PropertyChanged);

            }
        }

    

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            CurrentPlaylist.NextTrack();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            this.AudioFileReader.IsSourceActive = !this.AudioFileReader.IsSourceActive;
        }

        public PlayList CurrentPlaylist
        {
            get
            {
                return this.AudioFileReader.OurPlayList;
            }
        }

        private void ButtonQueueSong_Click(object sender, RoutedEventArgs e)
        {

            /// Let the user choose a song, then enqueue it
            /// 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic);
            dlg.Filter = "Audio Files (*.mp3;*.wma)|*.mp3;*.wma|All Files (*.*)|*.*";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == true)
            {
                foreach (string strFileName in dlg.FileNames)
                    CurrentPlaylist.Add(strFileName);
            }
        }

        private void ButtonRandom100_Click(object sender, RoutedEventArgs e)
        {
            //string strMusicDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic);
            string strMusicDirectory = "Music";
            Microsoft.WindowsAPICodePack.Shell.ShellLibrary music = Microsoft.WindowsAPICodePack.Shell.ShellLibrary.Load(strMusicDirectory, true);
            //string strDefault = music.DefaultSaveFolder;

            for (int i=0; i<music.Count; i++)
            {
                string strNextFolder = music[i].Path;
                CurrentPlaylist.PopulateFromDirectory(strNextFolder, 100);
            }

            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(TagThread), null);
        }

        void TagThread(object obj)
        {
            Track [] tracks = CurrentPlaylist.Tracks.ToArray();
            foreach (Track track in tracks)
            {
                track.GetTagData();
            }
        }

        private void ButtonDeleteQueue_Click(object sender, RoutedEventArgs e)
        {
            CurrentPlaylist.Clear();
        }


        private void ButtonMoveItemUp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonRemoveItemFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            Track track = this.ListViewPlayList.SelectedItem as Track;
            if (track == null)
                return;


            this.CurrentPlaylist.Remove(track);
        }

        private void ButtonOpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            dlg.Filter = "Playlists (*.playlist)|*.playlist|All Files (*.*)|*.*";
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
            {
                FileStream stream = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);
                DataContractSerializer ser = new DataContractSerializer(typeof(PlayList));

                try
                {
                    PlayList list = ser.ReadObject(stream) as PlayList;
                    this.CurrentPlaylist.Clone(list);
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    stream.Close();
                }
            }

        }

        private void ButtonSavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            dlg.Filter = "Playlists (*.playlist)|*.playlist|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                FileStream stream = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
                DataContractSerializer ser = new DataContractSerializer(typeof(PlayList));

                try
                {
                    ser.WriteObject(stream, this.CurrentPlaylist);
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    stream.Close();
                }
            }
        }


        private void ListViewPlayList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                string[] strList = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                if (strList != null)
                {
                    foreach (string strFile in strList)
                    {
                        this.CurrentPlaylist.Add(strFile);
                    }
                }

            }
        }

        private void ListViewPlayList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
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

                XMPPClient.XMPPAccount.Capabilities = new Capabilities();
                XMPPClient.XMPPAccount.Capabilities.Node = "http://xmedianet.codeplex.com/wpfclient/caps";
                XMPPClient.XMPPAccount.Capabilities.Version = "1.0";
                XMPPClient.XMPPAccount.Capabilities.Extensions = "voice-v1"; /// google talk capabilities

                XMPPClient.FileTransferManager.AutoDownload = true;
                XMPPClient.AutoAcceptPresenceSubscribe = true;
                XMPPClient.Connect();

            }
            else if (XMPPClient.XMPPState > XMPPState.Connected)
            {
                XMPPClient.Disconnect();
            }
        }


        void SaveAccount()
        {

            string strPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string strFileName = string.Format("{0}\\{1}", strPath, "musicaccount.xml");
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
            string strFileName = string.Format("{0}\\{1}", strPath, "musicaccount.xml");
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

       
        static int FirstPort = 30000;
        static int LastPort = 30100;

        static int PortOn = 30000;

        static MainWindow()
        {
            Random rand = new Random();
            PortOn = rand.Next(80) + FirstPort;
            if (PortOn % 2 != 0)
                PortOn++;
        }


        public static int GetNextPort()
        {
            int nRet = PortOn;
            PortOn += 2;
            if (PortOn > LastPort)
                PortOn = FirstPort;
            return nRet;
        }


        public static IPAddress[] FindAddresses()
        {
            List<IPAddress> IPs = new List<IPAddress>();
            /// See what interfaces can connect to our itpcluster
            /// 

            IPAddress BindAddress = IPAddress.Any;
            NetworkInterface[] infs = NetworkInterface.GetAllNetworkInterfaces();


            foreach (NetworkInterface inf in infs)
            {
                try
                {
                    IPInterfaceProperties props = inf.GetIPProperties();
                    if (props == null)
                        continue;

                    IPv4InterfaceProperties ip4 = props.GetIPv4Properties();
                    if (ip4 == null)  /// TODO.. allow for IPV6 interfaces
                        continue;
                    if (ip4.IsAutomaticPrivateAddressingActive == true)
                        continue;
                    foreach (UnicastIPAddressInformation addrinfo in props.UnicastAddresses)
                    {

                        if (addrinfo.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                            continue;

                        //addrinfo.SuffixOrigin == SuffixOrigin.OriginDhcp
                        //addrinfo.PrefixOrigin == PrefixOrigin.Dhcp

                        if (addrinfo.PrefixOrigin == PrefixOrigin.WellKnown)
                            continue; /// ignore well known IP addresses


                        if (addrinfo.Address.Equals(IPAddress.Any) == false)
                        {

                            if (addrinfo.Address.Equals(IPAddress.Parse("127.0.0.1")) == false)
                                IPs.Add(new IPAddress(addrinfo.Address.GetAddressBytes()));
                        }

                    }
                }
                catch (Exception)
                {
                }
            }

            return IPs.ToArray();
        }


        private void ListViewPlayList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Track track = this.ListViewPlayList.SelectedItem as Track;
            if (track == null)
                return;

            this.CurrentPlaylist.CurrentTrack = track;
        }


        #region IAudioSink Members

        /// <summary>
        ///  Push to our speaker
        /// </summary>
        /// <param name="sample"></param>
        public void PushSample(MediaSample sample, object objSource)
        {
            //lock (SpeakerLock)
            //{
            //    // send this sample to our speakers, please
            //    if (Speaker != null)
            //    {
            //        Speaker.PushSample(sample, this);
            //    }
            //}
        }

        private bool m_bSpeakerMute = false;

        public bool SpeakerMute
        {
            get { return m_bSpeakerMute; }
            set
            {
                if (m_bSpeakerMute != value)
                {
                    m_bSpeakerMute = value;
                }
            }
        }

        public bool IsSinkActive
        {
            get
            {
                return !m_bSpeakerMute;
            }
            set
            {
            }
        }

        double m_fSinkAmplitudeMultiplier = 1.0f;
        public double SinkAmplitudeMultiplier
        {
            get { return m_fSinkAmplitudeMultiplier; }
            set { m_fSinkAmplitudeMultiplier = value; }
        }

        #endregion

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {

        }


      
      
    }
}
