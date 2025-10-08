using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Reflection.Metadata;

namespace newgame
{
    public class DataManager
    {
        static DataManager? instance;
        public static DataManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new DataManager();
                }

                return instance;
            }
        }

        #region 저장 & 불러오기
        public void Save(Status playerData)
        {
            string path, data;
            #region Player
            path = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Player.json");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            data = JsonConvert.SerializeObject(playerData);
            File.WriteAllText(path, data);
            #endregion

            #region Inventory
            path = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Inventory.json");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            data = JsonConvert.SerializeObject(Inventory.Instance, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            });

            File.WriteAllText(path, data);
            #endregion
        }

        public Status Load()
        {
            #region Player
            Status playerData = new Status();

            string path = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Player.json");

            if (File.Exists(path))
            {
                string data = File.ReadAllText(path);
                Status? deserialized = JsonConvert.DeserializeObject<Status>(data);
                if (deserialized != null)
                {
                    playerData = deserialized;
                }
            }
            #endregion

            #region Inventory
            Inventory.Instance.Load();
            #endregion

            return playerData;
        }

        public bool IsPlayerData()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Player.json");

            return File.Exists(path);
        }

        public void DeleteData()
        {
            List<string> datas = new List<string>();
            string playerData = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Player.json");
            datas.Add(playerData);
            string inventoryData = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Inventory.json");
            datas.Add(inventoryData);

            foreach(string path in datas)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
        #endregion

        #region 착용 장비
        /// <summary>
        /// 착용 장비 데이터 불러오기
        /// </summary>
        public void LoadAllEquipData()
        {
            // 데이터 폴더 경로
            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            for (int i = 1; i < (int)EquipType.MAX; i++)
            {
                // 텍스트 파일 이름
                string fileName = $"Equip_{(EquipType)i}.txt";
                // 텍스트 파일 경로
                string filePath = Path.Combine(dataPath, fileName);

                // 텍스트 파일 존재 유무 판단
                if(!File.Exists(filePath))
                {
                    // 파일이 없는 경우
                    Console.WriteLine($"해당 경로 [{filePath}]가 존재하지 않습니다. ");
                    Console.WriteLine($"[{fileName}] 파일을 확인해주세요.");
                    return;
                }

                SetEquipData(filePath, (EquipType)i);
            }
        }

        void SetEquipData(string filePath, EquipType _type)
        {
            try
            {
                // 텍스트 파일에서 모든 라인 읽어오기
                string[] lines = File.ReadAllLines(filePath);
                string name = string.Empty;         // 아이템 이름
                int[] data = new int[3];            // id, stat, price
                foreach(string line in lines)
                {
                    if(line == "#")
                    {
                        // 현재까지 얻어진 정보로 Equipment 클래스 생성하고
                        Equipment equip = new Equipment(_type, data[0], name, data[1], data[2]);
                        // GameManager 에서 equips 리스트에 등록하기
                        GameManager.Instance.SetEquipList(equip);

                        continue;
                    }

                    // 문자열 자르기 ( 해당 형식은 ":" 를 기준으로 문자열을 구분하고 있음 )
                    string[] curLine = line.Split(':');
                    if (curLine[0].Trim() == "ID")                 // ID 일 때
                    {
                        data[0] = int.Parse(curLine[1].Trim());
                    }
                    else if(curLine[0].Trim() == "NAME")           // NAME 일 때
                    {
                        name = curLine[1].Trim();
                    }
                    else if(curLine[0].Trim() == "STAT")           // STAT 일 때
                    {
                        data[1] = int.Parse(curLine[1].Trim());
                    }
                    else if(curLine[0].Trim() == "PRICE")          // PRICE 일 때
                    {
                        data[2] = int.Parse(curLine[1].Trim());
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[오류] : 파일 읽기 실패 ({ex.Message})");
            }
        }
        #endregion

        #region 몬스터
        public void LoadEnemyData()
        {
            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            // 텍스트 파일 이름
            string fileName = $"Monster.txt";
            // 텍스트 파일 경로
            string filePath = Path.Combine(dataPath, fileName);

            // 파일 체크
            if(File.Exists(filePath) == false)
            {
                // 파일이 없는 경우
                Console.WriteLine($"해당 경로 [{filePath}]가 존재하지 않습니다. ");
                Console.WriteLine($"[{fileName}] 파일을 확인해주세요.");
                return;
            }

            SetEnemyData(filePath);
        }

        void SetEnemyData(string filePath)
        {
            try
            {
                // 텍스트 파일에서 모든 라인 읽어오기
                string[] lines = File.ReadAllLines(filePath);

                Status monStat = new Status();

                foreach (string line in lines)
                {
                    if (line == "#")
                    {
                        monStat.charType = CharType.MONSTER;
                        GameManager.Instance.SetMonsterInfo(monStat);
                        monStat = new Status();
                        continue;
                    }

                    string[] curLine = line.Split(':');
                    if (curLine.Length < 2)
                    {
                        continue;
                    }

                    string key = curLine[0].Trim().ToUpperInvariant();
                    string value = curLine[1].Trim();

                    switch (key)
                    {
                        case "NAME":
                            monStat.Name = value;
                            break;
                        case "LEVEL":
                            monStat.level = int.Parse(value);
                            break;
                        case "HP":
                            monStat.Hp = int.Parse(value);
                            monStat.MaxHp = monStat.Hp;
                            break;
                        case "ATK":
                            monStat.ATK = int.Parse(value);
                            break;
                        case "DEF":
                            monStat.DEF = int.Parse(value);
                            break;
                        case "EXP":
                            monStat.exp = int.Parse(value);
                            break;
                        case "GOLD":
                        case "COIN":
                            monStat.gold = int.Parse(value);
                            break;
                        case "CRICHANCE":
                            monStat.CriticalChance = int.Parse(value);
                            break;
                        case "CRIDMG":
                            monStat.CriticalDamage = int.Parse(value);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[오류] : 파일 읽기 실패 ({ex.Message})");
            }
        }
        #endregion 

        #region 보스
        public void LoadBossData()
        {
            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            // 텍스트 파일 이름
            string fileName = $"Boss.txt";
            // 텍스트 파일 경로
            string filePath = Path.Combine(dataPath, fileName);

            // 파일 체크
            if (File.Exists(filePath) == false)
            {
                // 파일이 없는 경우
                Console.WriteLine($"해당 경로 [{filePath}]가 존재하지 않습니다. ");
                Console.WriteLine($"[{fileName}] 파일을 확인해주세요.");
                return;
            }

            SetBossData(filePath);
        }

        void SetBossData(string filePath)
        {
            try
            {
                // 텍스트 파일에서 모든 라인 읽어오기
                string[] lines = File.ReadAllLines(filePath);

                Status BossStat = new Status();
                List<string> skillNames = new List<string>();

                foreach (string line in lines)
                {
                    if (line == "#")
                    {
                        BossStat.charType = CharType.MONSTER;
                        int bossKey = GameManager.Instance.SetBossInfo(BossStat);
                        GameManager.Instance.SetBossSkills(bossKey, skillNames);
                        BossStat = new Status();
                        skillNames = new List<string>();
                        continue;
                    }

                    string[] curLine = line.Split(':');
                    if (curLine.Length < 2)
                    {
                        continue;
                    }

                    string key = curLine[0].Trim().ToUpperInvariant();
                    string value = curLine[1].Trim();

                    switch (key)
                    {
                        case "NAME":
                            BossStat.Name = value;
                            break;
                        case "LEVEL":
                            BossStat.level = int.Parse(value);
                            break;
                        case "HP":
                            BossStat.Hp = int.Parse(value);
                            BossStat.MaxHp = BossStat.Hp;
                            break;
                        case "ATK":
                            BossStat.ATK = int.Parse(value);
                            break;
                        case "DEF":
                            BossStat.DEF = int.Parse(value);
                            break;
                        case "EXP":
                            BossStat.exp = int.Parse(value);
                            break;
                        case "GOLD":
                        case "COIN":
                            BossStat.gold = int.Parse(value);
                            break;
                        case "CRICHANCE":
                            BossStat.CriticalChance = int.Parse(value);
                            break;
                        case "CRIDMG":
                            BossStat.CriticalDamage = int.Parse(value);
                            break;
                        case "SKILL":
                            skillNames.Add(value);
                            break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(BossStat.Name) || skillNames.Count > 0)
                {
                    BossStat.charType = CharType.MONSTER;
                    int bossKey = GameManager.Instance.SetBossInfo(BossStat);
                    GameManager.Instance.SetBossSkills(bossKey, skillNames);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[오류] : 파일 읽기 실패 ({ex.Message})");
            }
        }
        #endregion 

        #region 던전 맵
        public void LoadDungeonMap()
        {
            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            string fileName = $"Dungeon_Map.txt";
            string filePath = Path.Combine(dataPath, fileName);

            if (!File.Exists(filePath))
            {
                // 파일이 없는 경우
                Console.WriteLine($"해당 경로 [{filePath}]가 존재하지 않습니다. ");
                Console.WriteLine($"[{fileName}] 파일을 확인해주세요.");
                return;
            }

            SetDungeonMapData(filePath);
        }

        void SetDungeonMapData(string filePath)
        {
            var mapRows = new List<List<int>>();
            foreach (string raw in File.ReadLines(filePath))
            {
                string line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line == "#")
                {
                    if (mapRows.Count > 0)
                        GameManager.Instance.SetDungeonMapInfo(mapRows);
                    mapRows = new List<List<int>>();
                    continue;
                }

                // 콤마 단위로 잘라 int 리스트로 변환
                List<int> row = line.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => int.Parse(s.Trim()))
                                    .ToList();
                mapRows.Add(row);
            }
            if (mapRows.Count > 0)
                GameManager.Instance.SetDungeonMapInfo(mapRows);
        }



        #endregion

        #region 스킬

        public void LoadSkillData()
        {
            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            // 텍스트 파일 이름
            string fileName = $"Skill.txt";
            // 텍스트 파일 경로
            string filePath = Path.Combine(dataPath, fileName);

            // 파일 체크
            if (File.Exists(filePath) == false)
            {
                // 파일이 없는 경우
                Console.WriteLine($"해당 경로 [{filePath}]가 존재하지 않습니다. ");
                Console.WriteLine($"[{fileName}] 파일을 확인해주세요.");
                return;
            }

            SetSkillData(filePath);
        }

        void SetSkillData(string filePath)
        {
            try
            {
                // 텍스트 파일에서 모든 라인 읽어오기
                string[] lines = File.ReadAllLines(filePath);

                SkillType skills = new SkillType();

                foreach (string line in lines)
                {
                    if (line == "#")
                    {
                        GameManager.Instance.SetSkill(skills);
                        skills = new SkillType();
                        continue;
                    }
                    string[] curLine = line.Split(':');

                    if (curLine[0].Trim() == "NAME")
                    {
                        skills.name = curLine[1].Trim();
                    }
                    else if(curLine[0].Trim() == "ID")
                    {
                        skills.skillId = int.Parse(curLine[1].Trim());
                    }
                    else if(curLine[0].Trim() == "DAMAGE")
                    {
                        skills.skillDamage = int.Parse(curLine[1].Trim());
                    }
                    else if(curLine[0].Trim() == "MANA")
                    {
                        skills.skillMana = int.Parse(curLine[1].Trim());
                    }
                    else if (curLine[0].Trim() == "DURATION")
                    {
                        skills.skillTurn = int.Parse(curLine[1].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[오류] : 파일 읽기 실패 ({ex.Message})");
            }
        }
        #endregion

        #region 직업

        public void LoadPlayer_ClassData()
        {
            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            // 텍스트 파일 이름
            string fileName = $"Player_Class.txt";
            // 텍스트 파일 경로
            string filePath = Path.Combine(dataPath, fileName);

            // 파일 체크
            if (File.Exists(filePath) == false)
            {
                // 파일이 없는 경우
                Console.WriteLine($"해당 경로 [{filePath}]가 존재하지 않습니다. ");
                Console.WriteLine($"[{fileName}] 파일을 확인해주세요.");
                return;
            }

            SetPlayer_ClassData(filePath);
        }

        void SetPlayer_ClassData(string filePath)
        {
            try
            {
                // 텍스트 파일에서 모든 라인 읽어오기
                string[] lines = File.ReadAllLines(filePath);

                CharacterClassType classType = new CharacterClassType();
                List<string> skillNames = new List<string>();

                foreach (string line in lines)
                {
                    if (line == "#")
                    {
                        // 현재까지 수집된 직업 데이터와 스킬을 GameManager에 등록
                        GameManager.Instance.SetPlayerClassInfo(classType, skillNames);
                        // 새로운 클래스 데이터를 위한 초기화
                        classType = new CharacterClassType();
                        skillNames = new List<string>();
                        continue;
                    }

                    string[] curLine = line.Split(':');
                    if (curLine.Length < 2)
                    {
                        continue;
                    }

                    string key = curLine[0].Trim().ToUpperInvariant();
                    string value = curLine[1].Trim();

                    switch (key)
                    {
                        case "NAME":
                            classType.name = value;
                            break;
                        case "DESCRIPTION":
                            classType.description = value;
                            break;
                        case "ATK":
                            classType.atk = int.Parse(value);
                            break;
                        case "DEF":
                            classType.def = int.Parse(value);
                            break;
                        case "HP":
                            classType.hp = int.Parse(value);
                            break;
                        case "MP":
                            classType.mp = int.Parse(value);
                            break;
                        case "CC":
                            classType.CC = int.Parse(value);
                            break;
                        case "CD":
                            classType.CD = int.Parse(value);
                            break;
                        case "SKILL":
                            skillNames.Add(value);
                            break;
                    }
                }

                // 파일 끝에 도달했지만 데이터가 남아있는 경우 처리
                if (!string.IsNullOrWhiteSpace(classType.name) || skillNames.Count > 0)
                {
                    GameManager.Instance.SetPlayerClassInfo(classType, skillNames);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[오류] : 파일 읽기 실패 ({ex.Message})");
            }
        }

        #endregion
    }
}