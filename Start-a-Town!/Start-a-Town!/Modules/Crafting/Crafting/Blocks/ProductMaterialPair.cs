using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_.Components.Crafting
{
    public class ProductMaterialPair
    {
        public override string ToString() => "Type: " + this.Block.Type.ToString() + "\nData: " + this.Data.ToString();
        public string GetName() => this.Requirement.ToString();
        public Block Block;
        public byte Data;
        public int Orientation;
        public ToolAbilityDef Skill;
        public ItemDefMaterialAmount Requirement;

        public ToolAbilityDef GetSkill()
        {
            return this.Skill;
        }

        public ProductMaterialPair(Block block, ItemDefMaterialAmount itemMaterial)
        {
            this.Block = block;
            this.Requirement = itemMaterial;
            this.Data = block.GetDataFromMaterial(this.Requirement.Material);
        }
        public ProductMaterialPair(BinaryReader r)
        {
            this.Block = Block.Registry[(Block.Types)r.ReadInt32()];
            this.Data = r.ReadByte();
            this.Requirement = new ItemDefMaterialAmount(r);
        }
        public ProductMaterialPair(SaveTag tag)
        {
            this.Block = Block.Registry[(Block.Types)tag.GetValue<int>("Product")];
            this.Data = tag.TagValueOrDefault<byte>("Data", 0);
            this.Requirement = new ItemDefMaterialAmount(tag["Requirement"]);
        }

        internal int GetMaterialRequirement(ItemDef def)
        {
            return this.Block.Ingredient.Amount;
        }

        internal void Save(SaveTag tag, string name)
        {
            tag.Add(new SaveTag(SaveTag.Types.Compound, name, this.Save()));
        }
        public List<SaveTag> Save()
        {
            var save = new List<SaveTag>
            {
                new SaveTag(SaveTag.Types.Int, "Product", (int)this.Block.Type),
                new SaveTag(SaveTag.Types.Byte, "Data", this.Data),
                this.Requirement.Save("Requirement")
            };
            return save;
        }

        public void Write(BinaryWriter w)
        {
            w.Write((int)this.Block.Type);
            w.Write(this.Data);
            this.Requirement.Write(w);
        }
    }
}
