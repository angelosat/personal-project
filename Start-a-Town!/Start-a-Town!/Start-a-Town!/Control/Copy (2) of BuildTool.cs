using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.ConstructionSystem;
using Start_a_Town_.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_.Control
{
    class BuildTool : ControlTool
    {
        //store a reference to the construction to be placed, check mouseover tile against allowed tiles for the construction to be placed on

        public ConstructionPreview Preview;
        public event EventHandler<EventArgs> Destroy;

        public void OnDestroy()
        {
            if (Destroy != null)
                Destroy(this, EventArgs.Empty);
        }

        public void Execute()
        {
            Console.WriteLine("BUILD TOOL STANDING BY");
            //if (Preview != null)
            //{
            //    Console.WriteLine(Preview.MapCoords.ToString());
            //    Player.Instance.TaskAssign(new Task(Player.Instance, new TaskArgs(Map.TileAt(Preview.MapCoords), Preview, 3)));
            //    OnDestroy();
            //}
        }
    }
}
