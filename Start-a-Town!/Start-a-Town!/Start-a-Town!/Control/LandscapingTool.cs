using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Rooms;

using Start_a_Town_.Components;

namespace Start_a_Town_.Control
{
    public class LandscapingTool : ControlTool
    {
        public Block.Types Type;
        public LandscapingTool()
        {
            Icon = new Icon(Map.ItemSheet, 0, 32);
        }
        public LandscapingTool(Block.Types type)
        {
            Type = type;

            Icon = new Icon(Map.ItemSheet, 0, 32);
        }

        public override Messages OnMouseLeft(bool held)
        {
            GameObject tar;// = Controller.Instance.MouseoverNext.Object as GameObject;
          //  if (tar == null)
            if (!Controller.Instance.MouseoverNext.TryGet<GameObject>(out tar))
                return Messages.Default;

           // Position pos = Player.Actor.GetComponent<MovementComponent>("Position").CurrentPosition;
            Position targetPos = tar.GetComponent<PositionComponent>("Position").GetProperty<Position>("Position");


                BlockComponent tileComp;
                if (!tar.TryGetComponent<BlockComponent>("Physics", out tileComp))
                    return Messages.Default;

                if (Controller.Instance.ksCurrent.IsKeyDown(Keys.LeftControl))
                {
                    tileComp.Break(tar);
                    return Messages.Default;
                }

                GameObject obj = GameObject.Create(BlockComponent.Blocks[Type].Entity.ID); //GameObject.Create(GameObject.Types.Tile);
                obj.GetComponent<BlockComponent>("Physics").Type = Type;// TileBase.Types.Soil;



                Vector3 offSet;
                //switch (tileComp.Face)
           // switch(tileComp.GetProperty<int>("Face"))
            //switch(Controller.Instance.Mouseover.Face)
            //    {
            //        case 1:
            //            offSet = new Vector3(0, 0, 1);
            //            break;
            //        case 2:
            //            offSet = new Vector3(0, 1, 0);
            //            break;
            //        case 3:
            //            offSet = new Vector3(1, 0, 0);
            //            break;
            //        default:
            //            Console.WriteLine("face selection error");
            //            return Messages.Default;
            //            break;
            //    }
                offSet = Controller.Instance.Mouseover.Face;//.ToVector3();

                obj.AddComponent("Position", new PositionComponent(new Position(Engine.Map, targetPos.Global + offSet)));
                throw new NotImplementedException();
                //obj.Spawn();

                return Messages.Default;

        }

        public override Messages MouseRight(System.Windows.Forms.HandledMouseEventArgs e)
        {
            Type = 0;
            Rooms.Ingame.Instance.ToolManager.ActiveTool = null;
            return base.MouseRight(e);
        }
    }
}
