using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    [Obsolete]
    class SpeechBubbleOld : Control
    {
        Panel Graphic;
        float Timer;
        float Duration;
        const float FadeSpeed = 10;
        DateTime TimeStamp;
        static Dictionary<GameObject, List<SpeechBubbleOld>> List = new Dictionary<GameObject, List<SpeechBubbleOld>>();

        static public SpeechBubbleOld Create(DateTime time, GameObject obj, string text)
        {
            SpeechBubbleOld bubble = Create(obj, text);
            bubble.TimeStamp = time;
            return bubble;
        }
        static public SpeechBubbleOld Create(GameObject obj, string text)
        {
            SpeechBubbleOld bubble = new SpeechBubbleOld(obj, text);
            
            //List.Add(bubble);
            bubble.TimeStamp = DateTime.Now;
            List<SpeechBubbleOld> _list;
            if (!List.TryGetValue(obj, out _list))
            {
                _list = new List<SpeechBubbleOld>();
                List[obj] = _list;
            }
            _list.Add(bubble);
            bubble.Show();
            return bubble;
        }
        SpeechBubbleOld(GameObject obj, string text)
        {
            this.Layer = UIManager.LayerSpeechbubbles;
            this.Tag = obj;
            this.AutoSize = true;
            Graphic = new Panel() { Color = Color.White, MouseThrough = true };
            this.Duration = Math.Max(text.Length * Ticks.TicksPerSecond / 3, Ticks.TicksPerSecond * 10);
            this.MouseThrough = true;

            Graphic.AutoSize = true;
            Graphic.Controls.Add(new Label(Vector2.Zero, text, fill: Color.Black, outline: Color.White, font: UIManager.FontBold ) { TextColor = Color.Black }.SetMousethrough(true));
         
            Graphic.BackgroundStyle = BackgroundStyle.Window;

            Controls.Add(Graphic);
            this.Timer = this.Duration;
        }
        public void Restart()
        {
            this.Timer = this.Duration;
        }
        public override bool Hide()
        {
            GameObject obj = this.Tag as GameObject;
            List[obj].Remove(this);
            if (List[obj].Count == 0)
                List.Remove(obj);
            return base.Hide();
        }

        public void Initialize(string text, params string[] options)
        {
            Graphic.Controls.Clear();
            Graphic.Controls.Add(new Label(Vector2.Zero, text, fill: Color.Black, outline: Color.White).SetMousethrough(true));
            foreach (var option in options)
            {
                Button btn = new Button(Graphic.Controls.Last().BottomLeft, Graphic.ClientSize.Width, option);
                btn.Tag = option;
                Graphic.Controls.Add(btn);
            }
        }

        static void Collision(SpeechBubbleOld b1, SpeechBubbleOld b2)
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
            this.Timer -= 1;
            GameObject obj = Tag as GameObject;
            if (this.Timer < 0)
            {
                this.Hide();
                return;
            }
            Camera camera = ScreenManager.CurrentScreen.Camera;
            Rectangle rect = camera.GetScreenBounds(obj.Global, (obj.GetSprite().GetBounds()));
            Vector2 loc = new Vector2(rect.X + rect.Width / 2 - this.ClientSize.Width / 2, rect.Y - this.ClientSize.Height);
            this.Location = loc;
          
            float a = FadeSpeed * (float)Math.Sin(Math.PI * (1 - this.Timer / (float)this.Duration));
            SetOpacity(a);
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
            List<SpeechBubbleOld> bubbles;
            if (!List.TryGetValue(obj, out bubbles))
                return false;
            foreach (var bubble in bubbles.ToList())
                bubble.Hide();
            return bubbles.Count > 0;
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }
    }
}
