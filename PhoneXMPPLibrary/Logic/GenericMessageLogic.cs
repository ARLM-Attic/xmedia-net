using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using System.Text.RegularExpressions;


namespace System.Net.XMPP
{

    /// <summary>
    /// Parses incoming messages and adds them to a RosterItems' conversation list
    /// </summary>
    public class GenericMessageLogic : Logic
    {
        public GenericMessageLogic(XMPPClient client)
            : base(client)
        {
            IsCompleted = false;
        }


        public void SendChatMessage(TextMessage txtmsg)
        {
            txtmsg.Sent = true;
            ChatMessage msg = new ChatMessage(null);
            msg.From = txtmsg.From;
            msg.To = txtmsg.To;
            msg.Type = "chat";
            msg.Body = txtmsg.Message;
            //msg.InnerXML = string.Format(@"<body>{0}</body>", txtmsg.Message);

            /// Find the roster guy for this message and add it to their conversation
            /// 
            RosterItem item = XMPPClient.FindRosterItem(txtmsg.To);
            if (item != null)
            {
                item.AddSendTextMessage(txtmsg);
                // Notify XMPPClient that a new conversation item has been added
                XMPPClient.FireNewConversationItem(item, false, txtmsg);
            }

            XMPPClient.SendXMPP(msg);
        }


        public override bool NewMessage(Message iq)
        {
            /// See if this is a standard text message
            /// 
            if (iq is ChatMessage)
            {
                ChatMessage chatmsg = iq as ChatMessage;
                RosterItem item = XMPPClient.FindRosterItem(chatmsg.From);
                if (item != null)
                {
                    if (chatmsg.ConversationState == ConversationState.none)
                    {
                        TextMessage txtmsg = new TextMessage();
                        txtmsg.From = chatmsg.From;
                        txtmsg.To = chatmsg.To;
                        txtmsg.Received = DateTime.Now;
                        if (chatmsg.Delivered > DateTime.MinValue)
                            txtmsg.Received = chatmsg.Delivered; /// May have been a server stored message
                        txtmsg.Message = chatmsg.Body;
                        txtmsg.Sent = false;
                        item.AddRecvTextMessage(txtmsg);
                        item.HasNewMessages = true;

                        // Notify XMPPClient that a new conversation item has been added
                        XMPPClient.FireNewConversationItem(item, true, txtmsg);
                    }
                    else // A conversation message
                    {
                        item.Conversation.ConversationState = chatmsg.ConversationState;
                        XMPPClient.FireNewConversationState(item, item.Conversation.ConversationState);
                    }
                }
                return true;
            }

            return false;
        }
    }
}
