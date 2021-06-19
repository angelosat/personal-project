using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors.Tasks
{
    class BehaviorFindTask : Behavior
    {
        static readonly int TimerMax = Engine.TicksPerSecond;
        int Timer = TimerMax;
        //BehaviorPerformTask CurrentBehavior
        //{
        //    get { return AIComponent.GetState(this.Actor).CurrentTaskBehavior; }
        //    set { AIComponent.GetState(this.Actor).CurrentTaskBehavior = value; }
        //}
        TaskGiver CurrentTaskGiver;
       

        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (parent.Velocity.Z != 0)
                return BehaviorState.Running;

            if(state.ForcedTask != null)
            {
                var task = state.ForcedTask;
                state.ForcedTask = null;
                this.CleanUp(parent);
                this.TryForceTask(parent, task, state);
            }
            //if(state.Task?.IsCancelled ?? false)
            //{
            //    this.CleanUp(parent);
            //}
            if (state.CurrentTaskBehavior != null)
            {
                //var result = state.CurrentTaskBehavior.Execute(parent, state);
                var (result, source) = state.CurrentTaskBehavior.Tick(parent, state);
                switch (result)
                {
                    case BehaviorState.Running:
                        return BehaviorState.Success;

                    case BehaviorState.Fail:
                    case BehaviorState.Success:
                        parent.MoveToggle(false);

                        /// LATEST FINDINGS:
                        /// the problem ended up not being that his call was canceling the interaction at the client, 
                        /// but that the interaction wasn't being serialized properly. its state wasnt synced to the client, and was left as unstarted
                        /// which resulted in the the intearction starting again and re-adding its animation to the entity
                        /// after fixing that, the cancelinteraction now seem to work even after a success
                      
                        parent.CancelInteraction(); // (the following is not true anymore, see above comment) THIS CANNOT BE HERE BECAUSE IT WILL CANCEL THE CLIENT ENTITY'S INTERACTION WHILE THE ANIMATION IS ON THE LAST FRAME

                        //if (result == BehaviorState.Fail) // ONLY CANCEL INTERACTION ON FAILURE?
                            //parent.CancelInteraction(); 
                            // DO I ACTUALLY NEED IT? i dont remember why i added this here
                            // i think i was only cancelling the interaction server-side and the problem appeared after sync-cancelling to the clients

                        // TODO: unreserve here?
                        parent.Unreserve();
                        state.LastBehavior = state.CurrentTaskBehavior;
                        state.CurrentTaskBehavior.CleanUp();
                        state.CurrentTaskBehavior = null;

                        // ADDED THIS HERE because when immediately getting a new task from the same taskgiver,
                        // the pathfinding behavior saw that the path wasn't null and didn't calculate a new path for the new behavior/targets
                        state.Path = null;

                        // I MOVED THIS FROM HERE SO THAT THE FALLBACK BEHAVIOR, IF ANY, STARTS IN THE NEXT FRAME
                        //this.CleanUp(parent, state);
                        return BehaviorState.Fail;

                    default:
                        break;
                }
            }
            else
            {
                var stamina = parent.GetResource(ResourceDef.Stamina);
                var staminaTaskThreshold = 20;
                var tired = stamina.Value <= staminaTaskThreshold;
                //if (tired)
                //    return BehaviorState.Fail;// Success;

                if (this.CurrentTaskGiver != null)
                {
                    if (tired)
                    {
                        this.CleanUp(parent, state);
                        return BehaviorState.Fail;
                    }
                    var next = this.CurrentTaskGiver.FindTask(parent);

                    if (next.Task != null)
                    {
                        var bhav = next.Task.CreateBehavior(parent);
                        if (bhav.InitBaseReservations())
                        {
                            //throw new Exception();
                            string.Format("found followup task from same taskgiver {0}", this.CurrentTaskGiver).ToConsole();
                            //this.CleanUp(parent, state);
                            state.CurrentTaskBehavior = bhav;
                            state.CurrentTask = next.Task;
                            return BehaviorState.Success;
                        }
                        else
                            this.CleanUp(parent, state);
                    }
                    else
                    {
                        this.CleanUp(parent, state);
                        return BehaviorState.Fail;

                    }
                }

                if (!tired)
                {
                    if (this.Timer < TimerMax)
                    {
                        this.Timer++;
                    }
                    else
                    {
                        this.Timer = 0;

                        //var task = this.FindNewTask(parent, state); // TODO: needs optimization
                        var task = this.FindNewTaskNew(parent, state); // TODO: needs optimization

                        if (task != null)
                        {
                            //state.CurrentTask = task;
                            //Start_a_Town_.Modules.AI.Net.PacketTaskUpdate.Send(parent.Net as Net.Server, parent.InstanceID, state.CurrentTaskBehavior.Name);
                            return BehaviorState.Success;
                        }
                    }
                }
            }
            //Start_a_Town_.Modules.AI.Net.PacketTaskUpdate.Send(parent.Net as Net.Server, parent.InstanceID, "Idle");

            return BehaviorState.Fail;// Success;
        }

        bool TryForceTask(Actor parent, AITask task, AIState state)
        {
            var bhav = task.CreateBehavior(parent);
            if (!bhav.InitBaseReservations())
                return false;

            //throw new Exception();
            string.Format("found followup task from same taskgiver {0}", this.CurrentTaskGiver).ToConsole();
            //this.CleanUp(parent, state);
            state.CurrentTaskBehavior = bhav;
            state.CurrentTask = task;
            return true;
        }
        
        


        private void CleanUp(GameObject parent)
        {
            this.CleanUp(parent, parent.GetState());
        }
        private void CleanUp(GameObject parent, AIState state)
        {
            //AIManager.AIStopMoveNew(parent);

            // TODO: fully stop moving before throwing carried item
            //var equippedSlot = GearComponent.GetSlot(parent, GearType.Mainhand);
            //if (equippedSlot.Object != null)
            //AIManager.Interact(parent, new Components.Interactions.DropEquippedTarget(new TargetArgs(equippedSlot.Object)), TargetArgs.Null);

            //AIManager.Interact(parent, new Components.Interactions.Throw(true), TargetArgs.Null);
            //AIManager.Interact(parent, new Components.Interactions.DropEquipped(GearType.Mainhand), TargetArgs.Null);
            var actor = parent as Actor;
            if(parent.Carried != null)
                parent.Interact(new InteractionThrow(true));
            //if(parent.GetEquipmentSlot(GearType.Mainhand).Object != null)
            //    parent.Interact(new InteractionDropEquipped(GearType.Mainhand));

            if (actor.GetEquipmentSlot(GearType.Mainhand) is Entity item)
            {
                if (actor.ItemPreferences.IsPreference(item))
                    parent.Interact(new Equip(), new TargetArgs(item)); // equip() currently toggles gear. if target is currently equipped, it unequips it
                else
                    parent.Interact(new InteractionDropEquipped(GearType.Mainhand));
            }

            // todo: drop hauled too
            //state.CurrentTask.CleanUp(parent);
            /*state.CurrentTaskBehavior.CleanUp();*/ // pay attention for any problems after commenting this out
            parent.Unreserve();
            state.CurrentTask = null;
            state.LastBehavior = null;
            state.Path = null;
            state.CurrentTaskBehavior = null;
            this.CurrentTaskGiver = null;
        }

        AITask FindNewTaskNew(Actor parent, AIState state)
        {
            //var givers = parent.GetComponent<NeedsComponent>().NeedsNew.Select(n => n.TaskGiver);
            //givers = givers.Concat(TaskGiver.EssentialTaskGivers);
            //givers = parent.IsCitizen ? givers.Concat(TaskGiver.CitizenTaskGivers) : givers.Concat(TaskGiver.VisitorTaskGivers);
            var givers = parent.GetTaskGivers();

            foreach (var giver in givers)
            {
                if (giver == null)
                    continue;
                var giverResult = giver.FindTask(parent);
                var task = giverResult.Task;
                if (task == null)
                    continue;
                var bhav = task.CreateBehavior(parent);
                if (!bhav.InitBaseReservations())
                {
                    parent.Unreserve();
                    continue;
                }
                //state.CurrentTasktask.CreateBehavior(parent); = task;
                //bhav.GenerateBehaviors();
                state.CurrentTaskBehavior = bhav;
                this.CurrentTaskGiver = giver;
                state.CurrentTask = task;
                //if (CurrentBehavior.Execute(parent, state) == BehaviorState.Running)
                    return task;
            }

            return null;
        }

        public override object Clone()
        {
            return new BehaviorFindTask();
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Timer);
            //var hasBehav = this.CurrentBehavior != null;
            //w.Write(hasBehav);
            //if (!hasBehav)
            //    return;
            //w.Write(this.CurrentBehavior.GetType().FullName);
            //this.CurrentBehavior.Write(w);

        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Timer = r.ReadInt32();
            //var hasBehav = r.ReadBoolean();
            //if (!hasBehav)
            //    return;
            //var bhavtype = r.ReadString();
            //this.CurrentBehavior = Activator.CreateInstance(Type.GetType(bhavtype)) as BehaviorPerformTask;
            //this.CurrentBehavior.Read(r);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.Timer.Save("Timer"));
            //if (this.CurrentBehavior != null)
            //{
            //    //var bhavtag = new SaveTag(SaveTag.Types.Compound, "Behavior");
            //    //tag.Add(this.CurrentBehavior.GetType().FullName.Save("TypeName"));
            //    //tag.Add(this.CurrentBehavior.Save("Behavior"));
            //    var bhavtag = this.CurrentBehavior.Save("Behavior");
            //    bhavtag.Add(this.CurrentBehavior.GetType().FullName.Save("TypeName"));
            //    tag.Add(bhavtag);
            //}
            if (this.CurrentTaskGiver != null)
            {
                tag.Add(this.CurrentTaskGiver.GetType().FullName.Save("CurrentTaskGiver")); ;
            }
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);

            tag.TryGetTagValue<int>("Timer", out this.Timer);
            //tag.TryGetTag("Behavior", t =>
            //{
            //    var bhavtype = (string)t["TypeName"].Value;
            //    this.CurrentBehavior = Activator.CreateInstance(Type.GetType(bhavtype)) as BehaviorPerformTask;
            //    this.CurrentBehavior.Load(t);
            //});
            tag.TryGetTagValue<string>("CurrentTaskGiver", t =>
            {
                this.CurrentTaskGiver = Activator.CreateInstance(Type.GetType(t)) as TaskGiver;
            });
        }
        internal override void MapLoaded(Actor parent)
        {
            this.Actor = parent;
            //if (this.CurrentBehavior == null)
            //    return;
            //this.CurrentBehavior.MapLoaded(parent);
        }

        internal void EndCurrentTask(Actor actor)
        {
            this.CleanUp(actor);
        }
        //internal override void ObjectLoaded(GameObject parent)
        //{
        //    var state = AIState.GetState(parent);
        //    if (state.CurrentTask != null)
        //        state.CurrentTask.Behavior.ObjectLoaded(parent);
        //}
        //internal void EndCurrentTask()
        //{

        //    //throw new NotImplementedException();
        //}
    }
}
