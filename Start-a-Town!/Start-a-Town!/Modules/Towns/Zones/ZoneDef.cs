﻿using System;

namespace Start_a_Town_
{
    public abstract class ZoneDef
    {
        public abstract Type ZoneType { get; }
        public abstract bool IsValidLocation(IMap map, IntVec3 global);

        public ZoneNew Create()
        {
            var zone = Activator.CreateInstance(this.ZoneType) as ZoneNew;
            return zone;
        }
        public virtual void OnBlockChanged(IntVec3 global) { }
        public void OnBlockChanged(ZoneNew zone, IntVec3 global)
        {
            var map = zone.Map;
            var below = global.Below;
            if (zone.Positions.Contains(global) && !Block.IsBlockSolid(map, global))
            {
                zone.RemovePosition(global);
                return;
            }
            else if (zone.Positions.Contains(below) && !map.IsAir(global))
            {
                zone.RemovePosition(below);
                return;
            }
        }
    }
}
