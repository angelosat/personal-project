using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Modules.Towns.Housing;
using Start_a_Town_.Components.Interactions;
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
                    _HousesUI.SnapToScreenCenter();
                    _HousesUI.Client.Controls.Add(new UIHouses());
                }
                return _HousesUI;
            }
        }

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

            PacketStorageFilters.Init();
            PacketStorageFiltersNew.Init();

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
        }

        public override void OnHudCreated(Hud hud)
        {
            StockpileManager.OnHudCreated(hud);
        }

        public override void OnContextActionBarCreated(ContextActionBar.ContextActionBarArgs args)
        {
            foreach (var comp in Engine.Map.GetTown().TownComponents)
                comp.OnContextActionBarCreated(args);
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
    }
}
