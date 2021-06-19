using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class GameComponent
    {
        public virtual void Initialize() { }
        public virtual void InitHUD(UI.Hud hud) { }
        public virtual void OnGameEvent(GameEvent e) { }
        //void DrawWorld(MySpriteBatch sb, Map map, Camera cam);

        public virtual void HandlePacket(Server server, Packet msg) { }
        public virtual void HandlePacket(Client client, Packet msg) { }

        public virtual void OnHudCreated(Hud hud) { }
        public virtual void OnContextMenuCreated(IContextable obj, ContextArgs a) { }

        //public virtual void Update() { }
    }

    //public interface IGameComponent
    //{
    //    void Initialize();
    //    void InitHUD(UI.Hud hud);
    //    void OnGameEvent(GameEvent e);
    //    //void DrawWorld(MySpriteBatch sb, Map map, Camera cam);
    //}
}
