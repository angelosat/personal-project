﻿using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    record ItemFilter : ISaveable, ISerializable, ISyncable
    {
        public ItemDef Item { get; private set; }
        public bool Enabled = true;
        readonly HashSet<MaterialDef> DisallowedMaterials = new(); // TODO make materials implement iitemdefvariator too?
        readonly HashSet<Def> DisallowedVariations = new();
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
        public bool IsAllowed(Def v)
        {
            return this.Enabled && !this.DisallowedVariations.Contains(v);
        }
        internal void Toggle()
        {
            this.Enabled = !this.Enabled;
        }
        internal void Toggle(MaterialDef mat)
        {
            if (this.DisallowedMaterials.Contains(mat))
                this.DisallowedMaterials.Remove(mat);
            else
                this.DisallowedMaterials.Add(mat);
        }
        internal void Toggle(Def v)
        {
            if (this.DisallowedVariations.Contains(v))
                this.DisallowedVariations.Remove(v);
            else
                this.DisallowedVariations.Add(v);
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Item.Save(tag, "Item");
            this.Enabled.Save(tag, "Enabled");
            this.DisallowedMaterials.SaveDefs(tag, "Materials");
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            this.Item = tag.LoadDef<ItemDef>("Item");
            this.Enabled = (bool)tag["Enabled"].Value;
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
            w.Write(this.Enabled);
            this.DisallowedMaterials.WriteDefs(w);
            return this;
        }

        public ISyncable Sync(BinaryReader r)
        {
            this.Enabled = r.ReadBoolean();
            this.DisallowedMaterials.ReadDefs(r);
            return this;
        }

        public void CopyFrom(ItemFilter toCopy)
        {
            this.Enabled = toCopy.Enabled;
            foreach (var m in toCopy.DisallowedMaterials)
                this.DisallowedMaterials.Add(m);
        }
    }

    //record ItemFilter : ISaveable, ISerializable, ISyncable
    //{
    //    public ItemDef Item { get; private set; }
    //    public bool Enabled = true;
    //    readonly HashSet<MaterialDef> DisallowedMaterials = new();
    //    public ItemFilter()
    //    {

    //    }
    //    public ItemFilter(ItemDef item, bool enabled = true)
    //    {
    //        this.Item = item;
    //        this.Enabled = enabled;
    //    }
    //    public bool IsAllowed(MaterialDef mat)
    //    {
    //        return this.Enabled && !this.DisallowedMaterials.Contains(mat);
    //    }
    //    internal ItemFilter SetAllow(MaterialDef m, bool allow)
    //    {
    //        if (allow)
    //            this.DisallowedMaterials.Remove(m);
    //        else
    //            this.DisallowedMaterials.Add(m);
    //        return this;
    //    }

    //    internal void Toggle(MaterialDef mat = null)
    //    {
    //        if (mat == null)
    //            this.Enabled = !this.Enabled;
    //        else
    //        {
    //            if (this.DisallowedMaterials.Contains(mat))
    //                this.DisallowedMaterials.Remove(mat);
    //            else
    //                this.DisallowedMaterials.Add(mat);
    //        }
    //    }

    //    public SaveTag Save(string name = "")
    //    {
    //        var tag = new SaveTag(SaveTag.Types.Compound, name);
    //        this.Item.Save(tag, "Item");
    //        this.Enabled.Save(tag, "Enabled");
    //        this.DisallowedMaterials.SaveDefs(tag, "Materials");
    //        return tag;
    //    }

    //    public ISaveable Load(SaveTag tag)
    //    {
    //        this.Item = tag.LoadDef<ItemDef>("Item");
    //        this.Enabled = (bool)tag["Enabled"].Value;
    //        this.DisallowedMaterials.TryLoadDefs(tag, "Materials");
    //        return this;
    //    }

    //    public void Write(BinaryWriter w)
    //    {
    //        this.Item.Write(w);
    //        this.Sync(w);
    //    }

    //    public ISerializable Read(BinaryReader r)
    //    {
    //        this.Item = Def.GetDef<ItemDef>(r.ReadString());
    //        this.Sync(r);
    //        return this;
    //    }

    //    public ISyncable Sync(BinaryWriter w)
    //    {
    //        w.Write(this.Enabled);
    //        this.DisallowedMaterials.WriteDefs(w);
    //        return this;
    //    }

    //    public ISyncable Sync(BinaryReader r)
    //    {
    //        this.Enabled = r.ReadBoolean();
    //        this.DisallowedMaterials.ReadDefs(r);
    //        return this;
    //    }

    //    public void CopyFrom(ItemFilter toCopy)
    //    {
    //        this.Enabled = toCopy.Enabled;
    //        foreach (var m in toCopy.DisallowedMaterials)
    //            this.DisallowedMaterials.Add(m);
    //    }
    //}
}
