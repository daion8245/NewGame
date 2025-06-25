using static newgame.MyDiffain;

namespace newgame
{
    internal class Player : Character
    {
        MyDiffain MyDiffain = new MyDiffain();

        public void Start()
        {
            Create();
        }

        void Create()
        {
            GameManager.Instance.player.MyStatus = new Status();
            MyStatus.charType = CharType.PLAYER;

            Console.Clear();
            SetName();
            EnterLobby();
        }

        public void Load()
        {
            GameManager.Instance.player.MyStatus = DataManager.Instance.Load();
        }

        void SetName()
        {
            while (true)
            {
                Console.Clear();
                SlowTxtout("플레이어의 이름을 입력하세요.", 30);
                Console.Write("> ");
                string str = Console.ReadLine();

                MyStatus.Name = str;
                SlowTxtout($"입력된 이름: {MyStatus.Name} 이 정말 맞습니까?", 30);
                Console.WriteLine("Y / F");
                Console.Write("> ");
                string str2 = Console.ReadLine();
                if (str2.ToUpper() == "Y")
                {
                    SetStatus();
                    break;
                }
                else
                {
                    MyStatus.Name = null;
                    str = null;
                    str2 = null;
                }
            }

        }

        void SetStatus()
        {
            int point = 10;
            Random random = new Random();

            while (point != 0)
            {
                Console.Clear();

                Console.WriteLine("-------------------------");
                Console.WriteLine($"-\t1. 공격력: {MyStatus.ATK}\t-");
                Console.WriteLine($"-\t2. 체력: {MyStatus.maxHp}\t-");
                Console.WriteLine($"-\t3. 방어력: {MyStatus.DEF}\t-");
                Console.WriteLine("-\t4. 랜덤   \t-");
                Console.WriteLine("-------------------------");

                Console.WriteLine($"능력치 설정: 남은 스텟포인트 {point}");
                Console.Write("> ");
                string str = Console.ReadLine();
                if (!int.TryParse(str, out int num1))
                {
                    Console.WriteLine("잘못된 입력입니다!");
                    Thread.Sleep(1000);
                    continue;
                }

                switch (num1)
                {
                    case 1:
                        point--;
                        MyStatus.ATK++;
                        break;
                    case 2:
                        point--;
                        MyStatus.maxHp++;
                        break;
                    case 3:
                        point--;
                        MyStatus.DEF++;
                        break;
                    case 4:
                        point--;
                        int randomStat = random.Next(1, 4);
                        if (randomStat == 1)
                        {
                            MyStatus.ATK++;
                        }
                        else if (randomStat == 2)
                        {
                            MyStatus.maxHp++;
                        }
                        else if (randomStat == 3)
                        {
                            MyStatus.DEF++;
                        }
                        break;
                    default:
                        Console.WriteLine("잘못된 입력입니다!");
                        Thread.Sleep(1000);
                        break;
                }
            }

            MyStatus.level = 1;
            MyStatus.ATK += 1;
            MyStatus.DEF += 1;
            MyStatus.maxHp *= 10;
            MyStatus.maxHp += 100;
            MyStatus.hp = MyStatus.maxHp;
            MyStatus.gold = 10000;
            MyStatus.exp = 0;
            MyStatus.nextEXP = 10;

            MyStatus.ShowStatus();
        }

        void EnterLobby()
        {
            Thread.Sleep(1);
            SetPlayerStarterItem();

            Console.WriteLine("계속하기 [ENTER]");
            Console.ReadKey();

            Console.Clear();
            Lobby lobby = new Lobby();
            lobby.Start();
        }
        void SetPlayerStarterItem()
        {
            for (int i = 1; i < (int)EquipType.MAX; i++)
            {
                Inventory.Instance.SetEquip((EquipType)i, 1);
            }

            Inventory.Instance.ShowEquipList();
        }

        public override void Attack(Character target)
        {
            Console.WriteLine("------------------");
            Console.WriteLine("---- 1. 공격  ----");
            Console.WriteLine("---- 2. 아이템 ---");
            Console.WriteLine("---- 3. 도망 -----");
            Console.WriteLine("------------------");

            string input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    {
                        Console.Clear();
                        base.Attack(target);
                        break;
                    }
                case "2":
                    {
                        break;
                    }
                case "3":
                    {
                        BattleRun();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        void BattleRun()
        {
            Random range = new Random();
            int rdx = range.Next(1, 101);
            if (rdx >= 50)
            {
                isbattleRun = true;
                Console.Clear();
                Console.WriteLine("전투에서 탈출했다");

                Lobby lobby = new Lobby();
                lobby.Start();
                isbattleRun = false;
            }
        }
    }
}
