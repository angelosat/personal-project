using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.UI
{
    class ThoughtsWindow : Window
    {
        static Dictionary<GameObject, ThoughtsWindow> _OpenThoughts;
        static Dictionary<GameObject, ThoughtsWindow> OpenThoughts
        {
            get
            {
                if (_OpenThoughts == null)
                    _OpenThoughts = new Dictionary<GameObject, ThoughtsWindow>();
                return _OpenThoughts;
            }
        }

        //PanelList<Thought> List_Thoughts;
        Panel Panel_List;
        ListBox<Thought, Label> List_Thoughts;

        ThoughtsWindow()
        {
            this.Title = "Thoughts";
            //this.Size = new Rectangle(0, 0, 300, 500);
            this.AutoSize = true;
            this.Movable = true;
            Panel_List = new Panel() { AutoSize = true, Color = Color.Black }; 
            List_Thoughts = new ListBox<Thought, Label>(new Rectangle(0, 0, 200, 300));
            Panel_List.Controls.Add(List_Thoughts);
          //  List_Thoughts = new PanelList<Thought>(Vector2.Zero, new Vector2(300, 500), foo => foo.Title);
           // List_Thoughts.HoverFunc = (foo => foo.Time.ToString("MMM dd, HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-GB")) + "\n" + foo.Text);
        }

        ThoughtsWindow Refresh(GameObject npc)
        {
            Client.Controls.Remove(Panel_List);
            //List_Thoughts.Build();
            //List_Thoughts.Build((npc["AI"]["Thoughts"] as ThoughtCollection).OrderByDescending(foo => foo.Time));
            this.List_Thoughts.Build((npc["AI"]["Thoughts"] as ThoughtCollection).OrderByDescending(foo => foo.Time), foo => foo.Title, onControlInit: (t, l) => 
            {
                l.HoverFunc = () => t.Time.ToString() + "\n" + t.Text;
            });
            Client.Controls.Add(Panel_List);
            return this;
        }

        static public ThoughtsWindow Show(GameObject obj)
        {
            ThoughtsWindow win;
            if (!OpenThoughts.TryGetValue(obj, out win))
            {
                win = new ThoughtsWindow().Refresh(obj);
                win.Tag = obj;
                OpenThoughts[obj] = win;
            }
            win.Location = win.CenterScreen;
            win.Show();
            return win;
        }

        public override bool Hide()
        {
            OpenThoughts.Remove(this.Tag as GameObject);
            return base.Hide();
        }
        public override bool Close()
        {
            OpenThoughts.Remove(this.Tag as GameObject);
            return base.Close();
        }
    }
}
