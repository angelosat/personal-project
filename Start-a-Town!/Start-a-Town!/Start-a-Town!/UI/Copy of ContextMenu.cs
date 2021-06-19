using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ContextMenu : Control
    {
        ScrollableList<Interaction, Button> Panel_List;

        static ContextMenu _Instance;
        public static ContextMenu Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ContextMenu();
                return _Instance;
            }
        }

        public Interaction SelectedItem;
        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }

        public event EventHandler ContextMenuInit;
        void OnContextMenuInit()
        {
            if (ContextMenuInit != null)
                ContextMenuInit(this, EventArgs.Empty);
        }

       

        ContextMenu()
        {
            Panel_List = new ScrollableList<Interaction, Button>(Vector2.Zero, new Rectangle(0, 0, 100, 0), foo => foo.Name);
            Panel_List.SelectedItemChanged += new EventHandler<EventArgs>(Panel_List_SelectedItemChanged);
            Panel_List.AutoSize = true;
          //  Controls.Add(Panel_List);
            AutoSize = true;
        }

        void Panel_List_SelectedItemChanged(object sender, EventArgs e)
        {
            SelectedItem = Panel_List.SelectedItem;
            OnSelectedItemChanged();
        }

        public ContextMenu Initialize(GameObject actor, Mouseover mouseover)
        {
            this.Tag = mouseover;
            return Initialize(actor, mouseover.Object as GameObject);
        }

        public ContextMenu Initialize(GameObject actor, GameObject obj)
        {
            Controls.Clear();
            Panel_List.SelectedItemChanged -= Panel_List_SelectedItemChanged;
         //   Panel_List.Controls.Clear();
            List<Interaction> interactions = new List<Interaction>();
          //  obj.HandleMessage(Message.Types.Query, actor, interactions);
            obj.Query(actor, UpdateActions);
            return this;
        }

        void UpdateActions(List<Interaction> interactions)
        {
            if (interactions.Count == 0)
                return;
            //foreach(Interaction i in interactions)
            Panel_List = new ScrollableList<Interaction, Button>(Vector2.Zero, new Rectangle(0, 0, 100, 100), foo => foo.Name);
            Panel_List.SelectedItemChanged += new EventHandler<EventArgs>(Panel_List_SelectedItemChanged);
            Panel_List.AutoSize = true;
            Panel_List.Build(interactions);
            Controls.Add(Panel_List);
            Location = UIManager.Mouse - ClientLocation - Panel_List.Location - (Panel_List.Controls.Count > 0 ? Panel_List.Controls.First().Location : Vector2.Zero) - new Vector2(Label.DefaultHeight / 2);
            Show();
            OnContextMenuInit();
      //      return this;
        }


    }
}
