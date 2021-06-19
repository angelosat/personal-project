using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptAnimation : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptAnimation"; }
        }
        public override object Clone()
        {
            return new ScriptAnimation(this.Animation);
        }

        Graphics.AnimationCollection Animation;

        public ScriptAnimation(Graphics.AnimationCollection ani)
        {
            this.Animation = ani;
        }

        public override void Start(ScriptArgs args)
        {
            args.Actor.GetComponent<SpriteComponent>().Body.Start(this.Animation);
        }
        public override void Finish(ScriptArgs args)
        {
            args.Actor.GetComponent<SpriteComponent>().Body.FadeOut(this.Animation);
        }
    }
}
