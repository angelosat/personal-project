using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class StoragePriority
    {
        StoragePriority()
        {

        }
        public string Name;
        public int Value;
        static public readonly StoragePriority Low = new StoragePriority() { Name = "Low", Value = 0 };
        static public readonly StoragePriority Normal = new StoragePriority() { Name = "Normal", Value = 1 };
        static public readonly StoragePriority High = new StoragePriority() { Name = "High", Value = 2 };
        static public readonly StoragePriority[] All = new StoragePriority[] { Low, Normal, High };
        static public StoragePriority GetFromValue(int value)
        {
            return All.First(p => p.Value == value);
        }
        public override string ToString()
        {
            return this.Name;
            //return string.Format("Priority: {0}", this.Name);
        }
    }
}
