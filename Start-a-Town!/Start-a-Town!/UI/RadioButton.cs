using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class RadioButton : CheckBox
    {
        public event EventHandler<EventArgs> CheckedChanged;
        public RadioButton(string text, bool check = false) : base(text, check) { }
        public RadioButton(string text, Vector2 location, bool check = false) : base(text, location, check) { }
        
        public void OnCheckedChanged()
        {
            if (CheckedChanged != null)
                CheckedChanged(this, EventArgs.Empty);
        }

        protected override void OnLeftClick()
        {
            if (Checked)
                return;
            Rectangle bounds = this.BoundsScreen;
           
            base.OnLeftClick(); 
            OnCheckedChanged();
            if (Checked)
            {
                if (Parent.Controls != null)
                    foreach (Control control in Parent.Controls.Where(foo => foo is RadioButton))
                        if (control != this)
                            (control as RadioButton).Checked = false;
            }
        }
    }
}
