using newgame.Characters;
using newgame.Enemies;
using newgame.Services;
using newgame.Systems;
using newgame.UI;

namespace newgame.Locations.DungeonRooms
{
    internal enum EventRoomsId
    {
        OldNotice, //낡은 안내문
        SlimeRaid, //슬라임 습격
        MirrorPuzzle //거울 퍼즐
        
    }

    internal class EventRoomsCreater
    {
        private Player Player => GameManager.Instance.RequirePlayer();
        #region 던전 이벤트 방 유틸

        void DungeonEventRoomEventTrigger(Func<int> dungeonEvent, Action<Type>[] callbacks)
        {
            int result = dungeonEvent();

            if (callbacks != null && result >= 0 && result < callbacks.Length)
            {
                callbacks[result]?.Invoke(dungeonEvent.GetType());
            }
        }

        // 간편한 콜백 배열 생성을 위한 헬퍼 메서드
        Action<Type>[] CreateCallbacks(params Action<Type>[] callbacks) => callbacks;

        #region 예제용 구문

        //void testRoomEvent()
        //{
        //    DungeonEventRoomEventTrigger(
        //        () => UiHelper.MessageAndSelect(new[] { "1", "2", "3" }, new[] { "1", "2", "3" }),
        //        CreateCallbacks(
        //            t => DungeonEventRoomEventTrigger(
        //                () => UiHelper.MessageAndSelect(new[] { "1", "2", "3" }, new[] { "1", "2", "3" }),
        //                CreateCallbacks(
        //                    tt => Console.WriteLine("선택: 1"),
        //                    tt => Console.WriteLine("선택: 2"),
        //                    tt => GameManager.Instance.Player.MyStatus.Hp += 10
        //                )
        //            ),
        //            t => Console.WriteLine("선택: 1"),
        //            t => Console.WriteLine("선택: 2")
        //        )
        //    );
        //}

        #endregion

        #endregion

        #region 던전 이벤트 방 생성

        // 이미 사용된 이벤트 방 ID를 추적하는 집합
        private static HashSet<int> _usedEventRoomIds = new HashSet<int>();

        public void CreateDungeonEventRoom()
        {
            int randomRoomSelect;
            Random rand = new Random();
            
            // 모든 이벤트 방이 사용되었으면 사용 기록 초기화
            if (_usedEventRoomIds.Count >= Enum.GetNames(typeof(EventRoomsId)).Length)
            {
                _usedEventRoomIds.Clear();
            }
            
            do
            {
                randomRoomSelect = rand.Next(0, Enum.GetNames(typeof(EventRoomsId)).Length);
            } while (_usedEventRoomIds.Contains(randomRoomSelect));
            
            // 선택된 ID를 사용된 목록에 추가
            _usedEventRoomIds.Add(randomRoomSelect);
            
            // 선택된 ID에 따라 이벤트 방 생성
            EventRoomsId selectedEventRoom = (EventRoomsId)randomRoomSelect;

            switch (selectedEventRoom)
            {
                case EventRoomsId.OldNotice:
                    _ = new EventRooms("낡은 표지판", "저 멀리 무언가가 깜빡인다.", OldNotice);
                    break;
                case EventRoomsId.SlimeRaid:
                    _ = new EventRooms("슬라임 습격", "축축한 방 안에 들어섰다", SlimeRaid);
                    break;
                case EventRoomsId.MirrorPuzzle:
                    _ = new EventRooms("신비한 거울", "눈앞에 거울이 수없이 펼쳐진 방이 보인다..", MirrorPuzzle);
                    break;
            }
        }

        #endregion

        #region 몬스터와 배틀
        private void MonsterBattle(int monsterId)
        {
            Monster monster = new Monster();
            GameManager.Instance.monster = monster;
            monster.Start(monsterId);
            Battle battle = new Battle();
            battle.Start();
        }
        #endregion

        #region 던전 이벤트 방 내용

        #region 튜토리얼

        /*
         * 이벤트 방 만드는법
         *
         * 1. void (이벤트 방 이름(영어로)) => DungeonEventRoomEventTrigger(.....);을 쓴다
         * 
         * 2. () 괄호 안에 넣고싶은걸 넣는다 왠만해서는 UiHelper.MessageAndSelect(new [] {메세지(예: 테스트 방인듯 하다..) , // <- 콤마 넣기 \\(1.1번 선택지를 고른다 2.2번 선택지를 고른다 3.3번 선택지를 고른다 )}
         * , CreateCallbacks(t=> console.WriteLine("1번 선택지 고름"), t=> console.writeline("2번 선택지 고름"), t=> console.writeline("3번 선택지 고름")) ); 형식으로 넣는다
         *  CreateCallbacks() <- 선택지를 구현하는 함수 이 안에 다시 CrateCallbacks가능 선택지 안에 선택지 만들고 싶을때 사용
         * }) 형식으로 넣는다
         *
         * 3. 다 만든 후 맨 위의 EventRoomsId enum에다가 (영어로 된 이벤트 방 이름)을 넣는다
         *
         * 4.그 후 던전 이벤트 방 생성 region을 열고 그 맨 아레 switch문에다가
         * case EventRoomsId.(영어로 된 이벤트 방 이름): _ = new EventRooms("(한글로 된 이벤트 방 이름)", "(이벤트 방 설명)", (영어로 된 이벤트 방 이름)); break; 형식으로 넣는다
        */
        
