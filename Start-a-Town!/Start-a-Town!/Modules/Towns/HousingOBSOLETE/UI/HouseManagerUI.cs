using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Housing
{
    class HouseManagerUI : GroupBox
    {
        HouseManager Manager;
        public HouseManagerUI(HouseManager manager)
        {
            this.Manager = manager;
            var panellist = new Panel() { AutoSize = true };
            panellist.AddControls(new HouseListUI(manager, 150, 300));
            this.AddControls(panellist);

            var panelbuttons = new Panel() { Location = panellist.BottomLeft, AutoSize = true };
            var btnadd = new Button("Add");
            btnadd.LeftClickAction = () =>
            {
                ToolManager.SetTool(new ToolZoningPositions(Add, manager.GetAllResidencePositions));
                //ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ToolZoningPositions(Add, manager.GetAllResidencePositions);
            };
            panelbuttons.AddControls(btnadd);
            this.AddControls(panelbuttons);
        }

        private void ConfirmDesignation(Vector3 arg1, int arg2, int arg3, bool arg4)
        {
            throw new NotImplementedException();
        }

        private void Add(Vector3 global, int w, int h, bool remove)
        {
            Client.Instance.Send(PacketType.ResidenceAdd, PacketResidenceAdd.Write(PlayerOld.Actor.RefID, 0, global, w, h, remove));
            ScreenManager.CurrentScreen.ToolManager.ClearTool();
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.ResidenceAdded:
                    var residence = e.Parameters[0] as Residence;
                    FloatingText.Manager.Create(() => residence.Positions.First(), "Residence created", ft => ft.Font = UIManager.FontBold);
                    ResidenceUI.GetWindow(residence).Show();
                    break;

                default:
                    break;
            }
            base.OnGameEvent(e);
        }
    }
}
