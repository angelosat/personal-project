namespace Start_a_Town_
{
    public class AttributeDef : Def
    {
        public static readonly AttributeDef Strength = new AttributeDef("Strength").AddAwarders(AttributeProgressAwarderDef.FromCarrying);
        public static readonly AttributeDef Intelligence = new AttributeDef("Intelligence");
        public static readonly AttributeDef Dexterity = new AttributeDef("Dexterity");

        public string Description;
        AttributeProgressAwarderDef[] Awarders;
        public readonly string Label;
        AttributeDef(string label, string description = "") : base($"Attributename{label}")
        {
            this.Label = label;
            this.Description = description;
            this.Awarders = new AttributeProgressAwarderDef[0];
        }
        AttributeDef AddAwarders(params AttributeProgressAwarderDef[] awarders)
        {
            this.Awarders = awarders;
            return this;
        }

        internal void TryAward(GameObject parent, AttributeStat attributeStat)
        {
            for (int i = 0; i < this.Awarders.Length; i++)
            {
                var value = this.Awarders[i].GetValue(parent);
                attributeStat.Progress.Value += value;
            }
        }
        static AttributeDef()
        {
            Register(Strength);
            Register(Intelligence);
            Register(Dexterity);
        }
    }
}
