using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Towns.Constructions;
//namespace Start_a_Town_.UI
namespace Start_a_Town_.Modules.Construction.UI
{
    class ConstructionsWindowNewNew : Window
    {
        //static ConstructionsWindowNewNew _Instance;
        //public static ConstructionsWindowNewNew Instance
        //{
        //    get
        //    {
        //        if (_Instance == null)
        //            _Instance = new ConstructionsWindowNewNew();
        //        return _Instance;
        //    }
        //}
        static ConstructionsWindowNewNew CurrentlyOpen;
        public override bool Show()
        {
            var result = base.Show();
            if (result)
            {
                if (CurrentlyOpen != null)
                {
                    this.Location = CurrentlyOpen.Location;
                    CurrentlyOpen.Hide();
                }
                CurrentlyOpen = this;
            }
            else
                CurrentlyOpen = null;
            return result;
        }
        List<BlockRecipe> PopulateList()
        {
            return
                (from block in Block.Registry
                 let recipe = block.Value.GetRecipe()
                 where recipe != null
                 select recipe).ToList();
        }

        void refreshBpList()
        {
            var list = this.PopulateList();
            this.List_Constructions.Build(
                //BlockConstruction.Dictionary.Values,
                list,
                foo => foo.Name, (tag, btn) =>
            {
                btn.LeftClickAction = () => VariationInitializer(tag, btn);
                //BtnInitializer(btn);
            });
        }

        BlockRecipe.ProductMaterialPair SelectedItem;
        ListBox<BlockRecipe, Button> List_Constructions = new ListBox<BlockRecipe, Button>(new Rectangle(0, 0, 150, 200));
        //ListBox<BlockConstruction.ProductMaterialPair, Button> List_Variations = new ListBox<BlockConstruction.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
        Block SelectedBlock;

        Panel Panel_Reagents;// = new Panel() { };
        Panel Panel_Selected;
        List<GameObjectSlot> matSlots;
        public ConstructionsWindowNewNew(ConstructionCategory cat)
        {
            this.Title = "Constructions";
            this.AutoSize = true;
            this.Movable = true;

            //this.List_Variations = new ListBox<BlockConstruction.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
            Panel_Selected = new Panel();// { AutoSize = true };

            Panel_Reagents = new Panel() { };
            Panel_Reagents.ClientSize = this.List_Constructions.Size;// new Rectangle(0, 0, this.List_Constructions.Size.Width / 2, this.List_Constructions.Size.Height);

            matSlots = new List<GameObjectSlot>();


            //Panel panel_filters = new Panel() { Location = this.Client.Controls.BottomLeft, AutoSize = true };
            //panel_filters.Controls.Add(rd_All, rd_MatsReady);

            this.List_Constructions.Build(
                    cat.List,
                    foo => foo.Name, (tag, btn) =>
                    {
                        ListBoxNew<BlockRecipe.ProductMaterialPair, Label> reagentsList = new ListBoxNew<BlockRecipe.ProductMaterialPair, Label>();
                        reagentsList.Build(tag.GetVariants(), item => item.Req.ObjectID.GetObject().Name, (item, ctrl) => { });// var tool = cat.GetTool(item); });
                        reagentsList.SelectItem(0);
                        
                        btn.Tag = reagentsList;
                        btn.LeftClickAction = () => 
                        { 
                            VariationInitializer(tag, btn);
                            //var tool = cat.GetTool(() => reagentsList.SelectedItem);
                            //ToolManager.SetTool(tool);
                            //var windowtools = cat.GetPanelTools(() => reagentsList.SelectedItem).ToWindow("Tools");
                            var windowtools = cat.GetPanelTools(() => reagentsList.SelectedItem);
                            //if (windowtools.Tools.Count == 1)
                            //{
                            //    windowtools.Hide();
                            //}
                            //else
                            //{
                                windowtools.Location = this.BottomLeft;// UIManager.Mouse;
                                //windowtools.Movable = true;
                                windowtools.Show();
                            //}
                        };
                    });

            //Panel panel_List = new Panel() { Location = panel_filters.BottomLeft, AutoSize = true };
            Panel panel_List = new Panel() { AutoSize = true };

            panel_List.Controls.Add(this.List_Constructions);
            //panel_Reagents.Controls.Add(this.List_Variations);
            Panel_Reagents.Location = panel_List.BottomLeft;// panel_List.TopRight;


            Panel_Selected.Location = panel_List.BottomLeft;

            Panel_Selected.Size = new Rectangle(0, 0, panel_List.Size.Width + Panel_Reagents.Size.Width, Panel_Reagents.Size.Height);

            this.Client.Controls.Add(
                //panel_filters, 
                panel_List, Panel_Reagents
                //, Panel_Selected
                );//, btn_craft);
        }
        void VariationInitializer(BlockRecipe constr, Button btn)
        {
            var list = btn.Tag as ListBoxNew<BlockRecipe.ProductMaterialPair, Label>;
            Panel_Reagents.ClearControls();
            Panel_Reagents.AddControls(list);

         

            //this.List_Variations.Build(constr.GetVariants(), item => item.Req.ObjectID.GetObject().Name, (item, ctrl) =>
            //{
            //    BtnInitializer(item, ctrl);
            //});          
        }
        //private void BtnInitializer(BlockConstruction.ProductMaterialPair item, Button btn)
        //{
            
