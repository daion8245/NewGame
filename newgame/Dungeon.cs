using static newgame.UiHelper;
using System.Text;

namespace newgame
{
    internal class Dungeon
    {
        /* ──────────────── 1. 기본 설정 ──────────────── */
        enum RoomType { Wall, Empty, Ladder, Monster, Treasure, Shop, Event, Boss, Exit, Skip }

        const int Padding  = 1;                 // 방 둘레 공백
        const int CellSize = Padding * 2 + 1;   // 한 칸의 실제 출력 높이·너비

        /* ──────────────── 2. 필드 ──────────────── */
        List<List<int>> map = new();            // 2D 맵 데이터
        static int playerX = 1, playerY = 1;    // 플레이어 위치

        /* ──────────────── 3. 진입점 ──────────────── */
        public void Start()
        {
            Console.Clear();
            Console.OutputEncoding = Encoding.UTF8;   // 특수문자 출력
            LoadMapData();
            RunDungeonLoop();
        }

        /* ──────────────── 4. 맵 로딩 ──────────────── */
        void LoadMapData() => map = GameManager.Instance.GetDungeonMap(1);

        /* ──────────────── 5. 메인 루프 ──────────────── */
        void RunDungeonLoop()
        {
            int height = map.Count;
            int width  = map[0].Count;

            while (true)
            {
                Console.Clear();
                DrawMap(width, height);
                DrawPlayer();

                ConsoleKeyInfo key = Console.ReadKey(true);
                int newX = playerX, newY = playerY;

                if (key.Key is ConsoleKey.UpArrow)        newY--;
                else if (key.Key is ConsoleKey.DownArrow) newY++;
                else if (key.Key is ConsoleKey.LeftArrow) newX--;
                else if (key.Key is ConsoleKey.RightArrow)newX++;

                // 맵 범위 + 벽 체크
                if (newX >= 0 && newX < width &&
                    newY >= 0 && newY < height &&
                    map[newY][newX] != (int)RoomType.Wall)
                {
                    playerX = newX;
                    playerY = newY;
                    HandleRoomEvent();   // 이동 완료 후 이벤트 처리
                }
            }
        }

        /* ──────────────── 6. 이벤트 처리 ──────────────── */
        void HandleRoomEvent()
        {
            RoomType room = (RoomType)map[playerY][playerX];

            switch (room)
            {
                case RoomType.Monster:   CreateMonster();             break;
                case RoomType.Treasure:  WriteLineCenter("보물을 찾았다!");  break;
                case RoomType.Shop:      WriteLineCenter("상점이다.");       break;
                case RoomType.Event:     WriteLineCenter("이벤트 발생!");    break;
                case RoomType.Boss:      WriteLineCenter("보스 등장!"); CreateMonster(); break;
                case RoomType.Exit:      WriteLineCenter("던전 클리어!");   break;
                case RoomType.Empty:     /* 아무 일도 없음 */          break;
            }
        }

        /* ──────────────── 7. 그리기 ──────────────── */
        char GetRoomSymbol(RoomType room) => room switch
        {
            RoomType.Wall     => '■',
            RoomType.Empty    => ' ',
            RoomType.Ladder   => '▲',
            RoomType.Monster  => 'M',
            RoomType.Treasure => 'T',
            RoomType.Shop     => 'S',
            RoomType.Event    => 'E',
            RoomType.Boss     => 'B',
            RoomType.Exit     => 'X',
            _                 => ' '
        };

        void DrawRoomWithPadding(RoomType room, int x, int y)
        {
            int left = x * CellSize;
            int top  = y * CellSize;
            char sym = GetRoomSymbol(room);

            for (int i = 0; i < CellSize; i++)
            {
                Console.SetCursorPosition(left, top + i);

                if (room == RoomType.Wall)
                    Console.Write(new string(sym, CellSize));
                else
                    Console.Write(new string(' ', CellSize));
            }

            if (room != RoomType.Wall)
            {
                Console.SetCursorPosition(left + Padding, top + Padding);
                Console.Write(sym);
            }
        }

        void DrawMap(int width, int height)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    DrawRoomWithPadding((RoomType)map[y][x], x, y);
        }

        void DrawPlayer()
        {
            Console.SetCursorPosition(playerX * CellSize + Padding,
                                      playerY * CellSize + Padding);
            Console.Write('@');
        }

        /* ──────────────── 8. 몬스터 & 전투 ──────────────── */
        void CreateMonster()
        {
            var monster = new Monster();
            GameManager.Instance.monster = monster;
            monster.Start();
            new Battle().Start();
        }

        /* ──────────────── 9. 출력 보조 ──────────────── */
        void WriteLineCenter(string msg)
        {
            Console.SetCursorPosition(0, map.Count * CellSize + 1);
            Console.WriteLine(msg);
            Console.ReadKey(true);   // 확인 후 계속
        }
    }
}
