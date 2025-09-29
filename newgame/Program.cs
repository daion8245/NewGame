public using newgame.Manager;
using newgame.Room;

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
