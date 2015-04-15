using System;
using System.Collections.Generic;

namespace IRCBot.Common
{
    public delegate void CommandReceived(string IrcCommand);
    public delegate void Join(string Channel, string User);
    public delegate void Part(string Channel, string User);
    public delegate void Mode(string Channel, string User, string UserMode);
    public delegate void NickChange(string UserOldNick, string UserNewNick);
    public delegate void Kick(string Channel, string UserKicker, string UserKicked, string KickMessage);
    public delegate void Quit(string UserQuit, string QuitMessage);
    public delegate void IrcEndMotd();
    public interface IIRCBot
    {
        List<string> Channels { get; set; }
        int Port { get; set; }
        bool Secure { get; set; }
        string Server { get; set; }
        string UserName { get; set; }
        string UserPassword { get; set; }
        bool Connected { get; }

        event IrcEndMotd eventEndMotd;
        event Join eventJoin;
        event Kick eventKick;
        event Mode eventMode;
        event NickChange eventNickChange;
        event Part eventPart;
        event Quit eventQuit;
        event CommandReceived eventReceiving;

        void CloseIRC(string message = "Outta this place");
        void Connect();
        void Connect(string server, int port, bool secure);
        void ConnectChannel(string chanName);
        void WriteMessage(string message);
        void WriteRawMessage(string message);
    }
}
