using System;
using System.Collections.Generic;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components
{
    class RawMaterialComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "RawMaterial"; }
        }
        public override object Clone()
        {
            return new RawMaterialComponent() { SkillToProcess = this.SkillToProcess };
        }

        public ToolAbilityDef SkillToProcess { get { return (ToolAbilityDef)this["SkillToProcess"]; } set { this["SkillToProcess"] = value; } }
        public RawMaterialComponent Initialize(ToolAbilityDef skillToProcess)
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
            var currentStep = chain.FindIndex(item => item.IDType == parent.IDType);
            if (currentStep == chain.Count - 1)
                return;
            var product = chain[currentStep + 1].Clone();
            parent.Net.PopLoot(product, parent.Global, parent.Velocity);
            parent.Net.Despawn(parent);
            parent.Net.DisposeObject(parent);
        }

        internal override void GetAvailableTasks(GameObject parent, List<Interaction> list)
        {
            list.Add(
                new InteractionCustom(
                    this.SkillToProcess.Name,
                    2,
                    (a, t) => this.Process(t.Object, a),
                    new TaskConditions(new AllCheck(
                        new RangeCheck(t => t.Global, Interaction.DefaultRange)
                        )),
                    this.SkillToProcess
                    )
            );
        }

        public override void GetRightClickActions(GameObject parent, List<ContextAction> actions)
        {
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
                                new RangeCheck(t => t.Global, Interaction.DefaultRange)
                                )),
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
            actions.Add(PlayerInput.RButton, interaction);
        }

        class InteractionProcessMaterial : Interaction
        {
            Action<GameObject, GameObject> ProcessAction = (a, t) => { };
            public InteractionProcessMaterial(string name, ToolAbilityDef skill, Action<GameObject, GameObject> processAction)
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
                            new RangeCheck(t => t.Global, Interaction.DefaultRange),
                            new ScriptTaskCondition("IsRawMaterial", (a, t) => t.Object.HasComponent<RawMaterialComponent>()))
                    );
                }
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
