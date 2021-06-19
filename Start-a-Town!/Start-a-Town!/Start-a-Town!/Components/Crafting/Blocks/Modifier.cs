using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    partial class BlockConstruction
    {
        partial class Product
        {
            public abstract class Modifier
            {
                public string LocalMaterialName { get; set; }
                public Modifier(string localMaterialName)
                {
                    this.LocalMaterialName = localMaterialName;
                }
                public abstract void Apply(GameObject reagent, ref byte data);

            }
        }
    }
}
