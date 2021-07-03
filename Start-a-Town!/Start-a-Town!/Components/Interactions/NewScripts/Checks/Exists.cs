namespace Start_a_Town_
{
    public class Exists : ScriptTaskCondition
    {
        public Exists()
            : base("Exists")
        {

        }

        public override bool Condition(GameObject actor, TargetArgs target)
        {
            return target.Object.IsSpawned;
        }
    }
}
