using Start_a_Town_.Components;

namespace Start_a_Town_.Interactions
{
    class InteractionConstruct : InteractionPerpetual
    {
        Progress BuildProgress;
        public InteractionConstruct()
            : base("Construct")
        {
        }

        public override void Start(GameObject a, TargetArgs t)
        {
            base.Start(a, t);
            var entity = a.Map.GetBlockEntity(t.Global) as IConstructible;
            this.BuildProgress = entity.BuildProgress;
            var speed = a.GetStat(Stat.Types.Building);
            var actor = a as Actor;
            var tool = actor.GetEquipmentSlot(GearType.Mainhand);
            var toolspeed = tool is null ? 0 : StatDefOf.ToolSpeed.GetValue(tool);
            speed *= (1 + toolspeed);
            this.Animation.Speed = speed;
        }
        bool SuccessCondition()
        {
            return this.BuildProgress.IsFinished;
        }
        public override void OnUpdate(GameObject a, TargetArgs t)
        {
            var workAmount = a.GetToolWorkAmount(ToolAbilityDef.Building.ID);
            this.BuildProgress.Value += workAmount;
            if (SuccessCondition())
            {
                this.Done(a, t);
                return;
            }
        }
        public void Done(GameObject a, TargetArgs t)
        {
            var global = t.Global;
            var map = a.Map;
            var entity = map.GetBlockEntity(global) as BlockConstructionEntity;
            entity.Container.Clear(); // clear materials because they get ejected when the blockconstruction remove method is called
            var block = entity.Product.Block;
            var cell = map.GetCell(global);
            var ori = cell.Orientation;
            foreach (var child in entity.Children)
            {
                map.RemoveBlock(child, false);
            }
            block.Place(map, entity.Origin, entity.Product.Data, 0, ori, true);
            map.GetBlockEntity(t.Global)?.IsMadeFrom(new ItemDefMaterialAmount[] { entity.Product.Requirement });
            this.Finish(a, t);
        }

        public override object Clone()
        {
            return new InteractionConstruct();
        }
    }


    

}
