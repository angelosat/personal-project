using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class BlockEntityCompSwitchable : BlockEntityComp
    {
        public bool SwitchedOn { get; private set; } = true;
        public bool IsSwitchedOn()
        {
            return this.SwitchedOn;
        }
        public void Toggle(GameObject actor, TargetArgs target)
        {
            this.SwitchedOn = !this.SwitchedOn;
            //actor.Map.EventOccured(Components.Message.Types.SwitchToggled, target.Global);
            actor.Map.Town.DesignationManager.RemoveDesignation(DesignationDef.Switch, target.Global);
        }
        public override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.SwitchedOn.Save("SwitchedOn"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<bool>("SwitchedOn", v => this.SwitchedOn = v);
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.SwitchedOn);
        }
        public override ISerializable Read(BinaryReader r)
        {
            this.SwitchedOn = r.ReadBoolean();
            return this;
        }
    }
}
