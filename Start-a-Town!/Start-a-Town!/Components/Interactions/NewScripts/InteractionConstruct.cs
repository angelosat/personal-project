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

        public override void Start(Actor a, TargetArgs t)
        {
            base.Start(a, t);
            var entity = a.Map.GetBlockEntity(t.Global) as IConstructible;
            this.BuildProgress = entity.BuildProgress;
            var tool = a.GetEquipmentSlot(GearType.Mainhand);
            var toolspeed = tool is null ? 0 : StatDefOf.ToolSpeed.GetValue(tool);
            var speed = 1 + toolspeed;
            this.Animation.Speed = speed;
        }
        bool SuccessCondition()
        {
            return this.BuildProgress.IsFinished;
        }
        public override void OnUpdate(Actor a, TargetArgs t)
        {
            var workAmount = a.GetToolWorkAmount(ToolAbilityDef.Building.ID);
            this.BuildProgress.Value += workAmount;
            if (SuccessCondition())
            {
                this.Done(a, t);
                return;
            }
        }
        public void Done(Actor a, TargetArgs t)
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
            block.Place(map, entity.OriginGlobal, entity.Product.Material, entity.Product.Data, 0, ori, true);
            map.GetBlockEntity(t.Global)?.IsMadeFrom(new ItemDefMaterialAmount[] { entity.Product.Requirement });
            this.Finish(a, t);
        }

        public override object Clone()
        {
            return new InteractionConstruct();
        }
    }


    

}
