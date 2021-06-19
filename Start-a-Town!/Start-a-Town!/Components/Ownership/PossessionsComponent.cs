using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class PossessionsComponent : EntityComponent
    {
        public override string ComponentName => "Possesions";
        readonly HashSet<Room> Rooms = new();
        public override object Clone()
        {
            return new PossessionsComponent();
        }
        public bool Owns(Room room)
        {
            return this.Rooms.Contains(room);
        }
        internal bool Has(RoomRoleDef roomDef)
        {
            return this.Rooms.Any(r => r.RoomRole == roomDef);
        }

        public void Claim(Room room)
        {
            if (this.Rooms.Contains(room))
                throw new Exception();
            //room.TimeClaimed = this.Parent.NetNew.Clock;
            this.Rooms.Add(room);
            room.AddOwner(this.Parent as Actor);
        }
        public void Unclaim(Room room)
        {
            if (!this.Rooms.Remove(room))
                throw new Exception();
            room.RemoveOwner(this.Parent as Actor);
        }
    }
}
