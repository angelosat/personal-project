using System;

namespace Start_a_Town_
{
    public class BuildToolDef : Def
    {
        readonly Type ToolClass;
        readonly Type ToolWorkerClass;

        public BuildToolDef(string name, Type toolClass, Type toolWorkerClass ) : base(name)
        {
            this.ToolClass = toolClass;
            this.ToolWorkerClass = toolWorkerClass;
        }

        internal ToolBlockBuild Create(Action<ToolBlockBuild.Args> callback)
        {
            var tool = (ToolBlockBuild)Activator.CreateInstance(this.ToolClass);
            tool.Callback = callback;
            tool.ToolDef = this;
            return tool;
        }
        BuildToolWorker _cachedWorker;
        public BuildToolWorker Worker => _cachedWorker ??= (BuildToolWorker)Activator.CreateInstance(this.ToolWorkerClass);
    }
}
