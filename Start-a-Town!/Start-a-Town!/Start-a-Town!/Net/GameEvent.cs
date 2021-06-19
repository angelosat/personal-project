using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class GameEvent : EventArgs
    {
        public double TimeStamp { get; set; }
        public Components.Message.Types Type { get; set; }
        public object[] Parameters { get; set; }
        public IObjectProvider Net { get; set; }

        public GameEvent(IObjectProvider net, double timestamp, Components.Message.Types type, params object[] parameters)
        {
            this.Net = net;
            this.TimeStamp = timestamp;
            this.Type = type;
            this.Parameters = parameters;
        }
    }
}
