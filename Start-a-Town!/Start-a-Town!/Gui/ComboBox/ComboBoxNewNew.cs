using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    class ComboBoxNewNew<T> : GroupBox where T : class
    {
        readonly Button Button;
        readonly ListBoxNoScroll<T, Button> ListControl;
        readonly Func<T> CurrentlySelectedGetter;
        readonly Func<IEnumerable<T>> ItemsGetter;
        public ComboBoxNewNew(IEnumerable<T> list, int width, Func<T, string> nameGetter, Action<T> callBack, Func<T> currentlySelectedGetter)
            //: base(width, list.Count() * Button.DefaultHeight)
        {
            this.CurrentlySelectedGetter = currentlySelectedGetter;
            this.Button = new Button(() => this.CurrentlySelectedGetter != null ? nameGetter(this.CurrentlySelectedGetter()) : "undefined", BtnPress, width);
            var maxVisibleItems = list.Count();
            this.ListControl = new ListBoxNoScroll<T, Button>(i => new Button(nameGetter(i), () => onSelect(i)))
                .AddItems(list);
            this.ListControl.ToPanel()
                .HideOnAnyClick();

            this.Controls.Add(this.Button);

            void onSelect(T i)
            {
                callBack(i);
                this.ListControl.Hide();
            }
        }
        public ComboBoxNewNew(IEnumerable<T> list, int width, Func<T, string> labelGetter, Func<T, string> listNameGetter, Action<T> callBack, Func<T> currentlySelectedGetter)
            //: base(width, list.Count() * Button.DefaultHeight)
        {
            this.CurrentlySelectedGetter = currentlySelectedGetter;
            this.Button = new Button(()=>this.CurrentlySelectedGetter != null ? labelGetter(this.CurrentlySelectedGetter()) : "undefined", BtnPress, width);
            this.ListControl = new ListBoxNoScroll<T, Button>(i => new Button(listNameGetter(i), () => onSelect(i)))
                .AddItems(list);
            this.ListControl.ToPanel()
                .HideOnAnyClick();

            this.Controls.Add(this.Button);

            void onSelect(T i)
            {
                callBack(i);
                this.ListControl.Hide();
            }
        }
        public ComboBoxNewNew(int width, string label, Func<T, string> listNameGetter, Action<T> callBack, Func<T> currentlySelectedGetter, Func<IEnumerable<T>> itemsGetter)
        {
            this.CurrentlySelectedGetter = currentlySelectedGetter;
            this.ItemsGetter = itemsGetter;
            this.Button = new Button(() =>
                $"{label}: {(this.CurrentlySelectedGetter() is T item ? listNameGetter(item) : "none")}", BtnPress, width);

            this.ListControl = new ListBoxNoScroll<T, Button>(i => new Button(listNameGetter(i), () => onSelect(i)));
            this.ListControl.ToPanel()
                .HideOnAnyClick();

            this.Controls.Add(this.Button);

            void onSelect(T i)
            {
                callBack(i);
                this.ListControl.TopLevelControl.Hide();
            }
        }

        public ComboBoxNewNew(IEnumerable<T> list, int width, string label, Func<T, string> nameGetter, Func<string> currentlySelectedGetter, Action<T> callBack)
            //: base(width, list.Count() * Button.DefaultHeight)
        {
            this.Button = new Button(() =>
                $"{label}: {currentlySelectedGetter?.Invoke() ?? "undefined"}", BtnPress, width);

            this.ListControl = new ListBoxNoScroll<T, Button>(i => new Button(nameGetter(i), () => onSelect(i)))
                .AddItems(list);
            this.ListControl.ToPanel()
                .HideOnAnyClick();

            this.Controls.Add(this.Button);

            void onSelect(T i)
            {
                callBack(i);
                this.ListControl.Hide();
            }
        }
        public ComboBoxNewNew(IEnumerable<T> list, int width, string label, Func<T, string> nameGetter, Func<T> currentlySelectedGetter, Action<T> callBack)
            //: base(width, list.Count() * Button.DefaultHeight)
        {
            this.Button = new Button(() =>
                $"{label}: {(currentlySelectedGetter() is T item ? nameGetter(item) : "none")}", BtnPress, width);

            this.ListControl = new ListBoxNoScroll<T, Button>(i => new Button(nameGetter(i), () => onSelect(i)))
                .AddItems(list);
            this.ListControl.ToPanel()
                .HideOnAnyClick();

            this.Controls.Add(this.Button);

            void onSelect(T i)
            {
                callBack(i);
                this.ListControl.Hide();
            }
        }

        private void BtnPress()
        {
            if (this.ItemsGetter is not null)
                this.Initialize(this.ItemsGetter());
            var panel = this.ListControl.TopLevelControl;
            panel.Location = UIManager.Mouse;
            panel.Show();
        }
        
        public ComboBoxNewNew<T> Initialize(IEnumerable<T> items)
        {
            var count = items.Count();
            //this.ListControl.Height = count * Button.DefaultHeight;
            this.ListControl.Clear();
            this.ListControl.AddItems(items);
            return this;
        }
    }
}
