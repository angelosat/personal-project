using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public override UI.Icon GetIcon()
        {
            return Icon.Construction;
        }
        public ToolDigging(Action<Vector3, Vector3, bool> callback)
        {
            this.Callback = callback;
        }
        public override void UpdateRemote(TargetArgs target)
        {
            if(target.Type == TargetType.Position)
            this.End = target.Global;
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            if (!this.Enabled)
                return;
            var positions = this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air);
            camera.DrawGridBlocks(sb, Block.BlockBlueprint, positions, Color.White);
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, IMap map, Camera camera, PlayerData player)
        {
            if (!this.Enabled)
                return;
            var positions = this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air);
            camera.DrawGridBlocks(sb, Block.BlockBlueprint, positions, Color.Red);
        }
    }
}
