using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class RandomizeButton : IconButton
    {
        public RandomizeButton()
        {
            BackgroundTexture = UIManager.Icon16Background;
            Icon = new Icon(UIManager.Icons16x16, 1, 16);
            HoverText = "Randomize";
        }
    }
}
