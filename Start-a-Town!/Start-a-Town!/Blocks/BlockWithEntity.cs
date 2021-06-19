using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    abstract class BlockWithEntity : Block
    {
        protected BlockWithEntity(Types type, float transparency = 0, float density = 1, bool opaque = true, bool solid = true) : base(type, transparency, density, opaque, solid)
        {
        }
        public override void OnDrop(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            actor.Map.GetBlockEntity(target.Global).OnDrop(actor, dropped, target, amount);
        }
        //public override void OnRemove(IMap map, Vector3 global)
        //{
        //    map.GetBlockEntity(global).OnRemove(map, global);
        //}
        //internal override void DrawSelected(MySpriteBatch sb, Camera cam, IMap map, Vector3 global)
        //{
        //    var e = map.GetBlockEntity(global);
        //    foreach (var c in e.Comps)
        //        c.DrawSelected(sb, cam, map, global);
        //}
    }
}
