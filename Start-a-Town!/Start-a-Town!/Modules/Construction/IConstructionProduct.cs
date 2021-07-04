using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.Modules.Construction
{
    interface IConstructionProduct
    {
        void SpawnProduct(IMap map, Vector3 global);
        List<ItemRequirement> GetReq();
        ToolAbilityDef GetSkill();
    }
}
