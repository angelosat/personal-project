using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public abstract class ConstructionCategory : INamed
    {
        GuiConstructionsBrowser _window;
        protected GuiConstructionsBrowser Window => this._window ??= new GuiConstructionsBrowser();
      
        public abstract string Name { get; }
        public ToolBlockBuild Tool;
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
       
        static protected void CallBack(Func<ProductMaterialPair> itemGetter, ToolBlockBuild.Args a)
        {
            PacketDesignateConstruction.Send(Client.Instance, itemGetter(), a);
        }

        public abstract IEnumerable<BuildToolDef> GetAvailableTools();

        public Func<ProductMaterialPair> ProductGetter;

        static public Window WindowToolsBox;
        static public UIToolsBox ToolsBox;

        internal ToolBlockBuild GetTool(BuildToolDef toolDef, ProductMaterialPair productMaterialPair)
        {
            var tool = toolDef.Create(a => CallBack(() => productMaterialPair, a)); // TODO improve
            tool.Block = productMaterialPair.Block;
            tool.Material = productMaterialPair.Material;
            tool.State = productMaterialPair.Data;
            return tool;
        }
    }
}
