using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class CameraSettings : GameSettings
    {
        GroupBox _Gui;
        internal override GroupBox Gui => this._Gui ??= this.CreateGui();
        bool _tmpFog;

        GroupBox CreateGui()
        {
            var box = new GroupBox();
            box.Name = "Camera";

            var Chk_Fog = new CheckBoxNew("Fog", () => _tmpFog = !_tmpFog, () => _tmpFog);
            box.Controls.Add(Chk_Fog);
            return box;
        }

        internal override void Apply()
        {
            Camera.Fog = _tmpFog;
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Camera").GetOrCreateElement("Fog").Value = _tmpFog.ToString();
        }
        internal override void Cancel()
        {
        }
    }
}
