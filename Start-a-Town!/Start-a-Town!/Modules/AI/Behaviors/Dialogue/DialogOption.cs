using Start_a_Town_.Net;
using System;

namespace Start_a_Town_.AI.Behaviors
{
    [Obsolete]
    public class DialogOption
    {
        public string Value;
        public GameObject Target;
        public DialogOption(string value, GameObject target)
        {
            this.Value = value;
            this.Target = target;
        }

        static public void Select(GameObject speaker, GameObject target, string text)
        {
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
