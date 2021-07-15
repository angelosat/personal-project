using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class Resource : IProgressBar, ISaveable, ISerializable, INamed
    {
        public readonly ResourceDef ResourceDef;
        public List<ResourceRateModifier> Modifiers = new List<ResourceRateModifier>();
        public float Max;
        float _Value;
        public float Value
        {
            get{ return this._Value; }
            set { this._Value = Math.Max(0, Math.Min(value, this.Max)); }
        }

        public Progress Rec = ResourceDef.Recovery;
        public float Percentage { get { return this.Value / this.Max; } set { this.Value = this.Max * value; } }
        public float Min => 0;

        public string Name => this.ResourceDef.Name;

        public Resource(ResourceDef def)
        {
            this.ResourceDef = def;
            this.Max = def.BaseMax;
            this.Value = this.Max;
        }

        public void Tick(GameObject parent)
        {
            this.ResourceDef.Tick(parent, this);
        }

        internal virtual void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        {
            this.ResourceDef.HandleRemoteCall(parent, e, this);
        }
        public void SyncAdjust(Entity parent, float value)
        {
            Packets.SendSyncAdjust(parent, this.ResourceDef, value);
        }
        public void Adjust(float add)
        {
            this.ResourceDef.Add(add, this);
        }
        public Resource Initialize(float max, float initPercentage)
        {
            this.Value = this.Max =  max * initPercentage;
            return this;
        }
        internal Resource Clone()
        {
            return new Resource(this.ResourceDef) { Max = this.Max, Value = this.Value, Rec = new Progress(0, this.Rec.Max, this.Rec.Value) };// this.Rec.Clone() };
        }

        internal void HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            this.ResourceDef.HandleMessage(this, parent, e);
        }

        internal void OnNameplateCreated(GameObject parent, Nameplate plate)
        {
            this.ResourceDef.OnHealthBarCreated(parent, plate, this);
        }

        internal void OnHealthBarCreated(GameObject parent, Nameplate plate)
        {
            this.ResourceDef.OnHealthBarCreated(parent, plate, this);
        }

        internal Control GetControl()
        {
            return this.ResourceDef.GetControl(this);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} / {2}", this.ResourceDef.Name, this.Value.ToString(this.ResourceDef.Format), this.Max.ToString(this.ResourceDef.Format));
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, this.ResourceDef.Name);
            tag.Add(this.Value.Save("Value"));
            tag.Add(this.Max.Save("Max"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValue("Value", out this._Value);
            tag.TryGetTagValue("Max", out this.Max);
            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this._Value);
            w.Write(this.Max);
        }

        public ISerializable Read(BinaryReader r)
        {
            this._Value = r.ReadSingle();
            this.Max = r.ReadSingle();
            return this;
        }

        internal void AddModifier(ResourceRateModifier resourceModifier)
        {
            if (this.Modifiers.Any(m => m.Def == resourceModifier.Def))
                throw new Exception();
            this.Modifiers.Add(resourceModifier);
        }

        internal float GetThresholdDepth()
        {
            return this.ResourceDef.GetThresholdDepth(this);
        }
        public float GetThresholdValue(int index)
        {
            return this.ResourceDef.GetThresholdValue(this, index);
        }
        static Resource()
        {
            Packets.Init();
        }
        class Packets
        {
            static int PacketSyncAdjust;
            public static void Init()
            {
                PacketSyncAdjust = Network.RegisterPacketHandler(HandleSyncAdjust);
            }
            public static void SendSyncAdjust(Entity actor, ResourceDef def, float value)
            {
                var net = actor.Net;
                if (net is Server)
                    actor.GetResource(def).Adjust(value);
                net.GetOutgoingStream().Write(PacketSyncAdjust, actor.RefID, def.Name, value);
            }
            private static void HandleSyncAdjust(INetwork net, BinaryReader r)
            {
                var actor = net.GetNetworkObject(r.ReadInt32()) as Actor;
                var resource = Def.GetDef<ResourceDef>(r.ReadString());
                var value = r.ReadSingle();
                if (net is Server)
                    SendSyncAdjust(actor, resource, value);
                else
                    actor.GetResource(resource).Adjust(value);
            }
        }
    }
}
