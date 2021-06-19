using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class BlueprintMemory : IComparable<BlueprintMemory>
    {
        public int CompareTo(BlueprintMemory other)
        {
            if (this.Value < other.Value)
                return -1;
            else if (this.Value == other.Value)
                return 0;
            else return 1;
        }

        public BlueprintMemory(GameObject.Types bp, int value = 0)
        {
            // TODO: Complete member initialization
            this.Blueprint = bp;// GameObject.Objects[bp];// WorkbenchComponent.Blueprints.;
            this.Value = value;
            this.MemorySize = 1; //TODO: relate to bp level
        }

        public int MemorySize { get; protected set; } // maybe a function of the blueprints level/complexity?
        public int Value { get; set; }
        public int Max { get; set; }
        public float Retaining { get; set; } // a value that indicates how fast a forgotten blueprint can be relearned
        /// <summary>
        /// Change this to an index
        /// </summary>
        public GameObject.Types Blueprint { get; set; }

        public BlueprintMemory Clone()
        {
            return new BlueprintMemory(this.Blueprint, this.Value)
            {
                MemorySize = this.MemorySize,
                Max = this.Max,
            };
        }

        public override string ToString()
        {
            //return this.Blueprint.Name + ": " + Value.ToString("##0%");
            return GameObject.Objects[this.Blueprint].Name + ": " + Value.ToString() + "%";//"##0%");
        }
    }
}
