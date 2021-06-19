using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components.Consumables;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components
{
    enum ConsumableType { Food, Drink }
    class Verbs
    {
        //string Verb;
        //public Consumable(ConsumableType type, string verb)
        //{

        //}
        static public string Consume { get { return "Consume"; } }
        static public string Eat { get { return "Eat"; } }
        static public string Drink { get { return "Drink"; } }

    }

    public class ConsumableComponent : EntityComponent
    {
        public static readonly string Name = "Consumable";
        public override string ComponentName
        {
            get
            {
                return Name;
            }
        }

        //public GameObject Byproducts;
        public LootTable Byproducts;
        //List<GameObject> Conditions { get { return (List<GameObject>)this["Conditions"]; } set { this["Conditions"] = value; } }
        //public string Verb { get { return (string)this["Verb"]; } set { this["Verb"] = value; } }
        //public List<AIAdvertisement> NeedEffects { get { return (List<AIAdvertisement>)this["NeedEffects"]; } set { this["NeedEffects"] = value; } }
        public List<ConsumableEffect> Effects = new List<ConsumableEffect>();
        public GameObject Seeds;
        public ItemDefMaterialAmount[] Ingredients;

        public ConsumableComponent InitIngredients(params ItemDefMaterialAmount[] ingredients)
        {
            this.Ingredients = ingredients;
            return this;
        }

        public ConsumableComponent()
        {

        }
        public ConsumableComponent(ConsumableComponent toCopy)
        {
            this.Effects = toCopy.Effects;
        }
      
        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        //{
        //    return true;
        //    Message.Types msg = e.Type;
        //    GameObject sender = e.Sender;
        //    switch (msg)
        //    {
        //        case Message.Types.Activate:
        //            Log.Enqueue(Log.EntryTypes.Buff, sender, parent);

        //            //parent.Despawn(e.Network);
        //            parent.Despawn();
        //            ApplyConditions(parent, sender);
        //            throw new NotImplementedException();
        //            //foreach (var need in this.NeedEffects)
        //            //    e.Sender.PostMessage(Message.Types.ModifyNeed, parent, need.Name, need.Value);
        //            //e.Sender.PostMessage(Message.Types.Dropped, parent);
        //            InventoryComponent inv;
        //            if (!sender.TryGetComponent<InventoryComponent>("Inventory", out inv))
        //                return true;
        //            inv.TryRemove(parent);
        //            return true;

        //        case Message.Types.ConsumeItem:
        //            if (InventoryComponent.IsHauling(e.Sender, obj => obj == parent))
        //                return true;
        //            throw new NotImplementedException();
        //            //e.Sender.PostMessage(Message.Types.Dropped, parent);

        //            return true;

        //        default:
        //            return true;
        //    }
        //}

        private void ApplyConditions(GameObject parent, GameObject sender)
        {
            //throw new NotImplementedException();
            //sender.PostMessage(Message.Types.Consume, parent, Conditions);
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            //tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, "Effects:"));
            //foreach (GameObject condition in Conditions)
            //    tooltip.Controls.Add(
            //        new Label(tooltip.Controls.BottomLeft, condition.ToString()) { TextColorFunc = () => Color.Teal }
            //        );

            //var txt = "";
            //foreach (var effect in this.Effects)
            //    txt += effect.ToString() + '\n';
            //txt = txt.TrimEnd('\n');
            foreach (var effect in this.Effects)
                tooltip.Controls.Add(
                    new Label(effect.ToString()) { Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.ForestGreen }
                    );
        }

        public override object Clone()
        {
            //ConsumableComponent comp = new ConsumableComponent(Verb, Conditions.ToArray()) { NeedEffects = new List<AIAdvertisement>(this.NeedEffects), Byproducts = this.Byproducts, Effects = this.Effects };
            //comp.Seeds = this.Seeds;
            //return comp;
            return new ConsumableComponent(this);
        }

        //public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        //{
        //    list.Add(
        //        new InteractionOld(
        //            new TimeSpan(0, 0, 1),
        //            Message.Types.Activate,
        //            name: Verb,
        //            source: parent,
        //            effect: this.NeedEffects,//new InteractionEffect("Hunger"),
        //            range: (r1, r2) => true,
        //            cond: new ConditionCollection(
        //                new Condition(
        //                    (actor, target) => InventoryComponent.IsHauling(actor, obj => obj == parent),
        //                    "Must be held",
        //                    new Precondition("Carrying", i => i.Source == parent, PlanType.FindInventory)
        //            ))));

        //    list.Add(
        //        new InteractionOld(
        //            new TimeSpan(0, 0, 1),
        //            Message.Types.Activate,
        //            name: Verb,
        //            source: parent,
        //            range: (r1, r2) => true,
        //            cond: new ConditionCollection(
        //                new Condition(
        //                    (actor, target) => InventoryComponent.HasObject(actor, obj => obj == parent),
        //                    "Must be in inventory"
        //            ))));
        //}
        //public override void GetInventoryActions(GameObject actor, GameObjectSlot parentSlot, List<ContextAction> actions)
        //{
        //    actions.Add(new ContextAction(() => this.Verb, () => actor.GetComponent<WorkComponent>().Perform(actor, new Interactions.InteractionConsume(this), new TargetArgs(parentSlot))));
        //}

        internal void Consume(GameObject actor)
        {
            
            //actor.Net.Spawn(this.Byproduct.Clone(), actor.Global);

            foreach (var effect in this.Effects)
                effect.Apply(actor);

            //actor.Net.PopLoot(this.Byproducts.Clone(), actor);


            if (this.Byproducts == null)
                return;
            actor.Net.PopLoot(this.Byproducts, actor.Global, actor.Velocity);
        }

        public override void GetInventoryTooltip(GameObject parent, Control tooltip)
        {
            this.OnTooltipCreated(parent, tooltip);
            var label = new Label("Use: " + new Interactions.InteractionConsume(this).Name) { Font = UIManager.FontBold, TextColorFunc = () => Color.Lime, Location = tooltip.Controls.BottomLeft };
            tooltip.Controls.Add(label);
        }

        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            actions.Add(new InteractionConsume());
        }

        public class InteractionConsume : Interaction
        {
            public InteractionConsume()
                : base("Consume", 4)
            {
                this.Verb = "Eating";
            }

            static readonly Dictionary<Need.Types, float> needs = new() { { Need.Types.Hunger, 50 } };
            //static readonly ScriptTaskCondition cancel = new Exists();

            //static readonly TaskConditions conds = new TaskConditions(
            //        new AllCheck(
            //            new Exists(),
            //            RangeCheck.One
            //            ));

            /// i commented those out
            /// so the interaction works
            /// with held items
            //public override TaskConditions Conditions
            //{
            //    get
            //    {
            //        return conds;
            //    }
            //}
            //public override ScriptTaskCondition CancelState
            //{
            //    get
            //    {
            //        return cancel;
            //    }
            //}

            public override Dictionary<Need.Types, float> NeedSatisfaction
            {
                get
                {
                    return needs;
                }
            }

            public override void Perform(GameObject actor, TargetArgs target)
            {
                var consumable = target.Object as Entity;

                var comp = consumable.GetComponent<ConsumableComponent>();
                comp.Consume(actor);

                var seeds = consumable.Def.ConsumableProperties.Byproduct?.Invoke(consumable);// ItemFactory.CreateSeeds(consumable.Def.ConsumableProperties.PlantOrigin);
                if (seeds != null)
                    actor.Net.PopLoot(seeds, actor.Global, actor.Velocity);

                consumable.SetStackSize(target.Object.StackSize - 1);
                (actor as Actor).AddMoodlet(MoodletDef.JustAte.Create());
            }
            public override object Clone()
            {
                return new InteractionConsume();
            }
        }

        public class Props : ComponentProps
        {
            public NeedEffect[] Effects;
            public override Type CompType => typeof(ConsumableComponent);
            public Props()
            {
                this.Effects = new NeedEffect[] { };
            }
            public Props(NeedEffect[] needEffects)
            {
                this.Effects = needEffects;
            }
        }

        internal override void Initialize(ComponentProps componentProps)
        {
            var props = componentProps as Props;
            this.Effects = new List<ConsumableEffect>(props.Effects);
        }

        //public override string ToString()
        //{
        //    var txt = "";
        //    foreach(var effect in this.Effects)
        //        txt += effect.ToString
        //}
    }
}
