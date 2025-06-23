using System.Threading.Channels;

namespace newgame
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Init();

            GameStart();
        }

        static void Init()
        {
            DataManager.Instance.LoadAllEquipData();
            DataManager.Instance.LoadEnemyData();
        }

        static void GameStart()
        {
            Console.WriteLine("------------------------");
            SlowTxtOut("---------TXTRPG---------", 20);
            SlowTxtOut("게임에 오신걸 환영합니다.", 50);
            Console.WriteLine("------------------------");
            MyDiffain.SelectMenu(
                ("새로하기", NewGameCreate),
                ("이어하기", continueGame),
                ("게임종료", () => Environment.Exit(0))
                );
        }

        static void SlowTxtOut(string s, int t)
        {
            for (int i = 0; i < s.Length; i++)
            {
                Console.Write(s[i]);
                Thread.Sleep(t);
            }
            Console.WriteLine();
        }

        static void NewGameCreate()
        {
            Player player = new Player();
            GameManager.Instance.player = player;
            player.Start();
        }

        static void continueGame()
        {
            if (DataManager.Instance.IsPlayerData())
            {
                Player player = new Player();
                GameManager.Instance.player = player;

                player.Load();

                Lobby lobby = new Lobby();
                lobby.Start();
            }

            static void EndGame()
            {

            }

        static void InputMenu()
        {
            string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        {
                            Player player = new Player();
                            GameManager.Instance.player = player;
                            player.Start();
                            break;
                        }
                    case "2":
                        {
                            if (DataManager.Instance.IsPlayerData())
                            {
                                Player player = new Player();
                                GameManager.Instance.player = player;

                                player.Load();

                                Lobby lobby = new Lobby();
                                lobby.Start();
                            }
                            break;
                        }
                    case "3":
                        {
                            Environment.Exit(0);
                            break;
                        }
                    default:
                        {
                            Console.Clear();
                            GameStart();
                            break;
                        }
                }
            }
        }
    }
}
