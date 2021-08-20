﻿using System;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System.Linq;
using Start_a_Town_.Net;
using System.IO;

namespace Start_a_Town_
{
    public class BlockBedEntity : BlockEntity
    {
        public enum Types { Citizen, Visitor };
        public bool Occupied { get { return this.CurrentOccupant != -1; } }
        public int CurrentOccupant = -1;
        public Actor Owner;
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
        public void ToggleSleep(GameObject agent, IntVec3 bedGlobal)
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
                //var bedPos = BlockBed.GetPartsDic(agent.Map, bedGlobal)[BlockBed.Part.Top];
                var bedPos = this.OriginGlobal;
                //agent.SetPosition(bedPos + new Vector3(0, 0, BlockBed.GetBlockHeight(agent.Map, bedPos)));
                var cell = agent.Map.GetCell(bedPos);
                agent.SetPosition(bedPos + new IntVec3(0, 0, cell.Block.GetHeight(cell.BlockData, 0, 0)));
                agent.GetNeed(NeedDef.Energy).Mod += 1;
            }
        }

        internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            //info.AddInfo(new Label(() => $"Owner: {this.Owner?.Name ?? ""}"));
            var room = map.GetRoomAt(vector3);
            if (room is not null)
                room.GetSelectionInfo(info);
            //info.AddInfo(new Label(() => $"Owner: {$"{room?.Owner} (from room)" ?? this.Owner?.Name ?? ""}"));

            var roomOwner = room?.Owner;
            //info.AddInfo(new Label(() => $"Owner: {(roomOwner is not null ? (roomOwner.Name  + " (from room)") : (this.Owner?.Name ?? ""))}"));
            info.AddInfo(new ComboBoxNewNew<Actor>(128, "Owner", a => a?.Name ?? "none", setOwner, () => this.Owner, () => map.Town.GetAgents().Prepend(null)));
            void setOwner(Actor newOwner) => Packets.SetOwner(map.Net, map.Net.GetPlayer(), vector3, newOwner);
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

        private static void SetOwner(MapBase map, IntVec3 global, Actor owner)
        {
            map.GetBlockEntity<BlockBedEntity>(global).Owner = owner;
        }

        [EnsureStaticCtorCall]
        static class Packets
        {
            static readonly int pOwner;
            static Packets()
            {
                pOwner = Network.RegisterPacketHandler(SetOwner);
            }

            internal static void SetOwner(INetwork net, PlayerData playerData, IntVec3 global, Actor owner)
            {
                if (net is Server)
                    BlockBedEntity.SetOwner(net.Map, global, owner);

                net.GetOutgoingStream().Write(pOwner, playerData.ID, global, owner?.RefID ?? -1);
            }

            private static void SetOwner(INetwork net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var global = r.ReadIntVec3();
                var owner = r.ReadInt32() is int refID && refID > -1 ? net.GetNetworkObject<Actor>(refID) : null;
                if (net is Client)
                    BlockBedEntity.SetOwner(net.Map, global, owner);
                else
                    SetOwner(net, player, global, owner);
            }
        }
    }
}
