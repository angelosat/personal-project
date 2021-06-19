﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AICarryItem : Behavior
    {
        float Range { get { return (float)this["Range"]; } set { this["Range"] = value; } }
        Predicate<GameObject> Filter
        {
            get { return (Predicate<GameObject>)this["Filter"]; }
            set
            {
                //Console.WriteLine(value); 
                this["Filter"] = value;
            }
        }
        GameObject Target { get { return (GameObject)this["Target"]; } set { this["Target"] = value; } }
        Message.Types Action { get { return (Message.Types)this["Message"]; } set { this["Message"] = value; } }

        public override string Name
        {
            get
            {
                return "Getting item: " + (Target != null ? Target.Name : "");
            }
        }

        public AICarryItem()
        {
            Range = 1;
            Filter = null;
            Target = null;
            Action = Message.Types.PickUp;
        }

        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            if (Filter == null)
            {
              //  Console.WriteLine("no filter");
                return BehaviorState.Fail;

            }
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
                    return BehaviorState.Fail;
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
            Clear();
            return BehaviorState.Success;
        }

        private void Clear()
        {
        //    Console.WriteLine("cleared");
            Filter = null;
            Target = null;
        }


        public override bool HandleMessage(GameObject parent, GameObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.CarryItem:
                    Message.Types msg = (Message.Types)e.Parameters[0];
                    InventoryComponent inv;
                    Predicate<GameObject> filter = obj => FunctionComponent.HasAbility(obj, msg);
                    if (!parent.TryGetComponent<InventoryComponent>("Inventory", out inv))
                        return true;
                    GameObjectSlot objSlot = GameObjectSlot.Empty;
                    if (!inv.TryFind(filter, out objSlot))
                    {
                        parent.HandleMessage(Message.Types.NeedItem, parent, filter);
                        return true;
                    }
                    parent.HandleMessage(Message.Types.Equip, null, objSlot);
                    return true;
                default:
                    return false;
            }
        }
    }
}
