namespace newgame
{
    internal class Monster : Character
    {
        public Monster() : this(GameManager.Instance.BattleLogService)
        {
        }

        public Monster(BattleLogService battleLogService) : base(battleLogService)
        {
        }

        public void Start(int monsterType)
        {
            if (monsterType >= 1)
            {
                MyStatus = GameManager.Instance.GetMonsterStat(monsterType);
            }
            else
            {
                MyStatus = GameManager.Instance.GetMonsterStat(1);
                Console.WriteLine("잘못된 몬스터 타입입니다.");
            }
            Console.Clear();

            UiHelper.TxtOut([$"{MyStatus.Name}이(가) 나타났다!",""]);
            UiHelper.WaitForInput();
        }

        public override void Dead(Character target)//override = 상속 (상속받은 dead 함수를 호출한다 다른 대상은 target으로 설정한다)
        {
            target.MyStatus.gold += MyStatus.gold;
            target.MyStatus.exp += MyStatus.exp;

            base.Dead(target);
        }
    }
}
