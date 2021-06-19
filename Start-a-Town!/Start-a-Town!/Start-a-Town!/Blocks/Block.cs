using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections;
using Start_a_Town_.Net;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Particles;
using Start_a_Town_.Blocks;
using Start_a_Town_.GameModes;
using Start_a_Town_.Tokens;

namespace Start_a_Town_
{
    abstract public partial class Block : ISlottable, ITooltippable
    {
        public class Data
        {
            public byte Value { get; set; }
            public Data(byte value = 0)
            {
                this.Value = value;
            }
        }

        public class DefaultState : IBlockState
        {
            public void Apply(IMap map, Vector3 global)
            { }
            public void Apply(ref byte data)
            {
            }
            public void Apply(Block.Data data)
            {
            }
            public void FromMaterial(GameObject material) { }
            public Color GetTint(byte d)
            { return Color.White; }
            public string GetName(byte d)
            {
                return "";
            }
        }
        //static public AtlasWithDepth Atlas = Sprite.Atlas;//
        //static public AtlasWithDepth Atlas = new AtlasWithDepth("Blocks"); ///Sprite.Atlas;//

        static Dictionary<Block, GameObject> BlockObjects = new Dictionary<Block, GameObject>();

        static public Color[] BlockCoordinatesFull,
            BlockCoordinatesHalf,
            BlockCoordinatesQuarter;
        static public void Initialize()
        {
            BlockCoordinatesFull = new Color[32 * 40];
            Game1.Instance.Content.Load<Texture2D>("Graphics/goodUV").GetData<Color>(BlockCoordinatesFull, 0, 32 * 40);
            BlockCoordinatesHalf = new Color[32 * 40];
            Game1.Instance.Content.Load<Texture2D>("Graphics/goodUVhalf").GetData<Color>(BlockCoordinatesHalf, 0, 32 * 40);
            BlockCoordinatesQuarter = new Color[32 * 40];
            Game1.Instance.Content.Load<Texture2D>("Graphics/goodUVquarter").GetData<Color>(BlockCoordinatesQuarter, 0, 32 * 40);


            Atlas.Initialize();
            LoadMouseMap();
            foreach (var item in Registry.Values)
                if (item.Type != Types.Air)
                {
                    BlockObjects[item] = item.ToObject();
                    //GameObject.Objects.Add(item.ToObject());
                    //GameObject.Objects.Add(BlockEntityPacked.Create(item, 0));
                }
        }

        private static void LoadMouseMap()
        {
            //MouseMapSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube");
            //MouseMapSpriteOpposite = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube - back"); //UpsideDown"); //
            //BlockMouseMap = new MouseMap(MouseMapSprite, MouseMapSpriteOpposite, MouseMapSprite.Bounds, true);
        }

        protected static AtlasDepthNormals.Node.Token LoadTexture(string name, string localfilepath)
        {
            return Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/" + localfilepath).ToGrayscale(), name);
        }

        //static public Texture2D MouseMapSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube");
        //static public Texture2D NormalMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockNormalsFilled"); //"Graphics/mousemap - Cube");
        //static public Texture2D HalfBlockNormalMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockHalfNormalsFilled"); //"Graphics/mousemap - Cube");
        //static public Texture2D HalfBlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockHalfDepth09"); //"Graphics/mousemap - Cube");
        //static public Texture2D MouseMapSpriteOpposite = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube - back");

        
        static public readonly Texture2D MouseMapSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube");
        static public readonly Texture2D HalfBlockMouseMapTexture = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube half");
        static public readonly Texture2D QuarterBlockMouseMapTexture = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube quarter");

