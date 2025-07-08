using NewGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace newgame
{
    /// <summary>
    /// Shows the initial game menu and handles basic start actions.
    /// </summary>
    internal class StartMessage
    {
        public void Start()
        {
            GameStartMessage();
        }

        /// <summary>
        /// Displays the start menu and processes the player's selection.
        /// </summary>
        void GameStartMessage()
        {
            TextDisplayConfig.SlowTxtOut = true;
            TextDisplayConfig.SlowTxtOutTime = 30;
            TextDisplayConfig.SlowTxtLineTime = 0;
            UiHelper.TxtOut(
                [
                "TXTRPG-Remake",
                "버전 0.0.1",
                "제작자: 다이온",
                "게임에 오신걸 환영합니다!",
                "",
                ]);

            Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine("계속하실려면 Enter를 눌러주세요.");
            //Console.ReadKey();
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
                    LoadGame();
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
            GameBuild gameBuild = new GameBuild();
            gameBuild.Start();
        }

        void LoadGame()
        {
            if (DataManager.Instance.IsPlayerData())
            {
                Player player = new Player();
                GameManager.Instance.player = player;

                player.Load();

                Lobby lobby = new Lobby();
                lobby.Start();
            }
        }
    }
}
