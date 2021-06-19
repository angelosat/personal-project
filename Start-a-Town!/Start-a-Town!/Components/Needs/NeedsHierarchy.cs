using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components
{
    class NeedsHierarchy //: Dictionary<string, NeedsCollection>
    {
        public Dictionary<string, NeedsCollection>.ValueCollection Values { get { return this.Inner.Values; } }
        public Dictionary<string, NeedsCollection> Inner = new Dictionary<string, NeedsCollection>();

        public NeedsHierarchy Add(string name, NeedsCollection needs)
        {
            this.Inner.Add(name, needs);
            return this;
        }

        public void Update(GameObject parent)
        {
            foreach (var layer in this.Inner)
                foreach (var need in layer.Value)
                    need.Value.Tick(parent);
        }

        public override string ToString()
        {
            string text = "";
            foreach (var layer in this.Inner)
            {
                text += layer.Key + '\n';
                foreach (var need in layer.Value)
                    text += "   [" + need.Key + ": " + need.Value.ToString() + " (" + (need.Value.Decay <= 0 ? "+" : "-") + need.Value.Decay.ToString("n2") + ")]\n";
            }
            return text.TrimEnd('\n');
        }

        //public NeedsHierarchy Clone()
        //{
        //    NeedsHierarchy copy = new NeedsHierarchy();
        //    foreach (var layer in this.Inner)
        //    {
        //        NeedsCollection newLayer = new NeedsCollection();
        //        foreach (var need in layer.Value)
        //            newLayer.Add(need.Key, need.Value.Clone() as Need);// new Need(need.Value.ID, need.Value.Name, need.Value.Value, need.Value.Decay, need.Value.Tolerance));
        //        copy.Inner.Add(layer.Key, newLayer);
        //    }
        //    return copy;
        //}

        public List<Need> GetNeeds(Func<Need, bool> layerTest)
        {
           // List<Need> needs = new List<Need>();

            foreach (var layer in this.Inner)
                if (!layer.Value.Values.All(layerTest) || this.Inner.Last().Value == layer.Value) // if the layer fails the test or it's the topmost one
                {
                    return layer.Value.Values
                        .TakeWhile(n => n.Value < n.Tolerance)
                        .OrderBy(n => n.Value)
                        .ToList();
                    //foreach (var need in layer.Value)
                    //    needs.Add(need.Value);
                    //break;
                }
            return new List<Need>();
            //return needs.OrderBy(n=>n.Value).ToList();
        }

        public GroupBox GetUI(GameObject entity)
        {
            var box = new GroupBox();
            foreach (var item in this.Inner)
            {
                var panel = new PanelLabeled(item.Key) { Location = box.BottomLeft };
                foreach (var n in item.Value.Values)
                {
                    var ui = n.GetUI(entity);
                    ui.Location = panel.Controls.BottomLeft;
                    panel.AddControls(ui);
                }
                box.AddControls(panel);
            }
            return box;
        }
    }
}
