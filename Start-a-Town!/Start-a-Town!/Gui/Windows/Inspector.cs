using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    static class Inspector
    {
        static Window WindowHelp;
        readonly static List<Control> History = new();
        static int HistoryIndex = -1;
        static IconButton BtnBack, BtnForward;
        static Control Container;
        
        public static void Refresh(IInspectable obj)
        {
            if (WindowHelp is null)
            {
                WindowHelp = new Window() { Title = "Details", Movable = true, Closable = true, AutoSize = true }.MoveToScreenCenter() as Window;
                var toolbar = new GroupBox();
                BtnBack = IconButton.CreateSmall(Icon.ArrowLeft, Back);
                BtnForward = IconButton.CreateSmall(Icon.ArrowRight, Forward);
                toolbar.AddControlsHorizontally(BtnBack, BtnForward);
                Container = new GroupBox();
                WindowHelp.AddControlsVertically(toolbar, Container);
                WindowHelp.HideAction = () =>
                {
                    History.Clear();
                    HistoryIndex = -1;
                };
            }
            Container.ClearControls();
            var table = new Table<(object item, object value)>()
                .AddColumn("name", 128, i => new Label(i.item))
                //.AddColumn("value", 128, i => new Label(i.value) { HoverText = i.value.ToString() });
                .AddColumn("value", 128, i => new GroupBox().AddControlsLineWrap(128, Label.ParseNewNew(i.value).ToArray()));// { HoverText = i.value.ToString() });

            table.AddItems(obj.Inspect()
                .Prepend((nameof(obj.Label), obj.Label))
                .Prepend((nameof(Type), obj.GetType())));
            table.Tag = obj;
            var box = table.Box(table.Width, 512);
            var panel = box.ToPanel();

            if(HistoryIndex < History.Count -1)
                History.RemoveRange(HistoryIndex + 1, History.Count - HistoryIndex - 1);
            History.Add(panel);
            HistoryIndex++;

            Container.AddControls(panel);
            BtnBack.Active = HistoryIndex > 0;
            BtnForward.Active = HistoryIndex < History.Count - 1;
            WindowHelp.Validate(true);
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
        internal static void Show()
        {
            if (!WindowHelp.IsOpen)
                WindowHelp.Show();
        }
    }
}
