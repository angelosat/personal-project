using System.IO;

namespace Start_a_Town_.Components.Crafting
{
    public class ProductMaterialPair : Inspectable
    {
        public Block Block;

        public byte Data;

        public int Orientation;

        public ToolUseDef Skill;

        public ItemMaterialAmount Requirement;

        public ProductMaterialPair(Block block, ItemMaterialAmount itemMaterial)
        {
            this.Block = block;
            this.Requirement = itemMaterial;
        }

        public ProductMaterialPair(BinaryReader r)
        {
            this.Block = r.ReadBlock();
            this.Data = r.ReadByte();
            this.Requirement = new ItemMaterialAmount(r);
        }

        public ProductMaterialPair(SaveTag tag)
        {
            this.Block = tag.LoadBlock("Product");
            this.Data = tag.TagValueOrDefault<byte>("Data", 0);
            this.Requirement = new ItemMaterialAmount(tag["Requirement"]);
        }

        internal MaterialDef Material => this.Requirement.Material;

        public override string ToString() => $"Type: {this.Block.Label}\nData: {this.Data}";

        //public override string Label => $"{this.Requirement.Material.Label} {this.Requirement.Item.Label} {0} / {this.Requirement.Amount}";
        public override string Label => this.Requirement.Label;// $"{this.Requirement.Amount}x {this.Requirement.Material.Label} {this.Requirement.Item.Label}";

        public string GetName() => this.Requirement.ToString();

        public ToolUseDef GetSkill()
        {
            return this.Skill;
        }

        internal void Save(SaveTag tag, string name)
        {
            var save = new SaveTag(SaveTag.Types.Compound, name);
            save.Save(this.Block, "Product");
            this.Data.Save(save, "Data");
            this.Requirement.Save(save, "Requirement");
            tag.Add(save);
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Block);
            w.Write(this.Data);
            this.Requirement.Write(w);
        }
    }
}
