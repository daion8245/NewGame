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

        public Dungeon()
        {
            Start();
        }
        public void Start()
        {
            Console.Clear();
            SetDungeon();
        }

        #region 던전

        // 맵 데이터 (2차원 배열)
        static int[,] map = {
            {0,0,0,0,0,0,0,0},
            {0,1,1,2,3,5,6,4},
            {0,4,4,5,4,5,2,0},
            {0,1,1,2,3,4,2,3},
            {0,0,0,0,0,0,0,0}
        };

        // 플레이어 위치
        static int playerX = 1, playerY = 1;

        void SetDungeon()
        {
            // 게임 시작
            while (true)
            {
                // 화면 지우기
                Console.Clear();

                // 맵 출력
                for (int y = 0; y < 5; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        // 플레이어가 있는 위치면 @ 출력
                        if (x == playerX && y == playerY)
                            Console.Write("@");
                        else
                            Console.Write(map[y, x]);
                    }
                    Console.WriteLine();
                }

                // 현재 방 정보 출력
                Console.WriteLine();
                Console.WriteLine("현재 방: " + GetRoomName((RoomType)map[playerY, playerX]));

                // 키 입력 받기
                ConsoleKeyInfo key = Console.ReadKey(true);

                // 이동 처리
                int newX = playerX, newY = playerY;

                if (key.Key == ConsoleKey.UpArrow) newY--;      // 위로
                else if (key.Key == ConsoleKey.DownArrow) newY++; // 아래로
                else if (key.Key == ConsoleKey.LeftArrow) newX--; // 왼쪽으로
                else if (key.Key == ConsoleKey.RightArrow) newX++; // 오른쪽으로

                // 이동 가능한지 확인 (맵 안에 있고 벽이 아닌 경우)
                if (newX >= 0 && newX < 8 && newY >= 0 && newY < 5 && map[newY, newX] != 0)
                {
                    playerX = newX;
                    playerY = newY;
                }
            }

        }
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

        #region 몬스터 소환/배틀
        void CreateMonster()
        {
            Monster monster = new Monster();
            GameManager.Instance.monster = monster;
            monster.Start();
            Battle();
        }

        void Battle()
        {
            Battle battle = new Battle();
            battle.Start();
        }
        #endregion
    }
}
