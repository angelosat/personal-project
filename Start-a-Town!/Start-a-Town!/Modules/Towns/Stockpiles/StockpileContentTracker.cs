using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Towns
{
    class StockpileContentTracker
    {
        Dictionary<int, int> CachedInventory = new Dictionary<int, int>();
        StockpileManager Manager;
        public StockpileContentTracker(StockpileManager manager)
        {
            this.Manager = manager;
        }
        Queue<Stockpile> ToHandle = new Queue<Stockpile>();
        int CacheTimer = 0;
        public void Update()
        {
            if (this.CacheTimer++ >= Engine.TicksPerSecond)
            {
                this.CacheTimer = 0;
                this.UpdateInvetory();
            }
            if (!this.ToHandle.Any())
                this.ToHandle = new Queue<Stockpile>(this.Manager.Stockpiles.Values);
            else
            {
                this.ToHandle.Dequeue().CacheContents();
            }
        }
        public IEnumerable<(Entity item, int amount)> FindItems(Func<Entity, bool> filter, int amount)
        {
            var remaining = amount;
            var enumerator = this.GetContents().GetEnumerator();
            while(enumerator.MoveNext() && remaining > 0)
            {
                var i = enumerator.Current as Entity;
                if(filter(i))
                {
                    var found = Math.Min(i.StackSize, remaining);
                    remaining -= found;
                    yield return (i, found);
                }
            }
        }
        public IEnumerable<GameObject> GetContents()
        {
            foreach (var st in this.Manager.Stockpiles.Values)
                foreach (var i in st.GetContentsNew())
                    yield return i;
        }
        public Dictionary<int, int> GetCachedInventory()
        {
            Dictionary<int, int> total = new Dictionary<int, int>();
            foreach (var st in this.Manager.Stockpiles.Values)
            {
                var contents = st.CachedContents;
                foreach (var item in contents)
                    total.AddOrUpdate(item.Key, item.Value, (count) => count += item.Value);
            }
            return total;
        }
        public void UpdateInvetory()
        {
            var newinv = this.GetCachedInventory();
            var added = newinv.Where(p => !this.CachedInventory.ContainsKey(p.Key)).ToDictionary(p=>p.Key, p=>p.Value);
            var removed = this.CachedInventory.Where(p => !newinv.ContainsKey(p.Key)).ToDictionary(p => p.Key, p => p.Value);
            var updated = newinv
                .Where(p => (this.CachedInventory.ContainsKey(p.Key) && this.CachedInventory[p.Key] != p.Value))
                .ToDictionary(p => p.Key, p => p.Value);
            this.Manager.Map.EventOccured(Components.Message.Types.StockpileContentsUpdated, added, removed, updated);

            this.CachedInventory = newinv;
        }
    }
}
