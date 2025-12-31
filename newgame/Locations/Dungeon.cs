using System.Drawing;
using System.Linq;
using System.Text;
using newgame.Characters;
using newgame.Enemies;
using newgame.Locations.DungeonRooms;
using newgame.Systems;
using newgame.UI;

namespace newgame.Locations
{
    internal class Dungeon
    {
        private static readonly Dungeon _instance = new Dungeon();
        public static Dungeon Instance { get { return _instance; } }

        private Dungeon() { }

        public int Floor { get; private set; } = 1;
        public void NextFloor() { Floor++; }

        enum RoomType
        {
            Wall,      //0 벽
            Empty,     //1 없음
            Ladder,    //2 사다리(다음 층 가는거)
            Monster,   //3 몬스터
            Treasure,  //4 보물방(미구현)
            Shop,      //5 상점
            Event,     //6 이벤트방
            Boss,      //7 보스
            Exit       //8 나가는곳 (마을로 가기)
        }

        public static int floor = 1; // 현재 층수

        public void Start()
        {
            EnsureClearedFloorsInitialized();
            Console.Clear();
            LoadMapData(floor);
            SetDungeon();
        }

        #region 던전

        // 맵 데이터 (2차원 배열)
        List<List<int>> map = new List<List<int>>();
        void LoadMapData(int number)
        {
            map = GameManager.Instance.GetDungeonMap(number);
            NormalizeToRectangle(); // 맵을 직사각형으로 정규화
        }

        // 플레이어 위치
        public static Point PlayerPos = new Point(1,1);

        private void SetDungeon()
        {
            EnsureClearedFloorsInitialized();
            if(GameManager.Instance.clearedFloors[floor])
                ClearedFloorSetup();
            
            // 게임 시작
            while (true)
            {
                int height = map.Count;
                int width = (height > 0 && map[0].Count > 0) ? map[0].Count : 0;

                if (width == 0)
                {
                    Console.Clear();
                    UiHelper.WaitForInput("던전 맵 정보를 불러오지 못했습니다. [ENTER를 눌러 계속]");
                    GameManager.Instance.ReturnToLobby();
                    return;
                }

                Console.Clear();

                DrawMap(width, height);
                DrawPlayer();

                // 키 입력 받기
                ConsoleKeyInfo key = Console.ReadKey(true);

                Player? activePlayerForCleanup = GameManager.Instance.Player;
                if (activePlayerForCleanup != null && !activePlayerForCleanup.IsDead)
                {
                    RoomDelete();
                }

                GameManager.Instance.UpdateDungeonMap(floor, map);
                // 이동 처리
                int newX = PlayerPos.X, newY = PlayerPos.Y;

                if (key.Key == ConsoleKey.UpArrow) newY--;         // 위로
                else if (key.Key == ConsoleKey.DownArrow) newY++;  // 아래로
                else if (key.Key == ConsoleKey.LeftArrow) newX--;  // 왼쪽으로
                else if (key.Key == ConsoleKey.RightArrow) newX++; // 오른쪽으로

                // 이동 가능한지 확인 (맵 안에 있고 벽이 아닌 경우)
                if (newX >= 0 && newX < width && newY >= 0 && newY < height && map[newY][newX] != 0)
                {
                    PlayerPos.X = newX;
                    PlayerPos.Y = newY;
                }

                RoomType currentRoom = (RoomType)map[PlayerPos.Y][PlayerPos.X];
                RoomEvent(currentRoom);

                if (HandlePlayerDefeatIfNeeded(currentRoom))
                {
                    return;
                }
            }

        }

        private void EnsureClearedFloorsInitialized()
        {
            int floorCount = GameManager.Instance.dungeonMapInfo.Count;

            if (floorCount == 0)
            {
                if (!GameManager.Instance.clearedFloors.ContainsKey(floor))
                {
                    GameManager.Instance.clearedFloors[floor] = false;
                }
                return;
            }

            for (int i = 1; i <= floorCount; i++)
            {
                if (!GameManager.Instance.clearedFloors.ContainsKey(i))
                {
                    GameManager.Instance.clearedFloors[i] = false;
                }
            }

            if (!GameManager.Instance.clearedFloors.ContainsKey(floor))
            {
                GameManager.Instance.clearedFloors[floor] = false;
            }
        }

        private void ClearedFloorSetup()
        {
            RestoreWallsAndMonstersFromOriginal();
        }

