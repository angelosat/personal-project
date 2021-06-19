using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class UINpcFrameContainer : GroupBox
    {
        List<Actor> PrevActors = new List<Actor>();
        const int Spacing = 5;
        public UINpcFrameContainer()
        {
            //this.BackgroundColorFunc = () => Color.Black * .5f;
        }
        public override void Update()
        {
            if (!Camera.DrawnOnce)
                return;
            var actors = Net.Client.Instance.Map.Town.GetAgents().Where(a=>a!=null).ToList();
            var toInit = actors.Where(a => !this.PrevActors.Contains(a));
            foreach (var a in toInit)
                this.AddControlsTopRight(Spacing, new UINpcFrame(a));// InitControl(a));
            var toRemove = this.PrevActors.Where(a => !actors.Contains(a));
            if (toRemove.Any())
            {
                foreach (var a in toRemove)
                {
                    this.Controls.RemoveAll(c => c.Tag == a);
                }
                this.AlignLeftToRight();
            }
            this.PrevActors = actors;
            //foreach (var p in this.Controls)
            //{
            //    //p.Invalidate();
            //    var pbox = p as PictureBox;
            //    var npc = pbox.Tag as GameObject;
            //    //npc.Body.RenderIcon(npc, pbox.Sprite as RenderTarget2D);
            //    if(pbox.Sprite != null)
            //    {

            //    }
            //    //    ////pbox.Sprite = npc.Body.RenderIcon(npc);
            //}
            base.Update();
        }
        //internal override void OnGameEvent(GameEvent e)
        //{
        //    if (e.Type == Components.Message.Types.EntitySpawned)
        //    {
        //        var npc = e.Parameters[0] as GameObject;
        //        if (npc.HasComponent<NpcComponent>())
        //        {
        //            this.AddControlsTopRight(InitControl(npc));
        //        }
        //    }
        //    else if(e.Type == Components.Message.Types.EntityDespawned)
        //    {
        //        var npc = e.Parameters[0] as GameObject;
        //        if (this.Controls.RemoveAll(c => c.Tag == npc)>0)
        //        {
        //            this.AlignLeftToRight();
        //        }
        //    }
        //}

        //private static PictureBox InitControl(GameObject npc)
        //{
        //    //var minrect = npc.Body.GetMinimumRectangle();
        //    //GraphicsDevice gd = Game1.Instance.GraphicsDevice;
        //    //var texture = new RenderTarget2D(gd, (int)minrect.Width, (int)minrect.Height);

        //    //return new PictureBox(texture, scale: 2)
        //    return new PictureBox(npc.Body.RenderIcon(npc), scale: 2)
        //    //return new PictureBox(new Vector2(32, 48), (r) => npc.Body.RenderIcon(npc, r))
        //    {
        //        Tag = npc,
        //        LeftClickAction = () =>
        //        {
        //            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
        //                UISelectedInfo.AddToSelection(new TargetArgs(npc));
        //            //Hud.AddToSelection(target);
        //            else
        //                UISelectedInfo.Refresh(new TargetArgs(npc));
        //            //UISelectedInfo.Refresh(new TargetArgs(npc));
        //        }
        //    };
        //}
        
    }
}
