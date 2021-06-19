using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Objects;

namespace Start_a_Town_.Components
{
    class Work
    {
        public Progress Progress;
        public int InteractionID;

        public Work(Progress progress, int interactionID)
        {
            Progress = progress;
            InteractionID = interactionID;
        }

        public void Perform(GameObject tool)
        {
            if (tool != null)
            {
                EquipComponent eq;
                if (tool.TryGetComponent<EquipComponent>("Equip", out eq))
                {
                    int interID = eq.GetProperty<int>("Use");
                    if (eq.GetProperty<int>("Use") == InteractionID)
                        Progress.Value += 0.1f;
                }
            }
            //Progress.Value += 0.1f;
        }
    }

    class TasksComponent : Component
    {
        public Queue<Task> TaskQueue = new Queue<Task>();
        public Stack<Task> TaskStack = new Stack<Task>();
        //public override Component.Types Type
        //{
        //    get { return Types.Tasks; }
        //}
        public Task CurrentTask
        {
            get
            {
                if (TaskStack.Count == 0)
                {
                    if (TaskQueue.Count > 0)
                        TaskStack.Push(TaskQueue.Dequeue());
                    else
                        return null;
                    //    TaskStack.Push(new Task(this, null, 16));
                }
                return TaskStack.Peek();
            }
        }

        public override void Update(GameObject entity)
        {
            if(CurrentTask==null)
                TaskStack.Push(new Task(entity, null, 0));//16));
            if (CurrentTask.Perform(entity))
                TaskStack.Pop();
            //base.Update(entity);
        }

        //public void Target(Selection selection)
        //{
        //    CurrentTask.Object = selection;
        //}
        public bool Target(GameObject selection)
        {
            if (CurrentTask.Object == null)
            {
                CurrentTask.Object = selection;
                return true;
            }
            return false;
        }

        public virtual void TaskAssign(Task task)
        {
            TaskQueue.Enqueue(task);
        }

        public bool Interrupt()
        {
            if (CurrentTask != null)
            {
                if (CurrentTask.Progress != null)
                {
                    UI.NotificationArea.Write("Interrupted!");
                    CurrentTask.End();
                    TaskStack.Pop();
                    return true;
                }
            }
            return false;
        }

        //public bool Interrupt()
        //{
        //    ClearState(WorldEntity.Flags.Looting);
        //    OnInterrupted();
        //    if (CurrentTask != null)
        //    {
        //        CurrentTask.Interrupt();
        //        return true;
        //    }

        //    return false;

        //}

        //public event EventHandler<EventArgs> Interrupted;
        //protected void OnInterrupted()
        //{
        //    if (Interrupted != null)
        //        Interrupted(this, EventArgs.Empty);
        //}

        public override string ToString()
        {
            return
                //"Target: " + (Target != null ? Target.ToString() : "") + 
                //"Item Buffer: " + (ItemBuffer != null ? ItemBuffer.Name : "") +
                "Task queue: " + TaskQueue.Count.ToString() +
                "\nTask Stack: " + TaskStack.Count.ToString() + //Player.Actor.CurrentTask.InteractionStack.Count + //
                "\nTask Interaction Stack: " + CurrentTask.InteractionStack.Count + //
                "\nTask Object: " + (CurrentTask.Object != null ? CurrentTask.Object.ToString() : "null") +
                "\nCurrent Task: " + CurrentTask.ToString();// +
                //"\nCurrent Task: " + CurrentTask.CurrentInteraction.ToString();// +
               // "\nCurrent Effect: " + CurrentTask.CurrentInstruction.Name;
                //"\n" + Cell.ToString();
            //+                "\nCellBelow: " + CellBelow.ToString();

        }

        public override object Clone()
        {
            return new TasksComponent();
        }

        
        internal void Work()
        {
            if (CurrentTask != null)
                if (CurrentTask.Progress != null)
                {
                    //CurrentTask.Progress.Value += 0.1f;
                    InventoryComponent inv = Player.Actor.GetComponent<InventoryComponent>("Inventory");
                    GameObject tool = inv.GetProperty<Dictionary<string, GameObjectSlot>>("Equipment")["Mainhand"].Object;

                    if (tool != null)
                    {
                        EquipComponent eq;
                        if (tool.TryGetComponent<EquipComponent>("Equip", out eq))
                        {
                            InteractionComponent inter;
                            if ((CurrentTask.Object as GameObject).TryGetComponent<InteractionComponent>("Interactions", out inter))
                            {
                                int interID = eq.GetProperty<int>("Use");
                                if (interID == inter.GetInteractions()[0].ID)
                                    CurrentTask.Progress.Value += 0.1f;
                            }
                        }
                    }
                }
        }
    }
}
