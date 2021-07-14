using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Constructions
{
    public abstract class ConstructionCategory : INamed
    {
        TerrainWindowNew _Window;
        protected TerrainWindowNew Window => this._Window ??= new TerrainWindowNew(this);
      
        public abstract string Name { get; }
        public ToolDrawing Tool;
        public HashSet<BlockRecipe> List = new HashSet<BlockRecipe>();

        public void Add(BlockRecipe constr)
        {
            constr.Category = this;
            this.List.Add(constr);
        }

        public abstract IconButton GetButton();
       
        static protected void CallBack(Func<BlockRecipe.ProductMaterialPair> itemGetter, ToolDrawing.Args a)
        {
            PacketDesignateConstruction.Send(Client.Instance, itemGetter(), a);
        }

        public abstract ToolDrawing GetTool(Func<BlockRecipe.ProductMaterialPair> itemGetter);
        public abstract List<ToolDrawing> GetAvailableTools(Func<BlockRecipe.ProductMaterialPair> itemGetter);

        static public UITools PanelTools;
        public UITools GetPanelTools(Func<BlockRecipe.ProductMaterialPair> itemGetter)
        {
            PanelTools ??= new UITools();
            PanelTools.Refresh(this.GetAvailableTools(itemGetter), itemGetter);
            return PanelTools;
        }
        static public Window WindowToolsBox;
        static public UIToolsBox ToolsBox;

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
