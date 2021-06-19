using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class Building
    {
        static int _IDSequence = 0;
        public static int IDSequence { get { return _IDSequence++; } }
        const int ObjectIDRange = 2000;

        static Dictionary<int, Building> _Dictionary;
        public static Dictionary<int, Building> Dictionary
        {
            get
            {
                if (_Dictionary.IsNull())
                    _Dictionary = new Dictionary<int, Building>();
                return _Dictionary;
            }
        }

        public int ID { get; set; }
        public string Name { get; set; }
        //public GameObject.Types Building { get; set; }
        public List<Reaction.Reagent> Reagents { get; set; }
        public Block.Types Block { get; set; }

        //static public readonly 
    }
}
