using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components
{
    public class MultiTileComponent : Component
    {
        
        //public Vector3 Offset = Vector3.Zero;
        //double OffsetTimer;
       // Dictionary<Vector3, Sprite> _MultiTile = new Dictionary<Vector3, Sprite>();
        static public void DrawShadows(SpriteBatch sb)
        {
            if (ShadowsEnabled)
                foreach (Shadow shadow in ShadowList)
                    shadow.Draw(sb);
            ShadowList.Clear();
        }



        static public bool ShadowsEnabled = true;
        static List<Shadow> ShadowList = new List<Shadow>();
     //   static List<GameObject> PreviewList = new List<GameObject>();
        //public Sprite Sprite;
        //public int Variation, Orientation;

        public Dictionary<Vector3, Sprite> MultiTile { get { return GetProperty<Dictionary<Vector3, Sprite>>("MultiTile"); } set { Properties["MultiTile"] = value; } }
        public Sprite Sprite { get { return GetProperty<Sprite>("Sprite"); } set { Properties["Sprite"] = value; } }
        // TODO: variation and orientation should not be part of graphics (variation => infocomponent, orientation => position)
        public int Variation { get { return GetProperty<int>("Variation"); } set { Properties["Variation"] = value; } }
        public int Orientation { get { return GetProperty<int>("Orientation"); } set { Properties["Orientation"] = value; } }
        public Vector3 Offset { get { return (Vector3)this["Offset"]; } set { this["Offset"] = value; } }
        public double OffsetTimer { get { return (double)this["OffsetTimer"]; } set { this["OffsetTimer"] = value; } }
        public bool Flash { get { return (bool)this["Flash"]; } set { this["Flash"] = value; } }

        public MultiTileComponent()
        {
            Properties["Hidden"] = false;
            Properties.Add("Flash", false);
            Properties.Add("Offset", Vector3.Zero);
            Properties.Add("OffsetTimer", 1d);
            Properties.Add("Sprite");
            Properties.Add("Variation");
            Properties.Add("Orientation");
            Properties.Add("MultiTile");
        }


        public MultiTileComponent(Sprite fullSprite, Dictionary<Vector3, Sprite> sprites, int variation = 0, int orientation = 0)
            : this()
        {
            Sprite = fullSprite;
            MultiTile = sprites;
            Variation = (byte)variation;
            Orientation = (byte)orientation;
        }

        public Vector3 GetOffset()
        {
            double t = Math.Sin(OffsetTimer * 2 * Math.PI);
            return (float)t * Offset;
        }

        public override void Draw(
            SpriteBatch sb, DrawObjectArgs a
            )
        {
            foreach (KeyValuePair<Vector3, Sprite> multi in MultiTile)
                DrawMulti(sb, a.Camera, a.Controller, a.Player, a.Map, a.Object, a.Depth, multi.Key, multi.Value);
         //   DrawMulti(sb, a.Camera, a.Controller, a.Player, a.Map, a.Object, a.Depth, Vector3.Zero, MultiTile[Vector3.Zero]);
            Color face;
            if (HitTest(a.Bounds, Sprite.SourceRect.First().First(), a.Camera, out face))
            {
                //  DrawHighlight(sb, camera, bounds);
                if (Sprite.MouseMap.Multifaceted)
                {
                    a.Controller.MouseoverNext.TrySet(Cell.GetGlobalDepth(a.Cell.GetGlobalCoords(a.Chunk)), a.Object, face);
                    return;
                }

                TileSprite tileSprite = Tile.TileSprites[Tile.Types.Sand];
                //  Position pos = posComp.GetProperty<Position>("Position");
                //  Rectangle cellBounds = camera.GetScreenBounds(pos.Global.X + part.X, pos.Global.Y + part.Y, pos.Global.Z + part.Z, tileSprite.GetBounds());
                Rectangle cellBounds = a.Camera.GetScreenBounds(a.Object.Global, tileSprite.GetBounds());
             //   if (Tile.HitTest(tileSprite, cellBounds, a.Camera, a.Controller, out face))
                    a.Controller.MouseoverNext.TrySet(Cell.GetGlobalDepth(a.Cell.GetGlobalCoords(a.Chunk)), a.Object, face);
                //  else
                //      color = Color.Lerp(color, Color.Transparent, 0.8f);
            }
        }

        public void DrawMulti(
            SpriteBatch sb,
            Camera camera,
            Controller controller,
            Player player,
            Map map,
            GameObject obj,
            float depth,
            Vector3 part,
            Sprite sprite
            )
        {
            // MovementComponent posComp = (MovementComponent)obj["Position"];
            Chunk chunk; Cell cell;
            //int rpx, rpy;
            //Coords.Rotate(camera, part.X, part.Y, out rpx, out rpy);
            //part = new Vector3(rpx, rpy, part.Z);
            //Console.WriteLine(part);
            Vector3 global = obj.Global + part;
            Rectangle bounds = camera.GetScreenBounds(global, sprite.GetBounds());
            Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
            Position.TryGet(global, out cell, out chunk);
            byte sunlight = Math.Max((byte)(chunk.GetSunlight(cell.LocalCoords) - map.SkyDarkness), chunk.GetCellLight(cell.LocalCoords));
            Color color = new Color((sunlight + 1) / 16f, 0, 0);

            Vector3 off = GetOffset();
            if (camera.CullingCheck(global.X + off.X, global.Y + off.Y, global.Z + off.Z, new Rectangle((int)-sprite.Origin.X, (int)-sprite.Origin.Y, sprite[0, 0].Width, sprite[0, 0].Height), out bounds))//  Sprite.GetBounds(), out bounds))
                screenLoc = new Vector2(bounds.X, bounds.Y);
            // TODO: slow?
            if (OffsetTimer < 1)
                OffsetTimer += GlobalVars.DeltaTime / 10f;
            else
                Offset = Vector3.Zero;

            // TODO: fix this crap
            //int orientation = (Orientation + (int)camera.Rotation) % Sprite.SourceRect[Variation].Length;
            int orientation = GetOrientation(camera, sprite);
         //   Console.WriteLine(orientation);
            //camera.SpriteBatch.Draw(Sprite.Texture, screenLoc, Sprite.SourceRect[Variation][orientation], color, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);

            // TODO: checking for speed is slow
            Rectangle source = sprite.SourceRect[Variation][orientation];
            Vector4 shaderRect = new Vector4(source.X / (float)sprite.Texture.Width, source.Y / (float)sprite.Texture.Height, source.Width / (float)sprite.Texture.Width, source.Height / (float)sprite.Texture.Height);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);

            //  float depth = Cell.GetGlobalDepth(global);

            if (Flash)
            {
                Game1.Instance.Effect.Parameters["Overlay"].SetValue(new Vector4(10, 0, 0, 0.5f));
                sb.Draw(Sprite.Texture, screenLoc, source, color, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);
                Game1.Instance.Effect.Parameters["Overlay"].SetValue(new Vector4(1, 1, 1, 1));
                Flash = false;
            }
            else
                sb.Draw(sprite.Texture, screenLoc, source, color, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);

            if (SpriteComponent.ShadowsEnabled)
                SpriteComponent.DrawShadow(sb, camera, Sprite.GetBounds(), global, depth); // TODO: cache sprite.getbounds

            //Color face;
            //if (!HitTest(bounds, sprite.SourceRect.First().First(), camera, out face))
            //    return;
            ////  DrawHighlight(sb, camera, bounds);
            //if (Sprite.MouseMap.Multifaceted)
            //{
            //    controller.MouseoverNext.TrySet(Cell.GetGlobalDepth(cell.GetGlobalCoords(chunk)), obj, face);
            //    return;
            //}

            //TileSprite tileSprite = Tile.TileSprites[Tile.Types.Sand];

            //Rectangle cellBounds = camera.GetScreenBounds(global, tileSprite.GetBounds());
            //if (Tile.HitTest(tileSprite, cellBounds, camera, controller, out face))
            //    controller.MouseoverNext.TrySet(Cell.GetGlobalDepth(cell.GetGlobalCoords(chunk)), obj, face);
        }

        public int GetOrientation(Camera camera)
        {
            int orientation = (Orientation - (int)camera.Rotation) % Sprite.SourceRect[Variation].Length;
            orientation = orientation < 0 ? Sprite.SourceRect[Variation].Length + orientation : orientation;
            return orientation;
        }

        public int GetOrientation(Camera camera, Sprite sprite)
        {
            int orientation = (Orientation - (int)camera.Rotation) % sprite.SourceRect[Variation].Length;
            orientation = orientation < 0 ? sprite.SourceRect[Variation].Length + orientation : orientation;
            return orientation;
        }

        static public int GetOrientation(int orientation, Camera camera)
        {
            int o = (orientation - (int)camera.Rotation) % 4;
            return o < 0 ? 4 + o : o;
        }

        public static void DrawShadow(SpriteBatch sb, Camera camera, Rectangle spriteBounds, MovementComponent posComp, float depth)
        {
            Vector3 global = posComp.GetProperty<Position>("Position").Global;
           // Rectangle spriteBounds = sprite.GetBounds();
            int n = (int)Position.Round(global).Z;
            bool drawn = false;
            while (n >= 0 && !drawn)
            {
                Cell cellShadow;
                if (Position.TryGetCell(new Vector3(global.X, global.Y, n), out cellShadow))
                {
                    //if (cellShadow.Solid)
                    if(cellShadow.TileType != Tile.Types.Air)
                    {
                        Rectangle shadowBounds;
                        //if (camera.CullingCheck(global.X, global.Y, n, new Rectangle(-16, -8, 32, 16), out shadowBounds))
                        if (camera.CullingCheck(global.X, global.Y, n, new Rectangle(-spriteBounds.Width / 2, -spriteBounds.Width / 4, spriteBounds.Width, spriteBounds.Width / 2), out shadowBounds))
                        {
                            
                            //Chunk chunk;
                            //Position.TryGetChunk(global, out chunk);
                            ////float localdNear, localdFar;
                            ////chunk.GetLocalDepthRange(camera, Map.Instance, out localdNear, out localdFar);
                            ////float cd = (cellShadow.X + cellShadow.Y + cellShadow.Z) / (float)(Chunk.Size + Chunk.Size - 3 + Map.MaxHeight);
                            ////cd = 1 - (localdFar + cd * (localdNear - localdFar));

                            //int rx, ry;
                            //Coords.Rotate(camera, cellShadow.X, cellShadow.Y, out rx, out ry);
                            //Vector3 rotated = new Vector3(rx, ry, cellShadow.Z + 1);
                            //float dmax = Chunk.DepthMax();
                            //float cd = 1 - Position.GetDepth(rotated) / dmax;
                            //float cd = 1 - Position.GetDepth(rotated + new Vector3(0, 0, spriteBounds.Width / 4)) / dmax;
                            //float _cd = 1 - Position.GetDepth(rotated + new Vector3(0, 0, -spriteBounds.Width / 4)) / dmax; 
                            //Game1.Instance.Effect.Parameters["ObjectHeight"].SetValue(_cd);//1 - Position.GetDepth(rotated + Vector3.Forward) / dmax);
                            //Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);
                            //sb.Draw(Map.Shadow, shadowBounds, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, cd);
                            //Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);

                            ShadowList.Add(new Shadow(shadowBounds, depth));
                        }
                        drawn = true;
                    }
                }
                n--;
            }
        }


        //public static void DrawPreview(GameObject obj)
        //{
        //    PreviewList.Add(obj);
        //}

        public void DrawHighlight(SpriteBatch sb, Camera camera, Rectangle bounds)
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.None, 0);
            //camera.SpriteBatch.Draw(UI.UIManager.Highlight, new Vector2(bounds.X, bounds.Y), null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        protected bool HitTest(Rectangle bounds, Rectangle src, Camera camera, out Color face)
        {
            if (bounds.Intersects(Controller.Instance.MouseRect))
            {
                int xx = (int)((Controller.Instance.msCurrent.X - bounds.X) / (float)camera.Zoom);
                int yy = (int)((Controller.Instance.msCurrent.Y - bounds.Y) / (float)camera.Zoom);

                //do a hit test with tha actual spirte first, before the mousemap. cause the mousemap might be bigger (as is the case with the walls)
                //Color alphaTest;
                //if (!Sprite.AlphaMap.HitTest(xx, yy, out alphaTest))
                //{
                //    face = alphaTest;
                //    return false;
                //}

                //if (Sprite.MouseMap == null)
                //{
                //    face = alphaTest;
                //    return true;
                //}
               // Rectangle src = Sprite.SourceRect[0][0];
                Color color = Sprite.MouseMap.Map[yy * src.Width + xx];
                //Color color = Sprite.MouseMap.Map[yy * Sprite.MouseMap.Texture.Width + xx];
                face = color;
                if (color.A > 0)
                {
                    // TODO: have the color array generated on creation of sprite and cache it
                    Color[] spriteMap = new Color[src.Width * src.Height];
                    Sprite.Texture.GetData(0, src, spriteMap, 0, src.Width * src.Height);
                    Color c = spriteMap[yy * src.Width + xx];
                    if (c.A == 0)
                    {
                        return false;
                    }

                    return true;
                }
            }
            face = Color.Transparent;
            return false;
        }

        public override bool HandleMessage(GameObject parent, GameObject sender, Message.Types msg)
        {
            if (msg == Message.Types.Attack)
            {
                Offset = parent.Global - sender.Global;
                Offset.Normalize();
                Offset /= 4;// (float)Tile.Depth;
                OffsetTimer = 0.25f;
                Flash = true;
            }
            return true;
        }

        public override object Clone()
        {
            MultiTileComponent spr = new MultiTileComponent(Sprite, MultiTile);
            //foreach (KeyValuePair<string, object> property in Properties)
            //{
            //    spr.Properties[property.Key] = property.Value;
            //}
            foreach (KeyValuePair<string, object> property in Properties)
            {
                spr[property.Key] = property.Value;
            }
            return spr;
        }

        internal override List<Tag> Save()
        {
            return new List<Tag>() { 
                new Tag(Tag.Types.Int, "Variation", (int)Variation),
                new Tag(Tag.Types.Int, "Orientation", (int)Orientation),
            };
        }

        internal override void Load(Tag compTag)
        {
           // List<Tag> data = compTag.Value as List<Tag>;
            //foreach (Tag tag in data)
            //{
            //    if (tag.Value != null)
            //        this.Properties[tag.Name] = tag.Value;
            //}
            this.Properties["Variation"] = (int)compTag["Variation"].Value;
            this.Properties["Orientation"] = (int)compTag["Orientation"].Value;
        }
    }
}
