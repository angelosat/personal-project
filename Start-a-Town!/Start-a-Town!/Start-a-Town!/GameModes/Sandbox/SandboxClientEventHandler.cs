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
    class SandboxClientEventHandler : ClientEventHandler
    {
        public override void HandleEvent(Client client, GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.EntityChangedChunk:
                    GameObject entity = e.Parameters[0] as GameObject;
                    if (entity != Player.Actor)
                        break;
                    Vector2 next = (Vector2)e.Parameters[1];
                    Vector2 prev = (Vector2)e.Parameters[2];

                    var nextNeighbors = next.GetSpiral();
                    var prevNeighbors = prev.GetSpiral();
                    nextNeighbors.Add(next);
                    prevNeighbors.Add(prev);
                    var chunksToUnload = prevNeighbors.Except(nextNeighbors).ToList();
                    var chunksToLoad = nextNeighbors.Except(prevNeighbors).ToList();

                    foreach (var chunk in chunksToLoad)
                    {
                        // if the chunk hasn't unloaded yet (is already active), reset unload timer, otherwise request chunk from server (?)
                        Chunk loaded;
                        if (client.Map.GetActiveChunks().TryGetValue(chunk, out loaded))
                            loaded.UnloadTimer = Chunk.UnloadTimerMax;// -1;
                        else
                            // dont request chunks maybe? just wait for the server to load on its own and send?
                            Network.Serialize(w => w.Write(chunk)).Send(Client.PacketID, PacketType.RequestChunk, Client.Host, Client.RemoteIP);
                    }

                    //foreach (var chunk in chunksToUnload)
                    //{
                    //    // unload only chunks that are out of range of every player
                    //    client.UnloadChunk(chunk);
                    //}
                    break;

                case Message.Types.EntityEnteringUnloadedChunk:
                    // we don't want to despawn npcs! or save them first and then despawn them? but save them where?
                    //this.EntityEnteringUnloadedChunk(client, e.Parameters[0] as GameObject);
                    break;

                default:
                    break;
            }
        }

        void EntityEnteringUnloadedChunk(Client client, GameObject entity)
        {
            if(entity == Player.Actor)
            {
                // TODO: do something?
                return;
            }
            client.Despawn(entity);
            client.DisposeObject(entity);
        }
    }
}
