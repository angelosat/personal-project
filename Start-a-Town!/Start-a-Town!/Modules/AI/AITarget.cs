namespace Start_a_Town_.AI
{
    public class AITarget
    {
        public TargetArgs Target;
        /// <summary>
        /// used to maintain a minimum distance (ai moves away from target if distance is less than this value)
        /// </summary>
        public float RangeMin;
        public float RangeMax;

        // use a timer or a value threshold before the ai starts closing a distance that has increased past the max value?
        public float RangeThreshold;

        public AITarget(TargetArgs target, float min, float max)
        {
            this.Target = target;
            this.RangeMax = max;
            this.RangeMin = min;
            this.RangeThreshold = max;
        }
        public AITarget(TargetArgs target, float min, float max, float threshold)
        {
            this.Target = target;
            this.RangeMax = max;
            this.RangeMin = min;
            this.RangeThreshold = threshold;
        }
    }
}
