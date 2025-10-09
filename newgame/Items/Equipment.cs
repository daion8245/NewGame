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
        [JsonProperty] private int updateCount = 0;
        public int GetUpgradeCount
        {
            get => updateCount;
            private set => updateCount = value;
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
            updateCount++;
            Console.WriteLine("강화 성공");
        }
    }
}
