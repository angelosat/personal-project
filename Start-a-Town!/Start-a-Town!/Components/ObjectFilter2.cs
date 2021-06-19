﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class ObjectFilter2
    {
        public readonly bool Protected;
        public Func<GameObject, bool> Apply;
        public ObjectFilter2()
        {
            this.Apply = foo => true;
            this.Protected = false;
        }
        public ObjectFilter2(Func<GameObject, bool> apply, bool protect = false)
        {
            this.Apply = apply;
            this.Protected = protect;
        }

        public ObjectFilter2 Clone()
        {
            return new ObjectFilter2(Apply, Protected);
        }
    }
}
