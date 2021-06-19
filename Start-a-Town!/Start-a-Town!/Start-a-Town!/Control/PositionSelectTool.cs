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
    public class PositionSelectTool : ControlTool
    {
        //  public event EventHandler<EventArgs> MouseLeft;

        public Vector3 Global;
        public bool Valid;
        public GameObject.Types Type;
        public GameObject Object, TargetObject;
        public PositionSelectTool()
        {
        }
        public PositionSelectTool(GameObject.Types type)
        {
            Type = type;
            Icon = GameObject.Objects[type].GetGui().GetProperty<Icon>("Icon");
        }

        public override Messages OnMouseLeft(bool held)
        {
            if (held)
                return Messages.Default;

         //   GameObject tar;
            if (!Controller.Instance.MouseoverNext.TryGet<GameObject>(out TargetObject))
                return Messages.Default;

            BlockComponent tileComp;
            if (!TargetObject.TryGetComponent<BlockComponent>("Physics", out tileComp))
                return Messages.Default;

            Global = TargetObject.Global + Controller.Instance.Mouseover.Face;

            //if (MouseLeft != null)
            //    MouseLeft(this, EventArgs.Empty);
            base.OnMouseLeft(held);
            return Messages.Default;
        }

        //void orientationControl_Removed(object sender, EventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        //void orientationControl_MouseLeft(object sender, EventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        public override Messages MouseRight(bool held)
        {
            Type = 0;
            Rooms.Ingame.Instance.ToolManager.ActiveTool = null;
            return base.MouseRight(held);
        }

        // TODO: pass map as parameter
        internal override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            if (Controller.Instance.ksCurrent.IsKeyDown(Keys.LeftControl))
                return;

            GameObject tar;
            if (!Controller.Instance.Mouseover.TryGet<GameObject>(out tar))
                return;

            Vector3 f = Controller.Instance.Mouseover.Face;
            int rx, ry;
            Camera cam = new Camera(camera.Width, camera.Height);
            cam.Rotation = -camera.Rotation;
            Coords.Rotate(cam, f.X, f.Y, out rx, out ry);
            Vector3 global = tar.Global + new Vector3(rx, ry, f.Z);
            //Vector3 global = tar.Global + new Vector3(f.R, f.G, f.B);
            Sprite sprite = (Sprite)Object["Sprite"]["Sprite"];
            Sprite tileSprite = tar["Sprite"]["Sprite"] as Sprite;
            Rectangle tileBounds = tileSprite.GetBounds();
            Rectangle bounds = camera.GetScreenBounds(global, sprite.GetBounds()); // posComp.GetScreenBounds(camera, sprComp); // 
            Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
            Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);
            Valid = true;
            if (Object.Components.ContainsKey("Multi"))
                foreach (Vector3 vector in Object["Multi"].GetProperty<Dictionary<Vector3, Sprite>>("MultiTile").Keys)
                {
                    Vector3 vg = global + vector;
                    Rectangle scrBounds = camera.GetScreenBounds(vg, tileBounds);
                    Vector2 scr = new Vector2(scrBounds.X, scrBounds.Y);
                    // bool check = (Position.GetCell(vg).TileType == Tile.Types.Air && Position.GetCell(vg - new Vector3(0,0,1)).Solid);
                    Cell cell;
                    bool check;
                    if (!Position.TryGetCell(Engine.Map, vg, out cell))
                        check = false;
                    else
                        check = (cell.Type == Block.Types.Air && Position.GetCell(Engine.Map, vg - new Vector3(0, 0, 1)).Solid);
                    Color c = check ? Color.White : Color.Red;
                    Valid &= check;
                    //  sb.Draw(Map.TerrainSprites, scr, Tile.TileHighlights[0][0], c * 0.5f, 0, new Vector2(0, -tileSprite.Origin.Y + 16), camera.Zoom, SpriteEffects.None, 0);
                }

            //sb.Draw(sprComp.Sprite.Texture, screenLoc,
            //    sprComp.Sprite.SourceRect[sprComp.Variation][sprComp.GetOrientation(camera)], new Color(255, 255, 255, 127),
            int v = (int)Object["Sprite"]["Variation"];
            int o = (int)Object["Sprite"]["Orientation"];
            int oLength = sprite.SourceRects[v].Length;
            MultiTile2Component multi;
            if (Object.TryGetComponent<MultiTile2Component>("Multi", out multi))
                multi.DrawPreview(sb, camera, global, (int)Object["Sprite"]["Orientation"]);
            else
            {
                SpriteComponent spriteComp;
                if (Object.TryGetComponent<SpriteComponent>("Sprite", out spriteComp))
                    spriteComp.DrawPreview(sb, camera, global.Round(), (int)Object["Sprite"]["Orientation"]);
            }
            //sb.Draw(sprite.Texture, screenLoc,
            //        sprite.SourceRect[v][SpriteComponent.GetOrientation(o, camera, oLength)], Color.White * 0.5f,//new Color(255, 255, 255, 127),
            //        0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);

            if (!SpriteComponent.HasOrientation(Object))
                return;
            string
                text = "Press 'Q' to change orientation";
            Vector2
                textSize = UI.UIManager.Font.MeasureString(text);
            UI.UIManager.DrawStringOutlined(sb, text,
                new Vector2(UI.UIManager.Width / 2 - (int)(textSize.X / 2), UI.UIManager.Height / 4), Vector2.Zero);

        }
    }
}
