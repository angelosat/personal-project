using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    record ItemFilter : ISaveable, ISerializable, ISyncable
    {
        public ItemDef Item { get; private set; }
        public bool Enabled;
        readonly HashSet<MaterialDef> DisallowedMaterials = new();
        public ItemFilter()
        {

        }
        public ItemFilter(ItemDef item, bool enabled = true)
        {
            this.Item = item;
            this.Enabled = enabled;
        }
        public bool IsAllowed(MaterialDef mat)
        {
            return this.Enabled && !this.DisallowedMaterials.Contains(mat);
        }
        internal ItemFilter SetAllow(MaterialDef m, bool allow)
        {
            if (allow)
                this.DisallowedMaterials.Remove(m);
            else
                this.DisallowedMaterials.Add(m);
            return this;
        }

        internal void Toggle(MaterialDef mat = null)
        {
            if (mat == null)
                this.Enabled = !this.Enabled;
            else
            {
                if (this.DisallowedMaterials.Contains(mat))
                    this.DisallowedMaterials.Remove(mat);
                else
                    this.DisallowedMaterials.Add(mat);
            }
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Item.Save(tag, "Item");
            this.DisallowedMaterials.SaveDefs(tag, "Materials");
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            this.Item = tag.LoadDef<ItemDef>("Item");
            this.DisallowedMaterials.TryLoadDefs(tag, "Materials");
            return this;
        }

        public void Write(BinaryWriter w)
        {
            this.Item.Write(w);
            this.Sync(w);
        }

        public ISerializable Read(BinaryReader r)
        {
            this.Item = Def.GetDef<ItemDef>(r.ReadString());
            this.Sync(r);
            return this;
        }

        public ISyncable Sync(BinaryWriter w)
        {
            this.DisallowedMaterials.WriteDefs(w);
            return this;
        }

        public ISyncable Sync(BinaryReader r)
        {
            this.DisallowedMaterials.ReadDefs(r);
            return this;
        }
    }
}
