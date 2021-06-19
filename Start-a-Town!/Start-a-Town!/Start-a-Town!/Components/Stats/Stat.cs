using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Stats;

namespace Start_a_Town_.Components
{
    public class Stat : ITooltippable
    {
        static public Stat Strength { get { return new Stat(Types.Strength, "Strength"); } }
        static public Stat AtkSpeed { get { return new Stat(Types.AtkSpeed, "Attack Speed"); } }
        
        static public Stat WorkSpeed { get { return new Stat(Types.WorkSpeed, "Work Speed"); } }

        static public Stat MaxSkills { get { return new Stat(Types.MaxSkills, "Maximum Skills"); } }
        static public Stat Tilling { get { return new Stat(Types.Tilling, "Tilling"); } }
        static public Stat Farming { get { return new Stat(Types.Farming, "Farming"); } }
        static public Stat Lumberjacking { get { return new Stat(Types.Lumberjacking, "Lumberjacking"); } }
        static public Stat Harvesting { get { return new Stat(Types.Harvesting, "Harvesting"); } }
        static public Stat Building { get { return new Stat(Types.Building, "Building"); } }
        static public Stat Blunt { get { return new Stat(Types.Blunt, "Blunt"); } }
        static public Stat Chop { get { return new Stat(Types.Chop, "Chop"); } }
        static public Stat Slash { get { return new Stat(Types.Slash, "Slash"); } }
        static public Stat Pierce { get { return new Stat(Types.Pierce, "Pierce"); } }
        static public Stat Mining { get { return new Stat(Types.Mining, "Mining"); } }
        static public Stat ValueOld { get { return new Stat(Types.Value, "Value"); } }
        static public Stat Level { get { return new Stat(Types.Level, "Level"); } }
        static public Stat Experience { get { return new Stat(Types.Experience, "Experience"); } }
        static public Stat Loot { get { return new Stat(Types.Loot, "Loot"); } }
        static public Stat Max { get { return new Stat(Types.Max, "Max"); } }
        static public Stat Skill { get { return new Stat(Types.Skill, "Skill"); } }
        static public Stat Cooldown { get { return new Stat(Types.Cooldown, "Cooldown"); } }
        static public Stat Mainhand { get { return new Stat(Types.Mainhand, "Mainhand"); } }
        static public Stat Offhand { get { return new Stat(Types.Offhand, "Offhand"); } }
        static public Stat Chest { get { return new Stat(Types.Chest, "Chest"); } }
        static public Stat Feet { get { return new Stat(Types.Feet, "Feet"); } }
        static public Stat Hands { get { return new Stat(Types.Hands, "Hands"); } }
        static public Stat Weapon { get { return new Stat(Types.Weapon, "Weapon"); } }
        static public Stat Density { get { return new Stat(Types.Density, "Density"); } }
        static public Stat Timer { get { return new Stat(Types.Timer, "Timer"); } }
        static public Stat Friction { get { return new Stat(Types.Friction, "Friction"); } }
        static public Stat Dps { get { return new Stat(Types.Dps, "Dps"); } }
        static public Stat Damage { get { return new Stat(Types.Damage, "Damage"); } }
        static public Stat SpeedX { get { return new Stat(Types.SpeedX, "SpeedX"); } }
        static public Stat SpeedY { get { return new Stat(Types.SpeedY, "SpeedY"); } }
        static public Stat SpeedZ { get { return new Stat(Types.SpeedZ, "SpeedZ"); } }
        static public Stat X { get { return new Stat(Types.X, "X"); } }
        static public Stat Y { get { return new Stat(Types.Y, "Y"); } }
        static public Stat Z { get { return new Stat(Types.Z, "Z"); } }
        static public Stat Health { get { return new Stat(Types.Health, "Health"); } }
        static public Stat Stage { get { return new Stat(Types.Stage, "Stage"); } }

        public enum BonusType { Flat, Percentile }
        public enum Types
        {
            Dps, AtkSpeed, Damage, DmgChop, DmgBlunt, DmgSlash, DmgPierce, Strength, WorkSpeed, Digging, JumpHeight, MaxSkills,
            Blunt, Chop, Slash, Pierce, Mining, Lumberjacking, Value, Level, Experience, WalkSpeed, Farming, Tilling,
            Loot,
            Max,
            Skill,
            Cooldown,
            Mainhand,
            Offhand,
            Chest,
            Feet,
            Weapon,
            Hands,
            Density,
            Timer,
            Friction,
            SpeedX,
            SpeedY,
            SpeedZ,
            X,
            Y,
            Z,
            Health,
            Stage,
            Building,
            Harvesting,
            MatRecover,
            Knockback,
            KnockbackResistance,
            DmgReduction,
            MaxWeight,
        }