        static public readonly Texture2D NormalMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockNormalsFilled19");//blockNormalsFilled"); //"Graphics/mousemap - Cube");
        static public readonly Texture2D BlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepth09height19"); //blockDepth09");

        static public readonly Texture2D HalfBlockNormalMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockHalfNormalsFilled"); //"Graphics/mousemap - Cube");
        static public readonly Texture2D HalfBlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockHalfDepth09"); //"Graphics/mousemap - Cube");

        static public readonly Texture2D QuarterBlockMapNormal = Game1.Instance.Content.Load<Texture2D>("Graphics/blockQuarterNormalsFilled"); //"Graphics/mousemap - Cube");
        static public readonly Texture2D QuarterBlockMapDepth = Game1.Instance.Content.Load<Texture2D>("Graphics/blockQuarterDepth09"); //"Graphics/mousemap - Cube");


        static public readonly Texture2D SliceBlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockOneDepth09"); //"Graphics/mousemap - Cube");
        //static public readonly Texture2D BlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepth09height19"); //blockDepth09");

        static public readonly Texture2D MouseMapSpriteOpposite = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube - back");

        static public readonly MouseMap BlockMouseMap = new MouseMap(MouseMapSprite, MouseMapSpriteOpposite, MouseMapSprite.Bounds, true);
        static public readonly MouseMap BlockHalfMouseMap = new MouseMap(HalfBlockMouseMapTexture, MouseMapSpriteOpposite, HalfBlockMouseMapTexture.Bounds, true);
        static public readonly MouseMap BlockQuarterMouseMap = new MouseMap(QuarterBlockMouseMapTexture, MouseMapSpriteOpposite, QuarterBlockMouseMapTexture.Bounds, true);

        static public AtlasDepthNormals Atlas = new AtlasDepthNormals("Blocks") { DefaultDepthMask = Map.BlockDepthMap, DefaultNormalMask = Block.NormalMap }; ///Sprite.Atlas;//

        //public static void LoadContent()
        //{
        //    MouseMapSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube");
        //    NormalMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockNormalsFilled"); //"Graphics/mousemap - Cube");
        //    HalfBlockNormalMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockHalfNormalsFilled"); //"Graphics/mousemap - Cube");
        //    HalfBlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockHalfDepth09"); //"Graphics/mousemap - Cube");
        //    MouseMapSpriteOpposite = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap cube - back");
        //}

        #region Interfaces
        public string GetName()
        {
            return this.Type.ToString();
        }
        public Icon GetIcon()
        {
            return new Icon(this.Variations.First());
        }
        public string GetCornerText()
        {
            return "";
        }
        public Color GetSlotColor()
        {
            return Color.White;
        }
        public void GetTooltipInfo(UI.Tooltip tooltip)
        {
            return;
            this.GetTooltip(tooltip as UI.Control);
            

            //tooltip.Controls.Add(new UI.Label(this.Type.ToString()) { Location = tooltip.Controls.BottomLeft });
            //var tasks = this.GetAvailableTasks();
            //if(tasks.Count>0)
            //    tasks.First().GetTooltip(tooltip);
        }
        public void GetTooltip(UI.Control tooltip)
        {
            return;

            //tooltip.Controls.Add(new UI.Label(this.Type.ToString()) { Location = tooltip.Controls.BottomLeft });
            //var tasks = this.GetAvailableTasks();
            //if (tasks.Count > 0)
            //    tasks.First().GetTooltip(tooltip);

            //var entity = this.GetBlockEntity();
            //if (entity != null)
            //    entity.GetTooltipInfo(tooltip as UI.Tooltip);
        }
        public virtual void GetTooltip(UI.Control tooltip, IMap map, Vector3 global)
        {
            if(this == Block.Air)
                return;
            //this.GetTooltip(tooltip);
            tooltip.Controls.Add(new UI.Label(this.Name) { Location = tooltip.Controls.BottomLeft, Font = UI.UIManager.FontBold, TextColor = Color.Goldenrod });
            var mat = this.GetMaterial(map.GetData(global));
            tooltip.Controls.Add(new UI.Label(mat.ToString()) { TextColorFunc = () => mat.Color, Location = tooltip.Controls.BottomLeft });
            tooltip.Controls.Add(new UI.Label(global.ToString()) { Location = tooltip.Controls.BottomLeft });
            tooltip.Controls.Add(new UI.Label("Chunk: " + map.GetChunk(global).MapCoords.ToString()) { Location = tooltip.Controls.BottomLeft });
            var cell = map.GetCell(global);
            var data = cell.BlockData;// map.GetData(global);
            var binary = Convert.ToString(data, 2);
            var datastring = Int32.Parse(binary).ToString("00000000");
            tooltip.Controls.Add(new UI.Label("BlockData: " + datastring + " (" + data.ToString() + ")") { Location = tooltip.Controls.BottomLeft });
            tooltip.Controls.Add(new UI.Label("Variation: " + cell.Variation.ToString()) { Location = tooltip.Controls.BottomLeft });
            tooltip.Controls.Add(new UI.Label("Orientation: " + cell.Orientation.ToString()) { Location = tooltip.Controls.BottomLeft });

            var blockentity = map.GetBlockEntity(global);
            if (blockentity != null)
                blockentity.GetTooltip(tooltip);
        }
        public virtual void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            //this.Draw(sb, pos, Color.White, Vector4.One, 1, 0, new Cell());
            var token = this.Variations.First();
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, this.BlockState.GetTint(0));
        }
        public virtual AtlasDepthNormals.Node.Token GetUIToken()
        {
            return this.Variations.First();
        }
        //public virtual void Draw(SpriteBatch sb, Vector2 pos, byte state)
        //{
        //    //this.Draw(sb, pos, Color.White, Vector4.One, 1, 0, new Cell());
        //    var token = this.Variations.First();
        //    sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, this.BlockState.GetTint(state));
        //}
        public virtual void DrawUI(SpriteBatch sb, Vector2 pos, byte state)
        {
            var token = this.Variations.First();
            //var c = this.GetColor(state);
            var c = this.BlockState.GetTint(state);
            c.A = 255;
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, c);
        }
        #endregion

        public override string ToString()
        {
            return "Block:" + this.Type.ToString();
        }

        public virtual string Name { get { return this.Type.ToString(); } }
        static public int Width = 32, Depth = 16, Height = 40, BlockHeight = 20;//19;// 16;
        //static public int Width = 32 + 2 * Borders.Thickness, Depth = 16, Height = 40 + 2 * Borders.Thickness, BlockHeight = 16;
        static public int HeightQuarter = Height / 4;

        public TokenCollection Tokens = new TokenCollection();

        static public Vector2 OriginCenter = new Vector2(Width/2f, Height - Depth/2f);//16, Height - BlockHeight);//16);
        static public readonly Vector2 Joint = new Vector2(Block.Width / 2, Block.Height);
        public enum Types : byte
        {
            //Empty,
            Air,
            Soil,
            Farmland,
            Water,
            Sand,
            Stone,
            Coal,

            Wall,
            Cobblestone,
            Grass,
            Flowers,
            WoodenDeck,
            //Light
            WallSlice,
            Iron,
            Scaffolding,
            Construction,
            Blueprint,
            EditorOrigin,
            EditorAir,
            Gravel,
            Door,
            Empty,
            WoodenFrame,
            Mineral,
            Bed,
            WoodPaneling,
            Smeltery,
            Chest,
            Sapling,
            Chair,
            Stool,
            Bricks,
            FlowersNew,
            Campfire,
            Window,
            Roof,
            Counter,
            Stairs,
            Workbench,
            CarpenterBench,
            Slab
        }
        static public Rectangle Bounds = new Rectangle(-(int)Block.OriginCenter.X, -(int)Block.OriginCenter.Y, Block.Width, Block.Height);
        //static public Rectangle Bounds = new Rectangle(-(int)Block.OriginCenter.X, -(int)Block.OriginCenter.Y, Block.Width + 2 * Borders.Thickness, Block.Height + 2 * Borders.Thickness);

        static public Rectangle[][] Create(int x, int y, int vars)
        {
            Rectangle[][] rects = new Rectangle[][] { new Rectangle[vars] };
            //{
               
            //}};
            for (int i = 0; i < vars; i++)
                rects[0][i] = new Rectangle((x + i) * Block.Width, y * Block.Height, Block.Width, Block.Height);
            return rects;
        }

        static public Rectangle[][] TileHighlights;
        //static public readonly MouseMap BlockMouseMap = new MouseMap(MouseMapSprite, MouseMapSpriteOpposite, MouseMapSprite.Bounds, true);

        static public MouseMap WallMouseMap, WallHalfMouseMap, WallQuarterMouseMap;

        static public SortedList<Types, Rectangle[][]> SourceRects;


        static public void DrawHighlight(SpriteBatch sb, Rectangle bounds)//Camera camera, )
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.None, 0);
            //camera.SpriteBatch.Draw(UI.UIManager.Highlight, new Vector2(bounds.X, bounds.Y), null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
        static public bool HitTest(Sprite tileSprite, Rectangle bounds, Camera camera, Controller controller, out Vector3 face)
        {
            //TODO optimize get_MouseRect
            if (bounds.Intersects(controller.MouseRect))
            {
                int xx = (int)((controller.msCurrent.X - bounds.X) / (float)camera.Zoom);
                int yy = (int)((controller.msCurrent.Y - bounds.Y) / (float)camera.Zoom);
                return tileSprite.MouseMap.HitTest(xx, yy, out face);
            }
            face = Vector3.Zero;
            return false;
        }
        static public void Build(Net.IObjectProvider net, GameObject builder, TargetArgs target, GameObject.Types consumeType, GameObject.Types blockType, Action<GameObject> onFinish)
        {
            if (InventoryComponent.ConsumeEquipped(builder, slot => slot.Object.ID == consumeType))
            //onFinish(GameObject.Create(blockType).Spawn(net.GetMap(), target.Object.Global + target.Face));
            //onFinish(net.Spawn(GameObject.Create(blockType).SetGlobal(target.Object.Global + target.Face)));
            {
                GameObject objBlock = GameObject.Create(blockType);//.SetGlobal(target.Object.Global + target.Face);
                //net.Spawn(objBlock, target.Object.Global + target.Face);
                //objBlock.Chunk.AddBlockObject(net.Map, objBlock, target.Object.Global + target.Face);
                objBlock.Global = target.Object.Global + target.Face;
                objBlock.Spawn(net);
                onFinish(objBlock);
            }
        }
        public virtual Color[] UV
        {
            get { return BlockCoordinatesFull; }
        }
        public virtual MouseMap MouseMap
        {
            get { return BlockMouseMap; }
        }
        // TODO find a way to make this method required for blocks tha have entity
        public virtual BlockEntity GetBlockEntity() 
        { return null; }

        //public readonly GameObject Entity;
        public GameObject GetEntity()
        {
            return BlockObjects[this];
            GameObject obj;
            GameObject.Objects.TryGetValue(this.EntityID, out obj);
            return obj;
        }
        //public readonly GameObject.Types Entity;
   //     public readonly Sprite Sprite;
        public readonly Block.Types Type;
        /// <summary>
        /// Defines the alpha channel of the block's sprite during drawing.
        /// </summary>
        public readonly float Transparency;
        /// <summary>
        /// Defines how fast the block can be dug?
        /// </summary>
        public readonly float Density;
        /// <summary>
        /// Defines how much light the block lets pass through it.
        /// </summary>
        public readonly bool Opaque;
        /// <summary>
        /// Defines whether the block can be walked upon (maybe combine this with density to make things like moving sand?)
        /// </summary>
        public readonly bool Solid;
        public virtual bool IsTargetable(Vector3 global)
        {
            return true;
        }


        public virtual Color DirtColor
        {
            get
            {
                return Color.White;
            }
        }
        protected virtual ParticleEmitterSphere GetDustEmitter()
        {
            var emitter = new ParticleEmitterSphere() //DustEmitter.Clone() as ParticleEmitterSphere;
            {
                Lifetime = Engine.TargetFps / 2f,
                Offset = Vector3.Zero,
                Rate = 0,
                ParticleWeight = 0f,//1f,
                ColorEnd = Color.White * .5f,
                ColorBegin = Color.White,
                SizeEnd = 1,
                SizeBegin = 3,
                Force = .01f,
                Friction = 0f
            };
            return emitter;
        }
        protected virtual ParticleEmitterSphere GetDirtEmitter()
        {
            //var block = parent.Map.GetBlock(parent.Global - Vector3.UnitZ * 0.1f);
            var dustcolor = this.DirtColor;
            var emitter = new ParticleEmitterSphere() //DustEmitter.Clone() as ParticleEmitterSphere;
            {
                Lifetime = Engine.TargetFps / 2f,
                Offset = Vector3.Zero,
                Rate = 0,
                ParticleWeight = 1f,//1f,
                ColorEnd = dustcolor,// * .5f,//Color.SaddleBrown * .5f,
                ColorBegin = dustcolor,// Color.SaddleBrown,
                SizeEnd = 2,
                SizeBegin = 2,
                Force = .05f
            };
            return emitter;
        }
        public virtual ParticleEmitterSphere GetEmitter() 
        { return this.GetDustEmitter(); }
        public List<Rectangle> GetParticleRects(int count)
        {
            var list = new List<Rectangle>();
            var sqrt = (int)Math.Sqrt(count);
            var rect = this.Variations[0].Rectangle;
            var w = rect.Width / sqrt;
            var h = rect.Height / sqrt;
            for (int i = 0; i < sqrt; i++)
                for (int j = 0; j < sqrt; j++)
                    list.Add(new Rectangle(rect.X + i * w, rect.Y + j * h, w, h));
            return list;
        }

        public virtual BlockConstruction GetRecipe()
        {
            //return null;
            return this.Recipe;
        }

        //MaterialType _MaterialType;
        //public MaterialType MaterialType
        //{
        //    get { return this._MaterialType ?? this.Material.Type; }
        //    set { this._MaterialType = value; }
        //}

        //Material Material { get; set; }
        public LootTable LootTable { get; set; }



        public virtual byte ParseData(string data)
        {
            return 0;
        }

        public BlockConstruction Recipe;
        public List<Components.Crafting.Reaction.Reagent> Reagents = new List<Components.Crafting.Reaction.Reagent>();
        //public List<GameObject.Types> Reagents = new List<GameObject.Types>();

        public readonly bool HasData;

        static public void UpdateBlocks()
        {
            foreach (var block in Registry.Values)
                block.Update();
        }
        internal static void UpdateBlocks(IMap map)
        {
            foreach (var block in Registry.Values)
                block.Update(map);
        }

       

        public virtual void Update() { }
        public virtual void Update(IMap map) { }
        /// <summary>
        /// TODO: maybe pass position of neighbor that changed?
        /// </summary>
        /// <param name="net"></param>
        /// <param name="global"></param>
        public virtual void NeighborChanged(IObjectProvider net, Vector3 global) { }
        public virtual void Placed(IObjectProvider net, Vector3 global) 
        {
            //global.TrySetCell(net, this.Type); 
            net.Map.SetBlock(global, this.Type);
        }
        public virtual void Removed(IObjectProvider net, Vector3 global) 
        {
            //global.TrySetCell(net, Block.Types.Air); 

        }
        public virtual void Place(IMap map, Vector3 global, byte data, int variation, int orientation)
        {
            //map.SetBlock(global, this.Type, data, variation);
            // TODO: change this method so it returns true or false depending if the block was placed succesfully? (if the cell was empty of any entities)
            // or keep it as it is and just only check if the cell is empty if the block is solid, otherwise don't check?
            //if (!map.IsEmpty(global))
            //    return;

            // TODO: ONLY CALL SETBLOCK ONCE WHEN PLACING BLOCKS
            map.SetBlock(global, this.Type, data, variation);
            map.GetCell(global).Orientation = orientation; // TODO: keep or remove orientation field for cells afterall???
            var entity = this.GetBlockEntity();
            if (entity != null)
                map.AddBlockEntity(global, entity);
        }
        public virtual void Remove(IMap map, Vector3 global)
        {
            // TODO: ONLY CALL SETBLOCK ONCE WHEN PLACING BLOCKS
            map.SetBlock(global, Block.Types.Air, 0, 0);
            var blockentity = map.RemoveBlockEntity(global);
            if (blockentity != null)
            {
                blockentity.Remove(map, global);
                blockentity.Dispose();
                map.GetNetwork().EventOccured(Message.Types.BlockEntityRemoved, blockentity, global);
            }

            // reenable physics of entities resting on block
            foreach (var entity in map.GetObjects(global - new Vector3(1,1,0), global + new Vector3(1,1,2)))
            {
                PhysicsComponent.Enable(entity);
            }
        }
        public virtual LootTable GetLootTable(byte data)
        {
            return this.LootTable;
        }
        public virtual void Break(IMap map, Vector3 global)
        {
            var net = map.Net;
            net.PopLoot(this.GetLootTable(net.Map.GetData(global)), global, Vector3.Zero);
            var blockentity = map.RemoveBlockEntity(global);
            if (blockentity != null)
            {
                blockentity.Break(map, global);
                blockentity.Dispose();
                net.EventOccured(Message.Types.BlockEntityRemoved, blockentity, global);
            }
            this.Remove(map, global);
            //net.SyncSetBlock(global, Block.Types.Air);
            //net.SetBlock(global, Block.Types.Air);
        }
        public virtual void Break(GameObject actor, Vector3 global)
        {
            var mat = Block.GetBlockMaterial(actor.Map, global);
            var net = actor.Net;
            net.PopLoot(this.GetLootTable(actor.Map.GetData(global)), global, Vector3.Zero);
            this.Remove(net.Map, global);

            var emitters = WorkComponent.GetEmitters(actor);
            if (emitters == null)
                return;

            var e = this.GetEmitter();
            e.Source = global + Vector3.UnitZ * 0.5f;
            e.SizeBegin = 1;
            e.SizeEnd = 1;
            e.ParticleWeight = 1;
            e.Radius = 1f;// .5f;
            e.Force = .1f;
            e.Friction = .5f;
            e.AlphaBegin = 1;
            e.AlphaEnd = 0;
            var color = mat.Color;// this.Material.Color;
            e.ColorBegin = color;
            e.ColorEnd = color;

            e.Lifetime = Engine.TargetFps * 2;
            var pieces = this.GetParticleRects(25);
            e.Emit(Block.Atlas.Texture, pieces, Vector3.Zero);
            emitters.Add(e);
        }
        public virtual bool IsSolid(IMap map, Vector3 global) 
        {
            //return this.Solid;
            var offset = global + 0.5f * new Vector3(1, 1, 0);
            offset = offset - offset.RoundXY();
            var h = this.GetHeight(offset.X, offset.Y);
            return this.Solid && offset.Z < h;
        }
        public virtual float GetDensity(IMap map, Vector3 global)
        {
            //return this.Solid;
            var offset = global + 0.5f * new Vector3(1, 1, 0);
            offset = offset - offset.RoundXY();
            var data = map.GetData(global);
            var h = this.GetHeight(data, offset.X, offset.Y);
            return offset.Z < h ? this.Density : 0;
        }
        public virtual float GetDensity(byte data, Vector3 global)
        {
            //var offset = global + 0.5f * new Vector3(1, 1, 0);
            //offset = offset - offset.FloorXY();// offset.RoundXY();
            var offset = global.ToBlock();
            var h = this.GetHeight(data, offset.X, offset.Y);
            return offset.Z < h ? this.Density : 0;
        }

        public virtual bool IsSolid(Cell cell) 
        {
            return this.Solid; 
        }
        public virtual bool IsSolid(Cell cell, Vector3 withinBlock) 
        { 
            //var h = this.GetHeight(withinBlock.X, withinBlock.Y);
            var h = this.GetHeight(cell.BlockData, withinBlock.X, withinBlock.Y);
            return this.Solid && withinBlock.Z < h; 
        }
        public virtual bool IsOpaque(IObjectProvider net, Vector3 global) { return this.Opaque; }// return true; }
        public virtual bool IsOpaque(Cell cell) { return this.Opaque; }// return true; }
        public virtual Material GetMaterial(IMap map, Vector3 global) 
        {
            return this.GetMaterial(map.GetData(global));
            //return null; 
        }
        //public virtual Material GetMaterial(byte blockdata) { return this.Material; }
        public abstract Material GetMaterial(byte blockdata);

        public virtual float GetTransparency(IObjectProvider net, Vector3 global) { return 0; }
        public virtual float GetDensity(IObjectProvider net, Vector3 global) { return 0; }
        public virtual void OnMessage(GameObject parent, ObjectEventArgs args) { }
        public virtual void RandomBlockUpdate(Net.IObjectProvider net, Vector3 global, Cell cell) { }
        //public virtual void OnMessage(GameObject parent, ObjectEventArgs args) { }
        protected virtual void HandleMessage(Vector3 global, ObjectEventArgs e) { }

        public static void HandleMessage(IObjectProvider net, Vector3 global, ObjectEventArgs e)
        {
            //global.GetBlock(net.Map).HandleMessage(global, e);
            net.Map.GetBlock(global).HandleMessage(global, e);

        }
        internal virtual void RemoteProcedureCall(IObjectProvider net, Vector3 vector3, Message.Types type, System.IO.BinaryReader r) { }

        //static Dictionary<Block.Types, Block> _Registry;
        static public Dictionary<Block.Types, Block> Registry = new Dictionary<Types, Block>();
        //{ get { if (_Registry.IsNull()) _Registry = new Dictionary<Types, Block>(); return _Registry; } }

        //static public void Initialize()
        //{
        //    BlockAtlas.Initialize();
        //}

        /// <summary>
        /// The index to the block's variation is stored within a cell. Add variations to this list so they can be picked at random whenever a block is placed.
        /// </summary>
        public List<AtlasDepthNormals.Node.Token> Variations = new List<AtlasDepthNormals.Node.Token>();
        public List<Sprite> Sprites = new List<Sprite>();
        string AssetName { get; set; }
        // TODO: turn this into a method SetAssetNames(param string[] names) ???
        protected string AssetNames
        {
            set
            {
                foreach (string name in value.Split(','))
                {
                    var token = Block.Atlas.Load("blocks/" + name.Trim(), Map.BlockDepthMap, Block.NormalMap);
                    this.Variations.Add(token);

                    //Sprite sprite = new Sprite(token);
                    //this.Sprites.Add(sprite);
                    //this.Variations.Add(sprite.AtlasToken);
                }
                this.AssetName = value.Split(',').First().Trim();
            }
        }
        Atlas.Node.Token SpriteToken;

        protected Block(Block.Types type, GameObject.Types entityType, float transparency = 0f, float density = 1f, bool opaque = true, bool solid = true)
            : this(type, transparency, density, opaque, solid)
        {
            //this.Entity = entityType;
        }
        protected Block(Block.Types type, float transparency = 0f, float density = 1f, bool opaque = true, bool solid = true)
        {
            this.Type = type;
            this.Transparency = transparency;
            this.Density = density;
            this.Opaque = opaque;
            this.Solid = solid;
            //this.Sprite = Block.TileSprites[type];
            //this.Entity = GameObject.Types.Default;
            //this.Entity = null;// GameObject.Types.Default;
            this.LootTable = new Components.LootTable();
            Registry[type] = this;

            //if(type != Types.Air)
            //GameObject.Objects.Add(this.ToObject());
        }

        public virtual void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        {
            //if (cell.Type == Types.Air)
            //    return;
            //sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[cell.Variation], zoom, Color.White, sunlight, blocklight, depth);
            this.Draw(sb, screenBounds, sunlight, blocklight, Color.White, zoom, depth, cell);
        }
        public virtual void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color tint, float zoom, float depth, Cell cell)
        {
            if (cell.Block.Type == Types.Air)
                return;
            //sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[cell.Variation], zoom, tint, sunlight, blocklight, depth);
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], zoom, tint, sunlight, blocklight, depth);

        }
        public virtual void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            if (cell.Block.Type == Types.Air)
                return;
            //sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[cell.Variation], zoom, tint, sunlight, blocklight, depth);
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], zoom, fog, tint, sunlight, blocklight, depth);
        }
        public virtual void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            if (cell.Block.Type == Types.Air)
                return;
            //tint.A = (byte)((1-this.Transparency) * 255);// *= this.Transparency;
            //tint *= (1 - this.Transparency);

            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], zoom, fog, tint, sunlight, blocklight, depth);
        }
        public virtual void Draw(Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, Cell cell)
        {
            this.Draw(blockCoordinates, camera, screenBounds, sunlight, blocklight, fog, tint, depth, cell.Variation, cell.Orientation, cell.BlockData);
            //if (cell.Block.Type == Types.Air)
            //    return;
            ////tint.A = (byte)((1-this.Transparency) * 255);// *= this.Transparency;
            ////tint *= (1 - this.Transparency);
            //var material = this.GetColor(cell.BlockData);
            ////camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], camera.Zoom, fog, color.Multiply(tint), sunlight, blocklight, depth);

            //camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, 
            //    this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], 
            //    camera.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth);

        }
        //public virtual void Draw(Camera camera, Vector4 screenBounds, int x, int y, int z, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        //{
        //    this.Draw(camera, screenBounds, sunlight, blocklight, fog, tint, depth, variation, orientation, data);
        //}
        public virtual MyVertex[] Draw(Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            return this.Draw(camera.SpriteBatch, blockCoordinates, camera, screenBounds, sunlight, blocklight, fog, tint, depth, variation, orientation, data);
        }
        public virtual MyVertex[] Draw(MySpriteBatch sb, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            if (this == Block.Air)
                return null;
            //tint.A = (byte)((1-this.Transparency) * 255);// *= this.Transparency;
            //tint *= (1 - this.Transparency);
            //var material = this.GetColor(data);
            var material = this.GetColorVector(data);
 
            //camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], camera.Zoom, fog, color.Multiply(tint), sunlight, blocklight, depth);
            var token = this.GetToken(variation, orientation, (int)camera.Rotation, data);// maybe change the method to accept double so i don't have to cast the camera rotation to int?
            return sb.DrawBlock(Block.Atlas.Texture, screenBounds,
                //this.Variations[Math.Min(variation, this.Variations.Count - 1)],
                token,
                camera.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, blockCoordinates);

        }
        public virtual MyVertex[] Draw(Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, Cell cell)
        {
            return this.Draw(chunk, blockCoordinates, camera, screenBounds, sunlight, blocklight, fog, tint, depth, cell.Variation, cell.Orientation, cell.BlockData);
        }

        public virtual MyVertex[] Draw(Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            if (this == Block.Air)
                return null;
            //tint.A = (byte)((1-this.Transparency) * 255);// *= this.Transparency;
            //tint *= (1 - this.Transparency);
            //var material = this.GetColor(data);
            var material = this.GetColorVector(data);

            //camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], camera.Zoom, fog, color.Multiply(tint), sunlight, blocklight, depth);
            var token = this.GetToken(variation, orientation, (int)camera.Rotation, data);// maybe change the method to accept double so i don't have to cast the camera rotation to int?
            return chunk.VertexBuffer.DrawBlock(Block.Atlas.Texture, screenBounds,
                //this.Variations[Math.Min(variation, this.Variations.Count - 1)],
                token,
                camera.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, blockCoordinates);
        }
        public virtual MyVertex[] Draw(MySpriteBatch opaquemesh, MySpriteBatch nonopaquemesh, MySpriteBatch transparentMesh, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            if (this == Block.Air)
                return null;
            //tint.A = (byte)((1-this.Transparency) * 255);// *= this.Transparency;
            //tint *= (1 - this.Transparency);
            //var material = this.GetColor(data);
            var material = this.GetColorVector(data);
            MySpriteBatch mesh = this.Opaque ? opaquemesh : nonopaquemesh;
            //camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], camera.Zoom, fog, color.Multiply(tint), sunlight, blocklight, depth);
            var token = this.GetToken(variation, orientation, (int)camera.Rotation, data);// maybe change the method to accept double so i don't have to cast the camera rotation to int?
            return mesh.DrawBlock(Block.Atlas.Texture, screenBounds,
                //this.Variations[Math.Min(variation, this.Variations.Count - 1)],
                token,
                camera.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, blockCoordinates);
        }
        public virtual void Draw(Camera camera, Vector3 global, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, Cell cell)
        {
            if (cell.Block.Type == Types.Air)
                return;
            var screenBounds = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, Block.Bounds, Vector2.Zero);// Block.OriginCenter);
            camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], camera.Zoom, fog, tint, sunlight, blocklight, depth);
        }
        public virtual void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, byte data)
        {
            if (this.Type == Types.Air)
                return;
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[0], zoom, fog, tint, sunlight, blocklight, depth);
        }
        public virtual void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        {
            //if (cell.Type == Types.Air)
            //    return;
            //Rectangle sourceRect = this.Variations[cell.Variation].Rectangle;
            //sb.DrawBlock(Block.Atlas.Texture, screenPos, sourceRect, zoom, Color.White, light, depth);
            this.Draw(sb, screenPos, sunlight, blocklight, Color.White, zoom, depth, cell);
        }
        public virtual void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, Color tint, float zoom, float depth, Cell cell)
        {
            if (cell.Block.Type == Types.Air)
                return;
            sb.DrawBlock(Block.Atlas.Texture, screenPos, this.Variations[cell.Variation], zoom, tint, sunlight, blocklight, depth);
            //Rectangle sourceRect = this.Variations[cell.Variation].Rectangle;
            //sb.DrawBlock(Block.Atlas.Texture, screenPos, sourceRect, zoom, tint, sunlight, blocklight, depth);
        }
        public virtual void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, Color tint, Color fog, float zoom, float depth, byte data)
        {
            if (this.Type == Types.Air)
                return;
            sb.DrawBlock(Block.Atlas.Texture, screenPos, this.Variations[0], zoom, tint, sunlight, blocklight, depth);
        }

        public virtual void DrawPreview(MySpriteBatch sb, IMap map, Vector3 global, Camera cam, byte data, int orientation = 0)
        {
            var pos = cam.GetScreenPosition(global);
            var depth = global.GetDrawDepth(map, cam);
            var screenPos = pos - Block.OriginCenter * cam.Zoom;
            var materialcolor = this.GetColor(data);
            //materialcolor = materialcolor.Multiply(Color.White); //*.66f;
            //materialcolor.A = 255 / 2;
            var tint = Color.White *.5f;
            //var token = this.GetAtlasNodeToken(data);
            var token = this.GetPreviewToken(0, orientation, (int)cam.Rotation, data); // change the method to accept double so i don't have to cast the camera rotation to int?

            var bounds = cam.GetScreenBoundsVector4(global.X, global.Y, global.Z, Block.Bounds, Vector2.Zero);
            //sb.DrawBlock(Block.Atlas.Texture, bounds, token, cam.Zoom, Color.Transparent, color, Color.White, Vector4.One, depth);
            sb.DrawBlock(Block.Atlas.Texture, bounds, token, cam.Zoom, Color.Transparent, tint, materialcolor, Color.White, Vector4.One, Vector4.Zero, depth);
        }

        public virtual AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.Variations[Math.Min(variation, this.Variations.Count - 1)];
        }
        public virtual AtlasDepthNormals.Node.Token GetPreviewToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.GetToken(variation, orientation, cameraRotation, data);
        }

        public virtual IBlockState BlockState { get { return new DefaultState(); } }

        public virtual Color GetColor(byte data)
        {
            return Color.White;
        }
        public virtual Vector4 GetColorVector(byte data)
        {
            return DefaultColorVector;
        }
        static Vector4 DefaultColorVector = Vector4.One;
        //protected virtual AtlasDepthNormals.Node.Token GetAtlasNodeToken(byte data)
        //{
        //    return this.Variations[0];
        //}

        public virtual List<byte> GetVariations()
        {
            List<byte> list = new List<byte>();
            //(from block in BlockConstruction.Dictionary )
            BlockConstruction constr = BlockConstruction.Dictionary.Values.FirstOrDefault(c => c.BlockProduct.Block == this);
            if (constr.IsNull())
            {
                list.Add(0);
                return list;
            }
            list.AddRange(from con in constr.GetVariants() select con.Data);
            return list;
        }
        public string GetName(byte p)
        {
            var statename = this.BlockState.GetName(p);
            return this.GetName() + (string.IsNullOrWhiteSpace(statename) ? "" : ":" + statename); 
        }
        internal virtual ContextAction GetContextRB(GameObject player, Vector3 global)
        {
            Dictionary<PlayerInput, Interaction> list = new Dictionary<PlayerInput, Interaction>();
            this.GetPlayerActionsWorld(player, global, list);
            var action = list.Values.FirstOrDefault();
            if (action != null)
                return new ContextAction(action) { Shortcut = PlayerInput.RButton };
            return null;
        }
        internal virtual ContextAction GetContextActivate(GameObject player, Vector3 global)
        {
            return null;
        }

        public virtual List<Interaction> GetAvailableTasks(IMap map, Vector3 global) 
        { 
            return new List<Interaction>(); 
        }
        public virtual ContextAction GetRightClickAction(Vector3 global) { return null; }

        public virtual void GetContextActions(GameObject player, Vector3 global, ContextArgs a)
        {
            var list = new Dictionary<PlayerInput, Interaction>();
            this.GetPlayerActionsWorld(player, global, list);
            var t = new TargetArgs(global);
            foreach (var i in list)
                //if (i.Value.InRange(Player.Actor, t))
                    //a.Actions.Add(new ContextAction(i.Key.ToString() + ": " + i.Value.Name, null, () => i.Value.Conditions.Evaluate(Player.Actor, t)));// () => true));
                a.Actions.Add(new ContextAction(i.Value) { Shortcut = i.Key });// () => true));

            return;



            var actions = new List<ContextAction>();
            actions.AddRange(from action in this.GetAvailableTasks(player.Map, global)
                             select new ContextAction(() => action.Name, () =>
                             {
                                 Client.Instance.Send(PacketType.Towns, new Towns.PacketAddJob(Player.Actor.Network.ID, new TargetArgs(global), action.Name).Write());
                                 //Client.PlayerRemoteCall(this, )
                             }));

            a.Actions = actions;
        }

        //public virtual Dictionary<PlayerInput, Interaction> GetPlayerActionsWorld()
        public virtual void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            var hauled = Components.PersonalInventoryComponent.GetHauling(Player.Actor);
            if (hauled.Object != null)
            {
                list.Add(PlayerInput.RButton, new DropCarriedSnap());
                return;
            }
            var mat = Block.GetBlockMaterial(player.Map, global);
            //if (this.MaterialType == null)
            if (mat.Type == null)
                return;
            var skill = mat.Type.SkillToExtract;// this.MaterialType.SkillToExtract;
            if (skill == null)
                return;
            var interaction = skill.GetWork(Player.Actor, new TargetArgs());
            list.Add(new PlayerInput(PlayerActions.Interact), interaction);

            //if (this.Material == null)
            //    return;
            //var skill = this.Material.Type.SkillToExtract;
            //if (skill == null)
            //    return;
            //var interaction = skill.GetWork(new TargetArgs());
            //list.Add(new PlayerInput(PlayerActions.RB), interaction);
        }
        public virtual void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, ContextAction> list)
        {
            Dictionary<PlayerInput, Interaction> dic = new Dictionary<PlayerInput, Interaction>();
            this.GetPlayerActionsWorld(player, global, dic);
            //var newdic = dic.ToDictionary(i => i.Key, i => new ContextAction(i.Value));
            //foreach (var c in newdic)
            //    list.Add(c.Key, c.Value);
            foreach (var c in dic)
                list.Add(c.Key, new ContextAction(c.Value));
        }

        static public float GetPathingCost(IMap map, Vector3 global)
        {
            var cell = map.GetCell(global);
            return cell.Block.GetPathingCost(cell.BlockData);
        }
        public virtual float GetPathingCost(byte data)
        {
            return 1;
        }

        //protected const int EntityIDRange = 50000;
        public const int EntityIDRange = 50000;
        public int EntityID { get { return EntityIDRange + (int)this.Type; } }
        protected virtual GameObject ToObject()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<GeneralComponent>().Initialize(EntityIDRange + (int)this.Type, ObjectType.Block, this.GetName());
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            obj.AddComponent<BlockComponent>().Initialize(this);
            //obj.AddComponent<SpriteComponent>().Initialize(new Sprite(this.Variations.First().Name, Map.BlockDepthMap) { Origin = Block.OriginCenter, MouseMap = BlockMouseMap });
            //obj.AddComponent<GuiComponent>().Initialize(new Icon(obj.GetSprite()));
            return obj;
        }
        public virtual GameObject Create(List<GameObjectSlot> reagents)
        {
            return this.GetEntity().Clone();
        }
        //static public GameObject CreateFromReagents(List<GameObjectSlot> reagents)
        //{
        //    GameObject obj = this.GetObject().Clone();
        //    //IBlockState state = new State(reagents.First().Object.GetComponent<MaterialsComponent>().Parts["Body"].Material);
        //    IBlockState state = this.BlockState;
        //    state.FromMaterial(reagents.First().Object);
        //    byte data = 0;
        //    state.Apply(ref data);
        //    obj.GetComponent<BlockComponent>().Data = data;
        //    return obj;
        //}

        public virtual void OnSteppedOn(GameObject actor, Vector3 global) { }

        public virtual void OnDrop(GameObject actor, GameObject dropped, TargetArgs target)
        {
            dropped.SetGlobal(target.Global + target.Face);
            actor.Net.Spawn(dropped);
        }

        static public Material GetBlockMaterial(IMap map, Vector3 global)
        {
            //return map.GetBlock(global).GetMaterial(map, global);
            var cell = map.GetCell(global);
            var mat = cell.Block.GetMaterial(cell.BlockData);
            return mat;
        }
        static public float GetBlockHeight(IMap map, Vector3 global)
        {
            var block = map.GetBlock(global);
            var offset = global.ToBlock();
            var data = map.GetData(global);
            var h = block.GetHeight(data, offset.X, offset.Y);
            return h;
        }

        public virtual float GetHeight(byte data, float x, float y)
        {
            return this.GetHeight(x, y);
            //return this.GetHeight(data, x, y);

            // return this.Solid ? 1f : 0f; }
        }
        public virtual float GetHeight(float x, float y) { return this.Solid ? 1f : 0f; }
        public virtual float GetHeight(Vector3 blockcoords) { return this.GetHeight(blockcoords.X, blockcoords.Y); }
        public virtual float GetHeight(byte data, Vector3 blockcoords) { return this.GetHeight(data, blockcoords.X, blockcoords.Y); }

        static public readonly AtlasDepthNormals.Node.Token ParticlePixel = Block.Atlas.Load(UI.UIManager.Highlight, "particle");

        static public readonly Block Air = //new Block(Block.Types.Air, GameObject.Types.Air, opaque: false, solid: false, density: 0);
            new BlockAir();
        static public readonly Block Grass = new BlockGrass();
        //static public readonly Block Flowers = new Block(Block.Types.Flowers, GameObject.Types.Flowers)
        //{
        //    MaterialType = MaterialType.Soil,
        //    Material = Material.Soil,
        //    AssetNames = "flowersred, flowersyellow, flowerswhite, flowerspurple"
        //};
        //static public readonly Block FlowersNew = new BlockFlowers();
        static public readonly Block Farmland = new BlockFarmland();
        static public readonly Block Cobblestone = new BlockStone();
        static public readonly Block Mineral = new BlockMineral();
        static public readonly Block Sand = new BlockSand();
        static public readonly Block WoodenDeck = new BlockWoodenDeck();
        static public readonly Block Soil = new BlockSoil();
        //static public readonly Block Gravel = new Block(Block.Types.Gravel, GameObject.Types.Gravel) { Material = Material.Sand, AssetNames = "gravel1" };
        //static public readonly Block WoodenFrame = new Block(Block.Types.WoodenFrame, GameObject.Types.WoodenFrame) { Material = Material.LightWood, AssetNames = "frame" };

        static public readonly Block Stone = new BlockBedrock();
        //new Block(Block.Types.Stone, GameObject.Types.Rock)
        //{
        //    //MaterialType = MaterialType.Mineral, 
        //    Material = Material.Stone,
        //    AssetNames = "smoothstone"
        //};

        //public void StartDraw(GraphicsDevice gd)
        //{
        //    gd.Textures[0] = Block.Atlas.Texture;
        //    gd.Textures[1] = Block.Atlas.DepthTexture;
        //}

        static public readonly Block Door = new BlockDoor(); // TODO: different door materials???
        static public readonly Block Bed = new BlockBed();
        static public readonly Block WoodPaneling = new BlockWoodPaneling();
        static public readonly Block Smeltery = new Blocks.Smeltery.BlockSmeltery();
        static public readonly Block Chest = new Blocks.Chest.BlockChest();
        static public readonly Block Sapling = new Blocks.Sapling.BlockSapling();
        static public readonly Block Water = new BlockWater();
        static public readonly Block Stool = new BlockStool();
        static public readonly Block Chair = new BlockChair();
        static public readonly Block Bricks = new BlockBricks();
        static public readonly Block Campfire = new BlockCampfire();
        static public readonly Block Window = new BlockWindow();
        static public readonly Block Roof = new BlockRoof();
        static public readonly Block Stairs = new BlockStairs();
        static public readonly Block Counter = new BlockCounter();
        static public readonly Block Workbench = new BlockWorkbench();
        static public readonly Block Designation = new BlockDesignation();
        static public readonly Block CarpentryBench = new BlockCarpentryBench();
        static public readonly Block Slab = new BlockSlab();


        internal static bool IsBlockSolid(IMap map, Vector3 global)
        {
            return map.GetBlock(global).IsSolid(map, global);
        }



        
    }
}
