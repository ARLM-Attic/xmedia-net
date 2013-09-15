using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using XAlerts.Resources;

using System.Net.XMPP;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

namespace XAlerts
{
    public partial class App : Application
    {
      

        public static System.Net.XMPP.XMPPClient client = new System.Net.XMPP.XMPPClient();

        public static System.Net.XMPP.ObservableCollectionEx<Subscription> PubSubSubscriptions = new ObservableCollectionEx<Subscription>();

        public static void Connect()
        {
            if (client.XMPPState != XMPPState.Ready)
            {
                client.Connect();
            }
        }

        public static void Disconnect()
        {
            if (client.XMPPState != XMPPState.Ready)
            {
                client.Disconnect();
            }
        }

        static System.Threading.ManualResetEvent LoadDoneEvent = new System.Threading.ManualResetEvent(false);
        static bool m_bLoaded = false;
        static void client_OnStateChanged(object sender, EventArgs e)
        {
            if (client.XMPPState == XMPPState.Ready)
            {
                if (m_bLoaded == false)
                {
                   System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(SubscribeThread));
                    m_bLoaded = true;
                }
            }
        }

        static void SubscribeThread(object obj)
        {
            LoadDoneEvent.Reset();
            LoadStuff();
            LoadDoneEvent.WaitOne();


            foreach (Subscription sub in PubSubSubscriptions)
            {
                if ((sub.SubscriptionId != null) && (sub.SubscriptionId.Length > 0))
                    GetAllItemsNodeThread(sub);
                else
                    SubscribeNodeThread(sub);
            }

        }
    

        public static void AddNewNode(string strNodeName)
        {
            Subscription sub = new Subscription() { NodeName = strNodeName };
            sub.Events = new PubSubNodeManager<AlertMessage>(strNodeName, client);
            client.AddLogic(sub.Events);
            PubSubSubscriptions.Add(sub);
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(SubscribeNodeThread), sub);
        }

        static void SubscribeNodeThread(object obj)
        {

            Subscription sub = obj as Subscription;
            sub.SubscriptionId = PubSubOperation.SubscribeNode(client, sub.NodeName, client.JID, true);
            sub.Events.GetAllItems(sub.SubscriptionId);

            SaveStuff();
        }

        static void GetAllItemsNodeThread(object obj)
        {
            Subscription sub = obj as Subscription;
            sub.Events.GetAllItems(sub.SubscriptionId);
        }


        static void LoadStuff()
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                //Subscriptions.Clear();

                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string strFileName = string.Format("Alerts_{0}.xml", client.JID).Replace("/", "-");

                    if (storage.FileExists(strFileName) == true)
                    {
                        // Load from storage
                        IsolatedStorageFileStream location = null;
                        try
                        {
                            location = new IsolatedStorageFileStream(strFileName, System.IO.FileMode.Open, storage);
                            DataContractSerializer ser = new DataContractSerializer(typeof(Subscription []));
                             
                            Subscription [] subs = ser.ReadObject(location) as Subscription [];

                            foreach (Subscription sub in subs)
                            {
                                bool bExists = false;
                                foreach (Subscription testsub in PubSubSubscriptions)
                                {
                                    if (sub.NodeName == testsub.NodeName)
                                    {
                                        bExists = true;
                                        break;
                                    }
                                }
                                if (bExists == true)
                                    continue;

                                sub.Events = new PubSubNodeManager<AlertMessage>(sub.NodeName, client);
                                client.AddLogic(sub.Events);
                                PubSubSubscriptions.AddNoNotify(sub);
                            }

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
                LoadDoneEvent.Set();
            });

            
        }

        static void SaveStuff()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Load from storage
                string strFileName = string.Format("Alerts_{0}.xml", client.JID).Replace("/", "-");
                IsolatedStorageFileStream location = new IsolatedStorageFileStream(strFileName, System.IO.FileMode.Create, storage);
                DataContractSerializer ser = new DataContractSerializer(typeof(Subscription []));


                try
                {
                    ser.WriteObject(location, PubSubSubscriptions.ToArray());
                }
                catch (Exception)
                {
                }
                location.Close();
            }

        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        static App()
        {
            client.OnStateChanged += client_OnStateChanged;
        }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            /// Fake data first
            /// 

            //Subscription one = new Subscription() { NodeName = "RIMS Production" };
            //one.Events.Items.Add(new AlertMessage() { AlertNode = "RIMS Production", Message = "Bill Liese hacking system", Time = DateTime.Now, Device = "RIMS Web Service", Event = "FailedLogin", Source = "Monitor", Guid = "235625" });
            //one.Events.Items.Add(new AlertMessage() { AlertNode = "RIMS Production", Message = "Fred failed", Time = DateTime.Now, Device = "RIMS Web Servic", Event = "Outage", Source = "Monitor", Guid = "ls73457kjdf" });
            //Subscription two = new Subscription() { NodeName = "ITP Dialogics" };
            //two.Events.Items.Add(new AlertMessage() { AlertNode = "ITP Dialogics", Message = "Dialogic1 down", Time = DateTime.Now, Device = "Dialogic1", Event = "Outage", Source = "Monitor", Guid = "lskjdf" });
            //two.Events.Items.Add(new AlertMessage() { AlertNode = "ITP Dialogics", Message = "Dialogic2 down", Time = DateTime.Now, Device = "Dialogic2", Event = "Outage", Source = "Monitor", Guid = "lskjdssdff" });

            //Subscriptions.Add(one);
            //Subscriptions.Add(two);
            


            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
          
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            Disconnect();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
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

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
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

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }
    }
}