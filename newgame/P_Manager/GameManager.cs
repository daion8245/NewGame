using newgame.Entity;
using newgame.Entity.Equip;
using newgame.Entity.Item;
using newgame.Entity.Player;
using newgame.Entity.Skill;
using newgame.Room;
using System.Text.Json.Serialization.Metadata;

namespace newgame.Manager
{
    /// <summary>
    /// 게임 전역 상태를 관리하는 싱글톤 매니저
    /// - 플레이어/몬스터, 로비, 전투 로그, 몬스터/보스/던전/아이템/장비/스킬 데이터를 단일 진입점에서 관리한다.
    /// </summary>
    internal class GameManager
    {
        // 싱글톤 인스턴스 보관 필드
        static GameManager? instance;

        /// <summary>
        /// <para>GameManager 싱글톤 인스턴스</para>
        /// 존재하지 않으면 즉시 생성(lazy init)한다.
        /// </summary>
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

        // 전투 메시지를 한 곳에서 관리하는 서비스 (UI/로그 출력 일관성 유지)
        private readonly BattleLogService battleLogService;
        // 지연 생성되는 로비 인스턴스 (필요할 때만 생성)
        private Lobby? _lobby;

        /// <summary>
        /// 외부 생성 방지. 내부에서만 생성하도록 막는다.
        /// </summary>
        private GameManager()
        {
            battleLogService = new BattleLogService();
        }

        /// <summary>
        /// 전투 로그 서비스에 대한 읽기 전용 참조
        /// </summary>
        public BattleLogService BattleLogService => battleLogService;

        /// <summary>
        /// 현재 플레이어 인스턴스(없을 수 있음)
        /// </summary>
        public Player? player;

        /// <summary>
        /// 플레이어 접근용 프로퍼티. null 허용.
        /// </summary>
        public Player? Player
        {
            get => player;
            set => player = value;
        }

        /// <summary>
        /// 플레이어가 초기화되었는지 여부
        /// </summary>
        public bool HasPlayer => player != null;

        /// <summary>
        /// null 검사 후 플레이어를 반환. 없으면 예외 발생.
        /// </summary>
        /// <exception cref="InvalidOperationException">플레이어가 아직 초기화되지 않은 경우</exception>
        public Player RequirePlayer()
        {
            return player ?? throw new InvalidOperationException("Player has not been initialized yet.");
        }

        /// <summary>
        /// 현재 몬스터 인스턴스(없을 수 있음)
        /// </summary>
        public Monster? monster;

        /// <summary>
        /// 몬스터가 초기화되었는지 여부
        /// </summary>
        public bool HasMonster => monster != null;

        /// <summary>
        /// null 검사 후 몬스터를 반환. 없으면 예외 발생.
        /// </summary>
        /// <exception cref="InvalidOperationException">몬스터가 아직 초기화되지 않은 경우</exception>
        public Monster RequireMonster()
        {
            return monster ?? throw new InvalidOperationException("Monster has not been initialized yet.");
        }

        /// <summary>
        /// 로비를 반환하거나, 없으면 새로 생성한다.
        /// </summary>
        /// <returns>로비 인스턴스</returns>
        public Lobby GetOrCreateLobby()
        {
            _lobby ??= new Lobby();
            return _lobby;
        }

        /// <summary>
        /// 로비 화면으로 복귀한다. (로비가 없으면 생성 후 시작)
        /// </summary>
        public void ReturnToLobby()
        {
            GetOrCreateLobby().Start();
        }

        #region 몬스터 정보
        // 일반 몬스터 스탯: key = 1,2,3... (등록 순서 기반)
        Dictionary<int, Status> monsterInfo = new Dictionary<int, Status>();
        // 보스 스탯: key = 층수(floor)
        readonly Dictionary<int, Status> bossInfo = new Dictionary<int, Status>();
        // 보스 스킬: key = 층수(floor), value = 스킬 이름 목록
        readonly Dictionary<int, List<string>> bossSkills = new Dictionary<int, List<string>>();
        // 다음으로 등록될 보스 층수(1부터 시작)
        int nextBossFloor = 1;

        /// <summary>
        /// 일반 몬스터 스탯을 등록한다. 내부적으로 1부터 증가하는 키를 사용한다.
        /// </summary>
        /// <param name="_stat">등록할 스탯</param>
        public void SetMonsterInfo(Status _stat)
        {
            int key = monsterInfo.Count + 1;
            monsterInfo.Add(key, _stat);
        }

        /// <summary>
        /// 보스 스탯을 현재 지정된 다음 층수에 등록하고, 층수를 증가시킨다.
        /// </summary>
        /// <param name="_stat">등록할 보스 스탯</param>
        /// <returns>등록된 층수</returns>
        public int SetBossInfo(Status _stat)
        {
            int floor = nextBossFloor;
            // 외부 변형을 막기 위해 Clone() 보관
            bossInfo[floor] = _stat.Clone();
            nextBossFloor++;
            return floor;
        }

