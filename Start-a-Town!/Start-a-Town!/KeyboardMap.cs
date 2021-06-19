using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_
{
    class KeyboardMap
    {
        static public char MapKey(Keys lastkey, Keys[] pressedkeys)
        {
            
            switch (lastkey)
            {
                case Keys.OemQuestion:
                    if (pressedkeys.Contains(Keys.LeftShift) || pressedkeys.Contains(Keys.RightShift))
                        return '?';
                    return '/';
                case Keys.OemPipe:
                    return '\\';
                case Keys.OemPeriod:
                    return '.';
                case Keys.OemComma:
                    return ',';
                default:
                    char c = (char)((int)lastkey);
                    if (c >= 160 && c <= 165)
                        return '\0';
                    if (c >= 65 && c <= 90)
                        if (!(pressedkeys.Contains(Keys.LeftShift) || pressedkeys.Contains(Keys.RightShift)))
                            c = Char.ToLower(c);
                    return c;
            }
            //return '\0';
        }
    }
}
