using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Control
{
    class InputManager
    {
        #region Fields
        //SortedList<float, IInputHandler> Handlers;
        List<IInputHandler> Handlers;
        #endregion

        #region Initialization
        static InputManager _Instance;
        static InputManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new InputManager();
                return _Instance;
            }
            set { _Instance = value; }
        }
        public InputManager()
        {
            //Handlers = new SortedList<float, IInputHandler>();
            Handlers = new List<IInputHandler>();
        }
        #endregion

        #region Static Public Methods
        public bool Add(IInputHandler handler)
        {
            if (Handlers.Contains(handler))
                return false;
            Handlers.Add(handler);
            return true;
        }
        public bool Remove(IInputHandler handler)
        {
            if (!Handlers.Contains(handler))
                return false;
            Handlers.Remove(handler);
            return true;
        }

        //static public bool Add(IInputHandler handler)
        //{
        //    //if (Instance.Handlers.ContainsKey(handler.ZOrder))
        //    //    return false;

        //    //Instance.Handlers.Add(handler.ZOrder, handler);
        //    if (Instance.Handlers.Contains(handler))
        //        return false;
        //    Instance.Handlers.Add(handler);
        //    return true;
        //}
        //static public bool Remove(IInputHandler handler)
        //{
        //    if (!Instance.Handlers.Contains(handler))
        //        return false;
        //    Instance.Handlers.Remove(handler);
        //    return true;
        //}
        #endregion


    }
}
