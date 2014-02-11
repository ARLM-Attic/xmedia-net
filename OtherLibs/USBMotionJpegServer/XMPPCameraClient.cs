using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.XMPP;
using System.Reflection;
using System.Text.RegularExpressions;

using AudioClasses;
using System.Runtime.Serialization;
using ImageAquisition;
using WPFImageWindows;
using SensorLibrary;

namespace USBMotionJpegServer
{
    public class XMPPCameraClient : XMPPClient
    {
        public XMPPCameraClient(VideoCaptureSource videosource, string strName)
            : base(new xmedianet.socketserver.ConsoleLogClient())
        {
            VideoSource = videosource;
            Name = strName.Replace("/", "").Replace("\\", "").Replace(" ", ""); ;
            MessageBuilder = new SensorMessagBuilder(this);
            //this.RemoveLogic(base.PersonalEventingLogic);
            this.RemoveLogic(base.StreamInitiationAndTransferLogic);
            this.RemoveLogic(base.JingleSessionManager);
            this.RemoveLogic(base.ServiceDiscoveryLogic);

            this.OnNewConversationItem += EventNewConversation;

            if (videosource.RecorderWithMotion.MotionDetector != null)
            {
                videosource.RecorderWithMotion.MotionDetector.OnMotionDetected += MotionDetector_OnMotionDetected;
            }
        }

        protected VideoCaptureSource VideoSource = null;
        protected SensorMessagBuilder MessageBuilder = null;
        protected string Name = "";
        public bool Connect(string XMPPDomain, string XMPPServer, string XMPPUser, string XMPPPassword)
        {
             Domain = Properties.Settings.Default.XMPPDomain;
             Server = Properties.Settings.Default.XMPPServer;
             UserName = Properties.Settings.Default.XMPPUser;
             Password = Properties.Settings.Default.XMPPPassword;
             Resource = Name;
             AutoReconnect = true;
             RetrieveRoster = true;
             AutoAcceptPresenceSubscribe = true;
             AutoQueryServerFeatures = false;
             AutomaticallyDownloadAvatars = false;
             UseTLS = true;
             Connect();
             bool bRet = ConnectHandle.WaitOne(15000);
             if (bRet == true)
                UpdatePresence();
             return bRet;

        }

        protected void EventNewConversation(RosterItem item, bool bReceived, TextMessage msg)
        {
            HandleOnNewConversationItem(item, bReceived, msg);
        }

        void MotionDetector_OnMotionDetected(object sender, EventArgs e)
        {
            FireEvent(VideoSource.RecorderWithMotion.MotionDetector.LastMeasuredValue.ToString(), MotionStarted, "MotionStarted");
        }

        [SensorEvent]
        public event DelegateSensorEvent MotionStarted = null;

        [SensorEvent]
        public event DelegateSensorEvent MotionCleared = null;


        [SensorProperty]
        public bool Motion
        {
            get
            {
                if (VideoSource.RecorderWithMotion.MotionDetector.LastMeasuredValue >= VideoSource.RecorderWithMotion.MotionDetector.Threshold)
                    return true;
                return false;
            }
        }

        [SensorProperty]
        public bool Recording
        {
            get
            {
                return VideoSource.RecorderWithMotion.Recording;
            }
        }

        [SensorProperty]
        public int VideoWidth
        {
            get
            {
                return VideoSource.VideoCaptureDevice.ActiveVideoCaptureRate.Width;
            }
        }

        [SensorProperty]
        public int VideoHeight
        {
            get
            {
                return VideoSource.VideoCaptureDevice.ActiveVideoCaptureRate.Height;
            }
        }

        [SensorProperty]
        public int Focus
        {
            set
            {
                VideoSource.SetFocus(value);
            }
        }

        [SensorProperty]
        public int Exposure
        {
            set
            {
                VideoSource.SetExposure(value);
            }
        }

