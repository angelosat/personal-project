using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Modules.Construction.UI
{
    class InterfaceBlockDrawingTools : Window
    {
        static InterfaceBlockDrawingTools _Instance;
        static public InterfaceBlockDrawingTools Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new InterfaceBlockDrawingTools();
                return _Instance;
            }
        }
        //BlockConstruction.ProductMaterialPair SelectedItem;
        Func<BlockRecipe.ProductMaterialPair> Getter;
        static public void Refresh(Func<BlockRecipe.ProductMaterialPair> getter)
        {
            Instance.Getter = getter;
            List<ToolDrawing> tools = new List<ToolDrawing>()
            {
                new ToolDrawingSingle(Instance.CallBack),
                new ToolDrawingLine(Instance.CallBack),
                new ToolDrawingEnclosure(Instance.CallBack),
                new ToolDrawingSinglePreview(Instance.CallBack, ()=> Instance.Getter().Block)
            };
            var list = new ListBoxNew<ToolDrawing, Label>();
            list.Build(tools, t => t.Name, (t, c) => c.LeftClickAction = () => ToolManager.SetTool(t));
            list.SelectItem(tools.First());
            Instance.Client.Controls.Clear();
            Instance.Client.AddControls(list);
        }
        public InterfaceBlockDrawingTools()//Func<BlockConstruction.ProductMaterialPair> getter)
        {
            this.Movable = true;
            this.AutoSize = true;
            this.Closable = true;
            //this.Getter = getter;
            //var buttonSingle = new Button("Single", 100);
            //var buttonLine = new Button("Line", 100);

            List<ToolDrawing> tools = new List<ToolDrawing>()
            {
                new ToolDrawingSingle(this.CallBack),
                new ToolDrawingLine(this.CallBack),
                new ToolDrawingEnclosure(this.CallBack)
                //,
                //new ToolDrawingSinglePreview(this.CallBack, this.Getter().Block)
            };
            var list = new ListBoxNew<ToolDrawing, Label>();
            list.Build(tools, t => t.Name, (t, c) => c.LeftClickAction = () => ToolManager.Instance.ActiveTool = t);
            list.SelectItem(tools.First());
            //this.AddControls(buttonSingle, buttonLine);
            this.Client.AddControls(list);
            this.Client.AlignTopToBottom();
        }

        //void BtnSingleAction()
        //{
        //    //ToolManager.Instance.ActiveTool = new ToolPlaceWall(Instance.PlayerConstructNew) { Mode = item.Block.Multi ? ToolPlaceWall.Modes.Single : ToolPlaceWall.Modes.Wall };
        //    ToolManager.Instance.ActiveTool = new ToolDrawingSingle(this.CallBack);
        //}

        public void CallBack(ToolDrawing.Args a)
        {
            PacketDesignateConstruction.Send(Net.Client.Instance, this.Getter(), a);

            //var data = Network.Serialize(w =>
            //{
            //    w.Write(Player.Actor.InstanceID);
            //    //this.SelectedItem.Write(w);
            //    this.Getter().Write(w);
            //    a.Write(w);
            //});
            //Client.Instance.Send(PacketType.PlaceWallConstruction, data);
        }
    }
}
