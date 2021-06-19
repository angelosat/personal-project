using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Tasks;

namespace Start_a_Town_.Control
{
    class DefaultTool : ITool
    {
        public void Execute()
        {
            //if (Controller.MouseOverObject == null)
                Player.Instance.TaskAssign(new Task(Player.Instance, new TaskArgs(Controller.MouseOverTile)));
            //else
            //    if (Controller.MouseOverObject.IsInteractive)
            //        Player.Instance.TaskAssign(new Task(Player.Instance, new TaskArgs(Controller.MouseOverObject.CurrentTile, Controller.MouseOverObject, Controller.MouseOverObject.GetInteractions()[0])));
        }
    }
}
