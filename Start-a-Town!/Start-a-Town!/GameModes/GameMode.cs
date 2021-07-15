using System.Collections.Generic;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes.StaticMaps;
using Start_a_Town_.Rooms;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameModes
{
    abstract class GameMode
    {
        static public GameMode Current;

        static List<GameMode> _Registry;
        static public List<GameMode> Registry
        {
            get
            {
                if (_Registry == null)
                {
                    _Registry = new List<GameMode>()
                    {
                        StaticMaps
                    };
                }
                return _Registry;
            }
        }

        public string Name;
        
        internal abstract void OnMainMenuCreated(MainMenuWindow mainmenu);

        public virtual void ParseCommand(INetwork net, string command)
        {

        }

        protected List<GameComponent> GameComponents = new List<GameComponent>();
        public abstract GameScreen GetWorldSelectScreen(INetwork net);
        public static readonly GameMode StaticMaps = new GameModeStaticMaps();

        public virtual void OnIngameMenuCreated(IngameMenu menu) { }
        public virtual void OnHudCreated(Hud hud)
        {
            foreach (var comp in this.GameComponents)
                comp.OnHudCreated(hud);
        }
        public virtual void OnUIEvent(UIManager.Events e, params object[] p)
        {
            foreach (var comp in this.GameComponents)
                comp.OnUIEvent(e, p);
        }
        
        internal virtual void HandlePacket(INetwork net, PacketType type, System.IO.BinaryReader r)
        {
            foreach (var gc in this.GameComponents)
                gc.HandlePacket(net, type, r);
        }
        public virtual void HandlePacket(Server server, Packet msg)
        {
            foreach (var gc in this.GameComponents)
                gc.HandlePacket(server, msg);
        }
        public virtual void HandlePacket(Client client, Packet msg)
        {
            foreach (var gc in this.GameComponents)
                gc.HandlePacket(client, msg);
        }

        public abstract bool IsPlayerWithinRangeForPacket(PlayerData playerData, Vector3 packetEventGlobal);
      
        internal virtual void PlayerConnected(Server server, PlayerData player) { }
        internal virtual void PlayerIDAssigned(Client client) { }
        internal virtual void MapReceived(MapBase map) { }
        internal virtual void Update(Client client) { }
        internal virtual void Update(Server server) { }

        internal abstract Control Load();
        internal virtual Control GetNewGameGui() { return null; }

        internal virtual void ChunkReceived(Server server, int playerid, Vector2 vec) { }
        internal virtual void HandleEvent(INetwork net, GameEvent e) { }
        internal virtual void HandleEvent(INetwork net, object e, object[] p) { }
        internal virtual void AllChunksReceived(INetwork net) { }
    }
}
