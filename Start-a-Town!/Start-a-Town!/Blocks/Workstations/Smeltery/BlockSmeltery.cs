using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Crafting.Blocks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Start_a_Town_.Tokens;
using Start_a_Town_.Crafting;
using Start_a_Town_.AI;

namespace Start_a_Town_.Blocks.Smeltery
{
    partial class BlockSmeltery : BlockWithEntity
    {
        //public override AILabor Labor { get { return AILabor.Smelter; } }
        //public override bool IsDeconstructable => true;

        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Stone;
        }
        public BlockSmeltery()
            : base(Types.Smeltery)
        {
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
            this.AssetNames = "smoothstone";
            //this.Material = Material.Stone;
            //this.MaterialType = MaterialType.Mineral;
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfSubType(ItemSubType.Rock),
                        Reaction.Reagent.IsOfMaterial(MaterialDefOf.Stone),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            this.Tokens.Add(new IsWorkstation(IsWorkstation.Types.Smeltery));
            Towns.Constructions.ConstructionsManager.Production.Add(this.Recipe);
        }

        //public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        //{
        //    var list = new List<Interaction>();
        //    list.Add(new InteractionAddMaterial());
        //    //list.Add(new BlockSmeltery.InteractionCraft());
        //    list.Add(new InteractionCraft());
        //    return list;
        //}
        public override BlockEntity CreateBlockEntity()
        {
            return new BlockSmelteryEntityNew();
        }
        //public override BlockEntityWorkstation CreateWorkstationBlockEntity()
        //{
        //    return new BlockSmelteryEntity(4, 4, 4);
        //}
        //internal override void ConsumeFuel(IMap map, Vector3 global, float amount = 1)
        //{
        //    map.GetBlockEntity<BlockSmelteryEntity>(global).GetComp<EntityCompRefuelable>().ConsumeFuel(amount);
        //}

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


    //partial class BlockSmeltery : BlockWorkstation// Block, IBlockWorkstation
    //{
    //    public override AILabor Labor { get { return AILabor.Smelter; } }

    //    public override Material GetMaterial(byte blockdata)
    //    {
    //        return Material.Stone;
    //    }
    //    public BlockSmeltery()
    //        : base(Types.Smeltery)
    //    {
    //        this.AssetNames = "smoothstone";
    //        //this.Material = Material.Stone;
    //        //this.MaterialType = MaterialType.Mineral;
    //        this.Recipe = new BlockRecipe(
    //            Reaction.Reagent.Create(
    //                new Reaction.Reagent(
    //                    "Base",
    //                    Reaction.Reagent.IsOfSubType(ItemSubType.Rock),
    //                    Reaction.Reagent.IsOfMaterial(Material.Stone),
    //                    Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
    //                new BlockRecipe.Product(this),
    //                Skill.Building);
    //        this.Tokens.Add(new IsWorkstation(IsWorkstation.Types.Smeltery));
    //        Towns.Constructions.ConstructionsManager.Production.Add(this.Recipe);
    //    }

    //    public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
    //    {
    //        var list = new List<Interaction>();
    //        list.Add(new InteractionAddMaterial());
    //        //list.Add(new BlockSmeltery.InteractionCraft());
    //        list.Add(new InteractionCraft());
    //        return list;
    //    }

    //    //public override BlockEntity GetBlockEntity()
    //    public override BlockEntityWorkstation CreateWorkstationBlockEntity()
    //    {
    //        return new BlockSmelteryEntity(4, 4, 4);
    //    }
    //    internal override void ConsumeFuel(IMap map, Vector3 global, float amount = 1)
    //    {
    //        map.GetBlockEntity<BlockSmelteryEntity>(global).GetComp<EntityCompRefuelable>().ConsumeFuel(amount);
    //    }

    //    internal override void RemoteProcedureCall(IObjectProvider net, Vector3 vector3, Components.Message.Types type, System.IO.BinaryReader r)
    //    {
    //        var entity = net.Map.GetBlockEntity(vector3) as BlockSmelteryEntity;
    //        if (entity == null)
    //            throw new Exception();
    //        int dataLength = (int)(r.BaseStream.Length - r.BaseStream.Position); // TODO
    //        byte[] args = r.ReadBytes(dataLength);
    //        entity.HandleRemoteCall(net.Map, vector3, ObjectEventArgs.Create(type, args));
    //    }
    //    internal override string GetName(IMap map, Vector3 global)
    //    {
    //        var e = map.GetBlockEntity(global) as BlockSmelteryEntity;
    //        return string.Format("{0} (Power: {1})", this.Name, e.Power.ToStringPercentage());
    //    }

    //    //public override void OnDrop(GameObject actor, GameObject item, TargetArgs target, int quantity = -1)
    //    //{
    //    //    base.OnDrop(actor, item, target, quantity);
    //    //    actor.Map.GetBlockEntity(target.Global).OnDrop(actor, item, target, quantity);
    //    //    return;
    //    //    AddFuelNew(actor, item, target, quantity);
    //    //    return;
    //    //    AddFuelOld(actor, item, target, quantity);
    //    //}
    //    //private static void AddFuelNew(GameObject actor, GameObject item, TargetArgs target, int quantity)
    //    //{
    //    //    var e = actor.Map.GetBlockEntity<BlockSmelteryEntity>(target.Global);
    //    //    var amount = quantity == -1 ? item.StackMax : quantity;
    //    //    if (amount == 0)
    //    //        throw new Exception();
    //    //    var fuel = item.Fuel;
    //    //    //var totalFuel = fuel * amount;
    //    //    var fuelMissing = e.Power.Max - e.Power.Value;
    //    //    var desiredAmount = (int)(fuelMissing / fuel);
    //    //    var actualAmountToAdd = Math.Min(amount, desiredAmount);
    //    //    var actualFuelToAdd = actualAmountToAdd * fuel;
    //    //    //e.Power.Value += totalFuel;
    //    //    //item.StackSize -= amount;

    //    //    e.Power.Value += actualFuelToAdd;
    //    //    item.StackSize -= actualAmountToAdd;
    //    //}
    //    private static void AddFuelOld(GameObject actor, GameObject item, TargetArgs target, int quantity)
    //    {
    //        var e = actor.Map.GetBlockEntity<BlockSmelteryEntity>(target.Global);
    //        var amount = quantity == -1 ? item.StackMax : quantity;
    //        if (amount == 0)
    //            throw new Exception();
    //        var fuel = item.Material.Fuel * amount;
    //        //var comp = e.GetComp<EntityCompRefuelable>();
    //        e.Power.Value += fuel;
    //        //comp.Fuel.Value += fuel;
    //        item.StackSize -= amount;
    //    }

    //    public class InteractionCraftOld : Interaction
    //    {
    //        public InteractionCraftOld()
    //            : base("Produce", 4)
    //        {

    //        }
    //        static readonly TaskConditions conds =
    //                new TaskConditions(
    //                    new AllCheck(
    //                        RangeCheck.One,
    //                        new MaterialsPresent()
    //                        ));
    //        public override TaskConditions Conditions
    //        {
    //            get
    //            {
    //                return conds;
    //            }
    //        }

    //        public override void Perform(GameObject a, TargetArgs t)
    //        {
    //            var block = a.Map.GetBlock(t.Global) as BlockSmeltery;
    //            block.Produce(a, t.Global);
    //        }
    //        public override object Clone()
    //        {
    //            return new BlockSmeltery.InteractionCraftOld();
    //        }
    //    }
    //}
}
