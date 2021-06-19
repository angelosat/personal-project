using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class BodypartComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Bodypart";
            }
        }

        public BodypartComponent()
            : base()
        {
            // TODO: put default values instead of null
            Properties.Add("Slot", null);
            //Properties.Add("Use", null);
        }
        public BodypartComponent(string slot)//, int use)
        {
            //Slot = slot;
            //Use = use;
            Properties["Slot"] = slot;
            //Properties["Use"] = use;
        }

        public bool Equip(GameObject actor, GameObject parent)
        {
            InventoryComponent inv;
           // if (actor.TryGetComponent<InventoryComponent>("Inventory", out inv))
           // {
               // inv.Equipment[Slot].Object = parent;

                StatsComponent parentstats, actorstats;
                if (actor.TryGetComponent<StatsComponent>("Stats", out actorstats))
                {
                    // TODO: combine stats in the same component maybe
                    if (parent.TryGetComponent<StatsComponent>("Stats", out parentstats))
                    {
                        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                        {
                            //actorstats.Parameters[parameter.Key].Value += parameter.Value.Value;
                            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) + (float)parameter.Value;
                            // Console.WriteLine(actorstats.Parameters[parameter.Key].Value);
                        }
                    }

                    if (parent.TryGetComponent<StatsComponent>("Skills", out parentstats))
                        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) + (float)parameter.Value;

                    if (parent.TryGetComponent<StatsComponent>("Damage", out parentstats))
                        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) + (float)parameter.Value;
                }
                return true;
          //  }
            return false;
        }

        public bool Unequip(GameObject actor, GameObject parent)
        {
            InventoryComponent inv;
           // if (actor.TryGetComponent<InventoryComponent>("Inventory", out inv))
           // {
                // inv.Equipment[Slot].Object = parent;

                StatsComponent parentstats, actorstats;
                if (actor.TryGetComponent<StatsComponent>("Stats", out actorstats))
                {
                    if (parent.TryGetComponent<StatsComponent>("Stats", out parentstats))
                    {
                        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                        {
                            //actorstats.Parameters[parameter.Key].Value += parameter.Value.Value;
                            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) - (float)parameter.Value;
                            //Console.WriteLine(actorstats.Parameters[parameter.Key].Value);
                        }
                    }

                    if (parent.TryGetComponent<StatsComponent>("Skills", out parentstats))
                        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) - (float)parameter.Value;

                    if (parent.TryGetComponent<StatsComponent>("Damage", out parentstats))
                        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) - (float)parameter.Value;

                }
                return true;
          //  }
            return false;
        }


        public override string GetTooltipText()
        {
            return "Right click: Equip";
        }

        public override object Clone()
        {
            //  EquipComponent eq = new EquipComponent(Slot, Use);
            //return eq;


            BodypartComponent phys = new BodypartComponent();
            foreach (KeyValuePair<string, object> property in Properties)
            {
                phys.Properties[property.Key] = property.Value;
            }
            return phys;

        }

        //public override string GetInventoryText(GameObject actor)
        //{
        //    return "Bodypart";///Right click: Equip";
        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            InventoryComponent inv;
            GameObjectSlot slot;
            BodyComponent body;
            BodyPart bodyPart;
            string slotName;
            switch (msg)
            {
                case Message.Types.Equip:
                    //inv = sender.GetComponent<InventoryComponent>("Inventory");
                    //if (inv.TryTake(parent, out slot))
                    //{
                        body = sender.GetComponent<BodyComponent>("Equipment");
                        slotName = GetProperty<string>("Slot");
                        
                        if (body.Properties.ContainsKey(slotName))
                        {
                            slot = body[slotName] as GameObjectSlot;
                            bodyPart = body.GetProperty<BodyPart>(slotName);


                            Log.Enqueue(Log.EntryTypes.Equip, sender, parent, slot.Object);
                            throw new NotImplementedException();
                            //sender.PostMessage(Message.Types.Equip, parent, parent.ToSlot(), true);
                            return true;

                            
                        }
                    //}
                    return false;
              //  case Message.Types.Unequip:
              ////      UI.NotificationArea.Write("Unequipped: " + parent.GetInfo().GetProperty<string>("Name"));
              //      inv = sender.GetComponent<InventoryComponent>("Inventory");
              //      slot = inv.GetProperty<Dictionary<string, GameObjectSlot>>("Equipment")[GetProperty<string>("Slot")];
              //      slotName = GetProperty<string>("Slot");
              //      body = sender.GetComponent<BodyComponent>("Equipment");
              //      bodyPart = body.GetProperty<BodyPart>(slotName);
              //      //if (inv.TryGive(slot.Object))

              //      if (parent == bodyPart.Base.Object)
              //          return false;
              //      if (inv.TryGive(bodyPart.Wearing.Object))
              //      {
              //          //slot.Object = null;
              //          bodyPart.Wearing.Object = null;
              //          Unequip(sender, parent);
              //          if (bodyPart.Base != null)
              //              Equip(sender, bodyPart.Base.Object);
              //          Log.Enqueue(Log.EntryTypes.Unequip, sender, parent, slot.Object);
              //      }
              //      return false;
                default:
                    return false;
            }
        }
    }
}
