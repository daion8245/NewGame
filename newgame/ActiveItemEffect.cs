
namespace newgame
{
    public enum ConsumeType
    {
        BattleStart, // 전투 시작 시 1턴 소모
        PerTurn      // 매 턴마다 1턴 소모
    }

    internal class ActiveItemEffect
    {
        public ItemType ItemType { get; private set; }
        public int RemainingTurn { get; set; }
        public int TotalBonus { get; set; }
        public ConsumeType ConsumeType { get; private set; }

        public ActiveItemEffect(Item item)
        {
            ItemType = item.ItemType;
            RemainingTurn = item.ItemUsedCount;
            TotalBonus = item.ItemStatus;
            ConsumeType = GetConsumeType(item.ItemType); // 종류별로 설정
        }

        private static ConsumeType GetConsumeType(ItemType type)
        {
            switch (type)
            {
                case ItemType.T_POTION_EXPUP:
                    return ConsumeType.BattleStart;
                case ItemType.T_POTION_ATKUP:
                    return ConsumeType.PerTurn;
                default:
                    return ConsumeType.PerTurn;
            }
        }
    }
}
