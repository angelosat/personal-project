using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Modules.Towns.Housing;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.AI;
using Start_a_Town_.Towns.Farming;

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
                    //_HousesUI.Location = _HousesUI.CenterScreen / 2;
                    _HousesUI.SnapToScreenCenter();
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
            Server.Instance.RegisterPacketHandler(PacketType.Towns, new TownsPacketHandler());
            Client.Instance.RegisterPacketHandler(PacketType.Towns, new TownsPacketHandler());

            PacketInventoryDrop.Init();
            PacketInventoryEquip.Init();
            PacketInventoryInsertItem.Init();
            PopulationManager.Init();

            PacketEditAppearance.Init();
            PacketPlayerSetItemOwner.Init();
            PacketZoneDelete.Init();
            //PacketFarmSync.Init();
            PacketZoneDesignation.Init();
            PacketDesignateConstruction.Init();

            PacketCommandNpc.Init();
            PacketControlNpc.Init();
            NpcComponent.Init();
            PacketPlayerInput.Init();
            PacketPlayerToggleMove.Init();
            PacketPlayerToggleWalk.Init();
            PacketPlayerToggleSprint.Init();
            PacketPlayerJump.Init();

            PacketDiggingDesignate.Init();
            PacketEntityDesignation.Init();
            PacketToggleForbidden.Init();

            //PacketStockpileFiltersNew.Init();
            PacketStorageFilters.Init();
            PacketStorageFiltersNew.Init();

            //PacketStockpileSync.Init();
            ZoneManager.Init();
            StockpileManager.Init();
            FarmingManager.Initialize();
        }

        public override void InitHUD(Hud hud)
        {
            this.TownsUI = new Towns.TownsUI();
            this.TownsUI.InitHud(hud);
        }

        public override void OnGameEvent(GameEvent e)
        {
            return;
            //this.TownsUI.OnGameEvent(e);
        }

        public override void OnHudCreated(Hud hud)
        {
            StockpileManager.OnHudCreated(hud);

            return;

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
            AddDesignation(a, new InteractionTilling());
            //a.Actions.Add(new ContextAction(() => "Designate", () =>
            //{
            //    PlayerControl.ToolManager.Instance.ActiveTool = new ToolDesignation(v => {
            //        Client.Instance.Send(PacketType.Towns, new Towns.PacketAddJob(Player.Actor.InstanceID, new TargetArgs(v), new Components.Interactions.Digging().Name).Write());                
            //    });
            //}));
            //a.Actions.Add(new ContextAction(() => "Designate", () =>
            //{
            //    PlayerControl.ToolManager.Instance.ActiveTool = new ToolDesignation(v =>
            //    {
            //        Client.Instance.Send(PacketType.Towns, new Towns.PacketAddJob(Player.Actor.InstanceID, new TargetArgs(v), new Components.Interactions.Tilling().Name).Write());
            //    });
            //}));

            foreach (var comp in Engine.Map.GetTown().TownComponents)
                comp.OnContextMenuCreated(obj, a);
        }
        //public override void OnTargetInterfaceCreated(TargetArgs t, Control ui)
        //{
        //    foreach (var comp in Engine.Map.GetTown().TownComponents)
        //        comp.OnTargetInterfaceCreated(t, ui);
        //}
        public override void OnContextActionBarCreated(ContextActionBar.ContextActionBarArgs args)
        {
            foreach (var comp in Engine.Map.GetTown().TownComponents)
                comp.OnContextActionBarCreated(args);
        }
        static void AddDesignation(ContextArgs a, Interaction i)
        {
            a.Actions.Add(new ContextAction(() => "Designate: " + i.Verb, () =>
            {
                ToolManager.SetTool(new ToolDesignateSingle(v =>
                {
                    Client.Instance.Send(PacketType.Towns, new Towns.PacketAddJob(PlayerOld.Actor.RefID, new TargetArgs(v), i.Name).Write());
                }));
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
        internal override void HandlePacket(IObjectProvider net, PacketType type, System.IO.BinaryReader r)
        {
            var map = net.Map;
            if (map == null)
                return;
            var town = map.GetTown();
            if (town == null)
                return;
            town.HandlePacket(net, type, r);
        }

        //public override void OnUIEvent(UIManager.Events e, object[] p)
        //{
        //    switch(e)
        //    {
        //        case UIManager.Events.NpcUICreated:
        //            break;
        //            var npcui = p[0] as NpcUI;
        //            var npc = npcui.Npc;
        //            var win = new NpcLaborsUI(npc).ToWindow(npc.Name + "'s labors");
        //            npcui.AddButton(new Button("Labors")
        //            {
        //                LeftClickAction = () =>
        //                {
        //                    win.ToggleSmart();
        //                }
        //            });
        //            break;

        //        default:
        //            break;
        //    }
        //}
    }
}
