using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components.AI
{
    enum BehaviorState { Running, Success, Fail }

    abstract class Behavior : ICloneable
    {
        public Behavior(params Behavior[] children)
        {
            this.Children = new List<Behavior>();
            this.ScriptComponents = new List<Interactions.ScriptComponent>();
            //this.Name = name;
            this.Children.AddRange(children);
        }
        public List<Interactions.ScriptComponent> ScriptComponents { get; set; }
        public List<Behavior> Children { get { return (List<Behavior>)this["Children"]; } set { this["Children"] = value; } }
        Dictionary<string, object> Properties = new Dictionary<string, object>();
        public virtual string Name { get { return ""; } }
        public override string ToString() { return Name; }
        public object this[string propertyName] { get { return Properties[propertyName]; } set { Properties[propertyName] = value; } }
        //public virtual BehaviorState Execute(Net.IObjectProvider net, GameObject parent, AIState state)
        //{
        //    foreach (var scr in ScriptComponents)
        //        scr.Update(net, parent);
        //    return BehaviorState.Running;
        //}
        public virtual BehaviorState Execute(GameObject parent, AIState state)
        {
            //return Execute(parent.Net, parent, state);
            foreach (var scr in ScriptComponents)
                scr.Update(parent);
            return BehaviorState.Running;
        }
        public virtual Behavior Initialize(GameObject parent) { return this; }
        public virtual Behavior Initialize(AIState state) { return this; }
        public virtual Behavior Initialize(Personality personality) { return this; }
        public virtual Behavior Finalize(GameObject parent) { return this; }
        //public virtual Behavior Initialize(AIState state) { return this; }
        public virtual bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            Message.Types msg = e.Type;
            switch (msg)
            {
                case Message.Types.DialogueOption:
                    var sender = e.Parameters[0] as GameObject;
                    var option = (string)e.Parameters[1];
                    this.HandleCommunication(parent, sender, option);
                    return true;

                default:
                    foreach (var item in this.Children)
                        item.HandleMessage(parent, e);
                    return true;
            }
            return false;
        }
        public virtual Conversation.States HandleConversation(GameObject parent, GameObject speaker, string option, out string speech, out DialogueOptionCollection options) { speech = ""; options = new DialogueOptionCollection(); return Conversation.States.Finished; }
        public virtual void GetDialogueOptions(GameObject parent, GameObject speaker, DialogueOptionCollection options) { }
        public virtual void GetDialogOptions(GameObject parent, GameObject speaker, List<DialogOption> options)
        {
            foreach (var item in this.Children)
                item.GetDialogOptions(parent, speaker, options);
        }
        public virtual string Satisfies { get; set; }
        public abstract object Clone();
        public virtual void Query(GameObject parent, List<InteractionOld> actions) { }

        public virtual void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            foreach (var item in this.Children)
                item.GetInteractions(parent, actions);
        }

        public virtual void Write(BinaryWriter w) { }
        public virtual void Read(BinaryReader r) { }

        public virtual void Sync(int seed)
        {
            foreach (var child in this.Children) child.Sync(seed);
        }

        public virtual void GetRightClickAction(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
            foreach (var child in this.Children)
                child.GetRightClickAction(parent, actions);
        }

        protected virtual void HandleCommunication(GameObject parent, GameObject sender, string option) 
        {
            foreach (var item in this.Children)
                item.HandleCommunication(parent, sender, option);
        }

        //internal virtual void HandleRPC(GameObject parent, ObjectEventArgs e)
        //{
        //    foreach (var child in this.Children)
        //        child.HandleRPC(parent, e);
        //}

        internal virtual void HandleRPC(GameObject parent, Message.Types type, BinaryReader r)
        {
            foreach (var child in this.Children)
                child.HandleRPC(parent, type, r);
        }

        

        //public class BhavProperties
        //{

        //}

        internal virtual void OnSpawn(GameObject parent, AIState state)
        {
            foreach (var child in this.Children)
                child.OnSpawn(parent, state);
        }
    }
}

