using Newtonsoft.Json;

namespace newgame
{
    // 아이템 타입 설정 규칙
    // 타입명은 [0]_[1]_[2]
    // 0 : 지속여부 -> T, F
    // 1 : 종류
    // 2 : 효과
    public enum ItemType
    {
        NONE,           // 없음
        #region 포션
        F_POTION_HP,      // 회복 물약 - 지속여부 : false
        T_POTION_EXPUP,   // 경험치 획득량 증가 물약 - 지속여부 : true
        T_POTION_ATKUP,   // 공격력 증가 물약 - 지속여부 : true
        #endregion
        #region 기타
        F_ETC_RESETNAME, // 닉네임 변경권 - 지속여부 : false
        #endregion
    }

    [JsonObject(MemberSerialization.Fields)]
    internal class Item
    {
        // 아이템 타입
        public ItemType ItemType { get; private set; } = ItemType.NONE;

        // 아이템 능력치
        public int ItemStatus { get; private set; } = 0;

        // 아이템 유지 횟수
        public int ItemUsedCount { get; private set; } = 0;

        // 아이템 가격
        public int ItemPrice { get; private set; } = 0;

        public bool IsPersistent() => ItemType.ToString().StartsWith("T_");

        // 재정의
        public Item(ItemType _type, int _status, int _usedCount, int _price)
        {
            ItemType = _type;                   // 아이템 타입 설정
            ItemStatus = _status;               // 아이템 능력치 설정
            ItemUsedCount = _usedCount;         // 아이템 유지 횟수 설정
            ItemPrice = _price;                 // 아이템 가격 설정
        }

        // 아이템 사용
        public void Use()
        {
            if (IsPersistent() == false)
            {
                Console.WriteLine($"[단일 아이템 사용] {ItemType}: +{ItemStatus} 효과 즉시 적용");
                ApplyInstantEffect();
            }
        }

        private void ApplyInstantEffect()
        {
            switch (ItemType)
            {
                case ItemType.F_POTION_HP:
                    {
                        // 회복
                        break;
                    }
                // TODO
            }
        }
    }
}