        /// <summary>
        /// 특정 보스 층수에 스킬 이름 목록을 등록한다.
        /// </summary>
        /// <param name="bossKey">보스 층수</param>
        /// <param name="skillNames">스킬 이름들</param>
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

        /// <summary>
        /// 보스 층수에 해당하는 스킬 목록을 조회한다.
        /// 등록된 스킬이 없으면 마지막 보스 층수의 스킬 또는 플레이어 전체 스킬을 반환한다.
        /// </summary>
        /// <param name="bossKey">조회할 보스 층수</param>
        /// <returns>스킬 타입 목록</returns>
        public List<SkillType> GetBossSkills(int bossKey)
        {
            List<SkillType> results = new List<SkillType>();

            int effectiveKey = bossKey;
            if (effectiveKey < 1)
            {
                effectiveKey = 1;
            }

            // 해당 층 데이터가 없으면, 마지막으로 등록된 층을 사용
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

            // 아무 것도 못 찾으면 기본으로 플레이어 전체 스킬을 사용
            if (results.Count == 0)
            {
                results.AddRange(GetSkills());
            }

            return results;
        }

        /// <summary>
        /// 특정 층수의 보스 스탯을 반환한다.
        /// 존재하지 않으면 마지막으로 등록된 보스 또는 기본 몬스터(1번)의 스탯을 반환한다.
        /// </summary>
        /// <param name="floor">층수(1 이상)</param>
        /// <returns>보스 스탯(복제본)</returns>
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

        /// <summary>
        /// 일반 몬스터 스탯을 키로 조회한다. 없으면 기본 <see cref="Status"/>를 반환한다.
        /// </summary>
        /// <param name="_key">등록 키(1부터 시작)</param>
        /// <returns>몬스터 스탯(복제본) 또는 기본 스탯</returns>
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
        // 상점/드랍 등에서 참조하는 아이템 풀
        List<Item> items = new List<Item>();

        /// <summary>
        /// 아이템 타입으로 아이템을 검색한다.
        /// </summary>
        /// <param name="_type">찾을 아이템 타입</param>
        /// <returns>아이템 또는 null</returns>
        public Item? FindItem(ItemType _type)
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

        /// <summary>
        /// 아이템 목록을 초기화하고 기본 아이템들을 등록한다.
        /// </summary>
        public void SetItemList()
        {
            items = new List<Item>();
            #region 포션
            items.Add(new Item(ItemType.F_POTION_LOW_HP, 8, 1,   15));
            items.Add(new Item(ItemType.F_POTION_MIDDLE_HP, 30, 1, 50));
            items.Add(new Item(ItemType.F_POTION_HIGH_HP, 100, 1, 200));
            items.Add(new Item(ItemType.T_POTION_EXPUP, 2,  3,  40));
            items.Add(new Item(ItemType.T_POTION_ATKUP, 10, 3,  20));
            items.Add(new Item(ItemType.F_ETC_RESETNAME,0,  1, 1000));
            #endregion

            #region 제작재료
            items.Add(new Item(ItemType.M_WOOD, 0, 1, 1));
            #endregion
        }
        #endregion

        #region 장비 상태 확인
        // 보유 중인 장비 목록
        List<Equipment> equips = new List<Equipment>();

        /// <summary>
        /// 현재 보유 중인 장비 목록(읽기 전용 컬렉션 스냅샷)
        /// </summary>
        public List<Equipment> GetEquipment { get => Instance.equips; }

        /// <summary>
        /// 타입과 ID로 장비를 검색한다.
        /// </summary>
        /// <param name="_type">장비 타입</param>
        /// <param name="_id">장비 식별자</param>
        /// <returns>장비 또는 null</returns>
        public Equipment? FindEquipment(EquipType _type, int _id)
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

        /// <summary>
        /// 장비를 보유 목록에 추가한다. 동일 타입/ID의 장비가 이미 있으면 추가하지 않는다.
        /// </summary>
        /// <param name="equip">추가할 장비</param>
        /// <returns>true = 추가됨, false = 중복으로 미추가</returns>
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
        /// <summary>
        /// 던전 맵 데이터: key = 층수, value = 타일 맵(행렬)
        /// </summary>
        public Dictionary<int, List<List<int>>> dungeonMapInfo = new Dictionary<int, List<List<int>>>();

        /// <summary>
        /// 새로운 던전 맵을 등록한다. 키는 내부적으로 1부터 증가한다.
        /// </summary>
        /// <param name="_Map">타일 맵(행렬)</param>
        public void SetDungeonMapInfo(List<List<int>> _Map)
        {
            int key = dungeonMapInfo.Count + 1;
            dungeonMapInfo.Add(key, _Map);
        }

