using System;
using System.Collections.Generic;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI
{
    [Obsolete]
    class PacketDialogueOptions : Packet
    {
        public GameObject Parent, Target;
        public string Text;
        public List<string> DialogOptions = new List<string>();
        public Progress Attention = new Progress();

        public PacketDialogueOptions(GameObject parent, GameObject target, string text, List<string> options)
        {
            this.Parent = parent;
            this.Target = target;
            this.Text = text;
            this.DialogOptions = options;
        }
        public PacketDialogueOptions(IObjectProvider net, BinaryReader r)
        {
            this.Read(net, r);
        }
        public override byte[] Write()
        {
            byte[] data = Network.Serialize(this.Write);
            return data;
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.Parent.RefID);
            w.Write(this.Target.RefID);
            w.Write(this.Text);
            w.Write(this.DialogOptions.Count);
            foreach (var item in this.DialogOptions)
                w.Write(item);
        }
        public override void Read(IObjectProvider net, BinaryReader r)
        {
            this.Parent = net.GetNetworkObject( r.ReadInt32());
            this.Target = net.GetNetworkObject(r.ReadInt32());
            this.Text = r.ReadString();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.DialogOptions.Add(r.ReadString());
            }
        }
    }
}
