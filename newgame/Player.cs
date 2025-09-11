using System;
using System.Collections.Generic;
using SkiaSharp;

namespace newgame
{
    internal class Player : Character
    {
        private readonly Skills skillSystem = new Skills();

        string[] battleLog = new string[2] {"",""};

        #region 플레이어 초기 설정

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
            SetPlayerStarterSkill();

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
            MyStatus.Hp = MyStatus.maxHp;
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
                    $"  체력 : {MyStatus.Hp}/{MyStatus.maxHp}",
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

        #region 기본스킬 설정
        void SetPlayerStarterSkill()
        {
            skillSystem.AddCanUseSkill("파이어볼");
            skillSystem.AddCanUseSkill("아쿠아 볼");
        }
        #endregion

        #endregion

        #region 플레이어 전투

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

        #region 플레이어 기본공격
        public override string[] Attack(Character target)
        {
            ShowBattleInfo(target, battleLog);

            int input = SelectBattleAction();

            switch (input)
            {
                
                case 0:
                    {
                        beforHP[0] = MyStatus.Hp;
                        beforHP[1] = target.MyStatus.Hp;
                        battleLog = base.Attack(target);
                        ShowBattleInfo(target, battleLog);
                        break;
                    }
                case 1:
                    {
                        BattleSkillLogic(target);
                        ShowBattleInfo(target, battleLog);
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
        #endregion

        #region 스킬 사용

        #region 스킬 리스트 표시
        public SkillType ShowSkillList()
        {
            return skillSystem.ShowCanUseSkill();
        }
        #endregion

        //스킬 클래스에서 스킬을 가져와 사용하는 함수

        void BattleSkillLogic(Character target)
        {
            SkillType useSkill = ShowSkillList();
            battleLog = UseAttackSkill(target, useSkill);

            ShowBattleInfo(target, battleLog);

            // 스킬 특수효과 추가 처리
            if (useSkill.name == null) return;

            switch (useSkill.name)
            {
                case "파이어볼":
                    {
                        AddTickSkill(useSkill.name, useSkill.skillTurn);
                        break;
                    }
                case "아쿠아 볼":
                    {
                        AddTickSkill(useSkill.name, useSkill.skillTurn);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion

        #region 도망치기

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

        #endregion

        #region 플레이어&적 정보
        int[] beforHP = new int[2];

        public void ShowBattleInfo(Character target, string[] battleLog)
        {
            Console.Clear();

            Console.WriteLine($"Name.{MyStatus.Name} \t Name.{target.MyStatus.Name} \t {battleLog[0]}");
            Console.WriteLine($"Lv.{MyStatus.level} \t\t Lv.{target.MyStatus.level} \t\t {battleLog[1]}");


            Console.WriteLine($"Hp.{MyStatus.Hp} \t\t Hp.{target.MyStatus.Hp}");
        }
        #endregion

        #endregion 플레이어 전투
    }
}

