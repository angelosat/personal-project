using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Components
{
    class FertilizerComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "FertilizerComponent"; }
        }
        public override object Clone()
        {
            return new FertilizerComponent(this.Potency);
        }
        public float Potency = 0;
        public FertilizerComponent()
        {

        }
        public FertilizerComponent(float potency)
        {
            this.Potency = potency;
        }
        //public override void GetEquippedActions(GameObject parent, List<Interactions.Interaction> actions)
        //{
        //    actions.Add(new InteractionFertilizing());
        //}
        //internal override void GetEquippedActionsWithTarget(GameObject parent, GameObject actor, TargetArgs t, List<Interaction> list)
        //{
        //    if (t.Type != TargetType.Position)
        //        return;
        //    var farmland = parent.Map.GetBlockEntity(t.Global) as BlockFarmland.Entity;
        //    if (farmland == null)
        //        return;
        //    if (farmland.Growth.Value > 0)
        //        list.Add(new InteractionFertilizing());
        //}

        public override void GetHauledActions(GameObject parent, TargetArgs t, List<Interaction> list)
        {
            if (t.Type != TargetType.Position)
                return;
            var farmland = parent.Map.GetBlockEntity(t.Global) as BlockFarmland.BlockFarmlandEntity;
            if (farmland == null)
                return;
            if (farmland.Sprout.Value > 0)
                list.Add(new InteractionFertilizing());
        }
    }
}
