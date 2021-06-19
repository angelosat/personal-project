using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.UI
{
    class NeedsWindow : Window
    {
        static Dictionary<GameObject, NeedsWindow> _OpenWindows;
        static Dictionary<GameObject, NeedsWindow> OpenWindows
        {
            get
            {
                if (_OpenWindows == null)
                    _OpenWindows = new Dictionary<GameObject, NeedsWindow>();
                return _OpenWindows;
            }
        }

        NeedsWindow()
        {
            AutoSize = true;
            Movable = true;
        }

        NeedsWindow Initialize(GameObject actor)
        {
            NeedsComponent needsComp;
            if (!actor.TryGetComponent<NeedsComponent>("Needs", out needsComp))
                throw new ArgumentException("Object \"" + actor.Name + "\" doesn't have a needs component");
            this.Tag = actor;
            this.Title = actor.Name + "'s needs";
            Client.Controls.Clear();
            NeedsHierarchy needs = needsComp.NeedsHierarchy;
            foreach (var level in needs.Values)
                foreach (var need in level)
                {
                    Panel panel = new Panel() { AutoSize = true, Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero, BackgroundStyle = BackgroundStyle.TickBox };
                    panel.Controls.Add(need.Value.ToBar(actor));
                    Client.Controls.Add(panel);
                }

            Location = CenterScreen;
            return this;
        }

        //public override void Update()
        //{
        //    base.Update();
        //}

        static public NeedsWindow Show(GameObject actor)
        {
            NeedsWindow win;
            if (!OpenWindows.TryGetValue(actor, out win))
            {
                win = new NeedsWindow().Initialize(actor);
                OpenWindows[actor] = win;
            }
            win.Show();
            return win;
        }
        static public NeedsWindow Toggle(GameObject actor)
        {
            NeedsWindow win;
            if (!OpenWindows.TryGetValue(actor, out win))
            {
                win = new NeedsWindow().Initialize(actor);
                OpenWindows[actor] = win;
            }
            win.Toggle();
            return win;
        }
        public override bool Hide()
        {
            OpenWindows.Remove(this.Tag as GameObject);
            return base.Hide();
        }
    }
}
