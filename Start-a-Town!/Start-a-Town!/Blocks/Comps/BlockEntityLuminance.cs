using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    class BlockEntityLuminance : BlockEntityComp
    {
        readonly IPowerSource PowerSource;
        readonly byte Intensity;
        readonly int Consumption;
        readonly static int ConsumptionRate = Engine.TicksPerSecond;// * 10;
        readonly Func<bool> IsSwitchedOn;

        public bool Powered;
        int ConsumptionTick = 0;

        public BlockEntityLuminance(byte intensity, IPowerSource powerSource, int consumption, Func<bool> isSwitchedOn = null)
        {
            this.Intensity = intensity;
            this.PowerSource = powerSource;
            this.Consumption = consumption;
            this.IsSwitchedOn = isSwitchedOn ?? (() => true);
        }
        public override void Tick(IObjectProvider net, BlockEntity entity, Vector3 global)
        {
            base.Tick(net, entity, global);
            var isOn = this.IsSwitchedOn();
            if (isOn)
            {
                if (this.Powered)
                {
                    this.ConsumptionTick++;
                    if (this.ConsumptionTick >= ConsumptionRate)
                    {
                        this.ConsumptionTick = 0;
                        this.PowerSource.ConsumePower(net.Map, this.Consumption);
                        if (!this.PowerSource.HasAvailablePower(this.Consumption))
                            this.TurnOff(net.Map, global);
                    }
                }
                else
                {
                    if (this.PowerSource.HasAvailablePower(this.Consumption))
                        this.TurnOn(net.Map, global);
                }
            }
            else
            {
                if(this.Powered)
                    this.TurnOff(net.Map, global);
            }

            //if (this.ConsumptionTick >= ConsumptionRate)
            //{
            //    this.ConsumptionTick = 0;
            //    if (this.Powered)
            //    {
            //        this.PowerSource.ConsumePower(net.Map, this.Consumption);
            //        if (!this.PowerSource.HasAvailablePower(this.Consumption))
            //            this.TurnOff(net.Map, global);
            //    }
            //    else
            //    {
            //        if (this.PowerSource.HasAvailablePower(this.Consumption))
            //            this.TurnOn(net.Map, global);
            //    }
            //}
        }
        void Toggle(IMap map, Vector3 global)
        {
            map.SetBlockLuminance(global, this.Powered ? (byte)0 : this.Intensity);
        }
        void TurnOn(IMap map, Vector3 global)
        {
            this.Powered = true;
            map.SetBlockLuminance(global, this.Intensity);
        }
        void TurnOff(IMap map, Vector3 global)
        {
            this.Powered = false;
            map.SetBlockLuminance(global, (byte)0);
        }

        public override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.ConsumptionTick.Save("Tick"));
            tag.Add(this.Powered.Save("Powered"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("Tick", out this.ConsumptionTick);
            tag.TryGetTagValue<bool>("Powered", out this.Powered);

        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.Powered);
            w.Write(this.ConsumptionTick);
        }
        public override ISerializable Read(BinaryReader r)
        {
            this.Powered = r.ReadBoolean();
            this.ConsumptionTick = r.ReadInt32();
            return this;
        }
    }
}
