using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class StatsComponentNew : EntityComponent
    {
        public override string ComponentName
        {
            get { return "StatsNew"; }
        }
        public Dictionary<Stat.Types, Stat> Stats;
        
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
        static public float GetStatValueOrDefault(GameObject obj, Stat.Types type, int defaultVal)
        {
            StatsComponentNew comp;
            if (obj.TryGetComponent<StatsComponentNew>(out comp))
            {
                Stat stat;
                if (comp.Stats.TryGetValue(type, out stat))
                    return stat.GetFinalValue(obj);
            }
            return defaultVal;
        }
        static public float GetStatValueOrDefault(GameObject obj, Stat.Types type)
        {
            StatsComponentNew comp;
            if (obj.TryGetComponent<StatsComponentNew>(out comp))
            {
                Stat stat;
                if (comp.Stats.TryGetValue(type, out stat))
                    return stat.GetFinalValue(obj);
            }
            return Stat.Registry.First(s=>s.ID == type).DefaultValue;
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

        readonly Dictionary<StatNewDef, List<StatNewModifier>> Modifiers = new();
        internal List<StatNewModifier> GetModifiers(StatNewDef statNewDef)
        {
            this.Modifiers.TryGetValue(statNewDef, out var item);
            return item ?? new List<StatNewModifier>();
        }
        public void AddModifier(StatNewModifier mod)
        {
            if (this.Modifiers.TryGetValue(mod.Def.Source, out var existing))
                existing.Add(mod);
            else
                this.Modifiers[mod.Def.Source] = new List<StatNewModifier>() { mod };
        }
        public bool RemoveModifier(StatNewModifier mod)
        {
            if (this.Modifiers.TryGetValue(mod.Def.Source, out var existing))
            {
                if (existing.Remove(mod))
                {
                    if (!existing.Any())
                        this.Modifiers.Remove(mod.Def.Source);
                    return true;
                }
            }
            return false;
        }
        internal override void GetInterface(GameObject gameObject, UI.Control box)
        {
            var gui = GUITable ??= new TableScrollableCompactNewNew<StatNewDef>(8)
                .AddColumn("name", "", 128, a => new Label(a.Label) { HoverText = a.Description })
                .AddColumn("value", "", (int)UIManager.Font.MeasureString("###").X, a => new Label(() => a.GetValue(this.Parent).ToString()));
            gui.ClearItems();
            gui.AddItems(StatDefOf.NpcStatPackage);
            box.AddControlsBottomLeft(gui);
            return;

            foreach (var stat in StatDefOf.NpcStatPackage)
                box.AddControlsBottomLeft(stat.GetControl(gameObject));
            
        }
        TableScrollableCompactNewNew<StatNewDef> GUITable;
        public override GroupBox GetGUI()
        {
            var gui = GUITable ??= new TableScrollableCompactNewNew<StatNewDef>(8)
                .AddColumn("name", "", 64, a => new Label(a.Label))
                .AddColumn("value", "", (int)UIManager.Font.MeasureString("###").X, a => new Label(() => a.GetValue(this.Parent).ToString()));
            gui.ClearItems();
            gui.AddItems(StatDefOf.NpcStatPackage);
            return GUITable;
        }
    }
}
