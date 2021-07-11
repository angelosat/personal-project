using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    public abstract class EntityComponent : ICloneable, IEntityComp
    {
        public enum Types
        {
            Camera,
            Position,
            Movement,
            Physics,
            Tasks,
            Inventory,
            Needs
        }

        internal virtual void Initialize(ComponentProps componentProps)
        {
        }

        public abstract string ComponentName{get;}

        public virtual void OnNameplateCreated(GameObject parent, UI.Nameplate plate) { }
        public virtual void OnHealthBarCreated(GameObject parent, UI.Nameplate plate) { }

        public EntityComponent SetValue(string name, object value) { this[name] = value; return this; }

        public override string ToString()
        {
            return Properties.ToString();
        }
        [Obsolete]
        public ComponentPropertyCollection Properties;
        public GameObject Parent;

        public object this[string propertyName]
        {
            set { Properties[propertyName] = value; }
            get { return this.Properties[propertyName]; }
        }

        public EntityComponent()
        {
            Properties = new ComponentPropertyCollection();
        }
        public EntityComponent(GameObject parent)
            : this()
        { }
        public virtual T GetPropertyOrDefault<T>(string propertyName, T defaultValue)
        {
            T property;
            if (!TryGetProperty<T>(propertyName, out property))
                return defaultValue;
            return property;
        }
        public virtual T GetPropertyOrDefault<T>(string propertyName)
        {
            T property;
            if (!TryGetProperty<T>(propertyName, out property))
                return default(T);
            return property;
        }
        public virtual T GetProperty<T>(string propertyName)
        {
            return (T)this.Properties[propertyName];
        }
        public virtual bool TryGetProperty<T>(string propertyName, out T property)
        {
            object prop;
            bool found = Properties.TryGetValue(propertyName, out prop);
            if (!found)
            {
                property = default(T);
                return false;
            }
            property = (T)prop;
            return property != null;

        }


        public abstract object Clone();


        public virtual bool Drop(GameObject self, GameObject actor, GameObject obj)
        {
            return false;
        }

        public virtual bool Give(GameObject parent, GameObject giver, GameObjectSlot objSlot) { return false; }

        public virtual bool Activate(GameObject actor, GameObject self)
        {
            return false;
        }

        public virtual void OnHitTestPass(GameObject parent, Vector3 face, float depth) { }

        public virtual bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Activate:
                    return Activate(e.Sender, parent);

                default:
                    break;
            }
            return false;
        }
        internal virtual void HandleRemoteCall(GameObject gameObject, ObjectEventArgs e) { }
        internal virtual void HandleRemoteCall(GameObject gameObject, Message.Types type, BinaryReader r) { }


        public virtual string GetInventoryText(GameObject parent, GameObject actor)
        {
            return "";
        }
        public virtual string GetWorldText(GameObject parent, GameObject actor)
        {
            return "";
        }
        
        public virtual void Instantiate(GameObject parent, Action<GameObject> instantiator) { }

        public virtual void Tick() { }
        public void Tick(MapBase map, IEntityCompContainer entity)
        {
            throw new NotImplementedException();
        }
        public void Tick(MapBase map, IEntityCompContainer entity, Vector3 global)
        {
            throw new NotImplementedException();
        }

        public virtual void Initialize(GameObject parent) { }
        public virtual void Initialize(GameObject parent, RandomThreaded random) { this.Initialize(parent); }
        public virtual void OnSpawn(IObjectProvider net, GameObject parent) { }
        public virtual void OnDespawn(GameObject parent) { }
        public virtual void OnDispose(GameObject parent) { }
        public virtual void OnObjectCreated(GameObject parent) { }
        public virtual void OnObjectLoaded(GameObject parent) { }
        public virtual void OnObjectSynced(GameObject parent) { }
        public virtual void Focus(GameObject parent) { }
        public virtual void FocusLost(GameObject parent) { }
        public virtual void ChunkLoaded(IObjectProvider net, GameObject parent) { }
        public virtual void SetMaterial(Material mat) { }
        internal virtual void Initialize(Entity parent, Dictionary<string, Material> materials) { }
        internal virtual void Initialize(Entity parent, Quality quality) { }

        public virtual void MakeChildOf(GameObject parent) { this.Parent = parent; }

        public virtual void Draw(SpriteBatch sb, DrawObjectArgs e) { }
        public virtual void Draw(MySpriteBatch sb, DrawObjectArgs e) { }
        public virtual void Draw(MySpriteBatch sb, GameObject parent, Camera camera) { }

        public virtual void DrawMouseover(SpriteBatch sb, Camera camera, GameObject parent) { }
        public virtual void DrawMouseover(MySpriteBatch sb, Camera camera, GameObject parent) { }
        public virtual void DrawUI(SpriteBatch sb, Camera camera, GameObject parent) { }
        public virtual void DrawAfter(MySpriteBatch sb, Camera cam, GameObject parent) { }
        public virtual void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global) { }
        public virtual void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, float depth) { }
        public virtual void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, Color color, float depth) { }
        public virtual void DrawFootprint(SpriteBatch sb, Camera camera, Vector3 global) { }

        public virtual void GetChildren(List<GameObjectSlot> list) { }
        public virtual void GetContainers(List<Container> list) { }
        public virtual string GetTooltipText() { return ""; }
        public virtual void OnTooltipCreated(GameObject parent, UI.Control tooltip) { }
        public virtual void GetInventoryTooltip(GameObject parent, UI.Control tooltip) { this.OnTooltipCreated(parent, tooltip); }
        public virtual void GetActorTooltip(GameObject parent, GameObject actor, UI.Tooltip tooltip) { }
        public virtual void GetUI(GameObject parent, UI.Control ui, List<EventHandler<ObjectEventArgs>> gameEventHandlers) { }
        public virtual void GetUI(GameObject parent, UI.Control ui, List<EventHandler<GameEvent>> gameEventHandlers) { }
        public virtual void GetContextActions(GameObject actor, List<GameObject> abilities) { }
        internal virtual ContextAction GetContextRB(GameObject parent, GameObject player)
        {
            return null;
        }

        internal virtual ContextAction GetContextActivate(GameObject parent, GameObject player)
        {
            return null;
        }
        public virtual void GetClientActions(GameObject parent, List<ContextAction> actions)
        {
            var list = new Dictionary<PlayerInput, Interaction>();
            this.GetPlayerActionsWorld(parent, list);
            var t = new TargetArgs(parent);
            var a = parent.Net.GetPlayer().ControllingEntity;
            foreach (var i in list)
                    actions.Add(new ContextAction(i.Key.ToString() + ": " + i.Value.Name, null) { Available = () => i.Value.Conditions.Evaluate(a, t) });// () => true));
        }
        public virtual void GetInteractions(GameObject parent, List<Interaction> actions) { }
        public virtual void GetRightClickActions(GameObject parent, List<ContextAction> actions) { }
        public virtual void GetEquippedActions(GameObject parent, List<Interaction> actions) { }
        internal virtual void GetEquippedActionsWithTarget(GameObject parent, GameObject actor, TargetArgs t, List<Interaction> list)
        {
        }
        public virtual void GetHauledActions(GameObject parent, TargetArgs target, List<Interaction> actions) { }
        public virtual void GetInventoryContext(GameObject actor, List<ContextAction> actions, int inventorySlotID) { }
        public virtual void GetInventoryActions(GameObject actor, GameObjectSlot parentSlot, List<ContextAction> actions) { }
        public virtual string GetStats() { return ""; }
        internal SaveTag SaveAs(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.AddSaveData(tag);
            return tag.Value != null ? tag : null;
        }
        internal virtual List<SaveTag> Save()
        {
            return null;
        }
        internal virtual void AddSaveData(SaveTag tag)
        {
            var list = this.Save();
            if (list != null)
                foreach (var t in list)
                    tag.Add(t);
        }
        internal virtual void Load(GameObject parent, SaveTag tag)
        {
            this.Load(tag);
        }
        internal virtual void Load(SaveTag tag)
        {

        }

        public virtual void Write(BinaryWriter w) { }
        public virtual void Read(BinaryReader r) { }
        internal virtual void GetAvailableTasks(GameObject parent, List<Interaction> list)
        {

        }
        
        internal virtual void GetInteractionsFromSkill(GameObject parent, ToolAbilityDef skill, List<Interaction> list) { }
        
        public virtual void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
        }
        public virtual GroupBox GetGUI() { return null; }
        internal virtual void GetInterface(GameObject parent, UI.Control box) { }
        internal virtual void GetManagementInterface(GameObject gameObject, UI.Control box) { }

        internal virtual void MapLoaded(GameObject parent)
        {
        }

        public virtual void Select(UI.UISelectedInfo info, GameObject parent)
        {

        }
        internal virtual void GetQuickButtons(UISelectedInfo info, GameObject parent)
        {
        }
        internal virtual IEnumerable<(string name, Action action)> GetInfoTabs() { yield break; }
        internal virtual void GetSelectionInfo(IUISelection info, GameObject parent)
        {
        }

        internal virtual void OnGameEvent(GameObject gameObject, GameEvent e)
        {
        }

        internal virtual void SyncWrite(BinaryWriter w)
        {
        }
        internal virtual void SyncRead(GameObject parent, BinaryReader r)
        {
        }

        void IEntityComp.Load(SaveTag tag)
        {
            throw new NotImplementedException();
        }

        public SaveTag Save(string name)
        {
            throw new NotImplementedException();
        }

        public void OnEntitySpawn(IEntityCompContainer entity, Vector3 global)
        {
            throw new NotImplementedException();
        }

        public void Draw(Camera camera, MapBase map, Vector3 global)
        {
            throw new NotImplementedException();
        }

        internal virtual IEnumerable<ContextAction> GetInventoryContextActions(GameObject actor)
        {
            yield break;
        }
    }
}
