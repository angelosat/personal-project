using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.PlayerControl;

namespace Start_a_Town_.UI.Editor
{
    class BldgLoadWindow : Window
    {
        static bool Open;
        //#region Singleton
        //static BldgLoadWindow _Instance;
        //public static BldgLoadWindow Instance
        //{
        //    get
        //    {
        //        if (_Instance.IsNull())
        //            _Instance = new BldgLoadWindow();
        //        return _Instance;
        //    }
        //}
        //#endregion

        Button Btn_Load;
        //public Action LoadAction = () => { };
       // public ListBox<FileInfo, Button> List;
        public BldgLoadWindow(Action<FileInfo> loadAction)
        {
            Title = "Load";
            AutoSize = true;
            Movable = true;
            string dir = GlobalVars.SaveDir + @"\Maps\";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            FileInfo[] mapFiles = dirInfo.GetFiles();

            Panel panel_list = new Panel() { AutoSize = true };
            ListBox<FileInfo, Button> list = new ListBox<FileInfo, Button>(new Rectangle(0, 0, 200, 300));// { ItemChangedFunc = () => { } };
            list.Build(mapFiles, foo => foo.Name);

            panel_list.Controls.Add(list);

            Panel panel_btns = new Panel() { Location = panel_list.BottomLeft, AutoSize = true };
            Btn_Load = new Button()
            {
                Text = "Load",
                Width = list.Width,// panel_list.Width,
                LeftClickAction = () =>
                {
                    loadAction(list.SelectedItem);
                    //Bldg bldg = new Bldg().Load(listbox.SelectedItem);
                    //EmptyTool tool = new EmptyTool();
                    //tool.LeftClick = () =>
                    //{
                    //    bldg.Apply(Engine.Map, tool.Target.Global);
                    //    return ControlTool.Messages.Default;
                    //};
                    //ScreenManager.CurrentScreen.ToolManager.ActiveTool = tool;
                    Hide();
                }
            };

            panel_btns.Controls.Add(Btn_Load);
            Client.Controls.Add(panel_list, panel_btns);
            Location = CenterScreen;
        }

        public override bool Show(params object[] p)
        {
            if (Open)
                return false;
            Open = true;
            return base.Show(p);
        }
        public override bool Hide()
        {
            Open = false;
            return base.Hide();
        }
    }
}
