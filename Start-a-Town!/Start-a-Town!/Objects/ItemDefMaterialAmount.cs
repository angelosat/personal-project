using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class ItemDefMaterialAmount : ISerializable, ISaveable
    {
        public ItemDef Def;
        public Material Material;
        public int Amount;
        public ItemDefMaterialAmount()
        {

        }
        public ItemDefMaterialAmount(ItemDef def, Material material, int amount)
        {
            Def = def;
            Material = material;
            this.Amount = amount;
        }

        public ItemDefMaterialAmount(BinaryReader r)
        {
            this.Read(r);
        }
        public ItemDefMaterialAmount(SaveTag s)
        {
            this.Load(s);
        }

        internal Entity Create()
        {
            return ItemFactory.CreateFrom(this.Def, this.Material).SetStackSize(this.Amount) as Entity;
        }

        public override string ToString()
        {
            return GetText(this.Def, this.Material, this.Amount);
            //return string.Format("{0}x {1} {2}", this.Amount, this.Material.Label, this.Def.Label); // TODO add a method to itemdefs that return the final name of the item depending on materials etc
        }
        static public string GetText(ItemDef def, Material material, int amount)
        {
            return string.Format("{0}x {1} {2}", amount, material.Label, def.Label); // TODO add a method to itemdefs that return the final name of the item depending on materials etc
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.Def.Name.Save("DefName"));
            tag.Add(this.Material.ID.Save("MaterialID"));
            tag.Add(this.Amount.Save("Amount"));
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            this.Def = tag.LoadDef<ItemDef>("DefName");
            this.Material = Material.GetMaterial(tag.GetValue<int>("MaterialID"));
            this.Amount = tag.GetValue<int>("Amount");
            return this;
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.Def.Name);
            w.Write(this.Material.ID);
            w.Write(this.Amount);
        }
        public ISerializable Read(BinaryReader r)
        {
            this.Def = Start_a_Town_.Def.GetDef<ItemDef>(r.ReadString());
            this.Material = Material.GetMaterial(r.ReadInt32());
            this.Amount = r.ReadInt32();
            return this;
        }
    }
}
