using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components.Vegetation
{
    class SaplingComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return Name; }
        }

        public readonly static string Name = "Sapling";


        //public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        //{
        //    actions.Add(PlayerInput.RButton, new InteractionPlant(parent));
        //}

        //public override void GetEquippedActions(GameObject parent, List<Interaction> actions)
        //{
        //    actions.Add(new InteractionPlant(parent));
        //}

        internal override void GetEquippedActionsWithTarget(GameObject parent, GameObject actor, TargetArgs t, List<Interaction> list)
        {
            var action = new InteractionPlant(parent);
            throw new NotImplementedException();
            //var fail = action.Conditions.GetFailedCondition(actor, t);
            //if (fail == null)
            //    list.Add(action);
        }
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
