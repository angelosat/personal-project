using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using Start_a_Town_.GameModes.StaticMaps;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    public class MapThumb : IContextable, ITooltippable, INameplateable, IDisposable
    {
        public IMap Map;
        public Sprite[] Sprites;
        bool Valid = true;
        //const int Valid = 1, Invalid = 0;
        //int State = Valid;
        static public int CurrentZoom = 0;

        //static public int Size = (int)((Engine.ChunkRadius + Engine.ChunkRadius + 1) * Chunk.Size * Tile.Width * (1 / 8f));
        //static public int Size { get { return (int)(StaticMap.SizeInBlocks * Block.Width * (1 / (8f * Math.Pow(2, CurrentZoom)))); } }
        public int Size { get { return (int)(this.Map.GetSizeInChunks() * Chunk.Size * Block.Width * (1 / (8f * Math.Pow(2, CurrentZoom)))); } }

        public Vector3 ObjectDimensions { get { return new Vector3(Size, Size / 2, Size / 2); } }

        public void Dispose()
        {
            foreach (var sprite in Sprites)
                if (!sprite.IsNull())
                    sprite.Dispose();
        }

        #region INameplateable
        public string Name
        { get { return Map.GetName().Length > 0 ? Map.GetName() : Map.GetOffset().ToString(); } }
        public Vector3 Global
        { get { return new Vector3(Map.GetOffset(), 0); } }
        public Color GetNameplateColor()
        {
            return Color.White;
        }
        public Rectangle GetBounds(Camera camera)
        {
            //Rectangle rect = camera.GetScreenBounds(Global, Sprites[CurrentZoom].Texture.Bounds);// -new Vector2(Sprites[0].Texture.Bounds.Width / 2, Sprites[0].Texture.Bounds.Height / 2);
            //return new Rectangle(rect.X - rect.Width / 2, rect.Y - rect.Height / 2, rect.Width, rect.Height);

            Vector2 screenPos = Coords.GetScreenCoords(Global, camera, new Vector3(Size, Size / 2, Size / 2));
            //return new Rectangle((int)screenPos.X, (int)screenPos.Y, Size, Size);
            //return new Rectangle((int)screenPos.X - Size / 2, (int)screenPos.Y - Size / 2, Size, Size);
            return new Rectangle((int)screenPos.X, (int)screenPos.Y, 1, 1);

            Rectangle bounds = camera.GetScreenBounds(Global, new Rectangle(0, 0, Size, Size));
            return bounds;
            Vector2 pos = new Vector2(bounds.X, bounds.Y);
            return new Rectangle((int)pos.X, (int)pos.Y, 0, 0);
        }
        public void OnNameplateCreated(Nameplate plate)
        {
            //plate.LeftClickAction = () =>
            //    {
            //        WorldScreenUI.Instance.Initialize(this.Map);
            //    };
            plate.Controls.Add(new Label()
            {
                Font = UIManager.FontBold,
                Text = this.Name,
                MouseThrough = true,
                LeftClickAction = () =>
                {
                    //WorldScreenUI.Instance.Initialize(this.Map);
                }
            });
        }
        public void DrawNameplate(SpriteBatch sb, Rectangle viewport, Nameplate plate)
        {
            plate.Draw(sb, viewport);
        }
        #endregion

        public void Invalidate()
        {
           // System.Threading.Thread.MemoryBarrier();
            this.Valid = false;
          //  System.Threading.Thread.MemoryBarrier();
            //System.Threading.Interlocked.Exchange(ref State, Invalid);// this.State = Invalid;// false;
        }

        public void Update()
        {
            //if (System.Threading.Interlocked.Exchange(ref State, Valid) == Invalid)

            if (Valid)
                return;

            Valid = true;
            Map.GenerateThumbnails(Map.GetFullPath());//Map.DirectoryInfo.FullName);
            Map.LoadThumbnails();//Map.GetFullPath());
        }

        public MapThumb(IMap map)
        {
            this.Map = map;
            this.Sprites = new Sprite[3];
            //wasworking      Nameplate.Create(this);
        }

        public void Draw(SpriteBatch sb, Camera camera)
        {
     //       Texture2D thumb = Sprites[1].Texture;
            Sprite thumb = Sprites[CurrentZoom];
            Vector2 screenLoc;
            Rectangle screenBounds;
            if (thumb == null)
            {
                return;
                Texture2D hourglass = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/cursor-hourglass");
                screenLoc = camera.GetScreenPosition(new Vector3(Map.GetOffset(), 0)) - new Vector2(hourglass.Width / 2, hourglass.Height / 2);
                screenBounds = new Rectangle((int)screenLoc.X, (int)screenLoc.Y, hourglass.Width, hourglass.Height);
                sb.Draw(hourglass, screenBounds, null, Color.White, (float)(4 * Math.PI * DateTime.Now.Millisecond / 2000f), new Vector2(hourglass.Width / 2, hourglass.Height / 2), SpriteEffects.None, 0);
                Rectangle hitbounds = new Rectangle(screenBounds.X - hourglass.Width / 2, screenBounds.Y - hourglass.Height / 2, hourglass.Width, hourglass.Height);
                if (hitbounds.Intersects(UIManager.MouseRect))
                {
                    //sb.Draw(UIManager.Highlight, screenBounds, null, new Color(0.5f, 0.5f,0.5f,0.5f), 0, new Vector2(5), SpriteEffects.None, 0);
                    Controller.Instance.MouseoverNext.Object = this; 
                    sb.Draw(hourglass, screenBounds, null, new Color(1, 1, 1, 0.5f), (float)(4 * Math.PI * DateTime.Now.Millisecond / 2000f), new Vector2(hourglass.Width / 2, hourglass.Height / 2), SpriteEffects.None, 0);
                }
                return;
            }
           // GetBounds(camera).DrawHighlight(sb);
            Texture2D tex = thumb.Texture;
            //screenLoc = camera.GetScreenBounds(new Vector3(Map.Coordinates, 0)) - new Vector2(tex.Width / 2, tex.Height / 2);
            screenLoc = Coords.GetScreenCoords(new Vector3(Map.GetOffset(), 0), camera, ObjectDimensions) - new Vector2(tex.Width / 2, tex.Height / 2);
            screenBounds = new Rectangle((int)screenLoc.X, (int)screenLoc.Y, tex.Width, tex.Height);
            //sb.Draw(tex, screenLoc, Color.White);
            sb.Draw(tex, screenLoc, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, new Vector3(Map.GetOffset(), 0).GetDrawDepth(this.Map, camera));
            if (Rooms.WorldScreen.Instance.Map == Map)
                sb.Draw(tex, screenLoc, new Color(1, 1, 1, 0.5f));
            if (thumb.HitTest(camera, 1, screenBounds, UIManager.MouseRect))
            {
                Controller.Instance.MouseoverNext.Object = this;//Map// new MapThumb(map.Value);
                sb.Draw(tex, screenLoc, new Color(1, 1, 1, 0.5f));
            }
        }

        public void GetContextActions(ContextArgs a)
        {
            a.Actions.Add(
                new ContextAction(
                    //name: () => "Refresh thumbnail",
                    "Refresh thumbnail",
                    action: () =>
                    {
                        //float progress = 0;
                        LoadingBox box = new LoadingBox()
                        {
                            LocationFunc = () => Coords.GetScreenCoords(new Vector3(Map.GetOffset(), 0), ScreenManager.CurrentScreen.Camera, ObjectDimensions),
                            ProgressFunc = () => Map.LoadProgress.ToString("##0%"),
                            TextFunc = () => "Loading map...",
                            TintFunc = () => Color.Lerp(Color.Red, Color.Lime, Map.LoadProgress),
                        };
                        box.Show();
                        System.Threading.Tasks.Task.Factory.StartNew(()=>
                        {
                            // TODO: generate and send thumbnails from server
                           // Engine.Map = Map;
                           // Map.ActiveChunks = new System.Collections.Concurrent.ConcurrentDictionary<Vector2, Chunk>();
                           // ChunkLoader.Restart();
                           // ChunkLoader.ForceLoad(Map, new System.Threading.CancellationToken(), (chunk) => chunk.Update(Map));

                           
                           // Map.Update();

                           // this.Invalidate();
                           // box.Hide();

                        });
                        return true;
                    }
            ));
        }

        

        public void GetTooltipInfo(Tooltip tooltip)
        {
            this.Map.GetTooltipInfo(tooltip);
        }
    }
}
