using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ContextAction
    {
        public Func<string> Name;//, HoverFunc;
        public Func<bool> Action;//, ControlInit = ;
        public Func<bool> Available = () => true;
        public Action<ContextAction, Button> ControlInit = (act, btn) => { };
        public PlayerInput Shortcut;

        public ContextAction(string name, Func<bool> action, Func<bool> avail)
        {
            this.Name = () => name;
            this.Action = action;
            //this.Action = () => { if(action!=null) action(); return true; };
            this.Available = avail;
        }
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
        public ContextAction(Interaction interaction, GameObject player, TargetArgs target)
        {
            this.Name = () => interaction.Name;
            this.Available = () => interaction.Conditions.Evaluate(player, target);
        }
        public override string ToString()
        {
            return ((this.Shortcut!=null) ? this.Shortcut.ToString() + ": " : "") +  this.Name();
        }
    }
}
