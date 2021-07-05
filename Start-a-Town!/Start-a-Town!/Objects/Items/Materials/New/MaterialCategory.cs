namespace Start_a_Town_
{
    public class MaterialCategory
    {
        public readonly string Label;

        public MaterialCategory(string label)
        {
            this.Label = label;
        }

        static public readonly MaterialCategory Creature = new("Creature");
        static public readonly MaterialCategory Plant = new("Plant");
        static public readonly MaterialCategory Inorganic = new("Inorganic");
    }
}
