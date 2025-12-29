using newgame.Characters;
using newgame.Items;
using newgame.Services;
using newgame.UI;

namespace newgame.Locations
{
    internal class Lobby
    {
        public void Start()
        {
            LobbyMenu();
        }

        #region 메뉴 출력
        void LobbyMenu()
        {
            Console.Clear();

            Console.WriteLine("\t[마을]");

            Player player = GameManager.Instance.RequirePlayer();

            int menusel = UiHelper.SelectMenu([
                "상태창 보기",
                "인벤토리 보기",
                "미궁으로 들어가기",
                "상점",
                "여관",
                "전직소",
                "대장간",
                "저장",
                "게임 종료",
                ]);

            switch (menusel)
            {
                //상태창
                case 0:
                    {
                        Console.Clear();
                        Console.WriteLine("[ 상태창 ] \r");
                        player.Initializer.ShowStat();
                        Console.WriteLine("[Enter]를 눌러 돌아가기");
                        Console.ReadKey();
                        Console.Clear();
                        break;
                    }
                //인벤토리
                case 1:
                    {
                        Console.Clear();
                        Console.WriteLine("[ 인벤토리 ] \r");
                        player.MyStatus.ShowInventory();
                        break;
                    }
                //던전
                case 2:
                    {
                        Console.Clear();
                        Dungeon.Instance.Start();
                        break;
                    }
                //상점
                case 3:
                    {
                        Console.Clear();

                        Shop shop = new Shop("마을 상점");

                        shop.AddEquip(EquipType.WEAPON, 2);
                        shop.AddEquip(EquipType.GLOVE, 2);
                        shop.AddEquip(EquipType.HELMET, 2);
                        shop.AddEquip(EquipType.SHIRT, 2);
                        shop.AddEquip(EquipType.PANTS, 2);
                        shop.AddEquip(EquipType.SHOES, 2);
                        shop.items.Add(ItemType.F_POTION_LOW_HP);
                        shop.items.Add(ItemType.F_POTION_MIDDLE_HP);
                        shop.items.Add(ItemType.F_ETC_RESETNAME);
                        shop.Start();
                        break;
                    }
                //여관
                case 4:
                    {
                        Tavern tavern = new Tavern();
                        tavern.Start();
                        return;
                    }
                //전직소
                case 5:
                    {
                        ClassHall classHall = new ClassHall();
                        classHall.Start();
                        break;
                    }
                //대장간
                case 6:
                    {
                        Smithy smithy = new Smithy();
                        smithy.Start();
                        break;
                    }
                //저장
                case 7:
                    {
                        Console.Clear();
                        DataManager.Instance.Save(player.MyStatus);
                        UiHelper.WaitForInput("게임 저장됨. (SHIFT를 눌러 계속)");
                        break;
                    }
                //종료
                case 8:
                    {
                        Console.Clear();
                        Console.WriteLine("게임을 종료합니다...");
                        Environment.Exit(0);
                        return;
                    }
            }
            Start();
        }
        #endregion
    }
}
