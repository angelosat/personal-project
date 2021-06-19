using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class Threat : IComparable<Threat>
    {
        public float Value { get; set; }
        public GameObject Parent { get; set; }
        public GameObject Entity { get; set; }
        public Threat(GameObject parent, float value, GameObject entity)
        {
            this.Parent = parent;
            this.Value = value;
            this.Entity = entity;
        }
        public int CompareTo(Threat other)
        {
            if (this.Entity == other.Entity)
                return 0;
            if (this.Value != other.Value)
                return this.Value > other.Value ? -1 : 1;
            float thisDistance = Vector3.Distance(this.Parent.Global, this.Entity.Global);
            float otherDistance = Vector3.Distance(this.Entity.Global, other.Entity.Global);
            if (thisDistance != otherDistance)
                return thisDistance < otherDistance ? -1 : 1;
            return this.Entity.Network.ID < other.Entity.Network.ID ? -1 : 1;
        }
    }
    class AIAggro : Behavior
    {
        SortedSet<Threat> Threats { get; set; }
        GameObject Target { get { return Threats.FirstOrDefault().Entity; } }
        float Timer { get { return (float)this["NeedsTimer"]; } set { this["NeedsTimer"] = value; } }
        float Range { get; set; }
        public override string Name
        {
            get
            {
                return this.Target.IsNull() ? "No Target" : "Attacking " + (Target == Player.Actor ? "YOU!" : Target.Name);
            }
        }
        public AIAggro()
        {
            this.Threats = new SortedSet<Threat>();
            this.Timer = Engine.TargetFps;
            this.Range = 5;
        }

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            Personality personality = state.Personality;
            Knowledge knowledge = state.Knowledge;
            var net = parent.Net;
            Threat Highest = this.Threats.FirstOrDefault();
            if (Highest.IsNull())
                return BehaviorState.Fail;
            GameObject Target = Highest.Entity;

            ControlComponent control = parent.GetComponent<ControlComponent>();

            Vector3 difference = (Target.Global - parent.Global);
            float dist = difference.Length();
            if (dist > this.Range)
            {
                // start timer instead of removing straight away
                this.Threats.Remove(Highest);
                control.FinishScript(Script.Types.Walk, new ScriptArgs(net, parent));
                control.TryGetScript(Script.Types.Attack, sc => sc.Stop(new ScriptArgs(net, parent))); ;
                return BehaviorState.Success;
            }

            control.TryStartScript(Script.Types.Attack, new ScriptArgs(net, parent));

            Vector3 direction;
            Vector3.Normalize(ref difference, out direction);
            parent.Direction = direction;
            control.TryGetScript(Script.Types.Attack, script => script.AIControl(new ScriptArgs(net, parent, new TargetArgs(Target))));

            if (dist > 1)
            {
                control.TryStartScript(Script.Types.Walk, new ScriptArgs(net, parent));
                return BehaviorState.Running;
            }
            control.FinishScript(Script.Types.Walk, new ScriptArgs(net, parent));
            return BehaviorState.Running;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Attack:
                    SetTarget(parent, e.Sender as GameObject);
                    return false;// true;
                case Message.Types.Aggro:
                    SetTarget(parent, e.Sender as GameObject);
                    return true;
                case Message.Types.SetTarget:
                    SetTarget(parent, e.Parameters[0] as GameObject);
                    return true;

                case Message.Types.BlockCollision:
                    var obstacle = (Vector3)e.Parameters[0];
                    if (!e.Network.Map.IsSolid(obstacle + Vector3.UnitZ))
                        parent.GetComponent<MobileComponent>().Jump(parent);
                    //if (!e.Network.Map.IsSolid(parent.Global + parent.Direction + Vector3.UnitZ))
                    //    parent.GetComponent<MobileComponent>().Jump(parent);
                    return true;

                case Message.Types.Threat:
                    GameObject tar = e.Parameters[0] as GameObject;
                    if (tar.IsNull())
                        return true;
                    if (Vector3.Distance(tar.Global, parent.Global) <= this.Range)
                        this.Threats.Add(new Threat(parent, 100, tar));
                    return true;

                default: return false;
            }
        }

        void SetTarget(GameObject parent, GameObject target)
        {
        }
        void ClearTarget()
        {
        }

        public override object Clone()
        {
            return new AIAggro();
        }
    }
}
