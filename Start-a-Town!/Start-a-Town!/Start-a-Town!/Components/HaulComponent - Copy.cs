using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class HaulComponent : Component
    {
        public override string ComponentName
        {
            get { return "Haul"; }
        }

        public GameObjectSlot Holding
        {
            get { return (GameObjectSlot)this["Holding"]; }
            set
            {
                this["Holding"] = value;
            }
        }

        public override void MakeChildOf(GameObject parent)
        {
            this.Holding.Parent = parent;
        }

        public HaulComponent()
        {
            this.Holding = GameObjectSlot.Empty;
        }

        static public void TryGetHeldObject(GameObject entity, Action<GameObjectSlot> action)
        {
            entity.TryGetComponent<HaulComponent>(i =>
            {
                if (i.Holding.HasValue)
                    action(i.Holding);
            });
        }
        static public GameObjectSlot GetHeldObject(GameObject entity)
        {
            HaulComponent inv;
            if (!entity.TryGetComponent<HaulComponent>(out inv))
                return GameObjectSlot.Empty;
            return inv.Holding;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.UseHauledItem:
                    e.Translate(r =>
                    {
                        TargetArgs t = TargetArgs.Read(e.Network, r);
                        if (!this.Holding.HasValue)
                            return;
                        UseComponent use;
                        if (!this.Holding.Object.TryGetComponent<UseComponent>(out use))
                            return;
                        parent.GetComponent<ControlComponent>().TryStartScript(use.InstantiatedScripts.FirstOrDefault(), new ScriptArgs(e.Network, parent, t));
                    });
                    return true;
                
                default:
                    return true;
            }
        }

        public bool Hold(Net.IObjectProvider net, GameObject parent, GameObjectSlot objSlot)
        {
            if (objSlot == null)
                return true;
            if (!objSlot.HasValue)
                return true;

            if (this.Holding.HasValue)
                net.PostLocalEvent(parent, Message.Types.Receive, this.Holding.Object);

            this.Holding.Clear();
            net.Despawn(objSlot.Object);
            this.Holding.Swap(objSlot);
            return true;
        }

        public bool Throw(Net.IObjectProvider net, Vector3 velocity, GameObject parent)
        {
            if (!this.Holding.HasValue)
                return false;
            GameObject newobj = this.Holding.Take();
            newobj.Global = parent.Global + new Vector3(0, 0, parent.GetPhysics().Height);
            newobj.Velocity = velocity;
            net.Spawn(newobj);
            return true;
        }

        public override string GetStats()
        {
            string text = "";
            if (Holding.HasValue)
                text += Holding.Object.GetStats();
            return text;
        }

        public override object Clone()
        {
            return new HaulComponent();
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();

            if (this.Holding.HasValue)
                data.Add(new SaveTag(SaveTag.Types.Compound, "Holding", this.Holding.Save()));

            return data;
        }
        internal override void Load(SaveTag data)
        {
            List<SaveTag> tag = data.Value as List<SaveTag>;
            Dictionary<string, SaveTag> byName = tag.ToDictionary(foo => foo.Name);
            if (byName.ContainsKey("Holding"))
            {
                SaveTag haulTag = byName["Holding"];
                this.Holding = GameObjectSlot.Create(haulTag);
            }

        }
    }
}
