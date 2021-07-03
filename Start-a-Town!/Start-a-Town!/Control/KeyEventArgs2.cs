using System;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_
{
    public class KeyEventArgs2 : EventArgs
    {
        public Keys[] KeysNew, KeysOld;
        public KeyEventArgs2(Keys[] keysnew, Keys[] keysold)
        {
            KeysNew = keysnew;
            KeysOld = keysold;
        }
    }
}
