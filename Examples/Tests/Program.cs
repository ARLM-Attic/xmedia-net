using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.XMPP;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AudioClasses;

namespace Tests
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //TestCamera();
            //TestSerialization();
            TestDirectXCapture();
        }

        
        static void TestCamera()
        {
            WPFImageWindows.CameraCaptureWindow win = new WPFImageWindows.CameraCaptureWindow();
            win.ShowDialog();
            
        }

        static void TestSerialization()
        {
         

        }

        static void TestDirectXCapture()
        {
            byte [] bImage = ImageUtils.Utils.DirectXScreenCap();
            BitmapEncoder objImageEncoder = null;
            objImageEncoder = new PngBitmapEncoder();
            byte[] bCompressedStream = null;
            try
            {
                BitmapSource source = BitmapFrame.Create(1920, 1080, 96.0f, 96.0f, PixelFormats.Bgr32, null, bImage, 1920*4);
                BitmapFrame frame = BitmapFrame.Create(source);
                frame.Freeze();
                objImageEncoder.Frames.Add(frame);

                FileStream outfil = new FileStream("c:/temp/screen.png", FileMode.Create, FileAccess.Write);
                using (outfil)
                {
                    objImageEncoder.Save(outfil);

                    outfil.Close();
                }

                frame = null;
                source = null;
                objImageEncoder = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
    }
}
