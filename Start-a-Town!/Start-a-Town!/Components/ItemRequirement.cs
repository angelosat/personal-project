using System.Collections.Generic;
using System.IO;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.Components
{
    [Obsolete]
    public class ItemRequirement
    {
        public string Name = "";
        public int ObjectID;
        public int AmountCurrent, AmountRequired;

        public override string ToString()
        {
            return string.Format("{0} x{1}/{2}", GameObject.Objects[this.ObjectID], this.AmountCurrent, this.AmountRequired);
        }

        public int Remaining
        {
            get { return this.AmountRequired - this.AmountCurrent; }
            set { this.AmountCurrent = this.AmountRequired - value; }
        }
        public ItemRequirement(int id, int max, int amount = 0)
        {
            this.ObjectID = id;
            this.AmountRequired = max;
            this.AmountCurrent = amount;
        }
        public ItemRequirement(GameObject.Types id, int max, int amount = 0)
        {
            this.ObjectID = (int)id;
            this.AmountRequired = max;
            this.AmountCurrent = amount;
        }
        public ItemRequirement(ItemRequirement toCopy)
        {
            this.ObjectID = toCopy.ObjectID;
            this.AmountRequired = toCopy.AmountRequired;
            this.AmountCurrent = toCopy.AmountCurrent;
        }
        public ItemRequirement(SaveTag tag)
        {
            this.Name = tag.GetValue<string>("Name");
            this.ObjectID = tag.GetValue<int>("Material");
            this.AmountRequired = tag.GetValue<int>("Max");
            this.AmountCurrent = tag.GetValue<int>("Amount");
        }
        public SaveTag Save(string name)
        {
            var tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Material", this.ObjectID));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Amount", this.AmountCurrent));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Max", this.AmountRequired));
            var savetag = new SaveTag(SaveTag.Types.Compound, name, tag);
            return savetag;
        }
        public List<SaveTag> Save()
        {
            List<SaveTag> tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Material", this.ObjectID));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Amount", this.AmountCurrent));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Max", this.AmountRequired));
            return tag;
        }
        public ItemRequirement(BinaryReader r)
        {
            this.Name = r.ReadString();
            this.ObjectID = r.ReadInt32();
            this.AmountRequired = r.ReadInt32();
            this.AmountCurrent = r.ReadInt32();
            this.Name = r.ReadString();
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.Name);
            w.Write((int)this.ObjectID);
            w.Write(this.AmountRequired);
            w.Write(this.AmountCurrent);
            w.Write(this.Name);
        }

        public bool Full { get { return this.AmountCurrent == this.AmountRequired; } }

        public SlotWithText GetUI(Vector2 loc)
        {
            SlotWithText slotReq = new SlotWithText(loc);
            slotReq.Tag = new GameObjectSlot() { Link = GameObject.Objects[this.ObjectID] };
            slotReq.Slot.CornerTextFunc = o => this.AmountCurrent.ToString() + "/" + this.AmountRequired.ToString();
            return slotReq;
        }

        internal GameObject GetObject()
        {
            return GameObject.Objects[this.ObjectID];
        }
    }
}