        /// <summary>
        /// 던전 맵을 조회한다. 내부 보존을 위해 깊은 복사본을 반환한다.
        /// </summary>
        /// <param name="_key">층수 키</param>
        /// <returns>타일 맵 복사본. 없으면 비어 있는 맵</returns>
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

        /// <summary>
        /// 던전 맵을 갱신한다. 입력 데이터를 복사하여 내부 상태와의 공유를 방지한다.
        /// </summary>
        /// <param name="floor">층수</param>
        /// <param name="updatedMap">새 맵 데이터</param>
        public void UpdateDungeonMap(int floor, List<List<int>> updatedMap)
        {
            if (updatedMap == null)
            {
                return;
            }

            var copy = new List<List<int>>();
            foreach (var row in updatedMap)
            {
                if (row == null)
                {
                    copy.Add(new List<int>());
                    continue;
                }

                copy.Add(new List<int>(row));
            }

            dungeonMapInfo[floor] = copy;
        }
        #endregion

        #region 스킬

        // 플레이어가 보유 중인 전체 스킬(이름 기반 탐색에도 사용)
        private List<SkillType> Skills = new List<SkillType>();
        
        /// <summary>
        /// 스킬을 목록에 추가한다. 중복 추가는 무시.
        /// </summary>
        /// <param name="skill">스킬</param>
        public void SetSkill(SkillType skill)
        {
            if (!Skills.Contains(skill))
            {
                Skills.Add(skill);
            }
        }

        /// <summary>
        /// 스킬을 목록에서 제거한다. 존재하지 않으면 무시.
        /// </summary>
        /// <param name="skill">스킬</param>
        public void RemoveSkill(SkillType skill)
        {
            if (Skills.Contains(skill))
            {
                Skills.Remove(skill);
            }
        }

        /// <summary>
        /// 스킬 이름으로 스킬을 찾는다.
        /// </summary>
        /// <param name="name">스킬 이름</param>
        /// <returns>찾은 스킬 또는 null</returns>
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

        /// <summary>
        /// 해당 스킬을 보유하고 있는지 확인한다.
        /// </summary>
        /// <param name="skill">스킬</param>
        /// <returns>true = 보유</returns>
        public bool HasSkill(SkillType skill)
        {
            return Skills.Contains(skill);
        }

        /// <summary>
        /// 보유 스킬 목록의 복사본을 반환한다.
        /// </summary>
        /// <returns>스킬 목록 복사본</returns>
        public List<SkillType> GetSkills()
        {
            return new List<SkillType>(Skills);
        }
        #endregion

        #region 직업
        // 직업 데이터 풀 (파일 로드 시 채워짐)
        private readonly List<CharacterClassType> Jobs = new List<CharacterClassType>();
        // 직업명 -> 스킬 이름 목록 매핑
        private readonly Dictionary<string, List<string>> playerClassSkillNames = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 플레이어 직업 정보와 해당 직업의 스킬 이름들을 등록/갱신한다.
        /// 동일 이름의 직업이 존재하면 덮어쓴다.
        /// </summary>
        /// <param name="classType">직업 정보</param>
        /// <param name="skillNames">스킬 이름 목록</param>
        public void SetPlayerClassInfo(CharacterClassType classType, IEnumerable<string> skillNames)
        {
            // 직업 정보 등록 또는 갱신
            int idx = Jobs.FindIndex(c => string.Equals(c.name, classType.name, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                Jobs[idx] = classType;
            }
            else
            {
                Jobs.Add(classType);
            }

            // 스킬 이름 정리 후 매핑 저장
            List<string> names = new List<string>();
            foreach (var n in skillNames)
            {
                if (!string.IsNullOrWhiteSpace(n))
                {
                    names.Add(n.Trim());
                }
            }
            playerClassSkillNames[classType.name] = names;
        }

        /// <summary>
        /// 등록된 모든 플레이어 직업 목록을 반환한다.
        /// </summary>
        public IReadOnlyList<CharacterClassType> GetPlayerClasses() => Jobs.AsReadOnly();

        public bool TryGetPlayerClass(string className, out CharacterClassType classType)
        {
            foreach (CharacterClassType job in Jobs)
            {
                if (string.Equals(job.name, className, StringComparison.OrdinalIgnoreCase))
                {
                    classType = job;
                    return true;
                }
            }

            classType = default;
            return false;
        }

        /// <summary>
        /// 직업명으로 스킬들을 조회한다. 이름이 매칭되는 스킬만 반환한다.
        /// </summary>
        /// <param name="className">직업 이름</param>
        /// <returns>해당 직업이 보유한 스킬 타입 목록</returns>
        public List<SkillType> GetClassSkills(string className)
        {
            var result = new List<SkillType>();
            if (playerClassSkillNames.TryGetValue(className, out var names))
            {
                foreach (var n in names)
                {
                    var s = FindSkillByName(n);
                    if (s != null)
                    {
                        result.Add(s.Value);
                    }
                }
            }
            return result;
        }

        #endregion
    }
}