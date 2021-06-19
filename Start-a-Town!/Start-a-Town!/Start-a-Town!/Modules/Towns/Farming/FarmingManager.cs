using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Farming
{
    public class FarmingManager : TownComponent
    {
        public override string Name
        {
            get
            {
                return "Farming";
            }
        }

        int IDSequence = 1;
        public Dictionary<int, Farmland> Farmlands = new Dictionary<int, Farmland>();

        const float UpdateFrequency = 1; // per second
        float UpdateTimerMax = (float)Engine.TargetFps / UpdateFrequency;
        float UpdateTimer;

        //public Town Town;
        public FarmingManager(Town town)
        {
            this.Town = town;
        }

        public override void Update()
        {
            if (this.Town.Map.GetNetwork() is Net.Client)
                return;
            if (this.UpdateTimer > 0)
            {
                this.UpdateTimer--;
                return;
            }
            this.UpdateTimer = UpdateTimerMax;
            this.GenerateWork();
        }

        private void GenerateWork()
        {
            foreach (var farm in this.Farmlands)
                farm.Value.GenerateWork();
        }

        public Farmland GetFarmAt(Vector3 global)
        {
            foreach (var farm in this.Farmlands.Values)
            {
                if (farm.Positions.Contains(global))
                    return farm;
                //var box = new BoundingBox(farm.Begin, farm.End);
                //var type = box.Contains(global);
                //if (type != ContainmentType.Disjoint)
                //{
                //    return farm;
                //}
            }
            return null;
        }
        public override void Handle(IObjectProvider net, Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.FarmlandDesignate:
                    msg.Payload.Deserialize(r =>
                    {
                        int entityid, farmid;
                        Vector3 begin, end;
                        bool value;
                        PacketDesignate.Read(r, out entityid, out farmid, out begin, out end, out value);
                        this.Farmlands[farmid].Edit(begin.GetBox(end), value);
                        var server = net as Server;
                        if (server != null)
                            server.Enqueue(msg.PacketType, msg.Payload);
                    });
                    break;


                case PacketType.FarmSync:
                    msg.Payload.Deserialize(r =>
                    {
                        int id, seedID;
                        bool harvesting, planting;
                        string name;
                        PacketFarmSync.Read(r, out id, out name, out seedID, out harvesting, out planting);
                        var farm = this.Farmlands[id];
                        farm.SeedType = seedID == -1 ? null : GameObject.Objects[seedID];
                        farm.SetHarvesting(harvesting);
                        farm.Planting = planting;
                        farm.Name = name;
                        this.Town.Net.EventOccured(Components.Message.Types.FarmUpdated, farm);
                        var server = net as Server;
                        if (server != null)
                            server.Enqueue(msg.PacketType, msg.Payload);
                    });
                    break;


                default:
                    break;
            }
        }

        public override void HandlePacket(Net.Server server, Net.Packet msg)
        {
            switch(msg.PacketType)
            {
                case Net.PacketType.FarmCreate:
                    msg.Payload.Deserialize(r =>
                        {
                            var p = new PacketCreateFarmland(server, r);
                            var farm = new Farmland(IDSequence++, p.Begin, p.Width, p.Height);
                            if (farm.Positions.Count == 0)
                                return;
                            if (p.Remove)
                                this.RemovePositions(p.Begin, p.Width, p.Height);
                            else
                                this.AddFarmland(farm);
                            var newpacket = new PacketCreateFarmland(p.EntityID, farm.ID, p.Begin, p.Width, p.Height, p.Remove);
                            server.Enqueue(PacketType.FarmCreate, newpacket.Write(), SendType.OrderedReliable, true);
                        });
                    break;

                case Net.PacketType.FarmDelete:
                    break;

                case PacketType.FarmSetSeed:
                    msg.Payload.Deserialize(r =>
                        {
                            PacketFarmSetSeed.Handle(server, msg, this);
                        });
                    break;

                case PacketType.FarmlandDesignate:
                case PacketType.FarmSync:

                    this.Handle(server, msg);
                    break;


                default:
                    break;

            }
        }
        public override void HandlePacket(Net.Client client, Net.Packet msg)
        {
            switch (msg.PacketType)
            {
                case Net.PacketType.FarmCreate:
                    msg.Payload.Deserialize(r =>
                    {
                        var p = new PacketCreateFarmland(client, r);
                        var farm = new Farmland(p.FarmlandID, p.Begin, p.Width, p.Height);
                        if (farm.Positions.Count == 0)
                            return;
                        if (p.Remove)
                            this.RemovePositions(p.Begin, p.Width, p.Height);
                        else
                            this.AddFarmland(farm);
                    });
                    break;

                case Net.PacketType.FarmDelete:
                    break;

                case PacketType.FarmSetSeed:
                    msg.Payload.Deserialize(r =>
                    {
                        PacketFarmSetSeed.Handle(client, msg, this);
                    });
                    break;

                case PacketType.FarmlandDesignate:
                case PacketType.FarmSync:
                    this.Handle(client, msg);
                    break;


                default:
                    break;

            }
        }

        void AddFarmland(Farmland farm)
        {
            if (farm.ID == 0)
                farm.ID = this.IDSequence++;
            farm.Town = this.Town;
            this.Farmlands.Add(farm.ID, farm);
            this.Town.Map.EventOccured(Components.Message.Types.FarmCreated, farm);
        }
        
        void RemoveFarmland(int id)
        {
            this.Farmlands.Remove(id);
        }

        void RemoveFarmland(Vector3 global, int w, int h)
        {

        }

        public override UI.GroupBox GetInterface()
        {
            return new FarmingManagerUI(this);
        }

        Window FarmWindow;
        internal override void OnContextMenuCreated(IContextable obj, ContextArgs a)
        {
            var target = obj as TargetArgs;
            if (target == null)
                return;
            if (target.Type != TargetType.Position)
                return;
            var global = target.FaceGlobal;
            var farm = this.GetFarmAt(global);
            if (farm != null)
            {
                a.Actions.Add(new ContextAction(() => "Set seed", () =>
                {
                    if (this.FarmWindow == null)
                    {
                        this.FarmWindow = new Window();
                        this.FarmWindow.Title = "Farm";
                        this.FarmWindow.AutoSize = true;
                        this.FarmWindow.Movable = true;
                    }
                    var farmui = farm.GetInterface();
                    this.FarmWindow.Client.Controls.Add(farmui);
                    this.FarmWindow.Toggle();
                    this.FarmWindow.Location = ScreenManager.CurrentScreen.Camera.GetScreenPosition(global);
                }));
            }
            //foreach(var farm in this.Farmlands.Values)
            //{
            //    var box = new BoundingBox(farm.Begin, farm.End);
            //    if (box.Contains(global) != ContainmentType.Disjoint)
            //    {
            //        a.Actions.Add(new ContextAction(() => "Set seed", () => {
            //            if(this.FarmWindow == null)
            //            {
            //                this.FarmWindow = new Window();
            //                this.FarmWindow.Title = "Farm";
            //                this.FarmWindow.AutoSize = true;
            //            }
            //            var farmui = farm.GetInterface();
            //            this.FarmWindow.Client.Controls.Add(farmui);
            //            this.FarmWindow.Toggle();
            //            this.FarmWindow.Location = ScreenManager.CurrentScreen.Camera.GetScreenPosition(global);
            //        }));
            //    }
            //}
        }

        //internal override void OnGameEvent(GameEvent e)
        //{
        //    switch(e.Type)
        //    {
        //        case Components.Message.Types.FarmCreated:
        //            var farm = e.Parameters[0] as Farmland;
        //            //FloatingText.Manager.Create(() => farm.Begin, "Farm created", ft => ft.Font = UIManager.FontBold);
        //            FloatingText.Manager.Create(() => farm.Positions.First(), "Farm created", ft => ft.Font = UIManager.FontBold);

        //            break;

        //        case Components.Message.Types.FarmSeedChanged:
        //            farm = e.Parameters[0] as Farmland;
        //            var seed = e.Parameters[1] as GameObject;
        //            //FloatingText.Manager.Create(() => farm.Begin, "Farm seed set to: " + seed.Name, ft => ft.Font = UIManager.FontBold);
        //            FloatingText.Manager.Create(() => farm.Positions.First(), "Farm seed set to: " + seed.Name, ft => ft.Font = UIManager.FontBold);

        //            break;

        //        case Components.Message.Types.PlantGrown:
        //            var plant = e.Parameters[0] as GameObject;
        //            farm = GetFarmAt(plant.Global);
        //            farm.AddPlant(plant);
        //            break;

        //        default:
        //            break;
        //    }
        //}

        public List<Vector3> GetPositions()
        {
            var list = new List<Vector3>();
            foreach (var farm in this.Farmlands)
                list.AddRange(farm.Value.Positions);
            return list;
        }


        internal void RemovePositions(Vector3 global, int w, int h)
        {
            foreach (var farm in this.Farmlands.Values.ToList())
            {
                farm.RemovePositions(global, w, h);
                if (farm.Positions.Count == 0)
                    this.Farmlands.Remove(farm.ID);
            }


            /// WHICH ONE IS FASTER??

            //for (int i = 0; i < w; i++)
            //{
            //    for (int j = 0; j < h; j++)
            //    {
            //        var pos = new Vector3(i, j, 0) + global;
            //        var farm = this.GetFarmAt(pos);
            //        if (farm != null)
            //            farm.Positions.Remove(pos);
            //    }
            //}
        }

        public bool RemoveFarm(Farmland farm)
        {
            if( this.Farmlands.Remove(farm.ID))
            {
                this.Town.Map.EventOccured(Components.Message.Types.FarmRemoved, farm);
                return true;
            }
            return false;
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.PlantGrown:
                    var plant = e.Parameters[0] as GameObject;
                    var farm = this.GetFarmAt(plant.Global);
                    farm.AddPlant(plant);
                    break;

                default:
                    break;
            }
        }

        public override List<SaveTag> Save()
        {
            List<SaveTag> save = new List<SaveTag>();
            save.Add(this.IDSequence.Save("IDSequence"));
            var farms = new SaveTag(SaveTag.Types.List, "Farms", SaveTag.Types.Compound);
            foreach (var farm in this.Farmlands)
                farms.Add(new SaveTag(SaveTag.Types.Compound, "", farm.Value.Save()));
            save.Add(farms);
            return save;
        }

        public override void Load(SaveTag tag)
        {
            //this.IDSequence = tag.GetValue<int>("IDSequence");
            tag.TryGetTagValue<int>("IDSequence", v => this.IDSequence = v);
            var list = new List<SaveTag>();
            if(tag.TryGetTagValue("Farms", out list))
                foreach(var farmtag in list)
                {
                    var farm = new Farmland(this.Town, farmtag);
                    this.Farmlands.Add(farm.ID, farm);
                }
        }

        public override void Write(BinaryWriter w)
        {
            w.Write(this.IDSequence);
            w.Write(this.Farmlands.Count);
            foreach (var farm in this.Farmlands)
                farm.Value.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            this.IDSequence = r.ReadInt32();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var farm = new Farmland(this.Town, r);
                this.Farmlands.Add(farm.ID, farm);
            }
        }
    }
}
