using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.UI
{
    public partial class UIChat
    {
        public class Entry : Label
        {
            static readonly int TimerLength = Engine.TicksPerSecond * 8; 
            int TimerTick = TimerLength;
            static readonly int FadeLength = Engine.TicksPerSecond;
            int FadeTick = FadeLength;

            public Entry(Color color, string text) : base(text)
            {
                this.BackgroundColor = Color.Black * .5f;
                this.TextColorFunc = () => color;
                this.MouseHover = true;
            }

            float Fade => this.FadeTick / (float)FadeLength;
            public override float Opacity
            {
                get
                {
                    float opacity;
                    if (this.TimerTick > 0)
                        opacity = 1;
                    else if (this.Fade > 0)
                        opacity= (float)Math.Sin(this.Fade * Math.PI / 2f);
                    else
                        opacity = 0;
                    return Math.Max(opacity, Parent.Opacity);
                    //if (this.TimerTick > 0)
                    //    return 1;
                    //else if (this.Fade > 0)
                    //    return (float)Math.Sin(this.Fade * Math.PI / 2f);
                    //else
                    //    return 0;
                }
                set
                {
                    this.TimerTick = TimerLength;
                    this.FadeTick = FadeLength;
                    base.Opacity = value;
                }
            }
            public override void Update()
            {
                base.Update();
                if (this.TimerTick > 0)
                {
                    this.TimerTick--;
                    return;
                }
                if (this.FadeTick > 0)
                {
                    this.FadeTick--;
                    return;
                }
            }
        }
    }
}
