using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.Net
{
    public partial class Server
    {
        public class ServerEventHandler
        {
            //public void HandleEvent(Message.Types msg, object[] args)
            public void HandleEvent(GameEvent e)
            {
                switch (e.Type)
                {
                    case Message.Types.EntityChangedChunk:
                        // load/unload chunks and send:
                        //"changedchunk".ToConsole();
                        Vector2 next = (Vector2)e.Parameters[0];
                        Vector2 prev = (Vector2)e.Parameters[1];

                        //var nextNeighbors = next.GetNeighbors();//GetSpiral(3);
                        //var prevNeighbors = prev.GetNeighbors();//GetSpiral(3);
                              var nextNeighbors = next.GetSpiral();
                        var prevNeighbors = prev.GetSpiral();
                        nextNeighbors.Add(next);
                        prevNeighbors.Add(prev);
                        var chunksToUnload = prevNeighbors.Except(nextNeighbors).ToList();
                        var chunksToLoad = nextNeighbors.Except(prevNeighbors).ToList();

                        Task.Factory.StartNew(() =>
                        {
                            Instance.LoadChunks(chunksToLoad);
                        });

                        foreach (var chunk in chunksToUnload)
                        {
                            Instance.UnloadChunk(chunk);
                        }
                        break;

                    default:
                        break;
                }
            }

        }
    }
}
