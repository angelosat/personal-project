using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class WorkshopComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Workshop"; }
        }
        GameObjectSlot Blueprint { get { return (GameObjectSlot)this["Blueprint"]; } set { this["Blueprint"] = value; } }
        GameObject Product { get { return (GameObject)this["Product"]; } set { this["Product"] = value; } }

        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<ObjectEventArgs>> handlers)
        {
            base.GetUI(parent, ui, handlers);

            List<GameObject> availableRecipes = new List<GameObject>();
            // TODO: code to get known recipes of every npc in town

            List<GameObject> playerRecipes = new List<GameObject>();
            // TODO: code to get player known recipes

            //panels
            Panel panel_list, panel_selected, panel_buttons, panel_blueprint;

            panel_list = new Panel() { Size = new Rectangle(0, 0, 150, 150) };
            panel_selected = new Panel() { Location = panel_list.BottomLeft, Size = panel_list.Size };
            panel_blueprint = new Panel() { Location = panel_selected.BottomLeft, Size = panel_selected.Size };
            panel_buttons = new Panel() { Location = panel_blueprint.BottomLeft, AutoSize = true, Width = panel_selected.Width };

            ListBox<GameObject, Button> list_recipes = new ListBox<GameObject, Button>(panel_list.ClientSize)
                .Build(playerRecipes, foo => foo.Name, (obj, btn) =>
                {
                    if (!availableRecipes.Contains(obj))
                    {
                        // TODO: code to handle case where a blueprint is required for selected recipe, in case no npc has it memorized
                    }
                    panel_selected.Controls.Clear();
                    this.Product = obj;
                    panel_selected.Controls.Add(obj.GetTooltip());
                });
            panel_list.Controls.Add(list_recipes);

            Slot slot_bp = new Slot();
            panel_blueprint.Controls.Add(slot_bp);

            Button btn_clear = new Button(Vector2.Zero, panel_buttons.ClientSize.Width, "Clear") { LeftClickAction = () => { this.Product = null; panel_selected.Controls.Clear(); } };
            panel_buttons.Controls.Add(btn_clear);

            ui.Controls.Add(panel_list, panel_selected, panel_buttons, panel_blueprint);
        }

        public override object Clone()
        {
            return new WorkshopComponent();
        }
    }
}
