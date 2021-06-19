using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Towns.Crafting
{
    class PacketCraftingPlaceOrder : Packet
    {
        public CraftOperation Craft;
        public PacketCraftingPlaceOrder()
        {

        }
    }
}
