using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public abstract class WorldBase : Inspectable
    {
        public string Name { get; set; }
        public override string Label => this.Name;
        public abstract MapBase GetMap(Vector2 mapCoords);
        public Random Random { get; set; }
        public virtual float Gravity { get; }
        public int Seed { get; set; }
        public int MaxHeight { get; set;  }
        public virtual ulong CurrentTick { get; set; }
        public virtual TimeSpan Clock { get; }
        public INetwork Net { get; set; }

        public byte[] SeedArray { get; set; }

        public virtual Block DefaultBlock { get; set; }
        public virtual PopulationManager Population { get; }

        public virtual List<Terraformer> Mutators { get; set; }

        public abstract void WriteData(BinaryWriter w);

        public abstract MapCollection GetMaps();

        public abstract void Draw(SpriteBatch sb, Camera cam);
        public abstract void Tick(INetwork net);
        public abstract void OnHudCreated(Hud hud);
        public abstract void OnTargetSelected(IUISelection info, ISelectable selection);

        public abstract void ResolveReferences();
    }

    //public interface IWorld
    //{
    //    string Name { get; set; }
    //    //string SeedString { get; set; }
    //    string GetName();
    //    string GetPath();
    //    MapBase GetMap(Vector2 mapCoords);
    //    Random Random { get; set; }
    //    int GetSeed();
    //    float Gravity { get; }
    //    int Seed { get; }
    //    int MaxHeight { get; }
    //    ulong CurrentTick { get; set; }
    //    TimeSpan Clock { get; }
    //    INetwork Net { get; set; }

    //    byte[] GetSeedArray();

    //    Block DefaultBlock { get; set; }
    //    PopulationManager Population { get; }

    //    List<Terraformer> Mutators { get; }

    //    void WriteData(BinaryWriter w);

    //    MapCollection GetMaps();

    //    void Draw(SpriteBatch sb, Camera cam);
    //    void Tick(INetwork net);
    //    void OnHudCreated(Hud hud);
    //    void OnTargetSelected(IUISelection info, ISelectable selection);

    //    void ResolveReferences();

    //}
}
