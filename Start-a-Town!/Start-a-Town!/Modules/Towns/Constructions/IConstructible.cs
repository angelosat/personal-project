using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    interface IConstructible
    {
        //bool IsReadyToBuild(IMap map, Vector3 global, out List<ObjectDefAmount> reqAmount);
        /// <summary>
        /// Return all requirements or just the first occuring one?
        /// </summary>
        /// <param name="reqAmount"></param>
        /// <returns></returns>
        //bool IsReadyToBuild(out ItemDefAmount reqAmount);
        bool IsReadyToBuild(out ItemDef def, out Material material, out int amount);
        bool IsValidHaulDestination(ItemDef objid);
        int GetMissingAmount(ItemDef objid);
        Progress BuildProgress { get; }
        //Vector3 Origin { get; }
        List<Vector3> Children { get; }

        //BlockRecipe.ProductMaterialPair Product { get; }
    }
}
