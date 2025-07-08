namespace newgame
{
    internal class Battle : Character //Battle 클래스 생성 Character를 상속받는다
    {
        public Battle()
        {
            //Start();
            Start_Battle();
        }

        public void Start()
        {
            for (int i = 3; i > 0; i--)//3번 반복
            {
                Thread.Sleep(1000);//1초 대기
                Console.WriteLine(i);//i출력

                Console.Clear();//콘솔 지우기
            }

            Console.WriteLine("전투 시작");//메세지
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
