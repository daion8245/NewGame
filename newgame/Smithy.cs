namespace newgame
{
    internal class Smithy
    {
        public void Start()
        {
            ShowMenu();
        }

        void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("=== 대장간 ===");
            Console.WriteLine("1. 장비 강화");
            Console.WriteLine("2. 이전으로");
            Console.Write("선택: ");

            string input = Console.ReadLine();

            if (input != "1")
            {
                Lobby lobby = new Lobby();
                lobby.Start();
                return;
            }

            EquipUpgrade();
        }

        void EquipUpgrade()
        {
            Equipment equip = null;
            for (int i = 1; i < (int)EquipType.MAX; i++)
            {
                equip = Inventory.Instance.GetEquip((EquipType)i);
                Console.WriteLine($"[{i}] {equip.GetEquipName}");
            }

            Console.WriteLine("입력 : ");
            int idx = 0;
            int.TryParse(Console.ReadLine(), out idx);
            if (idx < 1 || idx > (int)EquipType.MAX)
            {
                Console.WriteLine("잘못된 입력입니다. 다시 시도하세요.");
                Start();
                return;
            }

            equip = Inventory.Instance.GetEquip((EquipType)idx);
            equip.Upgrade();
        }
    }
}
