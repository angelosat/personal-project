using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AISatisfyItem : Behavior
    {
        float Range { get { return (float)this["Range"]; } set { this["Range"] = value; } }
        string Filter { get { return (string)this["Filter"]; } set { this["Filter"] = value; } }
      //  GameObject Target { get { return (GameObject)this["Target"]; } set { this["Target"] = value; } }
      //  Message.Types Action { get { return (Message.Types)this["Message"]; } set { this["Message"] = value; } }
        Dictionary<GameObject, Queue<Interaction>> InterestingInteractions { get { return (Dictionary<GameObject, Queue<Interaction>>)this["InterestingInteractions"]; } set { this["InterestingInteractions"] = value; } }
        Queue<Interaction> Interesting { get { return (Queue<Interaction>)this["Interesting"]; } set { this["Interesting"] = value; } }
        Stack<Interaction> InteractionChain { get { return (Stack<Interaction>)this["InteractionStack"]; } set { this["InteractionStack"] = value; } }
        Interaction CurrentInteraction// { get { return (Interaction)this["CurrentInteraction"]; } set { this["CurrentInteraction"] = value; } }
        {
            get { return InteractionChain.Peek(); }
        }
        
        public override string Name
        {
            get
            {
                return "Looking for " + Filter;
            }
        }

        public AISatisfyItem()
        {
            Interesting = new Queue<Interaction>();
            InteractionChain = new Stack<Interaction>();
        }

        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            //      Dictionary<GameObject, List<Interaction>> directory = new Dictionary<GameObject, List<Interaction>>();
           // if (Target != null)
            if(InteractionChain.Count>0)
                //if (CurrentInteraction != null)
            {
                Vector3 difference = (CurrentInteraction.Source.Global - parent.Global);
                if (difference.Length() > 1)
                {
                    difference.Normalize();
                    difference.Z = 0;
                    parent.HandleMessage(Message.Types.Move, parent, difference, 1f);
                    return BehaviorState.Running;
                }
                if (parent["Control"]["Task"] == null)
                    parent.HandleMessage(Message.Types.Begin, null, CurrentInteraction.Source, CurrentInteraction.Message);// Target, Action);
                parent.HandleMessage(Message.Types.Perform);
                if (parent["Control"]["Task"] != null)
                    return BehaviorState.Running;
                Clear();
                return BehaviorState.Success;
            }



            //  List<MemoryEntry> sortedMemory = new List<MemoryEntry>(knowledge.Objects.Values.ToList().FindAll(i => i.Needs.ContainsKey(Filter)).OrderByDescending(m => m.Score));//OrderBy(m => (m.Object.Global - parent.Global).Length()));

            //populate the interestinginceraction list if it's empty
            if (Interesting.Count == 0)
            {
                List<MemoryEntry> sortedMemory = new List<MemoryEntry>(knowledge.Objects.Values.ToList().FindAll(i => i.Needs.ContainsKey(Filter)).OrderByDescending(m => m.Score));//OrderBy(m => (m.Object.Global - parent.Global).Length()));
                if (sortedMemory.Count == 0)
                    return BehaviorState.Fail;

                // iterate through all objects in memory
                foreach (MemoryEntry mem in sortedMemory) 
                {
                    List<Interaction> interactions = new List<Interaction>();

                    // get interactions offered by object
                    mem.Object.HandleMessage(Message.Types.Query, parent, interactions); 
                    
                    // find interactions fulfiling specific need and order them, then iterate and populate the interesting list
                    foreach (Interaction inter in interactions.FindAll(i => i.Need != null).FindAll(foo => foo.Need.Name == Filter).OrderByDescending(foo => foo.Need.Value))
                    {
                       
                        inter.Source = mem.Object;// TODO: fix this
                        Interesting.Enqueue(inter);
                    }
                    //InterestingInteractions.Add(mem.Object, new Queue<Interaction>(interactions.FindAll(i => i.Need != null).FindAll(foo => foo.Need.Name == Filter).OrderByDescending(foo => foo.Need.Value)));

                }

                return BehaviorState.Running;
            }

            //CurrentInteraction = Interesting.Dequeue();
            InteractionChain.Clear();
            Interaction current = Interesting.Dequeue();
            InteractionChain.Push(current);
            //while (current.Requirement != null)
            //{
            //    InteractionChain.Push(new Interaction(TimeSpan.Zero, current.Requirement.Message,
            //}

            //InteractionChain.Push(Interesting.Dequeue());
            //while(CurrentInteraction.Requirement != null)


           // CurrentInteraction = InteractionChain.Pop();

            return BehaviorState.Running;

            //List<Interaction> interactions, interestingInteractions;
            //foreach (MemoryEntry mem in sortedMemory)
            //{
            //    GameObject memObj = mem.Object;
            //    interactions = new List<Interaction>();
            //    memObj.HandleMessage(Message.Types.Query, parent, interactions);
            //    interestingInteractions = new List<Interaction>(interactions.FindAll(i => i.Need != null).FindAll(foo => foo.Need.Name == Filter).OrderByDescending(foo => foo.Need.Value));
            //    if (interestingInteractions.Count == 0)
            //        continue;
            //    foreach (Interaction i in interestingInteractions)
            //    {
            //        if (i.Requirement.Condition(parent))
            //        {
            //            Target = memObj; ;
            //            Action = i.Message;
            //            return BehaviorState.Running;
            //        }
            //        else
            //        {
            //            parent.HandleMessage(i.Requirement.Message, parent, i.Requirement.Parameters);
            //            //return BehaviorState.Fail;
            //        }
            //    }
            //}
            //return BehaviorState.Fail;


        }

        private void Clear()
        {
         //   CurrentInteraction = null;
            InteractionChain.Pop();
            //Target = null;
            //Action = Message.Types.Default;
        }

        public override bool HandleMessage(GameObject parent, GameObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.NeedItem:

                    break;
                default:
                    break;
            }
            return false;
        }
    }
}
