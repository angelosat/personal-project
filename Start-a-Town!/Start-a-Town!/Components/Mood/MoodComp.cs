using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using System.IO;

namespace Start_a_Town_
{
    class MoodComp : EntityComponent, IProgressBar
    {
        List<Moodlet> Moodlets = new List<Moodlet>();
        //public Progress Mood = new Progress(0, 100, 50);
        const float BaseMood = 50;
        //float? _Mood;
        public float Mood = BaseMood;
        float Rate = 1f;
        int TicksRemaining = Engine.TicksPerSecond, TicksDelay = Engine.TicksPerSecond;
        //{
        //    get
        //    {
        //        if (!this._Mood.HasValue)
        //            this._Mood = GetValueTarget();
        //        return this._Mood.Value;
        //    }
        //}

        private float ValueTarget => BaseMood + this.Moodlets.Sum(m => m.Def.Value);
        

        public override string ComponentName => "Mood";
        public float Min => 0;

        public float Max => 100;

        public float Value => Mood;

        public float Percentage => Value / Max;

        public override void Tick(GameObject parent)
        {
            //this._Mood = null;
            if (this.TicksRemaining <= 0)
            {
                var target = this.ValueTarget;
                var resilience = StatDefOf.MoodChangeRate.GetValue(parent);
                var rate = this.Rate;// * resilience;
                if (target < this.Mood)
                    this.Mood = Math.Max(this.Mood - rate, target);
                else if (target > this.Mood)
                    this.Mood = Math.Min(this.Mood + rate, target);
                this.TicksRemaining = (int)Math.Round(this.TicksDelay * resilience);
                //this.TicksRemaining = this.TicksDelay;
            }
            this.TicksRemaining--;
            bool invalidatedMoodlets = false;
            var actor = parent as Actor;
            foreach (var m in MoodletDef.All)
            {
                invalidatedMoodlets = m.TryAssignOrRemove(actor);
            }
            var count = this.Moodlets.Count;
            var nextMoodlets = new List<Moodlet>(count);
            for (int i = 0; i < count; i++)
            {
                var m = this.Moodlets[i];
                var result = m.Tick(actor);
                if (result)
                    nextMoodlets.Add(m);
                else
                {
                    //this.Mood.Value -= m.Def.Value;
                    invalidatedMoodlets = true;
                }
            }
            this.Moodlets = nextMoodlets;
            //this.Mood.Value = 50 + this.Moodlets.Sum(m => m.Def.Value);
            if (invalidatedMoodlets)
                parent.Map.EventOccured(Message.Types.MoodletsUpdated, parent);
        }

        public void Add(Actor actor, Moodlet m)
        {
            this.Moodlets.Add(m);
            //this.Mood.Value += m.Def.Value;
            actor.Net.Map.EventOccured(Message.Types.MoodletsUpdated, actor);
        }
        public void Remove(Actor actor, MoodletDef mdef)
        {
            //if (!this.Contains(mdef))
            //throw new Exception();
            if (this.Moodlets.RemoveAll(m => m.Def == mdef) > 0)
            {
                //this.Mood.Value -= mdef.Value;
                actor.Net.Map.EventOccured(Message.Types.MoodletsUpdated, actor);
            }
            else
                throw new Exception();
        }
        public bool Contains(MoodletDef mdef)
        {
            return this.Moodlets.Any(m => m.Def == mdef);//.Contains(m);
        }
        public override object Clone()
        {
            return new MoodComp();
        }

        internal override void GetInterface(GameObject actor, Control box)
        {
            var panelMoodValue = new PanelLabeled("Mood");
            //var bar = new Bar(this.Mood);
            //bar.ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Mood.Value / 100f);
            //bar.HoverFunc = () => this.Mood.Value.ToString();
            //bar.NameFunc = () => this.Mood.Percentage.ToString("##0%");

            var bar = new Bar(this);
            bar.ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Mood / 100f);
            bar.HoverFunc = () => this.Mood.ToString();
            bar.NameFunc = () => this.Percentage.ToString("##0%");
            bar.OnDrawAction = (sb, bounds) =>
            {
                var indicator = new Rectangle(bounds.X + (int)Math.Round(bounds.Width * this.ValueTarget / this.Max), bounds.Y, 1, bounds.Height);
                indicator.DrawHighlight(sb);
            };

