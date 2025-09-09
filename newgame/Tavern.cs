using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newgame
{
    internal class Tavern
    {
        public static int TavernFavorability = 0;
        public void Start()
        {
            ShowTavernMenu();
        }

        #region 여관 진입
        void ShowTavernMenu()
        {
            Console.Clear();

            UiHelper.TxtOut(["\t 「여관」",
                               "여관 주인: 어서오게나 모험가씨",
                ""]);

            int sel;

            if (TavernFavorability < 10)
            {
                sel = UiHelper.SelectMenu([
                        "숙박",
                        "구매",
                        "대화",
                        "퀘스트",
                        "나가기",
                ]);
            }
            else
            {
                sel = UiHelper.SelectMenu([
                        "숙박",
                        "구매",
                        "대화",
                        "퀘스트",
                        "나가기",
                        "도박"
                    ]);
            }

            TavernSelet(sel);
        }
        void TavernSelet(int sel)
        {
            switch (sel)
            {
                case 0:
                    {
                        SleepTavern();
                        break;
                    }
                case 1:
                    {

                        break;
                    }
                case 2:
                    {
                        break;
                    }
                case 3:
                    {
                        break;
                    }
                case 4:
                    {
                        Lobby lobby = new Lobby();
                        Console.Clear();
                        lobby.Start(); 
                        break;
                    }
                case 5:
                    {
                        TavernGambling();
                        break;
                    }
            }
        }
        #endregion

        #region 숙박
        void SleepTavern()
        {
            var playerstat = GameManager.Instance.player.MyStatus;
            Console.Clear();

            while (true)
            {
                Console.WriteLine("숙박비는 10Gold 라네");
                Console.WriteLine();
                int sel = UiHelper.SelectMenu(["숙박하기",
                                            "돌아가기"]);

                switch (sel)
                {
                    case 0:
                        {
                            if (playerstat.gold >= 10)
                            {
                                UiHelper.TxtOut(["여관의 방을 빌렸다," +
                                        "드디어 쉴수 있어!"]);
                                Console.WriteLine();

                                Console.Write("Z");
                                Thread.Sleep(400);
                                Console.Write("z");
                                Thread.Sleep(400);
                                Console.WriteLine("z");
                                Thread.Sleep(1000);

                                playerstat.Hp = playerstat.maxHp;
                                UiHelper.TxtOut(["체력이 최대치로 회복되었다!",
                                               $"체력 : {playerstat.Hp}/{playerstat.maxHp}"]);

                                UiHelper.WaitForInput("[Enter]를 눌러 계속");
                                Start();
                            }
                            else
                            {
                                UiHelper.TxtOut([$"돈이 부족하다(현제 골드 : {playerstat.gold})"]);

                                UiHelper.WaitForInput("[Enter]를 눌러 계속");
                                SleepTavern();
                                break;
                            }
                            break;
                        }
                    case 1:
                        {
                            Start();
                            break;
                        }
                }
            }
        }
        #endregion

        #region 여관 상점
        void TavernShop()
        {
            var playerstat = GameManager.Instance.player.MyStatus;

            Console.Clear();

            Console.WriteLine("미구현");
            UiHelper.WaitForInput("[Enter]를 눌러 계속");

            SleepTavern();
        }
        #endregion

        #region 대화

        #endregion

        #region 퀘스트

        #endregion

        #region 도박
        void TavernGambling()
        {
            Console.Clear();

            Console.WriteLine("???:도박장에 오신걸 환영합니다. 모험가님");
            Console.WriteLine();

            int sel = UiHelper.SelectMenu(["도박",
                                        "나가기"]);

            switch(sel)
            {
                case 0:
                    {
                        TavernGambling_Betting();
                        break;
                    }
                case 1:
                    {
                        Start();
                        break ;
                    }
            }
        }

        void TavernGambling_Betting()
        {
            var playerstat = GameManager.Instance.player.MyStatus;
            Console.Clear();

            int sel = UiHelper.SelectMenu(["골드 배팅",
                                        "룰 설명",
                                        "나가기"]);

            switch(sel)
            {
                case 0:
                    {
                        Console.WriteLine("배팅할 금액을 설정해주세요.");
                        Console.Write(":");
                        if (int.TryParse(Console.ReadLine(), out int battingGold))
                        {
                            if(playerstat.gold >= battingGold)
                            {
                                playerstat.gold -= battingGold;

                                TavernGambling_Roulette(battingGold);
                            }
                            else
                            {
                                Console.WriteLine("골드가 부족합니다.");
                                UiHelper.WaitForInput("[ENTER]를 눌러 계속");
                                TavernGambling_Betting();
                            }
                        }
                        else
                        {
                            Console.WriteLine("올바른 숫자를 입력해주세요.");
                            Thread.Sleep(1000);
                            TavernGambling_Betting();
                        }

                        break;
                    }

                case 1:
                    {
                        Console.Clear();
                        UiHelper.TxtOut(["게임은 슬롯머신 형식으로 진행됩니다.",
                                       "$ x 3 = x10",
                                       "# x 3 = x5",
                                       "@ x 1 = x1",
                                       "& x 1 = x0.5",
                                       "* x 1 = x0.1"]);

                        Console.WriteLine();
                        UiHelper.WaitForInput("[ENTER]를 눌러 계속");
                        TavernGambling_Betting();
                        break;
                    }

                case 3:
                    {
                        Start();
                        break;
                    }
            }
        }

        void TavernGambling_Roulette(int battingGold)
        {
            Console.Clear();

            var symbols = new[] { "$", "#", "@",};
            var payout = new Dictionary<string, double>
            {
                { "$", 10.0 },
                { "#", 5.0 },
                { "@", 1.0 }
            };

            var rand = new Random();
            string[] result = new string[3];
            for (int i = 0; i < 3; i++)
            {
                result[i] = symbols[rand.Next(symbols.Length)];
            }

            Console.WriteLine($"[{result[0]}] [{result[1]}] [{result[2]}]");
            Thread.Sleep(1000);

            double reward = 0;
            if (result[0] == result[1] && result[1] == result[2])
            {
                reward = battingGold * payout[result[0]];
                Console.WriteLine($"{result[0]} 3개! 배당률 {payout[result[0]]}배 획득!");
            }
            else
            {
                Console.WriteLine("아쉽게도 당첨되지 않았습니다.");
            }

            var playerstat = GameManager.Instance.player.MyStatus;
            if (reward > 0)
            {
                playerstat.gold += (int)reward;
                Console.WriteLine($"획득 골드: {(int)reward}");
            }
            else
            {
                Console.WriteLine("획득 골드: 0");
            }

            UiHelper.WaitForInput("[ENTER]를 눌러 계속");
            TavernGambling();
        }
        #endregion
    }
}
