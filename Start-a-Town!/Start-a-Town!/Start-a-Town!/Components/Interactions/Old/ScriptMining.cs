using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptMining : Script
    {
        //public ScriptArgs Args { get; set; }
        public double Time { get; set; }
        public double Length { get; set; }
        public ScriptMining()
        {
            this.ID = Script.Types.Mining;
            this.Name = "Mining";
            //this.BaseTimeInSeconds = 3;
            this.SpeedBonus = Formula.GetFormula(Formula.Types.MiningSpeed);
            this.RangeCheck = DefaultRangeCheck;
            this.Conditions = 
                new ConditionCollection(
                    //new InteractionCondition((actor, target) => FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Mining), "Requires appropriate tool")
                    //new Condition((actor, target) => UseComponentOld.HasAbility(actor.GetComponent<GearComponent>().Holding.Object, Script.Types.Mining), "Requires appropriate tool")
                    new Condition((actor, target) => UseComponentOld.HasAbility(actor.GetComponent<HaulComponent>().Holding.Object, Script.Types.Mining), "Requires appropriate tool")

                    );
            this.AddComponent(new ScriptTimer(a => GetTimeInMs(a.Actor), "Mining", 1));//, this.Success));
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
            this.AddComponent(new ScriptModifyDurability(-1));
            this.AddComponent(new ScriptEvaluations((a) => this.ScriptState = Components.ScriptState.Finished,
                new ScriptEvaluation(
                    //a => a.Target.Object.ID == GameObject.Types.Cobblestone,
                    a => 
                    {
                        //Block block = a.Net.Map.GetCell(a.Target.Object.Global).Block;// Block.Registry[a.Target.Object.Global.GetCell(a.Net.Map).Type];
                        //if(block.Material.IsNull())
                        //    return false;
                        //return block.Material.Density <= 0.8f;

                        var blockMat = Block.GetBlockMaterial(a.Net.Map, a.Target.Object.Global);
                        if (blockMat.IsNull())
                            return false;
                        return blockMat.Density <= 0.8f;
                    },
                    a => a.Net.EventOccured(Message.Types.InvalidTarget, a.Actor)),
                new ScriptEvaluation(
                    a => DefaultRangeCheck(a.Actor, a.Target, this.RangeValue),
                    a => a.Net.EventOccured(Message.Types.OutOfRange, a.Actor))
                )
            );
        }

        //public override void OnStart(ScriptArgs args)
        //{
        //    base.OnStart(args);
        //}

        public override void Success(ScriptArgs args)
        {
            base.Success(args);
            args.Net.PostLocalEvent(args.Target.Object, ObjectEventArgs.Create(Message.Types.Mine, new object[] { args.Actor }));
        }

        //public override void OnStart(ScriptArgs args)
        //{
        //    //Block block = Block.Registry[args.Target.Object.GetComponent<BlockComponent>().Type];
        //    Block block = Block.Registry[args.Target.Object.Global.GetCell(args.Net.Map).Type];
        //}

        public override object Clone()
        {
            return new ScriptMining();
        }
    }
}
