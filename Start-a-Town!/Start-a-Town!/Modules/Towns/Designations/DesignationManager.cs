﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Towns;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class DesignationManager : TownComponent
    {
        public override string Name => "Designation Manager";

        readonly Dictionary<DesignationDef, HashSet<Vector3>> Designations;

        static DesignationManager()
        {
            PacketDesignation.Init();
        }

        internal HashSet<Vector3> GetDesignations(DesignationDef des)
        {
            return this.Designations[des];
        }

        internal bool RemoveDesignation(DesignationDef des, Vector3 global)
        {
            var removed = this.Designations[des].Remove(global);
            if (removed)
                this.UpdateQuickButtons();
            return removed;
        }

        public DesignationManager(Town town) : base(town)
        {
            Designations = new Dictionary<DesignationDef, HashSet<Vector3>>();
            Designations.Add(DesignationDef.Deconstruct, new HashSet<Vector3>());
            Designations.Add(DesignationDef.Mine, new HashSet<Vector3>());
            Designations.Add(DesignationDef.Switch, new HashSet<Vector3>());
        }
        internal void Add(DesignationDef designation, Vector3 position, bool remove = false)
        {
            this.Add(designation, new List<Vector3>(1) { position }, remove);
        }
        internal void Add(DesignationDef designation, List<Vector3> positions, bool remove)
        {
            if (designation == DesignationDef.Null)
            {
                foreach (var l in Designations.Values)
                    foreach (var p in positions)
                        l.Remove(p);
            }
            else
            {
                var list = Designations[designation];
                foreach (var pos in positions)
                {
                    if (remove)
                        list.Remove(pos);
                    else
                    {
                        if (designation.IsValid(this.Town.Map, pos))
                            list.Add(pos);
                    }
                }
            }
            this.UpdateQuickButtons();
        }

        public override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            foreach (var des in Designations)
                    cam.DrawGridBlocks(sb, Block.BlockBlueprint, des.Value, Color.White);
        }
        public DesignationDef GetDesignation(Vector3 global)
        {
            return this.Designations.FirstOrDefault(d => d.Value.Contains(global)).Key; // will this return null if no designation?
        }
        internal bool IsDesignation(Vector3 global)
        {
            return Designations.Values.Any(v => v.Contains(global));
        }
        internal bool IsDesignation(Vector3 global, DesignationDef desType)
        {
            var contains = Designations[desType].Contains(global);
            return contains;
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.BlocksChanged:
                    HandleBlocksChanged(e.Parameters[1] as IEnumerable<Vector3>);
                    break;

                case Components.Message.Types.BlockChanged:
                    MapBase map;
                    Vector3 global;
                    GameEvents.EventBlockChanged.Read(e.Parameters, out map, out global);
                    HandleBlocksChanged(new Vector3[] { global });
                    break;

                case Components.Message.Types.ZoneDesignation:
                    this.Add(e.Parameters[0] as DesignationDef, e.Parameters[1] as List<Vector3>, (bool)e.Parameters[2]);
                    break;

                default:
                    break;
            }
        }

        private void HandleBlocksChanged(IEnumerable<Vector3> globals)
        {
            foreach (var desType in Designations)
            {
                var des = DesignationDef.Dictionary[desType.Key.Name];
                foreach (var global in globals)
                {
                    if (!des.IsValid(this.Map, global))
                        desType.Value.Remove(global);
                }
            }
        }

        protected override void AddSaveData(SaveTag tag)
        {
            foreach (var des in Designations)
                tag.Add(des.Value.ToList().Save(des.Key.Name));
        }
        public override void Load(SaveTag tag)
        {
            foreach (var des in Designations.Keys.ToList())
                tag.TryGetTagValue<List<SaveTag>>(des.Name, v => Designations[des] = new HashSet<Vector3>(new List<Vector3>().Load(v)));
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            foreach(var des in Designations)
                w.Write(des.Value);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            foreach (var des in Designations.Keys.ToList())
                Designations[des] = new HashSet<Vector3>(r.ReadListVector3());
        }

        internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<string, Action>("Cancel designations", this.Cancel);
        }

        private void Cancel()
        {
            ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDesignation.Send(Client.Instance, DesignationDef.Null, a, b, r)));
        }
        internal override void UpdateQuickButtons()
        {
            if (this.Town.Net is Net.Server)
                return;
            var selectedCells = UISelectedInfo.GetSelectedPositions();
            var fromblockentities = selectedCells.Select(this.Map.GetBlockEntity).OfType<Blocks.BlockEntity>().Select(b => (Vector3)b.OriginGlobal);
            selectedCells = selectedCells.Concat(fromblockentities).Distinct();
            
            var areTask = selectedCells.Where(e => this.Designations.Values.Any(t => t.Contains(e)));
            foreach (var d in this.Designations) // need to handle construction designations differently because of multi-celled designations 
            {
                var selectedDesignations = d.Value.Intersect(selectedCells);
                if (selectedDesignations.Any())
                    UISelectedInfo.AddButton(d.Key.IconRemove, DesignationDef.Cancel, selectedDesignations.Select(i => new TargetArgs(this.Map, i)));
                else
                    UISelectedInfo.RemoveButton(d.Key.IconRemove);
            }

            var areNotTask = selectedCells.Except(areTask).Where(t =>
                DesignationDef.All.Any(d => d.IsValid(this.Town.Map, t))).ToList();

            var splits = DesignationDef.All.ToDictionary(d => d, d => areNotTask.FindAll(t => d.IsValid(this.Map, t)));
            foreach (var s in DesignationDef.All)
            {
                if (!splits.TryGetValue(s, out var list) || !list.Any())
                    UISelectedInfo.RemoveButton(s.IconAdd);
                else
                    UISelectedInfo.AddButton(s.IconAdd, targets => MineAdd(targets, s), list.Select(i => new TargetArgs(this.Map, i)));
            }
        }
        static public readonly Icon MineIcon = new Icon(ItemContent.PickaxeFull);

        static void MineAdd(List<TargetArgs> targets, DesignationDef des)
        {
            PacketDesignation.Send(Client.Instance, des, targets, false);
        }
    }
}
