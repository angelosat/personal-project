using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class Room : ISelectable, ISaveable, ISerializable
    {
        static class Packets
        {
            static readonly int PacketSetOwner, PacketSetRoomType, PacketSetWorkplace, PacketRefresh;
            static Packets()
            {
                PacketSetOwner = Network.RegisterPacketHandler(SetOwner);
                PacketSetRoomType = Network.RegisterPacketHandler(SetRoomType);
                PacketSetWorkplace = Network.RegisterPacketHandler(SetWorkplace);
                PacketRefresh = Network.RegisterPacketHandler(Refresh);
            }

          

            public static void SetRoomType(IObjectProvider net, PlayerData player, Room room, RoomRoleDef roomType)
            {
                if (net is Server)
                    room.RoomRole = roomType;
                net.GetOutgoingStream().Write(PacketSetRoomType, player.ID, room.ID, roomType?.Name ?? "");
            }
            private static void SetRoomType(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var room = net.Map.Town.RoomManager.GetRoom(r.ReadInt32());
                var roomdef = r.ReadString() is string roomRoleName && !roomRoleName.IsNullEmptyOrWhiteSpace() ? Def.GetDef<RoomRoleDef>(roomRoleName) : null;
                if (net is Client)
                    room.RoomRole = roomdef;
                else
                    SetRoomType(net, player, room, roomdef);
            }

            public static void SetOwner(IObjectProvider net, PlayerData player, Room room, Actor owner)
            {
                if (net is Net.Server)
                    //room.OwnerRef = ownerRef;
                    room.ForceAddOwner(owner);
                    //owner.Ownership.Claim(room);
                net.GetOutgoingStream().Write(PacketSetOwner, player.ID, room.ID, owner?.RefID ?? -1);
            }
            private static void SetOwner(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var roomID = r.ReadInt32();
                var room = net.Map.Town.RoomManager.GetRoom(roomID);
                var owner = r.ReadInt32() is int id && id != -1 ? net.GetNetworkObject<Actor>(id) : null;
                if (net is Net.Server)
                    SetOwner(net, player, room, owner);
                else
                    //room.OwnerRef = owner;
                    room.ForceAddOwner(owner);
                    //owner.Ownership.Claim(room);

            }

            internal static void SetWorkplace(IObjectProvider net, PlayerData player, Room room, Workplace wplace)
            {
                if (net is Net.Server)
                //room.Workplace = wplace;
                    //wplace.AddRoom(room);
                    room.SetWorkplace(wplace);
                var w = net.GetOutgoingStream();
                w.Write(PacketSetWorkplace);
                w.Write(player.ID);
                w.Write(room.ID);
                w.Write(wplace?.ID ?? -1);
            }
            private static void SetWorkplace(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var roomID = r.ReadInt32();
                var room = net.Map.Town.RoomManager.GetRoom(roomID);
                //var wplace = net.Map.Town.ShopManager.GetShop(r.ReadInt32());
                var wplace = r.ReadInt32() is int id && id != -1 ? net.Map.Town.ShopManager.GetShop(id) : null;

                if (net is Net.Server)
                    SetWorkplace(net, player, room, wplace);
                else
                    //room.Workplace = wplace;
                    //wplace.AddRoom(room);
                    room.SetWorkplace(wplace);
            }

            internal static void Refresh(IObjectProvider net, PlayerData playerData, Room room, IntVec3 center)
            {
                if (net is Server)
                    room.Refresh(center);
                net.GetOutgoingStream().Write(PacketRefresh, playerData.ID, room.ID, center);
            }
            private static void Refresh(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var room = net.Map.Town.RoomManager.GetRoom(r.ReadInt32());
                var center = r.ReadIntVec3();
                if (net is Client)
                    room.Refresh(center);
                else
                    Refresh(net, player, room, center);
            }
        }

        internal void Remove()
        {
            if (GUI?.Tag == this && GUI.GetWindow() is Window win && win.IsOpen)
                win.Hide();
        }

        internal void SetWorkplace(Workplace wplace)
        {
            if (this.Workplace == wplace)
                return;
            if(this.Workplace is Workplace existing)
                //existing.Rooms.Remove(this.ID);
                existing.RemoveRoom(this);
            this.Workplace = wplace;
            if (wplace is not null)
                wplace.AddRoom(this);
            //wplace.Rooms.Add(this.ID);
            this.Owner = null;
            this.Valid = false;
        }
        //internal void SetOwner(Actor actor)
        //{
        //    var id = actor?.RefID ?? -1;
        //    if (this.OwnerRef == id)
        //        return;
        //    this.OwnerRef = id;
        //    this.Workplace = null;
        //}
        internal void AddOwner(Actor actor)
        {
            this.OwnerRef = actor.RefID;
            //this.OwnerRef = actor?.RefID ?? -1;
        }
        internal void ForceAddOwner(Actor actor)
        {
            //this.AddOwner(actor);
            this.Owner = actor;
            if (this.workplace != null)
                this.Workplace = null;
        }
        internal void RemoveOwner(Actor actor)
        {
            if (this.OwnerRef == actor.RefID)
                this.OwnerRef = -1;
        }

        static Room()
        {
        }

        private RoomRoleDef roomRole;
        public HashSet<Vector3> Interior = new();// = new HashSet<Vector3>();
        public HashSet<Vector3> Border = new();
        public Color Color;
        public bool Exists => true;

        public RoomRoleDef RoomRole
        {
            get => roomRole;
            set
            {
                roomRole = value;
                this.Workplace?.RoomChanged(this);
            }
        }

        private int value;
        public int Value
        {
            get
            {
                if (!this.Valid)
                    this.Validate();
                return value;
            }
        }

        public int Size => this.Interior.Count;


        public int ID;
        public int OwnerRef = -1;
        public HashSet<FurnitureDef> Furnitures = new();
        public IMap Map;
        private int workplaceID =-1;
        private Workplace workplace;
        public Workplace Workplace
        {
            get => workplaceID != -1 ? (workplace ??= this.Map.Town.ShopManager.GetShop(workplaceID)) : null;
            set
            {
                workplace = value;
                workplaceID = value?.ID ?? -1;
            }
        }
        public bool HasRole(RoomRoleDef role)
        {
            return this.RoomRole == role;
        }
        public string GetName()
        {
            return $"Room {this.ID}";
        }
        public IEnumerable<Vector3> GetFurniturePositions(FurnitureDef furniture)
        {
            return this.Interior.Where(g => this.Map.GetBlock(g).Furniture == furniture);
        }
        public void GetQuickButtons(UISelectedInfo panel)
        {
            //throw new NotImplementedException();
        }

        public void GetSelectionInfo(IUISelection panel)
        {
            panel.AddInfo(new Label(() => $"Owner: {this.Owner?.Name ?? "none"}"));
            panel.AddInfo(new Label(() => $"Workplace: {this.Workplace?.Name ?? "none"}"));
            return;

            var ownerbox = new GroupBox();
            var lblOwner = new Label("Assigned to: ");
            var actorlist = this.Map.Town.GetAgents().ToList();
            actorlist.Insert(0, null);
            var cbox = new ComboBox<Actor>(actorlist, 150, 400, a => a?.Name ?? "None", (a, c) =>
            {
            c.LeftClickAction = () =>
                  Packets.SetOwner(Client.Instance, Client.Instance.CurrentPlayer, this, a);// a?.InstanceID ?? -1);
            })
            {
                Location = lblOwner.TopRight,
                TextFunc = () =>
                this.GetOwner()?.Name ?? "none"
            };
            ownerbox.AddControlsHorizontally(lblOwner, cbox);
            panel.AddInfo(ownerbox);
            //panel.AddInfo(cbox);// { TextFunc = () => this.GetOwner.Name });
        }
        internal void AddEdge(Vector3 global)
        {
            //this.Edges.Add(global);
        }
        internal void AddPosition(Vector3 global)
        {
            this.Interior.Add(global);
        }
        internal void AddPositions(IEnumerable<Vector3> positions)
        {
            foreach (var p in positions)
                this.AddPosition(p);
        }
        private void RemovePosition(Vector3 global)
        {
            this.Interior.Remove(global);
        }
        private void RemovePositions(IEnumerable<Vector3> globals)
        {
            foreach (var g in globals)
                this.RemovePosition(g);
        }
        internal void AddEdges(IEnumerable<Vector3> edges)
        {
            foreach (var p in edges)
                this.AddPosition(p);
        }
        internal void Absorb(Room smallerRoom)
        {
            this.AddPositions(smallerRoom.Interior);
            this.AddEdges(smallerRoom.Border);
        }
        internal void Invalidate()
        {
            this.Valid = false;
        }

        public Room()
        {
            this.Color = ColorHelper.GetRandomColor();
        }
        public Room(IMap map) : this()
        {
            this.Map = map;
        }
        Room(IMap map, HashSet<Vector3> positions) : this(map)
        {
            //this.ID = map.Town.RoomManager.GetNextRoomID();
            this.Interior = positions;
        }
        public Room(IMap map, IEnumerable<IntVec3> positions) : this(map)
        {
            //this.ID = map.Town.RoomManager.GetNextRoomID();
            this.Interior = new();// HashSet<Vector3>(positions.Cast<Vector3>());
            this.Border = new();

            foreach (var p in positions)
            {
                if (map.GetCell(p).IsRoomBorder)
                    //this.Edges.Add(p);
                    this.AddEdge(p);
                else
                    //this.Positions.Add(p);
                    this.AddPosition(p);
            }
        }
        public Room(RoomManager manager, ICollection<Vector3> positions) : this(manager.Map, new HashSet<Vector3>(positions)) { }
        static public bool TryCreate(IMap map, Vector3 global, out Room room)
        {
            room = new Room(map);
            var area = EnclosedArea.BeginExclusiveAsList(map, global);
            if (area == null)
                return false;
            room.Interior = area;
            //room.ID = map.Town.RoomManager.GetNextRoomID();
            return true;
        }
        static public Room TryCreate(IMap map, IntVec3 begin)
        {
            if (map.GetCell(begin) is not Cell cell)
                return null;
            if (cell.IsRoomBorder)
                return null;
            if (map.IsAboveHeightMap(begin))
                return null;
            HashSet<IntVec3> interior = new();
            HashSet<IntVec3> edges = new();

            interior.Add(begin);

            Queue<IntVec3> toHandle = new();
            HashSet<IntVec3> handled = new() { begin };
            toHandle.Enqueue(begin);
            while (toHandle.Any())
            {
                var current = toHandle.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    if (!map.TryGetCell(n, out var ncell))
                        continue;
                    if (map.IsAboveHeightMap(n))
                        return null;
                    if (ncell.IsRoomBorder)
                        edges.Add(n);
                    else
                    {
                        interior.Add(n);
                        toHandle.Enqueue(n);
                    }
                }
            }
            var room = new Room(map);// { Positions = interior, Edges = edges };
            foreach (var p in interior)
                //room.Positions.Add(p);
                room.AddPosition(p);
            foreach (var p in edges)
                //room.Edges.Add(p);
                room.AddEdge(p);
            return room;
        }
        public bool Contains(Vector3 global)
        {
            return this.Interior.Contains(global)
                //|| this.Border.Contains(global)
                ;
        }

        internal bool TryRemovePosition(Vector3 global, out List<Room> newRooms)
        {
            var map = this.Map;
            if (!this.Contains(global))
                throw new Exception();
            newRooms = new List<Room>();
            if (!map.GetCell(global).IsRoomBorder)
                return false;
            this.RemovePosition(global);
            //var neighbors = global.GetAdjacent();
            //for (int i = 0; i < neighbors.Length; i++)
            //{
            //    var n = neighbors[i];
            foreach(var n in global.GetAdjacentLazy())
            { 
                if (!map.TryGetCell(n, out var cell))
                    continue;
                if (cell.IsRoomBorder)
                    continue;
                //check if still connected
                var area = EnclosedArea.BeginExclusiveAsList(map, n);
                //if (area.Count < this.Positions.Count)// room is disconnected 
                if(area is not null)
                if (this.Interior.Any(p => !area.Contains(p)))
                {

                    // determine which is the dominant room
                    if (area.Count > (float)this.Interior.Count / 2) // if current room is larger 
                    {
                        var oldPositions = this.Interior;
                        this.Interior = area;

                        oldPositions.RemoveWhere(p => area.Contains(p));
                        newRooms.Add(new Room(map, oldPositions));
                    }
                    else
                    {
                        newRooms.Add(new Room(map, area));
                            //this.Positions.RemoveWhere(p => area.Contains(p));
                            this.RemovePositions(this.Interior.Where(area.Contains).ToList());
                    }

                }
            }
            return newRooms.Any();
        }

        

        internal bool TryExpandInto(Vector3 global)
        {
            //if (!this.Map.GetCell(global).IsRoomBoundary)
            //    throw new Exception();
            //foreach (var n in global.GetAdjacentLazy())
            //{
            //    if (this.Positions.Contains(n))
            //    {
            //        this.Positions.Add(global);
            //        return true;
            //    }
            //}
            //return false;

            var map = this.Map;
            var area = EnclosedArea.BeginExclusiveAsList(map, global);
            if (area == null)
                return false;
            if (this.Interior.Any(p => !area.Contains(p)))
                throw new Exception();
            this.Interior = area;
            return true;
        }
        void Validate()
        {
            this.Valid = true;
            this.value = 0;
            this.Furnitures.Clear();
            this.Border.Clear();
            var furnitureMultiplier = 10;
            foreach (var pos in this.Interior)
            {
                var cell = this.Map.GetCell(pos);
                var material = cell.Material;
                var value = material.Value;
                if (cell.Block.Furniture is FurnitureDef furn)
                {
                    this.Furnitures.Add(furn);
                    value *= furnitureMultiplier;
                }
                //if (this.Map.GetBlockEntity(pos) is BlockBedEntity)
                //    BlockBed.SetType(this.Map, pos, this.Workplace is Tavern ? BlockBedEntity.Types.Visitor : BlockBedEntity.Types.Citizen);
                this.value += value;
                this.DetectBorders(pos);
            }
            this.value /= 20;
            if (this.roomRole is not null)
                if (!this.roomRole.Furniture.IsSubsetOf(this.Furnitures))
                    this.RoomRole = null;
        }
        //Room DetectBorders()
        //{
        //    this.Border.Clear();
        //    foreach (var g in this.Interior)
        //        this.DetectBorders(g);
        //        //if (this.Map.GetCell(g).IsRoomBorder)
        //        //        this.Border.Add(g);
        //    return this;
        //}
        void DetectBorders(IntVec3 g)
        {
            foreach(var n in g.GetAdjacentLazy())
                if (this.Map.GetCell(n).IsRoomBorder)
                    this.Border.Add(n);
        }
        public void TabGetter(Action<string, Action> getter)
        {
            throw new NotImplementedException();
        }
        //private Actor owner;
        //public Actor Owner => owner ??= GetOwner();
        public Actor Owner { get => GetOwner(); set => this.OwnerRef = value?.RefID ?? -1; }
        //public TimeSpan TimeClaimed;

        public Actor GetOwner()
        {
            return this.Map.Net.GetNetworkObject(this.OwnerRef) as Actor;
        }
        public override string ToString()
        {
            return $"Room [id:{this.ID}][size:{this.Interior.Count}][owner:{this.GetOwner()?.Name ?? "<none>"}]";
        }
        public Control GetControl()
        {
            return new Label(this);
        }
        public void Refresh(IntVec3 center)
        {
            this.Valid = false;
            return;
            if (!EnclosedArea.BeginInclusive(this.Map, center, this.Interior, this.Border))
                this.Map.Town.RoomManager.RemoveRoom(this);
        }

        public void OnBlockChanged(IntVec3 global)
        {
            throw new NotImplementedException();
            if (!this.Contains(global))
                throw new Exception();
            bool wasInterior = this.Interior.Contains(global);
            var map = this.Map;
            var manager = map.Town.RoomManager;
            var cell = map.GetCell(global);
            if (cell.IsRoomBorder) // if the new block is a border
            {
                if (wasInterior) // if the old block was not a border
                {
                    this.Interior.Remove(global);
                    this.Border.Add(global);
                    foreach (var n in global.GetAdjacentLazy()) // remove border cells that don't have a neighbor that is contained in the interior
                    {
                        if (!n.GetAdjacentLazy().Any(i => this.Interior.Contains(i))) 
                            this.Border.Remove(n);
                    }
                }

            }
            else // the new block is not a border
            {
                if (!wasInterior) // if old block was border
                {
                    this.Border.Remove(global);
                    this.Interior.Add(global);
                    foreach (var n in global.GetAdjacentLazy())
                    {
                        // add new borders OR merge rooms OR remove room if exposed to outdoors
                        if (map.GetCell(n).IsRoomBorder)
                            this.Border.Add(n);
                    }
                }
            }
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ID.Save("ID"));
            tag.Add(this.Interior.Save("Positions"));
            //this.Edges.Save(tag, "Edges");
            //tag.Add(this.Edges.Save("Edges"));
            tag.Add(this.OwnerRef.Save("OwnerRef"));
            //tag.Add("Workplace", this.workplace?.ID ?? -1);
            tag.Add("Workplace", this.workplaceID);
            //tag.TrySaveRef(this.workplace, "Workplace");
            tag.Add("RoomDef", this.roomRole?.Name ?? "");
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("ID", out this.ID);
            tag.TryGetTag("Positions", t => this.Interior = new HashSet<Vector3>().LoadVectors(t));
            //this.Edges.TryLoad(tag, "Edges");
            tag.TryGetTagValue("OwnerRef", out this.OwnerRef);
            //tag.TryGetTagValue<int>("Workplace", v => this.workplace = v != -1 ? this.Map.Town.ShopManager.GetShop(v) : null);
            tag.TryGetTagValueNew<int>("Workplace", ref this.workplaceID);
            //tag.TryLoadRef("Workplace", out this.workplace);
            tag.TryGetTagValue<string>("RoomDef", v => this.roomRole = !v.IsNullEmptyOrWhiteSpace() ? Def.GetDef<RoomRoleDef>(v) : null);
            return this;
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            w.Write(this.Interior);
            //w.Write(this.Edges);
            w.Write(this.OwnerRef);
            //w.Write(this.workplace?.ID ?? -1);
            w.Write(this.workplaceID);
            w.Write(this.roomRole?.Name ?? "");
        }
        public ISerializable Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Interior = new HashSet<Vector3>().ReadVector3(r);
            //this.Edges.ReadVector3(r);
            this.OwnerRef = r.ReadInt32();
            //this.workplace = (r.ReadInt32() is int v && v != -1) ? this.Map.Town.ShopManager.GetShop(v) : null;
            this.workplaceID = r.ReadInt32();
            this.roomRole = (r.ReadString() is string s && !s.IsNullEmptyOrWhiteSpace()) ? Def.GetDef<RoomRoleDef>(s) : null;
            return this;
        }

        internal void ShowGUI(IntVec3 global)
        {
            var gui = GUI ??= Create();
            gui.GetData((this.Map, global));
            gui.GetWindow().Show();
        }

        static Control GUI;
        private bool Valid;

        static GroupBox Create()
        {
            var box = new GroupBox();
            Room currentRoom = null;
            IntVec3 center = default;
            box.AddControlsVertically(
                new ComboBoxNewNew<RoomRoleDef>(128, "Role", r => r?.Label ?? "none", setRoomDef, () => currentRoom?.RoomRole, () => currentRoom.Furnitures.SelectMany(f => RoomRoleDef.ByFurniture(f)).Distinct().Prepend(null)),
                new ComboBoxNewNew<Actor>(128, "Owner", a => a?.Name ?? "none", setOwner, () => currentRoom?.GetOwner(), () => currentRoom?.Map.Town.GetAgents().Prepend(null)),
                new ComboBoxNewNew<Workplace>(128, "Workplace", w => w?.Name ?? "none", setWorkplace, () => currentRoom?.Workplace, () => currentRoom.Map.Town.ShopManager.GetShops().Where(sh => sh.IsValidRoom(currentRoom)).Prepend(null)),
                //new Label(() => $"Size: {currentRoom?.Positions.Count} cells"),
                new Label(() => $"Interior: {currentRoom?.Interior.Count} cells"),
                new Label(() => $"Edges: {currentRoom?.Border.Count} cells"),
                new Label(() => $"Value: {currentRoom?.Value}"),
                //new Label(() => $"Time claimed: {currentRoom?.TimeClaimed.ToString("c")}"),

                new Button("Refresh", refresh)
                //new Button("Clear workplace", () => setWorkplace(null))
                );
            box.SetGetDataAction(o =>
            {
                var oo = ((IMap map, IntVec3 global))o;
                var map = oo.map;
                var global = oo.global;
                currentRoom = map.Town.RoomManager.GetRoomAt(global);
                //currentRoom.Validate();
                center = global;
                box.Tag = currentRoom;
                box.GetWindow().SetTitle(currentRoom.GetName());
            });
            box.ToWindow("Room settings");
            return box;

            void setRoomDef(RoomRoleDef rdef) => Packets.SetRoomType(currentRoom.Map.Net, currentRoom.Map.Net.CurrentPlayer, currentRoom, rdef);
            void setOwner(Actor actor) => Packets.SetOwner(currentRoom.Map.Net, currentRoom.Map.Net.CurrentPlayer, currentRoom, actor);//.InstanceID);
            void setWorkplace(Workplace wplace) => Packets.SetWorkplace(currentRoom.Map.Net, currentRoom.Map.Net.CurrentPlayer, currentRoom, wplace);
            void refresh() => Packets.Refresh(currentRoom.Map.Net, currentRoom.Map.Net.GetPlayer(), currentRoom, center);
        }

        public IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            throw new NotImplementedException();
        }
    }
}
