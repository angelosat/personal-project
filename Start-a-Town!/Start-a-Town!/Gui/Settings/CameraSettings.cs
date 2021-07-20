using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class CameraSettings : GameSettings
    {
        CheckBox Chk_Fog;
        bool Changed;
        GroupBox _Gui;
        internal override GroupBox Gui => this._Gui ??= this.CreateGui();

        GroupBox CreateGui()
        {
            var box = new GroupBox();
            box.Name = "Camera";

            this.Chk_Fog = new CheckBox("Fog", Camera.Fog);
            box.Controls.Add(this.Chk_Fog);
            return box;
        }

        internal override void Apply()
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
