namespace newgame
{
    internal class MyDiffain
    {

        public static void SlowTxtout(string s, int t)
        {
            for (int i = 0; i < s.Length; i++)
            {
                Console.Write(s[i]);
                Thread.Sleep(t / 10);
            }
            Console.WriteLine();
        }

        #region 선택 메뉴
        public static int SelectMenu(params (string Text, Action Action)[] items)
        {
            if (items == null || items.Length == 0)
                throw new ArgumentException("옵션이 필요합니다.");

            int selected = 0;
            bool firstRun = false;
            ConsoleKey key;

            do
            {
                if (firstRun)
                    ClearLines(items.Length);
                else
                    firstRun = true;

                for (int i = 0; i < items.Length; i++)
                {
                    Console.ForegroundColor = (i == selected) ? ConsoleColor.Green : ConsoleColor.Gray;
                    Console.WriteLine((i == selected ? "> " : "  ") + items[i].Text);
                }
                Console.ResetColor();

                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow)
                    selected = (selected - 1 + items.Length) % items.Length;
                else if (key == ConsoleKey.DownArrow)
                    selected = (selected + 1) % items.Length;

            } while (key != ConsoleKey.Enter);

            ClearLines(items.Length);
            Console.ResetColor();

            items[selected].Action?.Invoke();
            return selected;
        }

        static void ClearLines(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Console.Write("\u001b[2K");
                Console.Write("\u001b[1F");
            }
        }
        #endregion
    }
}
