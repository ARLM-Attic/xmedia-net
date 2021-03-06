﻿/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Runtime.Serialization;

using System.Runtime.InteropServices;
using System.ComponentModel;
using AudioClasses;
using System.IO;

namespace WPFImageWindows
{

    public partial class VideoCaptureSource : System.ComponentModel.INotifyPropertyChanged, IVideoSource, ICameraController, IVideoRecorder
    {
        public VideoCaptureSource(ImageAquisition.MFVideoCaptureDevice dev)
        {
            VideoCaptureDevice = dev;
            Name = VideoCaptureDevice.DisplayName;
            MonikerString = VideoCaptureDevice.UniqueName;


            RecorderWithMotion = new MutlitFormatMotionRecorder(dev, false);

            foreach (VideoCaptureRate format in VideoCaptureDevice.VideoFormats)
            {
                m_listRates.Add(format);
            }

        }

        public MutlitFormatMotionRecorder RecorderWithMotion = null;

        static Random Rand = new Random();
        ImageAquisition.MFVideoCaptureDevice m_objVideoCaptureDevice = null;
        public ImageAquisition.MFVideoCaptureDevice VideoCaptureDevice
        {
            get { return m_objVideoCaptureDevice; }
            set { m_objVideoCaptureDevice = value; }
        }


        public static VideoCaptureSource[] GetVideoCaptureDevices()
        {
            List<VideoCaptureSource> caps = new List<VideoCaptureSource>();

            ImageAquisition.MFVideoCaptureDevice[] capdev = ImageAquisition.MFVideoCaptureDevice.GetCaptureDevices();
            foreach (ImageAquisition.MFVideoCaptureDevice dev in capdev)
            {
                VideoCaptureSource cam = new VideoCaptureSource(dev);
                caps.Add(cam);
            }
            return caps.ToArray();
        }

        /// <summary>
        /// A list of capture rates supported by this capture device.  ActiveVideoCaptureRate should be set to this value
        /// before starting capture.  
        /// </summary>
        List<VideoCaptureRate> m_listRates = new List<VideoCaptureRate>();
        public List<VideoCaptureRate> SupportedRates
        {
            get 
            {
                return m_listRates;
            }
            set
            {
            }

        }

        public VideoCaptureRate[] GetSupportedCaptureRates()
        {
            return m_listRates.ToArray();
        }

        private VideoCaptureRate m_objActiveVideoCaptureRate = null;

     

        /// <summary>
        /// The capture rate we will capture at if a start is issued - can only be changed when the capture is inactive
        /// </summary>
        /// 
        public VideoCaptureRate ActiveVideoCaptureRate
        {
            get { return m_objActiveVideoCaptureRate; }
            set 
            {
                if (m_bCapturing == true)
                {
                    CameraActive = false;
                    m_objActiveVideoCaptureRate = value;
                }
                else
                {
                    m_objActiveVideoCaptureRate = value;
                }
            }
        }

        private string m_strMonikerString;

        /// <summary>
        /// The monikerstring of this video capture device - guid
        /// </summary>
        /// 
        public string MonikerString
        {
            get { return m_strMonikerString; }
            set { m_strMonikerString = value; }
        }
        
        private string m_strName;

        /// <summary>
        /// The name of the video capture device used by this instance
        /// </summary>
        /// 
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        public override string ToString()
        {
            return Name;
        }

        public bool CameraActive
        {
            get
            {
                return m_bCapturing;
            }
            set
            {
                if (value == true)
                    StartCapture();
                else
                    StopCapture();
                FirePropertyChanged("CameraActive");
            }
        }

        protected bool m_bCapturing = false;
        
        public virtual bool StartCapture()
        {
            if (m_bCapturing == true)
                return false;


            this.m_nNumberFramesCaptures = 0;
            m_nNumberFramesProcessed = 0;
            StartTime = DateTime.Now;

            VideoCaptureDevice.OnNewFrame += new AudioClasses.DelegateRawFrame(VideoCaptureDevice_OnNewFrame);
            VideoCaptureDevice.OnFailStartCapture += new ImageAquisition.MFVideoCaptureDevice.DelegateError(VideoCaptureDevice_OnFailStartCapture);

            VideoCaptureDevice.Start(ActiveVideoCaptureRate);

            m_bCapturing = true;
            return true;
        }

