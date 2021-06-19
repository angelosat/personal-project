using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class RegionManager
    {
        int RoomIDSequence = 0;
        public int GetRoomID()
        {
            return RoomIDSequence++;
        }
        int RegionIDSequence = 0;
        public int GetRegionID()
        {
            return RegionIDSequence++;
        }

        readonly Dictionary<Chunk, RegionTracker> Regions = new Dictionary<Chunk, RegionTracker>();
        readonly Dictionary<int, RegionRoom> Rooms = new Dictionary<int, RegionRoom>();
        readonly Queue<Vector3> ChangedBlocksFromLastFrame = new Queue<Vector3>();
        readonly IMap Map;
        public RegionManager(IMap map)
        {
            this.Map = map;
        }
        //bool Validated = true;

        internal void Init()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var chunk in this.Map.ActiveChunks.Values)
            {
                var tracker = new RegionTracker(chunk);
                this.Regions[chunk] = tracker;
                tracker.Init();
            }
            foreach (var tracker in this.Regions.Values)
                tracker.Update();
            foreach (var tracker in this.Regions.Values)
                tracker.Refresh();
            foreach (var room in this.Rooms.Values)
                room.Init();
            watch.Stop();
            string.Format("regions initialized in {0} ms", watch.ElapsedMilliseconds).ToConsole();
        }

        internal void Draw(Vector3 global, MySpriteBatch sb, Camera cam)
        {
            var region = this.GetRegionAt(global);
            if (region == null)
                return;
            foreach (var ch in this.Regions)
            {
                foreach (var r in ch.Value.Regions.Where(r => r.RoomID == region.RoomID))
                {
                    var color = region.Color * (r == region ? 1 : .5f); //(ch.Key == region.Chunk ? 1 : .5f);
                    cam.DrawGridCells(sb, color, r.GetPositions().Select(p => p.Above()));
                    //foreach (var pos in r.GetPositions())
                    //{
                    //    var glob = pos.Above();
                    //    cam.DrawGridCell(sb, color, glob);
                    //}
                }
            }
            region.DrawNode(global, sb, cam);
        }
        public RegionTracker GetRegions(Chunk chunk)
        {
            return this.Regions[chunk];
        }
        internal RegionNode GetNodeAt(Vector3 global)
        {
            var chunk = this.Map.GetChunk(global);
            if (chunk == null)
                return null;
            return this.Regions[chunk].GetNodeAt(global);
            //if (this.Regions.TryGetValue(chunk, out var regions))
            //    return regions.GetNodeAt(global);
            //return null;
        }
        public Region GetRegionAt(Vector3 global)
        {
            var chunk = this.Map.GetChunk(global);
            RegionTracker regions;
            if (this.Regions.TryGetValue(chunk, out regions))
                return regions.GetRegionAt(global);
            return null;
        }

        internal void OnGameEvent(GameEvent e)
        {
            IMap map;
            switch (e.Type)
            {
                case Components.Message.Types.BlocksChanged:
                
                    IEnumerable<Vector3> positions;
                    GameEvents.EventBlocksChanged.Read(e.Parameters, out map, out positions);
                    this.Update(positions);

                    break;

                case Components.Message.Types.BlockChanged:
                
                    Vector3 global;
                    GameEvents.EventBlockChanged.Read(e.Parameters, out map, out global);
                    this.Update(new List<Vector3>() { global });
                    return;
                    ////this.ChangedBlocksFromLastFrame.Enqueue(global);
                    ////this.Validated = false;
                    //// TODO: in case of batch-changing blocks, find a way to handle them after they have all changed, instead of handling each one individually while they're being changed
                    //{
                    //    var chunk = this.Map.GetChunk(global);
                    //    this.Regions[chunk].Handle(global);
                    //}
                    //// TODO: in case of multiple positions changing during the same frame, instead of handling each one when it changed, handle them all together after they all finished changing
                    //break;

                default:
                    break;
            }
        }

        private void Update(IEnumerable<Vector3> positions)
        {
            //if (this.Map.Net is Client)
            //    return;
            foreach (var pos in positions)
            {
                var chunk = this.Map.GetChunk(pos);
                this.Regions[chunk].Handle(pos);
            }
            var affectedRooms = new HashSet<RegionRoom>();
            foreach (var pos in positions)
            {
                var node = this.GetNodeAt(pos);
                if (node == null)
                    continue;
                affectedRooms.Add(node.Region.Room);
                foreach (var link in node.GetLinksLazy())
                    affectedRooms.Add(link.Region.Room);
            }
            foreach (var room in affectedRooms)
                room.Init();
        }

        internal void Update()
        {
            while(this.ChangedBlocksFromLastFrame.Any())
            {
                var global = this.ChangedBlocksFromLastFrame.Dequeue();
                var chunk = this.Map.GetChunk(global);
                this.Regions[chunk].Handle(global);
                //this.Validated = true;
            }
        }

        internal void Add(Region newRegion)
        {
            this.Regions[newRegion.Chunk].Add(newRegion);
        }

        public bool CanReach(GameObject entity, Vector3 target)
        {
            //if (!this.Validated)
            //    return false;
            var startNode = this.GetNodeAt(entity.Global.Below().SnapToBlock());
            if (startNode == null)
                throw new Exception();
            var currentRegion = startNode.Region;
            var targetNode = this.GetNodeAt(target);
            if (targetNode != null && (targetNode.Region == currentRegion || targetNode.Region.RoomID == currentRegion.RoomID))
                return true;
            //foreach (var pos in this.GetPotentialPositionsAroundDestination(entity, target))
            //{
            //    if (currentRegion.Contains(pos) || this.GetRegionAt(pos).ID == currentRegion.ID)
            //        return true;
            //}
            foreach (var node in this.GetPotentialNodesAroundDestination((int)entity.Physics.Height, target))
            {
                if (node.Region.RoomID == startNode.Region.RoomID)
                    return true;
            }
            return false;
        }
        public IEnumerable<Region> GetAdjacentRegions(int height, Vector3 target)
        {
            HashSet<Region> adjRegions = new HashSet<Region>();
            var targetNode = this.GetNodeAt(target);
            if (targetNode != null)
                adjRegions.Add(targetNode.Region);

            foreach (var node in this.GetPotentialNodesAroundDestination(height, target))
                adjRegions.Add(node.Region);

            return adjRegions;
        }
        //IEnumerable<Vector3> GetPotentialPositionsAroundDestination(GameObject entity, Vector3 global)
        //{
        //    var height = (int)entity.Physics.Height;
        //    var n = global.North();
        //    var s = global.South();
        //    var w = global.West();
        //    var e = global.East();
        //    // WRONG
        //    //for (int i = -1; i < height + 3; i++)
        //    //{
        //    //    yield return n + new Vector3(0, 0, i);
        //    //    yield return s + new Vector3(0, 0, i);
        //    //    yield return w + new Vector3(0, 0, i);
        //    //    yield return e + new Vector3(0, 0, i);
        //    //}
        //    //yield return global + new Vector3(0, 0, height + 1); // directly below? not always prefferable
        //    for (int i = 0; i < 4; i++)
        //    {
        //        yield return n + new Vector3(0, 0, -height - 1 + i);
        //        yield return s + new Vector3(0, 0, -height - 1 + i);
        //        yield return w + new Vector3(0, 0, -height - 1 + i);
        //        yield return e + new Vector3(0, 0, -height - 1 + i);
        //    }
        //    yield return global + new Vector3(0, 0, -height - 1); // directly below? not always prefferable
        //}
        public IEnumerable<RegionNode> GetPotentialNodesAroundDestination(int reach, Vector3 global)
        {
            var n = global.North();
            var s = global.South();
            var w = global.West();
            var e = global.East();
            for (int i = -reach; i < 1; i++)
            {
                //var nn = n + new Vector3(0, 0, -reach - 1 + i);
                //var ss = s + new Vector3(0, 0, -reach - 1 + i);
                //var ww = w + new Vector3(0, 0, -reach - 1 + i);
                //var ee = e + new Vector3(0, 0, -reach - 1 + i);
                var nn = n + new Vector3(0, 0, i);
                var ss = s + new Vector3(0, 0, i);
                var ww = w + new Vector3(0, 0, i);
                var ee = e + new Vector3(0, 0, i);
                var nnode = this.GetNodeAt(nn);
                var snode = this.GetNodeAt(ss);
                var wnode = this.GetNodeAt(ww);
                var enode = this.GetNodeAt(ee);
                if (nnode != null)
                    yield return nnode;
                if (snode != null)
                    yield return snode;
                if (wnode != null)
                    yield return wnode;
                if (enode != null)
                    yield return enode;
            }
            var belowNode = this.GetNodeAt(global + new Vector3(0, 0, -reach));// - 1));
            if (belowNode != null)
                yield return belowNode;
        }

        int GetRegionDistance(Region source, Region target)
        {
            if (source == target)
                return 0;
            //if (source.ID != target.ID)
            //    throw new Exception();
            var all = source.GetConnected();
            Dictionary<Region, int> dist = new Dictionary<Region, int>();
            Dictionary<Region, Region> prev = new Dictionary<Region, Region>();
            HashSet<Region> toHandle = new HashSet<Region>();
            foreach (var reg in all)
            {
                dist[reg] = int.MaxValue;
                prev[reg] = null;
                toHandle.Add(reg);
            }
            dist[source] = 0;
            while (toHandle.Any())
            {
                var ordered = toHandle.OrderBy(r => dist[r]);
                var current = ordered.First();
                toHandle.Remove(current);
                if (current == target)
                {
                    return dist[current];
                }
                foreach (var neigh in current.Neighbors.Where(toHandle.Contains))
                {
                    var d = dist[current] + 1;
                    if (d < dist[neigh])
                    {
                        dist[neigh] = d;
                        prev[neigh] = current;
                    }
                }
            }
            throw new Exception(); // regions weren't connected!
            //var ordered = handled.OrderBy(r => dist[r]);
            //while (ordered.Any())
            //{
            //    var current = ordered.First();

            //}
        }

        internal int GetRegionDistance(Vector3 source, Vector3 target, Actor actor)
        {
            var reach = actor.Physics.Reach;
            // because the vector might have been returned by gameobject.standingon() which returns vector3(int.minvalue) and so we treat the negative z as an invalid position
            // might have to convert it to a nullable vector at some point
            if (source.Z < 0)
                return -1; 
            var sourceReg = this.GetRegionAt(source);
            if (sourceReg == null)
                return -1;
            var adjRegions = this.GetAdjacentRegions(reach, target)
                //.Where(r => r.ID == sourceReg.ID);
                .Where(r => this.AreConnected(sourceReg, r));
            if (!adjRegions.Any())
                return -1;// throw new Exception();
            int minDist = int.MaxValue;
            //foreach (var reg in adjRegions)
            //{
            //    var d = this.GetRegionDistance(sourceReg, reg);
            //    if (d < minDist)
            //        minDist = d;
            //}
            //if (minDist == int.MaxValue)
            //    throw new Exception();
            minDist = adjRegions.Min(reg => this.GetRegionDistance(sourceReg, reg));
            
            return minDist;
        }
        internal bool CanReach(IntVec3 start, IntVec3 goal, Actor actor)
        {
            var reachHeight = actor.Physics.Reach;
            //return this.GetRegionAt(start).Room == this.GetRegionAt(goal).Room;
            var sourceReg = this.GetRegionAt(start);
            var adjRegions = this.GetAdjacentRegions(reachHeight, goal);
            return adjRegions.Any(r => r.Room == sourceReg.Room);
        }
        internal bool CanReach(Vector3 source, Vector3 target, int height, out int dist)
        {
            var sourceReg = this.GetRegionAt(source);
            var adjRegions = this.GetAdjacentRegions(height, target).Where(r => r.RoomID == sourceReg.RoomID);
            dist = int.MaxValue;
            if (!adjRegions.Any())
                return false;
            //foreach (var reg in adjRegions)
            //{
            //    var d = this.GetRegionDistance(sourceReg, reg);
            //    if (d < minDist)
            //        minDist = d;
            //}
            //if (minDist == int.MaxValue)
            //    throw new Exception();
            dist = adjRegions.Min(reg => this.GetRegionDistance(sourceReg, reg));
            return true;
        }
        internal bool AreConnected(Region reg1, Region reg2) // TODO can't just check if roomID of both regions is the same?
        {
            Queue<Region> toCheck = new Queue<Region>();
            toCheck.Enqueue(reg1);
            HashSet<Region> handled = new HashSet<Region>() { reg1 };
            while(toCheck.Any())
            {
                var currentRegion = toCheck.Dequeue();
                if (currentRegion == reg2)
                    return true;
                foreach(var n in currentRegion.Neighbors)
                {
                    if(!handled.Contains(n))
                    {
                        toCheck.Enqueue(n);
                        handled.Add(n);
                    }
                }
            }
            return false;
        }

        internal RegionRoom CreateRoom(Region region)
        {
            var room = new RegionRoom(this);
            this.Rooms[room.ID] = room;
            room.Add(region);
            return room;
        }

        internal RegionRoom GetRoomID(int id)
        {
            return this.Rooms[id];
        }

        internal void RemoveRoom(RegionRoom room)
        {
            this.RemoveRoom(room.ID);
        }
        internal void RemoveRoom(int roomID)
        {
            this.Rooms.Remove(roomID);
        }
        internal void AddRoom(RegionRoom room)
        {
            this.Rooms[room.ID] = room;
        }

        public void Write(BinaryWriter w)
        {
            foreach(var item in this.Regions)
            {
                w.Write(item.Key.MapCoords);
                item.Value.Write(w);
            }
        }
    }
}
