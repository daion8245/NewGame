using System.Threading.Channels;

namespace newgame
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Init();

            StartMasage startMasage = new StartMasage();
            startMasage.Start();
        }

        static void Init()
        {
            DataManager.Instance.LoadAllEquipData();
            DataManager.Instance.LoadEnemyData();
        }
    }
}
