using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.GameModes
{
    public interface IMap
    {
        MapRules Rules { get; }

        Color GetAmbientColor();
        void SetAmbientColor(Color color);
        double GetDayTimeNormal();
        Texture2D GetThumbnail();
        //string Name;
        float LoadProgress { get; }
        TimeSpan Time { get; set; }
        IWorld GetWorld();
        Vector2 GetOffset();
        Vector2 Coordinates { get; }
        string GetName();

        //Dictionary<Vector2, Chunk> ActiveChunks { get; set; }
        Dictionary<Vector2, Chunk> GetActiveChunks();
        bool AddChunk(Chunk chunk);
        List<GameObject> GetObjects();
        IEnumerable<GameObject> GetObjects(Vector3 min, Vector3 max);
        IEnumerable<GameObject> GetObjects(BoundingBox box);

        int GetMaxHeight();
        int GetSizeInChunks();

        void RemoveBlock(Vector3 global);
        BlockEntity RemoveBlockEntity(Vector3 global);
        void AddBlockEntity(Vector3 global, BlockEntity entity);
        BlockEntity GetBlockEntity(Vector3 global);
        int GetHeightmapValue(Vector3 global);
        Block GetBlock(Vector3 global);
        Cell GetCell(Vector3 global);
        Chunk GetChunkAt(Vector2 chunkCoords);
        Chunk GetChunk(Vector3 global);
        bool TryGetBlock(Vector3 global, out Block block);
        bool TryGetCell(Vector3 global, out Cell cell);
        bool TryGetChunk(Vector3 global, out Chunk chunk);
        bool TryGetChunk(int globalx, int globaly, out Chunk chunk);

        bool ChunkExists(Vector2 chunkCoords);
        bool ChunksExist(Vector2 centerChunk, int radius);
        bool ChunkNeighborsExist(Vector2 chunkCoords);

        List<Chunk> GetChunks(Vector2 pos, int radius = 1);

        bool TryGetAll(Vector3 global, out Chunk chunk, out Cell cell);
        bool TryGetAll(Vector3 global, out Chunk chunk, out Cell cell, out Vector3 local);
        bool TryGetAll(int gx, int gy, int gz, out Chunk chunk, out Cell cell, out int lx, out int ly);

        bool IsSolid(Vector3 global);
        bool IsEmpty(Vector3 global);

        List<GameObject> GetEntitiesAround(Vector3 global);

        bool PositionExists(Vector3 global);

        void SetLight(Vector3 global, byte sky, byte block);
        void SetSkyLight(Vector3 global, byte value);
        void SetBlockLight(Vector3 global, byte value);

        void AddSkyLightChanges(Dictionary<Vector3, byte> List);
        void AddBlockLightChanges(Dictionary<Vector3, byte> List);
        void ApplyLightChanges();

        /// <summary>
        /// Vector must be rounded!!!
        /// </summary>
        /// <param name="global">must be rounded!!!</param>
        /// <param name="sun"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        bool GetLight(Vector3 global, out byte sun, out byte block);
        byte GetSkyDarkness();
        byte GetSunLight(Vector3 global);
        byte GetData(Vector3 global);
        byte SetData(Vector3 global, byte data = 0);

        void Update(IObjectProvider net);
        SaveTag Save();

        bool InvalidateCell(Vector3 global);
        bool InvalidateCell(Vector3 global, Cell cell);
        void GenerateThumbnails();
        void GenerateThumbnails(string fullpath);
        void LoadThumbnails();//string fullpath);
        MapThumb GetThumb();
        void Generate();

        void SetNetwork(IObjectProvider net);
        IObjectProvider GetNetwork();
        IObjectProvider Net { get; }

        Towns.Town GetTown();
        Towns.Town Town { get; }

        void GetData(BinaryWriter w);

        string GetFolderName();
        string GetFullPath();

        void UpdateLight(IEnumerable<WorldPosition> positions);

        WorldPosition GetMouseover();
        void SetMouseover(WorldPosition position);

        void DrawBlocks(MySpriteBatch sb, Camera cam, EngineArgs a);
        void DrawObjects(MySpriteBatch sb, Camera cam, SceneState scene);
        void DrawInterface(SpriteBatch sb, Camera cam);
        void DrawWorld(MySpriteBatch sb, Camera cam);

        void GetTooltipInfo(Tooltip tooltip);

        void HandleEvent(GameEvent e);

        bool SetCell(Vector3 global, Block.Types type, byte data, int variation = 0);
        bool SetBlock(Vector3 global, Block.Types type);
        bool SetBlock(Vector3 global, Block.Types type, byte data, int variation = 0);
        bool SetBlockLuminance(Vector3 global, byte luminance);


        void EventOccured(Components.Message.Types types, params object[] p);

        void OnGameEvent(GameEvent e);

        float GetSolidObjectHeight(Vector3 global);
    }
}
