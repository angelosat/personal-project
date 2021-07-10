using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Blocks;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Modules.Construction;
using System.IO;

namespace Start_a_Town_.Towns.Constructions
{
    public class ConstructionsManager : TownComponent, ISelectable
    {
        public override string Name => "Constructions";

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
        public ConstructionsManager(Town town)
        {
            this.Town = town;
        }
        readonly Dictionary<Vector3, ConstructionParams> PendingDesignations = new();
        readonly HashSet<IntVec3> Designations = new();
       
        internal IEnumerable<IntVec3> GetAllBuildableCurrently()
        {
            return this.Designations.Where(IsBuildableCurrently);
        }
        
        internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        {
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
       
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                /// I ACTUALLY NEED IT TO ADD PENDING DESIGNATIONS
                case Components.Message.Types.BlocksChanged:
                    foreach (var pos in e.Parameters[1] as IEnumerable<Vector3>)
                        this.TryAddPendingDesignation(pos);
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
            if (designation == DesignationDef.Null)
            {
                foreach (var pos in positions)
                {
                    if(this.Map.GetBlockEntity<BlockDesignation.BlockDesignationEntity>(pos) is BlockDesignation.BlockDesignationEntity blockEntity)
                    {
                        var origin = blockEntity.OriginGlobal;
                        this.Map.RemoveBlockNew(origin);
                    }
                    else if (this.PendingDesignations.ContainsKey(pos))
                        this.PendingDesignations.Remove(pos);
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
                    this.PlaceDesignation(global, 0, 0, pending.Orientation, pending.Product);
                    this.PendingDesignations.Remove(global);
                    return true;
                }
            }
            return false;
        }
       
        internal bool IsDesignatedConstruction(Vector3 vector3)
        {
            return this.Designations.Contains(vector3);
        }
        internal bool IsBuildableCurrently(IntVec3 global)
        {
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
        }

        public void Handle(ToolDrawing.Args args, BlockRecipe.ProductMaterialPair product, List<Vector3> positions)
        {
            var cheat = false;
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
                        this.PlaceDesignation(pos, 0, 0, args.Orientation, product);
                        this.Designations.Add(pos);
                    }
                    else
                    {
                        this.Town.DesignationManager.Add(DesignationDef.Mine, pos);
                        this.PendingDesignations[pos] = new ConstructionParams(pos, args.Orientation, product);
                    }
                }
        }

        private static void PlaceDesignationsGodMode(ToolDrawing.Args args, BlockRecipe.ProductMaterialPair product, List<Vector3> positions, MapBase map)
        {
            if (!args.Removing)
            {
                product.Block.Place(
                    map,
                    positions.Where(vec => args.Replacing ? map.GetBlock(vec) != BlockDefOf.Air : map.GetBlock(vec) == BlockDefOf.Air).ToList(),
                    product.Data,
                    args.Orientation
                    , true);
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
            // LATEST DECISION: add the same entity to all occupied cells
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
            }
            else
            {
                map.AddBlockEntity(global, entity);// DIDNT I DECIDE THAT BLOCKENTITIES WILL BE PLACE ONLY IN THE ORIGIN CELL???
                entity.Children.Add(global);
                map.SetBlock(global, Block.Types.Designation, data, variation, orientation, false); // i put this last because there are blockchanged event handlers that look up the block entity which hadn't beeen added yet when I set the block beforehand
            }
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
