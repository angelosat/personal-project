using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class QuestObjectiveItem : QuestObjective
    {
        public ItemDefMaterialAmount Objective;
        public QuestObjectiveItem(QuestDef parent) : base(parent)
        {

        }
        public QuestObjectiveItem(QuestDef parent, ItemDefMaterialAmount requirement):base(parent)
        {
            Objective = requirement;
        }

        public override string Text => string.Format("Gather {0}", this.Objective.ToString());

        public override int GetValue()
        {
            return this.Objective.Def.BaseValue * this.Objective.Material.Value * this.Objective.Amount ;
        }
        public override void Write(BinaryWriter w)
        {
            this.Objective.Write(w);
        }
        public override ISerializable Read(BinaryReader r)
        {
            this.Objective = new ItemDefMaterialAmount(r);
            return this;
        }
        protected override void AddSaveData(SaveTag save)
        {
            save.Add(this.Objective.Save("Objective"));
        }
        protected override void Load(SaveTag load)
        {
            this.Objective = new(load["Objective"]);
        }

        public override bool IsCompleted(Actor actor)
        {
            return actor.Inventory.Count(this.Objective.Def, this.Objective.Material) >= this.Objective.Amount;
        }
        internal override void TryComplete(Actor actor, OffsiteAreaDef area)
        {
            if (this.IsCompleted(actor))
                return;
            var item = this.Objective.Def;
            var mat = this.Objective.Material;
            if (!area.CanBeFound(item, mat, out var chance))
                return;
            actor.Loot(item.CreateFrom(mat), area);
        }
        internal override IEnumerable<ObjectAmount> GetQuestItemsInInventory(Actor actor)
        {
            var inv = actor.Inventory;
            foreach(var i in inv.Take(e=> e.Def == this.Objective.Def && e.PrimaryMaterial == this.Objective.Material, this.Objective.Amount))
                yield return i;
        }
    }
}
