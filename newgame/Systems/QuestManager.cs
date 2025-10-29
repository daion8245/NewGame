namespace newgame.Systems
{
    internal delegate void QuestMobDeadHandler(string mobName);

    internal class QuestManager
    {
        private event QuestMobDeadHandler? MobDead;

        public QuestManager()
        {
            MobDead += OnMonsterDead;
        }

        public void NotifyMonsterDeath(string mobName)
        {
            MobDead?.Invoke(mobName);
        }

        private void OnMonsterDead(string mobName)
        {
        }
    }
}
