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
    class ConstructionsWindowNew : Window
    {
        static ConstructionsWindowNew _Instance;
        public static ConstructionsWindowNew Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ConstructionsWindowNew();
                return _Instance;
            }
        }

        List<BlockRecipe> PopuplateList()
        {
            return
                (from block in Block.Registry
                 let recipe = block.Value.GetRecipe()
                 where recipe != null
                 select recipe).ToList();
        }

        void refreshBpList()
        {
            var list = this.PopuplateList();
            this.List_Costructions.Build(
                //BlockConstruction.Dictionary.Values,
                list,
                foo => foo.Name, (tag, btn) =>
            {
                btn.LeftClickAction = () => VariationInitializer(tag, btn);
                //BtnInitializer(btn);
            });
        }

        BlockRecipe.ProductMaterialPair SelectedItem;
        ListBox<BlockRecipe, Button> List_Costructions = new ListBox<BlockRecipe, Button>(new Rectangle(0, 0, 150, 200));
        ListBox<BlockRecipe.ProductMaterialPair, Button> List_Variations = new ListBox<BlockRecipe.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
        Block SelectedBlock;

        Panel panel_Reagents;// = new Panel() { };
        Panel panel_Selected;
        List<GameObjectSlot> matSlots;
        ConstructionsWindowNew()
        {
            this.Title = "Constructions";
            this.AutoSize = true;
            this.Movable = true;

            this.List_Variations = new ListBox<BlockRecipe.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
            panel_Selected = new Panel();// { AutoSize = true };

            panel_Reagents = new Panel() { };
            panel_Reagents.ClientSize = this.List_Costructions.Size;

            matSlots = new List<GameObjectSlot>();
            //refreshBpList();


            //RadioButton
            //    rd_All = new RadioButton("All", Vector2.Zero, true)
            //    {
            //        LeftClickAction = () =>
            //        {
            //            refreshBpList();
            //        }
            //    },
            //    rd_MatsReady = new RadioButton("Have Materials", rd_All.TopRight)
            //    {
            //        LeftClickAction = () =>
            //        {
            //            //list.Build(GetAvailableBlueprints(parent).FindAll(foo => BlueprintComponent.MaterialsAvailable(foo, this.Slots)), foo => foo.Name, RecipeListControlInitializer(panel_Selected));
            //        }
            //    };

            //Panel panel_filters = new Panel() { Location = this.Client.Controls.BottomLeft, AutoSize = true };
            //panel_filters.Controls.Add(rd_All, rd_MatsReady);

            //Panel panel_List = new Panel() { Location = panel_filters.BottomLeft, AutoSize = true };
            Panel panel_List = new Panel() { AutoSize = true };

            panel_List.Controls.Add(this.List_Costructions);
            panel_Reagents.Controls.Add(this.List_Variations);
            panel_Reagents.Location = panel_List.TopRight;


            panel_Selected.Location = panel_List.BottomLeft;

            panel_Selected.Size = new Rectangle(0, 0, panel_List.Size.Width + panel_Reagents.Size.Width, panel_Reagents.Size.Height);

            this.Client.Controls.Add(
                //panel_filters, 
                panel_List, panel_Reagents, panel_Selected);//, btn_craft);
        }
        void VariationInitializer(BlockRecipe constr, Button btn)
        {
            this.List_Variations.Build(constr.GetVariants(), item => item.Req.ObjectID.GetObject().Name, (item, ctrl) =>
            {
                BtnInitializer(item, ctrl);
            });          
        }
        private static void BtnInitializer(BlockRecipe.ProductMaterialPair item, Button btn)
        {
            //EmptyTool tool = new EmptyTool();
            //tool.LeftClick = (target) =>
            //{
            //    if (target.IsNull())
            //        return ControlTool.Messages.Default;
            //    if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            //    {
            //        var entity = target.Object;
            //        if (entity == null)
            //            return ControlTool.Messages.Default;
            //        if (entity.HasComponent<Components.ConstructionComponent>())
            //        {
            //            Client.RemoveObject(entity);
            //            return ControlTool.Messages.Default;
            //        }
            //        return ControlTool.Messages.Default;
            //    }
            //    //Client.PlayerConstruct(item, target.Global + face);
            //    var cell = Client.Instance.Map.GetCell(target.Global);
            //    var targetposition = cell.IsSolid() ? target.FaceGlobal : target.Global;
            //    //Instance.PlayerConstruct(item, targetposition, InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey), InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey));
            //    Instance.PlayerConstruct(item, tool.Target, InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey), InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey));

            //    return ControlTool.Messages.Default;
            //};
            //tool.DrawActionMy = (sb, cam) =>
            //{
            //    if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            //    {
            //        Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
            //        return;
            //    }
            //    //if (tool.TargetOld.IsNull())
            //    //    return;
            //    //if (!tool.TargetOld.Exists)
            //    //    return;
            //    if (tool.Target == null)
            //        return;
            //    var atlastoken = item.Block.Variations.First();
            //    //var global = tool.TargetOld.Global + tool.Face;
            //    var global = tool.Target.FaceGlobal;
            //    var pos = cam.GetScreenPosition(global);
            //    var depth = global.GetDrawDepth(Engine.Map, cam);
            //    Game1.Instance.GraphicsDevice.Textures[0] = atlastoken.Atlas.Texture;
            //    Cell cell = new Cell();
            //    //cell.Variation = this.Variation;
            //    cell.BlockData = item.Data;
            //    //cell.Type = item.Product.Type;
            //    cell.SetBlockType(item.Block.Type);
            //    item.Block.Draw(sb, pos - Block.OriginCenter * cam.Zoom, Color.White, Vector4.One, Color.White * 0.5f, cam.Zoom, depth, cell);
            //    sb.Flush();
            //};
            btn.LeftClickAction = () =>
            {
                //ToolManager.Instance.ActiveTool = new PlaceBlockConstructionTool(item, Instance.PlayerConstruct);
                Instance.SelectedItem = item;

                InterfaceBlockDrawingTools.Refresh(() => Instance.SelectedItem);
                InterfaceBlockDrawingTools.Instance.Location = UIManager.Mouse;
                InterfaceBlockDrawingTools.Instance.Show();

                ToolManager.Instance.ActiveTool = new ToolPlaceWall(Instance.PlayerConstructNew){Mode = item.Block.Multi ? ToolPlaceWall.Modes.Single : ToolPlaceWall.Modes.Wall};
                Instance.panel_Selected.Controls.Clear();
                var box = Instance.Selected(item);
                Instance.panel_Selected.Controls.Add(box);//new Label(item.Product.Type.ToString()) { Location = Instance.panel_Selected.Controls.BottomLeft });
            };
            btn.TooltipFunc = t => t.Controls.Add(Instance.Selected(item));
        }

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
        void PlayerConstructNew(ToolPlaceWall.Args args)// Vector3 start, Vector3 end)
        {
            throw new Exception();
            //var data = Network.Serialize(w =>
            //{
            //    w.Write(Player.Actor.InstanceID);
            //    this.SelectedItem.Write(w);
            //    //w.Write(start);
            //    //w.Write(end);
            //    args.Write(w);
            //});
            //Client.Instance.Send(PacketType.PlaceWallConstruction, data);
        }
        public ConstructionsWindowNew Refresh(ConstructionCategory cat)
        {
            if (this.IsOpen && this.List_Costructions.List == cat.List)
            {
                // if window already open with the same list, close it
                this.Hide();
                return this;
            }

            this.List_Costructions.Build(
                    cat.List,
                    foo => foo.Name, (tag, btn) =>
                    {
                        btn.LeftClickAction = () => VariationInitializer(tag, btn);
                    });
            if (!this.IsOpen)
                this.ToggleSmart();
            return this;
        }
        public ConstructionsWindowNew Refresh(IEnumerable<BlockRecipe> constrs)
        {
            if (this.IsOpen && this.List_Costructions.List == constrs)
            {
                // if window already open with the same list, close it
                this.Hide();
                return this;
            }

            this.List_Costructions.Build(
                    constrs,
                    foo => foo.Name, (tag, btn) =>
                    {
                        btn.LeftClickAction = () => VariationInitializer(tag, btn);
                    });
            if (!this.IsOpen)
                this.ToggleSmart();
            return this;
        }
    }
}
