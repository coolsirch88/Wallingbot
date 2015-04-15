using System;
using IRCBot.Common;

namespace IRCBot.Lib
{
    class Program
    {
        static IRC cIRC;
        static void Main(string[] args)
        {
            cIRC = new IRC();
            cIRC.UserName = "TestWallingbot";
            cIRC.UserPassword = "pass@word1";
            cIRC.Channels.Add("#wallingbot");
            cIRC.eventReceiving += new CommandReceived(IrcCommandReceived);
            cIRC.eventJoin += new Join(IrcJoin);
            cIRC.eventPart += new Part(IrcPart);
            cIRC.eventMode += new Mode(IrcMode);
            cIRC.eventNickChange += new NickChange(IrcNickChange);
            cIRC.eventKick += new Kick(IrcKick);
            cIRC.eventQuit += new Quit(IrcQuit);
            cIRC.eventEndMotd += new IrcEndMotd(EndMotd);
            cIRC.Connect("stark.coldfront.net", 6667, false);

            //var text = Console.ReadLine();
            //var splitText = text.Split(' ');
            //var command = splitText[0];
            //while(command != "quit")
            //{
                //cIRC.WriteMessage(text);
                //text = Console.ReadLine();
                //splitText = text.Split(' ');
                //command = splitText[0];
            //}

            //cIRC.CloseIRC(splitText.Length > 1 ? text.Substring(5) : "Outta here");
            //Console.WriteLine("Terminated");
            //Console.ReadLine();
        }
        static void IrcCommandReceived(string IrcCommand)
        {
            //Console.WriteLine(IrcCommand);
            string[] commandParts = new string[IrcCommand.Split(' ').Length];
            commandParts = IrcCommand.Split(' ');
            if(commandParts[1] == "NOTICE")
            {
                string IrcUser = commandParts[0].Split('!')[0];
                string NoticeText = "";
                for (int intI = 3; intI < commandParts.Length; intI++)
                {
                    NoticeText += commandParts[intI] + " ";
                }
                if(NoticeText.Contains("Password accepted"))
                {
                    foreach(var channel in cIRC.Channels)
                    {
                        cIRC.ConnectChannel(channel);
                    }
                }
            }
        }
        static void IrcJoin(string channel, string userName)
        {
            //Console.WriteLine("{0} Joined {1}", userName, channel);
        }
        static void IrcPart(string channel, string userName)
        {
            //Console.WriteLine("{0} parted {1}", userName, channel);
        }
        static void IrcMode(string channel, string userName, string mode)
        {
            //Console.WriteLine("{0} set mode {1} on {2}", userName, mode, channel);
        }
        static void IrcNickChange(string oldNick, string newNick)
        {
            //Console.WriteLine("{0} is now known as {1}", oldNick, newNick);
        }
        static void IrcKick(string channel, string userName, string userKicked, string kickedMessage)
        {
            //Console.WriteLine("{0} kicked {1} from {2}: {3}", userName, userKicked, channel, kickedMessage);
        }
        static void IrcQuit(string userName, string Message)
        {
            //Console.WriteLine("{0} left with message {1}", userName, Message);
        }

        static void EndMotd()
        {
            //Console.WriteLine("Logging In");
            //cIRC.WriteRawMessage(String.Format("PRIVMSG NickServ IDENTIFY {0}", cIRC.UserPassword));
        }
    }
}
