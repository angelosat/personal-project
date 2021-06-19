using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Farming;

namespace Start_a_Town_.Towns.Digging
{
    class DiggingManagerUI : GroupBox
    {
        DiggingManager Manager;
        IconButton BtnDesignate;
        public DiggingManagerUI(DiggingManager manager)
        {
            this.Manager = manager;
            this.BtnDesignate = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate digging\n\nLeft click & drag: Add digging\nCtrl+Left click: Remove digging",// "Add/Remove stockpiles",
                LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ToolDesignatePositions(// new ToolZoningPositions(
                    CreateDigging
                , manager.Town.DiggingManager.GetPositions// manager.Town.GetZones
                ) { ValidityCheck = g => true }
            };
            this.Controls.Add(this.BtnDesignate);
        }

        private static void CreateDigging(Vector3 begin, Vector3 end, bool remove)
        {
            Client.Instance.Send(PacketType.DiggingDesignate, new PacketDiggingDesignate(Player.Actor.Network.ID, begin, end, remove).Write());
        }
        
    }
}
