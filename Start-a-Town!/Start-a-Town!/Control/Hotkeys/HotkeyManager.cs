using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    partial class HotkeyManager : GameSettings
    {
        static readonly HashSet<Hotkey> Hotkeys = new();
        GroupBox _gui;
        internal override GroupBox Gui => this._gui ??= this.CreateGui();
        readonly Lazy<WindowInputKey> EnterKeyGui = new();
        
        public static IHotkey RegisterHotkey(HotkeyContext context, string label, Action action, System.Windows.Forms.Keys key1 = System.Windows.Forms.Keys.None, System.Windows.Forms.Keys key2 = System.Windows.Forms.Keys.None)
        {
            var hotkey = new Hotkey(context, label, action, key1, key2);
            handleConflicts(hotkey, false);
            Hotkeys.Add(hotkey);
            return hotkey;
        }
        static void handleConflicts(Hotkey newHotkey, bool overwrite)
        {
            foreach (var h in Hotkeys.Where(hk => hk.Context == newHotkey.Context))
            {
                if (h == newHotkey)
                    continue;
                if (newHotkey.Key1 != 0)
                {
                    if (h.ShortcutKeys[0] == newHotkey.Key1)
                    {
                        if (!overwrite)
                            newHotkey.Key1 = 0;
                        else
                            h.Key1 = 0;
                        return;
                    }
                    else if (h.ShortcutKeys[1] == newHotkey.Key1)
                    {
                        if (!overwrite)
                            newHotkey.Key1 = 0;
                        else
                            h.Key2 = 0;
                        return;
                    }
                }
                if (newHotkey.Key2 != 0)
                {
                    if (h.ShortcutKeys[0] == newHotkey.Key2)
                    {
                        if (!overwrite)
                            newHotkey.Key2 = 0;
                        else
                            h.Key1 = 0;
                        return;
                    }
                    else if (h.ShortcutKeys[1] == newHotkey.Key2)
                    {
                        if (!overwrite)
                            newHotkey.Key2 = 0;
                        else
                            h.Key2 = 0;
                        return;
                    }
                }
            }
        }

        public static bool PerformHotkey(System.Windows.Forms.Keys key, HotkeyContext context)
        {
            if (Hotkeys.FirstOrDefault(h => h.Context == context && h.ShortcutKeys.Contains(key)) is not Hotkey hotkey)
                return false;
            hotkey.ActionPress();
            return true;
        }
        public static bool Press(System.Windows.Forms.Keys key, HotkeyContext context)
        {
            if (Hotkeys.FirstOrDefault(h => h.Context == context && h.ShortcutKeys.Contains(key)) is Hotkey hotkey && !hotkey.Pressed)
            {
                hotkey.Pressed = true;
                hotkey.ActionPress();
                return true;
            }
            return false;
        }
        public static bool Release(System.Windows.Forms.Keys key, HotkeyContext context)
        {
            if (Hotkeys.FirstOrDefault(h => h.Context == context && h.ShortcutKeys.Contains(key)) is Hotkey hotkey && hotkey.Pressed)
            {
                hotkey.Pressed = false;
                hotkey.ActionRelease();
                return true;
            }
            return false;
        }
        internal override void Apply()
        {
            throw new NotImplementedException();
        }

        GroupBox CreateGui()
        {
            var box = new GroupBox() { Name = "Hotkeys" };
            var byContext = Hotkeys.GroupBy(h => h.Context);
            box.AddControlsVertically(1, byContext.Select(cat =>
                new TableScrollableCompactNewNew<Hotkey>(cat.Count(), true)
                    .AddColumn(null, cat.Key.Name, 192, h => h.Label.ToLabel())
                    .AddColumn(null, "Primary", 64, h => new Label(() => $"{h.Key1}", delegate { editHotkey(h, 0); }))
                    .AddColumn(null, "Secondary", 64, h => new Label(() => $"{h.Key2}", delegate { editHotkey(h, 1); }))
                    .AddItems(cat)).ToArray());
            return box;
            static void setHotkey(Hotkey hotkey, int keyIndex, System.Windows.Forms.Keys key)
            {
                hotkey._keys[keyIndex] = key;
                handleConflicts(hotkey, true);
            }

            void editHotkey(Hotkey h, int index)
            {
                this.EnterKeyGui.Value.EditHotkey(h.Label, key => setHotkey(h, index, key));
            }
        }
    }
}
