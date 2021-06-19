using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Interactions;
using Start_a_Town_.Components;
using Start_a_Town_.Objects;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    /// <summary>
    /// Gives the object the behaviour of a wall tile.
    /// </summary>
    public class WallComponent : Component //TileComponent
    {
       

        //public override void Initialize(GameObject parent)
        //{
        //    MovementComponent posComp;
        //    if (!parent.TryGetComponent<MovementComponent>("Position", out posComp))
        //        return;
        //    Position pos = posComp.GetProperty<Position>("Position");
        //    Vector3 global = posComp.GetProperty<Position>("Position").Global;

        //    Cell cell = pos.GetCell();
        //    cell.Tile = Type;
        //    if (cell.Tile == TileBase.Types.Air)
        //    {
        //        Chunk.Hide(global);
        //        return;
        //    }
        //    int style = parent.GetComponent<SpriteComponent>("Sprite").GetProperty<int>("Variation");
        //    int styleCount = TileBase.TileSprites[Type].SourceRects.GetUpperBound(0);
        //    cell.Variation = (style >= 0) ? style : Map.Instance.Random.Next(styleCount + 1);
        //    int orientation = parent.GetComponent<SpriteComponent>("Sprite").GetProperty<int>("Orientation");

        //    cell.Orientation = (orientation >= 0) ? orientation : Map.Instance.Random.Next(4);
        //    for (int i = 0; i < parent["Physics"].GetProperty<int>("Height"); i++)
        //    {
        //        Position.GetCell(pos.Global + new Vector3(0, 0, i)).Solid = true;
        //    }
        //    Chunk.Show(global);

        //}


        public WallComponent()
            //: base(TileBase.Types.Wall)
        {
            Properties["Height"] = 8;
        }

        //static public WallComponent Create(GameObject obj)
        //{
        //    TileComponent.Create(obj, TileBase.Types.Wall);
        //    return new WallComponent();
        //}

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
        public override bool HandleMessage(GameObject parent, GameObject sender, Message.Types msg)
        {
            switch (msg)
            {
                case Message.Types.Spawn:

                    break;
                case Message.Types.Death:
                    //Break(parent, parent.GetComponent<MovementComponent>("Position").GetProperty<Position>("Position"));
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
