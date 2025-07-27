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
        public override void Attack(Character target)
        {

            // ── 콘솔 출력: 최대한 간단 ────────────────────────────────
            const int lineWidth = 40;                                        // 글자 수
            Console.WriteLine("(이미지)".PadRight(lineWidth));               // :contentReference[oaicite:1]{index=1}
            Console.WriteLine(new string('-', lineWidth));                   // :contentReference[oaicite:2]{index=2}

            Console.WriteLine();
            int input = UiHelper.SelectMenu(["공격","스킬","아이템","도망"]);

            switch (input)
            {
                case 0:
                    {
                        Console.Clear();
                        base.Attack(target);
                        break;
                    }
                case 1:
                    {
                        UseItem();
                        break;
                    }
                case 2:
                    {
                        
                        break;
                    }
                    case 3:
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
    }
}
