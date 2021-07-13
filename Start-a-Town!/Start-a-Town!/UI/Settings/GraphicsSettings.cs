﻿using Start_a_Town_.Particles;

namespace Start_a_Town_.UI.Settings
{
    class GraphicsSettings : GroupBox
    {
        ComboBoxNewNew<ParticleDensityLevel> Combo_Particles;
        bool Changed;
        ParticleDensityLevel TempParticles;

        public GraphicsSettings()
        {
            this.Name = "Graphics";
            this.TempParticles = ParticleDensityLevel.Current;
            this.Combo_Particles = new(ParticleDensityLevel.All, 200, "Particle Density", i => i.Name, () => this.TempParticles, i => { this.TempParticles = i; });
            this.Controls.Add(this.Combo_Particles);
        }

        public void Apply()
        {
            if (!Changed)
                return;
            this.Changed = false;

            ParticleDensityLevel.Current = this.TempParticles;

            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Graphics").GetOrCreateElement("Particles").Value = ParticleDensityLevel.Current.Name;
            Engine.Config.Save("config.xml");
        }
    }
}
