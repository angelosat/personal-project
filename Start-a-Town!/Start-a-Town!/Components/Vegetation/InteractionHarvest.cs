namespace Start_a_Town_.Components
{
    public class InteractionHarvest : Interaction
    {
        GameObject Plant;
        PlantComponent PlantComp;
        public InteractionHarvest(GameObject parent)
            : base(
                "Harvest", 2)
        {
            this.Plant = parent;
            this.PlantComp = parent.GetComponent<PlantComponent>();
            this.Verb = "Harvesting";
        }
        public InteractionHarvest(GameObject parent, PlantComponent comp)
            : base(
                "Harvest", 2)
        {
            this.Plant = parent;
            this.PlantComp = comp;
            this.Verb = "Harvesting";
        }
        public InteractionHarvest()
            : base(
                "Harvest", 2)
        {
            this.Verb = "Harvesting";
        }
        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                RangeCheck.Sqrt2//One
                                //,
                                //new ScriptTaskCondition("IsPlantReady", (a, t) =>
                                //{
                                //    var comp = t.Object.GetComponent<PlantComponent>();
                                //    return comp.CurrentState == comp.Grown;
                                //})// comp.CurrentGrowthState == GrowthStates.Ready)
                ));
        //public override TaskConditions Conditions
        //{
        //    get
        //    {
        //        return conds;
        //    }
        //}
        public override void Perform(GameObject a, TargetArgs t)
        {
            //this.PlantComp.Harvest(this.Plant, a);
            var comp = t.Object.GetComponent<PlantComponent>();
            comp.Harvest(t.Object, a);
        }
        public override object Clone()
        {
            //return new InteractionHarvest(this.Plant, this.PlantComp);
            return new InteractionHarvest(this.Plant, this.PlantComp);

        }
    }
}
