using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AIWork : Behavior
    {
        List<Behavior> Children { get { return (List<Behavior>)this["Children"]; } set { this["Children"] = value; } }
        Behavior Current { get { return (Behavior)this["Current"]; } set { this["Current"] = value; } }
        GameObject Target { get { return (GameObject)this["Target"]; } set { this["Target"] = value; } }
        int CurrentIndex = 0;

        public override string Name
        {
            get
            {
                //return "Satisfying need";
                return Current.Name;
            }
        }

        public override string ToString()
        {
            return "Looking for work";
        }

        public AIWork()
        {
            Children = new List<Behavior>() { 
                //new AICondition(personality => personality.Needs["Work"].Value<50),
            //    new AICondition(agent => agent["Needs"].GetProperty<NeedCollection>("Needs")["Work"].Value<50),
              //  new AIFindNeed(20, "Work"),
                //new AIFindItem(),
             //   new AIInteraction()
            };
            Current = Children[CurrentIndex];// Children.First();
        }

        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            //BehaviorState state = Current.Execute(parent, personality, knowledge, p);
            //switch (state)
            //{
            //    case BehaviorState.Success:
            //        //CurrentIndex = (CurrentIndex++ % Children.Count);
            //        CurrentIndex++;
            //        CurrentIndex = (CurrentIndex % Children.Count);
            //        Current = Children[CurrentIndex];
            //        // return BehaviorState.Success;
            //        return BehaviorState.Running;
            //    case BehaviorState.Fail:
            //        return BehaviorState.Fail;
            //}
            //return BehaviorState.Running;

            //BehaviorState state;
            //do
            //{
            //    state = Current.Execute(parent, personality, knowledge, p);
            //    CurrentIndex++;
            //    CurrentIndex = (CurrentIndex % Children.Count);
            //    Current = Children[CurrentIndex];
            //}
            //while (state != BehaviorState.Running);

            for (int i = CurrentIndex; i < Children.Count; i++)
            {
                BehaviorState state = Children[i].Execute(parent, personality, knowledge, p);
                if (state == BehaviorState.Running)
                {
                    CurrentIndex = i;
                    return BehaviorState.Running;
                }
                else if (state == BehaviorState.Fail)
                    return state;
            }
            CurrentIndex = 0;
            return BehaviorState.Success;
        }

        public override bool HandleMessage(GameObject parent, GameObjectEventArgs e)
        {
            bool handled = false;
            foreach (Behavior b in Children)
                handled |= b.HandleMessage(parent, e);
            return handled;
            //return Current.HandleMessage(parent, e);
        }
    }
}
