using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class JobDef : Def
    {
        public Func<Icon> GetIcon = ()=>Icon.Replace;

        readonly TaskGiver[] TaskGivers;
        readonly public HashSet<ToolAbilityDef> AssociatedTools = new();
        public ToolAbilityDef ToolUse;
        public readonly string Label;
        public JobDef(string name, params TaskGiver[] taskGivers) : base(name)
        {
            this.Label = name;
            this.TaskGivers = taskGivers;
        }
        public IEnumerable<TaskGiver> GetTaskGivers()
        {
            for (int i = 0; i < this.TaskGivers.Length; i++)
            {
                yield return this.TaskGivers[i];
            }
        }
        public override string ToString()
        {
            return this.Name;
        }
        public JobDef AddTools(params ToolAbilityDef[] abilities)
        {
            foreach (var a in abilities)
                this.AssociatedTools.Add(a);
            return this;
        }
        public JobDef AddTools(ToolAbilityDef toolUse)
        {
            this.ToolUse = toolUse;
            return this;
        }

        
    }
}
