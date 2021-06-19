using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.UI
{
    class UIConstructionFrame : Window
    {
        static UIConstructionFrame _Instance;
        public UIConstructionFrame Instance
        {
            get
            {
                if (_Instance is null)
                    _Instance = new UIConstructionFrame();
                return _Instance;
            }
        }

        UIConstructionFrame()
        {

        }
    }
}
