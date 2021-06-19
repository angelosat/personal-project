using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.GameModes.StaticMaps
{
    public class StaticWorld : Component, IWorld
    {
        public override string ComponentName
        {
            get { return "World: " + this.Name; }
        }

        public const int Zenith = 14;

        #region Properties
        public bool Lighting { get { return (bool)this["Lighting"]; } set { this["Lighting"] = value; } }
        public int MaxHeight;
        public Block.Types DefaultTile { get; set; }
        public string Name { get { return (string)this["Name"]; } set { this["Name"] = value; } }
        public int Seed { get; set; }
        //{
        //    get { return (int)this["Seed"]; }
        //    set
        //    {
        //        this.Random = new Random(value);
        //        this["Seed"] = value;
        //    }
        //}
    //    public bool Caves { get { return (bool)this["Caves"]; } set { this["Caves"] = value; } }
        public bool Flat { get { return (bool)this["Flat"]; } set { this["Flat"] = value; } }
        public bool Trees { get { return (bool)this["Trees"]; } set { this["Trees"] = value; } }
        public TimeSpan Time { get { return (TimeSpan)this["Time"]; } set { this["Time"] = value; } }
        public Random Random { get { return (Random)this["Random"]; } set { this["Random"] = value; } }
        public MapCollection Maps { get { return (MapCollection)this["Maps"]; } set { this["Maps"] = value; } }
        public SortedSet<Terraformer> Mutators;
        #endregion

        public string GetName()
        {
            return this.Name;
        }
        public IMap GetMap(Vector2 mapCoords)
        {
            //return this.Maps[mapCoords];
            return this.Maps.GetValueOrDefault(mapCoords);
        }
        public Random GetRandom()
        {
            return this.Random;
        }
        public SortedSet<Terraformer> GetMutators()
        {
            return this.Mutators;
        }
        public MapCollection GetMaps()
        {
            return this.Maps;
        }
        public int GetSeed()
        {
            return this.Seed;
        }

        public void Dispose()
        {
            //foreach (var map in Maps)
            //    map.Value.Dispose();
        }

        public byte[] GetSeedArray()
        {
            int Seed = GetProperty<int>("Seed");
            return new byte[]{
                (byte)(Seed >> 24),
                (byte)(Seed >> 16),
                (byte)(Seed >> 8),
                (byte)Seed};
        }

        public StaticWorld()
        {
            this.MaxHeight = StaticMap.MaxHeight;// 128;
            this.DefaultTile = Block.Types.Soil;
            //this.Mutators = new List<Terraformer>();
            this.Mutators = new SortedSet<Terraformer>();
            this.Trees = true;
            this.Maps = new MapCollection();
            //SeaLevel = MaxHeight / 2 + 4; // TODO: what is this?
            Properties["Time"] = new TimeSpan(Zenith, 0, 0);
            
        }

        public StaticWorld(WorldArgs a)
            : this()
        {
            this.Name = a.Name;
            this.Seed = a.Seed;
            //Properties["Trees"] = a.Trees;
            this.Random = new Random(a.Seed);
            Lighting = a.Lighting;
            this.Mutators = a.Mutators;
            this.DefaultTile = a.DefaultTile;
        }

        public StaticWorld(string name, int seed, IEnumerable<Terraformer> mutators, Block.Types defaultTile = Block.Types.Soil)
            : this()
        {
            this.Name = name;
            this.Seed = seed;
            this.Random = new Random(seed);
            this.Mutators = new SortedSet<Terraformer>(mutators);
            this.DefaultTile = defaultTile;
        }

        static public StaticWorld Create(WorldArgs a)
        {
            return new StaticWorld(a);
        }
        public IMap CreateMap(Vector2 mapCoords)
        {
            return new StaticMap(this, mapCoords);
        }

        public override object Clone()
        {
            return this;
        }

        public void GetFileInfo(out string saveDir, out string worldDir, out string worldFile)
        {
            saveDir = GlobalVars.SaveDir + @"/Worlds/";
            worldDir = this.Name + @"/";
            worldFile = this.Name + ".world.sat";
        }

        internal new void Save()
        {
            string directory = GlobalVars.SaveDir + @"/Worlds/Static/" + this["Name"] + "/";
            string worldPath = @"/Saves/Worlds/Static/" + this["Name"] + "/";
            string fullPath = worldPath + this["Name"] + ".world.sat";
            SaveTag tag;
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);

                tag = new SaveTag(SaveTag.Types.Compound, "World");

                tag.Add(new SaveTag(SaveTag.Types.Int, "Seed", this.Seed));
                //tag.Add(new SaveTag(SaveTag.Types.Bool, "Caves", GetProperty<bool>("Caves")));
                //tag.Add(new SaveTag(SaveTag.Types.Bool, "Trees", GetProperty<bool>("Trees")));
              // tag.Add(new Tag(Tag.Types.Bool, "Flat", GetProperty<bool>("Flat")));
                tag.Add(new SaveTag(SaveTag.Types.Double, "Time", Time.TotalSeconds));
                tag.Add(new SaveTag(SaveTag.Types.Int, "DefaultTile", (int)this.DefaultTile));
                SaveTag playerTag = new SaveTag(SaveTag.Types.Compound, "Player");
                if (Player.Actor != null)
                    playerTag.Add(new SaveTag(SaveTag.Types.Compound, Player.Actor.Name, Player.Actor.Save()));
                tag.Add(playerTag);
                tag.Add(new SaveTag(SaveTag.Types.String, "Name", GetProperty<string>("Name")));

                SaveTag mutatorsTag = new SaveTag(SaveTag.Types.List, "Mutators", SaveTag.Types.Compound);
                //this.Mutators.ForEach(m =>
                //{
                //    var mut = new SaveTag(SaveTag.Types.Compound);
                //    mut.Add(new SaveTag(SaveTag.Types.Int, "ID", (int)m.ID));
                //    mut.Add(new SaveTag(SaveTag.Types.Compound, "Data", m.Save()));
                //    mutatorsTag.Add(mut);//new SaveTag(SaveTag.Types.Compound, m.ID.ToString(), m.Save()));
                //});
                foreach(var item in this.Mutators)
                {
                    var mut = new SaveTag(SaveTag.Types.Compound);
                    mut.Add(new SaveTag(SaveTag.Types.Int, "ID", (int)item.ID));
                    mut.Add(new SaveTag(SaveTag.Types.Compound, "Data", item.Save()));
                    mutatorsTag.Add(mut);
                }
                tag.Add(mutatorsTag);
                //tag.Add(new Tag(Tag.Types.String, "Terraformer", this.Terraformer.Name));

                SaveTag mapsTag = new SaveTag(SaveTag.Types.Compound, "Maps");
                foreach (var map in Maps)
                    mapsTag.Add(new SaveTag(SaveTag.Types.String, "Coordinates", map.Value.GetFolderName()));
                tag.Add(mapsTag);

                tag.WriteTo(writer);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                Chunk.Compress(stream, Directory.GetCurrentDirectory() + fullPath);

                stream.Close();
            }
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
            //XDocument xml = Engine.Settings.ToXDocument();
            //return (from node in xml.Descendants() where node.Name == "LastWorld" select node.Value).FirstOrDefault() ?? "";
            return (string)Engine.Config.Descendants("LastWorld").FirstOrDefault();
        }
        static public StaticWorld LoadLastWorld()
        {
            return Load(GetLastWorldName());
        }
        static public StaticWorld Load(string worldName)
        {
            string path = WorldsPath + worldName + "/";
            if(!Directory.Exists(path))
                return null;
            return Load(new DirectoryInfo(path));
        }
        static public StaticWorld Load(DirectoryInfo worldFolder) //string filename)
        {
            //Initialize();
            StaticWorld world = new StaticWorld(new WorldArgs());
            DateTime start = DateTime.Now;
            string directory = GlobalVars.SaveDir;// Directory.GetCurrentDirectory() + @"/Saves/";
            //using (FileStream stream = new FileStream(directory + filename, System.IO.FileMode.Open))
            FileInfo[] worldFiles = worldFolder.GetFiles("*.world.sat", SearchOption.TopDirectoryOnly);
            if (worldFiles.Length == 0)
            {
                //throw (new Exception("World file missing!"));
                return null;
            }
            FileInfo worldSave = worldFiles.First();
            string filename = worldSave.FullName;
            using (FileStream stream = new FileStream(filename, System.IO.FileMode.Open))
            {

                using (MemoryStream decompressedStream = Chunk.Decompress(stream))
                {
                    BinaryReader reader = new BinaryReader(decompressedStream); //stream);//
                    SaveTag worldTag = SaveTag.Read(reader);


                    world["Seed"] = (int)worldTag["Seed"].Value;
                    world.Seed = (int)worldTag["Seed"].Value;
                    SaveTag flatTag;
                    if (worldTag.TryGetTag("Flat", out flatTag))
                        world["Flat"] = (bool)flatTag.Value;
                    //world["Caves"] = (bool)worldTag["Caves"].Value;
                    //world["Trees"] = (bool)worldTag["Trees"].Value;
                    world["Time"] = TimeSpan.FromSeconds((double)worldTag["Time"].Value);
                    world["DefaultTile"] = (Block.Types)worldTag.TagValueOrDefault<int>("DefaultTile", (int)Block.Types.Soil);
                    SaveTag playerTag = worldTag["Player"] as SaveTag;
                    //List<SaveTag> tagList = playerTag.Value as List<SaveTag>;
                    Dictionary<string, SaveTag> tagList = playerTag.Value as Dictionary<string, SaveTag>;
                    if (tagList.Count > 1)
                    {
                        GameObject player;
                        Dictionary<string, SaveTag> byName = playerTag.Value as Dictionary<string, SaveTag>;//.ToDictionary();
                        player = GameObject.Load(byName.First().Value);
                        player.Name = byName.First().Key;
                        world["Player"] = player;
                    }

                    world["Name"] = (string)worldTag["Name"].Value;

                    SaveTag mutatorsTag;
                    if (!worldTag.TryGetTag("Mutators", out mutatorsTag))
                        world.Mutators.Add(Terraformer.Land);
                    else
                    {
                        List<SaveTag> mutators = mutatorsTag.Value as List<SaveTag>;
                        foreach (var t in mutators)
                            //world.Mutators.Add((Terraformer.Dictionary[(Terraformer.Types)int.Parse(t.Name)].Clone() as Terraformer).Load(t.Value as SaveTag));
                        {
                            Terraformer.Types id = (Terraformer.Types)(int)t["ID"].Value;
                            SaveTag s = t["Data"] as SaveTag;
                            var newterra = Terraformer.Dictionary[id].Clone() as Terraformer;
                            world.Mutators.Add(newterra.Load(s));
                        }
                        //foreach (var t in mutators.Take(mutators.Count - 1))
                        //    world.Mutators.Add(Terraformer.All.Find(foo => foo.Name == t.Name));
                    }

                    foreach (var mapdir in worldFolder.GetDirectories("*.*", SearchOption.TopDirectoryOnly))
                    {
                        string[] c = mapdir.Name.Split('.');
                        Vector2 coords = new Vector2(Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
                        FileInfo mapFile = mapdir.GetFiles("*.map.sat").FirstOrDefault();
                        StaticMap map;
                        if (mapFile.IsNull())
                        {
                            map = StaticMap.Create(world, coords);
                            map.Save();
                        }
                        else
                        {
                            map = StaticMap.Load(mapdir, world, coords);
                            world.GetMaps().Add(coords, map);
                        }
                    }
                }

                CreateExpansionMaps(world);

                stream.Close();
            }

            //Console.WriteLine("world loaded in " + (DateTime.Now - start).ToString() + " ms");
            return world;
        }

        static public StaticWorld LoadWorld(string worldFilename)
        {
            DirectoryInfo worldDir = new DirectoryInfo(GlobalVars.SaveDir + "/Worlds/" + worldFilename);
            if (!Directory.Exists(worldDir.FullName))
                return null;

            FileInfo[] worldFiles = worldDir.GetFiles("*.world.sat", SearchOption.TopDirectoryOnly);
            if (worldFiles.Length == 0)
                return null;

            FileInfo worldSave = worldFiles.First();
            return StaticWorld.Load(worldDir);
        }

        public static void CreateExpansionMaps(StaticWorld world)
        {
            (new List<Vector2>() { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) }).ForEach(n =>
            {
                //world.Maps.ToList().ForEach(map =>
                foreach (var mapdir in world.GetDirectory().GetDirectories("*.*", SearchOption.TopDirectoryOnly))
                {
                    string[] c = mapdir.Name.Split('.');
                    Vector2 coords = new Vector2(Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
                    var newPos = coords + n;
                    //var newPos = map.Key + n;
                    if (!world.Maps.ContainsKey(newPos))
                        StaticMap.Create(world, newPos);
                    //world.Maps.Add(newPos, Map.Create(world, newPos));
                }
                //);
            });
        }

        void LoadMaps(DirectoryInfo[] mapDirs)
        {
            foreach (DirectoryInfo dir in mapDirs)
            {
                string[] c = dir.Name.Split('.');
                Vector2 coords = new Vector2(Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
                FileInfo mapFile = dir.GetFiles("*.map.sat").First();
                this.Maps.Add(coords, StaticMap.Load(dir, this, coords));//dir.FullName + @"\" + mapFile.Name, this, coords));
            }
        }

        public void Draw(SpriteBatch sb, Camera cam)
        {
            foreach (var map in this.Maps)//.OrderBy(m=>-m.Value.Global.X - m.Value.Global.Y))
            {
                map.Value.GetThumb().Draw(sb, cam);
            }
        }

        public void WriteData(BinaryWriter w)
        {
            w.Write(this.Name);
            w.Write(this.Seed);
            //writer.Write(this.SeaLevel);
            w.Write(this.Trees);
            w.Write((byte)this.DefaultTile);
            w.Write(this.Mutators.Count);
            //this.Mutators.ForEach(m =>
            //{
            //    w.Write((int)m.ID);
            //    m.Write(w);
            //});
            foreach(var m in this.Mutators)
            {
                w.Write((int)m.ID);
                m.Write(w);
            }
        }
        static public StaticWorld ReadData(BinaryReader r)
        {
            StaticWorld world = new StaticWorld();
            world.Name = r.ReadString();
            world.Seed = r.ReadInt32();
            //world.SeaLevel = reader.ReadInt32();
            world.Trees = r.ReadBoolean();
            world.DefaultTile = (Block.Types)r.ReadByte();
            int mutatorCount = r.ReadInt32();
            for (int i = 0; i < mutatorCount; i++)
            {
                Terraformer.Types id = (Terraformer.Types)r.ReadInt32();
                Terraformer terra = Terraformer.Dictionary[id].Clone() as Terraformer;
                terra.Read(r);
                world.Mutators.Add(terra);
                //world.Mutators.Add(Terraformer.All.Find(foo => foo.Name == reader.ReadString()));
            }
          
            return world;
        }
    }
}
