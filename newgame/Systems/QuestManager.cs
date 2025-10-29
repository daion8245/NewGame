using System;
using System.Collections.Generic;
using System.Linq;
using newgame.Items;
using newgame.UI;

namespace newgame.Systems
{
    internal delegate void QuestMobDeadHandler(string mobName);

    internal class QuestManager
    {
        private readonly List<Quest> quests = new();
        private event QuestMobDeadHandler? MobDead;

        public QuestManager()
        {
            MobDead += OnMonsterDead;
            SeedDefaultQuests();
        }

        public IEnumerable<Quest> GetAvailableQuests()
        {
            return quests.Where(q => !q.IsAccepted && !q.RewardClaimed);
        }

        public IEnumerable<Quest> GetActiveQuests()
        {
            return quests.Where(q => q.IsAccepted && !q.RewardClaimed);
        }

        public IEnumerable<Quest> GetReadyToClaimQuests()
        {
            return quests.Where(q => q.CanClaimReward);
        }

        public bool TryAcceptQuest(Quest quest)
        {
            if (quest == null)
            {
                return false;
            }

            if (!quests.Contains(quest))
            {
                return false;
            }

            bool accepted = quest.Accept();
            if (accepted)
            {
                UiHelper.TxtOut([
                    $"[퀘스트] '{quest.Name}' 퀘스트를 수락했습니다.",
                    $"목표: {quest.TargetMobName} {quest.RequiredCount}마리 처치"
                ], SlowTxtOut: false);
            }

            return accepted;
        }

        public bool TryClaimReward(Quest quest)
        {
            if (quest == null || !quests.Contains(quest))
            {
                return false;
            }

            bool claimed = quest.TryClaimReward();
            if (claimed)
            {
                List<string> rewardLines = new List<string>
                {
                    $"[퀘스트] '{quest.Name}' 보상을 획득했습니다.",
                    $"획득 골드: {quest.RewardGold}"
                };

                foreach ((ItemType type, int count) in quest.ItemRewards)
                {
                    string itemName = Inventory.Instance.GetItemName(type);
                    rewardLines.Add($"아이템: {itemName} x {count}");
                }

                UiHelper.TxtOut(rewardLines.ToArray(), SlowTxtOut: false);
                UiHelper.WaitForInput();
            }

            return claimed;
        }

        public void NotifyMonsterDeath(string mobName)
        {
            if (string.IsNullOrWhiteSpace(mobName))
            {
                return;
            }

            MobDead?.Invoke(mobName);
        }

        private void OnMonsterDead(string mobName)
        {
            foreach (Quest quest in GetActiveQuests())
            {
                bool progressed = quest.TryRegisterKill(mobName);
                if (!progressed)
                {
                    continue;
                }

                Console.WriteLine($"[퀘스트] {quest.Name} 진행도: {quest.CurrentCount}/{quest.RequiredCount}");
                if (quest.IsCompleted)
                {
                    Console.WriteLine($"[퀘스트] {quest.Name} 완료! 여관에서 보상을 받으세요.");
                }
            }
        }

        private void SeedDefaultQuests()
        {
            Quest slimeQuest = new Quest(
                "슬라임 킬러",
                "슬라임을 1마리 죽이시오",
                "슬라임",
                1,
                20);

            slimeQuest.AddItemReward(ItemType.F_POTION_LOW_HP, 1);
            quests.Add(slimeQuest);
        }
    }
}
