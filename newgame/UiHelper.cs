﻿using System.Net.Security;

namespace newgame
{
    internal class UiHelper
    {

        #region 선택 메뉴
        public static int SelectMenu(string[] options, string? upTxt = null)
        {
            int selected = 0;
            int menuStartLine = Console.CursorTop;

            // Display upTxt only once at the beginning if provided
            if (upTxt != null)
            {
                Console.WriteLine(upTxt);
                menuStartLine = Console.CursorTop;
            }

            // Initial menu draw
            DrawMenu(options, selected);

            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selected = (selected - 1 + options.Length) % options.Length;
                    RedrawMenu(options, selected, menuStartLine);
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selected = (selected + 1) % options.Length;
                    RedrawMenu(options, selected, menuStartLine);
                }

            } while (key != ConsoleKey.Enter);

            Console.ResetColor();
            return selected;
        }

        private static void DrawMenu(string[] options, int selected)
        {
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
        }

        private static void RedrawMenu(string[] options, int selected, int startLine)
        {
            Console.SetCursorPosition(0, startLine);
            DrawMenu(options, selected);
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
