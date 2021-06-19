using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class CraftingTooltip : GroupBox
    {
        public List<SlotWithText> Slots = new List<SlotWithText>();
        public IEnumerable<Components.ItemRequirement> Requirements;

        public CraftingTooltip(GameObjectSlot product, params Components.ItemRequirement[] materials) : this(product, materials.ToList()) { }
        public CraftingTooltip(GameObjectSlot product, IEnumerable<Components.ItemRequirement> materials)
        {
            this.Requirements = materials;

            PanelTitled panel = new PanelTitled("Product") { Location = this.Controls.BottomLeft, AutoSize = true };
            Slot icon = new Slot() { Tag = product };
            GroupBox box = new GroupBox() { Location = icon.TopRight };
            product.Object.GetInfo().OnTooltipCreated(product.Object, box);
            panel.AddControls(icon, box);
            this.Controls.Add(panel);

            PanelTitled panelMats = new PanelTitled("Materials") { Location = this.Controls.BottomLeft, AutoSize = true };
            //Label lblMats = new Label("Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            //panelMats.AddControls(lblMats);
            Vector2 nextPosition = Vector2.Zero;
            foreach (var req in materials)
            {
                //SlotWithText slotReq = new SlotWithText(panelMats.Controls.BottomLeft) { Tag = GameObject.Objects[req.ObjectID].ToSlot() };
                SlotWithText slotReq = new SlotWithText(nextPosition) { Tag = GameObject.Objects[req.ObjectID].ToSlotLink() };
                nextPosition = slotReq.TopRight;
                //slotReq.Slot.CornerTextFunc = o => req.Amount.ToString() + "/" + req.Max.ToString();
                slotReq.Slot.CornerTextFunc = o => req.AmountRequired.ToString();
                this.Slots.Add(slotReq);
                panelMats.AddControls(slotReq);
            }
            this.Controls.Add(panelMats);
        }
    }

    //class CraftingTooltip : GroupBox
    //{
    //    public List<SlotWithText> Slots = new List<SlotWithText>();
    //    public IEnumerable<Components.ItemRequirement> Requirements;

    //    public CraftingTooltip(GameObjectSlot product, params Components.ItemRequirement[] materials) : this(product, materials.ToList()) { }
    //    public CraftingTooltip(GameObjectSlot product, IEnumerable<Components.ItemRequirement> materials)
    //    {
    //        this.Requirements = materials;

    //        PanelLabeled panel = new PanelLabeled("Product") { Location = this.Controls.BottomLeft, AutoSize = true };
    //        Slot icon = new Slot() { Location = panel.Controls.BottomLeft, Tag = product };
    //        GroupBox box = new GroupBox() { Location = icon.TopRight };
    //        product.Object.GetInfo().GetTooltip(product.Object, box);
    //        panel.Controls.Add(icon, box);
    //        this.Controls.Add(panel);

    //        PanelLabeled panelMats = new UI.PanelLabeled("Materials") { Location = this.Controls.BottomLeft, AutoSize = true };
    //        Label lblMats = new Label("Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
    //        panelMats.Controls.Add(lblMats);
    //        Vector2 nextPosition = panelMats.Controls.BottomLeft;
    //        foreach (var req in materials)
    //        {
    //            //SlotWithText slotReq = new SlotWithText(panelMats.Controls.BottomLeft) { Tag = GameObject.Objects[req.ObjectID].ToSlot() };
    //            SlotWithText slotReq = new SlotWithText(nextPosition) { Tag = GameObject.Objects[req.ObjectID].ToSlot() };
    //            nextPosition = slotReq.TopRight;
    //            slotReq.Slot.CornerTextFunc = o => req.Amount.ToString() + "/" + req.Max.ToString();
    //            this.Slots.Add(slotReq);
    //            panelMats.Controls.Add(slotReq);
    //        }
    //        this.Controls.Add(panelMats);
    //    }

    //}
}
