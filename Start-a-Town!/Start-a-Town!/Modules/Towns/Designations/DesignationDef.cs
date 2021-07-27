using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class DesignationDef : Def
    {
        public static readonly Icon MineIcon = new(ItemContent.PickaxeFull);
        public static readonly Icon DeconstructIcon = new(ItemContent.HammerFull);

        static readonly QuickButton ButtonMineAdd = new(MineIcon, KeyBind.DigMine, "Mine") { HoverText = "Designate Mining" };
        static readonly QuickButton ButtonDeconstructAdd = new(DeconstructIcon, KeyBind.Deconstruct, "Deconstruct") { HoverText = "Designate Deconstruction" };
        static readonly QuickButton ButtonSwitch = new('☞', null, "Switch") { HoverText = "Switch on/off" };

        public static readonly QuickButton IconCancel = new QuickButton(UI.Icon.X, KeyBind.Cancel) { HoverText = "Cancel designation" };

        public static List<DesignationDef> All = new();

        public static readonly DesignationDef Deconstruct = new("Deconstruct", ButtonDeconstructAdd, (map, global) => map.IsDeconstructible(global));
        public static readonly DesignationDef Mine = new("Mine", ButtonMineAdd, (map, global) => map.GetBlock(global).IsMinable);
        public static readonly DesignationDef Switch = new("Switch", ButtonSwitch, (map, global) => map.GetBlockEntity(global)?.HasComp<BlockEntityCompSwitchable>() ?? false);
        public static readonly DesignationDef Remove = new("Remove", null, null);

        public static readonly Dictionary<string, DesignationDef> Dictionary = new()
        {
            { Deconstruct.Name, Deconstruct },
            { Mine.Name, Mine },
            { Switch.Name, Switch },
            { Remove.Name, Remove }
        };
        static DesignationDef()
        {
            All.AddRange(new DesignationDef[] {
                Deconstruct,
                Mine,
                Switch});

            Register(Deconstruct);
            Register(Mine);
            Register(Switch);
            Register(Remove);
        }

        public QuickButton IconAdd;
        public IconButton IconRemove;
        public Func<MapBase, IntVec3, bool> IsValid = (map, global) => true;

        public DesignationDef(string name, QuickButton icon, Func<MapBase, IntVec3, bool> isValid) : base(name)
        {
            this.IconAdd = icon;
            this.IsValid = isValid;
            this.IconRemove = icon != null ? new IconButton(icon.Icon, Icon.Cross) { HoverText = $"Cancel {name}" } : null;
        }
    }
}
