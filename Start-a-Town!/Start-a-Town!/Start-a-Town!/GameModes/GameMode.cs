using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                        //Sandbox, 
                        StaticMaps
                    };
                }
                return _Registry;
            }
        }

        public string Name;
        //public IPacketHandler ServerPacketHandler, ClientPacketHandler;
        //public IEventHandler ServerEventHandler, ClientEventHandler;
        public ServerEventHandler ServerEventHandler;
        public ClientEventHandler ClientEventHandler;
        //public ServerPacketHandler ServerPacketHandler;
        //public ClientPacketHandler ClientPacketHandler;
        public PacketHandler PacketHandler;

        public virtual void ParseCommand(IObjectProvider net, string command)
        {

        }

        protected List<GameComponent> GameComponents = new List<GameComponent>();

        //public List<GameComponent> GameComponents = new List<GameComponent>();

        public abstract GameScreen GetWorldSelectScreen();

        public static readonly GameMode Sandbox = new GameModeSandbox();
        public static readonly GameMode StaticMaps = new GameModeStaticMaps();

        public virtual void OnIngameMenuCreated(IngameMenu menu) { }
        public virtual void OnHudCreated(Hud hud)
        {
            foreach (var comp in this.GameComponents)
                comp.OnHudCreated(hud);
        }

        //public void Initialize()
        //{
        //    foreach (var comp in this.GameComponents)
        //    {
        //        comp.Initialize();
        //    }
        //}

        public virtual void HandlePacket(Server server, Packet msg)
        {
            this.PacketHandler.Handle(server, msg);
            foreach (var gc in this.GameComponents)
                gc.HandlePacket(server, msg);
        }
        public virtual void HandlePacket(Client client, Packet msg)
        {
            this.PacketHandler.Handle(client, msg);
            foreach (var gc in this.GameComponents)
                gc.HandlePacket(client, msg);
        }

        public abstract bool IsPlayerWithinRangeForPacket(GameObject actor, Vector3 packetEventGlobal);
        public virtual IEnterior GetEnterior(IMap map, Vector3 global) { return null; }

        //public virtual void Update()
        //{
        //    foreach (var comp in this.GameComponents)
        //        comp.Update();
        //}
    }
}
