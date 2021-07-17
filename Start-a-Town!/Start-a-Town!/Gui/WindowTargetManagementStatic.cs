using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class WindowTargetManagementStatic : Window
    {
        static WindowTargetManagementStatic _Instance;
        public static WindowTargetManagementStatic Instance => _Instance ??= new WindowTargetManagementStatic();

        public Panel PanelInfo, PanelActions;

        WindowTargetManagementStatic()
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
            switch(t.Type)
            {
                case TargetType.Entity:
                    if (t.Object.HasComponent<NpcComponent>())
                    {
                        var ui = AI.NpcUIStatic.Instance;
                        var actor = t.Object as Actor;
                        var win = ui.GetWindow();
                        if (win == null)
                        {
                            win = ui.ToWindow("test");
                            win.SmartPosition();
                        }
                        ui.Refresh(actor);
                        win.Show();
                        win.Title = actor.Name;
                        return win;
                    }
                    else
                    {
                        var existing = new WindowTargetManagementStatic() { Tag = t.Object };
                        t.Object.GetManagementInterface(existing as WindowTargetManagementStatic);
                        existing.Show();
                        return existing;
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
                var ui = AI.NpcUIStatic.Instance;
                var win = ui.GetWindow();
                if (win == null)
                {
                    win = ui.ToWindow();
                    win.SmartPosition();
                    win.Show();
                }
                ui.Refresh(obj as Actor);
                win.Title = obj.Name;
                return win;
            }
            else
            {
                return null;
            }
        }
    }
}
