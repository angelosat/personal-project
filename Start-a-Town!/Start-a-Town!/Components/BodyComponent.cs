using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Components
{
    [Obsolete]
    class BodyComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Body";
            }
        }

        public Dictionary<string, BodyPart> BodyParts { get { return (Dictionary<string, BodyPart>)this["Bodyparts"]; } set { this["Bodyparts"] = value; } }
       
        public BodyComponent()
        {
            this.BodyParts = new Dictionary<string, BodyPart>();
        }

        public BodyComponent Initialize(BodyComponent template)
        {
            this.BodyParts = template.BodyParts;
            return this;
        }

        public override object Clone()
        {
            BodyComponent comp = new BodyComponent();
            return comp;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.ChunkLoaded:
                    Initialize(parent);
                    return true;

                case Message.Types.Equip:
                    
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

                case Message.Types.Death:
                    foreach (BodyPart slot in this.BodyParts.Values)
                        if (slot.Wearing.HasValue)
                            e.Network.PopLoot(slot.Wearing.Take(), parent.Global, parent.Velocity);
                    return true;

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
            foreach (SaveTag tag in (compTag.Value as Dictionary<string, SaveTag>).Values)
                if (tag.Value != null)
                    this.BodyParts[tag.Name] = BodyPart.Load(tag);
        }
    }
}
