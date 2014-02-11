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
using System.Windows.Shapes;

namespace CameraViewer
{
    /// <summary>
    /// Interaction logic for EditCameraWindow.xaml
    /// </summary>
    public partial class EditCameraWindow : Window
    {
        public EditCameraWindow()
        {
            InitializeComponent();
        }

        public bool ShowDelete = true;
        public RTP.NetworkCameraClientInformation CameraInformation = new RTP.NetworkCameraClientInformation();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CameraResult = CameraResult.None;
            this.DataContext = CameraInformation;
            this.PasswordBox1.Password = CameraInformation.Password;
            if (ShowDelete == true)
                this.ButtonDeleteCamera.Visibility = System.Windows.Visibility.Visible;
            else
                this.ButtonDeleteCamera.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CameraInformation.Password = this.PasswordBox1.Password;
            CameraResult = CameraResult.Saved;
            this.DialogResult = true;
            this.Close();
        }


        public CameraResult CameraResult = CameraResult.None;

        private void ButtonDeleteCamera_Click(object sender, RoutedEventArgs e)
        {
            CameraInformation.Password = this.PasswordBox1.Password;
            CameraResult = CameraResult.Delete;
            this.DialogResult = true;
            this.Close();

        }
    }

    public enum CameraResult
    {
        Saved,
        Delete,
        None
    }
}