        private bool m_bShowMessageBoxes = true;

        public bool ShowMessageBoxes
        {
            get { return m_bShowMessageBoxes; }
            set { m_bShowMessageBoxes = value; }
        }

        void VideoCaptureDevice_OnFailStartCapture(string strError, object objDevice)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
              (Action<string>)((prop) =>
              {
                  CameraActive = false;
                  if (ShowMessageBoxes == true)
                    System.Windows.MessageBox.Show(strError, "Capture failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
              }
               )
             , strError);
        }

      

        public bool StopCapture()
        {
            StopTime = DateTime.Now;
            if (m_bCapturing == false)
                return false;

            if (VideoCaptureDevice != null)
            {
                VideoCaptureDevice.OnNewFrame -= new AudioClasses.DelegateRawFrame(VideoCaptureDevice_OnNewFrame);
                VideoCaptureDevice.OnFailStartCapture -= new ImageAquisition.MFVideoCaptureDevice.DelegateError(VideoCaptureDevice_OnFailStartCapture);
                VideoCaptureDevice.Stop();
            }

            m_bCapturing = false;

            return true;
        }

        
        bool m_bNeedReturnPicture = false;
        System.Threading.ManualResetEvent PictureEvent = new System.Threading.ManualResetEvent(false);
        byte[] m_bPictureRGB = null;
        public byte [] GetPictureRGB()
        {
            PictureEvent.Reset();
            m_bPictureRGB = null;
            m_bNeedReturnPicture = true;
            PictureEvent.WaitOne(1000);
            return m_bPictureRGB;
        }

        int m_nNumberFramesCaptures = 0;
        int m_nNumberFramesProcessed = 0;
        DateTime StartTime = DateTime.MinValue;
        DateTime StopTime = DateTime.MaxValue;
        public int CurrentFrameRate
        {
            get
            {
                if (m_nNumberFramesCaptures <= 0)
                    return this.ActiveVideoCaptureRate.FrameRate;

                TimeSpan tsElapsed = StartTime - StartTime;
                if (m_bCapturing == true)
                    tsElapsed = DateTime.Now - StartTime;

                return (int) (m_nNumberFramesCaptures / tsElapsed.TotalSeconds);
            }
        }

        private int m_nDesiredFramesPerSecond = 30;
        public int DesiredFramesPerSecond
        {
            get { return m_nDesiredFramesPerSecond; }
            set
            {
                m_nDesiredFramesPerSecond = value;
                m_nNumberFramesCaptures = 0;
                m_nNumberFramesProcessed = 0;
                StartTime = DateTime.Now;
            }
        }

        public int ActualFramesProcessedPerSecond
        {
            get 
            {
                if (m_nNumberFramesProcessed <= 0)
                    return 0;

                TimeSpan tsElapsed = StartTime - StartTime;
                if (m_bCapturing == true)
                    tsElapsed = DateTime.Now - StartTime;

                return (int)(m_nNumberFramesProcessed / tsElapsed.TotalSeconds);
            }
        }


        object CurrentFrameLock = new object();
        MediaSample CurrentFrame = null;

        public string StatusString
        {
            get
            {
                if ((RecorderWithMotion != null) && (RecorderWithMotion.MotionDetector != null))
                    return RecorderWithMotion.MotionDetector.StatusString;
                return "";
            }
            internal set
            {
                FirePropertyChanged("StatusString");
            }
        }

        void VideoCaptureDevice_OnNewFrame(byte[] pFrame, VideoCaptureRate videoformat, object objSource)
        {

            m_nNumberFramesCaptures++;

            bool bIgnoreFrame = (ActualFramesProcessedPerSecond > DesiredFramesPerSecond); // User wants to analyze less than what the camera is providing


            lock (CurrentFrameLock)
            {
                if (CurrentFrame != null)
                {
                    CurrentFrame.Data = null;
                    CurrentFrame = null;
                }
                    
                CurrentFrame = new MediaSample(pFrame, videoformat);
            }

            if (bIgnoreFrame == true)
                return;

            m_nNumberFramesProcessed++;


            pFrame = RecorderWithMotion.SetNewFrame(pFrame, videoformat, objSource);
            Dispatcher.CurrentDispatcher.Invoke(new Action(() => { StatusString = ""; }));


            if (m_bNeedReturnPicture == true)
            {
                m_bPictureRGB = (byte [] ) pFrame.Clone();
                PictureEvent.Set();
                m_bNeedReturnPicture = false;
            }


            if (OnNewFrame != null)
                OnNewFrame(pFrame, videoformat, this);

           
        }


