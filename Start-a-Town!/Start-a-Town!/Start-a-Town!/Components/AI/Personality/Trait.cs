using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_.AI
{
    public abstract class Trait : MeasurePositiveNegative
    {
        public enum Types { Patience, Attention, Composure }

        public abstract Types Type { get; }
        public static Dictionary<Types, Trait> All
        {
            get
            {
                return new Dictionary<Types, Trait>(){
                {Types.Patience, new TraitPatience()},
                {Types.Attention, new TraitAttention()},
                {Types.Composure, new TraitComposure()}
                };
            }
        }
        public abstract string Name { get; }
        //const int Max = 100;
        //const int Min = -Max;
        //int _Value = 0;
        //public int Value {
        //    get
        //    {
        //        return _Value;
        //    }
        //    set
        //    {
        //        this._Value = Math.Min(Max, Math.Max(Min, value));
        //    }
        //}

        public const float MinDefault = -100;
        public const float MaxDefault = 100;


        public override float Min
        {
            get
            {
                return MinDefault;// -100;
            }
            set
            {
                //base.Min = value;
            }
        }
        public override float Max
        {
            get
            {
                return MaxDefault;// 100;
            }
            set
            {
                //base.Max = value;
            }
        }

        public float Normalized
        {
            get { return (this.Value - Min) / (Max - Min); }
        }
        //public static List<Trait> All
        //{
        //    get
        //    {
        //        return new List<Trait>(){
        //            new TraitPatience()
        //        };
        //    }
        //}

        //public static readonly Trait Patience = new TraitPatience();

        public Control GetUI()
        {
            var box = new Panel() { AutoSize = true, BackgroundStyle = BackgroundStyle.TickBox };
            var bar = new BarSigned() { Object = this, TextFunc = () => this.Name };
            box.AddControls(bar);
            return box;
        }
    }
}
