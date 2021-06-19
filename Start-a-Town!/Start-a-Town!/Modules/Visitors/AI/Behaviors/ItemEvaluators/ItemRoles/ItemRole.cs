using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    //class ItemRoleDef : Def
    //{
    //    public ItemRoleDef(string name) : base(name)
    //    {
    //    }

    //    //static ItemRole GetItemRoleGear(GearType type)
    //    //{
    //    //    return new ItemRole();
    //    //}

    //    static public int EvaluateApparel(Actor actor, Entity item)
    //    {
    //        var apparel = item.Def.ApparelProperties;
    //        if (apparel == null)
    //            return 0;
    //        int benefit = 0;
    //        //var gear = actor.GetEquipmentSlot(apparel.GearType);
    //        var similarOwnedItem = actor.Inventory.FindItems((Entity item) => item.Def.ApparelProperties?.GearType == apparel.GearType).FirstOrDefault();
    //        benefit = apparel.ArmorValue - (similarOwnedItem?.Def.ApparelProperties.ArmorValue ?? 0);
    //        return benefit;
    //    }
    //    static public Entity FindBestItem(Actor actor, GearType geartype)
    //    {
    //        var best = actor.GetEquipmentSlot(geartype);
    //        var bestValue = best?.Def.ApparelProperties.ArmorValue ?? 0;
    //        best = actor.Inventory.FindItems((Entity item) => item.Def.ApparelProperties?.GearType == geartype).OrderByDescending(i => i.Def.ApparelProperties.ArmorValue).FirstOrDefault();
    //        return best;
    //    }
    //    Dictionary<ItemRole, Entity> GenerateItemRoles(Actor actor)
    //    {
    //        var d = new Dictionary<ItemRole, Entity>();

    //        //actor.GetComponent<GearComponent>().Equipment;
    //        var g = actor.GetGearTypes();
    //        foreach(var gear in g)
    //        {
    //            d.Add(ItemRole.ItemRolesGear[gear], null);
    //        }
    //        return d;
    //    }
    //}

    //static class ItemRoleDefOf
    //{
    //    static public readonly ItemRoleDef ItemRoleGear = new("ItemRoleGear");
    //}

    abstract class ItemRole// : Def
    {
        //public ItemRole(string name) : base($"ItemRole{name}")
        //{

        //}
        //ItemRoleDef ItemRoleDef;
        public ItemRole()
        {

        }
        abstract public int Score(Actor actor, Entity item);
        public Entity FindBest(Actor actor, IEnumerable<Entity> items)
        {
            Entity bestItem = null;
            int bestScore = 0;
            foreach (var i in items)
            {
                var score = this.Score(actor, i);
                if (score > bestScore)
                {
                    bestItem = i;
                    bestScore = score;
                }
            }
            return bestItem;
        }
    }
}
