using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class UIDebugCommands : Panel
    {
        static UIDebugCommands Instance;
        static UIDebugCommands()
        {
            Instance = new UIDebugCommands();
            Instance.ToWindow("Debug commands");
        }
        public UIDebugCommands()
        {
            this.AutoSize = true;
            this.AddControlsVertically(
                new Button("Spawn plants") { LeftClickAction = () => SpawnVegetation(Server.Instance.Map) },
                new Button("Grow selected") { LeftClickAction = () => GrowPlants(UISelectedInfo.GetSelectedEntities().Select(t=>t.RefID)) }
                );
        }

        private void GrowPlants(IEnumerable<int> enumerable)
        {
            foreach (var id in enumerable)
            {
                var plant = Server.Instance.GetNetworkObject(id);
                plant.TryGetComponent<Components.PlantComponent>(c => c.FinishGrowing(plant));
                plant.TryGetComponent<Components.TreeComponent>(c => c.FinishGrowing(plant));
                plant.Sync(Server.Instance);
            }
        }
        internal static void Refresh()
        {
            var win = Instance.GetWindow();
            win.Show();
            win.Location = UIManager.Mouse;
        }

        internal static void SpawnVegetation(IMap map)
        {
            GeneratorPlants.GeneratePlants(map);
        }
    }
}
