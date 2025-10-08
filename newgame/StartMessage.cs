using System;
using System.Threading;

namespace newgame
{
    /// <summary>
    /// Shows the initial game menu and handles basic start actions.
    /// </summary>
    internal class StartMessage
    {
        readonly Action _startNewGame;
        readonly Func<bool> _loadGame;

        public StartMessage(Action startNewGame, Func<bool> loadGame)
        {
            _startNewGame = startNewGame ?? throw new ArgumentNullException(nameof(startNewGame));
            _loadGame = loadGame ?? throw new ArgumentNullException(nameof(loadGame));
        }

        public void Start()
        {
            GameStartMessage();
        }

        void GameStartMessage()
        {
            UiHelper.TxtOut(
                [
                "TXTRPG-Remake",
                "버전 0.0.1",
                "제작자: 다이온",
                "게임에 오신걸 환영합니다!",
                "",
                ]);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.ResetColor();

            int sel = UiHelper.SelectMenu([
                "새로운 게임",
                "게임 불러오기",
                "게임 종료",
            ]);

            Console.Clear();
            switch (sel)
            {
                case 0:
                    Console.WriteLine("새로운 게임을 시작합니다...");
                    Thread.Sleep(1000);
                    NewGame();
                    return;
                case 1:
                    Console.Clear();
                    Console.WriteLine("게임을 불러옵니다...");
                    Thread.Sleep(1000);
                    if (!LoadGame())
                    {
                        Console.Clear();
                        Console.WriteLine("저장된 데이터가 없습니다.");
                        UiHelper.WaitForInput();
                        GameStartMessage();
                    }
                    return;

                case 2:
                    Console.Clear();
                    Console.WriteLine("게임을 종료합니다...");
                    Environment.Exit(0);
                return;
            }
        }

        void NewGame()
        {
            _startNewGame();
        }

        bool LoadGame()
        {
            return _loadGame();
        }
    }
}