            panelMoodValue.AddControlsBottomLeft(bar);

            var panelMoodlets = new TableScrollableCompact<Moodlet>(8, BackgroundStyle.TickBox);
            panelMoodlets.AddColumn(null, "Test", panelMoodValue.Width * 2, m => m.GetUI(), showColumnLabels: false);
            panelMoodlets.Build(this.Moodlets, false);
            //var panelMoodlets = new Panel(new Rectangle(0, 0, panelMoodValue.Width * 2, panelMoodValue.Width * 2));// { AutoSize = true };
            //panelMoodlets.Location = panelMoodValue.BottomLeft;

            //foreach (var m in this.Moodlets)
            //    panelMoodlets.AddControlsBottomLeft(m.GetUI());

            panelMoodlets.Location = panelMoodValue.BottomLeft;
            box.AddControls(panelMoodValue, panelMoodlets);

            panelMoodlets.OnGameEventAction = (e) =>
            {
                if (e.Type == Message.Types.MoodletsUpdated && e.Parameters[0] == actor)
                {
                    //panelMoodlets.ClearControls();
                    panelMoodlets.Build(this.Moodlets, false);
                    //foreach (var m in this.Moodlets)
                    //    panelMoodlets.AddControlsBottomLeft(m.GetUI());
                }
            };
        }

