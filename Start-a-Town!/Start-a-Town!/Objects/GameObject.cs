using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.AI;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class GameObject : IEntity, ITooltippable, IContextable, INameplateable, IDebuggable, ISlottable, ISelectable, ILabeled
    {
        public static Dictionary<int, GameObject> Templates = new();
        public string Label => this.Def.Label;
        static int GetNextTemplateID()
        {
            return Templates.Count + 1;
        }

        public static void AddTemplates(IEnumerable<GameObject> templates)
        {
            foreach (var o in templates)
                AddTemplate(o);
        }
        public static int AddTemplate(GameObject obj)
        {
            var id = GetNextTemplateID();
            Templates.Add(id, obj);
            return id;
        }
        internal static GameObject CloneTemplate(int templateID)
        {
            return Templates[templateID].Clone();
        }

        public string GetName()
        { return this.Name; }
        public Color GetSlotColor()
        { return this.GetInfo().GetQualityColor(); }
        public string GetCornerText()
        { return this.StackSize.ToString(); }
        public void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            var sprite = this.GetSpriteOrDefault();
            var source = sprite.GetSourceRect();
            sprite.Draw(sb, pos - new Vector2(source.Width, source.Height) * 0.5f, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        internal bool HasResource(ResourceDef type)
        {
            return this.GetResource(type) != null;
        }
        internal AttributeStat GetAttribute(AttributeDef att) => this.GetComponent<AttributesComponent>().GetAttribute(att);

        [Obsolete]
        public GameObject Debug() { return this; }


        private DefComponent _DefComponent;
        public DefComponent DefComponent => this._DefComponent ??= this.GetComponent<DefComponent>();
        public ItemDef Def;

        public Quality Quality { get { return this.DefComponent.Quality; } set { this.DefComponent.Quality = value; } }


        public GameObjectSlot ToSlotLink(int amount = 1)
        {
            return new GameObjectSlot() { Link = this };
        }
        public Memory ToMemory(GameObject actor)
        {
            return new Memory(this, 100, 100, 1, actor);
        }

        public static void LoadObjects()
        {
            PlantProperties.Init();
            PlantDefOf.Init();
            AddTemplate(ItemFactory.CreateItem(ApparelDefOf.Helmet));
            AddTemplate(Actor.Create(ActorDefOf.Npc).SetName("Npc"));
        }

        #region Common Properties
        public virtual string Name
        {
            get => $"{this.GetInfo().Name}{(this.StackSize > 1 ? $" (x{this.StackSize})" : "")}";
            
            set
            {
                var info = GetInfo();
                info.Name = value;
            }
        }

        public string Description => this.Def.Description; 
        public virtual float Height => this.Physics.Height;

        public int RefID;

        public INetwork Net;

        MapBase _map;
        public MapBase Map { get { return this.Parent?.Map ?? this._map; } set { this._map = value; } }

        public Town Town => this.Net.Map.Town;

        internal object GetPath()
        {
            return this.GetState().Path;
        }
       
        public IEnumerable<Button> GetTabs()
        {
            foreach (var comp in this.Components.Values)
                foreach (var i in comp.GetTabs())
                    yield return i;
            foreach (var i in this.GetInfoTabsExtraNew())
                yield return i;
        }
        protected virtual IEnumerable<(string name, Action action)> GetInfoTabsExtra() { yield break; }
        protected virtual IEnumerable<Button> GetInfoTabsExtraNew() { yield break; }

        public virtual void GetSelectionInfo(IUISelection info)
        {
            info.AddIcon(IconCameraFollow);
            this.Map.World.OnTargetSelected(info, this);
            foreach (var comp in this.Components.Values)
                comp.GetSelectionInfo(info, this);
        }
        public virtual void GetQuickButtons(UISelectedInfo info)
        {
            if (this.IsForbiddable())
            {
                info.AddButton(IconForbidden, RequestToggleForbidden, this);
            }

            foreach (var comp in this.Components.Values)
            {
                comp.GetQuickButtons(info, this);
            }
        }
        static readonly IconButton IconForbidden = new QuickButton(Icon.Cross, KeyBind.ToggleForbidden, "Forbid") { HoverText = "Toggle forbidden" };
        static readonly IconButton IconCameraFollow = new(Icon.Replace) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = FollowCam, HoverText = "Camera follow" };

        static void RequestToggleForbidden(List<TargetArgs> obj)
        {
            PacketToggleForbidden.Send(obj.First().Network, obj.Select(o => o.Object.RefID).ToList());
        }
        static void FollowCam()
        {
            ScreenManager.CurrentScreen.Camera.ToggleFollowing(UISelectedInfo.Instance.SelectedSource.Object);
        }

        public void ToggleForbidden()
        {
            this.IsForbidden = !this.IsForbidden;
        }
        public void DrawNameplate(SpriteBatch sb, Rectangle viewport, Nameplate plate)
        {
            plate.Draw(sb, viewport);
        }
        public void OnNameplateCreated(Nameplate plate)
        {
            foreach (var comp in Components.Values)
            {
                comp.OnNameplateCreated(this, plate);
            }
        }

        public Rectangle GetScreenBounds(Camera camera)
        {
            var g = this.Global;
            var bounds = camera.GetScreenBounds(g.X, g.Y, g.Z, this.SpriteComp.GetSpriteBounds(), 0, 0, this.Body.Scale);
            return bounds;
        }

        public virtual Color GetNameplateColor()
        {
            return this.DefComponent.Quality.Color;
        }

        public GameObject Parent
        {
            get => this.Transform.ParentEntity;
            set => this.Transform.ParentEntity = value;
        }
        public Vector3 Global
        {
            get => this.Transform.Global;
            set => this.Transform.Global = value;
        }
        public IntVec3? Cell { get { return this.IsSpawned ? this.Transform.Global.SnapToBlock() : null; } }
        public Vector3 GridCell
        {
            get { return this.Global.SnapToBlock(); }
            set { this.SetPosition(value); }
        }
        public Vector3 GridCellOffset
        {
            get
            {
                var grid = this.Global.SnapToBlock();
                return this.Global - grid;
            }
            set
            {
                this.SetPosition(this.GridCell + value);
            }
        }

        public float Acceleration
        {
            get => this.GetComponent<MobileComponent>().Acceleration;
            set => this.GetComponent<MobileComponent>().Acceleration = value;

        }
        public Vector3 Velocity
        {
            get => this.Transform.Velocity;
            set
            {
                if (float.IsNaN(value.X) || float.IsNaN(value.Y))
                {
                    throw new Exception();
                }

                this.Transform.Velocity = value;
                if (value != Vector3.Zero)
                {
                    PhysicsComponent.Enable(this);
                }
            }
        }
        public Vector3 Direction
        {
            get => new(this.Transform.Direction, 0);
            set
            {
                if (float.IsNaN(value.X) || float.IsNaN(value.Y))
                {
                    throw new Exception();
                }

                var transform = this.Transform;
                var newdir = new Vector2(value.X, value.Y);
                transform.Direction = newdir;
            }
        }
        public int StackMax => this.Def.StackCapacity;

        int _StackSize = 1;
        public int StackSize
        {
            get { return this._StackSize; }
            set
            {
                var oldSize = this.StackSize;
                var newSize = Math.Min(value, this.StackMax);
                if (oldSize == newSize)
                {
                    return;
                }

                value = newSize;
                this._StackSize = value;
                if (value == 0)
                {
                    if (this.IsSpawned)
                        this.Despawn();

                    if (this.Slot != null)
                        this.Slot.Clear();

                    this.Dispose();
                }
                else if (value < 0)
                    throw new Exception();
            }
        }

        public bool IsStackFull => this.StackSize == this.StackMax; 
        public Bone Body => this.SpriteComp.Body;
        internal Material PrimaryMaterial => this.Body.Material;

        public GameObject SetGlobal(Vector3 global)
        {
            this.SetPosition(global);
            return this;
        }

        /// <summary>
        /// Changes the objects global position, removing the object from the previous chunk's object list and adding it to the new one's accordingly.
        /// </summary>
        /// <param name="nextGlobal"></param>
        /// <returns></returns>
        public GameObject SetPosition(Vector3 nextGlobal) // TODO: merge this with SetGlobal
        {
            if (this.Map.IsSolid(nextGlobal))// + Vector3.UnitZ * 0.01f))// TODO: FIX THIS
                return this; // TODO: FIX: problem when desynced from server, block might be empty on server but solid on client

            this.Map.TryGetChunk(nextGlobal.RoundXY(), out var nextChunk);

            if (nextChunk is null)
                return this;

            this.Map.TryGetChunk(this.Global.Round(), out var lastChunk);
            this.Global = nextGlobal;

            if (nextChunk != lastChunk)
            {
                bool removed = lastChunk.Remove(this);
                if (!removed)
                    throw new Exception("Source chunk is't loaded"); //Could not remove object from previous chunk");

                nextChunk.Add(this);
            }

            this.Physics.Enabled = true;
            return this;
        }
        public bool IsForbidden;
        public bool IsSpawned => this._map is not null;
        public bool IsReserved { get { return this.Town.ReservationManager.IsReserved(this); } }
        public bool IsPlayerControlled { get { return this.Net.GetPlayers().Any(p => p.ControllingEntity == this); } }
        public virtual bool IsHaulable
        {
            get { return this.Def.Haulable; }
        }
        internal bool IsFuel
        {
            get { return this.Material?.Fuel.Value > 0; }
        }

        public GameObjectSlot Slot;
        #endregion

        public GameObject Clone()
        {
            var obj = this.Def.CreateRandom();
            if (obj is null)
            {
                obj = this.Create(); //for derived classes
                obj.Def = this.Def;
                foreach (var comp in this.Components)
                {
                    obj.AddComponent(comp.Value.Clone() as EntityComponent);
                }
            }
            obj.ObjectCreated();
            return obj;
        }

        public virtual GameObject Create()
        {
            return new GameObject();
        }

        public GameObject TrySplitOne()
        {
            throw new NotImplementedException(); // TODO sync instantiate new object
        }
        public GameObject Split(int amount)
        {
            if (amount >= this.StackSize)
            {
                throw new Exception();
            }

            return this.SetStackSize(this.StackSize - amount).Clone().SetStackSize(amount);
        }
        public GameObject SetStackSize(int value)
        {
            this.StackSize = value;
            return this;
        }
        
        #region Messaging
        public void PostMessage(ObjectEventArgs a)
        {
            GameObject.PostMessage(this, a);
        }
        public static void PostMessage(Message msg)
        {
            msg.Receiver.HandleMessage(msg.Args);
            if (msg.Callback != null)
            {
                msg.Callback(msg.Receiver);
            }
        }
        public static void PostMessage(GameObject receiver, ObjectEventArgs e)
        {
            var msg = new Message(receiver, e);
            PostMessage(msg);
        }

        public void HandleRemoteCall(ObjectEventArgs e)
        {
            foreach (var comp in this.Components.Values)
            {
                comp.HandleRemoteCall(this, e);
            }
        }

        bool HandleMessage(ObjectEventArgs e)
        {
            bool ok = false;
            foreach (var comp in this.Components.Values)
            {
                ok |= comp.HandleMessage(this, e);
            }

            return ok;
        }
        #endregion
        public IEnumerable<GameObject> GetNearbyObjects(Func<float, bool> range, Func<GameObject, bool> filter = null)
        {
            return this.Map.GetNearbyObjectsNew(this.Global, range, filter).Except(new GameObject[] { this });
        }

        public PositionComponent Transform;

        public GameObject()
        {
            this.Transform = this.AddComponent<PositionComponent>();
        }

        public EntityComponent this[string componentName]
        {
            get { return this.Components[componentName]; }
            set
            {
                Components[value.ComponentName] = value;
                value.MakeChildOf(this);
            }
        }

        DefComponent _Info;
        public DefComponent GetInfo()
        {
            return this._Info ??= GetComponent<DefComponent>("Info");
        }
       
        PhysicsComponent _PhysicsCached;
        public PhysicsComponent Physics => this._PhysicsCached ??= this.GetComponent<PhysicsComponent>();

        
        SpriteComponent _SpriteCompCached;
        public SpriteComponent SpriteComp => this._SpriteCompCached ??= this.GetComponent<SpriteComponent>();

        PersonalInventoryComponent _InventoryCached;
        public PersonalInventoryComponent Inventory => this._InventoryCached ??= this.GetComponent<PersonalInventoryComponent>();

        public ComponentCollection Components = new();

        public T GetComponent<T>(string name) where T : EntityComponent
        {
            return this.GetComponent<T>();
        }

        public T GetComponent<T>() where T : EntityComponent
        {
            return (from comp in Components.Values
                    where comp is T
                    select comp).SingleOrDefault() as T;
        }

        public bool HasComponent<T>() where T : EntityComponent
        {
            return this.GetComponent<T>() != null;
        }
        public bool HasComponent(Type compType)
        {
            return this.Components.Values.Any(c => c.GetType() == compType);
        }
        public bool TryGetComponent(Type compType, out EntityComponent comp)
        {
            comp = this.Components.Values.FirstOrDefault(c => c.GetType() == compType);
            return comp != null;
        }
        public bool TryGetComponent<T>(string name, out T component) where T : EntityComponent
        {
            return this.TryGetComponent(out component);
        }
        public bool TryGetComponent<T>(out T component) where T : EntityComponent
        {
            component = this.GetComponent<T>();
            return component != null;
        }
        public bool TryGetComponent<T>(Action<T> action) where T : EntityComponent
        {
            T component = this.GetComponent<T>();
            if (component is null)
            {
                return false;
            }

            action(component);
            return true;
        }

        public EntityComponent AddComponent(EntityComponent component)
        {
            this.Components[component.ComponentName] = component;
            component.Parent = this;
            component.MakeChildOf(this);
            return component;
        }

        public T AddComponent<T>() where T : EntityComponent, new()
        {
            T component = new();
            this.Components[component.ComponentName] = component;
            component.MakeChildOf(this);
            return component;
        }

        public virtual void Update()
        {
            this.Components.Update();
        }

        public override string ToString()
        {
            if (!GlobalVars.DebugMode)
                return Name;

            string info = "";
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
            {
                if (info.Length > 0)
                    info += "\n";
                info += "*" + comp.Key + "\n" + comp.Value.ToString();
            }
            if (info.Length > 0)
                info = info.Remove(info.Length - 1);

            return info;
        }

        #region Children
        byte _ChildrenSequence = 0;
        public byte ChildrenSequence
        {
            get => _ChildrenSequence++;
            private set => _ChildrenSequence = value; 
        }
        byte _ContainerSequence = 0;
        byte ContainerSequence
        {
            get => _ContainerSequence++;
            set => _ContainerSequence = value; 
        }
        public List<GameObjectSlot> GetChildren()
        {
            var list = new List<GameObjectSlot>();
            foreach (var c in this.GetContainers())
                foreach (var s in c.Slots)
                    list.Add(s);
            return list;
        }
        public List<Container> GetContainers()
        {
            var list = new List<Container>();
            foreach (var comp in this.Components.Values)
                comp.GetContainers(list);
            return list;
        }
        public Container GetContainer(int id)
        {
            return this.GetContainers().FirstOrDefault(c => c.ID == id);
        }
        public GameObjectSlot GetChild(int containerID, int slotID)
        {
            var c = this.GetContainer(containerID);
            if (c is null)
            {
                return null;
            }

            return c.Slots.FirstOrDefault(s => s.ID == slotID);
        }
        public void RegisterContainer(Container container)
        {
            container.ID = this.ContainerSequence;
            container.Parent = this;
        }
        public GameObjectSlot GetChild(byte childIndex)
        {
            return this.GetChild((int)childIndex);
        }
        public GameObjectSlot GetChild(int childIndex)
        {
            return this.GetChildren().FirstOrDefault(s => s.ID == childIndex);
        }
        #endregion

        public GameObject EnumerateChildren()
        {
            this.ChildrenSequence = 0;
            var list = new List<GameObjectSlot>();
            foreach (var comp in this.Components.Values)
            {
                comp.GetChildren(list);
            }

            foreach (var child in list)
            {
                child.ID = this.ChildrenSequence;
            }
            this.ChildrenSequence = 0;
            return this;
        }
        public void GetManagementInterface(WindowTargetManagement ui)
        {
            ui.Title = this.Name;
            foreach (var comp in this.Components)
            {
                comp.Value.GetManagementInterface(this, ui.PanelInfo);
            }

            ui.PanelActions.AddControls(
                new Button("Center Cam") { LeftClickAction = () => { ScreenManager.CurrentScreen.Camera.CenterOn(this.Global); } }
                );
        }
        public void GetManagementInterface(WindowTargetManagementStatic ui)
        {
            ui.Title = this.Name;
            foreach (var comp in this.Components)
            {
                comp.Value.GetManagementInterface(this, ui.PanelInfo);
            }

            ui.PanelActions.AddControls(
                new Button("Center Cam") { LeftClickAction = () => { ScreenManager.CurrentScreen.Camera.CenterOn(this.Global); } }
                );
        }

        public Window GetUi()
        {
            throw new Exception();
        }

        public void GetTooltip(UI.Control tooltip)//Message msg)
        {
            GetInfo().OnTooltipCreated(this, tooltip);
            // TODO: LOL fix, i need the object name to be on top
            foreach (var comp in Components.Except(new KeyValuePair<string, EntityComponent>[] { new KeyValuePair<string, EntityComponent>("Info", GetInfo()) }))
            {
                comp.Value.OnTooltipCreated(this, tooltip);
            }

            var value = this.GetValue();
            if (value > 0)
            {
                tooltip.AddControlsBottomLeft(new Label(string.Format("Value: {0} ({1})", value * this.StackSize, value)));
            }

            var stats = this.Def.Category?.Stats;
            if (stats is not null)
            {
                foreach (var stat in stats)
                {
                    tooltip.AddControlsBottomLeft(new Label($"{stat.Label}: {stat.GetValue(this).ToString(stat.StringFormat)}"));
                }
            }

            tooltip.AddControlsBottomLeft(new Label(string.Format("InstanceID: {0}", this.RefID)));
        }
        public void GetInventoryTooltip(UI.Control tooltip)
        {
            GetInfo().OnTooltipCreated(this, tooltip);
            // TODO: LOL fix, i need the object name to be on top
            foreach (KeyValuePair<string, EntityComponent> comp in Components.Except(new KeyValuePair<string, EntityComponent>[] { new KeyValuePair<string, EntityComponent>("Info", GetInfo()) }))
            {
                comp.Value.GetInventoryTooltip(this, tooltip);
            }

            var value = this.GetValue();
            if (value > 0)
            {
                tooltip.AddControlsBottomLeft(new Label(string.Format("Value: {0} ({1})", value * this.StackSize, value)));
            }

            tooltip.AddControlsBottomLeft(new Label(string.Format("InstanceID: {0}", this.RefID)));
        }

        public void Despawn()
        {
            if (!this.IsSpawned)
                return;
            var oldmap = this.Map;
            if (!this.Map.Remove(this))
                throw new Exception();
            this._map = null;
            foreach (var comp in this.Components.Values.ToList())
                comp.OnDespawn();

            oldmap.EventOccured(Message.Types.EntityDespawned, this);
            //this.Unreserve(); // UNDONE dont unreserve here because the ai might continue manipulating (placing/carrying) the item during the same behavior
        }

        protected virtual void OnSpawn(MapBase map)
        {
            this.Net = map.Net;
            this.Parent = null;

            if (!map.TryGetChunk(this.Global, out var chunk))
                throw new Exception("Chunk not loaded");

            map.Add(this);
            foreach (var comp in this.Components.Values)
                comp.OnSpawn();

            this.Map.EventOccured(Message.Types.EntitySpawned, this);
        }
        public void Spawn(MapBase map, Vector3 global)
        {
            this.Global = global;
            this.Spawn(map);
        }
        public void Spawn(MapBase map)
        {
            var net = map.Net;
            this.Net = net;
            this.Map = map;
            this.OnSpawn(map);
        }
        public void SyncSpawnNew(MapBase map)
        {
            if (this.RefID != 0)
                this.Spawn(map);
            if (map.Net is not Server)
                return;
            SyncInstantiate(map.Net);
            map.SyncSpawn(this, this.Global, this.Velocity);
        }

        public void SyncSpawn(MapBase map, Vector3 global)
        {
            if (map.Net is not Server)
                return;

            map.SyncSpawn(this, global, Vector3.Zero);
        }

        public void Focus()
        {
            foreach (var comp in this.Components.Values)
                comp.Focus(this);
        }
        public void FocusLost()
        {
            foreach (var comp in this.Components.Values)
                comp.FocusLost(this);
        }

        public virtual void Draw(MySpriteBatch sb, Camera camera)
        {
            foreach (var comp in this.Components.Values)
                comp.Draw(sb, this, camera);
        }
        public virtual void Draw(MySpriteBatch sb, DrawObjectArgs e)
        {
            foreach (var comp in this.Components.Values)
                comp.Draw(sb, e);
        }

        internal void DrawMouseover(MySpriteBatch sb, Camera camera)
        {
            foreach (var comp in this.Components.Values)
                comp.DrawMouseover(sb, camera, this);
        }
        internal void DrawInterface(SpriteBatch sb, Camera camera)
        {
            foreach (var comp in this.Components.Values)
                comp.DrawUI(sb, camera, this);
        }

        public void DrawPreview(MySpriteBatch sb, Camera cam, TargetArgs target, bool precise)
        {
            if (target.Type != TargetType.Position)
                return;

            var blockHeight = Block.GetBlockHeight(target.Map, target.Global);
            var global = target.Global + target.Face * new Vector3(1, 1, blockHeight) + (precise ? target.Precise : Vector3.Zero);
            this.DrawPreview(sb, cam, global);
        }

        public void DrawPreview(MySpriteBatch sb, Camera cam, Vector3 global)
        {
            var body = this.Body;
            var pos = cam.GetScreenPositionFloat(global);
            pos += body.OriginGroundOffset * cam.Zoom;
            // TODO: fix difference between tint and material in this drawtree method
            var tint = Color.White * .5f;
            body.DrawGhost(this, sb, pos, Color.White, Color.White, tint, Color.Transparent, 0, cam.Zoom, 0, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
        }

        public virtual void GetTooltipInfo(Control tooltip)
        {
            GetTooltip(tooltip);
        }
        Sprite CachedSprite;

        public Sprite GetSprite()
        {
            return this.CachedSprite ??= this.SpriteComp?.Sprite;
        }
        public Sprite GetSpriteOrDefault()
        {
            return this.GetSprite() ?? Sprite.Default;
        }
        public Icon GetIcon()
        {
            return new Icon(GetSpriteOrDefault());
        }

        public byte[] GetSnapshotData()
        {
            using var w = new BinaryWriter(new MemoryStream());
            this.Write(w);
            return (w.BaseStream as MemoryStream).ToArray();
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Def.Name);
            w.Write(this.RefID);
            w.Write(this.StackSize);
            w.Write(this.Components.Count);
            foreach (var comp in this.Components)
            {
                w.Write(comp.Key);
                comp.Value.Write(w);
            }
        }

        public static GameObject CreatePrefab(BinaryReader r)
        {
            string defName = r.ReadString();
            var def = Start_a_Town_.Def.GetDef<ItemDef>(defName);
            var obj = def.Create();
            obj.RefID = r.ReadInt32();
            obj.StackSize = r.ReadInt32();
            int compCount = r.ReadInt32();
            for (int i = 0; i < compCount; i++)
            {
                string compName = r.ReadString();
                obj[compName].Read(r);
            }
            obj.ObjectSynced();
            return obj;
        }
        public static GameObject CloneTemplate(int templateID, BinaryReader reader)
        {
            GameObject obj = CloneTemplate(templateID);
            _ = reader.ReadString(); // def name not necessary because we copy it from the existing cloned object
            obj.RefID = reader.ReadInt32();
            obj.StackSize = reader.ReadInt32();
            int compCount = reader.ReadInt32();
            for (int i = 0; i < compCount; i++)
            {
                string compName = reader.ReadString();
                obj[compName].Read(reader);
            }
            obj.ObjectSynced();
            return obj;
        }
        public GameObject ObjectLoaded()
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
            {
                comp.Value.OnObjectLoaded(this);
            }

            return this;
        }
        /// <summary>
        /// try to make this private
        /// </summary>
        /// <returns></returns>
        public GameObject ObjectCreated()
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
            {
                comp.Value.OnObjectCreated(this);
            }

            return this;
        }
        public GameObject ObjectSynced()
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
            {
                comp.Value.OnObjectSynced(this);
            }

            this.EnumerateChildren();
            return this;
        }
        public GameObject Initialize(RandomThreaded random)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
            {
                comp.Value.Initialize(this, random);
            }

            return this;
        }
        public SaveTag Save(string name = "")
        {
            return new SaveTag(SaveTag.Types.Compound, name, this.SaveInternal());
        }
        internal List<SaveTag> SaveInternal()
        {
            var data = new List<SaveTag>();
            data.Add(this.Def.Name.Save("Def"));
            data.Add(this.RefID.Save("InstanceID"));
            data.Add(this._StackSize.Save("Stack"));
            var compData = new SaveTag(SaveTag.Types.Compound, "Components");
            foreach (KeyValuePair<string, EntityComponent> comp in this.Components)
            {
                var compSave = comp.Value.SaveAs(comp.Key);
                if (compSave is null)
                {
                    compData.Add(compSave);
                }
            }
            data.Add(compData);
            return data;
        }

        /// <summary>
        /// Creates an object from a savetag node.
        /// </summary>
        /// <param name="tag">A tag with a list of tags as its value.</param>
        /// <returns></returns>
        internal static GameObject Load(SaveTag tag)
        {
            tag.TryGetTagValue("Def", out string defName);
            var def = Start_a_Town_.Def.GetDef<ItemDef>(defName);
            var obj = def.Create();
            tag.TryGetTagValue("InstanceID", out obj.RefID);
            tag.TryGetTagValue<int>("Stack", v=> obj._StackSize = v);
            Dictionary<string, SaveTag> compData = tag["Components"].Value as Dictionary<string, SaveTag>;
            foreach (SaveTag compTag in compData.Values)
            {
                if (compTag.Value == null)
                {
                    continue;
                }

                if (obj.Components.ContainsKey(compTag.Name))
                {
                    obj[compTag.Name].Load(obj, compTag);
                }
            }
            obj.ObjectLoaded();
            return obj;
        }

        public IEnumerable<ContextAction> GetInventoryContextActions(GameObject actor)
        {
            if (this.Def.GearType != null)
            {
                yield return new ContextAction(() => "Equip", () => PacketInventoryEquip.Send(this.Net, actor.RefID, this.RefID));
            }

            yield return new ContextAction(() => "Drop", () => PacketInventoryDrop.Send(this.Net, actor.RefID, this.RefID, this.StackSize));
        }

        public Dictionary<PlayerInput, Interaction> GetPlayerActionsWorld()
        {
            var list = new Dictionary<PlayerInput, Interaction>();
            foreach (var item in this.Components)
            {
                item.Value.GetPlayerActionsWorld(this, list);
            }

            return list;
        }

        public List<ContextAction> GetRightClickActions()
        {
            var list = new List<ContextAction>();
            foreach (var item in this.Components)
            {
                item.Value.GetRightClickActions(this, list);
            }

            return list;
        }

        public List<Interaction> GetHauledActions(TargetArgs a)
        {
            var list = new List<Interaction>();
            foreach (var item in this.Components)
            {
                item.Value.GetHauledActions(this, a, list);
            }

            return list;
        }

        internal ContextAction GetContextRB(GameObject player)
        {
            var list = new List<ContextAction>();
            foreach (var c in this.Components)
            {
                var a = c.Value.GetContextRB(this, player);
                if (a is null)
                {
                    list.Add(a);
                }
            }
            return list.FirstOrDefault();
        }
        internal ContextAction GetContextActivate(GameObject player)
        {
            var list = new List<ContextAction>();
            foreach (var c in this.Components)
            {
                var a = c.Value.GetContextActivate(this, player);
                if (a is null)
                {
                    list.Add(a);
                }
            }
            return list.FirstOrDefault();
        }
        public void GetContextActions(GameObject playerEntity, ContextArgs a)
        {
            if (playerEntity is null)
            {
                return;
            }

            foreach (var c in this.Components.Values)
            {
                c.GetClientActions(this, a.Actions);
            }
        }

        public List<Interaction> GetInteractionsList()
        {
            var list = new List<Interaction>();
            foreach (var item in this.Components)
            {
                item.Value.GetInteractions(this, list);
            }

            return list;
        }
        public Dictionary<string, Interaction> GetInteractions()
        {
            var list = new Dictionary<string, Interaction>();
            foreach (var item in this.GetInteractionsList())
            {
                list.Add(item.Name, item);
            }

            return list;
        }

        public List<Interaction> GetAvailableTasks()
        {
            var list = new List<Interaction>();
            foreach (var c in this.Components.Values)
                c.GetAvailableTasks(this, list);
            return list;
        }

        public bool IsDisposed => this.Net.GetNetworkObject(this.RefID) is null;

        public bool Dispose()
        {
            this.Net.EventOccured(Message.Types.ObjectDisposed, this);
            return this.Net.DisposeObject(this);
        }

        public GameObject Instantiate(Action<GameObject> instantiator)
        {
            instantiator(this);
            var children = this.GetChildren();
            (from slot in children
             where slot.HasValue
             select slot.Object).ToList()
             .ForEach(c => c.Instantiate(instantiator));

            foreach (var comp in this.Components.Values)
                comp.Instantiate(instantiator);

            return this;
        }

        internal void RemoteProcedureCall(Components.Message.Types type, BinaryReader r)
        {
            foreach (var comp in this.Components)
            {
                comp.Value.HandleRemoteCall(this, type, r);
            }
        }

        public bool IsInInteractionRange(TargetArgs target)
        {
            if (target.Type == TargetType.Position)
            {
                var actorCoords = this.Global;
                var actorBox = new BoundingBox(actorCoords - new Vector3(1, 1, 1), actorCoords + new Vector3(1, 1, this.Physics.Height + 2));
                var targetBox = new BoundingBox(target.Global - new Vector3(.5f, .5f, .5f), target.Global + new Vector3(1.5f, 1.5f, 1.5f));
                var result = actorBox.Intersects(targetBox);
                if (!result)
                {
                    throw new Exception();
                }
                return result;
            }
            else if (target.Type == TargetType.Entity)
            {
                var cylinderTarget = new BoundingCylinder(target.Global, .5f, target.Object.Physics.Height);
                var cylinderActor = new BoundingCylinder(this.Global - Vector3.UnitZ, Interaction.DefaultRange, this.Physics.Height + 2);
                var result = cylinderActor.Intersects(cylinderTarget);
                if (!result)
                {
                    throw new Exception();
                }
                return result;
            }
            throw new Exception();
        }

        internal void MapLoaded(MapBase map)
        {
            this.Map = map;
            foreach (var comp in this.Components)
            {
                comp.Value.MapLoaded(this);
            }
        }

        internal void HitTest(Camera camera)
        {
            this.SpriteComp.HitTest(this, camera);
        }

        public bool HasMatchingBody(GameObject otherItem)
        {
            return this.SpriteComp.HasMatchingBody(otherItem);
        }

        public int StackAvailableSpace { get { return this.StackMax - this.StackSize; } }
        public bool CanAbsorb(GameObject otherItem, int amount = -1)
        {
            if (this == otherItem)
            {
                return false;
            }

            if (this.IsStackFull)
            {
                return false;
            }

            if (otherItem.Def != null && this.Def != otherItem.Def)
            {
                return false;
            }

            if (!this.HasMatchingBody(otherItem))
            {
                return false;
            }

            if (amount == -1)
            {
                return true;
            }

            if (this.StackSize + amount > this.StackMax)
            {
                throw new Exception();
            }

            return true;
        }

        internal bool ProvidesSkill(ToolAbilityDef skill)
        {
            return ToolAbilityComponent.HasSkill(this, skill);
        }

        internal GameObjectSlot GetEquipmentSlot(GearType.Types type)
        {
            return GearComponent.GetSlot(this, GearType.Dictionary[type]);
        }

        internal GameObject GetHauled()
        {
            return PersonalInventoryComponent.GetHauling(this).Object;
        }

        internal GameObject Carried
        {
            get { return PersonalInventoryComponent.GetHauling(this).Object; }
        }
        internal GameObject ClearCarried()
        {
            var carried = PersonalInventoryComponent.GetHauling(this);
            var obj = carried.Object;
            carried.Clear();
            return obj;
        }
       
        internal void StoreCarried()
        {
            this.Inventory.StoreHauled();
        }
        internal List<GameObject> GetPossesions()
        {
            return NpcComponent.GetPossesions(this).Select(id => this.Net.GetNetworkObject(id)).ToList();
        }

        internal Need GetNeed(NeedDef def)
        {
            return this.GetComponent<NeedsComponent>().NeedsNew.First(n => n.NeedDef == def);
        }
        internal IEnumerable<Need> GetNeeds(NeedCategoryDef cat)
        {
            return this.GetComponent<NeedsComponent>().NeedsNew.Where(n => n.NeedDef.CategoryDef == cat);
        }
        public IEnumerable<Need> GetNeeds()
        {
            foreach (var n in this.GetComponent<NeedsComponent>().NeedsNew)
            {
                yield return n;
            }
        }
        internal Need GetNeed(string needName)
        {
            return this.GetComponent<NeedsComponent>().NeedsNew.First(n => n.NeedDef.Name == needName);
        }
     
        internal float GetToolWorkAmount(int skillID)
        {
            var tool = this.GetEquipmentSlot(GearType.Types.Mainhand).Object;
            if (tool is null)
            {
                return 1;
            }

            var ability = tool.Def.ToolProperties?.Ability;

            if (!ability.HasValue)
            {
                throw new Exception();
            }

            return ability.Value.Efficiency;
        }

        public bool CanReachNew(GameObject obj)
        {
            return this.Map.Regions.CanReach(this.GetCellStandingOn(), obj.Global.SnapToBlock(), this as Actor);
        }
        internal bool CanReachNew(Vector3 global)
        {
            return this.Map.Regions.CanReach(this.GetCellStandingOn(), global.SnapToBlock(), this as Actor);
        }
        internal bool CanReach(GameObject obj)
        {
            return this.Map.GetRegionDistance(this.GetCellStandingOn(), obj.Global.SnapToBlock(), this as Actor) != -1;
        }
        internal bool CanReach(Vector3 global)
        {
            return this.Map.GetRegionDistance(this.GetCellStandingOn(), global.SnapToBlock(), this as Actor) != -1;
        }

        internal BehaviorPerformTask GetLastBehavior()
        {
            return AIState.GetState(this).LastBehavior;
        }
        
        internal BoundingBox GetBoundingBox(Vector3 global)
        {
            return this.Physics.GetBoundingBox(global);
        }
        internal BoundingBox GetBoundingBox(Vector3 global, float height)
        {
            return this.Physics.GetBoundingBox(global, height);
        }
        internal bool Intersects(BoundingBox boundingBox)
        {
            return this.GetBoundingBox(this.Global).Intersects(boundingBox);
        }
        internal bool Intersects(Vector3 global, BoundingBox boundingBox)
        {
            return this.GetBoundingBox(global).Intersects(boundingBox);
        }

        internal Vector3 GetNextStep()
        {
            return this.Global + PhysicsComponent.Decelerate(this.Velocity);
        }
        public bool IsStockpilable()
        {
            return this.Def?.Category != null;
        }
        public bool IsFood
        {
            get
            {
                return this.GetComponent<ConsumableComponent>()?.Effects.OfType<NeedEffect>().Any(e => e.Type == NeedDef.Hunger) ?? false;
            }
        }
        
        public static void DrawIcon(Bone body, int w, int h, float scale = 1)
        {
            // same as Body.RenderNewererest
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            var sprite = body.Sprite;
            var loc = new Vector2(0, 0);
            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            MySpriteBatch mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            fx.Parameters["Viewport"].SetValue(new Vector2(w, h));
            Sprite.Atlas.Begin(gd);
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            loc += sprite.OriginGround;
            body.DrawGhost(mysb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
            mysb.Flush();
        }

        public bool Exists => this.IsSpawned;
       

        internal void MoveOrder(TargetArgs target, bool enqueue)
        {
            this.GetComponent<AIComponent>().MoveOrder(target, enqueue);
        }
        internal bool IsAt(Vector3 global)
        {
            var cylindermax = new BoundingCylinder(global, .1f, 1);
            return cylindermax.Contains(this.Global);
        }
        internal void DrawAfter(MySpriteBatch sb, Camera cam)
        {
            foreach (var comp in this.Components.Values)
            {
                comp.DrawAfter(sb, cam, this);
            }
        }
       
        internal bool IsIndoors()
        {
            var region = Server.Instance.Map.GetRegionAt(this.Global.Below().SnapToBlock()); // TODO: find first solid block below object
            return region != null && !region.Room.IsOutdoors;
        }

        internal bool IsForbiddable() => !this.HasComponent<NpcComponent>();
        
        internal void DrawHighlight(SpriteBatch sb, Camera camera)
        {
            SpriteComponent.DrawHighlight(this, sb, camera);
        }

        internal void DrawBorder(SpriteBatch sb, Camera camera)
        {
            this.GetScreenBounds(camera).DrawHighlightBorder(sb, .5f, camera.Zoom);
        }

        internal byte[] Serialize()
        {
            byte[] newData = Network.Serialize(this.Write);
            return newData;
        }
        static readonly Vector3[] HitboxCorners = new Vector3[] {
                    new Vector3(.25f, .25f, 0),
                    new Vector3(-.25f, .25f, 0),
                    new Vector3(.25f, -.25f, 0),
                    new Vector3(-.25f, -.25f, 0)
                };

        internal Vector3 GetCellStandingOn()
        {
            var global = this.Global;
            var below = global.CeilingZ().Below().SnapToBlock();
            var belownode = this.Map.GetNodeAt(below);
            if (belownode is null)
            {
                return below;
            }
            //else check corners because it's standing on the edge of a block
            foreach (var corner in HitboxCorners)
            {
                var pos = (global + corner).CeilingZ().Below().SnapToBlock();
                belownode = this.Map.GetNodeAt(pos);
                if (belownode is not null)
                {
                    return pos;
                }
            }
            throw new Exception(); //throwed when actor was stuck inside a block
        }

        internal bool IsFootprintWithinBlock(Vector3 target)
        {
            return target.ContainsEntityFootprint(this);
        }
        internal BoundingBox GetFootprint()
        {
            return this.GetBoundingBox(this.Global, 0);
        }

        internal void OnGameEvent(GameEvent e)
        {
            foreach (var c in this.Components)
            {
                c.Value.OnGameEvent(this, e);
            }
        }

        public Material Material => this.SpriteComp.GetMaterial(this.Body);

        internal bool HasFocus()
        {
            return Rooms.Ingame.Instance.ToolManager.ActiveTool?.Target?.Object == this;
        }

        internal void Sync(INetwork net)
        {
            PacketEntitySync.Send(net, this);
        }
        internal void SyncWrite(BinaryWriter w)
        {
            foreach (var comp in this.Components)
                comp.Value.SyncWrite(w);
        }

        internal void SyncRead(BinaryReader r)
        {
            foreach (var comp in this.Components)
                comp.Value.SyncRead(this, r);
        }

        internal int GetOwner()
        {
            return this.GetComponent<OwnershipComponent>().Owner;
        }
        internal void SetOwner(GameObject actor)
        {
            this.SetOwner(actor != null ? actor.RefID : -1);
        }
        internal void SetOwner(int actorID)
        {
            this.TryGetComponent<OwnershipComponent>(c => c.SetOwner(this, actorID));
        }

        internal bool IsPlant()
        {
            return this.HasComponent<PlantComponent>();
        }

        internal NpcSkill GetSkill(SkillDef skill)
        {
            return this.GetComponent<NpcSkillsComponent>().GetSkill(skill);
        }

        public float Fuel
        {
            get
            {
                return this.Material?.Fuel.Value ?? 0;
            }
        }
        public float TotalFuel
        {
            get
            {
                return this.Fuel * this.StackSize;
            }
        }

        internal AIState GetState()
        {
            return AIState.GetState(this);
        }
        internal float TotalWeight
        {
            get
            {
                return this.Def.Weight * this.StackSize;
            }
        }
        
        public virtual void TabGetter(Action<string, Action> getter)
        {

        }

        public string DebugName { get { return $"[{this.RefID}]{this.Name}"; } }

        internal List<StatNewModifier> GetStatModifiers(StatNewDef statNewDef)
        {
            return this.GetComponent<StatsComponentNew>()?.GetModifiers(statNewDef);
        }

        internal void AddResourceModifier(ResourceRateModifier resourceModifier)
        {
            this.GetComponent<ResourcesComponent>().AddModifier(resourceModifier);
        }

        internal void AddStatModifier(StatNewModifier statNewModifier)
        {
            this.GetComponent<StatsComponentNew>().AddModifier(statNewModifier);
        }

        public int GetValue()
        {
            if (this.Def.BaseValue == 0)
            {
                return 0;
            }

            var quality = this.DefComponent.Quality;
            var bones = this.Body.GetAllBones();
            var value = 0;
            foreach (var b in bones)
            {
                value += b.Material.ValueBase;
            }
            return (int)(value * this.Def.BaseValue * quality.Multiplier);
        }
        public int GetValueTotal()
        {
            return this.GetValue() * this.StackSize;
        }

        #region packets
        static readonly int PacketSyncInstantiate, PacketSyncSetStacksize, PacketSyncAbsorb;
        static GameObject()
        {
            PacketSyncInstantiate = Network.RegisterPacketHandler(SyncInstantiate);
            PacketSyncSetStacksize = Network.RegisterPacketHandler(SyncSetStacksize);
            PacketSyncAbsorb = Network.RegisterPacketHandler(SyncAbsorb);
        }

        public void SyncInstantiate(INetwork net)
        {
            if (net is not Server server)
                return;

            if (this.RefID != 0)
                throw new Exception();

            net.Instantiate(this);
            var w = server.GetOutgoingStream();
            w.Write(PacketSyncInstantiate);
            this.Write(w);
        }
        private static void SyncInstantiate(INetwork net, BinaryReader r)
        {
            if (net is Server)
            {
                throw new Exception();
            }

            var obj = CreatePrefab(r);
            net.Instantiate(obj);
        }
        public void SyncSetStackSize(int v)
        {
            var net = this.Net;
            if (net is Server)
            {
                this.SetStackSize(v);
            }

            var w = net.GetOutgoingStream();
            w.Write(PacketSyncSetStacksize);
            w.Write(this.RefID);
            w.Write(v);
        }
        private static void SyncSetStacksize(INetwork net, BinaryReader r)
        {
            var obj = net.GetNetworkObject(r.ReadInt32());
            var value = r.ReadInt32();
            if (net is Client)
            {
                obj.SetStackSize(value);
            }
            else
            {
                obj.SyncSetStackSize(value);
            }
        }
        public void Absorb(GameObject obj)
        {
            if (this.IsReserved)
            {
                return;
            }

            if (!this.CanAbsorb(obj))
            {
                return;
            }

            this.StackSize += obj.StackSize;
            obj.Despawn();
            obj.Dispose();
        }
        public void SyncAbsorb(GameObject obj)
        {
            var net = this.Net;
            if (net is Client)
            {
                throw new Exception();
            }

            this.Absorb(obj);
            var w = net.GetOutgoingStream();
            w.Write(PacketSyncAbsorb);
            w.Write(this.RefID);
            w.Write(obj.RefID);
        }
        private static void SyncAbsorb(INetwork net, BinaryReader r)
        {
            if (net is Server)
            {
                throw new Exception();
            }

            var master = net.GetNetworkObject(r.ReadInt32());
            var slave = net.GetNetworkObject(r.ReadInt32());
            master.Absorb(slave);
        }

        IEnumerable<(string name, Action action)> ISelectable.GetInfoTabs()
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
