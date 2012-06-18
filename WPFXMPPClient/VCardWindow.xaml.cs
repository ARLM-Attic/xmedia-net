/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

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

using System.Net.XMPP;
using System.IO;

namespace WPFXMPPClient
{
    /// <summary>
    /// Interaction logic for VCardWindow.xaml
    /// </summary>
    public partial class VCardWindow : Window
    {
        public VCardWindow()
        {
            InitializeComponent();
        }

        private vcard m_objvcard = new vcard();

        public vcard vcard
        {
          get { return m_objvcard; }
          set { m_objvcard = value; }
        }

        BitmapImage BitmapImageSrc = null;
        public ImageSource CurrentSource = null;

        public XMPPClient XMPPClient = null;

        private void ImagePicture_MouseDown(object sender, MouseButtonEventArgs e)
        {
       
               
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = vcard;
            if (CurrentSource != null)
                this.ImagePicture.Source = CurrentSource;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
                this.DragMove();
        }

        private void ButtonSendFromFile_Click(object sender, RoutedEventArgs e)
        {
            /// Select a new avatar
            /// 
            this.ImagePicture.Source = null;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image files|*.png;*.jpg|All Files|*.*";
            if (dlg.ShowDialog() == true)
            {
                string strExtension = System.IO.Path.GetExtension(dlg.FileName);
                string strContentType = "image/png";
                if (strExtension == ".png")
                    strContentType = "image/png";
                else if ((strExtension == ".jpg") || (strExtension == ".jpeg"))
                    strContentType = "image/jpeg";
                else if (strExtension == ".gif")
                    strContentType = "image/gif";
                else if (strExtension == ".bmp")
                    strContentType = "image/bmp";
                else
                {
                    MessageBox.Show("Unknown image type", "Can't set avatar", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                FileStream stream = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);
                byte[] bData = new byte[stream.Length];
                stream.Read(bData, 0, bData.Length);
                stream.Close();

                //MemoryStream ms = new MemoryStream(bData);
                Image i = new Image();

                BitmapImageSrc = new BitmapImage();
                BitmapImageSrc.BeginInit();
                BitmapImageSrc.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                BitmapImageSrc.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.None;
                BitmapImageSrc.UriSource = new Uri(dlg.FileName);
                //BitmapImageSrc.StreamSource = stream;
                BitmapImageSrc.EndInit();

                int nWidth = (int)BitmapImageSrc.Width;
                int nHeight = (int)BitmapImageSrc.Height;
                //ms.Close();

                float fScale = 1.0f;
                if ((nWidth > 100) || (nHeight > 100))
                {
                    float fWtoH = (float)nWidth / (float)nHeight;

                    if (fWtoH > 1.0f) /// width is greater than height
                        fScale = 100.0f / (float)nWidth;
                    else
                        fScale = 100.0f / (float)nHeight;

                        //if (MessageBox.Show("This image is larger than the recommend avatar size of 64x64.  Are you sure you want to use it", "Confirm use of large image", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                      //  return;
                }

                if (fScale != 1.0f)
                {
                    TransformedBitmap transbit = new TransformedBitmap(BitmapImageSrc, new ScaleTransform(fScale, fScale));
                    //bit.BeginInit();
                    //bit.EndInit();

                    PngBitmapEncoder objImageEncoder = new PngBitmapEncoder();
                    BitmapFrame frame = BitmapFrame.Create(transbit);
                    objImageEncoder.Frames.Add(frame);

                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    objImageEncoder.Save(ms);

                    ms.Seek(0, SeekOrigin.Begin);
                    bData = new byte[ms.Length];
                    ms.Read(bData, 0, bData.Length);
                    ms.Close();
                    ms.Dispose();

                }


                vcard.Photo = new Photo();
                vcard.Photo.Bytes = bData;
                vcard.Photo.Type = strContentType;
                this.ImagePicture.Source = BitmapImageSrc;

                //XMPPClient.SetAvatar(bData, nWidth, nHeight, strContentType);
            }
        }

        private void ButtonSendPhotoCapture_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureWindow win = new CameraCaptureWindow();
            if (win.ShowDialog() == true)
            {
                Image i = new Image();
                string strFileName = string.Format("camera_{0}.jpg", Guid.NewGuid());

                byte [] bData = win.CompressedAcceptedImage;
                string strContentType = "image/jpeg";
                MemoryStream stream = new MemoryStream(win.CompressedAcceptedImage);

                BitmapImageSrc = new BitmapImage();
                BitmapImageSrc.BeginInit();
                BitmapImageSrc.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                BitmapImageSrc.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.None;
                BitmapImageSrc.StreamSource = stream;
                BitmapImageSrc.EndInit();

                int nWidth = (int)BitmapImageSrc.Width;
                int nHeight = (int)BitmapImageSrc.Height;
                //ms.Close();

                float fScale = 1.0f;
                if ((nWidth > 100) || (nHeight > 100))
                {
                    float fWtoH = (float)nWidth / (float)nHeight;

                    if (fWtoH > 1.0f) /// width is greater than height
                        fScale = 100.0f / (float)nWidth;
                    else
                        fScale = 100.0f / (float)nHeight;
                }

                if (fScale != 1.0f)
                {
                    TransformedBitmap transbit = new TransformedBitmap(BitmapImageSrc, new ScaleTransform(fScale, fScale));
                    //bit.BeginInit();
                    //bit.EndInit();

                    PngBitmapEncoder objImageEncoder = new PngBitmapEncoder();
                    BitmapFrame frame = BitmapFrame.Create(transbit);
                    objImageEncoder.Frames.Add(frame);

                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    objImageEncoder.Save(ms);

                    ms.Seek(0, SeekOrigin.Begin);
                    bData = new byte[ms.Length];
                    ms.Read(bData, 0, bData.Length);
                    ms.Close();
                    ms.Dispose();

                }


                vcard.Photo = new Photo();
                vcard.Photo.Bytes = bData;
                vcard.Photo.Type = strContentType;
                this.ImagePicture.Source = BitmapImageSrc;
            }

        }


       
    }
}
