using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    sealed class UIConnecting : Panel
    {
        readonly Label LabelText;
        readonly string Address;
        readonly Button BtnBack;
        public static UIConnecting Create(string address)
        {
            var ui = new UIConnecting(address);
            ui.ShowDialog();
            ui.SnapToScreenCenter();
            return ui;
        }
        UIConnecting(string address)
        {
            this.Address = address;
            this.AutoSize = false;
            this.Size = new Rectangle(0, 0, 300, 100);
            this.LabelText = new Label(string.Format("Awaiting response from {0}", address)) { Location = this.ClientDimensions / 2, Anchor = Vector2.One / 2, TextHAlign = HorizontalAlignment.Center };
            this.AddControls(this.LabelText);
            this.BtnBack = new Button("Back") { Location = new Vector2(this.ClientDimensions.X / 2, this.ClientDimensions.Y), Anchor = new Vector2(.5f, 1), LeftClickAction = () => this.Hide() };
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.ServerResponseReceived:
                    this.SetText("Connected!\nReceiving session...");
                    break;

                case Components.Message.Types.ChunksLoaded:
                    this.Hide();
                    break;

                case Components.Message.Types.ServerNoResponse:
                    this.SetText(string.Format("No response from {0}", this.Address));
                    this.AddControls(this.BtnBack);
                    break;

                default:
                    break;
            }
        }

        private void SetText(string text)
        {
            this.LabelText.Text = text;
            this.LabelText.AnchorTo(this.ClientDimensions / 2, Vector2.One / 2);
        }
    }
}
