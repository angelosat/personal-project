using Microsoft.Xna.Framework;
using Start_a_Town_.Crafting;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public partial class CraftOrderNew
    {
        public class UI : GroupBox
        {
            readonly CraftOrderNew Order;
            readonly Label OrderName;
            readonly IconButton BtnClose;
            readonly ButtonIcon BtnUp, BtnDown;
            readonly Button BtnPlus, BtnMinus, BtnDetails;
            readonly CraftOrderDetailsInterface PanelDetails;
            readonly ComboBoxNewNew<CraftOrderFinishMode> ComboFinishMode;
            static readonly Color DefaultBackgroundColor = Color.SlateGray * .5f;
            public UI(CraftOrderNew order)
            {
                this.BackgroundColor = DefaultBackgroundColor;
                this.Order = order;

                this.BtnUp = new ButtonIcon(Icon.ArrowUp) { LeftClickAction = MoveUp };
                this.BtnDown = new ButtonIcon(Icon.ArrowDown) { LeftClickAction = MoveDown, Location = this.BtnUp.BottomLeft };
                this.AddControls(this.BtnUp, this.BtnDown);

                this.OrderName = new Label(order.Reaction.Name) { Location = this.BtnUp.TopRight };
                this.ComboFinishMode = new ComboBoxNewNew<CraftOrderFinishMode>(CraftOrderFinishMode.AllModes, 100, c => c.GetString(this.Order), this.ChangeFinishMode, () => this.Order.FinishMode) { Location = this.OrderName.BottomLeft };

                this.AddControls(this.OrderName,
                    this.ComboFinishMode);

                this.BtnClose = new IconButton(Icon.X) { LocationFunc = () => new Vector2(PanelTitled.GetClientLength(290), 0), BackgroundTexture = UIManager.Icon16Background };
                this.BtnClose.Anchor = Vector2.UnitX;
                this.BtnClose.LeftClickAction = this.RemoveOrder;
                this.AddControls(this.BtnClose);

                this.BtnMinus = new Button("-", Button.DefaultHeight) { Location = this.ComboFinishMode.TopRight, LeftClickAction = Minus };
                this.BtnPlus = new Button("+", Button.DefaultHeight) { Location = this.BtnMinus.TopRight, LeftClickAction = Plus };
                this.AddControls(this.BtnMinus, this.BtnPlus);

                this.BtnDetails = new Button("Details") { Anchor = Vector2.UnitX, LeftClickAction = ToggleDetails };
                this.BtnDetails.LocationFunc = () => this.BottomRight;
                this.BtnDetails.Anchor = Vector2.One;
                this.AddControls(this.BtnDetails);

                this.PanelDetails = new CraftOrderDetailsInterface(this.Order);
                this.PanelDetails.ToWindow(this.Order.Name);
            }

            private void ChangeFinishMode(CraftOrderFinishMode obj)
            {
                PacketCraftOrderChangeMode.Send(this.Order, (int)obj.Mode);
            }

            private void MoveDown()
            {
                this.ChangeOrderPriority(false);
            }
            private void MoveUp()
            {
                this.ChangeOrderPriority(true);
            }

            private void ChangeOrderPriority(bool p)
            {
                Towns.Crafting.CraftingManager.WriteOrderModifyPriority(Client.Instance.OutgoingStream, this.Order, p);
            }

            public int GetIndex()
            {
                return this.Order.GetIndex();
            }

            private void ToggleDetails()
            {
                var win = this.PanelDetails.GetWindow();
                win.Location = this.BtnDetails.ScreenLocation + this.BtnDetails.Width * Vector2.UnitX;
                win.Toggle();
            }

            private void Minus()
            {
                Towns.Crafting.CraftingManager.WriteOrderModifyQuantityParams(Client.Instance.OutgoingStream, this.Order, -1);
            }

            private void Plus()
            {
                Towns.Crafting.CraftingManager.WriteOrderModifyQuantityParams(Client.Instance.OutgoingStream, this.Order, 1);
            }
            private void RemoveOrder()
            {
                PacketOrderRemove.Send(this.Order.Map.Net, this.Order);
            }
        }
    }
}
