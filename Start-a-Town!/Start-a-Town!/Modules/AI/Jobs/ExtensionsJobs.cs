﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    static class ExtensionsJobs
    {
        static public void ToggleJob(this Actor actor, JobDef jobDef)
        {
            actor.GetState().ToggleJob(jobDef);
        }
        static public void HasJob(this Actor actor, JobDef jobDef)
        {
            actor.GetState().HasJob(jobDef);
        }
        static public Job GetJob(this Actor actor, JobDef jobDef)
        {
            return actor.GetState().GetJob(jobDef);
        }
        static public IEnumerable<Job> GetJobs(this Actor actor)
        {
            return actor.GetState().GetJobs();
        }
    }
}
