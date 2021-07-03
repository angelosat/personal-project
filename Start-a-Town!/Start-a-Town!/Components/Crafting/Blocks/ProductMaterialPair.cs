using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Modules.Construction;

namespace Start_a_Town_.Components.Crafting
{
    partial class BlockRecipe
    {
        public class ProductMaterialPair : IConstructionProduct
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
            public ItemRequirement Req;
            public ToolAbilityDef Skill;
            public Material MainMaterial;
            public ItemDefMaterialAmount Requirement;
            public BlockRecipe Recipe { get { return this.Block.Recipe; } }

            public List<ItemRequirement> GetReq()
            {
                return new List<ItemRequirement>() { this.Req };
            }
            public void SpawnProduct(IMap map, Vector3 global)
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
            [Obsolete]
            public ProductMaterialPair(Block block, byte data, ItemRequirement req)
            {
                throw new Exception();
                this.Data = data;
                this.Block = block;
                this.Req = req;
            }
            [Obsolete]
            public ProductMaterialPair AddMaterial(int objectID, int amount)
            {
                throw new Exception();
            }
            [Obsolete]
            public ProductMaterialPair(Block block, Material material)
            {
                throw new Exception();
                this.Block = block;
                this.MainMaterial = material;
                this.Data = block.GetDataFromMaterial(material);
            }
            public ProductMaterialPair(Block block, ItemDefMaterialAmount itemMaterial)
            {
                this.Block = block;
                this.MainMaterial = itemMaterial.Material;
                this.Data = block.GetDataFromMaterial(this.MainMaterial);
                this.Requirement = itemMaterial;
            }
            public void Write(BinaryWriter w)
            {
                w.Write((int)this.Block.Type);
                w.Write(this.Data);
                w.Write(this.MainMaterial.ID);
                this.Requirement.Write(w);
            }
            public ProductMaterialPair(BinaryReader r)
            {
                this.Block = Block.Registry[(Block.Types)r.ReadInt32()];
                this.Data = r.ReadByte();
                this.MainMaterial = Material.GetMaterial(r.ReadInt32());
                this.Requirement = new ItemDefMaterialAmount(r);
            }
            public List<SaveTag> Save()
            {
                List<SaveTag> save = new List<SaveTag>();
                save.Add(new SaveTag(SaveTag.Types.Int, "Product", (int)this.Block.Type));
                save.Add(new SaveTag(SaveTag.Types.Byte, "Data", this.Data));
                save.Add(this.MainMaterial.ID.Save("MainMaterial"));
                save.Add(this.Requirement.Save("Requirement"));
                return save;
            }
            public ProductMaterialPair(SaveTag tag)
            {
                this.Block = Block.Registry[(Block.Types)tag.GetValue<int>("Product")];
                this.Data = tag.TagValueOrDefault<byte>("Data", 0);
                tag.TryGetTagValue<int>("MainMaterial", v => this.MainMaterial = Material.GetMaterial(v));
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
        }
    }
}
