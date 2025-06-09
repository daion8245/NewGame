namespace newgame
{
    internal class Monster : Character
    {
        public void start()
        {
            MyStatus = new Status()
            {
                charType = CharType.MONSTER,
                Name = "몬스터",
                level = 1,
                hp = 50,
                maxHp = 50,
                ATK = 1,
                DEF = 0,
                exp = 5,
                coin = 10,
            };

            MyStatus.ShowStatus();
        }

        public override void Dead(Character target)//override = 상속 (상속받은 dead 함수를 호출한다 다른 대상은 target으로 설정한다)
        {
            target.MyStatus.coin += MyStatus.coin;
            target.MyStatus.exp += MyStatus.exp;

            base.Dead(target);
        }
    }
}
