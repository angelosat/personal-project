using System.Linq;
using Start_a_Town_.Particles;

namespace Start_a_Town_.UI.Settings
{
    class GraphicsSettings : GroupBox
    {
        ComboBox<ParticleDensityLevel> Combo_Particles;
        Label Label_Particles;
        bool Changed;

        public GraphicsSettings()
        {
            this.Name = "Graphics";
            this.Label_Particles = new Label("Particle Density");

            var particleList = new ListBox<ParticleDensityLevel, Button>((from i in ParticleDensityLevel.All select i.Name).GetMaxWidth() + UIManager.DefaultDropdownSprite.Width, ParticleDensityLevel.All.Count * Button.DefaultHeight);

            particleList.Build(ParticleDensityLevel.All, t => t.Name);
            this.Combo_Particles = new ComboBox<ParticleDensityLevel>(particleList, t => t.Name)
            {
                Location = this.Label_Particles.TopRight,
                Text = ParticleDensityLevel.Current.Name,
                ItemChangedFunction = i => { this.Changed = true; }
            };
            this.Controls.Add(this.Label_Particles, this.Combo_Particles);
        }

        public void Apply()
        {
            if (!Changed)
                return;
            this.Changed = false;

            ParticleDensityLevel.Current = this.Combo_Particles.SelectedItem;

            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Graphics").GetOrCreateElement("Particles").Value = ParticleDensityLevel.Current.Name;
            Engine.Config.Save("config.xml");
        }
    }
}
