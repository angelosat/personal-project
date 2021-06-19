﻿using System;
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
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Particles;
using Start_a_Town_.Blocks;
using Start_a_Town_.GameModes;
using Start_a_Town_.Tokens;
using Start_a_Town_.UI;
using UI;

namespace Start_a_Town_
{
    abstract public partial class Block : ISlottable, ITooltippable
    {
        static Block()
        {
            BlockDefOf.Init();
        }
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
            public void FromCraftingReagent(GameObject item) { }
            public Color GetTint(byte d)
            { return Color.White; }
            public string GetName(byte d)
            {
                return "";
            }
        }

       

        //static public AtlasWithDepth Atlas = Sprite.Atlas;//
        //static public AtlasWithDepth Atlas = new AtlasWithDepth("Blocks"); ///Sprite.Atlas;//

        static readonly Dictionary<Block, GameObject> BlockObjects = new();

        static public Color[] BlockCoordinatesFull,
            BlockCoordinatesHalf,
            BlockCoordinatesQuarter;

        static public AtlasDepthNormals.Node.Token BlockShadow;


        static public AtlasDepthNormals.Node.Token BlockBlueprint;
        static public AtlasDepthNormals.Node.Token BlockHighlight;
        static public AtlasDepthNormals.Node.Token BlockBlueprintGrayscale;

        static public void Initialize()
        {
            BlockCoordinatesFull = new Color[32 * 40];
            Game1.Instance.Content.Load<Texture2D>("Graphics/goodUV").GetData<Color>(BlockCoordinatesFull, 0, 32 * 40);
            BlockCoordinatesHalf = new Color[32 * 40];
            Game1.Instance.Content.Load<Texture2D>("Graphics/goodUVhalf").GetData<Color>(BlockCoordinatesHalf, 0, 32 * 40);
            BlockCoordinatesQuarter = new Color[32 * 40];
            Game1.Instance.Content.Load<Texture2D>("Graphics/goodUVquarter").GetData<Color>(BlockCoordinatesQuarter, 0, 32 * 40);

            BlockShadow = Block.Atlas.Load("blocks/block shadow smaller", Block.SliceBlockDepthMap, Block.NormalMap);// LoadTexture("blockshadow", "block shadow");
            BlockBlueprint = Block.Atlas.Load("blocks/blockblueprint");
            BlockBlueprintGrayscale = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("Graphics/items/blocks/blockblueprint").ToGrayscale(), "blocks/blockblueprint-grayscale");

            BlockHighlight = Block.Atlas.Load("blocks/highlightfull");
            //BlockBlueprint = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/blockblueprint").ToGrayscale(), "blueprint-grayscale"); 

            Atlas.Initialize();
            LoadMouseMap();


