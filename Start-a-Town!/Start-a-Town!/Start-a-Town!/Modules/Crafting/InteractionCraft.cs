using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Crafting
{
    class InteractionCraft : InteractionPerpetual
    {
        Progress Progress;
        public InteractionCraft()
            : base("Produce")
        {
        }

        static readonly TaskConditions conds =
                new TaskConditions(
                    new AllCheck(
                        //new RangeCheck(1),
                        RangeCheck.Sqrt2,
                        new MaterialsPresent()
                        ));

        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        //public override void Start(GameObject a, TargetArgs t)
        //{
        //    base.Start(a, t);
        //    (this.Animation as Graphics.Animations.AnimationTool).Contact = () => this.ToolContact(a, t);
        //    var entity = a.Map.GetBlockEntity(t.Global) as BlockEntityWorkstation;
        //    var craft = entity.GetCurrentOrder();
        //    this.Progress = craft.CraftProgress;
        //}

        //public override void Perform(GameObject a, TargetArgs t)
        //{
        //    if (this.Progress.Value >= this.Progress.Max)
        //    {
        //        var block = a.Map.GetBlock(t.Global) as BlockWorkstation;
        //        block.Produce(a, t.Global);
        //        this.State = States.Finished;
        //    }
        //}
        public override void OnUpdate(GameObject a, TargetArgs t)
        {
            var block = a.Map.GetBlock(t.Global) as BlockWorkstation;
            var entity = a.Map.GetBlockEntity(t.Global) as BlockEntityWorkstation;
            var craft = entity.GetCurrentOrder();
            craft.CraftProgress.Value += 25;
            if (craft.CraftProgress.Value >= craft.CraftProgress.Max)
            {
                block.Produce(a, t.Global);
                this.State = States.Finished;
            }
        }
        public override object Clone()
        {
            return new InteractionCraft();
        }
    }
    //public class InteractionCraft : Interaction
    //{
    //    Progress Progress;
    //    public InteractionCraft()
    //        : base("Produce", 4)
    //    {
    //        this.Conditions =
    //            new TaskConditions(
    //                new AllCheck(
    //                    new RangeCheck(1),
    //                    new MaterialsPresent()
    //                    ));
    //        this.RunningType = RunningTypes.Continuous;
    //        this.Animation = new Graphics.Animations.AnimationTool();
    //    }

    //    //public override void Perform(GameObject a, TargetArgs t)
    //    //{
    //    //    var block = a.Map.GetBlock(t.Global) as BlockSmeltery;
    //    //    //block.Produce(a, t.Global);
    //    //    var entity = a.Map.GetBlockEntity(t.Global) as Entity;
    //    //    var craft = entity.GetCurrentCrafting();//a.Map, t.Global);
    //    //    if (craft.CraftProgress.Value >= craft.CraftProgress.Max)
    //    //    {
    //    //        //craft.CraftProgress.Value = 0;
    //    //        block.Produce(a, t.Global);
    //    //        this.State = States.Finished;
    //    //    }
    //    //    else
    //    //        craft.CraftProgress.Value += .5f;
    //    //}
    //    public override void Start(GameObject a, TargetArgs t)
    //    {
    //        base.Start(a, t);
    //        (this.Animation as Graphics.Animations.AnimationTool).Contact = () => this.ToolContact(a, t);
    //        var entity = a.Map.GetBlockEntity(t.Global) as BlockEntityWorkstation;
    //        var craft = entity.GetCurrentOrder();//a.Map, t.Global);
    //        this.Progress = craft.CraftProgress;
    //    }

    //    public override void Perform(GameObject a, TargetArgs t)
    //    {
    //        if (this.Progress.Value >= this.Progress.Max)
    //        {
    //            //craft.CraftProgress.Value = 0;
    //            var block = a.Map.GetBlock(t.Global) as BlockWorkstation;// IBlockWorkstation;
    //            block.Produce(a, t.Global);
    //            this.State = States.Finished;
    //        }
    //    }
    //    public void ToolContact(GameObject a, TargetArgs t)
    //    {
    //        var block = a.Map.GetBlock(t.Global) as BlockWorkstation;//as IBlockWorkstation;
    //        //block.Produce(a, t.Global);
    //        var entity = a.Map.GetBlockEntity(t.Global) as BlockEntityWorkstation;
    //        var craft = entity.GetCurrentOrder();//a.Map, t.Global);
    //        craft.CraftProgress.Value += 25;// .5f;

    //        //if (craft.CraftProgress.Value >= craft.CraftProgress.Max)
    //        //{
    //        //    //craft.CraftProgress.Value = 0;
    //        //    block.Produce(a, t.Global);
    //        //    this.State = States.Finished;
    //        //}
    //    }
    //    public override object Clone()
    //    {
    //        return new InteractionCraft();
    //    }
    //}

}
