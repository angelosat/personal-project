using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class EntityTextManager : Control
    {
        //Dictionary<GameObject, Label> StackSizes = new Dictionary<GameObject, Label>();
        Dictionary<GameObject, RenderTarget2D> StackSizes = new Dictionary<GameObject, RenderTarget2D>();
        Queue<GameObject> ToValidate = new Queue<GameObject>();
        readonly Vector2 LabelSize = UIManager.Font.MeasureString("000");
        public override void Update()
        {
            base.Update();
            if (ToValidate.Any())
            {
                var gd = Game1.Instance.GraphicsDevice;
                while (ToValidate.Any())
                {
                    var sb = new SpriteBatch(gd);

                    var o = ToValidate.Dequeue();
                    var rt = new RenderTarget2D(gd, (int)LabelSize.X, (int)LabelSize.Y);
                    gd.Clear(Color.Red * .5f);
                    sb.Begin();
                    UIManager.DrawStringOutlined(sb, o.StackSize.ToString(), Vector2.Zero);// + border));
                    sb.End();
                    StackSizes[o] = rt;
                }
            }
            //var objects = Net.Client.Instance.Map.GetEntities();
            //foreach(var o in objects)
            //{

            //}
        }
        internal static void Draw(GameObject parent, int stackSize)
        {
            throw new NotImplementedException();
        }
        internal static void DrawStackSize(SpriteBatch sb, Camera camera, GameObject parent)
        {
            //DrawStackSizeTest(sb, camera, parent);
            //return;
            var border = 2;
            if (camera.Zoom <= 1)
                return;
            if (parent.StackSize <= 1)
                return;
            var text = parent.StackSize.ToString();
            var pos = camera.GetScreenPosition(parent.Global);// - new Vector2(UI.Icon.Cross.SourceRect.Width, UI.Icon.Cross.SourceRect.Height);// * camera.Zoom / 2; ;// -new Vector2(UI.Icon.Cross.SourceRect.Width / 2, rect.Height * camera.Zoom);
            var textSize = UIManager.Font.MeasureString(text) + Vector2.UnitX * (2 * border);
            var textbg = new Rectangle((int)(pos.X - textSize.X / 2), (int)pos.Y, (int)textSize.X, (int)textSize.Y);
            textbg.DrawHighlight(sb, Color.Black * .5f);
            UIManager.DrawStringOutlined(sb, text, new Vector2((int)(pos.X - textSize.X / 2 + border), (int)pos.Y));// + border));
        }
        internal static void DrawStackSizeTest(SpriteBatch sb, Camera camera, GameObject parent)
        {
            var border = 2;
            if (camera.Zoom <= 1)
                return;
            if (parent.StackSize <= 1)
                return;
            var text = parent.StackSize.ToString();
            var pos = camera.GetScreenPosition(parent.Global);// - new Vector2(UI.Icon.Cross.SourceRect.Width, UI.Icon.Cross.SourceRect.Height);// * camera.Zoom / 2; ;// -new Vector2(UI.Icon.Cross.SourceRect.Width / 2, rect.Height * camera.Zoom);
            var textSize = UIManager.Font.MeasureString(text) + Vector2.UnitX * (2 * border);
            var textbg = new Rectangle((int)(pos.X - textSize.X / 2), (int)pos.Y, (int)textSize.X, (int)textSize.Y);
            textbg.DrawHighlight(sb, Color.Black * .5f);
            var global = parent.Global + new Vector3(1,1,0);
            var entityDepth = camera.GetDrawDepth(parent.Map, global);
            var near = camera.GetNearDepth(parent.Map);
            var far = camera.GetFarDepth(parent.Map);
            var depthRange = Math.Abs(near - far);
            var depth = 1-(entityDepth / depthRange);// * camera.Zoom;
            //pos = Vector2.Zero;
            //pos.Y += parent.GetBounds(camera).Height;
            UIManager.DrawStringOutlined(sb, text, new Vector2(pos.X - textSize.X / 2 + border, pos.Y), depth: depth);// + border));
            //UIManager.DrawStringOutlined(sb, parent.StackSize.ToString(), pos, Vector2.UnitX*.5f);
        }
        public override void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            if (camera.Zoom <= 1)
                return;
           
            var objects = Net.Client.Instance.Map.GetEntities();
            foreach (var o in objects)
            {
                if (o.StackSize <= 1)
                    continue;
                if (!StackSizes.TryGetValue(o, out var texture))
                {
                    ToValidate.Enqueue(o);
                    continue;
                }
                var rt = StackSizes[o];
                var depth = camera.GetDrawDepth(o);
                var pos = camera.GetScreenPosition(o.Global);// - new Vector2(UI.Icon.Cross.SourceRect.Width, UI.Icon.Cross.SourceRect.Height);// * camera.Zoom / 2; ;// -new Vector2(UI.Icon.Cross.SourceRect.Width / 2, rect.Height * camera.Zoom);
                //sb.Draw(rt, pos, rt.Bounds, 1, Color.White, depth);
                sb.Draw(rt, Vector2.Zero, rt.Bounds, 0, Vector2.Zero, Vector2.One, Color.White, Color.White, Color.White, Color.White, Color.Transparent, SpriteEffects.None, depth); //color.Multiply(this.Tint)
            }
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.EntitySpawned:
                    this.Register(e.Parameters[0] as GameObject);
                    break;

                case Components.Message.Types.EntityDespawned:
                    this.Unregister(e.Parameters[0] as GameObject);
                    break;

                default:
                    break;
            }
        }

        private void Unregister(GameObject entity)
        {
            //this.StackSizes.TryGetValue(entity, l => l.Hide());
            //this.StackSizes.Remove(entity);
        }

        private void Register(GameObject entity)
        {
            //var l = new Label() { TextFunc = () => entity.StackSize.ToString() };
            //this.StackSizes.Add(entity, l);
            //l.Show();
        }
    }
}
