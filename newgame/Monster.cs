namespace newgame
{
    internal class Monster : Character
    {
        public void Start()
        {
            MyStatus = GameManager.Instance.GetMonsterStat(1);

            MyStatus.ShowStatus();
        }

        public override void Dead(Character target)//override = 상속 (상속받은 dead 함수를 호출한다 다른 대상은 target으로 설정한다)
        {
            target.MyStatus.gold += MyStatus.gold;
            target.MyStatus.exp += MyStatus.exp;

            base.Dead(target);
        }
    }
}
