using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class MeasurePositiveNegative : Progress
    {
        public override float Percentage
        {
            get 
            { 
                //return (this.Value - this.Min) / (this.Max - this.Min); 
                return this.Value > 0 ? this.Value / this.Max : this.Value / -this.Max;// (this.Value - this.Min) / (this.Max - this.Min); 
            }
            set { this.Value = (this.Max - this.Min) * value; }
        }
    }
}