        void TutorialRoom() => DungeonEventRoomEventTrigger(() => UiHelper.MessageAndSelect(
            new []{"이 방은 튜토리얼 방이다..","무언가 선택지를 고를 수 있을것 같다.."},
            new []{"1.1번 선택지를 고른다","2.2번 선택지를 고른다","3.3번 선택지를 고른다"}), CreateCallbacks(
            t => Console.WriteLine("1번 선택지 고름"),
            t => Console.WriteLine("2번 선택지 고름"),
            t => Console.WriteLine("3번 선택지 고름")
        ));
        #endregion

        #region 낡은 안내문 

        void OldNotice() => DungeonEventRoomEventTrigger(() => UiHelper.MessageAndSelect(
            new []{"안개 낀 표지판이 미세하게 깜빡인다.."},
            new []{"1.글자를 해독한다","2.표지판을 부순다","3.무시하고 지나간다"}), CreateCallbacks(
            t =>
            {
                Console.WriteLine();
                Random rand = new Random();
                int chance = rand.Next(1,3);
                if (chance > 1)
                {
                    UiHelper.TxtOut(["글자를 해독하는데 성공했다!","최대 마나가 소량 늘어났다",$"최대 마나:{Player.MyStatus.MaxMp}+8 -> {Player.MyStatus.MaxMp + 8}"]);
                    Player.MyStatus.MaxMp += 8;
                    Player.MyStatus.Mp += 8;
                }
                else
                {
                    UiHelper.TxtOut(["글자를 해독하는데 실패했다...","아무것도 얻지 못했다."]);
                }
            },
            t =>
            {
                Console.WriteLine();
                UiHelper.TxtOut(["표지판을 박살냈다...","야생의 유령이 나타났다!"], SlowTxtLineTime: 1000);
                MonsterBattle(6);
            },
            t => Console.WriteLine("\n"+"표지판을 무시하고 지나갔다.")
        ));

        #endregion

        #region 슬라임 습격

        void SlimeRaid() => DungeonEventRoomEventTrigger(() => UiHelper.MessageAndSelect(
            new[] { "방 안이 축축하다... 슬라임의 습격이다!" },
            new[] { "1.싸운다", "2.도망친다" }), CreateCallbacks(
            t =>
            {
                Console.WriteLine();
                UiHelper.TxtOut(["야생의 슬라임이 많이 나타났다!"], SlowTxtLineTime: 1000);
                for (int i = 0; i < 3; i++)
                {
                    MonsterBattle(1);
                }
                Random rand = new Random();
                int bonus = rand.Next(10, 150);
                UiHelper.TxtOut(["슬라임을 모두 물리쳤다!", "슬라임 방 뒤쪽에 보물 상자가 있다!","\n",$"보물 상자에서 {bonus}만큼의 골드를 획득했다!"]);
                Player.MyStatus.gold += bonus;
            },
            t =>
            {
                Console.WriteLine();
                int chance = UiHelper.GetRandomInt1To100();
                if (chance > 50)
                {
                    UiHelper.TxtOut(["도망치는데 성공했다!"]);
                }
                else
                {
                    UiHelper.TxtOut(["도망치는데 실패했다...", "야생의 슬라임이 매우 많이 나타났다!"], SlowTxtLineTime: 1000);
                    for (int i = 0; i < 5; i++)
                    {
                        MonsterBattle(1);
                    }

                    Console.WriteLine("슬라임을 모두 물리쳤다.");
                }
            }
        )); 

        #endregion

        #region 거울 퍼즐

