using Start_a_Town_.UI;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class JobDef : Def
    {
        readonly TaskGiver[] TaskGivers;
        readonly public HashSet<ToolUseDef> AssociatedTools = new();
        public ToolUseDef ToolUse;
        //public readonly string Label;
        public Icon Icon => Icon.Replace;

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

        public JobDef AddTools(params ToolUseDef[] abilities)
        {
            foreach (var a in abilities)
                this.AssociatedTools.Add(a);
            return this;
        }
        public JobDef AddTools(ToolUseDef toolUse)
        {
            this.ToolUse = toolUse;
            return this;
        }
    }
}
