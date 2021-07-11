using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public interface IEntityComp
    {
        void Tick(MapBase map, IEntityCompContainer entity);
        void Draw(Camera camera, MapBase map, Vector3 global);
       
        void Load(SaveTag tag);
        SaveTag Save(string name);
    }
}
