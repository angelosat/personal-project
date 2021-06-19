using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Skills;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    public class InteractionClearGrass : Interaction
    {
        public InteractionClearGrass()
            : base(
                "Clear Grass",
                1
                //new Action<GameObject, TargetArgs>((a, t) => Rip(a, t)),
                )
        {
            this.Verb = "Clearing";
        }
        static readonly TaskConditions conds = new TaskConditions(
                    new AllCheck(
                    new RangeCheck(t => t.Global, Interaction.DefaultRange),
                    new AllCheck(
                        new AllCheck(
                            new TargetTypeCheck(TargetType.Position),
                //new ScriptTaskCondition("IsGrass", (a, t) => t.Global.GetBlock(a.Map).Type == Block.Types.Grass))//,
                            new ScriptTaskCondition("IsGrass", (a, t) => a.Map.GetBlock(t.Global).Type == Block.Types.Grass))//,

                        //new RangeCheck(t => t.Global, InteractionOld.DefaultRange)))
                        ))
                );
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        public override void Perform(GameObject actor, TargetArgs target)
        {
            actor.Net.SetBlock(target.Global, Block.Types.Soil); //keep previous data?
            //actor.Net.PopLoot(
            //    new LootTable(
            //        //new Loot(GameObject.Types.Twig, 0.4f, 1),
            //        //new Loot(GameObject.Types.Cobble, 0.4f, 1)),
            //        new Loot(GameObject.Types.Cobblestones, chance: 0.25f, count: 1),
            //        new Loot(GameObject.Types.Twig, chance: 0.25f, count: 1)),
            //    target.Global + Vector3.UnitZ, Vector3.Zero);
        }

        public override object Clone()
        {
            return new InteractionClearGrass();
        }
    }
}
