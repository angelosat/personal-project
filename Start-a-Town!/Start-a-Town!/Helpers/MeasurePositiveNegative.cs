namespace Start_a_Town_
{
    public class MeasurePositiveNegative : Progress
    {
        public override float Percentage
        {
            get 
            { 
                return this.Value > 0 ? this.Value / this.Max : this.Value / -this.Max;
            }
            set { this.Value = (this.Max - this.Min) * value; }
        }
    }
}
