using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AINeed : Behavior
    {
        List<Behavior> Children { get { return (List<Behavior>)this["Children"]; } set { this["Children"] = value; } }
        GameObject Target { get { return (GameObject)this["Target"]; } set { this["Target"] = value; } }

        public override string Name
        {
            get
            {
                return "Satisfying need";
            }
        }

        public AINeed()
        {
            Children = new List<Behavior>() { new AIInteraction() };
        }

        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            if(personality.Needs.Count == 0)
                return BehaviorState.Success;
            float lowest = 100;
            string lowestNeed = "";
            foreach (KeyValuePair<string, Need> need in personality.Needs)
            {
                if (need.Value.Value < lowest)
                {
                    lowest = need.Value.Value;
                    lowestNeed = need.Key;
                }
            }
            if (lowest > 50)
            {
                //parent["AI"]["Current"] = new AIWander(0.3f);
                //return this;
                return BehaviorState.Success;
            }

            Dictionary<GameObject, List<Interaction>> directory = new Dictionary<GameObject, List<Interaction>>();
            int range = 15;
            GameObject closest = null, bestObject = null;
            float closestDist = range, bestNeedValue = 0;


            foreach (MemoryEntry mem in knowledge.Objects.Values)
            {
                if ((mem.Object.Global - parent.Global).Length() > 10) // ignore if it's too far
                    continue;
                Need need;
                if (mem.Needs.TryGetValue(lowestNeed, out need))
                    if (need.Value > bestNeedValue)
                    {
                        bestObject = mem.Object;
                        bestNeedValue = need.Value;
                    }
            }

            //foreach (GameObject obj in AIComponent.GetNearbyObjects(parent))
            //{
            //    if ((obj.Global - parent.Global).Length() > 10) // ignore if it's too far
            //        continue;
            //    List<Interaction> interactions = new List<Interaction>();
            //    obj.HandleMessage(Message.Types.Query, parent, interactions);
            //    Knowledge.Objects[obj] = new MemoryEntry(obj, 100, 100, 1, interactions.Select(i => i.Need).ToArray());
            //}

            if (bestObject == null)
            {
                //    Console.WriteLine("closest == null");
                return BehaviorState.Success;
            }

            //     List<Interaction> found = interactions.FindAll(foo => foo.Need == "Work");
            List<Interaction> found = new List<Interaction>();
            List<Interaction> interactions = new List<Interaction>();
            bestObject.HandleMessage(Message.Types.Query, parent, interactions);
            //      parent.HandleMessage(Message.Types.StartBehavior, parent, new AIInteraction(closest, found.First()).Initialize(parent));
            //return true;
            List<Interaction> foundInteractions = interactions.FindAll(i => i.Need != null);
            if (foundInteractions.Count == 0)
                return BehaviorState.Success;
            Interaction firstInteraction = foundInteractions.First(i => i.Need.Name == lowestNeed);
            if (firstInteraction.Message == Message.Types.Attack)
            {
                parent.HandleMessage(new GameObjectEventArgs(Message.Types.SetTarget, parent, bestObject));
                return BehaviorState.Success;
            }
            return Children.First().Execute(parent, personality, knowledge, bestObject, firstInteraction);//, found);
        }

        //public override bool HandleMessage(GameObject parent, GameObjectEventArgs e)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.NeedItem:
        //            Knowledge knowledge = (Knowledge)parent["AI"]["Memory"];
        //            Predicate<GameObject> filter = e.Parameters[0] as Predicate<GameObject>;
        //            List<GameObject> validObjects = knowledge.Objects.Keys.ToList().FindAll(filter);
        //            float closestDistance = 10000;
        //            GameObject closest = null;
        //            foreach (GameObject obj in validObjects)
        //            {
        //                if (obj.GetPosition()["Position"] == null)
        //                    continue;
        //                float dist = Vector3.Distance(obj.Global, parent.Global);
        //                if (dist < closestDistance)
        //                {
        //                    closest = obj;
        //                    closestDistance = dist;
        //                }
        //            }
        //            return true;
        //        default:
        //            return false;
        //    }
        //}
    }
}
