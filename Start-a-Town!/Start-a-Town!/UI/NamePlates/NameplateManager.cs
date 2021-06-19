using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class NameplateManager : Control
    {
        //enum Modes { None, Actors, All };
        //Modes Mode = Modes.Actors;

        public bool NameplatesEnabled { get { return this.Controls.Contains(this.Container); } }
        HashSet<GameObject> PreviousScene = new();// new SceneState();
        readonly Dictionary<INameplateable, Nameplate> Cache = new();
        readonly NameplatesContainer Container = new();
        readonly NameplatesContainer ContainerActors = new();

        public NameplateManager()
        {
            //this.Layer = LayerTypes.Nameplates;
            this.AddControls(this.ContainerActors);
        }
        public void Update(SceneState scene)
        {
            //var added = this.PreviousScene != null ? scene.ObjectsDrawn.Except(this.PreviousScene.ObjectsDrawn) : scene.ObjectsDrawn;
            //var removed = this.PreviousScene != null ? this.PreviousScene.ObjectsDrawn.Except(scene.ObjectsDrawn) : new GameObject[] { };
            var added = scene.ObjectsDrawn.Except(this.PreviousScene);
            var removed = this.PreviousScene.Except(scene.ObjectsDrawn);
            Nameplate plate;

            foreach (var entity in added)
            {
                //var plate = Nameplate.Create(entity);
                //this.Cache[entity] = plate;
                if (!this.Cache.TryGetValue(entity, out plate))
                {
                    plate = Nameplate.Create(entity);
                    this.Cache[entity] = plate;
                }
                //plate.Show();
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

            //this.Container.UpdateCollisions(Net.Client.Instance.Map, ScreenManager.CurrentScreen.Camera);
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.EntityDespawned:
                    var entity = e.Parameters[0] as GameObject;
                    //this.Cache[entity].Hide();
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
        //public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        //{
        //    //base.HandleKeyDown(e);
        //    switch(e.KeyCode)
        //    {
        //        case System.Windows.Forms.Keys.Z:
        //            //this.Show();
        //            if(!this.Controls.Contains(this.Container))
        //            this.AddControls(this.Container);
        //            break;
                    
        //        default:
        //            break;
        //    }
        //}
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            //base.HandleKeyUp(e);
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.N:
                    //this.Hide();
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
            return;

            if (!this.Controls.Contains(this.Container))
                this.AddControls(this.Container);
            else
                this.RemoveControls(this.Container);
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
    
    
    class NameplateManagerOld
    {
        HashSet<GameObject> PreviousScene = new HashSet<GameObject>();// new SceneState();
        Dictionary<INameplateable, Nameplate> Cache = new Dictionary<INameplateable, Nameplate>();

        public void Update(SceneState scene)
        {
            //var added = this.PreviousScene != null ? scene.ObjectsDrawn.Except(this.PreviousScene.ObjectsDrawn) : scene.ObjectsDrawn;
            //var removed = this.PreviousScene != null ? this.PreviousScene.ObjectsDrawn.Except(scene.ObjectsDrawn) : new GameObject[] { };
            var added = scene.ObjectsDrawn.Except(this.PreviousScene);
            var removed = this.PreviousScene.Except(scene.ObjectsDrawn);
            Nameplate plate;

            foreach (var entity in added)
            {
                //var plate = Nameplate.Create(entity);
                //this.Cache[entity] = plate;
                if(!this.Cache.TryGetValue(entity, out plate))
                {
                    plate = Nameplate.Create(entity);
                    this.Cache[entity] = plate;
                }
                plate.Show();
            }
            foreach (var entity in removed)
            {
                if(this.Cache.TryGetValue(entity, out plate))
                    plate.Hide();
                //this.Cache.Remove(entity);
            }
            this.PreviousScene = scene.ObjectsDrawn;
        }

        public void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.EntityDespawned:
                    var entity = e.Parameters[0] as GameObject;
                    this.Cache[entity].Hide();
                    this.Cache.Remove(entity);
                    break;

                default:
                    break;
            }
        }
    }
}
