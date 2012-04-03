/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

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
using System.Net.XMPP.Jingle;
using RTP;
using AudioClasses;

using System.Net;
using System.Net.NetworkInformation;
using System.ComponentModel;

namespace WPFXMPPClient
{
    /// <summary>
    /// Interaction logic for AudioMuxerWindow.xaml
    /// </summary>
    public partial class AudioMuxerWindow : Window, IAudioSink, IAudioSource, INotifyPropertyChanged
    {
        public AudioMuxerWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Our audio muxer.  Takes our microphone device and each incoming RTP stream as input, outputs
        /// to our speaker device and each outgoing RTP stream.  May also have a recording interface as ouput and tone/song generators as inputs
        /// Currently only supports HD voice because that's all the dmo's can echo cancel, but may add support for AAC if that becomes available
        /// </summary>
        AudioConferenceMixer AudioMixer = new AudioConferenceMixer(AudioFormat.SixteenBySixteenThousandMono);

        ObservableCollectionEx<JingleMediaSession> ObservSessionList = new ObservableCollectionEx<JingleMediaSession>();
        Dictionary<string, JingleMediaSession> SessionList = new Dictionary<string, JingleMediaSession>();

        /// <summary>
        /// Our XMPP client
        /// </summary>
        XMPPClient XMPPClient = null;

        public void RegisterXMPPClient(XMPPClient client)
        {
            addresses = AudioMuxerWindow.FindAddresses();

            XMPPClient = client;
            XMPPClient.JingleSessionManager.OnNewSession += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventWithInfo(JingleSessionManager_OnNewSession);
            XMPPClient.JingleSessionManager.OnNewSessionAckReceived += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventBool(JingleSessionManager_OnNewSessionAckReceived);
            XMPPClient.JingleSessionManager.OnSessionAcceptedAckReceived += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventBool(JingleSessionManager_OnSessionAcceptedAckReceived);
            XMPPClient.JingleSessionManager.OnSessionAcceptedReceived += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventWithInfo(JingleSessionManager_OnSessionAcceptedReceived);
            XMPPClient.JingleSessionManager.OnSessionTerminated += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEvent(JingleSessionManager_OnSessionTerminated);
            XMPPClient.JingleSessionManager.OnSessionTransportInfoReceived += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventWithInfo(JingleSessionManager_OnSessionTransportInfoReceived);

            /// Get all our speaker and mic devices
            /// 
            MicrophoneDevices = ImageAquisition.NarrowBandMic.GetMicrophoneDevices();
            SpeakerDevices = ImageAquisition.NarrowBandMic.GetSpeakerDevices();

        }

        AudioClasses.AudioDevice[] MicrophoneDevices = null;
        AudioClasses.AudioDevice[] SpeakerDevices = null;

        ImageAquisition.NarrowBandMic Microphone = null;
        DirectShowFilters.SpeakerFilter Speaker = null;
        bool m_bAudioActive = false;


        // TODO.. need speaker play object/method
        ImageAquisition.AudioDeviceVolume MicrophoneVolume = null;
        ImageAquisition.AudioDeviceVolume SpeakerVolume = null;

        IPAddress[] addresses = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            this.ListViewAudioSessions.ItemsSource = ObservSessionList;

            if ((MicrophoneDevices != null) && (MicrophoneDevices.Length > 0))
            {
                this.ComboBoxMicDevices.ItemsSource = MicrophoneDevices;
                this.ComboBoxMicDevices.SelectedItem = MicrophoneDevices[0];
                MicrophoneVolume = new ImageAquisition.AudioDeviceVolume(MicrophoneDevices[0]);
                this.SliderMicVolume.DataContext = MicrophoneVolume;
            }
            if ((SpeakerDevices != null) && (SpeakerDevices.Length > 0))
            {
                this.ComboBoxSpeakerDevices.ItemsSource = SpeakerDevices;
                this.ComboBoxSpeakerDevices.SelectedItem = SpeakerDevices[0];
                SpeakerVolume = new ImageAquisition.AudioDeviceVolume(SpeakerDevices[0]);
                this.SliderSpeakerVolume.DataContext = SpeakerVolume;
            }


