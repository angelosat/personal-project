using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class FpsCounterOld
    {
        public static float deltaFPSTime = 0, speed = 1;
        public int Fps;

        public void Update(Game1 game, GameTime gt)
        {
            return;
            //float elapsed = (float)gt.ElapsedGameTime.TotalSeconds;

            //Fps = (int)(1 / elapsed); //or float
            //deltaFPSTime += elapsed;

            //// calculate delta
            ////float timeDifference = (float)gt.ElapsedGameTime.TotalMilliseconds - timePrevious;
            ////timePrevious = (float)gt.ElapsedGameTime.TotalMilliseconds;
            ////deltaTime = (timeDifference / 1000f) * 60 * speed;

            //GlobalVars.DeltaTime = ((float)gt.ElapsedGameTime.TotalMilliseconds / 1000f) * Engine.TicksPerSecond * speed;

            //if (deltaFPSTime > 1)
            //{
            //    GlobalVars.Fps = Fps; 
            //    //game.Window.Title = "I am running at  <" + Global.Fps.ToString() + "> FPS, deltaTime = <" + Global.DeltaTime.ToString() + ">";
            //    game.Window.Title = "FPS: " + Fps.ToString();
            //    //OnUpdated();
            //    deltaFPSTime -= 1;
            //}

            
        }
    }
}
