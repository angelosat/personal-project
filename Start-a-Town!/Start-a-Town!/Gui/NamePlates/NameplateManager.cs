using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class NameplateManager : Control
    {
        public bool NameplatesEnabled { get { return this.Controls.Contains(this.Container); } }
        HashSet<GameObject> PreviousScene = new();
        readonly Dictionary<INameplateable, Nameplate> Cache = new();
        readonly NameplatesContainer Container = new();
        readonly NameplatesContainer ContainerActors = new();

        public NameplateManager()
        {
            this.AddControls(this.ContainerActors);
        }
        public void Update(SceneState scene)
        {
            var added = scene.ObjectsDrawn.Except(this.PreviousScene);
            var removed = this.PreviousScene.Except(scene.ObjectsDrawn);
            Nameplate plate;

            foreach (var entity in added)
            {
                if (!this.Cache.TryGetValue(entity, out plate))
                {
                    plate = Nameplate.Create(entity);
                    this.Cache[entity] = plate;
                }
                if (entity is Actor)
                    this.ContainerActors.AddControls(plate);
                else
                    this.Container.AddControls(plate);
            }
            foreach (var entity in removed)
            {
                if (this.Cache.TryGetValue(entity, out plate))
                {
                    if (entity is Actor)
                        this.ContainerActors.RemoveControls(plate);
                    else
                        this.Container.RemoveControls(plate);
                }
            }
            this.PreviousScene = scene.ObjectsDrawn;
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.EntityDespawned:
                    var entity = e.Parameters[0] as GameObject;
                    this.DisposeNameplate(entity);
                    break;

                default:
                    break;
            }
        }

        private void DisposeNameplate(GameObject entity)
        {
            if (!this.Cache.TryGetValue(entity, out var plate))
                return;
            this.Container.RemoveControls(plate);
            this.ContainerActors.RemoveControls(plate);
            this.Cache.Remove(entity);
        }
        
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.N:
                    ToggleNameplates();
                    break;

                default:
                    break;
            }
        }

        public void ToggleNameplates()
        {
            if (!this.Controls.Contains(this.ContainerActors))
                this.AddControls(this.ContainerActors);
            else if (!this.Controls.Contains(this.Container))
                this.AddControls(this.Container);
            else
                this.ClearControls();
        }

        //public override void Draw(SpriteBatch sb, Rectangle viewport)
        //{
        //    base.Draw(sb, viewport);
        //}
        //public override void DrawOnCamera(SpriteBatch sb, Camera camera)
        //{
        //    base.DrawOnCamera(sb, camera);
        //}
    }
}
