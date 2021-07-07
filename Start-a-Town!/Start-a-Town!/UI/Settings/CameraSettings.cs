namespace Start_a_Town_.UI.Settings
{
    class CameraSettings : GroupBox
    {
        CheckBox Chk_Fog;
        bool Changed;

        public CameraSettings()
        {
            this.Name = "Camera";

            this.Chk_Fog = new CheckBox("Fog", Camera.Fog);
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
