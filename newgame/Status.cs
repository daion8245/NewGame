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
        public string Name = "";
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
                    Equipment? equip = null;

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
                    Equipment? equip = null;

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

        private int _hp;

        public int Hp
        {
            get => _hp;
            set => _hp = (value < 0) ? 0 : value; // set은 return 금지, value를 필드에 대입
        }

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
            Equipment? equip = null;

            equip = Inventory.Instance.GetEquip(EquipType.WEAPON);
            int weapon = equip == null ? 0 : equip.GetEquipStat;

            equip = Inventory.Instance.GetEquip(EquipType.HELMET);
            int helmet = equip == null ? 0 : equip.GetEquipStat;

            equip = Inventory.Instance.GetEquip(EquipType.GLOVE);
            int glove = equip == null ? 0 : equip.GetEquipStat;

            equip = Inventory.Instance.GetEquip(EquipType.SHOES);
            int shoes = equip == null ? 0 : equip.GetEquipStat;

            return weapon + helmet + glove + shoes;
        }

        public void ShowStatus()
        {
            UiHelper.TxtOut([
                $"이름 : {Name}",
                $"  레벨 : {level}",
                $"  체력 : {_hp}/{maxHp}",
                $"  공격력 : {atk}",
                $"  방어력 : {def}",
                $"  마나 : {mp}/{maxMp}",
                $"  골드 : {gold}",
                $"  경험치 : {exp} / {nextEXP}"
                ],true,1,0);
        }

        public void ShowInventory()
        {
            Inventory.Instance.ShowEquipList();

            int input = UiHelper.SelectMenu(new string[]
            {
                "장비 착용",
                "장비 버리기",
                "이전으로"
            });
            
            switch(input)
            {
                case 0:
                    {
                        int canEquip = Inventory.Instance.ShowCanEquips();
                        if (canEquip == -1)
                        {
                            Console.WriteLine("착용 가능한 장비가 없습니다.");
                            UiHelper.WaitForInput("[ENTER]를 눌러 계속");
                            return;
                        }

                        SetEquip(canEquip);
                        return;
                    }

                case 1:
                    {
                        int canEquip = Inventory.Instance.ShowCanEquips();
                        if (canEquip == -1)
                        {
                            Console.WriteLine("버릴 장비가 없습니다.");
                            UiHelper.WaitForInput("[ENTER]를 눌러 계속");
                            return;
                        }

                        Inventory.Instance.RemoveCanEquip(canEquip + 1);
                        return;
                    }

                case 2:
                    {
                        return;
                    }
            }
        }

        public void LevelUp()
        {
            if(exp >= nextEXP)
            {
                level++;
                maxHp += 10;
                _hp = maxHp;
                mp += 5;
                nextEXP += 10;
                ATK += 3;
                DEF += 2;

                Console.WriteLine($"{Name} 레벨업! 현재 레벨 : {level - 1} -> {level}");
                Console.WriteLine($"체력 : {maxHp - 10} -> {maxHp}");
                Console.WriteLine($"마나 : {mp - 5} -> {mp}");
                Console.WriteLine($"공격력 : {ATK - 3} -> {ATK}");
                Console.WriteLine($"방어력 : {DEF - 2} -> {DEF}");
            }

        }

        void SetEquip(int sel)
        {
            //Console.WriteLine("착용할려는 장비 번호 : ");
            //int idx = 0;
            //int.TryParse(Console.ReadLine(), out idx);
            Inventory.Instance.SetEquip(sel + 1);
        }
    }
}
