using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Control;

namespace Start_a_Town_.UI
{
    class BlocksWindow : Window
    {
        static BlocksWindow _Instance;
        public static BlocksWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new BlocksWindow();
                return _Instance;
            }
        }

        void refreshBpList()
        {
            this.List_Costructions.Build(GetAvailableBlocks(), foo => foo.Type.ToString(), (tag, btn) =>
            {
                //btn.LeftClickAction = () =>
                //{
                //    this.SelectedBlock = tag;
                //    RefreshProductVariants(panel_Reagents, panel_Selected, tag, matSlots);
                //};
                BtnInitializer(btn);
            });
            //this.List_Costuctions.Build(GetAvailableBlueprints(), foo => foo.Name, (r, btn) =>
            //{
                    
            //    btn.LeftClickAction = () =>
            //    {
            //        selected = r;
            //        RefreshProductVariants(panel_Reagents, panel_Selected, r, matSlots);
            //    };
            //    BtnInitializer(btn);
            //});
        }

       
        ListBox<Block, Button> List_Costructions = new ListBox<Block, Button>(new Rectangle(0, 0, 150, 200));
        ListBox<Reaction.Product.ProductMaterialPair, Button> List_Variations = new ListBox<Reaction.Product.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
        Block SelectedBlock;

        Panel panel_Reagents;// = new Panel() { };
        Panel panel_Selected;
        List<GameObjectSlot> matSlots;
        //ListBox<GameObject, Button> List_Variations = new ListBox<GameObject, Button>(new Rectangle(0, 0, 150, 200));
        BlocksWindow()
        {
            this.Title = "Constructions";
            this.AutoSize = true;

            //var list = new ListBox<Reaction, Button>(new Rectangle(0, 0, 150, 200));
            //var list = new ListBox<Block, Button>(new Rectangle(0, 0, 150, 200));
            this.List_Variations = new ListBox<Reaction.Product.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
            panel_Selected = new Panel();

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

        private static void BtnInitializer(Button btn)
        {
            EmptyTool tool = new EmptyTool();
            tool.LeftClick = (target, face) =>
            {
                if (target.IsNull())
                    return ControlTool.Messages.Default;
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                {
                    if (target.HasComponent<Components.ConstructionFootprint>())
                    {
                        Net.Client.RemoveObject(target);
                        return ControlTool.Messages.Default;
                    }
                    return ControlTool.Messages.Default;
                }
                Block block = (Block)btn.Tag;
                //Net.Client.PlaceConstruction(block.Type, 0, target.Global + face);
                return ControlTool.Messages.Default;
            };

            //tool.DrawAction = (sb, cam) =>
            //{
            //    if (tool.TargetOld.IsNull())
            //        return;
            //    if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            //    {
            //        Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
            //        sb.Draw(UIManager.Icons16x16, loc + Vector2.UnitX * 8, new Rectangle(0, 0, 16, 16), Color.White);
            //        return;
            //    }
            //    GameObject.Objects[GameObject.Types.CobblestoneItem].DrawPreview(sb, cam, tool.TargetOld.Global + tool.Face, (tool.TargetOld.Global + tool.Face).GetDrawDepth(Engine.Map, cam));//GetDepth(cam));
            //};
            tool.DrawActionMy = (sb, cam) =>
            {
                if (tool.TargetOld.IsNull())
                    return;
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                {
                    Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                    //sb.Draw(UIManager.Icons16x16, loc + Vector2.UnitX * 8, new Rectangle(0, 0, 16, 16), Color.White);
                    return;
                }
                //GameObject.Objects[GameObject.Types.CobblestoneItem].DrawPreview(sb, cam, tool.TargetOld.Global + tool.Face, (tool.TargetOld.Global + tool.Face).GetDrawDepth(Engine.Map, cam));//GetDepth(cam));
                Vector3 global = (tool.TargetOld.Global + tool.Face);
                Sprite sprite = GameObject.Objects[GameObject.Types.CobblestoneItem].GetSprite();
                Rectangle bounds = cam.GetScreenBounds(global, sprite.GetBounds());
                Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
                float depth = global.GetDrawDepth(Engine.Map, cam);
                sprite.Draw(sb, screenLoc, Color.White * 0.5f, 0, Vector2.Zero, cam.Zoom, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, depth);
            };
            btn.LeftClickAction = () =>
            {
                ToolManager.Instance.ActiveTool = tool;
            };
        }

        private List<Block> GetAvailableBlocks()
        {
            return (from block in Block.Registry.Values
                    from r in Reaction.Dictionary.Values
                    //where r.Products.First().Type == (int)block.Entity
                    where r.Products.First().Type == block.EntityID
                    select block).ToList();
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
        private void RefreshProductVariants(Panel panel_Reagents, Panel panel_Selected, Block tag, List<GameObjectSlot> matSlots)
        {
            throw new NotImplementedException();
        }

        private void RefreshProductVariants(Panel panelVariants, Panel panelSelected, Reaction reaction, List<GameObjectSlot> slots)
        {
            //panelVariants.Controls.Clear();
            //var list = r.Reagents.First().GetMaterials();
            //this.List_Variations.Build(list, p => p.Name, (p, btn) =>
            this.List_Variations.Build(reaction.Products.First().Create(reaction), p => p.Req.ObjectID.ToString(), (p, btn) =>
            {
                btn.LeftClickAction = () => RefreshSelectedPanel(panelSelected, p);// RefreshSelectedPanel(panelSelected, obj);
            });
            return;
        }
        private void RefreshSelectedPanel(Panel panel_Selected, GameObject product)// Reaction reaction)
        {
            panel_Selected.Controls.Clear();
            panel_Selected.Tag = product;
            if (product.IsNull())
                return;

            //CraftingTooltip tip = new CraftingTooltip(product.ToSlot(), new ItemRequirement(GameObject.Types.WoodenPlank, 1));
            //panel_Selected.Controls.Add(tip);
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
            RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, matList), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
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
    }
}