            this.DataContext = this;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
                this.DragMove();

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        ToneGenerator OurToneGenerator = new ToneGenerator(350, 440);
        //TonGenerator noise2 = new TonGenerator(1004, 1004);

        void StartMicrophoneAndSpeaker(AudioFormat format)
        {
            if (this.IsLoaded == false)
            {
                this.Show();
            }
                 

            if (Microphone != null)
                return;

            PushPullObject thismember = AudioMixer.AddInputOutputSource(this, this);
            PushPullObject tonemember = AudioMixer.AddInputOutputSource(OurToneGenerator, OurToneGenerator);
            OurToneGenerator.IsSourceActive = false;
            //thismember.SourceExcludeList.Clear();  // clear so we can hear our mic
            thismember.SourceExcludeList.Add(tonemember.AudioSource); /// we don't want to hear the tone we're sending

            AudioDevice micdevice = this.ComboBoxMicDevices.SelectedItem as AudioDevice;
            AudioDevice speakdevice = this.ComboBoxSpeakerDevices.SelectedItem as AudioDevice;

            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(Window.GetWindow(this));


            if (SpeakerVolume != null)
            {
                SpeakerVolume.Dispose();
                SpeakerVolume = null;
            }
            if (MicrophoneVolume != null)
            {
                MicrophoneVolume.Dispose();
                MicrophoneVolume = null;
            }

            MicrophoneVolume = new ImageAquisition.AudioDeviceVolume(micdevice);
            this.SliderMicVolume.DataContext = MicrophoneVolume;

            SpeakerVolume = new ImageAquisition.AudioDeviceVolume(speakdevice);
            this.SliderSpeakerVolume.DataContext = SpeakerVolume;

            Microphone = new ImageAquisition.NarrowBandMic(micdevice, speakdevice.Guid, helper.Handle);
            Microphone.AGC = true;
            Speaker = new DirectShowFilters.SpeakerFilter(speakdevice.Guid, 20, format, helper.Handle);
            Speaker.Start();
            Microphone.Start();
            m_bAudioActive = true;
            AudioMixer.Start();
        }

        void StopMicrophoneAndSpeaker()
        {
            AudioMixer.Stop();
            Microphone.Stop();
            Speaker.Stop();
            Microphone = null;
            Speaker.Dispose();
            Speaker = null;
        }

        /// <summary>
        ///  See if we have an active call to this jid, if not, start one
        /// </summary>
        /// <param name="item"></param>
        public void InitiateOrShowCallTo(JID jidto)
        {
            StartMicrophoneAndSpeaker(AudioFormat.SixteenBySixteenThousandMono);

            if (addresses.Length <= 0)
                throw new Exception("No IP addresses on System");

            int nPort = GetNextPort();
            IPEndPoint ep = new IPEndPoint(addresses[0], nPort);


            
            /// may need a lock here to make sure we have this session added to our list before the xmpp response gets back, though this should be many times faster than network traffic
            JingleMediaSession jinglesession = new JingleMediaSession(jidto, ep, XMPPClient);
            jinglesession.UseStun = UseStun;
            try
            {
                string strSession = jinglesession.SendInitiateSession();
                ObservSessionList.Add(jinglesession);
                SessionList.Add(strSession, jinglesession);
            }
            catch (Exception ex)
            {
                /// Should never happen
            }
        }

        private bool m_bUseStun = true;

        public bool UseStun
        {
            get { return m_bUseStun; }
            set { m_bUseStun = value; }
        }

