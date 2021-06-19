using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class StorageSettingsNew
    {
  
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
 
    }
}
