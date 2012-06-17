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

namespace WPFXMPPClient
{
    /// <summary>
    /// Interaction logic for CameraCaptureWindow.xaml
    /// </summary>
    public partial class CameraCaptureWindow : Window
    {
        public CameraCaptureWindow()
        {
            InitializeComponent();
        }

        public byte[] CompressedAcceptedImage = null;
        private void CameraControl_OnAccept(object sender, EventArgs e)
        {
            CompressedAcceptedImage = CameraControl.CompressedAcceptedImage;
            this.DialogResult = true;
            this.Close();

        }

    }
}
