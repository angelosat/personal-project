using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public enum ConditionValue { Invalid, True, False }
    public class Precondition
    {
        public string Key;
        public Predicate<InteractionOld> Expression;
        public PlanType PlanType;
        public Precondition(string key, Predicate<InteractionOld> expression, PlanType plantype)
        {
            this.Key = key;
            this.Expression = expression;
            this.PlanType = plantype;
        }
    }
    public class ConditionCollection : List<Condition>
    {
        public ConditionCollection(params Condition[] conditions) : base(conditions) { }

        public bool FinalCondition(GameObject actor, GameObject target)
        {
            //bool ok = false;
            //foreach (InteractionCondition cond in this)
            //    ok &= cond.FinalCondition(agent);
            //return ok;
            foreach (Condition cond in this)
                //if (!cond.Condition(actor, target))
                    if (!cond.Predicate(actor, target, new byte[0]))
                    return false;
            return true;
        }

        ///// <summary>
        ///// Returns true if the final condition is satisfied.
        ///// </summary>
        ///// <param name="agent"></param>
        ///// <param name="condition"></param>
        ///// <returns></returns>
        //public bool TryFinalCondition(GameObject actor, GameObject target, out InteractionCondition condition)
        //{
        //    foreach (InteractionCondition cond in this)
        //        //if (!cond.Condition(actor, target))
        //        if (!cond.Predicate(actor, target, new byte[0]))
        //        {
        //            condition = cond;
        //            return false;
        //        }
        //    condition = null;
        //    return true;
        //}

        /// <summary>
        /// Returns true if there are one or more bad conditions.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="badConds"></param>
        /// <returns></returns>
        public bool TryGetBadConditions(GameObject actor, GameObject target, List<Condition> badConds)
        {
            foreach (Condition cond in this)
                //if (!cond.Condition(actor, target))
                    if (!cond.Predicate(actor, target, new byte[0]))
                        badConds.Add(cond);
            return badConds.Count > 0;
        }

        /// <summary>
        /// Returns true if there are one or more bad conditions.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="failedConds"></param>
        /// <returns></returns>
        public bool Pass(GameObject actor, GameObject target, List<Condition> failedConds)
        {
            foreach (Condition cond in this)
                //if (!cond.Condition(actor, target))
                if (!cond.Predicate(actor, target, new byte[0]))
                    failedConds.Add(cond);

            return failedConds.Count == 0;
        }

        public bool Pass(GameObject actor, GameObject target, byte[] args, List<Condition> failedConds)
        {
            foreach (Condition cond in this)
                if (!cond.Predicate(actor, target, args))
                    failedConds.Add(cond);

            return failedConds.Count == 0;
        }
        public bool Pass(GameObject actor, GameObject target)
        {
            byte[] args = new byte[0];
            foreach (Condition cond in this)
                if (!cond.Predicate(actor, target, args))
                    return false;

            return true;
        }
    }
    public class Condition
    {
        public string ErrorMessage;
        public PlanType PlanType;
        public Dictionary<String, Precondition> PreConditionsOld; //if the finalcondition isn't true, this is the check used to find the previous step in the interaction chain
        public List<AIPrecondition> Preconditions { get; set; }
     //   public Func<GameObject,GameObject, bool> Condition; //if this condition is true, the interaction chain is finished and execution begins
        public Func<GameObject, GameObject, byte[], bool> Predicate { get; set; } //if this condition is true, the interaction chain is finished and execution begins
        public bool Value;

        /// <summary>
        /// Checks if an interaction has the potential to satisfy this condition
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public bool Evaluate(InteractionOld interaction)
        {
            //Precondition pre;
            //if (!PreConditions.TryGetValue(interaction.Effect.Key, out pre))
            //    return false;
            //return pre.Expression(interaction);//.Source);

            foreach (var need in interaction.NeedEffects)
            {
                Precondition pre;
                if (PreConditionsOld.TryGetValue(need.Name, out pre))
                    if (pre.Expression(interaction))
                        return true;
            }
            return false;
        }
        public Condition()
        {
            this.Preconditions = new List<AIPrecondition>();
        }
        public Condition SetPreconditions(Dictionary<string, Precondition> eval)
        {
            PreConditionsOld = eval;
            return this;
        }
        public Condition TrySetPrecondition(Dictionary<string, Precondition> eval)
        {
            if (PreConditionsOld == null)
                PreConditionsOld = eval; // TODO
            return this;
        }

        public Condition(Func<GameObject, GameObject, bool> condition = null, params Precondition[] preconditions)
            : this()
        {
            PreConditionsOld = new Dictionary<string, Precondition>();
            foreach (Precondition pre in preconditions)
                PreConditionsOld[pre.Key] = pre;
           // this.Condition = condition == null ? (foo, bar) => true : condition;
            this.Predicate = (actor, target, args) => condition(actor, target);
            this.ErrorMessage = "Interaction condition not met!";
        }
        public Condition(Func<GameObject, GameObject, byte[], bool> predicate = null, params Precondition[] preconditions)
            : this()
        {
            PreConditionsOld = new Dictionary<string, Precondition>();
            foreach (Precondition pre in preconditions)
                PreConditionsOld[pre.Key] = pre;
            this.Predicate = predicate ?? ((actor, target, args) => true);
            this.ErrorMessage = "Interaction condition not met!";
        }
        public Condition(Func<GameObject, GameObject, bool> condition = null, string error = "", params Precondition[] preconditions)
            : this()
        {
            this.ErrorMessage = error;
            PreConditionsOld = new Dictionary<string, Precondition>();
            foreach (Precondition pre in preconditions)
                PreConditionsOld[pre.Key] = pre;
           // this.Condition = condition == null ? (foo, bar) => true : condition;
            this.Predicate = (actor, target, args) => condition(actor, target);
        }
        public Condition(Func<GameObject, GameObject, bool> condition, string error, IEnumerable<AIPrecondition> preconditions)
            : this()
        {
            this.Predicate = (actor, target, args) => condition(actor, target);
            this.ErrorMessage = error;
            this.Preconditions.AddRange(preconditions);
        }
        public Condition(Func<GameObject, GameObject, byte[], bool> predicate = null, string error = "", params Precondition[] preconditions)
            : this()
        {
            PreConditionsOld = new Dictionary<string, Precondition>();
            foreach (Precondition pre in preconditions)
                PreConditionsOld[pre.Key] = pre;
            this.Predicate = predicate ?? ((actor, target, args) => true);
            this.ErrorMessage = error;
        }
        //public InteractionCondition(Predicate<GameObject> actorCondition = null, Predicate<GameObject> targetCondition = null, string error = "", params Precondition[] preconditions)
        //{
        //    this.ErrorMessage = error;
        //    PreConditions = new Dictionary<string, Precondition>();
        //    foreach (Precondition pre in preconditions)
        //        PreConditions[pre.Key] = pre;
        //    this.ActorCondition = actorCondition == null ? foo => true : actorCondition;
        //}

    }


    struct InteractionTime
    {
        public TimeSpan TimeSpan;
        public Func<GameObject, float> Speed;

        public InteractionTime(TimeSpan timespan, Func<GameObject, float> speed = null)
        {
            this.TimeSpan = timespan;
            this.Speed = speed != null ? speed : foo => 1;
        }
    }

    public class InteractionWithParams
    {
        public InteractionOld Interaction;
        public object[] Parameters;
        public InteractionWithParams(InteractionOld interaction, params object[] p)
        {
            this.Interaction = interaction;
            this.Parameters = p;
        }
    }

    public class InteractionCollection : List<InteractionOld> { }




    /// <summary>
    /// Interactions are delayed object events.
    /// </summary>
    public class InteractionOld : ITooltippable
    {
        public Action<GameObject, TargetArgs> OnFinish { get; set; }
        public Action OnStart { get; set; }
        public Action OnEnd { get; set; }
        public GameObject Source { get; set; }
        public string Verb, Name;
        public Need Need;
        public TimeSpan Time;
        public InteractionTargetType TargetType;
        public Message.Types Message;
        public Stat Stat;
        public Func<GameObject, GameObject, bool> Range;
        public Func<GameObject, TargetArgs, float, bool> RangeCheck;
      //  public InteractionRequirement Requirement;
        //public InteractionCondition Conditions;
        public ConditionCollection Conditions;//, TargetConditions;
        //public List<InteractionEffect> Effect;
       // public InteractionEffect Effect;
        public List<AIAdvertisement> NeedEffects;
        public Func<GameObject, float> Speed;
        public bool CanBeJob = true, HideFromContext = false;
        public Component SourceComponent = null;
        public byte[] Parameters { get; set; }

        public Formula SpeedBonus { get; set; }

        public const float DefaultRange = 2;// (float)Math.Sqrt(3);
        public static Func<GameObject, GameObject, bool> DefaultRangeCheck = (a1, a2) =>
        {
            float distance = Vector3.Distance(a1.Global, a2.Global);
            return distance <= InteractionOld.DefaultRange;
        };
        public InteractionOld SetRange(Func<GameObject, GameObject, bool> range)
        {
            this.Range = range;
            return this;
        }

        public ObjectEventArgs EventArgs { get; set; }
        public Script.Types ScriptID { get; set; }
        public TargetArgs Target { get; set; }
        public int TimeInMs { get; set; }
        public Script ScriptCached;
        public InteractionOld(Script.Types scriptID, TargetArgs target, int ms)// ObjectEventArgs args, int ms)
        {
           // this.Recipient = recipient;
            this.ScriptID = scriptID;
        //    this.EventArgs = args;
            this.Target = target;
            this.TimeInMs = ms;
            this.ScriptCached = Components.Ability.GetScript(scriptID);
        }

        //public byte[] GetBytes()
        //{
        //    using(BinaryWriter w = new BinaryWriter(new MemoryStream()))
        //    {
        //        Write(w);
        //        return (w.BaseStream as MemoryStream).ToArray();
        //    }
        //}

        //public void Write(BinaryWriter w)
        //{
        ////    TargetArgs.Write(w, Recipient);
        //    w.Write((int)this.ScriptID);
        //    EventArgs.Write(w);
        //    w.Write(TimeInMs);
        //}

        public InteractionOld(
            TimeSpan timespan,
            Action<GameObject, TargetArgs> onFinish,
            GameObject target,
            string name = "<undefined>", 
            string verb = "",
            Func<GameObject, GameObject, bool> range = null, 
            ConditionCollection cond = null, 
        //    InteractionConditionCollection targetCond = null,
            Func<GameObject, float> speed = null
            )
        {
            this.Time = timespan;
            this.Name = name;
            this.Verb = verb;
            this.Source = target;
            this.Range = range ?? DefaultRangeCheck;// (actor => Vector3.Distance(actor.Global, this.Source.Global) <= Interaction.DefaultRange);
            this.Conditions = cond != null ? cond : new ConditionCollection();
           // this.TargetConditions = targetCond != null ? targetCond : new InteractionConditionCollection();
            this.OnFinish = onFinish;
            this.Speed = speed != null ? speed : foo => 1;
        }
        
        public InteractionOld(
            TimeSpan timespan, 
            Message.Types message, 
            GameObject source, 
            string name = "<undefined>", 
            string verb = "", 
            InteractionTargetType targettype = InteractionTargetType.None, 
            Stat stat = null, 
            Need need = null, 
           // InteractionRequirement req = null, 
            List<AIAdvertisement> effect = null, 
            Func<GameObject, GameObject, bool> range = null, 
            ConditionCollection cond = null, 
          //  InteractionConditionCollection targetCond = null,
            Func<GameObject, float> speed = null) //string need = "")  1.41421354f, float range = -1
        {
            //this.Range = range ?? (r => r <= Interaction.DefaultRange);//range == -1 ? Interaction.DefaultRange : range;
            this.Range = range ?? DefaultRangeCheck;
           // this.Range = range;
            this.Stat = stat;
            this.Message = message;
            this.TargetType = targettype;
            this.Source = source;
            this.Name = name;
            this.Verb = String.IsNullOrEmpty(verb) ? name : verb;
            this.Time = timespan;// TimeSpan.FromSeconds(length);// TimeSpan.FromSeconds(length);// new TimeSpan(length);// length;
            this.Need = need;
            this.Speed = speed != null ? speed : foo => 1;
        //    this.Requirement = req != null ? req : new InteractionRequirement();
            this.Conditions = cond != null ? cond : new ConditionCollection();// new List<InteractionCondition>(conditions);
          //  this.TargetConditions = targetCond != null ? targetCond : new InteractionConditionCollection();// new List<InteractionCondition>(conditions);
            //this.Effect = effect != null ? effect : new InteractionEffect("");
            this.NeedEffects = effect ?? new List<AIAdvertisement>();
        }

        public InteractionOld SetSource(GameObject source)
        {
            Source = source;
            return this;
        }

        public override string ToString()
        {
            //return Name + (Length > 0 ? " (" + (Length / 60f).ToString("f2") + "s)" : "");
            //    return Name + " " + Length.Ticks;// (Length != TimeSpan.Zero ? "(" + Length.TotalSeconds.ToString() + "s)" : "");// (Length > 0 ? " (" + (Length / 60f).ToString("f2") + "s)" : "");
            return Name + ": " + Source + " " + (Source.Exists ? Source.Global.ToString() : "") + " (" + Time.Seconds.ToString("f2") + "s)";
        }

        public string ToString(GameObject actor)
        {
            string text = Name;
            if (Stat == null)
                return text;

            float coef = 1;
            Bonus bonus;
            // interaction duration
            if (actor["Equipment"].GetProperty<BodyPart>("Mainhand").Object["Bonuses"].TryGetProperty<Bonus>(Stat.Name, out bonus))
                bonus.Apply(ref coef);
            //text += (Length > 0 ? " (" + ((1 - bonus.Value) * Length / 60f).ToString("f2") + "s)" : "");
            text += (Time != TimeSpan.Zero ? " (" + ((1 - bonus.Value) * Time.TotalSeconds).ToString("f2") + "s)" : "");
            return text;
        }

        //public bool CheckCondition(GameObject subject)
        //{
        //    if (ActorConditions != null)
        //        if (!ActorConditions.FinalCondition(subject))
        //            return false;
        //    return true;
        //}

        static public bool HasInteraction(GameObject obj, GameObject sender, Predicate<InteractionOld> pred)
        {
            List<InteractionOld> interactions = new List<InteractionOld>();
            obj.Query(sender, interactions);
            foreach (InteractionOld i in interactions)
                if (pred(i))
                    return true;
            return false;
        }

        static public bool FindMatch(GameObject target, GameObject actor, Components.Message.Types message, out InteractionOld interaction)
        {
            List<InteractionOld> interactions = new List<InteractionOld>();
            target.Query(actor, interactions);
            interaction = interactions.FirstOrDefault(i => i.Message == message);
            return interaction != null;
        }

        static public bool FindMatch(GameObject target, GameObject actor, AbilitySlot abilitySlot, out InteractionOld interaction)
        {
            GameObjectSlot ability = ControlComponent.GetAbility(actor)[abilitySlot];

            if (!ability.HasValue)
            {
                interaction = null;
                return false;
            }
            Message.Types message = (Components.Message.Types)ability.Object["Ability"]["Message"];
            List<InteractionOld> interactions = new List<InteractionOld>();
            target.Query(actor, interactions);
            interaction = interactions.FirstOrDefault(i => i.Message == message);
            return interaction != null;
        }

        /// <summary>
        /// Returns true if there are one or more bad conditions.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="failedConds"></param>
        /// <returns></returns>
        public bool TryConditions(GameObject actor, GameObject target, List<Condition> failedConds)
        {
            foreach (Condition cond in this.Conditions)
                //if (!cond.Condition(actor, target))
                if (!cond.Predicate(actor, target, new byte[0]))
                    failedConds.Add(cond);

            //foreach (InteractionCondition cond in this.ActorConditions)
            //    if (!cond.Condition(agent))
            //        failedConds.Add(cond);
            
            return failedConds.Count == 0;
        }
        public bool TryConditions(GameObject actor, List<Condition> failedConds)
        {
            foreach (Condition cond in this.Conditions)
                //if (!cond.Condition(actor, this.Source))
                if (!cond.Predicate(actor, this.Target.Object, new byte[0]))
                    failedConds.Add(cond);

            //foreach (InteractionCondition cond in this.ActorConditions)
            //    if (!cond.Condition(agent))
            //        failedConds.Add(cond);

            return failedConds.Count == 0;
        }

        //public bool TryConditions(GameObject actor, GameObject target)
        //{
        //    //foreach (InteractionCondition cond in this.TargetConditions)
        //    //    if (!cond.ActorCondition(Source))
        //    //        return false;

        //    foreach (InteractionCondition cond in this.Conditions)
        //        if (!cond.Condition(actor, target))
        //            return false;

        //    return true;
        //}
        public bool TryConditions(GameObject actor)
        {
            //foreach (InteractionCondition cond in this.TargetConditions)
            //    if (!cond.ActorCondition(Source))
            //        return false;

            foreach (Condition cond in this.Conditions)
                //if (!cond.Condition(actor, this.Source))
                if (!cond.Predicate(actor, this.Target.Object, new byte[0]))
                    return false;

            return true;
        }
        public void GetTooltipInfo(UI.Tooltip tooltip)
        {
            InteractionOld i = this;
            
            Label label_comp = new Label(i.SourceComponent.IsNull() ? "<undefined>" : i.SourceComponent.ToString());
            tooltip.Controls.Add(label_comp);
            string text = "";
            List<Condition> failed = new List<Condition>();
            if (i.TryConditions(Player.Actor, this.Source, failed))
                return;
            if (failed.Count == 0)
                return;
            foreach (Condition condition in failed)
                text += condition.ErrorMessage + "\n";
            text = text.TrimEnd('\n');
            Label label_cond = new UI.Label(text) { Location = label_comp.BottomLeft, TextColorFunc = () => Color.Red };
            
            tooltip.Controls.Add(label_cond);
        }

        public InteractionOld(
            Action onEnd,
            TargetArgs target, // do i really need to pass this?
            string name,
            int timeInMs = 0, 
            Formula speedBonus = null,
            Func<GameObject, GameObject, bool> range = null,
            ConditionCollection reqs = null)
        {
            this.OnEnd = onEnd ?? (() => { });
            this.Name = name;
            this.Target = target;
            this.TimeInMs = timeInMs;
            this.Range = range ?? InteractionOld.DefaultRangeCheck;
            this.SpeedBonus = speedBonus ?? Formula.GetFormula(Formula.Types.Default);
            this.Conditions = reqs ?? new ConditionCollection();
        }

        static public InteractionOld StartNew(
            Net.IObjectProvider net,
            GameObject actor,
            TargetArgs target,
            Action action,
            string name,
            int timeInMs = 0,
            Formula speedBonus = null,
            Func<GameObject, GameObject, bool> range = null,
            ConditionCollection reqs = null)
        {
            InteractionOld inter = new InteractionOld(action, target, name, timeInMs, speedBonus, range, reqs);
            actor.GetComponent<ControlComponent>().BeginInteraction(net, actor, inter);
            return inter;
        }
        static public InteractionOld StartNew(
            Net.IObjectProvider net,
            GameObject actor,
            InteractionOld interaction)
        {
            actor.GetComponent<ControlComponent>().BeginInteraction(net, actor, interaction);
            return interaction;
        }

        //static public Interaction StartNew(GameObject entity, Interaction i)
        //{
        //    return StartNew(entity, i, Vector3.Zero);
        //}
        //static public Interaction StartNew(Net.IObjectProvider net, GameObject entity, Interaction i, Vector3 face)
        //{
        //   entity.PostMessage(Components.Message.Types.BeginInteraction, null, i, face);
        //    return i;
        //}
        
        static public void StartNew(Net.IObjectProvider net, GameObject actor, TargetArgs target, Script.Types scriptID, byte[] parameters)
        {
            net.PostLocalEvent(actor, ObjectEventArgs.Create(Components.Message.Types.BeginInteraction, new TargetArgs(actor), scriptID, target, parameters));// BitConverter.GetBytes((int)scriptID)));
        }
        static public void StartNew(Net.IObjectProvider net, GameObject actor, TargetArgs target, Script.Types scriptID)
        {
            net.PostLocalEvent(actor, ObjectEventArgs.Create(Components.Message.Types.BeginInteraction, new TargetArgs(actor), scriptID, target));// BitConverter.GetBytes((int)scriptID)));
        }
        static public void StartNew(Net.IObjectProvider net, GameObject actor, TargetArgs target, Script script)
        {
            net.PostLocalEvent(actor, ObjectEventArgs.Create(Components.Message.Types.BeginScript, new object[] { script, target }));// BitConverter.GetBytes((int)scriptID)));
        }
        //static public void StartNew(Net.IObjectProvider net, GameObject actor, Action callback)
        //{
        //    net.PostLocalEvent(actor, ObjectEventArgs.Create(Components.Message.Types.BeginInteraction2, new TargetArgs(actor), scriptID, target));// BitConverter.GetBytes((int)scriptID)));
        //}

        //static public void StartNew(Net.IObjectProvider net, GameObject actor, Script.Types scriptID, object[] args)
        //{
        //    //AbilityComponent script = Script.GetScript(scriptID); 
        //    //Interaction interaction = new Interaction(actor, args, ms);
        //    //Interaction inter = script.GetInteraction(actor, 

        //    net.PostLocalEvent(actor, ObjectEventArgs.Create(Components.Message.Types.BeginInteraction, new object[] { scriptID, args }));// BitConverter.GetBytes((int)scriptID)));
        //}
        //static public Interaction StartNew(GameObject entity, ObjectEventArgs args, int ms)
        //{
        //    entity.PostMessage(Components.Message.Types.BeginInteraction, null, i, face);
        //    return i;
        //}
        //static public Interaction StartNew(GameObject entity, GameObject target, TimeSpan span, Action start, Action finish)
        //{
        //    Interaction i = new Interaction(span, Components.Message.Types.Default, target) { OnStart = start, OnFinish = finish };
        //    entity.PostMessage(Components.Message.Types.BeginInteraction, null, i);
        //    return i;
        //}
    }
}