              /// <summary>
        /// Pans the camera left -5 units, if supported
        /// </summary>
        public virtual void PanLeft()
        {
            PanRelative(-5);
        }

        /// <summary>
        /// Pans the camera right -5 units, if supported
        /// </summary>
        public virtual void PanRight()
        {
            PanRelative(5);
        }

        /// <summary>
        /// Pans the camera by the specified number of units.  Negative is to the left, Positive to the right
        /// </summary>
        /// <param name="nUnits"></param>
        public void PanRelative(int nUnits)
        {
            if (VideoCaptureDevice.SourceDevice == IntPtr.Zero)
                return;
            object Unknown = Marshal.GetObjectForIUnknown(VideoCaptureDevice.SourceDevice);
            IAMCameraControl control = Unknown as IAMCameraControl;
            if (control != null)
            {
                control.Set(CameraControlProperty.Pan, nUnits, CameraControlFlags.Manual | CameraControlFlags.Relative);
                Marshal.ReleaseComObject(control);
            }
        }

        public virtual void TiltUp()
        {
            TiltRelative(5);
        }

        public virtual void TiltDown()
        {
            TiltRelative(-5);
        }

        /// <summary>
        /// Tilts the camera up and down relative to the current position
        /// </summary>
        /// <param name="nUnits"></param>
        public virtual void TiltRelative(int nUnits)
        {
            if (VideoCaptureDevice.SourceDevice == IntPtr.Zero)
                return;
            object Unknown = Marshal.GetObjectForIUnknown(VideoCaptureDevice.SourceDevice);
            IAMCameraControl control = Unknown as IAMCameraControl;
            if (control != null)
            {
                control.Set(CameraControlProperty.Tilt, nUnits, CameraControlFlags.Manual | CameraControlFlags.Relative);
                Marshal.ReleaseComObject(control);
            }

        }

        public void GetFocusRange(out int nMin, out int nMax, out int nStep, out int nDefault)
        {
            nMin = 0;
            nMax = 0;
            nStep = 0;
            nDefault = 0;
            if (VideoCaptureDevice.SourceDevice == IntPtr.Zero)
                return;
            object Unknown = Marshal.GetObjectForIUnknown(VideoCaptureDevice.SourceDevice);
            IAMCameraControl control = Unknown as IAMCameraControl;
            if (control != null)
            {
                CameraControlFlags nFlags;
                control.GetRange(CameraControlProperty.Focus, out nMin, out nMax, out nStep, out nDefault, out nFlags);
                Marshal.ReleaseComObject(control);
            }
        }

        public virtual void SetFocus(int nFocus)
        {
            if (VideoCaptureDevice.SourceDevice == IntPtr.Zero)
                return;
            object Unknown = Marshal.GetObjectForIUnknown(VideoCaptureDevice.SourceDevice);
            IAMCameraControl control = Unknown as IAMCameraControl;
            if (control != null)
            {
                control.Set(CameraControlProperty.Focus, nFocus, CameraControlFlags.Manual);
                Marshal.ReleaseComObject(control);
            }
        }

        public void GetExposureRange(out int nMin, out int nMax, out int nStep, out int nDefault)
        {
            nMin = 0;
            nMax = 0;
            nStep = 0;
            nDefault = 0;
            if (VideoCaptureDevice.SourceDevice == IntPtr.Zero)
                return;
            object Unknown = Marshal.GetObjectForIUnknown(VideoCaptureDevice.SourceDevice);
            IAMCameraControl control = Unknown as IAMCameraControl;
            if (control != null)
            {
                CameraControlFlags nFlags;
                control.GetRange(CameraControlProperty.Exposure, out nMin, out nMax, out nStep, out nDefault, out nFlags);
                Marshal.ReleaseComObject(control);
            }
        }

