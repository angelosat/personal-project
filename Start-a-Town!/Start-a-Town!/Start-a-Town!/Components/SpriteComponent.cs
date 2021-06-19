using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Components
{
    public class SpriteComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Sprite";
            }
        }

        static public bool ShadowsEnabled = true;
        static List<Shadow> ShadowList = new List<Shadow>();
     //   static List<GameObject> PreviewList = new List<GameObject>();
        //public Sprite Sprite;
        //public int Variation, Orientation;
        public Bone Body;
        //{
        //    get { return this.Orientations[0];}// (Bone)this["Body"]; }
        //    set
        //    {
        //        this["Body"] = value;
        //        this.Orientations[0] = value;
        //        this.Orientations[1] = value;
        //        this.Orientations[2] = value;
        //        this.Orientations[3] = value;
        //    }
        //}
        //public Bone[] Orientations = new Bone[4];
        public Sprite Sprite;// { get { return (Sprite)this["Sprite"]; } set { this["Sprite"] = value; } }
       // public ActorSprite ActorSprite { get { return (ActorSprite)this["ActorSprite"]; } set { this["ActorSprite"] = value; } }
        // TODO: variation and orientation should not be part of graphics (variation => infocomponent, orientation => position)
        public int Variation { get { return GetProperty<int>("Variation"); } set { Properties["Variation"] = value; } }
        public int Orientation { get { return GetProperty<int>("Orientation"); } set { Properties["Orientation"] = value; } }
        public bool Flash;// { get { return (bool)this["Flash"]; } set { this["Flash"] = value; } }
        public Vector3 Offset;// { get { return (Vector3)this["Offset"]; } set { this["Offset"] = value; } }
        public double OffsetTimer { get { return (double)this["OffsetTimer"]; } set { this["OffsetTimer"] = value; } }
        public bool Shadow { get { return (bool)this["Shadow"]; } set { this["Shadow"] = value; } }
      //  public Animation Animation { get { return (Animation)this["Animation"]; } set { this["Animation"] = value; } }
        public Sprite FullSprite { get { return (Sprite)this["FullSprite"]; } set { this["FullSprite"] = value; } }
        public bool Hidden { get { return (bool)this["Hidden"]; } set { this["Hidden"] = value; } }
        public CharacterColors Customization { get; set; }
      //  float Frame { get { return (float)this["Frame"]; } set { this["Frame"] = value; } }
        /// <summary>
        /// TODO: decide if i want multiplicate or additive blend for this
        /// for additive, the default value should be transparent, for multiplicative it should be white
        /// change effect accordingly
        /// </summary>
        public Color Tint = Color.White; //Color.Transparent;
        public override void Spawn(Net.IObjectProvider net, GameObject parent)
        {
            this.ObjectLoaded(parent);
        }
        public override void ObjectLoaded(GameObject parent)
        {
            this.Customization.Apply(this.Body);
        }

        public SpriteComponent()
        {
            this.Hidden = false;
            this.Sprite = Sprite.Default;
            this.Body = Bone.Create(Bone.Types.Torso, Sprite);
            this.Flash = false;
            this.Offset = Vector3.Zero;
            this.OffsetTimer = 1d;
            this.Variation = 0;
            this.Orientation = 0;
            this.Customization = new CharacterColors();
        }
    
        public SpriteComponent Initialize(Bone bodySprite, Sprite fullSprite)
        {
            this.Sprite = fullSprite;
            this.Body = bodySprite;
            Variation = 0;
            Orientation = 0;
            return this;
        }
        public SpriteComponent Initialize(Sprite fullSprite)
        {
            this.Sprite = fullSprite;
            this.Body = Bone.Create(Bone.Types.Item, fullSprite);
            Variation = 0;
            Orientation = 0;
            return this;
        }
        public SpriteComponent(Bone bodySprite, Texture2D texture, Rectangle[][] sourcerect, Vector2 origin, MouseMap mousemap = null)
            : this()
        {
            //this.Sprite = sprite;
            this.Body = bodySprite;
            Sprite = new Sprite(texture, sourcerect, origin, mousemap);
            Variation = 0;
            Orientation = 0;

        }
        public SpriteComponent(Texture2D texture, Rectangle[][] sourcerect, Vector2 origin, MouseMap mousemap = null)
            : this()
        {
            this.Sprite = new Sprite(texture, sourcerect, origin, mousemap);
            this.Body = Bone.Create(Bone.Types.Torso, this.Sprite);
            Sprite = new Sprite(texture, sourcerect, origin, mousemap);
            Variation = 0;
            Orientation = 0;

        }
        public SpriteComponent(Bone bodySprite, Sprite fullSprite)
            : this()
        {
            this.Sprite = fullSprite ?? bodySprite.Sprite;
            this.Body = bodySprite;
       //     Sprite = Sprite;
            Variation = 0;
            Orientation = 0;
        }
        /// <summary>
        /// problem with mousemap! (color map)
        /// hit test is done against the default sprite!!!
        /// </summary>
        /// <param name="rootBone"></param>
        public SpriteComponent(Bone rootBone)
            : this()
        {
            this.Body = rootBone;
            this.Sprite = rootBone.Sprite; //workaround for problem in method summary
        }
        public SpriteComponent(Sprite fullSprite)
            : this()
        {
            this.Sprite = fullSprite;
            //this.Body = Bone.Create(Bone.Types.Torso, fullSprite);
            this.Body = Bone.Create(Bone.Types.Item, fullSprite);
           // Sprite = Sprite;
            Variation = 0;
            Orientation = 0;
        }
        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
          //  if (Walking)
                //Frame += GlobalVars.DeltaTime / 4f;
            Body.Update();
            //this.FullSprite = Body.Render();
            base.Update(net, parent, chunk);
        }

        public SpriteComponent(Sprite sprite, int variation = 0, int orientation = 0)
            : this()
        {
            Sprite = sprite;
            this.Body = Bone.Create(Bone.Types.Torso, Sprite);
            Variation = (byte)variation;
            Orientation = (byte)orientation;
        }

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

        public float _Angle;
        public override void Draw(
            MySpriteBatch sb,
            GameObject parent,
            Camera camera
            )
        {
            if (this.Hidden)
                return;
            Rectangle spriteBounds = this.Sprite.GetBounds();
            Vector3 global = parent.Transform.Global;
            //global = global.Floor();
            Rectangle bounds = camera.GetScreenBounds(global, spriteBounds);//, this.Sprite.Origin);
            var boundsVector4 = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, spriteBounds, Vector2.Zero);
            var map = parent.Net.Map;
            //Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
            Vector2 screenLoc = new Vector2(boundsVector4.X, boundsVector4.Y);

            byte sun, block;
            //global.Round().GetLight(map, out sun, out block);
            map.GetLight(global.RoundXY(), out sun, out block);
            float l = (Math.Max(sun, block) + 1) / 16f;
            Color light = new Color(l,l,l,1);

                //Vector3 off = GetOffset();
                //if (camera.CullingCheck(parent.Global.X + off.X, parent.Global.Y + off.Y, parent.Global.Z + off.Z, spriteBounds, out bounds))
                //    screenLoc = new Vector2(bounds.X, bounds.Y);
                //// TODO: slow?
                //if (OffsetTimer < 1)
                //    OffsetTimer += 1 / 10f;
                //else
                //    Offset = Vector3.Zero;
            


            Rectangle source = Sprite.AtlasToken.Rectangle;// Sprite.SourceRects[Variation][orientation];
            Vector4 shaderRect = new Vector4(source.X / (float)Sprite.Texture.Width, source.Y / (float)Sprite.Texture.Height, source.Width / (float)Sprite.Texture.Width, source.Height / (float)Sprite.Texture.Height);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);

            //   depth = Cell.GetGlobalDepthNew(obj.Global, map, camera);
            float depth = global.GetDrawDepth(map, camera);
            var body = this.Body;// this.Orientations[this.GetOrientation(camera)];
            // TODO: slow?
            if (Flash)
            {
                Game1.Instance.Effect.Parameters["Overlay"].SetValue(new Vector4(10, 0, 0, 0.5f));
                Game1.Instance.Effect.Parameters["Overlay"].SetValue(new Vector4(1, 1, 1, 1));
                Flash = false;
            }
            else
            {
                Vector2 direction = parent.Transform.Direction;
                Vector2 finalDir = Coords.Rotate(camera, direction);
                SpriteEffects sprfx = (finalDir.X - finalDir.Y) < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                //screenLoc = camera.GetScreenPosition(global).Floor();// + GetOffset()).Floor();
                screenLoc = camera.GetScreenPositionFloat(global);// + GetOffset()).Floor();
                //screenLoc *= 100;
                //screenLoc.Round();
                //screenLoc /= 100;
                //light = Color.Red;
                byte skylight, blocklight;
                parent.Map.GetLight(parent.Global.RoundXY(), out skylight, out blocklight);
                //var skyColor = Color.Lerp(map.GetAmbientColor(), Color.White, (skylight) / 15f);
                var skyColor = map.GetAmbientColor() * ((skylight+1) / 16f); //((skylight) / 15f);
                skyColor.A = 255;
                var blockColor = Color.Lerp(Color.Black, Color.White, (blocklight) / 15f);
                var fog = camera.GetFogColor((int)parent.Global.Z);
                var test = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, new Rectangle(0, 0, 0, 0), Vector2.Zero);

                var finalpos = new Vector2(test.X, test.Y) + (body.OriginGroundOffset * camera.Zoom); //screenLoc + 
                //body.DrawTree(parent, sb, finalpos, skyColor, blockColor, Color.White, fog, this._Angle, camera, sprfx, 1f, depth);
                body.DrawTree(parent, sb, finalpos, skyColor, blockColor, this.Tint, fog, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, depth);

                //var finalpos = new Vector2(test.X, test.Y); //screenLoc + 
                ////body.DrawTree(parent, sb, finalpos, skyColor, blockColor, Color.White, fog, this._Angle, camera, sprfx, 1f, depth);
                //body.DrawTree(parent, sb, Bone.Types.None, finalpos, skyColor, blockColor, this.Tint, fog, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, depth);
            }

            // DRAW SHADOW
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            Vector3 face;
            //bounds.DrawHighlight(sb);
                //sprite.Draw(sb, finalpos, sky, block, finalColor, fog, finalAngle, origin, scale, sprFx, finalDepth);

            //DrawFullSprite(sb, camera, screenLoc, depth);

            //if (HitTest(bounds, source, camera, out face))
            //if(!Camera.BlockTargeting)
            if(!Controller.IsBlockTargeting())
            if (HitTest(boundsVector4, source, camera, out face))
            //if (HitTest(camera.GetScreenBounds(parent.Global, body.Sprite.AtlasToken.Texture.Bounds), body.Sprite.AtlasToken.Texture.Bounds, camera, out face))
            {
                Controller.SetMouseoverEntity(camera, parent, global, face, depth);
                //float mouseoverDepth = global.GetMouseoverDepth(map, camera);
                //if (mouseoverDepth >= Controller.Instance.MouseoverEntityNext.Depth)
                //{
                //    Controller.Instance.MouseoverEntityNext.Target = new TargetArgs(parent, face);
                //    Controller.Instance.MouseoverEntityNext.Object = parent;
                //    Controller.Instance.MouseoverEntityNext.Face = face;
                //    Controller.Instance.MouseoverEntityNext.Depth = depth;
                //}
            }
            this.DrawShadow(camera, spriteBounds, parent);//.Map, global);
        }

        private void DrawFullSprite(MySpriteBatch sb, Camera camera, Vector2 screenLoc, float depth)
        {
            this.Sprite.Draw(sb, screenLoc, Color.White, Color.White, Color.White, Color.Transparent, 0, this.Sprite.Origin, camera.Zoom, SpriteEffects.None, depth + .001f);
        }

        public override void MakeChildOf(GameObject parent)
        {
            Body.MakeChildOf(parent);
        }

        public override void OnHitTestPass(GameObject parent, Vector3 face, float depth)
        {
            if (parent.Components.ContainsKey("Multi"))
                return;
            Controller.Instance.MouseoverNext.Target = new TargetArgs(parent, face);
            Controller.Instance.MouseoverNext.Object = new TargetArgs(parent, face); // parent;
            Controller.Instance.MouseoverNext.Face = face;
            Controller.Instance.MouseoverNext.Depth = depth;
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
        public override void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global)
        {
            DrawPreview(sb, camera, global, Orientation);
        }
        public override void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, float depth)
        {
            DrawPreview(sb, camera, global, Orientation, depth);
        }
        public void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, int orientation)
        {
            DrawFootprint(sb, camera, global, orientation);
            Rectangle bounds;
            Vector2 screenLoc;

            bounds = camera.GetScreenBounds(global, Sprite.GetBounds());
            screenLoc = new Vector2(bounds.X, bounds.Y);
            //          Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);



            sb.Draw(Sprite.Texture, screenLoc,
                Sprite.SourceRects[0][orientation], Color.White * 0.5f, //new Color(255, 255, 255, 127),
                0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
        }     
        public void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, int orientation, float depth)
        {
            // DrawFootprint(sb, camera, global, orientation);
            Rectangle bounds;
            Vector2 screenLoc;

            bounds = camera.GetScreenBounds(global, Sprite.GetBounds());
            screenLoc = new Vector2(bounds.X, bounds.Y);

            this.Sprite.Draw(sb, screenLoc, Color.White * 0.5f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
        }

        public override void DrawMouseover(MySpriteBatch sb, Camera camera, GameObject parent)
        {
            // replaced this cause texture was a bit off-center
            //Rectangle bounds = camera.GetScreenBounds(parent.Global, Sprite.GetBounds());
            //Vector2 loc = new Vector2(bounds.X, bounds.Y);
            //sb.Draw(Sprite.Texture, loc, Sprite.SourceRects[0][0], new Color(255, 255, 255, 127), 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);

            if (this.Hidden)
                return;

            //Vector2 loc = camera.GetScreenPosition(parent.Global).Floor();
            Vector2 loc = camera.GetScreenPositionFloat(parent.Global);

            //Sprite.Draw(sb, loc, new Color(255, 255, 255, 127), 0, this.Sprite.Origin, camera.Zoom, SpriteEffects.None, 0);
            Vector2 direction = parent.Transform.Direction;
            Vector2 finalDir = Coords.Rotate(camera, direction);
            SpriteEffects sprfx = (finalDir.X - finalDir.Y) < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            //this.Body.DrawTree(parent, sb, loc + (this.Body.OriginGroundOffset) * camera.Zoom, Color.White, Color.White, new Color(1f,1f,1f,0.5f), Color.Transparent, this._Angle, camera.Zoom, sprfx, 1f, .99f);
            var mouseovertint =  new Color(1f, 1f, 1f, 0.5f); //Color.White * .5f;

            this.Body.DrawTree(parent, sb, loc + (this.Body.OriginGroundOffset) * camera.Zoom, Color.White, Color.White, mouseovertint, Color.Transparent, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, .99f);
            //this.Body.DrawTree(parent, sb, loc, this.Body.GetOffset(Bone.Types.None), Color.White, Color.White, mouseovertint, Color.Transparent, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, .99f);


            // TODO: handle case where root bone doesn't have a sprite, or draw whole bone tree instead
            //Game1.Instance.GraphicsDevice.Textures[0] = this.Body.Sprite.AtlasToken.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            sb.Flush();
        }
        public void DrawFootprint(SpriteBatch sb, Camera camera, Vector3 global, int orientation)
        {
            Rectangle tileBounds = Block.Bounds;//ileSprite.GetBounds();
            Rectangle scrBounds = camera.GetScreenBounds(global, tileBounds);
            Vector2 scr = new Vector2(scrBounds.X, scrBounds.Y);
            Cell cell;
            bool check;
            //if (!Position.TryGetCell(Engine.Map, global, out cell))

            if (!Engine.Map.TryGetCell(global, out cell))
                check = false;
            else
                check = (cell.Block.Type == Block.Types.Air && Engine.Map.IsSolid(global - new Vector3(0, 0, 1)));//Position.GetCell(Engine.Map, global - new Vector3(0, 0, 1)).Solid);
                //check = (cell.Block.Type == Block.Types.Air && (global - new Vector3(0, 0, 1)).IsSolid(Engine.Map));//Position.GetCell(Engine.Map, global - new Vector3(0, 0, 1)).Solid);
            Color c = check ? Color.White : Color.Red;
            //sb.Draw(Map.TerrainSprites, scr, Tile.TileHighlights[0][0], c * 0.5f, 0, new Vector2(0, -Tile.OriginCenter.Y + 16), camera.Zoom, SpriteEffects.None, 0);
            sb.Draw(Map.TerrainSprites, scr, Block.TileHighlights[0][2], c * 0.5f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
        }
        public int GetOrientation(Camera camera)
        {
            //int orientation = (Orientation - (int)camera.Rotation) % Sprite.SourceRects[Variation].Length;
            //orientation = orientation < 0 ? Sprite.SourceRects[Variation].Length + orientation : orientation;
            int orientation = (Orientation - (int)camera.Rotation) % 4;
            orientation = orientation < 0 ? 4 + orientation : orientation;
            return orientation;
        }

        static public int GetOrientation(int orientation, Camera camera, int length)
        {
            //int o = (orientation - (int)camera.Rotation) % 4;
            //return o < 0 ? 4 + o : o;

            int o = (orientation - (int)camera.Rotation) % length;
            return o < 0 ? length + o : o; 
        }

        public void DrawHighlight(SpriteBatch sb, Camera camera, Rectangle bounds)
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.None, 0);
            //camera.SpriteBatch.Draw(UI.UIManager.Highlight, new Vector2(bounds.X, bounds.Y), null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        protected bool HitTest(Vector4 bounds, Rectangle src, Camera camera, out Vector3 face)
        {
            face = Vector3.Zero;
            //if (bounds.Intersects(Controller.Instance.MouseRect))
            if(bounds.Intersects(new Vector2(Controller.Instance.MouseRect.X, Controller.Instance.MouseRect.Y)))
            {
                int xx = (int)((Controller.Instance.msCurrent.X - bounds.X) / (float)camera.Zoom);
                int yy = (int)((Controller.Instance.msCurrent.Y - bounds.Y) / (float)camera.Zoom);

                // TODO: fix face detection
                //if (Sprite.MouseMap.HitTest(src, xx, yy, out face, 0, Orientation))
                //{
                // TODO: have the color array generated on creation of sprite and cache it
                //Color[] spriteMap = new Color[src.Width * src.Height];
                //Sprite.Texture.GetData(0, src, spriteMap, 0, src.Width * src.Height);
                Color[] spriteMap = this.Sprite.ColorArray;
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
                //Color[] spriteMap = new Color[src.Width * src.Height];
                //Sprite.Texture.GetData(0, src, spriteMap, 0, src.Width * src.Height);
                Color[] spriteMap = this.Sprite.ColorArray;
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

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            //Message.Types msg = e.Type;
            //GameObject sender = e.Sender;
            // if (msg == Message.Types.Attack)
            switch (e.Type)
            {
                case Message.Types.Attacked:
                    GameObject attacker = e.Parameters[0] as GameObject;
                    Offset = parent.Global - attacker.Global;
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
                //case Message.Types.InteractionFinished:
                //case Message.Types.InteractionFailed:
                //case Message.Types.StopWalking:
                //    //  Walking = false;
                //    if (!Walking)
                //        return true;

                //    //Body.Rest();
                //    //Body.Restart();
                //    Body.Rest(foo => foo.Animation.Name == "Walking");
                //    Body.Restart(foo => foo.Animation.Name == "Idle");
                //    Walking = false;

                //    return true;

                //case Message.Types.Move:

                //    if (Walking)
                //    {
                //        foreach (var a in AnimationCollection.Walking)
                //        {
                //            //Bone node;
                //            //if (!Body.Children.TryGetValue(a.Key, out node))
                //            //    continue;
                //            //if (node.State == Bone.States.Finished)
                //            //{
                //            //    node.Animation = a.Value;
                //            //    //node.Frame = - (Body.Keyframes.FrameCount - (Body.Frame%Body.Keyframes.FrameCount));
                //            //    node.Frame = Body.Frame;// -(a.Value.FrameCount - (node.Frame % a.Value.FrameCount));
                //            //}
                //            Body.ForEach(bone =>
                //            {
                //                if (bone.Name == a.Key)
                //                    if (bone.State == Bone.States.Finished)
                //                    {
                //                        bone.Animation = a.Value;
                //                        bone.Frame = Body.Frame;
                //                    }
                //            });
                //        }
                //        return true;
                //    }
                //    float speed = 1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.WalkSpeed, 0f);
                //    Animation.Start(this.Body, AnimationCollection.Walking, speed, b => b.Animation.Name == "Idle");


                //    Walking = true;
                //    return true;

             
                //case Message.Types.StartAnimation:
                //    AnimationCollection anim = e.Parameters[0] as AnimationCollection;
                //    //bool loop = (bool)e.Parameters[1];
                //    speed = (float)e.Parameters.ElementAtOrDefault(1);
                //    Action action = e.Parameters.ElementAtOrDefault(2) as Action;
                //    foreach (var a in anim)
                //    {
                //        Bone node;
                //        if (!Body.Children.TryGetValue(a.Key, out node))
                //            continue;
                //        node.Animation = a.Value;
                //        //   node.Animation.Speed = speed;
                //        node.Restart();
                //        // TODO: fix it because this way the action will be performed more than once if more than one node are animated
                //        node.FinishAction = action;
                //    }
                //    return true;



                ///
                /// find better way to animate hand for interactions
                ///
                //case Message.Types.BeginInteraction:
                //    Body["Right Hand"].Animation = AnimationCollection.Working["Right Hand"];
                //    Body["Right Hand"].Restart();
                //    return true;


                //case Message.Types.InteractionFinished:
                //case Message.Types.InteractionFailed:
                //case Message.Types.OutOfRange:
                //case Message.Types.Interrupt:
                //    Body.Rest();
                //    Body.Restart();
                //    return true;

                //case Message.Types.Receive:
                //case Message.Types.Hold:
                //   // GameObject obj = e.Parameters[1] as GameObject;
                //    GameObject obj = e.Parameters.Translate<SenderEventArgs>(e.Network).Sender;// e.TargetArgs.Object;//.Data.Translate<SenderEventArgs>(e.Network).Sender;
                //    GameObjectSlot objSlot = obj.Exists ? obj.ToSlot() : InventoryComponent.GetFirstOrDefault(parent, foo => foo == obj);
                //    UpdateHeldObject(obj);
                //    return true;

                //case Message.Types.Thrown:
                //case Message.Types.Dropped:
                //    //case Message.Types.Throw:
                //    //case Message.Types.Drop:
                    

                //    // NOTE: i don't have to check, remove sprite anyway
                //    //GameObjectSlot heldSlot;
                //    //if (InventoryComponent.TryGetHeldObject(parent, out heldSlot))
                //    //    //   if (heldSlot.StackSize > 0)
                //    //    if (heldSlot.HasValue)
                //    //        return true;

                //    Body[Bone.Types.RightHand].Children.Remove("Tool");
                //    return true;

                //case Message.Types.EquipItem:
                //    GameObject item = e.Sender;
                //    string slot = EquipComponent.GetSlot(item);

                //    Queue<Bone> toHandle = new Queue<Bone>();
                //    toHandle.Enqueue(this.Body);
                //    while (toHandle.Count > 0)
                //    {
                //        Bone bone = toHandle.Dequeue();
                //        Tuple<Vector2, float> boneSlot;
                //       // BoneSlot boneSlot;
                //        Sprite sprite = item["Sprite"]["Sprite"] as Sprite;
                //        sprite = new Sprite(sprite.Texture, sprite.SourceRects, sprite.Joint, sprite.MouseMap);
                //        if (bone.ChildrenJoints.TryGetValue(slot, out boneSlot))
                //        {
                //            Bone b = Bone.Create(slot, sprite, bone, boneSlot.Item1, bone.Order + 0.0005f);
                //            b.RestingFrame = new Keyframe(10, Vector2.Zero, boneSlot.Item2, Interpolation.Lerp);
                //            //Bone b = Bone.Create(slot, sprite, bone, boneSlot.Joint, bone.Order + 0.0005f); // put depth from boneslot
                //            //b.RestingFrame = new Keyframe(10, Vector2.Zero, boneSlot.Angle, Interpolation.Lerp);
                //            return true;
                //        }

                //        foreach (var sub in bone.Children)
                //            toHandle.Enqueue(sub.Value);
                //    }

                //    return true;
            }
            return false;
        }

        public override string ToString()
        {
            return this.Body.ToString();
        }

        ///// <summary>
        ///// need to improve later to update whole paperdoll
        ///// make a new method to receive a bone as an arg and add it to a specified node, to generalize beyond actor sprites
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public bool UpdateHeldObject(GameObject obj)
        //{
        //    return true;
        //    if (obj == null)
        //    {
        //        Body["Right Hand"].Children.Remove("Tool");
        //        return false;
        //    }
        //    if (!this.Body.Children.ContainsKey("Right Hand"))
        //        return false;
        //    Sprite sprite = obj["Sprite"]["Sprite"] as Sprite;
        //    Bone parentNode = Body["Right Hand"];
        //    //Sprite sprite2 = new Sprite(sprite.Texture, sprite.SourceRects, sprite.Joint, sprite.MouseMap);
        //    //sprite = new Sprite(sprite.Texture, sprite.SourceRects, sprite.Joint, sprite.MouseMap);


        //    //Bone toolNode = Bone.Create("Tool", sprite, parentNode, new Vector2(3, 11), parentNode.Order + 0.0005f);
        //    Bone toolNode = Body["Right Hand"][Stat.Mainhand.Name];
        //    toolNode.SetSprite(sprite);
        //    toolNode.RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp);
        //    return true;
        //}

        //static public bool UpdateHeldObjectSprite(GameObject parent, GameObject obj)
        //{
        //    return parent.GetComponent<SpriteComponent>("Sprite").UpdateHeldObject(obj);
        //}

        public void DrawShadow(Camera camera, Rectangle spriteBounds, GameObject parent)// IMap map, Vector3 global)// MovementComponent posComp, float depth)
        {
            var global = parent.Global;
            var map = parent.Map;
            int n = (int)global.RoundXY().Z;
            bool drawn = false;
            while (n >= 0 && !drawn)
            {
                Cell cellShadow;
                //if (Position.TryGetCell(map, new Vector3(global.X, global.Y, n), out cellShadow))
                if (map.TryGetCell(new Vector3(global.X, global.Y, n), out cellShadow))
                {
                    //if (cellShadow.Block != Block.Air && cellShadow.Block != Block.Water)
                    if(cellShadow.Block.IsOpaque(cellShadow))
                    {
                        Rectangle shadowBounds;
                        if (camera.CullingCheck(global.X, global.Y, n + 1, new Rectangle(-spriteBounds.Width / 2, -spriteBounds.Width / 4, spriteBounds.Width, spriteBounds.Width / 2), out shadowBounds))
                            ShadowList.Add(new Shadow(parent, new Vector3(global.X, global.Y, n + 1), shadowBounds));

                        drawn = true;
                    }
                }
                n--;
            }
        }
        //public static void DrawShadow(Camera camera, Rectangle spriteBounds, IMap map, Vector3 global, float depthNear, float depthFar)// MovementComponent posComp, float depth)
        public static void DrawShadow(Camera camera, Rectangle spriteBounds, IMap map, GameObject parent, float depthNear, float depthFar)// MovementComponent posComp, float depth)
        {
            var global = parent.Global;
            int n = (int)global.RoundXY().Z;
            bool drawn = false;
            while (n >= 0 && !drawn)
            {
                Cell cellShadow;
                //if (Position.TryGetCell(map, new Vector3(global.X, global.Y, n), out cellShadow))

                if (map.TryGetCell(new Vector3(global.X, global.Y, n), out cellShadow))
                {
                    //if (cellShadow.Solid)
                    if (cellShadow.Block.Type != Block.Types.Air)
                    {
                        Rectangle shadowBounds;
                        if (camera.CullingCheck(global.X, global.Y, n + 1, new Rectangle(-spriteBounds.Width / 2, -spriteBounds.Width / 4, spriteBounds.Width, spriteBounds.Width / 2), out shadowBounds))
                        {
                            //ShadowList.Add(new Shadow(global, shadowBounds));
                            ShadowList.Add(new Shadow(parent, new Vector3(global.X, global.Y, n + 1), shadowBounds));
                        }
                        drawn = true;
                    }
                }
                n--;
            }
        }

        //public static void DrawShadow(SpriteBatch sb, Camera camera, Rectangle spriteBounds, Map map, Vector3 global, float depthNear, float depthFar)// MovementComponent posComp, float depth)
        //{
        //    int n = (int)global.Round().Z;
        //    bool drawn = false;
        //    while (n >= 0 && !drawn)
        //    {
        //        Cell cellShadow;
        //        if (Position.TryGetCell(map, new Vector3(global.X, global.Y, n), out cellShadow))
        //        {
        //            //if (cellShadow.Solid)
        //            if (cellShadow.Block.Type != Block.Types.Air)
        //            {
        //                Rectangle shadowBounds;
        //                if (camera.CullingCheck(global.X, global.Y, n + 1, new Rectangle(-spriteBounds.Width / 2, -spriteBounds.Width / 4, spriteBounds.Width, spriteBounds.Width / 2), out shadowBounds))
        //                {
        //                    ShadowList.Add(new Shadow(global, shadowBounds));//, depthNear, depthFar));
        //                }
        //                drawn = true;
        //            }
        //        }
        //        n--;
        //    }
        //}
        static public void DrawShadows(MySpriteBatch sb, IMap map, Camera camera)
        {
            if (ShadowsEnabled)
                foreach (Shadow shadow in ShadowList.OrderBy(foo => foo.Global.GetDrawDepth(map, camera)))// -Cell.GetGlobalDepthNew(foo.Global, camera)))
                    shadow.Draw(sb, map, camera);
            ShadowList.Clear();
        }
        static public void DrawShadows(SpriteBatch sb, Map map, Camera camera)
        {
            //if (ShadowsEnabled)
            //    foreach (Shadow shadow in ShadowList.OrderBy(foo => -Cell.GetGlobalDepthNew(foo.Global, camera)))
            //        shadow.Draw(sb, map, camera);
            ShadowList.Clear();
        }

       
        public override object Clone()
        {

            SpriteComponent spr = new SpriteComponent(Bone.Copy(this.Body), Sprite) { Hidden = this.Hidden, Customization = new CharacterColors(this.Customization) };
            //SpriteComponent spr = new SpriteComponent(Bone.Create(Body), new Sprite(this.Sprite)) { Hidden = this.Hidden, Customization = new CharacterColors(this.Customization) };
            if (this.Body.Sprite != null)
                if (spr.Body.Sprite == this.Body.Sprite)
                    throw new Exception();
            //for (int i = 0; i < 4; i++)
            //    spr.Orientations[i] = Bone.Copy(this.Orientations[i]);
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

        public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            //base.DrawUI(sb, camera, parent);
            //if (Player.Actor == null)
            //    return;
            //var isPlayerTarget = Player.Actor.GetComponent<AttackComponent>().NearestEnemy == parent;
            //if(isPlayerTarget)
            //    this.DrawHighlight(sb, camera, camera.GetScreenBounds(parent.Global, Sprite.AtlasToken.Rectangle, Sprite.Origin));
        }

       
        public override void ComponentsCreated(GameObject parent)
        {
            if (!parent.Components.ContainsKey("Inventory"))
                return;
            GameObjectSlot objSlot = parent["Inventory"]["Holding"] as GameObjectSlot;
            //UpdateHeldObject(objSlot.Object);
        }

        internal override List<SaveTag> Save()
        {
            var list = new List<SaveTag>() { 
                new SaveTag(SaveTag.Types.Int, "Variation", (int)Variation),
                new SaveTag(SaveTag.Types.Int, "Orientation", (int)Orientation),
                new SaveTag(SaveTag.Types.Compound, "Customization", this.Customization.Save())
            };
            return list;
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
            //this.Customization = new CharacterColors(compTag.GetValue<SaveTag>("Customization"));
            if (!compTag.TryGetTag("Customization", t => this.Customization = new CharacterColors(t)))
                this.Customization = new CharacterColors();
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            this.Customization.Write(w);
            this.Body.Write(w);
            
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Customization = new CharacterColors(r);
            this.Body.Read(r);
        }

        public static Bone GetRootBone(GameObject parent)
        {
            if (parent == null)
                return null;
            return parent.GetComponent<SpriteComponent>().Body;
        }
    }
}
