using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Stats;

namespace Start_a_Town_.Components.Resources
{
    class Health : ResourceDef
    {
        //public override string ComponentName
        //{
        //    get { return "Health"; }
        //}

        //public override string Name
        //{
        //    get { return "Health"; }
        //}
        public Health():base("Health")
        {
            //this.Name = "Health";
        }
        public override ResourceDef.ResourceTypes ID
        {
            get { return ResourceDef.ResourceTypes.Health; }
        }

        public override string Description
        {
            get { return "Basic health resource"; }
        }

        //public Recovery Rec = new Recovery();
        //public Progress Rec = Recovery;
        public float TickRate = Engine.TicksPerSecond / 2f;

        static readonly float SpriteFlashFramesCount = Engine.TicksPerSecond / 10f;
        float SpriteFlashTimer;

        //public override string GetLabel(Resource values)
        //{
        //    var val = values.Percentage;
        //    var thresh1 = .75f;
        //    var thresh2 = .5f;
        //    var thresh3 = .25f;

        //    if (val < thresh3)
        //        return "Dying";
        //    else if (val < thresh2)
        //        return "Critical";
        //    else if (val < thresh1)
        //        return "Injured";
        //    else
        //        return "Healthy";
        //}

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
                    //parent.Body.Sprite.Tint = Color.Transparent;
                    //parent.TryGetComponent<SpriteComponent>(t => t.Tint = Color.Transparent);
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
            //if (this.Rec.Value > 0)
            //{
            //    this.Rec.Value--;
            //    return;
            //}
            //this.Add(this.GetRate());
        }
        float GetRate(Resource values)
        {
            //float rate = (1 + (float)Math.Pow(this.Percentage, 2)) / Tick;
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
                    //float reduction =  1 - StatsComponent.GetStatOrDefault(parent, Stat.Types.DmgReduction, 0f);
                    float reduction = 1 - StatsComponentNew.GetStatValueOrDefault(parent, Stat.Types.DmgReduction);//, 0f);

                    int finalValue = (int)(attack.Value * reduction);
                    //this.Value -= finalValue;
                    //this.Add(-finalValue);
                    this.Add(-finalValue, resource);
                    //e.Network.EventOccured(parent, new ObjectEventArgs(Message.Types.Damage, parent, finalValue));
                    e.Network.EventOccured(Message.Types.HealthLost, parent, finalValue);
                    e.Network.EventOccured(Message.Types.EntityAttacked, attacker, parent, finalValue);

                    this.SpriteFlashTimer = SpriteFlashFramesCount;
                    //parent.Body.Sprite.Tint = Color.Red;// new Color(1f,.1f,.1f,1f);//Color.Red + 
                    parent.TryGetComponent<SpriteComponent>(t => t.Tint = Color.Red);
                    if (resource.Value <= 0)
                    {
                        e.Network.PostLocalEvent(parent, Message.Types.Death);
                        e.Network.Despawn(parent);
                        //e.Network.SyncDisposeObject(parent);
                        e.Network.DisposeObject(parent);// if i call syncdispose on the server, the entity might be disposed on the client before the attack message comes
                    }
                    resource.Rec.Value = resource.Rec.Max;
                    return true;

                case Message.Types.HitGround:
                    float zForce = (float)e.Parameters[0];
                    this.HitGround(resource, parent, zForce);
                    //float value = zForce * 10;
                    //this.Add(value);
                    //e.Network.EventOccured(Message.Types.Damage, parent, (int)value);
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

        //public override object Clone()
        //{
        //    return new Health();
        //}

        //public override void OnNameplateCreated(UI.Nameplate plate)
        public override void OnHealthBarCreated(GameObject parent, UI.Nameplate plate, Resource values)
        {
            return;
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
                ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, values.Percentage),
                Tag = values,
                Object = values
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
        //public override string GetLabel(Resource values)
        //{
        //    return this.Name;
        //}
        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent)
        {
            base.DrawUI(sb, camera, parent);
        }

