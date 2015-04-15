using System;
using System.Collections.Generic;

namespace IRCBot.Common.Models
{
    public class IRCModel
    {
        public IRCModel()
        {
            Channels = new List<ChannelModel>();
        }
        public string UserName { get; set; }
        public Network Network { get; set; }
        public List<ChannelModel> Channels {get; set;}
    }
}