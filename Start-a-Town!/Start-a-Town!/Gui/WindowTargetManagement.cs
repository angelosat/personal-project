using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class WindowTargetManagement : Window
    {
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

        public Panel PanelInfo, PanelActions;

        WindowTargetManagement()
        {
            this.AutoSize = true;
            this.Movable = true;
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
                        var win = new Window() { Title = "not implemented" };// new AI.NpcUI(t.Object as Actor).ToWindow(t.Object.Name);
                        win.SmartPosition();
                        win.Show();
                        OpenWindows[t.Object] = win;
                        win.HideAction = () => OpenWindows.Remove(t.Object);
                        return win;
                    }
                    else
                    {
                        existing = new WindowTargetManagement
                        {
                            Tag = t.Object
                        };
                        t.Object.GetManagementInterface(existing as WindowTargetManagement);
                        existing.Show();
                        return existing;
                    }

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
                    }
                    else
                    { 
                        inter.Show();
                        return inter;
                    }

                case TargetType.Slot:
                    if (t.Slot.Object != null)
                        return OpenEntityWindow(t.Slot.Object);
                    break;
              
                default:
                    break;
            }
            return null;
        }
        
        static Window OpenEntityWindow(GameObject obj)
        {
            if (obj.HasComponent<NpcComponent>())
            {
                var win = new Window() { Title = "not implemented" };// new AI.NpcUI(obj as Actor).ToWindow(obj.Name);
                win.SmartPosition();
                win.Show();
                return win;
            }
            else
            {
                if (OpenWindows.TryGetValue(obj, out Window existing))
                    return existing;
                existing = new WindowTargetManagement();
                existing.Tag = obj;
                obj.GetManagementInterface(existing as WindowTargetManagement);
                existing.Show();
                return existing;
            }
        }
        
        public override bool Show()
        {
            this.Location = UIManager.Mouse;
            OpenWindows[this.Tag] = this;
            return base.Show();
        }
        public override bool Hide()
        {
            var obj = this.Tag;
            OpenWindows.Remove(obj);
            this.PanelInfo.ClearControls();
            this.PanelActions.ClearControls();
            return base.Hide();
        }
    }
}
