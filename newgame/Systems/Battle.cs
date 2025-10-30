using newgame.Characters;
using newgame.Services;

namespace newgame.Systems
{
    internal class Battle : Character //Battle 클래스 생성 Character를 상속받는다
    {
        public Battle() : this(GameManager.Instance.BattleLogService)
        {
        }

        public Battle(BattleLogService battleLogService) : base(battleLogService)
        {
            // 생성자에서 전투를 바로 시작하지 않는다.
        }

        public bool Start()
        {
            Console.Clear();

            return Start_Battle();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static bool Start_Battle()
        {
            Character player = GameManager.Instance.RequirePlayer();
            Character monster = GameManager.Instance.RequireMonster();

            Character[] chars = new Character[] { player, monster };

            player.IsbattleRun = false;     // 필요시 monster도 false 초기화

            int current = 0; // 0: player, 1: monster

            player.EnteringBattle(monster);
            monster.EnteringBattle(player);
            
            bool playerBattelWin = false;

            while (true)
            {
                int attackerIdx = current;
                int defenderIdx = (current + 1) % chars.Length;

                Character attacker = chars[attackerIdx];
                Character defender = chars[defenderIdx];
                if (attacker is Player playerActor)
                {
                    playerActor.PerformAction(defender);
                }
                else
                {
                    attacker.Attack(defender);
                }

                // 1) 공격자가 '도주' 선택했으면 종료
                if (attacker.IsbattleRun)
                {
                    playerBattelWin = false;
                    break;
                }

                // 2) 피격자가 죽었으면 즉시 종료
                if (defender.IsDead)
                {
                    playerBattelWin = defender is Player ? false : true;
                    break;
                }

                // 턴 교대
                current = defenderIdx;

                Thread.Sleep(1000);
            }
            
            return playerBattelWin;
        }

    }
}
