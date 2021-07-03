using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Net;

namespace Start_a_Town_.Blocks.Smeltery
{
    class SmeltingInterfaceNew : GroupBox
    {
        ListBox<Reaction.Product.ProductMaterialPair, Button> List_Recipes = new(new Rectangle(0, 0, 150, 200));

        Panel Panel_Selected;
        Panel Panel_Reagents;
        SlotGrid SlotsInput, SlotsOutput, SlotsFuel;
        PanelLabeled PanelInput, PanelOutput, PanelFuel;
        Bar BarPower, BarProgress;
        Button Button;
        BlockSmelteryEntity Entity;
        Vector3 Global;

        public SmeltingInterfaceNew()
        {
            this.AutoSize = true;
            var list = new ListBox<Reaction, Button>(new Rectangle(0, 0, 150, 200));
            this.List_Recipes = new ListBox<Reaction.Product.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
            Panel_Selected = new Panel();

            Panel_Reagents = new ReagentPanel();
            Panel_Reagents.ClientSize = list.Size;

            List<GameObjectSlot> matSlots = new List<GameObjectSlot>();
            Reaction selected = null;
            Action refreshBpList = () =>
            {
                var bps = GetAvailableBlueprints();
                list.Build(bps, foo => foo.Name, (r, btn) =>
                {
                    btn.LeftClickAction = () =>
                    {
                        selected = r;
                        RefreshMaterialPicking(r);
                    };
                });
            };
            refreshBpList();

            RadioButton
                rd_All = new RadioButton("All", Vector2.Zero, true)
                {
                    LeftClickAction = () =>
                    {
                        refreshBpList();
                    }
                },
                rd_MatsReady = new RadioButton("Have Materials", rd_All.TopRight)
                {
                    LeftClickAction = () =>
                    {
                    }
                };

            Panel panel_filters = new Panel() { Location = this.Controls.BottomLeft, AutoSize = true };
            panel_filters.Controls.Add(rd_All, rd_MatsReady);

            Panel panel_List = new Panel() { Location = panel_filters.BottomLeft, AutoSize = true };
            panel_List.Controls.Add(list);
            Panel_Reagents.Controls.Add(this.List_Recipes);
            Panel_Reagents.Location = panel_List.TopRight;


            Panel_Selected.Location = panel_List.BottomLeft;

            Panel_Selected.Size = new Rectangle(0, 0, panel_List.Size.Width + Panel_Reagents.Size.Width, Panel_Reagents.Size.Height);
            
            this.Controls.Add(
                panel_filters,
                panel_List, 
                Panel_Reagents,
                Panel_Selected
                );
        }

        public SmeltingInterfaceNew Refresh(Vector3 global, BlockSmelteryEntity entity)
        {
            this.Entity = entity;
            this.Global = global;

            this.SlotsFuel = new SlotGrid(entity.Fuels.Slots, 4);
            this.SlotsInput = new SlotGrid(entity.Storage.Slots, 4);
            this.SlotsOutput = new SlotGrid(entity.Output.Slots, 4);

            this.PanelInput = new PanelLabeled("Input") { Location = this.Panel_Reagents.TopRight };
            this.SlotsInput = new SlotGrid(entity.Storage.Slots, 4, this.SlotInitializer) { Location = this.PanelInput.Controls.BottomLeft };
            this.PanelInput.Controls.Add(this.SlotsInput);

            this.PanelOutput = new PanelLabeled("Output") { Location = this.PanelInput.BottomLeft };
            this.SlotsOutput = new SlotGrid(entity.Output.Slots, 4, this.OutputSlotInitializer) { Location = this.PanelOutput.Controls.BottomLeft };
            this.PanelOutput.Controls.Add(this.SlotsOutput);

            this.PanelFuel = new PanelLabeled("Fuel") { Location = this.PanelOutput.BottomLeft };
            this.SlotsFuel = new SlotGrid(entity.Fuels.Slots, 4, this.SlotInitializer) { Location = this.PanelFuel.Controls.BottomLeft };
            this.PanelFuel.Controls.Add(this.SlotsFuel);

            var panelbarpower = new Panel() { Location = this.PanelFuel.BottomLeft, AutoSize = true };
            this.BarPower = new Bar() { Object = entity.Power, Name = "Power" };
            panelbarpower.Controls.Add(this.BarPower);
            this.Controls.Add(panelbarpower);

            var panelbarprogress = new Panel() { Location = panelbarpower.BottomLeft, AutoSize = true };
            this.BarProgress = new Bar() { Object = entity.SmeltProgress, Name = "Progress" };
            panelbarprogress.Controls.Add(this.BarProgress);
            this.Controls.Add(panelbarprogress);

            this.Button = new Button("Burn") { Location = panelbarprogress.BottomLeft, LeftClickAction = () => BurnFuel(global), TextFunc = () => this.Entity.State == BlockSmelteryEntity.States.Running ? "Stop" : "Start" };

            this.Controls.Add(this.PanelInput, this.PanelOutput, this.PanelFuel, panelbarpower, panelbarprogress, this.Button);
            return this;
        }

