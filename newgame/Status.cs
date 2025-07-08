using Newtonsoft.Json;
using System.Net.NetworkInformation;
using static newgame.UiHelper;

namespace newgame
{
    public enum CharType
    {
        NONE,
        PLAYER,
        MONSTER,
        MAX
    }

    public class Status
    {
        #region Status
        public CharType charType;
        public string Name;
        public int level;
        [JsonProperty]
        int atk;
        [JsonIgnore]
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
        [JsonProperty]
        int def;
        [JsonIgnore]
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
        public int mp;
        public int maxMp;
        public int gold;
        public int exp;
        public int nextEXP;
        #endregion

        public Status Clone()
        {
            return (Status)this.MemberwiseClone();
        }
        int GetStrAtk()
        {
            Equipment equip = null;

            equip = Inventory.Instance.GetEquip(EquipType.WEAPON);
            int weapon = equip == null ? 0 : equip.GetEquipStat;

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
            TextDisplayConfig.SlowTxtOut = true;
            TextDisplayConfig.SlowTxtOutTime = 1;
            TextDisplayConfig.SlowTxtLineTime = 0;

            UiHelper.TxtOut([
                $"이름 : {Name}",
                $"  레벨 : {level}",
                $"  체력 : {hp}/{maxHp}",
                $"  공격력 : {atk}",
                $"  방어력 : {def}",
                $"  마나 : {mp}/{maxMp}",
                $"  골드 : {gold}",
                $"  경험치 : {hp} / {nextEXP}"
                ]);
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
