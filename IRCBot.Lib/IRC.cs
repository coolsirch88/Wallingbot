using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using IRCBot.Common;

namespace IRCBot.Lib
{
    public class IRC : IIRCBot
    {
        private TcpClient _ircConnection;
        private NetworkStream _ircStream;
        private StreamReader _ircInput;
        private StreamWriter _ircOutput;
        private volatile bool _stopIrc = false;
        private Thread _workerThread;
        public string Server { get; set; }
        public int Port { get; set; }
        public bool Secure { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public List<string> Channels { get; set; }
        public bool Connected { get { return _ircConnection != null && _ircConnection.Connected; } }
        public event CommandReceived eventReceiving;
        public event Join eventJoin;
        public event Part eventPart;
        public event Mode eventMode;
        public event NickChange eventNickChange;
        public event Kick eventKick;
        public event Quit eventQuit;
        public event IrcEndMotd eventEndMotd;
        public IRC()
        { 
            Channels = new List<string>();
        }

        public void Connect()
        {
            _stopIrc = false;
            // Connect with the IRC server.
            if (this.Secure)
            {
                this.Server = "https://" + this.Server;
            }
            _ircConnection = new TcpClient(this.Server, this.Port);
            _ircStream = _ircConnection.GetStream();
            _ircInput = new StreamReader(_ircStream);
            _ircOutput = new StreamWriter(_ircStream);

            // Authenticate our user
            _ircOutput.WriteLine(String.Format("USER {0} {1} * :{2}", this.UserName, 8, "wallingbot"));
            _ircOutput.Flush();
            _ircOutput.WriteLine(String.Format("NICK {0}", this.UserName));
            _ircOutput.Flush();
            _workerThread = new Thread(ReadIRCLoop);
            _workerThread.Start();
        }

        private void ReadIRCLoop()
        {
            while (!_stopIrc)
            {
                string ircCommand;
                while (_ircConnection.Connected && (ircCommand = _ircInput.ReadLine()) != null && !_stopIrc)
                {
                    if (eventReceiving != null) { this.eventReceiving(ircCommand); }

                    string[] commandParts = new string[ircCommand.Split(' ').Length];
                    commandParts = ircCommand.Split(' ');
                    if (commandParts[0].Substring(0, 1) == ":")
                    {
                        commandParts[0] = commandParts[0].Remove(0, 1);
                    }

                    if (commandParts[0] == this.Server)
                    {
                        // Server message
                        switch (commandParts[1])
                        {
                            case "332": /*this.IrcTopic(commandParts);*/ break;
                            case "333": /*this.IrcTopicOwner(commandParts);*/ break;
                            case "353": /*this.IrcNamesList(commandParts);*/ break;
                            case "366": /*this.IrcEndNamesList(commandParts);*/ break;
                            case "372": /*this.IrcMOTD(commandParts);*/ break;
                            case "376": this.IrcEndMOTD(commandParts); break;
                            default: /*this.IrcServerMessage(commandParts);*/ break;
                        }
                    }
                    else if (commandParts[0] == "PING")
                    {
                        // Server PING, send PONG back
                        this.IrcPing(commandParts);
                    }
                    else
                    {
                        // Normal message
                        string commandAction = commandParts[1];
                        switch (commandAction)
                        {
                            case "JOIN": this.IrcJoin(commandParts); break;
                            case "PART": this.IrcPart(commandParts); break;
                            case "MODE": this.IrcMode(commandParts); break;
                            case "NICK": this.IrcNickChange(commandParts); break;
                            case "KICK": this.IrcKick(commandParts); break;
                            case "QUIT": this.IrcQuit(commandParts); break;
                        }
                    }
                }
            }
        }

        public void Connect(string server, int port, bool secure)
        {
            this.Server = server;
            this.Port = port;
            this.Secure = secure;
            Connect();
        }

        public void ConnectChannel(string chanName)
        {
            if (!Channels.Contains(chanName))
            {
                Channels.Add(chanName);
            }
            _ircOutput.WriteLine(String.Format("JOIN {0}", chanName));
            _ircOutput.Flush();
        }

        private void IrcEndMOTD(string[] IrcCommand)
        {
            if (eventEndMotd != null) { this.eventEndMotd(); };
        }
        private void IrcPing(string[] IrcCommand)
        {
            string PingHash = "";
            for (int intI = 1; intI < IrcCommand.Length; intI++)
            {
                PingHash += IrcCommand[intI] + " ";
            }
            _ircOutput.WriteLine("PONG " + PingHash);
            _ircOutput.Flush();
        }

        #region User Messages
        private void IrcJoin(string[] IrcCommand)
        {
            string IrcChannel = IrcCommand[2];
            string IrcUser = IrcCommand[0].Split('!')[0];
            if (eventJoin != null) { this.eventJoin(IrcChannel.Remove(0, 1), IrcUser); }
        } /* IrcJoin */

        private void IrcPart(string[] IrcCommand)
        {
            string IrcChannel = IrcCommand[2];
            string IrcUser = IrcCommand[0].Split('!')[0];
            if (eventPart != null) { this.eventPart(IrcChannel, IrcUser); }
        } /* IrcPart */

        private void IrcMode(string[] IrcCommand)
        {
            string IrcChannel = IrcCommand[2];
            string IrcUser = IrcCommand[0].Split('!')[0];
            string UserMode = "";
            for (int intI = 3; intI < IrcCommand.Length; intI++)
            {
                UserMode += IrcCommand[intI] + " ";
            }
            if (UserMode.Substring(0, 1) == ":")
            {
                UserMode = UserMode.Remove(0, 1);
            }
            if (eventMode != null) { this.eventMode(IrcChannel, IrcUser, UserMode.Trim()); }
        } /* IrcMode */

        private void IrcNickChange(string[] IrcCommand)
        {
            string UserOldNick = IrcCommand[0].Split('!')[0];
            string UserNewNick = IrcCommand[2].Remove(0, 1);
            if (eventNickChange != null) { this.eventNickChange(UserOldNick, UserNewNick); }
        } /* IrcNickChange */

        private void IrcKick(string[] IrcCommand)
        {
            string UserKicker = IrcCommand[0].Split('!')[0];
            string UserKicked = IrcCommand[3];
            string IrcChannel = IrcCommand[2];
            string KickMessage = "";
            for (int intI = 4; intI < IrcCommand.Length; intI++)
            {
                KickMessage += IrcCommand[intI] + " ";
            }
            if (eventKick != null) { this.eventKick(IrcChannel, UserKicker, UserKicked, KickMessage.Remove(0, 1).Trim()); }
        } /* IrcKick */

        private void IrcQuit(string[] IrcCommand)
        {
            string UserQuit = IrcCommand[0].Split('!')[0];
            string QuitMessage = "";
            string Channel = IrcCommand[1];
            for (int intI = 2; intI < IrcCommand.Length; intI++)
            {
                QuitMessage += IrcCommand[intI] + " ";
            }
            if (eventQuit != null) { this.eventQuit(UserQuit, QuitMessage.Remove(0, 1).Trim()); }
        } /* IrcQuit */
        #endregion

        public void WriteRawMessage(string message)
        {
            _ircOutput.WriteLine(message);
            _ircOutput.Flush();
        }

        public void WriteMessage(string message)
        {
            _ircOutput.WriteLine("PRIVMSG {0} {1}", Channels[0], message);
            _ircOutput.Flush();
        }

        public void CloseIRC(string message = "Outta this place")
        {
            _stopIrc = true;
            _ircOutput.WriteLine("QUIT {0}", message);
            _ircOutput.Flush();
            _workerThread.Join();
        }
    }
}
