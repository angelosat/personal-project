using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_.GameModes
{
    class GameModeSandbox : GameMode
    {
        public GameModeSandbox()
        {
            this.Name = "Infinite";
            this.ServerEventHandler = new SandboxServerEventHandler();
            this.ClientEventHandler = new SandboxClientEventHandler();
        }

        public override Rooms.GameScreen GetWorldSelectScreen(IObjectProvider net)
        {
            return Rooms.WorldScreen.Instance.Initialize(net);
        }

        public override bool IsPlayerWithinRangeForPacket(PlayerData player, Vector3 packetEventGlobal)
        {
            var actor = player.ControllingEntity;
            var playerchunk = actor.GetChunk();
            var targetchunk = actor.Net.Map.GetChunk(packetEventGlobal);
            return Vector2.Distance(playerchunk.MapCoords, targetchunk.MapCoords) <= Engine.ChunkRadius;
        }

        internal override Control Load()
        {
            throw new NotImplementedException();
        }

        internal override void OnMainMenuCreated(MainMenuWindow mainmenu)
        {
            throw new NotImplementedException();
        }
    }
}
