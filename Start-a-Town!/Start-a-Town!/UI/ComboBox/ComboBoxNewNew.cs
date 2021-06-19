using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.UI
{
    class ComboBoxNewNew<T> : GroupBox where T : class
    {
        readonly Button Button;
        readonly ButtonList<T> List;
        readonly ListBoxNew<T, Button> ListControl;
        readonly Func<T> CurrentlySelectedGetter;
        readonly Func<IEnumerable<T>> ItemsGetter;
        public ComboBoxNewNew(IEnumerable<T> list, int width, Func<T, string> nameGetter, Action<T> callBack, Func<T> currentlySelectedGetter)
        {
            this.CurrentlySelectedGetter = currentlySelectedGetter;
            this.Button = new Button(() => this.CurrentlySelectedGetter != null ? nameGetter(this.CurrentlySelectedGetter()) : "undefined", BtnPress, width);

            //this.List = new ButtonList<T>(list, width, nameGetter, (t, btn) =>
            //{
            //    btn.TextFunc = () => nameGetter(t);
            //    btn.LeftClickAction = () => { this.List.Hide(); callBack(t); };
            //}); 
            var maxVisibleItems = list.Count();
            var height = maxVisibleItems * Button.DefaultHeight;

            this.ListControl = new ListBoxNew<T, Button>(width, height, i => new Button(nameGetter(i), () => onSelect(i)), ScrollableBoxNew.ScrollModes.None)
                .AddItems(list);
            this.ListControl.ToPanel()
                .HideOnAnyClick();// as ListBoxNew<T, Button>;

            this.Controls.Add(this.Button);

            void onSelect(T i)
            {
                callBack(i);
                this.ListControl.Hide();
            }
        }
        public ComboBoxNewNew(IEnumerable<T> list, int width, Func<T, string> labelGetter, Func<T, string> listNameGetter, Action<T> callBack, Func<T> currentlySelectedGetter)
        {
            this.CurrentlySelectedGetter = currentlySelectedGetter;
            this.Button = new Button(()=>this.CurrentlySelectedGetter != null ? labelGetter(this.CurrentlySelectedGetter()) : "undefined", BtnPress, width);
            //this.List = new ButtonList<T>(list, width, listNameGetter, (t, btn) =>
            //{
            //    btn.TextFunc = () => listNameGetter(t);
            //    btn.LeftClickAction = () => { this.List.Hide(); callBack(t); };
            //});
            var maxVisibleItems = list.Count();
            var height = maxVisibleItems * Button.DefaultHeight;
            this.ListControl = new ListBoxNew<T, Button>(width, height, i => new Button(listNameGetter(i), () => onSelect(i)), ScrollableBoxNew.ScrollModes.None)
                .AddItems(list);
            this.ListControl.ToPanel()
                .HideOnAnyClick();// as ListBoxNew<T, Button>;

            this.Controls.Add(this.Button);

            void onSelect(T i)
            {
                callBack(i);
                this.ListControl.Hide();
            }
        }
        public ComboBoxNewNew(int width, Func<T, string> labelGetter, Func<T, string> listNameGetter, Action<T> callBack, Func<T> currentlySelectedGetter)
        {
            this.CurrentlySelectedGetter = currentlySelectedGetter;
            this.Button = new Button(() => this.CurrentlySelectedGetter != null ? labelGetter(this.CurrentlySelectedGetter()) : "undefined", BtnPress, width);
            var height = Button.DefaultHeight;
            this.ListControl = new ListBoxNew<T, Button>(width, height, i => new Button(listNameGetter(i), () => onSelect(i)), ScrollableBoxNew.ScrollModes.None)
             ;
            this.ListControl.ToPanel()
                .HideOnAnyClick();// as ListBoxNew<T, Button>;

            this.Controls.Add(this.Button);

            void onSelect(T i)
            {
                callBack(i);
                this.ListControl.TopLevelControl.Hide();
            }
        }
        public ComboBoxNewNew(int width, string label, Func<T, string> listNameGetter, Action<T> callBack, Func<T> currentlySelectedGetter, Func<IEnumerable<T>> itemsGetter)
        {
            this.CurrentlySelectedGetter = currentlySelectedGetter;
            this.ItemsGetter = itemsGetter;
            this.Button = new Button(() =>
                //this.CurrentlySelectedGetter != null ? labelGetter(this.CurrentlySelectedGetter()) : "undefined", BtnPress, width);
                $"{label}: {(this.CurrentlySelectedGetter() is T item ? listNameGetter(item) : "none")}", BtnPress, width);

            var height = Button.DefaultHeight;
            this.ListControl = new ListBoxNew<T, Button>(width, height, i => new Button(listNameGetter(i), () => onSelect(i)), ScrollableBoxNew.ScrollModes.None)
             ;
            this.ListControl.ToPanel()
                .HideOnAnyClick();// as ListBoxNew<T, Button>;

            this.Controls.Add(this.Button);

            void onSelect(T i)
            {
                callBack(i);
                this.ListControl.TopLevelControl.Hide();
            }
        }
        private void BtnPress()
        {
            if (this.ItemsGetter is not null)
                this.Initialize(this.ItemsGetter());
            var panel = this.ListControl.TopLevelControl;
            panel.Location = UIManager.Mouse;
            panel.Show();

            //this.ListControl.Location = UIManager.Mouse;
            //this.ListControl.Show();

        }

        //public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if (!this.List.HitTest() && this.List.IsOpen)
        //        this.List.Hide();
        //    base.HandleLButtonDown(e);
        //}
        //public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    this.List.Hide();
        //    base.HandleRButtonDown(e);
        //}
        public ComboBoxNewNew<T> Initialize(IEnumerable<T> items)
        {
            var count = items.Count();
            this.ListControl.Height = count * Button.DefaultHeight;
            this.ListControl.Remeasure();
            this.ListControl.Clear();
            this.ListControl.AddItems(items);
            return this;
        }
    }
}
