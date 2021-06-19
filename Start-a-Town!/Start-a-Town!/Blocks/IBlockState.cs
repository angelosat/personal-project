using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public interface IBlockState
    {
        void Apply(IMap map, Vector3 global);
        void Apply(ref byte blockdata);
        void Apply(Block.Data data);
        void FromCraftingReagent(GameObject material);
        //void FromMaterial(Material material);
        Color GetTint(byte p);
        string GetName(byte p);
    }
}
