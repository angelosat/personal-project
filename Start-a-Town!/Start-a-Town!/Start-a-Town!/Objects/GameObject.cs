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

namespace Start_a_Town_
{
    public class DrawObjectArgs
    {
        public Camera Camera;
        public Controller Controller;
        public Player Player;
        public IMap Map;
        public Chunk Chunk;
        public Cell Cell;
        public Rectangle ScreenBounds, SpriteBounds;
        public GameObject Object;
        public float Depth;
        public Color Light;

        public DrawObjectArgs(Camera camera,
            Controller controller,
            Player player,
            IMap map,
            Chunk chunk,
            Cell cell,
            Rectangle spriteBounds,
            Rectangle screenBounds,
            GameObject obj,
            Color color,
            float depth)
        {
            this.Camera = camera;
            this.Controller = controller;
            this.Player = player;
            this.Map = map;
            this.Chunk = chunk;
            this.Cell = cell;
            this.SpriteBounds = spriteBounds;
            this.ScreenBounds = screenBounds;
            this.Object = obj;
            this.Depth = depth;
            this.Light = color;
        }
    }

    public class ParameterEventArgs : EventArgs
    {
        public object[] Parameters;
        public ParameterEventArgs(params object[] p)
        {
            this.Parameters = p;
        }
    }

    public class GameObjectCollection// SortedDictionary<GameObject.Types, GameObject>
    {
        SortedDictionary<int, GameObject> Dictionary = new SortedDictionary<int, GameObject>();

        public GameObject this[GameObject.Types type]
        {
            get { return this.Dictionary[(int)type]; }
            set { this.Dictionary[(int)type] = value; }
        }
        public GameObject this[int type]
        {
            get { return this.Dictionary[type]; }
            set { this.Dictionary[type] = value; }
        }
        public SortedDictionary<int, GameObject>.ValueCollection Values
        {
            get { return Dictionary.Values; }
        }
        public void Add(GameObject obj)
        {
            //Add(obj.ID, obj);
            this.Dictionary.Add((int)obj.ID, obj);
        }
        public void Add(params GameObject[] objects)
        {
            foreach (GameObject obj in objects)
                Add(obj);
        }
        public SortedDictionary<string, GameObject> ByName()
        {
            //return new SortedDictionary<string, GameObject>(GameObject.Objects.ToDictionary(pair => pair.Value.Name, pair => pair.Value));
            return new SortedDictionary<string, GameObject>(this.Dictionary.ToDictionary(pair => pair.Value.Name, pair => pair.Value));
        }

        public GameObject this[string objName]
        { get { return ByName()[objName]; } }
        public bool TryGetValue(string objName, out GameObject obj)
        {
            if (ByName().TryGetValue(objName, out obj))
                return true;
            return false;
        }
        public bool TryGetValue(int id, out GameObject obj)
        {
            if (this.Dictionary.TryGetValue(id, out obj))
                return true;
            return false;
        }
        internal bool ContainsKey(GameObject.Types id)
        {
            return this.Dictionary.ContainsKey((int)id);
        }
    }

    public class GameObject : IEntity, ITooltippable, IContextable, INameplateable, IDebuggable, ISlottable//, IHasChildren
    {
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

        public GameObject Debug() { return this; }

        static public event EventHandler<ObjectEventArgs> MessageHandled;
        static void OnMessageHandled(GameObject receiver, ObjectEventArgs e)
        {
            if (!MessageHandled.IsNull())
                MessageHandled(receiver, e);
        }

        public GameObjectSlot ToSlot(int amount = 1)
        {
            return new GameObjectSlot(this, this.StackSize);
            return new GameObjectSlot(this);//, amount);
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
            Cobble,
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
        }

        static Queue<Message> MessageQueue;
        static public GameObjectCollection Objects = new GameObjectCollection();
        #region Initialization
        static public void LoadObjects()
        {
            MessageQueue = new Queue<Message>();

            //Objects = new GameObjectCollection();

            Objects.Add(GameObjectDb.Actor);
            Objects.Add(GameObjectDb.Npc);

            Objects.Add(GameObjectDb.Tree);
            Objects.Add(GameObjectDb.BerryBush);
            Objects.Add(GameObjectDb.Campfire);
            //Objects.Add(GameObjectDb.Log);
            Objects.Add(GameObjectDb.Stone);
            Objects.Add(GameObjectDb.Coal);
            //Objects.Add(GameObjectDb.Soilbag);
            Objects.Add(GameObjectDb.Bench);
            Objects.Add(GameObjectDb.Package);
            //Objects.Add(GameObjectDb.WoodenPlank);
            //Objects.Add(GameObjectDb.FurnitureParts);
            //Objects.Add(GameObjectDb.Shovel);
            Objects.Add(GameObjectDb.ShovelHead);
            //Objects.Add(GameObjectDb.EpicShovel);
            //Objects.Add(GameObjectDb.Axe);
            //Objects.Add(GameObjectDb.Pickaxe);
            //Objects.Add(GameObjectDb.Hoe);
            //Objects.Add(GameObjectDb.Handsaw);
            //Objects.Add(GameObjectDb.Hammer);
            //Objects.Add(GameObjectDb.Construction);
            //Objects.Add(GameObjectDb.Air);
            //Objects.Add(GameObjectDb.Soil);
            //Objects.Add(GameObjectDb.Grass);
            //Objects.Add(GameObjectDb.Gravel);
            //Objects.Add(GameObjectDb.Flowers);
            //Objects.Add(GameObjectDb.Water);
            //Objects.Add(GameObjectDb.Farmland);
            //Objects.Add(GameObjectDb.Door);
            //Objects.Add(GameObjectDb.Bed);
            //Objects.Add(GameObjectDb.WoodenDeck);
            //Objects.Add(GameObjectDb.Scaffolding);
            //Objects.Add(GameObjectDb.Sand);
            //Objects.Add(GameObjectDb.Rock);
            //Objects.Add(GameObjectDb.Iron);
            //Objects.Add(GameObjectDb.Mineral);
            Objects.Add(GameObjectDb.Seeds);
            Objects.Add(GameObjectDb.Berries);
            Objects.Add(GameObjectDb.Fertilizer);
            Objects.Add(GameObjectDb.StrengthPotion);
            Objects.Add(GameObjectDb.Fists);
            Objects.Add(GameObjectDb.RottenFeet);
            Objects.Add(GameObjectDb.BareFeet);
            Objects.Add(GameObjectDb.BareHands);
            Objects.Add(GameObjectDb.Crate);
            Objects.Add(GameObjectDb.CheatShoes);
            Objects.Add(GameObjectDb.PickaxeHead);
            Objects.Add(GameObjectDb.AxeHead);
            Objects.Add(GameObjectDb.Handle);
            Objects.Add(GameObjectDb.Twig);
            Objects.Add(GameObjectDb.Cobble);
            //Objects.Add(GameObjectDb.Zombie);
            //Objects.Add(GameObjectDb.TestJob);
            //Objects.Add(GameObjectDb.ConstructionReservedTile);
            Objects.Add(GameObjectDb.TrainingDummy);
            Objects.Add(GameObjectDb.JobBoard);
            //Objects.Add(GameObjectDb.BlueprintBlock);
            //Objects.Add(GameObjectDb.Sword);
            //Objects.Add(EditorUI.BlueprintAir);
            //Objects.Add(GameObjectDb.WoodenFrame);
            //Objects.Add(GameObjectDb.CheatHammer);
            //Objects.Add(GameObjectDb.Smeltery);
            //Objects.Add(GameObjectDb.Furnace);
            //Objects.Add(GameObjectDb.IronBar);
            //Objects.Add(GameObjectDb.IronOre);
            Objects.Add(
                GameObjectDb.Shield,
                GameObjectDb.Brain,
                GameObjectDb.Helmet,
                GameObjectDb.Paper,
                GameObjectDb.ScribeBench,
                //GameObjectDb.BenchReactions,
                //GameObjectDb.Cobblestone,
                //GameObjectDb.BlockEmpty,
                GameObjectDb.ConstructionBlock,
                //GameObjectDb.WoodenPlankDark,
                //GameObjectDb.WoodenPlankRed
                BombComponent.GetEntity(3, Engine.TargetFps * 2)
                );
       //     Objects.Add(Blueprint.NoBlueprint);
            WorkbenchComponent.LoadBlueprints();
        }
        #endregion

