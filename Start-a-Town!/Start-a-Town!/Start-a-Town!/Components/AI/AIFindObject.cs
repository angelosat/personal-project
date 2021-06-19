using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AIFindObject : Behavior
    {
        float Range { get { return (float)this["Range"]; } set { this["Range"] = value; } }
        Predicate<GameObject> Filter { get { return (Predicate<GameObject>)this["Filter"]; } set { this["Filter"] = value; } }
        GameObject Target { get { return (GameObject)this["Target"]; } set { this["Target"] = value; } }
        Message.Types Action { get { return (Message.Types)this["Message"]; } set { this["Message"] = value; } }

        public override string Name
        {
            get
            {
                return "Finding object";
            }
        }

        public AIFindObject(float range, Predicate<GameObject> filter, Message.Types message)
        {
            this.Range = range;
            this.Filter = filter;
            this.Action = message;
            this.Target = null;
        }

        //public override Behavior Initialize(GameObject parent, Behavior previous)
        //{
        //    //Chunk chunk = Position.GetChunk(parent.Global);

        //    //List<GameObject> objects = new List<GameObject>();
        //    //foreach (Chunk ch in Position.GetChunks(chunk.MapCoords))
        //    //    objects.AddRange(ch.GetObjects().FindAll(Filter));
        //    //GameObject closest = null;
        //    //float closestDist = Range;
        //    //foreach (GameObject obj in objects)
        //    //{
        //    //    if (obj == parent)
        //    //        continue;
        //    //    float dist = Vector3.Distance(obj.Global, parent.Global);
        //    //    if (dist < closestDist)
        //    //    {
        //    //        closest = obj;
        //    //        closestDist = dist;
        //    //    }
        //    //}
        //    //if (closest != null)
        //    //{
        //    //    //closest.HandleMessage(Message.Types.Aggro, parent);
        //    //    //return new AIAttack(closest);
        //    //    Target = closest;
        //    //   // parent["AI"]["Focus"] = closest;
        //    //   // parent.HandleMessage(Message.Types.Begin, null, Target, Action);
        //    //}
            
        //    return this;
        //}

        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            //if (Child != null)
            //{
            //    if (!Child.Execute(parent, personality, memory))
            //        return false;
            //    Child = null;
            //}
            if (Target == null)
            {
              //  parent["AI"]["Current"] = new AIIdle(new TimeSpan(0, 0, 10));
                GameObject closest = null;
                float closestDistance = 10000;
                foreach (GameObject obj in knowledge.Objects.Keys.ToList().FindAll(Filter)) // knowledge.Select(i=>i.Object).ToList().FindAll(Filter))
                {
                    if (obj.GetPosition()["Position"] == null)
                        continue;
                    float dist = Vector3.Distance(obj.Global, parent.Global);
                    if (dist < closestDistance)
                    {
                        closest = obj;
                        closestDistance = dist;
                    }
                }
                if (closest == null)
                    return BehaviorState.Success;
                Target = closest;
            }
            Vector3 difference = (Target.Global - parent.Global);
            if (difference.Length() > 1)
            {
                difference.Normalize();
                difference.Z = 0;
                parent.HandleMessage(Message.Types.Move, parent, difference, 1f);
                return BehaviorState.Running;
            }
            if (parent["Control"]["Task"] == null)
                parent.HandleMessage(Message.Types.Begin, null, Target, Action);
            parent.HandleMessage(Message.Types.Perform);
            if (parent["Control"]["Task"] != null)
                // parent["AI"]["Current"] = new AIIdle();
                return BehaviorState.Running;
            return BehaviorState.Success;
        }
    }
}
