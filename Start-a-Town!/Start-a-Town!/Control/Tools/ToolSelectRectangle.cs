using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class ToolSelectRectangle : ControlTool
    {
        protected Vector2 Begin;
        protected Rectangle Selection;
        List<GameObject> CurrentSelected;
        public ToolSelectRectangle()
        {

        }
        public ToolSelectRectangle(Vector2 begin)
        {
            this.Begin = begin;
            this.Selection = this.Begin.GetRectangle(this.Begin);
        }

        protected virtual void Select()
        {
            //var cam = Rooms.Ingame.Instance.Camera;
            //var entities = Rooms.Ingame.Instance.Scene.ObjectsDrawn.Where(o=>o.GetBounds(cam).Intersects(this.Selection));
            //UISelectedInfo.Refresh(entities);
            if (this.CurrentSelected != null)
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
                    UISelectedInfo.AddToSelection(this.CurrentSelected);
                else
                    UISelectedInfo.Refresh(this.CurrentSelected);
            }
        }
        public override void Update()
        {
            //this.End = UIManager.Mouse;
            this.Selection = this.Begin.GetRectangle(UIManager.Mouse);
            //var cam = Rooms.Ingame.Instance.Camera;
            //var cam = Net.Client.Instance.Map.Camera;
            var cam = Rooms.Ingame.CurrentMap.Camera;

            this.CurrentSelected = Rooms.Ingame.Instance.Scene.ObjectsDrawn.Where(o => o.GetScreenBounds(cam).Intersects(this.Selection)).ToList();
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Select();
            return Messages.Remove;
        }
        internal override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            //this.Selection.DrawHighlight(sb);
            this.Selection.DrawHighlight(sb);
            if (this.CurrentSelected != null)
                foreach (var obj in this.CurrentSelected)
                    obj.DrawBorder(sb, camera);
        }

        //protected override void WriteData(System.IO.BinaryWriter w)
        //{
        //    w.Write(this.Begin);
        //}
        //protected override void ReadData(System.IO.BinaryReader r)
        //{
        //    this.Begin = r.ReadVector2();
        //}
        internal override ControlTool Read(PlayerData player)
        {
            //return this;
            this.Begin = player.MousePosition;
            return base.Read(player);
        }
        internal override void DrawUIRemote(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, Vector2 vector2, TargetArgs targetArgs, Net.PlayerData player)
        {
            PlayerData.GetMousePosition(player.CameraPosition, this.Begin, player.CameraZoom, camera).GetRectangle(vector2).DrawHighlight(sb, Color.Yellow*.5f);
        }
    }
}