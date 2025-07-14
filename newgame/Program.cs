using System.Threading.Channels;

namespace newgame
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Init();

            StartMessage startMessage = new StartMessage();
            startMessage.Start();
        }

        static void Init()
        {
            DataManager.Instance.LoadAllEquipData();
            DataManager.Instance.LoadEnemyData();
            DataManager.Instance.LoadDungeonMap();
        }
    }
}
