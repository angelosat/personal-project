using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Towns;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI
{
    public class ReservationManager : TownComponent
    {
        class Reservation
        {
            public int Actor;
            public TargetArgs Target;
            int _Amount;
            public int Amount
            {
                get { return this._Amount; }
                set
                {
                    //if (this.Target.HasObject && value > this.Target.Object.StackSize)
                    //    throw new Exception();
                    this._Amount = value;
                }

            }

            public int TaskID;
            public AITask Task { set { this.TaskID = value.ID; } }
            //public override string ToString()
            //{
            //    return string.Format("Actor: {0} Target: {1} Amount: {2} Task: {3}", this.Actor, this.Target, this.Amount, this.Task);
            //}
            public override string ToString()
            {
                return string.Format("Actor: {0} Target: {1} Amount: {2}", this.Actor, this.Target, this.Amount);
            }
            //public Reservation(GameObject actor, AITask task, TargetArgs target, int stackcount)
            //{
            //    this.Actor = actor.InstanceID;
            //    this.Target = target;
            //    this.Amount = stackcount;
            //    this.Task = task;
            //}
            public Reservation(GameObject actor, TargetArgs target, int stackcount)
            {
                if (stackcount == -1)
                    throw new Exception();
                //this.Actor = actor;
                this.Actor = actor.RefID;
                this.Target = target;
                this.Amount = stackcount;
                if (target.HasObject && stackcount > target.Object.StackSize)
                    throw new Exception();
            }
            public void Write(BinaryWriter w)
            {
                //w.Write(this.Actor.InstanceID);
                w.Write(this.Actor);
                this.Target.Write(w);
                w.Write(this.Amount);
                w.Write(this.TaskID);
            }
            public Reservation(IMap map, BinaryReader r)
            {
                this.Actor = r.ReadInt32();// map.Net.GetNetworkObject(r.ReadInt32());
                this.Target =  TargetArgs.Read(map, r);
                this.Amount = r.ReadInt32();
                this.TaskID = r.ReadInt32();
            }
            public SaveTag Save()
            {
                var tag = new SaveTag(SaveTag.Types.Compound);
                //tag.Add(this.Actor.InstanceID.Save("ActorID"));
                tag.Add(this.Actor.Save("ActorID"));
                tag.Add(this.Target.Save("Target"));
                tag.Add(this.Amount.Save("Amount"));
                return tag;
            }
            public Reservation(IMap map, SaveTag tag)
            {
                this.Actor = tag.GetValue<int>("ActorID");
                this.Target = new TargetArgs(map, tag["Target"]);

                //this.Target = new TargetArgs(map, tag["Target"]);

                this.Amount = tag.GetValue<int>("Amount");
            }

            
        }

        

        public override string Name
        {
            get { return "Reservations"; }
        }
        public ReservationManager(Town town)
        {
            this.Town = town;
        }
        readonly List<Reservation> Reservations = new();
        static int TaskIDSequence = 0;
        static public int GetNextTaskID()
        {
            return TaskIDSequence++;
        }

        public override void Tick()
        {
            /// THIS IS FOR TESTING.
            //foreach (var r in this.Reservations)
            //{
            //    if (r.Target.HasObject && r.Amount > r.Target.Object.StackSize)
            //        "reservation amount more than actual object stacksize".ToConsole();

            //    //if (!r.Target.HasObject)
            //    //    continue;
            //    //var stack = r.Target.Object.StackSize;
            //    ///// reserved items can be reduced in stacksize during a behavior (such as when delivering hauled object to multiple targets)
            //    //if (stack > 0 && r.Amount > stack)
            //    //    "reservation amount more than actual object stacksize".ToConsole();
            //}
        }

        public bool Reserve(GameObject actor, TargetArgs target, int stackCount = -1)
        {
            //return this.Reserve(actor, null, target, stackCount);
            return this.Reserve(actor, actor.CurrentTask, target, stackCount);

            //if (!actor.CanReserve(target, stackCount))
            //    throw new Exception();
            //if (target.Type == TargetType.Null)
            //    return true;
            //if (target.Type == TargetType.Position)
            //    stackCount = 1;
            //else if (target.Type == TargetType.Entity)
            //    stackCount = (stackCount == -1) ? target.Object.StackMax : stackCount;
            //var vation = new Reservation(actor, target, stackCount);
            //Reservations.Add(vation);
            //return true;
        }
        internal bool Reserve(GameObject actor, AITask task, TargetArgs target, int stackCount)
        {
            //if (stackCount == -1)
            //    throw new Exception();
            if (target.Type == TargetType.Null)
                throw new Exception();

            /// MOVED THIS HERE FROM BELOW (check comment below)
            if (target.Type == TargetType.Position)
                stackCount = 1;
            else if (target.Type == TargetType.Entity)
                stackCount = (stackCount != -1) ? stackCount : target.Object.StackSize;//.StackMax; // UNDONE was there a reason i put stackmax?

            // update existing reservation if it exists
            var existing = this.Reservations.FirstOrDefault(r => r.Target.IsEqual(target) && r.Actor == actor.RefID);
            if(existing!=null)
            {
                if (stackCount == existing.Amount)
                    return true;
                var availableAmount = this.GetUnreservedAmount(target) + existing.Amount;
                if (availableAmount < stackCount)
                    return false;
                existing.Amount = stackCount;
                return true;
            }

            if (!actor.CanReserve(target, stackCount))
                throw new Exception();
            /// I MOVED THIS TO THE BEGINNING OF THE FUNCTION because I check the stackCount against any existing reservations which are NEVER -1
            ///if (target.Type == TargetType.Position)
            ///    stackCount = 1;
            ///else if (target.Type == TargetType.Entity)
            ///    stackCount = (stackCount != -1) ? stackCount : target.Object.StackSize;//.StackMax; // UNDONE was there a reason i put stackmax?
            var vation = new Reservation(actor, target, stackCount)
            {
                Task = task
            };
            if (target.HasObject && stackCount > target.Object.StackSize)
                throw new Exception();
            // signal holders of possible existing reservations
            TryCancelExistingReservations(target, stackCount);
            Reservations.Add(vation);
            return true;
        }
        internal bool ReserveAsManyAsPossible(Actor actor, AITask task, TargetArgs target, int desiredAmount = -1)
        {
            if (target.Type == TargetType.Null || target.Type == TargetType.Position)
                throw new Exception();
            var unreservedAmount = this.GetUnreservedAmount(target);
            if (unreservedAmount == 0)
                throw new Exception();
            desiredAmount = desiredAmount == -1 ? target.Object.StackMax : desiredAmount;
            var count = Math.Min(desiredAmount, unreservedAmount);
            if (count > target.Object.StackMax)
                throw new Exception();
            var vation = new Reservation(actor, target, count) { Task = task };
            if (target.HasObject && count > target.Object.StackSize)
                throw new Exception();
            Reservations.Add(vation);
            return true;
        }
        private void TryCancelExistingReservations(TargetArgs target, int stackCount)
        {
            List<Reservation> foundStacks = new();
            int foundAmount = 0;
            for (int i = 0; i < this.Reservations.Count; i++)
            {
                var r = this.Reservations[i];

                if (r.Target.Type != target.Type)
                    continue;
                else if (r.Target.Type == TargetType.Position && r.Target.Global == target.Global)
                {
                    //var actor = this.Map.Net.GetNetworkObject(r.Actor);
                    //var task = actor.CurrentTask;
                    //if (task.ID != r.TaskID)
                    //    throw new Exception();
                    //actor.Net.Log.Write("cancelling " + actor.Name + "'s task's reservations ");
                    //task.Cancel();
                    CancelReservation(r);
                }
                else if (r.Target.Type == TargetType.Entity && r.Target.Object != null && r.Target.Object == target.Object)
                {
                    //if (stackCount > r.Target.Object.StackSize - r.Amount)
                    //    return true;
                    foundStacks.Add(r);
                    foundAmount += r.Amount;
                }
            }
            if (target.HasObject)
            {
                if (foundAmount + stackCount > target.Object.StackMax)
                {
                    for (int i = 0; i < foundStacks.Count; i++)
                    {
                        var r = foundStacks[i];
                        CancelReservation(r);
                    }
                }
            }

            //var existing = Reservations.FirstOrDefault(r =>
            //{
            //    if (r.Target.Type != target.Type)
            //        return false;
            //    if (r.Target.Type == TargetType.Entity && r.Target.Object != null && r.Target.Object == target.Object)
            //    {
            //        if (stackCount > r.Target.Object.StackSize - r.Amount)
            //            return true;
            //    }
            //    else if (r.Target.Type == TargetType.Position && r.Target.Global == target.Global)
            //        return true;
            //    return false;
            //});
        }

        private void CancelReservation(Reservation r)
        {
            var actor = this.Map.Net.GetNetworkObject(r.Actor);
            var task = actor.CurrentTask;
            if (task.ID != r.TaskID)
                throw new Exception();
            actor.Net.Log.Write("cancelling " + actor.Name + "'s task's reservations ");
            task.Cancel();
        }

        internal void Unreserve(GameObject actor)
        {
            Reservations.RemoveAll(r => r.Actor == actor.RefID);
        }
        internal void Unreserve(GameObject actor, AITask task)
        {
            Reservations.RemoveAll(r => r.Actor == actor.RefID && r.TaskID == task.ID);
        }
        internal void Unreserve(GameObject actor, TargetArgs target)
        {
            Reservations.RemoveAll(r => r.Actor == actor.RefID && r.Target.IsEqual(target));
        }
        internal bool ReservedBy(TargetArgs t, GameObject actor, AITask task)
        {
            return Reservations.Any(r =>
                r.Target.IsEqual(t) && r.Actor == actor.RefID && r.TaskID == task.ID
            );
        }
        internal bool CanReserve(GameObject actor, TargetArgs target, int stackcount = -1, bool ignoreOtherReservations = false)
        {
            if (target.Type == TargetType.Entity && target.Object.Parent == actor)
                return true;
            if (target.Type == TargetType.Position && stackcount > 1)
                throw new Exception();
            if (target.IsForbidden)
                return false;
            //var maxPossibleStackCount = target.HasObject ? target.Object.StackSize : 1;
            //stackcount = stackcount != -1 ? stackcount : maxPossibleStackCount;
            //if (stackcount > maxPossibleStackCount)
            //    throw new Exception();
            if (ignoreOtherReservations)
                return true;

            var unreservedAmount = this.GetUnreservedAmount(target);
            stackcount = stackcount == -1 ? (target.HasObject ? target.Object.StackSize : 1) : stackcount;
            return stackcount <= unreservedAmount;

            //var existing = Reservations.FirstOrDefault(r =>
            //    {
            //        if (r.Target.Type != target.Type)
            //            return false;
            //        if (r.Target.Type == TargetType.Entity && r.Target.Object != null && r.Target.Object == target.Object)
            //        {
            //            if (stackcount > r.Target.Object.StackSize - r.Amount)
            //                return true;
            //        }
            //        else if (r.Target.Type == TargetType.Position && r.Target.Global == target.Global)
            //            return true;
            //        return false;
            //    });
            //var can = existing == null;
            //return can;
        }
        internal bool CanReserve(TargetArgs target)
        {
            if (target.IsForbidden)
                return false;
            return Reservations.FirstOrDefault(r =>
            {
                if (r.Target.Type != target.Type)
                    return false;
                if (r.Target.Type == TargetType.Entity && r.Target.Object != null && r.Target.Object == target.Object)
                    return true;
                else if (r.Target.Type == TargetType.Position && r.Target.Global == target.Global)
                    return true;
                return false;
            }) == null;
        }
        internal bool CanReserve(Vector3 global)
        {
            return Reservations.FirstOrDefault(r =>
               r.Target.Type == TargetType.Position && r.Target.Global == global
            ) == null;
        }
        internal int GetUnreservedAmount(TargetArgs target)
        {
            var sum = 0;
            foreach (var r in Reservations) // there might be multiple reservations for the same target, for example an item with stacksize > 1 might be reserved by multiple actors for different tasks
                if (r.Target.IsEqual(target)) 
                {
                    if (r.Amount == -1) // if any of the reservations have amount == -1, it automatically means that the whole stack is reserved, so return 0
                        return 0;
                    sum += r.Amount;
                }
            int amount = 0;

            if (target.Type == TargetType.Entity)
                amount = target.Object.StackSize - sum;
            else if (target.Type == TargetType.Position)
                amount = 1 - sum;

            if (amount < 0)
                throw new Exception(); // CHECK if probably the item was partially reserved for a haul action and it wasn't unreserved after the split stack was picked up

            return amount;
        }
        internal int GetReservedAmount(Actor actor, GameObject item)
        {
            if (item == null)
                throw new Exception();
            return this.Reservations.FirstOrDefault(t => t.Actor == actor.RefID && t.Target.Object == item)?.Amount ?? 0;
        }
        internal bool IsReserved(GameObject gameObject)
        {
            return this.Reservations.Any(t => t.Target.Object == gameObject);
        }
        internal bool IsReserved(IntVec3 global)
        {
            return this.Reservations.Any(t => t.Target.Global == (Vector3)global);
        }
        public override void Write(BinaryWriter w)
        {
            /// i dont want to sync reservations to client
            //w.Write(this.Reservations.Count);
            //foreach (var r in this.Reservations)
            //    r.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            /// i dont want to sync reservations to client
            //this.Reservations.Clear();
            //var count = r.ReadInt32();
            //for (int i = 0; i < count; i++)
            //    this.Reservations.Add(new Reservation(this.Town.Map, r));
        }
        protected override void AddSaveData(SaveTag tag)
        {
            var reservationsTag = new SaveTag(SaveTag.Types.List, "Reservations", SaveTag.Types.Compound);
            foreach (var r in this.Reservations)
                reservationsTag.Add(r.Save());
            tag.Add(reservationsTag);
            tag.Add(TaskIDSequence.Save("TaskIDSequence"));
        }
        public override void Load(SaveTag tag)
        {
            this.Reservations.Clear();
            //var list = tag["Reservations"].Value as List<SaveTag>;
            //foreach (var t in list)
            //    this.Reservations.Add(new Reservation(this.Town.Map, t));
            tag.TryGetTag("Reservations", v =>
            {
                var list = v.Value as List<SaveTag>;
                foreach (var t in list)
                    this.Reservations.Add(new Reservation(this.Town.Map, t));
            });
            tag.TryGetTagValue<int>("TaskIDSequence", out TaskIDSequence);// t => TaskIDSequence = t);
        }

        
    }
}
