namespace Start_a_Town_
{
    public class BuildProperties
    {
        public Ingredient Ingredient;
        public float ToolSensitivity;
        public ConstructionCategory Category;
        public int WorkAmount = 1;
        public BuildProperties()
        {

        }
        public BuildProperties(Ingredient ingredient, float toolContribution)
        {
            this.Ingredient = ingredient;
            this.ToolSensitivity = toolContribution;
        }
    }
}
