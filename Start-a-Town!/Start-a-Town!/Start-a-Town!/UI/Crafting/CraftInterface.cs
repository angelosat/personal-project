using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Control;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class CraftInterface : GroupBox
    {
        static CraftInterface _Instance;
        public static CraftInterface Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new CraftInterface();
                return _Instance;
            }
        }

        ListBox<Reaction.Product.ProductMaterialPair, Button> List_Variations = new ListBox<Reaction.Product.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
        Panel Panel_Selected;
        Panel Panel_Reagents;
        //Panel Panel_Tool;
        Button Btn_Build;
        Reaction SelectedReaction;

        CraftInterface()
        {
            Net.Client.Instance.GameEvent += Client_GameEvent;
            //this.Title = "Constructions";
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
                        this.SelectedReaction = r;
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
                panel_filters, panel_List, Panel_Reagents, Panel_Selected, Btn_Build);//, btn_craft);
        }

        private void Client_GameEvent(object sender, Net.GameEvent e)
        {
            switch(e.Type)
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
            if (product.IsNull())
                return;
            var container = Player.Actor.GetComponent<PersonalInventoryComponent>().Slots;
            Net.Client.PlayerCraftRequest(new Components.Crafting.Crafting(this.SelectedReaction.ID, product.Requirements, Player.Actor, product.Tool, container));
        }

        int UpdateFrequency = Engine.TargetFps;
        public override void Update()
        {
            base.Update();
            this.UpdateFrequency--;
            if (this.UpdateFrequency > 0)
                return;
            this.UpdateFrequency = Engine.TargetFps;
            this.UpdateAvailableMaterials();
        }

        private void UpdateAvailableMaterials()
        {
            var product = this.Panel_Selected.Tag as Reaction.Product.ProductMaterialPair;
            if (product.IsNull())
                return;
            var nearbyMaterials = Player.Actor.GetNearbyObjects(r => r <= 2).ConvertAll(o => o.ToSlot());
            var container = Player.Actor.GetComponent<PersonalInventoryComponent>().Children.Where(o => o.HasValue);
            var availableMats = nearbyMaterials.Concat(container);

            var tip = this.Panel_Selected.Controls.FirstOrDefault() as CraftingTooltip;
            if (tip == null)
                return;
            var reqs = tip.Requirements;
            foreach (var item in reqs)
            {
                var found = availableMats.FirstOrDefault(o => (int)o.Object.ID == item.ObjectID);
                if (found == null)
                    item.Amount = 0;
                else
                    item.Amount = found.StackSize;
            }
            foreach (var slot in from ctrl in tip.Slots where ctrl is SlotWithText select ctrl)
            {
                slot.Invalidate(true);
                //var currentMat = (slot.Tag as GameObjectSlot).Object;
                //if (currentMat == null)
                //    continue;
                //TODO: update slot text here with updated amount
            }
        }

        private void RefreshMaterialPicking(Reaction reaction)
        {
            this.Panel_Reagents.Controls.Clear();
            this.Panel_Reagents.Controls.Add(new Label(reaction.Name) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            this.Panel_Reagents.Controls.Add(new Label(this.Panel_Reagents.Controls.BottomLeft, "Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            List<GameObjectSlot> mats = new List<GameObjectSlot>();
            GameObjectSlot tool = GameObjectSlot.Empty;
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
                    //MaterialPicker.Instance.Label.Text = "Material for: " + reagent.Name;
                    //MaterialPicker.Instance.Show(UIManager.Mouse, reagent, o => { slot.Tag.Object = o; });
                    ItemPicker.Instance.Label.Text = "Choose: " + reagent.Name;
                    ItemPicker.Instance.Show(UIManager.Mouse, reagent.Pass, o => { slot.Tag.Object = o; });
                };
                slot.RightClickAction = () =>
                {
                    slot.Tag.Clear();
                };
                matSlot.Filter = reagent.Pass;
                matSlot.ObjectChanged = o =>
                {
                    CheckIfProduct(reaction, mats, tool);
                    //if ((from sl in mats where !sl.HasValue select sl).FirstOrDefault() != null)
                    //{
                    //    this.Panel_Selected.Controls.Clear();
                    //    return;
                    //}

                    //var product = reaction.Products.First().GetProduct(reaction, Player.Actor, mats, tool.Object);
                    ////var container = Player.Actor.GetComponent<PersonalInventoryComponent>().Children;
                    ////foreach(var item in product.Requirements)
                    ////{
                    ////    int amountFound = 0;
                    ////    foreach (var found in from found in container where found.HasValue where (int)found.Object.ID == item.ObjectID select found.Object)
                    ////        amountFound += found.StackSize;
                    ////    item.Amount = amountFound;
                    ////}
                    //RefreshSelectedPanel(product);

                    ////var container = Player.Actor.GetComponent<PersonalInventoryComponent>().Children;
                    ////this.CreateItemRequirements(reaction, mats, container);
                };
                this.Panel_Reagents.Controls.Add(slot, matName);
            }
                if (reaction.Skill != null)
                {
                    this.Panel_Reagents.Controls.Add(new Label(this.Panel_Reagents.Controls.BottomLeft, "Tool") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });

                    tool.Name = "Tool";
                    Slot toolSlot = new Slot(this.Panel_Reagents.Controls.BottomLeft) { Tag = tool, CustomTooltip = true };
                    toolSlot.HoverFunc = () => "Has ability: " + reaction.Skill.Skill.Name;
                    //{ string t = ""; foreach (var filter in reagent.Conditions) { t += filter.ToString() + "\n"; } return t.TrimEnd('\n'); };
                    //if(reaction.Skill.ToolRequired)
                    this.Panel_Reagents.Controls.Add(new Label(toolSlot.TopRight, reaction.Skill.ToolRequired ? "Required" : "Optional"));
                    toolSlot.LeftClickAction = () =>
                    {
                        ItemPicker.Instance.Label.Text = "Select tool";
                        ItemPicker.Instance.Show(UIManager.Mouse, o => SkillComponent.HasSkill(o, reaction.Skill.Skill), Player.Actor.GetChildren(), o => { toolSlot.Tag.Object = o; });
                    };
                    toolSlot.RightClickAction = () =>
                    {
                        toolSlot.Tag.Clear();
                    };
                    tool.ObjectChanged = o =>
                    {
                        CheckIfProduct(reaction, mats, tool);
                    };
                    this.Panel_Reagents.Controls.Add(toolSlot);
                }
        }

        private void CheckIfProduct(Reaction reaction, List<GameObjectSlot> mats, GameObjectSlot tool)
        {
            if ((from sl in mats where !sl.HasValue select sl).FirstOrDefault() != null)
            {
                this.Panel_Selected.Controls.Clear();
                return;
            }
            if (reaction.Skill != null)
                if (reaction.Skill.ToolRequired)
                    if (tool.Object == null)
                    {
                        this.Panel_Selected.Controls.Clear();
                        return;
                    }
            var product = reaction.Products.First().GetProduct(reaction, Player.Actor, mats, tool.Object);
            RefreshSelectedPanel(product);
        }

        protected virtual List<Reaction> GetAvailableBlueprints()
        {
            return (from reaction in Reaction.Dictionary.Values
                    //where reaction.Building == parent.ID
                    where reaction.Sites.Contains(Reaction.Site.Person)
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

        //private void RefreshProductVariants(Panel panelVariants, Panel panelSelected, Reaction reaction, List<GameObjectSlot> slots)
        //{
        //    this.List_Variations.Build(reaction.Products.First().Create(reaction), p => p.Req.ObjectID.ToString(), (p, btn) =>
        //    {
        //        btn.LeftClickAction = () =>
        //        {
        //            RefreshSelectedPanel(panelSelected, p);
        //            ScreenManager.CurrentScreen.ToolManager.ActiveTool = new Start_a_Town_.Control.BuildingPlacer(p);
        //        };
        //    });
        //    return;
        //}

        private void RefreshSelectedPanel(GameObject product, List<GameObjectSlot> mats)
        {
            this.Panel_Selected.Controls.Clear();
            this.Panel_Selected.Tag = product;
            if (product.IsNull())
                return;

            List<ItemRequirement> reqs = new List<ItemRequirement>();
            foreach (var mat in mats)
                reqs.Add(new ItemRequirement(mat.Object.ID, 1));
            CraftingTooltip tip = new CraftingTooltip(product.ToSlot(), reqs);
            this.Panel_Selected.Controls.Add(tip);
        }

        private void RefreshSelectedPanel()
        {
            var product = this.Panel_Selected.Tag as Reaction.Product.ProductMaterialPair;
            if (product.IsNull())
                return;
            this.RefreshSelectedPanel(product);
        }
        private void RefreshSelectedPanel(Reaction.Product.ProductMaterialPair product)
        {
            var nearbyMaterials = Player.Actor.GetNearbyObjects(r => r <= 2).ConvertAll(o=>o.ToSlot());

            var container = Player.Actor.GetComponent<PersonalInventoryComponent>().Children;
            var availableMats = nearbyMaterials.Concat(container);
            foreach (var item in product.Requirements)
            {
                int amountFound = 0;
                foreach (var found in from found in availableMats where found.HasValue where (int)found.Object.ID == item.ObjectID select found.Object)
                    amountFound += found.StackSize;
                item.Amount = amountFound;
            }

            this.Panel_Selected.Controls.Clear();
            this.Panel_Selected.Tag = product;//.Product;
            if (product.IsNull())
                return;

            //CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlot(), product.Req);
            CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlot(), product.Requirements);
            this.Panel_Selected.Controls.Add(tip);
            return;
        }
        private void RefreshSelectedPanel(Panel panel_Selected, Reaction.Product.ProductMaterialPair product)// Reaction reaction)
        {
            panel_Selected.Controls.Clear();
            panel_Selected.Tag = product.Product;
            if (product.IsNull())
                return;

            CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlot(), product.Req);
            panel_Selected.Controls.Add(tip);
            return;
        }
        private void RefreshSelectedPanel(Panel panel_Selected, Reaction reaction, List<GameObjectSlot> materials, List<GameObjectSlot> container)// Reaction reaction)
        {
            if (reaction.IsNull())
                return;
            var matList = (from s in materials select s.Object).ToList();
            List<ItemRequirement> reqs = new List<ItemRequirement>();
            foreach (var mat in matList)
            {
                int amount = container.GetAmount(obj => obj.ID == mat.ID);
                reqs.Add(new ItemRequirement(mat.ID, 1, amount));
            }

            RefreshSelectedPanel(reaction.Products.First().Create(reaction, materials), reqs);
        }
        private void RefreshSelectedPanel(GameObject product, List<ItemRequirement> materials)// Reaction reaction)
        {
            this.Panel_Selected.Controls.Clear();
            this.Panel_Selected.Tag = product;
            if (product.IsNull())
                return;

            CraftingTooltip tip = new CraftingTooltip(product.ToSlot(), materials);
            this.Panel_Selected.Controls.Add(tip);

            //panel_Selected.Controls.Clear();
            //panel_Selected.Tag = product;
            //if (product.IsNull())
            //    return;

            //CraftingTooltip tip = new CraftingTooltip(product.ToSlot(), materials);
            //panel_Selected.Controls.Add(tip);
            //return;
        }


        private void CreateItemRequirements(Reaction reaction, List<GameObjectSlot> materials, List<GameObjectSlot> container)// Reaction reaction)
        {
            if (reaction.IsNull())
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
                int amount = container.GetAmount(obj => obj.ID == mat.ID);
                reqs.Add(new ItemRequirement(mat.ID, 1, amount));
            }
            //RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, matList), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
            //RefreshSelectedPanel(reaction.Products.First().Create(reaction, materials), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
            RefreshSelectedPanel(reaction.Products.First().GetProduct(reaction, materials));
        }
    }
}
