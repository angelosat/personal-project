using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Stats;

namespace Start_a_Town_.Components.Resources
{
    class Health : Resource
    {
        public override string ComponentName
        {
            get { return "Health"; }
        }

        public override string Name
        {
            get { return "Health"; }
        }

        public override Resource.Types ID
        {
            get { return Resource.Types.Health; }
        }

        public override string Description
        {
            get { return "Basic health resource"; }
        }

        public Recovery Rec = new Recovery();
        public float Tick = Engine.TargetFps / 2f;

        static readonly float SpriteFlashFramesCount = Engine.TargetFps / 10f;
        float SpriteFlashTimer;



        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            FlashSprite(parent);
            RecoverHealth();
        }

        private void FlashSprite(GameObject parent)
        {
            if (this.SpriteFlashTimer > 0)
            {
                this.SpriteFlashTimer--;
                if (this.SpriteFlashTimer <= 0)
                {
                    //parent.Body.Sprite.Tint = Color.Transparent;
                    //parent.TryGetComponent<SpriteComponent>(t => t.Tint = Color.Transparent);
                    parent.TryGetComponent<SpriteComponent>(t => t.Tint = Color.White);

                }
            }
        }

        private void RecoverHealth()
        {
            if (this.Rec.Value > 0)
            {
                this.Rec.Value--;
                return;
            }
            this.Add(this.GetRate());
        }
        float GetRate()
        {
            //float rate = (1 + (float)Math.Pow(this.Percentage, 2)) / Tick;
            float rate = ((float)Math.Pow(this.Percentage, 2)) / Tick;

            return rate;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Attacked:
                    GameObject attacker = e.Parameters[0] as GameObject;
                    Attack attack = e.Parameters[1] as Attack;
                    //float reduction =  1 - StatsComponent.GetStatOrDefault(parent, Stat.Types.DmgReduction, 0f);
                    float reduction = 1 - StatsComponentNew.GetStatValueOrDefault(parent, Stat.Types.DmgReduction, 0f);

                    int finalValue = (int)(attack.Value * reduction);
                    //this.Value -= finalValue;
                    this.Add(-finalValue);
                    //e.Network.EventOccured(parent, new ObjectEventArgs(Message.Types.Damage, parent, finalValue));
                    e.Network.EventOccured(Message.Types.HealthLost, parent, finalValue);

                    this.SpriteFlashTimer = SpriteFlashFramesCount;
                    //parent.Body.Sprite.Tint = Color.Red;// new Color(1f,.1f,.1f,1f);//Color.Red + 
                    parent.TryGetComponent<SpriteComponent>(t => t.Tint = Color.Red);
                    if (this.Value <= 0)
                    {
                        e.Network.PostLocalEvent(parent, Message.Types.Death);
                        e.Network.Despawn(parent);
                        //e.Network.SyncDisposeObject(parent);
                        e.Network.DisposeObject(parent);// if i call syncdispose on the server, the entity might be disposed on the client before the attack message comes
                    }
                    this.Rec.Value = this.Rec.Max;
                    return true;

                case Message.Types.HitGround:
                    float zForce = (float)e.Parameters[0];
                    this.HitGround(parent, zForce);
                    //float value = zForce * 10;
                    //this.Add(value);
                    //e.Network.EventOccured(Message.Types.Damage, parent, (int)value);
                    return true;

                default:
                    return base.HandleMessage(parent, e);
            }
        }

        internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        {
            switch(e.Type)
            {
                case Message.Types.HitGround:
                    e.Data.Translate(parent.Net, r =>
                    {
                        float zForce = r.ReadSingle();
                        HitGround(parent, zForce);
                    });
                    break;

                default: break;
            }
        }

        private void HitGround(GameObject parent, float zForce)
        {
            float value = zForce * 0;
            if (value >= -3)
                return;
            value += 2;
            this.Add(value);
            parent.Net.EventOccured(Message.Types.HealthLost, parent, (int)value);
        }

        public override object Clone()
        {
            return new Health();
        }

        //public override void OnNameplateCreated(UI.Nameplate plate)
        public override void OnHealthBarCreated(GameObject parent, UI.Nameplate plate)
        {
            plate.AlwaysShow = true;
            plate.Controls.Add(new Label()
            {
                //Width = 100,
                //Font = UIManager.FontBold,
                Text = parent.Name,
                //TextColorFunc = () => GetNameplateColor(),
                MouseThrough = true,
            });
            var bar = new Bar()
            {
                Location = plate.Controls.Last().BottomLeft,
                //Location = plate.Controls.Last().BottomCenter - 50*Vector2.UnitX,
                Width = 50,//100,// plate.Controls.Last().Width,
                Height = 3,
                MouseThrough = true,
                ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Percentage),
                Tag = this,
                Object = this
                //PercFunc = () => this.Percentage,
            };
            plate.AddControls(bar);
            plate.SetMousethrough(true, true);
        }

        public override string Format
        {
            get
            {
                return "##0.00";
            }
        }

        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent)
        {
            base.DrawUI(sb, camera, parent);
        }

        public override Control GetControl()
        {
            var bar = new Bar() { Color = Color.Orange, Object = this };
            return bar;
        }
    }
}
