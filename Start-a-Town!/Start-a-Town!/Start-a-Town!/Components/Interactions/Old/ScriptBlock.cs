using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptBlock : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.Block;
            }
        }
        public override string Name
        {
            get
            {
                return "Block";
            }
        }
        AnimationCollection Animation = AnimationCollection.Block;
        public override void OnStart(ScriptArgs args)
        {
            //base.Start(args);
            args.Actor.GetComponent<StatsComponent>().Stats.Add(Stat.Types.DmgReduction, 0.5f);
            //args.Actor.GetComponent<StatsComponent>().Stats.AddOrUpdate(Stat.Types.DmgReduction, 0.5f, (id, val) => val * 1.5f);
            args.Actor.Body.Start(this.Animation);
            args.Actor.GetComponent<MobileComponent>().ToggleBlock(true);
        }
        public override void Update(GameObject parent)
        {
            base.Update(parent);
        }
        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            //base.Update(net, parent, chunk);
        }
        public override void Finish(ScriptArgs args)
        {
            base.Finish(args);

            args.Actor.GetComponent<StatsComponent>().Stats.Remove(Stat.Types.DmgReduction);
            args.Actor.Body.FadeOut(this.Animation);
            args.Actor.GetComponent<MobileComponent>().ToggleBlock(false);
        }
        public override void Interrupt(ScriptArgs args)
        {
        }
        public override object Clone()
        {
            return new ScriptBlock();
        }
    }
}
