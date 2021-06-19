using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Start_a_Town_.Towns;
using Microsoft.Xna.Framework;
using System.IO;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class RoomManager : TownComponent
    {
        int RoomIDSequence;
        int GetNextRoomID()
        {
            return RoomIDSequence++;
        }
        public override string Name => "InsideManager";
        readonly Dictionary<int, Room> Rooms = new();
        //readonly ObservableCollection<Room> RoomsObservable = new();
        public RoomManager(Town town)
        {
            this.Town = town;
        }
        //internal override void OnGameEvent(GameEvent e)
        //{
        //    switch (e.Type)
        //    {
        //        case Components.Message.Types.BlocksChanged:
        //            //Handle(e.Parameters[0] as IMap, e.Parameters[1] as IEnumerable<Vector3>);
        //            foreach (var pos in e.Parameters[1] as IEnumerable<Vector3>)
        //                Handle(e.Parameters[0] as IMap, pos);
        //            break;

        //        case Components.Message.Types.BlockChanged:
        //            Handle(e.Parameters[0] as IMap, (Vector3)e.Parameters[1]);
        //            break;

        //        default:
        //            break;
        //    }
        //}
        internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
        {
            foreach (var pos in positions)
                Handle(pos);
        }
        internal Room GetRoom(int roomID)
        {
            return this.Rooms[roomID];
        }
        public bool TryGetRoom(int roomID, out Room room)
        {
            return this.Rooms.TryGetValue(roomID, out room);
        }
        internal Room FindRoom(int actorID)
        {
            return this.Rooms.Values.FirstOrDefault(r => r.OwnerRef == actorID);
        }
        internal IEnumerable<Room> GetRoomsByOwner(Actor actor)
        {
            return this.GetRoomsByOwner(actor.RefID);
        }
        internal IEnumerable<Room> GetRoomsByOwner(int actorID)
        {
            return this.Rooms.Values.Where(r => r.OwnerRef == actorID);
        }
        bool Valid = true;
        public void Init()
        {
            if (!this.Valid)
            {
                this.Valid = true;
            this.ScanMap();
            }
        }

        private void ScanMap()
        {
            var map = this.Map as GameModes.StaticMaps.StaticMap;
            if (map.Net is Client)
                return;
            HashSet<IntVec3> handled = new();
            var size = map.Size.Blocks;
            var maxh = map.MaxHeight;
            this.Rooms.Clear();
            var sw = Stopwatch.StartNew();
            foreach (var chunk in map.ActiveChunks.Values)
            {
                foreach (var cell in chunk.CellGrid2)
                {
                    var global = cell.GetGlobalCoords(chunk);
                    if (handled.Contains(global))
                        continue;
                    if (cell.IsRoomBorder)
                        continue;
                    var positions = FloodFill.BeginIncludeEdges(map, global, (cell, global) => !cell.IsRoomBorder);
                    bool isOutdoors = false;
                    foreach (var p in positions)
                    {
                        if (!isOutdoors)
                        {
                            if (map.IsAboveHeightMap(p))
                                isOutdoors = true;
                        }
                        handled.Add(p);
                    }

                    if (!isOutdoors)
                    {
                        var newroom = new Room(map, positions);
                        this.AddRoom(newroom);
                        $"Room found (size: {newroom.Size})".ToConsole();
                    }
                }
            }
            sw.Stop();
            $"{this.Rooms.Count} rooms found in {sw.ElapsedMilliseconds} ms".ToConsole();
        }

        void HandleNew(IMap map, Vector3 global)
        {
            var cell = map.GetCell(global);
            var block = cell.Block;
            if(!cell.IsRoomBorder)
            {
                var room = EnclosedArea.BeginExclusive(map, global);
                if (room != null)
                    Console.WriteLine(string.Format("room found at {0} with {1} cells", global.ToString(), room.Positions.Count));
            }
            else
            {
                foreach (var n in global.GetAdjacentLazy())
                {
                    if (!map.Contains(n))
                        continue;
                    if (cell.IsRoomBorder)
                        continue;
                    //var room = Room.Create(map, n);// EnclosedArea.BeginExclusive(map, n);
                    //if (room != null)
                    if(Room.TryCreate(map, n, out var room))
                    {
                        Console.WriteLine(string.Format("room found at {0} with {1} cells", global.ToString(), room.Interior.Count));
                        //this.Rooms.Add(room.ID, room);
                        this.AddRoom(room);
                    }
                }
            }
        }
        internal void AddRoom(Room room)
        {
            room.ID = GetNextRoomID();
            this.Rooms.Add(room.ID, room);
            //this.RoomsObservable.Add(room);
        }
        internal void RemoveRoom(Room room)
        {
            this.Rooms.Remove(room.ID);
            room.Remove();
            //this.RoomsObservable.Remove(room);
        }
        void HandleWithBorders(IMap map, Vector3 global)
        {
            throw new NotImplementedException();

            var block = map.GetBlock(global);
            if (!block.IsRoomBorder)
                TryConnectRoomsAtNew(global);
            else
                TryDisconnectRoomsAt(map, global);
        }

       

        void Handle(Vector3 global)
        {
            var map = this.Map;
            var block = map.GetBlock(global);
            //if (block.Type == Block.Types.Air)
            if(!block.IsRoomBorder)
            {
                //TryConnectRoomsAt(global);
                if (this.TryGetRoomAt(global, out var existing))
                {
                    existing.Invalidate();
                }
                TryConnectRoomsAtNew(global);
            }
            else
            {
                if (this.TryGetRoomAt(global, out var existing))
                {
                    existing.Invalidate();
                    if (existing.TryRemovePosition(global, out var newRooms)) // TODO this method is old and doesnt include edges
                    {
                        foreach (var newroom in newRooms)
                            //this.Rooms.Add(newroom.ID, newroom);
                            this.AddRoom(newroom);
                    }
                    else
                    {
                        if (!existing.Interior.Any())
                            this.RemoveRoom(existing);
                    }
                    return; // temporary
                }
                else // the changed position wasn't part of a room, so no room has been split. check adjacent cells for newly formed rooms
                {
                    foreach (var n in global.GetAdjacentLazy())
                    {
                        // TODO check if a new indoors area has been created after placing this block
                        if (Room.TryCreate(map, n) is Room newRoom)
                            this.AddRoom(newRoom);
                    }
                }
                // i check neighbors in existing.TryRemovePosition
                //foreach (var n in global.GetAdjacentLazy())
                //{
                //    if (!map.Contains(n))
                //        continue;
                //    if (map.GetBlock(n).Type != Block.Types.Air)
                //        continue;
                //    if (Room.TryCreate(map, n, out var room))
                //        this.Rooms.Add(room.ID, room);
                //}
            }
            //if (this.TryGetRoomAt(global, out var room)) // TODO put this somewhere else
            //    room.Invalidate();
        }
        private void TryConnectRoomsAtNew(Vector3 global)
        {
            //foreach (var n in global.GetAdjacentLazy())
            //{
            //    if(this.TryGetRoomAt(n, out var nroom))
            //    {

            //    }
            //}
            // TODO can optimize below
            var adjGlobals = global.GetAdjacentLazy();
            var adjRooms = adjGlobals.Select(this.GetRoomAt);
            var nrooms = adjRooms.OfType<Room>().Distinct().ToArray();

            if (adjGlobals.Any(g => !this.Map.GetCell(g).IsRoomBorder && this.GetRoomAt(g) is null))
            // if an adjacent cell is NOT a room boundary and it's not contained in an existing room
            // it means that it adjacent to an outdoors area. so delete all adjacent rooms
            {
                foreach (var r in nrooms)
                    this.RemoveRoom(r);
                return;
            }

            if (!nrooms.Any()) // if the position was surrounded by room borders, create a new room of size 1
            {
                var newroom = new Room(this, new[] { global });
                this.AddRoom(newroom);
                return;
            }
            else if (nrooms.Length == 1)
            {
                var singleRoom = nrooms[0];
                singleRoom.AddPosition(global);
                singleRoom.Invalidate();
                return;
            }
            else
            {
                var bySize = nrooms.OrderByDescending(r => r.Size);
                var largestRoom = bySize.First();
                largestRoom.AddPosition(global);
                var otherRooms = bySize.Skip(1);
                foreach(var other in otherRooms)
                {
                    largestRoom.Absorb(other);
                    this.RemoveRoom(other);
                }
                largestRoom.Invalidate();
            }
        }
        private void TryConnectRoomsAtNewBorders(Vector3 global) //blockremoved
        {
            // TODO can optimize below
            var adjGlobals = global.GetAdjacentLazy();
            var adjRooms = adjGlobals.Select(this.GetRoomAt);
            var nrooms = adjRooms.OfType<Room>().Distinct().ToArray();

            if (adjGlobals.Any(g => !this.Map.GetCell(g).IsRoomBorder && this.GetRoomAt(g) is null))
            // if an adjacent cell is NOT a room boundary and it's not contained in an existing room
            // it means that it adjacent to an outdoors area. so delete all adjacent rooms
            {
                foreach (var r in nrooms)
                    this.RemoveRoom(r);
                return;
            }

            if (!nrooms.Any()) // if the position was surrounded by room borders, create a new room of size 1
            {
                var newroom = new Room(this, new[] { global });
                this.AddRoom(newroom);
                return;
            }
            else if (nrooms.Length == 1)
            {
                var singleRoom = nrooms[0];
                singleRoom.AddPosition(global);
                singleRoom.Invalidate();
                return;
            }
            else
            {
                var bySize = nrooms.OrderByDescending(r => r.Size);
                var largestRoom = bySize.First();
                largestRoom.AddPosition(global);
                var otherRooms = bySize.Skip(1);
                foreach (var other in otherRooms)
                {
                    largestRoom.Absorb(other);
                    this.RemoveRoom(other);
                }
                largestRoom.Invalidate();
            }
        }
        private void TryDisconnectRoomsAt(IMap map, Vector3 global)
        {
            if (this.TryGetRoomAt(global, out var existing))
            {
                existing.Invalidate();
                if (existing.TryRemovePosition(global, out var newRooms)) // TODO this method is old and doesnt include edges
                {
                    foreach (var newroom in newRooms)
                        this.AddRoom(newroom);
                }
                else
                {
                    if (!existing.Interior.Any())
                        this.RemoveRoom(existing);
                }
                return; // temporary
            }
            else // the changed position wasn't part of a room, so no room has been split. check adjacent cells for newly formed rooms
            {
                foreach (var n in global.GetAdjacentLazy())
                {
                    // TODO check if a new indoors area has been created after placing this block
                    if (Room.TryCreate(map, n) is Room newRoom)
                        this.AddRoom(newRoom);
                }
            }

        }
        [Obsolete]
        private void TryConnectRoomsAt(Vector3 global)
        {
            foreach (var n in global.GetAdjacentLazy())
            {
                if (this.TryGetRoomAt(n, out var existing))
                {
                    if (!existing.TryExpandInto(global)) // returns false if expanding to an outdoors area
                        this.RemoveRoom(existing);
                    else
                        existing.Invalidate();
                    break; // no need to check for other adjacent rooms
                           // TODO need to merge rooms if a shared room boundary was removed
                }
            }
        }

        internal override void OnTargetSelected(IUISelection info, TargetArgs selected)
        {
            if(this.GetRoomAt(selected.FaceGlobal) is Room r)
                info.AddTabAction("Roomm", () => r.ShowGUI(selected.FaceGlobal));
            return;

            var global = selected.Global;
            var block = this.Map.GetBlock(global);
            if(RoomRoleDef.ByFurniture(block.Furniture).Any())
            {
                var room = this.GetRoomAt(global);// this.Rooms.Values.FirstOrDefault(r => r.Contains(global));// face));
                if (room is not null)
                    info.AddTabAction("Roomm", () => room.ShowGUI(global));
            }
        }
        public override ISelectable QuerySelectable(TargetArgs selected)
        {
            return null; // instead of selecting the room itself, add a tab when selecting a block that is contained in the room
            var face = selected.FaceGlobal;
            return this.Rooms.Values.FirstOrDefault(r => r.Contains(face));
        }

        public override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            if (!Engine.DrawRooms)
                return;
            foreach (var room in this.Rooms)
                cam.DrawGridBlocks(sb, Block.BlockHighlight, room.Value.Interior, room.Value.Color);
        }

        public bool TryGetRoomAt(Vector3 global, out Room room)
        {
            room = this.Rooms.Values.FirstOrDefault(r => r.Contains(global));
            return room != null;
        }
        public Room GetRoomAt(Vector3 global)
        {
            return this.Rooms.Values.FirstOrDefault(r => r.Contains(global));
        }

        internal override void OnTooltipCreated(Tooltip tooltip, TargetArgs targetArgs)
        {
            if (targetArgs.Type != TargetType.Position)
                return;
            var global = targetArgs.FaceGlobal;
            if (!this.TryGetRoomAt(global, out var room))
                return;
            var control = room.GetControl().ToPanelLabeled("Room");// { LocationFunc = () => tooltip.BottomLeft };
            tooltip.AddControlsBottomLeft(control);
        }
       
        protected override void AddSaveData(SaveTag tag)
        {
            this.Rooms.Values.TrySaveNewBEST(tag, "Rooms");
            tag.Add(this.RoomIDSequence.Save("RoomIDSequence"));
            this.Valid.Save(tag, "Valid");
        }
        public override void Load(SaveTag tag)
        {
            this.Rooms.Load(tag, "Rooms", r => { r.Map = this.Map; return r.ID; });
            tag.TryGetTagValue<int>("RoomIDSequence", out this.RoomIDSequence);
            tag.TryGetTagValueNew("Valid", ref this.Valid);
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.RoomIDSequence);
            w.Write(this.Rooms.Values.ToList());
            w.Write(this.Valid);
        }
        public override void Read(BinaryReader r)
        {
            this.RoomIDSequence = r.ReadInt32();
            this.Rooms.Read(r, room => room.ID, this.Map);
            this.Valid = r.ReadBoolean();
        }
    }
}
