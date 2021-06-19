using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public abstract class ListControl : Control
    {
        public ObjectCollection Items;
        public string DisplayMember = "", ValueMember = "";
        public Func<object, string> DisplayMemberFunc;
        protected int _SelectedIndex = -1;
        public virtual int SelectedIndex { get { return _SelectedIndex; } set { _SelectedIndex = value; } }
        Object _SelectedValue;
        public Object SelectedValue
        {
            get { return _SelectedValue; }
            set
            {
                object old = _SelectedValue;
                _SelectedValue = value;
                if (old != _SelectedValue)
                    OnSelectedValueChanged();
            }
        }
        public event EventHandler<EventArgs> SelectedValueChanged;
        protected virtual void OnSelectedValueChanged()
        {
            if (SelectedValueChanged != null)
                SelectedValueChanged(this, EventArgs.Empty);
        }

        public ListControl(Vector2 location) : base(location) { }
        public ListControl() { }
        public abstract void Build();

        
    }
}
