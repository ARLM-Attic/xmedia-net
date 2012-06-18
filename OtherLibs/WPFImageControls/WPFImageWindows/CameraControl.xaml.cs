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
using System.ComponentModel;

namespace WPFImageWindows
{
    /// <summary>
    /// Interaction logic for CameraWindow.xaml
    /// </summary>
    public partial class CameraControl : UserControl, INotifyPropertyChanged
    {
        public CameraControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            VideoDevices = MFVideoCaptureDevice.GetCaptureDevices();
            this.ComboBoxSource.ItemsSource = VideoDevices;

            foreach (MFVideoCaptureDevice nextdev in VideoDevices)
            {
                if (nextdev.UniqueName ==Properties.Settings.Default.LastCamera)
                {
                    this.ComboBoxSource.SelectedItem = nextdev;
                    return;
                }
            }

            this.ComboBoxSource.SelectedIndex = 0;
        }

        private int m_nFrameLimit = 5;

        public int FrameLimit
        {
            get { return m_nFrameLimit; }
            set { m_nFrameLimit = value; }
        }

        MFVideoCaptureDevice[] VideoDevices = null;
        MFVideoCaptureDevice CurrentVideoDevice = null;
        
        /// <summary>
        /// Encapsulates an MFVideoCaptureDevice to provide functions like capture image, motion detection, etc
        /// </summary>
        VideoCaptureSource CurrentSource = null;
        VideoCaptureRate CurrentRate = null;

        void StopCurrentVideo()
        {
            if (CurrentSource != null)
            {
                CurrentSource.CameraActive = false;
                CurrentSource = null;
            }
            CurrentRate = null;
            CurrentVideoDevice = null;
        }

        private void ComboBoxSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StopCurrentVideo();

