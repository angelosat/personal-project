using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class GameComponent
    {
        public virtual void Initialize() { }
        public virtual void InitHUD(Hud hud) { }
        public virtual void OnGameEvent(GameEvent e) { }

        public virtual void HandlePacket(Server server, Packet msg) { }
        public virtual void HandlePacket(Client client, Packet msg) { }
        internal virtual void HandlePacket(INetwork net, PacketType type, System.IO.BinaryReader r) { }

        public virtual void OnHudCreated(Hud hud) { }
        public virtual void OnContextMenuCreated(IContextable obj, ContextArgs a) { }
        public virtual void OnTargetInterfaceCreated(TargetArgs t, Control ui) { }
        public virtual void OnContextActionBarCreated(ContextActionBar.ContextActionBarArgs a) { }
        public virtual void OnTooltipCreated(ITooltippable item, Tooltip t) { }

        public virtual void OnUIEvent(UIManager.Events e, object[] p) { }
    }
}
