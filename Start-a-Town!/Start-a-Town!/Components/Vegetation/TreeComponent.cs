﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Items;
using Start_a_Town_.UI;
using Start_a_Town_.Particles;


namespace Start_a_Town_.Components
{
    class TreeComponent : EntityComponent
    {
        //public Progress Growth = new Progress(0, 100, 100);
        public class States
        {
            static public void FreshlyPlanted(GameObject parent)
            {
                var growth = parent.GetComponent<TreeComponent>().Growth;
                growth.Percentage = InitialGrowthPercentage;
                parent.Body.Scale = growth.Percentage;// this.Growth / 100f;
            }
        }

        public Progress GrowthNew = new(0, 100, 5);
        int GrowthTick;
        int GrowthRate = Engine.TicksPerSecond;
        const int YieldThreshold = 60;
        const float InitialGrowthPercentage = .05f;
        public Growth Growth = new Growth(100);
        public void FinishGrowing(GameObject parent)
        {
            this.Growth.Set(parent, this.Growth.Max);
        }
        public override string ComponentName
        {
            get { return "Tree"; }
        }
        public override object Clone()
        {
            return new TreeComponent();
        }

        public TreeComponent()
        {

        }
        public TreeComponent(float initialGrowth)
        {
            this.GrowthNew.Percentage = initialGrowth;
        }
        public override void MakeChildOf(GameObject parent)
        {
            //parent.Body.ScaleFunc = () => this.GrowthNew.Percentage;
            parent.Body.ScaleFunc = () => .25f + .75f * this.GrowthNew.Percentage;
        }

        //public override void OnSpawn(IObjectProvider net, GameObject parent)
        //{
        //    States.FreshlyPlanted(parent);
        //}
        //public override void OnObjectCreated(GameObject parent)
        //{
        //    parent.Body.Scale = this.Growth.Percentage;// this.Growth / GrowthMax;
        //}

        public override void Tick(GameObject parent)
        {
            base.Tick(parent);
            if (this.GrowthNew.IsFinished)
                return;
            this.GrowthTick++;
            if(this.GrowthTick>=this.GrowthRate)
            {
                this.GrowthTick = 0;
                this.GrowthNew.Value++;
            }

            //this.Growth.Update(parent);
            
        }

