using System.Collections.Generic;

namespace Start_a_Town_
{
    public class StorageSettings
    {
        public HashSet<StorageFilter> ActiveFilters = new(StorageFilter.CreateFilterSet());
        readonly Dictionary<ItemDef, ItemFilter> Allowed = new();
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
