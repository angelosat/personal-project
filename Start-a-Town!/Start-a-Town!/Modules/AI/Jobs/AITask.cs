using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.UI;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    public enum TargetIndex { A, B, C, Tool = 15 }
    public class AITask
    {
        static Dictionary<string, Func<AITask>> Factory = new Dictionary<string, Func<AITask>>();

        public TargetArgs GetTarget(TargetIndex targetInd)
        {
            switch (targetInd)
            {
                case TargetIndex.Tool:
                    return this.Tool;
                case TargetIndex.A:
                    return this.TargetA;
                case TargetIndex.B:
                    return this.TargetB;
                case TargetIndex.C:
                    return this.TargetC;
                default:
                    throw new Exception();
            }
        }

      
        internal TargetArgs GetTarget(int targetInd)
        {
            return this.GetTarget((TargetIndex)targetInd);
        }

        internal bool ReserveAll(GameObject actor, TargetIndex sourceIndex)
        {
         
            var targets = this.GetTargetQueue(sourceIndex);
            var amounts = this.GetAmountQueue(sourceIndex);
            var count = targets.Count;
            if (count != amounts.Count)
                throw new Exception();
            for (int i = 0; i < count; i++)
            {
                var target = targets[i];
                var amount = amounts[i];
                if (!actor.Town.ReservationManager.Reserve(actor, target, amount))
                    return false;
            }
            return true;
        }
        internal bool Reserve(GameObject actor, TargetIndex index)
        {
            return actor.Town.ReservationManager.Reserve(actor, this.GetTarget(index), this.GetAmount(index));
        }

        
        internal int GetAmount(TargetIndex amountInd)
        {
            switch (amountInd)
            {
                case TargetIndex.A:
                    return this.AmountA;
                case TargetIndex.B:
                    return this.AmountB;
                case TargetIndex.C:
                    return this.AmountC;
                default:
                    throw new Exception();
            }
        }
        
        internal List<TargetArgs> GetTargetQueue(TargetIndex targetInd)
        {
            switch (targetInd)
            {
                case TargetIndex.A:
                    return this.TargetsA;
                case TargetIndex.B:
                    return this.TargetsB;
                case TargetIndex.C:
                    return this.TargetsC;
                default:
                    throw new Exception();
            }
        }
        
        internal List<int> GetAmountQueue(TargetIndex amountInd)
        {
            switch (amountInd)
            {
                case TargetIndex.A:
                    return this.AmountsA;
                case TargetIndex.B:
                    return this.AmountsB;
                case TargetIndex.C:
                    return this.AmountsC;
                default:
                    throw new Exception();
            }
        }

        internal AITask SetTarget(TargetIndex targetInd, GameObject target, int amount)
        {
            this.SetAmount(targetInd, amount);
            return this.SetTarget(targetInd, new TargetArgs(target));
        }

        internal AITask SetTarget(TargetIndex targetInd, Vector3 target)
        {
            return this.SetTarget(targetInd, new TargetArgs(target));
        }
        internal AITask SetTarget(TargetIndex targetInd, TargetArgs targetArgs)
        {
            switch (targetInd)
            {
                case TargetIndex.A:
                    this.TargetA = targetArgs;
                    break;
                case TargetIndex.B:
                    this.TargetB = targetArgs;
                    break;
                case TargetIndex.C:
                    this.TargetC = targetArgs;
                    break;
                default:
                    throw new Exception();
            }
            return this;
        }

        internal void AddPlacedObject(GameObject hauledObj)
        {
            this.PlacedObjects.Add(new ObjectAmount(hauledObj));
        }
        internal void AddCraftedItem(Entity item)
        {
            this.CraftedItems.Add(item);
        }

        internal void SetAmount(TargetIndex ind, int amount)
        {
            switch (ind)
            {
                case TargetIndex.A:
                    this.AmountA = amount;
                    break;
                case TargetIndex.B:
                    this.AmountB = amount;
                    break;
                case TargetIndex.C:
                    this.AmountC = amount;
                    break;
                default:
                    throw new Exception();
            }
        }
        internal void SetTool(TargetArgs t)
        {
            this.Tool = t;
        }
        internal bool NextTarget(TargetIndex ind)
        {
            var targets = this.GetTargetQueue(ind);
            if (!targets.Any())
                return false;
            this.SetTarget(ind, targets[0]);
            targets.RemoveAt(0);
            return true;
        }
        
        internal bool NextAmount(TargetIndex ind)
        {
            var targets = this.GetAmountQueue(ind);
            if (!targets.Any())
                return false;
            this.SetAmount(ind, targets[0]);
            targets.RemoveAt(0);
            return true;
        }

        static public void AddTask<T>() where T : AITask, new()
        {
            Factory[typeof(T).FullName] = () => new T();
        }
        
        static public AITask Load(SaveTag tag)
        {
            var typeName = (string)tag["Type"].Value;
            var task = Activator.CreateInstance(Type.GetType(typeName)) as AITask;
            task = new AITask();
            task.LoadData(tag);
            return task;
        }
        static public AITask Load(BinaryReader r)
        {
            var typeName = r.ReadString();
            var task = Activator.CreateInstance(Type.GetType(typeName)) as AITask;
            task.Read(r);
            return task;
        }
        internal static void Initialize()
        {
        }
        public AITask()
        {
            this.ID = ReservationManager.GetNextTaskID();
        }
        static Dictionary<GameObject, AIItemReservation> ReservedItems = new Dictionary<GameObject, AIItemReservation>();
        static public void Reserve(GameObject parent, GameObject item, int quantity)
        {
            var existing = ReservedItems.GetValueOrDefault(item);
            if (existing == null)
            {
                existing = new AIItemReservation(item);
                ReservedItems.Add(item, existing);
            }
            existing.Add(parent, quantity);
        }
        static public int GetUnreservedAmount(GameObject item)
        {
            var existing = ReservedItems.GetValueOrDefault(item);
            if (existing == null)
                return item.StackSize;
            var sum = existing.OwnersQuantities.Values.Sum();
            return item.StackSize - sum;
        }
        static public void UnreserveAll(GameObject actor)
        {
            foreach(var item in ReservedItems.Keys.ToList())
            {
                var existing = ReservedItems[item];
                existing.OwnersQuantities.Remove(actor);
                if (existing.OwnersQuantities.Count == 0)
                    ReservedItems.Remove(item);
            }
        }

        

        public virtual string Name
        {
            get { return "unnamed task"; }
        }
        public TargetArgs Tool = TargetArgs.Null;
        public TargetArgs TargetA = TargetArgs.Null;
        public TargetArgs TargetB = TargetArgs.Null;
        public TargetArgs TargetC = TargetArgs.Null;
        public List<TargetArgs> TargetsA = new();
        public List<TargetArgs> TargetsB = new();
        public List<TargetArgs> TargetsC = new();
        public int Count;
        public List<List<TargetArgs>> TargetQueues = new();
        public List<List<int>> AmountQueues = new();
        public int AmountA = -1, AmountB = -1, AmountC = -1;
        public List<int> AmountsA = new();
        public List<int> AmountsB = new();
        public List<int> AmountsC = new();
        public List<ObjectAmount> PlacedObjects = new();
        public List<Entity> CraftedItems = new();

        //public Dictionary<ReactionIngredientIndex, ItemDef> CraftingIngredients = new Dictionary<ReactionIngredientIndex, ItemDef>();
        public List<Func<bool>> FailConditions = new();
        public CraftOrderNew Order;
        //public Dictionary<string, ObjectAmount> ReactionReagents = new Dictionary<string, ObjectAmount>();
        //public Dictionary<string, Entity> IngredientsUsed = new();
        public Dictionary<string, ObjectRefIDsAmount> IngredientsUsed = new();

        public TargetArgs Product = TargetArgs.Null;
        public bool Forced;
        public bool Urgent = true; // TODO default should be false
        int ReservedBy = -1;
        //int Retries = 0;
        public int TicksWaited = 0;
        public int Quest;
        public int ShopID; // TODO store shopid instead of shop object
        public Transaction Transaction;
        public CustomerProperties CustomerProps;
        public int CustomerID;

        bool Cancelled = false;


        internal void Cancel()
        {
            this.Cancelled = true;
        }
        public bool IsCancelled { get
        { 
            return this.Cancelled;
        } }


        public void Reserve(GameObject actor)
        {
            this.ReservedBy = actor.RefID;
        }
        public void Unreserve()
        {
            this.ReservedBy = -1;
        }
        public bool IsReservedBy(GameObject actor)
        {
            return actor.Net.GetNetworkObject(this.ReservedBy) == actor;
        }
        public bool IsReserved
        {
            get
            {
                return this.ReservedBy > -1;
            }
        }

        public TaskDef Def;
        public int ID { get; internal set; }

        //public AITask(Type behaviorType, params TargetArgs[] targets) : this()
        //{
        //    this.BehaviorType = behaviorType;
        //    if (targets.Any())
        //    {
        //        //this.Targets.Add(targets[0]);
        //        this.TargetA = targets[0];
        //        for (int i = 0; i < targets.Length; i++)
        //        {
        //            this.TargetsA.Add(targets[i]);
        //        }
        //    }
        //}
        public AITask(TaskDef taskDef)
        {
            this.Def = taskDef;
        }

        public AITask(Type behaviorType) : this()
        {
            this.BehaviorType = behaviorType;
            //if (targets.Any())
            //{
            //    //this.Targets.Add(targets[0]);
            //    this.TargetA = targets[0];
            //    for (int i = 0; i < targets.Length; i++)
            //    {
            //        this.TargetsA.Add(targets[i]);
            //    }
            //}
        }
        public AITask(Type behaviorType, TargetArgs targetA) : this()
        {
            this.BehaviorType = behaviorType;
            this.SetTarget(TargetIndex.A, targetA);
        }
        public AITask(TaskDef def, TargetArgs targetA) : this()
        {
            this.Def = def;
            this.SetTarget(TargetIndex.A, targetA);
        }
        public AITask(TaskDef def, TargetArgs targetA, TargetArgs targetB) : this()
        {
            this.Def = def;
            this.SetTarget(TargetIndex.A, targetA);
            this.SetTarget(TargetIndex.B, targetB);
        }
        public AITask(Type behaviorType, TargetArgs targetA, TargetArgs targetB) : this()
        {
            this.BehaviorType = behaviorType;
            this.SetTarget(TargetIndex.A, targetA);
            this.SetTarget(TargetIndex.B, targetB);
        }
        public AITask(Type behaviorType, TargetArgs targetA, TargetArgs targetB, TargetArgs targetC) : this()
        {
            this.BehaviorType = behaviorType;
            this.SetTarget(TargetIndex.A, targetA);
            this.SetTarget(TargetIndex.B, targetB);
            this.SetTarget(TargetIndex.C, targetC);
        }
        //public void AddTarget(int queueIndex, TargetArgs target)
        //{

        //}
        //public AITask(Type behaviorType, List<Queue<TargetArgs>> targets, List<Queue<int>> counts)
        //{
        //    this.BehaviorType = behaviorType;
        //    this.TargetQueues = targets;
        //    this.CountQueues = counts;
        //}
        //public abstract AIJob Create(GameObject actor);
        public override string ToString()
        {
            return this.Def?.Name ?? base.ToString();
        }
        public Control GetControl()
        {
            //return new Button("Force " + this.Def?.Name ?? this.BehaviorType.Name);
            return new Button(this.Def?.GetForceText(this) ?? this.BehaviorType.Name);
        }
        public string GetForceTaskText()
        {
            return this.Def?.GetForceText(this) ?? this.BehaviorType.Name;
        }
        //public virtual AIJob Create(GameObject actor)
        //{
        //    return new AIJob() { Task = this };
        //}
        Type _BehaviorType;
        public Type BehaviorType
        {
            get { return this.Def?.BehaviorClass ?? this._BehaviorType; }
            set { this._BehaviorType = value; }
        }


        public virtual Behavior GetBehavior(GameObject actor) { return null; }
        //public Behavior Behavior;
        public BehaviorPerformTask CreateBehavior(Actor actor)
        {
            var behav = Activator.CreateInstance(this.BehaviorType) as BehaviorPerformTask;
            behav.Actor = actor;
            behav.Task = this;
            //if (this.Behavior != null)
            //    throw new Exception();
            //this.Behavior = behav;
            return behav;
        }
        public virtual bool IsAvailable()
        {
            return true;
        }
        //public virtual bool IsValid()
        //{
        //    return true;
        //}
        public virtual void CleanUp(GameObject actor) { UnreserveAll(actor); }
        public virtual void Succeeded(GameObject actor) { }
        //public virtual SaveTag Save(string name = "") { return null; }
        public SaveTag Save(string name = "") 
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.GetType().FullName.Save("Type"));
            tag.Add(this.ID.Save("ID"));
            tag.Add(this.Tool.Save("Tool"));
            tag.Add(this.TargetA.Save("TargetA"));
            tag.Add(this.TargetB.Save("TargetB"));
            tag.Add(this.TargetB.Save("TargetC"));

            tag.Add(this.AmountA.Save("AmountA"));
            tag.Add(this.AmountB.Save("AmountB"));
            tag.Add(this.AmountB.Save("AmountC"));

            tag.Add(this.TargetsA.Save("TargetsA"));
            tag.Add(this.TargetsB.Save("TargetsB"));
            tag.Add(this.TargetsB.Save("TargetsC"));

            tag.Add(this.AmountsA.Save("AmountsA"));
            tag.Add(this.AmountsB.Save("AmountsB"));
            tag.Add(this.AmountsB.Save("AmountsC"));

            tag.Add(this.PlacedObjects.SaveOld("PlacedItems"));
            //tag.Add(this.IngredientsUsed.Save("IngredientsUsed"));
            tag.Add(this.Count.Save("Count"));
            tag.TrySaveRef(this.Order, "Order");
            tag.Add(this.Product.Save("Product"));
            tag.Add(this.Forced.Save("Forced"));
            var targetqueues = new SaveTag(SaveTag.Types.List, "Queues", SaveTag.Types.List);
            for (int i = 0; i < this.TargetQueues.Count; i++)
			{
			    var tarqueue = this.TargetQueues[i];
                var quantityqueue = this.AmountQueues[i];
                var queuetag = new SaveTag(SaveTag.Types.List, "", SaveTag.Types.Compound);
                for (int j = 0; j < tarqueue.Count; j++)
                {
                    var itemtag = new SaveTag(SaveTag.Types.Compound);
                    var tar = tarqueue[j];
                    var amount = quantityqueue[j];
                    itemtag.Add(tar.Save("Target"));
                    itemtag.Add(amount.Save("Amount"));
                    queuetag.Add(itemtag);
                }
                targetqueues.Add(queuetag);
			}
            tag.Add(targetqueues);

            this.ShopID.Save(tag, "ShopID");
            this.Quest.Save(tag, "QuestToAccept");
            this.Transaction.Save(tag, "Transaction");

            //tag.Add(this.Behavior.Save("Behavior"));
            this.AddSaveData(tag);
            return tag;
        }
        protected virtual void AddSaveData(SaveTag tag)
        { 

        }
        public virtual void LoadData(SaveTag tag)
        {
            tag.TryGetTagValue<int>("ID", t => this.ID = t);
            //this.TargetA = new TargetArgs(null, tag["Target"]);
            tag.TryGetTag("TargetA", t => this.TargetA = new TargetArgs(null, t));
            tag.TryGetTag("TargetB", t => this.TargetB = new TargetArgs(null, t));
            tag.TryGetTag("TargetC", t => this.TargetC = new TargetArgs(null, t));

            tag.TryGetTag("Tool", t => this.Tool = new TargetArgs(null, t));

            tag.TryGetTagValue<int>("AmountA", out this.AmountA);
            tag.TryGetTagValue<int>("AmountB", out this.AmountB);
            tag.TryGetTagValue<int>("AmountC", out this.AmountC);

            tag.TryGetTag("TargetsA", t => this.TargetsA.Load(t));
            tag.TryGetTag("TargetsB", t => this.TargetsB.Load(t));
            tag.TryGetTag("TargetsC", t => this.TargetsC.Load(t));

            tag.TryGetTag("AmountsA", t => this.AmountsA.Load(t));
            tag.TryGetTag("AmountsB", t => this.AmountsB.Load(t));
            tag.TryGetTag("AmountsC", t => this.AmountsC.Load(t));

            tag.TryGetTag("PlacedItems", t => this.PlacedObjects.Load(t));
            //tag.TryGetTag("IngredientsUsed", t => this.IngredientsUsed.Load(t));

            tag.TryLoadRef("Order", out this.Order);
            tag.TryGetTagValue("Count", out this.Count);
            tag.TryGetTag("Product", t => this.Product = new TargetArgs(null, t));
            tag.TryGetTagValue("Forced", out this.Forced);
            //var queuestag = tag["Queues"].Value as List<SaveTag>;
            List<SaveTag> queuestag;
            if (tag.TryGetTagValue<List<SaveTag>>("Queues", out queuestag))
            {
                foreach (var qtag in queuestag)
                {
                    var curqtag = qtag.Value as List<SaveTag>;
                    var tlist = new List<TargetArgs>();
                    var clist = new List<int>();
                    foreach (var ctag in curqtag)
                    {
                        var tar = new TargetArgs(null, ctag["Target"]);

                        var amount = (int)ctag["Amount"].Value;
                        tlist.Add(tar);
                        clist.Add(amount);
                    }
                    this.TargetQueues.Add(tlist);
                    this.AmountQueues.Add(clist);
                }
            }
            //this.Behavior.Load(tag["Behavior"]);
            tag.TryGetTagValue("ShopID", out this.ShopID);
            tag.TryGetTagValue("QuestToAccept", out this.Quest);
            tag.TryGetTag("Transaction", v => this.Transaction = new Transaction(v));
        }

        internal virtual void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.GetType().FullName);
            w.Write(this.ID);
            this.Tool.Write(w);
            this.TargetA.Write(w);
            w.Write(this.AmountA);
            w.Write(this.TargetsA);
            w.Write(this.AmountsA);
            w.Write(this.TargetQueues.Count);
            foreach(var i in TargetQueues)
                w.Write(i);
            w.Write(this.AmountQueues.Count);
            foreach (var i in AmountQueues)
                w.Write(i);
            //this.Behavior.Write(w);
            w.Write(this.Count);
            w.Write(this.Quest);
            w.Write(this.ShopID);
            this.Transaction.Write(w);
        }
        protected virtual void Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Tool = TargetArgs.Read((IObjectProvider)null, r);
            this.TargetA = TargetArgs.Read((IObjectProvider)null, r);
            this.AmountA = r.ReadInt32();
            this.TargetsA = r.ReadListTargets();
            this.AmountsA = r.ReadListInt();
            var targetqueuecount = r.ReadInt32();
            for (int i = 0; i < targetqueuecount; i++)
            {
                this.TargetQueues.Add(r.ReadListTargets());
            }
            var amountqueuecount = r.ReadInt32();
            for (int i = 0; i < amountqueuecount; i++)
            {
                this.AmountQueues.Add(r.ReadListInt());
            }
            //this.Behavior.Read(r);
            //throw new NotImplementedException();
            this.Count = r.ReadInt32();
            this.Quest = r.ReadInt32();
            this.ShopID = r.ReadInt32();
            this.Transaction = new Transaction(r);
        }
        
        internal virtual void WriteBlackboard(BinaryWriter w, Dictionary<string, object> blackboard) 
        {
            //this.Behavior.WriteBlackboard(w, blackboard);
        }
        internal virtual void ReadBlackboard(BinaryReader r, Dictionary<string, object> blackboard)
        {
            //this.Behavior.ReadBlackboard(r, blackboard);
        }
        internal virtual SaveTag SaveBlackboard(string name, Dictionary<string, object> blackboard)
        {
            return null;
            //return this.Behavior.SaveBlackboard(name, blackboard);
        }
        internal virtual void LoadBlackboard(SaveTag tag, Dictionary<string, object> blackboard) 
        {
            //this.Behavior.LoadBlackboard(tag, blackboard);
        }

        public void ObjectLoaded(GameObject parent)
        {
            //this.Behavior.ObjectLoaded(parent);
        }

        internal void MapLoaded(GameObject parent)
        {
            this.Tool.Map = parent.Map;
            this.TargetA.Map = parent.Map;
            this.TargetB.Map = parent.Map;
            this.TargetC.Map = parent.Map;

            foreach (var tar in this.TargetsA)
                tar.Map = parent.Map;
            foreach (var tar in this.TargetsB)
                tar.Map = parent.Map;
            foreach (var tar in this.TargetsC)
                tar.Map = parent.Map;

            foreach (var q in this.TargetQueues)
                foreach (var t in q)
                    t.Map = parent.Map;
            foreach (var t in this.GetCustomTargets())
                t.Map = parent.Map;

            //this.Tool.Network = parent.Net;
            //this.Target.Network = parent.Net;
            //foreach (var tar in this.Targets)
            //    tar.Network = parent.Net;
            //foreach (var q in this.TargetQueues)
            //    foreach (var t in q)
            //        t.Network = parent.Net;
            //foreach (var t in this.GetCustomTargets())
            //    t.Network = parent.Net;
        }
        internal void AddTargets(TargetIndex index, params TargetArgs[] targets)
        {
            //if (targets.Distinct().Count() != targets.Length)
            //    throw new Exception();
            var list = this.GetTargetQueue(index);
            foreach (var t in targets)
                list.Add(t);
        }
        
        internal void AddAmountNew(TargetIndex index, params int[] amounts)
        {
            var list = this.GetAmountQueue(index);
            foreach (var t in amounts)
                list.Add(t);
        }
        
        internal void AddAmount(int index, int amount)
        { 
        }
        
        internal void AddTarget(TargetIndex index, GameObject target, int count = -1)
        {
            this.AddTarget(index, new TargetArgs(target), count);
        }
        internal void AddTarget(TargetIndex index, Entity target, int count = -1)
        {
            this.AddTarget(index, new TargetArgs(target), count);
        }
        internal void AddTarget(TargetIndex index, TargetArgs target, int count = -1)
        {
            List<TargetArgs> t;
            List<int> a;
            switch (index)
            {
                case TargetIndex.A:
                    t = this.TargetsA;
                    a = this.AmountsA;
                    break;

                case TargetIndex.B:
                    t = this.TargetsB;
                    a = this.AmountsB;
                    break;

                case TargetIndex.C:
                    t = this.TargetsC;
                    a = this.AmountsC;
                    break;

                default:
                    throw new Exception();
            }
            t.Add(target);
            a.Add(count);
        }

        internal void AddTargetOld(int index, TargetArgs target, int count = -1)
        {
            var targets = this.TargetQueues.ElementAtOrDefault(index);
            if (targets == null)
                this.TargetQueues.Insert(index, new List<TargetArgs>() { target });
            else
                targets.Add(target);
            var counts = this.AmountQueues.ElementAtOrDefault(index);
            if (counts == null)
                this.AmountQueues.Insert(index, new List<int>() { count });
            else
                counts.Add(count);
        }
        internal void AddTarget(TargetArgs target, int count = -1)
        {
            this.TargetsA.Add(target);
            this.AmountsA.Add(count);
        }
        internal void AddTarget(GameObject target, int count = -1)
        {
            this.AddTarget(new TargetArgs(target), count);
        }
        public bool HasFailed()
        {
            foreach (var fail in this.FailConditions)
                if (fail())
                    return true;
            return false;
        }

        internal void AddFailCondition(Func<bool> condition)
        {
            this.FailConditions.Add(condition);
        }

        //internal virtual bool ReserveTargets(GameObject actor)
        //{
        //    throw new Exception();
        //    ////return true;
        //    ////foreach (var t in this.Task.Targets)
        //    ////    if (!this.Actor.Reserve(t))
        //    ////        return false;
        //    //if (!actor.Reserve(this, this.Tool))
        //    //    return false;
        //    //if (!actor.Reserve(this, this.TargetA, this.AmountA))
        //    //    return false;

        //    //for (int i = 0; i < this.TargetsA.Count; i++)
        //    //{
        //    //    if (!actor.Reserve(this, this.TargetsA[i], this.AmountsA[i]))
        //    //        return false;
        //    //}

        //    //for (int i = 0; i < this.TargetQueues.Count; i++)
        //    //{
        //    //    var list = this.TargetQueues[i];
        //    //    for (int j = 0; j < list.Count; j++)
        //    //    {
        //    //        if (!actor.Reserve(this, list[j], this.AmountQueues[i][j]))
        //    //            return false;
        //    //    }
        //    //}

        //    //return true;
        //}
        //internal virtual bool ReserveTargetsNoTask(GameObject actor)
        //{
        //    //return true;
        //    //foreach (var t in this.Task.Targets)
        //    //    if (!this.Actor.Reserve(t))
        //    //        return false;
        //    if (!actor.Reserve(this.Tool))
        //        return false;
        //    if (!actor.Reserve(this.TargetA, this.AmountA))
        //        return false;

        //    for (int i = 0; i < this.TargetsA.Count; i++)
        //    {
        //        if (!actor.Reserve(this.TargetsA[i], this.AmountsA[i]))
        //            return false;
        //    }

        //    for (int i = 0; i < this.TargetQueues.Count; i++)
        //    {
        //        var list = this.TargetQueues[i];
        //        for (int j = 0; j < list.Count; j++)
        //        {
        //            if (!actor.Reserve(list[j], this.AmountQueues[i][j]))
        //                return false;
        //        }
        //    }

        //    return true;
        //}

        protected virtual IEnumerable<TargetArgs> GetCustomTargets() { yield break; }
    }
}
