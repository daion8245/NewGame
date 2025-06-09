using static newgame.MyDiffain;

namespace newgame
{

    internal class Lobby
    {

        public void Start()
        {
            ShowMenu();
        }

        void ShowMenu()
        {
            MenuTxtout();

            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    {
                        GameManager.player.MyStatus.ShowStatus();
                        GameManager.player.MyStatus.ShowInventory();
                        Console.WriteLine();
                        Console.WriteLine("[Enter]를 눌러 돌아가기");
                        Console.ReadKey();
                        Start();
                        break;
                    }
                case "2":
                    {
                        Dungeon dungeon = new Dungeon();
                        break;
                    }
                case "3":
                    {
                        Shop shop = new Shop();
                        shop.Start();
                        break;
                    }
                case "4":
                    {
                        Smithy smithy = new Smithy();
                        smithy.Start();
                        break;
                    }
                case "5":
                    {
                        Hotel hotel = new Hotel();
                        hotel.Start();
                        break;
                    }
                case "6":
                    {
                        break;
                    }
                case "7":
                    {
                        break;
                    }
                case "8":
                default:
                    {
                        Environment.Exit(0);
                        break;
                    }
            }
            Start();
        }

        void MenuTxtout()
        {
            Console.WriteLine("-------------------");
            SlowTxtout("--------쉼터-------", 20);
            SlowTxtout("--1.플레이어 정보--", 5);
            SlowTxtout("--2.던전-----------", 5);
            SlowTxtout("--3.상점-----------", 5);
            SlowTxtout("--4.대장간---------", 5);
            SlowTxtout("--5.여관-----------", 5);
            SlowTxtout("--6.저장/불러오기--", 5);
            SlowTxtout("--7.설정-----------", 5);
            SlowTxtout("--8.게임종료-------", 5);
            Console.WriteLine("-------------------");
            Console.Write("> ");


        }
    }
}