            CurrentVideoDevice = this.ComboBoxSource.SelectedItem as MFVideoCaptureDevice;
            if (CurrentVideoDevice != null)
            {
                CurrentRate = null;
                /// Start capturing at the largest image capture rate... Usually the largest resolution and a real low frame rate, or a unrealistically high frame rate
                /// (Just my guess, here is what I have from current cameras)
                /// For now, make sure it's stream 0 until we fix our bugs
                /// 
                foreach (VideoCaptureRate rate in CurrentVideoDevice.VideoFormats)
                {
                    /// Can only handle stream index 0 now, other streams don't work, must have a bug
                    if (rate.StreamIndex != 0)
                        continue;
                    /// Can only handle formats that can be expanded to RGB right now
                    if ( !((rate.CompressedFormat == VideoDataFormat.MJPEG) || (rate.CompressedFormat == VideoDataFormat.RGB32) || (rate.CompressedFormat == VideoDataFormat.RGB24) || (rate.CompressedFormat == VideoDataFormat.MPNG)) )
                        continue;

                    if (CurrentRate == null)
                    {
                        CurrentRate = rate;
                    }
                    else
                    {
                        if (CurrentRate.PixelCount < rate.PixelCount)
                        {
                            CurrentRate = rate;
                        }
                    }
                }

                if (CurrentRate != null)
                {
                    CurrentSource = new VideoCaptureSource(this.CurrentVideoDevice);
                    this.CurrentVideoDevice.MaxFrameRate = FrameLimit;
                    this.MainVideoControl.DataContext = CurrentVideoDevice;
                    CurrentSource.ActiveVideoCaptureRate = CurrentRate;
                    CurrentSource.CameraActive = true;
                    this.DataContext = this;

                    Properties.Settings.Default.LastCamera = this.CurrentVideoDevice.UniqueName;
                    Properties.Settings.Default.Save();

                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(OnSetParams));
                }
            }
        }

        void OnSetParams(object obj)
        {
            System.Threading.Thread.Sleep(100);
            if (CurrentSource == null)
                return;

            Dispatcher.Invoke(new EventHandler(SafeOnSetParams), null, null);
        }

        void SafeOnSetParams(object obj, EventArgs args)
        {
            int nFocusMin = 0;
            int nFocusMax = 0;
            int nFocusStep = 0;
            int nFocusDefault = 0;

            CurrentSource.GetFocusRange(out nFocusMin, out nFocusMax, out nFocusStep, out nFocusDefault);
            SliderFocus.Minimum = nFocusMin;
            SliderFocus.Maximum = nFocusMax;
            SliderFocus.SmallChange = nFocusStep;
            SliderFocus.LargeChange = nFocusStep;
            SliderFocus.TickFrequency = nFocusStep;
            m_nFocusValue = nFocusDefault;

            SliderFocus.DataContext = this;

            int nExposureMin = 0;
            int nExposureMax = 0;
            int nExposureStep = 0;
            int nExposureDefault = 0;

            CurrentSource.GetExposureRange(out nExposureMin, out nExposureMax, out nExposureStep, out nExposureDefault);
            SliderExposure.Minimum = nExposureMin;
            SliderExposure.Maximum = nExposureMax;
            SliderExposure.SmallChange = nExposureStep;
            SliderExposure.LargeChange = nExposureStep;
            SliderExposure.TickFrequency = nExposureStep;
            m_nExposureValue = nExposureDefault;

            SliderExposure.DataContext = this;

            int nIrisMin = 0;
            int nIrisMax = 0;
            int nIrisStep = 0;
            int nIrisDefault = 0;

            CurrentSource.GetIrisRange(out nIrisMin, out nIrisMax, out nIrisStep, out nIrisDefault);
            SliderIris.Minimum = nIrisMin;
            SliderIris.Maximum = nIrisMax;
            SliderIris.SmallChange = nIrisStep;
            SliderIris.LargeChange = nIrisStep;
            SliderIris.TickFrequency = nIrisStep;
            m_nIrisValue = nIrisDefault;

            SliderIris.DataContext = this;
        }

        System.Windows.Media.Imaging.BitmapSource objImage = null;
        private void ButtonTakePicture_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentSource != null)
            {
                byte[] bPic = CurrentSource.GetPictureRGB();
                System.Media.SoundPlayer player = new System.Media.SoundPlayer("Sounds/shutter.wav");
                player.Play();

                if (bPic != null)
                {
                    PixelFormat format = PixelFormats.Rgb24;
                    int nBytesPerPixel = 3;
                    if (CurrentRate.UncompressedFormat == VideoDataFormat.RGB32)
                    {
                        format = PixelFormats.Bgr32;
                        nBytesPerPixel = 4;
                    }
                    int nStride = CurrentRate.Width * nBytesPerPixel;

                    objImage = BitmapSource.Create(CurrentRate.Width, CurrentRate.Height, 96, 96, format, null, bPic, nStride);

                    ImageCurrentPic.Source = objImage;
                    ImageCurrentPic.Visibility = System.Windows.Visibility.Visible;
                    this.MainVideoControl.Visibility = System.Windows.Visibility.Hidden;
                    
                    this.ButtonAccept.Visibility = System.Windows.Visibility.Visible;
                    this.ButtonReject.Visibility = System.Windows.Visibility.Visible;
                    this.ComboBoxSource.Visibility = System.Windows.Visibility.Collapsed;
                    this.ButtonTakePicture.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        public byte[] CompressedAcceptedImage = null;

        public event EventHandler OnAccept = null;
        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            /// Make a compressed jpeg from our image
            /// 
            if (objImage != null)
            {
                JpegBitmapEncoder objImageEncoder = new JpegBitmapEncoder();
                BitmapFrame frame = BitmapFrame.Create(objImage);
                objImageEncoder.Frames.Add(frame);

                //save to memory stream
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                objImageEncoder.Save(ms);

                ms.Seek(0, System.IO.SeekOrigin.Begin);
                CompressedAcceptedImage = new byte[ms.Length];
                ms.Read(CompressedAcceptedImage, 0, CompressedAcceptedImage.Length);
                ms.Close();
                ms.Dispose();
            }

            if (OnAccept != null)
                OnAccept(this, new EventArgs());
        }

        private void ButtonReject_Click(object sender, RoutedEventArgs e)
        {
            ImageCurrentPic.Source = null;
            objImage = null;
            ImageCurrentPic.Visibility = System.Windows.Visibility.Hidden;
            this.MainVideoControl.Visibility = System.Windows.Visibility.Visible;

            this.ButtonAccept.Visibility = System.Windows.Visibility.Collapsed;
            this.ButtonReject.Visibility = System.Windows.Visibility.Collapsed;
            this.ComboBoxSource.Visibility = System.Windows.Visibility.Visible;
            this.ButtonTakePicture.Visibility = System.Windows.Visibility.Visible;

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            StopCurrentVideo();
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }
        #endregion


        private int m_nFocusValue = 0;

        public int FocusValue
        {
            get
            {
                return m_nFocusValue;
            }
            set
            {
                if (m_nFocusValue != value)
                {
                    m_nFocusValue = value;
                    if (this.CurrentSource != null)
                    {
                        int nValue = (int)m_nFocusValue;
                        this.CurrentSource.SetFocus(nValue);
                    }
                    FirePropertyChanged("FocusValue");
                }
            }
        }

        private int m_nExposureValue = 0;

        public int ExposureValue
        {
            get
            {
                return m_nExposureValue;
            }
            set
            {
                if (m_nExposureValue != value)
                {
                    m_nExposureValue = value;
                    if (this.CurrentSource != null)
                    {
                        int nValue = (int)m_nExposureValue;
                        this.CurrentSource.SetExposure(nValue);
                    }
                    FirePropertyChanged("ExposureValue");
                }
            }
        }

        private int m_nIrisValue = 0;

        public int IrisValue
        {
            get
            {
                return m_nIrisValue;
            }
            set
            {
                if (m_nIrisValue != value)
                {
                    m_nIrisValue = value;
                    if (this.CurrentSource != null)
                    {
                        int nValue = (int)m_nIrisValue;
                        this.CurrentSource.SetIris(nValue);
                    }
                    FirePropertyChanged("IrisValue");
                }
            }
        }

    }
}
