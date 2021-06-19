using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components
{
    class TreeComponent : Component
    {
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

        static public void ChopDown(GameObject actor, GameObject parent)
        {
            var comp = parent.GetComponent<TreeComponent>();
            if (comp == null)
                return;
            //var logs = MaterialType.Logs.Create(Material.LightWood);
            //var logs = MaterialType.RawMaterial.Create(Material.LightWood);
            var logs = MaterialType.RawMaterial.Create(parent.Body.Material);

            var sapling = ItemTemplate.Sapling.Factory.Create();

            var table =
                new LootTable(
                   //old //new Loot(() => MaterialType.Logs.Create(Material.LightWood), 1, 3),
                   //old //new Loot(() => MaterialType.RawMaterial.Create(Material.LightWood), 1, 3),

                    //new Loot(() => MaterialType.RawMaterial.Create(parent.Body.Material), 1, 3),
                    //new Loot(ItemTemplate.Sapling.Factory.Create, 1, 3)
                    new Loot(() => MaterialType.RawMaterial.Create(parent.Body.Material), 1, 1, 1, 3),
                    new Loot(ItemTemplate.Sapling.Factory.Create, 1, 1, 1, 3)
                    );

            //actor.Net.PopLoot(logs, parent);
            //actor.Net.PopLoot(sapling, parent);
            actor.Net.PopLoot(table, parent.Global, Vector3.Zero);

            actor.Net.Despawn(parent);
            actor.Net.DisposeObject(parent);
        }

        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
            //actions.Add(PlayerInput.RButton, new InteractionChopDown(parent, ChopDown));
            actions.Add(PlayerInput.RButton, new InteractionChopping());

        }

        //internal override void GetInteractionsFromSkill(GameObject parent, Skills.Skill skill, List<Interaction> list)
        //{
        //    if (skill == Skills.Skill.Chopping)
        //        list.Add(new InteractionChopDown(parent, ChopDown));
        //}

        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            //actions.Add(new InteractionChopDown(parent, ChopDown));
            actions.Add(new InteractionChopping());
        }

        class InteractionChopDown : Interaction
        {
            GameObject Parent;
            Action<GameObject, GameObject> ProcessAction = (a, t) => { };

            public InteractionChopDown(GameObject parent, Action<GameObject, GameObject> callback):base("Chop Down", 1)
            {
                this.Parent = parent;
                this.Verb = "Chopping";
                this.ProcessAction = callback;
            }
            static readonly TaskConditions conds = new TaskConditions(
                        new AllCheck(
                            new RangeCheck(),
                            new SkillCheck(Skills.Skill.Chopping)
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

        public class InteractionChopping : Interaction
        {
            public InteractionChopping()
                : base("Chopping", 2)
            {
                this.Animation = new Graphics.Animations.AnimationTool();
                //this.CancelState = new Exists();
            }
            static readonly ScriptTaskCondition cancel = new Exists();
            static readonly TaskConditions conds = new TaskConditions(
                        new AllCheck(
                            new Exists(),
                            new RangeCheck(),
                            new SkillCheck(Skills.Skill.Chopping)
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
            public override void Perform(GameObject a, TargetArgs t)
            {
                TreeComponent.ChopDown(a, t.Object);
            }
            public override object Clone()
            {
                return new InteractionChopping();
            }
        }
    }
}