        static public void ChopDown(GameObject actor, GameObject parent)
        {
            var comp = parent.GetComponent<TreeComponent>();
            
            //var logs = MaterialType.RawMaterial.Create(parent.Body.Material);
            //var logs = ItemFactory.CreateFrom(RawMaterialDef.Logs, parent.Body.Material);

            var sapling = ItemTemplate.Sapling.Factory.Create();

            var table = comp.Growth.Value >= YieldThreshold ?
                new LootTable(

                    //new Loot(() => MaterialType.RawMaterial.Create(parent.Body.Material), 1, 1, 5, 10), //1, 3),
                    //new Loot(() => ItemFactory.CreateFrom(RawMaterialDef.Logs, parent.Body.Material ?? Material.LightWood), 1, 1, 5, 10) //1, 3),
                    new Loot(() => ItemFactory.CreateFrom(RawMaterialDef.Logs, parent.Body.Material ?? MaterialDefOf.LightWood), comp.GrowthNew.Percentage, 5, 1, 3) //1, 3),

                    //,new Loot(ItemTemplate.Sapling.Factory.Create, 1, 1, 1, 3)
                    )
                    :
                    new LootTable();

            //actor.Net.PopLoot(logs, parent);
            //actor.Net.PopLoot(sapling, parent);
            actor.Net.PopLoot(table, parent.Global, Vector3.Zero);

            actor.Net.Despawn(parent);
            actor.Net.DisposeObject(parent);
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            //w.Write(this.Growth);
            this.GrowthNew.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            //this.Growth = Growth.Create(r);// new Progress(r);
            this.GrowthNew.Read(r);
        }
        internal override void AddSaveData(SaveTag tag)
        {
            //tag.Add(this.Growth.Save("Growth"));
            tag.Add(this.GrowthNew.Save("GrowthNew"));
        }
        internal override void Load(SaveTag tag)
        {
            //tag.TryGetTag("Growth", (v)=>this.Growth = new Growth(v));// Progress(v));
            tag.TryGetTag("GrowthNew", t => this.GrowthNew = new Progress(t));
        }
        static public bool IsGrown(GameObject obj)
        {
            var comp = obj.GetComponent<TreeComponent>();
            return (comp != null && comp.Growth.IsFinished);
        }
        internal override void SyncWrite(System.IO.BinaryWriter w)
        {
            w.Write(this.Growth.Value);
        }
        internal override void SyncRead(GameObject parent, System.IO.BinaryReader r)
        {
            this.Growth.Set(parent, r.ReadInt32());
        }
        //public override void Select(UI.UISelectedInfo info, GameObject parent)
        //{
        //    info.AddButton(new IconButton(Icon.ArrowUp) { HoverText = "Move command" }, ChopDown, parent);
        //}
        //static void ChopDown(List<GameObject> targets)
        //{
        //    PacketEntityDesignation.Send(Client.Instance, (int)Start_a_Town_.Towns.Forestry.ChoppingManager.Types.Chopping, targets, false);
        //}
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Bar(this.GrowthNew) { Color = Color.MediumAquamarine, Name = "Growth: ", TextFunc = () => this.GrowthNew.Percentage.ToString("##0%") });
        }
        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            //tooltip.AddControlsBottomLeft(new Label() { TextFunc = () => string.Format("Growth: {0:P0}", this.Growth.Percentage) });
            tooltip.Controls.Add(new Bar()
            {
                Width = 200,
                Name = "Growth: ",
                Location = tooltip.Controls.BottomLeft,
                Object = this.GrowthNew,
                //TextFunc = () => this.Progress.Max > 0 ? (TimeSpan.FromMilliseconds(1000 * this.Progress.Value / 60f)).ToString(@"hh\:mm\:ss") : ""// this.Progress.Percentage.ToString() : ""
                //TextFunc = () => this.Progress.Max > 0 ? (ts.ToString("%h") + "h " + ts.ToString("%m") + "m " + ts.ToString("%s") + "s") : ""// this.Progress.Percentage.ToString() : ""
                //TextFunc = () => this.Progress.Max > 0 ? String.Format("{0:%h}h {0:%m}m {0:%s}s", ts) : ""// ts.ToString("{0:%h}h {0:%m}m {0:%s}s") : ""
                //TextFunc = () => this.Progress.Max > 0 ? ts.ToString(fmt) : ""// ts.ToString("{0:%h}h {0:%m}m {0:%s}s") : ""
                TextFunc = () => this.GrowthNew.Percentage.ToString("##0%")// ts.ToString("{0:%h}h {0:%m}m {0:%s}s") : ""

            });
        }

        public class Props : ComponentProps
        {
            public override Type CompType => typeof(TreeComponent);
        }
    }
    class InteractionChopDown : Interaction
    {
        GameObject Parent;
        Action<GameObject, GameObject> ProcessAction = (a, t) => { };

        public InteractionChopDown(GameObject parent, Action<GameObject, GameObject> callback)
            : base("Chop Down", 1)
        {
            this.Parent = parent;
            this.Verb = "Chopping";
            this.ProcessAction = callback;
        }
        static readonly TaskConditions conds = new TaskConditions(
                    new AllCheck(
                        new RangeCheck(),
                        new SkillCheck(ToolAbilityDef.Chopping)
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
            this.ProcessAction(a, t.Object);
        }
        public override object Clone()
        {
            return new InteractionChopDown(this.Parent, this.ProcessAction);
        }
    }
    public class InteractionChopping : InteractionPerpetual
    {
        static int MaxStrikes = 3;
        int StrikeCount = 0;
        ParticleEmitterSphere EmitterStrike;
        List<Rectangle> ParticleRects;
        GameObject Logs;
        public InteractionChopping()
            : base("Chopping")
        { }
        static readonly ScriptTaskCondition cancel = new Exists();
        static readonly TaskConditions conds = new TaskConditions(
                    new AllCheck(
                        new Exists(),
                        new RangeCheck(),
                        new SkillCheck(ToolAbilityDef.Chopping)
                ));
        public override ScriptTaskCondition CancelState
        {
            get
            {
                return cancel;
            }
            set
            {
                base.CancelState = value;
            }
        }
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }

        public override void Start(GameObject a, TargetArgs t)
        {
            base.Start(a, t);
            // cache variables
            //this.EmittersList = WorkComponent.GetEmitters(a);

            this.EmitterStrike = new ParticleEmitterSphere();
            this.EmitterStrike.Source = t.Global + Vector3.UnitZ;
            this.EmitterStrike.SizeBegin = 1;
            this.EmitterStrike.SizeEnd = 1;
            this.EmitterStrike.ParticleWeight = 1;
            this.EmitterStrike.Radius = 1f;// .5f;
            this.EmitterStrike.Force = .1f;
            this.EmitterStrike.Friction = .5f;
            this.EmitterStrike.AlphaBegin = 1;
            this.EmitterStrike.AlphaEnd = 0;
            this.EmitterStrike.ColorBegin = MaterialDefOf.LightWood.Color;
            this.EmitterStrike.ColorEnd = MaterialDefOf.LightWood.Color;
            this.EmitterStrike.Lifetime = Engine.TicksPerSecond * 2;
            this.EmitterStrike.Rate = 0;

            this.Logs = MaterialType.RawMaterial.Create(t.Object.Body.Material);
            this.ParticleRects = GameObject.Objects[Logs.IDType].Body.Sprite.AtlasToken.Rectangle.Divide(25);
        }

        public override void OnUpdate(GameObject a, TargetArgs t)
        {
            if (a.Net is Net.Client)
            {
                //this.EmitterStrike.Emit(25);
                this.EmitterStrike.Emit(this.Logs.Body.Sprite.AtlasToken.Atlas.Texture, this.ParticleRects, Vector3.Zero);
                //a.Map.EventOccured(Message.Types.ParticleEmitterAdd, this.EmitterStrike);
                a.Map.ParticleManager.AddEmitter(this.EmitterStrike);
            }
            this.StrikeCount++;
            if (this.StrikeCount < MaxStrikes)
            {
                return;
            }
            this.Done(a, t);
            //this.State = States.Finished;
            this.Finish(a, t);
        }
        public void Done(GameObject a, TargetArgs t)
        {
            TreeComponent.ChopDown(a, t.Object);
        }
        public override object Clone()
        {
            return new InteractionChopping();
        }
        
    }
}
