using static newgame.UiHelper;

namespace newgame
{
    public enum ItemType
    {
        NONE,
        #region 포션
        F_POTION_HP,
        T_POTION_EXPUP,
        T_POTION_ATKUP,
        #endregion
        #region 기타
        F_ETC_RESETNAME,
        #endregion
        MAX,
    }
    internal class Item
    {
        ItemType itemType = ItemType.NONE;
        public ItemType GetItemType
        {
            get => itemType;
            private set => itemType = value;
        }

        int itemStatus = 0;
        public int ItemStatus
        {
            get => itemStatus;
            private set => itemStatus = value;
        }

        int itemCount = 0;
        public int ItemCount
        {
            get => itemCount;
            private set => itemCount = value;
        }

        public int ItemUsedCount
        {
            get => ItemUsedCount;
            private set => ItemUsedCount = value;
        }

        int curItemUseCount = 0;

        bool isActivate = false;
        public bool IsActvate
        {
            get => isActivate;
            private set => isActivate = value;
        }

        int itemPrice = 0;
        public int ItemPrice
        {
            get => itemPrice;
            private set => itemPrice = value;
        }
        public Item(ItemType _type, int _status, int _UsedCount, int _price)
        {
            itemType = _type;
            itemStatus = _status;
            ItemUsedCount = _UsedCount;
            curItemUseCount = ItemUsedCount;
            ItemPrice = _price;
        }

        public void Use()
        {
            if (itemCount == 0)
            {
                return;
            }
            itemCount--;
            Console.WriteLine();

            string once = itemType.ToString().Split('_')[0];
            if (once[0] == 'F')
            {
                UseOnce();
            }
            else if (once[0] == 'T')
            {
                if (itemType == ItemType.T_POTION_ATKUP)
                {
                    ActiveUse();
                }
            }
        }

        void UseOnce()
        {
            switch (itemType)
            {
                case ItemType.F_POTION_HP:
                    if (GameManager.Instance.player.MyStatus.hp > GameManager.Instance.player.MyStatus.maxHp - ItemStatus)
                    { //플레이어 체력이 최대치이면 체력이 오버되지 않게 하는 if문
                        GameManager.Instance.player.MyStatus.hp = GameManager.Instance.player.MyStatus.maxHp;
                        Console.WriteLine("포션을 사용했다. hp가 최대치로 회복되었다.");
                        break;
                    }
                    else
                    {
                        GameManager.Instance.player.MyStatus.hp += ItemStatus;
                        Console.WriteLine($"포션을 사용했다. hp가 {ItemStatus} 회복되었다.");
                        break;
                    }
                case ItemType.F_ETC_RESETNAME:
                    {
                        PlayerNameSet();
                        break;
                    }
            }
        }

        void PlayerNameSet()
        {
            while (true)
            {
                Console.Clear();

                Console.Write("플레이어의 이름을 입력해 주세요 : ");
                try
                {
                    string inputName = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(inputName) || inputName.Length < 2 || inputName.Length > 10)
                    {
                        throw new Exception("이름은 2자 이상 10자 이하로 입력해야 합니다.");
                    }

                    Console.Clear();
                    Console.WriteLine($"입력하신 이름 [{inputName}] 이 정말 맞습니까?");

                    int sel = UiHelper.SelectMenu(new[] { "Y", "N" });

                    if (sel == 0)
                    {
                        Console.Clear();

                        GameManager.Instance.player.SetName(inputName);
                        return;
                    }
                }
                catch (Exception e)
                {

                    UiHelper.TxtOut(new[]
                    {
                    $"잘못된 이름입니다.",
                    "다시 입력해 주세요.",
                     "",
                    "enter를 눌러 계속"
                    });
                    Console.ReadKey();
                }
            }
        }

            void ActiveUse()
        {
            if (isActivate)
            {
                curItemUseCount += ItemUsedCount;
                return;
            }
            isActivate = true;
            curItemUseCount = ItemUsedCount;
        }

        public bool CheckActiveUse()
        {
            bool result = false;

            switch (itemType)
            {
                case ItemType.T_POTION_EXPUP:
                    {
                        curItemUseCount--;
                        break;
                    }
                case ItemType.T_POTION_ATKUP:
                    {
                        curItemUseCount--;
                        break;
                    }
            }

            result = curItemUseCount <= 0;
            return result;
        }
    }
}
