using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
    class UIGameSpeed : Panel
    {
        ButtonTogglable BtnPause, Btn1x, Btn2x, Btn3x;
        
        public UIGameSpeed(IObjectProvider net)
        {
            this.AutoSize = true;
            var w = Button.GetWidth(UIManager.Font, "►►►");

            this.BtnPause = new ButtonTogglable("▪ ▪", w) { IsToggled = () => net.GetPlayer().SuggestedSpeed == 0, LeftClickAction = () => this.SetSpeed(net, 0), HoverFunc = () => GetAdditionalHoverText("Pause", 0) };
            this.BtnPause.TexBackgroundColorFunc = () => (net.Speed == 0) ? Color.White : this.BtnPause.DefaultBackgroundColorFunc();

            this.Btn1x = new ButtonTogglable("►", w) { IsToggled = () => net.GetPlayer().SuggestedSpeed == 1, LeftClickAction = () => this.SetSpeed(net, 1), HoverFunc = () => GetAdditionalHoverText("Normal", 1) };
            this.Btn1x.TexBackgroundColorFunc = () => (net.Speed == 1) ? Color.White : this.Btn1x.DefaultBackgroundColorFunc();


            this.Btn2x = new ButtonTogglable("►►", w) { IsToggled = () => net.GetPlayer().SuggestedSpeed == 2, LeftClickAction = () => this.SetSpeed(net, 2), HoverFunc = () => GetAdditionalHoverText("Fast", 2) };
            this.Btn2x.TexBackgroundColorFunc = () => (net.Speed == 2) ? Color.White : this.Btn2x.DefaultBackgroundColorFunc();

            this.Btn3x = new ButtonTogglable("►►►", w) { IsToggled = () => net.GetPlayer().SuggestedSpeed == 3, LeftClickAction = () => this.SetSpeed(net, 3), HoverFunc = () => GetAdditionalHoverText("Fastest", 3) };
            this.Btn3x.TexBackgroundColorFunc = () => (net.Speed == 3) ? Color.White : this.Btn3x.DefaultBackgroundColorFunc();

            this.AddControlsHorizontally(1, this.BtnPause, this.Btn1x, this.Btn2x, this.Btn3x);
        }
        public override Vector2 ScreenLocation => base.ScreenLocation;

        private int GameSpeedPlayerCount(int speed)
        {
            return Client.Instance.GetPlayers().Where(p => p.SuggestedSpeed == speed).Count();
        }
        string GetAdditionalHoverText(string initialText, int speed)
        {
            var players = Client.Instance.GetPlayers().Where(p => p.SuggestedSpeed == speed);
            var count = players.Count();
            var text = string.Format("{0}\n\n{1} player(s) at {0}:\n", initialText, count);
            foreach (var pl in players)
            {
                text += pl.Name + '\n';
            }
            return text.TrimEnd('\n');
        }
        void SetSpeed(IObjectProvider net, int s)
        {
            PacketPlayerSetSpeed.Send(net, net.GetPlayer().ID, s);
        }
    }
}
