using System;

namespace Start_a_Town_
{

    partial class HotkeyManager
    {
        class Hotkey : IHotkey
        {
            public readonly Action Action;
            public readonly string Label;
            public readonly HotkeyContext Context;
            public readonly System.Windows.Forms.Keys[] _keys = new System.Windows.Forms.Keys[2];
            public System.Windows.Forms.Keys[] ShortcutKeys => this._keys;
            public System.Windows.Forms.Keys Key1
            {
                get => this._keys[0];
                set => this._keys[0] = value;
            }
            public System.Windows.Forms.Keys Key2
            {
                get => this._keys[1];
                set => this._keys[1] = value;
            }
            public Hotkey(HotkeyContext context, string label, Action action, System.Windows.Forms.Keys key1 = System.Windows.Forms.Keys.None, System.Windows.Forms.Keys key2 = System.Windows.Forms.Keys.None)
            {
                this.Context = context;
                this.Label = label;
                this.Action = action;
                this.Key1 = key1;
                this.Key2 = key2;
            }
            public string GetLabel()
            {
                if (this.Key2 != 0)
                {
                    if (this.Key1 != 0)
                        return $"{this.Key1}, {this.Key2}";
                    else
                        return this.Key2.ToString();
                }
                else if (this.Key1 != 0)
                    return this.Key1.ToString();
                return "";
            }
            internal bool Contains(System.Windows.Forms.Keys key)
            {
                return this._keys[0] == key || this._keys[1] == key;
            }
            public override string ToString()
            {
                return $"Hotkey:{this.Label}";
            }
        }
    }
}
