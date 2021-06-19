using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.GameModes;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Terraforming.Mutators
{
    public class MutatorProperty
    {
        public string Name { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        float _Value;
        public float Value { get { return _Value; }
            set
            {
                _Value = Math.Max(0, Math.Min(Map.MaxHeight, value));
            } }
        public float Step { get; set; }
        public MutatorProperty(string name, float value, float min, float max, float step = 1)
        {
            if (step <= 0)
                throw new ArgumentException();
            this.Name = name;
            this.Min = min;
            this.Max = max;
            this.Value = value;
            this.Step = step;
        }
    }
    class Sea : Terraformer
    {
        int _SeaLevel = Map.MaxHeight / 2;
        public int SeaLevel
        {
            get { return _SeaLevel; }
            set
            {
                _SeaLevel = Math.Max(0, Math.Min(Map.MaxHeight, value));
            }
        }

        public Sea()
        {
            this.ID = Terraformer.Types.Sea;
            this.Name = "Sea";
        }
        //public override void Initialize(IWorld w, Cell c, int x, int y, int z, double g)
        //{
        //    if (z > this.SeaLevel)
        //        return;
        //    if (c.Block == Block.Air)
        //        c.Block = Block.Water;
        //    else
        //        if (z == this.SeaLevel)
        //            c.Block = Block.Sand;
        //}
        public override void Finally(Chunk chunk)
        {
            foreach(var c in chunk.CellGrid2)
            {
                var z = c.Z;
                if (z > this.SeaLevel)
                    continue;
                else if (z == this.SeaLevel)
                {
                    if (c.Block == Block.Air)
                        c.Block = Block.Water;
                    else
                        c.Block = Block.Sand;
                }
                else
                {
                    if (c.Block == Block.Air)
                    {
                        c.Block = Block.Water;
                        c.BlockData = BlockWater.GetData(1);
                    }
                }
            }
        }
        public override Block.Types Initialize(IWorld w, Cell c, int x, int y, int z, Net.RandomThreaded r)
        {
            Block.Types type;
            if (z > this.SeaLevel)// world.SeaLevel)
                return Block.Types.Air;

            return w.DefaultTile;
        }
        public override List<MutatorProperty> GetAdjustableParameters()
        {
            List<MutatorProperty> list = new List<MutatorProperty>();
            //list.Add(new MutatorProperty("Sea Level", 0, Map.MaxHeight, Map.MaxHeight / 2));
            list.Add(new MutatorProperty("Sea Level", Map.MaxHeight / 2, 0, Map.MaxHeight, 1));
            return list;
        }

        public override List<SaveTag> Save()
        {
            List<SaveTag> tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.Int, "Level", this.SeaLevel));
            return tag;
        }
        public override Terraformer Load(SaveTag save)
        {
            this.SeaLevel = save.TagValueOrDefault<int>("Level", Map.MaxHeight / 2 - 1);
            return this;
        }

        public override object Clone()
        {
            return new Sea();
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.SeaLevel);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.SeaLevel = r.ReadInt32();
        }
    }
}
