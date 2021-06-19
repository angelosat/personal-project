using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Modules.Towns.Housing;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Towns
{
    class TownsManager : GameComponent
    {
        TownsUI TownsUI;
        Window _HousesUI;
        Window HousesUI
        {
            get
            {
                if (_HousesUI == null)
                {
                    _HousesUI = new Window()
                    {
                        Title = "Houses",
                        AutoSize = true,
                        Movable = true
                    };
                    _HousesUI.Location = _HousesUI.CenterScreen / 2;
                    _HousesUI.Client.Controls.Add(new UIHouses());
                }
                return _HousesUI;
            }
        }

        //public TownsManager()
        //{
        //    this.HousesUI = new Window()
        //    {
        //        Title = "Houses",
        //        AutoSize = true,
        //        Movable = true
        //    };
        //    this.HousesUI.Client.Controls.Add(new UIHouses());
        //}

        public override void Initialize()
        {
            // register handlers or just iterate through gamecomponents when handling packets at server and client?
            Net.Server.Instance.RegisterPacketHandler(PacketType.Towns, new TownsPacketHandler());
            Net.Client.Instance.RegisterPacketHandler(PacketType.Towns, new TownsPacketHandler());
        }

        public override void InitHUD(Hud hud)
        {
            this.TownsUI = new Towns.TownsUI();
            this.TownsUI.InitHud(hud);
        }

        public override void OnGameEvent(GameEvent e)
        {
            return;
            this.TownsUI.OnGameEvent(e);
        }

        public override void OnHudCreated(Hud hud)
        {
            base.OnHudCreated(hud);
            IconButton btn_house = new IconButton()
            {
                //Location = Btn_Structures.TopRight,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                LeftClickAction = () => this.HousesUI.Toggle(),// ScreenManager.CurrentScreen.ToolManager.ActiveTool = new HouseTool(),
                HoverFunc = () => "Designate housing"
            };
            hud.Box_Buttons.Controls.Insert(0, btn_house);
            hud.Box_Buttons.AlignHorizontally();
            hud.Box_Buttons.Location = hud.Box_Buttons.BottomRightScreen;
        }

        public override void OnContextMenuCreated(IContextable obj, ContextArgs a)
        {
            var target = obj as TargetArgs;
            if (target == null)
                return;
            AddDesignation(a, new InteractionDigging());
            AddDesignation(a, new Tilling());
            //a.Actions.Add(new ContextAction(() => "Designate", () =>
            //{
            //    PlayerControl.ToolManager.Instance.ActiveTool = new ToolDesignation(v => {
            //        Client.Instance.Send(PacketType.Towns, new Towns.PacketAddJob(Player.Actor.Network.ID, new TargetArgs(v), new Components.Interactions.Digging().Name).Write());                
            //    });
            //}));
            //a.Actions.Add(new ContextAction(() => "Designate", () =>
            //{
            //    PlayerControl.ToolManager.Instance.ActiveTool = new ToolDesignation(v =>
            //    {
            //        Client.Instance.Send(PacketType.Towns, new Towns.PacketAddJob(Player.Actor.Network.ID, new TargetArgs(v), new Components.Interactions.Tilling().Name).Write());
            //    });
            //}));

            foreach (var comp in Engine.Map.GetTown().TownComponents)
                comp.OnContextMenuCreated(obj, a);
        }

        static void AddDesignation(ContextArgs a, Interaction i)
        {
            a.Actions.Add(new ContextAction(() => "Designate: " + i.Verb, () =>
            {
                PlayerControl.ToolManager.Instance.ActiveTool = new ToolDesignateSingle(v =>
                {
                    Client.Instance.Send(PacketType.Towns, new Towns.PacketAddJob(Player.Actor.Network.ID, new TargetArgs(v), i.Name).Write());
                });
            }));
        }

        public override void HandlePacket(Server server, Packet msg)
        {
            var map = server.Map;
            if (map == null)
                return;
            var town = map.GetTown();
            if (town == null)
                return;
            town.HandlePacket(server, msg);
        }
        public override void HandlePacket(Client client, Packet msg)
        {
            var map = client.Map;
            if (map == null)
                return;
            var town = map.GetTown();
            if (town == null)
                return;
            town.HandlePacket(client, msg);
        }
    }
}
