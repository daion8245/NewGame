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
            // TODO: 퀘스트 진행 로직을 이곳에 구현한다.
        }
    }
}
