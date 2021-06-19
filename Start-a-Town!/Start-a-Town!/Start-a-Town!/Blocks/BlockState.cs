using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Blocks
{
    class BlockState : IBlockState
    {
        public virtual byte Data
        {
            get
            {
                return 0;
            }
        }

        public virtual void FromMaterial(GameObject material)
        {

        }
        public virtual Color GetTint(byte p)
        {
            return Color.White;
        }
        public virtual string GetName(byte p)
        {
            return "";
        }

        public void Apply(IMap map, Vector3 global)
        {
            //global.GetCell(map).BlockData = this.Data;// GetData();
            map.GetCell(global).BlockData = this.Data;// GetData();
        }
        public void Apply(ref byte blockdata)
        {
            blockdata = this.Data;//GetData();
        }
        public void Apply(Block.Data data)
        {
            data.Value = this.Data;//GetData();
        }
    }
}
