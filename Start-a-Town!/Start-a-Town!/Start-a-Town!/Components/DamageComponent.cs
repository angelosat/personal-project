using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class DamageCollection : Dictionary<Damage.Types, Damage>
    {
        public DamageCollection(params Damage[] damages)
        {
            for (int i = 0; i < damages.Length; i++)
            {
                Damage dmg = damages[i];
                Add(dmg.Type.ID, dmg);
            }
        }
    }
    //class DamageTypeCollection : Dictionary<string, Stat>
    //{
    //    //public void Add(params DamageType[] dmgTypes)
    //    //{
    //    //    foreach (DamageType dmgType in dmgTypes)
    //    //        Add(dmgType.ID, dmgType);
    //    //}

    //    //static public DamageTypeCollection Default
    //    //{
    //    //    get
    //    //    {
    //    //        return new DamageTypeCollection() { 
    //    //        {new DamageType(Damage.Types.Chop, "Chop") }, 
    //    //        {new DamageType(Damage.Types.Blunt, "Blunt") },
    //    //        {new DamageType(Damage.Types.Slash, "Slash") },
    //    //        {new DamageType(Damage.Types.Pierce, "Pierce") }
    //    //    };
    //    //    }
    //    //}

    //    public void Add(Stat dmgType)
    //    {
    //        base.Add(dmgType.Name, dmgType);
    //    }
    //}

    public class Damage
    {
        public enum Types { Chop, Blunt, Slash, Pierce };
        public DamageType Type;
        public float Value;
        public Damage(DamageType type, float value)
        {
            Type = type;
            Value = value;
        }

        static List<Stat> _DamageTypes;
        static public List<Stat> DamageTypes
        {
            get
            {
                if (_DamageTypes == null)
                    Initialize();
                return _DamageTypes;
            }
        }

        public static void Initialize()
        {
            //_DamageTypes = new StatCollection(){
            //    {Stat.Blunt},
            //    {Stat.Slash},
            //    {Stat.Chop},
            //    {Stat.Pierce},
            //    {Stat.Mining},
            //    {Stat.Shoveling},
            //};
            _DamageTypes = new List<Stat>(){
                Stat.Blunt,
                Stat.Slash,
                Stat.Chop,
                Stat.Pierce,
                Stat.Mining,
                Stat.Digging,
            };
        }


        static public StatsComponent GetValues(StatsComponent damageStats, HealthComponent health) //Dictionary<Damage.Types, float> resistances)
        {
            StatsComponent dmgComp = StatsComponent.Damage;
            foreach (KeyValuePair<string, object> dmg in damageStats.Properties)
            {
                // dmgComp.Properties[dmg.Key] = (float)dmg.Value * (1 - resistances[(Damage.Types)Enum.Parse(typeof(Damage.Types), dmg.Key)]);
                dmgComp.Properties[dmg.Key] = (float)dmg.Value * (1 - health.GetProperty<float>(dmg.Key));
            }
            return dmgComp;
        }

        static public float GetTotal(StatsComponent dmgStats)
        {
            float total = 0;
            //foreach (KeyValuePair<Damage.Types, float> dmg in Values)
            //{
            //    total += dmg.Value;
            //}
            foreach (KeyValuePair<string, object> dmg in dmgStats.Properties)
            {
                total += (float)dmg.Value;
            }
            return total;
        }
    }
    public class DamageType
    {
        public string Name;
        public Damage.Types ID;
        public Icon Icon;
        public DamageType(Damage.Types id, string name)
        {
            ID = id;
            Name = name;
        }


    }
    
    class DamageComponent : Component
    {
        

        //public Dictionary<Damage.Types, float> Values;
        public DamageComponent()
            : base()
        {
            Properties.Add("Values", null);
        }
        public DamageComponent(float chop = 0, float blunt = 0, float slash = 0)
        {
            //Values = new Dictionary<Damage.Types, float>() { {Damage.Types.Chop, chop}, {Damage.Types.Blunt, blunt}, {Damage.Types.Slash, slash} };
            Properties["Values"] = new Dictionary<Damage.Types, float>() { { Damage.Types.Chop, chop }, { Damage.Types.Blunt, blunt }, { Damage.Types.Slash, slash } };
        }



        public override object Clone()
        {
            return this;
        }

        

        //public override string ToString()
        //{
        //    string info = "";
        //    //for (int i = 0; i < Values.Count; i++)
        //    //{
        //    //    if(i!=0)
        //    //        info+="\n";
        //    //    string[] dmgNames = Enum.GetNames(typeof(Types));
        //    //    info += dmgNames[i] + " damage: " + Values[i];
        //    //}
        //    foreach (KeyValuePair<Damage.Types, float> dmg in Values)
        //    {

        //    }
        //    return info;
        //}


        //public DamageComponent GetValues(Dictionary<Damage.Types, float> resistances)
        //{
        //    DamageComponent dmgComp = new DamageComponent();
        //    foreach (KeyValuePair<Damage.Types, float> dmg in Values)
        //    {
        //        dmgComp.Values[dmg.Key] = dmg.Value * (1 - resistances[dmg.Key]);
        //    }
        //    return dmgComp;
        //}

        

        //public float GetTotal()
        //{
        //    float total = 0;
        //    foreach (KeyValuePair<Damage.Types, float> dmg in Values)
        //    {
        //        total += dmg.Value;
        //    }
        //    return total;
        //}

        
    }
}
