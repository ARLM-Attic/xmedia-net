
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

using System.ComponentModel;
using AudioClasses;


namespace WPFImageWindows
{
    /// <summary>
    /// Shows video from a USBSecurityCamera
    /// </summary>
    public partial class VideoControl : UserControl, System.ComponentModel.INotifyPropertyChanged
    {
        public VideoControl()
        {
            InitializeComponent();
            WriteableBitmap videobmp = new WriteableBitmap(VideoWidth, VideoHeight, 96, 96, PixelFormats.Bgr32, null);
            this.videoimage.Source = videobmp;

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(VideoControl_DataContextChanged);
        }


        void VideoControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            NewImage(null, 0, 0);
            Camera = this.DataContext as IVideoSource;
        }


        private ICameraController m_objController = null;

        public ICameraController Controller
        {
            get { return m_objController; }
            set { m_objController = value; }
        }


        private IVideoSource m_objCamera = null;


        public IVideoSource Camera
        {
            get { return m_objCamera; }
            set 
            {
                if (m_objCamera is ICameraController)
                    Controller = m_objCamera as ICameraController;

                if (m_objCamera != null)
                {
                    m_objCamera.OnNewFrame -= new DelegateRawFrame(m_objCamera_NewRawFrame);
                }

                if (value != null)
                {
                    m_objCamera = value;
                    m_objCamera.OnNewFrame += new DelegateRawFrame(m_objCamera_NewRawFrame);
                }
            }
        }


        public void SetSize()
        {
            if (m_objCamera != null)
            {
                SetSizeAndScale(m_objCamera.ActiveVideoCaptureRate.Width, m_objCamera.ActiveVideoCaptureRate.Height);
            }
        }

        public void SetSizeAndScale(int nWidth, int nHeight)
        {
            m_fScale = 1.0f;
            SetSize(nWidth, nHeight);
        }

        public void SetSize(int nWidth, int nHeight)
        {
           VideoWidth = nWidth;
           VideoHeight = nHeight;
           videobmp = new WriteableBitmap(VideoWidth, VideoHeight, 96, 96, PixelFormats.Bgr32, null);
           this.videoimage.Source = videobmp;

           if (PreventImageResizing == true)
               SetImageToActualSize();
        }


        public delegate void DelegateVoid();
      

        double m_fScale = 1.0f;
        private int m_nVideoWidth = 352;

        public int VideoWidth
        {
            get { return m_nVideoWidth; }
            protected set { m_nVideoWidth = value; }
        }

        private int m_nVideoHeight = 240;

        public int VideoHeight
        {
            get { return m_nVideoHeight; }
            protected set { m_nVideoHeight = value; }
        }

        WriteableBitmap videobmp = new WriteableBitmap(352, 240, 96, 96, PixelFormats.Bgr32, null);

        private bool m_bShowOnlyCleanFrame = true;

        public bool ShowOnlyCleanFrame
        {
            get { return m_bShowOnlyCleanFrame; }
            set 
            {
                m_bShowOnlyCleanFrame = value; 
            }
        }

        //void m_objCamera_NewFrame(System.Drawing.Bitmap Frame)
        //{
        //    if (m_bDisplay == false)
        //        return;
        //    if (ShowOnlyCleanFrame == true)
        //        return;

        //    if (Frame == null)
        //        return;

        //    /// Get BGR24 from our bitmap
        //   // byte[] bRGBFrame = H264.H264Compressor.CopyImageBits(Frame, Frame.Width, Frame.Height, Frame.Width * Frame.Height * 4);

        //    try
        //    {
        //        byte[] bRGBFrame = new byte[Frame.Width * Frame.Height * 4];
        //        System.Drawing.Imaging.BitmapData data = Frame.LockBits(new System.Drawing.Rectangle(0, 0, Frame.Width, Frame.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
        //        if (data == null)
        //            return;
        //        System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bRGBFrame, 0, data.Stride * Frame.Height);
        //        Frame.UnlockBits(data);

        //        NewImage(bRGBFrame);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.Write(ex.ToString());
        //    }
        //}

     

        void m_objCamera_NewRawFrame(byte[] bRawData, VideoCaptureRate format, object objSource)
        {
            if (m_bDisplay == false)
                return;
            if (ShowOnlyCleanFrame == false)
                return;

            NewImage(bRawData, format.Width, format.Height);
        }

