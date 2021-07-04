using System.Collections.Generic;

namespace Start_a_Town_
{
    public class WorkerRole
    {
        public readonly WorkerRoleDef Def;
        readonly HashSet<Actor> Workers = new();
        public WorkerRole(WorkerRoleDef def)
        {
            this.Def = def;
        }
        public bool Contains(Actor actor)
        {
            return this.Workers.Contains(actor);
        }
        public void Add(Actor actor)
        {
            this.Workers.Remove(actor);
        }
        public void Remove(Actor actor)
        {
            this.Workers.Remove(actor);
        }
        public void Toggle(IEnumerable<Actor> actors)
        {
            foreach (var a in actors)
            {
                if (this.Workers.Contains(a))
                    this.Workers.Remove(a);
                else
                    this.Workers.Add(a);
            }
        }
    }
}
