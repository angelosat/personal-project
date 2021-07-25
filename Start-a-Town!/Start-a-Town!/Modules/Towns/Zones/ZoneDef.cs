using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public abstract class ZoneDef : Def
    {
        public readonly string Label;
        public abstract Type ZoneType { get; }
        public abstract bool IsValidLocation(MapBase map, IntVec3 global);
        public ZoneDef(string name)
            : base($"Zone{name}")
        {
            this.Label = name;
        }
        public Zone Create()
        {
            var zone = Activator.CreateInstance(this.ZoneType) as Zone;
            return zone;
        }
        public virtual void OnBlockChanged(IntVec3 global) { }
        public void OnBlockChanged(Zone zone, IntVec3 global)
        {
            var map = zone.Map;
            var below = global.Below;
            if (zone.Contains(global) && !Block.IsBlockSolid(map, global))
            {
                zone.RemovePosition(global);
                return;
            }
            else if (zone.Contains(below) && !map.IsAir(global))
            {
                zone.RemovePosition(below);
                return;
            }
        }

        public Zone Create(ZoneManager manager, IEnumerable<IntVec3> positions)
        {
            return Activator.CreateInstance(this.ZoneType, manager, positions) as Zone;
        }
    }
}
