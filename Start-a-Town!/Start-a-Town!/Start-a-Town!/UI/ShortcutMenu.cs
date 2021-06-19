using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class ShortcutMenuItem : ITooltippable// : Control
    {
        public string Name;
        public Action Command;
        public Object Tag;
        public ShortcutMenuItem(string name, Action command, Object tag= null)
           // : base(name)
        {
            Name = name;
            Command = command;
            Tag = tag;
        }
        public void GetTooltipInfo(Tooltip tooltip)
        {
        }
        //public List<GroupBox> TooltipGroups
        //{
        //    get
        //    {
        //        if (Tag is ITooltippable)
        //            return ((ITooltippable)Tag).TooltipGroups;
        //        return null;
        //    }
        //}
    }
    public class ShortcutMenu : Panel
    {
        static public void Clear()
        {
            Instance.Shortcuts.Clear();
        }

        //public ListBox ContextInteractions;
        public Object Source;
        public SuperDataView Shortcuts;

        static ShortcutMenu _Instance;
        static public ShortcutMenu Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ShortcutMenu();
                return _Instance;
            }
        }

        ShortcutMenu()
        {
            //ClientLocation = Vector2.Zero;
            //ContextInteractions = new ListBox(Vector2.Zero);
            //Controls.Add(ContextInteractions);
            Shortcuts = new SuperDataView();
            Shortcuts.SelectedItemChanged += new EventHandler(Shortcuts_SelectedItemChanged);
            AutoSize = true;
        }

        void Shortcuts_SelectedItemChanged(object sender, EventArgs e)
        {
            SuperDataView list = (SuperDataView)sender;
            ShortcutMenuItem shortcut = (ShortcutMenuItem)list.SelectedItem;
            if (shortcut == null)
                throw (new Exception("Invalid shortcut."));
            shortcut.Command();
            list.Clear();
            Controls.Clear();
         //   Hide();
        }

        /// <summary>
        /// Adds a new shortcut to the ShortcutMenu.
        /// </summary>
        /// <param name="name">The displayed name of the shortcut.</param>
        /// <param name="command">The code to be executed when the shortcut is selected.</param>
        /// <param name="tag">An optional object to be associated with the shortcut, that provides additional functionality such as a tooltip.</param>
        static public void Add(string name, Action command, Object tag = null)
        {
            ShortcutMenuItem shortcut = new ShortcutMenuItem(name, command, tag);
            Instance.Shortcuts.Add(shortcut.Name, shortcut);
            Instance.Controls.Clear();
            Instance.Controls.Add(Instance.Shortcuts);
        }

        static public void Activate()
        {
            Instance.Show();
        }

        public bool Show()
        {
            Location = Controller.Instance.MouseLocation - new Vector2(Width - 1, 0);
            //WindowManager = Game1.Instance.CurrentRoom.WindowManager;
            foreach (Control c in Controls)
                // base.Show();
                // WindowManager.BringToFront(this);   
                this.BringToFront();
            return false;            
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }
    }
}
