using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.Construction.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Constructions
{
    public abstract class ConstructionCategory : INamed
    {
        TerrainWindowNew _Window;
        protected TerrainWindowNew Window
        {
            get
            {
                if (this._Window == null)
                    this._Window = new TerrainWindowNew(this);// new ConstructionsWindowNewNew(this);
                return this._Window;
            }
        }
        public TerrainWindowNew GetWindow()
        {
            return this.Window;
        }
        public abstract string Name { get; }
        public ToolDrawing Tool;
        public HashSet<BlockRecipe> List = new HashSet<BlockRecipe>();
        //protected ConstructionsWindowNewNew WindowConstructions;

        public void Add(BlockRecipe constr)
        {
            constr.Category = this;
            this.List.Add(constr);
        }

        public abstract IconButton GetButton();
        //public abstract ToolDrawing GetTool(Action<ToolDrawingSinglePreview.Args> callback, Block block);

        //public void CallBack(ToolDrawing.Args a)
        //{
        //    var data = Network.Serialize(w =>
        //    {
        //        w.Write(Player.Actor.InstanceID);
        //        //this.SelectedItem.Write(w);
        //        this.Getter().Write(w);
        //        a.Write(w);
        //    });
        //    Client.Instance.Send(PacketType.PlaceWallConstruction, data);
        //}

        //public abstract ToolDrawing GetTool(BlockConstruction.ProductMaterialPair item);

        static protected void CallBack(Func<BlockRecipe.ProductMaterialPair> itemGetter, ToolDrawing.Args a)
        {
            PacketDesignateConstruction.Send(Client.Instance, itemGetter(), a);
            //var data = Network.Serialize(w =>
            //{
            //    w.Write(Player.Actor.InstanceID);
            //    item.Write(w);
            //    a.Write(w);
            //});
            //Client.Instance.Send(PacketType.PlaceWallConstruction, data);
        }

        public abstract ToolDrawing GetTool(Func<BlockRecipe.ProductMaterialPair> itemGetter);
        public abstract List<ToolDrawing> GetAvailableTools(Func<BlockRecipe.ProductMaterialPair> itemGetter);

        static public UITools PanelTools;
        public UITools GetPanelTools(Func<BlockRecipe.ProductMaterialPair> itemGetter)
        {
            if (PanelTools == null)
                PanelTools = new UITools();
            PanelTools.Refresh(this.GetAvailableTools(itemGetter), itemGetter);
            return PanelTools;
        }
        static public Window WindowToolsBox;
        static public UIToolsBox ToolsBox;

        //public Window GetPanelToolsBox(Func<BlockConstruction.ProductMaterialPair> itemGetter)
        //{
        //    if (WindowToolsBox == null)
        //    {
        //        ToolsBox =new UIToolsBox(itemGetter());
        //        WindowToolsBox = ToolsBox.ToWindow("Brushes");
        //    }
        //    ToolsBox.Refresh(this.GetAvailableTools(itemGetter));//, itemGetter);
        //    return WindowToolsBox;
        //}

        internal ToolDrawing CreateTool(Type toolType, BlockRecipe.ProductMaterialPair productMaterialPair)
        {
            var tools = GetAvailableTools(() => productMaterialPair);
            var tool = tools.First(t => t.GetType() == toolType);
            tool.Block = productMaterialPair.Block;
            tool.State = productMaterialPair.Data;
            return tool;
        }
    }
}