        void JingleSessionManager_OnNewSession(string strSession, System.Net.XMPP.Jingle.Jingle jingle, XMPPClient client)
        {
            bool bAcceptNewCall = (bool)this.Dispatcher.Invoke(new DelegateAcceptSession(ShouldAcceptSession), strSession, jingle);

            if (bAcceptNewCall == true)
            {
                int nPort = GetNextPort();
                IPEndPoint ep = new IPEndPoint(addresses[0], nPort);

                JingleMediaSession session = new JingleMediaSession(strSession, jingle, KnownAudioPayload.G722|KnownAudioPayload.Speex16000 | KnownAudioPayload.Speex8000 | KnownAudioPayload.G711, ep, client);
                session.UseStun = UseStun;
                try
                {
                    session.SendAcceptSession();
                    SessionList.Add(strSession, session);
                    ObservSessionList.Add(session);
                }
                catch (Exception ex)
                {
                    /// No compatible codecs probably
                    XMPPClient.JingleSessionManager.TerminateSession(strSession, TerminateReason.MediaError);
                }
            }
            else
            {
                XMPPClient.JingleSessionManager.TerminateSession(strSession, TerminateReason.Decline);
            }

        }


        void JingleSessionManager_OnSessionTransportInfoReceived(string strSession, System.Net.XMPP.Jingle.Jingle jingle, XMPPClient client)
        {
            if (SessionList.ContainsKey(strSession) == true)
            {
                JingleMediaSession session = SessionList[strSession];
                session.GotTransportInfo(jingle);
                session.SendAcceptSession();
            }
        }


        void JingleSessionManager_OnSessionTerminated(string strSession, XMPPClient client)
        {
            if (SessionList.ContainsKey(strSession) == true)
            {
                JingleMediaSession session = SessionList[strSession];
                session.StopMedia(AudioMixer);
                SessionList.Remove(strSession);
                ObservSessionList.Remove(session);
            }
        }

        void JingleSessionManager_OnSessionAcceptedReceived(string strSession, System.Net.XMPP.Jingle.Jingle jingle, XMPPClient client)
        {
            Console.WriteLine("Session {0} has accepted our invitation", strSession);
            if (SessionList.ContainsKey(strSession) == true)
            {
                JingleMediaSession session = SessionList[strSession];
                session.SessionAccepted(jingle);
                session.StartMedia(AudioMixer);
            }
        }

        void JingleSessionManager_OnSessionAcceptedAckReceived(string strSession, System.Net.XMPP.Jingle.IQResponseAction response, XMPPClient client)
        {
            if (response.AcceptIQ == true)
            {
                Console.WriteLine("Session {0} has said OK to our Accept invitation", strSession);
                if (SessionList.ContainsKey(strSession) == true)
                {
                    JingleMediaSession session = SessionList[strSession];
                    session.StartMedia(AudioMixer);
                }
            }
            
        }

        void JingleSessionManager_OnNewSessionAckReceived(string strSession, System.Net.XMPP.Jingle.IQResponseAction response, XMPPClient client)
        {
            
        }

        delegate bool DelegateAcceptSession(string strSession, System.Net.XMPP.Jingle.Jingle jingle);

