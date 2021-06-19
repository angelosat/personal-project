using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.AI;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    public class Actor : Entity
    {
        public Actor()
        {

        }


        //public override float Height => base.Height * (this.Mobile.Crouching ? .5f : 1);
        public override float Height => this.Physics.Height - (this.Mobile.Crouching ? 1 : 0);

        //internal Workplace GetShop()
        //{
        //    return this.Town.ShopManager.GetShop(this);
        //}
        internal Workplace Workplace => this.Town.ShopManager.GetShop(this);
        internal T GetWorkplace<T>() where T : Workplace
        {
            return this.Town.ShopManager.GetShop(this) as T;
        }

        public override string Name { get => this.Npc.FullName; }
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
            var net = this.NetNew;
            if (net is Server server)
            {
                //server.SyncInstantiate(loot);
                loot.SyncInstantiate(server);
                PacketInventoryInsertItem.Send(server, this, loot, area);
            }
            //else
            //    throw new Exception();
            //this.Log.Write($"[{this.InstanceID}] has looted [{loot.InstanceID}] while exploring {area.Name}"); // call this before inserting because the item might be absorbed/disposed
            //this.Log.Write($"[{this.Name}] has looted [{loot.Name},{loot.PrimaryMaterial.Color}] while exploring [{area.Name}]"); // call this before inserting because the item might be absorbed/disposed
            this.Log.Write($"Looted [{loot.Name},{loot.PrimaryMaterial.Color}] while exploring [{area.Name}]"); // call this before inserting because the item might be absorbed/disposed

            this.Inventory.Insert(loot);
            //net.SyncReport();
            //this.Log.Write($"{this.Name} has looted {loot.Name} while exploring {area.Name}");
        }
        internal bool InitiateTrade(Actor actor, Entity item, int itemcost)
        {
            // TODO do stuff with item and itemcost
            var state = this.GetState();
            if (state.TradingPartner != null)
                return false;
            //if (!this.HasMoney(itemcost))
            //    return false;
            state.TradingPartner = actor;
            return true;
        }

        internal bool HasMoney(int amount)
        {
            var coins = this.InventoryFirst(i => i.Def == ItemDefOf.Coins); // TODO find all ammount instead of find first
            return coins?.StackSize >= amount;
        }
        internal Entity GetMoney()
        {
            return this.InventoryFirst(i => i.Def == ItemDefOf.Coins) as Entity;
        }
        internal int GetMoneyTotal()
        {
            return PersonalInventoryComponent.Count(this, o => o.Def == ItemDefOf.Coins);
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
            PersonalInventoryComponent.GetHauling(this).Object = item;
        }
        public int CountItemsInInventory(Func<Entity, bool> filter)
        {
            return PersonalInventoryComponent.Count(this, filter);
        }
        public int CountItemsInInventory(ItemDef def)
        {
            return this.CountItemsInInventory(o => o.Def == def);
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

        public override bool IsHaulable => false;



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
            if (task != null)
            {
                this.GetState().ForceTask(task);
            }
        }

        internal bool CanStandIn(Vector3 global)
        {
            var map = this.Map;
            return
                map.GetBlock(global).IsStandableIn &&
                map.GetBlock(global.Above()).IsStandableIn &&
                map.GetBlock(global.Below()).IsStandableOn; //TODO: take into account actor's height instead of hardcoding checks 2 blocks above
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
            //topic.ApplyNew(this, this.GetState().ConversationPartner);
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

        internal bool CanStandOn(Vector3 global)
        {
            var map = this.Map;
            var above = global.Above();
            return
                map.GetBlock(global).IsStandableOn &&
                map.GetBlock(above).IsStandableIn &&
                map.GetBlock(above.Above()).IsStandableIn; //TODO: take into account actor's height instead of hardcoding checks 2 blocks above
        }
        internal void CancelInteraction()
        {
            AIManager.EndInteraction(this);
            return;
            this.GetComponent<WorkComponent>().Interrupt(this, false);
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



        MobileComponent _Mobile;
        public MobileComponent Mobile
        {
            get
            {
                if (this._Mobile == null)
                    this._Mobile = this.GetComponent<MobileComponent>();
                return this._Mobile;
            }
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




        static public Actor Create(ItemDef def)
        {
            var obj = new Actor
            {
                Def = def
            };
            obj.Physics.Height = def.Height;
            obj.Physics.Weight = def.Weight;
            //obj.GetInfo().CustomName = true;
            //obj.AddComponent(new GearComponent(
            //    //GearType.Hauling,
            //    GearType.Mainhand,
            //    GearType.Offhand,
            //    GearType.Head,
            //    GearType.Chest,
            //    GearType.Feet,
            //    GearType.Hands,
            //    GearType.Legs
            //    ));
            //obj.AddComponent(new ResourcesComponent(ResourceDef.Health, ResourceDef.Stamina));
            //obj.AddComponent(new AttributesComponent(AttributeDef.Strength, AttributeDef.Intelligence, AttributeDef.Dexterity));
            //obj.AddComponent(new NeedsComponent(NeedDef.Energy, NeedDef.Hunger, NeedDef.Social, NeedDef.Work));
            //obj.AddComponent(new ComponentNpcSkills(SkillDef.Digging, SkillDef.Construction));// new NpcSkillDigging()));
            //obj.AddComponent(new PersonalityComponent(TraitDef.Attention, TraitDef.Composure, TraitDef.Patience, TraitDef.Activity, TraitDef.Planning));


            obj.AddComponent(new AttributesComponent(def).Randomize());
            obj.AddComponent(new NpcSkillsComponent(def).Randomize());
            obj.AddComponent(new PersonalityComponent(def).Randomize());

            obj.AddComponent(new GearComponent(def));
            obj.AddComponent(new ResourcesComponent(def));
            //obj.AddComponent(new NeedsComponent(def));
            obj.AddComponent(new NeedsComponent(obj));
            obj.AddComponent(new PossessionsComponent());
            obj.AddComponent(new HaulComponent());
            obj.AddComponent(new NpcComponent());
            obj.AddComponent(new SpriteComponent(def.Body));
            obj.AddComponent(new PersonalInventoryComponent(16));
            obj.AddComponent(new StatsComponentNew());
            obj.AddComponent(new MobileComponent());
            obj.AddComponent(new WorkComponent());
            obj.AddComponent(new MoodComp());
            obj.AddComponent(new AIComponent().Initialize(//new Personality(TraitDef.Attention, TraitDef.Composure, TraitDef.Patience),// reaction: ReactionType.Friendly),
               new BehaviorQueue(
                   //new AIAwareness(),
                   new AIMemory(),
                   new BehaviorCombat(),
                   new BehaviorHandleResources(),
                   //new AIDialogue(),
                   //new AIFollow(),
                   //new BehaviorMoodlets(),
                   new BehaviorHandleOrders(),
                   new AI.Behaviors.Tasks.BehaviorFindTask(),

                   //new BehaviorSatisfyNeed(),
                   //new BehaviorFindJobNew(),
                   //new BehaviorAct(),

                   new BehaviorIdle()
                   )));
            //Sprite sprite = def.DefaultSprite;
            obj.AddComponent(new SpriteComponent(def.Body));//, sprite));
            foreach (var b in obj.Body.GetAllBones())
                b.Material = def.DefaultMaterial;

            obj.Sprite.Customization = new CharacterColors(obj.Body).Randomize();

            //Sprite sprite = new Sprite("mobs/skeleton/full", new Vector2(17 / 2, 38));
            //sprite.OriginGround = new Vector2(sprite.AtlasToken.Texture.Bounds.Width / 2, sprite.AtlasToken.Texture.Bounds.Height);
            //sprite.OriginGround = new Vector2(sprite.AtlasToken.Texture.Bounds.Width / 2, sprite.AtlasToken.Texture.Bounds.Height);

            //obj.AddComponent<SpriteComponent>().Initialize(BodyDef.Skeleton, sprite);
            //foreach (var b in obj.Body.GetAllBones())
            //    b.Material = Start_a_Town_.Components.Materials.Material.Flesh;
            //obj.InitComps();
            obj.ObjectCreated();
            return obj;
        }
        //protected override Color NameColorFunc => this.IsCitizen ? Color.White : Color.Cyan;
        public override Color GetNameplateColor()
        {
            return this.IsCitizen ? Color.White : Color.Cyan;
        }
        //protected override void NameplateCreated(Nameplate plate)
        //{
        //    var defcomp = this.DefComponent;
        //    plate.Controls.Add(new Label()
        //    {
        //        //Width = 100,
        //        Font = UIManager.FontBold,
        //        //TextFunc = GetName,// this.GetNameplateText,
        //        TextFunc = () => this.Name,
        //        TextColorFunc = ()=>this.IsCitizen ? Color.White : Color.Cyan,
        //        MouseThrough = true,
        //        TextBackgroundFunc = () => this.HasFocus() ? defcomp.Quality.Color * .5f : Color.Black * .5f
        //        //TextBackgroundFunc = () => Color.Red,//parent.HasFocus() ? this.Quality.Color * .5f : Color.Black * .5f
        //        //BackgroundColorFunc = () => parent.HasFocus() ? this.Quality.Color * .5f : Color.Black * .5f
        //    });
        //}
        internal void EndCurrentTask()
        {
            //this.GetState().CurrentTaskBehavior = null;

            this.GetComponent<AIComponent>().FindBehavior<global::Start_a_Town_.AI.Behaviors.Tasks.BehaviorFindTask>().EndCurrentTask(this);
        }
        internal void MoveToggle(bool toggle)
        {
            if (this.Net is Server)
                PacketEntityMoveToggle.Send(this.Net, this.RefID, toggle);

            this.Mobile.Toggle(this, toggle);
            //if (toggle)
            //    this.Mobile.Start(this);
            //else
            //    this.Mobile.Stop(this);
        }
        public void AddMoodlet(Moodlet m)
        {
            this.GetComponent<MoodComp>().Add(this, m);
        }
        public void RemoveMoodlet(MoodletDef mdef)
        {
            this.GetComponent<MoodComp>().Remove(this, mdef);
        }
        public bool HasMoodlet(MoodletDef mdef)
        {
            return this.GetComponent<MoodComp>().Contains(mdef);
        }
        //public bool GetMoodlet(MoodletDef mdef)
        //{
        //    return this.GetComponent<MoodComp>().(mdef);
        //}
        public float GetMood()
        {
            return this.GetComponent<MoodComp>().Mood;
        }

        public bool HasRoomAssigned()
        {
            var manager = this.Map.Town.RoomManager;
            return manager.FindRoom(this.RefID) != null;
        }
        //public Room GetRoom()
        //{
        //    var manager = this.Map.Town.RoomManager;
        //    return manager.FindRoom(this.InstanceID);
        //}


        //public override void Select(UISelectedInfo info)
        //{
        //    base.Select(info);
        //    info.AddTabAction("Skills", () => WindowTargetManagementStatic.Refresh(this));
        //}
        protected override IEnumerable<(string name, Action action)> GetInfoTabsExtra()
        {
            yield return ("Log", () => NpcLogUINew.GetUI(this).Toggle());
            yield return ("Skills", () => NpcSkillsComponent.GetUI(this).Toggle());
            yield return ("Gear", () => InventoryUI.GetUI(this).Toggle());
            yield return ("Personality", () => PersonalityComponent.GetGUI(this).Toggle());
            yield return ("Needs", () => NeedsUI.GetUI(this).Toggle());
            yield return ("Stats", () => StatsInterface.GetUI(this).Toggle());
            if (!this.IsCitizen)
                yield return ("Visitor", this.GetVisitorProperties().ShowGUI);
        }
        public override void TabGetter(Action<string, Action> getter)
        {
            throw new Exception();
            base.TabGetter(getter);
            //getter("Details", () => WindowTargetManagementStatic.Refresh(this));
            getter("Log", () => NpcLogUINew.GetUI(this).Toggle());
            getter("Skills", () => NpcSkillsComponent.GetUI(this).Toggle());
            getter("Gear", () => InventoryUI.GetUI(this).Toggle());
            getter("Personality", () => PersonalityComponent.GetGUI(this).Toggle());
            getter("Needs", () => NeedsUI.GetUI(this).Toggle());
            getter("Stats", () => StatsInterface.GetUI(this).Toggle());
            if (!this.IsCitizen)
                getter("Visitor", this.GetVisitorProperties().ShowGUI);
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
            var operatingPositions = this.Map.GetCell(global).GetOperatingPositions();
            if (!operatingPositions.Any())
                return true;
            foreach (var pos in operatingPositions)
            {
                if (this.CanReach(global + pos) && this.Map.GetBlock(global + pos).IsStandableIn)
                    return true;
            }
            return false;
        }
        public bool CanOperate(Vector3 global, out IntVec3 operatingPos)
        {
            var poos = this.FindOperatablePosition(global);
            operatingPos = poos.Value;
            return poos.HasValue;
            var operatingPositions = this.Map.GetCell(global).GetOperatingPositions();
            if (!operatingPositions.Any())
                return true;
            foreach (var pos in operatingPositions)
            {
                if (this.CanReach(global + pos) && this.Map.GetBlock(global + pos).IsStandableIn)
                    return true;
            }
            return false;
        }
        public IntVec3? FindOperatablePosition(Vector3 facilityGlobal)
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
            //var maxHaulWeight = StatDefOf.MaxHaulWeight.GetValue(this);
            //var activityLevel = this.GetTrait(TraitDefOf.Activity)?.Normalized ?? 0;
            //var maxDesiredEncumberance = maxHaulWeight + maxHaulWeight * activityLevel * .5f;
            //var unitWeight = haulable.Physics.Weight;
            //int stackEnduranceLimit = (int)Math.Floor(maxDesiredEncumberance / unitWeight);
            //return stackEnduranceLimit;
        }
        
        internal float GetOpportunisticHaulSearchRange(int baseSearchRange)
        {
            var organizationValue = this.GetTrait(TraitDefOf.Planning)?.Normalized ?? 0;
            var num = baseSearchRange * organizationValue * .5f;
            return baseSearchRange + num;
            //return (float)Math.Round(baseSearchRange + num);
        }

        public bool IsTooTiredToWork
        {
            get
            {
                var stamina = this.GetResource(ResourceDef.Stamina);
                var staminaPercentage = stamina.Percentage;
                //var staminaBaseThreshold = .2f;
                //var activity1 = this.GetTrait(TraitDef.Activity).Normalized;
                //var num = activity1 * staminaBaseThreshold  *.5f;
                //var threshold = staminaBaseThreshold - num;
                var threshold = StatDefOf.StaminaThresholdForWork.GetValue(this);
                var tired = staminaPercentage < threshold;
                return tired;
            }
        }
        public IEnumerable<TaskGiver> GetTaskGivers()
        {
            var givers = this.GetComponent<NeedsComponent>().NeedsNew.Select(n => n.TaskGiver);
            givers = givers.Concat(TaskGiver.EssentialTaskGivers);
            //if(parent.IsCitizen)
            //    givers = givers.Concat(TaskGiver.CitizenTaskGivers);
            var jobs = this.State.GetJobs().Where(j => j.Enabled);
            jobs.OrderBy(j => j.Priority);
            var jobTaskGivers = jobs.SelectMany(j => j.Def.GetTaskGivers());
            givers = this.IsCitizen ? givers.Concat(jobTaskGivers) : givers.Concat(TaskGiver.VisitorTaskGivers);
            return givers;
        }
        AIState _CachedState;
        public AIState State
        {
            get
            {
                return this._CachedState ??= this.GetComponent<AIComponent>().State;
            }
        }
        internal PersonalityComponent Personality
        { get { return this.GetComponent<PersonalityComponent>(); } }

        internal void InsertToInventory(Entity item)
        {
            PersonalInventoryComponent.InsertItem(this, item);
        }
        internal void RemoveFromInventory(Entity item)
        {
            PersonalInventoryComponent.RemoveItem(this, item);
        }
        internal NpcSkillsComponent Skills
        { get { return this.GetComponent<NpcSkillsComponent>(); } }
        internal AttributesComponent Attributes
        { get { return this.GetComponent<AttributesComponent>(); } }
        internal NpcComponent Npc
        { get { return this.GetComponent<NpcComponent>(); } }
        PossessionsComponent _Ownership;
        public PossessionsComponent Ownership => this._Ownership ??= this.GetComponent<PossessionsComponent>(); 
        public Interaction CurrentInteraction { get { return this.GetComponent<WorkComponent>().Task; } }

        public AILog Log
        {
            get
            {
                return AIState.GetState(this).History;
            }
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
                //foreach (var giver in TaskGiver.CitizenTaskGivers)
                {
                    var task = giver.TryTaskOn(this, target);
                if (task != null) yield return new TaskGiverResult(task, giver);// task;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="trueIfNoOwner">If true, then returns true even if the item has no set owner</param>
        /// <returns></returns>
        internal bool Owns(Entity item, bool trueIfNoOwner)
        {
            OwnershipComponent ownership;
            if (!item.TryGetComponent<OwnershipComponent>("Ownership", out ownership))
                return true; // items that can't be owned return true
            if (trueIfNoOwner)
                return ownership.Owner == this.RefID || ownership.Owner == -1;
            else
                return ownership.Owner == this.RefID;
        }
        internal bool OwnsOrCanClaim(Entity item)
        {
            return this.GetPossesions().Contains(item) ? true : !this.Town.GetAgents().Any(a => a.Owns(item));
        }
        internal bool Owns(Entity item)
        {
            return this.GetPossesions().Contains(item);
        }
        public Room AssignedRoom
        {
            get
            {
                return this.Town.RoomManager.FindRoom(this.RefID);
              }
        }

        public bool IsCitizen
        {
            get
            {
                return this.Town.Agents.Contains(this.RefID);
            }
        }

        internal GearType[] GetGearTypes()
        {
            return this.GetComponent<GearComponent>().Equipment.Slots.Select(s=>GearType.Dictionary[(GearType.Types)s.ID]).ToArray();
        }
        internal Entity[] GetGear()
        {
            return this.GetComponent<GearComponent>().Equipment.Slots.Where(sl=>sl.Object != null).Select(sl=>sl.Object as Entity).ToArray();
        }
        internal Entity GetEquipmentSlot(GearType type)
        {
            return GearComponent.GetSlot(this, type).Object as Entity;
        }
        public ButtonNew GetButton(int width, Func<string> bottomText, Action onLeftClick)
        {
            //return ButtonNew.CreateBig(onLeftClick, width, this.RenderIcon(), () => this.Npc.FullName, bottomText);
            return ButtonNew.CreateBig(onLeftClick, width, this.RenderIcon(), () => this.Name, bottomText);
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
            //this.Town.QuestManager.GetQuest(questID).AcceptBy(this);
            this.GetVisitorProperties().AcceptQuest(this.Town.QuestManager.GetQuest(questID));
        }
        internal bool AcceptQuest(QuestDef quest)
        {
            return this.GetVisitorProperties().AcceptQuest(quest);
        }
        public override void Spawn(IObjectProvider net)
        {
            base.Spawn(net);
            if (this.GetVisitorProperties() is VisitorProperties props)
                props.OffsiteArea = null;
        }
        public IItemPreferencesManager ItemPreferences => this.GetItemPreferences();
        public IItemPreferencesManager GetItemPreferences()
        {
            return this.GetState().ItemPreferences;
            //var props = actor.Map.World.Population.GetVisitorProperties(actor);
            //var prefs = props.ItemPreferences;
            //return prefs;
        }
        internal void AwardSkillXP(SkillDef skill, float v)
        {
            this.GetComponent<NpcSkillsComponent>().AwardSkillXP(this, skill, v);
        }
    }
}
