using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public class StatRegistry : List<Stat> { }
    public class StatCollection : Dictionary<Stat.Types, float>
    {
        public StatCollection()
        {
        }
        public StatCollection(IEnumerable<KeyValuePair<Stat.Types, float>> stats)
        {
            foreach (var stat in stats)
                this[stat.Key] = stat.Value;
        }
        public StatCollection(StatCollection toCopy)
            : base(toCopy)
        { }
        public Dictionary<Stat.Types, float> Dictionary { get { return this as Dictionary<Stat.Types, float>; } }
        public override string ToString()
        {
            string text = "";
            foreach (var stat in this)
                text += Stat.GetStat(stat.Key).ToString(stat.Value) + "\n";
            return text.TrimEnd('\n');
        }
    }
    [Obsolete]
    public class StatsComponent : EntityComponent
    {
        static public StatsComponent Actor
        {
            get
            {
                StatsComponent stats = new StatsComponent();
                return stats;
            }
        }

        public override string ComponentName
        {
            get
            {
                return "Stats";
            }
        }
        public StatCollection Stats;
        public StatCollection BaseStats;
        static public StatsComponent Skills
        {
            get
            {
                StatsComponent stats = new StatsComponent();
      
                return stats;
            }
        }
        static public StatsComponent Damage
        {
            get
            {
                StatsComponent stats = new StatsComponent();
              
                return stats;
            }
        }

        public StatsComponent()
        {
            this.BaseStats = new StatCollection();
            this.Stats = new StatCollection();
        }

        public StatsComponent Initialize(Dictionary<Stat.Types, float> stats)
        {
            foreach (var stat in stats)
                this.BaseStats.Add(stat.Key, stat.Value);
                //this.BaseStats.TryAdd(stat.Key, stat.Value);
            return this;
        }

        public override object Clone()
        {
            StatsComponent comp = new StatsComponent();
            return comp;
        }

        public override void OnObjectCreated(GameObject parent)
        {
            Refresh(parent);
        }

        private void Refresh(GameObject parent)
        {
            this.Stats = new StatCollection(this.BaseStats);
            BodyComponent.PollStats(parent, this.Stats);
        }

        static public StatCollection GetStats(GameObject obj, Stat.Groups group)
        {
            StatCollection stats = new StatCollection();
            StatsComponent statsComp;
            if (!obj.TryGetComponent<StatsComponent>("Stats", out statsComp))
                return stats;
            //foreach (var stat in statsComp.Stats)
            //    if (Stat.GetStat(stat.Key).Group == group)
            //        stats[stat.Key] = stat.Value;
            return new StatCollection(stats.Where(stat => Stat.GetStat(stat.Key).Group == group));
        }

        static public float GetStatOrDefault(GameObject obj, Stat.Types id, float defaultValue)
        {
            StatsComponent stats;
            if (!obj.TryGetComponent<StatsComponent>("Stats", out stats))
                return defaultValue;
            float value;
            if (stats.Stats.TryGetValue(id, out value))
                return value;
            return defaultValue;
        }
       
        public override string ToString()
        {
            return this.Stats.ToString();

            //string text = "";
            //foreach (var stat in this.Stats)
            //    text += Stat.GetStat(stat.Key).ToString(stat.Value);
            //return text;
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            foreach (var property in this.Stats)
            {
                float value = (float)property.Value;
                string text = "[" + property.Key + ": " + value.ToString() + "]";
                tooltip.Controls.Add(new Label(new Vector2(0, tooltip.Controls.Count > 0 ? tooltip.Controls.Last().Bottom : 0), text));
            }
        }

        
        static public StatsComponent Add(GameObject obj, params Tuple<Stat.Types, float>[] stats)
        {
            StatsComponent statsComp;
            if (!obj.TryGetComponent<StatsComponent>("Stats", out statsComp))
                return null;

            stats.ToList().ForEach(s => statsComp.BaseStats.Add(s.Item1, s.Item2));
            //stats.ToList().ForEach(s => statsComp.BaseStats.TryAdd(s.Item1, s.Item2));
            return statsComp;
        }
        static public StatsComponent SetStat(GameObject obj, Stat.Types type, Func<float, float> updater)
        {
            StatsComponent statsComp;
            if (!obj.TryGetComponent<StatsComponent>("Stats", out statsComp))
                return null;
            statsComp.BaseStats.Update(type, updater);
            return statsComp;
        }
    }
}
