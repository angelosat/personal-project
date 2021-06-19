using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.PlayerControl
{
    class CameraMover
    {
        Vector2 Origin;

        private void MouseScroll(Camera cam)
        {
            var currentMouse = UIManager.Mouse;
            var delta = currentMouse - this.Origin;
            var l = delta.Length();
            if (l < 5)
                return;
            l -= 5;
            delta.Normalize();
            var minL = Math.Min(Math.Max(l, 1), 50);
            delta *= minL;

            delta *= .1f;

            cam.Move(cam.Coordinates += delta * 4);
        }
        private void MouseDrag(Camera cam)
        {

        }
    }
}
