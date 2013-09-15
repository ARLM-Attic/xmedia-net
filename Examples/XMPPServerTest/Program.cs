using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.XMPP;
using System.Net.XMPP.Server;

namespace XMPPServerTest
{
    class Program
    {
        static void Main(string[] args)
        {

            XMPPServerConfig config =new XMPPServerConfig();
            config.AllowTLS = false; /// turn off for debugging
            config.DomainName = "tenfingers.com";

            XMPPServer Server = new XMPPServer(config, new ConsoleLogger());

            XMPPUser usertest = Server.Domain.UserList.AddUser(new XMPPUser(Server) { UserName = "test", Password = "password" });
            XMPPUser usertest2 = Server.Domain.UserList.AddUser(new XMPPUser(Server) { UserName = "test2", Password = "password" });

            usertest.Roster.Add(new rosteritem() { Name = "da-man", JID = "test2@tenfingers.com", Subscription = "both" });
            usertest2.Roster.Add(new rosteritem() { Name = "bob", JID = "test@tenfingers.com", Subscription = "both" });

            Server.AuthenticationMethods.Add(new PlainAuthenticationMechanism(Server, null));
            

            bool bStarted = Server.Start();
            if (bStarted == false)
            {
                Console.WriteLine("Failed to start the server.");
                return;
            }

            Console.WriteLine("Server running, type 'exit' to quit");
            while (true)
            {
                string strLine = Console.ReadLine();

                if (string.Compare(strLine.Trim(), "exit", true) == 0)
                    break;
            }

            Server.Stop();
        }

    }

    class ConsoleLogger : xmedianet.socketserver.ILogInterface
    {
        public ConsoleLogger()
        {
        }

        public void LogMessage(string strCateogry, string strGuid, xmedianet.socketserver.MessageImportance importance, string strMessage, params object[] msgparams)
        {
            Console.WriteLine(string.Format(strMessage, msgparams));
        }

        public void LogWarning(string strCateogry, string strGuid, xmedianet.socketserver.MessageImportance importance, string strMessage, params object[] msgparams)
        {
            Console.WriteLine(string.Format(strMessage, msgparams));
        }

        public void LogError(string strCateogry, string strGuid, xmedianet.socketserver.MessageImportance importance, string strMessage, params object[] msgparams)
        {
            Console.WriteLine(string.Format(strMessage, msgparams));
        }

        public void LogMessage(string strGuid, xmedianet.socketserver.MessageImportance importance, string strMessage, params object[] msgparams)
        {
            Console.WriteLine(string.Format(strMessage, msgparams));
        }

        public void LogWarning(string strGuid, xmedianet.socketserver.MessageImportance importance, string strMessage, params object[] msgparams)
        {
            Console.WriteLine(string.Format(strMessage, msgparams));
        }

        public void LogError(string strGuid, xmedianet.socketserver.MessageImportance importance, string strMessage, params object[] msgparams)
        {
            Console.WriteLine(string.Format(strMessage, msgparams));
        }

        public void ClearLog()
        {
        }
    }

}
