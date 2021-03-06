﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using AudioClasses;
using System.Runtime.Serialization;
using ImageAquisition;
using WPFImageWindows;

namespace USBMotionJpegServer
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }


        RTP.MotionJpegHttpServer Server = null;
        CameraConfig[] Cameras = null;
        List<VideoCaptureSource> ActiveDevices = new List<VideoCaptureSource>();
        public void StartInteractive()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            string strDirectory = System.IO.Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]);

            string strConfig = string.Format(@"{0}\{1}", strDirectory, Properties.Settings.Default.USBCameraConfigFile);
            Cameras = CameraConfig.LoadCameras(strConfig);
            if (Cameras == null)
            {
                CameraConfig.CreateDefault(strConfig);
                System.Environment.Exit(-2);
            }
            // Save it back out incase we have any new variables
            CameraConfig.SaveCameras(strConfig, Cameras);


            Server = new RTP.MotionJpegHttpServer();
            Server.Port = Properties.Settings.Default.Port;

            if (string.Compare(RTP.AuthenticationMethod.Windows.ToString(), Properties.Settings.Default.AuthenticationMethod, true) == 0)
            {
                Server.AuthenticationMethod = RTP.AuthenticationMethod.Windows;
            }
            if (string.Compare(RTP.AuthenticationMethod.Internal.ToString(), Properties.Settings.Default.AuthenticationMethod, true) == 0)
            {
                Server.AuthenticationMethod = RTP.AuthenticationMethod.None;
                Server.UserName = Properties.Settings.Default.InternalAuthenticationMethodUserName;
                Server.Password = Properties.Settings.Default.InternalAuthenticationMethodPassword;
            }
            else if (string.Compare(RTP.AuthenticationMethod.None.ToString(), Properties.Settings.Default.AuthenticationMethod, true) == 0)
            {
                Server.AuthenticationMethod = RTP.AuthenticationMethod.Windows;
            }

            Server.MaxConnections = Properties.Settings.Default.MaxHTTPConnections;

            MFVideoCaptureDevice [] Devices = MFVideoCaptureDevice.GetCaptureDevices();

            foreach (CameraConfig cam in Cameras)
            {
                VideoCaptureRate ActiveRate = null;
                /// Find the active video rate
                foreach (VideoCaptureRate rate in cam.VideoCaptureRates)
                {
                    if (rate.Active == true)
                    {
                        ActiveRate = rate;
                        break;
                    }
                }

                if (ActiveRate == null)
                {
                    System.Diagnostics.EventLog.WriteEntry("USBMotionJpegServer", string.Format("Could not find an active capture rate for camera {0}", cam.UniqueName), EventLogEntryType.Error);
                    continue;
                }

                if (System.IO.Directory.Exists(Properties.Settings.Default.RecordingDirectory) == false)
                    System.IO.Directory.CreateDirectory(Properties.Settings.Default.RecordingDirectory);

                string strMaskDir = string.Format(@"{0}\Masks", strDirectory);
                if (System.IO.Directory.Exists(strMaskDir) == false)
                    System.IO.Directory.CreateDirectory(strMaskDir);

                /// Find this device
                /// 
                bool bFound = false;
                foreach (MFVideoCaptureDevice dev in Devices)
                {
                    if (dev.UniqueName == cam.UniqueName)
                    {
                        dev.DisplayName = cam.Name;
                        VideoCaptureSource devwithcontrol = new VideoCaptureSource(dev);
                        if (cam.RecordMotion == true)
                        {

                            MotionDetection.ContourAreaMotionDetector detector = new MotionDetection.ContourAreaMotionDetector();
                            if ((cam.MaskFileName != null) && (cam.MaskFileName.Length > 0))
                            {
                                string strMaskName = string.Format(@"{0}\Masks\{1}", strDirectory, cam.MaskFileName);
                                if (System.IO.File.Exists(strMaskName) == true)
                                    detector.FileNameMotionMask = strMaskName;
                                else
                                    System.Diagnostics.EventLog.WriteEntry("USBMotionJpegServer", string.Format("Could not find mask file '{0}' for camera '{1}'", strMaskName, cam.UniqueName), EventLogEntryType.Error);
                            }
                            

                            if (cam.MaxFrameRateServed > 0)
                                devwithcontrol.DesiredFramesPerSecond = cam.MaxFrameRateServed;
                            if (cam.MaxFramesAnalyzed > 0)
                                detector.MaxAnalyzedFramesPerSecond = cam.MaxFramesAnalyzed;

                            detector.Active = cam.DoMotionAnalysis;

                            detector.ShowText = cam.ShowText;
                            detector.ContourAreaThreshold = cam.MinimumBlobSizeTriggerMotion;

                            devwithcontrol.RecorderWithMotion.PreviousFrames = cam.PreRecordFrames;
                            devwithcontrol.RecorderWithMotion.SubsequentFrames = cam.PostRecordFrames;
                            devwithcontrol.RecorderWithMotion.MaxEncodingQueueSize = cam.MaxEncodingQueueSize;
                            devwithcontrol.RecorderWithMotion.MotionDetector = detector;
                            devwithcontrol.RecorderWithMotion.ShowMotionImages = cam.ShowMotionImages;
                            
                            devwithcontrol.RecorderWithMotion.MotionDetector.Threshold = cam.MotionLevel;
                            devwithcontrol.RecorderWithMotion.IsRecordingMotion = cam.RecordMotion;

                        }


                        devwithcontrol.RecorderWithMotion.RecordingDirectory = Properties.Settings.Default.RecordingDirectory;
                        devwithcontrol.RecorderWithMotion.MaxRecordingFrames = Properties.Settings.Default.MaxRecordedFrames;
                        devwithcontrol.ShowMessageBoxes = false; /// no message boxes in a service, don't want a hang
                        ActiveDevices.Add(devwithcontrol);
                        devwithcontrol.ActiveVideoCaptureRate = ActiveRate;
                        dev.OnFailStartCapture += dev_OnFailStartCapture;
                        devwithcontrol.StartCapture();
                        InterFrameCompressor JpegCompressor = new InterFrameCompressor(ActiveRate) { QualityLevel = 90 };
                        Server.AddVideoSource(devwithcontrol, JpegCompressor);

                        if (Properties.Settings.Default.UseXMPPEvents == true)
                        {
                            XMPPCameraClient xmppclient = new XMPPCameraClient(devwithcontrol, cam.Name);
                            xmppclient.Connect(Properties.Settings.Default.XMPPDomain, Properties.Settings.Default.XMPPServer, Properties.Settings.Default.XMPPUser, Properties.Settings.Default.XMPPPassword);
                        }


                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                    System.Diagnostics.EventLog.WriteEntry("USBMotionJpegServer", string.Format("Could not find camera {0} in the list of active USB capture devices", cam.UniqueName), EventLogEntryType.Error);

            }

            Server.Start();
        }

        List<XMPPCameraClient> XMPPClients = new List<XMPPCameraClient>();

        void dev_OnFailStartCapture(string strError, object objDevice)
        {
            System.Diagnostics.EventLog.WriteEntry("USBMotionJpegServer", string.Format("Failed to start capture on {0}", ((MFVideoCaptureDevice)objDevice).Name), EventLogEntryType.Error);
        }

        protected override void OnStop()
        {
            foreach (VideoCaptureSource dev in ActiveDevices)
                dev.StopCapture();
            Server.Stop();
        }
    }
}
