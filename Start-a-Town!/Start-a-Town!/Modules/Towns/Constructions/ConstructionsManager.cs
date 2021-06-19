using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Blocks;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Modules.Construction;
using System.IO;
using Start_a_Town_.Components;

namespace Start_a_Town_.Towns.Constructions
{
    public class ConstructionsManager : TownComponent, ISelectable
    {
        public override string Name
        {
            get
            {
                return "Constructions";
            }
        }

        public bool Exists => true;

        static public ConstructionCategoryWalls Walls = new();
        static public ConstructionCategoryDoors Doors = new();
        static public ConstructionCategoryProduction Production = new();
        static public ConstructionCategoryFurniture Furniture = new();

        static public List<ConstructionCategory> AllCategories = new()
        {
            Walls,
            Doors,
            Production,
            Furniture
        };
        public readonly TerrainWindowNew WindowBuild = new();
        //const float UpdateFrequency = 1; // per second
        //float UpdateTimerMax = (float)Engine.TicksPerSecond / UpdateFrequency;
        //float UpdateTimer;
        public ConstructionsManager(Town town)
        {
            this.Town = town;
        }
        readonly Dictionary<Vector3, ConstructionParams> PendingDesignations = new();
        readonly HashSet<IntVec3> Designations = new();
        //{
        //    get { return this.Town.DesignationManager.GetDesignations(Designation.Build); }
        //}
        //public List<Vector3> GetConstructions()
        //{
        //    return this.Designations.ToList();
        //}
       
        //internal List<Vector3> GetAllBuildableCurrently()
        //{
        //    return this.Designations.Where(IsBuildableCurrently).ToList();
        //}
        internal IEnumerable<IntVec3> GetAllBuildableCurrently()
        {
            return this.Designations.Where(IsBuildableCurrently);//.ToList();
        }

