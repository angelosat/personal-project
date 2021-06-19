using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class InteractableComponent : Component
    {
        //// ComponentCollection Components { get { (ComponentCollection)this["Components"]; } set { this["Components"] = value; } }
        // SortedDictionary<Message.Types, Component> Components { get { return (SortedDictionary<Message.Types, Component>)this["Components"]; } set { this["Components"] = value; } }

        // public InteractableComponent(SortedDictionary<Message.Types, Component> components)
        // {
        //     this.Components = components;
        // }

        // public override bool HandleMessage(GameObject parent, GameObjectEventArgs e)
        // {
        //   //  Component comp;
        //     bool ok = false;
        //     switch (e.Type)
        //     {
        //         case Message.Types.Query:
        //             //if (Components.TryGetValue((Message.Types)e.Parameters[0], out comp))
        //             //    return comp.HandleMessage(parent, e);
        //             e.Sender.HandleMessage(Message.Types.Task, parent, new Task(100f, parent, e.Parameters[0] as GameObjectEventArgs));
        //             break;
        //         default:
        //             //if (Components.TryGetValue(e.Type, out comp))
        //             //    return comp.HandleMessage(parent, e);

        //             foreach (Component comp in Components.Values)
        //                 ok |= comp.HandleMessage(parent, e);
        //             break;
        //     }
        //     return ok;
        // }

        // //public float GetComplexity(GameObjectEventArgs a)
        // //{
        // //    return 100f;
        // //}

        //Dictionary<Message.Types, Action> Actions { get { return (Dictionary<Message.Types, Action>)this["Actions"]; } set { this["Actions"] = value; } }
        List<Interaction> Actions { get { return (List<Interaction>)this["Actions"]; } set { this["Actions"] = value; } }

        //InteractableComponent() { Actions = new Dictionary<Message.Types, Action>(); }
        InteractableComponent() { Actions = new List<Interaction>(); }
        public InteractableComponent(Message.Types msg, Interaction action)
            : this()
        {
            //Actions[msg] = action;
            Actions.Add(action);
        }

       // public InteractableComponent(Dictionary<Message.Types, Action> actions)
        public InteractableComponent(List<Interaction> actions)
            : this()
        {
            this.Actions = actions;
        }

        public InteractableComponent(params Interaction[] actions)
            : this()
        {
            this.Actions = new List<Interaction>(actions);
        }

        public override bool HandleMessage(GameObject parent, GameObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Query:
                    Dictionary<Message.Types, Interaction> lengths = e.Parameters[0] as Dictionary<Message.Types, Interaction>;

                   // foreach (KeyValuePair<Message.Types, Action> action in Actions)
                    //lengths[action.Key] = action.Value;
                    foreach(Interaction action in Actions)
                        lengths[action.Message] = action;
                    return true;
                    break;
                default:
                    break;
            }
            return false;
        }

        public override object Clone()
        {
            return new InteractableComponent(Actions);//this.Components);
        }

    }
}
