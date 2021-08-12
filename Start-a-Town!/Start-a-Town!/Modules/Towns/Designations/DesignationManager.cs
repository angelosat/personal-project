using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class DesignationManager : TownComponent
    {
        public override string Name => "Designation Manager";

        readonly ReadOnlyDictionary<DesignationDef, ObservableCollection<IntVec3>> Designations;
        readonly Dictionary<DesignationDef, BlockRendererObservable> Renderers = new();

        static DesignationManager()
        {
            PacketDesignation.Init();

            Hotkey = HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContext, "Designations", ToggleGui, System.Windows.Forms.Keys.U);
            
            foreach(var d in DesignationDef.Dictionary)
                HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContext, $"Designate: {d.Key}", delegate { SetTool(d.Value); });
        }

        internal ObservableCollection<IntVec3> GetDesignations(DesignationDef des)
        {
            return this.Designations[des];
        }

        internal bool RemoveDesignation(DesignationDef des, IntVec3 global)
        {
            var removed = this.Designations[des].Remove(global);
            if (removed)
                this.UpdateQuickButtons();
            return removed;
        }

        public DesignationManager(Town town) : base(town)
        {
            Designations = new ReadOnlyDictionary<DesignationDef, ObservableCollection<IntVec3>>(
                new Dictionary<DesignationDef, ObservableCollection<IntVec3>>() {
                { DesignationDef.Deconstruct, new ObservableCollection<IntVec3>() },
                { DesignationDef.Mine, new ObservableCollection<IntVec3>()},
                { DesignationDef.Switch, new ObservableCollection<IntVec3>()} 
            });

            Renderers.Add(DesignationDef.Deconstruct, new(Designations[DesignationDef.Deconstruct]));
            Renderers.Add(DesignationDef.Mine, new(Designations[DesignationDef.Mine]));
            Renderers.Add(DesignationDef.Switch, new(Designations[DesignationDef.Switch]));
        }
        internal void Add(DesignationDef designation, IntVec3 position, bool remove = false)
        {
            this.Add(designation, new List<IntVec3>(1) { position }, remove);
        }
        internal void Add(DesignationDef designation, List<IntVec3> positions, bool remove)
        {
            if (designation == DesignationDef.Remove)
            {
                foreach (var l in Designations)
                    foreach (var p in positions)
                        l.Value.Remove(p);
            }
            else
            {
                var list = Designations[designation];
                foreach (var pos in positions)
                {
                    if (remove)
                        list.Remove(pos);
                    else if (designation.IsValid(this.Town.Map, pos) || this.Map.IsUndiscovered(pos))
                        list.Add(pos);
                }
            }
            this.UpdateQuickButtons();
        }

        public override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            foreach (var r in this.Renderers)
                r.Value.DrawBlocks(map, cam);
        }
        public DesignationDef GetDesignation(IntVec3 global)
        {
            return this.Designations.FirstOrDefault(d => d.Value.Contains(global)).Key; // will this return null if no designation?
        }
        internal bool IsDesignation(IntVec3 global)
        {
            return Designations.Values.Any(v => v.Contains(global));
        }
        internal bool IsDesignation(IntVec3 global, DesignationDef desType)
        {
            var contains = Designations[desType].Contains(global);
            return contains;
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.BlocksChanged:
                    HandleBlocksChanged(e.Parameters[1] as IEnumerable<IntVec3>);
                    break;

                case Components.Message.Types.ZoneDesignation:
                    this.Add(e.Parameters[0] as DesignationDef, e.Parameters[1] as List<IntVec3>, (bool)e.Parameters[2]);
                    break;

                default:
                    break;
            }
        }

        private void HandleBlocksChanged(IEnumerable<IntVec3> globals)
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
                tag.TryGetTag(des.Name, v => Designations[des].LoadIntVecs(v));
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            foreach(var des in Designations)
                w.Write(des.Value);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            foreach (var des in Designations.Keys.ToList())
                Designations[des].ReadIntVec3(r);
        }

        internal override IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<Func<string>, Action>(() => $"Designations [{Hotkey.GetLabel()}]", ToggleGui);
        }
        static Window _gui;

        public static void ToggleGui()
        {
            if (_gui is null)
            {
                var box = new ListBoxNoScroll<DesignationDef, Button>(createButton, 0).AddItems(Ingame.CurrentMap.Town.DesignationManager.Designations.Keys.Prepend(DesignationDef.Remove));
                box.BackgroundColor = Microsoft.Xna.Framework.Color.Black * .5f;
                _gui = box.ToWindow("Designations").Transparent();
                _gui.Location = Controller.Instance.MouseLocation;
            }
            _gui.Toggle();
           
            Button createButton(DesignationDef d)
            {
                var btn = new Button(d.Label, () => SetTool(d), 96) { Tag = d };
                btn.IsToggledFunc = () => ToolManager.Instance.ActiveTool is ToolDigging tool && btn.Tag == tool.DesignationDef;
                return btn;
            }
        }

        private static void SetTool(DesignationDef d)
        {
            ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDesignation.Send(Client.Instance, d, a, b, r)) { DesignationDef = d });
        }

        static void Cancel()
        {
            ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDesignation.Send(Client.Instance, DesignationDef.Remove, a, b, r)));
        }
        internal override void UpdateQuickButtons()
        {
            if (this.Town.Net is Server)
                return;
            var selectedCells = SelectionManager.GetSelectedCells();
            var fromblockentities = selectedCells.Select(this.Map.GetBlockEntity).OfType<Blocks.BlockEntity>().Select(b => b.OriginGlobal);
            selectedCells = selectedCells.Concat(fromblockentities).Distinct();
            
            var areTask = selectedCells.Where(e => this.Designations.Values.Any(t => t.Contains(e)));
            foreach (var d in this.Designations) // need to handle construction designations differently because of multi-celled designations 
            {
                var selectedDesignations = d.Value.Intersect(selectedCells);
                if (selectedDesignations.Any())
                    SelectionManager.AddButton(d.Key.IconRemove, cancel, selectedDesignations.Select(i => new TargetArgs(this.Map, i)));
                else
                    SelectionManager.RemoveButton(d.Key.IconRemove);
            }

            var areNotTask = selectedCells.Except(areTask).Where(t =>
                DesignationDef.All.Any(d => d.IsValid(this.Town.Map, t))).ToList();

            var splits = DesignationDef.All.ToDictionary(d => d, d => areNotTask.FindAll(t => d.IsValid(this.Map, t)));
            foreach (var s in DesignationDef.All)
            {
                if (!splits.TryGetValue(s, out var list) || !list.Any())
                    SelectionManager.RemoveButton(s.IconAdd);
                else
                    SelectionManager.AddButton(s.IconAdd, targets => MineAdd(targets, s), list.Select(i => new TargetArgs(this.Map, i)));
            }

            static void cancel(List<TargetArgs> positions)
            {
                PacketDesignation.Send(Client.Instance, DesignationDef.Remove, positions, false);
            }
        }
        static public readonly Icon MineIcon = new(ItemContent.PickaxeFull);
        private static readonly IHotkey Hotkey;

        static void MineAdd(List<TargetArgs> targets, DesignationDef des)
        {
            PacketDesignation.Send(Client.Instance, des, targets, false);
        }
    }
}
