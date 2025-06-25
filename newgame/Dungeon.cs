using static newgame.MyDiffain;

namespace newgame
{
    internal class Dungeon
    {
        public Dungeon()
        {
            Start();
        }
        public void Start()
        {
            Console.Clear();
            MyDiffain.TxtOut(["던전 진입",
                              "앗! 야생의 몬스터가 나타났다"]);
            Console.Clear();

            CreateMonster();
        }
        void CreateMonster()
        {
            Monster monster = new Monster();
            GameManager.Instance.monster = monster;
            monster.start();
            Battle();
        }

        void Battle()
        {
            Battle battle = new Battle();
            battle.Start();
        }
    }
}
