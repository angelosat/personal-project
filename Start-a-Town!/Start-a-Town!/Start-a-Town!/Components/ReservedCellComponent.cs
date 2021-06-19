using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class ReservedCellComponent : Component
    {
        Block.Types Type { get { return (Block.Types)this["Type"]; } set { this["Type"] = value; } }

        public ReservedCellComponent(Block.Types type = Block.Types.Air)
        {
            this.Type = type;
        }

        public override object Clone()
        {
            return new ReservedCellComponent(Type);
        }

        public override bool HandleMessage(GameObject parent, ObjectLocalEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.SetTile:
                    Type = (Block.Types)e.Parameters[0];
                    parent.PostMessage(Message.Types.SetSprite, parent, new Sprite(Map.TerrainSprites, new Rectangle[][] { new Rectangle[] { Block.TileSprites[Type].SourceRects[0][0] } }, Block.OriginCenter, Block.TileSprites[Type].MouseMap));
               //     parent.HandleMessage(Message.Types.SetShadow, parent, false);
                    return true;
                default:
                    return false;
            }
        }
    }
}
