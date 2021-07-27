namespace Start_a_Town_.UI.Editor
{
    class ObjectsWindowDefs : Window
    {
        static ObjectsWindowDefs _Instance;
        public static ObjectsWindowDefs Instance => _Instance ??= new ObjectsWindowDefs();

        ObjectsWindowDefs()
        {
            Title = "Objects";
            AutoSize = true;
            Movable = true;

            var items = GameObject.Templates;
            var box = new ScrollableBoxNewNew(200, 400);
            var list = new ListBoxNoScroll<int, Label>(id =>
            {
                var obj = items[id];
                return new Label(obj.Name, () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ObjectSpawnToolFromTemplate(obj, id));
            });
            //foreach (var obj in items)
            //    list.AddItem(obj.Value, obj.Value.Name, () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ObjectSpawnToolFromTemplate(obj.Value, obj.Key));
            list.AddItems(items.Keys);
            box.AddControls(list);
            this.Client.AddControlsBottomLeft(box.ToPanelLabeled("Templates"));
            //this.Client.AddControlsBottomLeft(new SearchBar<GameObject>(200).BindTo(list).ToPanelLabeled("Search"));
            this.Client.AddControlsBottomLeft(new SearchBarNew<int>(200, id => items[id].Name).BindTo(list).ToPanelLabeled("Search"));
        }
    }
}
