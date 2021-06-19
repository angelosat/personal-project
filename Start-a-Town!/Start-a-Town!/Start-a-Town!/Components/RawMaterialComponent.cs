﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.Components
{
    class RawMaterialComponent : Component
    {
        public override string ComponentName
        {
            get { return "RawMaterial"; }
        }
        public override object Clone()
        {
            return new RawMaterialComponent() { SkillToProcess = this.SkillToProcess };
        }

        public Skill SkillToProcess { get { return (Skill)this["SkillToProcess"]; } set { this["SkillToProcess"] = value; } }
        public RawMaterialComponent Initialize(Skill skillToProcess)
        {
            this.SkillToProcess = skillToProcess;
            return this;
        }
        public RawMaterialComponent()
        {

        }

        public void Process(GameObject parent, GameObject actor)
        {
            Material mat = parent.GetComponent<MaterialsComponent>().Parts["Body"].Material;
            MaterialType matType = mat.Type;

            var chain = mat.ProcessingChain;
            var currentStep = chain.FindIndex(item => item.ID == parent.ID);
            if (currentStep == chain.Count - 1)
                return;
            var product = chain[currentStep + 1].Clone();

            //int prodID = mat.ProcessingChain.First().GetInfo().ID;
            //var product = GameObject.Create(prodID);
            parent.Net.PopLoot(product, parent.Global, parent.Velocity);
            parent.Net.Despawn(parent);
            parent.Net.DisposeObject(parent);
        }

        internal override void GetAvailableTasks(GameObject parent, List<Interaction> list)
        {
            //base.GetAvailableTasks(parent, list);

            list.Add(
                new InteractionCustom(
                    this.SkillToProcess.Name,
                    2,
                    (a, t) => this.Process(t.Object, a),
                    new TaskConditions(new AllCheck(
                        new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                        new SkillCheck(this.SkillToProcess))),
                    this.SkillToProcess
                    )
            );
        }

        public override void GetRightClickActions(GameObject parent, List<ContextAction> actions)
        {
            //Material mat = parent.GetComponent<MaterialsComponent>().Parts["Body"].Material;
            //actions.Add(new UI.ContextAction(() => mat.Type.SkillToExtract.Name, () => Net.Client.PlayerInteract(new TargetArgs(parent))));

            //actions.Add(new UI.ContextAction(() => this.SkillToProcess.Name, () => Net.Client.PlayerUse(new TargetArgs(parent))));
            actions.Add(new ContextAction(() => this.SkillToProcess.Name, () => Net.Client.PlayerInteract(new TargetArgs(parent))));

        }

        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            actions.Add(this.GetWork());
        }

        public Interaction GetWork()
        {
            //get
            //{
            return new InteractionCustom(
                            this.SkillToProcess.Name,
                            2,
                            (a, t) => this.Process(t.Object, a),
                            new TaskConditions(new AllCheck(
                                new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                                new SkillCheck(this.SkillToProcess))),
                            this.SkillToProcess
                            );
            //}
        }

        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
            var materialComp = parent.GetComponent<MaterialsComponent>();
            var mat = materialComp.Parts["Body"];
            var skill = mat.Material.Type.SkillToExtract;
            //var interaction = skill.GetWork(new TargetArgs());
            //var interaction = skill.GetWork(new TargetArgs(parent));
            var interaction = new InteractionProcessMaterial(skill.Name, skill, this.Process);

            //var interaction = new Interaction(
            //                    skill.Name,
            //                    2,
            //                    (a, t) => this.Process(t.Object, a),
            //                    new TaskConditions(
            //                        new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
            //                        new SkillCheck(skill)),
            //                    skill
            //                    );
            actions.Add(new PlayerInput(PlayerActions.Interact), interaction);
        }

        class InteractionProcessMaterial : Interaction
        {
            Action<GameObject, GameObject> ProcessAction = (a, t) => { };
            public InteractionProcessMaterial(string name, Skill skill, Action<GameObject, GameObject> processAction)
                : base(
                    name,
                    2)
            {
                this.Skill = skill;
                this.ProcessAction = processAction;
            }

            /// <summary>
            /// TODO: make static
            /// </summary>
            public override TaskConditions Conditions
            {
                get
                {
                    return new TaskConditions(
                        new AllCheck(
                            new TargetTypeCheck(TargetType.Entity),
                            new SkillCheck(this.Skill),
                            new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                            new ScriptTaskCondition("IsRawMaterial", (a, t) => t.Object.HasComponent<RawMaterialComponent>()))
                    );
                }
            }
            public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
            {
                return new SkillCheck(this.Skill).Condition(actor, target);
            }

            public override void Perform(GameObject a, TargetArgs t)
            {
                // TODO: destroy target here and spawn extracted raw materials
                this.ProcessAction(t.Object, a);
            }

            public override object Clone()
            {
                return new InteractionProcessMaterial(this.Name, this.Skill, this.ProcessAction);
            }
        }
    }
}
