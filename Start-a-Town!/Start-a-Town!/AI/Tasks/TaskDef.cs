﻿using System;

namespace Start_a_Town_
{
    public sealed class TaskDef : Def
    {
        public Type BehaviorClass;
        public string Format;
        public TargetIndex PrimaryTargetIndex;
        public Func<AITask, TargetArgs> GetPrimaryTarget;
        public TaskDef(string name, Type bhavClass):base(name)
        {
            this.BehaviorClass = bhavClass;
        }
        public string GetForceText(AITask task)
        {
            return string.Format(this.Format, this.GetPrimaryTarget(task).Label);
        }
    }
}
