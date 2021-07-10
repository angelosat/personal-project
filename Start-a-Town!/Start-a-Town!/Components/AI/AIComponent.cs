using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.UI;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
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
        Behavior Root;
        Knowledge Knowledge;
        bool Running;
        public AIState State;
        public bool Enabled = true;
        public AIComponent()
        {
            this.Running = true;
            this.Knowledge = new Knowledge();
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
            this.State = new AIState(parent as Actor) { Knowledge = this.Knowledge };
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
        }

        public override void OnSpawn(IObjectProvider net, GameObject parent)
        {
            this.State.Leash = parent.Global;
        }
        public override void OnDespawn(GameObject parent)
        {
            base.OnDespawn(parent);
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

                case Message.Types.ManageEquipment:
                    throw new NotImplementedException();

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
                    GameObject obj = e.Sender;
                    Memory mem;
                    if (this.Knowledge.Objects.TryGetValue(obj, out mem))
                    {

                        mem.State = Memory.States.Invalid;
                        throw new NotImplementedException();
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
                    return result;
            }
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
        
        public override object Clone()
        {
            AIComponent ai = new AIComponent().Initialize(
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
            return
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
                return "OBSOLETE";
                //return "Behavior: " + (this.State.Job != null ? this.State.Job.ToString() : "none");
            }
            public Interface(AIState state)
            {
                this.State = state;
                this.Label = new Label("Behavior: ") { Width = 300, TextFunc = this.GetText };
                this.AddControls(this.Label);
            }
            public Interface(int entity)
            {
                this.Label = new Label("Behavior: ") { Width = 300 };
                this.AddControls(this.Label);
                this.EntityID = entity;
            }
            internal override void OnGameEvent(GameEvent e)
            {
                switch (e.Type)
                {
                    case Message.Types.JobAccepted:
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
        }
        public override void DrawAfter(MySpriteBatch sb, Camera cam, GameObject parent)
        {
            var serverentity = parent;
            var state = AIState.GetState(serverentity);
            if (state is null)
                return; // we are in client
            var path = state.Path;
            if (!UISelectedInfo.IsSelected(parent))
                return;
            var first = true;
            if (path is not null)
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
