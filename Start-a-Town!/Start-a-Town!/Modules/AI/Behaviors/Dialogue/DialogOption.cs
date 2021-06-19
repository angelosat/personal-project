using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.AI;

namespace Start_a_Town_.AI.Behaviors
{
    public class DialogOption
    {
        public string Value;
        public GameObject Target;
        public DialogOption(string value, GameObject target)
        {
            this.Value = value;
            this.Target = target;
        }

        //public void Select(GameObject speaker)
        //{
        //    // net send selection
        //    byte[] data = Network.Serialize(w =>
        //    {
        //        w.Write((int)AIPacketHandler.Channels.DialogueOption);
        //        w.Write(speaker.InstanceID);
        //        w.Write(this.Target.InstanceID);
        //        w.Write(this.Value);
        //    });
        //    Client.Instance.Send(PacketType.AI, data);
        //}
        static public void Select(GameObject speaker, GameObject target, string text)
        {
            // net send selection
            byte[] data = Network.Serialize(w =>
            {
                w.Write((int)AIPacketHandler.Channels.DialogueOption);
                w.Write(speaker.RefID);
                w.Write(target.RefID);
                w.Write(text);
            });
            Client.Instance.Send(PacketType.AI, data);
        }
    }
}
