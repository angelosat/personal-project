using System.IO;

namespace Start_a_Town_
{
    class InteractionAssignVisitorRoom : Interaction
    {
        int RoomID;
        public InteractionAssignVisitorRoom()
        {

        }
        public InteractionAssignVisitorRoom(int roomID)
        {
            this.RoomID = roomID;
        }
        public override void Perform(Actor a, TargetArgs t)
        {
            var roomOwner = t.Object as Actor;
            var room = a.Map.Town.RoomManager.GetRoom(this.RoomID);
            roomOwner.Ownership.Claim(room);
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
