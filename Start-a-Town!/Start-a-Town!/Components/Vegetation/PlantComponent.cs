using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_.Components
{
    public class PlantComponent : EntityComponent
    {
        const float ForageThreshold = .5f;

        static readonly int GrowthRate = 20;

        public override string Name { get; } = "Plant";

        Progress GrowthBody = new(0, 100, 5);
        Progress FruitGrowth = new(0, 100, 0);

        public void SetBodyGrowth(float percentage)
        {
            this.GrowthBody.Percentage = percentage;
           
        }
        public void SetFruitGrowth(float percentage)
        {
            this.FruitGrowth.Percentage = percentage;
            if (this._fruitBone is not null)
                this._fruitBone.Sprite = this.IsHarvestable ? this._spriteFruit : null;
        }
        Bone _fruitBone;

        int GrowthTick, FruitGrowthTick;
        public enum GrowthStates { Growing, Ready }
        public Growth Growth = new(.05f);
        PlantProperties _plantProps;
        public PlantProperties PlantProperties
        { 
            get => this._plantProps;
            set
            {
                this._plantProps = value;
                var parent = this.Parent;
                var hitpoints = parent.GetResource(ResourceDefOf.HitPoints);
                hitpoints.Max = value.StemMaterial.Density;
                hitpoints.TicksPerRecoverOne = value.StemHealRate;

                this._spriteFruit = _plantProps.TextureFruit is string fruitTexturePath ? Sprite.Load(fruitTexturePath) : null;


                var body = parent.Body;
                body.ScaleFunc = () => .25f + .75f * this.GrowthBody.Percentage;
                body.Sprite = Sprite.Load(_plantProps.TextureGrowing);
                //body[BoneDefOf.PlantFruit].Material = _plantProps.FruitMaterial;
                if(body.TryFindBone(BoneDefOf.PlantFruit, out this._fruitBone))
                    this._fruitBone.Material = _plantProps.FruitMaterial;
                this.UpdateFruitTexture();
            }
        }
        void UpdateFruitTexture()
        {
            //this._spriteFruit = _plantProps.TextureFruit is string fruitTexturePath ? Sprite.Load(fruitTexturePath) : null;
            if (_spriteFruit is not null && this.Parent.Body.TryFindBone(BoneDefOf.PlantFruit, out var fruitBone) && this.IsHarvestable)
                fruitBone.Sprite = this._spriteFruit;
        }
        float Length;
        Progress Progress;
        public int Level;

        public PlantComponent()
        {
            this.Progress = new Progress();
        }
        public void SetProperties(PlantProperties props)
        {
            this.PlantProperties = props;
            this.Length = props.GrowTicks;
            this.Progress = new Progress(0, this.Length, 0);
        }
        public PlantComponent(PlantProperties props)
        {
            this.PlantProperties = props;
        }

        internal void SetGrowth(float growth, float fruitGrowth)
        {
            this.GrowthBody.Percentage = growth;
            this.FruitGrowth.Percentage = fruitGrowth;
        }

        public PlantComponent(float fruitGrowTicks)
        {
            this.Length = fruitGrowTicks;
            this.Progress = new Progress(0, this.Length, 0);
        }

        public PlantComponent(PlantComponent toCopy)
        {
            this.GrowthBody = new Progress(toCopy.GrowthBody);
            this.FruitGrowth = new Progress(toCopy.FruitGrowth);
        }

        public override void OnObjectLoaded(GameObject parent)
        {
            parent.Body.Sprite = Sprite.Load(this.IsHarvestable ? this.PlantProperties.TextureGrown : this.PlantProperties.TextureGrowing);
            this.UpdateFruitTexture();
        }
        
        public override void Tick()
        {
            var parent = this.Parent;

            this.TickWiggle();
            if (this.GrowthBody.Percentage >= ForageThreshold)
            {
                if (this.PlantProperties.ProducesFruit)
                    if (!this.FruitGrowth.IsFinished)
                    {
                        if (this.FruitGrowthTick++ >= GrowthRate)
                        {
                            this.FruitGrowthTick = 0;
                            var prevPercentage = this.FruitGrowth.Percentage;
                            this.FruitGrowth.Value++;
                            if (this.IsHarvestable)
                            {
                                if (prevPercentage < ForageThreshold)
                                    parent.Net.EventOccured(Message.Types.PlantReady, parent);
                                //parent.Body.Sprite = this.PlantProperties.TextureGrown;
                                parent.Body.Sprite = Sprite.Load(this.PlantProperties.TextureGrown);
                                this.Parent.Body.FindBone(BoneDefOf.PlantFruit).Sprite = this._spriteFruit;
                            }
                        }
                    }
            }
            if (this.GrowthBody.IsFinished)
                return;
            this.GrowthTick++;
            if (this.GrowthTick >= GrowthRate)
            {
                this.GrowthTick = 0;
                this.GrowthBody.Value++;
            }
            return;
        }
        public void FinishGrowing(GameObject parent)
        {
            this.Growth.Set(parent, this.Growth.Max);
            this.Progress.Value = 0;
        }
        public void Wiggle()
        {
            this.Wiggle(WiggleAngleMaxDefault, WiggleTickMaxDefault, WiggleIntensityDefault);
        }
        public void Wiggle(float angle, int ticks, int speed)
        {
            this.WiggleTick = ticks;
            this.WiggleAngleMax = angle;
            this.WiggleIntensity = speed;
            this.WiggleDirection = (new int[] { -1, 1 })[new Random().Next(2)];
        }
        private void TickWiggle()
        {
            var parent = this.Parent;
            var t = 1 - this.WiggleTick--/ (float)this.WiggleTickMax;
            if (t >= 1)
                return;
            var currentdepth = (1 - t) * this.WiggleAngleMax;
            var radians = this.WiggleIntensity * t * Math.PI * 2;
            var currentangle = this.WiggleDirection * currentdepth * (float)Math.Sin(radians);
            parent.SpriteComp._Angle = currentangle;
        }
        private int WiggleTick;
        private readonly int WiggleTickMax = 40;
        private float WiggleAngleMax;
        const float WiggleAngleMaxDefault = (float)Math.PI / 4f;
        const int WiggleTickMaxDefault = 40;
        private int WiggleIntensity;
        const int WiggleIntensityDefault = 1;
        int WiggleDirection;
        private Sprite _spriteFruit;

        public bool Harvest(GameObject parent, GameObject actor)
        {
            var plant = parent as Plant;
            var props = plant.PlantComponent.PlantProperties;
            if (props.Growth is null)
                return false;
            var yield = (int)(this.FruitGrowth.Percentage * props.Growth.MaxYieldHarvest);
            if (yield == 0)
                return false;

            parent.Net.PopLoot(
                props.Growth.CreateEntity()
                , parent.Global, parent.Velocity);

            this.ResetFruitGrowth(parent);
            parent.Net.EventOccured(Message.Types.PlantHarvested, parent);
            return true;
        }

        public void CutDown(GameObject plant, Actor actor)
        {
            var plantdef = this.PlantProperties;
            var yield = (int)(this.GrowthBody.Percentage * plantdef.MaxYieldCutDown);
            if (plantdef.ProductCutDown != null && yield > 0)
            {
                var product = plantdef.ProductCutDown.CreateFrom(plant.Body.Material ?? MaterialDefOf.LightWood).SetStackSize(yield);
                actor.Net.PopLoot(product, plant.Global, plant.Velocity);

                /// if the plant doesnt produce fruit, then the only seed source is by cutting the plant itself
                if(!plantdef.ProducesFruit)
                {
                    var seeds = plantdef.CreateSeeds().SetStackSize(yield);
                    actor.Net.PopLoot(seeds, plant.Global, plant.Velocity);
                }
            }
            plant.Despawn();
            actor.Net.DisposeObject(plant);
        }

        private void ResetFruitGrowth(GameObject parent)
        {
            this.FruitGrowth.Value = 0;
            this.FruitGrowthTick = 0;
            parent.Body.Sprite = Sprite.Load(this.PlantProperties.TextureGrowing);
            this.Parent.Body.FindBone(BoneDefOf.PlantFruit).Sprite = null;
        }

        public void ResetGrowth(GameObject parent)
        {
            this.Progress.Value = this.Progress.Max = this.Length;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.EntityCollision:
                    this.Wiggle();
                    break;

                default:
                    break;
            }
            return false;
        }

        public override object Clone()
        {
            return new PlantComponent(this) { PlantProperties = this.PlantProperties };
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            tooltip.Controls.Add(new Bar()
            {
                Width = 200,
                Name = "Growth: ",
                Location = tooltip.Controls.BottomLeft,
                Object = this.GrowthBody,
                TextFunc = () => this.GrowthBody.Percentage.ToString("##0%")
            });
        }
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Bar(this.GrowthBody) { Color = Color.MediumAquamarine, Name = "Growth: ", TextFunc = () => this.GrowthBody.Percentage.ToString("##0%") });
            if (this.PlantProperties.ProducesFruit)
                info.AddInfo(new Bar(this.FruitGrowth) { Color = Color.MediumAquamarine, Name = "Fruit: ", TextFunc = () => this.FruitGrowth.Percentage.ToString("##0%") });
        }
        string GrowthTimeSpan
        {
            get
            {
                var ts = TimeSpan.FromMilliseconds(1000 * this.Progress.Value / 60f);
                string fmt = "";
                if (ts.Hours > 0)
                    fmt += "%h'h '";
                if (ts.Minutes > 0)
                    fmt += "%m'm '";
                if (ts.Seconds > 0)
                    fmt += "%s's'";
                return ts.ToString(fmt);
            }
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            this.PlantProperties.Write(writer);
            this.FruitGrowth.Write(writer);
            this.GrowthBody.Write(writer);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.PlantProperties = Def.GetDef<PlantProperties>(reader.ReadString());
            this.FruitGrowth = new Progress(reader);
            this.GrowthBody = new Progress(reader);

        }
        internal override void SyncWrite(System.IO.BinaryWriter w)
        {
            this.Progress.Write(w);
            w.Write(this.Growth.Value);
        }
        internal override void SyncRead(GameObject parent, System.IO.BinaryReader r)
        {
            this.Progress.Read(r);
            this.Growth.Set(parent, r.ReadInt32());
        }
        internal override void AddSaveData(SaveTag tag)
        {
            this.PlantProperties.Save(tag, "Plant");
            tag.Add(this.GrowthBody.Save("GrowthNew"));
            tag.Add(this.FruitGrowth.Save("FruitGrowth"));
        }
        internal override void Load(SaveTag tag)
        {
            this.PlantProperties = tag.LoadDef<PlantProperties>("Plant");
            tag.TryGetTag("GrowthNew", t => this.GrowthBody = new Progress(t));
            tag.TryGetTag("FruitGrowth", t => this.FruitGrowth = new Progress(t));
        }
        public override void GetClientActions(GameObject parent, List<ContextAction> actions)
        {
            base.GetClientActions(parent, actions);
            actions.Add(new ContextAction("Debug: Grow", () => { return false; }));
        }

        internal bool IsHarvestable => this.PlantProperties.ProducesFruit && this.FruitGrowth.Percentage >= ForageThreshold;

        public class Props : ComponentProps
        {
            public override Type CompType => typeof(PlantComponent);
        }
    }
}
