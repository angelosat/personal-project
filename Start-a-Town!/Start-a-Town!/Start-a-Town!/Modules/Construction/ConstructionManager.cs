using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.Construction.UI;

namespace Start_a_Town_.Modules.Construction
{
    class ConstructionManager : GameComponent
    {
        ConstructionPacketHandler PacketHandler = new ConstructionPacketHandler();

        //HashSet<Vector3> CurrentConstructions = new HashSet<Vector3>();

        public override void Initialize()
        {
            // register handlers or just iterate through gamecomponents when handling packets at server and client?
            //Net.Server.Instance.RegisterPacketHandler(PacketType.Construction, this.PacketHandler);
            //Net.Client.Instance.RegisterPacketHandler(PacketType.Construction, this.PacketHandler);

            //Net.Server.Instance.RegisterPacket(PacketType.PlaceBlockConstruction, PacketPlaceBlockConstruction.Handle);
            //Net.Client.Instance.RegisterPacket(PacketType.PlaceBlockConstruction, PacketPlaceBlockConstruction.Handle);
        }

        public override void HandlePacket(Client client, Packet msg)
        {
            this.PacketHandler.HandlePacket(client, msg);
        }
        public override void HandlePacket(Server server, Packet msg)
        {
            this.PacketHandler.HandlePacket(server, msg);
        }

        public override void OnHudCreated(Hud hud)
        {
            base.OnHudCreated(hud);
            IconButton btn_Construct = new IconButton()
            {
                //Location = Btn_Structures.TopRight,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                LeftClickAction = () => ConstructionsWindow.Instance.Toggle(),
                HoverFunc = () => "Construct [" + GlobalVars.KeyBindings.Build + "]"
            };
            IconButton btn_Structures = new IconButton()
            {
                //Location = Btn_Prefabs.TopRight,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                LeftClickAction = () => StructureWindow.Instance.Toggle(),
                HoverFunc = () => "Build [" + GlobalVars.KeyBindings.Build + "]"
            };
            hud.Box_Buttons.Controls.Insert(0, btn_Construct);
            hud.Box_Buttons.Controls.Insert(0, btn_Structures);
            hud.Box_Buttons.AlignHorizontally();
            hud.Box_Buttons.Location = hud.Box_Buttons.BottomRightScreen;
        }
        

     
    }
}
