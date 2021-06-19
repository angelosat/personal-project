using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components
{

    public class SpriteComponent : Component
    {
        static public void DrawShadows(SpriteBatch sb, Map map, Camera camera)
        {
            if (ShadowsEnabled)
                foreach (Shadow shadow in ShadowList.OrderBy(foo => -Cell.GetGlobalDepthNew(foo.Global, camera)))
                    shadow.Draw(sb, map,  camera);
            ShadowList.Clear();
        }
        //static public void DrawPreview(SpriteBatch sb, Camera camera)
        //{
        //    foreach (GameObject obj in PreviewList)
        //    {
        //        MovementComponent posComp;
        //        SpriteComponent sprComp = (SpriteComponent)obj["Sprite"];
        //        // TODO: this is also at the chunk.drawobjects
        //        if (obj.TryGetComponent<MovementComponent>("Position", out posComp))
        //        {
        //            Rectangle bounds = posComp.GetScreenBounds(camera, sprComp); // camera.GetScreenBounds((int)Start.X + cellX, (int)Start.Y + cellY, cellZ, spriteComp.Sprite.GetBounds());
        //            Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
        //            Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);

        //            sb.Draw(sprComp.Sprite.Texture, screenLoc,
        //                sprComp.Sprite.SourceRect[sprComp.Variation][sprComp.Orientation], new Color(255, 255, 255, 127),
        //                0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 1);

        //            Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);
        //        }
        //    }
        //    PreviewList.Clear();
        //}


        static public bool ShadowsEnabled = true;
        static List<Shadow> ShadowList = new List<Shadow>();
     //   static List<GameObject> PreviewList = new List<GameObject>();
        //public Sprite Sprite;
        //public int Variation, Orientation;

        public Sprite Sprite { get { return GetProperty<Sprite>("Sprite"); } set { Properties["Sprite"] = value; } }
        // TODO: variation and orientation should not be part of graphics (variation => infocomponent, orientation => position)
        public int Variation { get { return GetProperty<int>("Variation"); } set { Properties["Variation"] = value; } }
        public int Orientation { get { return GetProperty<int>("Orientation"); } set { Properties["Orientation"] = value; } }
        public bool Flash { get { return (bool)this["Flash"]; } set { this["Flash"] = value; } }
        public Vector3 Offset { get { return (Vector3)this["Offset"]; } set { this["Offset"] = value; } }
        public double OffsetTimer { get { return (double)this["OffsetTimer"]; } set { this["OffsetTimer"] = value; } }
        public bool Shadow { get { return (bool)this["Shadow"]; } set { this["Shadow"] = value; } }
        public float Alpha { get { return (float)this["Alpha"]; } set { this["Alpha"] = value; } }
        public bool Hidden { get { return (bool)this["Hidden"]; } set { this["Hidden"] = value; } }

        public SpriteComponent()
        {
            this.Alpha = 1;
            Properties["Hidden"] = false;
            Properties.Add("Flash", false);
            Properties.Add("Offset", Vector3.Zero);
            Properties.Add("OffsetTimer", 1d);
            Properties.Add("Sprite");
            Properties.Add("Variation");
            Properties.Add("Orientation");
            this["Shadow"] = true;
     //       Properties.Add("Face", Color.Transparent);
        }

        public SpriteComponent(Texture2D texture, Rectangle[][] sourcerect, Vector2 origin, MouseMap mousemap = null)
            : this()
        {
            Sprite = new Sprite(texture, sourcerect, origin, mousemap);
            Variation = 0;
            Orientation = 0;

            //Texture = texture;
            //SourceRect = sourcerect;
            //Origin = origin;
            //MouseMap = new Color[SourceRect.Width * SourceRect.Height];
            //Texture.GetData(0, SourceRect, MouseMap, 0, SourceRect.Width * SourceRect.Height);
        }

        public SpriteComponent(Sprite sprite, int variation = 0, int orientation = 0)
            : this()
        {
            Sprite = sprite;
            Variation = (byte)variation;
            Orientation = (byte)orientation;
        }

        //public Rectangle GetBounds()
        //{ return new Rectangle(-(int)Origin.X, -(int)Origin.Y, SourceRect.Width, SourceRect.Height); }
        public Vector3 GetOffset()
        {
            double t = Math.Sin(OffsetTimer * 2 * Math.PI);
            return (float)t * Offset;
        }

        static public Vector3 GetOffset(Vector3 offset, double offsetTimer)
        {
            double t = Math.Sin(offsetTimer * 2 * Math.PI);
            return (float)t * offset;
        }

        public override void Draw(
            SpriteBatch sb,
            DrawObjectArgs a
            )
        {
            Rectangle bounds = a.ScreenBounds;
            Chunk chunk = a.Chunk;
            Cell cell = a.Cell;
            GameObject obj = a.Object;
            Camera camera = a.Camera;
            Controller controller = a.Controller;
            Map map = a.Map;
            float depth = a.Depth;

            Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);

            //byte sunlight = Math.Max((byte)(chunk.GetSunlight(cell.LocalCoords) - map.SkyDarkness), chunk.GetCellLight(cell.LocalCoords));
            //if(sunlight==0)
            //    Console.WriteLine(sunlight + ":" + cell.LocalCoords);
            //Color color = new Color((sunlight + 1) / 16f, 0, 0);
            Color color = a.Color;
            PositionComponent posComp;
            if (obj.TryGetComponent<PositionComponent>("Position", out posComp))
            {
                Position pos = posComp.GetProperty<Position>("Position");
                //if (camera.CullingCheck(pos.Global.X, pos.Global.Y, pos.Global.Z, Sprite.GetBounds(), out bounds))

                Vector3 off = GetOffset();
                if (camera.CullingCheck(pos.Global.X + off.X, pos.Global.Y + off.Y, pos.Global.Z + off.Z, Sprite.GetBounds(), out bounds))
                    screenLoc = new Vector2(bounds.X, bounds.Y);
                // TODO: slow?
                if (OffsetTimer < 1)
                    //OffsetTimer += GlobalVars.DeltaTime / 10f;
                    OffsetTimer += 1 / 10f;
                else
                    Offset = Vector3.Zero;
            }



            // TODO: fix this crap
            //int orientation = (Orientation + (int)camera.Rotation) % Sprite.SourceRect[Variation].Length;
            int orientation = GetOrientation(camera);
            //camera.SpriteBatch.Draw(Sprite.Texture, screenLoc, Sprite.SourceRect[Variation][orientation], color, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);

            //if (SpriteComponent.ShadowsEnabled)
            //    if (Shadow)
            //        SpriteComponent.DrawShadow(sb, camera, Sprite.GetBounds(), map, obj.Global, Cell.GetGlobalDepth(obj.Global));// depth); // TODO: sprite.getbounds optimize, i do it 4634 times


            // TODO: checking for speed is slow
            Rectangle source = Sprite.SourceRects[Variation][orientation];
            Vector4 shaderRect = new Vector4(source.X / (float)Sprite.Texture.Width, source.Y / (float)Sprite.Texture.Height, source.Width / (float)Sprite.Texture.Width, source.Height / (float)Sprite.Texture.Height);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);



            depth = Cell.GetGlobalDepthNew(obj.Global, map, camera);

            // TODO: slow?
            if (Flash)
            {
                //  Game1.Instance.Effect.Parameters["Overlay"].SetValue(new Vector4(10, 0, 0, 0.5f));
                // sb.Draw(Sprite.Texture, screenLoc + (Flash ? new Vector2(camera.Zoom, 0) : Vector2.Zero), source, Color.White, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);
                Game1.Instance.Effect.Parameters["Overlay"].SetValue(new Vector4(10, 0, 0, 0.5f));
                sb.Draw(Sprite.Texture, screenLoc, source, color, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);
                Game1.Instance.Effect.Parameters["Overlay"].SetValue(new Vector4(1, 1, 1, 1));
                Flash = false;
            }
            else
            {
                //  Game1.Instance.Effect.Parameters["Alpha"].SetValue(this.Alpha);
                // new Color(color.R, color.G, color.B, this.Alpha)
                //sb.Draw(Sprite.Texture, screenLoc, source, new Color(color.R, color.G, color.G, this.Alpha), 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, Cell.GetGlobalDepthNew(obj.Global, camera));// color*this.Alpha
                sb.Draw(Sprite.Texture, screenLoc, source, color * this.Alpha, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);// color*this.Alpha


                //sb.Draw(Sprite.Texture, screenLoc, source, color, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, a.Depth);
                // Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);
            }
            //else
            //{
            //    sb.Draw(Sprite.Texture, screenLoc + (Flash ? new Vector2(camera.Zoom, 0) : Vector2.Zero), source, color, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);
            //}
            //if (ShadowsEnabled)
            //    ShadowList.Add(new Shadow(bounds, depth));
            // DRAW SHADOW
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            Vector3 face;
            if (HitTest(bounds, source, camera, out face))
            {
                if (Sprite.MouseMap.Multifaceted)
                {
                    controller.MouseoverNext.TrySet(1 - Cell.GetGlobalDepth(cell.GetGlobalCoords(chunk)), obj, face);
                    return;
                }

                //Sprite tileSprite = Tile.TileSprites[Tile.Types.Sand];
                //Position pos = posComp.GetProperty<Position>("Position");
                //Rectangle cellBounds = camera.GetScreenBounds(pos.Global.X, pos.Global.Y, pos.Global.Z, tileSprite.GetBounds());

                //controller.MouseoverNext.TrySet(1 - Cell.GetGlobalDepth(cell.GetGlobalCoords(chunk)), obj, face);

                if (depth < controller.MouseoverNext.Depth)
                    obj.OnHitTestPass(face, depth);
            }
        }

        public override void DrawMouseover(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (parent.Components.ContainsKey("Multi"))
                return;
            Rectangle bounds = camera.GetScreenBounds(parent.Global, Sprite.GetBounds());
            Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
            sb.Draw(Sprite.Texture, screenLoc,
                Sprite.SourceRects[Variation][SpriteComponent.GetOrientation(Orientation, camera, Sprite.SourceRects[Variation].Length)], 
                new Color(255, 255, 255, 127), 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
        }

        public override void OnHitTestPass(GameObject parent, Vector3 face, float depth)
        {
            if (parent.Components.ContainsKey("Multi"))
                return;

            Controller.Instance.MouseoverNext.Object = parent;
            Controller.Instance.MouseoverNext.Face = face;
            Controller.Instance.MouseoverNext.Depth = depth;
        }

        public void DrawFootprint(SpriteBatch sb, Camera camera, Vector3 global, int orientation)
        {
            if (global.Z == 0)
                return;
            Rectangle tileBounds = Block.Bounds;//ileSprite.GetBounds();
            Rectangle scrBounds = camera.GetScreenBounds(global, tileBounds);
            Vector2 scr = new Vector2(scrBounds.X, scrBounds.Y);
            Cell cell;
            bool check;
            if (!Position.TryGetCell(Engine.Map, global, out cell))
                check = false;
            else
                check = (cell.Type == Block.Types.Air && (global - new Vector3(0, 0, 1)).IsSolid(Engine.Map));//Position.GetCell(Engine.Map, global - new Vector3(0, 0, 1)).Solid);
            Color c = check ? Color.White : Color.Red;
            //sb.Draw(Map.TerrainSprites, scr, Tile.TileHighlights[0][0], c * 0.5f, 0, new Vector2(0, -Tile.OriginCenter.Y + 16), camera.Zoom, SpriteEffects.None, 0);
            sb.Draw(Map.TerrainSprites, scr, Block.TileHighlights[0][2], c * 0.5f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
        }

        public void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, int orientation, float depth)
        {
           // DrawFootprint(sb, camera, global, orientation);
            Rectangle bounds;
            Vector2 screenLoc;

            bounds = camera.GetScreenBounds(global, Sprite.GetBounds());
            screenLoc = new Vector2(bounds.X, bounds.Y);
            //          Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);

            sb.Draw(Sprite.Texture, screenLoc,
                Sprite.SourceRects[0][orientation], Color.White * 0.5f, //new Color(255, 255, 255, 127),
                0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
        }
        public override void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, Color color, float depth)
        {
            // DrawFootprint(sb, camera, global, orientation);
            Rectangle bounds;
            Vector2 screenLoc;

            bounds = camera.GetScreenBounds(global, Sprite.GetBounds());
            screenLoc = new Vector2(bounds.X, bounds.Y);
            //          Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);

            sb.Draw(Sprite.Texture, screenLoc,
                Sprite.SourceRects[0][0], color, //new Color(255, 255, 255, 127),
                0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
        }
        static public void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, GameObject obj)
        {
            SpriteComponent spriteComp;
            if (!obj.TryGetComponent<SpriteComponent>("Sprite", out spriteComp))
                return;
            Rectangle bounds;
            Vector2 screenLoc;
            bounds = camera.GetScreenBounds(global, spriteComp.Sprite.GetBounds());
            screenLoc = new Vector2(bounds.X, bounds.Y);
            //          Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);

            sb.Draw(spriteComp.Sprite.Texture, screenLoc,
                spriteComp.Sprite.SourceRects[0][spriteComp.Orientation], Color.White * 0.5f, //new Color(255, 255, 255, 127),
                0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
        }

        public int GetOrientation(Camera camera)
        {
            int orientation = (Orientation - (int)camera.Rotation) % Sprite.SourceRects[Variation].Length;
            orientation = orientation < 0 ? Sprite.SourceRects[Variation].Length + orientation : orientation;
            return orientation;
        }

        static public int GetOrientation(int orientation, Camera camera, int length)
        {
            //int o = (orientation - (int)camera.Rotation) % 4;
            //return o < 0 ? 4 + o : o;

            int o = (orientation - (int)camera.Rotation) % length;
            return o < 0 ? length + o : o; 
        }

        public static void DrawShadow(SpriteBatch sb, Camera camera, Rectangle spriteBounds, Map map, Vector3 global, float depthNear, float depthFar)// MovementComponent posComp, float depth)
        {
          //  Vector3 global = posComp.GetProperty<Position>("Position").Global;
           // Rectangle spriteBounds = sprite.GetBounds();
            int n = (int)global.Round().Z;
            bool drawn = false;
            while (n >= 0 && !drawn)
            {
                Cell cellShadow;
                if (Position.TryGetCell(map, new Vector3(global.X, global.Y, n), out cellShadow))
                {
                    //if (cellShadow.Solid)
                    if(cellShadow.Type != Block.Types.Air)
                    {
                        Rectangle shadowBounds;
                        if (camera.CullingCheck(global.X, global.Y, n+1, new Rectangle(-spriteBounds.Width / 2, -spriteBounds.Width / 4, spriteBounds.Width, spriteBounds.Width / 2), out shadowBounds))
                        {                            
                            ShadowList.Add(new Shadow(global, shadowBounds));//, depthNear, depthFar));
                        }
                        drawn = true;
                    }
                }
                n--;
            }
        }


        public void DrawHighlight(SpriteBatch sb, Camera camera, Rectangle bounds)
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.None, 0);
            //camera.SpriteBatch.Draw(UI.UIManager.Highlight, new Vector2(bounds.X, bounds.Y), null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
        public override void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, float depth)
        {
            DrawPreview(sb, camera, global, Orientation, depth);
        }
        public override void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global)
        {
            DrawPreview(sb, camera, global, Orientation, 0);
        }

        protected bool HitTest(Rectangle bounds, Rectangle src, Camera camera, out Vector3 face)
        {
            face = Vector3.Zero;
            if (bounds.Intersects(Controller.Instance.MouseRect))
            {
                int xx = (int)((Controller.Instance.msCurrent.X - bounds.X) / (float)camera.Zoom);
                int yy = (int)((Controller.Instance.msCurrent.Y - bounds.Y) / (float)camera.Zoom);

                // TODO: fix face detection
                //if (Sprite.MouseMap.HitTest(src, xx, yy, out face, 0, Orientation))
                //{
                    // TODO: have the color array generated on creation of sprite and cache it
                Color[] spriteMap = this.Sprite.ColorArray;
                        //new Color[src.Width * src.Height];
                    //Sprite.Texture.GetData(0, src, spriteMap, 0, src.Width * src.Height);
                    Color c = spriteMap[yy * src.Width + xx];
                    if (c.A == 0)
                    {
                        return false;
                    }

                    if (Sprite.MouseMap.Multifaceted)
                    {
                        Sprite.MouseMap.HitTest(xx, yy, out face);
                      //  face = new Vector3(0, 0, 1);
                    }

                    return true;
                //}
            }
            //face = Vector3.Zero;
            return false;
        }

        //protected bool HitTest(Rectangle bounds, Rectangle src, Camera camera, out Color face)
        //{
        //    if (bounds.Intersects(Controller.Instance.MouseRect))
        //    {
        //        int xx = (int)((Controller.Instance.msCurrent.X - bounds.X) / (float)camera.Zoom);
        //        int yy = (int)((Controller.Instance.msCurrent.Y - bounds.Y) / (float)camera.Zoom);

        //        //do a hit test with tha actual spirte first, before the mousemap. cause the mousemap might be bigger (as is the case with the walls)
        //        //Color alphaTest;
        //        //if (!Sprite.AlphaMap.HitTest(xx, yy, out alphaTest))
        //        //{
        //        //    face = alphaTest;
        //        //    return false;
        //        //}

        //        //if (Sprite.MouseMap == null)
        //        //{
        //        //    face = alphaTest;
        //        //    return true;
        //        //}
        //       // Rectangle src = Sprite.SourceRect[0][0];

        //        // TODO: must put rotations in mousemap
        //        Color color = Sprite.MouseMap.Map[yy * src.Width + xx];
        //        //Color color = Sprite.MouseMap.Map[yy * Sprite.MouseMap.Texture.Width + xx];
        //        face = color;
        //        if (color.A > 0)
        //        {
        //            // TODO: have the color array generated on creation of sprite and cache it
        //            Color[] spriteMap = new Color[src.Width * src.Height];
        //            Sprite.Texture.GetData(0, src, spriteMap, 0, src.Width * src.Height);
        //            Color c = spriteMap[yy * src.Width + xx];
        //            if (c.A == 0)
        //            {
        //                return false;
        //            }

        //            return true;
        //        }
        //    }
        //    face = Color.Transparent;
        //    return false;
        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            //Message.Types msg = e.Type;
            //GameObject sender = e.Sender;
            // if (msg == Message.Types.Attack)
            switch (e.Type)
            {
                case Message.Types.Attacked:
                    Offset = parent.Global - e.Sender.Global;
                    Offset.Normalize();
                    Offset /= 4;// (float)Tile.Depth;
                    OffsetTimer = 0.25f;
                    Flash = true;
                    return true;
                case Message.Types.SetSprite:
                    Sprite = e.Parameters[0] as Sprite;
                    return true;
                case Message.Types.SetShadow:
                    Shadow = (bool)e.Parameters[0];
                    return true;
            }
            return false;
        }

        public override object Clone()
        {
            SpriteComponent spr = new SpriteComponent(Sprite);
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

        static public bool HasOrientation(GameObject obj)
        {
            SpriteComponent spriteComp = obj.GetComponent<SpriteComponent>("Sprite");
            Sprite sprite = spriteComp.Sprite;
            return sprite.SourceRects.First().Length > 1;
        }

        static public void ChangeOrientation(GameObject obj)
        {
            SpriteComponent spriteComp = obj.GetComponent<SpriteComponent>("Sprite");
            Sprite sprite = spriteComp.Sprite;
            int length = sprite.SourceRects.First().Length;
            spriteComp["Orientation"] = ((int)spriteComp["Orientation"] + 1) % length;
        }

        internal override List<SaveTag> Save()
        {
            return new List<SaveTag>() { 
                new SaveTag(SaveTag.Types.Int, "Variation", (int)Variation),
                new SaveTag(SaveTag.Types.Int, "Orientation", (int)Orientation),
            };
        }

        internal override void Load(SaveTag compTag)
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
