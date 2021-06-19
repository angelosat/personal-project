using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Towns;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public abstract class Workplace : ISerializable, ISaveable//, ILoadReferencable
    {
        class Packets
        {
            static int PacketUpdateWorkerRoles;
            static public void Init()
            {
                PacketUpdateWorkerRoles = Network.RegisterPacketHandler(UpdateWorkerRoles);
            }
            
            static void UpdateWorkerRoles(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var tavern = net.Map.Town.GetShop(r.ReadInt32());
                var role = Def.GetDef<JobDef>(r.ReadString());
                var actor = net.GetNetworkObject<Actor>(r.ReadInt32());// r.ReadIntArray());.Cast<Actor>();
                if (net is Client)
                    tavern.ToggleJob(actor, role);
                else
                    UpdateWorkerRoles(net, player, tavern, role, actor);
            }
            public static void UpdateWorkerRoles(IObjectProvider net, PlayerData player, Workplace tavern, JobDef role, Actor actor)
            {
                if (net is Server)
                    tavern.ToggleJob(actor, role);
                var w = net.GetOutgoingStream();
                w.Write(PacketUpdateWorkerRoles, player.ID, tavern.ID, role.Name, actor.RefID);
            }
        }

        static Workplace()
        {
            Packets.Init();
        }
        readonly protected HashSet<int> Stockpiles = new();
        readonly protected HashSet<int> Workers = new();
        protected Dictionary<int, WorkerProps> WorkerProps = new();
        public HashSet<int> Rooms = new();
        public IntVec3? Counter;

        readonly protected Dictionary<WorkerRoleDef, WorkerRole> Roles;/* = RolesAll.ToDictionary(d => d, d => new WorkerRole(d));*/

        public IObjectProvider Net => this.Town.Net;
        public Town Town;
        public IMap Map => this.Town.Map;
        public int ID;
        //string _Name;
        public string Name;// { return this._Name.IsNullEmptyOrWhiteSpace() ? string.Format("Shop{0}", this.ID) : this._Name; } set { this._Name = value; } }
        public string DefaultName => $"{this.GetType().Name}{this.ID}";
        //public string GetUniqueLoadID() => this.DefaultName;
        public Workplace()
        {

        }
        public Workplace(TownComponent manager, int id) : this(manager)
        {
            this.ID = id;
            //this.Name = string.Format("Shop{0}", this.ID);
        }
        public Workplace(TownComponent manager)
        {
            this.Town = manager.Town;
        }
        public virtual void Tick() { }
        public virtual IEnumerable<JobDef> GetRoleDefs() { yield break; }

        public virtual bool IsValidRoom(Room room) { return false; }
        //public virtual WorkerRole GetRole(WorkerRoleDef roleDef) { return null; }
        public bool HasWorker(Actor actor)
        {
            return this.Workers.Contains(actor.RefID);
        }
        internal IEnumerable<Actor> GetWorkers()
        {
            foreach (var actor in this.Workers)
                yield return this.Town.Net.GetNetworkObject(actor) as Actor;
        }
       
        private void ToggleJob(Actor actor, JobDef role)
        {
            this.GetWorkerProps(actor).GetJob(role).Toggle();
        }
        public WorkerProps GetWorkerProps(Actor a)
        {
            var aID = a.RefID;
            return this.WorkerProps[aID];
            if(!this.WorkerProps.TryGetValue(aID, out var props))
            {
                props = new WorkerProps(a, this.GetRoleDefs().ToArray());
                this.WorkerProps.Add(aID, props);
            }
            return props;
        }
        
        public Job GetWorkerJob(Actor a, JobDef j)
        {
            return this.GetWorkerProps(a).GetJob(j);
        }
        internal void AddWorker(Actor actor)
        {
            if (this.HasWorker(actor))
            {
                this.RemoveWorker(actor);
                return;
            }
            this.Town.ShopManager.GetShop<Shop>(actor)?.RemoveWorker(actor);
            this.Workers.Add(actor.RefID);
            this.WorkerProps.Add(actor.RefID, new WorkerProps(actor, this.GetRoleDefs().ToArray()));
            this.Town.Net.EventOccured(Components.Message.Types.ShopUpdated, this, new[] { actor });
        }
        void InitWorkerProps()
        {
            this.WorkerProps.Clear();
            foreach (var worker in this.Workers)
                this.WorkerProps.Add(worker, new WorkerProps(worker, this.GetRoleDefs().ToArray()));
        }
        internal void RoomChanged(Room room)
        {
            if (!this.IsValidRoom(room))
            {
                if (room.Workplace != this)
                    throw new Exception();
                //this.Rooms.Remove(room);
                room.SetWorkplace(null);
            }
        }

        internal void RemoveWorker(Actor actor)
        {
            this.Workers.Remove(actor.RefID);
            this.WorkerProps.Remove(actor.RefID);
            this.Town.Net.EventOccured(Components.Message.Types.ShopUpdated, this, new[] { actor });
        }
        public IEnumerable<Room> GetRooms()
        {
            var manager = this.Town.RoomManager;
            return this.Rooms.Select(manager.GetRoom);
        }
        internal void RemoveRoom(Room room)
        {
            this.Rooms.Remove(room.ID);
        }

        internal void AddRoom(Room room)
        {
            this.Rooms.Add(room.ID);
        }
        public bool HasStockpile(int stockpileID)
        {
            return this.Stockpiles.Contains(stockpileID);
        }

        public virtual CraftOrderNew GetOrder(int orderID)
        {
            return null;
        }

        public virtual bool IsValid() { return true; }
        public virtual bool IsAllowed(Block block) { return false; }
        public virtual AITask GetTask(Actor actor)
        {
            foreach (var role in this.GetWorkerProps(actor).Jobs.Values.Where(j => j.Enabled))
                foreach (var taskGiver in role.Def.GetTaskGivers())
                    if (taskGiver.FindTask(actor) is TaskGiverResult result)
                        return result.Task;
            return null; 
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            w.Write(this.Name);
            w.Write(this.Stockpiles);
            w.Write(this.Workers);
            w.Write(this.Rooms);
            w.Write(this.Counter);
            //this.WorkerProps.Sync(w);
            this.WorkerProps.Values.Write(w);
            this.WriteExtra(w);
        }
        protected virtual void WriteExtra(BinaryWriter w) { }
        public ISerializable Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Name = r.ReadString();
            this.Stockpiles.Read(r);
            this.Workers.Read(r);
            this.Rooms.Read(r);
            this.Counter = r.ReadVector3Nullable();
            //this.WorkerProps.Sync(r);
            this.WorkerProps = r.ReadList<WorkerProps>().ToDictionary(w => w.ActorID, w => w);
            this.ReadExtra(r);
            return this;
        }
        protected virtual void ReadExtra(BinaryReader r) { }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ID.Save("ID"));
            this.Name.Save(tag, "Name");
            tag.Add(this.Stockpiles.Save("Stockpiles"));
            tag.Add(this.Workers.Save("Workers"));
            tag.Add(this.Rooms.Save("Rooms"));
            this.WorkerProps.Values.SaveNewBEST(tag, "WorkerProps");

            if (this.Counter.HasValue)
                tag.Add(this.Counter.Value.Save("Counter"));
            this.SaveExtra(tag);
            return tag;
        }
        protected virtual void SaveExtra(SaveTag tag) { }
        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValue("ID", out this.ID);
            if (!tag.TryGetTagValue("Name", out this.Name))
                this.Name = this.DefaultName;
            this.Stockpiles.Load(tag, "Stockpiles");
            this.Workers.Load(tag, "Workers");
            //this.Rooms.Load(tag, "Rooms");
            this.Rooms.TryLoad(tag, "Rooms");
            //this.WorkerProps = tag.LoadList<WorkerProps>("WorkerProps").ToDictionary(i => i.ActorID, i => i);
            if (!tag.TryGetTag("WorkerProps", v => this.WorkerProps = v.LoadList<WorkerProps>().ToDictionary(i => i.ActorID, i => i)))
                this.InitWorkerProps();
            //tag.TryGetTagValue<Microsoft.Xna.Framework.Vector3>("Counter", v => this.Counter = v);
            tag.TryGetTagValue<Vector3>("Counter", v => this.Counter = v);
            this.LoadExtra(tag);
            return this;
        }
        protected virtual void LoadExtra(SaveTag tag) { }
        static public (Control control, Action<Workplace> refresh) CreateUI()
        {
            var box = new GroupBox();
            int listw = 200, listh = 300;
            var liststockpiles = new ListBoxNew<Stockpile, Button>(listw, listh, i => new Button(i.Name));
            
            var listfacilities = new ListBoxNew<TargetArgs, Button>(listw, listh,
                t => new Button(t.GetBlock().Name,
                    () =>
                    {
                        UISelectedInfo.Refresh(t);
                        global::Start_a_Town_.Rooms.Ingame.Instance.Camera.CenterOn(t.Global);
                    }));
           

            string nameGetter() => box.Tag is Workplace wp ? $" {wp.Name}" : "";
            var boxstockpiles = liststockpiles.ToPanelLabeled(() => $"Stockpiles{nameGetter()}");
            //var boxworkers = listworkers.ToPanelLabeled(() => $"Workers{nameGetter()}");
            var boxfacilities = listfacilities.ToPanelLabeled(() => $"Facilities{nameGetter()}");

          
            //var btnCounter = new Button("Counter", () => { });
            var btnCounter = new Button()
            {
                TextFunc = () =>
                {
                    var text = "Counter:";
                    if (box.Tag is Shop shop)
                        text += " " + (shop.Counter.HasValue ? shop.Counter.Value.ToString() : "null");
                    return text;
                }
            };
            var boxtabs = new GroupBox();
            var boxLists = new GroupBox();

            boxtabs.AddControlsLineWrap(new[] {//(UIHelper.Wrap(new[] {
                new Button("Stockpiles", ()=>selectTab(boxstockpiles)),
                new Button("Facilities", ()=>selectTab(boxfacilities)) },
                listw);
                //.ToPanel();

            void selectTab(Control tab)
            {
                boxLists.ClearControls();
                boxLists.AddControls(tab);
                tab.Validate(true);
            }

            boxLists.AddControlsHorizontally(boxstockpiles);//, boxworkers, boxfacilities);// listfacilities.ToPanelLabeled("Facilities"));

            //var boxbuttons = new GroupBox();
            //boxbuttons.AddControlsVertically(btnworkers);//, btnCounter);

            box.AddControlsVertically(boxtabs, boxLists);//, boxbuttons);
          
            void refresh(Workplace shop)
            //var refresh = new Action<Workplace>(shop =>
            {
                if (box.Tag?.GetType() != shop.GetType())
                    boxLists.ClearControls();
                
                liststockpiles.Clear().AddItems(shop.Stockpiles.Select(i => shop.Town.StockpileManager.GetStockpile(i)));//.ToArray());
                //listworkers.Clear().AddItems(shop.Workers.Select(i => shop.Town.Net.GetNetworkObject(i) as Actor));//.ToArray());
                listfacilities.Clear().AddItems(shop.GetFacilities().Select(f => new TargetArgs(shop.Town.Map, f)));

                boxtabs.ClearControls();
                boxtabs.AddControlsLineWrap(new[] {//(UIHelper.Wrap(new[] {
                new Button("Stockpiles", ()=>selectTab(boxstockpiles)),
                //new Button("Workers", ()=>selectTab(boxworkers)),
                new Button("Facilities", ()=>selectTab(boxfacilities)) },
                listw);
                boxtabs.AddControlsLineWrap(shop.GetUIBase().Select(b =>
                {
                    //b.GetData(shop);
                    //var bpanel = b.ToPanelLabeled(b.Name);
                    return new Button(b.Name, () =>
                    {
                        b.GetData(shop);
                        selectTab(b);
                        //selectTab(bpanel);
                    });
                }), listw);
            
                box.ClearControls();
                
                box.AddControlsVertically(boxtabs, boxLists);//, boxbuttons);
                box.Validate(true);
                box.Tag = shop;
                //workersUI.Value.Tag = shop;
            }//);
            //return refresh;
            liststockpiles.OnGameEventAction = e =>
            {
                var shop = box.Tag as Shop;
                switch (e.Type)
                {
                    case Components.Message.Types.ShopUpdated:
                        if (e.Parameters[0] != shop)
                            break;
                        if (e.Parameters[1] is Stockpile[] p)
                        {
                            for (int i = 0; i < p.Length; i++)
                            {
                                var st = p[i];
                                if (shop.Stockpiles.Contains(st.ID))
                                    liststockpiles.AddItems(st);
                                else
                                    liststockpiles.RemoveItems(st);
                            }
                        }
                        break;
                    default:
                        break;
                }
            };
            
            return (box, refresh);
        }

        
       
        IEnumerable<GroupBox> GetUIBase()
        {
            yield return WorkersUI ??= GetWorkersUI();
            foreach (var b in this.GetUI())
                yield return b;
        }

        protected virtual IEnumerable<GroupBox> GetUI() { yield break; }

        internal virtual void AddFacility(IntVec3 global)
        {

        }
        public virtual IEnumerable<IntVec3> GetFacilities() { yield break; }
        static Control CreateWorkersUI(Workplace shop)
        {
            var town = shop.Town;
            var manager = town.ShopManager;
            //var listworkers = new ListBoxNew<Actor, ButtonNew>(200, UIManager.LargeButton.Height * 7, (a, l) => a.GetButton(l.Client.Width, () => a.Town.ShopManager.FindShop(a)?.Name, () => { }));
            var listworkers = new ListBoxNew<Actor, ButtonNew>(
                 200, UIManager.LargeButton.Height * 7,
                 (a, l) =>
                 a.GetButton(l.Client.Width,
                 //() => a.Workplace?.Name ?? "",
                 () => a.Workplace != null ? $"Assigned to {a.Workplace.Name}" : "",

                 () => manager.ToggleWorker(a, shop)
                 ));
            listworkers.AddItems(town.GetAgents()
                //.Where(a => manager.GetShop(a) != shop)
                );
            return listworkers;
        }

        static GroupBox WorkersUI;
        static GroupBox GetWorkersUI()
        {
          
            var box = new GroupBox() { Name = "Workers" };
            Workplace tav = null;
            var table = new TableScrollableCompactNewNew<Actor>(16, true);//.Build();

            var btnworkers = new Button("Assign Workers", () =>
            {
                CreateWorkersUI(tav).ToContextMenu("Assign workers").SnapToMouse().Toggle();
            });
            var boxContainer = new GroupBox();
            boxContainer.AddControlsVertically(btnworkers, table);

            var tablePanel = boxContainer.ToPanelLabeled("Workers");
            table.OnGameEventAction = e =>
            {
                switch(e.Type)
                {
                    case Components.Message.Types.ShopUpdated:
                        if (e.Parameters[0] != tav)
                            break;
                        if (e.Parameters[1] is Actor[] actors)
                        {
                            for (int i = 0; i < actors.Length; i++)
                            {
                                var actor = actors[i];
                                if (tav.Workers.Contains(actor.RefID))
                                    table.AddItems(actor);
                                else
                                    table.RemoveItems(actor);
                            }
                        }
                        break;

                    default:
                        break;
                }
            };
            box.SetGetDataAction(o =>
            {
                tav = o as Workplace;// as Tavern;
                table.Clear();
                table.AddColumn(new(), "", 128, a => new Label(a.Name), 0);
                //foreach (var role in tav.GetRoles())
                //    table.AddColumn(role, role.Def.Label, 32, a => new CheckBoxNew()
                //    {
                //        TickedFunc = () => role.Contains(a),
                //        LeftClickAction = () => Packets.UpdateWorkerRoles(tav.Net, tav.Net.GetPlayer(), tav, role.Def, new[] { a })
                //    });
                foreach (var role in tav.GetRoleDefs())
                {
                    table.AddColumn(role, role.Label, 32, a =>
                    {
                        //var j = tav.WorkerProps[a.InstanceID].Jobs[role];
                        var j = tav.GetWorkerJob(a, role);// .Jobs[role];

                        return new CheckBoxNew()
                        {
                            TickedFunc = () => j.Enabled,
                            LeftClickAction = () => Packets.UpdateWorkerRoles(tav.Net, tav.Net.GetPlayer(), tav, role, a)
                        };
                    });
                }
                table.AddItems(tav.Workers.Select(tav.Net.GetNetworkObject).Cast<Actor>());
                //table.AddItems(this.Town.GetAgents()); // TODO add event handler for new/removed citizens
            });

           

            box.AddControlsVertically(
                //btnworkers, 
                tablePanel);
            //WorkersUI = box;
            return box;
        }

        internal virtual void OnBlocksChanged(IEnumerable<IntVec3> positions) { }

        internal void ResolveReferences()
        {
            if (this.Counter.HasValue)
                if (this.Town.Map.GetBlock(this.Counter.Value) is not BlockShopCounter)
                    this.Counter = null;
            this.Rooms.RemoveWhere(rID => !this.Town.RoomManager.TryGetRoom(rID, out _));
            this.ResolveExtraReferences();
        }
        protected virtual void ResolveExtraReferences() { }
        public bool ActorHasJob(Actor a, JobDef def)
        {
            if (!this.WorkerProps.TryGetValue(a.RefID, out var wprops))
                return false;
            return wprops.Jobs[def].Enabled;
        }
    }
}