        /// <summary>
        /// Updates the video with a new image.
        /// </summary>
        /// <param name="Bytes">The new image - must be the full width and height</param>
        public void NewImage(byte[] Bytes)
        {
            if (m_bDisplay == false)
                return;
            NewImage(Bytes, m_objCamera.ActiveVideoCaptureRate.Width, m_objCamera.ActiveVideoCaptureRate.Height);
            //if (nFrameCount == 30)
              //  this.Dispatcher.Invoke(new DelegateDisplayBitmap(DisplayBitmap), System.Windows.Threading.DispatcherPriority.Render, Bytes, m_objCamera.ActiveVideoCaptureRate.Width, m_objCamera.ActiveVideoCaptureRate.Height);
            //else
                //DisplayBitmap(Bytes, m_objCamera.ActiveVideoCaptureRate.Width, m_objCamera.ActiveVideoCaptureRate.Height);
         
        }

        /// <summary>
        /// Updates the video or a portion of the video with a new image
        /// </summary>
        /// <param name="Bytes">video data</param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        public void NewImage(byte [] Bytes, int nWidth, int nHeight)
        {
            if (m_bDisplay == false)
                return;

            if (Bytes == null)
            {
                /// Black out the screen
                /// 
                lock (CurrentDataLock)
                {
                    if (CurrentBytes != null)
                        Array.Clear(CurrentBytes, 0, CurrentBytes.Length);
                    m_bNewData = true;
                }
                return; 
            }

            //DisplayBitmap(Bytes, nWidth, nHeight);
            //this.Dispatcher.Invoke(new DelegateDisplayBitmap(DisplayBitmap), System.Windows.Threading.DispatcherPriority.Render, Bytes, nWidth, nHeight);

            lock (CurrentDataLock)
            {
                CurrentBytes = Bytes;
                CurrentWidth = nWidth;
                CurrentHeight = nHeight;
                m_bNewData = true;
            }
        }

        public void NewImageSegment(byte[] bRGBFrame, int nXAt, int nYAt, int nWidth, int nHeight)
        {
            lock (CurrentDataLock)
            {
                if (CurrentBytes == null)
                    return;

                ImageUtils.ImageWithPosition imagedest = new ImageUtils.ImageWithPosition(CurrentWidth, CurrentHeight, CurrentBytes);
                ImageUtils.ImageWithPosition imagesource = new ImageUtils.ImageWithPosition(nWidth, nHeight, bRGBFrame);
                imagesource.X = nXAt;
                imagesource.Y = nYAt;
                ImageUtils.Utils.BitBlt32(imagesource, imagedest);

                CheckRecord(CurrentBytes, CurrentWidth, CurrentHeight);
                //CurrentBytes = bRGBFrame;
                m_bNewData = true;
            }
        }

