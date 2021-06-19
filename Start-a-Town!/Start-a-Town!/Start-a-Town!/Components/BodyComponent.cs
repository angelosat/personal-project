using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class BodyComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Body";
            }
        }

        //public List<BodyPart> BodyParts { get { return (List<BodyPart>)this["Bodyparts"]; } set { this["Bodyparts"] = value; } }


        public Dictionary<string, BodyPart> BodyParts { get { return (Dictionary<string, BodyPart>)this["Bodyparts"]; } set { this["Bodyparts"] = value; } }

        static public BodyComponent Actor
        {
            get
            {
                BodyComponent equipment = new BodyComponent();
                //equipment.BodyParts.AddRange(new BodyPart[]{
                //    new BodyPart(Stat.Mainhand.Name, GameObjectDb.Fists),
                //    new BodyPart(Stat.Offhand.Name),
                //    new BodyPart(Stat.Hands.Name, GameObjectDb.BareHands),
                //    new BodyPart(Stat.Chest.Name),
                //    new BodyPart(Stat.Feet.Name, GameObjectDb.BareFeet)
                //});

                equipment.BodyParts[Stat.Mainhand.Name] = new BodyPart(GameObjectDb.Fists);
                equipment.BodyParts[Stat.Offhand.Name] = new BodyPart();
                equipment.BodyParts[Stat.Hands.Name] = new BodyPart(GameObjectDb.BareHands);
                equipment.BodyParts[Stat.Chest.Name] = new BodyPart();
                equipment.BodyParts[Stat.Feet.Name] = new BodyPart(GameObjectDb.BareFeet);
                equipment.BodyParts["Head"] = new BodyPart();
                //equipment.BodyParts[Stat.Mainhand.Name] = new BodyPart(GameObject.Objects[GameObject.Types.Fists].Clone());
                //equipment.BodyParts[Stat.Offhand.Name] = new BodyPart();
                //equipment.BodyParts[Stat.Hands.Name] = new BodyPart(GameObject.Objects[GameObject.Types.BareHands].Clone());
                //equipment.BodyParts[Stat.Chest.Name] = new BodyPart();
                //equipment.BodyParts[Stat.Feet.Name] = new BodyPart(GameObject.Objects[GameObject.Types.BareFeet].Clone());
                //equipment.BodyParts["Head"] = new BodyPart();

                //equipment[Stat.Mainhand.Name] = new BodyPart(GameObjectDb.Fists);
                //equipment[Stat.Offhand.Name] = new BodyPart();
                //equipment[Stat.Hands.Name] = new BodyPart(GameObjectDb.BareHands);
                //equipment[Stat.Chest.Name] = new BodyPart();
                //equipment[Stat.Feet.Name] = new BodyPart(GameObjectDb.BareFeet);
                return equipment;
            }
        }
        static public BodyComponent Zombie
        {
            get
            {
                BodyComponent equipment = new BodyComponent();
                //equipment.BodyParts.AddRange(new BodyPart[]{
                //    new BodyPart(GameObjectDb.Fists),
                //    new BodyPart(),
                //    new BodyPart(GameObjectDb.BareHands),
                //    new BodyPart(),
                //    new BodyPart(GameObjectDb.RottenFeet)
                //});
                equipment.BodyParts[Stat.Mainhand.Name] = new BodyPart(GameObjectDb.Fists);
                equipment.BodyParts[Stat.Offhand.Name] = new BodyPart();
                equipment.BodyParts[Stat.Hands.Name] = new BodyPart(GameObjectDb.BareHands);
                equipment.BodyParts[Stat.Chest.Name] = new BodyPart();
                equipment.BodyParts[Stat.Feet.Name] = new BodyPart(GameObjectDb.RottenFeet);

                //equipment[Stat.Mainhand.Name] = new BodyPart(GameObjectDb.Fists);
                //equipment[Stat.Offhand.Name] = new BodyPart();
                //equipment[Stat.Hands.Name] = new BodyPart(GameObjectDb.BareHands);
                //equipment[Stat.Chest.Name] = new BodyPart();
                //equipment[Stat.Feet.Name] = new BodyPart(GameObjectDb.RottenFeet);
                return equipment;
            }
        }

        public BodyComponent()
        {
            this.BodyParts = new Dictionary<string, BodyPart>();
        }

        public BodyComponent Initialize(BodyComponent template)
        {
            //foreach (var p in template.Properties)
            //    this[p.Key] = p.Value;
            this.BodyParts = template.BodyParts;
            return this;
        }

        public override object Clone()
        {
            BodyComponent comp = new BodyComponent();

            //foreach (KeyValuePair<string, BodyPart> parameter in Properties.ToDictionary(foo => foo.Key, foo => foo.Value as BodyPart))
            //{
            //    comp[parameter.Key] = new BodyPart(
            //        parameter.Value.Base.Object != null ? GameObject.Create(parameter.Value.Base.Object.ID) : null,
            //        parameter.Value.Wearing.Object != null ? GameObject.Create(parameter.Value.Wearing.Object.ID) : null);
            //}

            foreach (var bodypart in this.BodyParts)
            {
                comp.BodyParts[bodypart.Key] = new BodyPart(
                    bodypart.Value.Base.Object != null ? GameObject.Create(bodypart.Value.Base.Object.ID) : null,
                    bodypart.Value.Wearing.Object != null ? GameObject.Create(bodypart.Value.Wearing.Object.ID) : null);
            }

            //foreach (var bodypart in this.BodyParts)
            //{
            //    comp.BodyParts.Add(new BodyPart(
            //        bodypart.Base.Object != null ? GameObject.Create(bodypart.Base.Object.ID) : null,
            //        bodypart.Wearing.Object != null ? GameObject.Create(bodypart.Wearing.Object.ID) : null));
            //}
            return comp;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.ChunkLoaded:
                    Initialize(parent);
                    return true;

               

                case Message.Types.Equip:
                    //GameObject obj = e.Parameters[0] as GameObject;
                    //string slotName = (string)obj["Equip"]["Slot"];
                    //BodyPart thisSlot = (BodyPart)this[slotName];

                    //StatsComponent parentStats;
                    //if (parent.TryGetComponent<StatsComponent>("Stats", out parentStats))
                    //{
                    //    if (thisSlot.Wearing.HasValue)
                    //        foreach (KeyValuePair<string, float> stat in thisSlot.Wearing.Object["Stats"].Properties.ToDictionary(foo => foo.Key, foo => (float)foo.Value))
                    //            parentStats[stat.Key] = parentStats.GetPropertyOrDefault<float>(stat.Key, 0f) - stat.Value;

                    //        foreach (KeyValuePair<string, float> stat in obj.Object["Stats"].Properties.ToDictionary(foo => foo.Key, foo => (float)foo.Value))
                    //            parentStats[stat.Key] = parentStats.GetPropertyOrDefault<float>(stat.Key, 0f) + stat.Value;
                    //}
                    //Log.Enqueue(Log.EntryTypes.Equip, parent, obj);
                    //GameObjectSlot.Swap(obj, thisSlot.Wearing);
                    //return true;

                    GameObjectSlot objSlot = e.Parameters[0] as GameObjectSlot;
                    string slotName = (string)objSlot.Object["Equip"]["Slot"];
                    BodyPart thisSlot = (BodyPart)this[slotName];




                    StatsComponent parentStats;
                    if (objSlot.Object.Components.ContainsKey("Stats"))
                        if (parent.TryGetComponent<StatsComponent>("Stats", out parentStats))
                        {
                            if (thisSlot.Wearing.HasValue)
                                foreach (KeyValuePair<string, float> stat in thisSlot.Wearing.Object["Stats"].Properties.ToDictionary(foo => foo.Key, foo => (float)foo.Value))
                                    parentStats[stat.Key] = parentStats.GetPropertyOrDefault<float>(stat.Key, 0f) - stat.Value;

                            if (objSlot.HasValue)
                                foreach (KeyValuePair<string, float> stat in objSlot.Object["Stats"].Properties.ToDictionary(foo => foo.Key, foo => (float)foo.Value))
                                    parentStats[stat.Key] = parentStats.GetPropertyOrDefault<float>(stat.Key, 0f) + stat.Value;
                        }
                    Log.Enqueue(Log.EntryTypes.Equip, parent, objSlot.Object);
                    GameObjectSlot.Swap(objSlot, thisSlot.Wearing);
                    return true;

                //case Message.Types.Unequip:
                //    slotName = (string)e.Parameters[0];
                //    thisSlot = (BodyPart)this[slotName];
                //    if (!thisSlot.Wearing.HasValue)
                //        return true;
                //    if (!parent.GetComponent<InventoryComponent>("Inventory").TryGive(thisSlot.Wearing.Object))
                //        return true;

                //    if (thisSlot.Object.Components.ContainsKey("Stats"))
                //        if (parent.TryGetComponent<StatsComponent>("Stats", out parentStats))
                //            foreach (KeyValuePair<string, float> stat in thisSlot.Wearing.Object["Stats"].Properties.ToDictionary(foo => foo.Key, foo => (float)foo.Value))
                //                parentStats[stat.Key] = parentStats.GetPropertyOrDefault<float>(stat.Key, 0f) - stat.Value;
                //    Log.Enqueue(Log.EntryTypes.Unequip, parent, thisSlot.Wearing.Object);
                //    thisSlot.Wearing.Clear();

                //    return true;

                case Message.Types.Death:
                    //foreach (BodyPart slot in Properties.Values.Select(foo => foo as BodyPart))
                    foreach (BodyPart slot in this.BodyParts.Values)
                        if (slot.Wearing.HasValue)
                            //Loot.PopLoot(parent, slot.Wearing);
                            e.Network.PopLoot(slot.Wearing.Take(), parent.Global, parent.Velocity);//warning
                    return true;

                //case Message.Types.EquipItem:
                //    GameObjectSlot itemSlot = e.Parameters[0] as GameObjectSlot;
                //    BodyComponent.Wear(e.Sender, itemSlot);
                //    return true;

                default:
                    return false;

            }
        }

        static public void Wear(GameObject actor, GameObjectSlot wearable)
        {
            BodyComponent body;
            if (!actor.TryGetComponent<BodyComponent>("Equipment", out body))
                return;
            string slotName = wearable.Object["Equip"].GetProperty<string>("Slot");
            if (!body.Properties.ContainsKey(slotName))
                return;

            BodyPart bodyPart = body.GetProperty<BodyPart>(slotName);
            bodyPart.Wearing.Swap(wearable);
            actor.PostMessage(Message.Types.Refresh);
            //if (bodyPart.Object != null)
            //    Unequip(sender, bodyPart.Object);

            //sender.PostMessage(Message.Types.Equip, parent, slot, true);// parent, true); 

        }

        static public void CollectBonuses(GameObject obj, BonusCollection list)
        {
            obj["Equipment"].Properties.Values.ToList().ForEach(foo => BonusesComponent.GetBonuses(foo as BodyPart, list));
        }
        static public void PollStats(GameObject obj, StatCollection list)
        {
            BodyComponent body;
            if (!obj.TryGetComponent<BodyComponent>("Equipment", out body))
                return;
            body.BodyParts.Values.ToList().ForEach(foo => (foo as BodyPart).GetStats(list));
        }

        public override string ToString()
        {
            string text = "";
            foreach (KeyValuePair<string, object> property in Properties)
                text += "[" + property.Key + ": " + property.Value + "]\n";
                //text += "[" + property.Key + ": " + (property.Value is GameObject ? (property.Value as GameObject).Name : property.Value) + "]\n";
         //   if (text.Length > 0)
         //       text = text.Remove(text.Length - 1);
            return text;
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();
            foreach (var bodypart in this.BodyParts)
            {
                data.Add(new SaveTag(SaveTag.Types.Compound, bodypart.Key, (bodypart.Value as BodyPart).Save()));
            }
            return data;
        }

        internal override void Load(SaveTag compTag)
        {
            //foreach (SaveTag tag in compTag.Value as List<SaveTag>)
            foreach (SaveTag tag in (compTag.Value as Dictionary<string, SaveTag>).Values)
                if (tag.Value != null)
                    this.BodyParts[tag.Name] = BodyPart.Load(tag);

            //foreach (SaveTag tag in compTag.Value as List<SaveTag>)
            //    if (tag.Value != null)
            //        this.Properties[tag.Name] = BodyPart.Load(tag);
        }
    }
}
