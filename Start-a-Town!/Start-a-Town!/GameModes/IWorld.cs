﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.GameModes
{
    public interface IWorld
    {
        string Name { get; set; }
        string GetName();
        string GetPath();
        IMap GetMap(Vector2 mapCoords);
        Random Random { get; set; }
        int GetSeed();
        int Seed { get; }
        int MaxHeight { get; }
        ulong CurrentTick { get; set; }
        TimeSpan Clock { get; }
        byte[] GetSeedArray();

        Block.Types DefaultTile { get; set; }
        PopulationManager Population { get; }

        SortedSet<Terraformer> GetMutators();

        void WriteData(BinaryWriter w);

        MapCollection GetMaps();

        void Draw(SpriteBatch sb, Camera cam);
        void Tick(IObjectProvider net);
        void OnHudCreated(Hud hud);
        void OnTargetSelected(IUISelection info, ISelectable selection);

        void ResolveReferences();
    }
}
