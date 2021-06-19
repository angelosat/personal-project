﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class ItemComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Item";
            }
        }

        //public ResourceDef Durability { get { return (ResourceDef)this["Durability"]; } set { this["Durability"] = value; } }
        public int Level;// { get { return (int)this["Level"]; } set { this["Level"] = value; } }
        public Resource Durability;
        public ItemComponent()
        {
            //this.Durability = ResourceDef.Create(ResourceDef.ResourceTypes.Durability, 1);
            this.Level = 1;
        }


        public ItemComponent(int level)//, float durability)
        {
            //this.Durability = ResourceDef.Create(ResourceDef.ResourceTypes.Durability, durability);
            this.Level = level;
        }

        public override object Clone()
        {
            return new ItemComponent(this.Level);//, this.Durability.Max);
        }

        //static public ItemComponent Add(GameObject obj, int level, float durability)
        //{
        //    ItemComponent dur;
        //    if (!obj.TryGetComponent<ItemComponent>("Durability", out dur))
        //    {
        //        dur = new ItemComponent(level, durability);
        //        obj["Durability"] = dur;
        //    }
        //    dur.Durability.Max = durability;
        //    return dur;
        //}

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            //tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, this.ToString()) { Font = UIManager.FontBold, TextColorFunc = () => Color.Lerp(Color.Red, Color.White, this.Durability.Percentage) });
        }

        static public void Modify(GameObject obj, Func<int, float, float> durability)
        {
            ItemComponent dur;
            if (!obj.TryGetComponent<ItemComponent>("Item", out dur))
                return;
            dur.Durability.Max = durability(dur.Level, dur.Durability.Max);
            //dur.Durability.Value = dur.Durability.Max;
            dur.Durability.Adjust(dur.Durability.Max);
        }

        static public void Modify(GameObject obj, Action<ItemComponent> action)
        {
            ItemComponent dur;
            if (!obj.TryGetComponent<ItemComponent>("Item", out dur))
                return;
            action(dur);
        }
        static public int GetLevel(GameObject obj)
        {
            ItemComponent dur;
            if (!obj.TryGetComponent<ItemComponent>("Item", out dur))
                return 0;
            return dur.Level;
        }

    }
}
