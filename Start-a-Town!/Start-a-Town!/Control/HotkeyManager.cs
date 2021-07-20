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
            box.AddControlsVertically(4, byCategory.Select(cat =>
                new TableScrollableCompactNewNew<Hotkey>(cat.Count(), true)
                    .AddColumn(null, cat.Key, 96, h => h.Label.ToLabel())
                    .AddColumn(null, "Primary", 64, h => new Label(() => $"{h.Key1}", editHotkey))
                    .AddColumn(null, "Secondary", 64, h => new Label(() => $"{h.Key2}", editHotkey))
                    .AddItems(cat)).ToArray());
            return box;

            void editHotkey() { }
        }

        class Hotkey
        {
            public readonly Action Action;
            public readonly string Label;
            public readonly string Category;
            System.Windows.Forms.Keys _key1, _key2;
            public System.Windows.Forms.Keys Key1
            {
                get => this._key1;
                set
                {
                    this._key1 = value;
                }
            }
            public System.Windows.Forms.Keys Key2
            {
                get => this._key2;
                set
                {
                    this._key2 = value;
                }
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
    }
}