        void SlotInitializer(InventorySlot s)
        {
            s.LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) && s.Tag.StackSize > 1)
                {
                    SplitStackWindow.Instance.Refresh(new TargetArgs(this.Global, s.Tag)).Show();

                    return;
                }
                if (s.Tag.HasValue)
                {
                    DragDropManager.Create(new DragDropSlot(null, new TargetArgs(this.Global, s.Tag), new TargetArgs(this.Global, s.Tag), DragDropEffects.Move | DragDropEffects.Link));
                }
            };
            s.DragDropAction = (args) =>
            {
                var a = args as DragDropSlot;
                Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(this.Global, s.Tag), a.DraggedTarget.Slot.Object.StackSize);
                return DragDropEffects.Move;
            };
            s.RightClickAction = () =>
            {
                if (s.Tag.HasValue)
                    Client.PlayerSlotRightClick(new TargetArgs(this.Global), s.Tag.Object);
            };
        }
        void OutputSlotInitializer(InventorySlot s)
        {
            s.RightClickAction = () =>
            {
                if (s.Tag.HasValue)
                    Client.PlayerSlotRightClick(new TargetArgs(this.Global), s.Tag.Object);
            };
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.BlockEntityStateChanged:
                    var entity = e.Parameters[0] as BlockSmelteryEntity;
                    if (entity != this.Entity)
                        return;
                    this.Button.Invalidate();
                    break;

                default:
                    break;
            }
        }

        void BurnFuel(Vector3 entityGlobal)
        {
            Client.PlayerRemoteCall(new TargetArgs(entityGlobal), Message.Types.Start);
        }

        private void Client_GameEvent(object sender, GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.InventoryChanged:
                    this.RefreshSelectedPanel();
                    break;

                default:
                    break;
            }
        }

        private void RefreshMaterialPicking(Reaction reaction)
        {
            this.Panel_Reagents.Controls.Clear();
            this.Panel_Reagents.Controls.Add(new Label(reaction.Name) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            this.Panel_Reagents.Controls.Add(new Label(this.Panel_Reagents.Controls.BottomLeft, "Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            List<GameObjectSlot> mats = new();
            foreach (var reagent in reaction.Reagents)
            {
                GameObjectSlot matSlot = GameObjectSlot.Empty;
                matSlot.Name = reagent.Name;
                mats.Add(matSlot);
                Slot slot = new(this.Panel_Reagents.Controls.BottomLeft) { Tag = matSlot, CustomTooltip = true };
                slot.HoverFunc = () => { string t = ""; foreach (var filter in reagent.Conditions) { t += filter.ToString() + "\n"; } return t.TrimEnd('\n'); };
                Label matName = new Label(slot.TopRight, reagent.Name);
                slot.LeftClickAction = () =>
                {
                    MaterialPicker.Instance.Label.Text = "Material for: " + reagent.Name;
                    MaterialPicker.Instance.Show(UIManager.Mouse, reagent, o => { slot.Tag.Object = o; });
                };
                slot.RightClickAction = () =>
                {
                    slot.Tag.Clear();
                };
                matSlot.Filter = reagent.Filter;
                matSlot.ObjectChanged = o =>
                {
                    if ((from sl in mats where !sl.HasValue select sl).FirstOrDefault() != null)
                    {
                        this.Panel_Selected.Controls.Clear();
                        return;
                    }

                    var product = reaction.Products.First().GetProduct(reaction, mats);
                    RefreshSelectedPanel(product);

                    // set selected product in smeltery
                    List<ItemRequirement> reqs = product.Requirements;
                    var crafting = new CraftOperation(reaction.ID, reqs, null, null, this.Entity.Storage);
                    Client.PlayerRemoteCall(new TargetArgs(this.Global), Message.Types.SetProduct, w =>
                    {
                        w.Write(reaction.ID);
                        w.Write(reqs.Count);
                        foreach (var req in reqs)
                            req.Write(w);
                    });
                };
                this.Panel_Reagents.Controls.Add(slot, matName);
            }
        }

        protected virtual List<Reaction> GetAvailableBlueprints()
        {
            return (from reaction in Reaction.Dictionary.Values
                    where reaction.ValidWorkshops.Contains(IsWorkstation.Types.Smeltery)

                    select reaction).ToList();
        }
        
        private void RefreshSelectedPanel()
        {
            if (this.Panel_Selected.Tag is not Reaction.Product.ProductMaterialPair product)
                return;
            this.RefreshSelectedPanel(product);
        }
        private void RefreshSelectedPanel(Reaction.Product.ProductMaterialPair product)
        {
            var container = PlayerOld.Actor.GetComponent<PersonalInventoryComponent>().Slots.Slots;
            foreach (var item in product.Requirements)
            {
                int amountFound = 0;
                foreach (var found in from found in container where found.HasValue where (int)found.Object.IDType == item.ObjectID select found.Object)
                    amountFound += found.StackSize;
                item.AmountCurrent = amountFound;
            }

            this.Panel_Selected.Controls.Clear();
            this.Panel_Selected.Tag = product;
            if (product == null)
                return;

            CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlotLink(), product.Requirements);
            this.Panel_Selected.Controls.Add(tip);
            return;
        }
    }
}