        public enum Groups
        {
            General,
            Combat,
            Damage,
            Offense,
            Defense,
            Skills,
        }

        static StatRegistry _Registry;
        public static StatRegistry Registry
        {
            get
            {
                if (_Registry.IsNull())
                    Initialize();
                return _Registry;
            }
        }
        static void Initialize()
        {
            _Registry = new StatRegistry()   
            {
                WalkSpeed,
                WorkSpeed,
                JumpHeight,
                MaterialRecovery,
                new StatMaxWeight(),

                Blunt,
                Slash,
                Chop,
                Pierce,

                Knockback,
                KnockbackResistance,
                DmgReduction,

                Mining,
                Digging
            };
        }
        static public Stat GetStat(Stat.Types id)
        {
            return Registry.FirstOrDefault(s => s.ID == id);
        }
        protected Stat()
        {

        }
        protected Stat(Types id, string name, BonusType type = BonusType.Flat, string description = "")
        {
            ID = id;
            Name = name;
            this.Type = type;
            this.Description = description;
            this.Group = Groups.General;
            //     StatDB[type] = this;
        }
        public BonusType Type { get; set; }
        public string Name, Description;
        public Types ID { get; set; }
        public Groups Group { get; set; }
        public List<ValueModifier> Modifiers = new List<ValueModifier>();
        //public int ID
        //{ get { return (int)Type; } }
        public float Value { get; set; }

        public float GetFinalValue(GameObject parent)
        {
            var final = this.Value;
            foreach (var mod in this.Modifiers)
                final = mod.Modify(parent, final);
            return final;
        }

        public static Stat Parse(string name)
        {
            // return StatDB[(Stat.Types)Enum.Parse(typeof(Stat.Types), name)];
            return Registry.ToDictionary(stat => stat.ID, stat => stat)[(Stat.Types)Enum.Parse(typeof(Stat.Types), name)];
        }

        public string ToString(GameObject parent)
        {
            return this.Name + " " + this.GetFinalValue(parent).ToString(this.Type == Stat.BonusType.Percentile ? "+##0.##%;-##0.##%;+0%" : "+##0.##;-##0.##;+0") + " ";
        }

        public string ToString(float value)
        {
            return value.ToString(this.Type == Stat.BonusType.Percentile ? "+##0.##%;-##0.##%;+0%" : "+##0.##;-##0.##;+0") + " " + this.Name;
        }

        public Stat Clone()
        {
            var stat = new Stat(this.ID, this.Name, this.Type, this.Description) { Value = this.Value, Group = this.Group };
            foreach (var mod in this.Modifiers)
                stat.Modifiers.Add(mod);
            return stat;
        }
        static public Stat Create(Stat.Types type)
        {
            return GetStat(type).Clone();
        }
        static public Stat Create(Stat.Types type, float value)
        {
            var stat = GetStat(type).Clone();
            stat.Value = value;
            return stat;
        }

        //public UI.Control GetInterface()
        //{
        //    Label lbl = new Label(this.ToString());
        //}
        public void GetTooltipInfo(Tooltip tip)
        {
            tip.Controls.Add(new Label("Modifiers:") { Location = tip.Controls.BottomLeft });
            foreach(var mod in this.Modifiers)
            {
                tip.Controls.Add(new Label(mod.ToString()) { Location = tip.Controls.BottomLeft });
            }
        }

        static public Stat MaterialRecovery
        {
            get
            {
                return new Stat(Types.MatRecover, "Material recovery chance", BonusType.Percentile, "Chance to recover construction frame materials when finishing a structure.") { Group = Groups.Skills };
            }
        }
        static public Stat Digging
        {
            get { return new Stat(Types.Digging, "Digging speed", BonusType.Percentile) { Group = Groups.Skills }; }
        }
        static public Stat JumpHeight
        {
            get
            {
                return new Stat(Types.JumpHeight, "Jump Height", BonusType.Percentile);
            }
        }
        static public Stat WalkSpeed = new Stat(Types.WalkSpeed, "Movement Speed", BonusType.Percentile);
        //{
        //    get
        //    {
        //        return new Stat(Types.WalkSpeed, "Movement Speed", BonusType.Percentile);
        //    }
        //}
        static public Stat Knockback
        {
            get
            {
                return new Stat(Types.Knockback, "Knockback", BonusType.Percentile) { Group = Groups.Combat };
            }
        }
        static public Stat KnockbackResistance
        {
            get
            {
                return new Stat(Types.KnockbackResistance, "Knockback Resistance", BonusType.Percentile) { Group = Groups.Defense };
            }
        }
        static public Stat DmgReduction
        {
            get
            {
                return new Stat(Types.DmgReduction, "Damage Reduction", BonusType.Percentile) { Group = Groups.Defense };
            }
        }
    }

}
