﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ContextMenu2 : Panel
    {
        ListBox<ContextAction, Button> Panel_List;

        static ContextMenu2 _Instance;
        public static ContextMenu2 Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ContextMenu2();
                return _Instance;
            }
        }

        public event EventHandler ContextMenuInit;
        void OnContextMenuInit()
        {
            if (ContextMenuInit != null)
                ContextMenuInit(this, EventArgs.Empty);
        }

        ContextMenu2()
        {
            AutoSize = true;
            Panel_List = new ListBox<ContextAction, Button>(new Rectangle(0, 0, 100, 5));
            Panel_List.SelectedItemChanged += new EventHandler<EventArgs>(Panel_List_SelectedItemChanged);
            Panel_List.AutoSize = true;           
        }

        void Panel_List_SelectedItemChanged(object sender, EventArgs e)
        {
            //  TODO: make the controls inactive instead
            if (Panel_List.SelectedItem.Action())
                Hide();
        }

        public void Initialize(GameObject obj, ContextArgs a)
        {
    }
        public void Initialize(ContextArgs a)//List<ContextAction> actions)
        {
            List<ContextAction> actions = a.Actions;
            if (actions.Count == 0)
                return;

            Panel_List.AutoSize = true;
            Panel_List.Build(actions, foo => foo.Name(), (ContextAction act, Button btn) =>
            {
                //btn.Color = Color.Black;
                btn.Color = Color.White * 0.5f;
                btn.IdleColor = Color.Transparent;
                act.ControlInit(act, btn);
                //btn.BackgroundColor = Color.Black; 
               // btn.TextColorFunc = () => Color.Black;
                a.ControlInit(btn);
            });
            this.Controls.Add(Panel_List);
            Location = UIManager.Mouse - ClientLocation - Panel_List.Location - (Panel_List.Controls.Count > 0 ? Panel_List.Controls.First().Location : Vector2.Zero) - new Vector2(Label.DefaultHeight / 2);
            Show();
            Invalidate();
            Texture = null;
            OnContextMenuInit();
        }
        public void Initialize(params ContextAction[] actions)
        {
            List<ContextAction> a = actions.ToList();
            if (a.Count == 0)
                return;

            Panel_List.AutoSize = true;
            Panel_List.Build(a, foo => foo.Name(), (ContextAction act, Button btn) =>
            {
                btn.Color = Color.White * 0.5f;
                btn.IdleColor = Color.Transparent;
                act.ControlInit(act, btn);
            });
            this.Controls.Add(Panel_List);
            Location = UIManager.Mouse - ClientLocation - Panel_List.Location - (Panel_List.Controls.Count > 0 ? Panel_List.Controls.First().Location : Vector2.Zero) - new Vector2(Label.DefaultHeight / 2);
            Show();
            Invalidate();
            Texture = null;
            OnContextMenuInit();
        }
    }
}
