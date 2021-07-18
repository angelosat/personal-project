using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Constructions;

namespace Start_a_Town_
{
    public abstract class ConstructionCategory : INamed
    {
        TerrainWindowNew _window;
        protected TerrainWindowNew Window => this._window ??= new TerrainWindowNew();
      
        public abstract string Name { get; }
        public ToolDrawing Tool;
        public HashSet<Block> List = new();

        public void Add(Block constr)
        {
            constr.BuildProperties.Category = this;
            this.List.Add(constr);
        }
        public void Remove(Block constr)
        {
            constr.BuildProperties.Category = null;
            this.List.Remove(constr);
        }
        public abstract IconButton GetButton();
       
        static protected void CallBack(Func<ProductMaterialPair> itemGetter, ToolDrawing.Args a)
        {
            PacketDesignateConstruction.Send(Client.Instance, itemGetter(), a);
        }

        public abstract ToolDrawing GetTool(Func<ProductMaterialPair> itemGetter);
        public abstract List<ToolDrawing> GetAvailableTools(Func<ProductMaterialPair> itemGetter);

        static public UITools PanelTools;
        public UITools GetPanelTools(Func<ProductMaterialPair> itemGetter)
        {
            PanelTools ??= new UITools();
            PanelTools.Refresh(this.GetAvailableTools(itemGetter), itemGetter);
            return PanelTools;
        }
        static public Window WindowToolsBox;
        static public UIToolsBox ToolsBox;

        internal ToolDrawing CreateTool(Type toolType, ProductMaterialPair productMaterialPair)
        {
            var tools = GetAvailableTools(() => productMaterialPair);
            var tool = tools.First(t => t.GetType() == toolType);
            tool.Block = productMaterialPair.Block;
            tool.State = productMaterialPair.Data;
            return tool;
        }
    }
}
