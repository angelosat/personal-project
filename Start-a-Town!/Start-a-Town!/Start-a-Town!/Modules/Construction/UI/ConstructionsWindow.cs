using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
//namespace Start_a_Town_.UI
namespace Start_a_Town_.Modules.Construction.UI
{
    class ConstructionsWindow : Window
    {
        static ConstructionsWindow _Instance;
        public static ConstructionsWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ConstructionsWindow();
                return _Instance;
            }
        }

        List<BlockConstruction> PopuplateList()
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


        ListBox<BlockConstruction, Button> List_Costructions = new ListBox<BlockConstruction, Button>(new Rectangle(0, 0, 150, 200));
        ListBox<BlockConstruction.ProductMaterialPair, Button> List_Variations = new ListBox<BlockConstruction.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
        Block SelectedBlock;

        Panel panel_Reagents;// = new Panel() { };
        Panel panel_Selected;
        List<GameObjectSlot> matSlots;
        //ListBox<GameObject, Button> List_Variations = new ListBox<GameObject, Button>(new Rectangle(0, 0, 150, 200));
        ConstructionsWindow()
        {
            this.Title = "Constructions";
            this.AutoSize = true;
            this.Movable = true;

            //var list = new ListBox<Reaction, Button>(new Rectangle(0, 0, 150, 200));
            //var list = new ListBox<Block, Button>(new Rectangle(0, 0, 150, 200));
            this.List_Variations = new ListBox<BlockConstruction.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
            panel_Selected = new Panel();// { AutoSize = true };

            panel_Reagents = new Panel() { };
            panel_Reagents.ClientSize = this.List_Costructions.Size;

            matSlots = new List<GameObjectSlot>();
            //Reaction selected = null;
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

            Panel panel_filters = new Panel() { Location = this.Client.Controls.BottomLeft, AutoSize = true };
            panel_filters.Controls.Add(rd_All, rd_MatsReady);

            Panel panel_List = new Panel() { Location = panel_filters.BottomLeft, AutoSize = true };
            panel_List.Controls.Add(this.List_Costructions);
            panel_Reagents.Controls.Add(this.List_Variations);
            panel_Reagents.Location = panel_List.TopRight;


            panel_Selected.Location = panel_List.BottomLeft;

            panel_Selected.Size = new Rectangle(0, 0, panel_List.Size.Width + panel_Reagents.Size.Width, panel_Reagents.Size.Height);

            this.Client.Controls.Add(
                panel_filters, panel_List, panel_Reagents, panel_Selected);//, btn_craft);
        }
        void VariationInitializer(BlockConstruction constr, Button btn)
        {
            this.List_Variations.Build(constr.GetVariants(), item => item.Req.ObjectID.GetObject().Name, (item, ctrl) =>
            {
                BtnInitializer(item, ctrl);
            });          
        }
        private static void BtnInitializer(BlockConstruction.ProductMaterialPair item, Button btn)
        {
            EmptyTool tool = new EmptyTool();
            tool.LeftClick = (target) =>
            {
                if (target.IsNull())
                    return ControlTool.Messages.Default;
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                {
                    var entity = target.Object;
                    if (entity == null)
                        return ControlTool.Messages.Default;
                    if (entity.HasComponent<Components.ConstructionComponent>())
                    {
                        Net.Client.RemoveObject(entity);
                        return ControlTool.Messages.Default;
                    }
                    return ControlTool.Messages.Default;
                }
                //Net.Client.PlayerConstruct(item, target.Global + face);
                var cell = Net.Client.Instance.Map.GetCell(target.Global);
                var targetposition = cell.IsSolid() ? target.FaceGlobal : target.Global;
                //Instance.PlayerConstruct(item, targetposition, InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey), InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey));
                Instance.PlayerConstruct(item, tool.Target, InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey), InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey));

                return ControlTool.Messages.Default;
            };
            tool.DrawActionMy = (sb, cam) =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                {
                    Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                    return;
                }
                //if (tool.TargetOld.IsNull())
                //    return;
                //if (!tool.TargetOld.Exists)
                //    return;
                if (tool.Target == null)
                    return;
                var atlastoken = item.Product.Variations.First();
                //var global = tool.TargetOld.Global + tool.Face;
                var global = tool.Target.FaceGlobal;
                var pos = cam.GetScreenPosition(global);
                var depth = global.GetDrawDepth(Engine.Map, cam);
                Game1.Instance.GraphicsDevice.Textures[0] = atlastoken.Atlas.Texture;
                Cell cell = new Cell();
                //cell.Variation = this.Variation;
                cell.BlockData = item.Data;
                //cell.Type = item.Product.Type;
                cell.SetBlockType(item.Product.Type);
                item.Product.Draw(sb, pos - Block.OriginCenter * cam.Zoom, Color.White, Vector4.One, Color.White * 0.5f, cam.Zoom, depth, cell);
                sb.Flush();
            };
            btn.LeftClickAction = () =>
            {
                //ToolManager.Instance.ActiveTool = tool;
                ToolManager.Instance.ActiveTool = new PlaceBlockConstructionTool(item, Instance.PlayerConstruct);
                Instance.panel_Selected.Controls.Clear();
                var box = Instance.Selected(item);
                Instance.panel_Selected.Controls.Add(box);//new Label(item.Product.Type.ToString()) { Location = Instance.panel_Selected.Controls.BottomLeft });
            };
            btn.TooltipFunc = t => t.Controls.Add(Instance.Selected(item));
        }

        GroupBox Selected(BlockConstruction.ProductMaterialPair item)
        {
            GroupBox box = new GroupBox();
            PanelLabeled panelproduct = new PanelLabeled("Product") { AutoSize = true };
            Slot<Cell> slot = new Slot<Cell>() { Location = panelproduct.Controls.BottomLeft };
            slot.Tag = new Cell() { Block = item.Product, BlockData = item.Data };
            Label lbl = new Label(item.Product.Type.ToString()) { Location = slot.TopRight };
            panelproduct.Controls.Add(slot, lbl);

            PanelLabeled panelmats = new PanelLabeled("Materials") { AutoSize = true, Location = panelproduct.BottomLeft };
            SlotWithText mat = new SlotWithText() { Tag = item.Req.ObjectID.GetObject().ToSlot(), Location = panelmats.Controls.BottomLeft };
            mat.Slot.CornerTextFunc = (sl) => item.Req.Max.ToString();
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

            CraftingTooltip tip = new CraftingTooltip(product.ToSlot(), new ItemRequirement(GameObject.Types.Campfire, 1));
            panel_Selected.Controls.Add(tip);
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
            //RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, matList), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
            RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, materials), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
        }

        private void RefreshSelectedPanel(Panel panel_Selected, GameObject product, List<ItemRequirement> materials)// Reaction reaction)
        {
            panel_Selected.Controls.Clear();
            panel_Selected.Tag = product;
            if (product.IsNull())
                return;

            CraftingTooltip tip = new CraftingTooltip(product.ToSlot(), materials);
            panel_Selected.Controls.Add(tip);
            return;
        }

        void PlayerConstruct(Components.Crafting.BlockConstruction.ProductMaterialPair construction, TargetArgs target, bool designate, bool remove)
        {
            var data = Network.Serialize(w =>
            {
                //w.Write((int)type);
                //w.Write(data);
                w.Write(Player.Actor.Network.ID);
                construction.Write(w);
                //w.Write(global);
                target.Write(w);
                w.Write(designate);
                w.Write(remove);
            });
            Net.Client.Instance.Send(PacketType.PlaceBlockConstruction, data);
            //Packet.Create(PacketID, PacketType.PlaceBlockConstruction, Network.Serialize(w =>
            //{
            //    //w.Write((int)type);
            //    //w.Write(data);
            //    w.Write(Player.Actor.Network.ID);
            //    construction.Write(w);
            //    w.Write(global);
            //})).BeginSendTo(Host, RemoteIP);
        }

    }
}
