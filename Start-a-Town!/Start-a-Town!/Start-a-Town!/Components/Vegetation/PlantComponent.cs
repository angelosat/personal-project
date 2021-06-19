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
    class PlantComponent : Component
    {
        public class State
        {
            string Name { get; set; }
            public Sprite Sprite { get; set; }
            public State(string name, Sprite sprite)
            {
                this.Name = name;
                this.Sprite = sprite;
            }
            public void Apply(GameObject parent)
            {
                parent.Body.Sprite = this.Sprite;
            }
        }
        //static public readonly State Growing = new State("Growing");
        //static public readonly State Ready = new State("Grown");
        public State Growing, Grown;
        static float GrowthRate = 1;

        public override string ComponentName
        {
            get
            {
                return "Plant";
            }
        }
        public enum GrowthStates { Growing, Ready }
        float Length { get { return (float)this["Length"]; } set { this["Length"] = value; } }
        Progress Progress { get { return (Progress)this["Progress"]; } set { this["Progress"] = value; } }
        public int Level { get { return (int)this["Level"]; } set { this["Level"] = value; } }
        Loot Produce { get { return (Loot)this["Produce"]; } set { this["Produce"] = value; } }
        public GrowthStates CurrentGrowthState
        {
            get { return (GrowthStates)this["GrowthState"]; }
            set
            {
                this["GrowthState"] = value;
        } }
        public State CurrentState
        {
            get { return (State)this["State"]; }
            set
            {
                this["State"] = value;
            }
        }
        //Sprite SpriteGrowing, SpriteGrown;

        public PlantComponent()
        {
            Properties[Stat.Max.Name] = 3600f;
            //Properties[Stat.Value.Name] = 3600f;
            Properties.Add(Stat.Loot.Name);
            this.CurrentState = null;
            this.Progress = new Progress();
            //this.CurrentState = PlantComponent.GrowthStates.Ready;
        }

        //public override void ComponentsCreated(GameObject parent)
        //{
        //    if (this.Value <= 0)
        //        this.CurrentState = GrowthStates.Ready;
        //    else
        //        this.CurrentState = GrowthStates.Growing;
        //    parent.Body.Sprite.Variation = (int)this.CurrentState;
        //}
        //public override void ObjectSynced(GameObject parent)
        //{
        //    if (this.Value <= 0)
        //        this.CurrentState = GrowthStates.Ready;
        //    else
        //        this.CurrentState = GrowthStates.Growing;
        //    parent.Body.Sprite.Variation = (int)this.CurrentState;
        //}
        

        public PlantComponent(Loot lootList, float growthTime = 3600f)
            : this()
        {
            Properties[Stat.Max.Name] = growthTime;
            Properties[Stat.ValueOld.Name] = growthTime;
            Properties[Stat.Loot.Name] = lootList;
        }
        public PlantComponent Initialize(Sprite growing, Sprite grown)
        {
            //this.SpriteGrowing = growing;
            //this.SpriteGrown = grown;
            this.Growing = new State("Growing", new Sprite(growing));
            this.Grown = new State("Grown", new Sprite(grown));
            //this.Growing = new State("Growing", growing);
            //this.Grown = new State("Grown", grown);
            return this;
        }
        public override void Initialize(GameObject parent)
        {
            if (parent.GetComponent<SpriteComponent>("Sprite").Variation == 1)
                Properties[Stat.ValueOld.Name] = 0f;
            
        }
        public PlantComponent Initialize(Loot produce, int level)
        {
            this.Length = 2 * level * GrowthRate * Engine.TargetFps;
            this.Progress.Value = 0;
            this.Produce = produce;
            this.Level = level;
            return this;
        }
        public override void Update(GameObject parent)
        {
            Wiggle(parent);

            if(this.Progress.Value<=0)
                if (this.CurrentState != this.Grown)
                {
                    this.ApplyState(parent, this.Grown);
                    return;
                }
            this.Progress.Value--;
        }

        private void Wiggle(GameObject parent)
        {
            var t = this.WiggleTime / WiggleTimeMax;
            if (t >= 1)
                return;
            float currentangle, currentdepth = (1-t) * this.WiggleDepthMax;
            currentangle = this.WiggleDirection * currentdepth * (float)Math.Sin(t * Math.PI * 2);
            this.WiggleTime += 0.05f;
            var sprCmp = parent.GetComponent<SpriteComponent>(); // TODO: optimize
            sprCmp._Angle = currentangle;
        }
        float WiggleDepth, WiggleTime, WiggleTimeMax = 2, WiggleDepthMax = (float)Math.PI / 4f;
        int WiggleDirection;

        void ApplyState(GameObject parent, State state)
        {
            this.CurrentState = state;
            state.Apply(parent);
        }
        //public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk)
        //{
        //    if (this.CurrentState == GrowthStates.Ready)
        //        return;
        //    this.Value--;
        //    if (this.Value <= 0)
        //    {
        //        //parent.Avatar.Variation = 1;
        //        parent.Body.Sprite.Variation = 1;
        //        this.CurrentState = GrowthStates.Ready;
        //    }
        //}

        bool Activate(Net.IObjectProvider net, GameObject parent)
        {
            //if (this.CurrentState == GrowthStates.Growing)
            //    return true;
            //this.CurrentState = GrowthStates.Growing;
            if (this.CurrentState == this.Growing)
                return true;
            this.ApplyState(parent, this.Growing);

            this.Progress.Value = this.Progress.Max = this.Length;
            net.PopLoot(new LootTable(this.Produce), parent.Global, parent.Velocity);
            //parent.Avatar.Variation = 0;
            parent.Body.Sprite.Variation = 0;
            return true;
        }

        bool Harvest(GameObject parent, GameObject actor)
        {
            if (this.CurrentState == this.Growing)
                return true;
            ResetGrowth(parent);
            parent.Net.PopLoot(new LootTable(this.Produce), parent.Global, parent.Velocity);

            return true;
        }

        public void ResetGrowth(GameObject parent)
        {
            this.ApplyState(parent, this.Growing);
            this.Progress.Value = this.Progress.Max = this.Length;
        }

        internal override void GetAvailableTasks(GameObject parent, List<Interactions.Interaction> list)
        {
            //list.Add(new Interaction("Harvesting", 1, new Action<GameObject, TargetArgs>((a, t) => this.Harvest(parent, a))));
            list.Add(new InteractionHarvest(parent, this));
        }
        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            actions.Add(new InteractionHarvest(parent, this));
        }
        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
            //actions.Add(PlayerInput.Activate, new Interaction("Harvest", 1, new Action<GameObject, TargetArgs>((a, t) => this.Harvest(parent, a))) { Verb = "Harvesting" });
            actions.Add(PlayerInput.Activate, new InteractionHarvest(parent, this));
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.Activate:
                    return this.Activate(e.Network, parent);
                    //if (this.State == GrowthState.Growing)
                    //    return true;
                    //this.State = GrowthState.Growing;
                    //this.Value = this.Length;
                    //e.Network.PopLoot(new LootTable(this.Produce), parent.Global, parent.Velocity);
                    ////parent.GetComponent<ActorSpriteComponent>("Sprite").Variation = 0;
                    //parent.Avatar.Variation = 0;
                    //return true;


                    if (GetProperty<float>(Stat.ValueOld.Name) > 0)
                        return true;

                    Properties[Stat.ValueOld.Name] = GetProperty<float>(Stat.Max.Name);
                    parent.GetComponent<SpriteComponent>("Sprite").Variation = 0;

   
                    parent.PostMessage(new ObjectEventArgs(Message.Types.Loot));
                    throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.ModifyNeed, parent, "Work", 10);
                    return true;
                  //  break;
                case Message.Types.Loot:
                    Loot loot;
                    if (!TryGetProperty<Loot>(Stat.Loot.Name, out loot))
                        return true;
                    GameObject obj = GameObject.Create(loot.ObjID);

                    Chunk.AddObject(obj, parent.Map, parent.Global + new Vector3(0, 0, parent["Physics"].GetProperty<int>("Height")));
                    double angle = parent.Map.GetWorld().GetRandom().NextDouble() * (Math.PI + Math.PI);
                    throw new NotImplementedException();
                    //GameObject.PostMessage(obj, Message.Types.ApplyForce, parent, new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, 0.05f));

                    e.Network.Spawn(obj);
                    break;
                case Message.Types.Death:
                    e.Network.PopLoot(GameObject.Create(GameObject.Types.Twig), parent.Global, parent.Velocity);
                    return this.Activate(e.Network, parent);
                    //if (GetProperty<float>(Stat.Value.Name) <= 0)
                    //    HandleMessage(parent, new ObjectEventArgs(Message.Types.Loot));
                    //    //HandleMessage(parent, sender, Message.Types.Loot);
                    //break;

                case Message.Types.EntityCollision:
                    //var body = parent.Body;
                    //var sprCmp = parent.GetComponent<SpriteComponent>();
                    //sprCmp._Angle += 1;
                    //body.Angle *= 2;
                    this.WiggleDepth = this.WiggleDepthMax;
                    this.WiggleTime = 0;
                    this.WiggleDirection = (new int[]{-1,1})[new Random().Next(2)];
                    break;

                default:
                    break;
            }
            return false;
        }
        
        public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        {
            if (this.Progress.Value <= 0)
                list.Add(new InteractionOld(new TimeSpan(0, 0, 1), Message.Types.Activate, parent, "Harvest", "Harvesting", effect: new List<AIAdvertisement>() { new AIAdvertisement("Hunger", 20) }));//, // new InteractionEffect("Hunger")));
        }

        public override object Clone()
        {
            PlantComponent comp = new PlantComponent().Initialize(this.Growing.Sprite, this.Grown.Sprite);// { SpriteGrowing = this.SpriteGrowing, SpriteGrown = this.SpriteGrown };
            //comp["Loot"] = Properties["Loot"];
            foreach (KeyValuePair<string, object> parameter in Properties)
                comp[parameter.Key] = parameter.Value;
            //if (this.Value == 150)
            //    "ASDASD".ToConsole();
            return comp;
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            //if (Product == null)
            //    return;

       //     tooltip.Controls.Add(new Label(tooltip.Controls.Last().BottomLeft, "Product: " + (Product != null ? Product.Name : "")));
            //var ts = TimeSpan.FromMilliseconds(1000 * this.Progress.Value / 60f);
            //string fmt = "";
            //if (ts.Hours > 0)
            //    fmt += "%h'h '";
            //if (ts.Minutes > 0)
            //    fmt += "%m'm '";
            //if(ts.Seconds>0)
            //    fmt += "%s's'";
            //tooltip.Controls.Add(new Label(tooltip.Controls.Last().BottomLeft, "Time left: " + (TimeSpan.FromMilliseconds(1000*this.Progress.Value/60f)).ToString(@"hh\:mm\:ss")));
            tooltip.Controls.Add(new Bar()
            {
                Width = 200,
                Name = "Grows in: ",
                Location = tooltip.Controls.BottomLeft,
                Object = this.Progress,
                //TextFunc = () => this.Progress.Max > 0 ? (TimeSpan.FromMilliseconds(1000 * this.Progress.Value / 60f)).ToString(@"hh\:mm\:ss") : ""// this.Progress.Percentage.ToString() : ""
                //TextFunc = () => this.Progress.Max > 0 ? (ts.ToString("%h") + "h " + ts.ToString("%m") + "m " + ts.ToString("%s") + "s") : ""// this.Progress.Percentage.ToString() : ""
                //TextFunc = () => this.Progress.Max > 0 ? String.Format("{0:%h}h {0:%m}m {0:%s}s", ts) : ""// ts.ToString("{0:%h}h {0:%m}m {0:%s}s") : ""
                //TextFunc = () => this.Progress.Max > 0 ? ts.ToString(fmt) : ""// ts.ToString("{0:%h}h {0:%m}m {0:%s}s") : ""
                TextFunc = () => this.Progress.Max > 0 ? this.GrowthTimeSpan : ""// ts.ToString("{0:%h}h {0:%m}m {0:%s}s") : ""

            });
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

        //public override string ToString()
        //{
        //    return base.ToString() + "\nTime left: " + (TimeSpan.FromMilliseconds(1000 * this.Value / 60f)).ToString(@"hh\:mm\:ss");
        //}
        public override void DrawMouseover(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent)
        {
            //if (this.State == GrowthState.Ready)
            //    return;
            Bar.Draw(sb, camera, parent, GameObject.Objects[this.Produce.ObjID].Name, 1 - this.Progress.Percentage);// / this.Length);
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            //if (this.Value == 150)
            //    "ASDASD".ToConsole();
            //writer.Write(this.Progress);
            this.Progress.Write(writer);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            //this.Progress = reader.ReadSingle();
            this.Progress = new Progress(reader);
            //if (this.Value == 150)
            //    "ASDASD".ToConsole();

            //if (this.Value <= 0)
            //    //this.CurrentState = GrowthStates.Ready;
            //    this.ApplyState(this.Grown);
            //else
            //    //this.CurrentState = GrowthStates.Growing;
            //    this.ApplyState(this.Growing);
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();
            //data.Add(new SaveTag(SaveTag.Types.Float, "Value", this.Progress));
            data.Add(this.Progress.Save("Progress"));
            //if (this.Value == 150)
            //    "ASDASD".ToConsole();
            return data;
        }
        internal override void Load(SaveTag compTag)
        {
            //this.Progress = compTag.TagValueOrDefault<float>("Value", this.Length);
            //this.Progress = new Progress(compTag["Progress"]);
            compTag.TryGetTag("Progress", tag => this.Progress = new Progress(tag));

            //if (this.Value <= 0)
            //    this.CurrentState = GrowthStates.Ready;
            //else
            //    this.CurrentState = GrowthStates.Growing;

            //if (this.Value == 150)
            //    "ASDASD".ToConsole();
        }

        public class InteractionHarvest : Interaction
        {
            GameObject Plant;
            PlantComponent PlantComp;
            public InteractionHarvest(GameObject parent, PlantComponent comp)
                : base(
                    "Harvest", 2)
            {
                this.Plant = parent;
                this.PlantComp = comp;
            }
            static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                    RangeCheck.One,
                    new ScriptTaskCondition("IsPlantReady", (a, t) => 
                    {
                        var comp = t.Object.GetComponent<PlantComponent>();
                        return comp.CurrentState == comp.Grown;
                    })// comp.CurrentGrowthState == GrowthStates.Ready)
                    ));
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                this.PlantComp.Harvest(this.Plant, a);
            }
            public override object Clone()
            {
                return new InteractionHarvest(this.Plant, this.PlantComp);
            }
        }
    }
}