        [SensorProperty]
        public int Zoom
        {
            set
            {
                VideoSource.Zoom (value);
            }
        }

        [SensorProperty]
        public string ExposureRange
        {
            get
            {
                int nMin, nMax, nStep, nDefault;
                VideoSource.GetExposureRange(out nMin, out nMax, out nStep, out nDefault);
                return string.Format("Min: {0}, Max: {1}, Step: {2}, Default: {3}", nMin, nMax, nStep, nDefault);
            }
        }

        [SensorProperty]
        public string FocusRange
        {
            get
            {
                int nMin, nMax, nStep, nDefault;
                VideoSource.GetFocusRange(out nMin, out nMax, out nStep, out nDefault);
                return string.Format("Min: {0}, Max: {1}, Step: {2}, Default: {3}", nMin, nMax, nStep, nDefault);
            }
        }


        [SensorActionAttribute]
        public void Record()
        {
            VideoSource.StartRecording();
        }

        [SensorActionAttribute]
        public void Stop()
        {
            VideoSource.StopRecording();
        }

        [SensorActionAttribute]
        public void Left()
        {
            VideoSource.PanLeft();
        }

        [SensorActionAttribute]
        public void Right()
        {
            VideoSource.PanRight();
        }

        public void FireEvent(string strValue, DelegateSensorEvent eventname, string strDelegateEventName)
        {
            SensorEvent eventinfo = new SensorEvent();
            if (eventname != null)
                eventinfo.Event = eventname.Method.Name;
            else
                eventinfo.Event = strDelegateEventName;
            eventinfo.Source = this.Name;
            eventinfo.Start = DateTime.Now;
            eventinfo.Value = strValue;
            FireEvent(eventinfo, eventname, strDelegateEventName);
        }

        public virtual void FireEvent(SensorEvent eventinfo, DelegateSensorEvent eventname, string strDelegateEventName)
        {
            Console.WriteLine(string.Format("Calling FireEvent {0}, state is {1}", eventinfo, XMPPState));
            if (eventinfo == null)
                return;

            if (eventname != null)
                eventname.Invoke(this, eventinfo);

            /// Send to all users
            if (XMPPState == XMPPState.Ready)
            {
                RosterItem[] items = RosterItems.ToArray();
                foreach (RosterItem item in items)
                {
                    foreach (var clientinstance in item.ClientInstances)
                    {
                        if (clientinstance.FullJID == this.JID.FullJID)
                            continue; // don't send it to our self for now

                        try
                        {
                            /// Send our our message  (TODO... send to all instances as well, and not to ourselves?)
                            SensorEventMessage msg = new SensorEventMessage();
                            msg.Type = IQType.result.ToString();
                            msg.To = clientinstance.FullJID;
                            msg.From = this.JID;
                            msg.Event = eventinfo;
                            this.SendObject(msg);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("Exception Sending object: {0} ", ex));
                        }
                    }
                }

                if (items.Length <= 0)
                    Console.WriteLine("No Roster Items found to send event to");


            }


        }



