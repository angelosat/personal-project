using System;

namespace Start_a_Town_.GameModes.StaticMaps
{
    public class StaticMapLoadingProgressToken
    {
        public string Text;
        public float Percentage;
        public Action<StaticMapLoadingProgressToken> Callback = t => { };
        public StaticMapLoadingProgressToken()
        {

        }
        public StaticMapLoadingProgressToken(Action<StaticMapLoadingProgressToken> callback)
        {
            this.Callback = callback;
        }
        public void Refresh()
        {
            this.Callback(this);
        }
    }
}
