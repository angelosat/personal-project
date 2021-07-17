using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Crafting
{
    public class ProductMaterialPair
    {
        public override string ToString()
        {
            return "Type: " + this.Block.Type.ToString() + "\nData: " + this.Data.ToString();
        }
        public string GetName()
        {
            return this.Requirement.ToString();
        }
        public Block Block;
        public byte Data;
        public int Orientation;
        public ToolAbilityDef Skill;
        public ItemDefMaterialAmount Requirement;

        public void SpawnProduct(MapBase map, Vector3 global)
        {
            // TODO: WARNING: check if target cell is still empty !!!
            map.GetBlock(global).Remove(map, global);
            var block = Block.Registry[this.Block.Type];
            var orientation = 0; // TODO: input orientation
            block.Place(map, global, this.Data, 0, orientation);
        }
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
            var save = new List<SaveTag>();
            save.Add(new SaveTag(SaveTag.Types.Int, "Product", (int)this.Block.Type));
            save.Add(new SaveTag(SaveTag.Types.Byte, "Data", this.Data));
            save.Add(this.Requirement.Save("Requirement"));
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
