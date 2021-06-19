using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AIFindItems : Behavior
    {
        float Range { get { return (float)this["Range"]; } set { this["Range"] = value; } }
        Predicate<GameObject> Filter { get { return (Predicate<GameObject>)this["Filter"]; } set { this["Filter"] = value; } }
        GameObject Target { get { return (GameObject)this["Target"]; } set { this["Target"] = value; } }
        Message.Types Action { get { return (Message.Types)this["Message"]; } set { this["Message"] = value; } }
        Queue<GameObject> Found { get { return (Queue<GameObject>)this["Found"]; } set { this["Found"] = value; } }
        ObjectFilter ObjectFilter { get { return (ObjectFilter)this["ObjectFilter"]; } set { this["ObjectFilter"] = value; } }

        public override string Name
        {
            get
            {
                return "Getting item: " + (Target != null ? Target.Name : "");
            }
        }

        public AIFindItems()
        {
            Found = new Queue<GameObject>();
            Range = 1;
            Filter = null;
            Target = null;
            Action = Message.Types.PickUp;
        }

        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            if (Filter == null)
                return BehaviorState.Success;
            GameObjectSlot currentHauledSlot;
            if(InventoryComponent.TryGetHauling(parent, out currentHauledSlot))
            {
                if (ObjectFilter.Apply(currentHauledSlot.Object))
                    return BehaviorState.Success;
            }
            if (Target == null)
            {
                SortedList<float, GameObject> found = new SortedList<float, GameObject>();
                foreach (GameObject obj in knowledge.Objects.Keys.ToList().FindAll(Filter)) // knowledge.Select(i=>i.Object).ToList().FindAll(Filter))
                {
                    if (!obj.Exists) //obj.GetPosition()["Position"] == null)
                        continue;
                    found.Add(Vector3.Distance(obj.Global, parent.Global), obj);
                }
                if (found.Count == 0)
                    return BehaviorState.Success;
                this.Found = new Queue<GameObject>(found.Values);
                Target = this.Found.Dequeue();
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
            if (this.Found.Count > 0)
            {
                Target = this.Found.Dequeue();
                return BehaviorState.Running;
            }
            Clear();
            return BehaviorState.Success;
        }

        private void Clear()
        {
            Filter = null;
            Target = null;
            Found.Clear();
        }


        public override bool HandleMessage(GameObject parent, GameObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.NeedItem:
                    //Knowledge knowledge = (Knowledge)parent["AI"]["Memory"];
                    //Predicate<GameObject> filter = e.Parameters[0] as Predicate<GameObject>;
                    //List<GameObject> validObjects = knowledge.Objects.Keys.ToList().FindAll(filter);
                    //float closestDistance = 10000;
                    //GameObject closest = null;
                    //foreach (GameObject obj in validObjects)
                    //{
                    //    if (obj.GetPosition()["Position"] == null)
                    //        continue;
                    //    float dist = Vector3.Distance(obj.Global, parent.Global);
                    //    if (dist < closestDistance)
                    //    {
                    //        closest = obj;
                    //        closestDistance = dist;
                    //    }
                    //}
                    Filter = e.Parameters[0] as Predicate<GameObject>;
                    ObjectFilter = e.Parameters[1] as ObjectFilter;
                    Found = new Queue<GameObject>((int)e.Parameters[2]);
                    return true;
                default:
                    return false;
            }
        }
    }
}
