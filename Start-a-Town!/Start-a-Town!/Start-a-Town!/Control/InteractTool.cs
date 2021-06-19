using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Rooms;

using Start_a_Town_.Components;

namespace Start_a_Town_.PlayerControl
{
    public class InteractTool : ControlTool
    {
        public InteractTool()
        {

        }

        public override Messages OnMouseLeft(bool held)
        {
            Console.WriteLine("interact left");
            return Messages.Default;
        }

        public override Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            Console.WriteLine("interact right");
            return Messages.Default;
        }
    }
}
