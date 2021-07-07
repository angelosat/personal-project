using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.UI
{
    class SpeechBubble : Control
    {
        Panel Graphic;
        bool Paused;
        string Text;
        float Timer;
        float Duration;
        const float FadeSpeed = 10;
        DateTime TimeStamp;
        GameObject Owner;
        static Dictionary<GameObject, List<SpeechBubble>> List = new Dictionary<GameObject, List<SpeechBubble>>();
        Conversation Conversation;
        GroupBox Options;
        Bar AttentionCountdown;

        public override bool Show()
        {
            List<SpeechBubble> _list;
            if (!List.TryGetValue(this.Owner, out _list))
            {
                _list = new List<SpeechBubble>();
                List[this.Owner] = _list;
            }
            _list.Add(this);
            return base.Show();
        }

        static public SpeechBubble Create(DateTime time, GameObject obj, string text, params DialogOption[] options)
        {
            SpeechBubble bubble = ShowNew(obj, text, options);
            bubble.TimeStamp = time;
            return bubble;
        }
        static public SpeechBubble ShowNew(GameObject obj, string text, IEnumerable<DialogOption> options, Conversation convo)
        {
            var bubble = ShowNew(obj, text, options.ToArray());
            bubble.Conversation = convo;
            return bubble;
        }
        static public SpeechBubble ShowNew(GameObject obj, string text, IEnumerable<DialogOption> options, Conversation convo, IProgressBar attention)
        {
            var bubble = ShowNew(obj, text, options.ToArray(), convo);
            bubble.SetTimer(attention);
            return bubble;
        }
        static public SpeechBubble ShowNew(GameObject obj, string text, params DialogOption[] options)
        {
            return ShowNew(obj, text, null, options);
        }
        static public SpeechBubble ShowNew(GameObject obj, string text, Conversation convo, params DialogOption[] options)
        {
            SpeechBubble bubble = new SpeechBubble(obj, text, options);
            bubble.Conversation = convo;
            bubble.TimeStamp = DateTime.Now;
            bubble.Show();
            return bubble;
        }
        public SpeechBubble(GameObject obj, string text, params DialogOption[] options)
        {
            this.Layer = LayerTypes.Speechbubbles;
            this.Owner = obj;
            this.Text = text;
            this.AutoSize = true;
            Graphic = new Panel() { Color = Color.White, MouseThrough = true };
            this.Duration = Math.Max(text.Length * Engine.TicksPerSecond / 3, Engine.TicksPerSecond * 3);//10);
            this.MouseThrough = true;
            Graphic.AutoSize = true;
            Graphic.Controls.Add(new Label(Vector2.Zero, text, fill: Color.Black, outline: Color.White, font: UIManager.FontBold ) { TextColor = Color.Black }.SetMousethrough(true));
            this.Options = new GroupBox() { Location = Graphic.Controls.BottomLeft };
            foreach (var option in options)
            {
                Button btn = new Button(this.Options.Controls.BottomLeft, Graphic.ClientSize.Width, option.Value) { TextColor = Color.Black, TextOutline = Color.White };
                btn.Tag = option;
                btn.LeftClickAction = () =>
                {
                    this.Hide();
                    DialogOption.Select(PlayerOld.Actor, obj, option.Value);
                };
                this.Options.Controls.Add(btn);
            }
            this.Graphic.Controls.Add(this.Options);
            Controls.Add(Graphic); 
            Graphic.BackgroundStyle = BackgroundStyle.Window;
 
            this.Timer = this.Duration;
        }
        public SpeechBubble(GameObject obj, string text, List<string> options)
        {
            this.Layer = LayerTypes.Speechbubbles;
            this.Owner = obj;
            this.Conversation = this.Owner.GetComponent<SpeechComponent>().Conversation;
            this.Text = text;
            this.AutoSize = true;
            Graphic = new Panel() { Color = Color.White, MouseThrough = true };
            this.Duration = Math.Max(text.Length * Engine.TicksPerSecond / 3, Engine.TicksPerSecond * 3);//10);
            this.MouseThrough = true;
            Graphic.AutoSize = true;
            Graphic.Controls.Add(new Label(Vector2.Zero, text, fill: Color.Black, outline: Color.White, font: UIManager.FontBold) { TextColor = Color.Black }.SetMousethrough(true));
            this.Options = new GroupBox() { Location = Graphic.Controls.BottomLeft };
            foreach (var option in options)
            {
                Button btn = new Button(this.Options.Controls.BottomLeft, Graphic.ClientSize.Width, option) { TextColor = Color.Black, TextOutline = Color.White };

                var d = new DialogOption(option, obj);
                btn.Tag = option;
                btn.LeftClickAction = () =>
                {
                    this.Hide();
                    DialogOption.Select(PlayerOld.Actor, obj, option);
                };
                this.Options.Controls.Add(btn);
            }
            this.Graphic.Controls.Add(this.Options);
            Controls.Add(Graphic);
            Graphic.BackgroundStyle = BackgroundStyle.Window;

            this.Timer = this.Duration;
        }

        public void SetTimer(IProgressBar timer)
        {
            if (timer == null)
                return;
            this.AttentionCountdown = new Bar(timer) { Location = this.Graphic.Controls.BottomLeft, Height = 3 };
            this.Graphic.Controls.Add(this.AttentionCountdown);
        }

        public void Restart()
        {
            this.Timer = this.Duration;
        }
        public override bool Hide()
        {
            List[this.Owner].Remove(this);
            if (List[this.Owner].Count == 0)
                List.Remove(this.Owner);
            return base.Hide();
        }

        static void Collision(SpeechBubble b1, SpeechBubble b2)
        {
            if (b1 == b2)
                return;
            if (!b1.BoundsScreen.Intersects(b2.BoundsScreen))
                return;
            if (b1.TimeStamp < b2.TimeStamp)
            {
                b1.Location.Y = b2.Location.Y - b1.BoundsScreen.Height;
            }
            else
            {
                b2.Location.Y = b1.Location.Y - b2.BoundsScreen.Height;
            }
        }

        public override void Update()
        {
            if(this.AttentionCountdown != null)
            {
                (this.AttentionCountdown.Object as Progress).Value--;
            }
            if (this.Conversation != null)
                if (this.Conversation.State == Components.Conversation.States.Finished)
                {
                    this.Graphic.Controls.Remove(this.Options);
                    this.Graphic.Controls.Remove(this.AttentionCountdown);
                    this.Conversation = null;
                }
            if (Paused || this.Conversation != null)
                {
                    if (Timer > Duration / 2f)
                        this.Timer -= 1;
                }
                else
                    this.Timer -= 1;

            
                if (this.Timer < 0)
                {
                    this.Hide();
                    return;
                }
            Camera camera = ScreenManager.CurrentScreen.Camera;
            Rectangle rect = camera.GetScreenBounds(this.Owner.Global, (this.Owner.GetSprite().GetBounds()));
            Vector2 loc = new Vector2(rect.X + rect.Width / 2 - this.ClientSize.Width / 2, rect.Y - this.ClientSize.Height);
            this.Location = loc;
            foreach (var speech in List)
                foreach (var bubble in speech.Value)
                    Collision(this, bubble);
            float a = FadeSpeed * (float)Math.Sin(Math.PI * (1 - this.Timer / (float)this.Duration));
            SetOpacity(a, true);//a);
            base.Update();
        }

        public void Collision()
        {
            foreach (var speech in List)
                foreach (var bubble in speech.Value)
                    Collision(this, bubble);
        }
        static public void UpdateCollisions()
        {
            foreach (var speech in List)
                foreach (var bubble in speech.Value)
                    bubble.Collision();
        }

        static public bool Hide(GameObject obj)
        {
            List<SpeechBubble> bubbles;
            if (!List.TryGetValue(obj, out bubbles))
                return false;
            foreach (var bubble in bubbles.ToList())
                bubble.Hide();
            return bubbles.Count > 0;
        }
    }
}
