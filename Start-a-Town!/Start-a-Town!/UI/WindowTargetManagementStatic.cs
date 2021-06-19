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
    public class WindowTargetManagementStatic : Window
    {
        static WindowTargetManagementStatic _Instance;
        public static WindowTargetManagementStatic Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new WindowTargetManagementStatic();
                return _Instance;
            }
        }



        //Func<Vector3> LocationGetter;
        //object Entity;

        public Panel PanelInfo, PanelActions;

        WindowTargetManagementStatic()//object entity, string title)
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
            switch(t.Type)
            {
                case TargetType.Entity:
                    if (t.Object.HasComponent<NpcComponent>())
                    {
                        var ui = AI.NpcUIStatic.Instance;// new AI.NpcUI(t.Object).ToWindow(t.Object.Name);
                        var actor = t.Object as Actor;
                        var win = ui.GetWindow();
                        if (win == null)
                        {
                            win = ui.ToWindow("test");
                            win.SmartPosition();
                            //win.Show();
                        }
                        ui.Refresh(actor);
                        //win.SmartPosition();
                        win.Show();
                        //OpenWindows[t.Object] = win;
                        //win.HideAction = () => OpenWindows.Remove(t.Object);
                        win.Title = actor.Name;
                        return win;
                    }
                    else
                    {
                        //var existing = new WindowTargetManagement();
                        //existing.Tag = t.Object;
                        //t.Object.GetManagementInterface(existing as WindowTargetManagement);
                        //existing.Show();
                        //return existing;
                        //break;
                        var existing = new WindowTargetManagementStatic() { Tag = t.Object };
                        t.Object.GetManagementInterface(existing as WindowTargetManagementStatic);
                        existing.Show();
                        return existing;
                        //break;
                    }
                    //break;

                case TargetType.Position:
                    //var inter = new WindowTargetManagementStatic();
                    //object zone = t.Map.Town.QueryPosition(t.Global).FirstOrDefault();
                    //if (zone == null)
                    //{
                    //    zone = t.Global;
                    //    t.Map.GetBlock(t.Global).GetInterface(t.Map, t.Global, inter);
                    //    inter.Tag = zone;
                    //}
                    //else
                    //    t.Map.Town.GetManagementInterface(t, inter);
                    //Window existingWin;
                    //if (OpenWindows.TryGetValue(zone, out existingWin))
                    //{
                    //    existingWin.Location = UIManager.Mouse;
                    //    return existingWin;
                    //    break;
                    //}
                    //else
                    //{ 
                    //    inter.Show();
                    //    return inter;
                    //}
                    break;

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
            //if (obj.HasComponent<NpcComponent>())
            //{
            //    var win = new AI.NpcUI(obj).ToWindow(obj.Name);
            //    win.SmartPosition();
            //    win.Show();
            //    return win;
            //}
            //else
            //{
            //    //t.Object.GetManagementInterface(Instance);
            //    //Instance.Show();
            //    Window existing;
            //    if (OpenWindows.TryGetValue(obj, out existing))
            //        return existing;
            //    existing = new WindowTargetManagementStatic();
            //    existing.Tag = obj;
            //    obj.GetManagementInterface(existing as WindowTargetManagement);
            //    existing.Show();
            //    return existing;
            //}
            if (obj.HasComponent<NpcComponent>())
            {
                var ui = AI.NpcUIStatic.Instance;// new AI.NpcUI(t.Object).ToWindow(t.Object.Name);
                var win = ui.GetWindow();
                if (win == null)
                {
                    win = ui.ToWindow();
                    win.SmartPosition();
                    win.Show();
                }
                ui.Refresh(obj as Actor);
                //win.SmartPosition();
                //win.Show();
                //OpenWindows[t.Object] = win;
                //win.HideAction = () => OpenWindows.Remove(t.Object);
                win.Title = obj.Name;
                return win;
            }
            else
            {
                //var existing = new WindowTargetManagement();
                //existing.Tag = t.Object;
                //t.Object.GetManagementInterface(existing as WindowTargetManagement);
                //existing.Show();
                //return existing;
                return null;
                //break;
            }
        }

        //public override bool Show()
        //{

        //    this.Location = UIManager.Mouse;

        //    //OpenWindows[this.Tag as GameObject] = this;
        //    OpenWindows[this.Tag] = this;

        //    return base.Show();
        //}
        //public override bool Hide()
        //{
        //    var obj = this.Tag;// as GameObject;
        //    OpenWindows.Remove(obj);
        //    this.PanelInfo.ClearControls();
        //    this.PanelActions.ClearControls();
        //    return base.Hide();
        //}
        
    }
}
