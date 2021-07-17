using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Resources
{
    class Health : ResourceDef
    {
        public Health():base("Health")
        {
        }
        public override ResourceDef.ResourceTypes ID
        {
            get { return ResourceDef.ResourceTypes.Health; }
        }

        public override string Description
        {
            get { return "Basic health resource"; }
        }
        public float TickRate = Engine.TicksPerSecond / 2f;

        static readonly float SpriteFlashFramesCount = Engine.TicksPerSecond / 10f;
        float SpriteFlashTimer;


        public override void Tick(GameObject parent, Resource values)
        {
            FlashSprite(parent);
            RecoverHealth(values);
        }

        private void FlashSprite(GameObject parent)
        {
            if (this.SpriteFlashTimer > 0)
            {
                this.SpriteFlashTimer--;
                if (this.SpriteFlashTimer <= 0)
                {
                    parent.TryGetComponent<SpriteComponent>(t => t.Tint = Color.White);

                }
            }
        }

        private void RecoverHealth(Resource values)
        {
            if (values.Rec.Value > 0)
            {
                values.Rec.Value--;
                return;
            }
            this.Add(this.GetRate(values), values);
            
        }
        float GetRate(Resource values)
        {
            float rate = ((float)Math.Pow(values.Percentage, 2)) / TickRate;

            return rate;
        }

        public override bool HandleMessage(Resource resource, GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Attacked:
                    GameObject attacker = e.Parameters[0] as GameObject;
                    Attack attack = e.Parameters[1] as Attack;
                    float reduction = 1;// - StatsComponentNew.GetStatValueOrDefault(parent, Stat.Types.DmgReduction);

                    int finalValue = (int)(attack.Value * reduction);
                    this.Add(-finalValue, resource);
                    e.Network.EventOccured(Message.Types.HealthLost, parent, finalValue);
                    e.Network.EventOccured(Message.Types.EntityAttacked, attacker, parent, finalValue);

                    this.SpriteFlashTimer = SpriteFlashFramesCount;
                    parent.TryGetComponent<SpriteComponent>(t => t.Tint = Color.Red);
                    if (resource.Value <= 0)
                    {
                        e.Network.PostLocalEvent(parent, Message.Types.Death);
                        parent.Despawn();
                        e.Network.DisposeObject(parent);// if i call syncdispose on the server, the entity might be disposed on the client before the attack message comes
                    }
                    resource.Rec.Value = resource.Rec.Max;
                    return true;

                case Message.Types.HitGround:
                    float zForce = (float)e.Parameters[0];
                    this.HitGround(resource, parent, zForce);
                    return true;

                default:
                    return base.HandleMessage(resource, parent, e);
            }
        }

        internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e, Resource values)
        {
            switch (e.Type)
            {
                case Message.Types.HitGround:
                    e.Data.Translate(parent.Net, r =>
                    {
                        float zForce = r.ReadSingle();
                        HitGround(values, parent, zForce);
                    });
                    break;

                default: break;
            }
        }

        private void HitGround(Resource resource, GameObject parent, float zForce)
        {
            float value = zForce * 0;
            if (value >= -3)
                return;
            value += 2;
            this.Add(value, resource);
            parent.Net.EventOccured(Message.Types.HealthLost, parent, (int)value);
        }

        public override void OnHealthBarCreated(GameObject parent, UI.Nameplate plate, Resource values)
        {
            return;
            plate.AlwaysShow = true;
            plate.Controls.Add(new Label()
            {
                Text = parent.Name,
                MouseThrough = true,
            });
            var bar = new Bar()
            {
                Location = plate.Controls.Last().BottomLeft,
                Width = 50,
                Height = 3,
                MouseThrough = true,
                ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, values.Percentage),
                Tag = values,
                Object = values
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

        public override Color GetBarColor(Resource resource)
        {
            return Color.Orange;
        }
    }
}
