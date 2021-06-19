using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.GameModes
{
    class SandboxServerEventHandler : ServerEventHandler
    {
        public override void HandleEvent(Server server, GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.EntityChangedChunk:
                    // load/unload chunks and send:
                    //"changedchunk".ToConsole();
                    GameObject entity = e.Parameters[0] as GameObject;
                    PlayerData player = server.Players.GetList().FirstOrDefault(p=>p.Character == entity);
                    if(player.IsNull())
                        break;

                    Vector2 next = (Vector2)e.Parameters[1];
                    Vector2 prev = (Vector2)e.Parameters[2];

                    //var nextNeighbors = next.GetNeighbors();//GetSpiral(3);
                    //var prevNeighbors = prev.GetNeighbors();//GetSpiral(3);
                    var nextNeighbors = next.GetSpiral();
                    var prevNeighbors = prev.GetSpiral();
                    nextNeighbors.Add(next);
                    prevNeighbors.Add(prev);
                    var chunksToUnload = prevNeighbors.Except(nextNeighbors).ToList();
                    var chunksToLoad = nextNeighbors.Except(prevNeighbors).ToList();

                    foreach(var vector in chunksToLoad)
                    {
                        Chunk loaded;
                        // for each chunk in range, reset unload timer
                        if (server.Map.GetActiveChunks().TryGetValue(vector, out loaded))
                            loaded.UnloadTimer = Chunk.UnloadTimerMax;
                    }

                    //Task.Factory.StartNew(() =>
                    //{
                     //   server.SendChunks(chunksToLoad, player);
                    //});

                    //foreach (var chunk in chunksToUnload)
                    //{
                    //    // unload only chunks that are out of range of every player
                    //    server.UnloadChunk(chunk);
                    //}
                    break;

                case Message.Types.EntityEnteringUnloadedChunk:
                    // we don't want to despawn npcs! or save them first and then despawn them? but save them where?
                    //this.EntityEnteringUnloadedChunk(server, e.Parameters[0] as GameObject);
                    break;

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
