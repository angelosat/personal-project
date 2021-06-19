using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.GameModes;
using Start_a_Town_.Components.Skills;

namespace Start_a_Town_.Components.Crafting
{
    partial class BlockRecipe// : ISlottable
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
            public int Orientation; //new
            public ItemRequirement Req;
            public ToolAbilityDef Skill;
            //public List<ObjectIDAmount> Materials = new List<ObjectIDAmount>();
            public Material MainMaterial;
            public ItemDefMaterialAmount Requirement;
            public BlockRecipe Recipe { get { return this.Block.Recipe; } }

            //public ItemRequirement GetReq()
            public List<ItemRequirement> GetReq()
            {
                //return this.Req;
                return new List<ItemRequirement>() { this.Req };
            }
            public void SpawnProduct(IMap map, Vector3 global)
            {
                //map.GetNetwork().SyncSetBlock(global, this.Product.Type, this.Data);
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

            public ProductMaterialPair(Block block, byte data, ItemRequirement req)
            {
                throw new Exception();
                this.Data = data;
                this.Block = block;
                this.Req = req;
            }

            public ProductMaterialPair AddMaterial(int objectID, int amount)
            {
                throw new Exception();
                //this.Materials.Add(new ObjectIDAmount(objectID, amount));
                //return this;
            }
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
                //this.Req.Write(w);
                //w.Write(this.Skill == null ? 0 : this.Skill.ID);
                //this.Materials.Write(w);
                w.Write(this.MainMaterial.ID);
                this.Requirement.Write(w);
            }
            public ProductMaterialPair(BinaryReader r)
            {
                this.Block = Block.Registry[(Block.Types)r.ReadInt32()];
                this.Data = r.ReadByte();
                //this.Req = new ItemRequirement(r);
                //var skillid = r.ReadInt32();
                //if (skillid > 0)
                //    this.Skill = ToolAbilityDef.Dictionary[skillid];
                //this.Materials.ReadMutable(r);
                this.MainMaterial = Material.GetMaterial(r.ReadInt32());
                this.Requirement = new ItemDefMaterialAmount(r);
            }
            public List<SaveTag> Save()
            {
                List<SaveTag> save = new List<SaveTag>();
                save.Add(new SaveTag(SaveTag.Types.Int, "Product", (int)this.Block.Type));
                save.Add(new SaveTag(SaveTag.Types.Byte, "Data", this.Data));
                //save.Add(new SaveTag(SaveTag.Types.Compound, "Material", this.Req.Save()));
                //save.Add(this.Materials.SaveNewBEST("Materials"));
                save.Add(this.MainMaterial.ID.Save("MainMaterial"));
                save.Add(this.Requirement.Save("Requirement"));
                return save;
            }
            public ProductMaterialPair(SaveTag tag)
            {
                this.Block = Block.Registry[(Block.Types)tag.GetValue<int>("Product")];
                this.Data = tag.TagValueOrDefault<byte>("Data", 0);
                //this.Req = new ItemRequirement(tag["Material"]);
                //this.Materials.TryLoadMutable(tag, "Materials");
                tag.TryGetTagValue<int>("MainMaterial", v => this.MainMaterial = Material.GetMaterial(v));
                this.Requirement = new ItemDefMaterialAmount(tag["Requirement"]);
            }

            
            internal int GetMaterialRequirement(ItemDef def)
            {
                return this.Block.Ingredient.Amount;
                //return this.Materials.Find(r => r.ObjectID == objid).Amount;
            }

            internal void Save(SaveTag tag, string name)
            {
                tag.Add(new SaveTag(SaveTag.Types.Compound, name, this.Save()));
            }
            //internal int GetMaterialRequirement(int objid)
            //{
            //    return this.Materials.Find(r => r.ObjectID == objid).Amount;
            //}
        }

    }
}
