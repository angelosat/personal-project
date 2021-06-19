using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverTavernCustomer : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var town = actor.Town;
            var taverns = town.GetBusinesses<Tavern>();
            var favs = actor.Personality.GetFavorites().ToList();
            var budget = actor.GetMoneyTotal();
            var visitor = actor.GetVisitorProperties();
            foreach (var tavern in taverns)
            {
                if (visitor.HasRecentlyVisited(tavern))
                    continue;
                if (!tavern.TryGetAvailableTable(out var table))
                    continue;
                if(!TrySelectOrder(favs, tavern, out var order))
                    continue;
                var desiredIngredients = SelectIngredients(town.Map.World.Random, favs, order);
                //var customerProps = tavern.AddCustomer(actor, table.Value, order);
                var request = new VisitorCraftRequest(order, desiredIngredients);
                tavern.AddCustomer(actor, table, request);
                //var test = desiredIngredients.ToList();
                //return null;
                return new AITask(typeof(TaskBehaviorTavernCustomer), new TargetArgs(actor.Map, table)) { ShopID = tavern.ID };// CustomerProps = customerProps };
            }
            return null;
        }
        bool TrySelectOrder(List<Material> favs, Tavern tavern, out CraftOrderNew order)
        {
            order = SelectOrder(favs, tavern);
            return order != null;
        }
        CraftOrderNew SelectOrder(List<Material> favs, Tavern tavern)
        {
            var orders = tavern.GetAvailableOrders().ToList();
            if (!orders.Any())
                return null;
            return orders.SelectRandomWeighted(tavern.Town.Map.Random, o => favs.Count(o.IsAllowed));
            //int currentP = 0;
            //var count = orders.Count;

            //var weights = new (CraftOrderNew order, int prob)[count];// orders.ToDictionary(o => o, o => favs.Count(o.IsAllowed));
            //for (int i = 0; i < count; i++)
            //{
            //    var o = orders[i];
            //    var p = favs.Count(o.IsAllowed);
            //    currentP += p;
            //    weights[i] = (o, currentP);
            //}
            //var val = tavern.Town.Map.Random.Next(currentP);
            //for (int i = 0; i < count; i++)
            //{
            //    var (order, prob) = weights[i];
            //    if (val <= prob)
            //        return order;
            //}
            //throw new Exception();
        }
        IEnumerable<(string reagent, ItemDef itemDef, Material material)> SelectIngredients(Random rand, List<Material> favs, CraftOrderNew order)
        {
            foreach (var r in order.Reaction.Reagents)
            {
                var restr = order.Restrictions[r.Name];
                var combos =
                    r.Ingredient.GetAllValidItemDefs()
                                .Where(i => !restr.IsRestricted(i))
                                .SelectMany(itemDef => itemDef.GetValidMaterials()
                                                              .Where(m => !restr.IsRestricted(m))
                                                              .Select(material => (itemDef, material)));
                var selectedCombo = combos.SelectRandomWeighted(rand, k => favs.Contains(k.material) ? 1 : 0);
                yield return (r.Name, selectedCombo.itemDef, selectedCombo.material);
            }
            yield break;
        }
        (CraftOrderNew order, IEnumerable<(string reagent, ItemDef itemDef, Material material)>) SelectOrderIngredients(ICollection<CraftOrderNew> orders, Random rand, int budget, List<Material> favs)
        {
            var ingredients = new List<(string reagent, ItemDef itemDef, Material material)>();
            foreach(var order in orders.Randomize(rand))
            {

            }
            return (null, null);
        }
        //CraftOrderNew ChooseOrder(List<Material> favs, Tavern tavern)
        //{
        //    var orders = tavern.GetAvailableOrders().ToList();
        //    orders.Sort((a, b) =>
        //    {
        //        var favCountA = favs.Count(a.IsAllowed);
        //        var favCountB = favs.Count(b.IsAllowed);
        //        if (favCountA > favCountB)
        //            return -1;
        //        else if (favCountA < favCountB)
        //            return 1;
        //        else
        //            return 0;
        //    });
        //    return orders.FirstOrDefault();
        //}
    }
}
