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

        public RTP.MotionJpegClientInformation CameraInformation = new RTP.MotionJpegClientInformation();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = CameraInformation;
            this.PasswordBox1.Password = CameraInformation.Password;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CameraInformation.Password = this.PasswordBox1.Password;
            this.DialogResult = true;
            this.Close();
        }
    }
}
