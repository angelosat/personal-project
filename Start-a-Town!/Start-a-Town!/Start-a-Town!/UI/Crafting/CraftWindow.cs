using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Modules.Construction.UI;

namespace Start_a_Town_.UI
{
    class CraftWindow : StructureWindow
    {
        static CraftWindow _Instance;
        public static new CraftWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new CraftWindow();
                return _Instance;
            }
        }

        public CraftWindow():base()
        {
            this.Title = "Crafting";
        }

        protected override List<Reaction> GetAvailableBlueprints()
        {
            return (from reaction in Reaction.Dictionary.Values
                    //where reaction.ValidWorkshops.Contains(Reaction.Site.Person)
                    where reaction.ValidWorkshops.Count == 0
                    select reaction).ToList();
        } 
    }
}
