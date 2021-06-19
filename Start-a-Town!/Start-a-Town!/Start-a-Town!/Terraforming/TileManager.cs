using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    class TileManager
    {
        Dictionary<Block.Types, List<int>> TileInteractions;

        static TileManager _Instance;
        static TileManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new TileManager();
                return _Instance;
            }
        }

        TileManager()
        {
            TileInteractions = new Dictionary<Block.Types, List<int>>(){
                {Block.Types.Soil, new List<int>(){2}}
            };
        }

        static public bool TryGetInteractions(Block.Types tileType, out List<int> interactions)
        {
            return Instance.TileInteractions.TryGetValue(tileType, out interactions);
        }
    }
}