        #region Common Properties
        public string Name
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
                info.SaveName = true;
            }
        }
        public string Firstname
        {
            get
            {
                return this.GetInfo().GetName().Split(' ').First();
            }
        }
        public string Description
        {
            get { return GetComponent<GeneralComponent>("Info").GetProperty<string>("Description"); }
            set { GetInfo()["Description"] = value; }
        }
        public GameObject.Types ID
        {
            get { return GetComponent<GeneralComponent>("Info").GetProperty<GameObject.Types>("ID"); }
            set { this.GetInfo()["ID"] = value; }
        }
        public string Type
        {
            get { return GetComponent<GeneralComponent>("Info").GetProperty<string>("Type"); }
            set { GetInfo()["Type"] = value; }
        }

        public int InstanceID
        {
            get { return this.Network.ID; }
        }
        // TODO: make field
        public IObjectProvider Net//;
        {
            get { return this.Network.Net; }
            //get { return this.Map.GetNetwork(); }
            set { this.Network.Net = value; }
        }
        public IMap Map
        {
            get
            {
                return this.Net.Map;
            }
        }

        public void DrawNameplate(SpriteBatch sb, Rectangle viewport, Nameplate plate)
        {
            plate.Draw(sb, viewport);
        }
        public void OnNameplateCreated(Nameplate plate)
        {
            plate.Controls.Add(new Label()
            {
                //Width = 100,
                Font = UIManager.FontBold,
                Text = this.Name,
                TextColorFunc = () => GetNameplateColor(),
                MouseThrough = true,
            });
            foreach (var comp in Components.Values)
                comp.OnNameplateCreated(plate);
        }
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
        public Rectangle GetBounds(Camera camera)
        {
            return camera.GetScreenBounds(this.Global, this.GetSprite().GetBounds());
            //return camera.GetScreenBounds(this.Global, (this["Sprite"]["Sprite"] as Sprite).GetBounds());
        }
        public Color GetNameplateColor()
        {
            return this.GetInfo().GetQualityColor();// *0.5f;
        }
       // public GameObject SetGlobal(Vector3 global) { this.Global = global; return this; }
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
                this.Transform.Direction = new Vector2(value.X, value.Y);
            }
            //get { return new Vector3(this.Transform.Direction, this.Global.Z); }
            //set
            //{
            //    this.Transform.Direction = new Vector2(value.X, value.Y);
            //}
        }

        public Chunk GetChunk()
        {
            //return this.Global.GetChunk(this.Map);
            return this.Map.GetChunk(this.Global);

        }

        public int StackSize
        {
            get { return this.GetInfo().StackSize; }
            set { 
                this.GetInfo().StackSize = value;
                if (value <= 0)
                {
                    if (this.Exists)
                        this.Net.Despawn(this);
                    this.Dispose();
                }
            }
        }
        public void SetStack(int value)
        {
            this.StackSize = value;
            //if (value <= 0)
            //{
            //    if (this.Exists)
            //        this.Net.Despawn(this);
            //    this.Dispose();
            //}
        }
        //public void SetStackSize(int value)
        //{

        //}
        public int StackMax
        {
            get { return this.GetInfo().StackMax; }
        }
        public ControlComponent Control
        {
            get
            {
                return this.GetComponent<ControlComponent>();
            }
        }

        public Graphics.Bone Body
        {
            get { return this.GetComponent<SpriteComponent>().Body; }
        }

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
            this.Global = global;
            return this;
        }

        public GameObject ChangePosition(Vector3 global) // TODO: merge this with SetGlobal
        {
            //global = global.Round(2);
            //global.Z.ToConsole();

            if (this.Map.IsSolid(global))// + Vector3.UnitZ * 0.01f))// TODO: FIX THIS
                return this; // TODO: FIX: problem when desynced from server, block might be empty on server but solid on client
                //throw new Exception();
            Chunk nextChunk, lastChunk;
            Position pos = this.Transform.Position;
            if (pos.IsNull())
            {

                //this.Transform.Position = new Position(this.Map, global);
                this.Global = global;
                bool added = Chunk.AddObject(this, this.Map, global);
                if (!added)
                    throw new Exception("Could not add object to chunk");
                return this;
            }
            //Position.TryGetChunk(this.Map, global.Round(), out nextChunk);
            this.Map.TryGetChunk(global.RoundXY(), out nextChunk);

            if (nextChunk.IsNull())
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
            this.Map.TryGetChunk(pos.Rounded, out lastChunk);

            if (nextChunk != lastChunk)
            {
                bool removed = Chunk.RemoveObject(this, lastChunk);
                bool added = Chunk.AddObject(this, this.Map, nextChunk, Position.Floor(global));
                if (!removed)
                    throw new Exception("Source chunk is't loaded"); //Could not remove object from previous chunk");
                if (!added)
                    throw new Exception("Invalid move: Destination chunk is't loaded");
                this.Net.EventOccured(Message.Types.EntityChangedChunk, this, nextChunk.MapCoords, lastChunk.MapCoords);
            }
            
            pos.Global = global;
            return this;
        }

        public bool Exists
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
        #endregion

        static public GameObject Create(int id)
        {
            return Create((GameObject.Types)id);
        }
        [Obsolete] // find a way to restrict creation of object to client/server class
        static public GameObject Create(GameObject.Types id)//, int count = 1)
        {
            if (!GameObject.Objects.ContainsKey(id))
            {
                return null;
            }
            GameObject prototype = GameObject.Objects[id];
            GameObject obj = new GameObject();

            foreach (KeyValuePair<string, Component> comp in prototype.Components)
                obj.AddComponent(comp.Key, comp.Value.Clone() as Component);


            obj.ComponentsCreated();
            return obj;
        }
        public GameObject(GameObject toCopy)
        {
            foreach (KeyValuePair<string, Component> comp in toCopy.Components)
                this.AddComponent(comp.Key, comp.Value.Clone() as Component);

            this.ComponentsCreated();
        }

        public Vector2 Orientation(GameObject target)
        {
            Vector3 dir = (target.Global - this.Global);
            Vector2 dirr = new Vector2(dir.X, dir.Y);
            dirr.Normalize();
            return dirr;
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
            //}).Send(Client.PacketID, PacketType.ObjectEvent, Client.Host, Start_a_Town_.Net.Client.RemoteIP); 
            }).Send(Client.PacketID, PacketType.RemoteCall, Client.Host, Start_a_Town_.Net.Client.RemoteIP); 

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
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
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
            using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(new System.IO.MemoryStream()))
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
            MessageQueue.Enqueue(msg);
        }
        static public void PostMessage(GameObject receiver, ObjectEventArgs e)
        {
            MessageQueue.Enqueue(new Message(receiver, e));
        }
        [Obsolete]
        static public void PostMessage(GameObject receiver, Message.Types msg, GameObject source, IObjectProvider net, byte[] data, params object[] p)
        {
            MessageQueue.Enqueue(new Message(receiver, new ObjectEventArgs(msg, source, p) { Data = data, Network = net }));
        }
        //[Obsolete]
        //static public void PostMessage(GameObject receiver, Message.Types msg, GameObject source = null, params object[] p)
        //{
        //    MessageQueue.Enqueue(new Message(receiver, new ObjectEventArgs(msg, source, p)));
        //}
        [Obsolete]
        static public void PostMessage(GameObject receiver, Message.Types msg, Action<GameObject> callback, GameObject source = null, params object[] p)
        {
            MessageQueue.Enqueue(new Message(receiver, new ObjectEventArgs(msg, source, p), callback));
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

        static public void Update()
        {
            while (MessageQueue.Count > 0)
            {
                Message msg = MessageQueue.Dequeue();
                msg.Receiver.HandleMessage(msg.Args);
                OnMessageHandled(msg.Receiver, msg.Args);
                if (msg.Callback != null)
                    msg.Callback(msg.Receiver);
            }
        }

        public List<GameObject> GetNearbyObjects(Func<float, bool> range, Func<GameObject, bool> filter = null, Action<GameObject> action = null)
        {
            var a = action ?? ((obj) => { });
            var f = filter ?? ((obj) => { return true; });
            List<GameObject> nearbies = new List<GameObject>();
            var map = this.Map;
            //Chunk chunk = Position.GetChunk(map, this.Global);
            Chunk chunk = map.GetChunk(this.Global);

            List<GameObject> objects = new List<GameObject>();
            //foreach (Chunk ch in Position.GetChunks(map, chunk.MapCoords))
            foreach (Chunk ch in map.GetChunks(chunk.MapCoords))
                foreach (GameObject obj in ch.GetObjects())
                {
                    if (obj == this)
                        continue;
                    //if ((obj.Global - parent.Global).Length() > range)
                    if (!range(Vector3.Distance(obj.Global, this.Global)))
                        continue;
                    if (!f(obj))
                        continue;
                    a(obj);
                    nearbies.Add(obj);
                }
            return nearbies;
        }

        public static bool IsBlock(GameObject obj) { return obj.Type == ObjectType.Block; }
        public bool IsBlock()
        {
            return this.HasComponent<BlockComponent>();
            //return this.Type == ObjectType.Block; 
        }

        public bool IsPlayerEntity()
        {
            PlayerData pl;
            return this.IsPlayerEntity(out pl); 
        }
        public bool IsPlayerEntity(out PlayerData player)
        {
            //player = this.Net.GetPlayers().FirstOrDefault(p => p.Character == this);
            player = this.Net.GetPlayers().FirstOrDefault(p => p.ID == this.Network.PlayerID);
            return player != null;
        }


        //public int NetworkID { get { return this.Network.ID;}
        //    set { this.Network.ID = value; }
        //}// (int)this["Network"]["ID"]; } }

 

        public PositionComponent Transform;
        public NetworkComponent Network;
        public Components.Tokens.TokensComponent Tokens;// = new Components.Tokens.TokensComponent();

        public GameObject()
        {
            //this["Network"] = new NetworkComponent();
            this.Network = this.AddComponent<NetworkComponent>();
            this.Transform = 
                this.AddComponent<PositionComponent>();
            this.Tokens = this.AddComponent<Components.Tokens.TokensComponent>();
        }
        public GameObject(GameObject.Types id, string name, string description, string type = "")
            : this()
        {
            // AddComponent("Info", new InfoComponent(id, name, description));
            this["Info"] = new GeneralComponent(id, type, name, description);
        }

        public Component this[string componentName]
        {
            get { return this.Components[componentName]; }
            //set { Components[componentName] = value; }
            set {
                Factory.Register(value);
                Components[value.ComponentName] = value;
                value.MakeChildOf(this);
            }
        }

        public GeneralComponent GetInfo()
        {
            return GetComponent<GeneralComponent>("Info");
        }
        public int GetID()
        {
            return this.GetInfo().ID;
        }

        PhysicsComponent _PhysicsCached;
        public PhysicsComponent GetPhysics()
        {
            //return GetComponent<PhysicsComponent>();
            if(this._PhysicsCached == null)
                this._PhysicsCached = GetComponent<PhysicsComponent>();
            return this._PhysicsCached;
        }
        public PhysicsComponent Physics { get { return this.GetPhysics(); } }
        public GuiComponent GetGui()
        {
            return GetComponent<GuiComponent>("Gui");
        }
        //public GameObject SetGui(GuiComponent guiComp = null) { this["Gui"] = guiComp != null ? guiComp : new GuiComponent(); return this; }

        public ComponentCollection Components = new ComponentCollection();
        //public virtual bool Remove()
        //{ return false; }

        //public T GetComponent<T>() where T : Component
        //{
        //    //return (T)Components[name];
        //    return this.Components[name] as T;
        //}

        public T GetComponent<T>(string name) where T : Component, new()
        {
            //return this.Components[new T().ComponentName] as T;
            return this.GetComponent<T>();
        }
        public T GetComponent<T>() where T : Component, new()
        {
            return (from comp in Components.Values
                    where comp is T
                    select comp).SingleOrDefault() as T;
            //return this.Components[new T().ComponentName] as T;
        }
        //public bool GetComponent<T>(Action<T> action) where T : Component, new()
        //{
        //    T component = this.GetComponent<T>();
        //    if (component.IsNull())
        //        return false;
        //    action(component);
        //    return true;
        //}
        public bool HasComponent<T>() where T : Component, new()
        {
            return !this.GetComponent<T>().IsNull();
        }

        public bool TryGetComponent<T>(string name, out T component) where T : Component, new()
        {
            //Component comp;
            //bool found = Components.TryGetValue(name, out comp);
            //component = comp as T;
            //return (component != null);

            return this.TryGetComponent<T>(out component);
        }
        public bool TryGetComponent<T>(out T component) where T : Component, new()
        {
            //Component comp;
            //bool found = Components.TryGetValue(new T().ComponentName, out comp);
            //component = comp as T;
            //return (component != null);

            component = this.GetComponent<T>();
            return !component.IsNull();


            //T c = Components.SingleOrDefault(foo => foo.Value is T) as T;
            //object comp = Components.SingleOrDefault(foo => foo.Value is T);// as T;
            //component = comp as T;
            //bool found = comp is T;
            //return !component.IsNull();
        }
        public bool TryGetComponent<T>(Action<T> action) where T : Component, new()
        {
            T component = this.GetComponent<T>();
            if (component.IsNull())
                return false;
            action(component);
            return true;
        }
        public TReturn TryGetComponent<TComponent, TReturn>(Func<TComponent, TReturn> func) where TComponent : Component, new()
        {
            TComponent component = this.GetComponent<TComponent>();
            if (component.IsNull())
                return default(TReturn);
            return func(component);
        }
        public bool TryRemoveComponent<T>(string name, out T component) where T : Component, new()
        {

            //Component comp;
            //Components.TryGetValue(name, out comp);
            //component = comp as T;
            //return this.Components.Remove(name);


            component = this.GetComponent<T>();
            return this.RemoveComponent<T>();
            //if (!component.IsNull())
            //    return this.Components.Remove(component.ComponentName);
            //return false;
        }
        public bool TryRemoveComponent<T>(string name) where T: Component, new()
        {
            //return this.Components.Remove(name);

            return this.RemoveComponent<T>();
        }

        public bool RemoveComponent<T>() where T : Component, new()
        {
            //T component = Components.SingleOrDefault(foo => foo.Value is T) as T;
            //if (!component.IsNull())
            //    return this.Components.Remove(component.ComponentName);
            //return false;

            T component = this.GetComponent<T>();
            if (!component.IsNull())
                return this.Components.Remove(component.ComponentName);
            return false;

        }
        public bool TryRemoveComponent<T>(out T component) where T : Component, new()
        {
            component = Components.Values.SingleOrDefault(foo =>
            {
                return foo is T;
            }) as T;
            if (!component.IsNull())
                return this.Components.Remove(component.ComponentName);
            return false;


            //component = Components.SingleOrDefault(foo =>
            //{
            //    return foo.Value is T;
            //}) as T;
            //if (!component.IsNull())
            //    return this.Components.Remove(component.ComponentName);
            //return false;
        }

        public bool TryRemoveComponent<T>() where T : Component, new()
        {
            T comp;
            return this.TryRemoveComponent<T>(out comp);

        }

        public bool AddComponent(string name, Component component)
        {
            //Components.Add(name, component);
            Components[component.ComponentName] = component;
            component.MakeChildOf(this);
            return true;
        }
        public Component AddComponent(Component component)
        {
            //Components.Add(name, component);
            if (component.IsNull())
                return null;
            Components[component.ComponentName] = component;
            component.MakeChildOf(this);
            return component;
        }
        public Component AddComponent(string componentName)
        {
            Component component = Factory.Create(componentName);
            component.MakeChildOf(this);
            Components[componentName] = component;
            return component;
        }
        public T AddComponent<T>() where T : Component, new()
        {
            T component = new T();
            Components[component.ComponentName] = component;
            component.MakeChildOf(this);
            Factory.Register(component);

            return component;
        }

        public virtual void Update(IObjectProvider net, Chunk chunk = null)
        {
            //Components.Update(net, this, chunk);
            Components.Update(this);
        }
        //public virtual void RandomBlockUpdate(IObjectProvider net)
        //{
        //    Components.RandomBlockUpdate(net, this);
        //}
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
            foreach (KeyValuePair<string, Component> comp in Components)
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

        static public Dictionary<GameObject, Window> GameObjectWindows = new Dictionary<GameObject, Window>();
        public Window GetUi()
        {
            Window existing;
            if (GameObjectWindows.TryGetValue(this, out existing))
                return existing;

            //Window window = new Window() { Title = this.Name, Movable = true, AutoSize = true };
            //var window = new WindowEntityInterface(this) { Title = this.Name, Movable = true, AutoSize = true };
            var window = new WindowEntityInterface(this, this.Name, ()=>this.Global) { Title = this.Name, Movable = true, AutoSize = true };

            GameObjectWindows.Add(this, window);
            //List<EventHandler<ObjectEventArgs>> handlers = new List<EventHandler<ObjectEventArgs>>();
            List<EventHandler<GameEvent>> gameEventHandlers = new List<EventHandler<GameEvent>>();
            //this.GetUI(window.Client, handlers);
            this.GetUI(window.Client, gameEventHandlers);
            window.Location = window.CenterScreen;

            EventHandler<GameEvent> handler = (sender, e) =>
            {
                //gameEventHandlers.ForEach(foo =>
                //{
                //    foo(sender, e);
                //});
                foreach (var h in gameEventHandlers)
                    h(sender, e);
            };
            Client.Instance.GameEvent += handler;
            window.HideAction = () => { Client.Instance.GameEvent -= handler; GameObjectWindows.Remove(this); };
            return window;
        }
        //public Window GetUi()
        //{
        //    Window window = new Window() { Title = this.Name, Movable = true, AutoSize = true };
        //    //List<EventHandler<ObjectEventArgs>> handlers = new List<EventHandler<ObjectEventArgs>>();
        //    List<EventHandler<Net.GameEvent>> gameEventHandlers = new List<EventHandler<Net.GameEvent>>();
        //    //this.GetUI(window.Client, handlers);
        //    this.GetUI(window.Client, gameEventHandlers);
        //    window.Location = window.CenterScreen;

        //    EventHandler<ObjectEventArgs> handler = (sender, e) =>
        //    {
        //        handlers.ForEach(foo =>
        //        {
        //            foo(sender, e);
        //        });
        //    };
        //    MessageHandled += handler;
        //    window.HideAction = () => MessageHandled -= handler;
        //    return window;
        //}
        //public void GetUI(UI.Control ui, List<EventHandler<ObjectEventArgs>> uiUpdaters)
        public void GetUI(UI.Control ui, List<EventHandler<GameEvent>> gameEventHandlers)
        {
            Panel panel_tabs = new Panel() { AutoSize = true };
            GroupBox panel_ui = new GroupBox();
            int rd_y = 0;
            List<GroupBox> boxes = new List<GroupBox>();
            foreach (var c in this.Components)
            {
                GroupBox boxComp = new GroupBox();
                c.Value.GetUI(this, boxComp, gameEventHandlers);//uiUpdaters);
                boxes.Add(boxComp);
                if (boxComp.Controls.Count == 0)
                    continue;
                RadioButton rd = new RadioButton(c.Key, new Vector2(0, rd_y))
                {
                    Tag = boxComp, 
                    LeftClickAction = () => { panel_ui.Controls.Clear(); panel_ui.Controls.Add(boxComp); }
                };
                panel_tabs.Controls.Add(rd);
                rd_y += Label.DefaultHeight;
            }
            panel_ui.Conform(boxes.ToArray());
            //panel_ui.Location = panel_tabs.TopRight;
            //ui.Controls.Add(panel_tabs, panel_ui);
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
            //foreach (var c in this.Components)
            //    c.Value.GetUI(this, ui);
        }
        public UI.Control GetTooltip()//Message msg)
        {
            GroupBox box = new GroupBox();
            GetInfo().GetTooltip(this, box);
            // TODO: LOL fix, i need the object name to be on top
            foreach (KeyValuePair<string, Component> comp in Components.Except(new KeyValuePair<string, Component>[] { new KeyValuePair<string, Component>("Info", GetInfo()) }))
                comp.Value.GetTooltip(this, box);
            box.MouseThrough = true;
            return box;
        }
        public void GetTooltip(UI.Control tooltip)//Message msg)
        {
            GetInfo().GetTooltip(this, tooltip);
            // TODO: LOL fix, i need the object name to be on top
            foreach (KeyValuePair<string, Component> comp in Components.Except(new KeyValuePair<string, Component>[] { new KeyValuePair<string, Component>("Info", GetInfo()) }))
                comp.Value.GetTooltip(this, tooltip);
            return;
            var actions = this.GetPlayerActionsWorld();
            if (actions.Count == 0)
                return;
            PanelLabeled actionsPanel = new PanelLabeled("Actions") { Location = tooltip.Controls.BottomLeft };
            foreach (var act in actions)
                if (act.Value.AvailabilityCondition(Player.Actor, new TargetArgs(this)))
                    //actionsPanel.Controls.Add(new Label(act.Key.GetKey().ToString() + ": " + act.Value.Name) { Location = actionsPanel.Controls.BottomLeft });
                    actionsPanel.Controls.Add(new Label(act.Key.ToString() + ": " + act.Value.Name) { Location = actionsPanel.Controls.BottomLeft });
            tooltip.Controls.Add(actionsPanel);
        }
        public void GetInventoryTooltip(UI.Control tooltip)
        {
            GetInfo().GetTooltip(this, tooltip);
            // TODO: LOL fix, i need the object name to be on top
            foreach (KeyValuePair<string, Component> comp in Components.Except(new KeyValuePair<string, Component>[] { new KeyValuePair<string, Component>("Info", GetInfo()) }))
                comp.Value.GetInventoryTooltip(this, tooltip);
        }
        public virtual void GetTooltip(GameObject actor, UI.Tooltip tooltip)
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.GetActorTooltip(this, actor, tooltip);
        }

        public void AIQuery(GameObject ai, List<Components.AIAction> actions)
        {
            foreach (var comp in this.Components.Values)
                comp.AIQuery(this, ai, actions);
        }
        public List<InteractionOld> Query(GameObject actor, params object[] parameters)
        {
            List<InteractionOld> actions = new List<InteractionOld>();
            List<object> p = new List<object>() { actions };
            p.AddRange(parameters);
            foreach (Component c in Components.Values)
                //c.HandleMessage(this, new GameObjectEventArgs(Message.Types.Query, actor, p.ToArray()));
                c.Query(this, actions);//, new GameObjectEventArgs(Message.Types.Default, actor, p.ToArray()));
            return actions;
        }
        public bool Query(GameObject actor, Action<List<InteractionOld>> callback)
        {
            List<InteractionOld> actions = new List<InteractionOld>();
            foreach (Component c in Components.Values)
                //c.HandleMessage(this, new GameObjectEventArgs(Message.Types.Query, actor, actions));
                c.Query(this, actions);//new GameObjectEventArgs(Message.Types.Default, actor, actions));
            callback(actions);
            return actions.Count > 0;
        }
        public bool Query(GameObject actor, List<InteractionOld> list)
        {
            foreach (Component c in Components.Values)
                //c.HandleMessage(this, new GameObjectEventArgs(Message.Types.Query, actor, list));
                c.Query(this, list);//new GameObjectEventArgs(Message.Types.Default, actor, list));
            return list.Count > 0;
        }

        
        //public void Spawn(Map map)
        //{
        //    this.SetMap(map);
        //    this.Spawn();
        //}
        public void Despawn()
        {
            //this.Net.Despawn(this);
            foreach (var comp in this.Components.Values.ToList())
                comp.Despawn(this);
            this.Map.EventOccured(Message.Types.EntityDespawned, this);

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

        [Obsolete]
        public void Spawn(IObjectProvider net)
        {

            this.Parent = null;
            foreach(var comp in this.Components.Values)
                comp.Spawn(net, this);
            this.Map.EventOccured(Message.Types.EntitySpawned, this);

        }
       

        public void ChunkLoaded(IObjectProvider net)
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.ChunkLoaded(net, this);
            //UI.Nameplate.Create(this);
        }

        //public Nameplate NamePlate;
        public void Focus()
        {
            Nameplate.Show(this);
            //Nameplate.GetNameplate(this).OnFocus();
            //var plate = Nameplate.GetNameplate(this);
            //if (plate != null)
            //    plate.OnFocus();
            //this.NamePlate.OnFocus();
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.Focus(this);
        }
        public void FocusLost()
        {
            Nameplate.Hide(this);
            //Nameplate.GetNameplate(this).OnFocusLost();
            //this.NamePlate.OnFocusLost();
            //var plate = Nameplate.GetNameplate(this);
            //if (plate != null)
            //    plate.OnFocusLost();
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.FocusLost(this);
        }
        public DialogueOptionCollection GetDialogueOptions(GameObject speaker)
        {
            DialogueOptionCollection options = new DialogueOptionCollection();
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
            List<DialogOption> options = new List<DialogOption>();
            foreach (var comp in Components)
                comp.Value.GetDialogOptions(this, speaker, options);
            return options;
        }
        public virtual void Draw(SpriteBatch sb, DrawObjectArgs e)
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.Draw(sb, e);
        }
        public virtual void Draw(MySpriteBatch sb, Camera camera)
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.Draw(sb, this, camera);
        }
        public virtual void Draw(MySpriteBatch sb, DrawObjectArgs e)
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.Draw(sb, e);
        }
        internal void DrawMouseover(SpriteBatch sb, Camera camera)
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.DrawMouseover(sb, camera, this);
        }
        internal void DrawMouseover(MySpriteBatch sb, Camera camera)
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.DrawMouseover(sb, camera, this);
        }
        internal void DrawInterface(SpriteBatch sb, Camera camera)
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.DrawUI(sb, camera, this);
        }
        internal void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global)
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.DrawPreview(sb, camera, global);
        }
        internal void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, float depth)
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.DrawPreview(sb, camera, global, depth);
        }
        internal void DrawFootprint(SpriteBatch sb, Camera camera, Vector3 global)
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.DrawFootprint(sb, camera, global);
        }
        public virtual void GetTooltipInfo(Tooltip tooltip)//List<GroupBox> TooltipGroups
        {
            GetTooltip(tooltip);
        }
        public void GetTooltipBasic(Tooltip tooltip)
        {
            GameObject obj = this;// ctrl.Tag as GameObject;
            Color quality = GeneralComponent.GetQualityColor(obj);

            //var render = this.Body.RenderNew(this);
            //tooltip.Controls.Add(new Panel() { AutoSize = true }.AddControls(sprite.ToPictureBox()));
            //PictureBox pic = new PictureBox(Vector2.Zero, render, null);// sprite.ToPictureBox();
            PictureBox pic = new PictureBox(new Vector2(32), r=>this.Body.RenderNewer(this, r));// sprite.ToPictureBox();


            Label name = new Label(pic.TopRight, obj.Name, quality, Color.Black, UIManager.FontBold);// { Location = pic.TopRight };// obj.Name.ToLabel(pic.TopRight);
            Label desc = obj.Description.ToLabel(name.BottomLeft);
            tooltip.AddControls(pic, name, desc);
            tooltip.Color = quality;

            //GameObject obj = this;// ctrl.Tag as GameObject;
            //Color quality = GeneralComponent.GetQualityColor(obj);
            //Sprite sprite = GetSpriteOrDefault();
            ////tooltip.Controls.Add(new Panel() { AutoSize = true }.AddControls(sprite.ToPictureBox()));
            //PictureBox pic = sprite.ToPictureBox();
            //Label name = new Label(pic.TopRight, obj.Name, quality, Color.Black, UIManager.FontBold);// { Location = pic.TopRight };// obj.Name.ToLabel(pic.TopRight);
            //Label desc = obj.Description.ToLabel(name.BottomLeft);
            //tooltip.AddControls(pic, name, desc);
            //tooltip.Color = quality;
        }

        public void DrawPreview(MySpriteBatch sb, Camera cam, Vector3 global)
        {
            var body = this.Body;
            var pos = cam.GetScreenPositionFloat(global);
            pos += body.OriginGroundOffset * cam.Zoom;
            //body.DrawTree(this.Entity, sb, pos, Color.White, Color.White, Color.White, Color.Transparent, body.RestingFrame.Offset, 0, cam.Zoom, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
            // TODO: fix difference between tint and material in this drawtree method
            var tint = Color.White * .5f;// Color.Transparent;
            body.DrawTree(this, sb, pos, Color.White, Color.White, tint, Color.Transparent, 0, cam.Zoom, 0, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
        }

        public Sprite GetSprite()
        {
            SpriteComponent sprComp;
            if (!TryGetComponent<SpriteComponent>(out sprComp))
                return null;
            return sprComp.Sprite;
        }
        public Sprite GetSpriteOrDefault()
        {
            SpriteComponent sprComp;
            if (!TryGetComponent<SpriteComponent>("Sprite", out sprComp))
                return Sprite.Default;
            return sprComp.Sprite ?? Sprite.Default;
        }
        public Icon GetIcon()
        {
            return new Icon(GetSpriteOrDefault());
        }

        public GameObject Clone(bool initialize = true)
        {
            GameObject obj = new GameObject();
            foreach (KeyValuePair<string, Component> comp in Components)
            {
              //  obj.AddComponent(comp.Key, comp.Value.Clone() as Component);
                obj[comp.Key] = comp.Value.Clone() as Component;
            }
            //   obj.Initialize();

            if (initialize)
                obj.Initialize();
            return obj;
        }

        public GameObject New()
        {
            return GameObject.Create(ID);
        }


        public byte[] GetSnapshotData()
        {
            using(BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                this.Write(w);
                return (w.BaseStream as MemoryStream).ToArray();
            }
        }


        public void Write(BinaryWriter writer)
        {
            writer.Write((int)ID);
            writer.Write(this.Components.Count);
            foreach (var comp in this.Components)
            {
                //if(Factory.Create(comp.Key).IsNull())
                //    throw new Exception();
                writer.Write(comp.Key);
                comp.Value.Write(writer);
            }
        }
        /// <summary>
        /// Updates the object to the values provided by the reader
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Read(BinaryReader reader)
        {
            reader.ReadByte(); //skip the first byte (object type)
            int compCount = reader.ReadInt32();
            for (int i = 0; i < compCount; i++)
            {
                string compName = reader.ReadString();
                this[compName].Read(reader);
            }
          //  this.ObjectCreated();
        }
        public static GameObject CreateCustomObject(BinaryReader reader)
        {
            GameObject.Types type = (GameObject.Types)reader.ReadByte();
            GameObject obj = new GameObject(); // WARNING: must figure out way to reconstruct an object without it's creating a prefab

            int compCount = reader.ReadInt32();
            for (int i = 0; i < compCount; i++)
            {
                string compName = reader.ReadString();
                //if (obj.Components.ContainsKey(compName))
                //    obj[compName].Read(reader);

                Component comp = Factory.Create(compName);
                if (comp.IsNull())
                    continue;
                obj.AddComponent(comp).Read(reader);

            }
            //obj.ObjectCreated(); // i'll put that only where i need it after  calling reconstruct
            return obj;
        }
        public static GameObject CreatePrefab(BinaryReader reader)
        {
            int type = reader.ReadInt32();
            GameObject obj = Create(type); // WARNING: must figure out way to reconstruct an object without it's creating a prefab

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
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.ObjectLoaded(this);
            return this;
        }
        /// <summary>
        /// try to make this private
        /// </summary>
        /// <returns></returns>
        public GameObject ComponentsCreated()
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.ComponentsCreated(this);
            //this.EnumerateChildren();
            return this;
        }
        public GameObject ObjectSynced()
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.ObjectSynced(this);
            this.EnumerateChildren();
            return this;
        }
        public GameObject Initialize()
        {
            foreach (KeyValuePair<string, Component> comp in Components)
                comp.Value.Initialize(this);
            return this;
        }

        internal List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();
            data.Add(new SaveTag(SaveTag.Types.Int, "TypeID", (int)ID));

            //Tag compData = new Tag(Tag.Types.List, "Components", Tag.Types.Compound);
            SaveTag compData = new SaveTag(SaveTag.Types.Compound, "Components");
            foreach (KeyValuePair<string, Component> comp in Components)
            {
                List<SaveTag> compSave = comp.Value.Save();
                if (compSave != null)
                    compData.Add(new SaveTag(SaveTag.Types.Compound, comp.Key, compSave));
                //compData.Add(new Tag(Tag.Types.Compound, comp.Key, compSave));
            }
            data.Add(compData);
            return data;
        }

        ///// <summary>
        ///// Creates an object from a tag node.
        ///// </summary>
        ///// <param name="tag">A tag with a list of tags as its value.</param>
        ///// <returns></returns>
        //internal static GameObject Create(SaveTag tag)
        //{
        //    return Create(tag, obj => { });
        //}        /// <param name="objectFactory">A factory function to apply on the object after its creation.</param>

        /// <summary>
        /// Creates an object from a tag node.
        /// </summary>
        /// <param name="tag">A tag with a list of tags as its value.</param>
        /// <returns></returns>
        internal static GameObject Load(SaveTag tag)//, Action<GameObject> objectFactory) //IObjectProvider net, 
        {
            var val = tag["TypeID"].Value;
            GameObject.Types type = (GameObject.Types)val;
            // if (type == Types.Actor)
            //    Console.WriteLine("WOW");
            GameObject obj = GameObject.Create(type);

            if (obj.IsNull())
                return obj;
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
                        obj[compTag.Name].Load(compTag);//, objectFactory ?? new Action<GameObject>(o => { }));//.Value as List<Tag>);
            }
            //obj.ObjectCreated(); // UNCOMMENT IF PROBLEMS
            obj.ObjectLoaded();
            return obj;
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
            if (Player.Actor.IsNull())
                return;

            a.Actions.Add(new ContextAction(() => "Drop", () =>
            {
                GameObjectSlot slot = Player.Actor.GetChild((byte)slotID);
                if(slot.StackSize == 1)
                {
                    Client.PostPlayerInput(Message.Types.DropInventoryItem, w =>
                    {
                        w.Write(slotID);
                        w.Write(1);
                    });
                    return;// true; true;
                }
                SplitStackWindow.Instance.Show(slot, Player.Actor, (amount) =>
                {
                    Client.PostPlayerInput(Message.Types.DropInventoryItem, w =>
                    {
                        w.Write(slotID);
                        w.Write(amount);
                    });
                });
                //return true;
                //Client.PostPlayerInput(Message.Types.ExecuteScript, w => Script.Write(w, Script.Types.Drop, TargetArgs.Empty));
                Client.PostPlayerInput(Message.Types.DropInventoryItem,  w => w.Write(slotID));// Script.Write(w, Script.Types.Drop, TargetArgs.Empty));
                //return true;
            }));

            this.Components.Values.ToList().ForEach(c => c.GetInventoryContext(Player.Actor, a.Actions, slotID));
            a.Actions.Add(new ContextAction(() => "Inspect", () => this.GetTooltip().ToWindow().Show()));
        }

        //public Dictionary<KeyBinding, Interaction> GetPlayerActions()
        //{
        //    var list = new Dictionary<KeyBinding, Interaction>();
        //    foreach (var item in this.Components)
        //        item.Value.GetPlayerActions(list);
        //    return list;
        //}

        public List<Interaction> GetInteractionsFromSkill(Components.Skills.Skill skill)
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
            if (Player.Actor == null)
                return;
            //Vector3 face = (Vector3)a.Parameters[0];
            //this.Components.Values.ToList().ForEach(c => c.GetClientActions(this, a.Actions));
            foreach(var c in this.Components.Values)
            {
                c.GetClientActions(this, a.Actions);
                //var list = new Dictionary<PlayerInput, Interaction>();
                //c.GetPlayerActionsWorld(this, list);
                //foreach (var i in list)
                //    a.Actions.Add(new ContextAction(i.Key.ToString() + ": " + i.Value.Name, () => true));
            }
            return;

            var actions = new List<ContextAction>();
            var abilities = new List<GameObject>();

            this.Components.Values.ToList().ForEach(c => c.GetContextActions(Player.Actor, abilities));
            actions.Add(new ContextAction(() => "Inspect", () => this.GetTooltip().ToWindow().Show()));
            actions.Add(new ContextAction(() => "Interface", () => this.GetUi().Show()));
            this.Components.Values.ToList().ForEach(c => c.GetClientActions(this, actions));

            // TODO: move this to towns component
            actions.AddRange(from action in this.GetInteractionsList()
                             select new ContextAction(() => action.Name, () =>
                             {
                                 Client.Instance.Send(PacketType.Towns, new Towns.PacketAddJob(Player.Actor.Network.ID, new TargetArgs(this), action.Name).Write());
                                 //Client.PlayerRemoteCall(this, )
                             }));

            a.Actions = actions;
            a.ControlInit = ctrl =>
            {
                //   ctrl.HoverFunc = () => (ctrl.Tag as ContextAction)."";
            };
            //return actions;
        }

        public List<Interaction> GetInteractionsList()
        {
            List<Interaction> list = new List<Interaction>();
            foreach (var item in this.Components)
                item.Value.GetInteractions(this, list);
            return list;
        }
        public Dictionary<string, Interaction> GetInteractions()
        {
            Dictionary<string, Interaction> list = new Dictionary<string, Interaction>();
            foreach(var item in this.GetInteractionsList())
                list.Add(item.Name, item);
            return list;
        }


        public void GetContextActions2(ContextArgs a)
        {
            var actions = new List<ContextAction>();
            if (Player.Actor.IsNull())
                return;
            Vector3 face = (Vector3)a.Parameters[0];
            var interactions = this.Query(Player.Actor);//, a);//Player.Actor, a.Parameters);
            var abilities = new List<GameObject>();
            this.Components.Values.ToList().ForEach(c => c.GetContextActions(Player.Actor, abilities));
            actions.AddRange(interactions.ConvertAll(i => new ContextAction(() => i.Name, () =>
            {
                if (Controller.Input.GetKeyDown(System.Windows.Forms.Keys.LMenu))
                {
                    if (!i.CanBeJob)
                        return;// true; false;
                    JobBoardComponent.Enqueue(Job.Create(i));
                }
                else
                {
                    GameObjectSlot abilitySlot;
                    if (ControlComponent.TryGetAbility(Player.Actor, i.Message, out abilitySlot))
                    {
                        Player.LastAbilityUsed = abilitySlot;
                        ActionBar.Instance.Refresh();
                    }
                    GameObject.PostMessage(Player.Actor, new ObjectEventArgs(Message.Types.BeginInteraction, null, i, face, Player.Actor["Inventory"]["Holding"] as GameObjectSlot));
                }
                //return true;
            })
            {
                //HoverFunc = () =>
                ControlInit = (ContextAction act, Button btn) =>
                {
                    //btn.Controls.Add(new IconButton()
                    //{
                    //    Location = new Vector2(btn.Width, 0),
                    //    Icon = new Icon(UIManager.Icons16x16, 0, 16),
                    //   // Anchor = Vector2.UnitX,
                    //    LeftClickAction = ()=>JobBoardComponent.Enqueue(Job.Create(i)),
                    //    BackgroundTexture = UIManager.Icon16Background,
                    //    ClipToBounds = false
                    //});

                    btn.TooltipFunc = (tooltip) =>
                    {
                        i.GetTooltipInfo(tooltip);
                        if (i.CanBeJob)
                            tooltip.Controls.Add(new Label() { Location = tooltip.Controls.Count > 0 ? tooltip.Controls.Last().BottomLeft : Vector2.Zero, Text = "[Alt]+[LB]: Create Job" });
                    };
                    //{
                    //    string text = "";
                    //    List<InteractionCondition> failed = new List<InteractionCondition>();
                    //    if (i.TryConditions(Player.Actor, failed))
                    //        return;
                    //    if (failed.Count == 0)
                    //        return;
                    //    foreach (InteractionCondition condition in failed)
                    //        text += condition.ErrorMessage + "\n";
                    //    text = text.TrimEnd('\n');
                    //    tooltip.Controls.Add(new Label(text) { TextColorFunc = () => Color.Red });
                    //};
                    btn.TextColorFunc = () =>
                    {
                        List<Condition> failed = new List<Condition>();
                        i.TryConditions(Player.Actor, failed);
                        return failed.Count > 0 ? Color.Red : Color.White;
                    };
                    btn.TintFunc = () => (Controller.Input.GetKeyDown(System.Windows.Forms.Keys.LMenu) && i.CanBeJob) ? Color.Yellow : Color.White;
                }
            }));

            a.Actions = actions;
            a.ControlInit = ctrl =>
            {
                //   ctrl.HoverFunc = () => (ctrl.Tag as ContextAction)."";
            };
            //return actions;
        }

        public List<Script> GetAvailableActions()
        {
            List<Script> list = new List<Script>();
            foreach (var c in this.Components.Values)
                c.GetAvailableActions(list);
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

        //public static GameObject Spawn(GameObject.Types id, Map map, Vector3 global)
        //{
        //    return Create(id).Spawn(map, global);
        //}

        //public GameObject Spawn(Map map, Vector3 global)
        //{
        //    //this.Global = global;
        //    SetGlobal(global);
        //    this.Spawn(map);
        //    return this;
        //}

        //public Action<GameObject, Map, Vector3> OnSpawn = (obj, map, global) => { Chunk.AddObject(obj, map, global); };

        //public GameObject ApplyAffix(IEnumerable<Items.Affix> affixes)
        //{
        //    foreach(var a in affixes)
        //        a.Apply(this);
        //    return this;
        //}
        //public GameObject ApplyAffix(Items.Affix affix)
        //{
        //    return affix.Apply(this);
        //}

        //public GameObject SetName(string name)
        //{
        //    this.Name = name;
        //    return this;
        //}

        //public GameObject Instantiate(IObjectProvider instantiator)
        //{
        //    instantiator.Instantiate(this);
        //    foreach (var comp in this.Components.Values)
        //        comp.Instantiate(instantiator);
        //    return this;
        //}

        public bool Dispose()
        {
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



       
    }
}
