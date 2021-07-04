namespace Start_a_Town_
{
    public class WorkerRoleDef : Def
    {
        public readonly string Label;
        public WorkerRoleDef(string name, string label) : base(name)
        {
            this.Label = label;
        }
    }
}
