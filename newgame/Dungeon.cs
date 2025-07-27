using static newgame.UiHelper;

namespace newgame
{
    internal class Dungeon
    {

        enum RoomType
        {
            Wall,
            Empty,
            Ladder,
            Monster,
            Treasure,
            Shop,
            Event,
            Boss,
            Exit
        }

        public void Start()
        {
            Console.Clear();
            LoadMapData();
            SetDungeon();
        }

        #region 던전

        // 맵 데이터 (2차원 배열)
        List<List<int>> map = new List<List<int>>();

        void LoadMapData()
        {
            map = GameManager.Instance.GetDungeonMap(1);
        }

        // 플레이어 위치
        static int playerX = 1, playerY = 1;

        void SetDungeon()
        {
            int height = map.Count;
            int width = map[0].Count;

            // 게임 시작
            while (true)
            {
                Console.Clear();

                DrawMap(width, height);
                DrawPlayer();

                RoomEvent((RoomType)map[playerY][playerX]);

                // 키 입력 받기
                ConsoleKeyInfo key = Console.ReadKey(true);

                RoomDelete();
                // 이동 처리
                int newX = playerX, newY = playerY;

                if (key.Key == ConsoleKey.UpArrow) newY--;      // 위로
                else if (key.Key == ConsoleKey.DownArrow) newY++; // 아래로
                else if (key.Key == ConsoleKey.LeftArrow) newX--; // 왼쪽으로
                else if (key.Key == ConsoleKey.RightArrow) newX++; // 오른쪽으로

                // 이동 가능한지 확인 (맵 안에 있고 벽이 아닌 경우)
                if (newX >= 0 && newX < width && newY >= 0 && newY < height && map[newY][newX] != 0)
                {
                    playerX = newX;
                    playerY = newY;
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
            RoomType room = (RoomType)map[playerY][playerX]; // 수정
            switch (room)
            {
                case (RoomType.Monster):
                    {
                        MonsterCreate();
                        break;
                    }
                case (RoomType.Treasure):
                    {
                        // 보물 획득 로직 추가
                        break;
                    }
                case (RoomType.Shop):
                    {
                        // 상점 로직 추가
                        break;
                    }
                case (RoomType.Event):
                    {
                        // 이벤트 로직 추가
                        break;
                    }
                case (RoomType.Boss):
                    {
                        BossCreate(); // 보스 몬스터 생성
                        break;
                    }
                case(RoomType.Exit):
                    {
                        // 게임 종료 또는 다음 단계로 이동
                        break;
                    }
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
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Console.Write(GetRoomSymbol((RoomType)map[y][x]));
                }
                Console.WriteLine();
            }
            // 현재 방 정보 출력

            Console.WriteLine();
            Console.WriteLine("현재 방: " + GetRoomName((RoomType)map[playerY][playerX]));

            Console.WriteLine();
            Console.WriteLine($"\t↑{GetRoomName((RoomType)map[playerY + 1][playerX])}");
            Console.WriteLine($"←{GetRoomName((RoomType)map[playerY][playerX - 1])}" +
                              $"\t\t→{GetRoomName((RoomType)map[playerY][playerX + 1])}");
            Console.WriteLine($"\t↓{GetRoomName((RoomType)map[playerY - 1][playerX])}");

        }

        void DrawPlayer()
        {
            int left = playerX;
            int top = playerY;
            Console.SetCursorPosition(left, top);
            Console.Write('@');
        }

        void RoomDelete()
        {
            if (playerY >= 0 && playerY < map.Count && playerX >= 0 && playerX < map[playerY].Count && (RoomType)map[playerY][playerX] != RoomType.Empty)
            {
                map[playerY][playerX] = (int)RoomType.Empty;
            }
        }
        #endregion

        #endregion

        #region 몬스터 소환/배틀
        void MonsterCreate()
        {
            Monster monster = new Monster();
            GameManager.Instance.monster = monster;
            monster.Start(1);
            Battle battle = new Battle();
            battle.Start();
        }

        void BossCreate()
        {

        }
        #endregion
    }
}