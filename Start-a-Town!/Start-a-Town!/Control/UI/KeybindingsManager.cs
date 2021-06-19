using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class KeybindingsManager
    {
        //static Dictionary<object, Keys> Bindings = new Dictionary<object, Keys>();
        static readonly List<KeyBind> KeyBindings = new List<KeyBind>();
        static KeybindingsManager()
        {
            //Bindings.Add(KeyBindToggleForbidden, Keys.F);
            RegisterKeybind(KeyBind.ToggleForbidden);
            RegisterKeybind(KeyBind.Cancel);
        }

        static public void RegisterKeybind(KeyBind kb)//, Keys key)
        {
            KeyBindings.Add(kb);
        }
    }
    
}
