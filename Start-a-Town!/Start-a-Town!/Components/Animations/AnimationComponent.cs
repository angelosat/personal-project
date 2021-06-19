using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components.Animations
{
    class AnimationComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Animation"; }
        }

        Bone Body { get; set; }
        SortedList<float, AnimationLayer> Layers { get; set; }


        public override void MakeChildOf(GameObject parent)
        {
            this.Body = parent.GetComponent<SpriteComponent>().Body;
            this.Layers = new SortedList<float, AnimationLayer>();
        }

        public override void Tick(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            foreach (var layer in this.Layers.Values)
                layer.Update(this.Body);
        }

        public override object Clone()
        {
            return new AnimationComponent();
        }
    }
}