        private void RestoreWallsAndMonstersFromOriginal()
        {
            List<List<int>>? original = GameManager.Instance.GetOriginalDungeonMap(floor);
            if (original == null || original.Count == 0)
            {
                return;
            }

            int height = Math.Min(map.Count, original.Count);
            for (int y = 0; y < height; y++)
            {
                int width = Math.Min(map[y].Count, original[y].Count);
                for (int x = 0; x < width; x++)
                {
                    RoomType originalRoom = (RoomType)original[y][x];
                    if (originalRoom == RoomType.Wall || originalRoom == RoomType.Monster)
                    {
                        map[y][x] = (int)originalRoom;
                    }
                }
            }
        }

        #region 방 이름 가져오기
        static string GetRoomName(RoomType room)
        {
            switch (room)
            {
                case RoomType.Wall: return "벽";
                case RoomType.Empty: return "빈 방";
                case RoomType.Ladder: return "사다리";
                case RoomType.Monster: return "몬스터";
                case RoomType.Treasure: return "보물";
                case RoomType.Shop: return "상점";
                case RoomType.Event: return "이벤트";
                case RoomType.Boss: return "보스";
                case RoomType.Exit: return "출구";
                default: return "알 수 없음";
            }
        }
        #endregion

        #region 방 이벤트 처리
        void RoomEvent(RoomType playerRoom)
        {
            RoomType room = (RoomType)map[PlayerPos.Y][PlayerPos.X];
            Console.SetCursorPosition(0, map.Count + 1);
            switch (room)
            {
                #region 몬스터
                case RoomType.Monster:
                    {
                        Console.Clear();
                        UiHelper.TxtOut(new string[]
                        {
                            "몬스터 방에 진입했습니다.",""
                        });

                        int select = UiHelper.SelectMenu(new string[]
                        {
                            "몬스터와 전투",
                            "도망 시도(35%)"
                        });

                        Player activePlayer = GameManager.Instance.RequirePlayer();

                        if(select == 0)
                        {
                            UiHelper.WaitForInput("몬스터와의 전투를 시작합니다. [ENTER를 눌러 계속]");
                            MonsterCreate();
                        }
                        else
                        {
                            int randomChance = new Random().Next(1, 101);
                            if (randomChance <= 35) // 35% 확률로 도망 성공
                            {
                                UiHelper.WaitForInput("몬스터 방에서 도망치는데 성공했습니다!  [ENTER를 눌러 계속]");
                            }
                            else
                            {
                                UiHelper.WaitForInput("도망에 실패했습니다! 체력의 30%를 잃고 몬스터와 전투를 시작합니다!  [ENTER를 눌러 계속]");
                                activePlayer.MyStatus.Hp -= (int)(activePlayer.MyStatus.MaxHp * 0.3);
                                MonsterCreate();
                            }
                        }
                        if (GameManager.Instance.monster?.IsDead ?? false)
                        {
                            RoomDelete(true); // 승리 시 방 삭제
                        }
                        break;
                    }
                #endregion
                case (RoomType.Treasure):
                    {
                        TreasureRoomCreate();
                        break;
                    }
                case (RoomType.Shop):
                    {
                        EnterDungeonShop();
                        break;
                    }
                case (RoomType.Event):
                    {
                        EventRoomCreate();
                        RoomDelete(true);
                        break;
                    }
                case (RoomType.Ladder):
                    {
                        Console.Clear();
                        GameManager.Instance.clearedFloors[floor] = true;
                        UiHelper.WaitForInput($"사다리를 타고 다음 층({floor + 1}층) 으로 이동합니다. [ENTER를 눌러 계속]");
                        int currentFloor = floor;
                        GameManager.Instance.UpdateDungeonMap(currentFloor, map);
                        floor++; // 층수 증가
                        EnsureClearedFloorsInitialized();
                        LoadMapData(floor); // 다음 층 맵 데이터 로드
                        PlayerPos.X = 1; // 플레이어 위치 초기화
                        PlayerPos.Y = 1; // 플레이어 위치 초기화
                        break;
                    }
                case (RoomType.Boss):
                    {
                        bool bossDefeated = BossCreate(); // 보스 몬스터 생성 및 전투 시작

                        if (bossDefeated)
                        {
                            RoomDelete(true); // 승리 시 방 삭제
                            GameManager.Instance.UpdateDungeonMap(floor, map);
                        }
                        break;
                    }
                #region 마을로 돌아가기
                case (RoomType.Exit):
                    {
                        MovingFloors();
                        break;
                    }
                #endregion
                default:
                    {
                        break;
                    }
            }
        }

        #endregion

