﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Net
{
    public interface IObjectFactory
    {
        GameObject Instantiate(GameObject obj);
    }
}
