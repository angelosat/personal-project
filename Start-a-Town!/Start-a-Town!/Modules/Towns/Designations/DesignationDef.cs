using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public sealed class DesignationDef : Def
    {
        static public readonly Icon MineIcon = new(ItemContent.PickaxeFull);
        static public readonly Icon DeconstructIcon = new(ItemContent.HammerFull);

        static readonly QuickButton ButtonMineAdd = new(MineIcon, KeyBind.DigMine, "Mine") { HoverText = "Designate Mining" };
        static readonly QuickButton ButtonDeconstructAdd = new(DeconstructIcon, KeyBind.Deconstruct, "Deconstruct") { HoverText = "Designate Deconstruction" };
        static readonly QuickButton ButtonSwitch = new('☞', null, "Switch") { HoverText = "Switch on/off" };



        public static List<DesignationDef> All = new();
  

        static public readonly DesignationDef Deconstruct = new("Deconstruct", ButtonDeconstructAdd, (map, global) => map.IsDeconstructible(global));// map.GetBlock(global).IsDeconstructable);
        static public readonly DesignationDef Mine = new("Mine", ButtonMineAdd, (map, global) => map.GetBlock(global).IsMinable);
        //static public readonly Designation Build = new Designation("Build", ButtonDeconstructAdd, (map, global) => map.GetBlockEntity(global) is IConstructible, false);// map.GetBlock(global) == Block.Designation, false);//;
        static public readonly DesignationDef Switch = new("Switch", ButtonSwitch, (map, global) => map.GetBlockEntity(global)?.HasComp<BlockEntityCompSwitchable>() ?? false);
        static public readonly DesignationDef Null = new("Null", null, null);

        static public readonly Dictionary<string, DesignationDef> Dictionary = new() {
            { Deconstruct.Name, Deconstruct } ,
            { Mine.Name, Mine },
            //{ Build.Name, Build },
            { Switch.Name, Switch },
            { Null.Name, Null }
        };
        static DesignationDef()
        {
            All.AddRange(new DesignationDef[] {
                Deconstruct,
                Mine,
                Switch});

            Register(Deconstruct);
            Register(Mine);
            //Register(Build);
            Register(Switch);
            Register(Null);
        }
        public Func<IMap, Vector3, bool> IsValid = (map, global) => true;

        public DesignationDef(string name, QuickButton icon, Func<IMap, Vector3, bool> isValid) : base(name)
        {
            this.IconAdd = icon;
            this.IsValid = isValid;
            //this.Draw = draw;
            this.IconRemove = icon != null ? new IconButton(icon.Icon, Icon.Cross) { HoverText = $"Cancel {name}" } : null;
        }
        public QuickButton IconAdd;
        public IconButton IconRemove;
        //public bool Draw { get; } = true;
        

        public static readonly QuickButton IconCancel = new QuickButton(UI.Icon.X, KeyBind.Cancel) { HoverText = "Cancel designation" };
       
        public static void Cancel(List<TargetArgs> positions)
        {
            PacketDesignation.Send(Net.Client.Instance, Null, positions, false);
        }
    }
}
