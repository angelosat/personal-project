using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_
{
    public class MouseMap
    {
        public Color[][][] Map, MapBack;
        public bool Multifaceted;
        public Texture2D Texture, TextureBack;
        public MouseMap(Color[][][] mouseMap, bool multiFaceted = false)
        {
            this.Map = mouseMap;
            this.Multifaceted = multiFaceted;
        }
        public MouseMap(Texture2D texture, Rectangle[][] rect, bool multiFaceted = false)
        {
            this.Texture = texture;
            this.TextureBack = this.Texture;
            //foreach (Rectangle[] var in rect)
            //    foreach (Rectangle or in var)
            this.Map = new Color[rect.Length][][];
         //   this.Map[0] = new Color[1][];
            this.MapBack = new Color[rect.Length][][];
         //   this.MapBack[0] = new Color[1][];
            for (int i = 0; i < rect.Length; i++)
            {
       //         Rectangle[] var = rect[i];
                this.Map[i] = new Color[rect[i].Length][];
                this.MapBack[i] = new Color[rect[i].Length][];
                for (int j = 0; j < rect[i].Length; j++)
                {
                    this.Map[i][j] = new Color[texture.Width * texture.Height];
                    this.MapBack[i][j] = this.Map[i][j];
                    texture.GetData(0, null, Map[i][j], 0, texture.Width * texture.Height);
                }
            }
            this.Multifaceted = multiFaceted;
        }
        public MouseMap(Texture2D texture, Rectangle rect, bool multiFaceted = false)
        {
            this.Texture = texture;
            this.TextureBack = this.Texture;
            this.Map = new Color[1][][];
            this.Map[0] = new Color[1][];
            this.MapBack = new Color[1][][];
            this.MapBack[0] = new Color[1][];
            this.Map[0][0] = new Color[rect.Width * rect.Height];
            this.MapBack = this.Map;
            texture.GetData(0, rect, this.Map[0][0], 0, rect.Width * rect.Height);
            this.Multifaceted = multiFaceted;
        }
        public MouseMap(Texture2D front, Texture2D back, Rectangle rect, bool multiFaceted = false)
        {
            this.Texture = front;
            this.TextureBack = back;
            this.Map = new Color[1][][];
            this.Map[0] = new Color[1][];
            this.Map[0][0] = new Color[rect.Width * rect.Height];
            this.MapBack = new Color[1][][];
            this.MapBack[0] = new Color[1][];
            this.MapBack[0][0] = new Color[rect.Width * rect.Height];
            front.GetData(0, rect, Map[0][0], 0, rect.Width * rect.Height);
            back.GetData(0, rect, MapBack[0][0], 0, rect.Width * rect.Height);
            this.Multifaceted = multiFaceted;
        }
        public bool HitTest(int x, int y, int variation = 0, int orientation = 0)
        {
            Color color = Map[variation][orientation][y * Texture.Width + x];
            return color.A > 0;
        }

        public bool HitTest(int x, int y, out Color color, int variation = 0, int orientation = 0)
        {
            Color c = Map[variation][orientation][y * Texture.Width + x];
            color = c;
            return color.A > 0;
        }
        public bool HitTest(int x, int y, out Vector3 vector, int variation = 0, int orientation = 0)
        {
            //bool alt = InputState.IsKeyDown(System.Windows.Forms.Keys.RShiftKey);
            bool alt = InputState.IsKeyDown(System.Windows.Forms.Keys.Menu);
            Color[][][] map = alt ? MapBack : Map;

            Color c = map[variation][orientation][y * Texture.Width + x];
            //vector = new Vector3(c.R, c.G, c.B) * (alt ? -1 : 1);
            // see if it's better to use the "behind" key to just return the bottom face
            //vector = alt ? new Vector3(0,0,-1) : new Vector3(c.R, c.G, c.B);// * (alt ? -1 : 1);
            var sampled = new Vector3(c.R, c.G, c.B);
            vector = alt ? -sampled : sampled;// * (alt ? -1 : 1);
            return c.A > 0;
        }
        public bool HitTest(Rectangle source, int x, int y, out Vector3 vector, int variation = 0, int orientation = 0)
        {
            bool alt = InputState.IsKeyDown(System.Windows.Forms.Keys.RShiftKey);//Menu);
            Color[][][] map = alt ? MapBack : Map;
            //   Color c = map[y * Texture.Width + x];
            Color c = map[variation][orientation][y * source.Width + x];
            vector = new Vector3(c.R, c.G, c.B) * (alt ? -1 : 1);
            // Console.WriteLine(alt + " " + vector);
            return c.A > 0;
        }
    }

    public class Sprite : IDisposable
    {
        static public AtlasWithDepth Atlas = new AtlasWithDepth("Entities");

        public Dictionary<string, Sprite> Overlays = new Dictionary<string, Sprite>();

        public enum OrientationType { South, West, North, East };
        public string Name;
        public string OverlayName = "";
        Texture2D _Texture;
        public Texture2D Texture //{ get; set; }
        {
            get
            {
                if (AtlasToken != null)
                    return Atlas.Texture;
                return _Texture;
            }
            set 
            {
                _Texture = value; 
            }
        }
        public Rectangle[][] SourceRects;
        public Vector2 Origin;
        public MouseMap MouseMap;
        Color _Tint = Color.Transparent;// Color.White;
        public Color Tint
        {
            get
            {
                //if (this.Material != null)
                //    return this.Material.Color;
                return this._Tint;
            }
            set
            {
                this._Tint = value;
            }
        }
        public Material Material;
        float _Shininess = 0;
        public float Shininess
        {
            get
            {
                if (this.Material != null)
                    return this.Material.Type.Shininess;
                return this._Shininess;
            }
            set
            {
                this._Shininess = value;
            }
        }
        public Rectangle[][] Highlights;
        public float Alpha = 1;
        public Color[] ColorArray;
        //public Color[] ColorArray { get { return this.AtlasToken.ColorArray; } set { } }

        /// <summary>
        /// this is not currently used. maybe use it as a point of origin for when the sprite is attached to a parent bone? and have the origin field seperately for when it's not attached?
        /// </summary>
        public Vector2 Joint;
        public Rectangle this[int variation, int orientation] { get { return SourceRects[variation][orientation]; } }
        public int Variation = 0, Orientation = 0;
        public Vector2 AtlasCoords;// { get; set; }
        //public Atlas.Node AtlasNode { get; set; }
        public AtlasWithDepth.Node.Token AtlasToken;// { get; set; }
        //public Atlas.Node.Token DepthToken { get; set; }
        public void Dispose()
        {
            if (!Texture.IsNull())
                Texture.Dispose();
        }

        static Dictionary<string, Atlas.Node.Token> _Dictionary;
        public static Dictionary<string, Atlas.Node.Token> Dictionary
        {
            get
            {
                if (_Dictionary.IsNull())
                    _Dictionary = new Dictionary<string, Atlas.Node.Token>();// Initialize();
                return _Dictionary;
            }
        }
        public static Sprite Shadow;
        //static public Sprite[][] BlockHighlights;
        static public Dictionary<Vector3, Sprite> BlockFaceHighlights = new Dictionary<Vector3, Sprite>();
        static public Sprite BlockHighlight, BlockHightlightBack;
        //static public Texture2D CubeDepth, HalfCubeDepth;
        static public readonly Texture2D CubeDepth = Game1.Instance.Content.Load<Texture2D>("Graphics/cubedepth9");
        static public readonly Texture2D HalfCubeDepth = Game1.Instance.Content.Load<Texture2D>("Graphics/cubehalfdepth9");

        static public void Initialize()
        {
            Shadow = new Sprite("isoShadow", new Vector2(16, 8));
            BlockFaceHighlights[Vector3.UnitX] = new Sprite("blocks/highlightwest", "blockDepthFar", Block.OriginCenter);
            BlockFaceHighlights[Vector3.UnitY] = new Sprite("blocks/highlightnorth", "blockDepthFarLeft", Block.OriginCenter);
            BlockFaceHighlights[Vector3.UnitZ] = new Sprite("blocks/highlightdown", "blockDepthFarRight", Block.OriginCenter);
            BlockHighlight = new Sprite("blocks/highlightfull", Map.BlockDepthMap) { Origin = Block.OriginCenter };
            BlockHightlightBack = new Sprite("blocks/highlightfullback", Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepth09back")) { Origin = Block.OriginCenter };

            //LoadContent();
            Sprite.Atlas.Initialize();
            //Sprite.DepthAtlas.Initialize();
        }

        //public static void LoadContent()
        //{
        //    CubeDepth = Game1.Instance.Content.Load<Texture2D>("Graphics/cubedepth9");
        //    HalfCubeDepth = Game1.Instance.Content.Load<Texture2D>("Graphics/cubehalfdepth9");
        //}

        public bool TryGetMouseMap(out MouseMap map)
        { map = MouseMap; return map != null; }

        public Rectangle GetSourceRect()
        {
            //return this.SourceRects[this.Variation][this.Orientation];
            return this.AtlasToken.Rectangle;
        }
        //public Sprite()
        //{

        //}
        public Sprite(AtlasWithDepth.Node.Token token)
        {
            this.AtlasToken = token;
        }
        public Sprite(Sprite toClone)
        {
            this.Texture = toClone.Texture;
            this.SourceRects = toClone.SourceRects;
            this.Origin = toClone.Origin;
            this.Joint = toClone.Joint;
            this.MouseMap = toClone.MouseMap;
            this.Highlights = toClone.Highlights;
            this.Name = toClone.Name;
            this.Alpha = toClone.Alpha;
            this.AtlasToken = toClone.AtlasToken;
            this.ColorArray = toClone.ColorArray;
            this.Tint = toClone.Tint;
            this.Shininess = toClone.Shininess;
            this.OverlayName = toClone.OverlayName;
            this.Material = toClone.Material;
            //this.Overlays = toClone.Overlays;
            foreach (var ol in toClone.Overlays)
                this.Overlays.Add(ol.Key, new Sprite(ol.Value));
        }
        public Sprite(Texture2D texture, Rectangle[][] sourcerect, Vector2 origin, Vector2 joint, MouseMap mousemap = null, Rectangle[][] highlights = null)
            : this(texture, sourcerect, origin, mousemap, highlights)
        {
            this.Joint = joint;
        }
        public Sprite(Texture2D texture, Rectangle[][] sourcerect, Vector2 origin, MouseMap mousemap = null, Rectangle[][] highlights = null)
        {
            this.Highlights = highlights;
            this.Name = texture.Name;
            Texture = texture;
            SourceRects = sourcerect;
            Joint = Origin = origin;
            if (mousemap != null)
                MouseMap = mousemap;
            else
                MouseMap = new MouseMap(texture, sourcerect);//, SourceRect[0][0]);
            Alpha = 1;
            // Joint = origin;// new Vector2(SourceRects.First().First().Width / 2, SourceRects.First().First().Height / 2);
            ColorArray = CreateColorArray(this);
        }
        public Sprite(string assetName) : this(assetName, Vector2.Zero) { }
        //public Sprite(string name, string assetName) : this(name, assetName, Vector2.Zero) { }
        public Sprite(string assetName, string depthMap, Vector2 origin)
        {
            this.Name = assetName;

            Origin = origin + Vector2.One * Borders.Thickness;

            Texture2D texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + assetName);
            this.SourceRects = new Rectangle[][] { new Rectangle[] { texture.Bounds } };

                MouseMap = new MouseMap(texture, texture.Bounds);

            this.Texture = Atlas.Texture;
            AtlasToken = Atlas.Load(assetName, depthMap);
            //this.DepthToken = DepthAtlas.Load(depthMap);
            this.ColorArray = this.AtlasToken.ColorArray;

            Joint = new Vector2(SourceRects.First().First().Width / 2, SourceRects.First().First().Height / 2);
        }
        public Sprite(string assetName, float layer)
        //    : this(assetName, assetName, layer) { }
        //public Sprite(string name, string assetName, float layer)
        {
            //this.Name = name;
            this.Name = assetName;
            Origin = Vector2.Zero;

            Texture2D texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + assetName);
            this.SourceRects = new Rectangle[][] { new Rectangle[] { texture.Bounds } };

            MouseMap = new MouseMap(texture, texture.Bounds);

            this.Texture = Atlas.Texture;
            //AtlasToken = Atlas.Load(assetName);
            //this.DepthToken = DepthAtlas.Load(assetName + "-z", Borders.GenerateDepthTexture(this.AtlasToken.Texture, layer));
            AtlasToken = Atlas.Load(assetName, layer);
            this.ColorArray = this.AtlasToken.ColorArray;
            Joint = new Vector2(SourceRects.First().First().Width / 2, SourceRects.First().First().Height / 2);
            
        }
        public Sprite(string assetName, Texture2D depthMap)
        {
            this.Name = assetName;
            Origin = Vector2.Zero;

            Texture2D texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + assetName);
            this.SourceRects = new Rectangle[][] { new Rectangle[] { texture.Bounds } };
            
                MouseMap = new MouseMap(texture, texture.Bounds);

            this.Texture = Atlas.Texture;
            AtlasToken = Atlas.Load(assetName, depthMap);// new Graphics.Atlas.Node.Token(assetName);
            //this.DepthToken = DepthAtlas.Load(assetName + "-z", Borders.GenerateDepthTexture(this.AtlasToken.Texture, depthMap));
            this.ColorArray = this.AtlasToken.ColorArray;
            Joint = new Vector2(SourceRects.First().First().Width / 2, SourceRects.First().First().Height / 2);
        }
        public Sprite(string assetName, string depthMapName, Vector2 origin, Vector2 joint)
            : this(assetName, depthMapName, origin)
        {
            this.Joint = joint;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="origin"></param>
        /// <param name="mousemap"></param>
        /// <param name="highlights"></param>
        /// <param name="alpha"></param>
        public Sprite(string assetName, Vector2 origin, MouseMap mousemap = null, Rectangle[][] highlights = null, float alpha = 1)
        {
            this.Name = assetName;
            this.Alpha = alpha;
            Origin = origin + Vector2.One * Borders.Thickness;

            Texture2D texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + assetName);
            this.SourceRects = new Rectangle[][] { new Rectangle[] { texture.Bounds } };
            //ColorArray = CreateColorArray(texture);
            if (mousemap != null)
                MouseMap = mousemap;
            else
                MouseMap = new MouseMap(texture, texture.Bounds);

            this.Texture = Atlas.Texture;
            AtlasToken = Atlas.Load(assetName);// new Graphics.Atlas.Node.Token(assetName);
            //this.DepthToken = DepthAtlas.Load(assetName + "-z", Borders.GenerateDepthTexture(this.AtlasToken.Texture));//, Map.BlockDepthMap));
            this.ColorArray = this.AtlasToken.ColorArray;
            Joint = new Vector2(SourceRects.First().First().Width / 2, SourceRects.First().First().Height / 2);
        }
        public Sprite(string assetName, Vector2 origin, Vector2 joint, MouseMap mousemap = null, Rectangle[][] highlights = null, float alpha = 1)
        {
            //this.Name = name;
            this.Name = assetName;
            this.Alpha = alpha;
            Origin = origin + Vector2.One * Borders.Thickness;
            
            Texture2D texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + assetName);
            this.SourceRects = new Rectangle[][] { new Rectangle[] { texture.Bounds } };
            //ColorArray = CreateColorArray(texture);
            if (mousemap != null)
                MouseMap = mousemap;
            else
                MouseMap = new MouseMap(texture, texture.Bounds);

            this.Texture = Atlas.Texture;
            AtlasToken = Atlas.Load(assetName);// new Graphics.Atlas.Node.Token(assetName);
            //this.DepthToken = DepthAtlas.Load(assetName + "-z", Borders.GenerateDepthTexture(this.AtlasToken.Texture));
            this.ColorArray = this.AtlasToken.ColorArray;


            Joint = joint + Vector2.One * Graphics.Borders.Thickness;// new Vector2(SourceRects.First().First().Width / 2, SourceRects.First().First().Height / 2);
        }

        /// <summary>
        /// Creates an array used for hit testing
        /// </summary>
        static public Color[] CreateColorArray(Sprite sprite)
        {
            Rectangle source = sprite.SourceRects[0][0];
            Color[] spriteMap = new Color[source.Width * source.Height];
            sprite.Texture.GetData(0, source, spriteMap, 0, source.Width * source.Height);
            return spriteMap;
        }
        static public Color[] CreateColorArray(Texture2D tex)
        {
            Rectangle source = tex.Bounds;// sprite.SourceRects[0][0];
            Color[] spriteMap = new Color[source.Width * source.Height];
            tex.GetData(0, source, spriteMap, 0, source.Width * source.Height);
            return spriteMap;
        }

        //public Rectangle GetBounds()
        //{ return new Rectangle(-(int)Origin.X, -(int)Origin.Y, SourceRects[0][0].Width, SourceRects[0][0].Height); }
        //public Rectangle GetBounds()
        //{ return new Rectangle(-(int)Origin.X, -(int)Origin.Y, this.AtlasToken.Rectangle.Width, this.AtlasToken.Rectangle.Height); }
        public Rectangle GetBounds()
        //{ return new Rectangle(-(int)Origin.X, -(int)Origin.Y, SourceRects[0][0].Width + 2 * Graphics.Borders.Thickness, SourceRects[0][0].Height + 2 * Graphics.Borders.Thickness); }
        { return new Rectangle(-(int)Origin.X, -(int)Origin.Y, this.AtlasToken.Rectangle.Width + 2 * Graphics.Borders.Thickness, this.AtlasToken.Rectangle.Height + 2 * Graphics.Borders.Thickness); }

        public Sprite AddOverlay(string overlayName, Sprite overlay)
        {
            this.Overlays.Add(overlayName, overlay);
            return this;
        }
        public List<Sprite> GetOverlays()
        {
            List<Sprite> list = new List<Sprite>() { this };
            foreach (var child in this.Overlays.Values)
            {
                list.AddRange(child.GetOverlays());
            }
            return list;
        }

        //public override string ToString()
        //{
        //    return Texture.Name + 
        //        "
        //}

        public UI.PictureBox ToPictureBox()
        {
            return new UI.PictureBox(Vector2.Zero, this.Texture, this.SourceRects[0][0]);
            //return new UI.PictureBox(Vector2.Zero, this.AtlasToken.Texture, this.AtlasToken.Rectangle);
        }

        static Sprite _Default = new Sprite("default", new Vector2(16, 24), new Vector2(16, 24));
        static public Sprite Default// = new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[0] } }, new Vector2(16, 16));
        {
            get
            {
                return _Default;
                if (_Default.IsNull())
                    _Default = new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[0] } }, new Vector2(16, 16));
                return _Default;
            }// new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[0] } }, new Vector2(16, 16)); }
        }

        public bool HitTest(Camera camera, Rectangle bounds, Rectangle hitRect)
        {
            if (!bounds.Intersects(hitRect))
                return false;

            int xx = (int)((hitRect.X - bounds.X) / (float)camera.Zoom);
            int yy = (int)((hitRect.Y - bounds.Y) / (float)camera.Zoom);

            // TODO: fix face detection
            //if (Sprite.MouseMap.HitTest(src, xx, yy, out face, 0, Orientation))
            //{
            // TODO: have the color array generated on creation of sprite and cache it
            Color[] spriteMap = this.ColorArray;
            Color c = spriteMap[yy * Texture.Width + xx];
            if (c.A == 0)
                return false;
            return true;
        }
        public bool HitTest(Camera camera, float zoom, Rectangle bounds, Rectangle hitRect)
        {
            if (!bounds.Intersects(hitRect))
                return false;

            int xx = (int)((hitRect.X - bounds.X) / zoom);
            int yy = (int)((hitRect.Y - bounds.Y) / zoom);

            // TODO: fix face detection
            //if (Sprite.MouseMap.HitTest(src, xx, yy, out face, 0, Orientation))
            //{
            // TODO: have the color array generated on creation of sprite and cache it
            Color[] spriteMap = this.ColorArray;
            Color c = spriteMap[yy * Texture.Width + xx];
            if (c.A == 0)
                return false;
            return true;
        }
        public bool HitTest(Camera camera, Rectangle bounds)
        {
            if (!bounds.Intersects(UI.UIManager.MouseRect))
                return false;

            int xx = (int)((UI.UIManager.Mouse.X - bounds.X) / (float)camera.Zoom);
            int yy = (int)((UI.UIManager.Mouse.Y - bounds.Y) / (float)camera.Zoom);

            Color[] spriteMap = this.ColorArray;
            Color c = spriteMap[yy * this.AtlasToken.Rectangle.Width + xx];
            if (c.A == 0)
                return false;
            return true;
        }

        public void Draw(MySpriteBatch sb, Vector2 screenPos, Color color, float rotation, Vector2 origin, float scale, SpriteEffects sprFx, float depth)
        {
            //if (this.AtlasToken != null)
            //    sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, scale, color.Multiply(this.Tint), sprFx, depth); //color.Multiply(this.Tint)
            //foreach (var ol in this.Overlays.Values)
            //    ol.Draw(sb, screenPos, color, rotation, origin, scale, sprFx, depth);
            var c = color.Multiply(this.Tint) * this.Alpha;
            if (this.AtlasToken != null)
                sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, scale, c, sprFx, depth); //color.Multiply(this.Tint)
            foreach (var ol in this.Overlays.Values)
                ol.Draw(sb, screenPos, color, rotation, origin, scale, sprFx, depth);
        }
        public void Draw(MySpriteBatch sb, Vector2 screenPos, Color sky, Color block, Color tint, Color fog, float rotation, Vector2 origin, float scale, SpriteEffects sprFx, float depth)
        {
            //var c = color.Multiply(this.Tint) * this.Alpha;
            //c.A = (byte)(this.Shininess * 255);
            var matcol = this.Material != null ? new Color(this.Material.Color.R, this.Material.Color.G, this.Material.Color.B, (byte)(this.Material.Type.Shininess * 255)) : new Color(1f,1f,1f,0f);
            var t = tint; //this.Tint;
            if (this.AtlasToken != null)
                //sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, new Vector2(scale), sky, block, c, fog, sprFx, depth); //color.Multiply(this.Tint)
                sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, new Vector2(scale), sky, block, matcol, t, fog, sprFx, depth); //color.Multiply(this.Tint)

            foreach (var ol in this.Overlays.Values)
                ol.Draw(sb, screenPos, sky, block, tint, fog, rotation, origin, scale, sprFx, depth);
        }
        public void Draw(MySpriteBatch sb, Vector2 screenPos, Material material, Color sky, Color block, Color tint, Color fog, float rotation, Vector2 origin, float scale, SpriteEffects sprFx, float depth)
        {
            if (material == null) // TEMPORARY UNTIL I REMOVE MATERIAL FROM SPRITE
                material = this.Material;
            var matcol = material != null ? new Color(material.Color.R, material.Color.G, material.Color.B, (byte)(material.Type.Shininess * 255)) : new Color(1f, 1f, 1f, 0f);
            var t = tint; //this.Tint;
            if (this.AtlasToken != null)
                //sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, new Vector2(scale), sky, block, c, fog, sprFx, depth); //color.Multiply(this.Tint)
                sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, new Vector2(scale), sky, block, matcol, t, fog, sprFx, depth); //color.Multiply(this.Tint)

            foreach (var ol in this.Overlays.Values)
                ol.Draw(sb, screenPos, sky, block, tint, fog, rotation, origin, scale, sprFx, depth);
        }
        public void Draw(MySpriteBatch sb, Vector2 screenPos, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects sprFx, float depth)
        {
            sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, rotation, origin, scale, Color.White, sprFx, depth);
            foreach (var ol in this.Overlays.Values)
                ol.Draw(sb, screenPos, color, rotation, origin, scale, sprFx, depth);
        }
        public void Draw(SpriteBatch sb, Vector2 screenPos, Color color, float rotation, Vector2 origin, float scale, SpriteEffects sprFx, float depth)
        {
            if (this.AtlasToken != null)
                sb.Draw(Atlas.Texture, screenPos, this.AtlasToken.Rectangle, color.Multiply(this.Tint), rotation, origin, scale, sprFx, depth);
            else
                if (!this.Texture.IsNull())
                sb.Draw(this.Texture, screenPos, this.SourceRects.First().First(), color.Multiply(this.Tint), rotation, origin, scale, sprFx, depth);
            foreach (var ol in this.Overlays.Values)
                ol.Draw(sb, screenPos, color, rotation, origin, scale, sprFx, depth);
        }

        public void Draw(MySpriteBatch sb, Vector3 global, Camera cam, Color color)
        {
            var bounds = cam.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, cam);
            sb.Draw(this.AtlasToken.Atlas.Texture, pos, this.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, color, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, depth);
        }
    }
}
