using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.GameModes.StaticMaps
{
    public class StaticWorld : IWorld
    {
        static class Packets
        {
            static public readonly int PacketClockAdvanced;
            static Packets()
            {
                PacketClockAdvanced = Network.RegisterPacketHandler(ReceiveClockAdvanced);
            }

            private static void ReceiveClockAdvanced(INetwork net, BinaryReader r)
            {
                if (net is Server)
                    throw new Exception();
                net.Map.World.CurrentTick++;
            }
        }
        public float Gravity => -0.015f;//-0.04f;// -0.05f; //35f;
        public const int Zenith = 14;
       
        public bool Lighting;
        public int MaxHeight { get; set; }
        public Block DefaultBlock { get; set; }
        string _Name;
        public string Name { get => this._Name; set => this._Name = value; }
        public int Seed { get; set; }
        //public const int TickTime = (int)(60 * 1.44f);
        public bool Flat;
        public bool Trees;
        TimeSpan ClockOffset = TimeSpan.FromHours(12);
        public TimeSpan Clock => ClockOffset + TimeSpan.FromMilliseconds((double)this.CurrentTick * Ticks.IngameMillisecondsPerTick);
        public Random Random { get; set; }
        public MapCollection Maps;
        public StaticMap Map { get { return this.Maps.Values.First() as StaticMap; } }
        public PopulationManager Population => this.PopulationManager;
        public List<Terraformer> Mutators { get; }
        ulong currentTick;
        public ulong CurrentTick { get => this.currentTick; set => this.currentTick = value; }

        public INetwork Net { get; set; }

        PopulationManager PopulationManager;
        public void Tick(INetwork net)
        {
            this.PopulationManager.Update(net);
            this.CurrentTick++;
        }
        public string GetName()
        {
            return this.Name;
        }
        public MapBase GetMap(Vector2 mapCoords)
        {
            return this.Maps.GetValueOrDefault(mapCoords);
        }
        public Random GetRandom()
        {
            return this.Random;
        }
  
        public MapCollection GetMaps()
        {
            return this.Maps;
        }
        public int GetSeed()
        {
            return this.Seed;
        }

        public byte[] GetSeedArray()
        {
            return new byte[]{
                (byte)(this.Seed >> 24),
                (byte)(this.Seed >> 16),
                (byte)(this.Seed >> 8),
                (byte)this.Seed};
        }

        StaticWorld()
        {
            this.MaxHeight = 128;
            this.DefaultBlock = BlockDefOf.Soil;
            this.Mutators = new List<Terraformer>();
            this.Trees = true;
            this.Maps = new MapCollection();
            this.PopulationManager = new PopulationManager(this);
        }
        public StaticWorld(string name, IEnumerable<Terraformer> mutators)
           : this()
        {
            if (name.IsNullEmptyOrWhiteSpace())
                throw new ArgumentNullException();
            this.Name = name;
            this.Seed = name.GetHashCode();
            this.Random = new Random(this.Seed);
            this.Mutators = new List<Terraformer>(mutators);
            this.DefaultBlock = BlockDefOf.Soil;
        }
        //public StaticWorld(string name, string seedString, IEnumerable<Terraformer> mutators)
        //    : this()
        //{
        //    this.SeedString = seedString;
        //    if (seedString.IsNullEmptyOrWhiteSpace())
        //        seedString = Path.GetRandomFileName().Replace(".", "");
        //    this.Name = name;
        //    if (!int.TryParse(seedString, out var seed))
        //        seed = seedString.Length > 0 ? seedString.GetHashCode() : new Random().Next(int.MinValue, int.MaxValue);
        //    this.Seed = seed;
        //    this.Random = new Random(seed);
        //    this.Mutators = new SortedSet<Terraformer>(mutators);
        //    this.DefaultBlock = BlockDefOf.Soil;
        //}
        public StaticWorld(SaveTag save)
            : this()
        {
            this.Name = (string)save["Name"].Value;
            this.Seed = (int)save["Seed"].Value;
            //this.SeedString = (string)save["SeedString"].Value;
            save.TryGetTagValue<int>("RandomState", v =>
            {
                this.Random = new Random(v);
            });
            if (save.TryGetTag("Flat", out SaveTag flatTag))
                this.Flat = (bool)flatTag.Value;
            save.TryGetTagValue<double>("CurrentTick", v => this.CurrentTick = (ulong)v);

            //this.DefaultBlock = (Block.Types)save.TagValueOrDefault<int>("DefaultTile", (int)Block.Types.Soil);
            if (!save.TryGetTagValue<int>("DefaultBlock", v => { this.DefaultBlock = Block.GetBlock(v); }))
                this.DefaultBlock = BlockDefOf.Soil;
            this.Name = (string)save["Name"].Value;

            if (!save.TryGetTag("Mutators", out SaveTag mutatorsTag))
                this.Mutators.Add(Terraformer.Land);
            else
            {
                List<SaveTag> mutators = mutatorsTag.Value as List<SaveTag>;
                foreach (var t in mutators)
                {
                    Terraformer.Types id = (Terraformer.Types)(int)t["ID"].Value;
                    SaveTag s = t["Data"];
                    var newterra = Terraformer.Dictionary[id].Clone() as Terraformer;
                    this.Mutators.Add(newterra.Load(s));
                }
            }
            this.Population.TryLoad(save, "Population");

            var mapsList = save["Maps"].Value as List<SaveTag>;
            foreach (var tag in mapsList)
            {
                var map = StaticMap.Load(this, Vector2.Zero, tag);
                this.Maps.Add(map.Coordinates, map);
            }
        }
        public StaticWorld(BinaryReader r)
           : this()
        {
            this.Name = r.ReadString();
            //this.SeedString = r.ReadString();
            this.Seed = r.ReadInt32();
            this.CurrentTick = r.ReadUInt64();
            this.Trees = r.ReadBoolean();
            this.DefaultBlock = Block.GetBlock(r.ReadInt32());
            int mutatorCount = r.ReadInt32();
            for (int i = 0; i < mutatorCount; i++)
            {
                Terraformer.Types id = (Terraformer.Types)r.ReadInt32();
                Terraformer terra = Terraformer.Dictionary[id].Clone() as Terraformer;
                terra.Read(r);
                this.Mutators.Add(terra);
            }
            this.Population.Read(r);
        }
        public MapBase CreateMap(Vector2 mapCoords)
        {
            return new StaticMap(this, mapCoords);
        }

        public void GetFileInfo(out string saveDir, out string worldDir, out string worldFile)
        {
            saveDir = GlobalVars.SaveDir + @"/Worlds/";
            worldDir = this.Name + @"/";
            worldFile = this.Name + ".world.sat";
        }

        internal SaveTag SaveToTag()
        {
            var tag = new SaveTag(SaveTag.Types.Compound, "World");

            tag.Add(new SaveTag(SaveTag.Types.Int, "Seed", this.Seed));
            //this.SeedString.Save(tag, "SeedString");
            var currentRandomState = this.Random.Next();
            this.Random = new Random(currentRandomState);
            tag.Add(new SaveTag(SaveTag.Types.Int, "RandomState", currentRandomState));
            tag.Add(new SaveTag(SaveTag.Types.Double, "Time", Clock.TotalSeconds));
            this.CurrentTick.Save(tag, "CurrentTick");
            this.DefaultBlock.Hash.Save(tag, "DefaultBlock");
            this.Name.Save(tag, "Name");
            this.Population.Save(tag, "Population");
            var mutatorsTag = new SaveTag(SaveTag.Types.List, "Mutators", SaveTag.Types.Compound);
            foreach (var item in this.Mutators)
            {
                var mut = new SaveTag(SaveTag.Types.Compound);
                mut.Add(new SaveTag(SaveTag.Types.Int, "ID", (int)item.ID));
                mut.Add(new SaveTag(SaveTag.Types.Compound, "Data", item.Save()));
                mutatorsTag.Add(mut);
            }
            tag.Add(mutatorsTag);

            var mapsTag = new SaveTag(SaveTag.Types.List, "Maps", SaveTag.Types.Compound);
            foreach (var map in Maps.Values)
                mapsTag.Add(map.Save());
            tag.Add(mapsTag);
            return tag;
        }
     
        public string GetPath()
        {
            return WorldsPath + this.Name + "/";
        }
        static string WorldsPath = GlobalVars.SaveDir + "/Worlds/Static/";
        static public DirectoryInfo[] GetWorlds()
        {
            DirectoryInfo directory = new DirectoryInfo(WorldsPath);
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);
            return directory.GetDirectories();
        }

        public DirectoryInfo GetDirectory()
        {
            return new DirectoryInfo(GlobalVars.SaveDir + @"\Worlds\Static\" + this.Name + @"\");
        }

        static public string GetLastWorldName()
        {
            return (string)Engine.Config.Descendants("LastWorld").FirstOrDefault();
        }
       
        public void ResolveReferences()
        {
            this.Population.ResolveReferences();
        }

        public static void CreateExpansionMaps(StaticWorld world)
        {
            (new List<Vector2>() { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) }).ForEach(n =>
            {
                foreach (var mapdir in world.GetDirectory().GetDirectories("*.*", SearchOption.TopDirectoryOnly))
                {
                    string[] c = mapdir.Name.Split('.');
                    Vector2 coords = new Vector2(Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
                    var newPos = coords + n;
                    if (!world.Maps.ContainsKey(newPos))
                        StaticMap.Create(world, newPos);
                }
            });
        }
        
        public void Draw(SpriteBatch sb, Camera cam)
        {
            foreach (var map in this.Maps)
            {
                map.Value.GetThumb().Draw(sb, cam);
            }
        }

        public void WriteData(BinaryWriter w)
        {
            w.Write(this.Name);
            //w.Write(this.SeedString);
            w.Write(this.Seed);
            w.Write(this.CurrentTick);
            w.Write(this.Trees);
            w.Write(this.DefaultBlock.Hash);
            w.Write(this.Mutators.Count);
            foreach(var m in this.Mutators)
            {
                w.Write((int)m.ID);
                m.Write(w);
            }
            this.Population.Write(w);
        }
       
       
        public void OnHudCreated(Hud hud)
        {
            var win = new Window(this.CreateUI()) { Movable = true, Closable = true };
            win.AutoSize = true;
            hud.AddButton(new IconButton()
            {
                HoverFunc = () => "World",
                LeftClickAction = () =>
                {
                    win.Toggle();
                },
            });
            this.Map.OnHudCreated(hud);
        }

        GroupBox CreateUI()
        {
            var box = new GroupBox();
            var winPop = new Lazy<Window>(() => new Window(this.PopulationManager.Gui) { Title = "Population", Movable = true, Closable = true });
            var btnPop = new Button("Population").SetLeftClickAction(b => winPop.Value.Toggle());
            box.AddControls(btnPop);
            return box;
        }

        public void OnTargetSelected(IUISelection info, ISelectable selected)
        {
            this.PopulationManager.OnTargetSelected(info, selected);
        }

        static string[] NameFirst = {"glory", "thunder", "realm", "world", "city", "town", "far", "outer", "rim", "border",
            "land", "ville", "honor", "elder", "rock", "stone", "wood", "gold", "silver", "iron", "vale", "srping", "lake" };
        public static string GetRandomName()
        {
            var rand = new Random();
            var name = NameFirst.SelectRandom(rand) + NameFirst.SelectRandom(rand);
            return name.First().ToString().ToUpper() + name.Substring(1);
        }
    }
}
