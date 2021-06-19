using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AICombatOld : Behavior
    {
        GameObject Target { get { return (GameObject)this["Target"]; } set { this["Target"] = value; } }
        float Timer { get { return (float)this["NeedsTimer"]; } set { this["NeedsTimer"] = value; } }

        public override string Name
        {
            get
            {
                return "Attacking " + (Target == Player.Actor ? "YOU!" : Target.Name);
            }
        }

        public AICombatOld(GameObject target = null)
        {
            this.Target = target;
            this.Timer = 60f;
        }

        public override BehaviorState Execute(GameObject parent, AIState state)//Net.IObjectProvider net, GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            //GameObject parent = state.Parent;
            //Net.IObjectProvider net = state.Net;
            Personality personality = state.Personality;
            Knowledge knowledge = state.Knowledge;
            //if (Child != null)
            //{
            //    if (!Child.Execute(parent, personality, memory))
            //        return false;
            //    Child = null;
            //}
            //if ((Target.Global - parent.Global).Length() > 1)
            //    return new AIChase(Target);
            if (Target == null)
            {
                foreach (GameObject obj in state.Knowledge.Objects.Keys)
                {
                    if (!obj.Exists)
                        continue;
                    if (state.Personality.Hatelist.Contains(obj.Type))
                    {
                        float aggroRange = 5;
                        if (Vector3.Distance(parent.Global, obj.Global) <= aggroRange)
                            Target = obj;
                    }
                }
                if (Target == null)
                    return BehaviorState.Fail;
            }
            var net = parent.Net;
            ControlComponent control = parent.GetComponent<ControlComponent>();

            Vector3 difference = (Target.Global - parent.Global);
            float dist = difference.Length();
            if (dist > 5)
            {
                //parent["AI"]["Current"] = new AIIdle();
                Target = null;
                control.FinishScript(Script.Types.Walk, new ScriptArgs(net, parent));
                control.TryGetScript(Script.Types.Attack, sc => sc.Stop(new ScriptArgs(net, parent))); ;
                return BehaviorState.Success;
            }

            control.TryStartScript(Script.Types.Attack, new ScriptArgs(net, parent));
            //parent.GetComponent<ControlComponent>().RunningScripts[Script.Types.Attack].AIControl(new AbilityArgs(net, parent, new TargetArgs(Target)));
            Vector3 direction;
            Vector3.Normalize(ref difference, out direction);
            parent.Direction = direction;
            control.TryGetScript(Script.Types.Attack, script => script.AIControl(new ScriptArgs(net, parent, new TargetArgs(Target))));

            if (dist > 1)
            {
                //difference.Normalize();
                //difference.Z = 0;
                

                //parent.PostMessage(Message.Types.Move, parent, difference, 1f);
                //parent.GetPosition().Direction = new Vector2(difference.X, difference.Y);


                control.TryStartScript(Script.Types.Walk, new ScriptArgs(net, parent));


                //net.PostLocalEvent(parent, Message.Types.StartScript, Script.Types.Walk);
                return BehaviorState.Running;
            }
            //throw new NotImplementedException();
            //   if (parent.GetComponent<ControlComponent>().RunningScripts.ContainsKey(Script.Types.Walk))

            //parent.GetComponent<ControlComponent>().RunningScripts[Script.Types.Attack].AIControl(new AbilityArgs(net, parent, new TargetArgs(Target)));
            control.FinishScript(Script.Types.Walk, new ScriptArgs(net, parent));
            //parent.PostMessage(Message.Types.Begin, parent, Target, Message.Types.Attack);
            return BehaviorState.Running;

            if ((float)Target["Health"]["Value"] > 0)
                return BehaviorState.Running;
            Target = null;
            return BehaviorState.Success;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Attack:
                    //Target = e.Sender as GameObject;
                    SetTarget(parent, e.Sender as GameObject);
                    return false;// true;
                case Message.Types.Aggro:
                    SetTarget(parent, e.Sender as GameObject);
                    return true;
                case Message.Types.SetTarget:
                    SetTarget(parent, e.Parameters[0] as GameObject);
                    return true;


                case Message.Types.BlockCollision:
                    //if (!(parent.Global + parent.Direction + Vector3.UnitZ).IsSolid(e.Network.Map))
                    if (!e.Network.Map.IsSolid(parent.Global + parent.Direction + Vector3.UnitZ))
                        parent.GetComponent<MobileComponent>().Jump(parent);
                        //parent.GetComponent<ControlComponent>().TryStartScript(Script.Types.Jumping, new ScriptArgs(e.Network, parent));
                    return true;

                default: return false;
            }
        }

        void SetTarget(GameObject parent, GameObject target)
        {
            if (Target == target)
                return;
            Target = target;
            if (target != null)
                throw new NotImplementedException();
                //target.PostMessage(Message.Types.Aggro, parent);
        }
        void ClearTarget()
        {
            Target = null;
        }

        public override object Clone()
        {
            return new AICombatOld();
        }
    }
}
