using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class MemorizedBlueprints
    {
        /// <summary>
        /// how should they decay? 
        /// when you gain memorization for one blueprint: 
        /// *reduce all other's values (by a fraction of the gained memorization value)? 
        /// *reduce only the lowest?
        /// *reduce only the oldest blueprint memory?
        /// *have a timer that reduces all non-memorized values
        /// *have a set amount of "memorization points" that can be allocated
        /// 
        /// should max be variable? 
        /// 
        /// </summary>

        public MemorizedBlueprints(params BlueprintMemory[] memories)
        {
            this.InnerList = new SortedSet<BlueprintMemory>(memories);
            //this.InnerList = new List<BlueprintMemory>(memories);
        }
        public MemorizedBlueprints()
        {
            this.MemorizedCapacity = 4; //TODO: relate to actor intelligence
            this.InnerList = new SortedSet<BlueprintMemory>();
            //this.InnerList = new List<BlueprintMemory>();

            ////Comparer<BlueprintMemory>.Create((m1, m2)=>{
            //    if (m1.Value < m2.Value) return -1;
            //    else if (m1.Value == m2.Value) return 0;
            //    else return 1;
            //}));
        // new List<BlueprintMemory>();
            //this.InnerList.Add()
        }

        int MemorizedCapacity { get; set; }
        int OccupiedCapacity
        {
            get
            {
                int n = 0;
                foreach (var mem in this.InnerList)
                    if (mem.Value > 100)
                        n += mem.MemorySize;
                return n;
            }
        }

        //List<BlueprintMemory> InnerList { get; set; } //make it sorted according to memorization value?
        SortedSet<BlueprintMemory> InnerList { get; set; }
        public List<GameObject> GetMemorizedBlueprints()
        {
            var list = new List<GameObject>();
            int currentSize = 0;
            foreach (var mem in this.InnerList)
            {
                if (currentSize > MemorizedCapacity)
                    break;
                if (mem.Value < 100)
                    continue;

                //list.Add(mem.Blueprint);
                list.Add(GameObject.Objects[mem.Blueprint]);
                currentSize += mem.MemorySize;
            }
            return list;
        }

        public void Gain(GameObject crafter, GameObject bp, int value)
        {
            BlueprintMemory memory = (from mem in this.InnerList
                                      //where mem.Blueprint.ID == bp.ID
                                      where mem.Blueprint == bp.IDType
                                      select mem).FirstOrDefault();
            if (memory is null)
            {
                memory = new BlueprintMemory(bp.IDType);
                this.InnerList.Add(memory);
            }
            int intellect = crafter.GetComponent<AttributesComponent>().GetAttribute(AttributeDef.Intelligence).Level; StatsComponent.GetStat(crafter, AttributeDef.Intelligence.Name);

            int final = value * intellect;
            memory.Value += final;// (int)(value * intellect);
            crafter.Net.EventOccured(Message.Types.Memorization, crafter, bp, final);
            //reduce other memories values here?
            var toReduce = (from n in this.InnerList
                      //      where n.Value < 100
                            where n != memory
                            select n).ToList();
            //toReduce.Sort((bp1, bp2) =>
            //{
            //    if (bp1.Value < bp2.Value) return -1;
            //    else if (bp1.Value == bp2.Value) return 0;
            //    else return 1;
            //});
            BlueprintMemory lowest = toReduce.FirstOrDefault();
            if (lowest != null)
            {
                lowest.Value -= value;
                if (lowest.Value < 0)
                    this.InnerList.Remove(lowest);
            }
        }

        public MemorizedBlueprints Clone()
        {
            var n = new MemorizedBlueprints();
            foreach (var mem in this.InnerList)
                n.InnerList.Add(mem);
            return n;
        }

        public override string ToString()
        {
            string text = "";
            text += "Capacity: " + this.MemorizedCapacity.ToString() + "\n";
            text += "Occupied: " + this.OccupiedCapacity.ToString() + "\n";
            foreach (var mem in this.InnerList)
                text += mem.ToString() + "\n";
            return text.TrimEnd('\n');
        }
    }
}
