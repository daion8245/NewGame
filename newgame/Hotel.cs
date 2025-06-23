using static newgame.MyDiffain;

namespace newgame
{
    internal class Hotel
    {
        public bool doback = false;
        public bool HidenKey = false;
        public void Start()
        {
            HotelLobby();

            if (GameManager.Instance.player.IsDead = true)
            {
                RecoveryHP();
            }
        }

        void HotelLobby()
        {
            Console.Clear();

            Console.WriteLine("--------여관--------");
            SlowTxtout("1. 체력 회복", 20);
            SlowTxtout("2. 여관 상점", 20);
            SlowTxtout("3. 대화", 20);
            SlowTxtout("4. 여관 나가기", 20);
            Console.WriteLine("-------------------");
            Console.Write("🛏> ");
            string st1 = Console.ReadLine();

            switch (st1)
            {
                case "1":
                    {
                        RecoveryHP();
                        break;
                    }
                case "2":
                    {
                        HotelShop();
                        break;
                    }
                case "3":
                    {
                        HotelExit();
                        break;
                    }
                case "4":
                default:
                    {
                        HotelExit();
                        break;
                    }
                case "5":
                    {
                        if (doback == true)
                        {

                        }
                        break;
                    }
            }
        }

        void RecoveryHP()
        {
            Console.Clear();
            GameManager.Instance.player.MyStatus.hp = GameManager.Instance.player.MyStatus.maxHp;
            SlowTxtout("모든 체력이 회복되었습니다.", 30);
            SlowTxtout("체력: " + GameManager.Instance.player.MyStatus.hp, 30);

            Console.WriteLine("돌아가기(ENTER)");
            Console.ReadKey();

            HotelLobby();
        }

        void HotelShop()
        {
            Console.Clear();

            Console.WriteLine("--------여관 상점--------");
            SlowTxtout("1. 음식 구매(낮은 확률로 스텟 UP[8|coin])", 20);
            SlowTxtout("2. 맥주!!!!(랜덤 버프 or 디버프[5|coin])", 20);
            SlowTxtout("3. 상점 나가기", 20);
            Console.WriteLine("----------------------");
            Console.Write("> ");
            string st1 = Console.ReadLine();

            switch (st1)
            {
                case "1":
                    {
                        if (GameManager.Instance.player.MyStatus.coin < 8)
                        {
                            Console.WriteLine("코인이 부족합니다!");
                            Console.WriteLine("돌아가기(ENTER)");
                            Console.ReadKey();
                            HotelShop();
                        }
                        else
                        {
                            GameManager.Instance.player.MyStatus.coin -= 8;
                            food();
                        }
                        break;
                    }
                case "2":
                    {
                        if (GameManager.Instance.player.MyStatus.coin < 5)
                        {
                            Console.WriteLine("코인이 부족합니다!");
                            Console.WriteLine("돌아가기(ENTER)");
                            Console.ReadKey();
                            HotelShop();
                        }
                        else
                        {
                            GameManager.Instance.player.MyStatus.coin -= 5;
                            beer();
                        }
                        break;
                    }
                case "3":
                default:
                    {
                        HotelLobby();
                        break;
                    }
            }
        }


