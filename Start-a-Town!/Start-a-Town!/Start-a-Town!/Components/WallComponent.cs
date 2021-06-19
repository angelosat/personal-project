using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    /// <summary>
    /// Gives the object the behaviour of a wall tile.
    /// </summary>
    public class WallComponent : BlockComponent
    {


        public WallComponent(int height = 8)
            //: base(TileBase.Types.Wall)
        {
            Properties["Height"] = height;
            this["Solid"] = true;
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk)
        {
            return;
        }

        //static public WallComponent Create(GameObject obj)
        //{
        //    TileComponent.Create(obj, TileBase.Types.Wall);
        //    return new WallComponent();
        //}

        public bool Break(GameObject parent, Position pos)
        {
            return Chunk.RemoveObject(parent);
        }

        public override object Clone()
        {
            WallComponent comp = new WallComponent();
            foreach (KeyValuePair<string, object> property in Properties)
            {
                comp.Properties[property.Key] = property.Value;
            }
            return comp;
        }

        //public override bool Activate(GameObject actor, Objects.StaticObject self)
        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.Death:
                    break;
                case Message.Types.Attack:

                    break;
                case Message.Types.Give:

                    break;
                default:
                    return false;

            }
            return true;
        }




    }
}
