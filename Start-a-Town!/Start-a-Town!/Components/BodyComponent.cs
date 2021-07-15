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

        public Dictionary<string, BodyPart> BodyParts;
       
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
                case Message.Types.Death:
                    foreach (BodyPart slot in this.BodyParts.Values)
                        if (slot.Wearing.HasValue)
                            e.Network.PopLoot(slot.Wearing.Take(), parent.Global, parent.Velocity);
                    return true;

                default:
                    return false;

            }
        }

        static public void PollStats(GameObject obj, StatCollection list)
        {
            BodyComponent body;
            if (!obj.TryGetComponent<BodyComponent>("Equipment", out body))
                return;
            body.BodyParts.Values.ToList().ForEach(foo => (foo as BodyPart).GetStats(list));
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
