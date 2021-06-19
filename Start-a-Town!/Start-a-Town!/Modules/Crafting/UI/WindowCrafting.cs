using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Modules.Crafting.UI
{
    class WindowCrafting : Window
    {
        static WindowCrafting _Instance;
        public static WindowCrafting Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new WindowCrafting();
                return _Instance;
            }
        }

        WindowCrafting()
        {
            this.Title = "Crafting";
            this.Movable = true;
            this.AutoSize = true;
            this.Client.Controls.Add(CraftInterfaceNew.Instance);
        }

        //new public void Show(GameObject actor)
        //{

        //    base.Show();
        //}
    }
}
