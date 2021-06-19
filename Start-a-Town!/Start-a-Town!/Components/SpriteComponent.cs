using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Animations;
using Start_a_Town_.GameModes;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    public class SpriteComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Sprite";
            }
        }
        readonly static bool PreciseAlphaHitTest = false;

        static public bool ShadowsEnabled = true;
        readonly static List<Shadow> ShadowList = new();
        public Bone Body, DefaultBody;
        public List<Animation> Animations = new();
        public Sprite Sprite;
        public int Variation;// { get { return GetProperty<int>("Variation"); } set { Properties["Variation"] = value; } }
        public int Orientation;// { get { return GetProperty<int>("Orientation"); } set { Properties["Orientation"] = value; } }
        public bool Flash;
        public Vector3 Offset;
        public double OffsetTimer;// { get { return (double)this["OffsetTimer"]; } set { this["OffsetTimer"] = value; } }
        public bool Shadow;// { get { return (bool)this["Shadow"]; } set { this["Shadow"] = value; } }
        public Sprite FullSprite;// { get { return (Sprite)this["FullSprite"]; } set { this["FullSprite"] = value; } }
        public bool Hidden;// { get { return (bool)this["Hidden"]; } set { this["Hidden"] = value; } }
        Rectangle CachedMinimumRectangle;
        public Rectangle GetSpriteBounds()
        {
            //var offset = new Vector2(-SpriteBounds.Width / 2, -SpriteBounds.Height) + this.Body.Sprite?.OriginGround ?? Vector2.Zero;// this.Body.OriginGroundOffset;
            var offset = new Vector2(CachedMinimumRectangle.Width / 2, CachedMinimumRectangle.Height - this.Body.Sprite.OriginY);// + this.Body.Sprite.OriginY);// + Vector2.UnitY * (this.Body.OriginGroundOffset.Y);// + this.Body.Sprite?.OriginGround ?? Vector2.Zero;// this.Body.OriginGroundOffset;

            return new Rectangle(-(int)(offset.X), -(int)(offset.Y),
                this.CachedMinimumRectangle.Width
                //+ 2 * Graphics.Borders.Thickness
                ,
                this.CachedMinimumRectangle.Height
                //+ 2 * Graphics.Borders.Thickness
                );
        }

        public CharacterColors Customization = new(); //{ get; set; }
        readonly Dictionary<BoneDef, Bone.Props> BoneProps = new();
        Dictionary<BoneDef, Material> Materials = new();
        public SpriteComponent SetMaterial(BoneDef t, Material m)
        {
            if (this.Body.TryFindBone(t, out Bone b))
                b.Material = m;

            this.Materials[t] = m;
            if (this.BoneProps.TryGetValue(t, out var p))
                p.Material = m;
            else
                this.BoneProps.Add(t, new Bone.Props() { Material = m });
            return this;
        }
        public Material GetMaterial(BoneDef t)
        {
            return this.Body.FindBone(t)?.Material;

            //return this.Materials[t];
            //return this.BoneProps[t].Material;
        }
        public Material GetMaterial(Bone t)
        {
            return this.GetMaterial(t.Def);
            //return this.TryGetMaterial(t.Def, out var mat) ? mat : t.Material;
        }
        public Material TryGetMaterial(BoneDef t)
        {
            return this.Body.TryFindBone(t, out var b) ? b.Material : null;

            //return this.Materials.TryGetValue(t, out var mat) ? mat : null;
            //return this.BoneProps.TryGetValue(t, out var prop) ? prop.Material : null;
        }
        public bool TryGetMaterial(BoneDef t, out Material mat)
        {
            if (this.Body.TryFindBone(t, out var bone))
            {
                mat = bone.Material;
                return true;
            }
            {
                mat = null;
                return false;
            }
            //return this.Materials.TryGetValue(t, out mat);
        }


        //RenderTarget2D StackSizeTexture;

        /// <summary>
        /// TODO: decide if i want multiplicate or additive blend for this
        /// for additive, the default value should be transparent, for multiplicative it should be white
        /// change effect accordingly
        /// </summary>
        public Color Tint = Color.White; //Color.Transparent;

        //public override void OnSpawn(IObjectProvider net, GameObject parent)
        //{
        //    this.OnObjectLoaded(parent);
        //}
        //public override void OnObjectLoaded(GameObject parent)
        //{
        //    //this.Customization.Apply(this.Body);
        //    //this.Customization = new CharacterColors(this.Body).Randomize();
        //}
        //public override void OnObjectCreated(GameObject parent)
        //{
        //    //this.StackSizeTexture = UIManager.CreateTextureString(parent.StackSize.ToString(), Color.Black * .5f);
        //}
        public SpriteComponent()
        {
            this.Hidden = false;
            this.Sprite = Sprite.Default;
            this.Body = Bone.Create(BoneDef.Torso, Sprite);
            this.DefaultBody = this.Body;

            this.Flash = false;
            this.Offset = Vector3.Zero;
            this.OffsetTimer = 1d;
            this.Variation = 0;
            this.Orientation = 0;
            //this.Customization = new CharacterColors();
        }
    
        
        public SpriteComponent(Bone bodySprite, Texture2D texture, Rectangle[][] sourcerect, Vector2 origin, MouseMap mousemap = null)
            : this()
        {
            //this.Sprite = sprite;
            this.Body = bodySprite;
            this.DefaultBody = this.Body;

            Sprite = new Sprite(texture, sourcerect, origin, mousemap);
            Variation = 0;
            Orientation = 0;

        }
        public SpriteComponent(Texture2D texture, Rectangle[][] sourcerect, Vector2 origin, MouseMap mousemap = null)
            : this()
        {
            this.Sprite = new Sprite(texture, sourcerect, origin, mousemap);
            this.Body = Bone.Create(BoneDef.Torso, this.Sprite);
            this.DefaultBody = this.Body;

            Sprite = new Sprite(texture, sourcerect, origin, mousemap);
            Variation = 0;
            Orientation = 0;

        }
        public SpriteComponent(Bone bodySprite, Sprite fullSprite)
            : this()
        {
            this.Sprite = fullSprite ?? bodySprite.Sprite;
            this.Body = bodySprite.Clone();
            this.DefaultBody = this.Body;
            this.CachedMinimumRectangle = this.Body.GetMinimumRectangle();
            this.Customization = new CharacterColors(this.Body).Randomize();

            //     Sprite = Sprite;
            Variation = 0;
            Orientation = 0;
        }
        public SpriteComponent(ItemDef def)
              : this()
        {
            //this.Body = def.Body;
            this.Body = def.Body.Clone();
            this.CachedMinimumRectangle = this.Body.GetMinimumRectangle();

            this.Sprite = def.DefaultSprite ?? def.Body.Sprite;
            this.DefaultBody = this.Body;

            InitMaterials(def);

            this.Customization = new CharacterColors(this.Body).Randomize();

            //     Sprite = Sprite;
            Variation = 0;
            Orientation = 0;
        }
        public SpriteComponent Initialize(Bone bodySprite, Sprite fullSprite)
        {
            this.Sprite = fullSprite;
            this.Body = bodySprite;
            this.DefaultBody = this.Body;
            Variation = 0;
            Orientation = 0;
            return this;
        }
        public SpriteComponent Initialize(Sprite fullSprite)
        {
            this.Sprite = fullSprite;
            this.Body = Bone.Create(BoneDef.Item, fullSprite);
            this.DefaultBody = this.Body;

            Variation = 0;
            Orientation = 0;
            return this;
        }
        private void InitMaterials(ItemDef def)
        {
            var queue = new Queue<Bone>();
            queue.Enqueue(def.Body);
            //var current = def.Body;
            while(queue.Any())
            {
                var current = queue.Dequeue();
                this.SetMaterial(current.Def, def.DefaultMaterial);
                //this.Materials.Add(current.Def, def.DefaultMaterial);
                foreach (var j in current.Joints.Values)
                    if(j.Bone != null)
                    queue.Enqueue(j.Bone);
            }
        }
        //internal void InitMaterials(ItemDef def, Dictionary<string, Entity> ingredients)
        //{
        //    this.Materials.Clear();
        //    foreach (var c in def.CraftingProperties.Reagents)
        //    {
        //        var obj = ingredients[c.Value.Name];
        //        this.SetMaterial(c.Key, obj.GetComponent<SpriteComponent>().GetMaterial(obj.Body.Def));
        //    }
        //}
        
        internal override void Initialize(Entity parent, Dictionary<string, Material> ingredients)
        {
            //foreach(var i in ingredients)
            //    this.Materials[def.CraftingIngredientIndices[]
            var def = parent.Def;
            this.Materials.Clear();
            foreach (var i in def.CraftingProperties.Reagents)
            {
                this.SetMaterial(i.Key, ingredients[i.Value.Label]);
                //this.Materials[i.Key] = obj.GetComponent<SpriteComponent>().GetMaterial(obj.Body.Def);// .Materials[obj.Body.Def]; // temp
            }
        }
        public override void SetMaterial(Material mat)
        {
            //this.Materials.Add(this.Body.Def, mat);
            this.Materials[this.Body.Def] = mat;
            this.SetMaterial(this.Body.Def, mat);
        }
        
        /// <summary>
        /// problem with mousemap! (color map)
        /// hit test is done against the default sprite!!!
        /// </summary>
        /// <param name="rootBone"></param>
        public SpriteComponent(Bone rootBone)
            : this()
        {
            this.Body = rootBone.Clone();
            this.DefaultBody = this.Body;
            this.CachedMinimumRectangle = this.Body.GetMinimumRectangle();
            this.Customization = new CharacterColors(this.Body).Randomize();

            //this.Sprite = rootBone.Sprite; //workaround for problem in method summary
        }
        [Obsolete]
        public SpriteComponent(Sprite fullSprite)
            : this()
        {
            this.Sprite = fullSprite;
            //this.Body = Bone.Create(BoneDef.Torso, fullSprite);
            this.Body = Bone.Create(BoneDef.Item, fullSprite);
            this.DefaultBody = this.Body;

           // Sprite = Sprite;
            Variation = 0;
            Orientation = 0;
        }
        public override void Tick(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            if (this.Body == null)
                this.Body = this.DefaultBody;
            //Body.Update();

            //this.Body.UpdateAnimations(parent);
            //foreach (var ani in Animations)
            //    ani.Tick();
            var nextAnimations = new List<Animation>();
            foreach (var ani in Animations)
            {
                ani.Update(parent);
                if (!(ani.State == AnimationStates.Removed && ani.Weight <= 0))
                    nextAnimations.Add(ani);
            }
            this.Animations = nextAnimations;

            //if (net is Client)
            //    HitTest(parent);
        }
        public void HitTest(GameObject parent, Camera camera)
        {
            //if (Controller.IsBlockTargeting())
            //    return;
            if (ToolManager.Instance.ActiveTool.TargetOnlyBlocks)
                return;
            // UNDONE store both current block and entity under the mouse and let each controltool decide which one to target

            var source = this.GetSpriteBounds();// this.SpriteBounds;
            var global = parent.Global;
            //var w = source.Width;
            //var h = source.Height;
            var boundsVector4 = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, source, Vector2.Zero, this.Body.Scale);// + Body.Sprite.WhiteSpace));

            //if (!Controller.IsBlockTargeting())
                if (HitTest(boundsVector4, source, camera, out Vector3 face))
                {
                    float depth = global.GetDrawDepth(parent.Map, camera);
                    Controller.TrySetMouseoverEntity(camera, parent, face, depth);
                }
        }

        //public SpriteComponent(Sprite sprite, int variation = 0, int orientation = 0)
        //    : this()
        //{
        //    Sprite = sprite;
        //    this.Body = Bone.Create(BoneDef.Torso, Sprite);
        //    this.DefaultBody = this.Body;

        //    Variation = (byte)variation;
        //    Orientation = (byte)orientation;
        //}

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
            //var boundsVector4 = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, spriteBounds, Vector2.Zero);
            var map = parent.Net.Map;
            //Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
            //Vector2 screenLoc = new Vector2(boundsVector4.X, boundsVector4.Y);

            //byte sun, block;
            //map.GetLight(global.RoundXY(), out sun, out block);

            //float l = (Math.Max(sun, block) + 1) / 16f;
            //Color light = new Color(l, l, l, 1);

            //Vector3 off = GetOffset();
            //if (camera.CullingCheck(parent.Global.X + off.X, parent.Global.Y + off.Y, parent.Global.Z + off.Z, spriteBounds, out bounds))
            //    screenLoc = new Vector2(bounds.X, bounds.Y);
            //// TODO: slow?
            //if (OffsetTimer < 1)
            //    OffsetTimer += 1 / 10f;
            //else
            //    Offset = Vector3.Zero;



            var source = Sprite.AtlasToken.Rectangle;// Sprite.SourceRects[Variation][orientation];
            var shaderRect = new Vector4(source.X / (float)Sprite.Texture.Width, source.Y / (float)Sprite.Texture.Height, source.Width / (float)Sprite.Texture.Width, source.Height / (float)Sprite.Texture.Height);
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
                //var screenLoc = camera.GetScreenPositionFloat(global);// + GetOffset()).Floor();
                //screenLoc *= 100;
                //screenLoc.Round();
                //screenLoc /= 100;
                //light = Color.Red;
                parent.Map.GetLight(parent.Global.RoundXY(), out byte skylight, out byte blocklight);
                //var skyColor = Color.Lerp(map.GetAmbientColor(), Color.White, (skylight) / 15f);
                var skyColor = map.GetAmbientColor() * ((skylight + 1) / 16f); //((skylight) / 15f);
                skyColor.A = 255;
                var blockColor = Color.Lerp(Color.Black, Color.White, (blocklight) / 15f);
                //var fog = camera.GetFogColor((int)parent.Global.Z);
                var fog = camera.GetFogColorNew((int)parent.Global.Z);

                var test = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, new Rectangle(0, 0, 0, 0), Vector2.Zero);

                var finalpos = new Vector2(test.X, test.Y) + (body.OriginGroundOffset * camera.Zoom); //screenLoc + 
                //body.DrawTree(parent, sb, finalpos, skyColor, blockColor, this.Tint, fog, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, depth);
                if(parent is Actor)
                {

                }
                //body.DrawTreeAnimationDeltas(parent, sb, finalpos, skyColor, blockColor, this.Tint, fog, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, depth);
                body.DrawTreeAnimationDeltas(parent as Entity, this.Customization, this.Animations, sb, finalpos, skyColor, blockColor, this.Tint, fog, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, depth);

            }

            // DRAW SHADOW
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));


            this.DrawShadow(camera, spriteBounds, parent);//.Map, global);
        }

        private void DrawFullSprite(MySpriteBatch sb, Camera camera, Vector2 screenLoc, float depth)
        {
            this.Sprite.Draw(sb, screenLoc, Color.White, Color.White, Color.White, Color.Transparent, 0, this.Sprite.OriginGround, camera.Zoom, SpriteEffects.None, depth + .001f);
        }

        public override void MakeChildOf(GameObject parent)
        {
            Body.MakeChildOf(parent);
        }

        public override void OnHitTestPass(GameObject parent, Vector3 face, float depth)
        {
            if (parent.Components.ContainsKey("Multi"))
                return;
            Controller.Instance.MouseoverBlockNext.Target = new TargetArgs(parent, face);
            Controller.Instance.MouseoverBlockNext.Object = new TargetArgs(parent, face); // parent;
            Controller.Instance.MouseoverBlockNext.Face = face;
            Controller.Instance.MouseoverBlockNext.Depth = depth;
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
            if (this.Hidden)
                return;

            Vector2 loc = camera.GetScreenPositionFloat(parent.Global);

            Vector2 direction = parent.Transform.Direction;
            Vector2 finalDir = Coords.Rotate(camera, direction);
            SpriteEffects sprfx = (finalDir.X - finalDir.Y) < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            var mouseovertint =  new Color(1f, 1f, 1f, 0.5f); //Color.White * .5f;

            //this.Body.DrawGhost(parent, sb, loc + (this.Body.OriginGroundOffset) * camera.Zoom, Color.White, Color.White, mouseovertint, Color.Transparent, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, .99f);
            this.Body.DrawTreeAnimationDeltas(parent as Entity, this.Customization, this.Animations, sb, loc + (this.Body.OriginGroundOffset) * camera.Zoom, Color.White, Color.White, mouseovertint, Color.Transparent, this._Angle, camera.Zoom, (int)camera.Rotation, sprfx, 1f, .99f);

            // TODO: handle case where root bone doesn't have a sprite, or draw whole bone tree instead
            //Game1.Instance.GraphicsDevice.Textures[0] = this.Body.Sprite.AtlasToken.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            sb.Flush();
        }
        public void DrawFootprint(SpriteBatch sb, Camera camera, Vector3 global, int orientation)
        {
            var tileBounds = Block.Bounds;//ileSprite.GetBounds();
            var scrBounds = camera.GetScreenBounds(global, tileBounds);
            var scr = new Vector2(scrBounds.X, scrBounds.Y);
            bool check;
            //if (!Position.TryGetCell(Engine.Map, global, out cell))

            if (!Engine.Map.TryGetCell(global, out Cell cell))
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

        //public void DrawHighlight(SpriteBatch sb, Camera camera, Rectangle bounds)
        //{
        //    sb.Draw(UI.UIManager.Highlight, bounds, null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.None, 0);
        //    //camera.SpriteBatch.Draw(UI.UIManager.Highlight, new Vector2(bounds.X, bounds.Y), null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        //}
        static public void DrawHighlight(GameObject parent, SpriteBatch sb, Camera camera)
        {
            var sprite = parent.GetComponent<SpriteComponent>().Sprite;
            //Rectangle source = sprite.AtlasToken.Rectangle;// Sprite.AtlasToken.Rectangle;
            //var global = parent.Global;
            //var w = source.Width;
            //var h = source.Height;
            //var boundsVector4 = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, new Rectangle(0, 0, w, h), sprite.OriginGround);// + Body.Sprite.WhiteSpace));
            //var sprite = parent.GetComponent<SpriteComponent>().Sprite;
            var comp = parent.GetComponent<SpriteComponent>();
            var source = sprite.AtlasToken.Rectangle;// Sprite.AtlasToken.Rectangle;
            //var source = comp.SpriteBounds;
            var global = parent.Global;
            var w = source.Width;
            var h = source.Height;
            var boundsVector4 = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, new Rectangle(0, 0, w, h), comp.Body.OriginGroundOffset);// + Body.Sprite.WhiteSpace));

            var rect = boundsVector4.ToRectangle();
            rect.DrawHighlight(sb, .5f);
        }
        protected bool HitTest(Vector4 bounds, Rectangle src, Camera camera, out Vector3 face)
        {
            face = Vector3.Zero;
            //if (bounds.Intersects(Controller.Instance.MouseRect))
            if(bounds.Intersects(new Vector2(Controller.Instance.MouseRect.X, Controller.Instance.MouseRect.Y)))
            {
                if (!PreciseAlphaHitTest)
                    return true;
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
                
            }
            return false;
        }

        public override string ToString()
        {
            return this.Body.ToString();
        }

        
        public void DrawShadow(Camera camera, Rectangle spriteBounds, GameObject parent)// IMap map, Vector3 global)// MovementComponent posComp, float depth)
        {
            var global = parent.Global;
            var map = parent.Map;
            int n = (int)global.RoundXY().Z;
            bool drawn = false;
            while (n >= 0 && !drawn)
            {
                //if (Position.TryGetCell(map, new Vector3(global.X, global.Y, n), out cellShadow))
                var globalcheck = new Vector3(global.X, global.Y, n);
                if (map.TryGetCell(globalcheck, out Cell cellShadow))
                {
                    //if (cellShadow.Block != Block.Air && cellShadow.Block != Block.Water)

                    if (cellShadow.Block.IsSolid(cellShadow))//.IsOpaque(cellShadow))
                    {
                        var blockheight = Block.GetBlockHeight(map, globalcheck);
                        if (camera.CullingCheck(global.X, global.Y, n + 1, new Rectangle(-spriteBounds.Width / 2, -spriteBounds.Width / 4, spriteBounds.Width, spriteBounds.Width / 2), out Rectangle shadowBounds))
                            ShadowList.Add(new Shadow(parent, new Vector3(global.X, global.Y, n + blockheight)));// n + 1), shadowBounds));

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
                //if (Position.TryGetCell(map, new Vector3(global.X, global.Y, n), out cellShadow))

                if (map.TryGetCell(new Vector3(global.X, global.Y, n), out var cellShadow))
                {
                    //if (cellShadow.Solid)
                    if (cellShadow.Block.Type != Block.Types.Air)
                    {
                        if (camera.CullingCheck(global.X, global.Y, n + 1, new Rectangle(-spriteBounds.Width / 2, -spriteBounds.Width / 4, spriteBounds.Width, spriteBounds.Width / 2), out _))
                        {
                            //ShadowList.Add(new Shadow(global, shadowBounds));
                            ShadowList.Add(new Shadow(parent, new Vector3(global.X, global.Y, n + 1)));//, shadowBounds));
                        }
                        drawn = true;
                    }
                }
                n--;
            }
        }

        
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

            //SpriteComponent spr = new SpriteComponent(Bone.Copy(this.Body), Sprite) { Hidden = this.Hidden, Customization = new CharacterColors(this.Customization) };
            ////SpriteComponent spr = new SpriteComponent(Bone.Create(Body), new Sprite(this.Sprite)) { Hidden = this.Hidden, Customization = new CharacterColors(this.Customization) };
            //if (this.Body.Sprite != null)
            //    if (spr.Body.Sprite == this.Body.Sprite)
            //        throw new Exception();
            //spr.Materials = this.Materials;
            ////for (int i = 0; i < 4; i++)
            ////    spr.Orientations[i] = Bone.Copy(this.Orientations[i]);
            ///

            var comp = new SpriteComponent(this.Body, this.Sprite)
            {
                Materials = this.Materials.ToDictionary(t => t.Key, t => t.Value)
            };
            return comp;
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
            DrawForbidden(sb, camera, parent);
            EntityTextManager.DrawStackSize(sb, camera, parent);

        }

        private static void DrawForbidden(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (!parent.IsForbidden)
                return;
            if (camera.Zoom <= .5f)
                return;
            //var rect = this.Body.GetSprite(camera).GetSourceRect();
            var zoom = 1; //camera.Zoom;
            var pos = camera.GetScreenPosition(parent.Global) - new Vector2(UI.Icon.Cross.SourceRect.Width, UI.Icon.Cross.SourceRect.Height) * zoom / 2; ;// -new Vector2(UI.Icon.Cross.SourceRect.Width / 2, rect.Height * camera.Zoom);
            pos.Y -= UI.Icon.Cross.SourceRect.Height/2;
            UI.Icon.Cross.Draw(sb, pos, zoom);
        }
       


        internal override List<SaveTag> Save()
        {
            var list = new List<SaveTag>() { 
                new SaveTag(SaveTag.Types.Int, "Variation", (int)Variation),
                new SaveTag(SaveTag.Types.Int, "Orientation", (int)Orientation),
                //new SaveTag(SaveTag.Types.Compound, "Customization", this.Customization.Save()),
                this.Body.Save("Body")
            };

            //if (this.Materials != null)
            //{
            //    var matstag = new SaveTag(SaveTag.Types.Compound, "Materials");
            //    matstag.Add(this.Materials.Keys.Select(t => t.Name).Save("Keys"));
            //    matstag.Add(this.Materials.Values.Select(t => t.ID).Save("Values"));
            //    list.Add(matstag);
            //}
            return list;
        }
        internal override void Load(GameObject parent, SaveTag compTag)
        {
            this.Properties["Variation"] = (int)compTag["Variation"].Value;
            this.Properties["Orientation"] = (int)compTag["Orientation"].Value;
            //this.Customization = new CharacterColors(compTag.GetValue<SaveTag>("Customization"));

            //if (!compTag.TryGetTag("Customization", t => this.Customization = new CharacterColors(t)))
                this.Customization = new CharacterColors(this.Body).Randomize();


            //if (compTag.TryGetTag("Materials", t =>
            // {
            //     var keys = t["Keys"].Value as List<SaveTag>;
            //     var values = t["Values"].Value as List<SaveTag>;
            //     var keysBones = keys.Select(k => Def.GetDef<BoneDef>((string)k.Value)).ToArray();
            //     var valuesMaterials = values.Select(v => Material.GetMaterial((int)v.Value)).ToArray();
            //     //this.Materials = keysBones.Zip(valuesMaterials, (k, v) => new { k, v });

            //     for (int i = 0; i < keysBones.Length; i++)
            //     {
            //         this.Materials[keysBones[i]] = valuesMaterials[i];
            //     }

            // }));

            compTag.TryGetTag("Body", t =>
            {
                this.Body.Load(t);
            });

            if(this.Body.Material == null)
            {
                this.Body.Material = parent.Def.DefaultMaterial;
                Log.WriteToFile($"{parent.DebugName}'s body material was null, defaulting to {parent.Def.DefaultMaterial?.DebugName}");
            }
        }

        

        public override void Write(System.IO.BinaryWriter w)
        {
            this.Customization.Write(w);
            //this.DefaultBody.Write(w);

            this.Body.Write(w);

            
            //w.Write(this.Materials.Keys.Select(k => k.Name).ToArray());
            //w.Write(this.Materials.Values.Select(k => k.ID).ToArray());


        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Customization = new CharacterColors(r);
            //this.DefaultBody.Read(r);
            this.Body.Read(r);
           

            //var matkeys = r.ReadStringArray();
            //var matvalues = r.ReadIntArray();
            //this.Materials = matkeys.ToDictionary(matvalues, k => Start_a_Town_.Def.GetDef<BoneDef>(k), v => Material.GetMaterial(v));
        }

        public static Bone GetRootBone(GameObject parent)
        {
            if (parent == null)
                return null;
            return parent.GetComponent<SpriteComponent>().Body;
        }

        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            //var mat = this.Body.Material;
            //tooltip.AddControlsBottomLeft(new Label(string.Format("{0}: {1}", this.Body.Def.Name, mat?.Name ?? "undefined")) { TextColor = mat?.Color ?? Color.Gray });
            foreach (var b in this.Body.GetAllBones())
            {
                var mat = b.Material;
                tooltip.AddControlsBottomLeft(new Label(string.Format("{0}: {1}", b.Def.Label, mat?.Name ?? "undefined")) { TextColor = mat?.Color ?? Color.Gray });
            }
            //foreach (var m in this.Materials)
            //{
            //    tooltip.AddControlsBottomLeft(new Label(string.Format("{0}: {1}", m.Key.Label, m.Value)) { TextColor = m.Value.Color });
            //}
        }

        class Props : ComponentProps
        {
            public override Type CompType => typeof(SpriteComponent);
        }

        internal bool HasMatchingBody(GameObject otherItem)
        {
            //var otherProps = otherItem.GetComponent<SpriteComponent>().BoneProps;
            //foreach (var thisProp in this.BoneProps)
            //{
            //    if (!otherProps.TryGetValue(thisProp.Key, out var otherProp))
            //        return false;
            //    if (thisProp.Value.Material != otherProp.Material)
            //        return false;
            //}
            //return true;

            var bones = this.Body.GetAllBones();
            var other = otherItem.Body;
            foreach (var b in bones)
            {
                if (!other.TryFindBone(b.Def, out var otherb))
                    return false;
                if (b.Material != otherb.Material)
                    return false;
            }
            return true;

            //var thismats = this.Materials;
            //var othermats = otherItem.GetComponent<SpriteComponent>().Materials;
            //foreach (var thismat in thismats)
            //{
            //    if (!othermats.TryGetValue(thismat.Key, out var othermat))
            //        return false;
            //    if (thismat.Value != othermat)
            //        return false;
            //}
            //return true;
        }
    }
}
