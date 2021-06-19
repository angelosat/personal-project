using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components.GameObjectTemplates;

namespace Start_a_Town_.Components
{ 
    delegate GameObject GameObjectConstructor();
    class GameObjectManager
    {

        //Dictionary<GameObjectTemplate.Types, GameObjectTemplate> Templates;
        Dictionary<GameObjectTemplate.Types, GameObjectConstructor> Templates;
        static GameObjectManager _Instance;
        static GameObjectManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new GameObjectManager();
                return _Instance;
            }
        }

        GameObjectManager()
        {
            LoadTemplates();
        }

        void LoadTemplates()
        {
            Templates = new Dictionary<GameObjectTemplate.Types, GameObjectConstructor>(){
                {GameObjectTemplate.Types.Person, Person.Create}};
        }

        static public GameObject Create(GameObjectTemplate.Types type)
        {
            //return Instance.Templates[type].Create();
            return Instance.Templates[type]();
        }
    }
}
