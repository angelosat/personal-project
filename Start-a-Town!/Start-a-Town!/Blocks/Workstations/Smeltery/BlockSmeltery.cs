using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Blocks.Smeltery
{
    partial class BlockSmeltery : BlockWithEntity
    {
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Stone;
        }
        public BlockSmeltery()
            : base(Types.Smeltery)
        {
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
            this.AssetNames = "smoothstone";
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfSubType(ItemSubType.Rock),
                        Reaction.Reagent.IsOfMaterial(MaterialDefOf.Stone),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Production.Add(this.Recipe);
        }

        public override BlockEntity CreateBlockEntity()
        {
            return new BlockSmelteryEntityNew();
        }
        
        internal override void RemoteProcedureCall(IObjectProvider net, Vector3 vector3, Components.Message.Types type, System.IO.BinaryReader r)
        {
            var entity = net.Map.GetBlockEntity(vector3) as BlockSmelteryEntity;
            if (entity == null)
                throw new Exception();
            int dataLength = (int)(r.BaseStream.Length - r.BaseStream.Position); // TODO
            byte[] args = r.ReadBytes(dataLength);
            entity.HandleRemoteCall(net.Map, vector3, ObjectEventArgs.Create(type, args));
        }

        internal override string GetName(IMap map, Vector3 global)
        {
            var e = map.GetBlockEntity(global) as BlockSmelteryEntity;
            return string.Format("{0} (Power: {1})", this.Name, e.Power.ToStringPercentage());
        }
    }
}
