using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Stats
{
    class StatsComponentNew : Component
    {
        public override string ComponentName
        {
            get { return "StatsNew"; }
        }
        public Dictionary<Stat.Types, Stat> Stats { get { return (Dictionary<Stat.Types, Stat>)this["Stats"]; } set { this["Stats"] = value; } }
        public StatsComponentNew()
        {
            this.Stats = new Dictionary<Stat.Types, Stat>();
        }
        public StatsComponentNew(Dictionary<Stat.Types, Stat> stats)
        {
            this.Stats = new Dictionary<Stat.Types, Stat>();
            foreach (var st in stats)
                this.Stats[st.Key] = st.Value.Clone();
        }
        public StatsComponentNew Initialize(params Stat.Types[] types)
        {
            this.Stats.Clear();
            foreach (var t in types)
                this.Stats.Add(t, Stat.Create(t));
            return this;
        }
        public StatsComponentNew Initialize(params Stat[] stats)
        {
            this.Stats = new Dictionary<Stat.Types, Stat>();
            foreach (var t in stats)
                this.Stats.Add(t.ID, t);
            return this;
        }
        static public Stat GetStat(GameObject obj, Stat.Types type)
        {
            StatsComponentNew comp;
            if (obj.TryGetComponent<StatsComponentNew>(out comp))
                return comp.Stats.GetValueOrDefault(type);
            return null;
        }
        static public float GetStatValueOrDefault(GameObject obj, Stat.Types type, float defaultVal)
        {
            StatsComponentNew comp;
            if (obj.TryGetComponent<StatsComponentNew>(out comp))
            //return comp.Stats.GetValueOrDefault(type);
            {
                Stat stat;
                if (comp.Stats.TryGetValue(type, out stat))
                    return stat.GetFinalValue(obj);
            }
            return defaultVal;
        }
        static public void AddModifier(GameObject obj, Stat.Types type, ValueModifier modifier)
        {
            Stat stat = GetStat(obj, type);
            if (stat == null)
                return;
            stat.Modifiers.Add(modifier);
        }
        static public void RemoveModifier(GameObject obj, Stat.Types type, ValueModifier modifier)
        {
            Stat stat = GetStat(obj, type);
            if (stat == null)
                return;
            stat.Modifiers.Remove(modifier);
        }
        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<GameEvent>> gameEventHandlers)
        {
            foreach(var stat in this.Stats.Values)
            {
                var lbl = new UI.Label(stat.ToString(parent)) { Location = ui.Controls.BottomLeft };
                lbl.TooltipFunc = stat.GetTooltipInfo;
                ui.Controls.Add(lbl);
                
                //stat.GetTooltipInfo(ui as UI.Tooltip);
            }
        }
        public override object Clone()
        {
            return new StatsComponentNew(this.Stats);
        }
        public string ToString(GameObject parent)
        {
            string t = "";
            foreach (var stat in this.Stats.Values)
                t += stat.ToString(parent) + '\n';
            return t.TrimEnd('\n');
        }

    }
}
