using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components
{
    struct StatusCondition
    {
        //static public GameObject StatusCondition
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj.AddComponent("Info", new InfoComponent(GameObject.Types.StatusCondition, "Status Condition", "Base status condition"));
        //        Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
        //        obj.AddComponent("Sprite", new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[6] } }, new Vector2(16)));
        //        obj.AddComponent<GuiComponent>().Initialize(6));
        //        obj.AddComponent("Condition", new ConditionComponent());
        //        return obj;
        //    }
        //}

        static public GameObject Empty
        {
            get
            {
                //GameObject obj = new GameObject();
                //obj.AddComponent("Info", new InfoComponent(GameObject.Types.StatusCondition, "Status Condition", "Base status condition"));
                //Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
                //obj.AddComponent("Sprite", new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[6] } }, new Vector2(16)));
                //obj.AddComponent<GuiComponent>().Initialize(6));
                //obj.AddComponent("Condition", new ConditionComponent());
                return Create();// obj;
            }
        }

        static public GameObject Create(Message.Types message = Message.Types.Default, string name = "<undefined>", string description = "<undefined>", Stat stat = null, float value = 0f, float duration = 0f)
        {
                GameObject obj = new GameObject();
                obj.AddComponent("Info", new GeneralComponent(GameObject.Types.StatusCondition, ObjectType.Condition, name, description));
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
                obj.AddComponent("Sprite", new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[6] } }, new Vector2(16)));
                obj.AddComponent<GuiComponent>().Initialize(6);
                obj.AddComponent("Condition", new ConditionComponent(message, stat, value, duration));
                return obj;
        }
        //static public GameObject StatModCondition
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.StatusCondition;
        //        //     obj.GetComponent<StatusComponent>("Conditions").Properties.Add(
        //        return obj;
        //    }
        //}
    }

    class ConditionComponent : Component
    {
        public override string ComponentName
        {
            get { return "Condition"; }
        }

        Stat Stat { get { return (Stat)this["Stat"]; } set { this["Stat"] = value; } }
        float Value { get { return (float)this["Value"]; } set { this["Value"] = value; } }
        float Timer { get { return (float)this["Timer"]; } set { this["Timer"] = value; } }
        float Duration { get { return (float)this["Duration"]; } set { this["Duration"] = value; } }
        Message.Types Msg { get { return (Message.Types)this["Message"]; } set { this["Message"] = value; } }

        public enum ConditionTypes { StatMod }
        //static StatusConditionCollection _Conditions;
        //public static StatusConditionCollection Conditions
        //{
        //    get
        //    {
        //        if (_Conditions == null)
        //            Initialize();
        //        return _Conditions;
        //    }
        //}

        //private static void Initialize()
        //{
        //    _Conditions = new StatusConditionCollection();//{                new 

        //}

        public ConditionComponent(Message.Types type, Stat stat = null, float value = 0, float length = 1)
        {
            // Properties = new ComponentPropertyCollection();
            this.Stat = stat;
            this.Value = value;
            //Properties.Add("Duration", duration);
            this.Duration = length;
            this.Timer = length;
            this.Msg = type;
        }

        public override string ToString()
        {
            return Properties.ToString();
        }


        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            //Console.WriteLine(msg);
            StatsComponent stats;
            string statName;
            switch (msg)
            {
                case Message.Types.Equip:
                    stats = sender.GetComponent<StatsComponent>("Stats");
                    statName = Stat.Name;//Stat.StatDB[GetProperty<Stat.Types>("Type")].Name;
                    stats.Properties[statName] = stats.GetPropertyOrDefault<float>(statName) + GetProperty<float>("Value");
                    Log.Enqueue(Log.EntryTypes.Buff, sender, parent);
                    return true;

                //case Message.Types.Unequip:
                //    stats = sender.GetComponent<StatsComponent>("Stats");
                //    statName = Stat.Name;// Stat.StatDB[GetProperty<Stat.Types>("Type")].Name;
                //    stats.Properties[statName] = stats.GetPropertyOrDefault<float>(statName) - GetProperty<float>("Value");
                //    Log.Enqueue(Log.EntryTypes.Debuff, sender, parent);
                //    return true;

                case Message.Types.Update:
                    Properties["Timer"] = GetProperty<float>("Timer") - 1;//GlobalVars.DeltaTime;
                    if (GetProperty<float>("Timer") <= 0)
                        throw new NotImplementedException();
                        //sender.PostMessage(Message.Types.ConditionFinished, parent);
                    return true;
                default:
                    return false;
            }
        }

        //public override void Update(GameObject parent, Chunk chunk = null)
        //{
        //    Properties["Timer"] = GetProperty<float>("Timer") - GlobalVars.DeltaTime;
        //    if (GetProperty<float>("Timer") <= 0)
        //        sender.HandleMessage(parent, Message.Types.ConditionFinished);
        //    return false;
        //}

        public override object Clone()
        {
            ConditionComponent comp = new ConditionComponent(Msg, Stat, Value, Duration);
            return comp;
        }
    }
}
