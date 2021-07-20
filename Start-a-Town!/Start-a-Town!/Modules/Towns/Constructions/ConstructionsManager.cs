﻿using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Towns;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class ConstructionsManager : TownComponent
    {
        public override string Name => "Constructions";

        public static ConstructionCategoryWalls Walls = new();
        public static ConstructionCategoryDoors Doors = new();
        public static ConstructionCategoryProduction Production = new();
        public static ConstructionCategoryFurniture Furniture = new();

        public static List<ConstructionCategory> AllCategories = new()
        {
            Walls,
            Doors,
            Production,
            Furniture
        };
        static readonly Lazy<GuiConstructionsBrowser> WindowBuild = new();
        static ConstructionsManager()
        {
            HotkeyManager.RegisterHotkey("Town", "Build", () => WindowBuild.Value.Toggle(), System.Windows.Forms.Keys.B);
            HotkeyManager.RegisterHotkey("Ingame", "Pause/Resume", delegate { }, System.Windows.Forms.Keys.Space);
            HotkeyManager.RegisterHotkey("Ingame", "Speed: Normal", delegate { }, System.Windows.Forms.Keys.D1);
            HotkeyManager.RegisterHotkey("Ingame", "Speed: Fast", delegate { }, System.Windows.Forms.Keys.D2);
            HotkeyManager.RegisterHotkey("Ingame", "Speed: Faster", delegate { }, System.Windows.Forms.Keys.D3);

        }
        public ConstructionsManager(Town town)
        {
            this.Town = town;
        }
        readonly Dictionary<Vector3, ConstructionParams> PendingDesignations = new();
        readonly HashSet<IntVec3> Designations = new();

        internal IEnumerable<IntVec3> GetAllBuildableCurrently()
        {
            return this.Designations.Where(this.IsBuildableCurrently);
        }

        internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<string, Action>($"Build ({KeyBind.Build.Key})", () => WindowBuild.Value.Toggle());
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
            switch (e.Type)
            {
                /// I ACTUALLY NEED IT TO ADD PENDING DESIGNATIONS
                case Components.Message.Types.BlocksChanged:
                    foreach (var pos in e.Parameters[1] as IEnumerable<IntVec3>)
                        this.TryAddPendingDesignation(pos);
                    break;

                case Components.Message.Types.ZoneDesignation:
                    this.Add(e.Parameters[0] as DesignationDef, e.Parameters[1] as List<IntVec3>, (bool)e.Parameters[2]);
                    break;

                default:
                    break;
            }
        }

        private void Add(DesignationDef designation, List<IntVec3> positions, bool remove)
        {
            if (designation == DesignationDef.Null)
            {
                foreach (var pos in positions)
                {
                    if (this.Map.GetBlockEntity<BlockDesignation.BlockDesignationEntity>(pos) is BlockDesignation.BlockDesignationEntity blockEntity)
                    {
                        var origin = blockEntity.OriginGlobal;
                        this.Map.RemoveBlock(origin);
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

        public void Handle(ToolDrawing.Args args, ProductMaterialPair product, List<IntVec3> positions)
        {
            var cheat = false;
            var map = this.Map;
            if (cheat)
                PlaceDesignationsGodMode(args, product, positions, map);
            else
                this.PlaceDesignations(args, product, positions);
        }

        private void PlaceDesignations(ToolDrawing.Args args, ProductMaterialPair product, List<IntVec3> positions)
        {
            var map = this.Town.Map;
            if (args.Removing)
            {
                foreach (var pos in positions)
                {
                    if (map.GetBlockEntity(pos) is BlockDesignation.BlockDesignationEntity desEntity)
                    {
                        this.Designations.Remove(desEntity.OriginGlobal);
                    }
                }
                map.RemoveBlocks(positions.Where(vec => map.GetBlock(vec) == BlockDefOf.Designation), false);
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

        private static void PlaceDesignationsGodMode(ToolDrawing.Args args, ProductMaterialPair product, List<IntVec3> positions, MapBase map)
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
        public void PlaceDesignation(Vector3 global, byte data, int variation, int orientation, ProductMaterialPair product)
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
            public ProductMaterialPair Product;
            public ConstructionParams()
            {

            }
            public ConstructionParams(IntVec3 global, int orientation, ProductMaterialPair product)
            {
                this.Global = global;
                this.Orientation = orientation;
                this.Product = product;
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
                this.Product = new ProductMaterialPair(tag["Product"]);
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
