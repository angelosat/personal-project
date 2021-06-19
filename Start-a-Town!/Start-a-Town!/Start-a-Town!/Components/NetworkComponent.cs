using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    public class NetworkComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Network";
            }
        }

        public override string ToString()
        {
            return ID.ToString() +"\n" + (Net == null ? "null" : Net.ToString());
        }

        public override void MakeChildOf(GameObject parent)
        {
            parent.Network = this;
        }
        public bool Valid;
        public int ID;// { get { return (int)this["ID"]; } set { this["ID"] = value; } }
        public IObjectProvider Net;// { get { return (IObjectProvider)this["Net"]; } set { this["Net"] = value; } }
        public int PlayerID;
        //public NetworkComponent() { this.ID = 0; }
        public NetworkComponent():this(0)
        {
        }
        public NetworkComponent(int id)
        {
            this.ID = id;
            this.Net = null;
        }

        public override object Clone()
        {
            return new NetworkComponent();//this.ID);
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(this.ID);
            writer.Write(this.PlayerID);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.ID = reader.ReadInt32();
            this.PlayerID = reader.ReadInt32();
        }
    }
}