        /// <summary>
        /// Old way of handling stuff, in case the client only speaks messages
        /// </summary>
        /// <param name="item"></param>
        /// <param name="bReceived"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected virtual bool HandleOnNewConversationItem(RosterItem item, bool bReceived, TextMessage msg)
        {
            if (bReceived == true)
                Console.WriteLine(string.Format("{0}: {1}", msg.From, msg.Message));

            if (string.Compare(msg.Message, "record", true) == 0)
            {
                Record();
                return true;
            }
            else if (string.Compare(msg.Message, "stop", true) == 0)
            {
                Stop();
                return true;
            }
            else if (string.Compare(msg.Message, "left", true) == 0)
            {
                Left();
                return true;
            }
            else if (string.Compare(msg.Message, "right", true) == 0)
            {
                Right();
                return true;
            }
            else if (string.Compare(msg.Message, "query", true) == 0)
            {
                SendChatMessage(Query(), msg.From);
                return true;
            }
            else
            {
                Match matchman = Regex.Match(msg.Message, @"set\s+(?<property>\w+)\=(?<value>.+$)", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (matchman.Success == true)
                {
                    Set(matchman.Groups["property"].Value, matchman.Groups["value"].Value);
                    SendChatMessage("success", msg.From);
                    return true;
                }

            }


            return false;
        }

        protected override bool OnIQ(IQ iq)
        {
            if (iq is SensorActionIQ)
            {
                SensorActionIQ sensoriq = iq as SensorActionIQ;

                SensorResultIQ result = new SensorResultIQ();
                result.Result = new Result();
                result.Error = null;
                result.Type = IQType.result.ToString();
                result.To = sensoriq.From;
                result.From = this.JID;
                result.ID = sensoriq.ID;


                Type type = GetType();

                if (sensoriq.Action.Verb == Verb.query) /// get the info for this object
                {
                    result.Result = null;
                    result.ClassInfo = new ClassInformation();

                    List<MethodInformation> ourmethods = new List<MethodInformation>();
                    List<MethodInformation> ourevents = new List<MethodInformation>();
                    List<PropertyInformation> ourproperties = new List<PropertyInformation>();

                    PropertyInfo[] props = type.GetProperties();
                    foreach (PropertyInfo prop in props)
                    {
                        object[] attrs = prop.GetCustomAttributes(typeof(SensorPropertyAttribute), true);
                        if ((attrs != null) && (attrs.Length > 0))
                        {
                            ourproperties.Add(new PropertyInformation() { Name = prop.Name, Type = prop.PropertyType.ToString() });
                        }
                    }
                    MethodInfo[] methods = type.GetMethods();
                    foreach (MethodInfo method in methods)
                    {
                        object[] attrs = method.GetCustomAttributes(typeof(SensorActionAttribute), true);
                        if ((attrs != null) && (attrs.Length > 0))
                        {
                            MethodInformation nextmethod = new MethodInformation() { Name = method.Name };
                            ParameterInfo[] parameters = method.GetParameters();
                            List<PropertyInformation> ourparameters = new List<PropertyInformation>();
                            foreach (ParameterInfo nextparam in parameters)
                            {
                                ourparameters.Add(new PropertyInformation() { Name = nextparam.Name, Type = nextparam.ParameterType.ToString() });
                            }
                            nextmethod.Parameters = ourparameters.ToArray();

                            ourmethods.Add(nextmethod);
                        }
                    }
                    EventInfo[] events = type.GetEvents();
                    foreach (EventInfo nextevent in events)
                    {
                        object[] attrs = nextevent.GetCustomAttributes(typeof(SensorEventAttribute), true);
                        if ((attrs != null) && (attrs.Length > 0))
                        {
                            MethodInfo raisemethod = nextevent.GetAddMethod();
                            MethodInformation nextmethod = new MethodInformation() { Name = nextevent.Name };
                            ParameterInfo[] parameters = raisemethod.GetParameters();
                            List<PropertyInformation> ourparameters = new List<PropertyInformation>();
                            foreach (ParameterInfo nextparam in parameters)
                            {
                                ourparameters.Add(new PropertyInformation() { Name = nextparam.Name, Type = nextparam.ParameterType.ToString() });
                            }
                            nextmethod.Parameters = ourparameters.ToArray();

                            ourevents.Add(nextmethod);

                        }
                    }


                    result.ClassInfo.Methods = ourmethods.ToArray();
                    result.ClassInfo.Properties = ourproperties.ToArray();
                    result.ClassInfo.Events = ourevents.ToArray();
                    try
                    {
                        this.SendObject(result);
                    }
                    catch (Exception ex)
                    { }
                    return true;
                }
                else if (sensoriq.Action.Verb == Verb.get) /// get a property
                {
                    PropertyInfo[] props = type.GetProperties();
                    foreach (PropertyInfo prop in props)
                    {
                        object[] attrs = prop.GetCustomAttributes(typeof(SensorPropertyAttribute), true);
                        if ((attrs != null) && (attrs.Length > 0))
                        {
                            if (string.Compare(prop.Name, sensoriq.Action.MethodOrMember, true) == 0)
                            {

                                object objValue = null;
                                if ((sensoriq.Action.Parameters != null) && (sensoriq.Action.Parameters.Length >= 1))
                                    objValue = ConvertToType(prop.PropertyType, sensoriq.Action.Parameters[0].Value);

                                object objRet = prop.GetValue(this, null);
                                if (objRet != null)
                                    result.Result.Parameters = new Parameter[] { new Parameter() { Name = prop.Name, Value = objRet.ToString() } };


                                try
                                {
                                    this.SendObject(result);
                                }
                                catch (Exception ex)
                                { }

                                return true;
                            }
                        }
                    }
                }
                else if (sensoriq.Action.Verb == Verb.set) /// set a property
                {
                    PropertyInfo[] props = type.GetProperties();
                    foreach (PropertyInfo prop in props)
                    {
                        object[] attrs = prop.GetCustomAttributes(typeof(SensorPropertyAttribute), true);
                        if ((attrs != null) && (attrs.Length > 0))
                        {
                            if (string.Compare(prop.Name, sensoriq.Action.MethodOrMember, true) == 0)
                            {

                                object objValue = null;
                                if ((sensoriq.Action.Parameters != null) && (sensoriq.Action.Parameters.Length >= 1))
                                    objValue = ConvertToType(prop.PropertyType, sensoriq.Action.Parameters[0].Value);

                                prop.SetValue(this, objValue, null);

                                try
                                {
                                    this.SendObject(result);
                                }
                                catch (Exception ex)
                                { }

                                return true;
                            }
                        }
                    }
                }
                else if (sensoriq.Action.Verb == Verb.call) /// call a method
                {
                    MethodInfo[] methods = type.GetMethods();
                    foreach (MethodInfo method in methods)
                    {
                        object[] attrs = method.GetCustomAttributes(typeof(SensorActionAttribute), true);
                        if ((attrs != null) && (attrs.Length > 0))
                        {
                            if (string.Compare(method.Name, sensoriq.Action.MethodOrMember, true) == 0)
                            {
                                ParameterInfo[] parameters = method.GetParameters();
                                if ((parameters == null) || (parameters.Length <= 0))
                                {
                                    object objRet = method.Invoke(this, null);

                                }
                                else
                                {
                                    List<object> objParams = new List<object>();

                                    /// Add all the parameters they've passed in the order they passed.. hopefully the number lines up
                                    if ((sensoriq.Action.Parameters[0].Name == null) || (sensoriq.Action.Parameters[0].Name.Length <= 0)
                                        && (sensoriq.Action.Parameters.Length == parameters.Length))
                                    {
                                        for (int i = 0; i < parameters.Length; i++)
                                        {
                                            ParameterInfo param = parameters[i];
                                            Parameter inparam = sensoriq.Action.Parameters[i];

                                            objParams.Add(ConvertToType(param.ParameterType, inparam.Value));
                                        }
                                    }
                                    else
                                    {

                                        foreach (ParameterInfo param in parameters)
                                        {

                                            bool bFound = false;
                                            foreach (Parameter inparam in sensoriq.Action.Parameters)
                                            {
                                                if (string.Compare(inparam.Name, param.Name, true) == 0)
                                                {
                                                    bFound = true;
                                                    objParams.Add(ConvertToType(param.ParameterType, inparam.Value));
                                                    break;
                                                }
                                            }
                                            if (bFound == false)
                                                objParams.Add(null);
                                        }
                                    }

                                    object objRet = method.Invoke(this, objParams.ToArray());
                                    if (objRet != null)
                                        result.Result.Parameters = new Parameter[] { new Parameter() { Name = "return", Value = objRet.ToString() } };

                                    try
                                    {
                                        this.SendObject(result);
                                    }
                                    catch (Exception ex)
                                    { }


                                    return true;
                                }
                                break;
                            }
                        }
                    }

                }

                result.Type = IQType.result.ToString();
                result.Result.Error = "Method not found";

                try
                {
                    this.SendObject(result);
                }
                catch (Exception ex)
                { }



                return true;
            }
            return base.OnIQ(iq);
        }

        [SensorActionAttribute]
        protected virtual string Query()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            Type type = GetType();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                /// Only report properties that have the sensorproperty attribute
                object[] attrs = prop.GetCustomAttributes(typeof(SensorPropertyAttribute), true);
                if ((attrs != null) && (attrs.Length > 0))
                {

                    try
                    {
                        if (prop.CanRead == true)
                        {
                            object objValue = prop.GetValue(this, null);
                            sb.AppendFormat("{0}: {1}\r\n", prop.Name, objValue);
                        }
                        else if (prop.CanWrite == true)
                        {
                            sb.AppendFormat("{0}.set()\r\n", prop.Name);
                        }
                    }
                    catch (Exception ex) /// may not have a get accessor
                    {
                    }
                }
            }
            MethodInfo [] methods = type.GetMethods();
            foreach (MethodInfo method in methods)
            {
                object[] attrs = method.GetCustomAttributes(typeof(SensorActionAttribute), true);
                if ((attrs != null) && (attrs.Length > 0))
                {

                    try
                    {
                        sb.AppendFormat("{0}()\r\n", method.Name);
                    }
                    catch (Exception ex) 
                    {
                    }
                }
            }

