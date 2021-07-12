using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Farming;

namespace Start_a_Town_.Towns
{
    class TownsManager : GameComponent
    {
        TownsUI TownsUI;
        
        public override void Initialize()
        {
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
            foreach (var comp in Engine.Map.Town.TownComponents)
                comp.OnContextActionBarCreated(args);
        }
       
        public override void HandlePacket(Server server, Packet msg)
        {
            var map = server.Map;
            if (map == null)
                return;
            var town = map.Town;
            if (town == null)
                return;
            town.HandlePacket(server, msg);
        }
        public override void HandlePacket(Client client, Packet msg)
        {
            var map = client.Map;
            if (map == null)
                return;
            var town = map.Town;
            if (town == null)
                return;
            town.HandlePacket(client, msg);
        }
        internal override void HandlePacket(IObjectProvider net, PacketType type, System.IO.BinaryReader r)
        {
            var map = net.Map;
            if (map == null)
                return;
            var town = map.Town;
            if (town == null)
                return;
            town.HandlePacket(net, type, r);
        }
    }
}
