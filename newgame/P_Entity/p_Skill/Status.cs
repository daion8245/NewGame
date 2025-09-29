using newgame.Entity.Equip;
using newgame.Entity.Player;
using newgame.Manager;
using Newtonsoft.Json;
using System;
using System.Net.NetworkInformation;
using static newgame.Manager.UiHelper;

namespace newgame.Entity.Skill
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
                    // 무기 포함 장비 공격력 합산
                    equipatk = GetStrAtk();
                }
                return (int)((atk + equipatk) * clatk);
            }
            set => atk = value; // 기본 능력치만 대입
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

                    equipdef = shirt + pants;
                }
                return (int)((def + equipdef) * cldef);
            }
            set => def = value;
        }

        private int _hp;

        public int Hp
        {
            get => _hp;
            set
            {
                _hp = (value < 0) ? 0 : value; // set은 return 금지, value를 필드에 대입
            }
        }

        private int maxHp;

        public int MaxHp
        {
            get => (int)((maxHp) * clhp);
            set => maxHp = value;
        }

        private int mp;
        public int Mp
        {
            get => mp;
            set => mp = (value < 0) ? 0 : value; // set은 return 금지, value를 필드에 대입
        }
        private int maxMp;
        public int MaxMp
        {
            get { return (int)((maxMp) * clmp); }
            set => maxMp = value;
        }
        private int criticalChance;
        public int CriticalChance
        {
            get { return (int)((criticalChance) * clcrit); }
            set => criticalChance = value;
        }
        private int criticalDamage;
        public int CriticalDamage
        {
            get { return (int)((criticalDamage) * clcd); }
            set => criticalDamage = value;
        }

        public int gold;
        public int exp;
        public int nextEXP;

        #endregion
        int equipatk = 0; // 장비 공격력 합산용
        int equipdef = 0;

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
            List<string> statusLines = new List<string>
            {
                $"이름 : {Name}",
                $"  레벨 : {level}",
                $"  체력 : {_hp}/{maxHp}",
                $"  공격력 : {atk}",
                $"  방어력 : {def}",
                $"  마나 : {mp}/{maxMp}",
                $"  치명타 확률 : {CriticalChance}",
                $"  치명타 피해 : {CriticalDamage}",
                $"  골드 : {gold}",
                $"  경험치 : {exp} / {nextEXP}"
            };
            if (IsPlayer)
            {
                statusLines.Insert(1, $"  직업 : {ClassName}");
            }
            UiHelper.TxtOut(statusLines.ToArray());
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
            while (exp >= nextEXP)
            {
                // 증가 전 값(효과값) 저장
                int prevLevel = level;
                int prevMaxHp = maxHp;
                int prevMaxMp = maxMp;
                int prevATK = ATK; // 장비 포함
                int prevDEF = DEF; // 장비 포함
                int prevCritChance = CriticalChance;
                int prevCritDamage = CriticalDamage;

                // 경험치/레벨 처리
                exp -= nextEXP;
                level++;

                // 체력/마나 최대치 상승 및 전부 회복
                maxHp += 10;
                _hp = maxHp;

                maxMp += 5;
                mp = maxMp;

                // 다음 레벨 필요 경험치 증가
                nextEXP += 10;

                // 기본 능력치만 상승(프로퍼티 대신 필드 사용)
                atk += 3;
                def += 2;
                CriticalChance += 2;
                CriticalDamage += 5;

                // 출력(이전 효과값 -> 현재 효과값)
                Console.WriteLine($"{Name} 레벨업! 현재 레벨 : {prevLevel} -> {level}");
                Console.WriteLine($"체력 : {prevMaxHp} -> {maxHp}");
                Console.WriteLine($"마나 : {prevMaxMp} -> {maxMp}");
                Console.WriteLine($"공격력 : {prevATK} -> {ATK}");
                Console.WriteLine($"방어력 : {prevDEF} -> {DEF}");
                Console.WriteLine($"치명타 확률 : {prevCritChance} -> {CriticalChance}");
                Console.WriteLine($"치명타 피해 : {prevCritDamage} -> {CriticalDamage}");
            }
        }

        void SetEquip(int sel)
        {
            //Console.WriteLine("착용할려는 장비 번호 : ");
            //int idx = 0;
            //int.TryParse(Console.ReadLine(), out idx);
            Inventory.Instance.SetEquip(sel + 1);
        }

        float clatk = 1f;
        float cldef = 1f;
        float clhp = 1f;
        float clmp = 1f;
        float clcrit = 1f;
        float clcd = 1f;
        public void ApplyClass(CharacterClassType classType)
        {
            if (string.IsNullOrWhiteSpace(classType.name))
            {
                throw new ArgumentException("Class name cannot be empty.", nameof(classType));
            }

            ClassName = classType.name;

            clatk += ((float)classType.atk / 100);
            cldef += ((float)classType.def / 100);

            clhp += ((float)classType.hp / 100);

            clmp += ((float)classType.mp / 100);

            clcrit += ((float)classType.CC / 100);
            clcd += ((float)classType.CD / 100);
        }

        #region 직업 추가를 위한 플래이어에게만 className 추가
        [JsonIgnore]
        public bool IsPlayer => charType == CharType.PLAYER;

        [JsonProperty]
        public string ClassName { get; set; } = string.Empty;

        public bool ShouldSerializeClassName() => IsPlayer;
        #endregion
    }
}
