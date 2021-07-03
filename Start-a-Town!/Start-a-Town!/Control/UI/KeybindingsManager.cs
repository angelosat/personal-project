using System.Collections.Generic;

namespace Start_a_Town_
{
    class KeybindingsManager
    {
        static readonly List<KeyBind> KeyBindings = new List<KeyBind>();
        static KeybindingsManager()
        {
            RegisterKeybind(KeyBind.ToggleForbidden);
            RegisterKeybind(KeyBind.Cancel);
        }

        static public void RegisterKeybind(KeyBind kb)
        {
            KeyBindings.Add(kb);
        }
    }
    
}
