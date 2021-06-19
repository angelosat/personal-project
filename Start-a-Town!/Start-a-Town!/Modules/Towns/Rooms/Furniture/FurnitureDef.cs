using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class FurnitureDef : Def
    {
        //public HashSet<RoomDef> Rooms = new();

        public FurnitureDef(string label) : base($"Furniture{label}")
        {

        }

        //public FurnitureDef AddRoom(params RoomDef[] rooms)
        //{
        //    foreach (var r in rooms)
        //        this.Rooms.Add(r);
        //    return this;
        //}
    }
}
