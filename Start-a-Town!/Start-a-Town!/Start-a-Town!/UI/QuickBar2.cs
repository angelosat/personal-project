using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Tasks;
using Start_a_Town_.Interactions;
using Start_a_Town_.Components;
using Start_a_Town_.Control;
using Start_a_Town_.Objects;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_.UI
{
    class QuickBar : Window
    {
        Panel pInteractions;

        static QuickBar _Instance;
        static public QuickBar Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new QuickBar();
                return _Instance;
            }
        }

        public Task[] DefaultTasks = new Task[10];

        public QuickBar()
        {
            Opacity = 0;
            Title = "QuickBar";
            AutoSize = true;
            Closable = false;

            pInteractions = new Panel();
            pInteractions.AutoSize = true;
            pInteractions.MouseLeave += new EventHandler<EventArgs>(pInteractions_MouseLeave);
            pInteractions.Color = Color.Black;
            pInteractions.BackgroundStyle = BackgroundStyle.Tooltip;
            Panel panel = new Panel();
            panel.AutoSize = true;

            //Player.Instance.Tool.Hotbar.HotbarChanged += new EventHandler<HotbarEventArgs>(Hotbar_HotbarChanged);

            for (int i = 0; i < 10; i++)
            {
                Slot slot = new Slot(new Vector2(i * Slot.DefaultHeight, 0));

                //slot.Tag = Player.Instance.Tool.Hotbar[i];

                Label hotkey = new Label(new Vector2(slot.Width, 0), ((i + 1) % 10).ToString(), TextAlignment.Right, TextAlignment.Top);
                slot.Controls.Add(hotkey);
                slot.Index = i;
                slot.Click += new UIEvent(slot_Click);
                //slot.MouseEnter += new EventHandler<EventArgs>(slot_GotFocus);
                //slot.MouseLeave += new EventHandler<EventArgs>(slot_LostFocus);
                //slot.DrawItem += new EventHandler<DrawItemEventArgs>(slot_DrawItem);
                slot.DrawTooltip += new EventHandler<TooltipArgs>(slot_DrawTooltip);
                slot.CustomTooltip = true;
                //slot.DrawMode = DrawMode.OwnerDrawFixed;
                Controls.Add(slot);
            }

            Location = new Vector2(0, Game1.Instance.graphics.PreferredBackBufferHeight - Height);
        }

        void slot_DrawTooltip(object sender, TooltipArgs e)
        {
            GameObjectSlot slot = (e.Tooltip.Tag as Slot).Tag as GameObjectSlot;
            if (slot == null)
                return;
            GameObject obj = slot.Object as GameObject;
            if (obj == null)
                return;
            obj.TooltipGroups(e.Tooltip);
            //e.Tooltip.Controls.AddRange(obj.TooltipGroups);
        }

        void Hotbar_HotbarChanged(object sender, HotbarEventArgs e)
        {
            (Controls[e.SlotID] as Slot).Tag = (sender as Hotbar)[e.SlotID];
        }

        //void slot_DrawTooltip(object sender, TooltipEventArgs e)
        //{
        //    Slot slot = sender as Slot;
        //    if (DefaultTasks[slot.Index] == null)
        //        return;

        //    e.TooltipGroups.AddRange(DefaultTasks[slot.Index].TooltipGroups);
        //}

        //void slot_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    Slot slot = sender as Slot;

        //    e.SpriteBatch.Draw(UIManager.SlotSprite, e.Bounds, null, Color.Lerp(Color.Transparent, Color.White, slot.IsTopMost ? 1 : 0.5f), 0, Vector2.Zero, slot.SprFx, 0.1f);

        //    if (slot.Tag != null)
        //    {
        //        Task task = DefaultTasks[slot.Index];
        //        if (task != null)
        //        {
        //            e.SpriteBatch.Draw(ItemManager.Instance.ItemSheet, new Vector2(e.Bounds.X + 3, e.Bounds.Y + 3), ItemManager.Instance.Icons[task.IconIndex], Color.White);
        //        }
        //        else
        //            if (slot.Tag != null)
        //                e.SpriteBatch.Draw(ItemManager.Instance.ItemSheet, new Vector2(e.Bounds.X + 3, e.Bounds.Y + 3), ItemManager.Instance.Icons[task.IconIndex], Color.White);
        //    }
        //}

        void pInteractions_MouseLeave(object sender, EventArgs e)
        {
            Control p = sender as Control;
            if (!p.Parent.Bounds.Intersects(Controller.Instance.MouseRect))
                p.Parent.Controls.Remove(pInteractions);
        }

        void slot_LostFocus(object sender, EventArgs e)
        {
            if (!pInteractions.Bounds.Intersects(Controller.Instance.MouseRect))
                (sender as Slot).Controls.Remove(pInteractions);
        }


        void slot_GotFocus(object sender, EventArgs e)
        {
            Slot slot = sender as Slot;

            if (slot.Tag == null)
                return;
            pInteractions.Controls.Clear();
            ListView listview = new ListView();
            listview.View = View.IconOnly;
            listview.IconPropertyName = "Icon";

            listview.AddRange((slot.Tag as IInteractable).Interactions.GetTasks(Player.Actor, slot.Tag as GameObject).ToArray());
            listview.ItemActivate += new EventHandler(listview_ItemActivate);

            pInteractions.Controls.Add(listview);
            pInteractions.Location = new Vector2(0, slot.Top - pInteractions.Height);
            slot.Controls.Add(pInteractions);
        }

        void listview_ItemActivate(object sender, EventArgs e)
        {
            ListView list = sender as ListView;

            list.Parent.Parent.Controls.Remove(pInteractions);
        }


        void slot_Click(object sender, EventArgs e)
        {
            Task task = DefaultTasks[(sender as Slot).Index];
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            char c = (char)((int)e.Key);
            int n = (int)Char.GetNumericValue(c);
            if (!Char.IsDigit(c))
                return;
            (Controls[n > 0 ? n - 1 : 9] as Slot).Pressed = true;
            base.OnKeyPress(e);
        }

        protected override void OnKeyRelease(KeyPressEventArgs e)
        {
            char c = (char)((int)e.Key);
            int n = (int)Char.GetNumericValue(c);
            if (!Char.IsDigit(c))
                return;
            Slot slot = Controls[n > 0 ? n - 1 : 9] as Slot;
            if (slot.Pressed)
                slot.PerformClick();
            slot.Pressed = false;
            base.OnKeyRelease(e);
        }

        public override void HandleInput(InputState input)
        {
            Keys[] keys = input.CurrentKeyboardState.GetPressedKeys();
            if (keys.Length > 0)
                foreach (Keys key in keys)
                    if (input.IsKeyPressed(key))
                        OnKeyPress(new KeyPressEventArgs(key, input));

            keys = input.LastKeyboardState.GetPressedKeys();
            if (keys.Length > 0)
                foreach (Keys key in keys)
                    if (input.IsKeyReleased(key))
                        OnKeyRelease(new KeyPressEventArgs(key, input));

            base.HandleInput(input);
        }
    }
}
