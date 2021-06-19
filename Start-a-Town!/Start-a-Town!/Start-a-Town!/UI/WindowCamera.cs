using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.UI
{
    class WindowCamera : Window
    {
        static WindowCamera _Instance;
        static public WindowCamera Instance
        {
            get
            {
                if (_Instance.IsNull())
                    _Instance = new WindowCamera();
                return _Instance;
            }
        }

        WindowCamera()
        {
            ElevationWidget elev = new ElevationWidget();
            CameraWidget cam = new CameraWidget();
        }
    }
}
