using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class InteractionAssignVisitorRoom : Interaction
    {
        int RoomID;
        int v = 0;
        public InteractionAssignVisitorRoom()
        {

        }
        public InteractionAssignVisitorRoom(int roomID)
        {
            this.RoomID = roomID;
        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            var actor = a as Actor;
            var roomOwner = t.Object as Actor;
            var room = a.Map.Town.RoomManager.GetRoom(this.RoomID);
            //room.OwnerRef = roomOwner.InstanceID;
            roomOwner.Ownership.Claim(room);
            v++;
            //this.State = States.Finished;
        }
        public override object Clone()
        {
            return new InteractionAssignVisitorRoom(this.RoomID);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            this.RoomID.Save(tag, "RoomID");
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryGetTagValueNew<int>("RoomID", ref this.RoomID);
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.RoomID);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.RoomID = r.ReadInt32();
        }
    }
}
