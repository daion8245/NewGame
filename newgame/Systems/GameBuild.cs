using System;
using newgame.Systems;

namespace newgame
{
    /// <summary>
    /// Composition root responsible for wiring the manual dependency graph.
    /// </summary>
    internal sealed class GameBuild
    {
        private readonly DataManager _dataManager;
        private readonly GameManager _gameManager;
        private readonly Func<Player> _playerFactory;
        private StartMessage? _startMessage;

        private GameBuild(
            DataManager dataManager,
            GameManager gameManager,
            Func<Player> playerFactory)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
            _playerFactory = playerFactory ?? throw new ArgumentNullException(nameof(playerFactory));
        }

        public static GameBuild Create()
        {
            DataManager dataManager = DataManager.Instance;
            GameManager gameManager = GameManager.Instance;
            Func<Player> playerFactory = () => new Player(gameManager.BattleLogService);

            GameBuild build = new GameBuild(dataManager, gameManager, playerFactory);
            StartMessage startMessage = new StartMessage(build.StartNewGame, build.TryLoadGame);
            build.SetStartMessage(startMessage);

            return build;
        }

        public void Run()
        {
            Init();

            GetStartMessage().Start();
        }

        private void Init()
        {
            GameManager.Instance.SetItemList();
            _dataManager.LoadAllEquipData();
            _dataManager.LoadEnemyData();
            _dataManager.LoadBossData();
            _dataManager.LoadDungeonMap();
            _dataManager.LoadSkillData();
            _dataManager.LoadPlayer_ClassData();
            _gameManager.SetItemList();
        }

        private void SetStartMessage(StartMessage startMessage)
        {
            _startMessage = startMessage ?? throw new ArgumentNullException(nameof(startMessage));
        }

        private StartMessage GetStartMessage()
        {
            return _startMessage ?? throw new InvalidOperationException("StartMessage is not configured.");
        }

        private void StartNewGame()
        {
            PlayerNameSet();
            SetStatus();
            LaunchLobby();
        }

        private bool TryLoadGame()
        {
            if (!_dataManager.IsPlayerData())
            {
                return false;
            }

            Player player = _playerFactory();
            _gameManager.Player = player;
            player.Load();
            LaunchLobby();
            return true;

        }

        private void LaunchLobby()
        {
            Console.Clear();
            _gameManager.ReturnToLobby();
        }

        #region 플레이어 이름 정하기
        private void PlayerNameSet()
        {
            while (true)
            {
                Console.Clear();
                Console.Write("플레이어의 이름을 입력해 주세요 : ");
                string? inputName = Console.ReadLine();

                if (!IsValidName(inputName))
                {
                    ShowInvalidNameMessage();
                    continue;
                }

                Console.Clear();
                Console.WriteLine($"입력하신 이름 [{inputName}] 이 정말 맞습니까?");

                int sel = UiHelper.SelectMenu(new[] { "Y", "N" });

                if (sel == 0)
                {
                    Player player = _playerFactory();
                    Status freshStatus = player.Initializer.Start();
                    player.ApplyStatus(freshStatus);
                    player.SetName(inputName!);
                    _gameManager.Player = player;
                    return;
                }
            }
        }

        private static bool IsValidName(string? name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length >= 2 && name.Length <= 10;
        }

        private static void ShowInvalidNameMessage()
        {
            UiHelper.TxtOut(new[]
            {
                "잘못된 이름입니다.",
                "다시 입력해 주세요.",
                string.Empty,
                "enter를 눌러 계속"
            });
            UiHelper.WaitForInput();
        }
        #endregion

        #region 스텟 설정
        private void SetStatus()
        {
            Player player = CurrentPlayer;

            int atk = 0;
            int hp = 0;
            int def = 0;
            int mp = 0;

            Console.Clear();

            Console.WriteLine($"플레이어 {player.MyStatus.Name}의 기초 스텟을 설정합니다.");
            int sel = UiHelper.SelectMenu(new[]
            {
                "랜덤 설정",
                "직접 설정"
            });

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

            player.Initializer.SetDefStat(atk, hp, def, mp);

            AssignInitialClass(player);

            player.Initializer.ShowStat();
            Console.WriteLine("[Enter]를 눌러 계속");
            Console.ReadKey();
        }
        private void AssignInitialClass(Player player)
        {
            var classes = _gameManager.GetPlayerClasses();
            if (classes.Count == 0)
            {
                return;
            }

            CharacterClassType chosen = classes[0];
            player.AssignClass(chosen);
            Console.WriteLine($"기본 직업 [{chosen.name}] 이(가) 적용되었습니다.");

            Inventory.Instance.AddItem(ItemType.M_WOOD, 1);
            Inventory.Instance.AddItem(ItemType.F_POTION_LOW_HP, 3);
            Inventory.Instance.AddItem(ItemType.F_POTION_MIDDLE_HP);
            Inventory.Instance.AddItem(ItemType.F_POTION_HIGH_HP);
        }

        #endregion

        #region 랜덤 스텟 설정
        private static int[] RandomStat()
        {
            int atk = 0;
            int hp = 0;
            int def = 0;
            int mp = 0;

            for (int i = 0; i < 10; i++)
            {
                int ranstat = Random.Shared.Next(1, 5);
                switch (ranstat)
                {
                    case 1:
                        atk++;
                        break;
                    case 2:
                        hp++;
                        break;
                    case 3:
                        def++;
                        break;
                    default:
                        mp++;
                        break;
                }
            }
            return new[] { atk, hp, def, mp };
        }
        #endregion

        #region 선택 스텟 설정
        private static int[] SelstatSet()
        {
            int atk = 0;
            int hp = 0;
            int def = 0;
            int mp = 0;

            int statcoin = 10;

            while (statcoin > 0)
            {
                Console.Clear();
                Console.WriteLine($"남은 포인트 : {statcoin}");

                int selstat = UiHelper.SelectMenu(new[] { "공격력", "체력", "방어력" });

                switch (selstat)
                {
                    case 0:
                        atk++;
                        break;
                    case 1:
                        hp++;
                        break;
                    case 2:
                        def++;
                        break;
                    default:
                        mp++;
                        break;
                }

                statcoin--;
            }

            return new[] { atk, hp, def, mp };
        }
        #endregion

        private Player CurrentPlayer => _gameManager.RequirePlayer();
    }
}
