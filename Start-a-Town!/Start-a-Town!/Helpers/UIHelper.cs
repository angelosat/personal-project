using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    static class UIHelper
    {
        static public Panel ToPanel(this Control ctrl)
        {
            var panel = new Panel() { AutoSize = true };
            panel.AddControls(ctrl);
            return panel;
        }
        static public PanelLabeledNew ToPanelLabeled(this Control ctrl)
        {
            var panel = new PanelLabeledNew(ctrl.Name) { AutoSize = true };
            panel.Client.AddControls(ctrl);
            return panel;
        }
        static public PanelLabeledNew ToPanelLabeled(this Control ctrl, string label)
        {
            var panel = new PanelLabeledNew(label) { AutoSize = true };
            panel.Client.AddControls(ctrl);
            return panel;
        }
        static public PanelLabeledNew ToPanelLabeled(this Control ctrl, Func<string> label)
        {
            var panel = new PanelLabeledNew(label) { AutoSize = true };
            panel.Client.AddControls(ctrl);
            return panel;
        }
        static public PanelTitled ToPanelTitled(this Control ctrl, string label)
        {
            var panel = new PanelTitled(label) { AutoSize = true };
            panel.Client.AddControls(ctrl);
            return panel;
        }

        static public Panel ToPanel(this IGui obj)
        {
            var panel = new Panel() { AutoSize = true };
            panel.AddControls(obj.GetControl());
            return panel;
        }
        static public PanelLabeledNew ToPanelLabeled(this IGui obj, string label)
        {
            var panel = new PanelLabeledNew(label) { AutoSize = true };
            panel.Client.AddControls(obj.GetControl());
            return panel;
        }
        static public PanelTitled ToPanelTitled(this IGui obj, string label)
        {
            var panel = new PanelTitled(label) { AutoSize = true };
            panel.Client.AddControls(obj.GetControl());
            return panel;
        }

        static public Window ToWindow(this IGui obj, string label)
        {
            return obj.GetControl().ToWindow(label);
        }

        static public Control HideOnRightClick(this Control control)
        {
            control.MouseRBAction = () => { if (!control.ContainsMouse()) control.Hide(); };
            return control;
        }
        static public Control HideOnLeftClick(this Control control)
        {
            control.MouseLBAction = () => { if (!control.ContainsMouse()) control.Hide(); };
            return control;
        }
        static public Control HideOnAnyClick(this Control control)
        {
            control.HideOnLeftClick();
            control.HideOnRightClick();
            return control;
        }
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
        static public GroupBox Wrap(IEnumerable<ButtonBase> labels, int width = int.MaxValue)
        {
            var box = new GroupBox();
            //var pos = IntVec2.Zero;
            var currentX = 0;
            var currentY = 0;
            foreach (var l in labels)
            {
                if (currentX + l.Width > width)
                {
                    currentX = 0;
                    currentY += l.Height;
                }
                l.Location = new IntVec2(currentX, currentY);
                currentX += l.Width;
                box.AddControls(l);
            }
            if (width != int.MaxValue)
                box.Width = width;
            return box;
        }

        static public Control ToTabbedContainer(params Control[] namedControls)
        {
            var box = new GroupBox();

            var labeledPanels = namedControls.Select(c =>
            {
                if (c.Name.IsNullEmptyOrWhiteSpace())
                    throw new ArgumentException($"Control's \"Name\" field is null empty or whitespace");
                return  c.ToPanelLabeled(c.Name); 
            });

            var maxw = labeledPanels.Max(c => c.Width);
            var maxh = labeledPanels.Max(c => c.Height);

            //var boxtabs = new GroupBox();
            var boxclient = new GroupBox(maxw, maxh);

            //boxtabs.AddControlsLineWrap(namedControls.Select(c => new Button(c.Name, () => select(c))), maxw);
            //boxtabs.AddControlsLineWrap(namedControls.Select(c => new Button(c.Name, () => select(c))), maxw);
            var boxtabs = Wrap(labeledPanels.Select(c => new Button(c.Name, () => select(c))), maxw);

            void select(Control c)
            {
                boxclient.ClearControls();
                boxclient.AddControls(c);
            }

            box.AddControlsVertically(boxtabs, boxclient);
            return box;
        }
    }
}
