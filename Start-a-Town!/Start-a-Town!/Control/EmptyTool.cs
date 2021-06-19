using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.PlayerControl
{
    class EmptyTool : ControlTool
    {
        //public Func<ControlTool.Messages>
        //    LeftClick = () => { return ControlTool.Messages.Default; },
        //    RightClick = () => { return ControlTool.Messages.Remove; };
        //public Func<GameObject, Vector3, ControlTool.Messages>
        //    LeftClick = (target, face) => { return ControlTool.Messages.Default; },
        //    RightClick = (target, face) => { return ControlTool.Messages.Remove; };
        public Func<TargetArgs, ControlTool.Messages>
            LeftClick = (target) => { return ControlTool.Messages.Default; },
            RightClick = (target) => { return ControlTool.Messages.Remove; };
        public Func<KeyEventArgs, ControlTool.Messages>
            KeyUp = (e) => { return ControlTool.Messages.Default; };
        public Action<SpriteBatch, Camera> DrawAction = (sb, cam) => { };
        public Action<MySpriteBatch, Camera> DrawActionMy = (sb, cam) => { };

        //public override void Update()
        //{
        //    if (InputState.IsKeyDown(Keys.LButton))
        //        return;
        //    Target = Controller.Instance.Mouseover.Object as GameObject;
        //    Face = Controller.Instance.Mouseover.Face;
        //}
        public override ControlTool.Messages MouseLeftUp(HandledMouseEventArgs e)
        {
            //if (Controller.Instance.Mouseover.Object != TargetOld)
            //    return Messages.Default;
            //return !TargetOld.IsNull() ? LeftClick(TargetOld, Face) : Messages.Default;
            if (e.Handled)
                return Messages.Default;
            //if (Controller.Instance.Mouseover.Target != this.Target)
            //    return Messages.Default;
            if(this.Target == null)
                return Messages.Default;
            if (Controller.Instance.MouseoverBlock.Target.Global != Target.Global)
                return Messages.Default;
            return this.Target != null ? LeftClick(this.Target) : Messages.Default;
        }
        //public override ControlTool.Messages MouseLeftPressed(HandledMouseEventArgs e)
        //{
        //    return !Target.IsNull() ? LeftClick() : Messages.Default;
        //}
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //return RightClick(TargetOld, Face);
            return RightClick(this.Target);
        }
        //public override void HandleKeyPress(KeyPressEventArgs e)
        //{
        //    KeyPress(e);
        //}
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

        //public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    LeftClick();
        //}
        //public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    RightClick();
        //}
    }
}
