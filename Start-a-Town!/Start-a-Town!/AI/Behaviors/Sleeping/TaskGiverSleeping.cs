using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.AI;

namespace Start_a_Town_.AI
{
    class TaskGiverSleeping : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var map = actor.Map;
            var need = actor.GetNeed(NeedDef.Energy);// NeedsComponent.GetNeed(actor, Components.Needs.Need.Types.Energy);
            var energyValue = need.Value;

            //if (energyValue < 98)
            //    return new AITask(TaskDefOf.SleepingOnGround);

            if (energyValue > need.Threshold)// 95)
                return null;
           

            //var possibleBeds = FindBeds(actor);//.OrderByReachableRegionDistance(actor);
            var possibleBeds = FindOrClaimBedNew(actor);//.OrderByReachableRegionDistance(actor);

            foreach (var bed in possibleBeds)
            {
                // determine exact cell from which to operate bed
                var bedglobal = bed.Global;
                var bedCell = map.GetCell(bedglobal);
                var operatingPositions = bedCell.GetOperatingPositionsGlobal(bedglobal);
                foreach (var p in operatingPositions)
                {
                    if (!actor.CanReach(p))
                        continue;
                    if (!map.IsStandableIn(p))
                        continue;
                    var operatingPosition = new TargetArgs(map, p);

                    //var bedentity = map.GetBlock(bedglobal).GetBlockEntity(map, bedglobal) as Blocks.BlockBed.BlockBedEntity;

                    var task = new AITask(typeof(TaskBehaviorSleepingNew), bed, operatingPosition);//, bed);
                    return task;
                }
            }

            if(energyValue == 0)
                return new AITask(TaskDefOf.SleepingOnGround);

            return null;
        }
        private static IEnumerable<TargetArgs> FindOrClaimBedNew(Actor actor)
        {
            var assignedBedrooms = actor.Map.Town.RoomManager.GetRoomsByOwner(actor).Where(r => r.HasRole(RoomRoleDefOf.Bedroom));
            //var ownedBeds = assignedBedrooms.SelectMany(room => room.GetFurniturePositions(FurnitureDefOf.Bed));
            var allbeds = actor.Map.Town.GetUtilities(Utility.Types.Sleeping);
            var bedsByRoomm = allbeds.Select<Vector3, (Vector3 bed, Room room)>(b => (b, actor.Town.RoomManager.GetRoomAt(b)));
            //var bedsByRoom = allbeds.ToLookup(actor.Town.RoomManager.GetRoomAt);

            var bedsByRoom = bedsByRoomm.ToLookup(i => i.room);

            var availableBeds = assignedBedrooms.Any() ? assignedBedrooms.SelectMany(room => bedsByRoom[room]).Concat(bedsByRoom[null]) : bedsByRoom[null];
            foreach (var (bed, room) in availableBeds)
            {
                if (!actor.CanReserve(bed))
                    continue;
                if (!actor.CanOperate(bed))
                    continue;


                if ((room is Room r) // null check
                    && (r.Owner != actor || r.Workplace is not null))
                    throw new Exception();

                yield return new TargetArgs(actor.Map, bed);
            }
        }
        private static IEnumerable<TargetArgs> FindOrClaimBed(Actor actor)
        {
            var assignedBedrooms = actor.Map.Town.RoomManager.GetRoomsByOwner(actor).Where(r => r.HasRole(RoomRoleDefOf.Bedroom));
            var allbeds = actor.Map.Town.GetUtilities(Utility.Types.Sleeping);
            var bedsByRoom = allbeds.ToLookup(actor.Town.RoomManager.GetRoomAt);
            var availableBeds = assignedBedrooms.Any() ? assignedBedrooms.SelectMany(room => bedsByRoom[room]).Concat(bedsByRoom[null]) : bedsByRoom[null];
            foreach (var bed in availableBeds)
            {
                if (!actor.CanReserve(bed))
                    continue;
                if (!actor.CanOperate(bed))
                    continue;
                yield return new TargetArgs(actor.Map, bed);
            }
        }
        private static IEnumerable<TargetArgs> FindBeds(Actor actor)
        {
            var assignedRoom = actor.AssignedRoom;
            var allbeds = actor.Map.Town.GetUtilities(Utility.Types.Sleeping);
            var bedsByRoom = allbeds.ToLookup(actor.Town.RoomManager.GetRoomAt);
            var availableBeds = assignedRoom != null ? bedsByRoom[assignedRoom].Concat(bedsByRoom[null]) : bedsByRoom[null];
            foreach (var bed in availableBeds)
            {
                if (!actor.CanReserve(bed))
                    continue;
                if (!actor.CanOperate(bed))
                    continue;
                yield return new TargetArgs(actor.Map, bed);
            }
        }
    }
}
