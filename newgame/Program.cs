using newgame.Systems;

namespace newgame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameBuild game = GameBuild.Create();
            game.Run();
        }
    }
}
