using System;
using System.Collections.Generic;

namespace Start_a_Town_.Components.Vegetation
{
    class SaplingComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return Name; }
        }

        public readonly static string Name = "Sapling";

        public override void GetHauledActions(GameObject parent, TargetArgs target, List<Interaction> actions)
        {
            actions.Add(new InteractionPlant(parent));
        }
        public override object Clone()
        {
            return new SaplingComponent();
        }

        class InteractionPlant : Interaction
        {
            GameObject Parent;

            public InteractionPlant(GameObject parent) :base("Plant", 1)
            {
                //this.Name = "Plant";
                //this.Verb = "Planting";
                //this.Seconds = 1;
                //this.Conditions =
                //    new TaskConditions(
                //        new AllCheck(
                //            new RangeCheck(),
                //            new TargetTypeCheck(TargetType.Position),
                //            //new BlockCheck(b => b.MaterialType == MaterialType.Soil)
                //            new ScriptTaskCondition("IsSoil", (a, t) =>
                //            {
                //                var blockmat = Block.GetBlockMaterial(a.Map, t.Global);
                //                return blockmat.Type == MaterialType.Soil;
                //            })
                //));
                this.Parent = parent;
            }
            static readonly TaskConditions conds =
                    new TaskConditions(
                        new AllCheck(
                            new RangeCheck(),
                            new TargetTypeCheck(TargetType.Position),
                //new BlockCheck(b => b.MaterialType == MaterialType.Soil)
                            new ScriptTaskCondition("IsSoil", (a, t) =>
                            {
                                var blockmat = Block.GetBlockMaterial(a.Map, t.Global);
                                return blockmat.Type == MaterialType.Soil;
                            })
                ));
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                //var slot = GearComponent.GetSlot(a, GearType.Mainhand);
                var slot = PersonalInventoryComponent.GetHauling(a);

                if (slot.Object != this.Parent)
                    return;
                slot.Consume();
                BlockDefOf.Sapling.Place(a.Map, t.FaceGlobal, 0, 0, 0);
            }

            public override object Clone()
            {
                return new InteractionPlant(this.Parent);
            }
        }
    }
}
