using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Housing
{
    class HouseListUI : GroupBox
    {
        HouseManager Manager;
        ListBox<Residence, Label> List;
        public HouseListUI(HouseManager manager, int w, int h)
        {
            this.Manager = manager;
            this.List = new ListBox<Residence, Label>(w, h);
            this.Controls.Add(this.List);
            this.Refresh();
        }

        private new void Refresh()
        {
            this.List.Build(this.Manager.ResidenceList.Values, r => r.Name, (r, c) => c.LeftClickAction = () => ResidenceUI.GetWindow(r).ToggleSmart());
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.ResidenceRemoved:
                case Components.Message.Types.ResidenceUpdated:
                case Components.Message.Types.ResidenceAdded:
                    this.Refresh();
                    break;

                default:
                    break;
            }
        }
    }
}
