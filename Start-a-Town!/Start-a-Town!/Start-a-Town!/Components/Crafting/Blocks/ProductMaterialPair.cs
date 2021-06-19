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
    partial class BlockConstruction
    {
        public class ProductMaterialPair : IConstructionProduct
        {
            public override string ToString()
            {
                return "Type: " + this.Product.Type.ToString() + "\nData: " + this.Data.ToString();
            }

            public Block Product;
            public byte Data;
            public ItemRequirement Req;
            public Skill Skill;

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
                var block = Block.Registry[this.Product.Type];
                var orientation = 0; // TODO: input orientation
                block.Place(map, global, this.Data, 0, orientation);
            }
            public Skill GetSkill()
            {
                return this.Skill;
            }

            public ProductMaterialPair(Block block, byte data, ItemRequirement req)
            {
                this.Data = data;
                this.Product = block;
                this.Req = req;
            }


            public void Write(BinaryWriter w)
            {
                w.Write((int)this.Product.Type);
                w.Write(this.Data);
                this.Req.Write(w);
                w.Write(this.Skill == null ? 0 : this.Skill.ID);
            }
            public ProductMaterialPair(BinaryReader r)
            {
                this.Product = Block.Registry[(Block.Types)r.ReadInt32()];
                this.Data = r.ReadByte();
                this.Req = new ItemRequirement(r);
                var skillid = r.ReadInt32();
                if (skillid > 0)
                    this.Skill = Skill.Dictionary[skillid];
            }
            public List<SaveTag> Save()
            {
                List<SaveTag> save = new List<SaveTag>();
                save.Add(new SaveTag(SaveTag.Types.Int, "Product", (int)this.Product.Type));
                save.Add(new SaveTag(SaveTag.Types.Byte, "Data", this.Data));
                //save.Add(new SaveTag(SaveTag.Types.Int, "Material", (int)this.Req.ObjectID));
                //save.Add(new SaveTag(SaveTag.Types.Int, "Amount", this.Req.Max));
                save.Add(new SaveTag(SaveTag.Types.Compound, "Material", this.Req.Save()));
                return save;
            }
            public ProductMaterialPair(SaveTag tag)
            {
                //this.Product = 
                //tag.TryGetTagValue<int>("Product", value => this.Product = Block.Registry[(Block.Types)value]);
                this.Product = Block.Registry[(Block.Types)tag.GetValue<int>("Product")];
                this.Data = tag.TagValueOrDefault<byte>("Data", 0);
                //this.Req = new ItemRequirement(tag);//GameObject.Types.Default, 0);
                //this.Req = new ItemRequirement(tag);
                this.Req = new ItemRequirement(tag["Material"]);

            }
        }
    }
}
