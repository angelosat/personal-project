using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Towns.Stockpiles;

namespace Start_a_Town_.Towns
{
    public class TownManagerUI : Window
    {
        Town Town;
        //Panel PanelButtons;

        public TownManagerUI(Town town)
        {
            this.Title = "Town";
            this.Movable = true;
            this.Town = town;
            this.AutoSize = true;
            //this.Size = new Rectangle(0, 0, 400, 400);
            this.Location = Vector2.Zero;
            Panel panelTabs = new Panel() { AutoSize = true };
            var lastPoint = Vector2.Zero;

            var width = (int)town.TownComponents.Select(c => Button.GetWidth(UIManager.Font, c.Name)).Max();

            foreach (var comp in town.TownComponents)
            {
                var ui = comp.GetInterface();
                if (ui == null)
                    continue;
                var btn = new Button(comp.Name, width) { Location = panelTabs.Controls.BottomLeft };
                var win = new Window() { Title = comp.Name, AutoSize = true, Movable = true };
                win.Client.AddControls(ui);
                //win.CenterToScreen();
                //win.Location.X += UIManager.Width / 4f;
                

                btn.LeftClickAction =
                    () =>
                    {
                        //win.Location = UIManager.Mouse;
                        //win.ConformToScreen();
                        win.SmartPosition();
                        win.Toggle();
                    };
                panelTabs.Controls.Add(btn);
            }
            //this.PanelButtons = new Panel() { Location = panelTabs.BottomLeft, Size = new Microsoft.Xna.Framework.Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height - panelTabs.Height - this.Label_Title.Height) };
            //this.Client.Controls.Add(panelTabs, this.PanelButtons);
            //var rd = panelTabs.Controls.FirstOrDefault() as RadioButton;
            //if (rd != null)
            //    rd.PerformLeftClick();
            this.Client.AddControls(panelTabs);
        }


        public override void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            //this.Town.DrawBeforeWorld(sb, camera.Map, camera);
        }
    }
}