        //public override Control GetControl(Resource resource)
        //{
        //    var bar = new Bar() { Color = Color.Orange, Object = resource, TextFunc = () => this.GetLabel(resource) };
        //    return bar;
        //}
        public override Color GetBarColor(Resource resource)
        {
            return Color.Orange;
        }
    }

    ////class Health : ResourceDef
    ////{
    ////    //public override string ComponentName
    ////    //{
    ////    //    get { return "Health"; }
    ////    //}

    ////    public override string Name
    ////    {
    ////        get { return "Health"; }
    ////    }

    ////    public override ResourceDef.ResourceTypes ID
    ////    {
    ////        get { return ResourceDef.ResourceTypes.Health; }
    ////    }

    ////    public override string Description
    ////    {
    ////        get { return "Basic health resource"; }
    ////    }

    ////    public Recovery Rec = new Recovery();
    ////    public float TickRate = Engine.TicksPerSecond / 2f;

    ////    static readonly float SpriteFlashFramesCount = Engine.TicksPerSecond / 10f;
    ////    float SpriteFlashTimer;



    ////    public override void Tick(GameObject parent, Resource values)
    ////    {
    ////        FlashSprite(parent);
    ////        RecoverHealth();
    ////    }

    ////    private void FlashSprite(GameObject parent)
    ////    {
    ////        if (this.SpriteFlashTimer > 0)
    ////        {
    ////            this.SpriteFlashTimer--;
    ////            if (this.SpriteFlashTimer <= 0)
    ////            {
    ////                //parent.Body.Sprite.Tint = Color.Transparent;
    ////                //parent.TryGetComponent<SpriteComponent>(t => t.Tint = Color.Transparent);
    ////                parent.TryGetComponent<SpriteComponent>(t => t.Tint = Color.White);

    ////            }
    ////        }
    ////    }

    ////    private void RecoverHealth()
    ////    {
    ////        if (this.Rec.Value > 0)
    ////        {
    ////            this.Rec.Value--;
    ////            return;
    ////        }
    ////        this.Add(this.GetRate());
    ////    }
    ////    float GetRate()
    ////    {
    ////        //float rate = (1 + (float)Math.Pow(this.Percentage, 2)) / Tick;
    ////        float rate = ((float)Math.Pow(this.Percentage, 2)) / TickRate;

    ////        return rate;
    ////    }

    ////    public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
    ////    {
    ////        switch (e.Type)
    ////        {
    ////            case Message.Types.Attacked:
    ////                GameObject attacker = e.Parameters[0] as GameObject;
    ////                Attack attack = e.Parameters[1] as Attack;
    ////                //float reduction =  1 - StatsComponent.GetStatOrDefault(parent, Stat.Types.DmgReduction, 0f);
    ////                float reduction = 1 - StatsComponentNew.GetStatValueOrDefault(parent, Stat.Types.DmgReduction);//, 0f);

    ////                int finalValue = (int)(attack.Value * reduction);
    ////                //this.Value -= finalValue;
    ////                this.Add(-finalValue);
    ////                //e.Network.EventOccured(parent, new ObjectEventArgs(Message.Types.Damage, parent, finalValue));
    ////                e.Network.EventOccured(Message.Types.HealthLost, parent, finalValue);
    ////                e.Network.EventOccured(Message.Types.EntityAttacked, attacker, parent, finalValue);

    ////                this.SpriteFlashTimer = SpriteFlashFramesCount;
    ////                //parent.Body.Sprite.Tint = Color.Red;// new Color(1f,.1f,.1f,1f);//Color.Red + 
    ////                parent.TryGetComponent<SpriteComponent>(t => t.Tint = Color.Red);
    ////                if (this.Value <= 0)
    ////                {
    ////                    e.Network.PostLocalEvent(parent, Message.Types.Death);
    ////                    e.Network.Despawn(parent);
    ////                    //e.Network.SyncDisposeObject(parent);
    ////                    e.Network.DisposeObject(parent);// if i call syncdispose on the server, the entity might be disposed on the client before the attack message comes
    ////                }
    ////                this.Rec.Value = this.Rec.Max;
    ////                return true;

    ////            case Message.Types.HitGround:
    ////                float zForce = (float)e.Parameters[0];
    ////                this.HitGround(parent, zForce);
    ////                //float value = zForce * 10;
    ////                //this.Add(value);
    ////                //e.Network.EventOccured(Message.Types.Damage, parent, (int)value);
    ////                return true;

    ////            default:
    ////                return base.HandleMessage(parent, e);
    ////        }
    ////    }

    ////    internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
    ////    {
    ////        switch(e.Type)
    ////        {
    ////            case Message.Types.HitGround:
    ////                e.Data.Translate(parent.Net, r =>
    ////                {
    ////                    float zForce = r.ReadSingle();
    ////                    HitGround(parent, zForce);
    ////                });
    ////                break;

    ////            default: break;
    ////        }
    ////    }

    ////    private void HitGround(GameObject parent, float zForce)
    ////    {
    ////        float value = zForce * 0;
    ////        if (value >= -3)
    ////            return;
    ////        value += 2;
    ////        this.Add(value);
    ////        parent.Net.EventOccured(Message.Types.HealthLost, parent, (int)value);
    ////    }

    ////    public override object Clone()
    ////    {
    ////        return new Health();
    ////    }

    ////    //public override void OnNameplateCreated(UI.Nameplate plate)
    ////    public override void OnHealthBarCreated(GameObject parent, UI.Nameplate plate)
    ////    {
    ////        plate.AlwaysShow = true;
    ////        plate.Controls.Add(new Label()
    ////        {
    ////            //Width = 100,
    ////            //Font = UIManager.FontBold,
    ////            Text = parent.Name,
    ////            //TextColorFunc = () => GetNameplateColor(),
    ////            MouseThrough = true,
    ////        });
    ////        var bar = new Bar()
    ////        {
    ////            Location = plate.Controls.Last().BottomLeft,
    ////            //Location = plate.Controls.Last().BottomCenter - 50*Vector2.UnitX,
    ////            Width = 50,//100,// plate.Controls.Last().Width,
    ////            Height = 3,
    ////            MouseThrough = true,
    ////            ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Percentage),
    ////            Tag = this,
    ////            Object = this
    ////            //PercFunc = () => this.Percentage,
    ////        };
    ////        plate.AddControls(bar);
    ////        plate.SetMousethrough(true, true);
    ////    }

    ////    public override string Format
    ////    {
    ////        get
    ////        {
    ////            return "##0.00";
    ////        }
    ////    }

    ////    public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent)
    ////    {
    ////        base.DrawUI(sb, camera, parent);
    ////    }

    ////    public override Control GetControl()
    ////    {
    ////        var bar = new Bar() { Color = Color.Orange, Object = this };
    ////        return bar;
    ////    }
    ////}
}
