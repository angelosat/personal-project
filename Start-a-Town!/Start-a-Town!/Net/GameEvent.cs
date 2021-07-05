using System;

namespace Start_a_Town_
{
    public class GameEvent : EventArgs
    {
        public double TimeStamp;
        public Components.Message.Types Type;
        public object[] Parameters;
        public IObjectProvider Net;
        public object EventID;
        public GameEvent(IObjectProvider net, double timestamp, Components.Message.Types type, params object[] parameters)
        {
            this.Net = net;
            this.TimeStamp = timestamp;
            this.Type = type;
            this.Parameters = parameters;
        }
        public GameEvent(IObjectProvider net, double timestamp, object id, params object[] parameters)
        {
            this.Net = net;
            this.TimeStamp = timestamp;
            this.EventID = id;
            this.Parameters = parameters;
        }
    }
}
