using Newtonsoft.Json;

namespace newgame
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
    internal class Equipment
    {
        [JsonProperty]
        EquipType equiptype = EquipType.NONE;
        public EquipType GetEquipType
        {
            get => equiptype;
            private set => equiptype = value;
        }
        [JsonProperty]
        int equipStat = 0;
        public int GetEquipStat
        {
            get => equipStat;
            private set => equipStat = value;
        }
        [JsonProperty]
        string equipName = string.Empty;
        public string GetEquipName
        {
            get => equipName;
            private set => equipName = value;
        }
        [JsonProperty]
        int equipId = 0;
        public int GetEquipID
        {
            get => equipId;
            private set => equipId = value;
        }
        [JsonProperty]
        int price = 0;
        public int GetPrice
        {
            get => price;
            private set => price = value;
        }
        [JsonProperty]
        int updateCount = 0;
        public int GetUpdateCount
        {
            get => updateCount;
            private set => updateCount = value;
        }

        public Equipment(EquipType _equipType, int _equipID, string _equipName, int _equipStat, int _GetPrice)
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
            equipStat++;
            Console.WriteLine("강화 성공");
        }
    }
}
