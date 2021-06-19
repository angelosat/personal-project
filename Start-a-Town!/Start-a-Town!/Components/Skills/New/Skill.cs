using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    abstract class SkillNew : ISlottable, ICloneable
    {
        public string Name;
        public string Description;
        public Icon Icon;

        public virtual void Use(GameObject parent, TargetArgs target) { }

        public virtual void DrawUI(SpriteBatch sb, Vector2 pos) 
        {
            this.Icon.Draw(sb, pos - new Vector2(this.Icon.SourceRect.Width, this.Icon.SourceRect.Height) / 2);
        }
        public virtual string GetCornerText() { return ""; }
        public virtual Icon GetIcon() { return this.Icon; }
        public virtual Color GetSlotColor() { return Color.White; }
        public virtual string GetName() { return this.Name; }

        public virtual void GetTooltipInfo(Tooltip tooltip)
        {
            tooltip.Controls.Add(new Label(this.Name), new Label(this.Description));
            tooltip.AlignVertically();
        }

        public override string ToString()
        {
            return this.Name;
        }

        public abstract object Clone();
    }
}
