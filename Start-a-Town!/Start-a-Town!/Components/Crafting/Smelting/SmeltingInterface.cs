using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components.Crafting.Smelting
{
    class SmeltingInterfaceNew : GroupBox
    {
        ListBox<Reaction.Product.ProductMaterialPair, Button> List_Variations = new ListBox<Reaction.Product.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
        Panel Panel_Selected;
        Panel Panel_Reagents;
        Button Btn_Build;
        SlotGrid SlotsInput, SlotsOutput, SlotsFuel;
        PanelLabeled PanelInput, PanelOutput, PanelFuel;
        Bar BarPower, BarProgress;
        Button Button;
        SmelteryComponent Smeltery;
        GameObject Entity;

        public SmeltingInterfaceNew()
        {
            //Client.Instance.GameEvent += Client_GameEvent;
            this.AutoSize = true;

            var list = new ListBox<Reaction, Button>(new Rectangle(0, 0, 150, 200));
            this.List_Variations = new ListBox<Reaction.Product.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
            Panel_Selected = new Panel();

            Panel_Reagents = new Panel() { };
            Panel_Reagents.ClientSize = list.Size;

            List<GameObjectSlot> matSlots = new List<GameObjectSlot>();
            Reaction selected = null;
            Action refreshBpList = () =>
            {
                list.Build(GetAvailableBlueprints(), foo => foo.Name, (r, btn) =>
                {
                    btn.LeftClickAction = () =>
                    {
                        selected = r;
                        RefreshMaterialPicking(r);
                        //RefreshProductVariants(Panel_Reagents, Panel_Selected, r, matSlots);
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
                        //list.Build(GetAvailableBlueprints(parent).FindAll(foo => BlueprintComponent.MaterialsAvailable(foo, this.Slots)), foo => foo.Name, RecipeListControlInitializer(panel_Selected));
                    }
                };

            Panel panel_filters = new Panel() { Location = this.Controls.BottomLeft, AutoSize = true };
            panel_filters.Controls.Add(rd_All, rd_MatsReady);

            Panel panel_List = new Panel() { Location = panel_filters.BottomLeft, AutoSize = true };
            panel_List.Controls.Add(list);
            Panel_Reagents.Controls.Add(this.List_Variations);
            Panel_Reagents.Location = panel_List.TopRight;


            Panel_Selected.Location = panel_List.BottomLeft;

            Panel_Selected.Size = new Rectangle(0, 0, panel_List.Size.Width + Panel_Reagents.Size.Width, Panel_Reagents.Size.Height);
            this.Btn_Build = new Button("Build", this.Panel_Selected.Width)
            {
                Location = this.Panel_Selected.BottomLeft,
                LeftClickAction = () =>
                {
                    Build();
                }
            };



            this.Controls.Add(
                panel_filters, panel_List, Panel_Reagents, Panel_Selected);//, Btn_Build);//, btn_craft);
        }

        public SmeltingInterfaceNew Refresh(GameObject parent)
        {
            SmelteryComponent comp = parent.GetComponent<SmelteryComponent>();
            this.Smeltery = comp;
            this.Entity = parent;

            this.SlotsFuel = new SlotGrid(comp.Fuels.Slots, 4);
            this.SlotsInput = new SlotGrid(comp.Input.Slots, 4);
            this.SlotsOutput = new SlotGrid(comp.Output.Slots, 4);

            this.PanelInput = new PanelLabeled("Input") { Location = this.Panel_Reagents.TopRight };
            this.SlotsInput = new SlotGrid(comp.Input.Slots, 4, this.SlotInitializer) { Location = this.PanelInput.Controls.BottomLeft };
            this.PanelInput.Controls.Add(this.SlotsInput);

            this.PanelOutput = new PanelLabeled("Output") { Location = this.PanelInput.BottomLeft };
            this.SlotsOutput = new SlotGrid(comp.Output.Slots, 4, this.OutputSlotInitializer) { Location = this.PanelOutput.Controls.BottomLeft };
            this.PanelOutput.Controls.Add(this.SlotsOutput);

            this.PanelFuel = new PanelLabeled("Fuel") { Location = this.PanelOutput.BottomLeft };
            this.SlotsFuel = new SlotGrid(comp.Fuels.Slots, 4, this.SlotInitializer) { Location = this.PanelFuel.Controls.BottomLeft };
            this.PanelFuel.Controls.Add(this.SlotsFuel);

            var panelbarpower = new Panel() { Location = this.PanelFuel.BottomLeft, AutoSize = true };
            this.BarPower = new Bar() { Object = comp.Power, Name = "Power" };
            panelbarpower.Controls.Add(this.BarPower);
            this.Controls.Add(panelbarpower);

            var panelbarprogress = new Panel() { Location = panelbarpower.BottomLeft, AutoSize = true };
            this.BarProgress = new Bar() { Object = comp.SmeltProgress, Name = "Progress" };
            panelbarprogress.Controls.Add(this.BarProgress);
            this.Controls.Add(panelbarprogress);

            this.Button = new Button("Burn") { Location = panelbarprogress.BottomLeft, LeftClickAction = () => BurnFuel(parent), TextFunc = () => this.Smeltery.State == States.Running ? "Stop" : "Start" };

            this.Controls.Add(this.PanelInput, this.PanelOutput, this.PanelFuel, panelbarpower, panelbarprogress, this.Button);
            return this;
        }

        void SlotInitializer(InventorySlot s)
        {
            s.DragDropAction = (args) =>
            {
                var a = args as DragDropSlot;
                Net.Client.PlayerInventoryOperationNew(a.Source, s.Tag, a.Slot.Object.StackSize);
                return DragDropEffects.Move;
            };
            s.RightClickAction = () =>
            {
                if (s.Tag.HasValue)
                    Net.Client.PlayerSlotInteraction(s.Tag);
            };
        }
        void OutputSlotInitializer(InventorySlot s)
        {
            s.RightClickAction = () =>
            {
                if (s.Tag.HasValue)
                    Net.Client.PlayerSlotInteraction(s.Tag);
            };
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.ObjectStateChanged:
                    var entity = e.Parameters[0] as GameObject;
                    if(entity!=this.Entity)
                        return;
                    this.Button.Invalidate();
                    break;

                default:
                    break;
            }
        }

        void BurnFuel(GameObject parent)
        {
            Net.Client.PlayerRemoteCall(new TargetArgs(parent), Message.Types.Start);
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

        private void Build()
        {
            if (this.Panel_Selected.Tag == null)
                return;
            Reaction.Product.ProductMaterialPair product = this.Panel_Selected.Tag as Reaction.Product.ProductMaterialPair;
            if (product is null)
                return;


            Net.Client.PlayerCraft(product);
        }

        private void RefreshMaterialPicking(Reaction reaction)
        {
            this.Panel_Reagents.Controls.Clear();
            this.Panel_Reagents.Controls.Add(new Label(reaction.Name) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            this.Panel_Reagents.Controls.Add(new Label(this.Panel_Reagents.Controls.BottomLeft, "Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            List<GameObjectSlot> mats = new List<GameObjectSlot>();
            foreach (var reagent in reaction.Reagents)
            {
                GameObjectSlot matSlot = GameObjectSlot.Empty;
                matSlot.Name = reagent.Name;
                mats.Add(matSlot);
                Slot slot = new Slot(this.Panel_Reagents.Controls.BottomLeft) { Tag = matSlot, CustomTooltip = true };
                slot.HoverFunc = () => { string t = ""; foreach (var filter in reagent.Conditions) { t += filter.ToString() + "\n"; } return t.TrimEnd('\n'); };//
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

                    var product = reaction.Products.First().GetProduct(reaction, mats.ToDictionary(v => v.Name, v => v.Object as Entity));
                    RefreshSelectedPanel(product);

                    // set selected product in smeltery
                    List<ItemRequirement> reqs = product.Requirements;// (from mat in mats select new ItemRequirement(mat.Object.ID, 1)).ToList();
                    var crafting = new CraftOperation(reaction.ID, reqs, this.Entity, null, this.Smeltery.Input);
                    //Client.PlayerRemoteCall(this.Entity, Message.Types.SetProduct, w => crafting.Write(w));
                    Net.Client.PlayerRemoteCall(new TargetArgs(this.Entity), Message.Types.SetProduct, w => crafting.WriteOld(w));


                    //Client.PlayerRemoteCall(this.Entity, Message.Types.SetProduct, w => product.Write(w));
                    //this.Smeltery.SelectedProduct = product;
                };
                this.Panel_Reagents.Controls.Add(slot, matName);
            }
        }

        protected virtual List<Reaction> GetAvailableBlueprints()
        {
            return (from reaction in Reaction.Dictionary.Values
                    //where reaction.Building == parent.ID
                    //where reaction.ValidWorkshops.Contains(ItemTemplate.Smeltery.ID)
                    where reaction.ValidWorkshops.Contains(IsWorkstation.Types.Smeltery)
                    select reaction).ToList();
        }
        private Action<Reaction, Button> RecipeListControlInitializer(Panel panel_Selected)
        {
            return (foo, btn) =>
            {
                btn.LeftClickAction = () =>
                {
                    Reaction obj = foo;
                    return;
                };
            };
        }

        private void RefreshSelectedPanel()
        {
            var product = this.Panel_Selected.Tag as Reaction.Product.ProductMaterialPair;
            if (product is null)
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
            this.Panel_Selected.Tag = product;//.Product;
            if (product is null)
                return;

            //CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlot(), product.Req);
            CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlotLink(), product.Requirements);
            this.Panel_Selected.Controls.Add(tip);
            return;
        }

        private void CreateItemRequirements(Reaction reaction, List<GameObjectSlot> materials, List<GameObjectSlot> container)// Reaction reaction)
        {
            if (reaction is null)
                return;
            var matList = (from s in materials where s.HasValue select s.Object).ToList();
            if (matList.Count < materials.Count)
            {
                this.Panel_Selected.Controls.Clear();
                return;
            }
            List<ItemRequirement> reqs = new List<ItemRequirement>();
            foreach (var mat in matList)
            {
                int amount = container.GetAmount(obj => obj.IDType == mat.IDType);
                reqs.Add(new ItemRequirement(mat.IDType, 1, amount));
            }
            //RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, matList), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
            //RefreshSelectedPanel(reaction.Products.First().Create(reaction, materials), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
            RefreshSelectedPanel(reaction.Products.First().GetProduct(reaction, materials.ToDictionary(v => v.Name, v => v.Object as Entity)));
        }


    }
}
