using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components.Particles;
using Start_a_Town_.UI;

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

            //var particleList = new ListBox<ParticleDensityLevel, Button>(new Rectangle(0, 0, 150, 10 * Button.DefaultHeight));
            var particleList = new ListBox<ParticleDensityLevel, Button>((from i in ParticleDensityLevel.All select i.Name).GetMaxWidth() + UIManager.DefaultDropdownSprite.Width, ParticleDensityLevel.All.Count * Button.DefaultHeight);

            particleList.Build(ParticleDensityLevel.All, t => t.Name);
            this.Combo_Particles = new ComboBox<ParticleDensityLevel>(particleList, t => t.Name)
            {
                Location = this.Label_Particles.TopRight,//.BottomLeft,
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
            //Engine.Settings["Settings"]["Graphics"]["Particles"].InnerText = ParticleDensityLevel.Current.Name;
            //// TODO: save particle settings in config file
            //Engine.Settings.Save("config.xml");
        }
    }
}
