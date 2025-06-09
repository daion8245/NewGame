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
            GameManager.SetEquipList();
        }

        static void GameStart()
        {
            Console.WriteLine("------------------------");
            SlowTxtOut("---------TXTRPG---------", 20);
            SlowTxtOut("게임에 오신걸 환영합니다.", 50);
            Console.WriteLine("------------------------");
            SlowTxtOut("----- 1.   새로하기 ----", 35);
            SlowTxtOut("----- 2.   이어하기 ----", 35);
            SlowTxtOut("----- 3.   게임종료 ----", 35);
            Console.WriteLine("------------------------");
            Console.Write("> ");

            InputMenu();
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

        static void InputMenu()
        {
            string input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    {
                        Player player = new Player();
                        GameManager.player = player;
                        player.Start();
                        break;
                    }
                case "2":
                    {
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