        public void SetExposure(int nExposure)
        {
            if (VideoCaptureDevice.SourceDevice == IntPtr.Zero)
                return;
            object Unknown = Marshal.GetObjectForIUnknown(VideoCaptureDevice.SourceDevice);
            IAMCameraControl control = Unknown as IAMCameraControl;
            if (control != null)
            {
                control.Set(CameraControlProperty.Exposure, nExposure, CameraControlFlags.Manual);
                Marshal.ReleaseComObject(control);
            }
        }

        public void GetIrisRange(out int nMin, out int nMax, out int nStep, out int nDefault)
        {
            nMin = 0;
            nMax = 0;
            nStep = 0;
            nDefault = 0;
            if (VideoCaptureDevice.SourceDevice == IntPtr.Zero)
                return;
            object Unknown = Marshal.GetObjectForIUnknown(VideoCaptureDevice.SourceDevice);
            IAMCameraControl control = Unknown as IAMCameraControl;
            if (control != null)
            {
                CameraControlFlags nFlags;
                control.GetRange(CameraControlProperty.Iris, out nMin, out nMax, out nStep, out nDefault, out nFlags);
                Marshal.ReleaseComObject(control);
            }
        }

        public void SetIris(int nIris)
        {
            if (VideoCaptureDevice.SourceDevice == IntPtr.Zero)
                return;
            object Unknown = Marshal.GetObjectForIUnknown(VideoCaptureDevice.SourceDevice);
            IAMCameraControl control = Unknown as IAMCameraControl;
            if (control != null)
            {
                control.Set(CameraControlProperty.Iris, nIris, CameraControlFlags.Manual);
                Marshal.ReleaseComObject(control);
            }
        }
        /// <summary>
        /// Zooms the camera in... still need to get this working
        /// </summary>
        /// <param name="nFactor"></param>
        public virtual void Zoom(int nFactor)
        {
            if (VideoCaptureDevice.SourceDevice == IntPtr.Zero)
                return;
            object Unknown = Marshal.GetObjectForIUnknown(VideoCaptureDevice.SourceDevice);
            IAMCameraControl control = Unknown as IAMCameraControl;
            if (control != null)
            {
                control.Set(CameraControlProperty.Zoom, nFactor, CameraControlFlags.Manual);
                Marshal.ReleaseComObject(control);
            }
            
        }

        public void TurnOffLED()
        {
            if (VideoCaptureDevice.SourceDevice == IntPtr.Zero)
                return;
            object Unknown = Marshal.GetObjectForIUnknown(VideoCaptureDevice.SourceDevice);
            IKsPropertySet propset = Unknown as IKsPropertySet;

            if (propset != null)
            {
                KSPROPERTY_LP1_HEADER ledinfo;
                ledinfo.ulReserved1 = 0;
                ledinfo.ulReserved2 = 0;
                ledinfo.ulFlags = (uint)LVUVC_LP1_LED_MODE.LVUVC_LP1_LED_MODE_ON;

                IntPtr ptrstruct = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(24);
                System.Runtime.InteropServices.Marshal.StructureToPtr(ledinfo, ptrstruct, true);

                propset.Set(LogitechGuids.LogitechPropSetGuid, (int)LogitechPropertySet.KSPROPERTY_LP1_LED, IntPtr.Zero, 0, ptrstruct, 24);
                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptrstruct);

                Marshal.ReleaseComObject(propset);
            }
        }



        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strProp)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(strProp));
        }

        #endregion

      
        

        public void SetCaptureInformation(VideoCaptureRate captureinfo)
        {
            this.ActiveVideoCaptureRate = captureinfo;
        }


        #region IVideoSource Members


        public List<VideoCaptureRate> VideoFormats
        {
            get 
            {
                return VideoCaptureDevice.VideoFormats;
            }
        }

        public event DelegateRawFrame OnNewFrame = null;

        public MediaSample PullFrame()
        {
            lock (CurrentFrameLock)
            {
                MediaSample sample = CurrentFrame;
                CurrentFrame = null; /// null this out so if they ask again on the same frame they get nutin
                return sample;
            }
        }
      
        #endregion

        public bool StartRecording()
        {
            return RecorderWithMotion.StartRecording();
        }

        public string StopRecording()
        {
            return RecorderWithMotion.StopRecording();
        }

        public bool Recording
        {
            get
            {
                return RecorderWithMotion.Recording;
            }
            set
            {
                RecorderWithMotion.Recording = value;
                FirePropertyChanged("Recording");
            }
        }
    }


   
}
