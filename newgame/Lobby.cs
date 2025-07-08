using static newgame.UiHelper;

namespace newgame
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

            int menusel = UiHelper.SelectMenu([
                "상태창 보기",
                "인벤토리 보기",
                "미궁으로 들어가기",
                "상점",
                "여관",
                "대장간",
                "저장",
                "게임 종료",
                ]);

            switch (menusel)
            {
                case 0:
                    {
                        Console.Clear();
                        Console.WriteLine("[ 상태창 ] \r");
                        GameManager.Instance.player.ShowStat();
                        Console.WriteLine("[Enter]를 눌러 돌아가기");
                        Console.ReadKey();
                        Console.Clear();
                        break;
                    }
                case 1:
                    {
                        Console.Clear();
                        Console.WriteLine("[ 인벤토리 ] \r");
                        GameManager.Instance.player.MyStatus.ShowInventory();
                        break;
                    }
                case 2:
                    {
                        Console.Clear();
                        Dungeon dungeon = new Dungeon();
                        dungeon.Start();
                        break;
                    }
                case 3:
                    {
                        Console.Clear();

                        Shop shop = new Shop();
                        
                        shop.items.Add(EquipType.WEAPON, 2);
                        shop.items.Add(EquipType.GLOVE, 2);
                        shop.items.Add(EquipType.HELMET, 2);
                        shop.items.Add(EquipType.SHIRT, 2);
                        shop.items.Add(EquipType.PANTS, 2);
                        shop.items.Add(EquipType.SHOES, 2);
                        shop.Start();
                        break;
                    }
                case 4:
                    {
                        Tavern tavern = new Tavern();
                        tavern.Start();
                        return;
                    }
                case 5:
                    {
                        Smithy smithy = new Smithy();
                        smithy.Start();
                        break;
                    }
                case 6:
                    {
                        Console.Clear();
                        DataManager.Instance.Save(GameManager.Instance.player.MyStatus);
                        UiHelper.WaitForInput("게임 저장됨. (SHIFT를 눌러 계속)");
                        break;
                    }
                case 7:
                    {
                        return;
                    }
            }
            Start();
        }
        #endregion
    }
}