        internal override void AddSaveData(SaveTag tag)
        {
            this.Moodlets.SaveAsList(tag, "Moodlets");
            tag.Add(this.Mood.Save("Value"));
        }
        internal override void Load(SaveTag save)
        {
            this.Moodlets.TryLoadAsList<List<Moodlet>, Moodlet>(save, "Moodlets");
            save.TryGetTagValueNew("Value", ref this.Mood);
            //foreach (var m in this.Moodlets)
            //this.Mood.Value += m.Def.Value;
        }
        public override void Write(BinaryWriter w)
        {
            this.Moodlets.Write(w);
            w.Write(this.Mood);
        }
        public override void Read(BinaryReader r)
        {
            this.Moodlets.Read(r);
            this.Mood = r.ReadSingle();
            //foreach (var m in this.Moodlets)
            //this.Mood.Value += m.Def.Value;
        }
    }
    //class MoodComp : EntityComponent, IProgressBar
    //{
    //    List<Moodlet> Moodlets = new List<Moodlet>();
    //    //public Progress Mood = new Progress(0, 100, 50);
    //    const float BaseMood = 50;
    //    float? _Mood;
    //    public float Mood
    //    {
    //        get
    //        {
    //            if(!this._Mood.HasValue)
    //                _Mood = BaseMood + this.Moodlets.Sum(m => m.Def.Value);
    //            return this._Mood.Value;
    //        }
    //    }
    //    public override string ComponentName => "Mood";

    //    public float Min => 0;

    //    public float Max => 100;

    //    public float Value => Mood;

    //    public float Percentage => Value / Max;

    //    public override void Tick(GameObject parent)
    //    {
    //        this._Mood = null;
    //        bool invalid = false;
    //        var actor = parent as Actor;
    //        foreach(var m in MoodletDef.All)
    //        {
    //            invalid = m.TryAssignOrRemove(actor);
    //        }
    //        var count = this.Moodlets.Count;
    //        var nextMoodlets = new List<Moodlet>(count);
    //        for (int i = 0; i < count; i++)
    //        {
    //            var m = this.Moodlets[i];
    //            var result = m.Tick(actor);
    //            if (result)
    //                nextMoodlets.Add(m);
    //            else
    //            {
    //                //this.Mood.Value -= m.Def.Value;
    //                invalid = true;
    //            }
    //        }
    //        this.Moodlets = nextMoodlets;
    //        //this.Mood.Value = 50 + this.Moodlets.Sum(m => m.Def.Value);
    //        if (invalid)
    //            parent.Map.EventOccured(Message.Types.MoodletsUpdated, parent);
    //    }

    //    public void Add(Actor actor, Moodlet m)
    //    {
    //        this.Moodlets.Add(m);
    //        //this.Mood.Value += m.Def.Value;
    //        actor.Net.Map.EventOccured(Message.Types.MoodletsUpdated, actor);
    //    }
    //    public void Remove(Actor actor, MoodletDef mdef)
    //    {
    //        //if (!this.Contains(mdef))
    //        //throw new Exception();
    //        if (this.Moodlets.RemoveAll(m => m.Def == mdef) > 0)
    //        {
    //            //this.Mood.Value -= mdef.Value;
    //            actor.Net.Map.EventOccured(Message.Types.MoodletsUpdated, actor);
    //        }
    //        else
    //            throw new Exception();
    //    }
    //    public bool Contains(MoodletDef mdef)
    //    {
    //        return this.Moodlets.Any(m => m.Def == mdef);//.Contains(m);
    //    }
    //    public override object Clone()
    //    {
    //        return new MoodComp();
    //    }

    //    internal override void GetInterface(GameObject actor, Control box)
    //    {
    //        var panelMoodValue = new PanelLabeled("Mood");
    //        //var bar = new Bar(this.Mood);
    //        //bar.ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Mood.Value / 100f);
    //        //bar.HoverFunc = () => this.Mood.Value.ToString();
    //        //bar.NameFunc = () => this.Mood.Percentage.ToString("##0%");

    //        var bar = new Bar(this);
    //        bar.ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Mood / 100f);
    //        bar.HoverFunc = () => this.Mood.ToString();
    //        bar.NameFunc = () => this.Percentage.ToString("##0%");

    //        panelMoodValue.AddControlsBottomLeft(bar);

    //        var panelMoodlets = new TableScrollableCompact<Moodlet>(8, BackgroundStyle.TickBox);
    //        panelMoodlets.AddColumn(null, "Test", panelMoodValue.Width * 2, m => m.GetUI(), showColumnLabels: false);
    //        panelMoodlets.Build(this.Moodlets, false);
    //        //var panelMoodlets = new Panel(new Rectangle(0, 0, panelMoodValue.Width * 2, panelMoodValue.Width * 2));// { AutoSize = true };
    //        //panelMoodlets.Location = panelMoodValue.BottomLeft;

    //        //foreach (var m in this.Moodlets)
    //        //    panelMoodlets.AddControlsBottomLeft(m.GetUI());

    //        panelMoodlets.Location = panelMoodValue.BottomLeft;
    //        box.AddControls(panelMoodValue, panelMoodlets);

    //        panelMoodlets.OnGameEventAction = (e) =>
    //        {
    //            if (e.Type == Message.Types.MoodletsUpdated && e.Parameters[0] == actor)
    //            {
    //                //panelMoodlets.ClearControls();
    //                panelMoodlets.Build(this.Moodlets, false);
    //                //foreach (var m in this.Moodlets)
    //                //    panelMoodlets.AddControlsBottomLeft(m.GetUI());
    //            }
    //        };
    //    }

    //    internal override void AddSaveData(SaveTag tag)
    //    {
    //        this.Moodlets.SaveAsList(tag, "Moodlets");
    //    }
    //    internal override void Load(SaveTag save)
    //    {
    //        this.Moodlets.TryLoadAsList<List<Moodlet>,Moodlet>(save, "Moodlets");

    //        //foreach (var m in this.Moodlets)
    //            //this.Mood.Value += m.Def.Value;
    //    }
    //    public override void Write(BinaryWriter w)
    //    {
    //        this.Moodlets.Write(w);
    //    }
    //    public override void Read(BinaryReader r)
    //    {
    //        this.Moodlets.Read<List<Moodlet>, Moodlet>(r);

    //        //foreach (var m in this.Moodlets)
    //            //this.Mood.Value += m.Def.Value;
    //    }
    //}
}
