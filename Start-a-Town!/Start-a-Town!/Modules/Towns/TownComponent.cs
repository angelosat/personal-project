using System;
using System.Collections.Generic;
using System.IO;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Towns
{
    public abstract class TownComponent
    {
        public Town Town;
        public MapBase Map { get { return this.Town.Map; } }
        public INetwork Net { get { return this.Map.Net; } }
        const float UpdateFrequency = 1; // per second
        float UpdateTimerMax = (float)Engine.TicksPerSecond / UpdateFrequency;
        float UpdateTimer;
        protected TownComponent()
        {

        }
        public TownComponent(Town town)
        {
            this.Town = town;
        }

        public abstract string Name { get; }
        public virtual void Update()
        {
            if (this.UpdateTimer > 0)
            {
                this.UpdateTimer--;
                return;
            }
            this.UpdateTimer = UpdateTimerMax;
            this.OnUpdate();
        }
        public virtual void OnUpdate() { }

        public virtual GroupBox GetInterface()
        {
            return null;
        }

        public virtual void Handle(INetwork net, Net.Packet msg) { }
        public virtual void HandlePacket(INetwork net, Net.PacketType type, BinaryReader r) { }

        public virtual void HandlePacket(Server server, Packet msg)
        {
            this.Handle(server, msg);
        }

        public virtual void HandlePacket(Client client, Packet msg)
        {
            this.Handle(client, msg);
        }

        internal virtual IEnumerable<Tuple<string, Action>> OnQuickMenuCreated() { yield break; }
        internal virtual void OnContextMenuCreated(IContextable obj, ContextArgs a) { }
        internal virtual void OnContextActionBarCreated(ContextActionBar.ContextActionBarArgs a) { }
        internal virtual void OnGameEvent(GameEvent e)
        {
        }

        public SaveTag Save()
        {
            var tag = new SaveTag(SaveTag.Types.Compound, this.Name);
            this.AddSaveData(tag);
            return tag;
        }
        
        protected virtual void AddSaveData(SaveTag tag) { }

        public virtual void Load(SaveTag tag) { }
        public virtual void Write(BinaryWriter w) { }
        public virtual void Read(BinaryReader r) { }

        public virtual void GetManagementInterface(TargetArgs t, WindowTargetManagement inter) { }

        public virtual IContextable QueryPosition(Vector3 global) { return null; }
        public virtual ISelectable QuerySelectable(TargetArgs selected) { return null; }

        public virtual void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam) { }
        public virtual void DrawUI(SpriteBatch sb, MapBase map, Camera cam) { }

        internal virtual void UpdateQuickButtons() { }

        public virtual void Tick()
        {
        }
        internal virtual IEnumerable<(string name, Action action)> GetInfoTabs(ISelectable selected) { yield break; }

        internal virtual void OnTargetSelected(IUISelection info, ISelectable target)
        {
        }
        internal virtual void OnTargetSelected(IUISelection info, TargetArgs targetArgs)
        {
        }
        internal virtual void OnTooltipCreated(Control tooltip, TargetArgs targetArgs)
        {
        }

        internal virtual void OnCitizenAdded(int actorID) { }
        internal virtual void OnCitizenRemoved(int actorID) { }

        protected PlayerData GetPlayer() { return this.Town.Map.Net.GetPlayer(); }
        protected PlayerData Player { get { return this.Town.Map.Net.GetPlayer(); } }

        internal virtual void ResolveReferences() { }

        internal virtual void OnBlocksChanged(IEnumerable<IntVec3> positions) { }
    }
}
