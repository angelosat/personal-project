using Start_a_Town_.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class PlantComponentNew : EntityComponent
    {
        public override string ComponentName => "PlantComponentNew";
        float WiggleDepth, WiggleTime, WiggleTimeMax = 2, WiggleDepthMax = (float)Math.PI / 4f;
        int WiggleDirection;
        public Progress Growth = new Progress(0, 3600, 0);
        public override void Tick(GameObject parent)
        {
            this.Wiggle(parent);
            if (this.Growth.Percentage < 1)
                this.Growth.Value++;
        }
        private void Wiggle(GameObject parent)
        {
            var t = this.WiggleTime / WiggleTimeMax;
            if (t >= 1)
                return;
            float currentangle, currentdepth = (1 - t) * this.WiggleDepthMax;
            currentangle = this.WiggleDirection * currentdepth * (float)Math.Sin(t * Math.PI * 2);
            this.WiggleTime += 0.05f;
            var sprCmp = parent.GetComponent<SpriteComponent>(); // TODO: optimize
            sprCmp._Angle = currentangle;
        }
        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.EntityCollision:
                    //var body = parent.Body;
                    //var sprCmp = parent.GetComponent<SpriteComponent>();
                    //sprCmp._Angle += 1;
                    //body.Angle *= 2;
                    this.WiggleDepth = this.WiggleDepthMax;
                    this.WiggleTime = 0;
                    this.WiggleDirection = (new int[] { -1, 1 })[new Random().Next(2)];
                    break;

                default:
                    break;
            }
            return false;
        }
        public override object Clone()
        {
            return new PlantComponentNew();
        }
    }
}
