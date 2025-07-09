using System.ComponentModel;

namespace newgame
{
    internal class Shop
    {
        public Dictionary<EquipType,int> items = new Dictionary<EquipType,int>();

        public void Start()
        {
            ShowMenu();
        }
        #region 메뉴
        void ShowMenu()
        {
            Console.Clear();

            UiHelper.TxtOut(["\t 「상점」",
                               "상점 주인: 천천히 둘러보세요!",""]);

            int selet = UiHelper.SelectMenu(["구매",
                                             "판매",
                                             "나가기"]);

            switch (selet)
            {
                case 0:
                    {
                        BuyEquipItem();
                        break;
                    }
                case 1:
                    {
                        SellEquipItem();
                        break;
                    }
                case 2:
                default:
                    {
                        Lobby lobby = new Lobby();
                        lobby.Start();
                        break;
                    }
            }
        }
        #endregion

        #region 아이템 구매
        void BuyEquipItem()
        {
            Console.Clear();
            UiHelper.TxtOut(["\t 「상점/구매」",
                ""]);

            List<Equipment> items = new List<Equipment>();
            List<string> itemNames = new List<string>();

            foreach (var item in this.items)
            {
                foreach (Equipment equip in GameManager.Instance.GetEquipment)
                {
                    if (equip.GetEquipType == item.Key && equip.GetEquipID == item.Value)
                    {
                        itemNames.Add($"{equip.GetEquipName} - {equip.GetPrice}골드");
                        items.Add(equip);
                    }
                }
            }
            itemNames.Add("나가기");

            int menuSelect = UiHelper.SelectMenu(itemNames.ToArray());

            if (menuSelect == itemNames.Count - 1)
            {
                ShowMenu();
                return;
            }

            if (GameManager.Instance.player.MyStatus.gold >= items[menuSelect].GetPrice)
            {
                GameManager.Instance.player.MyStatus.gold -= items[menuSelect].GetPrice;
                Inventory.Instance.AddEquip(items[menuSelect]);

                UiHelper.TxtOut(["", $"{items[menuSelect].GetEquipName} 구매 완료",""]);

                int GoLobbySel = UiHelper.SelectMenu(["구매 계속하기","나가기"]);
                if (GoLobbySel == 0)
                {
                    BuyEquipItem();
                    return;
                }
                else
                {
                    ShowMenu();
                    return;
                }

            }
            else
            {
                Console.WriteLine("코인 부족");
                UiHelper.WaitForInput("[ENTER]를 눌러 계속");
                ShowMenu();
                return;
            }

        }
        #endregion

        #region 아이템 판매
        void SellEquipItem()
        {
            Console.Clear();

            int input = Inventory.Instance.ShowCanEquips();
            if (input == -1)
            {
                Console.WriteLine("┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
                Console.WriteLine("┃         .장비 없음.           ┃");
                Console.WriteLine("┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛");
                UiHelper.WaitForInput("[ENTER]를 눌러 계속");
                Start();
                return;
            }

            //int num = 0;

            //bool isNum = int.TryParse(input, out num);
            //if (isNum == false)
            //{
            //    SellEquipItem();
            //    return;
            //}

            Inventory.Instance.RemoveCanEquip(input + 1);

            //Console.WriteLine("다른 아이템도 판매하시겠습니까? (Y / N)");

            //if (input.ToUpper() == "Y")
            //{
            //    SellEquipItem();
            //}
            //else
            //{
            //    Start();
            //}

            int CanEquip = Inventory.Instance.ShowCanEquips();
            if (CanEquip == -1)
            {
                Console.WriteLine("┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
                Console.WriteLine("┃          인벤토리            ┃");
                Console.WriteLine("┃                             ┃");
                Console.WriteLine("┃         장비 없음            ┃");
                Console.WriteLine("┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛");
                return;
            }
        }
        #endregion

        #region 상점 판매품목 초기화
        public void ResetShopItems()
        {
            items.Clear();
        }
        #endregion
    }
}
