namespace Start_a_Town_
{
    class AttributeStrength : AttributeWorker
    {
        public AttributeStrength(AttributeDef def) : base(def)
        {
        }

        public override void Tick(GameObject obj, AttributeStat attributeStat)
        {
            var enc = StatDefOf.Encumberance.GetValue(obj);
            attributeStat.Progress.Value += enc;
        }
    }
}
