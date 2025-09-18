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
                        GameManager.Instance.ReturnToLobby();
                        break;
                    }
            }
        }
        #endregion

        #region 장비/아이템 구매
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

            Player player = GameManager.Instance.RequirePlayer();

            if (player.MyStatus.gold >= items[menuSelect].GetPrice)
            {
                player.MyStatus.gold -= items[menuSelect].GetPrice;
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

        #region 장비 판매
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

        #region 아이템 관련 추가
        void ShowBuyConsumableItemMenu()
        {
            for (int i = 1; i < Enum.GetValues(typeof(ItemType)).Length; i++)
            {
                Console.WriteLine($"[{i}] {Inventory.Instance.GetItemName((ItemType)i)}");
            }

            Console.Write("입력 : ");
            string? input = Console.ReadLine();
            BuyConsumableItem(input);
        }

        #region 아이템 관련 추가
        void BuyConsumableItem(string? _idx)
        {
            // _idx 문자열이 비어있다면
            if (string.IsNullOrEmpty(_idx))
            {
                // 잘못된 입력이므로, 이전 메뉴로 돌아가게 만들어줄 것
                // 현재 여기에서는 ShowBuyEquipMenu <- 이 상태로 돌아가게 해놓음
                ShowBuyConsumableItemMenu();
                return;
            }

            if (!int.TryParse(_idx, out int idx))
            {
                Console.WriteLine("잘못된 입력입니다.");
                ShowBuyConsumableItemMenu();
                return;
            }
            // idx = 1 ~ 5 범위가 아닌 경우에는 함수 종료
            if (idx <= (int)ItemType.NONE || idx >= Enum.GetValues(typeof(ItemType)).Length)
            {
                // 잘못된 입력이므로, 이전 메뉴로 돌아가게 만들어줄 것
                // 현재 여기에서는 ShowBuyEquipMenu <- 이 상태로 돌아가게 해놓음
                ShowBuyConsumableItemMenu();
                return;
            }

            // 구매 진행
            Item? item = GameManager.Instance.FindItem((ItemType)idx);
            if (item == null)
            {
                Console.WriteLine("해당 아이템을 찾을 수 없습니다.");
                return;
            }

            Player player = GameManager.Instance.RequirePlayer();
            if (player.MyStatus.gold >= item.ItemPrice)
            {
                // 돈이 있네?
                player.MyStatus.gold -= item.ItemPrice;
                Inventory.Instance.AddItem(item);

                ShowMenu();
            }
            else
            {
                Console.WriteLine("가지고 있는 재화가 부족합니다.");
            }
        }
        #endregion
        #endregion

        #region 상점 판매품목 초기화
        public void ResetShopItems()
        {
            items.Clear();
        }
        #endregion
    }
}
