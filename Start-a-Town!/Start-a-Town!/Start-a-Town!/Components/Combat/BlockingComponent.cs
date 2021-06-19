using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Stats;

namespace Start_a_Town_.Components.Combat
{
    class BlockingComponent : Component
    {
        public override string ComponentName
        {
            get { return "Blocking"; }
        }
        public override object Clone()
        {
            return new BlockingComponent();
        }
        AnimationCollection Animation;// = AnimationCollection.Block;
        public bool Active;
        public void Start(GameObject parent)
        {
            if (this.Active)
                return;
            this.Active = true;
            //parent.GetComponent<StatsComponentNew>().Stats.Add(Stat.Types.DmgReduction, 0.5f);
            var stat = StatsComponentNew.GetStat(parent, Stat.Types.DmgReduction);
            stat.Value += .5f;
            this.Animation = AnimationCollection.Block;
            parent.Body.AddAnimation(this.Animation);
            parent.GetComponent<MobileComponent>().ToggleBlock(true); // TODO: create a new movement state and set it in the mobile component?
        }
        public void Stop(GameObject parent)
        {
            this.Active = false;
            //parent.GetComponent<StatsComponentNew>().Stats.Remove(Stat.Types.DmgReduction);
            var stat = StatsComponentNew.GetStat(parent, Stat.Types.DmgReduction);
            stat.Value -= .5f;
            parent.Body.FadeOutAnimation(this.Animation);
            parent.GetComponent<MobileComponent>().ToggleBlock(false);
        }
    }
}