        //    btn.LeftClickAction = () =>
        //    {
        //        //ToolManager.Instance.ActiveTool = new PlaceBlockConstructionTool(item, Instance.PlayerConstruct);
        //        this.SelectedItem = item;

        //        InterfaceBlockDrawingTools.Refresh(() => this.SelectedItem);
        //        InterfaceBlockDrawingTools.Instance.Location = UIManager.Mouse;
        //        InterfaceBlockDrawingTools.Instance.Show();

        //        ToolManager.Instance.ActiveTool = new ToolPlaceWall(this.PlayerConstructNew) { Mode = item.Block.Multi ? ToolPlaceWall.Modes.Single : ToolPlaceWall.Modes.Wall };
        //        this.panel_Selected.Controls.Clear();
        //        var box = this.Selected(item);
        //        this.panel_Selected.Controls.Add(box);//new Label(item.Product.Type.ToString()) { Location = Instance.panel_Selected.Controls.BottomLeft });
        //    };
        //    btn.TooltipFunc = t => t.Controls.Add(this.Selected(item));
        //}

        GroupBox Selected(BlockRecipe.ProductMaterialPair item)
        {
            GroupBox box = new GroupBox();
            PanelLabeled panelproduct = new PanelLabeled("Product") { AutoSize = true };
            Slot<Cell> slot = new Slot<Cell>() { Location = panelproduct.Controls.BottomLeft };
            slot.Tag = new Cell() { Block = item.Block, BlockData = item.Data };
            Label lbl = new Label(item.Block.Type.ToString()) { Location = slot.TopRight };
            panelproduct.Controls.Add(slot, lbl);

            PanelLabeled panelmats = new PanelLabeled("Materials") { AutoSize = true, Location = panelproduct.BottomLeft };
            SlotWithText mat = new SlotWithText() { Tag = item.Req.ObjectID.GetObject().ToSlotLink(), Location = panelmats.Controls.BottomLeft };
            mat.Slot.CornerTextFunc = (sl) => item.Req.AmountRequired.ToString();
            panelmats.Controls.Add(mat);

            box.Controls.Add(panelproduct, panelmats);
            return box;
        }

        //private List<Block> GetAvailableBlocks()
        //{
        //    return (from block in Block.Registry.Values
        //            from r in Reaction.Dictionary.Values
        //            where r.Products.First().Type == block.Entity
        //            select block).ToList();
        //}
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
        private void RefreshProductVariants(Panel panel_Reagents, Panel panel_Selected, Block tag, List<GameObjectSlot> matSlots)
        {
            throw new NotImplementedException();
        }

        //private void RefreshProductVariants(Panel panelVariants, Panel panelSelected, Reaction reaction, List<GameObjectSlot> slots)
        //{
        //    //panelVariants.Controls.Clear();
        //    //var list = r.Reagents.First().GetMaterials();
        //    //this.List_Variations.Build(list, p => p.Name, (p, btn) =>
        //    this.List_Variations.Build(reaction.Products.First().Create(reaction), p => p.Req.ObjectID.ToString(), (p, btn) =>
        //    {
        //        btn.LeftClickAction = () => RefreshSelectedPanel(panelSelected, p);// RefreshSelectedPanel(panelSelected, obj);
        //    });
        //    return;
        //}

