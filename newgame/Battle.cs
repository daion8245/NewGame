namespace newgame
{
    internal class Battle : Character //Battle 클래스 생성 Character를 상속받는다
    {
        public Battle()
        {
            // 생성자에서 전투를 바로 시작하지 않는다.
        }

        public void Start()
        {
            Console.Clear();

            Start_Battle();
        }

        private static void Start_Battle()
        {
            Character player = GameManager.Instance.player;
            Character monster = GameManager.Instance.monster;

            Character[] chars = new Character[] { player, monster };

            player.isbattleRun = false;     // 필요시 monster도 false 초기화

            int current = 0; // 0: player, 1: monster
            while (true)
            {
                int attackerIdx = current;
                int defenderIdx = (current + 1) % chars.Length;

                Character attacker = chars[attackerIdx];
                Character defender = chars[defenderIdx];

                string[] log = attacker.Attack(defender);

                //// 둘 다 보여주고 싶으면 if 제거
                //if (attacker == monster)
                //{
                //    GameManager.Instance.player.ShowBattleInfo(GameManager.Instance.monster,bo);
                //}

                // 1) 공격자가 '도주' 선택했으면 종료
                if (attacker.isbattleRun)
                    break;

                // 2) 피격자가 죽었으면 즉시 종료
                if (defender.IsDead)
                    break;

                // 턴 교대
                current = defenderIdx;

                Thread.Sleep(1000);
            }
        }

    }
}
