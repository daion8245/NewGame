namespace newgame
{
    internal static class GameManager
    {
        public static Player player;
        public static Monster monster;
        #region 아이템
        static List<Item> items = new List<Item>();
        public static Item FindItem(ItemType _type)
        {
            foreach (Item item in items)
            {
                if (item.GetItemType == _type)
                {
                    return item;
                }
            }

            return null;
        }

        public static void SetItemList()
        {
            items = new List<Item>();
            #region 포션
            items.Add(new Item(ItemType.F_POTION_HP, 50, 1, 5));
            items.Add(new Item(ItemType.T_POTION_EXPUP, 2, 3, 40));
            items.Add(new Item(ItemType.T_POTION_ATKUP, 10, 3, 20));
            items.Add(new Item(ItemType.F_ETC_RESETNAME, 0, 1, 1000));
            #endregion
        }
        #endregion

        #region
        static List<Equipment> equips;
        public static List<Equipment> GetEquipment { get => equips; }
        public static Equipment FindEquipment(EquipType _type, int _id)
        {
            foreach (Equipment equip in equips)
            {
                if (equip.GetEquipType == _type && equip.GetEquipID == _id)
                {
                    return equip;
                }
            }
            return null;
        }
        #endregion


        public static void SetEquipList()
        {
            equips = new List<Equipment>();

            #region 모자
            equips.Add(new Equipment(EquipType.HELMET, 1, "초보자 모자", 0, 0));
            equips.Add(new Equipment(EquipType.HELMET, 2, "초보자 모자(강화)", 1, 1000));
            equips.Add(new Equipment(EquipType.HELMET, 3, "초보자 모자(강화2)", 2, 2000));
            #endregion

            #region 상의
            equips.Add(new Equipment(EquipType.SHIRT, 1, "초보자 상의", 0, 0));
            #endregion

            #region 하의
            equips.Add(new Equipment(EquipType.PANTS, 1, "초보자 하의", 0, 0));
            #endregion

            #region 장갑
            equips.Add(new Equipment(EquipType.GLOVE, 1, "초보자 장갑", 0, 0));
            #endregion

            #region 신발
            equips.Add(new Equipment(EquipType.SHOES, 1, "초보자 신발", 0, 0));
            #endregion
        }
    }
}
