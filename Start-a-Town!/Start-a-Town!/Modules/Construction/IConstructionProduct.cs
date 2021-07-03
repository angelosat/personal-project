using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Modules.Construction
{
    interface IConstructionProduct
    {
        void SpawnProduct(IMap map, Vector3 global);
        //ItemRequirement GetReq();
        List<ItemRequirement> GetReq();
        ToolAbilityDef GetSkill();
    }
}
