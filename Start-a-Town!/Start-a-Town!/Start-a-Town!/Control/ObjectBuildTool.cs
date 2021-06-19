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
    public class ObjectBuildTool : ControlTool
    {
        public bool Valid;
        public GameObject.Types Type;
        public GameObject Object;
        public ObjectBuildTool()
        {
        }
        public ObjectBuildTool(GameObject.Types type)
        {
            Type = type;
            Icon = GameObject.Objects[type].GetGui().GetProperty<Icon>("Icon");
        }

        public override Messages OnMouseLeft(bool held)
        {
            
            if (held)
                return Messages.Default;

            GameObject tar;// = Controller.Instance.Mouseover.Object as GameObject;// Next as GameObject;
           // if (tar == null)
           if(! Controller.Instance.MouseoverNext.TryGet<GameObject>(out tar))
                return Messages.Default;


            Position targetPos = tar.GetComponent<MovementComponent>("Position").GetProperty<Position>("Position");
            TileComponent tileComp;
            if (Controller.Instance.ksCurrent.IsKeyDown(Keys.LeftControl))
            {
                if (tar.TryGetComponent<TileComponent>("Physics", out tileComp))
                    tileComp.Break(tar);
                else
                    Map.RemoveObject(tar);
                tar.HandleMessage(null, Message.Types.Remove);
                return Messages.Default;
            }
            if (!Valid)
            {
                //NotificationArea.Write("Invalid Location");
                return Messages.Default;
            }

            if (!tar.TryGetComponent<TileComponent>("Physics", out tileComp))
                return Messages.Default;

            

       //     GameObject obj = Object;//.Clone();
            
            //Color faceColor = Controller.Instance.Mouseover.Face;// tileComp.GetProperty<Color>("Face");
            Vector3 face = Controller.Instance.Mouseover.Face;// tileComp.GetProperty<Color>("Face");

            int rx, ry;
            Camera cam = new Camera(Rooms.Ingame.Instance.Camera.Width, Rooms.Ingame.Instance.Camera.Height);
            cam.Rotation = -Rooms.Ingame.Instance.Camera.Rotation;
            Coords.Rotate(cam, face.X, face.Y, out rx, out ry);
            Vector3 global = tar.Global + new Vector3(rx, ry, face.Z);

            //Vector3 offset = new Vector3(faceColor.R, faceColor.G, faceColor.B);
            //Chunk.AddObject(obj, targetPos.Global + offset);
            //obj["Position"] = new MovementComponent(targetPos.Global + offset);
            GameObject obj = Object;



            SpriteComponent spriteComp = obj.GetComponent<SpriteComponent>("Sprite");
            Sprite sprite = spriteComp.Sprite;
            bool hasOrientations = sprite.SourceRect.First().Length > 1;
            // Console.WriteLine(hasOrientations);
            if (!hasOrientations)
            {
                Chunk.AddObject(obj, global);
                Object = GameObject.Create(Object.ID);
                obj.Initialize().Spawn();
                return Messages.Default; // Messages.Remove;
            }
            else
            {
                obj.GetPosition()["Position"] = new Position(global);
                ObjectOrientationTool orientationControl = new ObjectOrientationTool(obj);
                orientationControl.MouseLeft += new EventHandler<EventArgs>(orientationControl_MouseLeft);
                orientationControl.Removed += new EventHandler<EventArgs>(orientationControl_Removed);
                ToolManager.Instance.ActiveTool = orientationControl;
                //Log.Enqueue(Log.EntryTypes.Default, "Choose orientation");
            }

        }

        void orientationControl_Removed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void orientationControl_MouseLeft(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public override Messages MouseRight(bool held)
        {
            Type = 0;
            Rooms.Ingame.Instance.ToolManager.ActiveTool = null;
            return base.MouseRight(held);
        }

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
            if(Object.Components.ContainsKey("Multi"))
                foreach (Vector3 vector in Object["Multi"].GetProperty<Dictionary<Vector3, Sprite>>("MultiTile").Keys)
                {
                    Vector3 vg = global + vector;
                    Rectangle scrBounds = camera.GetScreenBounds(vg, tileBounds);
                    Vector2 scr = new Vector2(scrBounds.X, scrBounds.Y);
                   // bool check = (Position.GetCell(vg).TileType == Tile.Types.Air && Position.GetCell(vg - new Vector3(0,0,1)).Solid);
                    Cell cell;
                    bool check;
                    if (!Position.TryGetCell(vg, out cell))
                        check = false;
                    else
                        check = (cell.TileType == Tile.Types.Air && Position.GetCell(vg - new Vector3(0, 0, 1)).Solid);
                    Color c = check ? Color.White : Color.Red;
                    Valid &= check;
                    sb.Draw(Map.TerrainSprites, scr, Tile.TileHighlights[0][0], c * 0.5f, 0, new Vector2(0, -tileSprite.Origin.Y + 16), camera.Zoom, SpriteEffects.None, 0);
                }

            //sb.Draw(sprComp.Sprite.Texture, screenLoc,
            //    sprComp.Sprite.SourceRect[sprComp.Variation][sprComp.GetOrientation(camera)], new Color(255, 255, 255, 127),
            int v = (int)Object["Sprite"]["Variation"];
            int o = (int)Object["Sprite"]["Orientation"];
            int oLength = sprite.SourceRect[v].Length;
            sb.Draw(sprite.Texture, screenLoc,
                    sprite.SourceRect[v][SpriteComponent.GetOrientation(o, camera, oLength)], Color.White * 0.5f,//new Color(255, 255, 255, 127),
                    0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);
        }
    }
}
