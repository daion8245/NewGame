namespace newgame
{
    internal class Battle : Character //Battle 클래스 생성 Character를 상속받는다
    {
        int temp = 0; //int temp를 만들고 0으로 초기화
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

            int curAtkNum = 0;
            while (true)
            {
                int temp = (curAtkNum + 1) % charac.Length;
                charac[curAtkNum].Attack(charac[temp]);

                if (charac[curAtkNum].isbattleRun == true)
                {
                    break;
                }

                curAtkNum = temp;

                Thread.Sleep(1000);

                if (charac[curAtkNum].IsDead == true)
                {
                    break;
                }
            }
        }
    }
}
