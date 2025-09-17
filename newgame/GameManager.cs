using System.Text.Json.Serialization.Metadata;

namespace newgame
{
    internal class GameManager
    {
        static GameManager? instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameManager();
                }
                return instance;
            }
        }

        public Player? player;
        public Monster? monster;

        #region 몬스터 정보
        Dictionary<int, Status> monsterInfo = new Dictionary<int, Status>();
        readonly Dictionary<int, Status> bossInfo = new Dictionary<int, Status>();
        readonly Dictionary<int, List<string>> bossSkills = new Dictionary<int, List<string>>();
        int nextBossFloor = 1;

        public void SetMonsterInfo(Status _stat)
        {
            int key = monsterInfo.Count + 1;
            monsterInfo.Add(key, _stat);
        }

        public int SetBossInfo(Status _stat)
        {
            int floor = nextBossFloor;
            bossInfo[floor] = _stat.Clone();
            nextBossFloor++;
            return floor;
        }

        public void SetBossSkills(int bossKey, IEnumerable<string> skillNames)
        {
            List<string> names = new List<string>();
            foreach (string name in skillNames)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name.Trim());
                }
            }

            bossSkills[bossKey] = names;
        }

        public List<SkillType> GetBossSkills(int bossKey)
        {
            List<SkillType> results = new List<SkillType>();

            int effectiveKey = bossKey;
            if (effectiveKey < 1)
            {
                effectiveKey = 1;
            }

            if (!bossSkills.ContainsKey(effectiveKey))
            {
                int lastFloor = nextBossFloor - 1;
                if (lastFloor >= 1 && bossSkills.ContainsKey(lastFloor))
                {
                    effectiveKey = lastFloor;
                }
            }

            if (bossSkills.TryGetValue(effectiveKey, out List<string>? names) && names != null)
            {
                foreach (string name in names)
                {
                    var skill = FindSkillByName(name);
                    if (skill != null)
                    {
                        results.Add(skill.Value);
                    }
                }
            }

            if (results.Count == 0)
            {
                results.AddRange(GetSkills());
            }

            return results;
        }

        public Status GetBossStat(int floor)
        {
            if (floor < 1)
            {
                floor = 1;
            }

            if (bossInfo.TryGetValue(floor, out Status? stat))
            {
                return stat.Clone();
            }

            int lastFloor = nextBossFloor - 1;
            if (lastFloor >= 1 && bossInfo.TryGetValue(lastFloor, out stat))
            {
                return stat.Clone();
            }

            return GetMonsterStat(1);
        }

        public Status GetMonsterStat(int _key)
        {
            bool isKey = monsterInfo.ContainsKey(_key);
            if(isKey == false)
            {
                return new Status();
            }

            return monsterInfo[_key].Clone();
        }

        #endregion

        #region 아이템
        List<Item> items = new List<Item>();
        public Item FindItem(ItemType _type)
        {
            foreach (Item item in items)
            {
                if (item.ItemType == _type)
                {
                    return item;
                }
            }

            return null;
        }

        public void SetItemList()
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

        #region 장비 상태 확인
        List<Equipment> equips = new List<Equipment>();
        public List<Equipment> GetEquipment { get => Instance.equips; }
        public Equipment FindEquipment(EquipType _type, int _id)
        {
            foreach (Equipment equip in Instance.equips)
            {
                if (equip.GetEquipType == _type && equip.GetEquipID == _id)
                {
                    return equip;
                }
            }
            return null;
        }
        #endregion

        public bool SetEquipList(Equipment equip)
        {
            if(equips == null)
            {
                equips = new List<Equipment>();
            }

            foreach(Equipment item in equips)
            {
                if(equip.GetEquipType == item.GetEquipType && equip.GetEquipID == item.GetEquipID)
                {
                    return false;
                }
            }

            equips.Add(equip);
            return true;
        }

        #region 던전
        public Dictionary<int, List<List<int>>> dungeonMapInfo = new Dictionary<int, List<List<int>>>();
        public void SetDungeonMapInfo(List<List<int>> _Map)
        {
            int key = dungeonMapInfo.Count + 1;
            dungeonMapInfo.Add(key, _Map);
        }

        public List<List<int>> GetDungeonMap(int _key)
        {
            bool isKey = dungeonMapInfo.ContainsKey(_key);
            if (isKey == false)
            {
                return new List<List<int>>();
            }

            var original = dungeonMapInfo[_key];
            var copy = new List<List<int>>();
            foreach (var innerList in original)
            {
                copy.Add(new List<int>(innerList));
            }
            return copy;
        }
        #endregion

        #region 스킬

        private List<SkillType> Skills = new List<SkillType>();
        
        public void SetSkill(SkillType skill)
        {
            if (!Skills.Contains(skill))
            {
                Skills.Add(skill);
            }
        }

        public void RemoveSkill(SkillType skill)
        {
            if (Skills.Contains(skill))
            {
                Skills.Remove(skill);
            }
        }

        public SkillType? FindSkillByName(string name)
        {
            foreach (var skill in Skills)
            {
                if (skill.name == name)
                {
                    return skill;
                }
            }
            return null;
        }

        public bool HasSkill(SkillType skill)
        {
            return Skills.Contains(skill);
        }

        public List<SkillType> GetSkills()
        {
            return new List<SkillType>(Skills);
        }
        #endregion
    }
}