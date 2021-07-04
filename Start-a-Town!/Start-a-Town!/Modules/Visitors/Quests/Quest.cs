namespace Start_a_Town_
{
    class Quest
    {
        public QuestDef QuestGiver;
        QuestObjective[] Requirements;
        ItemDefMaterialAmount Reward;
       
        public Quest(QuestDef questGiver, QuestObjective[] requirements, ItemDefMaterialAmount reward)
        {
            QuestGiver = questGiver;
            Requirements = requirements;
            Reward = reward;
        }
    }
}
