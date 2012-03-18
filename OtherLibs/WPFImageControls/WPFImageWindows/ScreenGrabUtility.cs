using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFImageWindows
{
    public class ScreenGrabUtility
    {
        public static byte[] GetScreenPNG()
        {

            byte[] bCompressedStream = null;

            OverlayWindow win = new OverlayWindow();
            win.Left = 0;
            win.Top = 0;
            win.Width = System.Windows.SystemParameters.VirtualScreenWidth;
            win.Height = System.Windows.SystemParameters.VirtualScreenHeight;

            if (win.ShowDialog() == true)
            {
                ImageUtils.ImageWithPosition im = ImageUtils.Utils.GetDesktopWindowBytes((int)win.CaptureRectangle.X, (int)win.CaptureRectangle.Y, (int)win.CaptureRectangle.Width, (int)win.CaptureRectangle.Height);
                BitmapEncoder objImageEncoder = new PngBitmapEncoder();
                BitmapSource source = BitmapFrame.Create((int)win.CaptureRectangle.Width, (int)win.CaptureRectangle.Height, 96.0f, 96.0f, PixelFormats.Bgr24, null, im.ImageBytes, im.RowLengthBytes);
                BitmapFrame frame = BitmapFrame.Create(source);
                objImageEncoder.Frames.Add(frame);

                //save to memory stream
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                objImageEncoder.Save(ms);

                ms.Seek(0, SeekOrigin.Begin);
                bCompressedStream = new byte[ms.Length];
                ms.Read(bCompressedStream, 0, bCompressedStream.Length);
                ms.Close();
                ms.Dispose();

            }


            return bCompressedStream;
        }
    }
}
