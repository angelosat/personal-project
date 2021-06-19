using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    public abstract class TownComponent
    {
        public Town Town;

        const float UpdateFrequency = 1; // per second
        float UpdateTimerMax = (float)Engine.TargetFps / UpdateFrequency;
        float UpdateTimer;
        //TownComponent() { }
        //protected TownComponent(Town town)
        //{
        //    // TODO: Complete member initialization
        //    this.Town = town;
        //}

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

        //public abstract GroupBox GetInterface();
        public virtual GroupBox GetInterface()
        {
            return null;
        }


        public virtual void Handle(Net.IObjectProvider net, Net.Packet msg) { }
        public virtual void HandlePacket(Net.Server server, Net.Packet msg)
        {
            this.Handle(server, msg);
        }

        public virtual void HandlePacket(Net.Client client, Net.Packet msg)
        {
            this.Handle(client, msg);
        }

        internal virtual void OnContextMenuCreated(IContextable obj, ContextArgs a)
        {
        }

        internal virtual void OnGameEvent(GameEvent e)
        {
        }

        public virtual List<SaveTag> Save() { return new List<SaveTag>(); }
        public virtual void Load(SaveTag tag) { }
        public virtual void Write(BinaryWriter w) { }
        public virtual void Read(BinaryReader r) { }
    }
}
