using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class RoomRoleDef : Def
    {
        public string Label;
        public readonly HashSet<FurnitureDef> Furniture = new();
        public RoomRoleDef(string label) : base($"Room{label}")
        {
            this.Label = label;
        }
        public RoomRoleDef AddFurniture(params FurnitureDef[] furniture)
        {
            foreach (var f in furniture)
                this.Furniture.Add(f);
            return this;
        }
        static public IEnumerable<RoomRoleDef> ByFurniture(FurnitureDef furn)
        {
            return RoomRoleDefOf.All.Where(r => r.Furniture.Contains(furn));
        }
    }
}
