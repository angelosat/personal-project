﻿using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Modules.Base
{
    class GameManager : GameComponent
    {
        public override void Initialize()
        {
            PacketEntityInstantiate.Init();
            PacketEntitySync.Init();

            PacketEntityMoveToggle.Init();
            PacketEntityWalkToggle.Init();
            PacketEntityCrouchToggle.Init();
            PacketEntitySprintToggle.Init();
            PacketEntityJump.Init();

            PacketEntityInteract.Init();

            PacketRandomBlockUpdates.Init();
            PacketSnapshots.Init();
            PacketPlayerConnecting.Init();
            PacketPlayerToolSwitch.Init();
            PacketChat.Init();
            PacketChunk.Init();
            PacketMousePosition.Init();
            PacketEntityRequestDispose.Init();
            PacketEntityRequestSpawn.Init();

            PacketEntityDespawn.Init();

            PacketPlayerDisconnected.Init();
        }

        public override void InitHUD(Hud hud)
        {
            NpcSkill.Init(hud);

            hud.RegisterEventHandler(Components.Message.Types.NeedUpdated, e =>
            {
                var actor = e.Parameters[0] as Actor;
                var need = e.Parameters[1] as Need;
                var value = (float)e.Parameters[2];
                FloatingText.Manager.Create(actor, string.Format("{0:+;-}{1}", value, need.NeedDef.Name),
                     ft =>
                     {
                         ft.Font = UIManager.FontBold;
                         ft.ColorFunc = () => value < 0 ? Color.Red : Color.Lime;
                     }
                );
            });
        }
    }
}
