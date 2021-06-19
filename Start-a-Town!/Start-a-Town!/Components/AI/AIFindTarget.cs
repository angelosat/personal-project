using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;

namespace Start_a_Town_.AI.Behaviors
{
    class AIFindTarget : Behavior
    {
        float Range;// { get { return (float)this["Range"]; } set { this["Range"] = value; } }
        Predicate<GameObject> Filter;// { get { return (Predicate<GameObject>)this["Filter"]; } set { this["Filter"] = value; } }


        public override string Name
        {
            get
            {
                return "Finding target";
            }
        }
        public AIFindTarget(float range) : this(range, foo => true) { }
        public AIFindTarget(float range, Predicate<GameObject> filter)
        {
            this.Range = range;
            this.Filter = filter;
        }

        public override BehaviorState Execute(Actor parent, AIState state)//IObjectProvider net, GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            //if (Child != null)
            //{
            //    if (!Child.Execute(parent, personality, memory))
            //        return false;
            //    Child = null;
            //}
            //GameObject parent = state.Parent;
            //IObjectProvider net = state.Net;
            var net = parent.Net;
            var map = net.Map;
            //Chunk chunk = Position.GetChunk(map, parent.Global);
            Chunk chunk = map.GetChunk(parent.Global);

            //List<GameObject> objects = new List<GameObject>(chunk.GetObjects().Except(new GameObject[] { parent }));
            //if (objects.Count == 0)
            //    return new AIIdle(new TimeSpan(0, 0, 1));
            ////  SortedList<float, GameObject> byDistance = new SortedList<float, GameObject>(objects.ToDictionary<float, GameObject>(foo => (foo.Global - parent.Global).Length, foo=>foo));
            List<GameObject> objects = new List<GameObject>();
            //foreach (Chunk ch in Position.GetChunks(map, chunk.MapCoords))
            foreach (Chunk ch in map.GetChunks(chunk.MapCoords))
                objects.AddRange(ch.GetObjects().FindAll(Filter));//foo=>foo.Type == ObjectType.Human));
            //if (objects.Count == 1) //it means that the single object is the parent object
            //    return new AIIdle(new TimeSpan(0, 0, 1));
            GameObject closest = null;// objects.First(foo => foo != parent);
            float closestDist = Range;// Vector3.Distance(closest.Global, parent.Global);
            foreach (GameObject obj in objects)
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
                throw new NotImplementedException();
                //return BehaviorState.Running;
            }
            return BehaviorState.Success;
        }

        public override object Clone()
        {
            return new AIFindTarget(Range);
        }
    }
}
