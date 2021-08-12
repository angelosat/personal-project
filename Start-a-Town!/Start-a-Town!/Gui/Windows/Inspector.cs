using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    static class Inspector
    {
        static Inspector()
        {
        }
        static Control DefDirectory;
        static int MaxHeight = 512, Width = 384; // table columns TODO
        static Window WindowHelp;
        readonly static List<Control> History = new();
        static int HistoryIndex = -1;
        static IconButton BtnBack, BtnForward;
        static Control Container;

        public static void Refresh(Inspectable obj)
        {
            if (WindowHelp is null)
            {
                WindowHelp = new Window() { Title = "Details", Movable = true, Closable = true, AutoSize = true }.MoveToScreenCenter() as Window;
                var toolbar = new GroupBox();
                BtnBack = IconButton.CreateSmall(Icon.ArrowLeft, Back);
                BtnForward = IconButton.CreateSmall(Icon.ArrowRight, Forward);
                toolbar.AddControlsHorizontally(BtnBack, BtnForward, new Button("Defs", () => ShowContainer(DefDirectory)));
                Container = new GroupBox();
                WindowHelp.AddControlsVertically(toolbar, Container);
                WindowHelp.HideAction = () =>
                {
                    History.Clear();
                    HistoryIndex = -1;
                };
                InitDefDirectory();
            }
            Container.ClearControls();
            var table = new Table<(string item, object value)>()
                //.AddColumn("name", 128, i => new Label(i.item), 1)
                //.AddColumn("name", 96, i => new GroupBox().AddControlsLineWrap(96, new Label(i.item), new Label(": ")), 1)
                .AddColumn("name", Width / 3, i => new Label(i.item + ": "), 1)
                .AddColumn("value", 2*Width/3, i => new GroupBox().AddControlsLineWrap(256, Label.ParseNewNew(i.value).ToArray()));

            table.AddItems(obj.Inspect()
                .Prepend((nameof(obj.Label), obj.Label))
                .Prepend((nameof(Type), obj.GetType())));
            table.Tag = obj;
            var box = table.Box(table.Width, MaxHeight);
            var panel = box.ToPanel();

            if (HistoryIndex < History.Count - 1)
                History.RemoveRange(HistoryIndex + 1, History.Count - HistoryIndex - 1);
            History.Add(panel);
            HistoryIndex++;

            Container.AddControls(panel);
            BtnBack.Active = HistoryIndex > 0;
            BtnForward.Active = HistoryIndex < History.Count - 1;
            WindowHelp.Validate(true);
            Show();
        }
        private static void Back()
        {
            var prev = History[--HistoryIndex];
            Container.ClearControls();
            Container.AddControls(prev);
            BtnBack.Active = HistoryIndex > 0;
            BtnForward.Active = HistoryIndex < History.Count - 1;
            WindowHelp.Validate(true);
        }
        private static void Forward()
        {
            var next = History[++HistoryIndex];
            Container.ClearControls();
            Container.AddControls(next);
            BtnBack.Active = HistoryIndex > 0;
            BtnForward.Active = HistoryIndex < History.Count - 1;
            WindowHelp.Validate(true);
        }
        static void ShowContainer(Control container)
        {
            if (Container.Controls.Contains(container))
                return;
            Container.ClearControls();
            Container.AddControls(container);
            if (HistoryIndex < History.Count - 1)
                History.RemoveRange(HistoryIndex + 1, History.Count - HistoryIndex - 1);
            History.Add(container);
            HistoryIndex++;
            BtnBack.Active = HistoryIndex > 0;
            BtnForward.Active = HistoryIndex < History.Count - 1;
        } 
        internal static void Show()
        {
            if (!WindowHelp.IsOpen)
                WindowHelp.Show();
        }
        static void InitDefDirectory()
        {
            var list = new ListBoxNoScroll<Def>(d => new Label(d.Name, () => Refresh(d)));
            list.AddItems(Def.Database.Values.OrderBy(d => d.Name));
            var search = new SearchBarNew<Def>(Width, id => id.Name).BindTo(list).ToPanelLabeled("Search");
            var scrollablebox = new ScrollableBoxNewNew(Width, MaxHeight - search.Height, ScrollModes.Vertical);
            scrollablebox.AddControls(list);
            var box = new GroupBox().AddControlsVertically(search, scrollablebox.ToPanel());
            DefDirectory = box;
        }
    }
}
