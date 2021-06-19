using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class HelpWindow : Window
    {
        static HelpWindow _Instance;
        static public HelpWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new HelpWindow();
                return _Instance;
            }
        }

        public HelpWindow()
        {
            Title = "Help";
            AutoSize = true;

            Panel panel = new Panel();
            panel.AutoSize = true;
            Label text = new Label(text:
                "Left click to walk" +
            "\nRight click to interact" +
            "\nControl + Right click to dig" +
            "\nMousewheel to zoom in/out" +
            "\nControl + Mousewheel to raise/lower draw level" +
            "\nZ, X to rotate the map" +
            "\nH to hide walls and trees"
            );

            panel.Controls.Add(text);

            Controls.Add(panel);
            //Location = CenterScreen;
            this.SnapToScreenCenter();

        }
    }
}
