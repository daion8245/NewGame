using newgame.Characters;
using newgame.Items;

namespace newgame.Systems
{
    internal delegate void QuestMobDeadHandler(string mobName);

    internal sealed class Quest
    {
        private readonly List<QuestReward> _rewardItems;

        public Quest(
            string name,
            string description,
            string targetMobName,
            int requiredCount,
            int rewardGold,
            IEnumerable<QuestReward>? rewardItems = null)
        {
            Name = name;
            Description = description;
            TargetMobName = targetMobName;
            RequiredCount = Math.Max(1, requiredCount);
            RewardGold = Math.Max(0, rewardGold);
            _rewardItems = rewardItems?.ToList() ?? new List<QuestReward>();
        }

        public string Name { get; }

        public string Description { get; }

        public string TargetMobName { get; }

        public int RequiredCount { get; }

        public int RewardGold { get; }

        public int CurrentCount { get; private set; }

        public bool IsRewardGranted { get; private set; }

        public bool IsCompleted => CurrentCount >= RequiredCount;

        public IReadOnlyList<QuestReward> RewardItems => _rewardItems;

        public bool Matches(string mobName)
        {
            return string.Equals(TargetMobName, mobName, StringComparison.OrdinalIgnoreCase);
        }

        public bool TryAdvance()
        {
            if (IsCompleted)
            {
                return false;
            }

            CurrentCount++;
            if (CurrentCount > RequiredCount)
            {
                CurrentCount = RequiredCount;
            }

            return true;
        }

        public void MarkRewardGranted()
        {
            IsRewardGranted = true;
        }
    }

    internal readonly struct QuestReward
    {
        public QuestReward(ItemType itemType, int count)
        {
            ItemType = itemType;
            Count = Math.Max(1, count);
        }

        public ItemType ItemType { get; }

        public int Count { get; }
    }

    internal class QuestManager
    {
        private readonly List<Quest> quests = new();
        private event QuestMobDeadHandler? MobDead;

        public QuestManager()
        {
            MobDead += OnMonsterDead;
            InitializeDefaultQuests();
        }

        public void NotifyMonsterDeath(string mobName)
        {
            MobDead?.Invoke(mobName);
        }

        public void ShowQuestBoard()
        {
            if (quests.Count == 0)
            {
                Console.WriteLine("등록된 퀘스트가 없습니다.");
                return;
            }

            Console.WriteLine("┏━━━━━━━━━━━━━━━┓");
            Console.WriteLine("┃   퀘스트 현황   ┃");
            Console.WriteLine("┗━━━━━━━━━━━━━━━┛");

            foreach (Quest quest in quests)
            {
                Console.WriteLine($"[{quest.Name}]");
                Console.WriteLine($" 설명 : {quest.Description}");
                Console.WriteLine($" 목표 : {quest.TargetMobName} {quest.CurrentCount}/{quest.RequiredCount}");
                Console.WriteLine($" 보상 : {BuildRewardSummary(quest)}");
                Console.WriteLine($" 상태 : {(quest.IsRewardGranted ? "완료" : quest.IsCompleted ? "완료 (보상 수령 전)" : "진행 중")}");
                Console.WriteLine();
            }
        }

        private void InitializeDefaultQuests()
        {
            quests.Add(new Quest(
                name: "슬라임 킬러",
                description: "슬라임을 1마리 죽이시오",
                targetMobName: "슬라임",
                requiredCount: 1,
                rewardGold: 20,
                rewardItems: new[] { new QuestReward(ItemType.F_POTION_LOW_HP, 1) }));
        }

        private void OnMonsterDead(string mobName)
        {
            if (string.IsNullOrWhiteSpace(mobName))
            {
                return;
            }

            foreach (Quest quest in quests)
            {
                if (!quest.Matches(mobName))
                {
                    continue;
                }

                bool progressed = quest.TryAdvance();
                if (progressed)
                {
                    Console.WriteLine($"[퀘스트] {quest.Name} 진행도 {quest.CurrentCount}/{quest.RequiredCount}");
                }

                if (quest.IsCompleted && !quest.IsRewardGranted)
                {
                    Console.WriteLine($"[퀘스트] {quest.Name} 완료! 보상을 획득했습니다.");
                    GrantRewards(quest);
                }
            }
        }

        private void GrantRewards(Quest quest)
        {
            Player? player = GameManager.Instance.player;
            if (player == null)
            {
                return;
            }

            if (quest.RewardGold > 0)
            {
                player.MyStatus.gold += quest.RewardGold;
            }

            foreach (QuestReward reward in quest.RewardItems)
            {
                Inventory.Instance.AddItem(reward.ItemType, reward.Count);
            }

            quest.MarkRewardGranted();

            Console.WriteLine($"[퀘스트] 보상 : {BuildRewardSummary(quest)}");
        }

        private static string BuildRewardSummary(Quest quest)
        {
            List<string> rewardParts = new List<string>();

            if (quest.RewardGold > 0)
            {
                rewardParts.Add($"{quest.RewardGold}G");
            }

            foreach (QuestReward reward in quest.RewardItems)
            {
                string itemName = Inventory.Instance.GetItemName(reward.ItemType);
                if (string.IsNullOrWhiteSpace(itemName))
                {
                    itemName = reward.ItemType.ToString();
                }

                rewardParts.Add($"{itemName} x{reward.Count}");
            }

            return rewardParts.Count == 0 ? "보상 없음" : string.Join(", ", rewardParts);
        }
    }
}
