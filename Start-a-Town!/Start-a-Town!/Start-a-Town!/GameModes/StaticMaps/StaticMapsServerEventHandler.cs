using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class StaticMapsServerEventHandler : ServerEventHandler
    {
        public override void HandleEvent(Server server, GameEvent e)
        {
            switch (e.Type)
            {
                 
                default:
                    break;
            }
        }

        void EntityEnteringUnloadedChunk(Server server, GameObject entity)
        {
            foreach (var pl in server.GetPlayers())
                if (pl.Character == entity)
                {
                    // TODO: oops! do something?
                    return;
                }
            server.SyncDespawn(entity);
            server.SyncDisposeObject(entity);
        }

        static public Func<PlayerData, GameObject, Func<PlayerData, bool>> PlayerChunkDistance =
            (pl, obj) => (p) => Vector2.Distance(Server.Instance.Map.GetChunk(p.Character.Global).MapCoords, Server.Instance.Map.GetChunk(obj.Global).MapCoords) < Engine.ChunkRadius;
            //(pl, obj) => (p) => Vector2.Distance(p.Character.Global.GetChunk(Server.Instance.Map).MapCoords, obj.Global.GetChunk(Server.Instance.Map).MapCoords) < Engine.ChunkRadius;
        
    }
}
