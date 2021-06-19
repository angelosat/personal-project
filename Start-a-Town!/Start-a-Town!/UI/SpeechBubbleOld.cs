using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class SpeechBubbleOld : Control
    {
        //GameObject Object;
        Panel Graphic;
        bool Paused;
        string Text;
        float Timer;
        //Control Graphic;
        float Duration;
        const float FadeSpeed = 10;
        DateTime TimeStamp;
      //  GameObject ConversationPartner;
       // static List<SpeechBubble> List = new List<SpeechBubble>();
        static Dictionary<GameObject, List<SpeechBubbleOld>> List = new Dictionary<GameObject, List<SpeechBubbleOld>>();

        //public event EventHandler OptionSelected;
        //void OnOptionSelected(object option)
        //{
        //    if (OptionSelected != null)
        //        OptionSelected(option, EventArgs.Empty);
        //}
        static public SpeechBubbleOld Create(DateTime time, GameObject obj, string text, params DialogueOption[] options)
        {
            SpeechBubbleOld bubble = Create(obj, text, options);
            bubble.TimeStamp = time;
            return bubble;
        }
        static public SpeechBubbleOld Create(GameObject obj, string text, params DialogueOption[] options)
        {
            SpeechBubbleOld bubble = new SpeechBubbleOld(obj, text, options);
            
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
        SpeechBubbleOld(GameObject obj, string text, params DialogueOption[] options)
        {
            //this.Object = obj;
            this.Layer = LayerTypes.Speechbubbles;
            this.Tag = obj;
            this.Text = text;
            this.AutoSize = true;
            Graphic = new Panel() { Color = Color.White, MouseThrough = true };//, BackgroundStyle = BackgroundStyle.Window };
            this.Duration = Math.Max(text.Length * Engine.TicksPerSecond / 3, Engine.TicksPerSecond * 10);
            this.MouseThrough = true;

            Graphic.AutoSize = true;
            Graphic.Controls.Add(new Label(Vector2.Zero, text, fill: Color.Black, outline: Color.White, font: UIManager.FontBold ) { TextColor = Color.Black }.SetMousethrough(true));
            foreach (var option in options)
            {
                if (!option.Condition())
                    continue;
                Button btn = new Button(Graphic.Controls.Last().BottomLeft, Graphic.ClientSize.Width, option.Text) { TextColor = Color.Black, TextOutline = Color.White };

                btn.TexBackgroundColorFunc = () =>
                {
                    return (btn.MouseHover && btn.Active) ? Color.White * 0.5f : Color.Transparent;
                };

                //Label btn = new Label(Graphic.Controls.Last().BottomCenter, option.Text)
                //{
                //    Width = Graphic.ClientSize.Width,
                //    Anchor = new Vector2(0.5f, 0),
                //    Active = true,
                // //   ActiveColor = Color.Gold,
                //    TextColor = Color.Black,
                //    Outline = Color.White,
                //};
                //btn.BackgroundColorFunc = () => (btn.MouseHover && btn.Active) ? Color.Black * 0.5f : Color.Transparent;
             //   btn.Anchor = new Vector2(0.5f, 0);
                btn.Tag = option;
                btn.LeftClick += new UIEvent(option_Click);
                Graphic.Controls.Add(btn);
            }
            Graphic.BackgroundStyle = BackgroundStyle.Window;

            Controls.Add(Graphic);
            this.Timer = this.Duration;
            Paused = options.Length > 0 ? true : false;
        }
        public void Restart()
        {
            this.Timer = this.Duration;
        }
        public override bool Hide()
        {
            foreach (var btn in Graphic.Controls.FindAll(foo => foo is Button))
                btn.LeftClick -= option_Click;
            GameObject obj = this.Tag as GameObject;
            List[obj].Remove(this);
            if (List[obj].Count == 0)
                List.Remove(obj);
            return base.Hide();
        }

        public void Initialize(string text, params string[] options)
        {
            foreach (var btn in Graphic.Controls.FindAll(foo => foo is Button))
                btn.LeftClick -= option_Click;
            Graphic.Controls.Clear();
            Graphic.Controls.Add(new Label(Vector2.Zero, text, fill: Color.Black, outline: Color.White).SetMousethrough(true));
            foreach (var option in options)
            {
                Button btn = new Button(Graphic.Controls.Last().BottomLeft, Graphic.ClientSize.Width, option);
                btn.Tag = option;
                btn.LeftClick += new UIEvent(option_Click);
                Graphic.Controls.Add(btn);
            }
        }

        void option_Click(object sender, EventArgs e)
        {
            //OnOptionSelected((sender as Button).Tag);
            Log.Command(Log.EntryTypes.DialogueOption, PlayerOld.Actor, Tag as GameObject, (sender as Control).Tag);
            Paused = false;
            this.Hide();
        }

        static void Collision(SpeechBubbleOld b1, SpeechBubbleOld b2)
        {
            if (b1 == b2)
                return;
            if (!b1.BoundsScreen.Intersects(b2.BoundsScreen))
                return;
            Rectangle intersection = Rectangle.Intersect(b1.BoundsScreen, b2.BoundsScreen);
            if (b1.TimeStamp < b2.TimeStamp)
            {
                b1.Location.Y = b2.Location.Y - b1.BoundsScreen.Height;
            }
            else
            {
                b2.Location.Y = b1.Location.Y - b2.BoundsScreen.Height;
            }
            //if (intersection.Width > intersection.Height)
            //{
            //    if (b1.Location.Y < b2.Location.Y)
            //    {
            //        b1.Location.Y -= intersection.Height / 2;
            //        b2.Location.Y += intersection.Height / 2;
            //    }
            //    else
            //    {
            //        b2.Location.Y -= intersection.Height / 2;
            //        b1.Location.Y += intersection.Height / 2;
            //    }
            //}
            //else
            //{
            //    if (b1.Location.X < b2.Location.Y)
            //    {
            //        b1.Location.X -= intersection.Width / 2;
            //        b2.Location.X += intersection.Width / 2;
            //    }
            //    else
            //    {
            //        b2.Location.X -= intersection.Width / 2;
            //        b1.Location.X += intersection.Width / 2;
            //    }
            //}
           // Vector2 origin1 = b1.Location + new Vector2(b1.Width / 2, b1.Height / 2), origin2 = b2.Location + new Vector2(b2.Width / 2, b2.Height / 2);
        }
        //SpeechBubble(GameObject obj, string text, params string[] options)
        //{
        //    this.Object = obj;
        //    this.Text = text;
        //    this.Graphic = new Panel();
        //    this.Duration = Math.Max(text.Length * Engine.TargetFps / 3, Engine.TargetFps);
        //    //this.Graphic.MouseThrough = true;
        //    Graphic.AutoSize = true;
        //    Graphic.Controls.Add(new Label(Vector2.Zero, text, fill: Color.Black, outline: Color.White));
        //    foreach (var option in options)
        //        Graphic.Controls.Add(new Button(Graphic.Controls.Last().BottomLeft, Graphic.ClientSize.Width, option));
        //    this.Graphic.BackgroundStyle = BackgroundStyle.Window;
        //    this.Timer = this.Duration;
        //}

        //void Update()
        //{
        //    this.Timer -= GlobalVars.DeltaTime;
        //    if (Timer < 0)
        //    {
        //        List.Remove(this);
        //    }
        //}
        public override void Update()
        {
            if (Paused)
            {
                if (Timer > Duration / 2f)
                    this.Timer -= 1;//GlobalVars.DeltaTime;
            }
            else
                this.Timer -= 1;//GlobalVars.DeltaTime;
            GameObject obj = Tag as GameObject;
            
                if (this.Timer < 0)
                {
                    //List[obj].Remove(this);
                    //if (List[obj].Count == 0)
                    //    List.Remove(obj);
                    this.Hide();
                    return;
                }
            Camera camera = ScreenManager.CurrentScreen.Camera;
            //Rectangle rect = camera.GetScreenBounds(obj.Global, (obj["Sprite"]["Sprite"] as Sprite).GetBounds());
            Rectangle rect = camera.GetScreenBounds(obj.Global, (obj.GetSprite().GetBounds()));
            Vector2 loc = new Vector2(rect.X + rect.Width / 2 - this.ClientSize.Width / 2, rect.Y - this.ClientSize.Height);
            this.Location = loc;
            //foreach (var speech in List)
            //    foreach (var bubble in speech.Value)
            //        bubble.Collision(this, bubble);
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
        //static public void Update()
        //{
        //    //foreach (SpeechBubble bubble in List.ToList())
        //    //{
        //    //    bubble.Timer -= GlobalVars.DeltaTime;
        //    //    if (bubble.Timer < 0)
        //    //    {
        //    //        List.Remove(bubble);
        //    //    }
        //    //}

        //    foreach(KeyValuePair<GameObject, List<SpeechBubble>> speech in List.ToDictionary(foo=>foo.Key, foo=>foo.Value))
        //        foreach (SpeechBubble bubble in speech.Value.ToList())
        //        {
        //            bubble.Timer -= GlobalVars.DeltaTime;
        //            if (bubble.Timer < 0)
        //            {
        //                speech.Value.Remove(bubble);
        //                if (speech.Value.Count == 0)
        //                    List.Remove(speech.Key);
        //            }
        //        }
        //}

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }

        //static public void Draw(SpriteBatch sb, Camera camera)
        //{
        //    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

        //    //float a;
        //    //foreach (KeyValuePair<GameObject, List<SpeechBubble>> speech in List)
        //    //{
        //    //    int i = 0;
        //    //    List<SpeechBubble> list = speech.Value.ToList();
        //    //    list.Reverse();
        //    //    foreach (SpeechBubble bubble in list)
        //    //    {
        //    //        a = FadeSpeed * (float)Math.Sin(Math.PI * (1 - bubble.Timer / (float)bubble.Duration));
        //    //        bubble.Graphic.SetAllOpacity(a);
        //    //        Rectangle rect = camera.GetScreenBounds(bubble.Object.Global, (bubble.Object["Sprite"]["Sprite"] as Sprite).GetBounds());
        //    //        Vector2 loc = new Vector2(rect.X + rect.Width / 2 - bubble.Graphic.Width / 2, rect.Y - bubble.Graphic.Height - i);
        //    //        //UIManager.DrawBorder(sb, BackgroundStyle.Window, new Rectangle((int)loc.X, (int)loc.Y, bubble.Graphic.Width, bubble.Graphic.Height));
        //    //        bubble.Graphic.Location = loc;
        //    //        i += bubble.Graphic.Height;
        //    //        bubble.Graphic.Draw(sb);
        //    //    }
        //    //}

        //    sb.End();
        //}
    }
}
