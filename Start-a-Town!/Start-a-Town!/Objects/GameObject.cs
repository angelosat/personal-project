using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;
using Start_a_Town_.Towns;
using Start_a_Town_.Animations;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    public class GameObject : IEntity, ITooltippable, IContextable, INameplateable, IDebuggable, ISlottable, ISelectable, IEntityCompContainer, ILabeled//, IHasChildren
        //, ISaveable, ISerializable
    {
        static public Dictionary<int, GameObject> Templates = new();
        public string Label => this.Def.Label;
        static int GetNextTemplateID()
        {
            return Templates.Count + 1;
            //return Objects.Count + 1;

        }


        static public void AddTemplates(IEnumerable<GameObject> templates)
        {
            foreach (var o in templates)
                AddTemplate(o);
        }
        static public int AddTemplate(GameObject obj)
        {
            var id = GetNextTemplateID();
            Templates.Add(id, obj);
            // Objects.Add(id, obj);
            return id;
        }
        internal static GameObject CloneTemplate(int templateID)
        {
            return Templates[templateID].Clone();
        }


        public string GetName()
        { return this.Name; }
        public Color GetSlotColor()
        { return this.GetInfo().GetQualityColor(); }
        public string GetCornerText()
        { return this.GetInfo().StackSize.ToString(); }
        public void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            var sprite = this.GetSpriteOrDefault();
            var source = sprite.GetSourceRect();
            sprite.Draw(sb, pos - new Vector2(source.Width, source.Height) * 0.5f, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        //internal Resource GetResource(ResourceDef def)
        //{
        //    return this.GetComponent<ResourcesComponent>()?.GetResource(def);
        //}

        internal bool IsCloserTo(Vector3 global, ref float distanceToCompare)
        {
            var dist = Vector3.DistanceSquared(this.Global, global);
            if (dist < distanceToCompare)
            {
                distanceToCompare = dist;
                return true;
            }
            return false;
        }

        //internal T GetResource<T>(ResourceDef def) where T: ResourceDef
        //{
        //    return this.GetComponent<ResourcesComponent>()?.GetResource(def) as T;
        //}
        internal bool HasResource(ResourceDef type)// Resource resource)
        {
            return this.GetResource(type) != null;
        }
        internal AttributeStat GetAttribute(AttributeDef att) => this.GetComponent<AttributesComponent>().GetAttribute(att);


        [Obsolete]
        public GameObject Debug() { return this; }

        static public event EventHandler<ObjectEventArgs> MessageHandled;

        private DefComponent _DefComponent;
        public DefComponent DefComponent
        {
            get
            {
                if (this._DefComponent == null)
                    this._DefComponent = this.GetComponent<DefComponent>();
                return this._DefComponent;
            }
        }
        public ItemDef Def { get { return this.DefComponent.Def; } set { this.DefComponent.Def = value; } }
        public Quality Quality { get { return this.DefComponent.Quality; } set { this.DefComponent.Quality = value; } }

        public T GetDef<T>() where T : ItemDef
        {
            return this.Def as T;
        }
        static void OnMessageHandled(GameObject receiver, ObjectEventArgs e)
        {
                MessageHandled?.Invoke(receiver, e);
        }



        public GameObjectSlot ToSlotLink(int amount = 1)
        {
            return new GameObjectSlot() { Link = this };
            //return new GameObjectSlot(this, this.StackSize);
            //return new GameObjectSlot(this);//, amount);
        }
        public Memory ToMemory(GameObject actor)
        {
            return new Memory(this, 100, 100, 1, actor);//, this.Query(actor).Select(i => i.Effect.Key).ToArray());
        }

      

        public enum BaseTypes
        {
            Actor,
            Tree,
            Worktool,
            Tile,
            Blueprint
        }

        public enum Types
        {
            Actor,
            Tree,
            //Log,
            Workbench = 3,
            Package,
            //WoodenPlank,
            //Worktool = 6,
            Axe = 7,
            //Handsaw,
            Hammer = 9,
            Tile,
            Blueprint,
            //Soilbag,
            Soil = 13,
            Grass,
            Wall,
            WoodenDeck,
            Potion,
            StatusCondition,
            StrengthPotion,
            Sand,
            Material,
            Shovel,
            Stone,
            Rock,
            Pickaxe,
            Skill,
            SkillMining,
            SkillLumberjacking,
            Hoe,
            Farmland,
            Seeds,
            BerryBush,
            Consumable,
            Berries,
            Air,
            BareHands,
            BareFeet,
            BodyPart,
            Fists,
            Water,
            EpicShovel,
            Chunk,
            World,
            Map,
            Flowers,
            Construction,
            Cobblestone,
            BlockLight,
            CellLight,
            Mineral,
            Coal,
            Campfire,
            Door,
            WallQuarter,
            WallHalf,
            Bed,
            BigBed,
            Chest,
            Crate,
            WallTile,
            Dummy,
            Scaffolding,
            Action,
            Shoe,
            AbilityDigging,
            AbilityAttack,
            AbilityConsume,
            AbilityActivate,
            AbilityMining,
            AbilityBuilding,
            AbilityCrafting,
            AbilityTilling,
            AbilityPlanting,
            AbilityPickingUp,
            AbilityDrop,
            AbilityChop,
            AbilitySaw,
            AbilityGive,
            //FurnitureParts,
            BlueprintAxe,
            BlueprintFurnitureParts,
            //    BlueprintBlank,
            PickaxeHead = BlueprintFurnitureParts + 2,
            Handle,
            BlueprintPickaxe,
            BlueprintHandle,
            BlueprintPickaxeHead,
            Npc,
            Zombie,
            RottenFeet,
            TestJob,
            ManageEquipment,
            AssignJob,
            BlueprintBed,
            BlueprintWoodenDeck,
            BlueprintSoil,
            BlueprintWorkbench,
            Construct,
            Twig,
            Cobblestones,
            BlueprintHammer,
            BlueprintAxeHead,
            AxeHead,
            BlueprintHandsaw,
            ShovelHead,
            BlueprintShovelHead,
            BlueprintShovel,
            Project,
            ConstructionReservedTile,
            BuildingPlan,
            BlueprintCobblestone,
            BlueprintScaffold,
            TrainingDummy,
            BlueprintCampfire,
            ConstructionBlock,
            BlueprintDoor,
            JobBoard,
            AbilityThrow,
            AbilityWalk,
            AbilityJump,
            BlueprintBlock,
            EditorAir,
            //   BlockEmpty,
            Sword,
            Gravel,

            Spell,//
            WoodenFrame,
            AbilityFraming,
            Ability,
            CheatHammer,
            Iron,
            Furnace,
            //Bar,
            //Smeltery,
            Shield,
            //Ore,
            Default,
            Brain,
            Helmet,
            Paper,
            ScribeBench,
            BenchReactions,
            CobblestoneItem,
            BlueprintCobblestoneItem,
            Block,
            WoodenPlankDark,
            WoodenPlankRed,
            Fertilizer,
            Bomb,
            Pie,
        }

        //static Queue<Message> MessageQueue = new Queue<Message>();
        static public GameObjectCollection Objects = new();
        #region Initialization
        static public void LoadObjects()
        {
            PlantProperties.Init();
            PlantDefOf.Init();
            //Objects.Add(Npc.Create());
            //Objects.Add(GameObjectDb.Campfire);
            //Objects.Add(GameObjectDb.Package);
            //Objects.Add(GameObjectDb.ShovelHead);


            //Objects.Add(GameObjectDb.Fertilizer);
            //Objects.Add(GameObjectDb.Fists);
            //Objects.Add(GameObjectDb.RottenFeet);
            //Objects.Add(GameObjectDb.BareFeet);
            //Objects.Add(GameObjectDb.BareHands);
            //Objects.Add(GameObjectDb.Crate);
            //Objects.Add(GameObjectDb.CheatShoes);
            //Objects.Add(GameObjectDb.Twig);
            //Objects.Add(GameObjectDb.Cobblestones);
            //Objects.Add(GameObjectDb.TrainingDummy);
            //Objects.Add(
            //    GameObjectDb.Shield,
            //    GameObjectDb.Brain,
            //    GameObjectDb.Helmet,
            //    GameObjectDb.Paper,
            //    BombComponent.GetEntity(3, Engine.TicksPerSecond * 2)
            //    );

            //AddTemplate(ItemDefOf.Seeds.CreateFrom(MaterialDefOf.Berry));
            //AddTemplate(ItemFactory.CreateSeeds(PlantDefOf.BerryBush));

            AddTemplate(ItemFactory.CreateItem(ApparelDefOf.Helmet));
            //AddTemplate(ItemFactory.CreateItem(ItemDefOf.Coins).SetStackSize(100));
            //AddTemplate(ItemFactory.CreateItem(ItemDefOf.Berries));

            //AddTemplate(Actor.Create(ActorDefOf.Npc).SetName("Npc"));
            //AddTemplate(ActorDefOf.Npc.Create().SetName("Npc"));
            //AddTemplate(new Actor(ActorDefOf.Npc).SetName("Npc"));
            AddTemplate(Actor.Create(ActorDefOf.Npc).SetName("Npc"));
        }


        #endregion

        #region Common Properties
        public virtual string Name
        {
            get
            {
                //return GetComponent<GeneralComponent>("Info").GetProperty<string>("Name");
                return this.GetInfo().GetName();
            }
            set
            {
                var info = GetInfo();// GetInfo().Name = value; }//GetInfo()["Name"] = value; }
                info.Name = value;
                //info.CustomName = true;
            }
        }
        //public string Firstname
        //{
        //    get
        //    {
        //        return this.GetInfo().GetName().Split(' ').First();
        //    }
        //    set
        //    {
        //        var lstname = this.GetInfo().GetName().Split(' ').Last();
        //        this.Name = value + " " + lstname;
        //    }
        //}
        //public string Lastname
        //{
        //    get
        //    {
        //        return this.GetInfo().GetName().Split(' ').Last();
        //    }
        //    set
        //    {
        //        var fstname = this.GetInfo().GetName().Split(' ').First();
        //        this.Name = fstname + " " + value;
        //    }
        //}
        public string Description
        {
            //get { return GetComponent<GeneralComponent>("Info").GetProperty<string>("Description"); }
            //set { GetInfo()["Description"] = value; }
            get { return this.GetInfo().Description; }
            set { this.GetInfo().Description = value; }
        }
        public GameObject.Types IDType
        {
            //get { return GetComponent<GeneralComponent>("Info").GetProperty<GameObject.Types>("ID"); }
            //set { this.GetInfo()["ID"] = value; }
            get { return (GameObject.Types)this.GetInfo().ID; }
            //set { this.GetInfo()["ID"] = (int)value; }
            set { this.GetInfo().ID = (int)value; }
        }
        public int ID
        {
            get
            {
                //throw new Exception();
                return GetComponent<DefComponent>().ID;
            }
            //set { GetComponent<GeneralComponent>().ID = value; }
        }
        public string Type
        {
            //get { return GetComponent<GeneralComponent>("Info").GetProperty<string>("Type"); }
            //set { GetInfo()["Type"] = value; }
            get { return this.GetInfo().Type; }
            set { this.GetInfo().Type = value; }
        }

        public virtual float Height => this.Physics.Height;

        public int RefID;
        //{
        //    get { return this.InstanceID; }
        //}
        // TODO: make field
        public IObjectProvider NetNew;
        public IObjectProvider Net//;
        {
            //get { return this.Map?.Net; }
            get { return this.Parent?.Map.Net ?? this.Map?.Net; }

            //get { return this.Map.GetNetwork(); }
            //set { this.Network.Net = value; }
        }
        //public IMap Map { get; set; }
        IMap _Map;
        public IMap Map { get { return this.Parent?.Map ?? this._Map; } set { this._Map = value; } }

        //{
        //    get
        //    {
        //        return this.Net.Map;
        //    }
        //}
        public Town Town
        {
            get
            {
                return this.Net.Map.Town;
            }
        }
        readonly object ForbidActionToken = new();
        public virtual void Select(UISelectedInfo info)
        {
            if (this.IsForbiddable())
                info.AddButton(IconForbidden, RequestToggleForbidden, this);

            info.AddIcon(IconCameraFollow);//new IconButton(Icon.Replace) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = FollowCam, HoverText = "Camera follow" });

            //foreach (var comp in this.Components.Values)
            //    comp.Select(info, this);

            //this.Map.Town.Select(this, info);
        }

        internal object GetPath()
        {
            return this.GetState().Path;
        }
        public IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            foreach (var comp in this.Components.Values)
                foreach (var i in comp.GetInfoTabs())
                    yield return i;
            foreach (var i in this.GetInfoTabsExtra())
                yield return i;
        }
        protected virtual IEnumerable<(string name, Action action)> GetInfoTabsExtra() { yield break; }
        public virtual void GetSelectionInfo(IUISelection info)
        {
            info.AddIcon(IconCameraFollow);//new IconButton(Icon.Replace) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = FollowCam, HoverText = "Camera follow" });
            this.Map.World.OnTargetSelected(info, this);
            foreach (var comp in this.Components.Values)
                comp.GetSelectionInfo(info, this);
        }
        public virtual void GetQuickButtons(UISelectedInfo info)
        {
            if (this.IsForbiddable())
                info.AddButton(IconForbidden, RequestToggleForbidden, this);
            foreach (var comp in this.Components.Values)
                comp.GetQuickButtons(info, this);
            //this.Map.Town.Select(this, info);
        }
        //static IconButton IconForbidden = new IconButton(Icon.Cross) { HoverText = "Toggle forbidden" };
        static readonly IconButton IconForbidden = new QuickButton(Icon.Cross, KeyBind.ToggleForbidden, "Forbid") { HoverText = "Toggle forbidden" };

        static readonly IconButton IconCameraFollow = new(Icon.Replace) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = FollowCam, HoverText = "Camera follow" };

        static void RequestToggleForbidden(List<TargetArgs> obj)
        {
            //PacketToggleForbidden.Send(obj.First().Net, obj.Select(o => o.InstanceID).ToList());
            PacketToggleForbidden.Send(obj.First().Network, obj.Select(o => o.Object.RefID).ToList());

        }
        static void FollowCam()
        {
            ScreenManager.CurrentScreen.Camera.ToggleFollowing(UISelectedInfo.Instance.SelectedSource.Object);

            //ScreenManager.CurrentScreen.Camera.ToggleFollowing(this);
        }
        private void FollowCamOld()
        {
            ScreenManager.CurrentScreen.Camera.ToggleFollowing(this);
        }

        public void ToggleForbidden()
        {
            this.IsForbidden = !this.IsForbidden;
        }
        public void DrawNameplate(SpriteBatch sb, Rectangle viewport, Nameplate plate)
        {
            plate.Draw(sb, viewport);
        }
        public void OnNameplateCreated(Nameplate plate)
        {
            //plate.Controls.Add(new Label()
            //{
            //    //Width = 100,
            //    Font = UIManager.FontBold,
            //    Text = this.Name,
            //    TextColorFunc = () => GetNameplateColor(),
            //    MouseThrough = true,
            //});

            //this.NameplateCreated(plate);

            foreach (var comp in Components.Values)
                comp.OnNameplateCreated(this, plate);
        }
        //protected virtual Color NameColorFunc => this.DefComponent.Quality.Color;
        //void NameplateCreated(Nameplate plate)
        //{
        //    var defcomp = this.DefComponent;
        //    plate.Controls.Add(new Label()
        //    {
        //        //Width = 100,
        //        Font = UIManager.FontBold,
        //        //TextFunc = GetName,// this.GetNameplateText,
        //        TextFunc = () => this.Name,
        //        TextColorFunc = ()=>this.NameColorFunc,// defcomp.GetQualityColor,
        //        MouseThrough = true,
        //        TextBackgroundFunc = () => this.HasFocus() ? defcomp.Quality.Color * .5f : Color.Black * .5f
        //        //TextBackgroundFunc = () => Color.Red,//parent.HasFocus() ? this.Quality.Color * .5f : Color.Black * .5f
        //        //BackgroundColorFunc = () => parent.HasFocus() ? this.Quality.Color * .5f : Color.Black * .5f

        //    });
        //}
        public void OnHealthBarCreated(Nameplate plate)
        {
            //plate.Controls.Add(new Label()
            //{
            //    //Width = 100,
            //    Font = UIManager.FontBold,
            //    Text = this.Name,
            //    TextColorFunc = () => GetNameplateColor(),
            //    MouseThrough = true,
            //});
            foreach (var comp in Components.Values)
                comp.OnHealthBarCreated(this, plate);
        }
        public Rectangle GetScreenBounds(Camera camera)
        {
            var g = this.Global;
            //var boundsasd = camera.GetScreenBounds(g.X, g.Y, g.Z, this.GetSprite().GetBounds(), 0, 0, this.Body.Scale);
            var bounds = camera.GetScreenBounds(g.X, g.Y, g.Z, this.GetComponent<SpriteComponent>().GetSpriteBounds(), 0, 0, this.Body.Scale);
            return bounds;
        }



        public virtual Color GetNameplateColor()
        {
            //return this.GetInfo().GetQualityColor();// *0.5f;
            return this.DefComponent.Quality.Color;
        }

        public GameObject Parent
        {
            get
            {
                return
                    this.Transform.Parent;
            }
            set
            {
                this.Transform.Parent = value;
            }
        }
        public Vector3 Global
        {
            get
            {
                //return GetPosition().Global;
                return this.Transform.Global;
            }
            set
            {
                //this.GetPosition().Global = value;
                this.Transform.Global = value;
            }
        }
        //public IntVec3 Cell { get { return this.Transform.Global.SnapToBlock(); } }
        public IntVec3? Cell { get { return this.IsSpawned ? this.Transform.Global.SnapToBlock() : null; } }

        public float Acceleration
        {
            get
            {
                return this.GetComponent<MobileComponent>().Acceleration;
            }
            set
            {
                this.GetComponent<MobileComponent>().Acceleration = value;
            }
        }
        public Vector3 Velocity
        {
            get
            {
                //return this.GetPosition().Position.Velocity; 
                return this.Transform.Position.Velocity;
            }
            set
            {
                //this.GetPosition().Position.Velocity = value;
                if (float.IsNaN(value.X) || float.IsNaN(value.Y))
                    throw new Exception();
                this.Transform.Position.Velocity = value;
                if (value != Vector3.Zero)
                    PhysicsComponent.Enable(this);

                // TODO: change direction here? so that direction is update automatically when reading snapshots instead of constantly sending changedirection packets 
                // WARNING: direction becomes jiterry when deriving it from velocity! maybe try including it in the entity snapshots?

                //var dir = value.XY();
                //dir.Normalize();
                //this.Direction = new Vector3(dir, 0); 
            }
        }
        public Vector3 Direction
        {
            get { return new Vector3(this.Transform.Direction, 0); }
            set
            {
                if (float.IsNaN(value.X) || float.IsNaN(value.Y))
                    throw new Exception();
                var transform = this.Transform;
                var olddir = transform.Direction;
                var newdir = new Vector2(value.X, value.Y);
                //if (olddir != newdir)
                //    this.Net.LogStateChange(this.InstanceID);
                transform.Direction = newdir;
            }
            //get { return new Vector3(this.Transform.Direction, this.Global.Z); }
            //set
            //{
            //    this.Transform.Direction = new Vector2(value.X, value.Y);
            //}
        }

        public Vector3 GridCell
        {
            get { return this.Global.SnapToBlock(); }
            set { this.ChangePosition(value); }
        }
        public Vector3 GridCellOffset
        {
            get
            {
                var grid = this.Global.SnapToBlock();
                return this.Global - grid;
            }
            set
            {
                this.ChangePosition(this.GridCell + value);
            }
        }


        public Chunk GetChunk()
        {
            //return this.Global.GetChunk(this.Map);
            return this.Map.GetChunk(this.Global);

        }

        public int StackSize
        {
            get { return this.GetInfo().StackSize; }
            set
            {
                var oldSize = this.StackSize;
                var newSize = Math.Min(value, this.StackMax);
                if (oldSize == newSize)
                    return;
                value = newSize;
                this.GetInfo().StackSize = value;
                //if(this.Net!=null)
                //    this.Net.EventOccured(Message.Types.StackSizeChanged, this);
                if (value == 0)
                {
                    if (this.IsSpawned)
                        this.Net.Despawn(this);
                    if (this.Slot != null)
                        this.Slot.Clear();
                    this.Dispose();
                }
                else if (value < 0)
                    throw new Exception();
            }
        }

        //public void SetStackSize(int value)
        //{

        //}
        public bool Full
        {
            get { return this.StackSize == this.StackMax; }
        }
        public int StackMax
        {
            get { return this.GetInfo().StackMax; }
        }
        //public ControlComponent Control
        //{
        //    get
        //    {
        //        return this.GetComponent<ControlComponent>();
        //    }
        //}

        public Bone Body
        {
            get { return this.GetComponent<SpriteComponent>().Body; }
        }
        internal Material PrimaryMaterial => this.Body.Material;
        //public Vector2 Direction
        //{
        //    get { return this.GetPosition().Direction; }
        //    set
        //    {
        //        this.GetPosition().Direction = value;
        //    }
        //}
        public GameObject SetGlobal(Vector3 global)
        {
            this.ChangePosition(global);
            //this.Global = global;
            return this;
        }

        public GameObject ChangePosition(Vector3 global) // TODO: merge this with SetGlobal
        {
            //global = global.Round(2);
            //global.Z.ToConsole();

            if (this.Map.IsSolid(global))// + Vector3.UnitZ * 0.01f))// TODO: FIX THIS
                return this; // TODO: FIX: problem when desynced from server, block might be empty on server but solid on client
                             //throw new Exception();
                             //Chunk nextChunk, lastChunk;
            Position pos = this.Transform.Position;
            if (pos == null)
            {

                //this.Transform.Position = new Position(this.Map, global);
                this.Global = global;
                bool added = Chunk.AddObject(this, this.Map, global);
                if (!added)
                    throw new Exception("Could not add object to chunk");
                return this;
            }
            //Position.TryGetChunk(this.Map, global.Round(), out nextChunk);
            this.Map.TryGetChunk(global.RoundXY(), out Chunk nextChunk);

            if (nextChunk == null)
            {
                //this.Net.EventOccured(Message.Types.EntityEnteringUnloadedChunk, this);
                return this;
            }
            //if (pos.Map.IsNull())
            //{
            //    Chunk.AddObject(this, this.Map, nextChunk, Position.Floor(global));
            //    pos.Global = global;
            //    return this;
            //}

            //Position.TryGetChunk(this.Map, pos.Rounded, out lastChunk);
            this.Map.TryGetChunk(pos.Rounded, out Chunk lastChunk);

            if (nextChunk != lastChunk)
            {
                //if (this.Exists)
                //{
                bool removed = Chunk.RemoveObject(this, lastChunk);
                if (!removed)
                    throw new Exception("Source chunk is't loaded"); //Could not remove object from previous chunk");
                //}

                bool added = Chunk.AddObject(this, this.Map, nextChunk, Position.Floor(global));
                if (!added)
                    throw new Exception("Invalid move: Destination chunk is't loaded");
                this.Net.EventOccured(Message.Types.EntityChangedChunk, this, nextChunk.MapCoords, lastChunk.MapCoords);
            }

            pos.Global = global;
            this.Physics.Enabled = true;
            return this;
        }
        public bool IsForbidden;
        public bool IsSpawned
        {
            get
            {
                return this.Transform.Exists;
            }
            set
            {
                this.Transform.Exists = value;
            }
        }
        public bool IsReserved { get { return this.Town.ReservationManager.IsReserved(this); } }
        public bool IsPlayerControlled { get { return this.Net.GetPlayers().Any(p => p.ControllingEntity == this); } }
        public virtual bool IsHaulable
        {
            //get { return this.Physics.Size != ObjectSize.Immovable; }
            get { return this.Def.Haulable; }
        }
        internal bool IsFuel
        {
            get { return this.Material?.Fuel.Value > 0; }
        }

        public GameObjectSlot Slot;
        #endregion
        internal static GameObject Generate(int entityType, RandomThreaded random)
        {
            throw new NotImplementedException();
        }
        static public GameObject Create(int id)
        {
            return Create((GameObject.Types)id);
        }
        //[Obsolete] // todo: find a way to restrict creation of object to client/server class (edit: no need)
        static public GameObject Create(GameObject.Types id)//, int count = 1)
        {
            if (!GameObject.Objects.ContainsKey(id))
            {
                string.Format("object with id: {0} not found", id).ToConsole();
                //throw new Exception();
                return null;
            }
            GameObject prototype = GameObject.Objects[id];
            return prototype.Clone();
            //GameObject obj = new GameObject();

            //foreach (KeyValuePair<string, Component> comp in prototype.Components)
            //    obj.AddComponent(comp.Key, comp.Value.Clone() as Component);


            //obj.ObjectCreated();
            //return obj;
        }
        public GameObject(GameObject toCopy)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in toCopy.Components)
                this.AddComponent(comp.Key, comp.Value.Clone() as EntityComponent);

            this.ObjectCreated();
        }
        public GameObject Clone()
        {
            //var id = this.IDType;
            //if (!GameObject.Objects.ContainsKey(id))
            //{
            //    return null;
            //}
            var obj = this.Def.CreateRandom();
            if (obj == null)
            {
                obj = this.Create(); //for derived classes
                foreach (KeyValuePair<string, EntityComponent> comp in this.Components)
                    obj.AddComponent(comp.Value.Clone() as EntityComponent);
                    //obj.AddComponent(comp.Key, comp.Value.Clone() as EntityComponent);
            }
            obj.ObjectCreated();
            return obj;
        }

        public virtual GameObject Create()
        {
            return new GameObject();

        }
        public Vector2 Orientation(GameObject target)
        {
            var dir = (target.Global - this.Global);
            var dirr = new Vector2(dir.X, dir.Y);
            dirr.Normalize();
            return dirr;
        }
        //[Obsolete]
        public GameObject TrySplitOne()
        {
            throw new Exception(); // TODO sync instantiate new object
            //GameObject obj = this.StackSize == 1 ? this : GameObject.Create(this.IDType);
            //this.StackSize -= 1;
            //return obj;
        }
        public GameObject Split(int amount)
        {
            if (amount >= this.StackSize)
                throw new Exception();
            return this.SetStackSize(this.StackSize - amount).Clone().SetStackSize(amount);
        }
        public GameObject SetStackSize(int value)
        {
            this.StackSize = value;
            return this;
            //if (value <= 0)
            //{
            //    if (this.Exists)
            //        this.Net.Despawn(this);
            //    this.Dispose();
            //}
        }
        #region Messaging
        /// <summary>
        /// Posts a message to an object over the network
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dataWriter"></param>
        [Obsolete]
        public void PostMessageRemote(Message.Types type, Action<BinaryWriter> dataWriter)
        {
            Start_a_Town_.Net.Network.Serialize(writer =>
            {
                writer.Write((byte)type);
                TargetArgs.Write(writer, this);
                dataWriter(writer);
                //}).Send(Client.PacketID, PacketType.ObjectEvent, Client.Host, Start_a_Town_.Client.RemoteIP); 
            }).Send(Client.Instance.PacketID, PacketType.RemoteCall, Client.Instance.Host, Start_a_Town_.Net.Client.Instance.RemoteIP);

        }

        //internal bool IsSeedFor(ItemDef plant)
        //{
        //    if (plant == null)
        //        throw new Exception();
        //    return this.GetComponent<SeedComponent>()?.PlantDef == plant;
        //}
        internal bool IsSeedFor(PlantProperties plant)
        {
            return this.Def == ItemDefOf.Seeds && this.GetComp<SeedComponent>().Plant == plant;
                //this.PrimaryMaterial == plant.PlantMaterial;
        }
        /// <summary>
        /// Posts a message to a local object provided by the IObjectProvider
        /// </summary>
        /// <param name="net"></param>
        /// <param name="type"></param>
        /// <param name="dataWriter"></param>
        public void PostMessage(IObjectProvider net, Message.Types type, Action<BinaryWriter> dataWriter)
        {
            byte[] data;
            using (var writer = new BinaryWriter(new MemoryStream()))
            {
                dataWriter(writer);
                data = (writer.BaseStream as MemoryStream).ToArray();
            }
            this.PostMessage(type, this, net, data);
        }



        public void PostMessage(ObjectEventArgs a)
        {
            //throw new NotImplementedException();
            GameObject.PostMessage(this, a);
        }
        public void PostMessage(Message.Types msg) { GameObject.PostMessage(this, msg, null); }

        public GameObject PostMessageLocal(Message.Types msg, GameObject source, IObjectProvider net, Action<BinaryWriter> writer)
        {
            byte[] data;
            using (var w = new System.IO.BinaryWriter(new System.IO.MemoryStream()))
            {
                writer(w);
                data = (w.BaseStream as System.IO.MemoryStream).ToArray();
            }
            GameObject.PostMessage(this, msg, source, net, data);
            return this;
        }
        public GameObject PostMessage(Message.Types msg, GameObject source, IObjectProvider net, byte[] data)
        {
            GameObject.PostMessage(this, msg, source, net, data);
            return this;
        }
        //[Obsolete]
        //public GameObject PostMessage(Message.Types msg, GameObject source, params object[] parameters)
        //{
        //    GameObject.PostMessage(this, msg, source, parameters);
        //    return this;
        //}
        public void PostMessage(Message.Types msg, GameObject source, Action<GameObject> callback, params object[] parameters) { GameObject.PostMessage(this, msg, callback, source, parameters); }
        static public void PostMessage(Message msg)
        {
            //MessageQueue.Enqueue(msg);
            msg.Receiver.HandleMessage(msg.Args);
            OnMessageHandled(msg.Receiver, msg.Args);
            if (msg.Callback != null)
                msg.Callback(msg.Receiver);
        }
        static public void PostMessage(GameObject receiver, ObjectEventArgs e)
        {
            var msg = new Message(receiver, e);
            PostMessage(msg);
        }
        [Obsolete]
        static public void PostMessage(GameObject receiver, Message.Types msg, GameObject source, IObjectProvider net, byte[] data, params object[] p)
        {
            PostMessage(new Message(receiver, new ObjectEventArgs(msg, source, p) { Data = data, Network = net }));
        }
        //[Obsolete]
        //static public void PostMessage(GameObject receiver, Message.Types msg, GameObject source = null, params object[] p)
        //{
        //    MessageQueue.Enqueue(new Message(receiver, new ObjectEventArgs(msg, source, p)));
        //}
        [Obsolete]
        static public void PostMessage(GameObject receiver, Message.Types msg, Action<GameObject> callback, GameObject source = null, params object[] p)
        {
            PostMessage(new Message(receiver, new ObjectEventArgs(msg, source, p), callback));
        }

        public void HandleRemoteCall(ObjectEventArgs e)
        {
            foreach (var comp in this.Components.Values)
                comp.HandleRemoteCall(this, e);
        }
        public void HandleRemoteCall(Message.Types type)//, params object[] args)
        {
            var e = ObjectEventArgs.Create(type);//, args);
            foreach (var comp in this.Components.Values)
                comp.HandleRemoteCall(this, e);
        }
        bool HandleMessage(ObjectEventArgs e)//Message msg)
        {
            bool ok = false;
            //foreach (KeyValuePair<string, Component> comp in Components)
            foreach (var comp in this.Components.Values.ToList()) //duplicate component list in case components are changed during message handlnig
                ok |= comp.HandleMessage(this, e);
            return ok;
        }
        public void HandleRandom(RandomObjectEventArgs e)//Message msg)
        {
            foreach (var comp in this.Components.Values.ToList()) //duplicate component list in case components are changed during message handlnig
                comp.RandomEvent(this, e);
        }
        #endregion

        public List<GameObject> GetNearbyObjects(Func<float, bool> range, Func<GameObject, bool> filter = null, Action<GameObject> action = null)
        {
            return this.Map.GetNearbyObjects(this.Global, range, filter, action).Except(new GameObject[] { this }).ToList();

            //var a = action ?? ((obj) => { });
            //var f = filter ?? ((obj) => { return true; });
            //List<GameObject> nearbies = new List<GameObject>();
            //var map = this.Map;
            ////Chunk chunk = Position.GetChunk(map, this.Global);
            //Chunk chunk = map.GetChunk(this.Global);

            //List<GameObject> objects = new List<GameObject>();
            ////foreach (Chunk ch in Position.GetChunks(map, chunk.MapCoords))
            //foreach (Chunk ch in map.GetChunks(chunk.MapCoords))
            //    foreach (GameObject obj in ch.GetObjects())
            //    {
            //        if (obj == this)
            //            continue;
            //        //if ((obj.Global - parent.Global).Length() > range)
            //        if (!range(Vector3.Distance(obj.Global, this.Global)))
            //            continue;
            //        if (!f(obj))
            //            continue;
            //        a(obj);
            //        nearbies.Add(obj);
            //    }
            //return nearbies;
        }



        //public List<GameObject> GetNearbyObjectsInclusive(Func<float, bool> range, Func<GameObject, bool> filter = null, Action<GameObject> action = null)
        //{
        //    return this.Map.GetNearbyObjectsInclusive(this, range);
        //}
        //public IEnumerable<GameObject> GetNearbyObjectsNew(Func<float, bool> range, Func<GameObject, bool> filter = null, Action<GameObject> action = null)
        //{
        //    var all = this.Map.GetNearbyObjectsNew(this.Global, range, filter, action);
        //    foreach (var o in all)
        //        //if (o != this)
        //            yield return o;
        //}
        public static bool IsBlock(GameObject obj) { return obj.Type == ObjectType.Block; }
        //public bool IsBlock()
        //{
        //    return this.HasComponent<BlockComponent>();
        //    //return this.Type == ObjectType.Block; 
        //}

        public bool IsPlayerEntity()
        {
            //PlayerData pl;
            return this.IsPlayerEntity(out _);
        }
        public bool IsPlayerEntity(out PlayerData player)
        {
            //player = this.Net.GetPlayers().FirstOrDefault(p => p.ID == this.Network.PlayerID);
            //return player != null;
            player = null;
            return false;
        }

        public PositionComponent Transform;

        public GameObject()
        {
            this.Transform =
                this.AddComponent<PositionComponent>();
        }
        public GameObject(GameObject.Types id, string name, string description, string type = "")
            : this()
        {
            this["Info"] = new DefComponent(id, type, name, description);
        }

        public EntityComponent this[string componentName]
        {
            get { return this.Components[componentName]; }
            //set { Components[componentName] = value; }
            set
            {
                Factory.Register(value);
                Components[value.ComponentName] = value;
                value.MakeChildOf(this);
            }
        }

        public DefComponent GetInfo()
        {
            return GetComponent<DefComponent>("Info");
        }
        public int GetID()
        {
            return this.GetInfo().ID;
        }

        PhysicsComponent _PhysicsCached;
        PhysicsComponent GetPhysics()
        {
            //return GetComponent<PhysicsComponent>();
            if (this._PhysicsCached == null)
                this._PhysicsCached = GetComponent<PhysicsComponent>();
            return this._PhysicsCached;
        }
        public virtual PhysicsComponent Physics
        {
            get { return this.GetPhysics(); }
            set { this.AddComponent(value); }
        }
        public virtual WorkComponent Work
        {
            get { return this.GetComponent<WorkComponent>(); }
        }

        PersonalInventoryComponent _InventoryCached;
        PersonalInventoryComponent GetInventory()
        {
            //return GetComponent<PhysicsComponent>();
            if (this._InventoryCached == null)
                this._InventoryCached = GetComponent<PersonalInventoryComponent>();
            return this._InventoryCached;
        }
        public PersonalInventoryComponent Inventory { get { return this.GetInventory(); } }

        //public GuiComponent GetGui()
        //{
        //    return GetComponent<GuiComponent>("Gui");
        //}
        //public GameObject SetGui(GuiComponent guiComp = null) { this["Gui"] = guiComp != null ? guiComp : new GuiComponent(); return this; }

        public ComponentCollection Components = new();


        public T GetComponent<T>(string name) where T : EntityComponent
        {
            return this.GetComponent<T>();
        }


        public T GetComponent<T>() where T : EntityComponent
        {
            return (from comp in Components.Values
                    where comp is T
                    select comp).SingleOrDefault() as T;
        }


        public bool HasComponent<T>() where T : EntityComponent//, new()
        {
            return this.GetComponent<T>() != null;
        }
        public bool HasComponent(Type compType)
        {
            return this.Components.Values.Any(c => c.GetType() == compType);
        }
        public bool TryGetComponent(Type compType, out EntityComponent comp)
        {
            comp = this.Components.Values.FirstOrDefault(c => c.GetType() == compType);
            return comp != null;
        }
        public bool TryGetComponent<T>(string name, out T component) where T : EntityComponent
        {

            return this.TryGetComponent(out component);
        }
        public bool TryGetComponent<T>(out T component) where T : EntityComponent
        {

            component = this.GetComponent<T>();
            return component != null;

        }
        public bool TryGetComponent<T>(Action<T> action) where T : EntityComponent
        {
            T component = this.GetComponent<T>();
            if (component == null)
                return false;
            action(component);
            return true;
        }
        public TReturn TryGetComponent<TComponent, TReturn>(Func<TComponent, TReturn> func) where TComponent : EntityComponent//, new()
        {
            TComponent component = this.GetComponent<TComponent>();
            if (component == null)
                return default;
            return func(component);
        }
        public bool TryRemoveComponent<T>(string name, out T component) where T : EntityComponent//, new()
        {
            component = this.GetComponent<T>();
            return this.RemoveComponent<T>();
        }
        public bool TryRemoveComponent<T>(string name) where T : EntityComponent//, new()
        {
            return this.RemoveComponent<T>();
        }

        public bool RemoveComponent<T>() where T : EntityComponent//, new()
        {
            T component = this.GetComponent<T>();
            if (!component.IsNull())
                return this.Components.Remove(component.ComponentName);
            return false;
        }
        public bool TryRemoveComponent<T>(out T component) where T : EntityComponent//, new()
        {
            component = Components.Values.SingleOrDefault(foo =>
            {
                return foo is T;
            }) as T;
            if (!component.IsNull())
                return this.Components.Remove(component.ComponentName);
            return false;
        }

        public bool TryRemoveComponent<T>() where T : EntityComponent//, new()
        {
            return this.TryRemoveComponent<T>(out _);
        }
        [Obsolete]
        public bool AddComponent(string name, EntityComponent component)
        {
            Components[component.ComponentName] = component;
            component.MakeChildOf(this);
            return true;
        }
        public EntityComponent AddComponent(EntityComponent component)
        {
            Factory.Register(component);
            if (component == null)
                return null;
            Components[component.ComponentName] = component;
            component.Parent = this;
            component.MakeChildOf(this);
            return component;
        }
        public EntityComponent AddComponent(string componentName)
        {
            EntityComponent component = Factory.Create(componentName);
            Factory.Register(component);
            component.MakeChildOf(this);
            Components[componentName] = component;
            return component;
        }
        public T AddComponent<T>() where T : EntityComponent, new()
        {
            T component = new();
            Components[component.ComponentName] = component;
            component.MakeChildOf(this);
            Factory.Register(component);
            return component;
        }

        public virtual void Update(IObjectProvider net, Chunk chunk = null)
        {
            Components.Update(this);
        }

        public string GetStats()
        {
            string text = "";
            foreach (var comp in Components)
            {
                string stats = comp.Value.GetStats();
                if (stats.Length == 0)
                    continue;
                text += comp.Key + ":\n" + stats + "\n";
            }
            return text.TrimEnd('\n');
        }

        public override string ToString()
        {
            if (!GlobalVars.DebugMode)
                return Name;// +(this.GetInfo().StackSize > 1 ? " (x" + this.GetInfo().StackSize + ")" : "");
            string info = "";
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
            {
                if (info.Length > 0)
                    info += "\n";
                info += "*" + comp.Key + "\n" + comp.Value.ToString();
            }
            if (info.Length > 0)
                info = info.Remove(info.Length - 1);
            return info;
        }

        #region Children
        byte _ChildrenSequence = 0;
        public byte ChildrenSequence
        {
            get
            {
                return _ChildrenSequence++;
            }
            private set { _ChildrenSequence = value; }
        } //() {return _ChildrenSequence++; }//
        byte _ContainerSequence = 0;
        byte ContainerSequence
        {
            get
            {
                return _ContainerSequence++;
            }
            set { _ContainerSequence = value; }
        }
        public List<GameObjectSlot> GetChildren()
        {
            var list = new List<GameObjectSlot>();
            //foreach (var comp in this.Components.Values)
            //    comp.GetChildren(list);
            foreach (var c in this.GetContainers())
                foreach (var s in c.Slots)
                    list.Add(s);
            return list;
        }
        public List<Container> GetContainers()
        {
            var list = new List<Container>();
            foreach (var comp in this.Components.Values)
                comp.GetContainers(list);
            return list;
        }
        public Container GetContainer(int id)
        {
            return this.GetContainers().FirstOrDefault(c => c.ID == id);
        }
        public Container GetContainer(string name)
        {
            return this.GetContainers().FirstOrDefault(c => c.Name == name);
        }
        public GameObjectSlot GetChild(int containerID, int slotID)
        {
            var c = this.GetContainer(containerID);
            if (c == null)
                return null;
            return c.Slots.FirstOrDefault(s => s.ID == slotID);
        }
        public void RegisterContainer(Container container)
        {
            container.ID = this.ContainerSequence;
            container.Parent = this;
        }
        public void RegisterContainers(params Container[] containers)
        {
            foreach (var container in containers)
                this.RegisterContainer(container);
        }
        public bool TryGetChild(byte childIndex, out GameObjectSlot slot)
        {
            slot = this.GetChildren().FirstOrDefault(s => s.ID == childIndex);
            return !slot.IsNull();
        }
        public GameObjectSlot GetChild(byte childIndex)
        {
            return this.GetChild((int)childIndex);
        }
        public GameObjectSlot GetChild(int childIndex)
        {
            return this.GetChildren().FirstOrDefault(s => s.ID == childIndex);
        }
        #endregion

        public GameObject EnumerateChildren()
        {
            //if ((int)this.ID == 10000)
            //    "edw eimaste".ToConsole();
            this.ChildrenSequence = 0;
            var list = new List<GameObjectSlot>();
            foreach (var comp in this.Components.Values)
                comp.GetChildren(list);
            foreach (var child in list)
            {
                child.ID = this.ChildrenSequence;
            }
            this.ChildrenSequence = 0;
            return this;
        }
        public void GetInterface(WindowTargetInterface ui)
        {
            ui.Title = this.Name;
            foreach (var comp in this.Components)
                comp.Value.GetInterface(this, ui.PanelInfo);
        }
        public void GetManagementInterface(WindowTargetManagement ui)
        {
            ui.Title = this.Name;
            foreach (var comp in this.Components)
                comp.Value.GetManagementInterface(this, ui.PanelInfo);
            ui.PanelActions.AddControls(
                new Button("Center Cam") { LeftClickAction = () => { ScreenManager.CurrentScreen.Camera.CenterOn(this.Global); } }
                );
        }
        public void GetManagementInterface(WindowTargetManagementStatic ui)
        {
            ui.Title = this.Name;
            foreach (var comp in this.Components)
                comp.Value.GetManagementInterface(this, ui.PanelInfo);
            ui.PanelActions.AddControls(
                new Button("Center Cam") { LeftClickAction = () => { ScreenManager.CurrentScreen.Camera.CenterOn(this.Global); } }
                );
        }
        public Window GetInterface()
        {
            if (GameObjectWindows.TryGetValue(this, out Window window))
                return window;
            window = new();
            window.AutoSize = true;
            window.Title = this.Name;
            window.Movable = true;
            //this.GetInterface(window.Client);
            foreach (var comp in this.Components)
                comp.Value.GetInterface(this, window.Client);
            GameObjectWindows.Add(this, window);
            window.HideAction = () => GameObjectWindows.Remove(this);
            return window;
        }

        static public Dictionary<GameObject, Window> GameObjectWindows = new();
        public Window GetUi()
        {
            throw new Exception();

        }

        public void GetUI(UI.Control ui, List<EventHandler<GameEvent>> gameEventHandlers)
        {
            var panel_tabs = new Panel() { AutoSize = true };
            var panel_ui = new GroupBox();
            int rd_y = 0;
            var boxes = new List<GroupBox>();
            foreach (var c in this.Components)
            {
                var boxComp = new GroupBox();
                c.Value.GetUI(this, boxComp, gameEventHandlers);//uiUpdaters);
                boxes.Add(boxComp);
                if (boxComp.Controls.Count == 0)
                    continue;
                var rd = new RadioButton(c.Key, new Vector2(0, rd_y))
                {
                    Tag = boxComp,
                    LeftClickAction = () => { panel_ui.Controls.Clear(); panel_ui.Controls.Add(boxComp); }
                };
                panel_tabs.Controls.Add(rd);
                rd_y += UI.Label.DefaultHeight;
            }
            panel_ui.Conform(boxes.ToArray());
            if (panel_tabs.Controls.Count > 1)
            {
                panel_ui.Location = panel_tabs.TopRight;
                ui.Controls.Add(panel_tabs);
            }
            else if (panel_tabs.Controls.Count == 1)
                panel_ui.Controls.Add(panel_tabs.Controls.First().Tag as Control);
            ui.Controls.Add(panel_ui);

            if (panel_tabs.Controls.Count > 0)
                (panel_tabs.Controls.First() as RadioButton).PerformLeftClick();
        }
        public UI.Control GetTooltip()//Message msg)
        {
            var box = new GroupBox();
            GetInfo().OnTooltipCreated(this, box);
            // TODO: LOL fix, i need the object name to be on top
            foreach (KeyValuePair<string, EntityComponent> comp in Components.Except(new KeyValuePair<string, EntityComponent>[] { new KeyValuePair<string, EntityComponent>("Info", GetInfo()) }))
                comp.Value.OnTooltipCreated(this, box);
            var value = this.GetValue();
            if (value > 0)
                box.AddControlsBottomLeft(new Label(string.Format("Value: {0} ({1})", value * this.StackSize, value)));
            //box.AddControlsBottomLeft(new Label(string.Format("Value: {0} ({1} x {2})", value * this.StackSize, value, this.StackSize)));

            box.AddControlsBottomLeft(new Label(string.Format("InstanceID: {0}", this.RefID)));

            box.MouseThrough = true;
            return box;
        }
        public void GetTooltip(UI.Control tooltip)//Message msg)
        {
            GetInfo().OnTooltipCreated(this, tooltip);
            // TODO: LOL fix, i need the object name to be on top
            foreach (var comp in Components.Except(new KeyValuePair<string, EntityComponent>[] { new KeyValuePair<string, EntityComponent>("Info", GetInfo()) }))
                comp.Value.OnTooltipCreated(this, tooltip);
            var value = this.GetValue();
            if (value > 0)
                //tooltip.AddControlsBottomLeft(new Label(string.Format("Value: {0} ({1} x {2})", value * this.StackSize, value, this.StackSize)));
                tooltip.AddControlsBottomLeft(new Label(string.Format("Value: {0} ({1})", value * this.StackSize, value)));
            var stats = this.Def.Category?.Stats;
            if (stats is not null)
                foreach (var stat in stats)
                    tooltip.AddControlsBottomLeft(new Label($"{stat.Label}: {stat.GetValue(this).ToString(stat.StringFormat)}"));
            tooltip.AddControlsBottomLeft(new Label(string.Format("InstanceID: {0}", this.RefID)));
        }
        public void GetInventoryTooltip(UI.Control tooltip)
        {
            GetInfo().OnTooltipCreated(this, tooltip);
            // TODO: LOL fix, i need the object name to be on top
            foreach (KeyValuePair<string, EntityComponent> comp in Components.Except(new KeyValuePair<string, EntityComponent>[] { new KeyValuePair<string, EntityComponent>("Info", GetInfo()) }))
                comp.Value.GetInventoryTooltip(this, tooltip);
            var value = this.GetValue();
            if (value > 0)
                tooltip.AddControlsBottomLeft(new Label(string.Format("Value: {0} ({1})", value * this.StackSize, value)));
            //tooltip.AddControlsBottomLeft(new Label(string.Format("Value: {0} ({1} x {2})", value * this.StackSize, value, this.StackSize))); 
            tooltip.AddControlsBottomLeft(new Label(string.Format("InstanceID: {0}", this.RefID)));

        }
        public virtual void GetTooltip(GameObject actor, UI.Tooltip tooltip)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.GetActorTooltip(this, actor, tooltip);
        }

        //public void AIQuery(GameObject ai, List<Components.AIAction> actions)
        //{
        //    foreach (var comp in this.Components.Values)
        //        comp.AIQuery(this, ai, actions);
        //}
        //public List<InteractionOld> Query(GameObject actor, params object[] parameters)
        //{
        //    List<InteractionOld> actions = new List<InteractionOld>();
        //    List<object> p = new List<object>() { actions };
        //    p.AddRange(parameters);
        //    foreach (EntityComponent c in Components.Values)
        //        //c.HandleMessage(this, new GameObjectEventArgs(Message.Types.Query, actor, p.ToArray()));
        //        c.Query(this, actions);//, new GameObjectEventArgs(Message.Types.Default, actor, p.ToArray()));
        //    return actions;
        //}
        //public bool Query(GameObject actor, Action<List<InteractionOld>> callback)
        //{
        //    List<InteractionOld> actions = new List<InteractionOld>();
        //    foreach (EntityComponent c in Components.Values)
        //        //c.HandleMessage(this, new GameObjectEventArgs(Message.Types.Query, actor, actions));
        //        c.Query(this, actions);//new GameObjectEventArgs(Message.Types.Default, actor, actions));
        //    callback(actions);
        //    return actions.Count > 0;
        //}
        //public bool Query(GameObject actor, List<InteractionOld> list)
        //{
        //    foreach (EntityComponent c in Components.Values)
        //        //c.HandleMessage(this, new GameObjectEventArgs(Message.Types.Query, actor, list));
        //        c.Query(this, list);//new GameObjectEventArgs(Message.Types.Default, actor, list));
        //    return list.Count > 0;
        //}


        //public void Spawn(Map map)
        //{
        //    this.SetMap(map);
        //    this.Spawn();
        //}
        public void Despawn()
        {
            if (!this.IsSpawned)
                return;
            //this.Net.Despawn(this);
            foreach (var comp in this.Components.Values.ToList())
                comp.OnDespawn(this);
            this.Map.EventOccured(Message.Types.EntityDespawned, this);
            //this.Unreserve(); // UNDONE dont unreserve here because the ai might continue manipulating (placing/carrying) the item during the same behavior

        }

        //public void Despawn(IObjectProvider net)
        //{
        //    this.Despawn();
        //    return;
        //    //foreach (var comp in this.Components.Values.ToList()) //duplicate component list in case components are changed during message handlnig
        //    //    comp.HandleMessage(this, new ObjectEventArgs(Message.Types.Removed) { Network = net });


        //    //if (this.Exists)
        //        //this.PostMessage(new ObjectEventArgs(Message.Types.Despawn) { Network = net });
        //        foreach (var comp in this.Components.Values.ToList())
        //            comp.Despawn(net, this);
        //}

        public virtual void Spawn(IObjectProvider net)
        {
            this.NetNew = net;
            this.Parent = null;
            foreach (var comp in this.Components.Values)
                comp.OnSpawn(net, this);
            this.Map.EventOccured(Message.Types.EntitySpawned, this);
        }
        public void Spawn(IMap map, Vector3 global)
        {
            var net = map.Net;
            this.NetNew = net;
            this.Global = global;
            this.Map = map;
            this.Spawn(net);
            //map.Net.Spawn(this, global);
        }
        public void SyncSpawn(IMap map, Vector3 global)
        {
            if (map.Net is not Server)
                return;
            map.SyncSpawn(this, global, Vector3.Zero);
        }
        public void ChunkLoaded(IObjectProvider net)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.ChunkLoaded(net, this);
            //UI.Nameplate.Create(this);
        }

        //public Nameplate NamePlate;
        public void Focus()
        {
            //Nameplate.Show(this);
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.Focus(this);
        }
        public void FocusLost()
        {
            //Nameplate.Hide(this);
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.FocusLost(this);
        }
        public DialogueOptionCollection GetDialogueOptions(GameObject speaker)
        {
            var options = new DialogueOptionCollection();
            foreach (var comp in Components)
                comp.Value.GetDialogueOptions(this, speaker, options);
            return options;
        }
        /// <summary>
        /// no need for every entity to have dialogue options, move this to the appropriate AI behavior
        /// </summary>
        /// <param name="speaker"></param>
        /// <returns></returns>
        public List<DialogOption> GetDialogOptions(GameObject speaker)
        {
            var options = new List<DialogOption>();
            foreach (var comp in Components)
                comp.Value.GetDialogOptions(this, speaker, options);
            return options;
        }
        public virtual void Draw(SpriteBatch sb, DrawObjectArgs e)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.Draw(sb, e);
        }
        public virtual void Draw(MySpriteBatch sb, Camera camera)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.Draw(sb, this, camera);
        }
        public virtual void Draw(MySpriteBatch sb, DrawObjectArgs e)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.Draw(sb, e);
        }
        internal void DrawMouseover(SpriteBatch sb, Camera camera)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.DrawMouseover(sb, camera, this);
        }
        internal void DrawMouseover(MySpriteBatch sb, Camera camera)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.DrawMouseover(sb, camera, this);
        }
        internal void DrawInterface(SpriteBatch sb, Camera camera)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.DrawUI(sb, camera, this);
        }

        internal void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.DrawPreview(sb, camera, global);
        }
        internal void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, float depth)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.DrawPreview(sb, camera, global, depth);
        }
        internal void DrawFootprint(SpriteBatch sb, Camera camera, Vector3 global)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.DrawFootprint(sb, camera, global);
        }
        public virtual void GetTooltipInfo(Tooltip tooltip)//List<GroupBox> TooltipGroups
        {
            GetTooltip(tooltip);
        }
        public void GetTooltipBasic(Tooltip tooltip)
        {
            GameObject obj = this;// ctrl.Tag as GameObject;
            var quality = DefComponent.GetQualityColor(obj);


            var pic = new PictureBox(new Vector2(32), r => this.Body.RenderNewer(this, r));// sprite.ToPictureBox();


            var name = new Label(pic.TopRight, obj.Name, quality, Color.Black, UIManager.FontBold);// { Location = pic.TopRight };// obj.Name.ToLabel(pic.TopRight);
            var desc = obj.Description.ToLabel(name.BottomLeft);
            tooltip.AddControls(pic, name, desc);
            tooltip.Color = quality;

        }
        public void DrawPreview(MySpriteBatch sb, Camera cam, TargetArgs target, bool precise)
        {
            if (target.Type != TargetType.Position)
                return;
            var blockHeight = Block.GetBlockHeight(target.Map, target.Global);
            var global = target.Global + target.Face * new Vector3(1, 1, blockHeight) + (precise ? target.Precise : Vector3.Zero);
            this.DrawPreview(sb, cam, global);
        }
        public void DrawPreview(MySpriteBatch sb, Camera cam, Vector3 global)
        {
            var body = this.Body;
            var pos = cam.GetScreenPositionFloat(global);
            pos += body.OriginGroundOffset * cam.Zoom;
            //body.DrawTree(this.Entity, sb, pos, Color.White, Color.White, Color.White, Color.Transparent, body.RestingFrame.Offset, 0, cam.Zoom, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
            // TODO: fix difference between tint and material in this drawtree method
            var tint = Color.White * .5f;// Color.Transparent;
            body.DrawGhost(this, sb, pos, Color.White, Color.White, tint, Color.Transparent, 0, cam.Zoom, 0, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
        }
        Sprite CachedSprite;
        internal StorageFilter StorageCategory;

        public Sprite GetSprite()
        {
            if (this.CachedSprite == null)
            {
                if (!TryGetComponent(out SpriteComponent sprComp))
                    return null;
                this.CachedSprite = sprComp.Sprite;
            }
            return this.CachedSprite;
        }
        public Sprite GetSpriteOrDefault()
        {
            if (!TryGetComponent("Sprite", out SpriteComponent sprComp))
                return Sprite.Default;
            return sprComp.Sprite ?? Sprite.Default;
        }
        public Icon GetIcon()
        {
            return new Icon(GetSpriteOrDefault());
        }

        public GameObject Clone(bool initialize = true)
        {
            var obj = new GameObject();
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
            {
                //  obj.AddComponent(comp.Key, comp.Value.Clone() as Component);
                obj[comp.Key] = comp.Value.Clone() as EntityComponent;
            }
            //   obj.Initialize();

            if (initialize)
                obj.Initialize();
            return obj;
        }

        public GameObject New()
        {
            return GameObject.Create(IDType);
        }


        public byte[] GetSnapshotData()
        {
            using var w = new BinaryWriter(new MemoryStream());
            this.Write(w);
            return (w.BaseStream as MemoryStream).ToArray();
        }


        public void Write(BinaryWriter w)
        {
            //writer.Write((int)IDType);
            w.Write(this.ID);
            w.Write(this.Def?.Name ?? "");
            w.Write(this.RefID);
            w.Write(this.Components.Count);
            foreach (var comp in this.Components)
            {
                //if(Factory.Create(comp.Key).IsNull())
                //    throw new Exception();
                w.Write(comp.Key);
                comp.Value.Write(w);
            }
        }
        /// <summary>
        /// Updates the object to the values provided by the reader
        /// </summary>
        /// <param name="r"></param>
        public virtual void Read(BinaryReader r)
        {
            this.RefID = r.ReadInt32();
            r.ReadByte(); //skip the first byte (object type)
            int compCount = r.ReadInt32();
            for (int i = 0; i < compCount; i++)
            {
                string compName = r.ReadString();
                this[compName].Read(r);
            }
            //  this.ObjectCreated();
        }
        public static GameObject CreateCustomObject(BinaryReader reader)
        {
            GameObject.Types type = (GameObject.Types)reader.ReadByte();
            var obj = new GameObject(); // WARNING: must figure out way to reconstruct an object without it's creating a prefab
            int compCount = reader.ReadInt32();
            for (int i = 0; i < compCount; i++)
            {
                string compName = reader.ReadString();
                //if (obj.Components.ContainsKey(compName))
                //    obj[compName].Read(reader);

                EntityComponent comp = Factory.Create(compName);
                if (comp.IsNull())
                    continue;
                obj.AddComponent(comp).Read(reader);

            }
            //obj.ObjectCreated(); // i'll put that only where i need it after  calling reconstruct
            return obj;
        }
        public static GameObject CreatePrefab(BinaryReader r)
        {
            int type = r.ReadInt32();
            string defName = r.ReadString();
            var def = Start_a_Town_.Def.GetDef<ItemDef>(defName);
            var refid = r.ReadInt32();
            GameObject obj;
            if (def != null)
            {
                //obj =  ItemFactory.CreateItem(def);
                obj = def.Create();// ItemFactory.CreateItem(def);

            }
            else obj = Create(type); // WARNING: must figure out way to reconstruct an object without it's creating a prefab (update: NOT REALLY)
            //obj.ObjectCreated();
            int compCount = r.ReadInt32();
            for (int i = 0; i < compCount; i++)
            {
                string compName = r.ReadString();
                //compName.ToConsole();
                if (!obj.Components.ContainsKey(compName))
                    obj.AddComponent(Factory.Create(compName));
                //obj[compName].MakeChildOf(obj);
                obj[compName].Read(r);
            }
            //obj.ObjectCreated(); // i'll put that only where i need it after  calling reconstruct
            obj.ObjectSynced();
            obj.RefID = refid;

            return obj;

        }
        public static GameObject CloneTemplate(int templateID, BinaryReader reader)
        {
            //int templateID = reader.ReadInt32();
            var _id = reader.ReadInt32(); // because the unnecessary ID field has been written
            GameObject obj = CloneTemplate(templateID); // WARNING: must figure out way to reconstruct an object without it's creating a prefab
            var defname = reader.ReadString();
            obj.RefID = reader.ReadInt32();
            int compCount = reader.ReadInt32();
            for (int i = 0; i < compCount; i++)
            {
                string compName = reader.ReadString();
                //compName.ToConsole();
                if (!obj.Components.ContainsKey(compName))
                    obj.AddComponent(Factory.Create(compName));
                //obj[compName].MakeChildOf(obj);
                obj[compName].Read(reader);
            }
            //obj.ObjectCreated(); // i'll put that only where i need it after  calling reconstruct
            obj.ObjectSynced();
            return obj;
        }
        public GameObject ObjectLoaded()
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.OnObjectLoaded(this);
            return this;
        }
        /// <summary>
        /// try to make this private
        /// </summary>
        /// <returns></returns>
        public GameObject ObjectCreated()
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.OnObjectCreated(this);
            //this.EnumerateChildren();
            return this;
        }
        public GameObject ObjectSynced()
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.OnObjectSynced(this);
            this.EnumerateChildren();
            return this;
        }
        public GameObject Initialize(RandomThreaded random)
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.Initialize(this, random);
            return this;
        }
        public GameObject Initialize()
        {
            foreach (KeyValuePair<string, EntityComponent> comp in Components)
                comp.Value.Initialize(this);
            return this;
        }

        internal List<SaveTag> SaveInternal()
        {
            var data = new List<SaveTag>
            {
                new SaveTag(SaveTag.Types.Int, "TypeID", (int)IDType)
            };
            if (this.Def != null)
                data.Add(this.Def.Name.Save("Def"));
            //Tag compData = new Tag(Tag.Types.List, "Components", Tag.Types.Compound);
            data.Add(this.RefID.Save("InstanceID"));
            var compData = new SaveTag(SaveTag.Types.Compound, "Components");
            foreach (KeyValuePair<string, EntityComponent> comp in this.Components)
            {
                //List<SaveTag> compSave = comp.Value.Save();
                //if (compSave != null)
                //    compData.Add(new SaveTag(SaveTag.Types.Compound, comp.Key, compSave));
                var compSave = comp.Value.SaveAs(comp.Key);
                if (compSave != null)
                    compData.Add(compSave);
            }
            data.Add(compData);
            return data;
        }

        /// <summary>
        /// Creates an object from a savetag node.
        /// </summary>
        /// <param name="tag">A tag with a list of tags as its value.</param>
        /// <returns></returns>
        internal static GameObject Load(SaveTag tag)//, Action<GameObject> objectFactory) //IObjectProvider net, 
        {
            var val = tag["TypeID"].Value;
            tag.TryGetTagValue("Def", out string defName);
            var def = Start_a_Town_.Def.GetDef<ItemDef>(defName);
            GameObject obj;
            if (def != null)
            {
                //obj = ItemFactory.CreateItem(def);
                obj = def.Create();
            }
            else
            {
                Log.WriteToFile($"Error loading entity. Def \"{defName}\" does not exist.");
                //throw new Exception();
                return null;
                GameObject.Types type = (GameObject.Types)val;
                //GameObject obj = GameObject.Create(type);
                obj = GameObject.Create(type);
                if (obj == null)
                {
                    Console.WriteLine("tried to load gameobject with missing type: " + type.ToString());
                    //throw new Exception();
                    return null;
                }
            }
            //obj.ObjectCreated();
            tag.TryGetTagValue("InstanceID", out obj.RefID);
            //List<SaveTag> compData = tag["Components"].Value as List<SaveTag>;
            Dictionary<string, SaveTag> compData = tag["Components"].Value as Dictionary<string, SaveTag>;
            foreach (SaveTag compTag in compData.Values)
            {
                if (compTag.Value == null)
                    continue;
                //if (!obj.Components.ContainsKey(compTag.Name))
                //{
                //    if(obj.AddComponent(Factory.Create(compTag.Name))!=null)
                //        obj[compTag.Name].Load(compTag);//, objectFactory ?? new Action<GameObject>(o => { }));//.Value as List<Tag>);
                //}

                // DONT CREATE COMPONENTS THAT DONT EXIST ON THE TEMPLATE OBJECT
                //if (!obj.Components.ContainsKey(compTag.Name))
                //    obj.AddComponent(Factory.Create(compTag.Name));
                if (obj.Components.ContainsKey(compTag.Name))
                    obj[compTag.Name].Load(obj, compTag);//, objectFactory ?? new Action<GameObject>(o => { }));//.Value as List<Tag>);
            }
            //obj.ObjectCreated(); // UNCOMMENT IF PROBLEMS
            obj.ObjectLoaded();
            return obj;
        }

        public IEnumerable<ContextAction> GetInventoryContextActions(GameObject actor)
        {
            //foreach (var comp in this.Components)
            //    foreach (var action in comp.Value.GetInventoryContextActions(actor))
            //        yield return action;
            if (this.Def.GearType != null)
                yield return new ContextAction(() => "Equip", () => PacketInventoryEquip.Send(this.Net, actor.RefID, this.RefID));// actor.Interact(new Equip(), this));// () => "test equip context".ToConsole());
            yield return new ContextAction(() => "Drop", () => PacketInventoryDrop.Send(this.Net, actor.RefID, this.RefID, this.StackSize));
        }
        public List<ContextAction> GetInventoryActions(GameObject actor, GameObjectSlot slot)
        {
            List<ContextAction> actions = new List<ContextAction>();
            foreach (var item in this.Components)
                item.Value.GetInventoryActions(actor, slot, actions);
            return actions;
        }
        public void GetInventoryContext(ContextArgs a, int slotID)
        {
            if (PlayerOld.Actor.IsNull())
                return;

            a.Actions.Add(new ContextAction(() => "Drop", () =>
            {
                GameObjectSlot slot = PlayerOld.Actor.GetChild((byte)slotID);
                if (slot.StackSize == 1)
                {
                    Client.PostPlayerInput(Message.Types.DropInventoryItem, w =>
                    {
                        w.Write(slotID);
                        w.Write(1);
                    });
                    return;// true; true;
                }
                SplitStackWindow.Instance.Show(slot, PlayerOld.Actor, (amount) =>
                {
                    Client.PostPlayerInput(Message.Types.DropInventoryItem, w =>
                    {
                        w.Write(slotID);
                        w.Write(amount);
                    });
                });
                //return true;
                //Client.PostPlayerInput(Message.Types.ExecuteScript, w => Script.Write(w, Script.Types.Drop, TargetArgs.Empty));
                Client.PostPlayerInput(Message.Types.DropInventoryItem, w => w.Write(slotID));// Script.Write(w, Script.Types.Drop, TargetArgs.Empty));
                //return true;
            }));

            this.Components.Values.ToList().ForEach(c => c.GetInventoryContext(PlayerOld.Actor, a.Actions, slotID));
            a.Actions.Add(new ContextAction(() => "Inspect", () => this.GetTooltip().ToWindow().Show()));
        }


        public List<Interaction> GetInteractionsFromSkill(ToolAbilityDef skill)
        {
            var list = new List<Interaction>();
            foreach (var item in this.Components)
                item.Value.GetInteractionsFromSkill(this, skill, list);
            return list;
        }

        public Dictionary<PlayerInput, Interaction> GetPlayerActionsWorld()
        {
            var list = new Dictionary<PlayerInput, Interaction>();
            foreach (var item in this.Components)
                item.Value.GetPlayerActionsWorld(this, list);
            return list;
        }

        public List<ContextAction> GetRightClickActions()
        {
            var list = new List<ContextAction>();
            foreach (var item in this.Components)
                item.Value.GetRightClickActions(this, list);
            return list;
        }
        public List<Interaction> GetEquippedActionsWithTarget(GameObject actor, TargetArgs t)
        {
            var list = new List<Interaction>();
            foreach (var item in this.Components)
                item.Value.GetEquippedActionsWithTarget(this, actor, t, list);
            return list;
        }
        public List<Interaction> GetEquippedActions()
        {
            var list = new List<Interaction>();
            foreach (var item in this.Components)
                item.Value.GetEquippedActions(this, list);
            return list;
        }
        public List<Interaction> GetHauledActions(TargetArgs a)
        {
            var list = new List<Interaction>();
            foreach (var item in this.Components)
                item.Value.GetHauledActions(this, a, list);
            return list;
        }
        //public override void GetClientActions(GameObject parent, List<ContextAction> actions)
        //{
        //    var list = new Dictionary<PlayerInput, Interactions.Interaction>();
        //    this.GetPlayerActionsWorld(parent, list);
        //    foreach (var i in list)
        //        actions.Add(new ContextAction(i.Key.ToString() + ": " + i.Value.Name, () => true));
        //}
        internal ContextAction GetContextRB(GameObject player)
        {
            var list = new List<ContextAction>();
            foreach (var c in this.Components)
            {
                var a = c.Value.GetContextRB(this, player);
                if (a != null)
                    list.Add(a);
            }
            return list.FirstOrDefault();
        }

        internal ContextAction GetContextActivate(GameObject player)
        {
            var list = new List<ContextAction>();
            foreach (var c in this.Components)
            {
                var a = c.Value.GetContextActivate(this, player);
                if (a != null)
                    list.Add(a);
            }
            return list.FirstOrDefault();
        }
        public void GetContextActions(ContextArgs a)
        {
            if (PlayerOld.Actor == null)
                return;
            //Vector3 face = (Vector3)a.Parameters[0];
            //this.Components.Values.ToList().ForEach(c => c.GetClientActions(this, a.Actions));
            foreach (var c in this.Components.Values)
            {
                c.GetClientActions(this, a.Actions);
                //var list = new Dictionary<PlayerInput, Interaction>();
                //c.GetPlayerActionsWorld(this, list);
                //foreach (var i in list)
                //    a.Actions.Add(new ContextAction(i.Key.ToString() + ": " + i.Value.Name, () => true));
            }

        }

        public List<Interaction> GetInteractionsList()
        {
            var list = new List<Interaction>();
            foreach (var item in this.Components)
                item.Value.GetInteractions(this, list);
            return list;
        }
        public Dictionary<string, Interaction> GetInteractions()
        {
            var list = new Dictionary<string, Interaction>();
            foreach (var item in this.GetInteractionsList())
                list.Add(item.Name, item);
            return list;
        }






        public List<Interaction> GetAvailableTasks()
        {
            List<Interaction> list = new List<Interaction>();
            foreach (var c in this.Components.Values)
                c.GetAvailableTasks(this, list);
            return list;
        }

        public void OnHitTestPass(Vector3 face, float depth)
        {
            //Controller.Instance.MouseoverNext.Object = this;
            //Controller.Instance.MouseoverNext.Face = face;
            foreach (var comp in Components.Values)
                comp.OnHitTestPass(this, face, depth);
        }

        public bool IsDisposed
        {
            get
            {
                return this.Net.GetNetworkObject(this.RefID) == null;
            }
        }
        //public bool IsDisposed { get; set; }
        public bool Dispose()
        {
            //if(this.IsDisposed)
            //{
            //    string.Format("tried to dispose already disposed object " + this.Name).ToConsole();
            //    return false;
            //}
            foreach (var comp in this.Components.Values.ToList())
                comp.OnDispose(this);
            this.Net.EventOccured(Message.Types.ObjectDisposed, this);
            //this.IsDisposed = true;
            return this.Net.DisposeObject(this);
        }

        public GameObject Instantiate(Action<GameObject> instantiator)
        {
            instantiator(this);
            var children = this.GetChildren();
            (from slot in children
             where slot.HasValue
             select slot.Object).ToList()
             //.ForEach(c => instantiator(c));
             .ForEach(c => c.Instantiate(instantiator));

            foreach (var comp in this.Components.Values)
                comp.Instantiate(this, instantiator);

            return this;
        }

        internal void UpdateState(BinaryReader r)
        {
            throw new NotImplementedException();
        }
        internal void WriteState(BinaryWriter w)
        {
            throw new NotImplementedException();
        }

        internal void RemoteProcedureCall(Components.Message.Types type, BinaryReader r)
        {
            foreach (var comp in this.Components)
                comp.Value.HandleRemoteCall(this, type, r);
        }

        internal object GetPosition()
        {
            return new Location(this.Map, this.Global);
        }
        internal Location[] GetBottomCorners()
        {
            var global = this.Global;
            var h = this.Physics.Height;
            var map = this.Map;
            var size = .25f;
            var box = new BoundingBox(global - new Vector3(size, size, 0), global + new Vector3(size, size, h));
            var corners = new Location[] {
                    new Location(map, box.Min),
                    new Location(map, new Vector3(box.Min.X, box.Max.Y, global.Z)),
                    new Location(map, new Vector3(box.Max.X, box.Min.Y, global.Z)),
                    new Location(map, new Vector3(box.Max.X, box.Max.Y, global.Z))
                };
            return corners;
        }
        public BoundingBox GetBox()
        {
            var box = new BoundingBox(this.Global - new Vector3(.25f, .25f, 0), this.Global + new Vector3(.25f, .25f, this.Physics.Height));
            return box;
        }
        public bool IsInInteractionRange(TargetArgs target)
        {
            if (target.Type == TargetType.Position)
            {
                var actorCoords = this.Global;//.Round();
                //var actorBox = new BoundingBox(actorCoords - new Vector3(1, 1, 0), actorCoords + new Vector3(1, 1, this.Physics.Height));
                var actorBox = new BoundingBox(actorCoords - new Vector3(1, 1, 1), actorCoords + new Vector3(1, 1, this.Physics.Height + 2));

                //var targetBox = new BoundingBox(target.Global - Vector3.One, target.Global + Vector3.One);
                //var targetBox = new BoundingBox(target.Global - new Vector3(.5f, .5f, 0), target.Global + new Vector3(.5f, .5f, 1));
                var targetBox = new BoundingBox(target.Global - new Vector3(.5f, .5f, .5f), target.Global + new Vector3(1.5f, 1.5f, 1.5f));

                var result = actorBox.Intersects(targetBox);
                if (!result)
                {
                    throw new Exception();
                }
                return result;
            }
            else if (target.Type == TargetType.Entity)
            {
                var cylinderTarget = new BoundingCylinder(target.Global, .5f, target.Object.Physics.Height);
                var cylinderActor = new BoundingCylinder(this.Global - Vector3.UnitZ, RangeCheck.DefaultRange, this.Physics.Height + 2);
                //var result = cylinderTarget.Contains(this.Global);
                var result = cylinderActor.Intersects(cylinderTarget);
                if (!result)
                {
                    throw new Exception();
                }
                return result;
            }
            throw new Exception();
        }
        public bool IsAtBlock(Vector3 global)
        {
            var blockGlobal = global.SnapToBlock();
            var targetBox = new BoundingBox(blockGlobal - new Vector3(.5f, .5f, 0), blockGlobal + new Vector3(.5f, .5f, 1));
            var result = targetBox.Contains(this.Global);
            return result != ContainmentType.Disjoint;
        }

        internal void MapLoaded(IMap map)
        {
            this.Map = map;
            foreach (var comp in this.Components)
                comp.Value.MapLoaded(this);
        }

        public bool Reserve(AITask task, TargetArgs target, int stackcount)
        {
            return this.Map.Town.ReservationManager.Reserve(this, task, target, stackcount);
        }
        public bool Reserve(TargetArgs target, int stackcount = -1)
        {
            return this.Map.Town.ReservationManager.Reserve(this, target, stackcount);
        }
        //public bool Reserve(GameObject target, int stackcount)
        //{
        //    return this.Map.Town.ReservationManager.Reserve(this, new TargetArgs(target), stackcount);
        //}
        //public bool Reserve(GameObject target, int stackcount)
        //{
        //    return this.Map.Town.ReservationManager.Reserve(this, target, stackcount);
        //}
        public bool Reserve(Vector3 target)
        {
            return this.Map.Town.ReservationManager.Reserve(this, new TargetArgs(this.Map, target), 1);
        }
        public bool Reserve(TargetIndex targetIndex, int count)
        {
            var task = this.CurrentTask;
            return this.Map.Town.ReservationManager.Reserve(this, task, task.GetTarget(targetIndex), count);
        }

        //public bool CanReserve(TargetArgs target, int stackcount = -1)
        //{
        //    return this.Map.Town.ReservationManager.CanReserve(this, target, stackcount);
        //}
        //public bool CanReserve(GameObject target, int stackcount = -1)
        //{
        //    return this.Map.Town.ReservationManager.CanReserve(this, new TargetArgs(target), stackcount);
        //}
        public bool CanReserve(Vector3 target, int stackcount = -1, bool force = false)
        {
            return this.Map.Town.ReservationManager.CanReserve(this, new TargetArgs(this.Map, target), stackcount, force);
        }
        public bool CanReserve(TargetArgs target, int stackcount = -1, bool force = false)
        {
            return this.Map.Town.ReservationManager.CanReserve(this, target, stackcount, force);
        }
        public bool CanReserve(GameObject target, int stackcount = -1, bool force = false)
        {
            return this.Map.Town.ReservationManager.CanReserve(this, new TargetArgs(target), stackcount, force);
        }
        public bool CanReserve(Entity target, int stackcount = -1, bool force = false)
        {
            return this.Map.Town.ReservationManager.CanReserve(this, new TargetArgs(target), stackcount, force);
        }
        public void Unreserve()
        {
            this.Map.Town.ReservationManager.Unreserve(this);
        }
        public void Unreserve(GameObject obj)
        {
            this.Map.Town.ReservationManager.Unreserve(this, new TargetArgs(obj));
        }
        public void Unreserve(TargetArgs target)
        {
            this.Map.Town.ReservationManager.Unreserve(this, target);
        }
        //internal int GetUnreservedAmount(GameObject i)
        //{
        //    return this.Map.Town.ReservationManager.GetUnreservedAmount(this, new TargetArgs(i));
        //}
        //internal int GetUnreservedAmount(TargetArgs target)
        //{
        //    return this.Map.Town.ReservationManager.GetUnreservedAmount(this, target);
        //}
        internal int GetUnreservedAmount(TargetArgs i)
        {
            return this.Map.Town.ReservationManager.GetUnreservedAmount(i);
        }
        //internal int GetUnreservedAmount(GameObject i)
        //{
        //    return this.Map.Town.ReservationManager.GetUnreservedAmount(i);
        //}
        internal bool TryGetUnreservedAmount(GameObject i, out int amount)
        {
            amount = this.Map.Town.ReservationManager.GetUnreservedAmount(new TargetArgs(i));
            return amount > 0;
        }
        internal int GetUnreservedAmount(Vector3 i)
        {
            return this.Map.Town.ReservationManager.GetUnreservedAmount(new TargetArgs(this.Map, i));
        }

        internal void HitTest(Camera camera)
        {
            this.GetComponent<SpriteComponent>().HitTest(this, camera);
        }


        internal bool HasMatchingBody(GameObject otherItem)
        {
            //var thismats = this.Sprite.Materials; //this.GetComponent<SpriteComponent>().Materials;
            //var othermats = otherItem.Sprite.Materials; //otherItem.GetComponent<SpriteComponent>().Materials;

            return this.GetComponent<SpriteComponent>().HasMatchingBody(otherItem);
            //var thismats = this.GetComponent<SpriteComponent>().Materials;
            //var othermats = otherItem.GetComponent<SpriteComponent>().Materials;
            //foreach (var thismat in thismats)
            //{
            //    if (!othermats.TryGetValue(thismat.Key, out var othermat))
            //        return false;
            //    if (thismat.Value != othermat)
            //        return false;
            //}
            //return true;
        }
        //internal bool CanStackWith(GameObject otherItem)
        //{
        //    return this.CanStackWith(otherItem, otherItem.StackSize);
        //    //if (this == otherItem)
        //    //    throw new Exception();
        //    //if (this.IDType != otherItem.IDType)
        //    //    return false;
        //    //var correctStackSize = this.StackSize + otherItem.StackSize <= this.StackMax;
        //    //if (!correctStackSize)
        //    //    //throw new Exception();
        //    //    return false;
        //    //return true;
        //}
        internal int StackAvailableSpace { get { return this.StackMax - this.StackSize; } }
        internal bool CanAbsorb(GameObject otherItem, int amount = -1)
        {

            if (this == otherItem)
                return false;
            //throw new Exception();
            if (this.Full)
                return false;
            if (otherItem.Def != null && this.Def != otherItem.Def)
            {
                return false;
            }

            if (!this.HasMatchingBody(otherItem))
                return false;
            if (amount == -1)
                return true;
            if (this.StackSize + amount > this.StackMax)
            {
                throw new Exception();
                //return false;
            }

            return true;
        }
        internal bool CanStackWithOld(GameObject otherItem, int amount)
        {
            if (this == otherItem)
                throw new Exception();
            if (this.IDType != otherItem.IDType)
                return false;
            var correctStackSize = this.StackSize + amount <= this.StackMax;
            if (!correctStackSize)
                throw new Exception();
            return this.IDType == otherItem.IDType;// && correctStackSize;
        }
        internal bool CanStackWithOldest(GameObject otherItem, int amount = -1)
        {
            var correctStackSize = (amount == -1 ? this.StackSize : amount) + otherItem.StackSize <= this.StackMax;
            if (!correctStackSize)
                throw new Exception();
            return this.IDType == otherItem.IDType;// && correctStackSize;
        }

        internal bool ProvidesSkill(ToolAbilityDef skill)
        {
            return ToolAbilityComponent.HasSkill(this, skill);
            //return this.GetDef<ItemToolDef>()?.Ability.Def == skill;

        }
        //internal GameObjectSlot GetEquipmentSlot(GearType type)
        //{
        //    return GearComponent.GetSlot(this, type);
        //}
        internal GameObjectSlot GetEquipmentSlot(GearType.Types type)
        {
            return GearComponent.GetSlot(this, GearType.Dictionary[type]);
        }

        internal GameObject GetHauled()
        {
            return PersonalInventoryComponent.GetHauling(this).Object;
        }
        internal bool IsHauling()
        {
            return this.GetHauled() != null;
        }
        internal GameObject Carried
        {
            get { return PersonalInventoryComponent.GetHauling(this).Object; }
        }
        internal GameObject ClearCarried()
        {
            var carried = PersonalInventoryComponent.GetHauling(this);
            var obj = carried.Object;
            carried.Clear();
            return obj;
        }
        internal GameObject InventoryFirst(Func<GameObject, bool> condition)
        {
            return PersonalInventoryComponent.GetFirstObject(this, condition);
        }
        internal bool InventoryContains(GameObject item)
        {
            return this.InventoryContains(i => i == item);
        }
        internal bool InventoryContains(Func<GameObject, bool> condition)
        {
            return PersonalInventoryComponent.HasObject(this, condition);
        }
        internal void StoreCarried()
        {
            PersonalInventoryComponent.StoreHauled(this);
        }
        internal List<GameObject> GetPossesions()
        {
            return NpcComponent.GetPossesions(this).Select(id => this.Net.GetNetworkObject(id)).ToList();
        }
        internal List<GameObject> InventoryAll()
        {
            return PersonalInventoryComponent.GetAllItems(this);
        }
        //internal Need GetNeed(Need.Types need)
        //{
        //    return NeedsComponent.GetNeed(this, need);
        //}
        internal Need GetNeed(NeedDef def)
        {
            return this.GetComponent<NeedsComponent>().NeedsNew.First(n => n.NeedDef == def);
        }
        internal IEnumerable<Need> GetNeeds(NeedCategoryDef cat)
        {
            return this.GetComponent<NeedsComponent>().NeedsNew.Where(n => n.NeedDef.CategoryDef == cat);
        }
        public IEnumerable<Need> GetNeeds()
        {
            foreach (var n in this.GetComponent<NeedsComponent>().NeedsNew)
                yield return n;
        }
        internal Need GetNeed(string needName)
        {
            return this.GetComponent<NeedsComponent>().NeedsNew.First(n => n.NeedDef.Name == needName);
        }
        internal float GetStat(Stat.Types type)
        {
            return Start_a_Town_.Components.StatsComponentNew.GetStatValueOrDefault(this, type);
        }
        //internal float GetWorkCapability(int skillID)
        //{
        //    return ToolAbilityComponent.GetWorkCapability(this, skillID);
        //}
        internal float GetToolWorkAmount(int skillID)
        {
            var tool = this.GetEquipmentSlot(GearType.Types.Mainhand).Object;
            if (tool == null)
                return 1;
            //return tool.GetWorkCapability(skillID);
            var ability = tool.Def.ToolProperties?.Ability;

            if (!ability.HasValue)
                throw new Exception();
            return ability.Value.Efficiency;
        }

        //internal bool CanReach(TargetArgs target)
        //{
        //    return this.Map.GetRegionDistance(this.StandingOn(), target.Global.SnapToBlock(), (int)Math.Ceiling(this.Physics.Height)) != -1;
        //    //return this.Map.GetRegionDistance(this.Global.Below().SnapToBlock(), target.Global.SnapToBlock(), (int)this.Physics.Height) != -1;
        //}
        //internal bool CanReach(GameObject obj)
        //{
        //    return this.Map.GetRegionDistance(this.StandingOn(), obj.Global.SnapToBlock(), (int)Math.Ceiling(this.Physics.Height)) != -1;
        //    //return this.Map.GetRegionDistance(this.Global.Below().SnapToBlock(), global, (int)this.Physics.Height) != -1;
        //}
        public bool CanReachNew(GameObject obj)
        {
            //return this.Map.Regions.CanReach(this.StandingOn(), obj.Global.SnapToBlock(), (int)Math.Ceiling(this.Physics.Height));
            return this.Map.Regions.CanReach(this.StandingOn(), obj.Global.SnapToBlock(), this as Actor);

        }
        internal bool CanReachNew(Vector3 global)
        {
            //return this.Map.Regions.CanReach(this.StandingOn(), global.SnapToBlock(), (int)Math.Ceiling(this.Physics.Height));
            return this.Map.Regions.CanReach(this.StandingOn(), global.SnapToBlock(), this as Actor);

        }
        internal bool CanReach(GameObject obj)
        {
            //return this.Map.GetRegionDistance(this.StandingOn(), obj.Global.SnapToBlock(), (int)Math.Ceiling(this.Physics.Height)) != -1;
            return this.Map.GetRegionDistance(this.StandingOn(), obj.Global.SnapToBlock(), this as Actor) != -1;

        }
        internal bool CanReach(Vector3 global)
        {
            //return this.Map.GetRegionDistance(this.StandingOn(), global.SnapToBlock(), (int)Math.Ceiling(this.Physics.Height)) != -1;
            return this.Map.GetRegionDistance(this.StandingOn(), global.SnapToBlock(), this as Actor) != -1;
        }
        [Obsolete]
        internal bool CanReachOld(Vector3 global)
        {
            return this.Map.CanReach(this, global);
        }
        internal BehaviorPerformTask GetLastBehavior()
        {
            return AIState.GetState(this).LastBehavior;
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
        internal BoundingBox GetBoundingBox(Vector3 global)
        {
            return this.Physics.GetBoundingBox(global);
        }
        internal BoundingBox GetBoundingBox(Vector3 global, float height)
        {
            return this.Physics.GetBoundingBox(global, height);
        }
        internal bool Intersects(BoundingBox boundingBox)
        {
            return this.GetBoundingBox(this.Global).Intersects(boundingBox);
        }
        internal bool Intersects(Vector3 global, BoundingBox boundingBox)
        {
            return this.GetBoundingBox(global).Intersects(boundingBox);
        }

        internal Vector3 GetNextStep()
        {
            return this.Global + PhysicsComponent.Decelerate(this.Velocity);
        }
        public bool IsStockpilable()
        {
            return this.Def?.Category != null;
            //return Objects[this.ID].StorageCategory != null; // TODO: cleaner way?
        }
        public bool IsFood
        {
            get
            {
                return this.GetComponent<ConsumableComponent>()?.Effects.OfType<NeedEffect>().Any(e => e.Type == NeedDef.Hunger) ?? false;
                //return this.HasComponent<FoodComponent>();
            }
        }
        public void DrawIcon(int w, int h, float scale = 1)
        {
            // same as Body.RenderNewererest
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            var body = this.Body;
            var sprite = body.Sprite;
            //Rectangle rect = new Rectangle(3, 3, w - 6, h - 6);
            //var loc = new Vector2(rect.X, rect.Y);
            var loc = new Vector2(0, 0);

            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            MySpriteBatch mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"]; //EntitiesUI"];//
            //sb.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
            fx.Parameters["Viewport"].SetValue(new Vector2(w, h));
            Sprite.Atlas.Begin(gd);
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            //var scale = 1;
            loc += sprite.OriginGround;
            body.DrawGhost(this, mysb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
            mysb.Flush();
        }
        public static void DrawIcon(Bone body, int w, int h, float scale = 1)
        {
            // same as Body.RenderNewererest
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            var sprite = body.Sprite;
            //Rectangle rect = new Rectangle(3, 3, w - 6, h - 6);
            //var loc = new Vector2(rect.X, rect.Y);
            var loc = new Vector2(0, 0);

            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            MySpriteBatch mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"]; //EntitiesUI"];//
            //sb.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
            fx.Parameters["Viewport"].SetValue(new Vector2(w, h));
            Sprite.Atlas.Begin(gd);
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            //var scale = 1;
            loc += sprite.OriginGround;
            body.DrawGhost(mysb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
            mysb.Flush();
        }

        public bool Exists
        {
            get
            {
                return this.IsSpawned;
            }
        }

        internal void MoveOrder(TargetArgs target, bool enqueue)
        {
            this.GetComponent<AIComponent>().MoveOrder(target, enqueue);
        }
        internal bool IsAt(Vector3 global)
        {
            var cylindermax = new BoundingCylinder(global, .1f, 1);
            return cylindermax.Contains(this.Global);
        }
        internal void DrawAfter(MySpriteBatch sb, Camera cam)
        {
            foreach (var comp in this.Components.Values)
                comp.DrawAfter(sb, cam, this);
        }

        internal bool HasLabor(JobDef labor)
        {
            return labor == null ? true : AIState.GetState(this).HasJob(labor);
        }
        internal bool IsIndoors()
        {
            var region = Server.Instance.Map.GetRegionAt(this.Global.Below().SnapToBlock()); // TODO: find first solid block below object
            return region != null ? !region.Room.IsOutdoors : false;
        }

        internal bool IsForbiddable()
        {
            return !this.HasComponent<NpcComponent>();
        }

        internal void DrawHighlight(SpriteBatch sb, Camera camera)
        {
            SpriteComponent.DrawHighlight(this, sb, camera);
        }

        internal void DrawBorder(SpriteBatch sb, Camera camera)
        {
            this.GetScreenBounds(camera).DrawHighlightBorder(sb, .5f, camera.Zoom);//, Color.White, (int)camera.Zoom);
        }

        internal byte[] Serialize()
        {
            byte[] newData = global::Start_a_Town_.Net.Network.Serialize(w =>
            {
                this.Write(w);
            });
            return newData;
        }
        static readonly Vector3[] Corners = new Vector3[] {
                    new Vector3(.25f, .25f, 0),
                    new Vector3(-.25f, .25f, 0),
                    new Vector3(.25f, -.25f, 0),
                    new Vector3(-.25f, -.25f, 0)
                };
        internal Vector3 StandingOn()
        {
            //return this.Global.CeilingZ().Below().SnapToBlock();
            var global = this.Global;
            var below = global.CeilingZ().Below().SnapToBlock();
            var belownode = this.Map.GetNodeAt(below);
            if (belownode != null)
            {
                return below;
            }
            //else check corners because it's standing on the edge of a block
            foreach (var corner in Corners)
            {
                var pos = (global + corner).CeilingZ().Below().SnapToBlock();
                belownode = this.Map.GetNodeAt(pos);
                if (belownode != null)
                    return pos;
            }
            //return new Vector3(int.MinValue);
            throw new Exception(); //throwed when actor was stuck inside a block
        }
        internal Vector3 StandingOnOld()
        {
            //return this.Global.CeilingZ().Below().SnapToBlock();
            var global = this.Global;
            var below = global.CeilingZ().Below().SnapToBlock();
            var belownode = this.Map.GetNodeAt(below);
            if (belownode != null)
            {
                return below;
            }
            //else check corners because it's standing on the edge of a block
            foreach (var corner in Corners)
            {
                var pos = (global + corner).CeilingZ().Below().SnapToBlock();
                belownode = this.Map.GetNodeAt(pos);
                if (belownode != null)
                    return pos;
            }
            return new Vector3(int.MinValue);
            throw new Exception();
        }

        //internal void DrawIconAbove(SpriteBatch sb, Camera camera, Icon icon, float scale)
        //{
        //    var offset = IMap.IconOffset;
        //    scale *= camera.Zoom;
        //    var pos = camera.GetScreenPosition(this.Global) - new Vector2(icon.SourceRect.Width, icon.SourceRect.Height) * scale / 2; ;// -new Vector2(UI.Icon.Cross.SourceRect.Width / 2, rect.Height * camera.Zoom);
        //    pos.Y -= this.GetBounds(camera).Height;
        //    pos.Y += offset * icon.SourceRect.Height / 4 * scale;
        //    icon.Draw(sb, pos, scale, alpha: .5f);
        //}

        //public bool CanOperate(TargetArgs target)
        //{
        //    if (target.Type != TargetType.Position)
        //        throw new Exception();
        //    var global = target.Global;
        //    var operatingPositions = this.Map.GetCell(global).GetOperatingPositions();
        //    if (!operatingPositions.Any())
        //        return true;
        //    foreach(var pos in operatingPositions)
        //    {
        //        if (this.CanReach(global + pos))
        //            return true;
        //    }
        //    return false;
        //}

        //internal void MoveToggle(bool toggle)
        //{
        //    if (this.Net is Server)
        //        PacketEntityMoveToggle.Send(this.Net, this.InstanceID, toggle);

        //    if (toggle)
        //        this.GetComponent<MobileComponent>().Start(this);
        //    else
        //        this.GetComponent<MobileComponent>().Stop(this);
        //}
        //internal void WalkToggle(bool toggle)
        //{
        //    if (this.Net is Server)
        //        PacketEntityWalkToggle.Send(this.Net, this.InstanceID, toggle);

        //    this.GetComponent<MobileComponent>().ToggleWalk(toggle);
        //}
        //internal void SprintToggle(bool toggle)
        //{
        //    if (this.Net is Server)
        //        PacketEntitySprintToggle.Send(this.Net, this.InstanceID, toggle);

        //    this.GetComponent<MobileComponent>().ToggleSprint(toggle); 
        //}
        //internal void Jump()
        //{
        //    if (this.Net is Server)
        //        PacketEntityJump.Send(this.Net, this.InstanceID);
        //    this.GetComponent<MobileComponent>().Jump(this);
        //}

        internal bool IsFootprintWithinBlock(Vector3 target)
        {
            return target.ContainsEntityFootprint(this);
            //var feetbox = this.GetBoundingBox(this.Global, 0);//.25f);//actor.Global);
            //var blockbox = target.GetBoundingBox();
            //return blockbox.Contains(feetbox) == ContainmentType.Contains;
        }
        internal BoundingBox GetFootprint()
        {
            return this.GetBoundingBox(this.Global, 0);//.25f);//actor.Global);
        }


        internal void OnGameEvent(GameEvent e)
        {
            foreach (var c in this.Components)
                c.Value.OnGameEvent(this, e);
        }

        public Material Material
        {
            get
            {
                //return this.Body.Material;
                return this.GetComp<SpriteComponent>().GetMaterial(this.Body);
            }
        }

        internal Vector2 GetScreenPosition(Camera camera)
        {
            return camera.GetScreenPositionFloat(this.Global);
        }
        internal bool HasFocus()
        {
            if (Rooms.Ingame.Instance.ToolManager.ActiveTool != null)
                if (Rooms.Ingame.Instance.ToolManager.ActiveTool.Target != null)
                    return (Rooms.Ingame.Instance.ToolManager.ActiveTool.Target.Object == this);
            return false;
        }

        internal void Sync(IObjectProvider net)
        {
            PacketEntitySync.Send(net, this);
        }
        internal void SyncWrite(BinaryWriter w)
        {
            foreach (var comp in this.Components)
                comp.Value.SyncWrite(w);
        }

        internal void SyncRead(BinaryReader r)
        {
            foreach (var comp in this.Components)
                comp.Value.SyncRead(this, r);
        }

        //internal Personality GetPersonality()
        //{
        //    return AIState.GetState(this).Personality;
        //}

        //internal void ClaimOwnership(GameObject item, bool value)
        //{
        //    if (value)
        //        NpcComponent.AddPossesion(this, item);
        //    else
        //        NpcComponent.RemovePossesion(this, item);
        //}


        internal void DropInventoryItem(GameObject item, int amount)
        {
            PersonalInventoryComponent.DropInventoryItem(this, item, amount);
        }

        internal int GetOwner()
        {
            return this.GetComponent<OwnershipComponent>().Owner;
        }
        internal void SetOwner(GameObject actor)
        {
            //this.GetComponent<OwnershipComponent>().Owner = actor != null ? actor.InstanceID : -1;
            this.SetOwner(actor != null ? actor.RefID : -1);
            //this.GetComponent<OwnershipComponent>().SetOwner(this, actor);
        }
        internal void SetOwner(int actorID)
        {
            //this.GetComponent<OwnershipComponent>().Owner = actorID;
            this.TryGetComponent<OwnershipComponent>(c => c.SetOwner(this, actorID));
        }

        internal bool IsPlant()
        {
            return this.HasComponent<PlantComponent>();
        }



        //internal NpcSkill GetSkill(NpcSkill skill)
        //{
        //    return this.GetComponent<ComponentNpcSkills>().GetSkill(skill);
        //}
        internal NpcSkill GetSkill(SkillDef skill)
        {
            return this.GetComponent<NpcSkillsComponent>().GetSkill(skill);
        }


        internal AITask CurrentTask
        {
            get
            {
                return AIComponent.GetState(this).CurrentTask;
            }
            set
            {
                AIComponent.GetState(this).CurrentTask = value;
            }
        }
        internal BehaviorPerformTask CurrentTaskBehavior
        {
            get
            {
                return AIComponent.GetState(this).CurrentTaskBehavior;
            }
            set
            {
                AIComponent.GetState(this).CurrentTaskBehavior = value;
            }
        }

        public float Fuel
        {
            get
            {
                return this.Material?.Fuel.Value ?? 0;
            }
        }
        public float TotalFuel
        {
            get
            {
                return this.Fuel * this.StackSize;
            }
        }



        internal AIState GetState()
        {
            return AIState.GetState(this);
        }
        internal float TotalWeight
        {
            get
            {
                //return this.Physics.Weight * this.StackSize;
                return this.Def.Weight * this.StackSize;
            }
        }
        public ICollection<IEntityComp> Comps => throw new NotImplementedException();

        public T GetComp<T>() where T : class, IEntityComp
        {
            return this.Components.Values.First(c => c is T) as T;
        }

        public bool HasComp<T>() where T : class, IEntityComp
        {
            return this.Components.Values.Any(c => c is T);
        }

        public virtual void TabGetter(Action<string, Action> getter)
        {

        }

        public List<Animation> Animations => this.GetComponent<SpriteComponent>().Animations;

        public string DebugName { get { return $"[{this.RefID}]{this.Name}"; } }

        public void AddAnimation(Animation animation)
        {
            //if (this.Animations.Any(a => a.Def == animation.Def))
            //    throw new Exception();
            if (this.Animations.FirstOrDefault(a => a.Def == animation.Def) is Animation existing)
            {
                if (existing.WeightChange >= 0 && existing.State != AnimationStates.Removed)
                    throw new Exception(); // ANIMATION MIGHT STILL BE FADING OUT WHEN THE NEXT BEHAVIOR BEGINS AND ADDS THE SAME TYPE OF ANIMATION!
                //existing.FadeOutAndRemove();
            }
            //if (this.Animations.FirstOrDefault(a => a.Def == animation.Def) is Animation existing)
            //    existing.FadeOutAndRemove();
            if (this.Animations.Any(a => a == animation))
                throw new Exception();
            this.Animations.Add(animation);
        }
        internal void CrossFade(Animation animation, bool preFade, int fadeLength, Func<float, float, float, float> fadeInterpolation)
        {
            animation.FadeIn(preFade, fadeLength, fadeInterpolation);
            this.AddAnimation(animation);
        }
        internal void CrossFade(Animation animation, bool preFade, int fadeLength)
        {
            this.CrossFade(animation, preFade, fadeLength, Interpolation.Lerp);//(a, b, c) => Interpolation.Lerp(a, b, c));
        }

        //internal List<Func<GameObject, float, float>> GetModifiers(StatNewDef statNewDef)
        //{
        //    return this.GetComponent<StatsComponentNew>()?.GetModifiers(statNewDef);// ?? new List<Func<GameObject, float, float>>();
        //}
        internal List<StatNewModifier> GetStatModifiers(StatNewDef statNewDef)
        {
            return this.GetComponent<StatsComponentNew>()?.GetModifiers(statNewDef);// ?? new List<Func<GameObject, float, float>>();
        }

        internal void AddResourceModifier(ResourceRateModifier resourceModifier)
        {
            this.GetComponent<ResourcesComponent>().AddModifier(resourceModifier);
        }

        internal void AddStatModifier(StatNewModifier statNewModifier)
        {
            this.GetComponent<StatsComponentNew>().AddModifier(statNewModifier);
        }

        public int GetValue()
        {
            if (this.Def.BaseValue == 0)
                return 0;
            var quality = this.DefComponent.Quality;
            var bones = this.Body.GetAllBones();
            var value = 0;
            foreach (var b in bones)
            {
                value += b.Material.ValueBase;
            }
            return (int)(value * this.Def.BaseValue * quality.Multiplier);
        }
        public int GetValueTotal()
        {
            return this.GetValue() * this.StackSize;
        }


        static readonly int PacketSyncInstantiate, PacketSyncSetStacksize, PacketSyncAbsorb;
        static GameObject()
        {
            PacketSyncInstantiate = Network.RegisterPacketHandler(SyncInstantiate);
            PacketSyncSetStacksize = Network.RegisterPacketHandler(SyncSetStacksize);
            PacketSyncAbsorb = Network.RegisterPacketHandler(SyncAbsorb);
        }

        public void SyncInstantiate(IObjectProvider net)
        {
            if (net is not Server server)
                return;
                //throw new Exception();
            if (this.RefID != 0)
                throw new Exception();
            net.Instantiate(this);
            //var w = server.OutgoingStreamReliable;
            var w = server.GetOutgoingStream();
            w.Write(PacketSyncInstantiate);
            this.Write(w);
        }
        private static void SyncInstantiate(IObjectProvider net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var obj = CreatePrefab(r);
            net.Instantiate(obj);
        }
        public void SyncSetStackSize(int v)
        {
            var net = this.NetNew;
            if (net is Server)
                this.SetStackSize(v);
            var w = net.GetOutgoingStream();
            w.Write(PacketSyncSetStacksize);
            w.Write(this.RefID);
            w.Write(v);
        }
        private static void SyncSetStacksize(IObjectProvider net, BinaryReader r)
        {
            var obj = net.GetNetworkObject(r.ReadInt32());
            var value = r.ReadInt32();
            if (net is Client)
                obj.SetStackSize(value);
            else
                obj.SyncSetStackSize(value);
        }
        public void Absorb(GameObject obj)
        {

            //return; // disable temporarily until i make it server-side only

            if (this.IsReserved)
                return;

            if (!this.CanAbsorb(obj))
                return;

            this.StackSize += obj.StackSize;
            obj.Despawn();
            obj.Dispose();

        }
        public void SyncAbsorb(GameObject obj)
        {
            var net = this.NetNew;
            if (net is Client)
                throw new Exception();
            this.Absorb(obj);
            var w = net.GetOutgoingStream();
            w.Write(PacketSyncAbsorb);
            w.Write(this.RefID);
            w.Write(obj.RefID);
        }
        private static void SyncAbsorb(IObjectProvider net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var master = net.GetNetworkObject(r.ReadInt32());
            var slave = net.GetNetworkObject(r.ReadInt32());
            master.Absorb(slave);
        }

        public SaveTag Save(string name = "")
        {
            return new SaveTag(SaveTag.Types.Compound, name, this.SaveInternal());
        }

        
    }
}
