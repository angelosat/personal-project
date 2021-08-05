using Start_a_Town_.UI;
using System.IO;

namespace Start_a_Town_
{
    public class ItemDefMaterialAmount : ISerializable, ISaveable, IListable
    {
        public ItemDef Def;
        public MaterialDef Material;
        public int Amount;

        public string Label => throw new System.NotImplementedException();

        public ItemDefMaterialAmount()
        {

        }
        public ItemDefMaterialAmount(ItemDef def, MaterialDef material, int amount)
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
            return this.GetText();// GetText(this.Def, this.Material, this.Amount);
        }
        static public string GetText(ItemDef def, MaterialDef material, int amount)
        {
            return $"{amount}x {material.Label} {def.Label}"; // TODO add a method to itemdefs that return the final name of the item depending on materials etc
        }
        public string GetText()
        {
            return $"{this.Amount}x {this.Material.Label} {this.Def.Label}"; // TODO add a method to itemdefs that return the final name of the item depending on materials etc
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
            this.Material = MaterialDef.GetMaterial(tag.GetValue<int>("MaterialID"));
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
            this.Material = MaterialDef.GetMaterial(r.ReadInt32());
            this.Amount = r.ReadInt32();
            return this;
        }

        public Control GetListControlGui()
        {
            return new Label(this.GetText);
        }
    }
}
