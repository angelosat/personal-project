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
    public class ObjectSpawnTool : ControlTool
    {
        public bool Valid;
        public GameObject.Types Type;
        public GameObject Object;
        public ObjectSpawnTool()
        {
        }
        public ObjectSpawnTool(GameObject.Types type)
        {
            Type = type;
            Icon = GameObject.Objects[type].GetGui().GetProperty<Icon>("Icon");
            //Tool.Type = SelectedItem.ID;
            //Tool.Icon = new Icon(Map.ItemSheet, 0);// StaticObject.Objects[Tool.Type].GetGui().Icon;
            Object = GameObject.Create(type);//.Initialize();// obj.Clone().Initialize();
        }

        //public override Messages OnMouseLeft(bool held)
        public override ControlTool.Messages  MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            if (Target == null)
                return Messages.Default;



            Position targetPos = Target.GetComponent<PositionComponent>("Position").GetProperty<Position>("Position");
           // BlockComponent tileComp;
            if (Controller.Instance.ksCurrent.IsKeyDown(Keys.LeftControl))
            {
                throw new NotImplementedException();
                //Target.Remove();

                return Messages.Default;
            }
            if (!Valid)
            {
                //NotificationArea.Write("Invalid Location");
                return Messages.Default;
            }


            int rx, ry;

            //Coords.Rotate(-ScreenManager.CurrentScreen.Camera.Rotation, face.X, face.Y, out rx, out ry);
            //Vector3 rotatedFace = new Vector3(rx, ry, face.Z);
            Vector3 global = Target.Global + Face;// rotatedFace;
            

            GameObject obj = Object;
            Object = obj.Clone();

            //obj.Global = global;
            throw new Exception("Obsolete position handling");
            obj.Initialize();//.Spawn();
            return Messages.Default;

        }



      //  public override Messages MouseRight(bool held)
        public override ControlTool.Messages MouseRightPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            Type = 0;
            ScreenManager.CurrentScreen.ToolManager.ActiveTool = null;
            return base.MouseRight(e);
        }

        internal override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            if (Controller.Instance.ksCurrent.IsKeyDown(Keys.LeftControl))
                return;

            //GameObject tar;
            //if (!Controller.Instance.Mouseover.TryGet<GameObject>(out tar))
            //    return;
           // Target = Controller.Instance.Mouseover.Object as GameObject;
            if (Target == null)
                return;
            BlockComponent tileComp;
            if (!Target.TryGetComponent<BlockComponent>("Physics", out tileComp))
                return;

          //  Vector3 f = Controller.Instance.Mouseover.Face;
            
            int rx, ry;
            //Camera cam = new Camera(camera.Width, camera.Height);
            //cam.Rotation = -camera.Rotation;
            //Coords.Rotate(cam, f.X, f.Y, out rx, out ry);

            Coords.Rotate(-camera.Rotation, Face.X, Face.Y, out rx, out ry);
            Vector3 global = Target.Global + Face;// new Vector3(rx, ry, Face.Z);

            //Vector3 global = tar.Global + new Vector3(f.R, f.G, f.B);
            Sprite sprite = (Sprite)Object["Sprite"]["Sprite"];
            Sprite tileSprite = Target["Sprite"]["Sprite"] as Sprite;
            Rectangle tileBounds = tileSprite.GetBounds();
            Rectangle bounds = camera.GetScreenBounds(global, sprite.GetBounds()); // posComp.GetScreenBounds(camera, sprComp); // 
            Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
            Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);
            Valid = true;
            int v = (int)Object["Sprite"]["Variation"];
            int o = (int)Object["Sprite"]["Orientation"];
            if (Object.Components.ContainsKey("Multi"))
                MultiTile2Component.DrawPreview(sb, camera, this.Object, global, o);
            //foreach (Vector3 vector in Object["Multi"].GetProperty<Dictionary<Vector3, Sprite>>("MultiTile").Keys)
            //{
            //    Vector3 vg = global + vector;
            //    Rectangle scrBounds = camera.GetScreenBounds(vg, tileBounds);
            //    Vector2 scr = new Vector2(scrBounds.X, scrBounds.Y);
            //    Cell cell;
            //    bool check;
            //    if (!Position.TryGetCell(Engine.Map, vg, out cell))
            //        check = false;
            //    else
            //        check = (cell.TileType == Tile.Types.Air && Position.GetCell(Engine.Map, vg - new Vector3(0, 0, 1)).Solid);
            //    Color c = check ? Color.White : Color.Red;
            //    Valid &= check;
            //    sb.Draw(Map.TerrainSprites, scr, Tile.TileHighlights[0][2], c * 0.5f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
            //}
            else
            {
                int oLength = sprite.SourceRects[v].Length;
                sb.Draw(sprite.Texture, screenLoc,
                        sprite.SourceRects[v][ActorSpriteComponent.GetOrientation(o, camera, oLength)], Color.White * 0.5f,
                        0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
            }
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);
        }

        //public override void HandleInput(InputState e)
        //{
        //    Target = Controller.Instance.Mouseover.Object as GameObject;
        //    Face = Controller.Instance.Mouseover.Face;
        //    base.HandleInput(e);
        //}

        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyValue)
            {
                case (int)System.Windows.Forms.Keys.OemOpenBrackets:
                    ActorSpriteComponent.ChangeOrientation(this.Object);
                    break;
                default:
                    break;
            }
            base.HandleKeyUp(e);
        }

        //public override void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        public override void  Update()
        {
            Target = Controller.Instance.Mouseover.Object as GameObject;
            Face = Controller.Instance.Mouseover.Face;
           // base.HandleMouseMove(e);
        }
    }
}
