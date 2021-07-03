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
        
        public override void Perform(GameObject a, TargetArgs t)
        {
            var comp = t.Object.GetComponent<PlantComponent>();
            comp.Harvest(t.Object, a);
        }
        public override object Clone()
        {
            return new InteractionHarvest(this.Plant, this.PlantComp);
        }
    }
}
