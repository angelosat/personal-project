using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class LightComponent : EntityComponent
    {
        public byte Brightness { get { return (byte)this["Brightness"]; } set { this["Brightness"] = value; } }

        public override string ComponentName
        {
            get
            {
                return "Light";
            }
        }
        //public LightComponent(byte brightness)
        //{
        //    this.Brightness = brightness;
        //}
        public LightComponent()
        {
            this.Brightness = 15;
        }
        public LightComponent Initialize(byte brightness)
        {
            this.Brightness = brightness;
            return this;
        }

        public override void OnSpawn(IObjectProvider net, GameObject parent)
        {
            //parent.Global.TrySetLuminance(net, this.Brightness);
            net.Map.SetBlockLuminance(parent.Global, this.Brightness);
        }

        public override void OnDespawn(//IObjectProvider net,
                    GameObject parent)
        {
            //Vector3 global = parent.Global;
            //Map map = parent.Map;

            //parent.Global.TrySetLuminance(net, 0);
            parent.Map.SetBlockLuminance(parent.Global, 0);
            //parent.Global.Round().GetCell(net.Map).Luminance = 0;
            //net.SpreadBlockLight(parent.Global.Round(), 0);

            // todo: use updated lightingengine
            //ChunkLighter.Enqueue(global);
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch(e.Type)
            {
                case Message.Types.EntityMovedCell:
                    Vector3 last = (Vector3)e.Parameters[0];
                    Vector3 next = (Vector3)e.Parameters[1];
                    //next.TrySetLuminance(e.Network, this.Brightness);
                    //last.TrySetLuminance(e.Network, 0);
                    parent.Map.SetBlockLuminance(next, this.Brightness);
                    parent.Map.SetBlockLuminance(last, 0);

                    break;

                default:
                    break;
            }
            return true;
        }

        public override object Clone()
        {
            //return new LightComponent((byte)this["Brightness"]);
            return new LightComponent() { Brightness = this.Brightness };
        }
    }
}
