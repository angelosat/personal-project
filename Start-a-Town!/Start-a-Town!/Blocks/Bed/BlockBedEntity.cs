using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;
using Start_a_Town_.Blocks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    //partial class BlockBed : Block
    //{
    public class BlockBedEntity : BlockEntity
    {
        public enum Types { Citizen, Visitor };
        public bool Occupied { get { return this.CurrentOccupant != -1; } }
        public int CurrentOccupant = -1;
        public Types Type = Types.Citizen;
        //public int ReservedBy = -1;
        public BlockBedEntity()
        {

        }
        public override object Clone()
        {
            return new BlockBedEntity();
        }
        public void Sleep(GameObject agent)
        {
            if (this.Occupied)
                throw new Exception();
            //this.Occupied = true;
            this.CurrentOccupant = agent.RefID;
            //agent.GetComponent<SpriteComponent>().Body = agent.Body.Joints[Graphics.BoneDef.Head].Bone;
            agent.GetComponent<SpriteComponent>().Body = agent.Body.FindBone(BoneDef.Head);
        }
        public void Wake(GameObject agent)
        {
            if (agent.RefID != this.CurrentOccupant)
                throw new Exception();
            //this.Occupied = false;
            this.CurrentOccupant = -1;
            agent.GetComponent<SpriteComponent>().Body = null;
        }
        public void ToggleSleep(GameObject agent, Vector3 bedGlobal)
        {

            //agent.ChangePosition()
            if (this.CurrentOccupant != -1)
            {
                if (agent.RefID != this.CurrentOccupant)
                    throw new Exception();
                //this.Occupied = false;
                this.CurrentOccupant = -1;
                var spriteComp = agent.GetComponent<SpriteComponent>();
                var body = agent.Body;
                var head = body[BoneDef.Head];
                body.SetEnabled(true, true);

                //spriteComp.Body = spriteComp.DefaultBody;// null;
                //spriteComp.Body.Joints[Graphics.BoneDef.Head].Bone.RestingFrame = new Graphics.Keyframe(0, Vector2.Zero, 0);
                body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
                head.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
                //NeedsComponent.GetNeed(agent, Need.Types.Energy).Mod -= 1;
                agent.GetNeed(NeedDef.Energy).Mod -= 1;
                //this.ReservedBy = -1;
            }
            else
            {
                //this.Occupied = true;
                this.CurrentOccupant = agent.RefID;
                var body = agent.Body;
                //var headBone = agent.Body.Joints[Graphics.BoneDef.Head].Bone;
                var headBone = agent.Body.FindBone(BoneDef.Head);

                //agent.GetComponent<SpriteComponent>().Body = headBone;
                //headBone.RestingFrame = new Graphics.Keyframe(0, Vector2.Zero, -(float)(Math.PI / 3f));
                //headBone.RestingFrame = new Graphics.Keyframe(0, agent.Body[Graphics.BoneDef.Head].GetTotalOffset(), -(float)(Math.PI / 3f));
                body.RestingFrame = new Keyframe(0, agent.Body[BoneDef.Head].GetTotalOffset(), 0);
                body.SetEnabled(false, true);
                headBone.SetEnabled(true, false);
                headBone.RestingFrame = new Keyframe(0, Vector2.Zero, -(float)(Math.PI / 3f));
                var bedPos = BlockBed.GetPartsDic(agent.Map, bedGlobal)[BlockBed.Part.Top];
                //var bedPos = BlockBed.GetPartsDic(agent.Map, bedGlobal)[Part.Bottom];
                agent.ChangePosition(bedPos + new Vector3(0, 0, BlockBed.GetBlockHeight(agent.Map, bedPos)));

                //NeedsComponent.GetNeed(agent, Need.Types.Energy).Mod += 1;
                agent.GetNeed(NeedDef.Energy).Mod += 1;

            }
        }
        internal override void GetSelectionInfo(IUISelection info, IMap map, Vector3 vector3)
        {
            var room = map.GetRoomAt(vector3);
            room.GetSelectionInfo(info);
        }
        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.CurrentOccupant);
            w.Write((int)this.Type);
            //w.Write(this.ReservedBy);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
            this.CurrentOccupant = r.ReadInt32();
            this.Type = (Types)r.ReadInt32();
            //this.ReservedBy = r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(new SaveTag(SaveTag.Types.Int, "Occupant", this.CurrentOccupant));
            ((int)this.Type).Save(tag, "Type");
            //tag.Add(new SaveTag(SaveTag.Types.Int, "ReservedBy", this.ReservedBy));
        }
        //public override SaveTag Save(string name)
        //{
        //    var tag = new SaveTag(SaveTag.Types.Compound, name);
        //    tag.Add(new SaveTag(SaveTag.Types.Int, "Occupant", this.CurrentOccupant));
        //    //tag.Add(new SaveTag(SaveTag.Types.Int, "ReservedBy", this.ReservedBy));
        //    return tag;
        //}
        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValue("Occupant", out this.CurrentOccupant);
            tag.TryGetTagValue<int>("Type", v => this.Type = (Types)v);
            //tag.TryGetTagValue<int>("ReservedBy", out this.ReservedBy);
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