        bool ShouldAcceptSession(string strSession, System.Net.XMPP.Jingle.Jingle jingle)
        {
            StartMicrophoneAndSpeaker(AudioFormat.SixteenBySixteenThousandMono);

            if (AutoAnswer == true)
                return true;

            if (MessageBox.Show(string.Format("Accept new Call from {0}", jingle.Initiator), "New Call", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                return true;
            return false;
        }


        private bool m_bAutoAnswer = false;

        public bool AutoAnswer
        {
            get { return m_bAutoAnswer; }
            set 
            {
                if (m_bAutoAnswer != value)
                {
                    m_bAutoAnswer = value;
                    FirePropertyChanged("AutoAnswer");
                }
            }
        }


        #region IAudioSink Members

        /// <summary>
        ///  Push to our speaker
        /// </summary>
        /// <param name="sample"></param>
        public void PushSample(MediaSample sample)
        {
            lock (SpeakerLock)
            {
                // send this sample to our speakers, please
                if (Speaker != null)
                {
                    Speaker.PushSample(sample);
                }
            }
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
                    FirePropertyChanged("SpeakerMute");
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

        #region IAudioSource Members

        //Pull a sample from our mic to send to the conference
        public MediaSample PullSample(AudioFormat format, TimeSpan tsDuration)
        {
            lock (MicLock)
            {
                if (Microphone != null)
                {
                    return Microphone.PullSample(format, tsDuration);
                }
                else
                    return null;
            }
        }


        public bool m_bMuted = false;
        public bool IsSourceActive
        {
            get
            {
                return !m_bMuted;
            }
            set
            {
                if (m_bMuted != value)
                {
                    m_bMuted = !value;
                    FirePropertyChanged("IsSourceActive");
                }
            }
        }
        public bool Muted
        {
            get
            {
                return m_bMuted;
            }
            set
            {
                if (m_bMuted != value)
                {
                    m_bMuted = value;
                    FirePropertyChanged("Muted");
                }
            }
        }

        double m_fSourceAmplitudeMultiplier = 1.0f;
        public double SourceAmplitudeMultiplier
        {
            get { return m_fSourceAmplitudeMultiplier; }
            set { m_fSourceAmplitudeMultiplier = value; }
        }
        

        #endregion

        static int FirstPort = 30000;
        static int LastPort = 30100;

        static int PortOn = 30000;

        static AudioMuxerWindow()
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

        private void ButtonClose_Click_1(object sender, RoutedEventArgs e)
        {
            JingleMediaSession session = ((FrameworkElement)sender).DataContext as JingleMediaSession;
            if (session != null)
            {
                session.StopMedia(AudioMixer);
                XMPPClient.JingleSessionManager.TerminateSession(session.Session, TerminateReason.Gone);
                SessionList.Remove(session.Session);
                ObservSessionList.Remove(session);

            }
        }

        private void ButtonStartMixer_Click(object sender, RoutedEventArgs e)
        {
            StartMicrophoneAndSpeaker(AudioFormat.SixteenBySixteenThousandMono);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }
        #endregion

        object MicLock = new object();
        object SpeakerLock = new object();
        void ResetMicAndSpeakers()
        {

            AudioDevice micdevice = this.ComboBoxMicDevices.SelectedItem as AudioDevice;
            AudioDevice speakdevice = this.ComboBoxSpeakerDevices.SelectedItem as AudioDevice;
            if ((micdevice == null) || (speakdevice == null))
                return;


            if (SpeakerVolume != null)
            {
                SpeakerVolume.Dispose();
                SpeakerVolume = null;
            }
            if (MicrophoneVolume != null)
            {
                MicrophoneVolume.Dispose();
                MicrophoneVolume = null;
            }

            MicrophoneVolume = new ImageAquisition.AudioDeviceVolume(micdevice);
            this.SliderMicVolume.DataContext = MicrophoneVolume;

            SpeakerVolume = new ImageAquisition.AudioDeviceVolume(speakdevice);
            this.SliderSpeakerVolume.DataContext = SpeakerVolume;


            if (Microphone == null) /// Haven't start yet, no need to restart
                return;

            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(Window.GetWindow(this));

            lock (MicLock)
            {
                lock (SpeakerLock)
                {
                    Microphone.Stop();
                    Speaker.Stop();
                    Microphone = null;
                    Speaker.Dispose();
                    Speaker = null;


                    Speaker = new DirectShowFilters.SpeakerFilter(speakdevice.Guid, 20, AudioFormat.SixteenBySixteenThousandMono, helper.Handle);
                    Microphone = new ImageAquisition.NarrowBandMic(micdevice, speakdevice.Guid, helper.Handle);
                    Microphone.AGC = true;
                    Speaker.Start();
                    Microphone.Start();
                }
            }
        }
        private void ComboBoxSpeakerDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetMicAndSpeakers();
    
        }

        private void ComboBoxMicDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetMicAndSpeakers();
        }
    }

  
}
