using System;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;
using Start_a_Town_.Blocks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class BlockBedEntity : BlockEntity
    {
        public enum Types { Citizen, Visitor };
        public bool Occupied { get { return this.CurrentOccupant != -1; } }
        public int CurrentOccupant = -1;
        public Types Type = Types.Citizen;
        public BlockBedEntity(IntVec3 originGlobal)
            : base(originGlobal)
        {

        }
        public void Sleep(GameObject agent)
        {
            if (this.Occupied)
                throw new Exception();
            this.CurrentOccupant = agent.RefID;
            agent.GetComponent<SpriteComponent>().Body = agent.Body.FindBone(BoneDefOf.Head);
        }
        public void Wake(GameObject agent)
        {
            if (agent.RefID != this.CurrentOccupant)
                throw new Exception();
            this.CurrentOccupant = -1;
            agent.GetComponent<SpriteComponent>().Body = null;
        }
        public void ToggleSleep(GameObject agent, Vector3 bedGlobal)
        {
            if (this.CurrentOccupant != -1)
            {
                if (agent.RefID != this.CurrentOccupant)
                    throw new Exception();
                this.CurrentOccupant = -1;
                var body = agent.Body;
                var head = body[BoneDefOf.Head];
                body.SetEnabled(true, true);
                body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
                head.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
                agent.GetNeed(NeedDef.Energy).Mod -= 1;
            }
            else
            {
                this.CurrentOccupant = agent.RefID;
                var body = agent.Body;
                var headBone = agent.Body.FindBone(BoneDefOf.Head);
                body.RestingFrame = new Keyframe(0, agent.Body[BoneDefOf.Head].GetTotalOffset(), 0);
                body.SetEnabled(false, true);
                headBone.SetEnabled(true, false);
                headBone.RestingFrame = new Keyframe(0, Vector2.Zero, -(float)(Math.PI / 3f));
                var bedPos = BlockBed.GetPartsDic(agent.Map, bedGlobal)[BlockBed.Part.Top];
                agent.SetPosition(bedPos + new Vector3(0, 0, BlockBed.GetBlockHeight(agent.Map, bedPos)));
                agent.GetNeed(NeedDef.Energy).Mod += 1;
            }
        }
        internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            var room = map.GetRoomAt(vector3);
            room.GetSelectionInfo(info);
        }
        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.CurrentOccupant);
            w.Write((int)this.Type);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
            this.CurrentOccupant = r.ReadInt32();
            this.Type = (Types)r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(new SaveTag(SaveTag.Types.Int, "Occupant", this.CurrentOccupant));
            ((int)this.Type).Save(tag, "Type");
        }
        
        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValue("Occupant", out this.CurrentOccupant);
            tag.TryGetTagValue<int>("Type", v => this.Type = (Types)v);
        }

        internal Color GetColorFromType()
        {
            return this.Type switch
            {
                Types.Citizen => Color.White,
                Types.Visitor => Color.Cyan,
                _ => throw new Exception(),
            };
        }
    }
}
