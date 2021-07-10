using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    abstract public partial class Block
    {
        public class DefaultState : IBlockState
        {
            public void Apply(MapBase map, Vector3 global)
            { }
            public void Apply(ref byte data)
            {
            }
            public void Apply(Block.Data data)
            {
            }
            public void FromCraftingReagent(GameObject item) { }
            public Color GetTint(byte d)
            { return Color.White; }
            public string GetName(byte d)
            {
                return "";
            }
        }
    }
}
