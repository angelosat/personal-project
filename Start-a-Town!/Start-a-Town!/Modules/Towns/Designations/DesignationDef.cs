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

        public static readonly DesignationDef Deconstruct = new("Deconstruct", ButtonDeconstructAdd, typeof(DesignationWorkerDeconstruct));
        public static readonly DesignationDef Mine = new("Mine", ButtonMineAdd, typeof(DesignationWorkerMine));
        public static readonly DesignationDef Switch = new("Switch", ButtonSwitch, typeof(DesignationWorkerSwitch));
        public static readonly DesignationDef Remove = new("Remove", null, null);

        static DesignationDef()
        {
            Register(Deconstruct);
            Register(Mine);
            Register(Switch);
            Register(Remove);
        }

        public QuickButton IconAdd;
        public IconButton IconRemove;
        public bool IsValid(MapBase map, IntVec3 global) => this.Worker.IsValid(map, global);
        readonly Type WorkerClass;
        DesignationWorker _cachedWorker;
        DesignationWorker Worker => _cachedWorker ??= (DesignationWorker)Activator.CreateInstance(this.WorkerClass);
        public DesignationDef(string name, QuickButton icon, Type workerClass) : base(name)
        {
            this.WorkerClass = workerClass;
            this.IconAdd = icon;
            this.IconRemove = icon != null ? new IconButton(icon.Icon, Icon.Cross) { HoverText = $"Cancel {name}" } : null;
        }
    }
}