        #region 방 그리기
        char GetRoomSymbol(RoomType room)
        {
            return room switch
            {
                RoomType.Wall => '■',
                RoomType.Empty => ' ',
                RoomType.Ladder => '▲',
                RoomType.Monster => 'M',
                RoomType.Treasure => 'T',
                RoomType.Shop => 'S',
                RoomType.Event => 'E',
                RoomType.Boss => 'B',
                RoomType.Exit => 'X',
                _ => ' '
            };
        }

        void DrawMap(int width, int height)
        {
            for (int y = 0; y < map.Count; y++)
            {
                for (int x = 0; x < map[y].Count; x++)
                {
                    Console.Write(GetRoomSymbol((RoomType)map[y][x]));
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("현제 층: " + floor);
            Console.WriteLine("현재 방: " + GetRoomName((RoomType)map[PlayerPos.Y][PlayerPos.X]));
            Console.WriteLine();
            Console.WriteLine("[■ = 벽] [ = 빈 방] [▲ 사다리] [M = 몬스터] [T = 보물] [S = 상점] [E = 이벤트] [B = 보스] [X = 출구]");
            Console.WriteLine();
            // 인접 방 경계 체크
            Console.WriteLine($"\t↑{GetRoomName(GetRoomTypeSafe(PlayerPos.Y - 1, PlayerPos.X))}");
            Console.WriteLine($"←{GetRoomName(GetRoomTypeSafe(PlayerPos.Y, PlayerPos.X - 1))}" +
                              $"\t\t→{GetRoomName(GetRoomTypeSafe(PlayerPos.Y, PlayerPos.X + 1))}");
            Console.WriteLine($"\t↓{GetRoomName(GetRoomTypeSafe(PlayerPos.Y + 1, PlayerPos.X))}");
        }

        RoomType GetRoomTypeSafe(int y, int x)
        {
            if (y >= 0 && y < map.Count)
            {
                if (x >= 0 && x < map[y].Count)
                    return (RoomType)map[y][x];
                return RoomType.Empty;
            }

            return RoomType.Wall;
        }

        void NormalizeToRectangle()
        {
            int maxWidth = map.Max(r => r.Count);
            for (int i = 0; i < map.Count; i++)
            {
                while (map[i].Count < maxWidth)
                    map[i].Add((int)RoomType.Empty); // 비어 있는 칸으로 채움
            }
        }
        
        void DrawPlayer()
        {
            int left = PlayerPos.X;
            int top = PlayerPos.Y;
            Console.SetCursorPosition(left, top);
            Console.Write('@');
        }

        bool HandlePlayerDefeatIfNeeded(RoomType roomBeforeDefeat)
        {
            Player? activePlayer = GameManager.Instance.Player;
            if (activePlayer == null || !activePlayer.IsDead)
            {
                return false;
            }

            if (roomBeforeDefeat == RoomType.Monster || roomBeforeDefeat == RoomType.Boss)
            {
                map[PlayerPos.Y][PlayerPos.X] = (int)roomBeforeDefeat;
            }

            GameManager.Instance.UpdateDungeonMap(floor, map);
            UiHelper.WaitForInput("던전에서 패배하여 마을로 돌아갑니다. [ENTER를 눌러 계속]");

            activePlayer.RespawnAtTavern();
            activePlayer.IsDead = false;
            GameManager.Instance.monster = null;

            PlayerPos.X = 1; // 플레이어 위치 초기화
            PlayerPos.Y = 1; // 플레이어 위치 초기화

            return true;
        }

        void RoomDelete(bool force = false)
        {
            if (PlayerPos.Y >= 0 && PlayerPos.Y < map.Count && PlayerPos.X >= 0 && PlayerPos.X < map[PlayerPos.Y].Count)
            {
                RoomType current = (RoomType)map[PlayerPos.Y][PlayerPos.X];
                if (force || (current != RoomType.Empty && current != RoomType.Exit && current != RoomType.Monster && current != RoomType.Boss))
                {
                    map[PlayerPos.Y][PlayerPos.X] = (int)RoomType.Empty;
                }
            }
        }
        #endregion

        #endregion

        #region 몬스터 소환/배틀

        #region 층별 몬스터 지정

        Func<string,int> findMonsterIdByName = (name) => GameManager.Instance.FindAMonsterName(name);
        
        private readonly Dictionary<int, List<int>> _floorMonsters = new Dictionary<int, List<int>>()
        {
            { 0, new List<int> { GameManager.Instance.FindAMonsterName("슬라임"), GameManager.Instance.FindAMonsterName("빅 슬라임") } },    // 1층 몬스터 ID
            { 1, new List<int> { GameManager.Instance.FindAMonsterName("스켈레톤"), GameManager.Instance.FindAMonsterName("스켈레톤 영혼"), GameManager.Instance.FindAMonsterName("스켈레톤 기사") } }, // 2층 몬스터 ID
            { 2, new List<int> { GameManager.Instance.FindAMonsterName("고블린"), GameManager.Instance.FindAMonsterName("갱 고블린") } },    // 3층 몬스터 ID
            { 3, new List<int> { GameManager.Instance.FindAMonsterName("좀비"), GameManager.Instance.FindAMonsterName("애기 좀비") } },    // 4층 몬스터 ID
            { 4, new List<int> { GameManager.Instance.FindAMonsterName("거미"), GameManager.Instance.FindAMonsterName("독 거미") } },  // 5층 몬스터 ID
            { 5, new List<int> { GameManager.Instance.FindAMonsterName("작은 하피"), GameManager.Instance.FindAMonsterName("하피") } },  // 6층 몬스터 ID
            { 6, new List<int> { GameManager.Instance.FindAMonsterName("작은 트롤"), GameManager.Instance.FindAMonsterName("트롤") } },  // 7층 몬스터 ID
            // 필요시 추가 층수 및 몬스터 ID
        };

        #endregion
        void MonsterCreate()
        {
            int floorMonsterId;
            //층별 몬스터 설정
            Random rand = new Random();
            _floorMonsters.TryGetValue((floor - 1), out var monsterIds);
            if (monsterIds != null)
            {
                floorMonsterId = monsterIds[rand.Next(monsterIds.Count)];
            }
            else
            {
                floorMonsterId = -1;
            }
            
            Monster monster = new Monster();
            GameManager.Instance.monster = monster;
            monster.Start(floorMonsterId);

            Battle battle = new Battle();
            battle.Start();
        }
        #endregion

        #region 보스 소환/배틀

        bool BossCreate()
        {
            Boss boss = new Boss();
            GameManager.Instance.monster = boss;
            boss.StartBoss(floor);
            Battle battle = new Battle();
            bool playerWon = battle.Start();
            bool bossDefeated = playerWon || (GameManager.Instance.monster?.IsDead ?? false);

            return bossDefeated;
        }
        #endregion

        #region 던전 이벤트 방
        void EventRoomCreate()
        {
            EventRoomsCreater eventRoomsCreater = new EventRoomsCreater();
            eventRoomsCreater.CreateDungeonEventRoom();

        }

        #endregion

        #region 던전 상점 방

        private void EnterDungeonShop()
        {
            Shop shop = GameManager.Instance.GetDungeonShop(floor);
            shop.SetExitAction(() => { });
            shop.Start();
            shop.SetExitAction(null);
        }
        #endregion

        #region 보물 방

        void TreasureRoomCreate()
        {
            // 보물 방 로직 추가
            TreasureRooms treasureRooms = new TreasureRooms();
            treasureRooms.Start();
        }

        #endregion

        #region 귀환 사다리

        void MovingFloors()
        {
            Console.Clear();
            UiHelper.TxtOut(["사다리를 타고 다른 층으로 이동합니다.",""]);

            List<int> clearedFloorList = GameManager.Instance.clearedFloors
                .Where(pair => pair.Value)
                .Select(pair => pair.Key)
                .OrderBy(key => key)
                .ToList();

            List<string> options = new List<string> { "마을로 돌아가기" };
            options.AddRange(clearedFloorList.Select(floorNumber => $"{floorNumber}층"));
            options.Add("취소");

            int selection = UiHelper.SelectMenu(options.ToArray());

            if (selection == 0)
            {
                Console.Clear();
                UiHelper.WaitForInput("마을로 돌아갑니다. [ENTER를 눌러 계속]");
                GameManager.Instance.UpdateDungeonMap(floor, map);
                PlayerPos = new Point(1, 1);
                GameManager.Instance.ReturnToLobby();
                return;
            }

            if (selection > 0 && selection <= clearedFloorList.Count)
            {
                int targetFloor = clearedFloorList[selection - 1];
                Console.Clear();
                UiHelper.WaitForInput($"{targetFloor}층으로 이동합니다. [ENTER를 눌러 계속]");
                GameManager.Instance.UpdateDungeonMap(floor, map);
                floor = targetFloor;
                LoadMapData(floor); // 선택한 층 맵 데이터 로드
                EnsureClearedFloorsInitialized();
                if (GameManager.Instance.clearedFloors.TryGetValue(floor, out bool cleared) && cleared)
                {
                    ClearedFloorSetup();
                }
                PlayerPos = new Point(1, 1);
                return;
            }

            UiHelper.WaitForInput("던전 탐험을 계속합니다. [ENTER를 눌러 계속]");
        }

        #endregion
    }
}
