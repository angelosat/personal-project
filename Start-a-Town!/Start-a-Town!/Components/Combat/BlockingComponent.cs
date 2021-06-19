using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Animations;
using Start_a_Town_.Components.Stats;

namespace Start_a_Town_.Components
{
    class BlockingComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Blocking"; }
        }
        public override object Clone()
        {
            return new BlockingComponent();
        }
        Animation Animation;// = AnimationCollection.Block;
        public bool Active;
        public void Start(GameObject parent)
        {
            if (this.Active)
                return;
            this.Active = true;
            var stat = StatsComponentNew.GetStat(parent, Stat.Types.DmgReduction);
            stat.Value += .5f;
            this.Animation = Animation.Block;
            //parent.Body.AddAnimation(this.Animation);
            parent.AddAnimation(this.Animation);

            parent.GetComponent<MobileComponent>().ToggleBlock(true); // TODO: create a new movement state and set it in the mobile component?
        }
        public void Stop(GameObject parent)
        {
            this.Active = false;
            var stat = StatsComponentNew.GetStat(parent, Stat.Types.DmgReduction);
            stat.Value -= .5f;
            //parent.Body.FadeOutAnimation(this.Animation);
            this.Animation.FadeOut();

            parent.GetComponent<MobileComponent>().ToggleBlock(false);
        }
    }
}
