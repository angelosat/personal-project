using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI.Settings
{
    class CameraSettings : GroupBox
    {
        CheckBox Chk_Fog, Chk_HideCeiling, Chk_HideObscuringBlocks;
        bool Changed;

        public CameraSettings()
        {
            this.Name = "Camera";

            this.Chk_Fog = new CheckBox("Fog", Camera.Fog);
            //this.Chk_HideCeiling = new CheckBox("Draw blocks above player", Camera.hi)
            this.Controls.Add(this.Chk_Fog);
        }

        public void Apply()
        {
            if (!Changed)
                return;
            this.Changed = false;

            Camera.Fog = this.Chk_Fog.Checked;

            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Camera").GetOrCreateElement("Fog").Value = Camera.Fog.ToString();
            Engine.Config.Save("config.xml");
        }
    }
}
