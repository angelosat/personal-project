using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    abstract class PlayerEntity : GameObject
    {
        //public PlayerData Player;
        static public readonly GameObject Entity = GameObjectDb.Actor;
        static public void Initialize()
        {

        }

        static public GameObject Create(SaveTag tag)
        {
            var obj = new GameObject(GameObjectDb.Actor); // GameObjectDb.Actor;

            Dictionary<string, SaveTag> compData = tag["Components"].Value as Dictionary<string, SaveTag>;
            foreach (SaveTag compTag in compData.Values)
            {
                if (compTag.Value == null)
                    continue;
                //if (!obj.Components.ContainsKey(compTag.Name))
                //{
                //    if(obj.AddComponent(Factory.Create(compTag.Name))!=null)
                //        obj[compTag.Name].Load(compTag);//, objectFactory ?? new Action<GameObject>(o => { }));//.Value as List<Tag>);
                //}

                // DONT CREATE COMPONENTS THAT DONT EXIST ON THE TEMPLATE OBJECT
                //if (!obj.Components.ContainsKey(compTag.Name))
                //    obj.AddComponent(Factory.Create(compTag.Name));
                if (obj.Components.ContainsKey(compTag.Name))
                    obj[compTag.Name].Load(compTag);//, objectFactory ?? new Action<GameObject>(o => { }));//.Value as List<Tag>);
            }
            //obj.ObjectCreated(); // UNCOMMENT IF PROBLEMS
            obj.ObjectLoaded();
            return obj;
        }

        public static GameObject Create(BinaryReader reader)
        {
            int type = reader.ReadInt32();
            GameObject obj = new GameObject(GameObjectDb.Actor); // WARNING: must figure out way to reconstruct an object without it's creating a prefab

            int compCount = reader.ReadInt32();
            for (int i = 0; i < compCount; i++)
            {
                string compName = reader.ReadString();
                if (!obj.Components.ContainsKey(compName))
                    obj.AddComponent(Factory.Create(compName));
                obj[compName].Read(reader);
            }
            //obj.ObjectCreated(); // i'll put that only where i need it after  calling reconstruct
            obj.ObjectSynced();
            return obj;
        }

        //public static PlayerEntity Create(BinaryReader reader)
        //{
        //    int type = reader.ReadInt32();
        //    PlayerEntity obj = Create(type); // WARNING: must figure out way to reconstruct an object without it's creating a prefab

        //    int compCount = reader.ReadInt32();
        //    for (int i = 0; i < compCount; i++)
        //    {
        //        string compName = reader.ReadString();
        //        if (!obj.Components.ContainsKey(compName))
        //            obj.AddComponent(Factory.Create(compName));
        //        obj[compName].Read(reader);
        //    }
        //    //obj.ObjectCreated(); // i'll put that only where i need it after  calling reconstruct
        //    obj.ObjectSynced();
        //    return obj;
        //}
    }
}
