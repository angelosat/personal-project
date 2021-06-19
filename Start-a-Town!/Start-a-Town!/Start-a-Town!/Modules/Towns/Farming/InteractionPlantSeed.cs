using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;
using Start_a_Town_.Blocks;
using Start_a_Town_.Graphics.Animations;

namespace Start_a_Town_.Towns.Farming
{
    class InteractionPlantSeed : Interaction
    {
        //GameObject.Types SeedType;
        public InteractionPlantSeed()//GameObject.Types seed)
            : base("Plant", 2)
        {
            //this.SeedType = seed;
            this.Animation = new AnimationTool();
        }

        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                RangeCheck.One,
                new ScriptTaskCondition("IsSeedSet", (a, t) =>
                {
                    var global = t.Global + Vector3.UnitZ;
                    var farm = a.Map.GetTown().FarmingManager.GetFarmAt(global);
                    return farm.SeedType != null;
                }),
                new IsHauling(SeedValid, (a, t) =>
                {
                    var global = t.Global + Vector3.UnitZ;
                    var farm = a.Map.GetTown().FarmingManager.GetFarmAt(global);
                    return hauled =>
                    {
                        if (hauled == null)
                            return false;
                        return hauled.ID == farm.SeedType.ID;
                    };
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
            var item = PersonalInventoryComponent.GetHauling(a);
            BlockFarmland.Plant(a.Map, t.Global, item.Object);
            item.Consume(1);
            a.Net.EventOccured(Message.Types.ItemLost, a, item, 1);
        }

        static bool SeedValid(GameObject hauled)
        {
            throw new Exception();
            if (hauled == null)
                return false;
            //var parent = seed.Parent;
            return true;// this.SeedType == hauled.ID;
        }
        public override object Clone()
        {
            return new InteractionPlantSeed();
        }
    }
}
