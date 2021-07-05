using System.Collections.Generic;

namespace Start_a_Town_.UI.Editor
{
    class ObjectsWindowDefs : Window
    {
        static ObjectsWindowDefs _Instance;
        public static ObjectsWindowDefs Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ObjectsWindowDefs();
                return _Instance;
            }
        }

        ObjectsWindowDefs()
        {
            Title = "Objects";
            AutoSize = true;
            Movable = true;

            //Load();

            var items = GameObject.Templates;
            var list = new ListBoxNew<GameObject, Label>(200, 400);
            foreach (var obj in items)
            {
                list.AddItem(obj.Value, o => o.Name, (o, label) =>
                      {
                          label.LeftClickAction = () =>
                          {
                              ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ObjectSpawnToolFromTemplate(obj.Value, obj.Key);
                          };
                      });
            }

            this.Client.AddControlsBottomLeft(list.ToPanelLabeled("Templates"));
            this.Client.AddControlsBottomLeft(new SearchBar<GameObject>(200).BindTo(list).ToPanelLabeled("Search"));
        }
    }
}
