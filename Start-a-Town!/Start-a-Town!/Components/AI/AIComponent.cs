using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.UI;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    public class Memory
    {
        public enum States { Invalid, Valid }

        public States State;
       // public bool Valid;
        public GameObject Object;
        public List<string> Needs;
        public float Score, Decay, Interest;
        Memory(GameObject obj, float interest, float score, float decay)
        {
            this.Interest = interest;
            this.Object = obj;
            this.Score = score;
            this.Decay = decay;
            //  this.Needs = new List<string>();// new NeedCollection();// new List<Need>(needs);
            this.State = States.Invalid;
        }
        public Memory(GameObject obj, float interest, float score, float decay, GameObject actor)
        {
            this.Interest = interest;
            this.Object = obj;
            this.Score = score;
            this.Decay = decay;
          //  this.Needs = new List<string>();// new NeedCollection();// new List<Need>(needs);
            this.State = States.Invalid;
            Validate(actor);
            //foreach (var need in needs)
            //    this.Needs.Add(need);

            //foreach (Need need in needs)
            //    if (need != null)
            //    {
            //        //this.Needs.Add(need.Name, need);
            //        Need existingNeed;
            //        if (this.Needs.TryGetValue(need.Name, out existingNeed))
            //        {
            //            if (need.Value >= existingNeed.Value)
            //                this.Needs[need.Name] = need;
            //        }
            //        else
            //            this.Needs[need.Name] = need;
            //    }
        }
        public Memory Refresh(GameObject parent)
        {
            this.Score = 100;
            if (State == States.Invalid)
                Validate(parent);
            return this;
        }
        public bool Update()
        {
            Score -= Decay;
            if (Score <= 0)
                return true;
            return false;
        }
        public override string ToString()
        {
            return Object.Name + " - Interest: " + Interest + " - Score: " + Score;
        }
        public void Validate(GameObject actor)
        {
            this.State = Memory.States.Valid;
            this.Needs = new List<string>();
            //this.Object.Query(actor).ForEach(i => i.NeedEffects.ForEach(n=>this.Needs.Add(n.Name)));//.ToList();
        }
        
    }


    class AIComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "AI";
            }
        }

        readonly Behavior Current;
        public Guid Guid = Guid.NewGuid();
        static public Dictionary<Guid, GameObject> Registry = new Dictionary<Guid, GameObject>();
        static public Guid GetGuid(GameObject agent)
        {
            return agent.GetComponent<AIComponent>().Guid;
        }
        static public GameObject GetAgent(Guid guid)
        {
            return Registry[guid];
        }
        public void Sync(int seed)
        {
            this.State.InSync = true;
        }

        static public void Invalidate(GameObject obj)
        {
            // TODO: signal each npc that remembers obj, that obj's state has changed, so that they evaluate it again next time they see it
            throw new NotImplementedException();
        }
        Behavior Root;// { get { return (BehaviorComposite)this["Root"]; } set { this["Root"] = value; } }
        PersonalityComponent Personality { get { return (PersonalityComponent)this["Personality"]; } set { this["Personality"] = value; } }
        Knowledge Knowledge { get { return (Knowledge)this["Memory"]; } set { this["Memory"] = value; } }
        bool Running { get { return (bool)this["Running"]; } set { this["Running"] = value; } }
        ThoughtCollection Thoughts { get { return (ThoughtCollection)this["Thoughts"]; } set { this["Thoughts"] = value; } }
        public AIState State;
        public bool Enabled = true;
        public AIComponent()//Actor actor) : base(actor)
        {
            this.Running = true;
            this.Thoughts = new ThoughtCollection();
            this.Knowledge = new Knowledge();
            //this.State = new AIState(actor) { Knowledge = this.Knowledge };//, Personality);
            this.Root = null;
        }
        public override void Initialize(GameObject parent, RandomThreaded random)
        {
            this.State.Generate(parent, random);
        }

        internal T FindBehavior<T>() where T : Behavior
        {
            return (this.Root as BehaviorComposite).Find(typeof(T)) as T;
        }

        public override void MakeChildOf(GameObject parent)
        {
            //this.State.Parent = parent;
            this.State = new AIState(parent as Actor) { Knowledge = this.Knowledge };//, Personality);
        }

        public AIComponent Initialize(Behavior root)
        {
            this.Root = root;
            return this;
        }

        public Behavior GetCurrentBehavior()
        {
            return (this.Root as BehaviorQueue).Current;
        }

        public override void Tick(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            if (net is Client) // do i want to run some deterministic behaviors locally too? UPDATE: NO
                return;
            if (!Running)
                return;
            if(this.Enabled)
                Root.Execute(parent as Actor, this.State);
            return;
            //if (!this.State.InSync)
            //    if (net is Server)
            //        (net as Server).SyncAI(parent);

        }

        //static public List<GameObject> Agents = new List<GameObject>();

        public override void OnSpawn(IObjectProvider net, GameObject parent)
        {
            this.State.Leash = parent.Global;
            //this.Root.OnSpawn(parent, this.State);
            //this.Tick(net, parent);
        }
        public override void OnDespawn(GameObject parent)
        {
            //Agents.Remove(parent);
            base.OnDespawn(parent);
        }

        /// <summary>
        /// TODO: only add entities in memory if there's LOS ?
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        //List<GameObject> UpdateNearbyObjects(GameObject parent)
        //{
        //    return parent.GetNearbyObjects( 
        //    range: range => range < 16,
        //    action: obj =>
        //        {

        //            Memory mem;
        //            if (!Knowledge.Objects.TryGetValue(obj, out mem))
        //            {
        //                List<InteractionOld> interactions = new List<InteractionOld>();
        //                obj.Query(parent, interactions);
        //                Knowledge.Objects[obj] = obj.ToMemory(parent);
        //            }
        //            else
        //                mem.Refresh(parent);
        //        });

        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            if (parent.Net as Server == null) // why did i do this?
                return true;
            switch (e.Type)
            {


                case Message.Types.SyncAI:
                    var seed = (int)e.Parameters[0];
                    this.Sync(seed);
                    break;




                case Message.Types.Think:
                    var thought = new Thought() { Time = DateTime.Now.ToTime(), Title = (string)e.Parameters[0], Text = (string)e.Parameters[1] };
                    Thoughts.Add(thought);
                    return true;

                case Message.Types.ManageEquipment:
                    throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.ManageEquipmentOk, parent);

                case Message.Types.AIStart:
                    Running = true;
                    return true;
                case Message.Types.AIStop:
                    Running = false;
                    return true;
                case Message.Types.AIToggle:
                    Running = !Running;
                    return true;

                case Message.Types.ObjectStateChanged:
                    GameObject obj = e.Sender;//e.Parameters[0] as GameObject;
                    //foreach (var memory in this.Knowledge.Objects)
                    //{
                    //}
                    Memory mem;
                    if (this.Knowledge.Objects.TryGetValue(obj, out mem))
                    {

                        mem.State = Memory.States.Invalid;
                        throw new NotImplementedException();
                        //parent.PostMessage(Message.Types.Think, parent, obj.Name + "'s state has changed.", "It seems " + obj.Name + "'s state has changed since the last time I checked it out...");
                    }
                    return true;

             

                case Message.Types.Remember:
                    var memObj = e.Parameters[0] as GameObject;
                    var a = e.Parameters[1] as Action<Memory>;
                    Memory m;
                    if (!Knowledge.Objects.TryGetValue(memObj, out m))
                        return true;
                    a(m);
                    return true;

              

                default:
                    bool result = false;
                    //this.Root.HandleMessage(parent, e);
                    return result;
            }
            //return false;
            return false;
        }
        internal override void OnGameEvent(GameObject gameObject, GameEvent e)
        {
            if (e.Type == Message.Types.BlockChanged || e.Type == Message.Types.BlocksChanged)
            {
                if (!this.State.Path?.IsValid(gameObject as Actor) ?? false)
                    this.State.Path = null;
            }
        }
        //internal override void HandleRemoteCall(GameObject gameObject, ObjectEventArgs e)
        //{
        //    this.Root.HandleRPC(gameObject, e);
        //    //switch(e.Type)
        //    //{
        //    //    case Message.Types.AssignJob:
        //    //        "ole2".ToConsole();
        //    //        break;

        //    //    default:
        //    //        break;
        //    //}
        //}
        //internal override void HandleRemoteCall(GameObject gameObject, Message.Types type, System.IO.BinaryReader r)
        //{
        //    this.Root.HandleRPC(gameObject, type, r);

        //}
        //public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        //{
        //    //foreach (var child in this.Behaviors)
        //    //    child.GetRightClickAction(parent, actions);
        //    this.Root.GetRightClickAction(parent, actions);
        //}

        //internal override void GetAvailableTasks(GameObject parent, List<Interaction> list)
        //{
        //    this.Root.GetInteractions(parent, list);
        //}

        //public override void Query(GameObject parent, List<InteractionOld> list)// GameObjectEventArgs e)
        //{

        //    foreach (Behavior bhav in Behaviors)
        //        bhav.Query(parent, list);

        //}

        //public override void GetDialogueOptions(GameObject parent, GameObject speaker, DialogueOptionCollection options)
        //{
        //    this.Behaviors.ForEach(foo => foo.GetDialogueOptions(parent, speaker, options));

        //    options.AddRange(new DialogueOption[]{
        //        "Thoughts".ToDialogueOption(HandleConversation)
        //    });
        //}
        //public override void GetDialogOptions(GameObject parent, GameObject speaker, List<DialogOption> options)
        //{
        //    this.Root.GetDialogOptions(parent, speaker, options);
        //}
        public Conversation.States HandleConversation(GameObject parent, GameObject speaker, string option, out string speech, out DialogueOptionCollection options)
        {
            switch (option)
            {
                case "Thoughts":
                    speech = "Here's what I was thinking about...";
                    Log.Command(Log.EntryTypes.Thoughts, parent);
                    break;
                default:
                    speech = "I don't know about " + option + ".";
                    break;
            }
            speech += "\n\nAnything else?";// I can help you with?";
            options = parent.GetDialogueOptions(speaker);
            return Conversation.States.InProgress;
        }

        public override object Clone()
        {
            //AIComponent ai = new AIComponent();
            //ai.Personality = Personality.Clone() as Personality;
            //foreach (Behavior bh in Behaviors)
            //    ai.Behaviors.Add(bh.Clone() as Behavior);

            AIComponent ai = new AIComponent().Initialize(
                //this.Personality.Clone() as Personality, 
                this.Root.Clone() as Behavior);
            return ai;
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            if(Current!=null)
                tooltip.Controls.Add(new UI.Label(tooltip.Controls.Last().BottomLeft, Current.ToString(), fill: Color.Lime)
                {
                    TextFunc = () => Current.ToString(),
                });
        }

        public override string ToString()
        {
            return //"Depth: " + Depth + 
               // "\nDeepest: " +GetDeepest().ToString() +
               (Current != null ? "Current: " + Current.ToString() : "<null>") +
                "\n" + base.ToString();
        }

        static public AIState GetState(GameObject entity)
        {
            return entity.GetComponent<AIComponent>().State;
        }

        internal override List<SaveTag> Save()
        {
            var save = new List<SaveTag>();
            save.Add(new SaveTag(SaveTag.Types.ByteArray, "Guid", this.Guid.ToByteArray()));
            save.Add(this.State.Save("State"));
            save.Add(this.Root.Save("Root"));
            return save;
        }
        internal override void Load(SaveTag save)
        {
            save.TryGetTagValue<byte[]>("Guid", v => this.Guid = new Guid(v));
            this.State.Load(save["State"]);
            this.Root.Load(save["Root"]);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Guid.ToByteArray());
            this.State.Write(w); // i dont want to sync the state for the time being
            this.Root.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Guid = new Guid(r.ReadBytes(16));
            this.State.Read(r);// i dont want to sync the state for the time being
            this.Root.Read(r);
        }

        internal override void GetInterface(GameObject gameObject, Control box)
        {
            base.GetInterface(gameObject, box);
            //var label = new Label("Behavior: ") { TextFunc = () => "Behavior: " + (this.State.Job != null ? this.State.Job.ToString() : "none") };
            //box.AddControls(label);
            box.AddControls(new Interface(gameObject.RefID));
        }
        internal override void MapLoaded(GameObject parent)
        {
            this.State.MapLoaded(parent as Actor);
            this.Root.MapLoaded(parent as Actor);
        }
        public override void OnObjectLoaded(GameObject parent)
        {
            this.State.ObjectLoaded(parent);
            this.Root.ObjectLoaded(parent);
        }
        class Interface : GroupBox
        {
            readonly Label Label;
            AIState State;
            int EntityID;
            string GetText()
            {
                //get { 
                return "Behavior: " + (this.State.Job != null ? this.State.Job.ToString() : "none");
                //}
            }
            public Interface(AIState state)
            {
                this.State = state;
                this.Label = new Label("Behavior: ") { Width = 300, TextFunc = this.GetText };
                this.AddControls(this.Label);
            }
            public Interface(int entity)
            {
                //this.Label = new Label("Behavior: none");
                //this.Label = new Label(300);
                //this.Label.Text = "Behavior: ";
                this.Label = new Label("Behavior: ") { Width = 300 };
                this.AddControls(this.Label);
                this.EntityID = entity;
            }
            internal override void OnGameEvent(GameEvent e)
            {
                switch (e.Type)
                {
                    case Message.Types.JobAccepted:
                        //this.Label.Text = this.Text;
                        if ((int)e.Parameters[0] == this.EntityID)
                            this.Label.Text = "Behavior: " + (string)e.Parameters[1];
                        break;

                    case Message.Types.JobComplete:
                        if ((int)e.Parameters[0] == this.EntityID)
                            this.Label.Text = "Behavior: ";
                        break;


                    default:
                        break;
                }
            }
        }

        internal void MoveOrder(TargetArgs target, bool enqueue)
        {
            this.State.AddMoveOrder(target, enqueue);
            //this.State.MoveOrder = target;
        }
        public override void DrawAfter(MySpriteBatch sb, Camera cam, GameObject parent)
        {
            var serverentity = parent;// Server.Instance.GetNetworkObject(parent.InstanceID);
            var state = AIState.GetState(serverentity);
            if (state == null) return; // we are in client
            var path = state.Path;// this.State.Path;
            if (!UISelectedInfo.IsSelected(parent))
                return;
            var first = true;
            if (path != null)
            {
                cam.DrawBlockMouseover(sb, parent.Map, path.Current, Color.Lime);

                if (path.Stack != null)
                {
                    foreach (var global in path.Stack)
                    {
                        cam.DrawBlockMouseover(sb, parent.Map, global, first ? Color.Red : Color.Blue);
                        first = false;
                    }
                }
            }
            foreach (var target in state.MoveOrders)
                cam.DrawBlockMouseover(sb, parent.Map, target.Global.Above(), Color.Yellow);
        }

        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Label() { TextFunc = () => string.Format("Current Task: {0}", this.State.TaskString) });
        }
    }
}
