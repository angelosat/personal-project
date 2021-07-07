using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ComboBoxPanel<T> : Panel where T : class
    {
        public ListBox<T, Button> List;
        public ComboBoxPanel(ListBox<T, Button> List, Action<T> selectedItemChanged)
        {
            this.AutoSize = true;
            this.List = List;
            this.List.ItemChangedFunc = selectedItemChanged;
            this.List.BackgroundColor = Color.Black;
            this.AddControls(this.List);
        }
        public ComboBoxPanel(IEnumerable<T> list, int w, int h , Func<T, string> textFunc, Action<T, Button> ctrlFunc, Action<T> selectedItemChanged)
        {
            this.AutoSize = true;
            this.List = new ListBox<T, Button>(w, h);
            this.List.Build(list, textFunc, ctrlFunc);
            this.List.ItemChangedFunc = selectedItemChanged;
            this.List.BackgroundColor = Color.Black;
            this.AddControls(this.List);
        }
        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.HandleLButtonDown(e);
            if (e.Handled)
                return;
                this.Hide();
            e.Handled = true;
        }
        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.HandleLButtonDown(e);
            if (e.Handled)
                return;
            if (!this.HitTest())
                this.Hide();
        }
    }
}
