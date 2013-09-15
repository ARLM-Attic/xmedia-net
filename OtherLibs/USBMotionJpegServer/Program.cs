using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace USBMotionJpegServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string [] args)
        {
            if ((args.Length == 1) && ("-runAsApp" == args[0]))
            {
                Service1 service = new Service1();
                service.StartInteractive();
                Console.WriteLine("Starting usb camera server interactively.  Type 'Exit' to quit");
                while (true)
                {
                    string strLine = Console.ReadLine();
                    if (strLine == null)
                    {
                        System.Threading.Thread.Sleep(1000);
                        continue;
                    }
                    if (string.Compare(strLine.Trim(), "Exit", true) == 0)
                        break;
                }
                service.Stop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new Service1() 
                };
                ServiceBase.Run(ServicesToRun);
            }


          
        }
    }
}
