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
            var list = new ListBoxNew<GameObject, Label>(200, 400);
            foreach (var obj in items)
                list.AddItem(obj.Value, obj.Value.Name, () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ObjectSpawnToolFromTemplate(obj.Value, obj.Key));

            this.Client.AddControlsBottomLeft(list.ToPanelLabeled("Templates"));
            this.Client.AddControlsBottomLeft(new SearchBar<GameObject>(200).BindTo(list).ToPanelLabeled("Search"));
        }
    }
}
