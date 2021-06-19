using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.Items
{
    class ItemFactory
    {
        static List<Prefix> _Prefixes;
        static List<Suffix> _Suffixes;
        
        static List<Prefix> Prefixes
        {
            get
            {
                if (_Prefixes.IsNull())
                {
                    _Prefixes = new List<Prefix>() {
                        Prefix.Create(Affix.Types.Tremendous, "Tremendous"), 
                        Prefix.Create(Affix.Types.Magnificent, "Magnificent"), 
                        Prefix.Create(Affix.Types.Unbreakable, "Unbreakable"), 
                        Prefix.Create(Affix.Types.Furious, "Furious"),
                        Prefix.Create(Affix.Types.Durable, "Durable", o=> ItemComponent.Modify(o, (level, current) => current * (1 + 0.1f * level))),
                    };
                }
                return _Prefixes;
            }
        }
        static List<Suffix> Suffixes
        {
            get
            {
                if (_Suffixes.IsNull())
                {
                    _Suffixes = new List<Suffix>() {
                        Suffix.Create(Affix.Types.Slaying, "Slaying"),
                        Suffix.Create(Affix.Types.Doom, "Doom"), 
                        Suffix.Create(Affix.Types.Fire, "Fire"), 
                        Suffix.Create(Affix.Types.MillionTruths, "A Million Truths"),
                        Suffix.Create(Affix.Types.Shoveling, "Shoveling", o=>EquipComponent.Add(o, Tuple.Create(Stat.Types.Digging, 0.1f * ItemComponent.GetLevel(o))))
                    };
                }
                return _Suffixes;
            }
        }

        static List<Affix> _Registry;
        static List<Affix> Registry
        {
            get
            {
                if (_Registry.IsNull())
                {
                    _Registry = new List<Affix>() {
                        Prefix.Create(Affix.Types.Tremendous, "Tremendous"), 
                        Prefix.Create(Affix.Types.Magnificent, "Magnificent"), 
                        Prefix.Create(Affix.Types.Unbreakable, "Unbreakable"), 
                        Prefix.Create(Affix.Types.Furious, "Furious"),
                        Prefix.Create(Affix.Types.Durable, "Durable", o=> ItemComponent.Modify(o, (level, current) => current * (1 + 0.1f * level))),

                        Suffix.Create(Affix.Types.Slaying, "Slaying"),
                        Suffix.Create(Affix.Types.Doom, "Doom"), 
                        Suffix.Create(Affix.Types.Fire, "Fire"), 
                        Suffix.Create(Affix.Types.MillionTruths, "A Million Truths"),
                        Suffix.Create(Affix.Types.Shoveling, "Shoveling", o=>EquipComponent.Add(o, Tuple.Create(Stat.Types.Digging, 0.1f * ItemComponent.GetLevel(o))))
                    };
                }
                return _Registry;
            }
        }

        List<GameObject.Types> BaseItems = new List<GameObject.Types>() { GameObject.Types.Shovel, GameObject.Types.Axe, GameObject.Types.Pickaxe, GameObject.Types.Hoe, GameObject.Types.Sword };
        static public Prefix GetPrefix(RandomThreaded random)
        {
            return Prefixes[random.Next(Prefixes.Count)];
        }
        static public Suffix GetSuffix(RandomThreaded random)
        {
            return Suffixes[random.Next(Suffixes.Count)];
        }
        static public Prefix GetPrefix(Affix.Types id)
        {
            return Prefixes.Find(f => f.ID == id);
        }
        static public Suffix GetSuffix(Affix.Types id)
        {
            return Suffixes.Find(f => f.ID == id);
        }
        //void Load()
        //{
        //    _Affixes = new List<Affix>()
        //    {
        //        new Affix("Doom"),
        //        new Affix("Death"),
        //        new Affix("Slaying"),
        //        new Affix("Strength"),
        //        new Affix("A Million Truths"),
        //    };
        //}

        //List<Affix> _Affixes;
        //List<Affix> Affixes
        //{
        //    get
        //    {
        //        if (_Affixes.IsNull())
        //            Load();
        //        return _Affixes;
        //    }
        //}

        GameObject.Types GetType(RandomThreaded random)
        {
            return BaseItems[random.Next(BaseItems.Count)];
        }

        //Affix GetAffix()
        //{
        //    return Affixes[Engine.Random.Next(Affixes.Count - 1)];
        //}

        public GameObject Generate(RandomThreaded random)
        {
            GameObject obj = GameObject.Create(GetType(random));
           // obj.Name = GetPrefix() + " " + obj.Name + " of " + GetSuffix();
            //return obj.ApplyAffix(GetPrefix()).ApplyAffix(GetSuffix());
            return GameObject
                .Create(GetType(random))
                .ApplyAffix(GetPrefix(random))
                .ApplyAffix(GetSuffix(random));
        }

        static public GameObject Apply(GameObject obj, params Affix.Types[] affixes)
        {
            affixes.ToList().ForEach(id => Registry.Find(a => a.ID == id).Apply(obj));
            return obj;
        }
    }
}
