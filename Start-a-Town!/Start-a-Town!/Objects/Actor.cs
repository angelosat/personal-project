﻿using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class Actor : Entity
    {
        public Actor()
        {

        }

        public override float Height => this.Physics.Height - (this.Mobile.Crouching ? 1 : 0);
        public override bool IsHaulable => false;

        MobileComponent _cachedMobile;
        public MobileComponent Mobile => this._cachedMobile ??= this.GetComponent<MobileComponent>();

        public float Acceleration
        {
            get => this.Mobile.Acceleration;
            set => this.Mobile.Acceleration = value;
        }

        internal NpcSkillsComponent Skills => this.GetComponent<NpcSkillsComponent>();
        internal AttributesComponent Attributes => this.GetComponent<AttributesComponent>();
        internal NpcComponent Npc => this.GetComponent<NpcComponent>();

        PossessionsComponent _ownership;
        public PossessionsComponent Ownership => this._ownership ??= this.GetComponent<PossessionsComponent>();

        WorkComponent _work;
        public WorkComponent Work => this._work ??= this.GetComponent<WorkComponent>();
        public Interaction CurrentInteraction => this.Work.Task;

        AIComponent _ai;
        public AIComponent AI => this._ai ??= this.GetComponent<AIComponent>();

        AIState _state;
        public AIState State => this._state ??= this.GetComponent<AIComponent>().State;
        internal AITask CurrentTask
        {
            get => this.State.CurrentTask;
            set => this.State.CurrentTask = value;
        }
        internal BehaviorPerformTask CurrentTaskBehavior
        {
            get => this.State.CurrentTaskBehavior;
            set => this.State.CurrentTaskBehavior = value;
        }
        internal PersonalityComponent Personality => this.GetComponent<PersonalityComponent>();
        public AILog Log => AIState.GetState(this).History;
        public IItemPreferencesManager ItemPreferences => this.GetState().ItemPreferences;

        public Room AssignedRoom => this.Net.Map.Town.RoomManager.FindRoom(this.RefID); // replaced this.town with this.net.map.town because when the actor leaves the map, this.town returns null
        internal Workplace Workplace => this.Net.Map.Town.ShopManager.GetShop(this); // replaced this.town with this.net.map.town because when the actor leaves the map, this.town returns null
        public bool IsCitizen => this.Net.Map.Town.Agents.Contains(this.RefID); // replaced this.town with this.net.map.town because when the actor leaves the map, this.town returns null

        public float Mood => this.GetComponent<MoodComp>().Mood;

        public override string Name => this.Npc.FullName;
        internal override GameObject SetName(string name)
        {
            // HACK
            var splitname = name.Split(' ');
            this.Npc.FirstName = splitname[0];
            this.Npc.LastName = splitname.Length > 1 ? splitname[1] : "";
            return this;
        }

        internal void Loot(Entity loot, OffsiteAreaDef area)
        {
            var net = this.Net;
            if (net is Server server)
            {
                loot.SyncInstantiate(server);
                PacketInventoryInsertItem.Send(server, this, loot, area);
            }
            this.Log.Write($"Looted [{loot.Name},{loot.PrimaryMaterial.Color}] while exploring [{area.Name}]"); // call this before inserting because the item might be absorbed/disposed
            this.Inventory.Insert(loot);
        }
        internal bool InitiateTrade(Actor actor, Entity item, int itemcost)
        {
            // TODO do stuff with item and itemcost
            var state = this.GetState();
            if (state.TradingPartner != null)
                return false;
            state.TradingPartner = actor;
            return true;
        }

        internal bool HasMoney(int amount)
        {
            var coins = this.Inventory.First(i => i.Def == ItemDefOf.Coins); // TODO find all ammount instead of find first
            return coins?.StackSize >= amount;
        }
        internal Entity GetMoney()
        {
            return this.Inventory.First(i => i.Def == ItemDefOf.Coins) as Entity;
        }
        internal int GetMoneyTotal()
        {
            return this.Inventory.Count(o => o.Def == ItemDefOf.Coins);
        }

        internal void ModifyNeed(NeedDef def, Func<float, float> modOldValue)
        {
            var need = this.GetNeed(def);
            var old = need.Value;
            need.Value = modOldValue(need.Value);
            this.Net?.EventOccured(Message.Types.NeedUpdated, this, need, need.Value - old);
        }

        internal void Carry(Entity item)
        {
            this.Inventory.Haul(item);
            //PersonalInventoryComponent.GetHauling(this).Object = item;
        }

        /// <summary>
        /// if force is true, target actor drops current carried item and replaces it with the given one
        /// </summary>
        /// <param name="seller"></param>
        /// <param name="force"></param>
        internal void GiveCarriedTo(Actor target, bool force = false)
        {
            throw new NotImplementedException();
        }

        internal void ForceTask(TaskDef taskdef, TargetArgs target)
        {
            throw new NotImplementedException();
        }

        internal void FaceTowards(TargetArgs targetA)
        {
            this.Direction = targetA.Global - this.Global;
            this.Direction.Normalize();
            this.Net.LogStateChange(this.RefID);
        }

        internal void ForceTask(TaskGiver taskGiver, TargetArgs target)
        {
            var task = taskGiver.TryTaskOn(this, target, true);
            if (task is not null)
                this.GetState().ForceTask(task);
        }

        internal bool CanStandIn(Vector3 global)
        {
            var map = this.Map;
            return
                map.GetBlock(global).IsStandableIn &&
                map.GetBlock(global.Above()).IsStandableIn &&
                map.GetBlock(global.Below()).IsStandableOn; //TODO: take into account actor's height instead of hardcoding checks 2 blocks above
        }
        internal bool CanStandOn(Vector3 global)
        {
            var map = this.Map;
            var above = global.Above();
            return
                map.GetBlock(global).IsStandableOn &&
                map.GetBlock(above).IsStandableIn &&
                map.GetBlock(above.Above()).IsStandableIn; //TODO: take into account actor's height instead of hardcoding checks 2 blocks above
        }

        internal void FinishConversation()
        {
            if (this.Net is Client)
                return;
            var state = this.GetState();
            state.ConversationPartner.GetState().ConversationPartner = null;
            state.ConversationPartner = null;
        }

        internal void TalkTo(Actor target, ConversationTopic topic)
        {
            topic.ApplyNew(this, target);
        }

        internal void EnqueueCommunication(Actor target, ConversationTopic topic)
        {
            this.GetState().CommunicationPending.Add(target, topic);
        }

        internal ConversationTopic GetNextConversationTopicFor(Actor target)
        {
            var state = this.GetState();
            var topic = state.CommunicationPending[target];
            state.CommunicationPending.Remove(target);
            return topic;
        }
        internal void Interact(Interaction interaction)
        {
            AIManager.Interact(this, interaction, TargetArgs.Null);
        }
        internal void Interact(Interaction interaction, TargetArgs targetArgs)
        {
            AIManager.Interact(this, interaction, targetArgs);
        }
        internal void Interact(Interaction interaction, Vector3 target)
        {
            AIManager.Interact(this, interaction, new TargetArgs(this.Map, target));
        }
        internal void Interact(Interaction interaction, GameObject target)
        {
            AIManager.Interact(this, interaction, new TargetArgs(target));
        }
        internal void CancelInteraction()
        {
            AIManager.EndInteraction(this);
        }

        internal void Equip(GameObject item)
        {
            this.Interact(new Equip(), item);
        }
        internal bool IsEquipping(Entity item)
        {
            return this.GetGear().Any(i => i == item);
        }
        internal int GetReservedAmount(GameObject item)
        {
            return this.Town.ReservationManager.GetReservedAmount(this, item);
        }

        internal void StopPathing()
        {
            this.GetState().Path = null;
        }

        public override GameObject Create()
        {
            return new Actor();
        }

        internal bool ReserveAsManyAsPossible(TargetArgs item, int desiredAmount)
        {
            return this.Town.ReservationManager.ReserveAsManyAsPossible(this, this.CurrentTask, item, desiredAmount);
        }

        internal void AddNeed(params NeedDef[] defs)
        {
            this.GetComponent<NeedsComponent>().AddNeed(defs);
        }

        public static Actor Create(ItemDef def)
        {
            var obj = new Actor
            {
                Def = def
            };
            obj.Physics.Height = def.Height;
            obj.Physics.Weight = def.Weight;

            obj.AddComponent(new AIComponent().Initialize( /// MOVING AICOMPONENT TO THE TOP because it generates packets that are being processed in the client before the entities even get ticked
               new BehaviorQueue(
                   new AIMemory(),
                   new BehaviorHandleResources(),
                   new BehaviorHandleOrders(),
                   new BehaviorHandleTasks(),
                   new BehaviorIdle()
                   )));
            obj.AddComponent(new AttributesComponent(def).Randomize());
            obj.AddComponent(new NpcSkillsComponent(def).Randomize());
            obj.AddComponent(new PersonalityComponent(def));//.Randomize());
            obj.AddComponent(new GearComponent(def));
            obj.AddComponent(new ResourcesComponent(def));
            obj.AddComponent(new NeedsComponent(obj));
            obj.AddComponent(new PossessionsComponent());
            obj.AddComponent(new HaulComponent());
            obj.AddComponent(new NpcComponent());
            obj.AddComponent(new SpriteComponent(def.Body));
            obj.AddComponent(new PersonalInventoryComponent(16));
            obj.AddComponent(new StatsComponentNew());
            obj.AddComponent(new MobileComponent());
            //obj.AddComponent(new WorkComponent());
            obj.AddComponent(new MoodComp());
            //obj.AddComponent(new AIComponent().Initialize(
            //   new BehaviorQueue(
            //       new AIMemory(),
            //       new BehaviorHandleResources(),
            //       new BehaviorHandleOrders(),
            //       new BehaviorHandleTasks(),
            //       new BehaviorIdle()
            //       )));
            obj.AddComponent(new SpriteComponent(def.Body));
            obj.AddComponent(new WorkComponent()); /// MOVED THIS HERE AFTER THE AICOMPONENT, so that when the ai starts an interaction, it gets ticked during the same frame
                                                   /// because when the interaction is received on the client, the packet is processed before the entity even ticks at all
                                                   /// resulting in a one-tick difference in the interaction progress beteween server and client

            foreach (var b in obj.Body.GetAllBones())
                b.Material = def.DefaultMaterial;

            obj.Sprite.Customization = new CharacterColors(obj.Body).Randomize();

            obj.ObjectCreated();
            return obj;
        }
        public override Color GetNameplateColor()
        {
            if (this.IsPlayerControlled)
                return Color.Yellow;
            if (this.IsCitizen)
                return Color.White;
            return Color.Cyan;
        }

        internal void EndCurrentTask()
        {
            this.Work.Interrupt();
            this.GetComponent<AIComponent>().FindBehavior<BehaviorHandleTasks>().EndCurrentTask(this);
        }
        internal void MoveToggle(bool toggle)
        {
            if (this.Net is Server)
                PacketEntityMoveToggle.Send(this.Net, this.RefID, toggle);

            this.Mobile.Toggle(this, toggle);
        }
        public void AddMoodlet(Moodlet m)
        {
            this.GetComponent<MoodComp>().Add(m);
        }
        public void RemoveMoodlet(MoodletDef mdef)
        {
            this.GetComponent<MoodComp>().Remove(mdef);
        }
        public bool HasMoodlet(MoodletDef mdef)
        {
            return this.GetComponent<MoodComp>().Contains(mdef);
        }

        
       
        readonly Button btnLog = new("Log");
        readonly Button btnSkills = new("Skills");
        readonly Button btnGear = new("Gear");
        readonly Button btnPersonality = new("Personality");
        readonly Button btnNeeds = new("Needs");
        readonly Button btnStats = new("Stats");
        readonly Button btnVisitor = new("Visitor");

        protected override IEnumerable<Button> GetInfoTabsExtraNew()
        {
            yield return this.btnLog.SetLeftClickAction(b => NpcLogUINew.GetGui(this).Toggle()) as Button;
            yield return this.btnSkills.SetLeftClickAction(b => NpcSkillsComponent.GetGui(this).Toggle()) as Button;
            yield return this.btnGear.SetLeftClickAction(b => InventoryUI.GetGui(this).Toggle()) as Button;
            yield return this.btnPersonality.SetLeftClickAction(b => PersonalityComponent.GetGui(this).Toggle()) as Button;
            yield return this.btnNeeds.SetLeftClickAction(b => NeedsUI.GetGui(this).Toggle()) as Button;
            yield return this.btnStats.SetLeftClickAction(b => StatsGui.GetGui(this).Toggle()) as Button;
            if (!this.IsCitizen)
                yield return this.btnVisitor.SetLeftClickAction(b => this.GetVisitorProperties().ShowGui()) as Button;
        }
        public bool CanOperate(TargetArgs target)
        {
            if (target.Type != TargetType.Position)
                throw new Exception();
            var global = target.Global;
            return this.CanOperate(global);
        }
        public bool CanOperate(Vector3 global)
        {
            return this.FindOperatablePosition(global).HasValue;
        }
        public bool CanOperate(Vector3 global, out IntVec3 operatingPos)
        {
            var poos = this.FindOperatablePosition(global);
            operatingPos = poos.Value;
            return poos.HasValue;
        }
        public IntVec3? FindOperatablePosition(IntVec3 facilityGlobal)
        {
            var operatingPositions = this.Map.GetCell(facilityGlobal).GetOperatingPositions();
            if (!operatingPositions.Any())
                return null;
            foreach (var pos in operatingPositions)
            {
                var globalpos = facilityGlobal + pos;
                if (this.CanReach(globalpos) && this.Map.GetBlock(globalpos).IsStandableIn)
                    return globalpos;
            }
            return null;
        }

        internal int GetHaulStackLimitFromEndurance(ItemDef def)
        {
            var maxHaulWeight = StatDefOf.MaxHaulWeight.GetValue(this);
            var activityLevel = this.GetTrait(TraitDefOf.Activity)?.Normalized ?? 0;
            var maxDesiredEncumberance = maxHaulWeight + maxHaulWeight * activityLevel * .5f;
            var unitWeight = def.Weight;
            int stackEnduranceLimit = (int)Math.Floor(maxDesiredEncumberance / unitWeight);
            var max = Math.Min(def.StackCapacity, stackEnduranceLimit); // this was missing and i was calculating it when i was calling this func
            return max;
        }
        internal int GetHaulStackLimitFromEndurance(GameObject haulable)
        {
            return this.GetHaulStackLimitFromEndurance(haulable.Def);
        }

        internal float GetOpportunisticHaulSearchRange(int baseSearchRange)
        {
            var organizationValue = this.GetTrait(TraitDefOf.Planning)?.Normalized ?? 0;
            var num = baseSearchRange * organizationValue * .5f;
            return baseSearchRange + num;
        }

        public bool IsTooTiredToWork
        {
            get
            {
                var stamina = this.GetResource(ResourceDef.Stamina);
                var staminaPercentage = stamina.Percentage;
                var threshold = StatDefOf.StaminaThresholdForWork.GetValue(this);
                var tired = staminaPercentage < threshold;
                return tired;
            }
        }

        public GameObject AttackTarget => null;//.GetComponent<AttackComponent>().Target;

        public IEnumerable<TaskGiver> GetTaskGivers()
        {
            var givers = this.GetComponent<NeedsComponent>().NeedsNew.Select(n => n.TaskGiver);
            givers = givers.Concat(TaskGiver.EssentialTaskGivers);
            var jobs = this.State.GetJobs().Where(j => j.Enabled);
            jobs.OrderBy(j => j.Priority);
            var jobTaskGivers = jobs.SelectMany(j => j.Def.GetTaskGivers());
            givers = this.IsCitizen ? givers.Concat(jobTaskGivers) : givers.Concat(TaskGiver.VisitorTaskGivers);
            return givers;
        }

        internal Trait GetTrait(TraitDef trait)
        {
            return this.GetComponent<PersonalityComponent>().Traits.First(t => t.Def == trait);
        }

        internal void WalkToggle(bool toggle)
        {
            if (this.Net is Server)
                PacketEntityWalkToggle.Send(this.Net, this.RefID, toggle);

            this.Mobile.ToggleWalk(toggle);
        }
        internal void SprintToggle(bool toggle)
        {
            if (this.Net is Server)
                PacketEntitySprintToggle.Send(this.Net, this.RefID, toggle);

            this.Mobile.ToggleSprint(toggle);
        }
        internal void CrouchToggle(bool toggle)
        {
            if (this.Net is Server)
                PacketEntityCrouchToggle.Send(this.Net, this.RefID, toggle);

            this.Mobile.ToggleCrouch(toggle);
        }
        internal void Jump()
        {
            if (this.Net is Server)
                PacketEntityJump.Send(this.Net, this.RefID);

            this.Mobile.Jump(this);
        }

        internal IEnumerable<TaskGiverResult> GetPossibleTasksOnTarget(TargetArgs target)
        {
            if (target == null || target.Type == TargetType.Null)
                yield break;
            var givers = TaskGiver.CitizenTaskGivers.Concat(TaskGiver.EssentialTaskGivers);
            foreach (var giver in givers)
            {
                var task = giver.TryTaskOn(this, target);
                if (task != null) yield return new TaskGiverResult(task, giver);// task;
            }
        }

        internal bool OwnsOrCanClaim(Entity item)
        {
            return this.GetPossesions().Contains(item) ? true : !this.Town.GetAgents().Any(a => a.Owns(item));
        }
        internal bool Owns(Entity item)
        {
            return this.GetPossesions().Contains(item);
        }

        internal GearType[] GetGearTypes()
        {
            return this.GetComponent<GearComponent>().Equipment.Slots.Select(s => GearType.Dictionary[(GearType.Types)s.ID]).ToArray();
        }
        internal Entity[] GetGear()
        {
            return this.GetComponent<GearComponent>().Equipment.Slots.Where(sl => sl.Object != null).Select(sl => sl.Object as Entity).ToArray();
        }
        internal Entity GetEquipmentSlot(GearType type)
        {
            return this.Gear.GetSlot(type).Object as Entity;
        }
        public ButtonNew GetButton(int width, Func<string> bottomText, Action onLeftClick)
        {
            return ButtonNew.CreateBig(onLeftClick, width, this.RenderIcon(), () => this.Name, bottomText);
        }
        internal float GetToolWorkAmount(int skillID)
        {
            if (this.Gear.GetSlot(GearType.Mainhand).Object is not Tool tool)
                return 1;

            var ability = tool.ToolComponent.Props.Ability;
            return ability.Effectiveness;
        }
        public int EvaluateItem(Entity item)
        {
            var score = ItemUsefulnessEvaluator.Evaluate(this, item);
            return score;
        }

        internal bool CanAcceptQuest(QuestDef quest)
        {
            return !this.GetVisitorProperties().HasQuest(quest);
        }
        internal void AcceptQuest(int questID)
        {
            this.GetVisitorProperties().AcceptQuest(this.Town.QuestManager.GetQuest(questID));
        }
        internal bool AcceptQuest(QuestDef quest)
        {
            return this.GetVisitorProperties().AcceptQuest(quest);
        }
        protected override void OnSpawn(MapBase map)
        {
            base.OnSpawn(map);
            if (this.GetVisitorProperties() is VisitorProperties props)
                props.OffsiteArea = null;
        }

        internal void AwardSkillXP(SkillDef skill, float v)
        {
            this.GetComponent<NpcSkillsComponent>().AwardSkillXP(this, skill, v);
        }


    }
}
