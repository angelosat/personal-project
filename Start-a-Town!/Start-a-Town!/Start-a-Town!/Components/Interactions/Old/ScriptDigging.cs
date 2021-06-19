using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptDigging : Script
    {
        //public ScriptArgs Args { get; set; }
        public double Time { get; set; }
        public double Length { get; set; }
        public ScriptDigging()
        {
            this.ID = Script.Types.Digging;
            this.Name = "Mining";
            //this.BaseTimeInSeconds = 3;
            this.SpeedBonus = Formula.GetFormula(Formula.Types.MiningSpeed);
            this.RangeCheck = DefaultRangeCheck;
            this.Conditions = 
                new ConditionCollection(
                    //new InteractionCondition((actor, target) => FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Mining), "Requires appropriate tool")
                    //new Condition((actor, target) => UseComponentOld.HasAbility(actor.GetComponent<GearComponent>().Holding.Object, Script.Types.Digging), "Requires appropriate tool")
                    new Condition((actor, target) => UseComponentOld.HasAbility(actor.GetComponent<HaulComponent>().Holding.Object, Script.Types.Digging), "Requires appropriate tool")

                    );
            this.AddComponent(new ScriptTargetTypeFilter(a => a.Type == TargetType.Position));
            this.AddComponent(new ScriptTimer(a => GetTimeInMs(a.Actor), "Digging", 1));//, this.Success));
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
            this.AddComponent(new ScriptModifyDurability(-1));
            this.AddComponent(new ScriptEvaluations((a) => this.ScriptState = Components.ScriptState.Finished,
                new ScriptEvaluation(
                    //a => a.Target.Object.ID == GameObject.Types.Cobblestone,
                    a => 
                    {
                        //Block block = a.Net.Map.GetCell(a.Target.Global).Block;
                        //if(block.Material.IsNull())
                        //    return false;
                        //return block.Material.Density <= 0.3f;

                        var blockmat = Block.GetBlockMaterial(a.Net.Map, a.Target.Global);
                        if (blockmat.IsNull())
                            return false;
                        return blockmat.Density <= 0.3f;
                    },
                    a => a.Net.EventOccured(Message.Types.TooDense, a.Actor)),
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
            //args.Net.Map.GetBlock(args.Target.Global).Break(args.Net, args.Target.Global);
        }

        //public override void OnStart(ScriptArgs args)
        //{
        //    //Block block = Block.Registry[args.Target.Object.GetComponent<BlockComponent>().Type];
        //    Block block = Block.Registry[args.Target.Object.Global.GetCell(args.Net.Map).Type];
        //}

        public override object Clone()
        {
            return new ScriptDigging();
        }
    }
}
