using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.GameModes
{
    public interface IWorld
    {
        //GameMode GameMode { get; }
        string GetName();
        string GetPath();
        IMap GetMap(Vector2 mapCoords);
        Random GetRandom();
        int GetSeed();
        int Seed { get; }

        byte[] GetSeedArray();

        Block.Types DefaultTile { get; set; }
        SortedSet<Terraformer> GetMutators();

        void WriteData(BinaryWriter w);

        MapCollection GetMaps();

        void Draw(SpriteBatch sb, Camera cam);
    }
}
