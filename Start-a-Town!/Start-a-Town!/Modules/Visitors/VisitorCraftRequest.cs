using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    public class VisitorCraftRequest
    {
        public readonly CraftOrderNew Order;
        readonly Dictionary<string, ItemDefMaterialAmount> Preferences = new();

        public VisitorCraftRequest(CraftOrderNew order, IEnumerable<(string reagentName, ItemDef item, Material material)> preferences)
        {
            this.Order = order;
            foreach (var (reagentName, item, material) in preferences)
                this.Preferences.Add(reagentName, new ItemDefMaterialAmount(item, material, 1));
        }
        public (ItemDef item, Material material) GetPreference(string reagentName)
        {
            var i = this.Preferences[reagentName];
            return (i.Def, i.Material);
        }

        public IEnumerable<(string reagentName, ItemDef item, Material material)> GetPreferences()
        {
            foreach (var i in this.Preferences)
                yield return (i.Key, i.Value.Def, i.Value.Material);
        }
    }
}
