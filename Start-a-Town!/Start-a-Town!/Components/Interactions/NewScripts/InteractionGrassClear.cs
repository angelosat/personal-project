using System;

namespace Start_a_Town_.Components.Interactions
{
    [Obsolete]
    public class InteractionClearGrass : Interaction
    {
        public InteractionClearGrass()
            : base(
                "Clear Grass",
                1
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
                            new ScriptTaskCondition("IsGrass", (a, t) => a.Map.GetBlock(t.Global).Type == Block.Types.Grass))//,
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
        }

        public override object Clone()
        {
            return new InteractionClearGrass();
        }
    }
}
