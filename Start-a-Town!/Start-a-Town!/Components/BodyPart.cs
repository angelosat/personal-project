using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class BodyPart// : GameObjectSlot
    {
        public GameObjectSlot Base;
        public GameObjectSlot Wearing;

        //ItemContainer Slots;
        //public GameObjectSlot Base { get { return Slots[0]; } }
        //public GameObjectSlot Wearing { get { return Slots[1]; } }

        public BodyPart(GameObject bodyPart = null, GameObject wearing = null)
        {
            //this.Slots = new ItemContainer()
            Base = new GameObjectSlot(bodyPart);
            Wearing = new GameObjectSlot(wearing);
        }

        public override string ToString()
        {
            return "Base: " + Base.ToString() +
                "\nWearing: " + Wearing.ToString();
        }

        public virtual GameObject Object
        {
            get
            {
                return Wearing.Object ?? Base.Object;
            }
        }

        public void GetStats(StatCollection stats)
        {
            if (Wearing.HasValue)
                EquipComponent.GetStats(Wearing.Object, stats);
            if (Base.HasValue) 
                EquipComponent.GetStats(Base.Object, stats);
        }

        public List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();
            if (Base.Object != null)
                data.Add(new SaveTag(SaveTag.Types.Compound, "Base", Base.Save()));
            if (Wearing.Object != null)
                data.Add(new SaveTag(SaveTag.Types.Compound, "Wearing", Wearing.Save()));

            //Tag data = new List<Tag>();
            //List<Tag> data = new List<Tag>();// Tag(Tag.Types.Compound, "BodyPart");
            //data.Add(new Tag(Tag.Types.Compound, "Base"));
            //data.Add(new Tag(Tag.Types.Compound, "Wearing"));
            //if (Base.Object != null)
            //    data["Base"].Add(Base.Object.Save());
            //if (Wearing.Object != null)
            //    data["Wearing"].Add(Wearing.Object.Save());
            return data;
        }

        static public BodyPart Load(SaveTag data)
        {
            BodyPart part = new BodyPart();
            //if (data.ToDictionary().ContainsKey("Base"))
            //    part.Base = GameObjectSlot.Create(data["Base"]);
            //if (data.ToDictionary().ContainsKey("Wearing"))
            //    part.Wearing = GameObjectSlot.Create(data["Wearing"]);
            if ((data.Value as Dictionary<string, SaveTag>).ContainsKey("Base"))
                part.Base = GameObjectSlot.Create(data["Base"]);
            if ((data.Value as Dictionary<string, SaveTag>).ContainsKey("Wearing"))
                part.Wearing = GameObjectSlot.Create(data["Wearing"]);
            return part;

            //this.Value as Dictionary<string, SaveTag>


            //BodyPart part = new BodyPart();
            //if (data["Base"].ToDictionary().ContainsKey("Object"))
            //    if (data["Base"]["Object"].Value != null)
            //        part.Base = new GameObjectSlot(GameObject.Create(data["Base"]["Object"]));
            //if (data["Wearing"].ToDictionary().ContainsKey("Object"))
            //    if (data["Wearing"]["Object"].Value != null)
            //        part.Base = new GameObjectSlot(GameObject.Create(data["Wearing"]["Object"]));
            //return part;
        }
    }
}
