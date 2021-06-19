using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components
{
    class BedComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "BedComponent"; }
        }
        GameObjectSlot Slot { get { return (GameObjectSlot)this["Slot"]; } set { this["Slot"] = value; } }
        float Timer { get { return (float)this["NeedsTimer"]; } set { this["NeedsTimer"] = value; } }

        public BedComponent()
        {
            this.Timer = 60;
            this.Slot = GameObjectSlot.Empty;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Activate:
                   
                    this.Slot.Object = e.Sender;
                    throw new NotImplementedException();
                    //GameObject.PostMessage(e.Sender, Message.Types.AIStop, parent);
                    //GameObject.PostMessage(e.Sender, Message.Types.ControlDisable, parent);
  
                    //e.Sender.Remove();
                    e.Network.Despawn(e.Sender);
                    break;


                case Message.Types.ManageEquipment:
                    throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.UIOwnership, parent, new Predicate<GameObject>(bed => bed.Components.ContainsKey("Bed")));
                    break;

                default:
                    return false;
            }
            return true;
        }

        public override void Tick(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            if (!Slot.HasValue)
                return;
            throw new NotImplementedException();

            //Slot.Object.Update(map, chunk);
            //this.Timer -= 1;// GlobalVars.DeltaTime;
            //if (this.Timer > 0)
            //    return;
            //this.Timer = 60f;
            //Slot.Object.PostMessage(Message.Types.ModifyNeed, parent, "Sleep", 5);
            //Slot.Object.PostMessage(Message.Types.AIStart, parent);
            ////if (NeedsComponent.GetNeed(Slot.Object, "Sleep") < 95)
            ////    return;
            //Chunk.AddObject(Slot.Object, map);
            //GameObject.PostMessage(Slot.Object, Message.Types.ControlEnable, parent);
            //// TODO: maybe handle the ejection of the agent by messaging, also handle case where bed is being deleted while agent is sleeping
            //Slot.Object = null;
        }

        //public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        //{
        //    // if(OwnershipComponent.Owns(e.Sender, parent))
        //    list.Add(new InteractionOld(new TimeSpan(0, 0, 1), Message.Types.Activate, source: parent, name: "Sleep", verb: "Sleeping", effect: new List<AIAdvertisement>(){new AIAdvertisement("Sleep", 100)},// new InteractionEffect("Sleep"),
        //        cond: new ConditionCollection(
        //            new Condition((actor, target) => OwnershipComponent.Owns(actor, target), "This " + parent.Name + " is not mine.")
        //            )));

            
        //}

        public override object Clone()
        {
            return new BedComponent();
        }

        
    }
}
