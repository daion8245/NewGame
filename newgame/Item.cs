using static newgame.MyDiffain;

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
                OnecUse();
            }
            else if (once[0] == 'T')
            {
                if (itemType == ItemType.T_POTION_ATKUP)
                {
                    ActiveUse();
                }
            }
        }

        void OnecUse()
        {
            switch (itemType)
            {
                case ItemType.F_POTION_HP:
                    if (GameManager.player.MyStatus.hp > GameManager.player.MyStatus.maxHp - ItemStatus)
                    { //플레이어 체력이 최대치이면 체력이 오버되지 않게 하는 if문
                        GameManager.player.MyStatus.hp = GameManager.player.MyStatus.maxHp;
                        Console.WriteLine("포션을 사용했다. HP가 최대치로 회복되었다.");
                        break;
                    }
                    else
                    {
                        GameManager.player.MyStatus.hp += ItemStatus;
                        Console.WriteLine($"포션을 사용했다. HP가 {ItemStatus} 회복되었다.");
                        break;
                    }
                case ItemType.F_ETC_RESETNAME:
                    {
                        SetName();
                        break;
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

        void SetName()
        {
            while (true)
            {
                Console.Clear();
                SlowTxtout("플레이어의 이름을 입력하세요.", 30);
                Console.Write("> ");
                string str = Console.ReadLine();

                GameManager.player.MyStatus.Name = str;
                SlowTxtout($"입력된 이름: {GameManager.player.MyStatus.Name} 이 정말 맞습니까?", 30);
                Console.WriteLine("Y / F");
                Console.Write("> ");
                string str2 = Console.ReadLine();
                if (str2.ToUpper() == "Y")
                {
                    break;
                }
            }

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
