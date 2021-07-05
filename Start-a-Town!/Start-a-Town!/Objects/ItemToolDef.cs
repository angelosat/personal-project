using System.Collections.Generic;

namespace Start_a_Town_
{
    public class ItemToolDef
    {
        public ToolAbility Ability;
        public readonly HashSet<JobDef> AssociatedJobs = new();
        
        public ItemToolDef(ToolAbility ability)
        {
            this.Ability = ability;
        }
        public ItemToolDef AssociateJob(params JobDef[] jobs)
        {
            foreach (var j in jobs)
                this.AssociatedJobs.Add(j);
            return this;
        }
    }
}
