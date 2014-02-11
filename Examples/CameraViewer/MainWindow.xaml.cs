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
            string strPath = string.Format("{0}\\CameraViewer", Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
            if (Directory.Exists(strPath) == false)
                Directory.CreateDirectory(strPath);
            string strFileName = string.Format("{0}\\cameraconfig.xml", strPath);
            Load(strFileName);
            this.CameraList.ItemsSource = Cameras;
            this.CameraQuickList.ItemsSource = Cameras;
            this.CameraList.DataContext = this;
        }

        string FileNameLoaded = null;
        void Load(string strFileName)
        {
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
                    Cameras.Clear();
                    FileNameLoaded = strFileName;
                    foreach (NetworkCameraClientInformation nextcam in cams)
                    {
                        Cameras.Add(new MotionJpegClient(JpegCompressor, nextcam));
                    }
                }
            }

        }
        void Save(string strFileName)
        {

            List<NetworkCameraClientInformation> camerainfo = new List<NetworkCameraClientInformation>();
            foreach (MotionJpegClient client in Cameras)
                camerainfo.Add(client.NetworkCameraInformation);

            NetworkCameraClientInformation.Save(strFileName, camerainfo.ToArray());
            FileNameLoaded = strFileName;
        }

        void Save()
        {
            if (FileNameLoaded == null)
            {
                string strPath = string.Format("{0}\\CameraViewer", Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
                if (Directory.Exists(strPath) == false)
                    Directory.CreateDirectory(strPath);
                FileNameLoaded = string.Format("{0}\\cameraconfig.xml", strPath);
            }
            Save(FileNameLoaded);
        }

        private void ButtonAddCamera_Click(object sender, RoutedEventArgs e)
        {
            EditCameraWindow cameradlg = new EditCameraWindow();
            cameradlg.ShowDelete = false;
            if (cameradlg.ShowDialog() == true)
            {
                Cameras.Add(new MotionJpegClient(JpegCompressor, cameradlg.CameraInformation));
                Save();
            }
        }

        private bool m_bVerticalLayout = true;

        public bool VerticalLayout
        {
            get { return m_bVerticalLayout; }
            set { m_bVerticalLayout = value; }
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (VerticalLayout == false)
                return;

            double nNewEffectiveWidth = MainGrid.ColumnDefinitions[1].ActualWidth-60; // e.NewSize.Width - 80;
            double nNewEffectiveHeight = MainGrid.RowDefinitions[1].ActualHeight-60; // e.NewSize.Height - 60;
            if (nNewEffectiveWidth <= 0)
                nNewEffectiveWidth = 1;
            if (nNewEffectiveHeight <= 0)
                nNewEffectiveHeight = 1;


            double fWidthToHeightRatio = ((double)nNewEffectiveWidth) / ((double)nNewEffectiveHeight);

            if (fWidthToHeightRatio > 3)
                SetHorizontal(nNewEffectiveWidth, nNewEffectiveHeight);
            else if (fWidthToHeightRatio < .6)
                SetVertical(nNewEffectiveWidth, nNewEffectiveHeight);
            else
                SetFill(nNewEffectiveWidth, nNewEffectiveHeight);

        }

        void SetVertical(double nNewEffectiveWidth, double nNewEffectiveHeight)
        {
            var scrollViewer = GetDescendantByType(CameraList, typeof(ScrollViewer)) as ScrollViewer;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            double fProjectedItemWidth = nNewEffectiveWidth;
            double fProjectedItemHeight = fProjectedItemWidth * 9 / 16;


            ItemWidth = fProjectedItemWidth;
            ItemHeight = fProjectedItemHeight;

            foreach (MotionJpegClient cam in Cameras)
            {
                cam.ItemWidth = ItemWidth;
                cam.ItemHeight = ItemHeight;
            }
        }

        void SetHorizontal(double nNewEffectiveWidth, double nNewEffectiveHeight)
        {
            var scrollViewer = GetDescendantByType(CameraList, typeof(ScrollViewer)) as ScrollViewer;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

            double fProjectedItemHeight = nNewEffectiveHeight;
            double fProjectedItemWidth = fProjectedItemHeight * 16 / 9;

            ItemWidth = fProjectedItemWidth;
            ItemHeight = fProjectedItemHeight;

            foreach (MotionJpegClient cam in Cameras)
            {
                cam.ItemWidth = ItemWidth;
                cam.ItemHeight = ItemHeight;
            }
        }

        
        private void SetFill(double nNewEffectiveWidth, double nNewEffectiveHeight)
        {
            var scrollViewer = GetDescendantByType(CameraList, typeof(ScrollViewer)) as ScrollViewer;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            double fWidthToHeightRatio = ((double)nNewEffectiveWidth) / ((double)nNewEffectiveHeight);


            int nItemsPerRow = 1;

            double fProjectedItemWidth = nNewEffectiveWidth / nItemsPerRow;
            double fProjectedItemHeight = fProjectedItemWidth * 9 / 16;

            int nCount = 0;
            foreach (var cam in Cameras)
            {
                if (cam.Visible == true)
                    nCount++;
            }


            int nRows = (int)Math.Ceiling((double)nCount / (double)nItemsPerRow);
            double fTotalRowHeight = nRows * fProjectedItemHeight;

            double fVerticalSpaceLeft = nNewEffectiveHeight - fTotalRowHeight;

            while ((fVerticalSpaceLeft < 0) && (nRows > 1) && (fProjectedItemWidth > 320)) // Not fitting everything on the screen
            {

                nItemsPerRow++;
                fProjectedItemWidth = nNewEffectiveWidth / nItemsPerRow;
                fProjectedItemHeight = fProjectedItemWidth * 9 / 16;

                nRows = (int)Math.Ceiling((double)nCount / (double)nItemsPerRow);
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



            ItemWidth = fProjectedItemWidth;
            ItemHeight = fProjectedItemHeight;

            foreach (MotionJpegClient cam in Cameras)
            {
                cam.ItemWidth = ItemWidth;
                cam.ItemHeight = ItemHeight;
            }
        }

        public static Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null)
            {
                return null;
            }
            if (element.GetType() == type)
            {
                return element;
            }
            Visual foundElement = null;
            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                {
                    break;
                }
            }
            return foundElement;
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
            cameradlg.ShowDelete = true;
            cameradlg.CameraInformation = client.NetworkCameraInformation;
            if (cameradlg.ShowDialog() == true)
            {
                if (cameradlg.CameraResult == CameraResult.Saved)
                {

                    if (client.CameraActive == true)
                    {
                        client.CameraActive = false;
                        client.CameraActive = true;
                    }
                    Save();
                }
                else if (cameradlg.CameraResult == CameraResult.Delete)
                {
                    if (client.CameraActive == true)
                        client.CameraActive = false;
                    Cameras.Remove(client);
                    Save();
                }
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

        private void CommandBack_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.GridFullVideo.Visibility = System.Windows.Visibility.Collapsed;
            this.GridFullVideo.DataContext = null;
            this.ButtonBack.Visibility = System.Windows.Visibility.Hidden;
        }

        private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                Load(dlg.FileName);
            }

        }

        private void CommandBack_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (this.GridFullVideo.DataContext != null)
                e.CanExecute = true;
            else
                e.CanExecute = false;

        }

        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                Save(dlg.FileName);
            }
        }

        private void ButtonBig_Click(object sender, RoutedEventArgs e)
        {
            this.GridFullVideo.Visibility = System.Windows.Visibility.Visible;
            this.GridFullVideo.DataContext = ((FrameworkElement)sender).DataContext;
            this.ButtonBack.Visibility = System.Windows.Visibility.Visible;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {

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
