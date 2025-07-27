using System;
using SkiaSharp;

namespace newgame
{
    internal class Player : Character
    {

        public void Start()
        {
            Create();
        }
        #region 플레이어 생성
        void Create()
        {
            GameManager.Instance.player.MyStatus = new Status();
            MyStatus.charType = CharType.PLAYER;
            SetPlayerStarterItem();

            Console.Clear();
        }
        #endregion

        #region 저장된 플레이어 불러오기
        public void Load()
        {
            GameManager.Instance.player.MyStatus = DataManager.Instance.Load();
        }
        #endregion

        #region 이름 설정
        public void SetName(string name)
        {
            MyStatus.Name = name;
            Console.WriteLine($"설정된 이름 : {MyStatus.Name}");
        }
        public void SetDefStat(int atk, int hp, int def, int mp)
        {
            // 기본값 설정
            MyStatus.level = 1;
            MyStatus.ATK = 8 + atk;
            MyStatus.maxHp = 45 + (hp * 10);
            MyStatus.hp = MyStatus.maxHp;
            MyStatus.DEF = 2 + def;
            MyStatus.maxMp = 30 + (mp * 2);
            MyStatus.mp = MyStatus.maxMp;
            MyStatus.exp = 0;
            MyStatus.nextEXP = 50;
            MyStatus.gold = 25;
        }
        #endregion

        #region 스텟 표시
        public void ShowStat()
        {
            TextDisplayConfig.SlowTxtOut = true;
            TextDisplayConfig.SlowTxtOutTime = 1;
            TextDisplayConfig.SlowTxtLineTime = 0;

            UiHelper.TxtOut(new string[] {
                $"이름 : {MyStatus.Name}",
                $"  레벨 : {MyStatus.level}",
                $"  체력 : {MyStatus.hp}/{MyStatus.maxHp}",
                $"  공격력 : {MyStatus.ATK}",
                $"  방어력 : {MyStatus.DEF}",
                $"  마나 : {MyStatus.mp}/{MyStatus.maxMp}",
                $"  골드 : {MyStatus.gold}",
                $"  경험치 : {MyStatus.exp}/{MyStatus.nextEXP}"
            });
        }
        #endregion

        #region 기본템 설정
        void SetPlayerStarterItem()
        {
            for (int i = 1; i < (int)EquipType.MAX; i++)
            {
                Inventory.Instance.SetEquip((EquipType)i, 1);
            }

            Inventory.Instance.ShowEquipList();
        }
        #endregion

        #region 플레이어 전투
        public override string Attack(Character target)
        {
            string[] battleLog = new string[2];
            battleLog[0] = " ";
            battleLog[1] = " ";

            ShowBattleInfo(target, battleLog);
            int input = SelectBattleAction();

            switch (input)
            {
                case 0:
                    {
                        battleLog[0] = base.Attack(target);
                        ShowBattleInfo(target, battleLog);
                        break;
                    }
                case 1:
                    {
                        //스킬 시스템 구현
                        break;
                    }
                case 2:
                    {
                        UseItem();
                        break;
                    }
                case 3:
                    {
                        Console.Clear();
                        Console.WriteLine("탐색 중...");
                        // 탐색 로직 구현
                        break;
                    }
                case 4:
                    {
                        BattleRun();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return null;
        }

        void BattleRun()
        {
            Random random = new Random();
            int chance = random.Next(1, 101);
            if (chance >= 50)
            {
                isbattleRun = true;
                Console.Clear();
                Console.WriteLine("전투에서 탈출했다");

                Lobby lobby = new Lobby();
                lobby.Start();
                isbattleRun = false;
            }
        }

        #region 전투 액션 선택
        int SelectBattleAction()
        {
            const string Esc = "\u001b[";
            int selected = 0;
            int lineCoordinate;
            ConsoleKey key;
            bool firstRun = false;
            string[] menuOptions = new string[]
            {
                "공격",
                "스킬",
                "아이템",
                "탐색",
                "포기"
            };

            lineCoordinate = Console.CursorTop + menuOptions.Length;

            do
            {
                if (firstRun)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Console.Write($"{Esc}2K");
                        Console.Write($"{Esc}1F");
                    }
                }
                else
                {
                    firstRun = true;
                }

                Console.WriteLine();
                Console.Write("|");
                for (int i = 0; i < menuOptions.Length; i++)
                {

                    Console.Write(" ");

                    if (i == selected)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(">" + menuOptions[i]);
                    }
                    else
                    {
                        Console.Write(" " + menuOptions[i]);
                    }

                    Console.ResetColor();
                }
                Console.WriteLine(" |");

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.RightArrow)
                {
                    selected = (selected + 1) % menuOptions.Length;
                }
                if (key == ConsoleKey.LeftArrow)
                {
                    selected = (selected - 1 + menuOptions.Length) % menuOptions.Length;
                }
            }
            while (key != ConsoleKey.Enter);

            return selected;
        }
        #endregion

        #region 플레이어&적 정보
        void ShowBattleInfo(Character target, string[] battleLog)
        {
            Console.Clear();
            Console.WriteLine($"Name.{MyStatus.Name} \t Name.{target.MyStatus.Name} \t {battleLog[0]}");
            Console.WriteLine($"Lv.{MyStatus.level} \t\t Lv.{target.MyStatus.level} \t\t {battleLog[1]}");
            Console.WriteLine($"Hp.{MyStatus.hp} \t\t Hp.{target.MyStatus.hp}");
        }
        #endregion
        #endregion
    }
}
