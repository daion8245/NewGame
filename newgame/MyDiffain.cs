using System.Net.Security;

namespace newgame
{
    internal class MyDiffain
    {

        #region 선택 메뉴
        public static int SeletMenu(string[] str)
        {
            int line_coordinates;
            int selected = 0;
            bool FirstRun = false;

            ConsoleKey key;

            line_coordinates = Console.CursorTop + str.Length;

            do
            {
                if (FirstRun == true)
                {
                    MenuTxtDel(str.Length, line_coordinates);
                }
                else
                {
                    FirstRun = true;
                }
                for (int i = 0; i < str.Length; i++)
                {
                    if (i == selected)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"> {str[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {str[i]}");
                    }
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selected = (selected - 1 + str.Length) % str.Length;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selected = (selected + 1) % str.Length;
                }

            } while (key != ConsoleKey.Enter);

            Console.ResetColor();

            return selected;

        }

        const string ESC = "\u001b[";
        static void MenuTxtDel(int Line, int coordinates)
        {
            Console.SetCursorPosition(0, coordinates);

            for (int i = 0; i < Line; i++)
            {
                Console.Write($"{ESC}2K");
                Console.Write($"{ESC}1F");
            }
        }
        #endregion

        #region 텍스트 고급 출력
        public static void TxtOut(string[] str)
        {
            foreach (string line in str)
            {
                if (DeffenStatic.SlowTxtOut)
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        Console.Write(line[i]);
                        Thread.Sleep(DeffenStatic.SlowTxtOutTime);
                    }

                    Console.WriteLine();
                    Thread.Sleep(DeffenStatic.SlowTxtLineTime);
                }
                else
                {
                    Console.WriteLine(line);
                    Thread.Sleep(DeffenStatic.SlowTxtLineTime);
                }
            }
        }
        #endregion

        #region 계속하기
        public static void Continue(string str)
        {
            Console.WriteLine(str);
            Console.ReadKey();
        }
        #endregion

    }
    public static class DeffenStatic
    {
        public static bool SlowTxtOut = false;
        public static int SlowTxtOutTime = 0;
        public static int SlowTxtLineTime = 0;
    }
}