        ImageAquisition.MFVideoEncoder Recorder = null;
        AudioClasses.VideoCaptureRate rate = null;
        public bool Record
        {
            get
            {
                if (Recorder == null)
                    return false;
                return true;
            }
            set
            {
                if (Recorder != null)
                {
                    Recorder.Stop();
                    Recorder = null;
                }

                if (value == true)
                {
                    ImageAquisition.MFVideoEncoder tempRecorder = new ImageAquisition.MFVideoEncoder();
                    string strFileName = string.Format("{0}/{1}.wmv", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyVideos), Guid.NewGuid().ToString());
                    rate = new VideoCaptureRate(this.VideoWidth, this.VideoHeight, 5, 3000000);
                    rate.CompressedFormat = AudioClasses.VideoDataFormat.WMV9;
                    tempRecorder.Start(strFileName, rate, DateTime.Now, false);
                    Recorder = tempRecorder;
                }
                else
                {
                    if (Recorder != null)
                    {
                        Recorder.Stop();
                        Recorder = null;
                    }
                }
            }
        }


        void CheckRecord(byte[] bRGBData, int nWidth, int nHeight)
        {
            if (bRGBData == null)
                return;
            if (nWidth <= 0)
                return;
            if (nHeight <= 0)
                return;

            if (Recorder != null)
            {
                if ((nWidth == rate.Width) && (nHeight == rate.Height))
                {
                    byte[] bRGB32Data = ImageUtils.Utils.Convert24BitImageTo32BitImage(bRGBData, nWidth, nHeight);
                    Recorder.AddVideoFrame(bRGB32Data, DateTime.Now);
                }
                else
                    Record = false;
            }
        }

     

        object CurrentDataLock = new object();
        byte [] CurrentBytes = null;
        int CurrentWidth = 10;
        int CurrentHeight = 10;
        bool m_bNewData = true;

        delegate void DelegateDisplayBitmap(byte[] bRGBFrame, int nWidth, int nHeight);

        int nFrameCount = 0;
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        private bool m_bDisplay = true;

        public bool Display
        {
            get { return m_bDisplay; }
            set 
            { 
                m_bDisplay = value; 
                if (PropertyChanged != null)
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("Display"));
            }
        }


        void CompositionTarget_Rendering(object sender, EventArgs e)
        {

            if ((m_bDisplay == false) || (m_bNewData == false))
                return;

            if (nFrameCount == 0)
            {
                watch.Start();

            }

            bool bNewFrameDrawn = false;

            int nWidth = 0;
            int nHeight = 0;
            byte[] bBytes = null;
            lock (CurrentDataLock)  /// store current data
            {
                nWidth = CurrentWidth;
                nHeight = CurrentHeight;
                bBytes = CurrentBytes;
                bNewFrameDrawn = m_bNewData;
                m_bNewData = false;
            }

            if (bNewFrameDrawn == true)
            {
                if ((VideoWidth != nWidth) || (VideoHeight != nHeight))
                    SetSize(nWidth, nHeight);

                if (bBytes != null) 
                {
                    if (bBytes.Length == nWidth * 4 * nHeight)
                    {

                        videobmp.Lock();
                        Int32Rect rect = new Int32Rect(0, 0, nWidth, nHeight);
                        videobmp.WritePixels(rect, bBytes, nWidth * 4, 0);

                        videobmp.AddDirtyRect(rect);
                        videobmp.Unlock();
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine("Incoming frame has the wrong resolution");
                    }
                }

            }

       
            //if (Camera != null)
            //this.labelStatus.Content = string.Format("Motion Level: {0}", this.Camera.CurrentMotionValue);

            if (nFrameCount == 30)
            {
                long nElapseMs = watch.ElapsedMilliseconds;
                double fFramesPersecond = 30* 1000.0f / watch.ElapsedMilliseconds;
                //this.labelStatus.Content = fFramesPersecond.ToString();
                watch.Restart();
                nFrameCount = 0;
            }
            if (bNewFrameDrawn == true)
                nFrameCount++;

            
        }


        public static readonly DependencyProperty PreventImageResizingProperty = DependencyProperty.Register("PreventImageResizing",
            typeof(bool), typeof(VideoControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, null));
        
        public bool PreventImageResizing
        {
            get { return (bool) GetValue(PreventImageResizingProperty); }
            set
            {
                SetValue(PreventImageResizingProperty, value);
                AllowPanning = !value;
                if (value == true)
                {
                    m_bSafePreventImageResizing = true;
                    Dispatcher.Invoke(new DelegateVoid(SetImageToActualSize), null);
                }
                else
                {
                    m_bSafePreventImageResizing = false;
                    Dispatcher.Invoke(new DelegateVoid(DoSizeChange), null);
                }

                FirePropertyChanged("PreventImageResizing");
            }
        }

        bool m_bSafePreventImageResizing = false;

        public bool SafePreventImageResizing
        {
            get { return m_bSafePreventImageResizing; }
        }

        delegate void DelegateDoSizeChange(Size NewSize);

        /// <summary>
        /// Makes the image its actual size, so the scroll viewer may have scroll bars visible to pan the image
        /// </summary>
        public void SetImageToActualSize()
        {
            this.videoimage.Width = this.Width;
            this.videoimage.Height = this.Height;

            m_fScale = 1.0f;
        }

        void ScrollViewerThing_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DoSizeChange(e.NewSize);
        }

        void DoSizeChange()
        {
            DoSizeChange(new Size(this.ScrollViewerThing.ViewportWidth, this.ScrollViewerThing.ViewportHeight));
        }

        void DoSizeChange(Size NewSize)
        {
            if (PreventImageResizing == false)
            {
                this.videoimage.Width = NewSize.Width * m_fScale;
                this.videoimage.Height = NewSize.Height * m_fScale;


                CalculateScale();
            }
        }

        void CalculateScale()
        {
            /// Determine which way we are letter boxed before we determine our scale
            /// 
            double fWidthToHeightActualVideo = this.VideoWidth / this.VideoHeight;
            double fWidthToHeightScrollViewer = this.ScrollViewerThing.ViewportWidth / this.ScrollViewerThing.ViewportHeight;

            if (fWidthToHeightScrollViewer < fWidthToHeightActualVideo)
            {
                /// Our scroll viewer as a smaller width/height than our video, so we have bars on top and bottom.  
                /// Use the width to determine our scale factor
                /// 
                if ((this.ScrollViewerThing.ViewportWidth > 0) && (this.Width > 0))
                    m_fScale = this.ScrollViewerThing.ViewportWidth / this.Width;
            }
            else
            {
                if ((this.ScrollViewerThing.ViewportHeight > 0) && (this.Height > 0))
                    m_fScale = this.ScrollViewerThing.ViewportHeight / this.Height;
            }
        }

        System.Windows.Point scrollStartOffset;
        System.Windows.Point MouseStartPoint;
        bool bCaptured = true;


        private bool m_bAllowPanning = true;

        public bool AllowPanning
        {
            get { return m_bAllowPanning; }
            set { m_bAllowPanning = value; }
        }

        public event EventHandler OnOpenVideo = null;
        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (OnOpenVideo != null)
                OnOpenVideo(this, new EventArgs());
            base.OnPreviewMouseDoubleClick(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Control)
            {
                StartingPoint = e.GetPosition(this);
            }
            else if ((this.ScrollViewerThing.IsMouseOver == true) && (AllowPanning == true) )
            {
                // Save starting point, used later when determining     
                //how much to scroll.    
                MouseStartPoint = e.GetPosition(this);
                scrollStartOffset.X = this.ScrollViewerThing.HorizontalOffset;
                scrollStartOffset.Y = this.ScrollViewerThing.VerticalOffset;

                bCaptured = true;

                this.CaptureMouse();
                //bCaptured = Mouse.Capture(this);
            }
            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            
            if (System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Control)
            {

            }
            else if ((bCaptured == true) && (AllowPanning == true) )
            {
                // Get the new scroll position.   
                System.Windows.Point PointAtNow = e.GetPosition(this);

                // Determine the new amount to scroll.   

                System.Windows.Point delta = new System.Windows.Point((PointAtNow.X > this.MouseStartPoint.X) ? -(PointAtNow.X - this.MouseStartPoint.X) : (this.MouseStartPoint.X - PointAtNow.X),
                         (PointAtNow.Y > this.MouseStartPoint.Y) ? -(PointAtNow.Y - this.MouseStartPoint.Y) : (this.MouseStartPoint.Y - PointAtNow.Y));

                if (delta.X < 0)
                    System.Threading.Thread.Sleep(0);
                this.ScrollViewerThing.ScrollToHorizontalOffset(this.scrollStartOffset.X + delta.X);
                this.ScrollViewerThing.ScrollToVerticalOffset(this.scrollStartOffset.Y + delta.Y);
            }

            base.OnPreviewMouseMove(e);

        }

        
       
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Control)
            {
                DoMove(e.GetPosition(this));
            }
            else if ((bCaptured == true) && (AllowPanning == true))
            {
                // Get the new scroll position.   
                System.Windows.Point PointAtNow = e.GetPosition(this);

                // Determine the new amount to scroll.   

                System.Windows.Point delta = new System.Windows.Point((PointAtNow.X > this.MouseStartPoint.X) ? -(PointAtNow.X - this.MouseStartPoint.X) : (this.MouseStartPoint.X - PointAtNow.X),
                         (PointAtNow.Y > this.MouseStartPoint.Y) ? -(PointAtNow.Y - this.MouseStartPoint.Y) : (this.MouseStartPoint.Y - PointAtNow.Y));

                if (delta.X < 0)
                    System.Threading.Thread.Sleep(0);
                this.ScrollViewerThing.ScrollToHorizontalOffset(this.scrollStartOffset.X + delta.X);
                this.ScrollViewerThing.ScrollToVerticalOffset(this.scrollStartOffset.Y + delta.Y);

                bCaptured = false;
                this.ReleaseMouseCapture();
                //Mouse.Capture(null);
            }
            base.OnPreviewMouseUp(e);
        }


        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Left)
                {
                    if (this.Controller != null)
                        Controller.PanLeft();
                }
                else if (e.Key == Key.Right)
                {
                    if (this.Controller != null)
                        Controller.PanRight();
                }
                else if (e.Key == Key.Up)
                {
                    if (this.Controller != null)
                        Controller.TiltUp();
                }
                else if (e.Key == Key.Down)
                {
                    if (this.Controller != null)
                        Controller.TiltDown();
                }
            }


            if (this.PreventImageResizing == false)
            {
                if (e.Key == Key.OemPlus)
                {
                    if (System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        m_fScale = m_fScale + .1;
                        if (m_fScale < .1)
                            m_fScale = .1;
                        if (m_fScale > 5)
                            m_fScale = 5;
                        this.videoimage.Width = m_fScale * VideoWidth;
                        this.videoimage.Height = m_fScale * VideoHeight;
                    }
                }
                else if (e.Key == Key.OemMinus)
                {
                    if (System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        m_fScale = m_fScale - .1;
                        if (m_fScale < .1)
                            m_fScale = .1;
                        if (m_fScale > 5)
                            m_fScale = 5;
                        this.videoimage.Width = m_fScale * VideoWidth;
                        this.videoimage.Height = m_fScale * VideoHeight;
                    }
                }
            }
          
            base.OnPreviewKeyDown(e);
        }

       
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if ((System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Control) && (this.PreventImageResizing == false))
            {
                /// If the control key is down, zoom in and out as our mouse wheel turns (just like visual studio does)

                if (double.IsNaN(m_fScale) == true)
                    CalculateScale();

                if (e.Delta > 0) // zoom in some
                {
                    m_fScale = m_fScale + .1;
                }
                else if (e.Delta < 0)
                {
                    m_fScale = m_fScale - .1;
                }

                if (m_fScale < .1)
                    m_fScale = .1;
                if (m_fScale > 5)
                    m_fScale = 5;

                this.videoimage.Width = m_fScale * VideoWidth;
                this.videoimage.Height = m_fScale * VideoHeight;

            }
            else
            {
                base.OnPreviewMouseWheel(e);
            }
        }
  


        Point StartingPoint;
        private void videoimage_TouchDown(object sender, TouchEventArgs e)
        {
            StartingPoint = e.GetTouchPoint(sender as IInputElement).Position;
        }
        private void videoimage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StartingPoint = e.GetPosition(sender as IInputElement);
        }

        private void videoimage_TouchMove(object sender, TouchEventArgs e)
        {

        }

        private void videoimage_TouchUp(object sender, TouchEventArgs e)
        {

            Point Endpoint = e.GetTouchPoint(sender as IInputElement).Position;
            DoMove(Endpoint);
        }
       
        void DoMove(Point Endpoint)
        {
            int nXDif = (int) (Endpoint.X - StartingPoint.X);
            int nYDif = (int) (Endpoint.Y - StartingPoint.Y);

            if (this.Controller != null)
            {
                if (Math.Abs(nXDif) > this.VideoWidth / 10)
                {
                    if (nXDif > 0)
                        Controller.PanLeft();
                    else
                        Controller.PanRight();
                }
                if (Math.Abs(nYDif) > this.VideoHeight / 10)
                {
                    if (nYDif > 0)
                        Controller.TiltUp();
                    else
                        Controller.TiltDown();
                }
            }
        }

        private bool m_bDrawingMotionZone = false;

        public bool DrawingMotionZone
        {
            get { return m_bDrawingMotionZone; }
            set
            {
                m_bDrawingMotionZone = value;

            }
        }


        private void videoimage_MouseUp(object sender, MouseButtonEventArgs e)
        {

            Point Endpoint = e.GetPosition(sender as IInputElement);
            if (DrawingMotionZone == false)
                DoMove(Endpoint);
      
        }

        private void videoimage_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
         //   this.Width = e.NewSize.Width;
            //this.Height = e.NewSize.Height;
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
