using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    /// <summary>
    /// <para>string GetName();</para>
    /// <para>Icon GetIcon();</para>
    /// <para>Color GetSlotColor();</para>
    /// <para>string GetCornerText();</para>
    /// <para>void Draw(SpriteBatch sb, Vector2 pos);</para>
    /// </summary>
    public interface ISlottable : ITooltippable
    {
       // Atlas.Node.Token GetGraphic();
        string GetName();
        Icon GetIcon();
        Color GetSlotColor();
        string GetCornerText();
        void DrawUI(SpriteBatch sb, Vector2 pos);
    }
}
