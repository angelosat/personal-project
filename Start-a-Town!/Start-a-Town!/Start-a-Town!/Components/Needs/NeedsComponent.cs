using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Needs;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class NeedsComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Needs";
            }
        }
        public NeedsHierarchy NeedsHierarchy { get { return (NeedsHierarchy)this["NeedsHierarchy"]; } set { this["NeedsHierarchy"] = value; } }
      //  public NeedsCollection Needs { get { return (NeedsCollection)this["Needs"]; } set { this["Needs"] = value; } }
        float Timer { get { return (float)this["NeedsTimer"]; } set { this["NeedsTimer"] = value; } }

        public NeedsComponent(params Need[] needs)
        {
            this.NeedsHierarchy = new NeedsHierarchy();
            this.Timer = Engine.TargetFps;
            //this.Needs = new NeedsCollection();
            //foreach (Need need in needs)
            //    this.Needs.Add(need.Name, need);
        }

        public NeedsComponent()
        {
            this.NeedsHierarchy = new NeedsHierarchy();
            this.Timer = Engine.TargetFps;
        }

        public NeedsComponent Initialize(NeedsHierarchy needs)
        {
            this.NeedsHierarchy = needs;
            this.Timer = Engine.TargetFps;
            return this;
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            //    float lowest = 100;
            Timer -= 1;// GlobalVars.DeltaTime;
            if (Timer > 0)
                return;

            Timer = Engine.TargetFps;
            //foreach (Need need in Needs.Values)
            //{
            //    need.Update();
            //    //float d = need.Decay * (1 + 5 * (1 - need.Value / 100f) * (1 - need.Value / 100f));
            //    //float newValue = Math.Max(0, need.Value - d);//need.Decay);
            //    //need.Value = newValue;
            //}
            this.NeedsHierarchy.Update(parent);
        }

        public override object Clone()
        {
            return new NeedsComponent() { NeedsHierarchy = this.NeedsHierarchy.Clone() }; //this.Needs.Values.ToArray()
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                //case Message.Types.ModifyNeed:
                //    string name = (string)e.Parameters[0];
                //    Need n;
                //    if (!Needs.TryGetValue(name, out n))
                //        return true;
                //    n.Value = Math.Max(0, Math.Min(n.Value += (int)e.Parameters[1], 100));
                //    return true;

                case Message.Types.ModifyNeed:
                    string name = (string)e.Parameters[0];
                    float value = (float)e.Parameters[1];
                    foreach (var level in this.NeedsHierarchy.Inner)
                        foreach (var need in level.Value)
                            if (need.Value.Name == name)
                                need.Value.Value = MathHelper.Clamp(need.Value.Value + value, 0, 100);
                    return true;

                default:
                    return false;
            }
        }

        static public Need GetNeed(GameObject actor, Need.Types type)
        {
            NeedsComponent comp;
            if (!actor.TryGetComponent<NeedsComponent>(out comp))
                return null;
            foreach(var needcollection in comp.NeedsHierarchy.Values)
            {
                Need found;
                if (needcollection.TryGetValue(type, out found))
                    return found;
            }
            return null;
        }

        static public Need ModifyNeed(GameObject actor, Need.Types type, float value)
        {
            var need = GetNeed(actor, type);
            need.SetValue(need.Value + value, actor);
            return need;
        }

        /// <summary>
        /// Returns true if the specified need is the agent's lowest need.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="need"></param>
        /// <returns></returns>
        //static public bool HasNeed(GameObject subject,  string need)
        //{
        //    NeedsComponent needs;
        //    if(!subject.TryGetComponent<NeedsComponent>("Needs", out needs))
        //        return false;
        //    return (needs.Needs.Values.OrderBy(n => n.Value).First().Name == need);
        //}

        //static public float GetNeed(GameObject subject, string need)
        //{
        //    NeedsComponent needs = subject.GetComponent<NeedsComponent>("Needs");
        //    return needs.Needs[need].Value;
        //}

        //static public bool TryGetNeed(GameObject subject, string need, out float value)
        //{
        //    NeedsComponent needs;
        //    value = 0;
        //    if (!subject.TryGetComponent<NeedsComponent>("Needs", out needs))
        //        return false;
        //    if (!needs.Needs.ContainsKey(need))
        //        return false;
        //    value = needs.Needs[need].Value;
        //    return true;
        //}
    }
}
