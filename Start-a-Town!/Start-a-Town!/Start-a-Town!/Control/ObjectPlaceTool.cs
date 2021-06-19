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

namespace Start_a_Town_.PlayerControl
{
    public class ObjectPlaceTool : ControlTool
    {
        public GameObject Object;
        public ObjectPlaceTool()
        {
        }
        public ObjectPlaceTool(GameObject obj)
        {
            Object = obj;

            Icon = obj.GetGui().GetProperty<Icon>("Icon");
        }

        public override Messages OnMouseLeft(bool held)
        {
            GameObject tar;//; = Controller.Instance.MouseoverNext.Object as GameObject;
           // if (tar == null)
            if(!Controller.Instance.Mouseover.TryGet<GameObject>(out tar))
                return Messages.Default;

            Position targetPos = tar.Transform.Position;

            BlockComponent tileComp;
            if (!tar.TryGetComponent<BlockComponent>("Physics", out tileComp))
                return Messages.Default;

            if (Controller.Instance.ksCurrent.IsKeyDown(Keys.LeftControl))
            {
                tileComp.Break(Net.Client.Instance, tar);
                return Messages.Default;
            }

        //    Color faceColor = Controller.Instance.MouseoverNext.Face;// tileComp.GetProperty<Color>("Face");
            Vector3 offSet = Controller.Instance.MouseoverNext.Face;// tileComp.GetProperty<Color>("Face");
           // Vector3 offSet = new Vector3(faceColor.R, faceColor.G, faceColor.B);

            Chunk.AddObject(Object, Engine.Map, targetPos.Global + offSet);
            return Messages.Remove;
        }

        public override Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            Rooms.Ingame.Instance.ToolManager.ActiveTool = null;
            return base.MouseRightDown(e);
        }

        //internal override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        //{
        //    Object.DrawPreview(sb, camera, Target.Global);
        //}
    }
}