            void MirrorPuzzle() => DungeonEventRoomEventTrigger(
        () => UiHelper.MessageAndSelect(
            new[] {
                "벽면 가득 거울이 늘어서 있다.",
                "중앙 문양은 빛을 기다리는 듯 미세하게 깜빡인다.",
                "문엔 새겨져 있다: \"빛을 잃은 길은 닫힌다\""
            },
            new[] {
                "1. 거울 각도를 맞춰 본다(퍼즐 시도)",
                "2. 주변을 조사한다",
                "3. 거울을 깨뜨린다" 
            }
        ),
        CreateCallbacks(
            // 1) 퍼즐 시도
            t =>
        {
            Console.WriteLine();
            int route = UiHelper.MessageAndSelect(
                new[] { "세 개의 거울이 있다. 어떤 순서로 조정할까?" },
                new[] { "1. 왼-중-오", "2. 오-중-왼", "3. 되는대로 막 돌린다" }
            );

            // 선택별 성공 난이도(작을수록 쉬움): 0→60%, 1→40%, 2→20%
            int need = 0;
            if (route == 0) need = 40;      // 61~100 성공
            else if (route == 1) need = 60; // 61~100 성공(=40%)
            else need = 80;                 // 81~100 성공(=20%)

            int roll = UiHelper.GetRandomInt1To100();
            if (roll > need)
            {
                int before = Player.MyStatus.MaxMp;
                int gainMp = 12;
                Random rand = new Random();
                int bonusGold = rand.Next(60, 161);

                UiHelper.TxtOut(
                    new[] {
                        "빛이 반사되어 문양이 밝게 점등된다!",
                        "내면의 마력이 깨어난 듯하다.",
                        $"최대 마나: {before}+{gainMp} -> {before + gainMp}",
                        $"보물 상자에서 {bonusGold} 골드를 획득했다!"
                    },
                    SlowTxtLineTime: 800
                );

                Player.MyStatus.MaxMp += gainMp;
                Player.MyStatus.Mp += gainMp;
                
                Player.MyStatus.gold += bonusGold;
            }
            else
            {
                UiHelper.TxtOut(
                    new[] {
                        "거울이 역광을 뿜어내며 퍼즐이 초기화됐다...",
                        "차가운 기운이 방을 메운다. 유령이 나타났다!"
                    },
                    SlowTxtLineTime: 800
                );
                MonsterBattle(6);
            }
        },

        // 2) 주변을 조사
        t =>
        {
            Console.WriteLine();
            int chance = UiHelper.GetRandomInt1To100();
            if (chance > 55)
            {
                Random rand = new Random();
                int addMp = 6;
                int gold = rand.Next(30, 91);

                UiHelper.TxtOut(new[] {
                    "거울 틈새에서 숨겨진 스위치를 발견했다!",
                    "약하게 반사된 빛이 문양을 돕는다.",
                    $"최대 마나가 소량 늘어났다: {Player.MyStatus.MaxMp}+{addMp} -> {Player.MyStatus.MaxMp + addMp}",
                    $"주변에서 귀금속 파편을 모아 {gold} 골드를 얻었다."
                });

                Player.MyStatus.MaxMp += addMp;
                Player.MyStatus.Mp += addMp;
                Player.MyStatus.gold += gold;
            }
            else
            {
                UiHelper.TxtOut(new[] {
                    "발판을 잘못 밟았다!",
                    "거울이 흔들리며 어둠이 스며든다... 유령이 나타났다!"
                }, SlowTxtLineTime: 800);
                MonsterBattle(6);
            }
        },

        // 3) 거울을 깨뜨린다
        t =>
        {
            Console.WriteLine();
            UiHelper.TxtOut(new[] {
                "거울을 강하게 내려쳤다!",
                "깨진 파편 속에서 한기가 피어오른다... 유령이 나타났다!"
            }, SlowTxtLineTime: 800);

            MonsterBattle(6);

            Random rand = new Random();
            int scrap = rand.Next(40, 81);
            UiHelper.TxtOut(new[] {
                "전투가 끝났다.",
                $"깨진 은 파편을 모아 {scrap} 골드를 팔아치웠다."
            });

            Player.MyStatus.gold += scrap;
        }
        ));

        #endregion

        #region 강한 보스의 시련

        void HardBossTrial() => DungeonEventRoomEventTrigger(() => UiHelper.MessageAndSelect(
            new[] { "앞에 범접하지 못할 무시무시한 몬스터가 보인다." }, new[] { "1.싸운다", "2.도망친다" }), CreateCallbacks(t =>
        {
            Console.WriteLine();
            UiHelper.TxtOut(["강력한 몬스터와의 전투가 시작된다!"], SlowTxtLineTime: 1000);
            
            Boss boss = new Boss();
            GameManager.Instance.monster = boss;
            boss.StartBoss(Dungeon.floor + 1);
            Battle battle = new Battle();
            bool win = battle.Start();
            
            if (win)
            {
                Random rand = new Random();
                //스킬 보상 로직
                
                UiHelper.WaitForInput();
            }
            else
            {
                UiHelper.TxtOut(["강력한 몬스터에게 패배했다...", "던전 밖으로 쫓겨났다..."]);
                UiHelper.WaitForInput();
            }
        }));

        #endregion

        #endregion
    }
}
