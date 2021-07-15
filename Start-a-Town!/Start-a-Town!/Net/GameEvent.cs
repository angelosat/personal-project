using System;

namespace Start_a_Town_
{
    public class GameEvent : EventArgs
    {
        public double TimeStamp;
        public Components.Message.Types Type;
        public object[] Parameters;
        public INetwork Net;
        public object EventID;
        public GameEvent(INetwork net, double timestamp, Components.Message.Types type, params object[] parameters)
        {
            this.Net = net;
            this.TimeStamp = timestamp;
            this.Type = type;
            this.Parameters = parameters;
        }
        public GameEvent(INetwork net, double timestamp, object id, params object[] parameters)
        {
            this.Net = net;
            this.TimeStamp = timestamp;
            this.EventID = id;
            this.Parameters = parameters;
        }
    }
}
