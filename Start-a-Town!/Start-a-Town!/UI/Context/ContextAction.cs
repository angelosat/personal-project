using System;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ContextAction
    {
        public Func<string> Name;
        public Func<bool> Action;
        public Func<bool> Available = () => true;
        public Action<ContextAction, Button> ControlInit = (act, btn) => { };
        public PlayerInput Shortcut;

        public ContextAction(string name, Func<bool> action)
        {
            this.Name = () => name;
            this.Action = action;
        }
        public ContextAction(Func<string> name, Func<bool> action)
        {
            this.Name = name;
            this.Action = action;
        }
        public ContextAction(Func<string> name, Action action)
        {
            this.Name = name;
            this.Action = () => { action(); return true; };
        }
        public ContextAction(Interaction interaction)
        {
            this.Name = () => interaction.Name;
        }
        public override string ToString()
        {
            return ((this.Shortcut!=null) ? this.Shortcut.ToString() + ": " : "") +  this.Name();
        }
    }
}
