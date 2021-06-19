using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class Growth : Progress
    {
        const int InitialGrowth = 10;
        const float GrowthMax = 100;
        readonly float GrowthTimerMax = Engine.TicksPerSecond;
        public int GrowthTimer = 0;
        Action<GameObject> OnValueChangedFunc = a => { };

        public Growth()
            : base(0, GrowthMax, InitialGrowth)
        {
            //this.Parent = parent;
        }
        public Growth(float initialGrowthPercentage)
            : base(0, GrowthMax, 0)
        {
            //this.Parent = parent;
            this.Percentage = initialGrowthPercentage;
        }
        public void Update(GameObject parent)
        {
            if (this.IsFinished)// >= GrowthMax)
                return;
            this.GrowthTimer++;
            if (this.GrowthTimer == Engine.TicksPerSecond)
            {
                this.GrowthTimer = 0;
                this.Value++;
                //this.OnValueChangedFunc(parent);
                parent.Body.Scale = this.Percentage;// this.Growth / 100f;
            }
        }
        public override string ToString()
        {
 	         return string.Format("Growth: {0:P0}", this.Percentage);
        }
        //public SaveTag Save(string name)
        //{
        //    return base.Save(name);
        //}
        static public Growth Create(SaveTag tag)
        {
            //return new Growth().Load(tag) as Growth;
            var gr = new Growth();
            gr.Load(tag);
            return gr;
        }
        public Growth(SaveTag tag)
        {
            this.Load(tag);
        }
        static public Growth Create(BinaryReader r)
        {
            var gr = new Growth();
            gr.Read(r);
            return gr;
        }

        internal void Set(GameObject parent, float value)
        {
            this.Value = value;
            parent.Body.Scale = this.Percentage;// this.Growth / 100f;
        }
    }
}
