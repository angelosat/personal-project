﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class UISelectedInfo : GroupBox, IUISelection
    {
        public static UISelectedInfo Instance = new UISelectedInfo();

        GroupBox BoxTabs, BoxButtons, BoxIcons, BoxInfo;
        public Panel PanelInfo;
        public Label LabelName;
        IconButton IconInfo, IconCenter;
        IconButton IconCycle;
        int PreviousDrawLevel = -1;

        QuickButton IconSlice = new QuickButton(Icon.ArrowDown, KeyBind.SliceZ)
        {
            BackgroundTexture = UIManager.Icon16Background,
            LeftClickAction = Slice,
            HoverText = "Slice z-level"
        };
        public TargetArgs SelectedSource = TargetArgs.Null;
        ISelectable Selectable;
        Window WindowInfo;
        IEnumerator<ISelectable> SelectedStack;
        public IEnumerable<TargetArgs> MultipleSelected = new List<TargetArgs>(); // TODO: make this a list of iselectables

        int MultipleSelectedCount
        {
            get { return this.MultipleSelected.Count(); }
            set
            {
            }
        }

        UISelectedInfo()
        {
            this.PanelInfo = new Panel(new Rectangle(0, 0, 300, 100));
            this.BoxTabs = new GroupBox()
            {
                AutoSize = false,
                Size = new Rectangle(0, 0, 300, Button.DefaultHeight)
            };
            this.PanelInfo.Location = this.BoxTabs.BottomLeft;
            this.LabelName = new Label() { TextFunc = () => "<none>" };
            this.IconInfo = new IconButton(Icon.ArrowUp)
            {
                BackgroundTexture = UIManager.Icon16Background,
                LeftClickAction = ToggleInfo,
                HoverText = "Show details"
            };
            this.IconCenter = new IconButton(Icon.ArrowUp)
            {
                BackgroundTexture = UIManager.Icon16Background,
                LeftClickAction = CenterCamera,
                HoverText = "Center camera"
            };
            this.IconCycle = new IconButton(Icon.Replace)
            {
                BackgroundTexture = UIManager.Icon16Background,
                LeftClickAction = this.CycleTargets,
                HoverText = "Cycle targets"
            };
           
            this.BoxIcons = new GroupBox();
            this.PopulateBoxIcons();
            
            this.BoxButtons = new GroupBox();
            BoxButtons.BackgroundColorFunc = () => Color.Black * .5f;
            this.BoxButtons.LocationFunc = () => this.BottomRight;
            this.BoxButtons.Anchor = new Vector2(0, 1);
            this.BoxButtons.ControlsChangedAction = this.BoxButtons.AlignLeftToRight;

            this.BoxInfo = new GroupBox() { Location = this.LabelName.BottomLeft};
            this.PanelInfo.AddControls(
                this.LabelName,
                this.BoxIcons,
                this.BoxInfo
                );
            
            this.AddControls(
                this.BoxTabs,
                this.PanelInfo
                );
        }

        private void RepositionsBoxIcons()
        {
            this.BoxIcons.AlignLeftToRight();
            this.BoxIcons.Location = new Vector2(this.PanelInfo.ClientSize.Right - Panel.DefaultStyle.Border, this.PanelInfo.ClientSize.Top);
            this.BoxIcons.Anchor = new Vector2(1, 0);
        }

        private void PopulateBoxIcons()
        {
            this.BoxIcons.ClearControls();
            this.BoxIcons.AddControls(
                this.IconSlice,
                this.IconCenter,
                this.IconInfo
                );

            if (this.SelectedStack != null)
                this.BoxIcons.AddControls(this.IconCycle);

            this.RepositionsBoxIcons();
        }

        private void CenterCamera()
        {
            if (this.SelectedSource != null)
                if (this.SelectedSource.Type != TargetType.Null)
                    ScreenManager.CurrentScreen.Camera.CenterOn(this.SelectedSource.Global);
        }
        public void SetName(string text)
        {
            this.LabelName.TextFunc = () => text;
        }
        static private void ToggleInfo()
        {
            if(Instance.WindowInfo is null)
                Instance.WindowInfo = WindowTargetManagementStatic.Refresh(Instance.SelectedSource);
            else
            {
                Instance.WindowInfo.Toggle();
            }
        }
        
        static public void Refresh(TargetArgs target)
        {
            Instance.Select(target);
        }
        static public void Refresh(MapBase map, BoundingBox box)
        {
            Refresh(map.GetObjects(box).Select(s => new TargetArgs(s)));
        }
        internal static void Refresh(IEnumerable<GameObject> entities)
        {
            Refresh(entities.Select(e => new TargetArgs(e)));
        }
        static public void Refresh(IEnumerable<TargetArgs> targets)
        {
            Instance.Select(targets);
        }
        internal static void SelectAllVisible(ItemDef def)
        {
            var objects = Rooms.Ingame.Instance.Scene.ObjectsDrawn.Where(i => i.Def == def).Select(o => new TargetArgs(o));
            Refresh(objects);
        }
        internal static void AddToSelection(IEnumerable<GameObject> targets)
        {
            AddToSelection(targets.Select(o => new TargetArgs(o)));
        }
        internal static void AddToSelection(IEnumerable<TargetArgs> targets)
        {
            var list = Instance.MultipleSelected.Where(t => !targets.Any(t2 => t2.IsEqual(t))).Concat(targets).ToList();
            Instance.Select(list);
        }
        internal static void AddToSelection(TargetArgs target)
        {
            var existing = Instance.MultipleSelected.FirstOrDefault(t => t.IsEqual(target));
            if (existing != null)
                Instance.Select(Instance.MultipleSelected.Except(new TargetArgs[] { existing }));
            else
                Instance.Select(Instance.MultipleSelected.Concat(new TargetArgs[] { target }));
        }
        private IEnumerable<TargetArgs> Filter(IEnumerable<TargetArgs> targets)
        {
            if (targets.Any(i => i.Type == TargetType.Entity && i.Object.HasComponent<NpcComponent>()))
                return targets.Where(i => i.Type == TargetType.Entity && i.Object.HasComponent<NpcComponent>());
            return targets;
        }
        
        private void Select(IEnumerable<TargetArgs> targets)
        {
            this.Select(TargetArgs.Null);
            this.MultipleSelected = this.Filter(targets).Where(t => t.Exists);
            this.MultipleSelectedCount = targets.Count();
            if (this.MultipleSelectedCount == 0)
                return;
            if (this.MultipleSelectedCount == 1)
            {
                this.Select(targets.First());
                return;
            }

            this.LabelName.TextFunc = () => string.Format("Multiple x{0}", this.MultipleSelectedCount);

            this.CreateButtons(targets);
            this.PanelInfo.RemoveControls(this.BoxIcons);
            this.Show();
        }
        private void Select(TargetArgs target)
        {
            if (this.SelectedSource.IsEqual(target))
            {
                CycleTargets();
                return;
            }
            this.SelectedSource = target;
            this.SelectedStack = null;
            this.WindowInfo = null;
            this.Clear();
            switch (target.Type)
            {
                case TargetType.Entity:
                    var entity = target.Object;
                    this.LabelName.TextFunc = () => entity.GetName();
                    this.MultipleSelectedCount = 1;
                    this.MultipleSelected = new TargetArgs[1] { target };
                    entity.GetSelectionInfo(this);
                    entity.GetQuickButtons(this);
                    this.InitInfoTabs(entity.GetInfoTabs());
                    entity.Town.Select(target, this);
                    this.InitInfoTabs(entity.Town.GetInfoTabs(target));
                    break;

                case TargetType.Position:
                    this.MultipleSelectedCount = 1;
                    this.MultipleSelected = new TargetArgs[1] { target };
                    var selectables = target.Map.Town.QuerySelectables(target);
                    if (selectables.Any())
                    {
                        this.SelectedStack = selectables.GetEnumerator();
                        CycleTargets();
                        if (target.Map.IsHidden(target.Global))
                        {
                            this.LabelName.TextFunc = () => "Unknown block";
                        }
                    }
                    break;

                case TargetType.Null:
                    this.Hide();
                    this.LabelName.TextFunc = () => "<none>";
                    if (this.WindowInfo != null)
                        this.WindowInfo.Hide();
                    this.WindowInfo = null;
                    this.SelectedSource = TargetArgs.Null;
                    this.Selectable = null;
                    this.MultipleSelectedCount = 0;
                    this.MultipleSelected = new TargetArgs[] { };
                    return;

                default:
                    break;
            }
            this.SelectedSource = target;
            this.Show();
            GameModes.GameMode.Current.OnUIEvent(UIManager.Events.SelectedChanged, this.SelectedSource);
            if (target.Type != TargetType.Null)
                target.Network.EventOccured(Components.Message.Types.SelectedChanged, target);
            this.WindowManager.OnSelectedTargetChanged(target);
            this.Validate(true);
        }
        public override bool Show()
        {
            this.Location = Instance.BottomCenterScreen;
            Instance.BoxButtons.AlignLeftToRight();
            this.BoxButtons.Show();
            return base.Show();
        }
        public override bool Hide()
        {
            this.BoxButtons.Hide();
            return base.Hide();
        }
        private void Clear()
        {
            foreach (var a in this.ActionsAdded)
                a.Value.Clear();
            this.ActionsAdded.Clear();
            this.BoxTabs.ClearControls();
            this.BoxButtons.ClearControls();
            this.BoxInfo.ClearControls();
            this.PanelInfo.ClearControls();
            this.PopulateBoxIcons();
            this.PanelInfo.AddControls(
                this.LabelName,
                this.BoxInfo,
                this.BoxIcons);
        }

        private void CycleTargets()
        {
            if (this.SelectedStack == null)
                return;
            this.SelectedStack.MoveNext();
            var first = this.SelectedStack.Current;
            this.SetName(first.GetName());
            Clear();

            first.GetSelectionInfo(this);
            first.GetQuickButtons(this);
            this.InitInfoTabs(first.GetInfoTabs());
            Net.Client.Instance.Map.Town.Select(first, this);
            this.Selectable = first;
        }
        void InitInfoTabs(IEnumerable<(string name, Action action)> tabs)
        {
            foreach (var (name, action) in tabs)
                this.AddTabAction(name, action, Color.Orange);
        }
        internal static bool IsSelected(ISelectable item)
        {
            if (Instance.SelectedStack == null)
                return false;
            return Instance.SelectedStack.Current == item;
        }
        Dictionary<Action<List<TargetArgs>>, List<TargetArgs>> ActionsAdded = new Dictionary<Action<List<TargetArgs>>, List<TargetArgs>>();

        private void CreateButtons(IEnumerable<TargetArgs> targets)
        {
            this.BoxButtons.ClearControls();
            this.ActionsAdded.Clear();
            foreach (var tar in targets)
                tar.GetQuickButtons(this);
            Net.Client.Instance.Map.Town.Select(null, this);

        }
        void AddTabAction(string label, Action action, Color col)
        {
            this.BoxTabs.AddControlsLineWrap(new[] { new Button(label) { LeftClickAction = action, BackgroundColor = col * .5f } }, this.PanelInfo.Width);
        }
        public void AddTabAction(string label, Action action)
        {
            this.AddTabAction(label, action, Color.PaleVioletRed);
        }
        internal void AddTabs(params Button[] buttons)
        {
            this.BoxTabs.AddControls(buttons);
        }
        internal void AddButtons(params IconButton[] buttons)
        {
            this.BoxButtons.AddControls(buttons);
        }
        internal void AddButton(IconButton button, Action<List<TargetArgs>> action, GameObject obj, bool singleTargetOnly = false)
        {
            AddButton(button, action, new TargetArgs(obj), singleTargetOnly);
        }
        public void AddButton(IconButton button, Action<TargetArgs> action, TargetArgs target)
        {
            this.AddButton(button, targets => action(targets.First()), target, true);
        }
        internal void AddButton(IconButton button, Action<List<TargetArgs>> action, TargetArgs obj, bool singleTargetOnly = false)
        {
            if (singleTargetOnly && this.MultipleSelectedCount > 1)
                return;

            if (this.ActionsAdded.TryGetValue(action, out List<TargetArgs> existing))
            {
                existing.Add(obj);
                return;
            }
            else
                this.ActionsAdded.Add(action, new List<TargetArgs>() { obj });
            button.LeftClickAction = () => this.MultipleSelectedAction(action);
                this.BoxButtons.AddControls(button);
        }
    
        internal static void AddButton(IconButton button)
        {
            Instance.AddButtons(new IconButton[] { button });
        }
        private void MultipleSelectedAction(Action<List<TargetArgs>> action)
        {
            action(this.ActionsAdded[action]);
        }

        public override void Update()
        {
            base.Update();
            if (this.SelectedSource is not null && this.SelectedSource.Type == TargetType.Entity && this.SelectedSource.Object.IsDisposed)
                this.Select(TargetArgs.Null);
    
            if (this.Selectable is null)
            {
                if (!this.MultipleSelected.Any())
                    if (this.IsOpen)
                        this.Hide();
                return;
            }

            if (!this.Selectable.Exists)
                this.Select(TargetArgs.Null);
           
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.BlockChanged:
                    var map = e.Parameters[0] as MapBase;
                    var global = (Vector3)e.Parameters[1];
                    var target = new TargetArgs(map, global);
                    if (IsSelected(map, global))
                    ClearTargets();
                    break;

                case Components.Message.Types.BlocksChanged:
                    map = e.Parameters[0] as MapBase;
                    var globals = e.Parameters[1] as Vector3[];
                    var targets = globals.Select(g => new TargetArgs(map, g));
                    if (targets.Any(t => IsSelected(t)))
                    ClearTargets();
                    break;

                case Components.Message.Types.EntityDespawned:
                    // TODO: deselect entity on despawn?
                    break;

                default:
                    base.OnGameEvent(e);
                    break;
            }
        }
        bool IsSelected(MapBase map, Vector3 global)
        {
            return this.MultipleSelected.Any(t => (t.Type == TargetType.Position && t.Map == map && t.Global == global) || (this.SelectedSource?.Global == global));
        }
        public override void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            if (this.MultipleSelected.Any())
            {
                foreach (var obj in this.MultipleSelected)
                    if (obj.Type == TargetType.Position)
                    {
                        camera.DrawBlockMouseover(sb, obj.Map, obj.Global, Color.Yellow);
                        var map = obj.Map;
                        var global = obj.Global;
                        map.GetBlock(global).DrawSelected(sb, camera, map, global);
                    }
            }
            else if (this.SelectedSource != null)
            {
                if (this.SelectedSource.Type == TargetType.Position)
                {
                    camera.DrawBlockMouseover(sb, this.SelectedSource.Map, this.SelectedSource.Global, Color.Yellow);
                    var map = this.SelectedSource.Map;
                    var global = this.SelectedSource.Global;
                    map.GetBlock(global).DrawSelected(sb, camera, map, global);
                }
            }
        }
        public override void DrawOnCamera(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            if (this.MultipleSelected.Any())
                foreach (var obj in this.MultipleSelected)
                    if (obj.Type == TargetType.Entity)
                        obj.Object.DrawBorder(sb, camera);
            else if (this.SelectedSource != null)
                if (this.SelectedSource.Type == TargetType.Entity)
                    this.SelectedSource.Object.DrawBorder(sb, camera);
        }
        
        static public bool IsSelected(TargetArgs tar)
        {
            return Instance.MultipleSelected.Any(t => t.IsEqual(tar)) || Instance.SelectedSource.IsEqual(tar);
        }
        internal static bool ClearTargets()
        {
            if (!Instance.MultipleSelected.Any() && Instance.SelectedSource.Type == TargetType.Null)
                return false;
            Instance.Select(TargetArgs.Null);
            return true;
        }

        static void Slice()
        {
            var next =  (int)Instance.SelectedSource.Global.Z;
            var current = ScreenManager.CurrentScreen.Camera.DrawLevel;
            if (next != current)
            {
                Instance.PreviousDrawLevel = current;
                ScreenManager.CurrentScreen.Camera.DrawLevel = next;
            }
            else if (Instance.PreviousDrawLevel != -1)
                ScreenManager.CurrentScreen.Camera.DrawLevel = Instance.PreviousDrawLevel;
        }
        public void AddIcon(IconButton icon)
        {
            if (this.MultipleSelectedCount > 1)
                return;

            this.BoxIcons.Controls.Insert(0, icon);
            this.RepositionsBoxIcons();
        }

        public void AddInfo(Control ctrl)
        {
            this.BoxInfo.AddControls(ctrl);
            this.BoxInfo.AlignVertically();
        }

        static public void RemoveButton(IconButton button)
        {
            Instance.BoxButtons.RemoveControls(button);
        }
        internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, IEnumerable<GameObject> targets)
        {
            AddButton(button, action, targets.Select(t => new TargetArgs(t)));
        }
        internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, IEnumerable<TargetArgs> targets)
        {
            List<TargetArgs> existing;
            if (Instance.ActionsAdded.TryGetValue(action, out existing))
            {
                Instance.ActionsAdded.Remove(action);
            }
            Instance.ActionsAdded.Add(action, targets.ToList());
            if (!Instance.BoxButtons.Controls.Contains(button))
                Instance.BoxButtons.AddControls(button);
            button.LeftClickAction = () => Instance.MultipleSelectedAction(action);
        }
        internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, TargetArgs target)
        {
            if (Instance.ActionsAdded.TryGetValue(action, out List<TargetArgs> existing))
            {
                existing.Add(target);
            }
            else
            {
                Instance.ActionsAdded.Add(action, new List<TargetArgs>() { target });
                Instance.BoxButtons.AddControls(button);
                button.LeftClickAction = () => Instance.MultipleSelectedAction(action);
            }
        }

        internal static IEnumerable<TargetArgs> GetSelected()
        {
            return Instance.MultipleSelected;
        }

        internal static IEnumerable<GameObject> GetSelectedEntities()
        {
            return GetSelected()
                .Where(tar => tar.Type == TargetType.Entity)
                .Select(t => t.Object);
        }
        internal static IEnumerable<IntVec3> GetSelectedPositions()
        {
            return GetSelected()
                .Where(tar => tar.Type == TargetType.Position)
                .Select(t => (IntVec3)t.Global);
        }
        internal static Entity GetSingleSelectedEntity()
        {
            if (GetSelected().Count() != 1)
                return null;

            return GetSelected().First().Object as Entity;
        }
    }
}
