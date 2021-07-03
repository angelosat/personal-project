using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.PlayerControl
{
    class EmptyTool : ControlTool
    {
        public Func<TargetArgs, ControlTool.Messages>
            LeftClick = (target) => { return ControlTool.Messages.Default; },
            RightClick = (target) => { return ControlTool.Messages.Remove; };
        public Func<KeyEventArgs, ControlTool.Messages>
            KeyUp = (e) => { return ControlTool.Messages.Default; };
        public Action<SpriteBatch, Camera> DrawAction = (sb, cam) => { };
        public Action<MySpriteBatch, Camera> DrawActionMy = (sb, cam) => { };

        public override ControlTool.Messages MouseLeftUp(HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            if(this.Target == null)
                return Messages.Default;
            if (Controller.Instance.MouseoverBlock.Target.Global != Target.Global)
                return Messages.Default;
            return this.Target != null ? LeftClick(this.Target) : Messages.Default;
        }
        
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return RightClick(this.Target);
        }
        
        public override void HandleKeyUp(KeyEventArgs e)
        {
            KeyUp(e);
        }

        internal override void DrawWorld(SpriteBatch sb, IMap map, Camera camera)
        {
            base.DrawWorld(sb, map, camera);
            DrawAction(sb, camera);
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            base.DrawBeforeWorld(sb, map, camera);
            DrawActionMy(sb, camera);
        }
    }
}
