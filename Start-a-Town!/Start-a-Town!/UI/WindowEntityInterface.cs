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
    class WindowEntityInterface : Window
    {
        static Dictionary<object, WindowEntityInterface> Open = new Dictionary<object, WindowEntityInterface>();
        static public WindowEntityInterface GetWindow(object entity)
        {
            WindowEntityInterface win;
            if (Open.TryGetValue(entity, out win))
                return win;
            return null;
        }

        Func<Vector3> LocationGetter;
        object Entity;

        public WindowEntityInterface(object entity, string title, Func<Vector3> globalGetter)
        {
            this.AutoSize = true;
            this.Movable = true;
            this.LocationGetter = globalGetter;
            this.Entity = entity;
            this.Title = title;
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.EntityMovedCell:
                    if (this.LocationGetter == null)
                        break;
                    var entity = e.Parameters[0] as GameObject;
                    if (entity != PlayerOld.Actor)
                        break;
                    var distance = Vector3.DistanceSquared(PlayerOld.Actor.Global, this.LocationGetter());// this.ParentEntity.Global);
                    if (distance > 2)
                        this.Hide();
                    break;

                case Message.Types.BlockEntityRemoved:
                    var blockentity = e.Parameters[0];
                    if (this.Entity == blockentity)
                        this.Hide();
                    break;

                default:
                    break;
            }
            base.OnGameEvent(e);
        }

        public override bool Show()
        {
            var existing = GetWindow(this.Entity);
            if (existing != null)
            {
                return false;
                this.Location = existing.Location;
                existing.Hide();
            }
            Open[this.Entity] = this;
            this.Location = UIManager.Mouse - new Vector2(UIManager.frameSprite.Width / 2); //maybe use last stored location?
            this.ConformToScreen();
            this.SmartPosition();
            return base.Show();
        }
        public override bool Hide()
        {
            base.Hide();
            //return Open.Remove(this);
            return Open.Remove(this.Entity);

        }
        public override bool Close()
        {
            return base.Close();
        }
    }
}
