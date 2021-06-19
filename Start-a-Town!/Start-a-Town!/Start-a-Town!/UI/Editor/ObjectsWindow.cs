﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI.Editor
{
    class ObjectsWindow : Window
    {
        static ObjectsWindow _Instance;
        public static ObjectsWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ObjectsWindow();
                return _Instance;
            }
        }

        Panel Panel_Search, Panel_Filters, Panel_Items;
        TextBox Txt_Search;

        void Load()
        {
          
            Panel_Filters = new Panel() { AutoSize = true };
            Panel_Filters.Controls.Add(new RadioButton("All", Vector2.Zero, true) { LeftClickAction = () => Refresh(GameObject.Objects.Values) });
          //  int i = Panel_Filters.Bottom;

            List<string> types = new List<string>();
            GameObject.Objects.Values.ToList().ForEach(foo => { if (!types.Contains(foo.Type)) types.Add(foo.Type); });

            foreach (var type in types)//new List<string>() { ObjectType.Block })
            {
                RadioButton rd = new RadioButton(type, Panel_Filters.Controls.Last().BottomLeft) { Tag = type };
             //   i += rd.Height;
                rd.LeftClickAction = () =>
                {
                    Refresh(GameObject.Objects.Values.Where(obj => obj.Type == (string)rd.Tag));
                };
                Panel_Filters.Controls.Add(rd);
            }

            Txt_Search = new TextBox()
            {
                Width = Slot.DefaultHeight * 8,// (int)Panel_Items.ClientDimensions.X,
                TextEnterFunc = (e) =>
                {
                    switch (e.KeyChar)
                    {
                        case '\b':
                            if (Txt_Search.Text == "")
                            {
                                Refresh(GameObject.Objects.Values);
                                return;
                            }
                            break;

                        default:
                            if (char.IsLetterOrDigit(e.KeyChar))
                                Txt_Search.Text += e.KeyChar;
                            break;
                    }
                    Refresh(GameObject.Objects.Values.ToList().FindAll(foo => foo.Name.ToLower().Contains(Txt_Search.Text.ToLower())));
                }
            };
            Panel_Search = new Panel() { Location = Panel_Filters.TopRight, AutoSize = true };
            Panel_Search.Controls.Add(Txt_Search);

        }

        ObjectsWindow()
        {
            Title = "Objects";
            AutoSize = true;
            Movable = true;

            Load();
            Panel_Items = new Panel() { Location = Panel_Search.BottomLeft, AutoSize = true };
            Refresh(GameObject.Objects.Values);

            Client.Controls.Add(Panel_Search, Panel_Filters, Panel_Items);
            Location = CenterScreen;
            Panel_Items.AutoSize = false;
            AutoSize = false;
        }

        private void Refresh(IEnumerable<GameObject> collection)
        {
            Panel_Items.Controls.Clear();
            int i = 0, j = 0, n = 0;
            foreach (var obj in collection)
            {
                Slot slot = new Slot()
                {
                    Location = new Vector2(i * Slot.DefaultHeight, j * Slot.DefaultHeight),
                    Tag = obj.ToSlot()
                };
                n++;
                i = n % 8;
                j = n / 8;
                Panel_Items.Controls.Add(slot);
                slot.LeftClickAction = () =>
                {
                    GameObjectSlot newSlot = slot.Tag.Object.ToSlot();
                    //DragDropManager.Create(new DragDropSlot(null, null, newSlot, DragDropEffects.None));
                    DragDropManager.Create(new DragDropSlot(null, null, new TargetArgs(newSlot), DragDropEffects.None));

                    EmptyTool tool = new EmptyTool();
                    tool.LeftClick = (target) =>
                    {
                        if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                        {

                            //if (!target.IsBlock())
                            if(tool.Target.Type == TargetType.Entity)
                                Net.Client.RemoveObject(tool.Target.Object);
                        }
                        else
                        {
                            //Net.Client.AddObject(slot.Tag.Object.Clone(), target.Global + face + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : tool.Precise));
                            Net.Client.AddObject(slot.Tag.Object.Clone(), tool.Target.FaceGlobal + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : tool.Target.Precise));
                        }
                        return ControlTool.Messages.Default;
                    };
                    tool.DrawActionMy = (sb, cam) =>
                    {
                        //if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                        //    return;
                        if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                        {
                            Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                         //   sb.Draw(UIManager.Icons16x16, loc + Vector2.UnitX * 8, new Rectangle(0, 0, 16, 16), Color.White);
                            return;
                        }
                        if (slot.Tag.Object.IsNull())
                            return;
                        //if (tool.TargetOld.IsNull())
                        //    return;
                        //if (!tool.TargetOld.Exists)
                        //    return;
                        if (tool.Target == null)
                            return;
                        var sprite = slot.Tag.Object.GetSprite();
                        //var global = tool.TargetOld.Global + tool.Face + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : tool.Precise);
                        var global = tool.Target.FaceGlobal + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : tool.Target.Precise);

                        var pos = cam.GetScreenPosition(global);
                        sprite.Draw(sb, pos, Color.White * 0.5f, 0, sprite.Origin, cam.Zoom, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, global.GetDrawDepth(Engine.Map, cam));
                        //slot.Tag.Object.DrawPreview(sb, cam, tool.Target.Global + tool.Face, (tool.Target.Global + tool.Face).GetDepth(cam));
                    };
                    ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ObjectSpawnTool(slot.Tag.Object);// tool;
                };
            }
        }
    }
}
