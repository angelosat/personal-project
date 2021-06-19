using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class WindowTargetInterface : Window
    {
        static WindowTargetInterface _Instance;
        public static WindowTargetInterface Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new WindowTargetInterface();
                return _Instance;
            }
        }
        //readonly static WindowTargetInterface Open;

        //Func<Vector3> LocationGetter;
        //object Entity;

        public Panel PanelInfo, PanelActions;

        public WindowTargetInterface()//object entity, string title)
        {
            this.AutoSize = true;
            this.Movable = true;
            //this.Entity = entity;
            //this.Title = title;
            this.PanelInfo = new Panel(new Rectangle(0, 0, 150, 200));
            this.PanelActions = new Panel() { AutoSize = true, Location = this.PanelInfo.TopRight };
            this.Client.Controls.Add(this.PanelInfo, this.PanelActions);
        }

        static public void Refresh(TargetArgs t)
        {
            Instance.PanelInfo.ClearControls();
            Instance.PanelActions.ClearControls();
            switch(t.Type)
            {
                case TargetType.Entity:
                    t.Object.GetInterface(Instance);
                    break;

                default:
                    break;
            }
            Instance.Show();
        }
        //internal override void OnGameEvent(GameEvent e)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.EntityMovedCell:
        //            if (this.LocationGetter == null)
        //                break;
        //            var entity = e.Parameters[0] as GameObject;
        //            if (entity != Player.Actor)
        //                break;
        //            var distance = Vector3.DistanceSquared(Player.Actor.Global, this.LocationGetter());// this.ParentEntity.Global);
        //            if (distance > 2)
        //                this.Hide();
        //            break;

        //        case Message.Types.BlockEntityRemoved:
        //            var blockentity = e.Parameters[0];
        //            if (this.Entity == blockentity)
        //                this.Hide();
        //            break;

        //        default:
        //            break;
        //    }
        //    base.OnGameEvent(e);
        //}

        //public override bool Show()
        //{
        //    if (Open != null)
        //        Open.Hide();
        //    Open = this;
        //    return base.Show();
        //}
        public override bool Show()
        {
            //this.Location = CenterScreen * .5f;// new Vector2(.5f, 1);
            this.SnapToScreenCenter();

            return base.Show();
        }
        public override bool Hide()
        {
            this.PanelInfo.ClearControls();
            this.PanelActions.ClearControls();
            return base.Hide();
        }

        //public override bool Show()
        //{
        //    var existing = GetWindow(this.Entity);
        //    if (existing != null)
        //    {
        //        return false;
        //        this.Location = existing.Location;
        //        existing.Hide();
        //    }
        //    Open[this.Entity] = this;
        //    this.Location = UIManager.Mouse - new Vector2(UIManager.frameSprite.Width / 2); //maybe use last stored location?
        //    this.ConformToScreen();
        //    this.SmartPosition();
        //    return base.Show();
        //}
        //public override bool Hide()
        //{
        //    base.Hide();
        //    //return Open.Remove(this);
        //    return Open.Remove(this.Entity);

        //}
        //public override bool Close()
        //{
        //    return base.Close();
        //}
    }
}
