using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Towns;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ZoneManager : TownComponent
    {
        public override string Name => "ZoneManager";
        int _ZoneIDSequence = 1;

        public int GetNextID() => _ZoneIDSequence++;
        readonly public Dictionary<int, ZoneNew> Zones = new();

        public ZoneManager(Town town)
        {
            this.Town = town;
        }

        internal ZoneNew RegisterNewZone(Type zoneType, IEnumerable<IntVec3> allpositions)
        {
            if (!allpositions.IsConnectedNew())
                return null;
            var finalPositions = allpositions.Where(
                po => this.Town.GetZoneAt(po) == null &&
                ZoneNew.IsPositionValid(this.Town.Map, po));
            //var stockpile = new Stockpile(this.Town, finalPositions);
            var zone = Activator.CreateInstance(zoneType, this, finalPositions) as ZoneNew;
            this.RegisterZone(zone);
            return zone;
        }

        internal void Delete(ZoneNew zone)
        {
            this.Delete(zone.ID);
            //if (!this.Zones.ContainsKey(zone.ID))
            //    throw new Exception();
            //this.Zones.Remove(zone.ID);
            //FloatingText.Create(this.Map, zone.Positions.Average(), $"{zone.GetType()} deleted", ft => ft.Font = UIManager.FontBold);
        }
        internal void Delete(int zoneID)
        {
            if (!this.Zones.TryGetValue(zoneID, out var zone))
                throw new Exception();
            this.Zones.Remove(zoneID);
            FloatingText.Create(this.Map, zone.Positions.Average(), $"{zone.GetType()} deleted", ft => ft.Font = UIManager.FontBold);
        }
        void RegisterZone(ZoneNew zone)
        {
            if (zone.ID == 0)
                zone.ID = this.GetNextID();
            //farm.Town = this.Town;
            this.Zones.Add(zone.ID, zone);
            zone.Manager = this;
            zone.Name = zone.UniqueName;
            //this.Town.Map.EventOccured(Components.Message.Types.FarmCreated, farm);
            //if (this.Town.Map.Net is Client)
                //FloatingText.Manager.Create(() => farm.Positions.First(), "Farm created", ft => ft.Font = UIManager.FontBold);
            FloatingText.Create(this.Town.Map, zone.Positions.Average(), $"{zone.GetType()} created", ft => ft.Font = UIManager.FontBold);

            //this.Town.Map.EventOccured(Components.Message.Types.FarmCreated, farm);
            //if (this.Town.Map.Net is Client)
            //    FloatingText.Manager.Create(() => farm.Positions.First(), "Farm created", ft => ft.Font = UIManager.FontBold);
        }

        internal static void Init()
        {
            ZoneDefOf.Init();
        }

        internal T GetZone<T>(int zoneID) where T : ZoneNew
        {
            return this.Zones[zoneID] as T;
        }

        public ZoneNew GetZoneAt(IntVec3 global)
        {
            return this.Zones.Values.FirstOrDefault(z => z.Contains(global));
        }
        public T GetZoneAt<T>(IntVec3 global) where T : ZoneNew
        {
            return this.Zones.Values.FirstOrDefault(z => z.Contains(global)) as T;
        }
        public IEnumerable<T> GetZones<T>() where T : ZoneNew
        {
            return this.Zones.Values.OfType<T>();
        }
        public IEnumerable<ZoneNew> GetZones()
        {
            foreach (var z in this.Zones.Values)
                yield return z;
        }
        internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
        {
            //foreach (var s in this.GetZones())
            //    foreach (var pos in positions)
            //        s.OnBlockChanged(pos);
            //foreach (var pos in positions)
            //{

            //}
            for (int i = this.Zones.Count - 1; i >= 0; i--)
            {
                var item = this.Zones.ElementAt(i);
                //foreach (var pos in positions)
                //    item.Value.OnBlockChanged(pos);
                foreach (var pos in positions)
                {
                    item.Value.OnBlockChangedNew(pos);
                    item.Value.OnBlockChangedNew(pos.Below);
                }
            }
        }
        internal ZoneNew PlayerEdit(int zoneID, Type zoneType, IntVec3 a, int w, int h, bool remove)
        {
            if (remove)
            {
                foreach (var zone in this.Zones.Values.ToList())
                    zone.Edit(a, a + new IntVec3(w - 1, h - 1, 0), remove);
            }
            else
            {
                if (zoneID == 0)
                    return RegisterNewZone(zoneType, a.GetBoxLazy(a + new IntVec3(w - 1, h - 1, 0)));
                else
                    this.Zones[zoneID].Edit(a, a + new IntVec3(w - 1, h - 1, 0), remove);
            }
            return null;
        }
        static Type[] ZoneTypes = { typeof(Stockpile), typeof(GrowingZone) }; // TODO make these defs

        //public bool IsValidHaulDestination(IntVec3 global, GameObject item)
        //{
        //    var zoneBelow = this.GetZoneAt(global.Below);
        //    if(zoneBelow)
        //}

        internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        {
            //yield return new Tuple<string, Action>("Stockpile", () => ZoneNew.Edit(typeof(Stockpile)));
            //yield return new Tuple<string, Action>("Stockpile", () => ZoneNew.Edit(typeof(Stockpile)));
            foreach(var zoneType in ZoneTypes)
                yield return new Tuple<string, Action>(zoneType.Name, () => ZoneNew.Edit(zoneType));
        }
        //public override IContextable QueryPosition(Vector3 global)
        //{
        //    return this.GetZoneAt(global);
        //}
        public override ISelectable QuerySelectable(TargetArgs target)
        {
            var global = target.Global;
            return this.GetZoneAt(global);
        }
        public override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            if (!cam.DrawZones)
                return;
            foreach (var s in this.Zones.Values)
                s.DrawBeforeWorld(sb, map, cam);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            this._ZoneIDSequence.Save(tag, "IDSequence");
            this.Zones.Values.SaveVariableTypes(tag, "Zones");
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValueNew<int>("IDSequence", ref this._ZoneIDSequence);
            this.Zones.TryLoadByValueAbstractTypes(tag, "Zones", zone => zone.ID, this);
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this._ZoneIDSequence);
            this.Zones.Values.WriteAbstract(w);
        }
        public override void Read(BinaryReader r)
        {
            this._ZoneIDSequence = r.ReadInt32();
            this.Zones.ReadByValueAbstractTypes(r, zone => zone.ID, this);
        }
    }
}
