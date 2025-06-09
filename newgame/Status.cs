using static newgame.MyDiffain;

namespace newgame
{
    public enum CharType
    {
        NONE,
        PLAYER,
        MONSTER,
        MAX
    }

    internal class Status
    {
        #region Status
        public CharType charType;
        public string Name;
        public int level;
        int atk;
        public int ATK
        {
            get
            {
                if (charType == CharType.PLAYER)
                {
                    Equipment equip = null;

                    equip = Inventory.Instance.GetEquip(EquipType.HELMET);
                    int helmet = equip == null ? 0 : equip.GetEquipStat;

                    equip = Inventory.Instance.GetEquip(EquipType.GLOVE);
                    int glove = equip == null ? 0 : equip.GetEquipStat;

                    equip = Inventory.Instance.GetEquip(EquipType.SHOES);
                    int shoes = equip == null ? 0 : equip.GetEquipStat;

                    return atk + helmet + glove + shoes;
                }
                return atk;
            }

            set => atk = value;
        }
        int def;
        public int DEF
        {
            get
            {
                if (charType == CharType.PLAYER)
                {
                    Equipment equip = null;

                    equip = Inventory.Instance.GetEquip(EquipType.SHIRT);
                    int shirt = equip == null ? 0 : equip.GetEquipStat;

                    equip = Inventory.Instance.GetEquip(EquipType.PANTS);
                    int pants = equip == null ? 0 : equip.GetEquipStat;

                    return def + shirt + pants;
                }
                return def;
            }
            set => def = value;
        }
        public int hp;
        public int maxHp;
        public int coin;
        public int exp;
        public int nextEXP;
        #endregion

        int GetStrAtk()
        {
            Equipment equip = null;

            equip = Inventory.Instance.GetEquip(EquipType.HELMET);
            int helmet = equip == null ? 0 : equip.GetEquipStat;

            equip = Inventory.Instance.GetEquip(EquipType.GLOVE);
            int glove = equip == null ? 0 : equip.GetEquipStat;

            equip = Inventory.Instance.GetEquip(EquipType.SHOES);
            int shoes = equip == null ? 0 : equip.GetEquipStat;

            return helmet + glove + shoes;
        }

        public void ShowStatus()
        {
            Console.Clear();

            Console.WriteLine("--------------------------");
            SlowTxtout($"--이름: {Name}\t\t--", 10);
            SlowTxtout($"--레벨: {level}\t\t--", 10);
            SlowTxtout($"--공격력: {ATK}({atk} + {GetStrAtk()})--", 10);
            SlowTxtout($"--체력: {hp}/{maxHp}\t\t--", 10);
            SlowTxtout($"--방어력: {DEF}\t\t--", 10);
            SlowTxtout($"--코인: {coin}\t\t--", 10);
            SlowTxtout($"--경험치: {exp}/{nextEXP}\t\t--", 10);
            Console.WriteLine("---------------------------");
        }

        public void ShowInventory()
        {
            Console.WriteLine("1. 인벤토리 2.이전으로");
            string input = Console.ReadLine();
            if (input != "1") return;

            Inventory.Instance.ShowEquipList();

            Console.WriteLine("1. 장비 착용 2.이전으로");
            input = Console.ReadLine();
            if (input != "1") return;

            bool canEquip = Inventory.Instance.ShowCanEquips();
            if (!canEquip)
            {
                Console.WriteLine("착용 가능한 장비가 없습니다.");
                return;
            }

            SetEquip();
        }


        void SetEquip()
        {
            Console.WriteLine("착용할려는 장비 번호 : ");
            int idx = 0;
            int.TryParse(Console.ReadLine(), out idx);
            Inventory.Instance.SetEquip(idx);
        }
    }
}