        private void RemoveZone(Vector3 arg1, Vector3 arg2, bool arg3)
        {
            var data = Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                w.Write(arg1);
                w.Write(arg2);
            });
            Client.Instance.Send(PacketType.ConstructionRemove, data);
        }

        public override GroupBox GetInterface()
        {
            var box = new GroupBox();
           
            var btn_Remove = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = Icon.X,// new Icon(UIManager.Icons32, 12, 32),
                LeftClickAction = () => ToolManager.Instance.ActiveTool = new ToolSelect3D(RemoveZone),// new ToolDesignate3D(RemoveZone),
                HoverFunc = () => "Remove"
            };
            var btn_Construct = Walls.GetButton();
            var btn_ConstructDoors = Doors.GetButton();
            var btn_ConstructFurniture = Furniture.GetButton();
            var btn_ConstructProduction = Production.GetButton();

            box.AddControls(btn_Construct, btn_ConstructDoors, btn_ConstructFurniture, btn_ConstructProduction, btn_Remove);
            box.AlignLeftToRight();
            return box;
        }
        internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        {
            //foreach (var cat in AllCategories)
            //    yield return new Tuple<string, Action>(string.Format("Build {0}", cat.Name), () => cat.GetWindow().Toggle());
            yield return new Tuple<string, Action>(string.Format("{0} ({1})", "Build", KeyBind.Build.Key), () => WindowBuild.Toggle());
        }

        
        public override void Write(BinaryWriter w)
        {
            this.Designations.Write(w);
            this.PendingDesignations.Values.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            this.Designations.Read(r);
            this.PendingDesignations.Read(r, i => i.Global);
        }

        protected override void AddSaveData(SaveTag tag)
        {
            this.Designations.Save(tag, "Designations");
            this.PendingDesignations.Values.SaveNewBEST(tag, "PendingDesignations");
        }
        public override void Load(SaveTag tag)
        {
            this.Designations.Load(tag, "Designations");
            this.PendingDesignations.Load(tag, "PendingDesignations", i => i.Global);
        }
       
        public override void Handle(IObjectProvider net, Packet msg)
        {
            switch (msg.PacketType)
            {

                case PacketType.ConstructionRemove:
                    msg.Payload.Deserialize(r =>
                    {
                        var netid = r.ReadInt32();
                        var a = r.ReadVector3();
                        var b = r.ReadVector3();
                        var box = new BoundingBox(a, b);
                        var positions = box.GetBox();
                        foreach (var p in positions)
                            net.Map.RemoveBlock(p);
                        var server = net as Server;
                        if (server != null)
                            server.Enqueue(PacketType.ConstructionRemove, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    break;

                default:
                    break;
            }
        }

        internal override void OnGameEvent(GameEvent e)
        {
            //IMap map;
            switch(e.Type)
            {
                /// no need to remove designations here because the designationmanager handles it
                //case Components.Message.Types.BlocksChanged:
                //    foreach(var pos in e.Parameters[1] as IEnumerable<Vector3>)
                //        Handle(this.Town.Map, pos);
                //        //HandleNew(this.Town.Map, pos);
                //    break;
                //case Components.Message.Types.BlockChanged:
                //    {
                //        Vector3 global;
                //        GameEvents.EventBlockChanged.Read(e.Parameters, out map, out global);
                //        //HandleNew(map, global);
                //        Handle(map, global);
                //    }
                //    break;
                /// no need to remove designations here because the designationmanager handles it

                /// I ACTUALLY NEED IT TO ADD PENDING DESIGNATIONS
                case Components.Message.Types.BlocksChanged:
                    foreach (var pos in e.Parameters[1] as IEnumerable<Vector3>)
                        this.TryAddPendingDesignation(pos);
                    break;
                case Components.Message.Types.BlockChanged:
                    throw new Exception();
                    break;


                case Components.Message.Types.ConstructionRemoved:
                case Components.Message.Types.ConstructionAdded:
                    throw new Exception();

                    foreach (var pos in e.Parameters[0] as IEnumerable<Vector3>)
                        Handle(this.Town.Map, pos);
                    break;

                case Components.Message.Types.ZoneDesignation:
                    this.Add(e.Parameters[0] as DesignationDef, e.Parameters[1] as List<Vector3>, (bool)e.Parameters[2]);
                    break;

                default:
                    break;
            }
        }

        private void Add(DesignationDef designation, List<Vector3> positions, bool remove)
        {
            //if(designation == Designation.Null)// && remove)
            //{
            //    foreach(var pos in positions)
            //    {
            //        if (this.PendingDesignations.ContainsKey(pos))
            //            this.PendingDesignations.Remove(pos);
            //    }
            //}
            //return;
            if (designation == DesignationDef.Null)// && remove)
            {
                foreach (var pos in positions)
                {
                    //if (this.Designations.Contains(pos))
                    if(this.Map.GetBlockEntity<BlockDesignation.BlockDesignationEntity>(pos) is BlockDesignation.BlockDesignationEntity blockEntity)
                    {
                        var origin = blockEntity.OriginGlobal;
                        //this.Designations.Remove(origin);
                        this.Map.RemoveBlockNew(origin);

                        //foreach (var global in blockEntity.CellsOccupied)
                        //{
                        //    this.Map.RemoveBlockNew(global);
                        //}
                    }
                    else if (this.PendingDesignations.ContainsKey(pos))
                        this.PendingDesignations.Remove(pos);
                }
            }
            return;

            //if (designation.GetType() == Designation.Null.GetType())
            //    this.Map.RemoveBlocks(positions.Where(p => this.Designations.Contains(p)).ToList());
        }

        private void Handle(IMap map, Vector3 global)
        {
            throw new Exception();
            var block = map.GetBlock(global);
            //IConstructible entity = map.GetBlockEntity(global) as IConstructible;
            var entity = map.GetBlockEntity(global);
            if (entity != null)///block == Block.Designation)
            {
                //var origin = entity.Origin;
                var origin = entity.OriginGlobal;
                this.Designations.Add(origin);
            }
            else
            {
                //AITask existingtask;
                //if (this.Tasks.TryGetValue(global, out existingtask))
                //{
                //    this.Tasks = this.Tasks.Where(p => p.Value != existingtask).ToDictionary(p=>p.Key, p=>p.Value);
                //}

                if (block == BlockDefOf.Air)
                {
                    if (this.PendingDesignations.TryGetValue(global, out var pending))
                    {
                        //BlockDesignation.Place(map, global, 0, 0, pending.Orientation, pending.Product);
                        this.PlaceDesignation(global, 0, 0, pending.Orientation, pending.Product);

                        this.PendingDesignations.Remove(global);
                    }
                }
                else
                {
                    if (this.Designations.Contains(global))
                    {
                        this.Designations.Remove(global);
                    }
                }
            }
        }
        bool TryAddPendingDesignation(IntVec3 global)
        {
            var map = this.Map;
            if (this.PendingDesignations.TryGetValue(global, out var pending))
            {
                if (map.GetBlock(global) is BlockAir)
                {
                    //BlockDesignation.Place(map, global, 0, 0, pending.Orientation, pending.Product);
                    this.PlaceDesignation(global, 0, 0, pending.Orientation, pending.Product);

                    this.PendingDesignations.Remove(global);
                    return true;
                }
            }
            return false;
        }
       

        internal bool IsDesignatedConstruction(Vector3 vector3)
        {
            return this.Designations.Contains(vector3);// && this.IsBuildable(vector3);
        }
        internal bool IsBuildableCurrently(IntVec3 global)
        {
            // check if there's an adjacent solid block
            //var adj = global.GetNeighbors();
            if (!this.IsDesignatedConstruction(global))
                return false;
            return this.Map.IsAdjacentToSolid(global);
        }

        

        public string GetName()
        {
            return "Build";
        }

        public void GetSelectionInfo(IUISelection panel)
        {
            panel.AddInfo(new Label() { Text = "Buildings" });
        }

        public void GetQuickButtons(UISelectedInfo panel)
        {
            //throw new NotImplementedException();
        }

        //internal void Add(List<Vector3> positions, BlockConstruction.ProductMaterialPair product, int orientation)
        //{
        //    //var block = product.Block;
        //    //product.Block.Place(
        //    //    this.Map,
        //    //    positions,
        //    //    product.Data,
        //    //    orientation,
        //    //    false);
        //    foreach (var pos in positions)
        //    {
        //        BlockDesignation.Place(this.Map, pos, 0, 0, orientation, product);
        //    }
        //}

        public void Handle(ToolDrawing.Args args, BlockRecipe.ProductMaterialPair product, List<Vector3> positions)
        {
            var cheat = false;// true;// args.Cheat;
            var map = this.Map;
            if (cheat)
            {
                PlaceDesignationsGodMode(args, product, positions, map);
            }
            else
            {
                PlaceDesignations(args, product, positions);
            }
        }

        private void PlaceDesignations(ToolDrawing.Args args, BlockRecipe.ProductMaterialPair product, List<Vector3> positions)
        {
            var map = this.Town.Map;
            if (args.Removing)
            {
                foreach(var pos in positions)
                {
                    if(map.GetBlockEntity(pos) is BlockDesignation.BlockDesignationEntity desEntity)
                    {
                        this.Designations.Remove(desEntity.OriginGlobal);
                    }
                }
                map.RemoveBlocks(positions.Where(vec => map.GetBlock(vec) == BlockDefOf.Designation).ToList(), false);
            }
            else
                foreach (var pos in positions)
                {
                    if (map.GetBlock(pos) == BlockDefOf.Air)
                    {
                        //BlockDesignation.Place(map, pos, 0, 0, args.Orientation, product);
                        this.PlaceDesignation(pos, 0, 0, args.Orientation, product);
                        this.Designations.Add(pos);
                    }
                    else
                    {
                        this.Town.DesignationManager.Add(DesignationDef.Mine, pos);
                        this.PendingDesignations[pos] = new ConstructionParams(pos, args.Orientation, product);
                    }
                    //var designationParams = new ConstructionParams(args.Orientation, product);// new BlockDesignation.BlockDesignationEntity(product, pos);
                    //this.DesignationsNew[pos] = designationParams;
                    //this.Constructions.Add(pos);
                    //// if the cell isn't empty, set its block to a blockdesignation
                    //var cell = map.GetCell(pos);
                    //if(cell.Block == Block.Air)
                    //    BlockDesignation.Place(map, pos, 0, 0, args.Orientation, product);
                }
        }

        private static void PlaceDesignationsGodMode(ToolDrawing.Args args, BlockRecipe.ProductMaterialPair product, List<Vector3> positions, IMap map)
        {
            if (!args.Removing)
            {
                product.Block.Place(
                    map,
                    positions.Where(vec => args.Replacing ? map.GetBlock(vec) != BlockDefOf.Air : map.GetBlock(vec) == BlockDefOf.Air).ToList(),
                    product.Data,
                    args.Orientation
                    , true);
                //map.Town.ConstructionsManager.Add(positions.Where(vec => args.Replacing ? map.GetBlock(vec) != Block.Air : map.GetBlock(vec) == Block.Air).ToList(), product, args.Orientation);
            }
            else
            {
                map.RemoveBlocks(positions);
            }
        }

        public void TabGetter(Action<string, Action> getter)
        {
            throw new NotImplementedException();
        }
        public void PlaceDesignation(Vector3 global, byte data, int variation, int orientation, BlockRecipe.ProductMaterialPair product)
        {
            var map = this.Map;
            var entity = new BlockDesignation.BlockDesignationEntity(product, global);
            bool ismulti = product.Block.Multi;
            //map.AddBlockEntity(global, entity); // DIDNT I DECIDE THAT BLOCKENTITIES WILL BE PLACE ONLY IN THE ORIGIN CELL???
            // LAST DECISION: add the same entity to all occupied cells
            // NOT FOR BLOCKDESIGNATION because i add every entity and child entities should have their origin field set
            if (ismulti)
            {
                var parts = product.Block.GetParts(global, orientation);
                foreach (var p in parts)
                {
                    map.AddBlockEntity(p.Key, entity);// DIDNT I DECIDE THAT BLOCKENTITIES WILL BE PLACE ONLY IN THE ORIGIN CELL???
                    entity.Children.Add(p.Key);
                    map.SetBlock(p.Key, Block.Types.Designation, p.Value, variation, orientation, false);
                }
                //map.EventOccured(Message.Types.ConstructionAdded, parts.Keys);
            }
            else
            {
                map.AddBlockEntity(global, entity);// DIDNT I DECIDE THAT BLOCKENTITIES WILL BE PLACE ONLY IN THE ORIGIN CELL???
                entity.Children.Add(global);
                map.SetBlock(global, Block.Types.Designation, data, variation, orientation, false); // i put this last because there are blockchanged event handlers that look up the block entity which hadn't beeen added yet when I set the block beforehand
                //map.EventOccured(Message.Types.ConstructionAdded, new Vector3[] { global });
            }
            //map.Town.ConstructionsManager.Add(map, global); // TODO: handle block change event instead
            // TODO: add blockentities on construction manager instead?
        }

        public IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            throw new NotImplementedException();
        }

        class ConstructionParams : ISaveable, ISerializable
        {
            public IntVec3 Global;
            public int Orientation;
            public BlockRecipe.ProductMaterialPair Product;
            public ConstructionParams()
            {

            }
            public ConstructionParams(IntVec3 global, int orientation, BlockRecipe.ProductMaterialPair product)
            {
                this.Global = global;
                Orientation = orientation;
                Product = product;
            }

            public SaveTag Save(string name = "")
            {
                var tag = new SaveTag(SaveTag.Types.Compound, name);
                this.Global.Save(tag, "Global");
                this.Orientation.Save(tag, "Orientation");
                this.Product.Save(tag, "Product");
                return tag;
            }
            public ISaveable Load(SaveTag tag)
            {
                //tag.TryGetTag("Product", t => Product = new BlockRecipe.ProductMaterialPair(t));
                this.Global = tag.LoadIntVec3("Global");
                this.Product = new BlockRecipe.ProductMaterialPair(tag["Product"]);
                this.Orientation = tag.GetValue<int>("Orientation");
                return this;
            }
           
            public void Write(BinaryWriter w)
            {
                w.Write(this.Global);
                w.Write(this.Orientation);
                this.Product.Write(w);
            }
            public ISerializable Read(BinaryReader r)
            {
                this.Global = r.ReadIntVec3();
                this.Orientation = r.ReadInt32();
                this.Product = new(r);
                return this;
            }
        }
    }
}
