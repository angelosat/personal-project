using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.AI
{
    public class Knowledge
    {
        public List<Blueprint> Blueprints = new List<Blueprint>();
        public Dictionary<GameObject, Memory> Objects = new Dictionary<GameObject, Memory>();
        public void Update()
        {
            // TODO: maybe create a new list and add only continuing memories instead of removing them
            //foreach (MemoryEntry memory in this.Objects.ToList())
            foreach (KeyValuePair<GameObject, Memory> memory in this.Objects.ToDictionary(foo => foo.Key, foo => foo.Value))
            {
                if (memory.Value.Update())
                    this.Objects.Remove(memory.Key);
            }
        }

        public void Invalidate()
        {
            foreach(var obj in Objects.Values)
                obj.State = Memory.States.Invalid;
        }

        public override string ToString()
        {
            string text = "";
            foreach (Memory m in Objects.Values)
                text += m.ToString() + "\n";
            return text.TrimEnd('\n');
        }
    }
}
