using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class TestWindow : Window
    {
        static TestWindow _Instance;
        static public TestWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new TestWindow();
                return _Instance;
            }
        }

        Panel Panel_Main;
        ScrollableBox Box;
        Button Btn;

        TestWindow()
        {
            this.Title = "Test";
            this.AutoSize = true;
            this.Client.AutoSize = true;
            Movable = true;

            Panel_Main = new Panel(new Rectangle(0, 0, 300, 300));

            Box = new ScrollableBox(Panel_Main.ClientSize);// { Size = Panel_Main.ClientSize };
            Btn = new Button(new Vector2(250, 50), 100, "Test");

            Box.Add(
                Btn
                ,"test".ToLabel(new Vector2(50, 300), 100)
                );
            Panel_Main.Controls.Add(Box);

            this.Client.Controls.Add(Panel_Main);

            this.Controls.Remove(Client);
            this.Controls.Add(Client);

            //Location = CenterScreen;
            this.SnapToScreenCenter();

        }
    }
}
