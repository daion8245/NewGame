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

        public static void MenuSelet()
        {

        }
    }
}
