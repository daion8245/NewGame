using System.Net.Security;

namespace newgame
{
    /// <summary>
    /// Utility functions for console UI operations.
    /// </summary>
    internal class UiHelper
    {

        #region 선택 메뉴
        public static int SelectMenu(string[] options)
        {
            int lineCoordinate;
            int selected = 0;
            bool firstRun = false;

            ConsoleKey key;

            lineCoordinate = Console.CursorTop + options.Length;

            do
            {
                if (firstRun)
                {
                    ClearMenuLines(options.Length, lineCoordinate);
                }
                else
                {
                    firstRun = true;
                }
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selected)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selected = (selected - 1 + options.Length) % options.Length;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selected = (selected + 1) % options.Length;
                }

            } while (key != ConsoleKey.Enter);

            Console.ResetColor();

            return selected;

        }

        const string Esc = "\u001b[";
        static void ClearMenuLines(int lineCount, int coordinates)
        {
            Console.SetCursorPosition(0, coordinates);

            for (int i = 0; i < lineCount; i++)
            {
                Console.Write($"{Esc}2K");
                Console.Write($"{Esc}1F");
            }
        }
        #endregion

        #region 텍스트 고급 출력
        public static void TxtOut(string[] str)
        {
            foreach (string line in str)
            {
                if (TextDisplayConfig.SlowTxtOut)
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        Console.Write(line[i]);
                        Thread.Sleep(TextDisplayConfig.SlowTxtOutTime);
                    }

                    Console.WriteLine();
                    Thread.Sleep(TextDisplayConfig.SlowTxtLineTime);
                }
                else
                {
                    Console.WriteLine(line);
                    Thread.Sleep(TextDisplayConfig.SlowTxtLineTime);
                }
            }
        }
        #endregion

        #region 입력 대기
        public static void WaitForInput(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
        }
        #endregion

    }
    /// <summary>
    /// Global configuration for text output.
    /// </summary>
    public static class TextDisplayConfig
    {
        public static bool SlowTxtOut = false;
        public static int SlowTxtOutTime = 0;
        public static int SlowTxtLineTime = 0;
    }
}
