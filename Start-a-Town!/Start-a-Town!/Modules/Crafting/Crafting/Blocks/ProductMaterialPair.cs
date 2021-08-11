using System.IO;

namespace Start_a_Town_.Components.Crafting
{
    public class ProductMaterialPair
    {
        public override string ToString() => "Type: " + this.Block.Label + "\nData: " + this.Data.ToString();
        public string GetName() => this.Requirement.ToString();
        public Block Block;
        public byte Data;
        public int Orientation;
        public ToolUseDef Skill;
        public ItemMaterialAmount Requirement;
        internal MaterialDef Material => this.Requirement.Material;// MaterialDefOf.Air;

        public ToolUseDef GetSkill()
        {
            return this.Skill;
        }

        public ProductMaterialPair(Block block, ItemMaterialAmount itemMaterial)
        {
            this.Block = block;
            this.Requirement = itemMaterial;
            //this.Data = block.GetDataFromMaterial(this.Requirement.Material);
        }
        public ProductMaterialPair(BinaryReader r)
        {
            this.Block = r.ReadBlock();
            this.Data = r.ReadByte();
            this.Requirement = new ItemMaterialAmount(r);
        }
        public ProductMaterialPair(SaveTag tag)
        {
            this.Block = tag.LoadBlock("Product");// Block.Registry[(Block.Types)tag.GetValue<int>("Product")];
            this.Data = tag.TagValueOrDefault<byte>("Data", 0);
            this.Requirement = new ItemMaterialAmount(tag["Requirement"]);
        }

        internal int GetMaterialRequirement(ItemDef def)
        {
            return this.Block.Ingredient.Amount;
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
