using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class SmelteryInterface : GroupBox
    {
        PanelLabeled PanelContents, PanelFuel, PanelOre, PanelProduct;
        Panel PanelButtons;
        SlotGrid<Slot> SlotsContainer;
        Slot SlotFuel
            , SlotOre, SlotProduct
            ;
        Button BtnStart;
        //Label LblFuel;
        Bar BarPower, BarProgress;

        GameObject Smeltery;
        SmelteryComponent SmelteryComponent;

        public SmelteryInterface(GameObject smeltery)
        {
            this.Initialize(smeltery);
        }

        public void Initialize(GameObject smeltery)
        {
            this.Controls.Clear();
            if (!smeltery.TryGetComponent<SmelteryComponent>(out this.SmelteryComponent))
                return;
            this.Smeltery = smeltery;

            this.PanelContents = new PanelLabeled("Contents") { AutoSize = true };
            this.SlotsContainer = new SlotGrid<Slot>(this.SmelteryComponent.Children, 4) { Location = this.PanelContents.Controls.BottomLeft };
            this.PanelContents.Controls.Add(this.SlotsContainer);
            //this.Controls.Add(this.PanelContents);

            this.PanelOre = new PanelLabeled("Ore") { Location = this.Controls.BottomLeft, AutoSize = true };
            this.SlotOre = new Slot()
            {
                Location = this.PanelOre.Controls.BottomLeft,
                Tag = this.SmelteryComponent.Materials,
                DragDropCondition = this.SmelteryComponent.Materials.Filter,
                RightClickAction = () =>
                {
                    Net.Client.PlayerRemoteCall(new TargetArgs(smeltery), Message.Types.Retrieve, w => w.Write(this.SmelteryComponent.Materials.ID));
                }
            };
            this.PanelOre.Controls.Add(this.SlotOre);

            this.PanelFuel = new PanelLabeled("Fuel") { Location = this.PanelOre.BottomLeft, AutoSize = true };
            this.SlotFuel = new Slot()
            {
                Location = this.PanelFuel.Controls.BottomLeft,
                Tag = this.SmelteryComponent.Fuel,
                DragDropCondition = this.SmelteryComponent.Fuel.Filter,
                RightClickAction = () =>
                {
                    Net.Client.PlayerRemoteCall(new TargetArgs(smeltery), Message.Types.Retrieve, w => w.Write(this.SmelteryComponent.Fuel.ID));
                }
            };
            this.PanelFuel.Controls.Add(this.SlotFuel);
            //this.Controls.Add(this.PanelFuel);

            this.PanelProduct = new PanelLabeled("Product") { Location = this.PanelFuel.BottomLeft, AutoSize = true };
            this.SlotProduct = new Slot()
            {
                Location = this.PanelProduct.Controls.BottomLeft,
                Tag = this.SmelteryComponent.Product,
                DragDropCondition = obj => false,
                RightClickAction = () =>
                {
                    Net.Client.PlayerRemoteCall(new TargetArgs(smeltery), Message.Types.Retrieve, w => w.Write(this.SmelteryComponent.Product.ID));
                }
            };
            this.PanelProduct.Controls.Add(this.SlotProduct);

            this.Controls.Add(this.PanelOre, this.PanelFuel, this.PanelProduct); 

            this.PanelButtons = new Panel() { Location = this.Controls.BottomLeft, AutoSize = true };
            this.BtnStart = new Button("Start") { LeftClickAction = () => Net.Client.PlayerRemoteCall(new TargetArgs(smeltery), Message.Types.Start) };
            this.PanelButtons.Controls.Add(this.BtnStart);
            this.Controls.Add(this.PanelButtons);

            //LblFuel = new Label("Power: " + SmelteryComponent.Fuel) { Location = this.Controls.BottomLeft };
            //this.Controls.Add(LblFuel);
            BarPower = new Bar() { Location = this.Controls.BottomLeft, Object = this.SmelteryComponent.Power, Name= "Power" };
            this.Controls.Add(BarPower);
            BarProgress = new Bar() { Location = this.Controls.BottomLeft, Object = this.SmelteryComponent.SmeltProgress, Name = "Progress" };
            this.Controls.Add(BarProgress);
        }

        //public override void Update()
        //{
        //    this.LblFuel.Text = "Power: " + this.SmelteryComponent.Power;
        //    base.Update();
        //}

    }
}