        private void RefreshSelectedPanel(Panel panel_Selected, GameObject product)// Reaction reaction)
        {
            panel_Selected.Controls.Clear();
            panel_Selected.Tag = product;
            if (product.IsNull())
                return;

            CraftingTooltip tip = new CraftingTooltip(product.ToSlotLink(), new ItemRequirement(GameObject.Types.Campfire, 1));
            panel_Selected.Controls.Add(tip);
            return;
        }
        private void RefreshSelectedPanel(Panel panel_Selected, Reaction.Product.ProductMaterialPair product)// Reaction reaction)
        {
            panel_Selected.Controls.Clear();
            panel_Selected.Tag = product.Product;
            if (product.IsNull())
                return;

            CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlotLink(), product.Req);
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
                int amount = container.GetAmount(obj => obj.IDType == mat.IDType);
                reqs.Add(new ItemRequirement(mat.IDType, 1, amount));
            }
            //RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, matList), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
            RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, materials), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
        }

        private void RefreshSelectedPanel(Panel panel_Selected, GameObject product, List<ItemRequirement> materials)// Reaction reaction)
        {
            panel_Selected.Controls.Clear();
            panel_Selected.Tag = product;
            if (product.IsNull())
                return;

            CraftingTooltip tip = new CraftingTooltip(product.ToSlotLink(), materials);
            panel_Selected.Controls.Add(tip);
            return;
        }

        void PlayerConstruct(Components.Crafting.BlockRecipe.ProductMaterialPair construction, TargetArgs target, int orientation, bool designate, bool remove)
        {
            var data = Network.Serialize(w =>
            {
                //w.Write((int)type);
                //w.Write(data);
                w.Write(PlayerOld.Actor.RefID);
                construction.Write(w);
                //w.Write(global);
                target.Write(w);
                w.Write(orientation);
                w.Write(designate);
                w.Write(remove);
            });
            Net.Client.Instance.Send(PacketType.PlaceBlockConstruction, data);
            //Packet.Create(PacketID, PacketType.PlaceBlockConstruction, Network.Serialize(w =>
            //{
            //    //w.Write((int)type);
            //    //w.Write(data);
            //    w.Write(Player.Actor.InstanceID);
            //    construction.Write(w);
            //    w.Write(global);
            //})).BeginSendTo(Host, RemoteIP);
        }
        //void PlayerConstructNew(ToolPlaceWall.Args args)// Vector3 start, Vector3 end)
        //{
        //    var data = Network.Serialize(w =>
        //    {
        //        w.Write(Player.Actor.InstanceID);
        //        this.SelectedItem.Write(w);
        //        //w.Write(start);
        //        //w.Write(end);
        //        args.Write(w);
        //    });
        //    Client.Instance.Send(PacketType.PlaceWallConstruction, data);
        //}
        public ConstructionsWindowNewNew Refresh(ConstructionCategory cat)
        {
            if (this.IsOpen && this.List_Constructions.List == cat.List)
            {
                // if window already open with the same list, close it
                this.Hide();
                return this;
            }

            this.List_Constructions.Build(
                    cat.List,
                    foo => foo.Name, (tag, btn) =>
                    {
                        btn.LeftClickAction = () => VariationInitializer(tag, btn);
                    });
            if (!this.IsOpen)
                this.ToggleSmart();
            return this;
        }
        public ConstructionsWindowNewNew Refresh(IEnumerable<BlockRecipe> constrs)
        {
            if (this.IsOpen && this.List_Constructions.List == constrs)
            {
                // if window already open with the same list, close it
                this.Hide();
                return this;
            }

            this.List_Constructions.Build(
                    constrs,
                    foo => foo.Name, (tag, btn) =>
                    {
                        btn.LeftClickAction = () => VariationInitializer(tag, btn);
                    });
            if (!this.IsOpen)
                this.ToggleSmart();
            return this;
        }
        public override bool Hide()
        {
            if (ConstructionCategory.PanelTools!=null)
            ConstructionCategory.PanelTools.Hide();
            return base.Hide();
        }
    }
}