            //foreach (var item in Registry.Values)
            //    if (item.Type != Types.Air)
            //    {
            //        BlockObjects[item] = item.ToObject();
            //    }
            //EntityPrefabBlock.Initialize();
        }
        //static void InitPrefabs()
        //{
        //    foreach (var b in Registry.Values)
        //    {
        //        var r = b.GetPrefabRecipe();
        //    }
        //}

        public void Deconstruct(GameObject actor, Vector3 global)
        {
            var map = actor.Map;
            var cell = map.GetCell(global);
            var material = this.GetMaterial(cell.BlockData);
            var scraps = RawMaterialDef.Scraps;
            var materialQuantity = this.Ingredient.Amount;
            var obj = scraps.CreateFrom(material).SetStackSize(materialQuantity);
            actor.Net.PopLoot(obj, global, Vector3.Zero);


            this.OnDeconstruct(actor, global);
            actor.Map.GetBlockEntity(global)?.Deconstruct(actor, global);
            actor.Map.RemoveBlock(global);
        }
        protected virtual void OnDeconstruct(GameObject actor, Vector3 global) { }
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
        protected static AtlasDepthNormals.Node.Token LoadTexture(string localfilepath)
        {
            return Block.Atlas.Load("graphics/items/blocks/" + localfilepath, Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/" + localfilepath).ToGrayscale(), Block.BlockDepthMap, Block.NormalMap);
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

        static public readonly MouseMap BlockMouseMap = new(MouseMapSprite, MouseMapSpriteOpposite, MouseMapSprite.Bounds, true);
        static public readonly MouseMap BlockHalfMouseMap = new(HalfBlockMouseMapTexture, MouseMapSpriteOpposite, HalfBlockMouseMapTexture.Bounds, true);
        static public readonly MouseMap BlockQuarterMouseMap = new(QuarterBlockMouseMapTexture, MouseMapSpriteOpposite, QuarterBlockMouseMapTexture.Bounds, true);

        static public AtlasDepthNormals Atlas = new("Blocks") { DefaultDepthMask = Map.BlockDepthMap, DefaultNormalMask = Block.NormalMap }; ///Sprite.Atlas;//

        static public readonly AtlasDepthNormals.Node.Token[] TexturesCounter = 
            new AtlasDepthNormals.Node.Token[4] {
                LoadTexture("counter1grayscale", "/counters/counter1"),
                LoadTexture("counter4grayscale", "/counters/counter4"),
                LoadTexture("counter3grayscale", "/counters/counter3"),
                LoadTexture("counter2grayscale", "/counters/counter2")};


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
        public virtual Icon GetIcon()
        {
            return new Icon(this.GetDefault());
            //return new Icon(this.Variations.First());
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
            //return;
            //this.GetTooltip(tooltip as UI.Control);

        }
        //public void GetTooltip(UI.Control tooltip)
        //{
        //    return;

        //    //tooltip.Controls.Add(new UI.Label(this.Type.ToString()) { Location = tooltip.Controls.BottomLeft });
        //    //var tasks = this.GetAvailableTasks();
        //    //if (tasks.Count > 0)
        //    //    tasks.First().GetTooltip(tooltip);

        //    //var entity = this.GetBlockEntity();
        //    //if (entity != null)
        //    //    entity.GetTooltipInfo(tooltip as UI.Tooltip);
        //}
        public virtual void GetTooltip(Control tooltip, IMap map, Vector3 global)
        {
            //if (this == Block.Air) // create tooltip even if air, in case of undiscovered area
            //    return;
            var cell = map.GetCell(global);
            tooltip.Controls.Add(new Label($"Global: {global}") { Location = tooltip.Controls.BottomLeft });
            tooltip.Controls.Add(new Label($"Local: {global.ToLocal()}") { Location = tooltip.Controls.BottomLeft });
            tooltip.Controls.Add(new Label("Chunk: " + map.GetChunk(global).MapCoords.ToString()) { Location = tooltip.Controls.BottomLeft });
            if (map.Camera.HideUnknownBlocks && (cell.IsHidden() || map.IsUndiscovered(global)))
            //if (map.Camera.HideUnknownBlocks && !cell.Discovered)// (cell.IsHidden() || map.IsUndiscovered(global)))
            {
                    tooltip.AddControlsBottomLeft(new Label("Undiscovered area") { Font = UI.UIManager.FontBold, TextColor = Color.Goldenrod });
                return;
            }
            tooltip.Controls.Add(new UI.Label(this.Name) { Location = tooltip.Controls.BottomLeft, Font = UI.UIManager.FontBold, TextColor = Color.Goldenrod });
            var mat = this.GetMaterial(map.GetData(global));
            if(mat!=null)
                tooltip.Controls.Add(new UI.Label(mat.ToString()) { TextColorFunc = () => mat.Color, Location = tooltip.Controls.BottomLeft });

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

        //internal virtual string GetLabel(IMap map, Vector3 global)
        //{
        //    return this.Name;
        //}

        public virtual void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            //this.Draw(sb, pos, Color.White, Vector4.One, 1, 0, new Cell());
            var token = GetDefault();
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, this.BlockState.GetTint(0));
        }

        public virtual AtlasDepthNormals.Node.Token GetDefault()
        {
            //var token = this.Variations.First();
            var token = this.GetToken(0, 0, 0, 0);
            return token;
        }

        public virtual void DrawUI(SpriteBatch sb, Vector2 pos, byte state)
        {
            var token = this.GetToken(0, 0, 0, 0);// this.Variations.First();
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

        public TokenCollection Tokens = new();

        static public Vector2 OriginCenter = new(Width / 2f, Height - Depth / 2f);//16, Height - BlockHeight);//16);
        static public readonly Vector2 Joint = new(Block.Width / 2, Block.Height);
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
            Designation,
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
            Slab,
            Prefab,
            Kitchen,
            Conveyor,
            PlantProcessing,
            Bin,
            Construction,
            ShopCounter
        }
        static public Rectangle Bounds = new(-(int)Block.OriginCenter.X, -(int)Block.OriginCenter.Y, Block.Width, Block.Height);
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
        static public void Build(IObjectProvider net, GameObject builder, TargetArgs target, GameObject.Types consumeType, GameObject.Types blockType, Action<GameObject> onFinish)
        {
            if (InventoryComponent.ConsumeEquipped(builder, slot => slot.Object.IDType == consumeType))
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
        public virtual BlockEntity CreateBlockEntity()
        { return null; }
        [Obsolete]
        public virtual BlockEntity GetBlockEntity(IMap map, Vector3 global)
        { return null; }
        public virtual T GetBlockEntityNew<T>(IMap map, IntVec3 global) where T : BlockEntity
        { 
            return map.GetBlockEntity<T>(global); 
        }
        //public readonly GameObject Entity;
        public GameObject GetEntity()
        {
            return BlockObjects[this];
            //GameObject obj;
            //GameObject.Objects.TryGetValue(this.EntityID, out obj);
            //return obj;
        }
        public virtual Dictionary<Vector3, byte> GetParts(Vector3 global, int orientation) // TODO: depend on orientation
        {
            return new Dictionary<Vector3, byte>() { { global, 0 } };
        }
        public virtual List<Vector3> GetParts(IMap map, Vector3 global) { return new List<Vector3> { global }; }
        public virtual List<Vector3> GetParts(byte data) { return new List<Vector3> { Vector3.Zero }; }
        public virtual IEnumerable<IntVec3> GetParts(byte data, IntVec3 global) { return new List<IntVec3> { global }; }

        public IntVec3 GetCenter(IMap map, Vector3 global) { return this.GetCenter(map.GetCell(global).BlockData, global); }
        public virtual IntVec3 GetCenter(byte blockData, Vector3 global) { return global; }

        public virtual bool Multi { get { return false; } }
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
        /// <summary>
        /// Determines wether the block will be considered as a pathfinding finish node for the AI to stand IN
        /// </summary>
        public virtual bool IsStandableIn => !this.Solid;
        /// <summary>
        /// Determines wether the block will be considered as a pathfinding finish node for the AI to stand ON
        /// </summary>
        public virtual bool IsStandableOn => this.Solid;
        public FurnitureDef Furniture;
        public virtual byte Luminance { get; }
        public virtual bool IsMinable { get { return false; } }
        public virtual bool IsDeconstructible { get { return false; } }
        public virtual bool IsSwitchable { get; }
        public virtual bool IsRoomBorder => this.Opaque;
        public virtual FurnitureDef GetFurnitureRole(IMap map, IntVec3 global) { return null; }
        public virtual bool IsTargetable(Vector3 global)
        {
            return true;
        }
        public List<Utility.Types> UtilitiesProvided = new();
        public virtual float GetWorkToBreak(IMap map, Vector3 global)
        {
            return map.GetBlockMaterial(global).WorkToBreak;
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
                Lifetime = Engine.TicksPerSecond / 2f,
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
                Lifetime = Engine.TicksPerSecond / 2f,
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

        public virtual BlockRecipe GetRecipe()
        {
            //return null;
            return this.Recipe;
        }

        //Reaction GetPrefabRecipe()
        //{
        //    var recipe = this.GetRecipe();
        //    if (recipe == null)
        //        return null;
        //    var reaction = new Reaction(
        //        "Prefab: " + this.Name,
        //        Reaction.CanBeMadeAt(IsWorkstation.Types.Carpentry),
        //        recipe.Reagents,
        //        Reaction.Product.Create(new Reaction.Product(new EntityPrefabBlock.Factory())
        //        ));
        //    return reaction;
        //}

        //static public List<Reaction> GetPrefabRecipeList()
        //{
        //    List<Reaction> list = new List<Reaction>();
        //    foreach (var b in Registry)
        //        list.Add(b.Value.GetPrefabRecipe());
        //    return list;
        //}

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

        public BlockRecipe Recipe;
        public List<Components.Crafting.Reaction.Reagent> Reagents = new();
        
        //public Ingredient Ingredient;
        public BuildProperties BuildProperties;
        public Ingredient Ingredient { 
            get => this.BuildProperties?.Ingredient;
            set
            {
                if (this.BuildProperties == null)
                    this.BuildProperties = new BuildProperties(value, 1);
                else 
                    this.BuildProperties.Ingredient = value;
            }
        }
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
        
        public virtual void Removed(IMap map, Vector3 global)
        {
            //global.TrySetCell(net, Block.Types.Air); 

        }
        public virtual bool IsValidPosition(IMap map, IntVec3 global, int orientation) { return true; }
        [Obsolete]
        internal void Place(IMap map, List<Vector3> positions, byte data, int orientation, bool notify)
        {
            throw new Exception();
            foreach (var pos in positions)
                this.Place(map, pos, data, 0, orientation, false);
            if (notify)
                map.NotifyBlocksChanged(positions);
        }
        //[Obsolete]
        public virtual void Place(IMap map, Vector3 global, byte data, int variation, int orientation, bool notify = true)
        {
            //map.SetBlock(global, this.Type, data, variation);
            // TODO: change this method so it returns true or false depending if the block was placed succesfully? (if the cell was empty of any entities)
            // or keep it as it is and just only check if the cell is empty if the block is solid, otherwise don't check?
            //if (!map.IsEmpty(global))
            //    return;

            // TODO: ONLY CALL SETBLOCK ONCE WHEN PLACING BLOCKS
            //throw new Exception();
            map.SetBlock(global, this.Type, data, variation, orientation, notify);
            //map.GetCell(global).Orientation = orientation; // dont i set orientation in the setblock method? YES I DO
            var entity = this.CreateBlockEntity();
            if (entity != null)
            {
                map.AddBlockEntity(global, entity);
                map.EventOccured(Message.Types.BlockEntityAdded, entity, global);
            }
            map.SetBlockLuminance(global, this.Luminance);
        }
        [Obsolete]
        public virtual void Placed(IObjectProvider net, Vector3 global)
        {
            //global.TrySetCell(net, this.Type); 
            throw new Exception();
            net.Map.SetBlock(global, this.Type);
        }
        //public virtual void OnRemove(IMap map, Vector3 global) { }

        [Obsolete]
        public virtual void Remove(IMap map, Vector3 global, bool notify = true)
        {
            map.RemoveBlockNew(global, notify);
            return;
            //this.OnRemove(map, global);
            // TODO: ONLY CALL SETBLOCK ONCE WHEN PLACING BLOCKS

            ///moved this after blockentityremoval
            //map.SetBlock(global, Block.Types.Air, 0, 0, 0, notify); 
            throw new Exception();

            var blockentity = map.RemoveBlockEntity(global);
            if (blockentity != null)
            {
                blockentity.OnRemove(map, global);
                blockentity.Dispose();
                if(notify)
                    map.Net.EventOccured(Message.Types.BlockEntityRemoved, blockentity, global);
            }

            /// moved this after the removal of the block entity 
            /// because when handling the block change event, another block might be immediately added,
            /// and that block would immediately get its blockentity removed
            map.SetBlock(global, Block.Types.Air, 0, 0, 0, notify);
            map.SetBlockLuminance(global, 0);


            // reenable physics of entities resting on block
            foreach (var entity in map.GetObjects(global - new Vector3(1, 1, 0), global + new Vector3(1, 1, 2)))
            {
                PhysicsComponent.Enable(entity);
            }
            var above = global.Above();
            map.GetBlock(above)?.BlockBelowChanged(map, above);
            this.OnRemove(map, global);
        }
        protected virtual void OnRemove(IMap map, Vector3 global) { }
        
        public void BlockBelowChanged(IMap map, Vector3 global)
        {
            map.GetBlockEntity(global)?.OnBlockBelowChanged(map, global);
            this.OnBlockBelowChanged(map, global);
        }
        protected virtual void OnBlockBelowChanged(IMap map, Vector3 global) { }
        public virtual LootTable GetLootTable(byte data)
        {
            return this.LootTable;
        }
        public virtual void Break(IMap map, Vector3 global)
        {
            var net = map.Net;
            net.PopLoot(this.GetLootTable(net.Map.GetData(global)), global, Vector3.Zero);

            /// 
            //var blockentity = map.RemoveBlockEntity(global);
            //if (blockentity != null)
            //{
            //    blockentity.Break(map, global);
            //    blockentity.Dispose();
            //    net.EventOccured(Message.Types.BlockEntityRemoved, blockentity, global);
            //}
            /// commented this out because i'm also doing it in the remove method

            this.Remove(map, global);
        }
        public virtual void Break(GameObject actor, Vector3 global)
        {
            var mat = Block.GetBlockMaterial(actor.Map, global);
            var net = actor.Net;
            net.PopLoot(this.GetLootTable(actor.Map.GetData(global)), global, Vector3.Zero);
            this.Remove(net.Map, global);

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

            e.Lifetime = Engine.TicksPerSecond * 2;
            var pieces = this.GetParticleRects(25);
            e.Emit(Block.Atlas.Texture, pieces, Vector3.Zero);
            //actor.Map.EventOccured(Message.Types.ParticleEmitterAdd, e);
            actor.Map.ParticleManager.AddEmitter(e);
        }
        public virtual bool IsSolid(IMap map, Vector3 global, byte data)
        {
            //return this.Solid;
            var offset = global + 0.5f * new Vector3(1, 1, 0);
            offset -= offset.RoundXY();
            var h = this.GetHeight(data, offset.X, offset.Y);
            return this.Solid && offset.Z < h;
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
            offset -= offset.RoundXY();
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
        public virtual void RandomBlockUpdate(IObjectProvider net, IntVec3 global, Cell cell) { }
        //public virtual void OnMessage(GameObject parent, ObjectEventArgs args) { }
        protected virtual void HandleMessage(Vector3 global, ObjectEventArgs e) { }

        public static void HandleMessage(IObjectProvider net, Vector3 global, ObjectEventArgs e)
        {
            //global.GetBlock(net.Map).HandleMessage(global, e);
            net.Map.GetBlock(global).HandleMessage(global, e);

        }
        internal virtual void RemoteProcedureCall(IObjectProvider net, Vector3 vector3, Message.Types type, System.IO.BinaryReader r) { }

        //static Dictionary<Block.Types, Block> _Registry;
        static public Dictionary<Block.Types, Block> Registry = new();
        //{ get { if (_Registry.IsNull()) _Registry = new Dictionary<Types, Block>(); return _Registry; } }

        //static public void Initialize()
        //{
        //    BlockAtlas.Initialize();
        //}

        /// <summary>
        /// The index to the block's variation is stored within a cell. Add variations to this list so they can be picked at random whenever a block is placed.
        /// </summary>
        public List<AtlasDepthNormals.Node.Token> Variations = new();
        public List<Sprite> Sprites = new();
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
        //Atlas.Node.Token SpriteToken;

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
            this.LootTable = new LootTable();
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

            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], zoom, fog, tint, sunlight, blocklight, depth, this);
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
            if (this == BlockDefOf.Air)
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
                camera.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, this, blockCoordinates);

        }
        public virtual MyVertex[] Draw(Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, Cell cell)
        {
            return this.Draw(chunk, blockCoordinates, camera, screenBounds, sunlight, blocklight, fog, tint, depth, cell.Variation, cell.Orientation, cell.BlockData);
        }

        public virtual MyVertex[] Draw(Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            if (this == BlockDefOf.Air)
                return null;
            //tint.A = (byte)((1-this.Transparency) * 255);// *= this.Transparency;
            //tint *= (1 - this.Transparency);
            //var material = this.GetColor(data);
            var material = this.GetColorVector(data);

            //camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], camera.Zoom, fog, color.Multiply(tint), sunlight, blocklight, depth);
            var token = this.GetToken(variation, orientation, (int)camera.Rotation, data);// maybe change the method to accept double so i don't have to cast the camera rotation to int?
            return chunk.Canvas.Opaque.DrawBlock(Block.Atlas.Texture, screenBounds,
                //this.Variations[Math.Min(variation, this.Variations.Count - 1)],
                token,
                camera.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, this, blockCoordinates);
        }
        public virtual MyVertex[] Draw(Canvas canvas, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            if (this == BlockDefOf.Air)
                return null;
            //tint.A = (byte)((1-this.Transparency) * 255);// *= this.Transparency;
            //tint *= (1 - this.Transparency);
            //var material = this.GetColor(data);
            var material = this.GetColorVector(data);
            MySpriteBatch mesh = this.Opaque ? canvas.Opaque : canvas.NonOpaque;
            //camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], camera.Zoom, fog, color.Multiply(tint), sunlight, blocklight, depth);
            var token = this.GetToken(variation, orientation, (int)camera.Rotation, data);// maybe change the method to accept double so i don't have to cast the camera rotation to int?
            return mesh.DrawBlock(Block.Atlas.Texture, screenBounds,
                //this.Variations[Math.Min(variation, this.Variations.Count - 1)],
                token,
                camera.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, this, blockCoordinates);
        }

        public virtual void Draw(Camera camera, Vector3 global, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation)
        {
            var screenBounds = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, Block.Bounds, Vector2.Zero);// Block.OriginCenter);
            camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(variation, this.Variations.Count - 1)], camera.Zoom, fog, tint, sunlight, blocklight, depth, this);
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
        public virtual void DrawPreview(MySpriteBatch sb, IMap map, Vector3 global, Camera cam, byte data, int variation = 0, int orientation = 0)
        {
            this.DrawPreview(sb, map, global, cam, Color.White * .5f, data, variation, orientation);
        }
        public virtual void DrawPreview(MySpriteBatch sb, IMap map, Vector3 global, Camera cam, Color tint, byte data, int variation = 0, int orientation = 0)
        {
            //var pos = cam.GetScreenPosition(global);
            var depth = global.GetDrawDepth(map, cam);
            //var screenPos = pos - Block.OriginCenter * cam.Zoom;
            var materialcolor = this.GetColor(data);
            //var tint = Color.White *.5f;
            var token = this.GetPreviewToken(variation, orientation, (int)cam.Rotation, data); // change the method to accept double so i don't have to cast the camera rotation to int?
            var bounds = cam.GetScreenBoundsVector4(global.X, global.Y, global.Z, Block.Bounds, Vector2.Zero);
            sb.DrawBlock(Block.Atlas.Texture, bounds, token, cam.Zoom, Color.Transparent, tint, materialcolor, Color.White, Vector4.One, Vector4.Zero, depth, this);
        }

        protected static void DrawShadow(MySpriteBatch nonopaquemesh, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth)
        {
            nonopaquemesh.DrawBlock(Block.Atlas.Texture, screenBounds, Block.BlockShadow, camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Vector4.Zero, depth, null, blockCoordinates);
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
        public Vector4 GetColorFromMaterial(byte data)
        {
            var mat = this.GetMaterial(data);
            var c = mat.ColorVector;
            return c;
        }
        static Vector4 DefaultColorVector = Vector4.One;
       
        public virtual byte GetDataFromMaterial(GameObject craftingReagent)
        {
            return (byte)craftingReagent.Body.Material.ID;
            //byte data = 0;
            //IBlockState state = this.BlockState;
            //state.FromCraftingReagent(craftingReagent);
            //state.Apply(ref data);
            //return data;
        }
        public virtual byte GetDataFromMaterial(Material mat)
        {
            return (byte)mat.ID;
        }
        public virtual IEnumerable<byte> GetCraftingVariations()
        {
            var list = new List<byte>();
            BlockRecipe constr = BlockRecipe.Dictionary.Values.FirstOrDefault(c => c.Block == this);
            if (constr == null)
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
            var list = new Dictionary<PlayerInput, Interaction>();
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
            //var t = new TargetArgs(global);
            foreach (var i in list)
                //if (i.Value.InRange(Player.Actor, t))
                //a.Actions.Add(new ContextAction(i.Key.ToString() + ": " + i.Value.Name, null, () => i.Value.Conditions.Evaluate(Player.Actor, t)));// () => true));
                a.Actions.Add(new ContextAction(i.Value) { Shortcut = i.Key });// () => true));

            return;



            //var actions = new List<ContextAction>();
            //actions.AddRange(from action in this.GetAvailableTasks(player.Map, global)
            //                 select new ContextAction(() => action.Name, () =>
            //                 {
            //                     Client.Instance.Send(PacketType.Towns, new Towns.PacketAddJob(Player.Actor.InstanceID, new TargetArgs(global), action.Name).Write());
            //                     //Client.PlayerRemoteCall(this, )
            //                 }));

            //a.Actions = actions;
        }

        //public virtual Dictionary<PlayerInput, Interaction> GetPlayerActionsWorld()
        public virtual void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            var hauled = Components.PersonalInventoryComponent.GetHauling(PlayerOld.Actor);
            if (hauled.Object != null)
            {
                list[PlayerInput.RButton] = new DropCarriedSnap();
                return;
            }
            //var mat = Block.GetBlockMaterial(player.Map, global);
            ////if (this.MaterialType == null)
            //if (mat.Type == null)
            //    return;
            //var skill = mat.Type.SkillToExtract;// this.MaterialType.SkillToExtract;
            //if (skill == null)
            //    return;
            //var interaction = skill.GetWork(Player.Actor, new TargetArgs());
            //list[PlayerInput.RButton] = interaction;
        }
        public virtual void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, ContextAction> list)
        {
            var dic = new Dictionary<PlayerInput, Interaction>();
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

        public Material DefaultMaterial
        {
            get
            {
                return this.GetAllValidConstructionMaterials().First();
                //return this.Ingredient.GetAllValidMaterials().First();
                //return this.Reagent.Def.PreferredMaterialType.SubTypes.First();
            }
        }

        public bool RequiresConstruction = true;

        internal IEnumerable<Material> GetAllValidConstructionMaterials()
        {
            return this.Ingredient.GetAllValidMaterials();
        }
        internal IEnumerable<ItemDefMaterialAmount> GetAllValidConstructionMaterialsNew()
        {
            return this.Ingredient.GetAllValidMaterialsNew();
        }
        internal void IngredientRequirements(Material mainMaterial, out ItemDef def, out int amount)
        {
            if (!this.Ingredient.ItemDef.DefaultMaterialType.SubTypes.Contains(mainMaterial))
                throw new Exception();
            def = this.Ingredient.ItemDef;
            amount = this.Ingredient.Amount;
        }
        internal string GetIngredientLabel()
        {
            return this.Ingredient.GetLabel();
        }
        protected virtual GameObject ToObject()
        {
            var obj = new GameObject();
            obj.AddComponent<DefComponent>().Initialize(EntityIDRange + (int)this.Type, ObjectType.Block, this.GetName());
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            //obj.AddComponent<BlockComponent>().Initialize(this);
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

        public virtual void OnDrop(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            //dropped.SetGlobal(target.Global + target.Face + target.Precise);
            dropped.Global = target.Global + target.Face + target.Precise;
            if (dropped.Slot != null)
                dropped.Slot.Clear(); // ugly
            // TODO: handle case where we split the stack when dropping it. instantiate new object with server etc...
            actor.Map.EventOccured(Message.Types.EntityPlacedItem, actor, dropped);

            // WARNING spawning the item locally by calling its own method because we dont want the server to syncspawn, as is the case my calling server.spawn at the moment
            dropped.Spawn(actor.Map, target.Global + target.Face + target.Precise); 
            //actor.Net.Spawn(dropped);
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
            var offset = global.ToBlock();
            var cell = map.GetCell(global);
            //var block = map.GetBlock(global);
            //var data = map.GetData(global);
            var h = cell.Block.GetHeight(cell.BlockData, offset.X, offset.Y);
            return h;
        }

        public virtual float GetHeight(byte data, float x, float y)
        {
            return this.GetHeight(x, y);
            //return this.GetHeight(data, x, y);

            // return this.Solid ? 1f : 0f; }
        }
        public virtual float GetHeight(float x, float y) { return this.Solid ? 1f : 0f; }
        public virtual Vector3 GetVelocityTransform(byte data, Vector3 blockcoords) { return Vector3.Zero; }
        public float GetHeight(Vector3 blockcoords) { return this.GetHeight(blockcoords.X, blockcoords.Y); }
        public float GetHeight(byte data, Vector3 blockcoords) { return this.GetHeight(data, blockcoords.X, blockcoords.Y); }
        //public bool HasUsage(BlockUsageDef usage)
        //{
        //    throw new NotImplementedException();
        //}
        static public readonly AtlasDepthNormals.Node.Token ParticlePixel = Block.Atlas.Load(UI.UIManager.Highlight, "particle");

        //static public readonly Block Air = //new Block(Block.Types.Air, GameObject.Types.Air, opaque: false, solid: false, density: 0);
        //    new BlockAir();
        //static public readonly Block Grass = new BlockGrass();
        //static public readonly Block Stone = new BlockBedrock();
        //static public readonly Block Farmland = new BlockFarmland();
        //static public readonly Block Cobblestone = new BlockStone();
        //static public readonly Block Mineral = new BlockMineral();
        //static public readonly Block Sand = new BlockSand();
        //static public readonly Block WoodenDeck = new BlockWoodenDeck();
        //static public readonly Block Soil = new BlockSoil();
        //static public readonly Block Door = new BlockDoor(); // TODO: different door materials???
        //static public readonly Block Bed = new BlockBed();
        //static public readonly Block WoodPaneling = new BlockWoodPaneling();
        //static public readonly Block Smeltery = new Blocks.Smeltery.BlockSmeltery();
        //static public readonly Block Chest = new Blocks.Chest.BlockChest();
        //static public readonly Block Bin = new BlockBin();
        //static public readonly Block Sapling = new Blocks.Sapling.BlockSapling();
        //static public readonly Block Water = new BlockWater();
        //static public readonly Block Stool = new BlockStool();
        //static public readonly Block Chair = new BlockChair();
        //static public readonly Block Bricks = new BlockBricks();
        //static public readonly Block Campfire = new BlockCampfire();
        //static public readonly Block Window = new BlockWindow();
        //static public readonly Block Roof = new BlockRoof();
        //static public readonly Block Stairs = new BlockStairs();
        //static public readonly Block Counter = new BlockCounter();
        //static public readonly Block Workbench = new BlockWorkbench();
        //static public readonly Block Kitchen = new BlockKitchen();
        //static public readonly Block PlantProcessingBench = new BlockPlantProcessing();
        //static public readonly Block Designation = new BlockDesignation();
        //static public readonly Block CarpentryBench = new BlockCarpentryBench();
        //static public readonly Block Slab = new BlockSlab();
        //static public readonly Block Conveyor = new BlockConveyor();
        //static public readonly Block Prefab = new BlockPrefab();
        //static public readonly Block Construction = new BlockConstruction();
        //static public readonly Block ShopCounter = new BlockShopCounter();

        static readonly AtlasDepthNormals.Node.Token Token = Block.Atlas.Load("blocks/blockunknown", Map.BlockDepthMap, Block.NormalMap);

        public static MyVertex[] DrawUnknown(MySpriteBatch opaquemesh, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth)
        {
            return opaquemesh.DrawBlock(Block.Atlas.Texture, screenBounds,
                Token,
                camera.Zoom, fog, tint, Vector4.One, sunlight, blocklight, Vector4.Zero, depth, null, blockCoordinates);
        }

        internal static bool IsBlockSolid(IMap map, Vector3 global)
        {
            //return map.GetBlock(global).IsSolid(map, global);
            var cell = map.GetCell(global);
            //return cell == null ? true : cell.Block.IsSolid(map, global);
            return cell == null || cell.Block.IsSolid(map, global, cell.BlockData);
        }


        internal virtual bool IsPathable(Cell cell, Vector3 blockCoords)
        {
            return !this.IsSolid(cell, blockCoords);
        }

        //public virtual WindowTargetInterface GetInterface(Vector3 global) { return null; }
        public virtual void GetInterface(Vector3 global) { WindowTargetInterface.Instance.Client.ClearControls(); }
        public virtual void GetInterface(IMap map, Vector3 global, WindowTargetManagement window) { }
        public virtual void ShowUI(Vector3 global)
        {

        }

        //void PlayerConstructNew(PlaceWallTool.Args args)// Vector3 start, Vector3 end)
        //{
        //    var data = Network.Serialize(w =>
        //    {
        //        w.Write(Player.Actor.InstanceID);
        //        this.SelectedItem.Write(w);
        //        args.Write(w);
        //    });
        //    Client.Instance.Send(PacketType.PlaceWallConstruction, data);
        //}

        public void PaintIcon(int width, int height, byte data)
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            var token = this.GetDefault();
            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            var mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["Combined"];
            fx.Parameters["Viewport"].SetValue(new Vector2(width, height));
            gd.Textures[0] = Block.Atlas.Texture;
            gd.Textures[1] = Block.Atlas.DepthTexture;
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            //var material = tag.Block.GetColor(tag.BlockData);// tag.Block.GetMaterial(tag.BlockData);
            var bounds = new Vector4((width - Block.Width) / 2, (height - Block.Height) / 2, token.Texture.Bounds.Width, token.Texture.Bounds.Height);
            var cam = new Camera
            {
                SpriteBatch = mysb
            };
            this.Draw(mysb, Vector3.Zero, cam, bounds, Color.White, Vector4.One, Color.Transparent, Color.White, 0.5f, 0, 0, data);
            mysb.Flush();
        }
        public RenderTarget2D PaintIcon(byte data)
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            var token = this.GetDefault();
            var w = token.Texture.Bounds.Width;
            var h = token.Texture.Bounds.Height;
            var renderTarget = new RenderTarget2D(gd, w, h);
            gd.SetRenderTarget(renderTarget);
            gd.Clear(Color.Transparent);
            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            var mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["Combined"];
            fx.Parameters["Viewport"].SetValue(new Vector2(w, h));
            gd.Textures[0] = Block.Atlas.Texture;
            gd.Textures[1] = Block.Atlas.DepthTexture;
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            //var material = tag.Block.GetColor(tag.BlockData);// tag.Block.GetMaterial(tag.BlockData);
            var bounds = new Vector4(0,0, w, h);
            var cam = new Camera
            {
                SpriteBatch = mysb
            };
            this.Draw(mysb, Vector3.Zero, cam, bounds, Color.White, Vector4.One, Color.Transparent, Color.White, 0.5f, 0, 0, data);
            mysb.Flush();
            gd.SetRenderTarget(null);
            return renderTarget;
        }
       

        public Vector3 Front(IMap map, Vector3 global)
        {
            return Front(map.GetCell(global));
        }
        public static Vector3 GetFrontSide(int orientation)
        {
            switch (orientation)
            {
                case 0:
                    return new Vector3(0, 1, 0);

                case 1:
                    return new Vector3(-1, 0, 0);

                case 2:
                    return new Vector3(0, -1, 0);

                case 3:
                    return new Vector3(1, 0, 0);

                default:
                    break;
            }
            throw new Exception();
        }
        public static Vector3 Front(Cell cell)
        {
            var orientation = cell.Orientation;
            switch (orientation)
            {
                case 0:
                    return new Vector3(0, 1, 0);

                case 1:
                    return new Vector3(-1, 0, 0);

                case 2:
                    return new Vector3(0, -1, 0);

                case 3:
                    return new Vector3(1, 0, 0);

                default:
                    break;
            }
            throw new Exception();
        }
        public static Vector3 Back(Cell cell)
        {
            return -Front(cell);
        }
        internal virtual IEnumerable<Vector3> GetOperatingPositions(Cell cell)
        {
            yield break;
        }
        internal IEnumerable<Vector3> GetOperatingPositions(Cell cell, Vector3 global)
        {
            foreach (var p in this.GetOperatingPositions(cell))
                yield return p + global;
        }
        [Obsolete]
        internal virtual void Select(UISelectedInfo uISelectedInfo, IMap map, Vector3 vector3)
        {
            throw new Exception();
            //return;
            var node = map.Regions.GetNodeAt(vector3);
            if (node == null)
                return;
            uISelectedInfo.AddInfo(new Label(node.ToString()));
            uISelectedInfo.AddInfo(new Label(node.Region.ToString()));
            uISelectedInfo.AddInfo(new Label(node.Region.Room.ToString()));
        }
        public virtual IEnumerable<(string name, Action action)> GetInfoTabs() { yield break; }
        internal virtual void GetSelectionInfo(IUISelection info, IMap map, Vector3 vector3)
        {
            map.GetBlockEntity(vector3)?.GetSelectionInfo(info, map, vector3);
            var node = map.Regions.GetNodeAt(vector3);
            if (node == null)
                return;
            info.AddInfo(new Label(node.ToString()));
            info.AddInfo(new Label(node.Region.ToString()));
            info.AddInfo(new Label(node.Region.Room.ToString()));
        }
        [Obsolete]
        internal void GetAdvancedInfo(Control container, IMap map, Vector3 global)
        {
            throw new Exception();
            var node = map.Regions.GetNodeAt(global);
            if (node == null)
                return;
            container.AddControls(new Label(node.ToString()));
            container.AddControls(new Label(node.Region.ToString()));
            container.AddControls(new Label(node.Region.Room.ToString()));
        }
        internal virtual void GetQuickButtons(UISelectedInfo uISelectedInfo, IMap map, Vector3 vector3)
        {
            var e = map.GetBlockEntity(vector3);
            e?.GetQuickButtons(uISelectedInfo, map, vector3);
            if (this.Furniture is not null)
            {
                //if (this.Furniture.Rooms.Any())
                uISelectedInfo.AddTabAction("Room", () => { });
            }
        }
        internal virtual bool IsValidHaulDestination(IMap map, Vector3 global, GameObject obj)
        {
            return false;
        }
        internal virtual string GetName(IMap map, Vector3 global)
        {
            return this.Name;
        }
        internal virtual float GetFertility(Cell cell)
        {
            return this.GetMaterial(cell.BlockData) == MaterialDefOf.Soil ? 1f : 0;
        }

        internal virtual void OnDrawSelected(MySpriteBatch sb, Camera camera, IMap map, Vector3 global)
        {

        }
        internal void DrawSelected(MySpriteBatch sb, Camera cam, IMap map, Vector3 global)
        {
            var e = map.GetBlockEntity(global);
            if (e != null)
                foreach (var c in e.Comps)
                    c.DrawSelected(sb, cam, map, global);
            this.OnDrawSelected(sb, cam, map, global);
        }

        public IEnumerable<BlockRecipe.ProductMaterialPair> GetAllValidProductMaterialPairs()
        {
            if (this.Ingredient.Material != null)
            {
                yield return new BlockRecipe.ProductMaterialPair(this, this.Ingredient.Material);//.Recipe.GetVariants();
            }
            else if (this.Ingredient.MaterialType != null)
            {
                foreach(var m in this.Ingredient.MaterialType.SubTypes)
                    yield return new BlockRecipe.ProductMaterialPair(this, m);//.Recipe.GetVariants();
            }
            else if (this.Ingredient.ItemDef != null)
            {
                if (this.Ingredient.MaterialType != null && this.Ingredient.ItemDef.DefaultMaterialType != this.Ingredient.MaterialType)
                    throw new Exception();
                if (this.Ingredient.Material != null)
                {
                    if(!this.Ingredient.ItemDef.DefaultMaterialType.SubTypes.Contains(this.Ingredient.Material))
                        throw new Exception();
                    yield return new BlockRecipe.ProductMaterialPair(this, this.Ingredient.Material);//.Recipe.GetVariants();
                }
                else
                {
                    foreach (var m in this.Ingredient.ItemDef.DefaultMaterialType.SubTypes)
                        yield return new BlockRecipe.ProductMaterialPair(this, m);
                }
            }
        }


    }
}
