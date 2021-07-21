using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class HotkeyManager : GameSettings
    {
        static readonly HashSet<Hotkey> Hotkeys = new();
        GroupBox _gui;
        internal override GroupBox Gui => this._gui ??= this.CreateGui();
        readonly Lazy<WindowInputKey> EnterKeyGui = new();

        public static void RegisterHotkey(string category, string label, Action action, System.Windows.Forms.Keys key1 = System.Windows.Forms.Keys.None, System.Windows.Forms.Keys key2 = System.Windows.Forms.Keys.None)
        {
            var hotkey = new Hotkey(category, label, action, key1, key2);
            Hotkeys.Add(hotkey);
        }

        internal override void Apply()
        {
            throw new NotImplementedException();
        }

        GroupBox CreateGui()
        {
            var box = new GroupBox() { Name = "Hotkeys" };
            var byCategory = Hotkeys.GroupBy(h => h.Category);
            box.AddControlsVertically(1, byCategory.Select(cat =>
                new TableScrollableCompactNewNew<Hotkey>(cat.Count(), true)
                    .AddColumn(null, cat.Key, 96, h => h.Label.ToLabel())
                    .AddColumn(null, "Primary", 64, h => new Label(() => $"{h.Key1}", delegate { editHotkey(h, 0); }))
                    .AddColumn(null, "Secondary", 64, h => new Label(() => $"{h.Key2}", delegate { editHotkey(h, 1); }))
                    .AddItems(cat)).ToArray());
            return box;
            static void setHotkey(Hotkey hotkey, int keyIndex, System.Windows.Forms.Keys key)
            {
                foreach (var hk in Hotkeys)
                    if (hk.Key1 == key)
                        hk.Key1 = 0;
                    else if (hk.Key2 == key)
                        hk.Key2 = 0;
                hotkey.Keys[keyIndex] = key;
            }

            void editHotkey(Hotkey h, int index)
            {
                this.EnterKeyGui.Value.EditHotkey(h.Label, key => setHotkey(h, index, key));
            }
        }
        class Hotkey
        {
            public readonly Action Action;
            public readonly string Label;
            public readonly string Category;
            public readonly System.Windows.Forms.Keys[] Keys = new System.Windows.Forms.Keys[2];
            public System.Windows.Forms.Keys Key1
            {
                get => this.Keys[0];
                set => this.Keys[0] = value;
            }
            public System.Windows.Forms.Keys Key2
            {
                get => this.Keys[1];
                set => this.Keys[1] = value;
            }
            public Hotkey(string category, string label, Action action, System.Windows.Forms.Keys key1 = System.Windows.Forms.Keys.None, System.Windows.Forms.Keys key2 = System.Windows.Forms.Keys.None)
            {
                this.Category = category;
                this.Label = label;
                this.Action = action;
                this.Key1 = key1;
                this.Key2 = key2;
            }
        }
        class WindowInputKey : Panel
        {
            string ActionLabel;
            public WindowInputKey()
            {
                this.Width = 256;
                this.Height = 64;
                this.Color = UIManager.Tint;
                var label = new Label(() => $"Press key for: [{this.ActionLabel}]") { AutoSize = true };
                this.AddControls(label);
                label.AnchorToParentCenter();
            }
            Action<System.Windows.Forms.Keys> CallBack;
            public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
            {
                var key = (System.Windows.Forms.Keys)e.KeyValue;
                if(key != System.Windows.Forms.Keys.Escape)
                    this.CallBack(key);
                this.Hide();
            }
            public void EditHotkey(string actionLabel, Action<System.Windows.Forms.Keys> callBack)
            {
                this.ActionLabel = actionLabel;
                this.CallBack = callBack;
                this.ShowDialog();
            }
        }
    }
}
