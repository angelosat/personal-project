using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components
{
    public class PlantComponent : EntityComponent
    {
        const float ForageThreshold = .5f;
        
        static int GrowthRate = 20;//60;

        public override string ComponentName
        {
            get
            {
                return "Plant";
            }
        }
        public Progress GrowthBody = new(0, 100, 5);
        public Progress FruitGrowth = new(0, 100, 0);

    
        int GrowthTick, FruitGrowthTick;
        public enum GrowthStates { Growing, Ready }
        public Growth Growth = new(.05f);
        public PlantProperties PlantProperties = PlantProperties.Berry;

        float Length;
        Progress Progress;
        public int Level { get { return (int)this["Level"]; } set { this["Level"] = value; } }
        Loot Produce { get { return (Loot)this["Produce"]; } set { this["Produce"] = value; } }
        public GrowthStates CurrentGrowthState
        {
            get { return (GrowthStates)this["GrowthState"]; }
            set
            {
                this["GrowthState"] = value;
            }
        }
       
        public override void OnObjectCreated(GameObject parent)
        {
            //var def = parent.Def;
            //var plant = def.PlantProperties;
            //this.Length = plant.FruitGrowTicks;
            //this.Progress = new Progress(0, this.Length, 0);
        }
        public override void OnObjectLoaded(GameObject parent)
        {
            //parent.Body.Sprite = this.IsHarvestable ? parent.Def.PlantProperties.TextureGrown : parent.Def.PlantProperties.TextureGrowing;
            parent.Body.Sprite = this.IsHarvestable ? this.PlantProperties.TextureGrown : this.PlantProperties.TextureGrowing;
        }
        public PlantComponent()
        {
            Properties[Stat.Max.Name] = 3600f;
            Properties.Add(Stat.Loot.Name);
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
        //public PlantComponent(ItemDef def)
        //{
        //    var plant = def.PlantProperties;
        //    this.Length = plant.GrowTicks;
        //    this.Progress = new Progress(0, this.Length, 0);
        //}

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
        public PlantComponent(Loot lootList, float growthTime = 3600f)
            : this()
        {
            Properties[Stat.Max.Name] = growthTime;
            Properties[Stat.ValueOld.Name] = growthTime;
            Properties[Stat.Loot.Name] = lootList;
        }
        public PlantComponent(PlantComponent toCopy)
        {
            this.GrowthBody = new Progress(toCopy.GrowthBody);
            this.FruitGrowth = new Progress(toCopy.FruitGrowth);
        }
       
        public override void Initialize(GameObject parent)
        {
            if (parent.GetComponent<SpriteComponent>("Sprite").Variation == 1)
                Properties[Stat.ValueOld.Name] = 0f;

        }
        public PlantComponent Initialize(Loot produce, int level)
        {
            this.Length = level * GrowthRate * Engine.TicksPerSecond; //*2;
            this.Progress.Value = 0;
            this.Produce = produce;
            this.Level = level;
            return this;
        }
        public override void MakeChildOf(GameObject parent)
        {
            parent.Body.ScaleFunc = () => .25f + .75f * this.GrowthBody.Percentage;
        }
        public override void Tick(GameObject parent)
        {
            Wiggle(parent);
            if (this.GrowthBody.Percentage >= ForageThreshold)
            {
                //if (parent.Def.PlantProperties.ProducesFruit)
                if(this.PlantProperties.ProducesFruit)
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
                                //parent.Body.Sprite = parent.Def.PlantProperties.TextureGrown;
                                parent.Body.Sprite = this.PlantProperties.TextureGrown;
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
        private void Wiggle(GameObject parent)
        {
            var t = this.WiggleTime / WiggleTimeMax;
            if (t >= 1)
                return;
            float currentangle, currentdepth = (1 - t) * this.WiggleDepthMax;
            currentangle = this.WiggleDirection * currentdepth * (float)Math.Sin(t * Math.PI * 2);
            this.WiggleTime += 0.05f;
            var sprCmp = parent.GetComponent<SpriteComponent>(); // TODO: optimize
            sprCmp._Angle = currentangle;
        }
        float WiggleDepth, WiggleTime, WiggleTimeMax = 2, WiggleDepthMax = (float)Math.PI / 4f;
        int WiggleDirection;
        
        public bool Harvest(GameObject parent, GameObject actor)
        {
            var plant = parent as Plant;
            //var props = plant.Def.PlantProperties;
            var props = plant.PlantComponent.PlantProperties;
            var yield = (int)(this.FruitGrowth.Percentage * props.Growth.MaxYieldHarvest);
            if (yield == 0)
                return false;
          
            parent.Net.PopLoot(
                //CreateFruit(props).SetStackSize(yield)
                props.Growth.CreateEntity()
                , parent.Global, parent.Velocity);

            ResetFruitGrowth(parent);
            parent.Net.EventOccured(Message.Types.PlantHarvested, parent);
            return true;
        }

        //private static GameObject CreateFruit(PlantProperties props) => props.ProductHarvest.CreateFrom(props.PlantMaterial);
        
        public void CutDown(GameObject plant, Actor actor)
        {
            var plantdef = this.PlantProperties;// plant.Def.PlantProperties;
            var yield = (int)(this.GrowthBody.Percentage * plantdef.MaxYieldCutDown);
            if (plantdef.ProductCutDown != null && yield > 0)
            {
                var product = plantdef.ProductCutDown.CreateFrom(plant.Body.Material ?? MaterialDefOf.LightWood).SetStackSize(yield);                                                                                                                           //,new Loot(ItemTemplate.Sapling.Factory.Create, 1, 1, 1, 3)
                actor.NetNew.PopLoot(product, plant.Global, plant.Velocity);
            }
            actor.NetNew.Despawn(plant);
            actor.NetNew.DisposeObject(plant);
        }

        private void ResetFruitGrowth(GameObject parent)
        {
            this.FruitGrowth.Value = 0;
            this.FruitGrowthTick = 0;
            //parent.Body.Sprite = parent.Def.PlantProperties.TextureGrowing;
            parent.Body.Sprite = this.PlantProperties.TextureGrowing;
        }

        public void ResetGrowth(GameObject parent)
        {
            this.Progress.Value = this.Progress.Max = this.Length;
        }

        internal override void GetAvailableTasks(GameObject parent, List<Interaction> list)
        {
            list.Add(new InteractionHarvest(parent, this));
        }
        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            actions.Add(new InteractionHarvest(parent, this));
        }
        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
            actions.Add(PlayerInput.Activate, new InteractionHarvest(parent, this));
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.EntityCollision:
                    this.WiggleDepth = this.WiggleDepthMax;
                    this.WiggleTime = 0;
                    this.WiggleDirection = (new int[] { -1, 1 })[new Random().Next(2)];
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


        public override void DrawMouseover(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent)
        {
            //if (this.State == GrowthState.Ready)
            //    return;
            Bar.Draw(sb, camera, parent, GameObject.Objects[this.Produce.ObjID].Name, 1 - this.Progress.Percentage);// / this.Length);
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

        internal bool IsHarvestable => this.FruitGrowth.Percentage >= ForageThreshold;// this.CurrentState == this.Grown;

        public class Props : ComponentProps
        {
            public override Type CompType => typeof(PlantComponent);
        }
    }
}
