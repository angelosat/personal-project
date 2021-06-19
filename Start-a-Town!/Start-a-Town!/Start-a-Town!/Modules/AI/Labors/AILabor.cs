using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI
{
    public class AILabor
    {
        static public HashSet<AILabor> All
        {
            get
            {
                return new HashSet<AILabor>()
                {
                    Digger, Miner, Hauler, Lumberjack, Forester, Craftsman, Smelter, Farmer, Harvester, Builder, Carpenter
                };
            }
        }

        public string Name;

        public AILabor(string name)
        {
            this.Name = name;
        }

        static public readonly AILabor Digger = new AILabor("Digger");
        static public readonly AILabor Miner = new AILabor("Miner");
        static public readonly AILabor Hauler = new AILabor("Hauler");
        static public readonly AILabor Lumberjack = new AILabor("Lumberjack");
        static public readonly AILabor Forester = new AILabor("Forester");
        static public readonly AILabor Craftsman = new AILabor("Craftsman");
        static public readonly AILabor Smelter = new AILabor("Smelter");
        static public readonly AILabor Farmer = new AILabor("Farmer");
        static public readonly AILabor Harvester = new AILabor("Harvester");
        static public readonly AILabor Builder = new AILabor("Builder");
        static public readonly AILabor Carpenter = new AILabor("Carpenter");
    }
}
