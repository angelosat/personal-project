using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class Popup : Panel
    {
        Popup ParentPopup;
        Popup()
        {
           // PopupManager.Instance.Popups.Add(this);
            this.AutoSize = true;         
        }
        Popup(Popup parentPopup)
            : this()
        {
            this.ParentPopup = parentPopup;
        }
        public override bool Show()
        {
            this.Location = UIManager.Mouse;
            ConformToScreen();
            return base.Show();
        }

        Popup Initialize(Action<Popup> initializer)
        {
            initializer(this);
            return this;
        }

        //static Popup ShowNew(Action<Popup> initializer)
        //{
        //    //return new Popup().Initialize(initializer).Show();
        //    var pop = new Popup().Initialize(initializer);
        //    pop.Show();
        //    return pop;
        //}

        public class Manager
        {
            static public Popup Create(Action<Popup> initializer, Popup parent = null)
            {
                var newpop = new Popup(parent).Initialize(initializer);
                if (parent == null)
                    foreach (var pop in PopupManager.Instance.Popups)
                        pop.Hide();
                PopupManager.Instance.Popups.Add(newpop);
                newpop.Show();
                return newpop;
            }
        }

        //public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    base.HandleRButtonDown(e);
        //    this.Hide();
        //}
    }
}
