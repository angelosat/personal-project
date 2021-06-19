using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Needs;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class NeedsComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Needs";
            }
        }
        //public NeedsHierarchy NeedsHierarchy { get { return (NeedsHierarchy)this["NeedsHierarchy"]; } set { this["NeedsHierarchy"] = value; } }
        //  public NeedsCollection Needs { get { return (NeedsCollection)this["Needs"]; } set { this["Needs"] = value; } }
        float Timer = Engine.TicksPerSecond;//{ get { return (float)this["NeedsTimer"]; } set { this["NeedsTimer"] = value; } }
        //public Need[] NeedsNew;
        public List<Need> NeedsNew;
        Actor Parent;
        public NeedsComponent(Actor actor)
        {
            this.Parent = actor;
            var def = actor.Def;
            var defs = def.ActorProperties.Needs;
            var size = defs.Length;
            //this.NeedsNew = new Need[size];
            this.NeedsNew = new List<Need>(size);

            for (int i = 0; i < size; i++)
            {
                //this.NeedsNew[i] = new Need(defs[i]);
                this.NeedsNew.Add(defs[i].Create(actor));// new Need(defs[i]));
            }
        }
        //public NeedsComponent(ItemDef def)
        //{
        //    var defs = def.ActorProperties.Needs;
        //    var size = defs.Length;
        //    //this.NeedsNew = new Need[size];
        //    this.NeedsNew = new List<Need>(size);

        //    for (int i = 0; i < size; i++)
        //    {
        //        //this.NeedsNew[i] = new Need(defs[i]);
        //        this.NeedsNew.Add(defs[i].Create());// new Need(defs[i]));
        //    }
        //}
        public NeedsComponent()
        {
        }

        //public NeedsComponent(params NeedDef[] defs)
        //{
        //    var size = defs.Length;
        //    //this.NeedsNew = new Need[size];
        //    this.NeedsNew = new List<Need>(size);

        //    for (int i = 0; i < size; i++)
        //    {
        //        //this.NeedsNew[i] = new Need(defs[i]);
        //        this.NeedsNew.Add(defs[i].Create());// new Need(defs[i]));
        //    }
        //}
        public void AddNeed(params NeedDef[] defs)
        {
            foreach (var d in defs)
                this.NeedsNew.Add(d.Create(this.Parent));// new Need(d));
        }

        public NeedsComponent Initialize(NeedsHierarchy needs)
        {
            throw new Exception();
            //this.NeedsHierarchy = needs;
            this.Timer = Engine.TicksPerSecond;
            return this;
        }

        public override void Tick(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            //    float lowest = 100;
            Timer -= 1;// GlobalVars.DeltaTime;
            if (Timer > 0)
                return;

            Timer = Engine.TicksPerSecond;
            //foreach (Need need in Needs.Values)
            //{
            //    need.Update();
            //    //float d = need.Decay * (1 + 5 * (1 - need.Value / 100f) * (1 - need.Value / 100f));
            //    //float newValue = Math.Max(0, need.Value - d);//need.Decay);
            //    //need.Value = newValue;
            //}


            for (int i = 0; i < this.NeedsNew.Count; i++)
            {
                this.NeedsNew[i].Tick(parent);
            }
            //this.NeedsHierarchy.Update(parent);
        }

        public override object Clone()
        {
            return new NeedsComponent(this.Parent);// this.NeedsNew.Select(n => n.Def).ToArray());
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {


                //case Message.Types.ModifyNeed:
                //    string name = (string)e.Parameters[0];
                //    float value = (float)e.Parameters[1];
                //    foreach (var level in this.NeedsHierarchy.Inner)
                //        foreach (var need in level.Value)
                //            if (need.Value.Name == name)
                //                need.Value.Value = MathHelper.Clamp(need.Value.Value + value, 0, 100);
                //    return true;

                default:
                    return false;
            }
        }

        //static public Need GetNeed(GameObject actor, Need.Types type)
        //{
        //    NeedsComponent comp;
        //    if (!actor.TryGetComponent<NeedsComponent>(out comp))
        //        return null;
        //    foreach (var needcollection in comp.NeedsHierarchy.Values)
        //    {
        //        Need found;
        //        if (needcollection.TryGetValue(type, out found))
        //            return found;
        //    }
        //    return null;
        //}
        static public Need ModifyNeed(GameObject actor, string needName, float value)
        {
            var need = actor.GetNeed(needName);// GetNeed(actor, type);
            //var oldvalue = need.Value;
            need.SetValue(need.Value + value, actor);
            if (actor.Net is Net.Server)
                PacketNeedModify.Send(actor.Net as Net.Server, actor.RefID, need.NeedDef, value);
            return need;
        }
        static public Need ModifyNeed(GameObject actor, NeedDef type, float value)
        {
            var need = actor.GetNeed(type);// GetNeed(actor, type);
            //var oldvalue = need.Value;
            need.SetValue(need.Value + value, actor);
            if (actor.Net is Net.Server)
                PacketNeedModify.Send(actor.Net as Net.Server, actor.RefID, need.NeedDef, value);
            return need;
        }

        internal override void GetManagementInterface(GameObject parent, UI.Control box)
        {
            box.AddControls(new NeedsUI(parent));
        }

        public void GetUI(GameObject parent, UI.Control container)
        {
            var box = new GroupBox();

            var byCategory = this.NeedsNew.GroupBy(n => n.NeedDef.CategoryDef);
            foreach (var cat in byCategory)
            {
                var panel = new PanelLabeled(cat.Key.Name) { Location = box.BottomLeft };
                foreach (var n in cat)
                {
                    var ui = n.GetUI(parent);
                    ui.Location = panel.Controls.BottomLeft;
                    panel.AddControls(ui);
                }
                box.AddControls(panel);
            }
            container.AddControls(box);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            //w.Write(this.NeedsNew);
            this.NeedsNew.WriteAbstract(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            //this.NeedsNew.ReadImmutable(r);
            this.NeedsNew.Clear();
            this.NeedsNew.InitializeAbstract(r, this.Parent);
        }
        internal override void AddSaveData(SaveTag tag)
        {
            this.NeedsNew.SaveVariableTypes(tag, "Needs");
            //var tagNew = new SaveTag(SaveTag.Types.Compound, "NeedsNew");
            //for (int i = 0; i < this.NeedsNew.Count; i++)
            //{
            //    var n = this.NeedsNew[i];
            //    tagNew.Add(n.Save());
            //}
            //tag.Add(tagNew);
        }
        internal override void Load(SaveTag tag)
        {
            this.NeedsNew.Clear();
            this.NeedsNew.TryLoadVariableTypes(tag, "Needs", this.Parent);
            //tag.TryGetTag("NeedsNew", t =>
            //{
            //    for (int i = 0; i < this.NeedsNew.Count; i++)
            //    {
            //        var n = this.NeedsNew[i];
            //        t.TryGetTag(n.NeedDef.Name, t => n.Load(t));
            //    }
            //});
        }
        public class Props : ComponentProps
        {
            public override Type CompType => typeof(NeedsComponent);
            public NeedDef[] Needs;
            public Props(params NeedDef[] defs)
            {
                this.Needs = defs;
            }
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
