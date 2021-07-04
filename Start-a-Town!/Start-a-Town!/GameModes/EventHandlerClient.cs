using Start_a_Town_.Net;

namespace Start_a_Town_.GameModes
{
    abstract class EventHandlerClient
    {
        public abstract void HandleEvent(Client client, GameEvent e);
    }
}