        void food()
        {
            Random random = new Random();
            int food = random.Next(1, 101);

            SlowTxtout("음식을 구매했다!", 30);
            SlowTxtout("으으음....", 100);
            SlowTxtout("이 맛은?..!!", 40);
            Thread.Sleep(500);

            if (food >= 80 && food != 100 && food != 2)
            {
                SlowTxtout("이 음식은 맛있다!", 30);
                int buff = random.Next(1, 4);
                int buff2 = random.Next(1, 5);
                if (buff == 1)
                {
                    if (GameManager.Instance.player.MyStatus.ATK <= 1)
                    {
                        Console.WriteLine("더이상 공격력은 내려갈수 없다!");
                        Console.WriteLine("체력이 감소했다.");
                        GameManager.Instance.player.MyStatus.hp -= buff2;
                    }
                    else
                    {
                        GameManager.Instance.player.MyStatus.ATK += buff2;
                        SlowTxtout($"{buff2} 만큼 공격력이 증가했다!", 30);
                    }
                }
                else if (buff == 2)
                {
                    if (GameManager.Instance.player.MyStatus.DEF <= 1)
                    {
                        Console.WriteLine("더이상 방어력은 내려갈수 없다!");
                        Console.WriteLine("체력이 감소했다.");
                        GameManager.Instance.player.MyStatus.hp -= buff2;
                    }
                    else
                    {
                        GameManager.Instance.player.MyStatus.DEF += buff2;
                        SlowTxtout($"{buff2} 만큼 방어력이 증가했다!", 30);
                    }
                }
                else
                {
                    if (GameManager.Instance.player.MyStatus.maxHp <= 1)
                    {
                        Console.WriteLine("더이상 최대 체력은 내려갈수 없다!");
                        Console.WriteLine("체력이 감소했다.");
                        GameManager.Instance.player.MyStatus.hp -= buff2;
                    }
                    else
                    {
                        GameManager.Instance.player.MyStatus.maxHp += buff2;
                        SlowTxtout($"{buff2} 만큼 최대 체력이 증가했다!", 30);
                    }

                }
            }
            else if (food == 100)
            {
                SlowTxtout("이 음식은 매우!!! 맛있다!!!!", 30);
                SlowTxtout("모든 스텟이 크게 증가했다!", 30);
                GameManager.Instance.player.MyStatus.ATK += 10;
                GameManager.Instance.player.MyStatus.DEF += 10;
                GameManager.Instance.player.MyStatus.maxHp += 10;
            }
            else if (food == 2)
            {
                SlowTxtout("이 음식은 혀에게 죄를 짓는 행위와도 같을정도로 맛없다!", 30);
                SlowTxtout("스텟이 매우 크게 감소했다!", 30);
                if (GameManager.Instance.player.MyStatus.ATK <= 1)
                {
                    Console.WriteLine("더이상 공격력은 내려갈수 없다!");
                    Console.WriteLine("체력이 감소했다.");
                    GameManager.Instance.player.MyStatus.hp -= 20;
                }
                else
                {
                    GameManager.Instance.player.MyStatus.ATK -= 20;
                }
                if (GameManager.Instance.player.MyStatus.DEF <= 1)
                {
                    Console.WriteLine("더이상 방어력은 내려갈수 없다!");
                    Console.WriteLine("체력이 감소했다.");
                    GameManager.Instance.player.MyStatus.hp -= 20;
                }
                else
                {
                    GameManager.Instance.player.MyStatus.DEF -= 20;
                }
                if (GameManager.Instance.player.MyStatus.maxHp <= 1)
                {
                    Console.WriteLine("더이상 최대 체력은 내려갈수 없다!");
                    Console.WriteLine("체력이 감소했다.");
                    GameManager.Instance.player.MyStatus.hp -= 20;
                }
                else
                {
                    GameManager.Instance.player.MyStatus.maxHp -= 20;
                }
            }
            else
            {
                SlowTxtout("평범한 음식이었다.", 30);
            }

            if (GameManager.Instance.player.MyStatus.hp <= 0)
            {
                SlowTxtout("끔찍한 음식이였다....", 30);
                SlowTxtout("게임오버", 30);
                Thread.Sleep(1000);
                Environment.Exit(0);
            }

            Console.WriteLine();
            Console.WriteLine("[Enter]를 눌러 돌아가기");
            Console.ReadKey();
            HotelShop();
        }


