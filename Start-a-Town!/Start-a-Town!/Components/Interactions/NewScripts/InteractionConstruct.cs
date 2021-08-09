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

        public override void Start()
        {
            var a = this.Actor;
            var t = this.Target; 
            base.Start();
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
        public override void OnUpdate()
        {
            var a = this.Actor;
            var t = this.Target; 
            var workAmount = a.GetToolWorkAmount(ToolAbilityDef.Building.ID);
            this.BuildProgress.Value += workAmount;
            if (SuccessCondition())
            {
                this.Done();
                return;
            }
        }
        public void Done()
        {
            var a = this.Actor;
            var t = this.Target;
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
            map.GetBlockEntity(t.Global)?.IsMadeFrom(new ItemMaterialAmount[] { entity.Product.Requirement });
            this.Finish();
        }

        public override object Clone()
        {
            return new InteractionConstruct();
        }
    }
}
