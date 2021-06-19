using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Components;

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

        List<Slot> AllSlots = new List<Slot>();

        //ButtonGridIcons<ItemDef> Grid;
        //ListBoxNew<GameObject, Label> List;
        //ButtonList<KeyValuePair<int, GameObject>> List;

        readonly Panel 
            //Panel_Search, Panel_Filters, 
            Panel_Items;
        //TextBox Txt_Search;
        const int MaxSlotsPerLine = 8;
        //void Load()
        //{
        //    //var defs = Def.Database.Values.Where(d => d is ItemDef).Select(d => d as ItemDef).ToList();
        //    var items = GameObject.Templates.Values;
        //    this.List = new ListBoxNew<GameObject, Label>(400, 400);
        //    this.List.Build(items, n => n.Name, (item, label) => label.LeftClickAction = () => item.Name.ToConsole());
        //    this.AddControlsBottomLeft(this.List);
        //    return;

        //    Panel_Filters = new Panel() { AutoSize = true };
        //    Panel_Filters.Controls.Add(new RadioButton("All", Vector2.Zero, true) { LeftClickAction = () => Refresh(GameObject.Objects.Values) });
        //    //  int i = Panel_Filters.Bottom;

        //    List<string> types = new List<string>();
        //    GameObject.Objects.Values.ToList().ForEach(foo => { if (!types.Contains(foo.Type)) types.Add(foo.Type); });

        //    foreach (var type in types)//new List<string>() { ObjectType.Block })
        //    {
        //        RadioButton rd = new RadioButton(type, Panel_Filters.Controls.Last().BottomLeft) { Tag = type };
        //        //   i += rd.Height;
        //        rd.LeftClickAction = () =>
        //        {
        //            Refresh(GameObject.Objects.Values.Where(obj => obj.Type == (string)rd.Tag));
        //        };
        //        Panel_Filters.Controls.Add(rd);
        //    }

        //    Txt_Search = new TextBox()
        //    {
        //        Width = Slot.DefaultHeight * 8,// (int)Panel_Items.ClientDimensions.X,
        //        TextChangedFunc = (text) => Refresh(text)// Refresh(GameObject.Objects.Values.ToList().FindAll(foo => foo.Name.ToLower().Contains(text.ToLower())))
        //    };
        //    Panel_Search = new Panel() { Location = Panel_Filters.TopRight, AutoSize = true };
        //    Panel_Search.Controls.Add(Txt_Search);

        //}



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

            //this.List.Build(items, n => n.Name, (item, label) => label.LeftClickAction = () =>
            //{
            //    ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ObjectSpawnTool(item);// tool;
            //}
            //);
        }
        void Arrange(IEnumerable<Slot> slots)
        {
            Panel_Items.Controls.Clear();
            int i = 0, j = 0, n = 0;
            foreach (var slot in slots)
            {
                slot.Location = new Vector2(i * Slot.DefaultHeight, j * Slot.DefaultHeight);
                n++;
                i = n % MaxSlotsPerLine;
                j = n / MaxSlotsPerLine;
                Panel_Items.Controls.Add(slot);
            }
        }
        private void Refresh(string text)
        {
            var validSlots = this.AllSlots.FindAll(foo => foo.Tag.Object.Name.ToLower().Contains(text.ToLower()));
            this.Arrange(validSlots);
        }
        private void Refresh(IEnumerable<GameObject> collection)
        {
            this.AllSlots.Clear();
            Panel_Items.Controls.Clear();
            int i = 0, j = 0, n = 0;
            foreach (var obj in collection)
            {
                Slot slot = new Slot()
                {
                    Location = new Vector2(i * Slot.DefaultHeight, j * Slot.DefaultHeight),
                    Tag = obj.ToSlotLink()
                };
                this.AllSlots.Add(slot);

                n++;
                i = n % MaxSlotsPerLine;
                j = n / MaxSlotsPerLine;
                Panel_Items.Controls.Add(slot);
                slot.LeftClickAction = () =>
                {
                    GameObjectSlot newSlot = slot.Tag;
                    DragDropManager.Create(new DragDropSlot(null, null, new TargetArgs(newSlot), DragDropEffects.None));
                    ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ObjectSpawnTool(slot.Tag.Object);// tool;
                };
            }
        }
    }
}
