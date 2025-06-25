using newgame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGame
{
    internal class GameBuild
    {
        public void Start()
        {
            PlayerNameSet();
            SetStatus();


            Lobby Lobby = new Lobby();
            Console.Clear();
            Lobby.Start();
        }

        #region 플레이어 이름 정하기
        void PlayerNameSet()
        {
            while (true)
            {
                Console.Clear();

                Console.Write("플레이어의 이름을 입력해 주세요 : ");
                try
                {
                    string inputName = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(inputName) || inputName.Length < 2 || inputName.Length > 10)
                    {
                        throw new Exception("이름은 2자 이상 10자 이하로 입력해야 합니다.");
                    }

                    Console.Clear();
                    Console.WriteLine($"입력하신 이름 [{inputName}] 이 정말 맞습니까?");

                    int sel = MyDiffain.SeletMenu(new[] { "Y", "N" });

                    if (sel == 0)
                    {
                        Console.Clear();

                        GameManager.Instance.player = new Player();
                        GameManager.Instance.player.Create();
                        GameManager.Instance.player.SetName(inputName);
                        return;
                    }
                }
                catch (Exception e)
                {

                    MyDiffain.TxtOut(new[]
                    {
                    $"잘못된 이름입니다.",
                    "다시 입력해 주세요.",
                     "",
                    "enter를 눌러 계속"
                    });
                    Console.ReadKey();
                }
            }
        }
        #endregion

        #region 스텟 설정
        void SetStatus()
        {
            int atk = 0;
            int hp = 0;
            int def = 0;
            int mp = 0;

            Console.Clear();

            Console.WriteLine($"플레이어 {GameManager.Instance.player.MyStatus.Name}" +
                                                        $"의 기초 스텟을 설정합니다.");
            int sel = MyDiffain.SeletMenu([
                "랜덤 설정",
                "직접 설정"]);

            if (sel == 0)
            {
                int[] ranstat = RandomStat();
                atk = ranstat[0];
                hp = ranstat[1];
                def = ranstat[2];
                mp = ranstat[3];

                Console.Clear();
            }

            else
            {
                int[] setstat = SelstatSet();
                atk = setstat[0];
                hp = setstat[1];
                def = setstat[2];
                mp = setstat[3];

                Console.Clear();
            }

            GameManager.Instance.player.SetDefStat(atk, hp, def, mp);

            GameManager.Instance.player.ShowStat();
            Console.WriteLine("[Enter]를 눌러 계속");
            Console.ReadKey();
        }
        #endregion

        #region 랜덤 스텟 설정
        int[] RandomStat()
        {
            int atk = 0;
            int hp = 0;
            int def = 0;
            int mp = 0;
            Random random = new Random();

            for (int i = 0; i < 10; i++)
            {
                int ranstat = random.Next(1, 4);
                switch (ranstat)
                {
                    case 1:
                        {
                            atk++;
                            break;
                        }
                    case 2:
                        {
                            hp++;
                            break;
                        }
                    case 3:
                        {
                            def++;
                            break;
                        }
                    case 4:
                        {
                            mp++;
                            break;
                        }
                }
            }
            return new int[] { atk, hp, def, mp };
        }
        #endregion

        #region 선택 스텟 설정
        int[] SelstatSet()
        {
            int atk = 0;
            int hp = 0;
            int def = 0;
            int mp = 0;

            int statcoin = 10;

            while (statcoin > 0)
            {
                Console.WriteLine($"남은 포인트 : {statcoin}");

                int selstat = Deffen.SeletMenu(new[] { "공격력", "체력", "방어력" });

                switch (selstat)
                {
                    case 0:
                        {
                            atk++;
                            break;
                        }
                    case 1:
                        {
                            hp++;
                            break;
                        }
                    case 2:
                        {
                            def++;
                            break;
                        }
                    case 3:
                        {
                            mp++;
                            break;
                        }
                }

                statcoin--;
            }

            return new int[] { atk, hp, def, mp };
        }
        #endregion

        #region 플레이어 기본 장비 설정
        void SetDefEquipment()
        {

        }
        #endregion
    }
}
