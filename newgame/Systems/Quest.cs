using System;
using System.Collections.Generic;
using newgame.Items;
using newgame.Services;

namespace newgame.Systems
{
    internal sealed class Quest
    {
        private readonly List<(ItemType type, int count)> _itemRewards = new();

        public Quest(string name, string description, string targetMobName, int requiredCount, int rewardGold)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("퀘스트 이름은 비어 있을 수 없습니다.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(targetMobName))
            {
                throw new ArgumentException("대상 몬스터 이름은 비어 있을 수 없습니다.", nameof(targetMobName));
            }

            if (requiredCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredCount));
            }

            Name = name;
            Description = description ?? string.Empty;
            TargetMobName = targetMobName;
            RequiredCount = requiredCount;
            RewardGold = rewardGold;
        }

        public string Name { get; }
        public string Description { get; }
        public string TargetMobName { get; }
        public int RequiredCount { get; }
        public int RewardGold { get; }
        public int CurrentCount { get; private set; }
        public bool IsAccepted { get; private set; }
        public bool RewardClaimed { get; private set; }
        public bool IsCompleted => IsAccepted && CurrentCount >= RequiredCount;
        public bool CanClaimReward => IsCompleted && !RewardClaimed;

        public IReadOnlyList<(ItemType type, int count)> ItemRewards => _itemRewards;

        public void AddItemReward(ItemType type, int count = 1)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            _itemRewards.Add((type, count));
        }

        public bool Accept()
        {
            if (IsAccepted || RewardClaimed)
            {
                return false;
            }

            IsAccepted = true;
            CurrentCount = 0;
            return true;
        }

        public bool TryRegisterKill(string mobName)
        {
            if (!IsAccepted || RewardClaimed)
            {
                return false;
            }

            if (!string.Equals(mobName, TargetMobName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (CurrentCount < RequiredCount)
            {
                CurrentCount++;
                return true;
            }

            return false;
        }

        public bool TryClaimReward()
        {
            if (!CanClaimReward)
            {
                return false;
            }

            var player = GameManager.Instance.RequirePlayer();
            player.MyStatus.gold += RewardGold;

            foreach ((ItemType type, int count) in _itemRewards)
            {
                Inventory.Instance.AddItem(type, count);
            }

            RewardClaimed = true;
            return true;
        }
    }
}
