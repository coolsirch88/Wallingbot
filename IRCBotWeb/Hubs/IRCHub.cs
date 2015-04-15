using IRCBot.Common;
using IRCBot.Lib;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;

namespace IRCBotWeb.Hubs
{
    [HubName("IRC")]
    public class IRCHub : Hub
    {
        private static bool _eventsSetup = false;
        private IIRCBot _ircBot;
        public IRCHub(IIRCBot ircBot)
        {
            _ircBot = ircBot;
        }

        private void RemoveEvents()
        {
            _ircBot.eventReceiving -= IrcCommandReceived;
            _ircBot.eventJoin -= IrcJoin;
            _ircBot.eventPart -= IrcPart;
            _ircBot.eventMode -= IrcMode;
            _ircBot.eventNickChange -= IrcNickChange;
            _ircBot.eventKick -= IrcKick;
            _ircBot.eventQuit -= IrcQuit;
            _ircBot.eventEndMotd -= EndMotd;
        }
        private void AddEvents()
        {
            _ircBot.eventReceiving += new CommandReceived(IrcCommandReceived);
            _ircBot.eventJoin += new Join(IrcJoin);
            _ircBot.eventPart += new Part(IrcPart);
            _ircBot.eventMode += new Mode(IrcMode);
            _ircBot.eventNickChange += new NickChange(IrcNickChange);
            _ircBot.eventKick += new Kick(IrcKick);
            _ircBot.eventQuit += new Quit(IrcQuit);
            _ircBot.eventEndMotd += new IrcEndMotd(EndMotd);
        }

        public void Response(string message)
        {
            Clients.All.addNewMessageToPage(message);
        }

        public void Send(string message)
        {
            Clients.All.addNewMessageToPage(message);
            _ircBot.WriteMessage(message);
        }

        public void SendRaw(string message)
        {
            _ircBot.WriteRawMessage(message);
        }

        public void Start()
        {
            _ircBot.UserName = "TestWallingbot";
            _ircBot.UserPassword = "pass@word1";
            _ircBot.Channels.Add("#wallingbot");
            _ircBot.Connect("stark.coldfront.net", 6667, false);
        }

        public bool IsConnected()
        {
            return _ircBot.Connected;
        }

        public void Stop()
        {
            RemoveEvents();
            _ircBot.CloseIRC();
        }

        private void IrcCommandReceived(string IrcCommand)
        {
            //Console.WriteLine(IrcCommand);
            Response(IrcCommand);
            string[] commandParts = new string[IrcCommand.Split(' ').Length];
            commandParts = IrcCommand.Split(' ');
            if (commandParts[1] == "NOTICE")
            {
                string IrcUser = commandParts[0].Split('!')[0];
                string NoticeText = "";
                for (int intI = 3; intI < commandParts.Length; intI++)
                {
                    NoticeText += commandParts[intI] + " ";
                }
                if (NoticeText.Contains("Password accepted"))
                {
                    foreach (var channel in _ircBot.Channels)
                    {
                        _ircBot.ConnectChannel(channel);
                    }
                }
            }
        }
        private void IrcJoin(string channel, string userName)
        {
            Response(string.Format("{0} Joined {1}", userName, channel));
        }
        private void IrcPart(string channel, string userName)
        {
            Response(string.Format("{0} parted {1}", userName, channel));
        }
        private void IrcMode(string channel, string userName, string mode)
        {
            Response(string.Format("{0} set mode {1} on {2}", userName, mode, channel));
        }
        private void IrcNickChange(string oldNick, string newNick)
        {
            Response(string.Format("{0} is now known as {1}", oldNick, newNick));
        }
        private void IrcKick(string channel, string userName, string userKicked, string kickedMessage)
        {
            Response(string.Format("{0} kicked {1} from {2}: {3}", userName, userKicked, channel, kickedMessage));
        }
        private void IrcQuit(string userName, string Message)
        {
            Response(string.Format("{0} left with message {1}", userName, Message));
        }

        private void EndMotd()
        {
            Response(string.Format("Logging In"));
            _ircBot.WriteRawMessage(string.Format("PRIVMSG NickServ IDENTIFY {0}", _ircBot.UserPassword));
        }
        public override Task OnConnected()
        {
            if (!_eventsSetup)
            {
                AddEvents();
                _eventsSetup = true;
            }
            return base.OnConnected();
        }
    }
}