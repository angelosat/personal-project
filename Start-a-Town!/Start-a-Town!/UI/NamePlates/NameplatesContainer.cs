using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    class NameplatesContainer : Control
    {
        public override Control Invalidate(bool invalidateChildren = false)
        {
            return this;
        }
    }
}
