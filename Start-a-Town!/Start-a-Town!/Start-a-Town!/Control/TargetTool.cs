using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.PlayerControl
{
    class TargetTool : ControlTool
    {

        void Controller_KeyPress(object sender, KeyEventArgs2 e)
        {
            if (e.KeysNew.Contains(Microsoft.Xna.Framework.Input.Keys.D))
                DebugWindow.Instance.Toggle();
                //Game1.Instance.CurrentRoom.WindowManager.ToggleSingletonWindow<DebugWindow>();
        }


        public event EventHandler<EventArgs> Destroy;

        public void OnDestroy()
        {
            if (Destroy != null)
                Destroy(this, EventArgs.Empty);
        }

        public override Messages OnMouseLeft(bool held)
        {
            //Player.Actor.GetComponent<Components.TasksComponent>("Tasks").Target(Map.GetSelection());
            return Messages.Remove;
        }

        //public WorldEntity CreateTempObject(Cell cell)
        //{
        //    TempEntity temp = new TempEntity();
        //    return temp;
        //}

        public override Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            NotificationArea.Write("Task cancelled.");
            //Player.Actor.Interrupt();
            return Messages.Remove;
        }
    }
}
