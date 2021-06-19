using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components
{
    public abstract class Component : ICloneable
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

        public abstract string ComponentName{get;}// { get { return ""; } }

        public virtual void OnNameplateCreated(UI.Nameplate plate) { }
        public virtual void OnHealthBarCreated(GameObject parent, UI.Nameplate plate) { }

        public Component SetValue(string name, object value) { this[name] = value; return this; }

        public override string ToString()
        {
            return Properties.ToString();
        }

        public ComponentPropertyCollection Properties; //Dictionary<string, object> Parameters;
        

        public object this[string propertyName]
        {
            set { Properties[propertyName] = value; }
            get { return this.Properties[propertyName]; }
        }

        public Component()
        {
            Properties = new ComponentPropertyCollection();
        }
        public Component(GameObject parent)
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
            //return found;
            return property != null;

            //object prop;
            //bool found = Properties.TryGetValue(propertyName, out prop);
            //property = prop as T;
            //return (property != null);

        }
        //internal void ModifyProperty<T>(string p, T p_2)
        //{
        //    (T)Properties[p].Value += p_2;
        //}

        public abstract object Clone();

     //   protected delegate void Initializer();

        public virtual bool Drop(GameObject self, GameObject actor, GameObject obj)
        {
            return false;
        }

        //public virtual bool Give(GameObject parent, GameObject sender, GameObject obj)
        //{
        //    return false;
        //}

        public virtual bool Give(GameObject parent, GameObject giver, GameObjectSlot objSlot) { return false; }

        public virtual bool Activate(GameObject actor, GameObject self)
        {
            return false;
        }
        //public virtual void Query(GameObject parent, GameObjectEventArgs e) { }
        //public virtual void Query(GameObjectEventArgs e) { }
        //public virtual void Query(List<Interaction> actions) { }
        public virtual void Query(GameObject parent, List<InteractionOld> actions) { }

        public virtual void OnHitTestPass(GameObject parent, Vector3 face, float depth) { }


        public virtual void GetDialogueOptions(GameObject parent, GameObject speaker, DialogueOptionCollection options) { }
        public virtual void GetDialogOptions(GameObject parent, GameObject speaker, List<DialogOption> options) { }
       // public virtual void Query(GameObject parent, List<Interaction> actions) { }
        //public virtual bool HandleMessage(GameObject parent, GameObject sender, Message.Types msg)// msg)
        //{
        //    switch (msg)
        //    {
        //        case Message.Types.Activate:
        //            return Activate(sender, parent); 
        //        default:
        //            break;
        //    }
        //    return true;
        //}

        //public virtual bool HandleMessage(GameObjectEventArgs e)
        //{
        //    return false;
        //}

        public virtual bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Activate:
                    return Activate(e.Sender, parent);
                //case Message.Types.Spawn:
                //    Spawn(e.Network, parent);
                //    return true;
                //case Message.Types.Despawn:
                //    Despawn(e.Network, parent);
                //    return true;
                default:
                    break;
            }
            return false;
        }
        internal virtual void HandleRemoteCall(GameObject gameObject, ObjectEventArgs e) { }
        internal virtual void HandleRemoteCall(GameObject gameObject, Message.Types type, BinaryReader r) { }

        public virtual void RandomEvent(GameObject parent, RandomObjectEventArgs e) { }

        public virtual string GetInventoryText(GameObject parent, GameObject actor)
        {
            return "";
        }
        public virtual string GetWorldText(GameObject parent, GameObject actor)
        {
            return "";
        }
        //public abstract Component.Types Type { get; }

        //public virtual void Attach(IEntity entity)
        //{
        //    entity.Components.Add(this);
        //}

        //public virtual void Detach(IEntity entity)
        //{
        //    entity.Components.Remove(this);
        //}

        //public virtual void Instantiate(Net.IObjectProvider instantiator) { }
        public virtual void Instantiate(GameObject parent, Action<GameObject> instantiator) { }

        public virtual void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null) { }//return ScriptState.Finished; }
        public virtual void Update(GameObject parent) { this.Update(parent.Net, parent); }//return ScriptState.Finished; }
        //public virtual void RandomBlockUpdate(Net.IObjectProvider net, GameObject parent) { }//return ScriptState.Finished; }
        public virtual void Initialize(GameObject parent) { }
        public virtual void Spawn(IObjectProvider net, GameObject parent) { }
        //[Obsolete] // pass iobjectprovider to remove object there
        //public virtual void Despawn(IObjectProvider net, GameObject parent) { }
        public virtual void Despawn(GameObject parent) { }
        public virtual void ComponentsCreated(GameObject parent) { }
        public virtual void ObjectLoaded(GameObject parent) { }
        public virtual void ObjectSynced(GameObject parent) { }
        public virtual void Focus(GameObject parent) { }
        public virtual void FocusLost(GameObject parent) { }
        public virtual void ChunkLoaded(Net.IObjectProvider net, GameObject parent) { }

        public virtual void MakeChildOf(GameObject parent) { }

        public virtual void Draw(
            SpriteBatch sb, DrawObjectArgs e
            //Camera camera,
            //Controller controller,
            //Player player,
            //Map map,
            //Chunk chunk,
            //Cell cell,
            //Rectangle bounds,
            //GameObject obj,
            //float depth
            ) { }
        public virtual void Draw(MySpriteBatch sb, DrawObjectArgs e) { }
        public virtual void Draw(MySpriteBatch sb, GameObject parent, Camera camera) { }

        public virtual void DrawMouseover(SpriteBatch sb, Camera camera, GameObject parent) { }
        public virtual void DrawMouseover(MySpriteBatch sb, Camera camera, GameObject parent) { }
        public virtual void DrawUI(SpriteBatch sb, Camera camera, GameObject parent) { }
        public virtual void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global) { }
        public virtual void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, float depth) { }
        public virtual void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, Color color, float depth) { }
        public virtual void DrawFootprint(SpriteBatch sb, Camera camera, Vector3 global) { }

        public virtual void GetChildren(List<GameObjectSlot> list) { }
        public virtual void GetContainers(List<Container> list) { }
        public virtual string GetTooltipText() { return ""; }
        public virtual void GetTooltip(GameObject parent, UI.Control tooltip) { }
        public virtual void GetInventoryTooltip(GameObject parent, UI.Control tooltip) { this.GetTooltip(parent, tooltip); }
        public virtual void GetActorTooltip(GameObject parent, GameObject actor, UI.Tooltip tooltip) { }
        //public virtual void GetUI(GameObject parent, UI.Control ui) { }
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
            var a = Player.Actor;
            foreach (var i in list)
                //if (i.Value.InRange(a, t))
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
        internal virtual List<SaveTag> Save()
        {
            return null;
        }
        internal virtual void Load(SaveTag save)
        {

        }

        public virtual void Write(BinaryWriter w) { }
        public virtual void Read(BinaryReader r) { }
        //public virtual void Read(IObjectProvider net, BinaryReader reader) { this.Read(reader); }

        public virtual void AIQuery(GameObject parent, GameObject ai, List<AIAction> actions) { }
        internal virtual void GetAvailableTasks(GameObject parent, List<Interaction> list)
        {

        }
        internal virtual void GetAvailableActions(List<Script> list)
        {
            
        }
        internal virtual void GetAvailableActions(List<Script.Types> list)
        {

        }
        internal virtual void GetInteractionsFromSkill(GameObject parent, Skills.Skill skill, List<Interaction> list) { }
        //public virtual void GetPlayerActions(Dictionary<KeyBinding, Interaction> list)
        //{

        //}
        public virtual void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
        }




        
    }

    public interface IEntity
    {
        //CompomentCollection Components { get; }
        T GetComponent<T>(string name) where T : Component, new();
        //bool TryGetComponent<T>(string name, out T component) where T : Component;
        //bool TryGetComponent<T>(string name, out Component component);
        bool TryGetComponent<T>(string name, out T component) where T : Component, new();
        void Update(Net.IObjectProvider net, Chunk chunk);
    }

    //public interface IHasChildren
    //{
    //    GameObjectSlot GetChild(int containerID, int slotID);
    //    Vector3 Global { get; }
    //    IObjectProvider Net { get; }
    //    //int InstanceID { get; }
    //    //void PostMessage(ObjectEventArgs a);
    //}
}
