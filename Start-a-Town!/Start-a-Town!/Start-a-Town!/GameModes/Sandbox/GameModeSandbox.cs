using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

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

        public override Rooms.GameScreen GetWorldSelectScreen()
        {
            return Rooms.WorldScreen.Instance.Initialize();
        }

        public override bool IsPlayerWithinRangeForPacket(GameObject actor, Vector3 packetEventGlobal)
        {
            var playerchunk = actor.GetChunk();
            var targetchunk = actor.Net.Map.GetChunk(packetEventGlobal);
            return Vector2.Distance(playerchunk.MapCoords, targetchunk.MapCoords) <= Engine.ChunkRadius;
        }
    }
}
