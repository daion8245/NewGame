namespace newgame
{
    internal class Shop
    {
        public void Start()
        {
            ShowMenu();
        }

        void ShowMenu()
        {
            Console.WriteLine("--------상점--------");
            Console.WriteLine("1 구매: ");
            Console.WriteLine("2 판매: ");
            Console.WriteLine("3 나가기: ");
            Console.WriteLine("----------------------");

            string selet = Console.ReadLine();

            switch (selet)
            {
                case "1":
                    {
                        ShowBuyMenu();
                        break;
                    }
                case "2":
                    {
                        ShowSellMenu();
                        break;
                    }
                case "3":
                default:
                    {
                        Lobby lobby = new Lobby();
                        lobby.Start();
                        break;
                    }
            }
        }

        void ShowBuyMenu()
        {
            Console.WriteLine("--------상점--------");
            Console.WriteLine("1 장비 구매: ");
            Console.WriteLine("2 아이템 구매: ");
            Console.WriteLine("3 나가기: ");
            Console.WriteLine("----------------------");

            string selet = Console.ReadLine();

            switch (selet)
            {
                case "1":
                    {
                        ShowBuyEquipMenu();
                        break;
                    }
                case "2":
                    {
                        break;
                    }
                case "3":
                default:
                    {
                        Lobby lobby = new Lobby();
                        lobby.Start();
                        break;
                    }
            }
        }

        void ShowSellMenu()
        {
            Console.WriteLine("--------상점--------");
            Console.WriteLine("1 장비 판매: ");
            Console.WriteLine("2 아이템 판매: ");
            Console.WriteLine("3 나가기: ");
            Console.WriteLine("----------------------");

            string selet = Console.ReadLine();

            switch (selet)
            {
                case "1":
                    {
                        break;
                    }
                case "2":
                    {
                        break;
                    }
                case "3":
                default:
                    {
                        Lobby lobby = new Lobby();
                        lobby.Start();
                        break;
                    }
            }
        }

        void ShowBuyEquipMenu()
        {
            Console.Clear();
            for (int i = 1; i < (int)EquipType.MAX; i++)
            {
                Console.WriteLine($"[{i}] {(EquipType)i}");
            }

            Console.WriteLine("입력 : ");
            string input = Console.ReadLine();
            BuyEquipItem(input);
        }

        void BuyEquipItem(string _idx)
        {
            if (string.IsNullOrEmpty(_idx))
            {
                ShowBuyEquipMenu();
                return;
            }
            int idx = int.Parse(_idx);
            //idx = 0 | 0 <= 0(t) | idx = 6 | 6 >= 6(t) | idx = 5 | 5 >= 6(F)
            if (idx <= (int)EquipType.NONE || idx >= (int)EquipType.MAX)
            {
                ShowBuyEquipMenu();
                return;
            }

            int count = 0;
            Console.WriteLine($"[{(EquipType)idx}]");
            List<Equipment> items = new List<Equipment>();
            foreach (Equipment equip in GameManager.Instance.GetEquipment)
            {
                if (equip.GetEquipType == (EquipType)idx)
                {
                    count++;
                    Console.WriteLine($"[{count}] {equip.GetEquipName} - {equip.GetPrice}");
                    items.Add(equip);
                }
            }

            Console.WriteLine("구입할 장비 번호: ");

            string input = Console.ReadLine();

            int listIdx = int.Parse(input) - 1;
            if (GameManager.Instance.player.MyStatus.coin >= items[listIdx].GetPrice)
            {
                GameManager.Instance.player.MyStatus.coin -= items[listIdx].GetPrice;

                Inventory.Instance.AddEquip(items[listIdx]);
                ShowMenu();
            }
            else
            {
                Console.WriteLine("코인 부족");
            }
        }

        void SellEquipItem()
        {
            bool canEquip = Inventory.Instance.ShowCanEquips();
            if (canEquip == false)
            {
                Console.WriteLine("판매할 장비가 없습니다.");
                Start();
                return;
            }

            Console.WriteLine("선택 : ");
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                SellEquipItem();
                return;
            }

            int num = 0;
            bool isNum = int.TryParse(input, out num);
            if (isNum == false)
            {
                SellEquipItem();
                return;
            }

            Inventory.Instance.RemoveCanEquip(num);

            Console.WriteLine("다른 아이템도 판매하시겠습니까? (Y / N)");
            input = Console.ReadLine();

            if (input.ToUpper() == "Y")
            {
                SellEquipItem();
            }
            else
            {
                Start();
            }

            bool CanEquip = Inventory.Instance.ShowCanEquips();
            if (CanEquip == false)
            {
                Console.WriteLine("판매할 장비가 없습니다.");
                return;
            }

            SetEquip();
        }

        void SetEquip()
        {
            Console.WriteLine("착용하려는 장비 번호 : ");
            int idx = 0;
        }
    }
}
