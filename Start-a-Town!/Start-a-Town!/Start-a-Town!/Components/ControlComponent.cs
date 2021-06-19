using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components
{
    class Progress2
    {
        float Length, T;
        float Percentage { get { return T / Length; } set { T = value * Length; } }
        //Components.Message.Types Action;

        public Progress2(float length, ObjectEventArgs parameters)
        {
            this.Length = length;
            this.Parameters = parameters;
            T = length;
        }

        public Message.Types Advance()
        {
            T -= 1;// GlobalVars.DeltaTime;
            if (T <= 0)
                return Message.Types.True;
            return Message.Types.False;
        }

        ObjectEventArgs Parameters;
    }

    
    public enum InteractionTargetType { None, Self, Sender }
    
    class InteractionTask
    {
        public enum States { Stopped, Running, Finished, Loop }
       // public TimeSpan Length;
        public TimeSpan T;
        public double Time, TotalTime;
       // public float T; //Length, 
        public double Percentage
        {
            get
            {
                //return T.TotalSeconds / Interaction.Time.TotalSeconds;
                return this.Time / this.TotalTime;// Interaction.TimeInMs;
            }
            set
            {
                //T = new TimeSpan(0, 0, (int)(value * Interaction.Time.TotalSeconds));
                Time =  value * Interaction.TimeInMs;
            }
        }//set { T = value * Length; } }
       // GameObject Target;
        public GameObject Parent, Target;
        object[] Args;
        public InteractionOld Interaction;
        string Name;

        public Script Script;
        //Components.Message.Types Action;

        //public Task(GameObject parent, Interaction interaction)
        //{
        //    this.Parent = parent;
        //    this.Interaction = interaction;
        //    this.Action = interaction.Message;
        //}

        public InteractionTask(GameObject parent, GameObject target, Script script)
        {
            this.Parent = parent;
            this.Target = target;
            this.Script = script;
            this.TotalTime = this.Time = script.GetTimeInMs(parent);
        }

        public InteractionTask(GameObject parent, InteractionOld action, params object[] parameters)
        {
            this.Parent = parent;
            this.Interaction = action;
            this.Args = parameters;
            //T = action.Time;
            this.TotalTime = this.Time = action.TimeInMs;
        }



        //public Message.Types Perform()
        //{
        //    T -= GlobalVars.DeltaTime;
        //    if (T <= 0)
        //        return Target.HandleMessage(Args) ? Message.Types.True : Message.Types.False;
        //    //    return Message.Types.True;
        //    //return Message.Types.False;
        //    return Message.Types.Default;
        //}

        public InteractionTask.States Update(Net.IObjectProvider net)
        {
           // if (T.TotalMilliseconds <= 0)
            if (this.Time <= 0)
            {

                if (this.Interaction.OnEnd != null)
                {
                    this.Interaction.OnEnd();
                    //Script.Action(new AbilityArgs(this.Parent));
                    return States.Finished;
                }

                //return Interaction.Source.HandleMessage(Interaction.Message, Parent, Args) ? Task.States.Finished : Task.States.Loop;// Message.Types.True : Message.Types.False;
                if (!Interaction.OnFinish.IsNull())
                    Interaction.OnFinish(Parent, new TargetArgs(Interaction.Source, (Vector3)Args[0]));
                else
                {
                    //Interaction.Source.PostMessage(Interaction.Message, Parent, Args);
                    Script script = Ability.GetScript(Interaction.ScriptID);
                    //  ObjectEventArgs interArgs = ObjectEventArgs.Create(

                    // WARNING! must pass network to script execution
                    //script.Execute(net, Interaction.Recipient.Object, new object[] { Parent });
                    script.Execute(net, Parent, Interaction.Target, Interaction.Parameters);
                    //script.Execute(Parent, Interaction.Recipient,  new byte[0]);
                }
                return States.Finished;
                //  return true;
            }

            //float coef = Interaction.ScriptCached.SpeedBonus.Function(this.Parent);// 1;
            //TimeSpan sub = new TimeSpan(0, 0, 0, 0, (int)(1000 * coef / 60f));
            //T = T.Subtract(sub);
            this.Time -= (double)1000 / Engine.TargetFps;

            return InteractionTask.States.Running;
        }

        public void Restart()
        {
            T = Interaction.Time;
        }

        public override string ToString()
        {
            //return Interaction.Verb + ": " + ((int)((1 - Percentage) * 100)).ToString() + "%";

            //return Script.Name + ": " + ((int)((1 - Percentage) * 100)).ToString() + "%";
            return Interaction.Name + ": " + ((int)((1 - Percentage) * 100)).ToString() + "%";
        }
    }
    //class Attack
    //{
        
    //}
    public class ControlComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Control";
            }
        }

        public enum States { Ready, Walking, Running, ControlLoss, Interacting }


        Progress2 Progress { get { return (Progress2)this["Progress"]; } set { this["Progress"] = value; } }
        ObjectEventArgs Action { get { return (ObjectEventArgs)this["Action"]; } set { this["Action"] = value; } }
        InteractionTask Task { get { return (InteractionTask)this["Task"]; } 
            set { this["Task"] = value; } }
        float Cooldown { get { return (float)this["Cooldown"]; } set { this["Cooldown"] = value; } }
    //    bool InControl { get { return (bool)this["InControl"]; } set { this["InControl"] = value; } }
        Attack.States Attacking { get { return (Attack.States)this["Attacking"]; } set { this["Attacking"] = value; } }

        States State { get { return (States)this["State"]; } set { this["State"] = value; } }
        //public Script CurrentScript { get { return (Script)this["Script"]; } set { this["Script"] = value; } }
        public ScriptCollection RunningScripts { get { return (ScriptCollection)this["RunningScripts"]; } set { this["RunningScripts"] = value; } }
        public event EventHandler<ObjectEventArgs> MessageReceived;

        public ControlComponent()
        {
          //  this.CurrentScript = null;
            this.RunningScripts = new ScriptCollection();
            this.State = States.Ready;
            this["Task"] = null;
            Cooldown = 0f;
            //InControl = true;
            this.Attacking = Components.Attack.States.Ready;
        }
        internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.StartScript:
                    ScriptEventArgs args = e.Data.Translate<ScriptEventArgs>(e.Network);
                    ScriptArgs abar = new ScriptArgs(e.Network, parent, args.Target, args.Parameters);
                    Script.Types scriptid = args.ScriptID;
                    TryStartScript(scriptid, abar);
                    break;

                case Message.Types.FinishScript:
                    args = e.Data.Translate<ScriptEventArgs>(e.Network);
                    abar = new ScriptArgs(e.Network, parent, args.Target, args.Parameters);
                    scriptid = args.ScriptID;
                    FinishScript(scriptid, abar);
                    break;

                default:
                    break;
            }
        }
        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            GameObject target;
            OnMessageReceived(e);
            switch (e.Type)
            {
                case Message.Types.StartScript:
                    ScriptEventArgs args = e.Data.Translate<ScriptEventArgs>(e.Network);
                    ScriptArgs abar = new ScriptArgs(e.Network, parent, args.Target, args.Parameters);
                    Script.Types scriptid = args.ScriptID;
                    TryStartScript(scriptid, abar);
                    return true;

                case Message.Types.FinishScript:
                    args = e.Data.Translate<ScriptEventArgs>(e.Network);
                    abar = new ScriptArgs(e.Network, parent, args.Target, args.Parameters);
                    scriptid = args.ScriptID;
                    FinishScript(scriptid, abar);
                    return true;

                case Message.Types.BeginInteraction:
                    if (State == States.ControlLoss)
                        return true;

                    // TODO: put interaction starting logic in SCRIPT class

                    Script.Types scriptID = (Script.Types)e.Parameters[0];
                    TargetArgs targetArgs = e.Parameters[1] as TargetArgs;
                    byte[] parameters = e.Parameters[2] as byte[];


                    Script script = Ability.GetScript(scriptID);

                    ConditionCollection failed = new ConditionCollection();
                    //if (!script.Requirements.TryConditions(parent, targetArgs.Object, failed))
                    //{
                    //    e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.InteractionFailed, new TargetArgs(parent), e.Parameters));// e.Data));
                    //    return true;
                    //}
                    if (!script.Conditions.Pass(parent, targetArgs.Object, parameters, failed))
                    {
                        e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.InteractionFailed, new TargetArgs(parent), e.Parameters));// e.Data));
                        return true;
                    }
                    if (!script.RangeCheck(parent, targetArgs, script.RangeValue))
                        e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.OutOfRange));

                    InteractionOld interaction = script.GetInteraction(parent, targetArgs, parameters);
                    
                    Task = new InteractionTask(parent, interaction);

                    State = States.Interacting;
                    e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Perform));

                    return true;
                
                case Message.Types.BeginScript:
                    script = e.Parameters[0] as Script;
                    targetArgs = e.Parameters[1] as TargetArgs;
                    Task = new InteractionTask(parent, targetArgs.Object, script);
                    State = States.Interacting;
                    e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Perform));
                    return true;

                case Message.Types.Perform:
                    if (Task == null)
                    {
                        parent.PostMessage(Message.Types.NoInteraction);
                        return true;
                    }

                    if (Cooldown > 0)
                        return true;

                    if (!Task.Interaction.Range(parent, Task.Interaction.Target.Object))//.Source))
                    {
                          e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.InteractionFailed));
                        return true;
                    }

                    switch (Task.Update(e.Network))
                    {
                        case Components.InteractionTask.States.Finished:
                            //GameObject.PostMessage(parent, Message.Types.InteractionFinished, parent, Task.Interaction);
                            e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.InteractionFinished));
                            Task = null;
                            break;

                        case Components.InteractionTask.States.Loop:
                            Task.Restart();
                            break;
                        default:
                            break;
                    }

                    return true;
                case Message.Types.Task:
                    Task = e.Parameters[0] as InteractionTask;
                    break;

                case Message.Types.InteractionFinished:
                    State = States.Ready;
                    break;

                case Message.Types.InteractionFailed:
                    State = States.Ready;

                    //interaction = e.Parameters[0] as Interaction;
                    List<Condition> failedConditions = new List<Condition>();
                    //if (interaction.TryConditions(parent, interaction.Source, failedConditions))
                    //    break;

                    //script = (Script.Types)e.Parameters[0];
                    script = Ability.GetScript((Script.Types)e.Parameters[0]);
                    TargetArgs ta = e.Parameters[1] as TargetArgs;
                    byte[] ar = e.Parameters[2] as byte[];
                    if (script.Conditions.Pass(parent, ta.Object, ar, failedConditions))
                        throw new Exception("Received received invalid interaction failed message.");

                    string text = "";
                    foreach (Condition condition in failedConditions)
                        text += condition.ErrorMessage + "\n";

                    //GameObject.PostMessage(parent, Message.Types.Speak, parent, text.TrimEnd('\n'));
                    e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Speak, new TargetArgs(parent),text.TrimEnd('\n')));
                    Task = null;
                    break;
                case Message.Types.Interrupt:
                    State = States.Ready;
                    Task = null;
                    break;
                case Message.Types.OutOfRange:

                    //GameObject.PostMessage(parent, Message.Types.Speak, parent, "I'm too far away.");
                    e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Speak, new TargetArgs(parent), "I'm too far away."));
                    Task = null;
                    break;

                case Message.Types.Move:
                    if (State == States.ControlLoss)
                        return true;

                    if (State == States.Interacting)
                        e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Interrupt));
        

                    float speed;

                    //MoveEventArgs moveArgs = e.Parameters.Translate<MoveEventArgs>(e.Network);
                    MoveEventArgs moveArgs = e.Data.Translate<MoveEventArgs>(e.Network);
                    Move(parent, moveArgs.Direction, moveArgs.Speed);

                    // log state change to server

                    return true;

                case Message.Types.MoveToObject:
                    if (State == States.ControlLoss)
                        return true;
                    GameObject tar = e.Parameters[0] as GameObject;
                    float range = (float)e.Parameters[1];
                    speed = (float)e.Parameters[2];
                    MoveToObject(parent, tar, range, speed);
                    return true;

                case Message.Types.ControlDisable:
                    State = States.ControlLoss;
                    return true;

                case Message.Types.ControlEnable:
                    State = States.Ready;
                    return true;


                default:
                    break;
            }
            return false;
        }
        public void StartScript(Script script, ScriptArgs abar)
        {
            if (script.ConditionsCheck(abar))
                return;

            script.Start(abar);
            if (script.ScriptState != ScriptState.Finished)
                this.RunningScripts[script.ID] = script;
        }
        public void StartScript(Script.Types scriptid, ScriptArgs abar)
        {
            Script script = Ability.GetScript(scriptid);
            if (script.ConditionsCheck(abar))
                return;
           
            script.Start(abar);
            if (script.ScriptState != ScriptState.Finished)
                this.RunningScripts[script.ID] = script;
        }
        public bool TryStartScript(Script scriptTemplate, ScriptArgs abar)
        {
            if (scriptTemplate.IsNull())
                return false;
            Script script = scriptTemplate.Clone() as Script;
            if (RunningScripts.ContainsKey(script.ID))
                return false;

            if (script.ConditionsCheck(abar))
                return false;
            
            script.Start(abar);
            if (script.ScriptState != ScriptState.Finished)
                this.RunningScripts[script.ID] = script;
            return true;
        }
        public bool TryStartScript(Script.Types scriptid, ScriptArgs abar)
        {
            if (RunningScripts.ContainsKey(scriptid))
                return false;
            Script script = Ability.GetScript(scriptid);
            if (script.ConditionsCheck(abar))
                return false;

            script.Start(abar);
            if (script.ScriptState != ScriptState.Finished)
                this.RunningScripts[script.ID] = script;
            return true;
        }
        public bool TryStartScript(Script.Types scriptid, ScriptArgs abar, out Script script)
        {
            script = null;
            if (RunningScripts.ContainsKey(scriptid))
                return false;
            script = Ability.GetScript(scriptid);
            if (script.ConditionsCheck(abar))
                return false;

            script.Start(abar);
            if (script.ScriptState != ScriptState.Finished)
                this.RunningScripts[script.ID] = script;
            return true;
        }
        public void FinishScript(Script.Types scriptid, ScriptArgs abar)
        {
            Script script;
            if (!this.RunningScripts.TryGetValue(scriptid, out script))
                return;
            script.Finish(abar);
           // this.RunningScripts.Remove(script.ID);
        }
        public void FinishScript(Script script)
        {
            if (!this.RunningScripts.TryGetValue(script.ID, out script))
                return;
            script.Finish();
        }
        public void FinishScript(Script.Types scriptid)
        {
            Script script;
            if (!this.RunningScripts.TryGetValue(scriptid, out script))
                return;
            script.Finish();
        }
        public void CancelScript(Script.Types scriptid, ScriptArgs abar)
        {
            Script script;
            if (!this.RunningScripts.TryGetValue(scriptid, out script))
                return;
            script.Stop(abar);
            // this.RunningScripts.Remove(script.ID);
        }
        public void TryGetScript(Script.Types scriptid, Action<Script> scriptAction)
        {
            Script script;
            if (!this.RunningScripts.TryGetValue(scriptid, out script))
                return;
            scriptAction(script);
        }
        public void TryGetScript<T>(Action<T> scriptAction) where T : Script
        {
            T script = this.RunningScripts.Values.FirstOrDefault(s => s is T) as T;
            if (script.IsNull())
                return;
            scriptAction(script);
        }
        public void BeginInteraction(Net.IObjectProvider net, GameObject parent, InteractionOld interaction)
        {
            ConditionCollection failed = new ConditionCollection();
            if (!interaction.Conditions.Pass(parent, interaction.Target.Object, failed))
            {
                net.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.InteractionFailed, new TargetArgs(parent)));// e.Data));
                return;
            }

            if (!interaction.Range(parent, interaction.Target.Object))
                net.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.OutOfRange));

            Task = new InteractionTask(parent, interaction);

            State = States.Interacting;
            net.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Perform));
        }

        internal void Interrupt(ScriptArgs abar)
        {
            foreach (var scr in this.RunningScripts.Values)
                scr.Interrupt(abar);
        }
        internal void Interrupt(GameObject parent)
        {
            foreach (var scr in this.RunningScripts.Values)
                scr.Interrupt(new ScriptArgs(parent.Net, parent));
        }
        private void MoveToObject(GameObject parent, GameObject target, float range, float speed)
        {
            Vector3 difference = (target.Global - parent.Global);
           // difference.Z = 0;

            float walkSpeed = (float)parent["Stats"]["Walk Speed"] * speed;
            float length = difference.Length();
            if (length - range < walkSpeed)
            {
            
                difference.Z = 0;
                difference.Normalize();
                throw new NotImplementedException();
                //parent.PostMessage(Message.Types.SetPosition, parent, parent.Global + difference * (length - range));
                return;
            }
            difference.Z = 0;
            difference.Normalize();
            if (walkSpeed == 0)
                Log.Enqueue(Log.EntryTypes.System, "Warning! " + parent.Name + " is trying to move but their movement speed is zero!");
            parent.Velocity = new Vector3(difference.X * walkSpeed, difference.Y * walkSpeed, parent.Transform.GetProperty<Vector3>("Speed").Z);
        }

        float acc = 0f;
        private void Move(GameObject parent, Vector3 direction, float speed)
        {
            State = speed > 0.5f ? States.Running : States.Walking;

            acc = Math.Min(1, acc + 0.1f);

            float walkSpeed = (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.WalkSpeed, 0f)) * speed * acc * PhysicsComponent.Walk;
            if (walkSpeed == 0)
                Log.Enqueue(Log.EntryTypes.System, "Warning! " + parent.Name + " is trying to move but their movement speed is zero!");
            parent.Velocity = new Vector3(direction.X * walkSpeed, direction.Y * walkSpeed, parent.Velocity.Z);
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            foreach (var script in this.RunningScripts.Values.ToList())
            {
                script.Update(net, parent);
                if (script.ScriptState == ScriptState.Finished)
                    this.RunningScripts.Remove(script.ID);
            }
            if (Task != null)
                //parent.PostMessage(Message.Types.Perform, parent);
                net.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Perform));
            Cooldown = Math.Max(0, Cooldown - 1); //- GlobalVars.DeltaTime);

            //this.NearbyTimer -= GlobalVars.DeltaTime;
            //if (NearbyTimer <= 0)
            //{
            //    NearbyTimer = Engine.TargetFps;
            //    this.NearbyObjects = parent.GetNearbyObjects(
            //}
        }

        //static public GameObjectSlot GetAbility(GameObject parent, AbilitySlot abilitySlot)
        //{
        //    GameObjectSlot hauling = (GameObjectSlot)parent["Inventory"]["Holding"];
        //    GameObjectSlot dragging = DragDropManager.Instance.Item as GameObjectSlot;
        //    switch (abilitySlot)
        //    {
        //        case AbilitySlot.Primary:
        //        case AbilitySlot.Secondary:
        //            return parent["Equipment"].GetProperty<GameObjectSlot>(Stat.Mainhand.Name).Object["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities")[(int)abilitySlot];
        //        case AbilitySlot.Function1:
        //            if(hauling.Object == null)
        //                return Ability.Activate;
        //            return GameObjectSlot.Empty;
        //        case AbilitySlot.PickUp:
        //            if (hauling.Object == null)
        //                return Ability.PickingUp;
        //            else
        //                return Ability.Drop;
        //        default:
        //            return GameObjectSlot.Empty;
        //    }
        //}
        static public bool HasAbility(GameObject parent, Message.Types abilityMsg)
        {
            //Dictionary<AbilitySlot, GameObjectSlot> abilities = GetAbility(parent);
            //if (abilities.Count == 0)
            //    return false;
            //return abilities.Values.ToList().FindAll(ability => ability.HasValue).Find(ability => (Message.Types)ability.Object["Ability"]["Message"] == abilityMsg) != null;

            List<GameObjectSlot> abilities = GetAbilities(parent);
            if (abilities.Count == 0)
                return false;
            return abilities.FindAll(ability => ability.HasValue).Find(ability => (Message.Types)ability.Object["Ability"]["Message"] == abilityMsg) != null;
        }
        static public bool TryGetAbility(GameObject parent, Message.Types abilityMsg, out GameObjectSlot ability)
        {
            //Dictionary<AbilitySlot, GameObjectSlot> abilities = GetAbility(parent);
            //if (abilities.Count == 0)
            //{
            //    ability = null;
            //    return false;
            //}
            //ability = abilities.Values.ToList().FindAll(a => a.HasValue).Find(a => (Message.Types)a.Object["Ability"]["Message"] == abilityMsg);
            //return ability != null;
            List<GameObjectSlot> abilities = GetAbilities(parent);
            if (abilities.Count == 0)
            {
                ability = null;
                return false;
            }
            ability = abilities.FindAll(a => a.HasValue).Find(a => (Message.Types)a.Object["Ability"]["Message"] == abilityMsg);
            return ability != null;
        }
        static public Dictionary<AbilitySlot, GameObjectSlot> GetAbility(GameObject parent)
        {
            GameObjectSlot dragging = DragDropManager.Instance.Item as GameObjectSlot;
            Dictionary<AbilitySlot, GameObjectSlot> list = new Dictionary<AbilitySlot, GameObjectSlot>();
            //GameObjectSlot hauling = (GameObjectSlot)parent["Inventory"]["Holding"];
            //list.Add(AbilitySlot.Primary, hauling.Object == null ? parent["Equipment"].GetProperty<BodyPart>(Stat.Mainhand.Name).Object["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities")[0] : FunctionComponent.GetAbility(hauling.Object, 0));//hauling.Object["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities")[0]);
            //list.Add(AbilitySlot.Secondary, hauling.Object == null ? Ability.Activate : FunctionComponent.GetAbility(hauling.Object, 1));// hauling.Object["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities")[1]);
            //list.Add(AbilitySlot.Function1, hauling.Object == null ? parent["Equipment"].GetProperty<BodyPart>(Stat.Mainhand.Name).Object["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities")[1] : GameObjectSlot.Empty);
            //list.Add(AbilitySlot.PickUp, hauling.Object == null ? Ability.PickingUp : Ability.Drop);
            //list.Add(AbilitySlot.Function3, Ability.ManageEquipment);
            return list;
        }
        static public GameObjectSlotCollection GetAbilities(GameObject actor)
        {
            GameObjectSlotCollection list = new GameObjectSlotCollection();
           // GameObjectSlot holding = (GameObjectSlot)actor["Inventory"]["Holding"];
           // if (holding.HasValue)
           // {
           //     list.AddRange(new GameObjectSlot[] { Ability.Drop, Ability.Throw });
           //     list.AddRange(FunctionComponent.GetAbilities(holding.Object));//holding.Object["Abilities"]["Abilities"] as List<GameObjectSlot>);
           // }
           //// else
           //     list.AddRange(new GameObjectSlot[] { Ability.PickingUp });
           // foreach (KeyValuePair<string, object> prop in actor["Equipment"].Properties)
           // {
           //     BodyPart slot = prop.Value as BodyPart;
           //     //if (!slot.HasValue)
           //     //    continue;
           //     if (slot.Object.IsNull())
           //         continue;
           //     list.AddRange(slot.Object["Abilities"]["Abilities"] as List<GameObjectSlot>);
           // }
           // list.Add(Ability.ManageEquipment);
            return list;
        }
        static public bool TryGetAbility(GameObject parent, AbilitySlot slot, out Message.Types abilityMsg)
        {
            Dictionary<AbilitySlot, GameObjectSlot> dictionary = GetAbility(parent);
            GameObjectSlot objSlot = dictionary[slot];
            if (objSlot.HasValue)
            {
               abilityMsg = (Message.Types)objSlot.Object["Ability"]["Message"];
               return true;
            }
            abilityMsg = Message.Types.Default;
            return false;
        }
        //static public List<GameObjectSlot> GetAbility(GameObject parent)
        //{
        //    List<GameObjectSlot> list = new List<GameObjectSlot>(parent["Equipment"].GetProperty<GameObjectSlot>(Stat.Mainhand.Name).Object["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities"));
        //    list.Add(Ability.Activate);
        //    return list;
        //}

        void OnMessageReceived(ObjectEventArgs a)
        {
            if (MessageReceived != null)
                MessageReceived(this, a);
        }

        //Message.Types Perform(GameObject target, GameObjectEventArgs a)
        //{
        //    return target.PostMessage(a) ? Message.Types.True : Message.Types.False;
        //}

        public override object Clone()
        {
            return new ControlComponent();
        }

        static public ControlComponent Create(GameObject obj)
        {
            Player.Actor = obj;
            ControlComponent comp = new ControlComponent();
            obj["Control"] = comp;
            Rooms.Ingame.Instance.ToolManager.ActiveTool = new PlayerControl.DefaultTool();
     //       comp.MessageReceived += new EventHandler<MessageArgs>(Player.Player_MessageReceived);
            return comp;
        }

        public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            foreach (var script in this.RunningScripts)
                script.Value.DrawUI(sb, camera, parent);
        }

        //public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        //{
        //    if (Task == null)
        //        return;
        //    //if (Task.Interaction.Time == TimeSpan.Zero)
        //    //    return;
        //    if (Task.Time == 0)
        //        return;

        //    Vector3 global = parent.Global; // Player.Actor.Global

        //    Rectangle bounds = camera.GetScreenBounds(global, parent["Sprite"].GetProperty<Sprite>("Sprite").GetBounds());
        //    Vector2 scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
        //    Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
        //    Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
        //    InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, 1 - (float)Task.Percentage);//t / Length);
        //    UIManager.DrawStringOutlined(sb, Task.ToString(), textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        //    //Bar.Draw(sb, new Vector2(UIManager.Width / 2, UIManager.Height / 2) - new Vector2(Bar.DefaultWidth / 2, Bar.DefaultHeight / 2), Bar.DefaultWidth, t / Length);
        //}

        
    }
}
