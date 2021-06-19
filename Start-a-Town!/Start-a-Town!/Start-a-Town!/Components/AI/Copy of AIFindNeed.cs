using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AIFindNeed : Behavior
    {
        float Range { get { return (float)this["Range"]; } set { this["Range"] = value; } }
        string Filter { get { return (string)this["Filter"]; } set { this["Filter"] = value; } }


        public override string Name
        {
            get
            {
                return "Looking for " + Filter;
            }
        }
     //   public AIFindNeed(float range) : this(range, foo => true) { }
        public AIFindNeed(float range, string filter)
        {
            this.Range = range;
            this.Filter = filter;
        }

        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            //if (Child != null)
            //{
            //    if (!Child.Execute(parent, personality, memory))
            //        return false;
            //    Child = null;
            //}

           // Chunk chunk = Position.GetChunk(parent.Global);

           // List<GameObject> objects = new List<GameObject>();
           // foreach (Chunk ch in Position.GetChunks(chunk.MapCoords))
           //     objects.AddRange(ch.GetObjects());//.FindAll(Filter));
            Dictionary<GameObject, List<Interaction>> directory = new Dictionary<GameObject, List<Interaction>>();
           //// List<GameObject> candidates = new List<GameObject>();
           // foreach (GameObject obj in objects)
           // {
           //     if (obj == parent)
           //         continue;
           //     if (memory.Find(i => i.Object == obj) != null)
           //         continue ;
           //     List<Interaction> interactions = new List<Interaction>();
           //     obj.HandleMessage(Message.Types.Query, parent, interactions);
           //     List<Interaction> found = interactions.FindAll(foo => foo.Need == Filter);
           //     if (found.Count > 0)
           //         directory[obj] = found;
           // }

            //foreach (MemoryEntry entry in memory)
            //{
            //    List<Interaction> interactions = new List<Interaction>();
            //    entry.Object.HandleMessage(Message.Types.Query, parent, interactions);
            //    List<Interaction> found = interactions.FindAll(foo => foo.Need == Filter);
            //    if (found.Count > 0)
            //        directory[entry.Object] = found;
            //}

            GameObject closest = null;
            float closestDist = Range;
         //   foreach (GameObject obj in directory.Keys)
           // foreach (GameObject obj in knowledge.FindAll(i => i.Needs.Exists(foo => foo.Name == Filter)).Select(i => i.Object))
            foreach (GameObject obj in knowledge.Objects.Values.ToList().FindAll(i => i.Needs.ContainsKey(Filter)).Select(i => i.Object))
            //foreach (GameObject obj in knowledge.FindAll(i => i.Needs.Contains(Filter)).Select(i => i.Object))
            {
                if (obj == parent)
                    continue;
                float dist = Vector3.Distance(obj.Global, parent.Global);
                if (dist < closestDist)
                {
                    closest = obj;
                    closestDist = dist;
                }
            }

            if (closest != null)
            {
              ////  closest.HandleMessage(Message.Types.Aggro, parent);
              //  parent["AI"]["Current"] = new AIInteraction(closest, directory[closest].First()).Initialize(parent);// AIAttack(closest);
              //  //memory.Add(new MemoryEntry(closest, 100, 30, 1, Filter));
              //  return this;
                List<Interaction> interactions = new List<Interaction>();
                closest.HandleMessage(Message.Types.Query, parent, interactions);
                List<Interaction> found = interactions.FindAll(foo => foo.Need.Name == Filter);
             //   Child = new AIInteraction(closest, directory[closest].First()).Initialize(parent);
               // Child = 
                parent.HandleMessage(Message.Types.StartBehavior, parent, new AIInteraction(closest, found.First()).Initialize(parent));
                return BehaviorState.Running;
            }
            //parent["AI"]["Current"] = new AIIdle(new TimeSpan(0, 0, 1));
            //return this;
            return BehaviorState.Finished;
        }
    }
}
