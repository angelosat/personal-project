using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class TownsManager : GameComponent
    {
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
            PacketPlayerInputDirection.Init();
            PacketPlayerToggleMove.Init();
            PacketPlayerToggleWalk.Init();
            PacketPlayerToggleSprint.Init();
            PacketPlayerJump.Init();

            PacketPlayerSetBlock.Init();

            PacketDiggingDesignate.Init();
            PacketEntityDesignation.Init();
            PacketToggleForbidden.Init();

            //PacketStorageFiltersNew.Init();

            ZoneManager.Init();
        }

        public override void OnGameEvent(GameEvent e)
        {
            return;
        }

        public override void OnContextActionBarCreated(ContextActionBar.ContextActionBarArgs args)
        {
            foreach (var comp in Engine.Map.Town.TownComponents)
                comp.OnContextActionBarCreated(args);
        }
    }
}
