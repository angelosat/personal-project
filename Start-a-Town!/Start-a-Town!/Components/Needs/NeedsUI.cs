using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class NeedsUI : GroupBox
    {
        GameObject Entity;
        GroupBox BoxNeeds, BoxMood;
        public NeedsUI()
        {
            this.BoxNeeds = new GroupBox();
            this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };

        }
        public NeedsUI(GameObject entity)
        {
            this.Entity = entity;

            var needs = entity.GetComponent<NeedsComponent>();
            var mood = entity.GetComponent<MoodComp>();

            this.BoxNeeds = new GroupBox();
            //this.BoxNeeds.AddControls(needs.NeedsHierarchy.GetUI(entity));
            needs.GetUI(entity, this.BoxNeeds);

            this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
            mood.GetInterface(entity, this.BoxMood);

            this.Controls.Add(this.BoxNeeds, this.BoxMood);

        }
        public void Refresh(GameObject entity)
        {
            this.Entity = entity;
            var needs = entity.GetComponent<NeedsComponent>();
            var mood = entity.GetComponent<MoodComp>();

            this.Controls.Clear();

            this.BoxNeeds.ClearControls();
            this.BoxMood.ClearControls();

            //this.BoxNeeds.AddControls(needs.NeedsHierarchy.GetUI(entity));
            needs.GetUI(entity, this.BoxNeeds);

            mood.GetInterface(entity, this.BoxMood);

            this.BoxMood.Location = this.BoxNeeds.TopRight;

            this.Controls.Add(this.BoxNeeds, this.BoxMood);
            this.Validate(true);
        }
        static NeedsUI Instance;
        internal static Window GetUI(Actor actor)
        {
            Window window;

            if (Instance == null)
            {
                Instance = new NeedsUI();
                window = new Window(Instance) { Movable = true, Closable = true };
            }
            else
                window = Instance.GetWindow();
            Instance.Tag = actor;
            window.Title = string.Format("{0} needs", actor.Name);
            Instance.Refresh(actor);
            return window;
        }
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            var actor = target.Object as Actor;
            if (!actor?.Equals(this.Tag) ?? false)
            {
                GetUI(actor);
            }
        }
    }
}
