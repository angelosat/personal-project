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
    public class WindowTargetManagement : Window
    {
        //static Dictionary<object, WindowTargetManagement> OpenWindows = new Dictionary<object, WindowTargetManagement>();
        static WindowTargetManagement _Instance;
        public static WindowTargetManagement Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new WindowTargetManagement();
                return _Instance;
            }
        }

        readonly static Dictionary<object, Window> OpenWindows = new();

        //static WindowTargetInterface Open;
        //Func<Vector3> LocationGetter;
        //object Entity;

        public Panel PanelInfo, PanelActions;

        WindowTargetManagement()//object entity, string title)
        {
            this.AutoSize = true;
            this.Movable = true;
            //this.Entity = entity;
            //this.Title = title;
            this.PanelInfo = new Panel(new Rectangle(0, 0, 250, 150));
            this.PanelActions = new Panel() { AutoSize = true, Location = this.PanelInfo.TopRight };
            this.Client.Controls.Add(this.PanelInfo, this.PanelActions);
        }

        static public Window Refresh(GameObject entity)
        {
            return Refresh(new TargetArgs(entity));
        }
        static public Window Refresh(TargetArgs t)
        {
            if (t.Object == null)
                return null;
            Instance.PanelInfo.ClearControls();
            Instance.PanelActions.ClearControls();
            if (OpenWindows.TryGetValue(t.Object, out Window existing))
                return existing;
            switch(t.Type)
            {
                case TargetType.Entity:
                    if (t.Object.HasComponent<NpcComponent>())
                    {
                        var win = new AI.NpcUI(t.Object as Actor).ToWindow(t.Object.Name);
                        win.SmartPosition();
                        win.Show();
                        OpenWindows[t.Object] = win;
                        win.HideAction = () => OpenWindows.Remove(t.Object);
                        return win;
                    }
                    else
                    {
                        //if (OpenWindows.TryGetValue(t.Object, out existing))
                        //    break;
                        existing = new WindowTargetManagement
                        {
                            Tag = t.Object
                        };
                        t.Object.GetManagementInterface(existing as WindowTargetManagement);
                        existing.Show();
                        return existing;
                        //break;
                    }
                    //break;

                case TargetType.Position:
                    var inter = new WindowTargetManagement();
                    object zone = t.Map.Town.QueryPosition(t.Global).FirstOrDefault();
                    if (zone == null)
                    {
                        zone = t.Global;
                        t.Map.GetBlock(t.Global).GetInterface(t.Map, t.Global, inter);
                        inter.Tag = zone;
                    }
                    else
                        t.Map.Town.GetManagementInterface(t, inter);
                    Window existingWin;
                    if (OpenWindows.TryGetValue(zone, out existingWin))
                    {
                        existingWin.Location = UIManager.Mouse;
                        return existingWin;
                        //break;
                    }
                    else
                    { 
                        inter.Show();
                        return inter;
                    }
                    //break;

                case TargetType.Slot:
                    if (t.Slot.Object != null)
                        return OpenEntityWindow(t.Slot.Object);
                    break;
                //case TargetType.Position:
                //    var inter = new WindowTargetManagement();
                //    var zone = t.Map.Town.QueryPosition(t.Global).FirstOrDefault();

                //    WindowTargetManagement existingWin;
                //    if (OpenWindows.TryGetValue(zone, out existingWin))
                //    {
                //        existingWin.Location = UIManager.Mouse;
                //        break;
                //    }
                //    if (zone == null)
                //        t.Map.GetBlock(t.Global).GetInterface(t.Map, t.Global, inter);
                //    else
                //        t.Map.Town.GetManagementInterface(t, inter);
                //    inter.Show();
                //    break;

                default:
                    break;
            }
            //Instance.Show();
            return null;
        }
        
        static Window OpenEntityWindow(GameObject obj)
        {
            if (obj.HasComponent<NpcComponent>())
            {
                var win = new AI.NpcUI(obj as Actor).ToWindow(obj.Name);
                win.SmartPosition();
                win.Show();
                return win;
            }
            else
            {
                //t.Object.GetManagementInterface(Instance);
                //Instance.Show();
                if (OpenWindows.TryGetValue(obj, out Window existing))
                    return existing;
                existing = new WindowTargetManagement();
                existing.Tag = obj;
                obj.GetManagementInterface(existing as WindowTargetManagement);
                existing.Show();
                return existing;
            }
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

            this.Location = UIManager.Mouse;

            //OpenWindows[this.Tag as GameObject] = this;
            OpenWindows[this.Tag] = this;

            return base.Show();
        }
        public override bool Hide()
        {
            var obj = this.Tag;// as GameObject;
            OpenWindows.Remove(obj);
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
