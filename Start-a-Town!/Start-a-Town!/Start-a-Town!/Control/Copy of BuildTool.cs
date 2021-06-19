using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.ConstructionSystem;
using Start_a_Town_.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_.Control
{
    class BuildTool : ITool
    {
        public ConstructionPreview Preview;

        public void Execute()
        {
            //Console.WriteLine("GAMW TO KERATO");
            if (Preview != null)
            {
                Console.WriteLine(Preview.MapCoords.ToString());
                Player.Instance.TaskAssign(new Task(Player.Instance, new TaskArgs(Map.TileAt(Preview.MapCoords), Preview, 3)));
            }
        }
    }
}