            return sb.ToString();
        }

        public static object ConvertToType(Type type, string strValue)
        {
            object objValue = strValue;
            try
            {
                if (type == typeof(byte))
                    objValue = Convert.ToByte(strValue);
                else if (type == typeof(sbyte))
                    objValue = Convert.ToSByte(strValue);
                else if (type == typeof(short))
                    objValue = Convert.ToInt16(strValue);
                else if (type == typeof(ushort))
                    objValue = Convert.ToUInt16(strValue);
                else if (type == typeof(int))
                    objValue = Convert.ToInt32(strValue);
                else if (type == typeof(uint))
                    objValue = Convert.ToUInt32(strValue);
                else if (type == typeof(long))
                    objValue = Convert.ToInt64(strValue);
                else if (type == typeof(ulong))
                    objValue = Convert.ToUInt64(strValue);
                else if (type == typeof(float))
                    objValue = Convert.ToSingle(strValue);
                else if (type == typeof(double))
                    objValue = Convert.ToDouble(strValue);
                else if (type == typeof(decimal))
                    objValue = Convert.ToDecimal(strValue);
                else if (type == typeof(DateTime))
                    objValue = Convert.ToDateTime(strValue);
                else if (type == typeof(bool))
                    objValue = Convert.ToBoolean(strValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception converting {0} to {1}: {2}", strValue, type, ex));
            }
            return objValue;
        }

        protected void Set(string strProperty, string strValue)
        {
            Console.WriteLine(string.Format("Attempting to set {0} to {1}", strProperty, strValue));
            Type type = GetType();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (string.Compare(prop.Name, strProperty, true) == 0)
                {
                    object[] attrs = prop.GetCustomAttributes(typeof(SensorPropertyAttribute), true);
                    if ((attrs == null) || (attrs.Length <= 0))
                        continue;
                    try
                    {
                        object objValue = ConvertToType(prop.PropertyType, strValue);
                        prop.SetValue(this, objValue, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("Exception setting {0} to {1}: {2}", strProperty, strValue, ex));
                    }
                    break;
                }
            }
        }
       

    }
}
