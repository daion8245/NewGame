namespace newgame
{
    internal class Battle : Character //Battle 클래스 생성 Character를 상속받는다
    {
        public Battle()
        {
            Start();
        }

        public void Start()
        {
            Console.Clear();

            Start_Battle();
        }

        void Start_Battle()
        {
            Character[] charac = new Character[]
            {
                GameManager.Instance.player,
                GameManager.Instance.monster,
            };

            GameManager.Instance.player.isbattleRun = false;

            int currentIndex = 0;
            while (true)
            {
                int nextIndex = (currentIndex + 1) % charac.Length;
                charac[currentIndex].Attack(charac[nextIndex]);

                if (charac[currentIndex].isbattleRun == true)
                {
                    break;
                }

                currentIndex = nextIndex;

                Thread.Sleep(1000);

                if (charac[currentIndex].IsDead == true)
                {
                    break;
                }
            }
        }
    }
}
