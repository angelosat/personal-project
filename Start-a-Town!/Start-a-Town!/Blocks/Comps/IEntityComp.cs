using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public interface IEntityComp
    {
        void Tick(IObjectProvider net, IEntityCompContainer entity);
        //void OnEntitySpawn(IEntityCompContainer entity, Vector3 global);
        void Draw(Camera camera, IMap map, Vector3 global);
       
        void Load(SaveTag tag);
        SaveTag Save(string name);
    }
}
