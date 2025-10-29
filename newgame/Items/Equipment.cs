using System.Collections.Generic;
using Newtonsoft.Json;

namespace newgame.Items
{
    public enum EquipType
    {
        NONE,
        WEAPON,
        HELMET,
        SHIRT,
        PANTS,
        GLOVE,
        SHOES,
        MAX
    }

    internal struct EquipStat(int hp = 0, int atk = 0, int def = 0, int mp = 0, int criChance = 0, int crlDam = 0)
    {
        public int Hp = hp;
        public int Atk = atk;
        public int Def = def;
        public int Mp = mp;
        public int CriChance = criChance;
        public int CrlDam = crlDam;

        public IEnumerable<string> EnumerateSummaryParts()
        {
            if (Hp != 0)
            {
                yield return $"HP+{Hp}";
            }

            if (Mp != 0)
            {
                yield return $"MP+{Mp}";
            }

            if (Atk != 0)
            {
                yield return $"ATK+{Atk}";
            }

            if (Def != 0)
            {
                yield return $"DEF+{Def}";
            }

            if (CriChance != 0)
            {
                yield return $"CC+{CriChance}";
            }

            if (CrlDam != 0)
            {
                yield return $"CD+{CrlDam}";
            }
        }

        public string ToSummary()
        {
            List<string> parts = new List<string>();

            foreach (string part in EnumerateSummaryParts())
            {
                parts.Add(part);
            }

            return parts.Count == 0 ? "능력치 없음" : string.Join(", ", parts);
        }
    }
    
    internal class Equipment
    {
        [JsonProperty] private EquipType equiptype = EquipType.NONE;
        public EquipType GetEquipType
        {
            get => equiptype;
            private set => equiptype = value;
        }
        [JsonProperty] private EquipStat equipStat = new EquipStat();
        public EquipStat GetEquipStat
        {
            get => equipStat;
            private set => equipStat = value;
        }
        [JsonProperty] private string equipName = string.Empty;
        public string GetEquipName
        {
            get => equipName;
            private set => equipName = value;
        }
        [JsonProperty] private int equipId = 0;
        public int GetEquipID
        {
            get => equipId;
            private set => equipId = value;
        }
        [JsonProperty] private int price = 0;
        public int GetPrice
        {
            get => price;
            private set => price = value;
        }
        [JsonProperty] private int _upgradeCount = 0;
        public int GetUpgradeCount
        {
            get => _upgradeCount;
            private set => _upgradeCount = value;
        }

        public Equipment(EquipType _equipType, int _equipID, string _equipName, EquipStat _equipStat, int _GetPrice)
        {
            equiptype = _equipType;
            equipName = _equipName;
            equipId = _equipID;
            equipStat = _equipStat;
            price = _GetPrice;
        }

        public void Upgrade()
        {
            int range = new Random().Next(0, 101);
            if (range <= 50)
            {
                Console.WriteLine("장비 강화 실패");
                return;
            }
            _upgradeCount++;

            ScaleBy1Point2(ref equipStat.Hp);
            ScaleBy1Point2(ref equipStat.Atk);
            ScaleBy1Point2(ref equipStat.Def);
            ScaleBy1Point2(ref equipStat.Mp);
            ScaleBy1Point2(ref equipStat.CrlDam);
            ScaleBy1Point2(ref equipStat.CriChance);
            
            Console.WriteLine("강화 성공");
        }
        
        private void ScaleBy1Point2(ref int value)
        {
            int original = value;

            float multiplyTheMultiplier = 1.5f;
            float upgrade = _upgradeCount;

            if (_upgradeCount >= 4)
            {
                multiplyTheMultiplier -= (upgrade / 10);
            }
            else
            {
                multiplyTheMultiplier = 1.1f;
            }

            // 1) 1.2를 곱한 뒤 정수로 변환(소수부 버림). 범위를 벗어나면 예외 발생(checked).
            value = checked((int)(value * multiplyTheMultiplier));

            // 2) 곱했는데 결과가 기존 값과 같다면 +1 (가능한 경우에만)
            if (value == original && value < int.MaxValue)
            {
                if (value == 0)
                {
                    return;
                }
                value += 1;
            }
        }
    }
}
