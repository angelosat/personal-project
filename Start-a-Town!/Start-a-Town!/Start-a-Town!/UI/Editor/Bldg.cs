using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Editor
{
    class Bldg
    {
        public string Name { get; set; }
        public Stack<CellOperation> Operations = new Stack<CellOperation>();
        public Vector3 Origin { get; set; }

        public void Apply(IMap map, Vector3 global)
        {
            int airTiles = 0;
            foreach (var op in Operations)
            {
                if (op.Type == Block.Types.Air)
                {
                    airTiles += 1;
                    continue;
                }

                // TEMPORARY WORKAROUND
                if (op.Type == Block.Types.EditorAir)
                    op.Type = Block.Types.Air;

                op.Perform(map, global - Origin);
            }
            if (airTiles > 0)
                Log.Enqueue(Log.EntryTypes.System, airTiles.ToString() + " air cell(s) omitted.");
        }

        public Bldg Load(FileInfo fileInfo)
        {
            Operations = new Stack<CellOperation>();
            using (FileStream stream = new FileStream(fileInfo.FullName, System.IO.FileMode.Open))
            {
                using (MemoryStream decompressedStream = Chunk.Decompress(stream))
                {
                    BinaryReader reader = new BinaryReader(decompressedStream);
                    SaveTag mapTag = Start_a_Town_.SaveTag.Read(reader);
                    List<SaveTag> opsTag = mapTag["Operations"].Value as List<SaveTag>;
                    this.Name = mapTag.TagValueOrDefault("Name", fileInfo.Name.Replace(".bldg.sat", ""));

                    foreach (var t in opsTag.Take(opsTag.Count - 1))
                        Operations.Push(CellOperation.Load(null, t));

                    //SaveTag originTag;
                    //if (mapTag.TryGetTag("Origin", out originTag))
                    //    Origin = originTag.ToVector3(); //Origin.Load(originTag);
                    //else
                    //    Origin = Operations.ElementAtOrDefault(0).Global; // TODO: will bug
                    Origin = mapTag.TagOrDefault("Origin", foo => foo.LoadVector3(), Operations.ElementAtOrDefault(0).Global);
                }

                stream.Close();
            }
            return this;
        }

        public Dictionary<Vector3, Block.Types> ToDictionary()
        {
            Dictionary<Vector3, Block.Types> dic = new Dictionary<Vector3, Block.Types>();
            foreach (var op in Operations)
                dic[op.Global - Origin] = op.Type;
            return dic;
        }

        // TODO:
        //public RenderTarget2D GenerateThumbnail()
        //{

        //}
    }
}
