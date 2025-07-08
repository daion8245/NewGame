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
            SetPlayerStarterItem();

            Console.Clear();
        }

        public void Load()
        {
            GameManager.Instance.player.MyStatus = DataManager.Instance.Load();
        }

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
            DeffenStatic.SlowTxtOut = true;
            DeffenStatic.SlowTxtOutTime = 1;
            DeffenStatic.SlowTxtLineTime = 0;

            MyDiffain.TxtOut(new string[] {
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
            Console.WriteLine();
            int input = MyDiffain.SeletMenu(["공격","스킬","아이템","도망"]);

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
