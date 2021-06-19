using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class StorageSettings
    {
        //public enum Priorities { Low, Normal, High };
        //static readonly public Priorities[] PrioritiesAll = new Priorities[] { Priorities.Low, Priorities.Normal, Priorities.High };
        //public Priorities Priority = Priorities.Normal;
        StorageFilterCategoryNew StorageFiltersRoot;

        public HashSet<StorageFilter> ActiveFilters = new HashSet<StorageFilter>(StorageFilter.CreateFilterSet());
        public StoragePriority Priority = StoragePriority.Normal;

        public void Toggle(StorageFilter filter, bool toggle)
        {
            if (toggle)
                this.ActiveFilters.Add(filter);
            else
                this.ActiveFilters.Remove(filter);
        }
        public void Toggle(StorageFilter filter)
        {
            if (!this.ActiveFilters.Contains(filter))
                this.ActiveFilters.Add(filter);
            else
                this.ActiveFilters.Remove(filter);
        }
        //public void Enable(StorageFilter filter)
        //{

        //}
        //public void Disable(StorageFilter filter)
        //{

        //}
    }
}
