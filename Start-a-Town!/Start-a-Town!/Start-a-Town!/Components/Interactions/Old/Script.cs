using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components
{
    public enum AbilitySlot { None, Primary, Secondary, Function1, PickUp, Function3, Throw }
    public enum ScriptFlow { Instant, Delayed, Channeled }
    public enum ScriptState { Unstarted, Running, Finished, Failed, Interrupted }
    public abstract class Script : Component
    {
        public static readonly Func<GameObject, TargetArgs, float, bool> DefaultRangeCheck = (actor, target, range) =>
        { 
            return Vector3.Distance(actor.Global, target.FinalGlobal) <= range; 
        };// Interaction.DefaultRange;

        public static readonly Func<GameObject, TargetArgs, float, bool> InventoryFirst = (actor, target, range) =>
        {
            if (PersonalInventoryComponent.HasObject(actor, o => o == target.Object))
                return true;
            return Vector3.Distance(actor.Global, target.FinalGlobal) <= range;
        };// Interaction.DefaultRange;

        public enum Types
        {
            //Jumping, 
            Digging, Tilling, Sawing, Framing, Equipping, Chopping, Activate, CraftingWorkbench, BuildFootprint, BuildFrame, Mining, PickUp, Drop, Crafting, Dynamic,
            Throw,
            Attack,
            Walk,
            Block,
            Threat,
            Test,
            PlaceBlock,
            Planting,
            //  ArrangeInventory
            PickUpSlot,
            Hauling,
            DropOnTarget,
            UseHeldItem,
            CraftingBench,
            Unpack,
            Default,
            Reaction,
            Build,
            Construct
        };
        public abstract override object Clone();
        static Dictionary<Script.Types, Script> _Registry;// = new List<Ability>();
        static public Dictionary<Script.Types, Script> Registry
        {
            get
            {
                if (_Registry.IsNull())
                    Initialize();
                return _Registry;
            }
        }
        private static void Register(params Script[] scripts)
        {
            foreach (var script in scripts)
                _Registry.Add(script.ID, script);
        }
        private static void Initialize()
        {
            _Registry = new Dictionary<Script.Types, Script>();
            Register(
                new ScriptThrow(),
                new ScriptAttack(),
                //new ScriptJump(),
                new ScriptWalking(),
                new ScriptMining(),
                new ScriptDigging(),
                new ScriptTilling(),
                new ScriptEquip(),
                new ScriptCrafting(),
                new ScriptFraming(),
                new ScriptActivate(),
                new ScriptBuildFootprint(),
                new ScriptBuild(),
                new ScriptWorkbench(),
                new ScriptBlock(),
                new ScriptThreat(),
                //new ScriptDefault(Types.PickUp, "Picking Up", a => a.Net.PostLocalEvent(a.Actor, Components.Message.Types.Receive, a.Target.Object)),
                //new ScriptChopping(),
                new ScriptTest(),
                new ScriptPlanting(),
                new ScriptSawing(),
                new ScriptPickUp(),
                new ScriptPickUpSlot(),
                new ScriptHauling(),
                new ScriptDrop(),
                new ScriptDropOnTarget(),
                new ScriptUseHeldItem(),
                new ScriptBench(),
                new ScriptUnpack(),
                new ScriptReaction()
                //new ScriptArrangeInventory()
            );
        }
       

        public override string ComponentName
        {
            get
            {
                return "Script";
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
        public virtual bool Evaluate(ScriptArgs args)
        {
            foreach (var c in this.ScriptBehaviors.Values)
                if (!c.Evaluate(this.ArgsSnapshot))
                {
                    this.Finish(this.ArgsSnapshot);
                    return false;
                }
            return true;
        }
        public virtual void AIControl(ScriptArgs args) { }
        public virtual void Interrupt(ScriptArgs args) 
        {
            args.Net.PostLocalEvent(args.Actor, ObjectEventArgs.Create(Components.Message.Types.Speak, new TargetArgs(args.Actor), "Interrupted!"));
            //OnInterrupt(args.Actor);
            Finish(args);
        }
        public virtual void Finish(ScriptArgs args)
        {
            foreach (var sc in this.ScriptBehaviors.Values)
                sc.Finish(args);
            this.ScriptState = ScriptState.Finished;

            // dangerous in case it's set outside of this method
            GameEvent e = new GameEvent(args.Net, args.Net.Clock.TotalMilliseconds, Components.Message.Types.ScriptFinished, args.Actor, args.Target, this.ID);
            args.Net.Map.HandleEvent(e);
            
            //args.Net.EventOccured(Components.Message.Types.ScriptFinished, args.Actor, args.Target, this.ID);
            //args.Net.PostLocalEvent(args.Actor, Components.Message.Types.ScriptFinished, )
        }
        public void Start(ScriptArgs args)
        {
            this.ArgsSnapshot = args;
            this.ScriptState = ScriptState.Running;

            //foreach (var c in this.ScriptBehaviors.Values)
            //    if (!c.Evaluate(this.ArgsSnapshot))
            //    {
            //        this.Finish(this.ArgsSnapshot);
            //        return;
            //    }
            if (!this.Evaluate(this.ArgsSnapshot))
            {
                this.Finish(this.ArgsSnapshot);
                return;
            }

            foreach (var scriptcomp in this.ScriptBehaviors.Values)
                scriptcomp.Start(args);
            
            OnStart(args);
        }
        public virtual void Success(ScriptArgs args)
        {
            foreach (var sc in this.ScriptBehaviors.Values)
                sc.Success(args);
            Finish(args);
            this.OnSuccess(args);
        }
        public virtual void Success()
        { 
            this.Success(this.ArgsSnapshot);

        }
        public virtual void OnStart(ScriptArgs args)
        {
      //      this.ScriptState = Components.ScriptState.Finished;
        }
        public virtual void OnSuccess(ScriptArgs args) { }
        public virtual void Finish()
        { this.Finish(this.ArgsSnapshot); }
        public virtual bool ConditionsCheck(ScriptArgs args)
        {
            if (!this.RangeCheck(args.Actor, args.Target, this.RangeValue))
            { 
                args.Net.PostLocalEvent(args.Actor, ObjectEventArgs.Create(Components.Message.Types.OutOfRange));
                Finish(args);
                return true;
            }

            List<Condition> failedConditions = new List<Condition>();

            if (this.Conditions.Pass(args.Actor, args.Target.Object, args.Args, failedConditions))
                return false;

            string text = "";
            foreach (Condition condition in failedConditions)
                text += condition.ErrorMessage + "\n";

            args.Net.PostLocalEvent(args.Actor, ObjectEventArgs.Create(Components.Message.Types.Speak, new TargetArgs(args.Actor), text.TrimEnd('\n')));
            Finish(args);
            return true;
        }
        public virtual void Stop(ScriptArgs args)
        {
            //args.Net.PostLocalEvent(args.Actor, ObjectEventArgs.Create(Components.Message.Types.Speak, new TargetArgs(args.Actor), "Canceled!"));
           // OnCancel(args.Actor);
            Finish(args);
            //this.State = ScriptState.Finished;
        }
        public virtual void Fail(ScriptArgs args)
        {
            this.ScriptState = Components.ScriptState.Finished;
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            bool ok = true;// Components.ScriptState.Finished;
            foreach (var c in this.ScriptBehaviors.Values)
            {
                if (!c.Evaluate(this.ArgsSnapshot))
                {
                    this.Finish(this.ArgsSnapshot);
                    return;
                }
                ScriptState result = c.Update(this.ArgsSnapshot);//net, parent, chunk);
                ok &= (result == Components.ScriptState.Finished);
            }
            if (ok)
            {
                this.Success();
            }
        }
        //public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        //{
        //    foreach (var c in this.ScriptBehaviors.Values)
        //    {
        //        if (!c.Evaluate(this.ArgsSnapshot))
        //            this.Finish(this.ArgsSnapshot);
        //        c.Update(this.ArgsSnapshot);//net, parent, chunk);
        //    }
        //}
        public ScriptArgs ArgsSnapshot { get; set; }

        public virtual Script.Types ID { get { return (Script.Types)this["ID"]; } set { this["ID"] = value; } }
        public virtual string Name { get { return (string)this["Name"]; } set { this["Name"] = value; } }
        public Action<ScriptArgs> Action { get { return (Action<ScriptArgs>)this["Action"]; } set { this["Action"] = value; } }
        //public Action<Net.IObjectProvider, GameObject, TargetArgs> Execute { get { return (Action<Net.IObjectProvider, GameObject, TargetArgs>)this["Execute"]; } set { this["Execute"] = value; } }
        public Action<Net.IObjectProvider, GameObject, TargetArgs, byte[]> Execute { get { return (Action<Net.IObjectProvider, GameObject, TargetArgs, byte[]>)this["Execute"]; } set { this["Execute"] = value; } }
        public Func<GameObject, float> Time { get { return (Func<GameObject, float>)this["Time"]; } set { this["Time"] = value; } }
        public virtual float BaseTimeInSeconds { get { return (float)this["Time"]; } set { this["Time"] = value; } }
        public Formula SpeedBonus { get { return (Formula)this["SpeedBonus"]; } set { this["SpeedBonus"] = value; } }
        Message.Types Message { get { return (Message.Types)this["Message"]; } set { this["Message"] = value; } }
        public ConditionCollection Conditions { get { return (ConditionCollection)this["Requirements"]; } set { this["Requirements"] = value; } }
        public Func<GameObject, TargetArgs, float, bool> RangeCheck { get { return (Func<GameObject, TargetArgs, float, bool>)this["Range"]; } set { this["Range"] = value; } }
        public float RangeValue { get { return (float)this["RangeValue"]; } set { this["RangeValue"] = value; } }
        public Func<GameObject, TargetArgs, TargetArgs> TargetSelector { get { return (Func<GameObject, TargetArgs, TargetArgs>)this["TargetSelector"]; } set { this["TargetSelector"] = value; } }
        public ScriptFlow Flow { get { return (ScriptFlow)this["Flow"]; } set { this["Flow"] = value; } }
        public ScriptState ScriptState { get { return (ScriptState)this["State"]; } set { this["State"] = value; } }

        //List<ScriptComponent> ScriptComponents { get; set; }

        #region ScriptBehaviors
        public Dictionary<string, ScriptComponent> ScriptBehaviors { get; set; }
        public T GetComponent<T>() where T : Component, new()
        {
            return (from comp in ScriptBehaviors.Values
                    where comp is T
                    select comp).SingleOrDefault() as T;
        }
        public bool TryGetComponent<T>(Action<T> action) where T : Component, new()
        {
            T component = this.GetComponent<T>();
            if (component.IsNull())
                return false;
            action(component);
            return true;
        }
        public void AddComponent(ScriptComponent comp)
        {
            ScriptBehaviors[comp.ComponentName] = comp;
        }
        public T AddComponent<T>() where T : ScriptComponent, new()
        {
            T component = new T();
            ScriptBehaviors[component.ComponentName] = component;
            return component;
        }
        #endregion

        public Script()
        {
            this.Conditions = new ConditionCollection();
            this.TargetSelector = (actor, target) => target;
            this.SpeedBonus = Formula.GetFormula(Formula.Types.Default);
            this.RangeCheck = (actor, target, range) => true;// Vector3.Distance(actor.Global, target.Global) <= Interaction.DefaultRange;
            this.RangeValue = InteractionOld.DefaultRange;
            this.Flow = ScriptFlow.Instant;
            this.Action = (a) => { };
            this.ScriptState = ScriptState.Unstarted;
            //this.ScriptComponents = new List<ScriptComponent>();
            this.BaseTimeInSeconds = 1;
            this.ScriptBehaviors = new Dictionary<string, ScriptComponent>();
        }

        public Script(
            Script.Types id,
            Action<ScriptArgs> action,
            string name = "unnamed",
            float baseTime = 0,
            Formula speedBonus = null,
            Func<GameObject, TargetArgs, float, bool> range = null,
            float rangevalue = InteractionOld.DefaultRange,
            ConditionCollection reqs = null)
        {
            this.ID = id;
            this.Name = name;
            this.Message = Components.Message.Types.Default;
            this.Action = action;
            this.BaseTimeInSeconds = baseTime;
            this.Conditions = reqs ?? new ConditionCollection();
            this.RangeCheck = range ?? DefaultRangeCheck;// Interaction.DefaultRangeCheck;
            this.RangeValue = rangevalue;
            this.SpeedBonus = speedBonus ?? Formula.GetFormula(Formula.Types.Default);
        }
        //public override object Clone()
        //{
        //    return new Script()
        //    {
        //        ID = this.ID,
        //        Action = this.Action,
        //        SpeedBonus = this.SpeedBonus,
        //        Conditions = this.Conditions,
        //        RangeCheck = this.RangeCheck,
        //        BaseTimeInSeconds = this.BaseTimeInSeconds,
        //        Name = this.Name,
        //       // Execute = this.Execute
        //    };
        //}

        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent)
        {
            //foreach (var comp in this.ScriptComponents)
            foreach (var comp in this.ScriptBehaviors.Values)
                comp.DrawUI(sb, camera, parent);
        }
        
        public InteractionOld GetInteraction(GameObject actor, TargetArgs target)// ObjectEventArgs args)
        {
            InteractionOld inter = new InteractionOld(this.ID, target, GetTimeInMs(actor)) { Name = this.Name, Verb = this.Name, };
            return inter;
        }
        public InteractionOld GetInteraction(GameObject actor, TargetArgs target, byte[] parameters)// ObjectEventArgs args)
        {
            InteractionOld inter = new InteractionOld(this.ID, target, GetTimeInMs(actor)) { Name = this.Name, Verb = this.Name, RangeCheck = this.RangeCheck, Parameters = parameters };
            return inter;
        }
        public float GetTimeInSeconds(GameObject actor)
        {
            return this.BaseTimeInSeconds * (1 - this.SpeedBonus.GetValue(actor));
        }

        public int GetTimeInMs(GameObject actor)
        {
            return (int)(GetTimeInSeconds(actor) * 1000);
        }

        //public bool DefaultRangeCheck(ScriptArgs args, float max, float min, 
        //{
        //    return this.RangeCheck(args.Actor, args.Target, max);
        //}

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
    //        tooltip.Controls.Add(("Time: " + this.GetTime(Player.Actor).ToString("0.##s")).ToLabel(tooltip.Controls.BottomLeft));
            tooltip.Controls.Add(new UI.Label(tooltip.Controls.BottomLeft, "Length: " + this.GetTimeInSeconds(Player.Actor).ToString("0.##s"), null, null, UI.UIManager.FontBold)  { TextColorFunc=()=>Color.CornflowerBlue});
            if(this.RangeCheck == DefaultRangeCheck)
                tooltip.Controls.Add(new UI.Label(tooltip.Controls.BottomLeft, "Range: " + this.RangeValue.ToString("##0m"), null, null, UI.UIManager.FontBold) { TextColorFunc = () => Color.CornflowerBlue });

            this.Conditions.ForEach(req =>
            {
                bool passed = req.Predicate(Player.Actor, parent, new byte[0]);
                
                tooltip.Controls.Add(new UI.Label(tooltip.Controls.BottomLeft, req.ErrorMessage, null, null, UI.UIManager.FontBold) { TextColorFunc = () => passed ? Color.Lime : Color.Red });
            });
        }

        public GameObject ToObject()
        {
            GameObject obj = new GameObject();
            obj["Info"] = new GeneralComponent(GameObject.Types.Ability, ObjectType.Ability, this.Name, "Default description");
            obj.AddComponent<GuiComponent>().Initialize(0);
            obj.AddComponent(this);
            return obj;
        }

        public bool BuildPlan(GameObject parent, TargetArgs target, List<GameObject> sortedEntities, AI.AIPlan plan)
        {
            List<Condition> failed = new List<Condition>();
            if (this.Conditions.Pass(parent, target.Object, failed))
            {
                plan.Enqueue(new AI.AIPlanStep(target, this));
                return true;
            }
            //   if (!script.Requirements.Pass(parent, target, failed))
            foreach (var fail in failed)
                foreach (var pre in fail.Preconditions)
                    foreach (var obj in
                        from obj in sortedEntities
                        where pre.TargetSelector(obj)
                        select obj)
                    {
                        // check if the object offers the script or start it straight away?
                        Script nextscript = Ability.GetScript(pre.Solution);
                        if (nextscript.BuildPlan(parent, new TargetArgs(obj), sortedEntities, plan))
                        {
                            plan.Enqueue(new AI.AIPlanStep(target, this));
                            return true;
                        }
                    }
            return false;
        }

        static public void Start(GameObject actor, string name, float length, Action callback)
        {
            Script script = new ScriptDefault();
            script.AddComponent(new ScriptCallback(callback));
            script.AddComponent(new ScriptTimer(name, length));
            actor.GetComponent<ControlComponent>().StartScript(script, new ScriptArgs(actor.Net, actor));
        }
        static public void Start(GameObject actor, GameObject target, string name, float length, Action callback)
        {
            Script script = new ScriptDefault();
            script.AddComponent(new ScriptCallback(callback));
            script.AddComponent(new ScriptTimer(name, length));
            actor.GetComponent<ControlComponent>().StartScript(script, new ScriptArgs(actor.Net, actor, new TargetArgs(target)));
        }
    }
}
