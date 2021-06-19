using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.UI
{
    class PopupManager : InputHandler, IKeyEventHandler
    {
        static PopupManager _Instance;
        public static PopupManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new PopupManager();
                return _Instance;
            }
        }

        public List<Popup> Popups { get; set; }

        PopupManager()
        {
            this.Popups = new List<Popup>();
        }

        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.HandleRButtonDown(e);
            foreach (var popup in this.Popups.ToList())
                popup.Hide();
            this.Popups.Clear();
        }
    }
}
