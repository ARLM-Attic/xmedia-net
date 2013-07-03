using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using System.ComponentModel;
using RTP;
using System.IO;

namespace CameraViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        ObservableCollection<MotionJpegClient> Cameras = new ObservableCollection<MotionJpegClient>();
        WPFImageWindows.InterFrameCompressor JpegCompressor = new WPFImageWindows.InterFrameCompressor(new AudioClasses.VideoCaptureRate());

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
            this.CameraList.ItemsSource = Cameras;
            this.CameraList.DataContext = this;
        }

        void Load()
        {
            string strPath = string.Format("{0}\\CameraViewer", Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
            if (Directory.Exists(strPath) == false)
                Directory.CreateDirectory(strPath);
            string strFileName = string.Format("{0}\\cameraconfig.xml", strPath);
            if (File.Exists(strFileName) == false)
            {
                MessageBox.Show("No camera config file found, creating default");
                return;
            }
            else
            {
                NetworkCameraClientInformation[] cams = NetworkCameraClientInformation.Load(strFileName);
                if (cams != null)
                {
                    foreach (NetworkCameraClientInformation nextcam in cams)
                    {
                        Cameras.Add(new MotionJpegClient(JpegCompressor, nextcam));
                    }
                }
            }

        }
        void Save()
        {
            string strPath = string.Format("{0}\\CameraViewer", Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
            if (Directory.Exists(strPath) == false)
                Directory.CreateDirectory(strPath);
            string strFileName = string.Format("{0}\\cameraconfig.xml", strPath);

            List<NetworkCameraClientInformation> camerainfo = new List<NetworkCameraClientInformation>();
            foreach (MotionJpegClient client in Cameras)
                camerainfo.Add(client.NetworkCameraInformation);

            NetworkCameraClientInformation.Save(strFileName, camerainfo.ToArray());
        }

        private void ButtonAddCamera_Click(object sender, RoutedEventArgs e)
        {
            EditCameraWindow cameradlg = new EditCameraWindow();
            if (cameradlg.ShowDialog() == true)
            {
                Cameras.Add(new MotionJpegClient(JpegCompressor, cameradlg.CameraInformation));
                Save();
            }
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double nNewEffectiveWidth = e.NewSize.Width-80;
            double nNewEffectiveHeight = e.NewSize.Height - 60;
            if (nNewEffectiveWidth <= 0)
                nNewEffectiveWidth = 1;
            if (nNewEffectiveHeight <= 0)
                nNewEffectiveHeight = 1;
            double fWidthToHeightRatio = ((double)nNewEffectiveWidth) / ((double)nNewEffectiveHeight);


            int nItemsPerRow = 1;

            double fProjectedItemWidth = nNewEffectiveWidth / nItemsPerRow;
            double fProjectedItemHeight = fProjectedItemWidth * 9 / 16;
          
            {

                int nRows = (int) Math.Ceiling((double) Cameras.Count / (double)nItemsPerRow);
                double fTotalRowHeight = nRows * fProjectedItemHeight;
                
                double fVerticalSpaceLeft = nNewEffectiveHeight - fTotalRowHeight;
                
                while ((fVerticalSpaceLeft < 0) && (nRows > 1) && (fProjectedItemWidth > 320)) // Not fitting everything on the screen
                {

                    nItemsPerRow++;
                    fProjectedItemWidth = nNewEffectiveWidth / nItemsPerRow;
                    fProjectedItemHeight = fProjectedItemWidth * 9 / 16;

                    nRows = (int)Math.Ceiling((double)Cameras.Count / (double)nItemsPerRow);
                    fTotalRowHeight = nRows * fProjectedItemHeight;
                    fVerticalSpaceLeft = nNewEffectiveHeight - fTotalRowHeight;
                }

                if (fProjectedItemWidth < 320)
                {
                    fProjectedItemWidth = 320;
                    fProjectedItemHeight = fProjectedItemWidth * 9 / 16;
                }

                /// Now maximize the space
                /// 
                double fHorizontalSpaceLeft = nNewEffectiveWidth - nItemsPerRow * fProjectedItemWidth;
                fVerticalSpaceLeft = nNewEffectiveHeight - fTotalRowHeight;
                
                while ((fVerticalSpaceLeft > 0) && (fHorizontalSpaceLeft > 0)) 
                {

                    double fNewWidth = fProjectedItemWidth + 10;
                    double fNewHeight = fNewWidth * 9 / 16;
                    fHorizontalSpaceLeft = nNewEffectiveWidth - nItemsPerRow * fProjectedItemWidth;
                    fVerticalSpaceLeft = nNewEffectiveHeight - fTotalRowHeight;

                    if ((fVerticalSpaceLeft <= 0) || (fHorizontalSpaceLeft <= 0))
                        break;

                    fProjectedItemWidth = fNewWidth;
                    fProjectedItemHeight = fNewHeight;
                }                
            }

            

            ItemWidth = fProjectedItemWidth;
            ItemHeight = fProjectedItemHeight;

            foreach (MotionJpegClient cam in Cameras)
            {
                cam.ItemWidth = ItemWidth;
                cam.ItemHeight = ItemHeight;
            }
        }

        private double m_nItemWidth = 320;

        public double ItemWidth
        {
            get { return m_nItemWidth; }
            set
            {
                if (m_nItemWidth != value)
                {
                    m_nItemWidth = value;
                    FirePropertyChanged("ItemWidth");
                }
            }
        }
        private double m_nItemHeight = 180;

        public double ItemHeight
        {
            get { return m_nItemHeight; }
            set
            {
                if (m_nItemHeight != value)
                {
                    m_nItemHeight = value;
                    FirePropertyChanged("ItemHeight");
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

        private void VideoControl_OnOpenVideo(object sender, EventArgs e)
        {

        }

        private void VideoControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.GridFullVideo.Visibility = System.Windows.Visibility.Visible;
            this.GridFullVideo.DataContext = ((FrameworkElement)sender).DataContext;
            this.ButtonBack.Visibility = System.Windows.Visibility.Visible;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.GridFullVideo.Visibility = System.Windows.Visibility.Collapsed;
            this.GridFullVideo.DataContext = null;
            this.ButtonBack.Visibility = System.Windows.Visibility.Hidden;

        }

        private void ButtonEditCamera_Click(object sender, RoutedEventArgs e)
        {
            MotionJpegClient client = ((FrameworkElement)sender).DataContext as MotionJpegClient;
            if (client == null)
                return;

            EditCameraWindow cameradlg = new EditCameraWindow();
            cameradlg.CameraInformation = client.NetworkCameraInformation;
            if (cameradlg.ShowDialog() == true)
            {
                if (client.CameraActive == true)
                {
                    client.CameraActive = false;
                    client.CameraActive = true;
                }
                Save();
            }

        }

        private void ButtonPanLeft_Click(object sender, RoutedEventArgs e)
        {
            MotionJpegClient client = ((FrameworkElement)sender).DataContext as MotionJpegClient;
            if (client == null)
                return;
            client.PanLeft();
        }


        private void ButtonPanRight_Click(object sender, RoutedEventArgs e)
        {
            MotionJpegClient client = ((FrameworkElement)sender).DataContext as MotionJpegClient;
            if (client == null)
                return;
            client.PanRight();
        }

     

     

        private void ButtonActivateCamera_Click(object sender, RoutedEventArgs e)
        {
            MotionJpegClient client = ((FrameworkElement)sender).DataContext as MotionJpegClient;
            if (client == null)
                return;
            client.CameraActive = true;
        }

        private void ButtonStopCamera_Click(object sender, RoutedEventArgs e)
        {
            MotionJpegClient client = ((FrameworkElement)sender).DataContext as MotionJpegClient;
            if (client == null)
                return;
            client.CameraActive = false;
        }

        private void ButtonPanUp_Click(object sender, RoutedEventArgs e)
        {
            MotionJpegClient client = ((FrameworkElement)sender).DataContext as MotionJpegClient;
            if (client == null)
                return;
            client.TiltUp();

        }

        private void ButtonPanDown_Click(object sender, RoutedEventArgs e)
        {
            MotionJpegClient client = ((FrameworkElement)sender).DataContext as MotionJpegClient;
            if (client == null)
                return;
            client.TiltDown();

        }
    }

    // Converter that negates a boolean value
    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class NotVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bValue = (bool)value;
            if (bValue == true)
                return System.Windows.Visibility.Collapsed;
            else
                return System.Windows.Visibility.Visible;

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Windows.Visibility vValue = (System.Windows.Visibility)value;
            if (vValue == System.Windows.Visibility.Collapsed)
                return true;
            else
                return false;
        }
        private static object ConvertValue(object value)
        {
            if (!(value is bool))
            {
                throw new NotSupportedException("Only bool is supported.");
            }
            return !(bool)value;
        }
    }

    // Converter that negates a boolean value
    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bValue = (bool)value;
            if (bValue == true)
                return System.Windows.Visibility.Visible;
            else
                return System.Windows.Visibility.Collapsed;

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Windows.Visibility vValue = (System.Windows.Visibility)value;
            if (vValue == System.Windows.Visibility.Collapsed)
                return false;
            else
                return true;
        }
        private static object ConvertValue(object value)
        {
            if (!(value is bool))
            {
                throw new NotSupportedException("Only bool is supported.");
            }
            return !(bool)value;
        }

    } 
    

}