        void beer()
        {
            Random random = new Random();
            int beer = random.Next(1, 101);
            SlowTxtout("맥주를 구매했다!", 30);
            SlowTxtout("먹고 취하자!!", 30);
            SlowTxtout("......", 100);

            if (beer > 60)
            {
                SlowTxtout("역시 맥주는 맜있다!", 30);
                SlowTxtout("기분이 좋다!", 30);
                int buff = random.Next(1, 4);
                int buff2 = random.Next(1, 5);
                if (buff == 1)
                {
                    GameManager.Instance.player.MyStatus.ATK += buff2;
                    SlowTxtout($"{buff2} 만큼 공격력이 증가했다!", 30);
                }
                else if (buff == 2)
                {
                    GameManager.Instance.player.MyStatus.DEF += buff2;
                    SlowTxtout($"{buff2} 만큼 방어력이 증가했다!", 30);
                }
                else
                {
                    GameManager.Instance.player.MyStatus.maxHp += buff2;
                    SlowTxtout($"{buff2} 만큼 체력이 증가했다!", 30);
                }
            }
            else
            {
                SlowTxtout("역시 맥주는 맜있다!", 50);
                Thread.Sleep(2000);
                SlowTxtout("부웱엑엑ㅇㄱㄱㅇㅇㅇㅇㅇㅇㅇㅇ.", 30);
                Thread.Sleep(500);
                int debuff = random.Next(1, 4);
                int debuff2 = random.Next(1, 5);
                if (debuff == 1)
                {
                    if (GameManager.Instance.player.MyStatus.ATK <= 1)
                    {
                        Console.WriteLine("더이상 공격력은 내려갈수 없다!");
                        Console.WriteLine("체력이 감소했다.");
                        GameManager.Instance.player.MyStatus.hp -= debuff2;
                    }
                    else
                    {
                        GameManager.Instance.player.MyStatus.ATK -= debuff2;
                        SlowTxtout($"{debuff2} 만큼 공격력이 감소했다!", 30);
                    }
                }
                else if (debuff == 2)
                {
                    if (GameManager.Instance.player.MyStatus.DEF <= 1)
                    {
                        Console.WriteLine("더이상 방어력은 내려갈수 없다!");
                        Console.WriteLine("체력이 감소했다.");
                        GameManager.Instance.player.MyStatus.hp -= debuff2;
                    }
                    else
                    {
                        GameManager.Instance.player.MyStatus.DEF -= debuff2;
                        SlowTxtout($"{debuff2} 만큼 방어력이 감소했다!", 30);
                    }
                }
                else
                {
                    if (GameManager.Instance.player.MyStatus.maxHp <= 1)
                    {
                        Console.WriteLine("더이상 최대 체력은 내려갈수 없다!");
                        Console.WriteLine("체력이 감소했다.");
                        GameManager.Instance.player.MyStatus.hp -= debuff2;
                    }
                    else
                    {
                        SlowTxtout($"{debuff2} 만큼 최대 체력이 감소했다!", 30);
                        GameManager.Instance.player.MyStatus.maxHp -= debuff2;
                    }
                }
            }

            if (GameManager.Instance.player.MyStatus.hp <= 0)
            {
                SlowTxtout("알코올 중독으로 사망..", 30);
                SlowTxtout("체력이 0이 되었다.", 30);
                SlowTxtout("역시 음주는 하지말자!", 30);
                SlowTxtout("게임오버", 30);
                Thread.Sleep(1000);
                Environment.Exit(0);
            }

            Console.WriteLine();
            Console.WriteLine("[Enter]를 눌러 돌아가기");
            Console.ReadKey();
            HotelShop();
        }


        void Hoteltalk()
        {
            int HotelAffinity = 0;

            Console.Clear();
            SlowTxtout("여관 주인: 흐음... 먼 길을 오셨군. 피곤한 얼굴이야.", 20);
            Console.WriteLine("");
            Console.WriteLine("--------대화--------");
            SlowTxtout("1. 잡담", 20);
            SlowTxtout("2. 농담", 20);
            SlowTxtout("3. 던전에 관하여", 20);
            SlowTxtout("4. 돈에 관하여", 20);
            SlowTxtout("5. 대화 나가기", 20);
            Console.WriteLine("-------------------");
            Console.Write("🛏> ");
            string st1 = Console.ReadLine();

            switch (st1)
            {
                case "1":
                    {
                        if (HotelAffinity <= 0)
                        {
                            SlowTxtout("여관 주인: 날씨가 좋군.", 20);
                        }
                        else if (HotelAffinity >= 20 || HotelAffinity < 50)
                        {
                            SlowTxtout("또 왔군 오늘은 무슨 일이 있었나", 20);
                        }
                        else if (HotelAffinity >= 50)
                        {
                            SlowTxtout("어서오게나! 맥주 하나 드릴까?", 20);
                            Console.WriteLine("------------");
                            Console.WriteLine("1. 마신다");
                            Console.WriteLine("2. 마시지 않는다");
                            string st2 = Console.ReadLine();

                            switch (st2)
                            {
                                case "1":
                                    {
                                        beer();
                                        break;
                                    }
                                case "2":
                                    {
                                        break;
                                    }
                            }

                        }
                        break;
                    }
                case "2":
                    {
                        SlowTxtout("여관 주인: 농담은 잘 못해.", 20);
                        break;
                    }
                case "3":
                    {
                        SlowTxtout("여관 주인: 던전은 위험해. 조심해.", 20);
                        break;
                    }
                case "4":
                    {
                        SlowTxtout("여관 주인: 돈은 많을수록 좋아.", 20);
                        break;
                    }
                case "5":
                default:
                    {
                        HotelLobby();
                        break;
                    }
            }
            Console.WriteLine();
            Console.WriteLine("[Enter]를 눌러 돌아가기");
            Console.ReadKey();
        }
        void HotelExit()
        {
            Lobby lobby = new Lobby();
            lobby.Start();
        }
    }
}
