using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    static class FurnitureDefOf
    {
        static public readonly FurnitureDef Bed = new FurnitureDef("Bed");//.AddRoom(RoomDefOf.Bedroom);
        static public readonly FurnitureDef Table = new FurnitureDef("Table");//.AddRoom(RoomDefOf.Dining);
        static public readonly FurnitureDef Counter = new FurnitureDef("Counter");//.AddRoom(RoomDefOf.Dining);
    }
}
