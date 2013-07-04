using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.XMPP;
namespace Tests
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            TestCamera();
            //TestSerialization();
        }

        
        static void TestCamera()
        {
            WPFImageWindows.CameraCaptureWindow win = new WPFImageWindows.CameraCaptureWindow();
            win.ShowDialog();
            
        }

        static void TestSerialization()
        {
         

        }
    }
}
