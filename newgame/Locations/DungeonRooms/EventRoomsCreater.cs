using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newgame.DungeonRooms
{
    internal enum EventRoomsId
    {
        OldNotice, //낡은 안내문
        SlimeRaid, //슬라임 습격
        //MirrorPuzzle //거울 퍼즐
    }

    internal class EventRoomsCreater
    {
        Player player = GameManager.Instance.player;
        EventRooms eventRoom;

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
        //                    tt => GameManager.Instance.player.MyStatus.Hp += 10
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
                    eventRoom = new EventRooms("낡은 표지판", "저 멀리 무언가가 깜빡인다.", OldNotice);
                    break;
                case EventRoomsId.SlimeRaid:
                    eventRoom = new EventRooms("슬라임 습격", "축축한 방 안에 들어섰다", SlimeRaid);
                    break;
                    //case EventRoomsId.MirrorPuzzle:
                    //    // 거울 퍼즐 이벤트 방 생성 로직
                    //    break;
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
        //낡은 안내문
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
                    UiHelper.TxtOut(["글자를 해독하는데 성공했다!","최대 마나가 소량 늘어났다",$"최대 마나:{player.MyStatus.MaxMp}+8 -> {player.MyStatus.MaxMp + 8}"]);
                    player.MyStatus.MaxMp += 8;
                    player.MyStatus.Mp += 8;
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

        //슬라임 습격
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
                player.MyStatus.gold += bonus;
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
    }
}
