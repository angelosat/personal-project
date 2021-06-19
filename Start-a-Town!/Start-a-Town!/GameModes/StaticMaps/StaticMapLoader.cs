using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.GameModes.StaticMaps
{
    public class StaticMapLoadingProgressToken
    {
        //class LoadingToken
        //{

        //}

        //StaticMap Map;

        //public void Load(StaticMap map)
        //{
        //    this.Map = map;
        //    //var token = new LoadingToken();
        //    //return token;
        //}
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
