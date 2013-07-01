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
            string strPath = string.Format("{0}\\CameraViewer", Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));
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
                MotionJpegClientInformation[] cams = MotionJpegClientInformation.Load(strFileName);
                if (cams != null)
                {
                    foreach (MotionJpegClientInformation nextcam in cams)
                    {
                        Cameras.Add(new MotionJpegClient(JpegCompressor, nextcam));
                    }
                }
            }

        }
        void Save()
        {
             string strPath = string.Format("{0}\\CameraViewer", Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));
            if (Directory.Exists(strPath) == false)
                Directory.CreateDirectory(strPath);
            string strFileName = string.Format("{0}\\cameraconfig.xml", strPath);

            List<MotionJpegClientInformation> camerainfo = new List<MotionJpegClientInformation>();
            foreach (MotionJpegClient client in Cameras)
                camerainfo.Add(client.NetworkCameraInformation);

            MotionJpegClientInformation.Save(strFileName, camerainfo.ToArray());
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
            this.FullVideo.Visibility = System.Windows.Visibility.Visible;
            this.FullVideo.DataContext = ((FrameworkElement)sender).DataContext;
            this.ButtonBack.Visibility = System.Windows.Visibility.Visible;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.FullVideo.Visibility = System.Windows.Visibility.Collapsed;
            this.FullVideo.DataContext = null;
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
    }
}
