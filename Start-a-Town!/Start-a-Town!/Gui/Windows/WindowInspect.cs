using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class WindowInspect
    {
        static Window WindowHelp;

        public static void Refresh(IInspectable obj)
        {
            if (WindowHelp is null)
                WindowHelp = new Window() { Title = "Details", Movable = true, Closable = true, AutoSize = true }.MoveToScreenCenter() as Window;
            WindowHelp.Client.ClearControls();
            var table = new Table<(object item, object value)>()
                .AddColumn("name", 128, i => new Label(i.item))
                .AddColumn("value", 128, i => new Label(i.value));
            table.AddItems(obj.Inspect());
            var box = table.Box(table.Width, 512);
            WindowHelp.Client.AddControls(box);
        }

        internal static void Show()
        {
            if (!WindowHelp.IsOpen)
                WindowHelp.Show();
        }
    }
}
