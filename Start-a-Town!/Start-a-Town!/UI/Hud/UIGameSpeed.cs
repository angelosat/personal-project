using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
    class UIGameSpeed : Panel// GroupBox
    {
        //IconButton BtnPause, Btn1x, Btn2x, Btn4x;
        ButtonTogglable BtnPause, Btn1x, Btn2x, Btn3x;
        

        //IObjectProvider Net;
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

            //this.BtnPause = new IconButton("▪") { BackgroundTexture = UIManager.Icon16Background, Font = UIManager.Font, LeftClickAction = () => this.SetSpeed(0) };
            //this.Btn1x = new IconButton("►") { BackgroundTexture = UIManager.Icon16Background, Font = UIManager.Font, LeftClickAction = () => this.SetSpeed(1) };
            //this.Btn2x = new IconButton("►►") { BackgroundTexture = UIManager.Icon16Background, Font = UIManager.Font, LeftClickAction = () => this.SetSpeed(2) };
            //this.Btn4x = new IconButton("►►►") { BackgroundTexture = UIManager.Icon16Background, Font = UIManager.Font, LeftClickAction = () => this.SetSpeed(4) };


            this.AddControlsHorizontally(1, this.BtnPause, this.Btn1x, this.Btn2x, this.Btn3x);

        }
        public override Vector2 ScreenLocation => base.ScreenLocation;
        //Button BtnPause, Btn1x, Btn2x, Btn4x;
        //public UIGameSpeed()
        //{
        //    //var w = 50;// Button.GetWidth(UIManager.Font, ">>>");
        //    //var w = Button.GetWidth(UIManager.Font, "►►► 0");
        //    var w = Button.GetWidth(UIManager.Font, "►►►");
        //    //this.BtnPause = new Button(w) { Font = UIManager.Font, TextFunc = GetPauseString, HoverFunc = () => GetAdditionalHoverText("Pause", 0), LeftClickAction = () => this.SetSpeed(0), HAlign = 0 };
        //    //this.Btn1x = new Button(w) { Font = UIManager.Font, TextFunc = GetNormaltring, HoverFunc = () => GetAdditionalHoverText("1x Speed", 1), LeftClickAction = () => this.SetSpeed(1), HAlign = 0 };
        //    //this.Btn2x = new Button(w) { Font = UIManager.Font, TextFunc = GetFastString, HoverFunc = () => GetAdditionalHoverText("2x Speed", 2), LeftClickAction = () => this.SetSpeed(2), HAlign = 0 };
        //    //this.Btn4x = new Button(w) { Font = UIManager.Font, TextFunc = GetFastestString, HoverFunc = () => GetAdditionalHoverText("4x Speed", 3), LeftClickAction = () => this.SetSpeed(4), HAlign = 0 };

        //    this.BtnPause = new Button("▪", w) { LeftClickAction = () => this.SetSpeed(0)};
        //    this.Btn1x = new Button("►", w) { LeftClickAction = () => this.SetSpeed(1)};
        //    this.Btn2x = new Button("►►", w) { LeftClickAction = () => this.SetSpeed(2) };
        //    this.Btn4x = new Button("►►►", w) { LeftClickAction = () => this.SetSpeed(4) };

        //    //this.BtnPause = new IconButton("▪") { BackgroundTexture = UIManager.Icon16Background, Font = UIManager.Font, LeftClickAction = () => this.SetSpeed(0) };
        //    //this.Btn1x = new IconButton("►") { BackgroundTexture = UIManager.Icon16Background, Font = UIManager.Font, LeftClickAction = () => this.SetSpeed(1) };
        //    //this.Btn2x = new IconButton("►►") { BackgroundTexture = UIManager.Icon16Background, Font = UIManager.Font, LeftClickAction = () => this.SetSpeed(2) };
        //    //this.Btn4x = new IconButton("►►►") { BackgroundTexture = UIManager.Icon16Background, Font = UIManager.Font, LeftClickAction = () => this.SetSpeed(4) };


        //    this.AddControlsLeftToRight(this.BtnPause, this.Btn1x, this.Btn2x, this.Btn4x);

        //}

        private string GetPauseString()
        {
            //return string.Format("❚❚ ({0})", this.GameSpeedPlayerCount(0));
            //return string.Format("|| || ({0})", this.GameSpeedPlayerCount(0));
            return string.Format("▪ {0}", this.GameSpeedPlayerCount(0));//■❚▪
            //return string.Format("■ {0}", this.GameSpeedPlayerCount(0));//■❚▪

        }
        private string GetNormaltring()
        {
            return string.Format("► {0}", this.GameSpeedPlayerCount(1));//▶▷▸
            //return string.Format("▶ {0}", this.GameSpeedPlayerCount(1));//▶▷▸

        }
        private string GetFastString()
        {
            return string.Format("►► {0}", this.GameSpeedPlayerCount(2));
            //return string.Format("▶▶ {0}", this.GameSpeedPlayerCount(2));

        }
        private string GetFastestString()
        {
            return string.Format("►►► {0}", this.GameSpeedPlayerCount(3));
            //return string.Format("▶▶▶ {0}", this.GameSpeedPlayerCount(3));

        }
        private int GameSpeedPlayerCount(int speed)
        {
            return Client.Instance.GetPlayers().Where(p => p.SuggestedSpeed == speed).Count();
        }
        string GetAdditionalHoverText(string initialText, int speed)
        {
            //var text = initialText + "\nPlayers at this speed:\n";
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

            //PacketPlayerSetSpeed.Send(net, Client.Instance.PlayerData.ID, s);
            PacketPlayerSetSpeed.Send(net, net.GetPlayer().ID, s);

        }
    }
    class UIGameSpeedOld : GroupBox
    {
        IconButton BtnPause, Btn1x, Btn2x, Btn3x;
        public UIGameSpeedOld()
        {
            this.BtnPause = new IconButton(UIManager.Icon16Background) { HoverText = "Pause", LeftClickAction = () => this.SetSpeed(0) };
            this.Btn1x = new IconButton(UIManager.Icon16Background) { Location = this.BtnPause.TopRight, HoverText = "1x Speed", LeftClickAction = () => this.SetSpeed(1) };
            this.Btn2x = new IconButton(UIManager.Icon16Background) { Location = this.Btn1x.TopRight, HoverText = "2x Speed", LeftClickAction = () => this.SetSpeed(2) };
            this.Btn3x = new IconButton(UIManager.Icon16Background) { Location = this.Btn2x.TopRight, HoverText = "3x Speed", LeftClickAction = () => this.SetSpeed(3) };

            Controls.Add(this.BtnPause, this.Btn1x, this.Btn2x, this.Btn3x);
        }

        void SetSpeed(int s)
        {
            //Client.PlayerSetSpeed(s);
            PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, s);

        }
    }
}
