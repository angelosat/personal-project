using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components
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
            //this.Needs = this.Object.Query(actor).Select(i => i.Effect.Key).ToList();
            this.Needs = new List<string>();
            this.Object.Query(actor).ForEach(i => i.NeedEffects.ForEach(n=>this.Needs.Add(n.Name)));//.ToList();
        }
        
    }


    class AIComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "AI";
            }
        }

        Behavior Current;

        int Seed;
        public void Sync(int seed)
        {
            //this.Seed = seed + parent.Network.ID.GetHashCode(); // cache hash on object initialization
            this.Seed = seed;
            this.Root.Sync(this.Seed);
            this.State.InSync = true;
        }

        static public void Invalidate(GameObject obj)
        {
            // TODO: signal each npc that remembers obj, that obj's state has changed, so that they evaluate it again next time they see it
            throw new NotImplementedException();
            //NpcComponent.NpcDirectory.ForEach(foo => GameObject.PostMessage(foo, Message.Types.ObjectStateChanged, obj));
        }
        Behavior Root { get { return (Behavior)this["Root"]; } set { this["Root"] = value; } }
        List<Behavior> Behaviors { get { return (List<Behavior>)this["Behaviors"]; } set { this["Behaviors"] = value; } }
      //  GameObject Focus { get { return (GameObject)this["Focus"]; } set { this["Focus"] = value; } }
        Personality Personality { get { return (Personality)this["Personality"]; } set { this["Personality"] = value; } }
        //float Timer { get { return (float)this["NeedsTimer"]; } set { this["NeedsTimer"] = value; } }
        Knowledge Knowledge { get { return (Knowledge)this["Memory"]; } set { this["Memory"] = value; } }
        bool Running { get { return (bool)this["Running"]; } set { this["Running"] = value; } }
        ThoughtCollection Thoughts { get { return (ThoughtCollection)this["Thoughts"]; } set { this["Thoughts"] = value; } }
        public AIState State { get; set; }
        public AIComponent()
        {
            this.Behaviors = new List<Behavior>();
            this.Running = true;
            this.Thoughts = new ThoughtCollection();
            this.Knowledge = new Knowledge();
            this.State = new AIState() { Knowledge = this.Knowledge };//, Personality);
            this.Root = null;
        }
       // public AIComponent Initialize(Personality personality, params Behavior[] behaviors)
       // {
            
       //  //   Focus = null;
       //     this.Personality = personality;
       ////     this.Timer = 0;
       //     this.State.Personality = this.Personality;
            
       //     Behaviors = new List<Behavior>(behaviors);
       //     return this;
       // }
        //GameObject Parent;
        //public override void MakeChildOf(GameObject parent)
        //{
        //    this.Parent = parent;
        //    //if (this.Root != null)
        //    //    this.Root.Initialize(parent);
        //}
        public AIComponent Initialize(Personality personality, Behavior root)
        {
            this.Personality = personality;
            this.State.Personality = this.Personality;
            this.Root = root;
            //root.Initialize(this.Parent);
            this.Root.Initialize(this.State);
       
            return this;
        }
        public override void Update(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            if (net is Client) // do i want to run some deterministic behaviors locally too? UPDATE: NO
                return;
            if (!Running)
                return;

            Root.Execute(parent, this.State);

            if (!this.State.InSync)
                if (net is Server)
                    (net as Server).SyncAI(parent);
            
            //Current = null;
            //foreach (Behavior behav in Behaviors.ToList())
            //{
            //    var result = behav.Execute(net, parent, this.State);
            //    if (result == BehaviorState.Running)
            //    {
            //        Current = behav;
            //        break;
            //    }
            //}
        }

        //static public List<GameObject> Agents = new List<GameObject>();

        public override void Spawn(IObjectProvider net, GameObject parent)
        {
            //Agents.Add(parent);
            //foreach (var bhav in this.Behaviors)
            //    bhav.OnSpawn(parent, this.State);
            this.Root.OnSpawn(parent, this.State);
        }
        public override void Despawn(GameObject parent)
        {
            //Agents.Remove(parent);
            base.Despawn(parent);
        }

        /// <summary>
        /// TODO: only add entities in memory if there's LOS ?
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        List<GameObject> UpdateNearbyObjects(GameObject parent)
        {
         //   Knowledge.Objects.Clear(); //TODO: temporary workaround


           // List<GameObject> newObjects = new List<GameObject>();

            return parent.GetNearbyObjects( 
            range: range => range < 16,
            action: obj =>
                {
                    //List<Interaction> interactions = new List<Interaction>();
                    ////obj.HandleMessage(Message.Types.Query, parent, interactions);
                    //obj.Query(parent, interactions);

                    //// if exists, update existing memory instead of creating new one
                    //Knowledge.Objects[obj] = new Memory(obj, 100, 100, 1, interactions.Select(i => i.Need).ToArray());
                    //-----
                    Memory mem;
                    if (!Knowledge.Objects.TryGetValue(obj, out mem))
                    {
                        List<InteractionOld> interactions = new List<InteractionOld>();
                        obj.Query(parent, interactions);
                        Knowledge.Objects[obj] = obj.ToMemory(parent);// new Memory(obj, 100, 100, 1, interactions.Select(i => i.Need).ToArray());
                    }
                    else
                        mem.Refresh(parent);
                });
            //Map map = parent.Map;
            //Chunk chunk = Position.GetChunk(map, parent.Global);
            //List<GameObject> objects = new List<GameObject>();
            //foreach (Chunk ch in Position.GetChunks(map, chunk.MapCoords))
            //    //objects.AddRange(ch.GetObjects());//foo=>foo.Type == ObjectType.Human));
            //    foreach (GameObject obj in ch.GetObjects())
            //    {
            //        if (obj == parent)
            //            continue;

            //        if((obj.Global - parent.Global).Length() > 16)
            //            continue;
            //        List<Interaction> interactions = new List<Interaction>();
            //        obj.HandleMessage(Message.Types.Query, parent, interactions);

            //        Knowledge.Objects[obj] = new Memory(obj, 100, 100, 1, interactions.Select(i => i.Need).ToArray());

            //        newObjects.Add(obj);
            //    }
          //  return newObjects;
        }

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
                //case Message.Types.Aggro:

                //    return true;
                //case Message.Types.Attack:
                //    if (!(Current is AIAttack))
                //        // Current = new AIAttack(e.Sender);
                //        Stack.Push(new AIAttack(e.Sender));
                //    return false;
                //case Message.Types.Need:
                //    // if (!(Current is AIFollow))
                //    if (Current is AIIdle)
                //        Current = new AINeed();
                //    return true;
                //case Message.Types.NeedItem:
                //    //if (!(Current is AIFollow))
                //    //    // Current = new AIFindObject(32, foo => foo.ID == (GameObject.Types)e.Parameters[0], Message.Types.PickUp).Initialize(parent);
                //    //    Current.Child = new AIFindObject(32, (Predicate<GameObject>)e.Parameters[0], Message.Types.PickUp).Initialize(parent);

                //    Behavior behav = new AIFindObject(32, (Predicate<GameObject>)e.Parameters[0], Message.Types.PickUp);

                //    return true;



                //case Message.Types.NeedSatisfy:
                //    string name = (string)e.Parameters[0];
                //    //Personality.Needs[name] = Personality.Needs[name] + (int)e.Parameters[1];
                //    Personality.Needs[name].Value += (int)e.Parameters[1];
                //    return true;



                //case Message.Types.Activate:
                //    // Current = Current is AIFollow ? new AIIdle() : new AIFollow(e.Sender);
                //    if (Current is AIFollow)
                //    {
                //        //  Current = Current.Finalize(parent);
                //        Stack.Pop();
                //        //Current = new AIIdle();
                //    }
                //    else
                //        //  Current = new AIFollow(e.Sender).Initialize(parent);
                //        Stack.Push(new AIFollow(e.Sender).Initialize(parent));
                //    // Log.Enqueue(Log.EntryTypes.Default, e.Sender.Name + " talked to " + parent.Name);
                //    //e.Sender.HandleMessage(Current is AIFollow ? Message.Types.Followed : Message.Types.Unfollowed, parent);
                //    return true;

                //case Message.Types.Query:
                //    Query(parent, e);
                //    foreach (Behavior bhav in Behaviors)
                //        bhav.HandleMessage(parent, e);
                //    return true;

                //case Message.Types.InteractionFinished:
                //    Current = new AIIdle();
                //    return true;
                //case Message.Types.StartBehavior:
                //    Stack.Push(e.Parameters[0] as Behavior);
                //    return true;
                //default:
                //   // Current.HandleMessage(parent, e);
                //    if (Current != null)
                //        Current.HandleMessage(parent, e);
                //    //GetDeepest().HandleMessage(parent, e);
                //    return false;

                //case Message.Types.Need:
                //    Need need = e.Parameters[0] as Need;
                //    parent.PostMessage(Message.Types.Think, parent, "Need low: " + need.Name, "I need to satisfy my: " + need.Name + " soon");
                //    return true;

                case Message.Types.Think:
                    Thought thought = new Thought() { Time = DateTime.Now.ToTime(), Title = (string)e.Parameters[0], Text = (string)e.Parameters[1] };
                    Thoughts.Add(thought);
                    return true;

                case Message.Types.ManageEquipment:
                    throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.ManageEquipmentOk, parent);
                    return true;

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

                //case Message.Types.UpdateJobs:
                //    Knowledge.Invalidate();
                //    return true;

                case Message.Types.Need:
                    Knowledge.Invalidate();
                    Need n = e.Parameters[0] as Need;
                    throw new NotImplementedException();
                    //parent.PostMessage(Message.Types.Speak, parent, "I need " + n.Name);
                    //parent.PostMessage(Message.Types.Think, parent, "Need low: " + n.Name, "I need to satisfy my: " + n.Name + " soon");
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
                    //foreach (Behavior bhav in Behaviors)
                    //    result |= bhav.HandleMessage(parent, e);
                    this.Root.HandleMessage(parent, e);
                    return result;
            }
            //return false;
            return false;
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
        internal override void HandleRemoteCall(GameObject gameObject, Message.Types type, System.IO.BinaryReader r)
        {
            this.Root.HandleRPC(gameObject, type, r);

        }
        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interactions.Interaction> actions)
        {
            //foreach (var child in this.Behaviors)
            //    child.GetRightClickAction(parent, actions);
            this.Root.GetRightClickAction(parent, actions);
        }

        internal override void GetAvailableTasks(GameObject parent, List<Interactions.Interaction> list)
        {
            this.Root.GetInteractions(parent, list);
        }

        public override void Query(GameObject parent, List<InteractionOld> list)// GameObjectEventArgs e)
        {

            foreach (Behavior bhav in Behaviors)
                bhav.Query(parent, list);

        }

        public override void GetDialogueOptions(GameObject parent, GameObject speaker, DialogueOptionCollection options)
        {
            this.Behaviors.ForEach(foo => foo.GetDialogueOptions(parent, speaker, options));

            options.AddRange(new DialogueOption[]{
                "Thoughts".ToDialogueOption(HandleConversation)
            });
        }
        public override void GetDialogOptions(GameObject parent, GameObject speaker, List<DialogOption> options)
        {
            this.Root.GetDialogOptions(parent, speaker, options);
        }
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
            AIComponent ai = new AIComponent().Initialize(this.Personality.Clone() as Personality, this.Root.Clone() as Behavior);
            return ai;
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
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

        public override void Write(System.IO.BinaryWriter w)
        {
            this.State.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.State.Read(r);
        }
    }
}
