using System.Net.Security;

namespace newgame
{
    internal class UiHelper
    {

        #region 선택 메뉴
        public static int SelectMenu(string[] options, string? upTxt = null)
        {
            int lineCoordinate;
            int selected = 0;
            bool firstRun = false;
            int test = 0;

            ConsoleKey key;

            lineCoordinate = Console.CursorTop + options.Length;

            if (upTxt != null)
            {
                Console.WriteLine(upTxt + test);
                lineCoordinate++;
            }

            do
            {
                if (firstRun)
                {
                    if (upTxt != null)
                    {
                        Console.WriteLine(upTxt + test);
                        test++;
                    }
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
        public static void TxtOut(string[] str, bool SlowTxtOut = true, int SlowTxtOutTime = 30, int SlowTxtLineTime = 0)
        {
            foreach (string line in str)
            {
                if (SlowTxtOut)
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        Console.Write(line[i]);
                        Thread.Sleep(SlowTxtOutTime);
                    }

                    Console.WriteLine();
                    Thread.Sleep(SlowTxtLineTime);
                }
                else
                {
                    Console.WriteLine(line);
                    Thread.Sleep(SlowTxtLineTime);
                }
            }
        }
        #endregion

        #region 입력 대기
        public static void WaitForInput(string message = "[ENTER]를 눌러 계속")
        {
            Console.WriteLine(message);
            Console.ReadKey();
            return;
        }
        #endregion

    }
}
