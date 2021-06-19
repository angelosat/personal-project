using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Modules.Crafting
{
    public class CraftOperationNew
    {
        public Vector3 WorkstationEntity;
        public Progress CraftProgress = new Progress();
        public Reaction Reaction;
        public Dictionary<string, int> Reagents;

        public CraftOperationNew(Reaction Reaction, Dictionary<string, int> reagents, Vector3 station)
        {
            this.Reaction = Reaction;
            this.Reagents = reagents;
            this.WorkstationEntity = station;
        }
    }
}
