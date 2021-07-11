using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class ToolDigging : ToolDesignate3D
    {
        public ToolDigging()
        {

        }
        public override Icon GetIcon()
        {
            return Icon.Construction;
        }
        public ToolDigging(Action<IntVec3, IntVec3, bool> callback)
        {
            this.Callback = callback;
        }
        public override void UpdateRemote(TargetArgs target)
        {
            if(target.Type == TargetType.Position)
            this.End = target.Global;
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
            if (!this.Enabled)
                return;
            var positions = this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air);
            camera.DrawGridBlocks(sb, Block.BlockBlueprint, positions, Color.White);
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, PlayerData player)
        {
            if (!this.Enabled)
                return;
            var positions = this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air);
            camera.DrawGridBlocks(sb, Block.BlockBlueprint, positions, Color.Red);
        }
    }
}
