using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    static public class UIHelper
    {
        static public Control ToContextMenu(this Control control, string title)
        {
            return control.ToPanelLabeled(title).SetLocation(UIManager.Mouse).HideOnAnyClick();
        }
        static public Control ToContextMenuClosable(this Control control, string title)
        {
            return control.ToWindow(title, movable: false).SetLocation(UIManager.Mouse).HideOnAnyClick();
        }
        static public Control ToContextMenuClosable(this Control control, string title, Vector2 screenLoc)
        {
            return control.ToWindow(title, movable: false).SetLocation(screenLoc).HideOnAnyClick();
        }
    }
}
