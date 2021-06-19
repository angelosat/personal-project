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
    class StaticMapsClientEventHandler : ClientEventHandler
    {
        public override void HandleEvent(Client client, GameEvent e)
        {
            switch (e.Type)
            {
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
