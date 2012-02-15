using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Net.XMPP;
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;

namespace XMPPClient
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            /// Load our options
            /// 
            LoadOptions();
            if (Options.RunWithScreenLocked == true)
               PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;

             XMPPClient.OnXMLReceived += new System.Net.XMPP.XMPPClient.DelegateString(XMPPClient_OnXMLReceived);
             XMPPClient.OnXMLSent += new System.Net.XMPP.XMPPClient.DelegateString(XMPPClient_OnXMLSent);


            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        public static System.Text.StringBuilder XMPPLogBuilder = new System.Text.StringBuilder();
        void XMPPClient_OnXMLSent(System.Net.XMPP.XMPPClient client, string strXML)
        {
            if (Options.LogXML == true)
               XMPPLogBuilder.AppendFormat("--> {0}\r\n", strXML);
        }

        void XMPPClient_OnXMLReceived(System.Net.XMPP.XMPPClient client, string strXML)
        {
            if (Options.LogXML == true)
                XMPPLogBuilder.AppendFormat("<-- {0}\r\n", strXML);
        }


        public static void LoadOptions()
        {
            string strFilename = "options.xml";
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Load from storage
                IsolatedStorageFileStream location = null;
                try
                {
                    location = new IsolatedStorageFileStream(strFilename, System.IO.FileMode.Open, storage);
                    DataContractSerializer ser = new DataContractSerializer(typeof(Options));

                    Options = ser.ReadObject(location) as Options;
                }
                catch (Exception ex)
                {
                    Options = new Options();
                }
                finally
                {
                    if (location != null)
                        location.Close();
                }

            }
        }

        public static void SaveOptions()
        {
            string strFilename = "options.xml";
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Load from storage
                IsolatedStorageFileStream location = null;
                try
                {
                    location = new IsolatedStorageFileStream(strFilename, System.IO.FileMode.Create, storage);
                    DataContractSerializer ser = new DataContractSerializer(typeof(Options));
                    ser.WriteObject(location, Options);
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (location != null)
                        location.Close();
                }

            }
        }

        public static System.Net.XMPP.XMPPClient XMPPClient = new System.Net.XMPP.XMPPClient();
        public static Options Options = new Options();

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
          
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (WasConnected == true)
            {
                App.XMPPClient.Disconnect();
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(DoConnect));
            }
        }



        public static void WaitConnected()
        {
            /// Wait for the client to reconnect... should have already been activated above
            if (App.XMPPClient.Connected == false)
                App.XMPPClient.ConnectHandle.WaitOne(15000);
        }

        static void DoConnect(object junk)
        {
            App.XMPPClient.Connect();
        }

        public static bool WasConnected = false;

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            WasConnected = App.XMPPClient.Connected;
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is QuitException)
                return;

            if (MessageBox.Show("An exception has occurred in this application, would you like to forward the details to the developers?", "Application Exception", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Microsoft.Phone.Tasks.EmailComposeTask task = new Microsoft.Phone.Tasks.EmailComposeTask();
                task.Body = e.ExceptionObject.ToString();
                if (XMPPLogBuilder.Length > 0)
                    task.Body += "\r\n\r\n" + XMPPLogBuilder.ToString();

                task.Subject = "[XMPPClient] Crash Notification Details";
                task.To = "bonnbria@gmail.com";
                task.Show();
            }
                

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            RootFrame.Obscured += new EventHandler<ObscuredEventArgs>(RootFrame_Obscured);
            RootFrame.Unobscured += new EventHandler(RootFrame_Unobscured);

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        void RootFrame_Unobscured(object sender, EventArgs e)
        {
            
        }

        void RootFrame_Obscured(object sender, ObscuredEventArgs e)
        {
            
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}