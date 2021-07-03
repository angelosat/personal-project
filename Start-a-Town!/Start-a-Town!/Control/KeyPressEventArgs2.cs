using System;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_
{
    public class KeyPressEventArgs2 : EventArgs
    {
        public Keys Key;
        public InputState Input;
        public KeyPressEventArgs2(Keys key, InputState input)
        {
            Key = key;
            Input = input;
        }
    }
}
